using Core;
using DatabaseLayer.Dapper.QueryModel;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Dapper.ImplementationInterfaces
{
    public interface IMongoDbRepository : IDapperRepository
    {
        Task<T> GetOneByFilter<T>(params KeyValuePair<string, object>[] parameters) where T : class, IHasId;
    }
    public class MongoDbRepository : IMongoDbRepository
    {
        private readonly IMongoDatabase _database;
        private readonly string _connectionString;

        public MongoDbRepository()
        {
            _connectionString = RepositoryDependencies.MongoConnectionString;

            var client = new MongoClient(_connectionString);
            _database = client.GetDatabase(RepositoryDependencies.Database);
        }
        public async Task<T> Add<T>(T item) where T : class, IHasIdOnly//IHasId
        {
            var collction = _database.GetCollection<T>(typeof(T).Name);
            var count = collction.CountDocuments(new FilterDefinitionBuilder<T>().Empty);
            item.Id = count + 1;
            await _database.GetCollection<T>(typeof(T).Name).InsertOneAsync(item);
            return item;
        }

        public async Task<IEnumerable<T>> AddMany<T>(IEnumerable<T> items) where T : class, IHasId
        {
            var collction = _database.GetCollection<T>(typeof(T).Name);
            var count = collction.CountDocuments(new FilterDefinitionBuilder<T>().Empty);
            foreach (var item in items)
            {
                count++;
                item.Id = count;
            }
            await collction.InsertManyAsync(items);
            return items;
        }

        public async Task<bool> Archive<T>(T item) where T : class, IHasIdSafeRecord
        {
            item.Archived = true;
            item.Updated = DateTime.Now;
            return await Update(item);
        }

        public async Task<IEnumerable<T>> GetAll<T>() where T : class, IHasIdOnly//IHasId
        {
            var collection = _database.GetCollection<T>(typeof(T).Name);
            var items = await collection.FindAsync(Builders<T>.Filter.Empty);
            return items.ToList();
        }

        public async Task<IEnumerable<T>> GetByFilter<T>(params KeyValuePair<string, object>[] parameters) where T : class, IHasIdOnly//IHasId
        {
            var filter = new FilterDefinitionBuilder<BsonDocument>();
            if (parameters != null && parameters.Any())
            {
                filter.And(filter.Eq(parameters[0].Key, parameters[0].Value));
                foreach (var f in parameters.Skip(1))
                {
                    filter.And(filter.Eq(f.Key, f.Value));
                }
            }

            var collection = _database.GetCollection<T>(typeof(T).Name);
            var items = await collection.FindAsync<T>(filter.ToBsonDocument());
            return items.ToList();
        }

        public async Task<T> GetOneByFilter<T>(params KeyValuePair<string, object>[] parameters) where T : class, IHasId
        {
            var filter = new FilterDefinitionBuilder<BsonDocument>();
            if (parameters != null && parameters.Any())
            {
                filter.And(filter.Eq(parameters[0].Key, parameters[0].Value));
                foreach (var f in parameters.Skip(1))
                {
                    filter.And(filter.Eq(f.Key, f.Value));
                }
            }

            var collection = _database.GetCollection<T>(typeof(T).Name);
            var items = (await collection.FindAsync<T>(filter.ToBsonDocument())).FirstOrDefault();
            return items;
        }

        public async Task<T> GetById<T>(long? id) where T : class, IHasIdOnly//IHasId
        {
            if (id == null)
                return default;
            var collection = _database.GetCollection<T>(typeof(T).Name);
            var item = await collection.FindAsync(Builders<T>.Filter.Eq("id", id));
            return item.FirstOrDefault();
        }

        public async Task<T> GetById<T>(Guid id) where T : class, IHasId
        {
            var collection = _database.GetCollection<T>(typeof(T).Name);
            var item = await collection.FindAsync(Builders<T>.Filter.Eq("id", id));
            return item.FirstOrDefault();
        }

        public async Task<bool> Update<T>(T item) where T : class, IHasIdOnly//IHasId
        {
            var collection = _database.GetCollection<T>(typeof(T).Name);
            var result = await collection.ReplaceOneAsync(Builders<T>.Filter.Eq("id", item.Id), item);
            return result.IsAcknowledged;
        }

        public async Task<bool> Delete<T>(long id) where T : class, IHasId
        {
            var collection = _database.GetCollection<T>(typeof(T).Name);
            return (await collection.DeleteOneAsync(Builders<T>.Filter.Eq("id", id))).IsAcknowledged;
            // TODO: Test whether this works as expected & this methods return result..
            //return true;
        }

        public async Task<bool> Delete<T>(Guid id) where T : class, IHasGId
        {
            var collection = _database.GetCollection<T>(typeof(T).Name);
            return (await collection.DeleteOneAsync(Builders<T>.Filter.Eq("id", id))).IsAcknowledged;
            // TODO: Test whether this works as expected & this methods return result..
            //return true;
        }

        public async Task<bool> DeleteHasIdOnly<T>(long id) where T : class, IHasIdOnly
        {
            var collection = _database.GetCollection<T>(typeof(T).Name);
            return (await collection.DeleteOneAsync(Builders<T>.Filter.Eq("id", id))).IsAcknowledged;
            // TODO: Test whether this works as expected & this methods return result..
            //return true;
        }

        public async Task<bool> Delete<T>(T item) where T : class, IHasId
        {
            return await Delete<T>(item.Id);
        }

        Task<IEnumerable<T>> IDapperRepository.SearchByParametersAsync<T>(int? count, params QueryPropertyPair[] parameters)
        {
            throw new NotImplementedException();
        }

        Task<T> IDapperRepository.GetLatestAsync<T>(string propName, object value, bool includeArchived)
        {
            throw new NotImplementedException();
        }

        Task<bool> IDapperRepository.UpdateAsync<T>(T item)
        {
            throw new NotImplementedException();
        }

        Task<T> IDapperRepository.FindByIdAsync<T>(string userId)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<T>> IDapperRepository.GetByFilt<T>(params KeyValuePair<string, object>[] parameters)
        {
            throw new NotImplementedException();
        }

        Task<T> IDapperRepository.AddAsync<T>(T item)
        {
            throw new NotImplementedException();
        }

        Task<T> IDapperRepository.GetLatestAsyncLong<T>(string propName, object value, bool includeArchived)
        {
            throw new NotImplementedException();
        }

        public Task<T> ExecuteScalar<T>(string sql, object parameters)
        {
            throw new NotImplementedException();
        }

        public Task<T> ExecuteInsert<T>(DapperHelper helper)
        {
            throw new NotImplementedException();
        }

        Task<T> IDapperRepository.GetLatestRecordAsync<T>()
        {
            throw new NotImplementedException();
        }
    }
}
