using DatabaseLayer.CustomStoredProcedures;
using DatabaseLayer.Exceptions;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Metadata;
using DatabaseLayer.Metadata.Differences;
using DatabaseLayer.Models;
using DatabaseLayer.SqlServerProvider;
using DatabaseLayer.SqlServerProvider.DataObjectInterfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DatabaseLayer
{
    public abstract class BaseDataProvider : IDisposable
    {
        private MetadataStore _metaData;
        private ConnectionInfo mConnection;

        public abstract string ConnectionString { get; }

        public abstract string GetTableName(Type type);

        protected abstract DbIndex[] GetIndices();

        public abstract void ConfigureDefaultData();

        public virtual List<IStoredProcedure> CustomStoredProcedures => new List<IStoredProcedure>();

        public IDataObjectInterface<BatchNumber> BatchNumbers { get; set; }

        public BaseDataProvider() => this.BatchNumbers = (IDataObjectInterface<BatchNumber>)new SpDataObjectInterface<BatchNumber>(this);

        public MetadataStore Metadata
        {
            get
            {
                if (this._metaData == null)
                    this._metaData = new MetadataStore(this);
                return this._metaData;
            }
        }

        public StructureDifferences CompareModelToDatabase()
        {
            this.EnsureMetadataProcedures();
            List<string> values = this.ValidateModel();
            if (values.Count > 0)
                throw new DeploymentException(string.Join(Environment.NewLine, (IEnumerable<string>)values));
            StructureDifferences database = new StructureDifferences();
            foreach (TableMetadata table in this.Metadata.Tables)
            {
                TableDifference tableDifference = CompareToDbObject.Compare(this, table, this.ConnectionString);
                if (tableDifference != null && !tableDifference.Match)
                    database.TableDifferences.Add(tableDifference);
            }
            foreach (DbIndex index in this.GetIndices())
            {
                IndexDifference indexDifference = CompareToDbObject.Compare(this, index, this.ConnectionString);
                if (indexDifference != null && !indexDifference.Match)
                    database.IndexDifferences.Add(indexDifference);
            }
            using (SqlConnection conn = new SqlConnection(this.ConnectionString))
            {
                ((DbConnection)conn).Open();
                foreach (IStoredProcedure storedProcedure in this.GetAllStoredProceduresToDeploy())
                {
                    if (!StoredProcedureHelper.CheckStoredProcedure(conn, storedProcedure))
                        database.StoredProcedureDifferences.Add(storedProcedure);
                }
            }
            return database;
        }

        public void EnsureMetadataProcedures()
        {
            var asm = Assembly.GetExecutingAssembly();
            var sqlResources = asm.GetManifestResourceNames()
                .Where(n => n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase));

            if (!sqlResources.Any())
                return; 

            using var conn = new SqlConnection(this.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();

            var splitter = new Regex(
                @"^\s*GO\s*($|\-\-.*$)",
                RegexOptions.Multiline | RegexOptions.IgnoreCase
            );

            foreach (var resourceName in sqlResources)
            {
                using var stream = asm.GetManifestResourceStream(resourceName);
                using var reader = new StreamReader(stream);
                var script = reader.ReadToEnd();

                foreach (var batch in splitter
                    .Split(script)
                    .Select(b => b.Trim())
                    .Where(b => !string.IsNullOrWhiteSpace(b)))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = batch;
                    cmd.Parameters.Clear();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void StartTransaction() => this.GetConnection().StartTransaction();

        public void CommitTransaction() => this.GetConnection().CommitTransaction();

        public void RollbackTransaction() => this.GetConnection().RollbackTransaction();

        internal ConnectionInfo GetConnection()
        {
            if (this.mConnection == null)
                this.mConnection = new ConnectionInfo(this.ConnectionString);
            this.mConnection.Open();
            return this.mConnection;
        }

        public void Deploy(StructureDifferences result, DeploySettings deploySettings = null)
        {
            DeploySettings deploySettings1 = deploySettings ?? new DeploySettings();
            using (SqlConnection conn = new SqlConnection(this.ConnectionString))
            {
                ((DbConnection)conn).Open();
                foreach (TableDifference tableDifference in result.TableDifferences)
                {
                    try
                    {
                        tableDifference.DeployTableChanges(conn, deploySettings1, this);
                        tableDifference.DeployStoredProcedureChanges(conn);
                    }
                    catch (Exception ex)
                    {
                        throw new DeploymentException("Error deploying tableDifferences." + tableDifference.Table.Type.ToString() + " , " + ex.Message, ex);
                    }
                }
                foreach (TableDifference tableDifference in result.TableDifferences)
                {
                    try
                    {
                        tableDifference.DeployForeignKeys(conn, this);
                    }
                    catch (Exception ex)
                    {
                        throw new DeploymentException("Error deploying foreign keys." + tableDifference.Table.Type.ToString() + ", " + ex.Message, ex);
                    }
                }
                foreach (IndexDifference indexDifference in result.IndexDifferences)
                    indexDifference.Deploy(conn, deploySettings1, this);
                foreach (IStoredProcedure procedureDifference in result.StoredProcedureDifferences)
                    StoredProcedureHelper.Deploy(conn, procedureDifference, deploySettings1, this);
            }
            foreach (string deploymentScript in deploySettings1.PostDeploymentScripts)
            {
                using (SqlCommand cmd = this.GetConnection().Connection.CreateCommand())
                {
                    cmd.CommandText = "dbo.ExecuteDynamicSql";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Sql", deploymentScript);
                    cmd.ExecuteNonQuery();
                }
            }
            if (!deploySettings1.IncludeDefaultData)
                return;
            this.ConfigureDefaultData();
        }

        internal TableMetadata GetTableSpec(string toType) => this.Metadata.Tables.FirstOrDefault<TableMetadata>((Func<TableMetadata, bool>)(t => ((MemberInfo)t.Type).Name == toType)) ?? throw new TypeNotFoundException("Could not find type " + toType + " in data provider of type " + this.GetType().FullName);

        public List<string> ValidateModel()
        {
            List<string> stringList = new List<string>();
            foreach (TableMetadata table in this.Metadata.Tables)
            {
                foreach (ColumnMetaData column in table.Columns)
                {
                    if (column.DataType == typeof(Decimal))
                    {
                        int? scale = column.Scale;
                        int? precision = column.Precision;
                        if (scale.GetValueOrDefault() > precision.GetValueOrDefault() & scale.HasValue & precision.HasValue)
                            stringList.Add(string.Format("Column {0} on type {1} has a larger scale({2}) than precision({3}).", (object)column.Name, (object)((MemberInfo)table.Type).Name, (object)column.Scale, (object)column.Precision));
                    }
                    if (column.DataType == typeof(Decimal))
                    {
                        int? precision1 = column.Precision;
                        int num1 = 38;
                        if (!(precision1.GetValueOrDefault() > num1 & precision1.HasValue))
                        {
                            int? precision2 = column.Precision;
                            int num2 = 1;
                            if (!(precision2.GetValueOrDefault() < num2 & precision2.HasValue))
                                goto label_11;
                        }
                        stringList.Add("Column " + column.Name + " on type " + ((MemberInfo)table.Type).Name + " has a precision outside it's range of 1 - 38.");
                    }
                label_11:
                    if (column.DataType == typeof(Decimal))
                    {
                        int? scale1 = column.Scale;
                        int num3 = 38;
                        if (!(scale1.GetValueOrDefault() > num3 & scale1.HasValue))
                        {
                            int? scale2 = column.Scale;
                            int num4 = 1;
                            if (!(scale2.GetValueOrDefault() < num4 & scale2.HasValue))
                                goto label_15;
                        }
                        stringList.Add("Column " + column.Name + " on type " + ((MemberInfo)table.Type).Name + " has a scale outside it's range of 1 - 38.");
                    }
                label_15:
                    if (column.DataType == typeof(string) && column.Length == 1)
                        stringList.Add("Column " + column.Name + " on type " + ((MemberInfo)table.Type).Name + " has a string length of 1, this should be a char instead.");
                    if (column.DataType == typeof(bool) && column.Length > 1)
                        stringList.Add("Column " + column.Name + " on type " + ((MemberInfo)table.Type).Name + " is a bool and cannot have a length of > 1.");
                }
            }
            return stringList;
        }

        public Task<int> ExecuteInsertStoredProcedureAsync(
          string storedProcedureName,
          params QueryParameter[] parameters)
        {
            return QueryHelper.ExecuteInsertStoredProcedureAsync(storedProcedureName, this.GetConnection(), parameters);
        }

        public async Task ExecuteStoredProcedureAsync(
          string storedProcedureName,
          params QueryParameter[] parameters)
        {
            int num = await QueryHelper.ExecuteStoredProcedureAsync(storedProcedureName, this.GetConnection(), parameters);
        }

        public Task<List<T>> ExecuteRetrievalProcedureAsync<T>(
          string storedProcedureName,
          Func<SqlDataReader, T> convertRowToObj,
          params QueryParameter[] parameters)
        {
            return QueryHelper.ExecuteRetrievalProcedureAsync<T>(storedProcedureName, convertRowToObj, this.GetConnection(), parameters);
        }

        public async Task<List<string>> GetNextBatchNumbersAsync(int batchNumberType, int count) => await this.ExecuteRetrievalProcedureAsync<string>("spBatchNumber_GetNext", (Func<SqlDataReader, string>)(reader => ((DbDataReader)reader).GetString(0)), new QueryParameter("@BatchNumberType", (SqlDbType)8, (object)batchNumberType), new QueryParameter("@NumberOfBatchNumbers", (SqlDbType)8, (object)count));

        internal List<IStoredProcedure> GetAllStoredProceduresToDeploy()
        {
            List<IStoredProcedure> storedProcedures = this.CustomStoredProcedures;
            storedProcedures.Add((IStoredProcedure)new spBatchNumber_GetNext(this));
            return storedProcedures;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize((object)this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.mConnection == null)
                return;
            this.mConnection.Dispose();
            this.mConnection = (ConnectionInfo)null;
        }
    }
}
