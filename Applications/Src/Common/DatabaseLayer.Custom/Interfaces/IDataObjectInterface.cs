using DatabaseLayer.Metadata;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatabaseLayer.Interfaces
{
    public interface IDataObjectInterface
    {
        TableMetadata Metadata { get; }
    }
    public interface IDataObjectInterface<T> : IDataObjectInterface
    {
        int Create(T obj);

        int CreateUpdate(T obj);

        void Delete(int id);

        List<T> Read(object filter);

        void Update(T obj);

        Task<int> CreateAsync(T obj);

        Task<int> CreateUpdateAsync(T obj);

        Task DeleteAsync(int id);

        Task<List<T>> ReadAsync(object filter);

        Task UpdateAsync(T obj);
    }
}
