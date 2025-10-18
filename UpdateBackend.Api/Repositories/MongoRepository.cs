using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;
using UpdateBackend.Api.Models;

namespace UpdateBackend.Api.Repositories
{
    public class MongoRepository<T> : IRepository<T> where T : class
    {
        protected readonly IMongoCollection<T> _collection;

        public MongoRepository(IMongoDatabase database, string collectionName)
        {
            _collection = database.GetCollection<T>(collectionName);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return null;
            }
            var filter = Builders<T>.Filter.Eq("_id", objectId);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<T?> GetByFieldAsync(Expression<Func<T, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetByFilterAsync(Expression<Func<T, bool>> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<T> CreateAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
            return entity;
        }

        public async Task<T?> UpdateAsync(string id, T entity)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return null;
            }
            var filter = Builders<T>.Filter.Eq("_id", objectId);
            var result = await _collection.ReplaceOneAsync(filter, entity);
            return result.ModifiedCount > 0 ? entity : null;
        }

        public async Task<T?> UpdateFieldAsync(string id, string fieldName, object value)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return null;
            }
            var filter = Builders<T>.Filter.Eq("_id", objectId);
            var update = Builders<T>.Update.Set(fieldName, value);
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0 ? await GetByIdAsync(id) : null;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return false;
            }
            var filter = Builders<T>.Filter.Eq("_id", objectId);
            var result = await _collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<long> CountAsync()
        {
            return await _collection.CountDocumentsAsync(_ => true);
        }

        public async Task<long> CountAsync(Expression<Func<T, bool>> filter)
        {
            return await _collection.CountDocumentsAsync(filter);
        }
    }
}
