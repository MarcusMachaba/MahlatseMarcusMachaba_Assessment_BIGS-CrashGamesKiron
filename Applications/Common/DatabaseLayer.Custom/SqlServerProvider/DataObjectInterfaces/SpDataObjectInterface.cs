using DatabaseLayer.Exceptions;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Metadata;
using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.SqlServerProvider.DataObjectInterfaces
{
    public class SpDataObjectInterface<T> : IDataObjectInterface<T>, IDataObjectInterface
    {
        private readonly BaseDataProvider mProvider;

        public TableMetadata Metadata { get; private set; }

        public SpDataObjectInterface(BaseDataProvider provider)
        {
            this.Metadata = Provider.Build<T>(provider);
            this.mProvider = provider;
        }

        public SpDataObjectInterface()
        {
        }

        public int CreateUpdate(T obj)
        {
            int update = this.Metadata.PrimaryKeyProperty.GetValue<int>((object)obj);
            if (update == 0)
                return this.Create(obj);
            this.Update(obj);
            return update;
        }

        public int Create(T obj) => QueryHelper.ExecuteInsertStoredProcedure("sp" + this.Metadata.TableContract.FileGroup + "_" + ((MemberInfo)this.Metadata.Type).Name + "_Insert", this.mProvider.GetConnection(), this.GetQueryParameters((object)obj, false));

        public void Update(T obj)
        {
            if (QueryHelper.ExecuteStoredProcedure("sp" + this.Metadata.TableContract.FileGroup + "_" + ((MemberInfo)this.Metadata.Type).Name + "_Update", this.mProvider.GetConnection(), this.GetQueryParameters((object)obj, true)) == 0)
                throw new EntryNotFoundException();
        }

        public void Delete(int id)
        {
            if (QueryHelper.ExecuteStoredProcedure("sp" + this.Metadata.TableContract.FileGroup + "_" + ((MemberInfo)this.Metadata.Type).Name + "_Delete", this.mProvider.GetConnection(), new QueryParameter("@" + this.Metadata.PrimaryKeyProperty.Name, TypeConverter.ConvertFromObjectType(this.Metadata.PrimaryKeyProperty.DataType, -1, false), (object)id)) == 0)
                throw new EntryNotFoundException();
        }

        public virtual List<T> Read(object filter)
        {
            object obj = filter;
            if (obj == null || obj == (ValueType)0)
                obj = (object)new { };
            if (obj is int num)
                return QueryHelper.ExecuteRetrievalProcedure<T>("sp" + this.Metadata.TableContract.FileGroup + "_" + ((MemberInfo)this.Metadata.Type).Name + "_Retrieve", new Func<SqlDataReader, T>(this.ConvertRowToObj), this.mProvider.GetConnection(), new QueryParameter("@" + this.Metadata.PrimaryKeyProperty.Name, TypeConverter.ConvertFromObjectType(this.Metadata.PrimaryKeyProperty.DataType, -1, false), (object)num));
            List<QueryParameter> queryParameterList = new List<QueryParameter>();
            foreach (PropertyInfo property1 in obj.GetType().GetProperties())
            {
                PropertyInfo property = property1;
                ColumnMetaData columnMetaData = this.Metadata.QueryableColumns.FirstOrDefault<ColumnMetaData>((Func<ColumnMetaData, bool>)(c => c.Name == ((MemberInfo)property).Name));
                if (columnMetaData == null)
                    throw new TypeNotFoundException("Could not find the appropriate retrieval property " + ((MemberInfo)property).Name + " on type " + ((MemberInfo)typeof(T)).Name + ".");
                queryParameterList.Add(new QueryParameter("@" + ((MemberInfo)property).Name, TypeConverter.ConvertFromObjectType(columnMetaData.DataType, columnMetaData.Length, columnMetaData.IsTimeStamp), property.GetValue(obj), columnMetaData.Length));
            }
            return QueryHelper.ExecuteRetrievalProcedure<T>("sp" + this.Metadata.TableContract.FileGroup + "_" + ((MemberInfo)this.Metadata.Type).Name + "_Retrieve", new Func<SqlDataReader, T>(this.ConvertRowToObj), this.mProvider.GetConnection(), queryParameterList.ToArray());
        }

        public T ConvertRowToObj(SqlDataReader row)
        {
            T instance = Activator.CreateInstance<T>();
            this.Metadata.PrimaryKeyProperty.SetValue<T>(instance, ((DbDataReader)row)[this.Metadata.PrimaryKeyProperty.Name] == DBNull.Value ? (object)null : ((DbDataReader)row)[this.Metadata.PrimaryKeyProperty.Name]);
            foreach (ColumnMetaData column in this.Metadata.Columns)
            {
                if (column.DataType == typeof(char))
                {
                    if (!string.IsNullOrEmpty(((DbDataReader)row)[column.Name].ToString()))
                        column.SetValue<T>(instance, (object)((DbDataReader)row)[column.Name].ToString()[0]);
                    else
                        column.SetValue<T>(instance, (object)char.MinValue);
                }
                else if (column.DataType == typeof(char?))
                {
                    if (!string.IsNullOrEmpty(((DbDataReader)row)[column.Name].ToString()))
                        column.SetValue<T>(instance, (object)(((DbDataReader)row)[column.Name] == DBNull.Value ? new char?() : new char?(((DbDataReader)row)[column.Name].ToString()[0])));
                    else
                        column.SetValue<T>(instance, (object)null);
                }
                else
                    column.SetValue<T>(instance, ((DbDataReader)row)[column.Name] == DBNull.Value ? (object)null : ((DbDataReader)row)[column.Name]);
            }
            foreach (ColumnMetaData joinedColumn in this.Metadata.JoinedColumns)
                joinedColumn.SetValue<T>(instance, ((DbDataReader)row)[joinedColumn.Name] == DBNull.Value ? (object)null : ((DbDataReader)row)[joinedColumn.Name]);
            return instance;
        }

        public QueryParameter[] GetQueryParameters(object obj, bool update)
        {
            List<QueryParameter> queryParameterList = new List<QueryParameter>();
            List<ColumnMetaData> columnMetaDataList = new List<ColumnMetaData>();
            columnMetaDataList.Add(this.Metadata.PrimaryKeyProperty);
            columnMetaDataList.AddRange((IEnumerable<ColumnMetaData>)this.Metadata.Columns);
            foreach (ColumnMetaData columnMetaData in columnMetaDataList)
            {
                if (!(columnMetaData.Name == this.Metadata.TableContract.PrimaryKey) || update)
                    queryParameterList.Add(new QueryParameter("@" + columnMetaData.Name, TypeConverter.ConvertFromObjectType(columnMetaData.DataType, columnMetaData.Length, columnMetaData.IsTimeStamp), columnMetaData.GetValue<object>(obj), columnMetaData.Length));
            }
            return queryParameterList.ToArray();
        }

        public Task<int> CreateAsync(T obj) => QueryHelper.ExecuteInsertStoredProcedureAsync("sp" + this.Metadata.TableContract.FileGroup + "_" + ((MemberInfo)this.Metadata.Type).Name + "_Insert", this.mProvider.GetConnection(), this.GetQueryParameters((object)obj, false));

        public async Task<int> CreateUpdateAsync(T obj)
        {
            int primaryKey = this.Metadata.PrimaryKeyProperty.GetValue<int>((object)obj);
            if (primaryKey == 0)
                primaryKey = this.CreateAsync(obj).Result;
            else
                await this.UpdateAsync(obj);
            return primaryKey;
        }

        public async Task DeleteAsync(int id)
        {
            if (await QueryHelper.ExecuteStoredProcedureAsync("sp" + this.Metadata.TableContract.FileGroup + "_" + ((MemberInfo)this.Metadata.Type).Name + "_Delete", this.mProvider.GetConnection(), new QueryParameter("@" + this.Metadata.PrimaryKeyProperty.Name, TypeConverter.ConvertFromObjectType(this.Metadata.PrimaryKeyProperty.DataType, -1, false), (object)id)) == 0)
                throw new EntryNotFoundException();
        }

        public Task<List<T>> ReadAsync(object filter)
        {
            object obj = filter;
            if (obj == null || obj == (ValueType)0)
                obj = (object)new { };
            if (obj is int num)
                return QueryHelper.ExecuteRetrievalProcedureAsync<T>("sp" + this.Metadata.TableContract.FileGroup + "_" + ((MemberInfo)this.Metadata.Type).Name + "_Retrieve", new Func<SqlDataReader, T>(this.ConvertRowToObj), this.mProvider.GetConnection(), new QueryParameter("@" + this.Metadata.PrimaryKeyProperty.Name, TypeConverter.ConvertFromObjectType(this.Metadata.PrimaryKeyProperty.DataType, -1, false), (object)num));
            List<QueryParameter> queryParameterList = new List<QueryParameter>();
            foreach (PropertyInfo property1 in obj.GetType().GetProperties())
            {
                PropertyInfo property = property1;
                ColumnMetaData columnMetaData = this.Metadata.QueryableColumns.FirstOrDefault<ColumnMetaData>((Func<ColumnMetaData, bool>)(c => c.Name == ((MemberInfo)property).Name));
                if (columnMetaData == null)
                    throw new TypeNotFoundException("Could not find the appropriate retrieval property " + ((MemberInfo)property).Name + " on type " + ((MemberInfo)typeof(T)).Name + ".");
                queryParameterList.Add(new QueryParameter("@" + ((MemberInfo)property).Name, TypeConverter.ConvertFromObjectType(columnMetaData.DataType, columnMetaData.Length, columnMetaData.IsTimeStamp), property.GetValue(obj), columnMetaData.Length));
            }
            return QueryHelper.ExecuteRetrievalProcedureAsync<T>("sp" + this.Metadata.TableContract.FileGroup + "_" + ((MemberInfo)this.Metadata.Type).Name + "_Retrieve", new Func<SqlDataReader, T>(this.ConvertRowToObj), this.mProvider.GetConnection(), queryParameterList.ToArray());
        }

        public async Task UpdateAsync(T obj)
        {
            if (await QueryHelper.ExecuteStoredProcedureAsync("sp" + this.Metadata.TableContract.FileGroup + "_" + ((MemberInfo)this.Metadata.Type).Name + "_Update", this.mProvider.GetConnection(), this.GetQueryParameters((object)obj, true)) == 0)
                throw new EntryNotFoundException();
        }
    }
}
