using Core;
using Dapper;
using Dapper.Contrib.Extensions;
using DatabaseLayer.Dapper.QueryModel;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Dapper.ImplementationInterfaces
{
    public interface ISqlDbRepository : IDapperRepository
    {
        Task<IEnumerable<T>> GetFromIdentifier<T>(string colName, long id, int records, bool lockTable = false) where T : class, IHasId;
        Task<IEnumerable<T>> GetFromIdentifi<T>(string colName, long id, int records, bool lockTable = false) where T : class, IHasGId;
    }
    public class SqlDbRepository : ISqlDbRepository
    {
        private readonly string _connectionString;

        public SqlDbRepository()
        {
            _connectionString = RepositoryDependencies.SqlConnectionString;
        }

        private IDbConnection GetConnection()
        {
            try
            {
                var connection = new SqlConnection(_connectionString);
                connection.Open();
                return connection;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<T> ExecuteScalar<T>(string sql, object parameters)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    T result = await conn.ExecuteScalarAsync<T>(sql, parameters);
                    return result;
                }
            }
            catch (Exception ex)
            {
                return default;
            }
        }

        public async Task<T> ExecuteInsert<T>(DapperHelper helper)
        {
            try
            {
                return await ExecuteScalar<T>(helper.InsertSql, helper.Parameters);
            }
            catch (Exception e)
            {
                throw;
            }

        }

        public async Task<T> Add<T>(T item) where T : class, IHasIdOnly// IHasId,
        {
            try
            {
                using (var c = GetConnection())
                    item.Id = await c.InsertAsync(item);
                return item;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public async Task<T> GetLatestRecordAsync<T>() where T : class, IHasIdOnly
        {
            try
            {
                using (var c = GetConnection())
                {
                    var sql = $"SELECT TOP(1) * FROM {typeof(T).GetTableName()} WITH (NOLOCK) ORDER BY Id DESC";
                    return await c.QuerySingleAsync<T>(sql);
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<T> AddAsync<T>(T item) where T : class, IHasGId
        {
            try
            {

                if (item == null)
                    throw new ArgumentNullException("item");

                using (var c = GetConnection())
                {
                    int rowsInserted = await c.InsertAsync(item);

                    return default;
                }
                //To be clear, you can't convert a column from numeric to uniqueidentifier directly,
                //but you can convert numeric to varchar to uniqueidentifier.
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<bool> Archive<T>(T item) where T : class, IHasIdSafeRecord
        {
            using (var c = GetConnection())
            {
                item.Updated = DateTime.Now;
                item.Archived = true;
                return await c.UpdateAsync(item);
            }
        }

        public async Task<IEnumerable<T>> GetAll<T>() where T : class, IHasIdOnly//IHasId
        {
            try
            {
                using (var c = GetConnection())
                {
                    var allItems = await c.GetAllAsync<T>();
                    return allItems;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetByFilter<T>(params KeyValuePair<string, object>[] parameters) where T : class, IHasIdOnly//IHasId
        {
            try
            {
                using (var c = GetConnection())
                {
                    var sql = $"SELECT * FROM {typeof(T).GetTableName()} WITH (NOLOCK)";
                    var queryParams = new DynamicParameters();
                    if (parameters != null && parameters.Any())
                    {
                        sql += $" WHERE {parameters[0].Key}=@{parameters[0].Key}";
                        queryParams.Add(parameters[0].Key, parameters[0].Value);

                        foreach (var p in parameters.Skip(1))
                        {
                            sql += $" AND {p.Key}=@{p.Key}";
                            queryParams.Add(p.Key, p.Value);
                        }
                    }
                    return await c.QueryAsync<T>(sql, queryParams);
                }
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async Task<IEnumerable<T>> GetByFilt<T>(params KeyValuePair<string, object>[] parameters) where T : class, IHasGId
        {
            try
            {
                using (var c = GetConnection())
                {
                    var sql = $"SELECT * FROM {typeof(T).GetTableName()} WITH (NOLOCK)";
                    var queryParams = new DynamicParameters();
                    if (parameters != null && parameters.Any())
                    {
                        sql += $" WHERE {parameters[0].Key}=@{parameters[0].Key}";
                        queryParams.Add(parameters[0].Key, parameters[0].Value);

                        foreach (var p in parameters.Skip(1))
                        {
                            sql += $" AND {p.Key}=@{p.Key}";
                            queryParams.Add(p.Key, p.Value);
                        }
                    }
                    return await c.QueryAsync<T>(sql, queryParams);
                }
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> SearchByParametersAsync<T>(int? count, params QueryPropertyPair[] parameters) where T : class, IHasId
        {
            using (var connection = GetConnection())
            {
                var query = SqlQueryHelper.SelectMultipleQuery<T>(parameters, count: count);
                return await connection.QueryAsync<T>(query.Query, param: query.Parameters);
            }
        }

        public async Task<IEnumerable<T>> GetFromIdentifier<T>(string colName, long id, int records, bool lockTable = false) where T : class, IHasId
        {
            //if (records < 20)
            //    records = 100;
            if (records < 3)
                records = 2;
            using (var c = GetConnection())
            {
                var sql = $"SELECT * FROM {typeof(T).GetTableName()} {(lockTable ? "" : "WITH (NOLOCK)")}";
                var queryParams = new DynamicParameters();

                sql += $" ORDER BY {colName} ASC" +
                    $" OFFSET({id}) ROWS FETCH NEXT({records}) ROWS ONLY";
                queryParams.Add(colName, id);
                return await c.QueryAsync<T>(sql, queryParams);
            }
        }

        public async Task<IEnumerable<T>> GetFromIdentifi<T>(string colName, long id, int records, bool lockTable = false) where T : class, IHasGId
        {
            //if (records < 20)
            //    records = 100;
            if (records < 3)
                records = 2;
            using (var c = GetConnection())
            {
                var sql = $"SELECT * FROM {typeof(T).GetTableName()} {(lockTable ? "" : "WITH (NOLOCK)")}";
                var queryParams = new DynamicParameters();

                sql += $" ORDER BY {colName} ASC" +
                    $" OFFSET({id}) ROWS FETCH NEXT({records}) ROWS ONLY";
                queryParams.Add(colName, id);
                return await c.QueryAsync<T>(sql, queryParams);
            }
        }

        public async Task<T> GetById<T>(long? id) where T : class, IHasIdOnly//IHasId
        {
            try
            {
                if (id == null)
                    return default;
                return (await GetByFilter<T>(
                    new KeyValuePair<string, object>(
                        nameof(IHasId.Id),
                        id)
                    )).FirstOrDefault();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<T> FindByIdAsync<T>(string userId) where T : class, IHasGId
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    throw new ArgumentNullException("userId");

                Guid parsedUserId;
                if (!Guid.TryParse(userId, out parsedUserId))
                    throw new ArgumentOutOfRangeException("userId", string.Format("'{0}' is not a valid GUID.", new { userId }));

                using (var c = GetConnection())
                {
                    return await c.QuerySingleOrDefaultAsync<T>($@"SELECT * FROM {typeof(T).GetTableName()}
                    WHERE [Id] = @{nameof(userId)}", new { userId = parsedUserId });
                }
            }
            catch (Exception e)
            {
                throw;
            }

        }

        public async Task<bool> Update<T>(T item) where T : class, IHasIdOnly//IHasId
        {
            using (var c = GetConnection())
                return await c.UpdateAsync(item);

        }
        public async Task<bool> UpdateAsync<T>(T item) where T : class, IHasGId
        {
            try
            {
                if (item == null)
                    throw new ArgumentNullException("item");

                using (var c = GetConnection())
                {
                    return await c.UpdateAsync(item);
                }
            }
            catch (Exception e)
            {
                throw;
            }

        }

        public async Task<bool> Delete<T>(long id) where T : class, IHasId
        {
            using (var c = GetConnection())
            {
                var sql = $"DELETE FROM {typeof(T).GetTableName()} WHERE Id=@Id";
                return (await c.ExecuteAsync(sql, param: new { _Id = id })) > 0;
            }
        }

        public async Task<bool> DeleteHasIdOnly<T>(long id) where T : class, IHasIdOnly
        {
            try
            {
                using (var c = GetConnection())
                {
                    var sql = $"DELETE FROM {typeof(T).GetTableName()} WHERE Id=@Id";
                    return (await c.ExecuteAsync(sql, param: new { Id = id })) > 0;
                }
            }
            catch (Exception e)
            {
                throw;
            }

        }

        public async Task<bool> Delete<T>(Guid id) where T : class, IHasId, IHasGId
        {
            using (var c = GetConnection())
            {
                var sql = $"DELETE FROM {typeof(T).GetTableName()} WHERE Id=@Id";
                return (await c.ExecuteAsync(sql, param: new { Id = id })) > 0;
            }
        }

        public async Task<bool> Delete<T>(T item) where T : class, IHasId
        {
            return await Delete<T>(item.Id);
        }

        public async Task<T> GetLatestAsync<T>(string propName, object value, bool includeArchived) where T : class, IHasIdSafeRecord
        {
            using (var connection = GetConnection())
            {
                var selectAllQuery = $"SELECT TOP 1 * FROM {typeof(T).GetTableName()} (NOLOCK)";
                var _params = new DynamicParameters();
                _params.Add(propName, value);
                selectAllQuery += " ORDER BY Id DESC";
                return await connection.QueryFirstOrDefaultAsync<T>(selectAllQuery, _params);
            }
        }

        public async Task<T> GetLatestAsyncLong<T>(string propName, object value, bool includeArchived) where T : class, IHasId
        {
            using (var connection = GetConnection())
            {
                var selectAllQuery = $"SELECT TOP 1 * FROM {typeof(T).GetTableName()} (NOLOCK)";
                var _params = new DynamicParameters();
                _params.Add(propName, value);
                selectAllQuery += " ORDER BY Id DESC";
                return await connection.QueryFirstOrDefaultAsync<T>(selectAllQuery, _params);
            }
            //return (await SearchByParametersAsync<T>(1, parameters)).FirstOrDefault();
        }


    }
}
