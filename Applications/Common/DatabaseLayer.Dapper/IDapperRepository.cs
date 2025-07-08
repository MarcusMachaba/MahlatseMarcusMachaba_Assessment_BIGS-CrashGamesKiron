using Core;
using Dapper;
using Dapper.Contrib.Extensions;
using DatabaseLayer.Dapper.QueryModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Dapper
{
    public interface IDapperRepository
    {
        Task<T> ExecuteScalar<T>(string sql, object parameters);
        Task<T> ExecuteInsert<T>(DapperHelper helper);
        Task<T> Add<T>(T item) where T : class, IHasIdOnly; //IHasId,
        Task<T> AddAsync<T>(T item) where T : class, IHasGId;
        Task<bool> Update<T>(T item) where T : class, IHasIdOnly; //IHasId,
        Task<bool> UpdateAsync<T>(T item) where T : class, IHasGId;
        Task<bool> Archive<T>(T item) where T : class, IHasIdSafeRecord;
        Task<bool> Delete<T>(long id) where T : class, IHasId;
        Task<bool> DeleteHasIdOnly<T>(long id) where T : class, IHasIdOnly;
        Task<bool> Delete<T>(T item) where T : class, IHasId;
        Task<IEnumerable<T>> GetAll<T>() where T : class, IHasIdOnly;//IHasId;
        Task<IEnumerable<T>> GetByFilter<T>(params KeyValuePair<string, object>[] parameters) where T : class, IHasIdOnly;//IHasId;
        Task<IEnumerable<T>> GetByFilt<T>(params KeyValuePair<string, object>[] parameters) where T : class, IHasGId;
        Task<T> GetById<T>(long? id) where T : class, IHasIdOnly;//IHasId;
        Task<T> FindByIdAsync<T>(string userId) where T : class, IHasGId;

        Task<IEnumerable<T>> SearchByParametersAsync<T>(int? count, params QueryPropertyPair[] parameters) where T : class, IHasId;
        Task<T> GetLatestAsync<T>(string propName, object value, bool includeArchived) where T : class, IHasIdSafeRecord;
        Task<T> GetLatestAsyncLong<T>(string propName, object value, bool includeArchived) where T : class, IHasId;
        Task<T> GetLatestRecordAsync<T>() where T : class, IHasIdOnly;
    }
}
