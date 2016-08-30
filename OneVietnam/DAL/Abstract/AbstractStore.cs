using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MongoDB.Bson;
using OneVietnam.Common;

namespace OneVietnam.DAL
{
    public abstract class AbstractStore<T> : IDisposable
    {
        protected internal IMongoCollection<T> Collection { get; set; }

        protected AbstractStore(IMongoCollection<T> collection)
        {
            Collection = collection;
        }
        
        public virtual async Task CreateAsync(T instance)
        {
            try
            {
                await Collection.InsertOneAsync(instance).ConfigureAwait(false);
            }
            catch (MongoConnectionException ex)
            {
                throw new MongoConnectionException(ex.ConnectionId, ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public abstract Task UpdateAsync(T instance);
        public abstract Task UpdateAsync(T instance,bool upsert);
        /// <summary>
        /// Find instance by ObjectId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<T> FindByIdAsync(ObjectId id)
        {
            try
            {
                var builder = Builders<T>.Filter;
                var filter = builder.Eq("_id", id);
                return await Collection.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
            }
            catch (MongoConnectionException ex)
            {
                throw new MongoConnectionException(ex.ConnectionId, ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// Find instance by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<T> FindByIdAsync(string id)
        {
            try
            {
                return await FindByIdAsync(new ObjectId(id)).ConfigureAwait(false);
            }
            catch (MongoConnectionException ex)
            {
                throw new MongoConnectionException(ex.ConnectionId, ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }        
        /// <summary>
        /// delete an instance by ObjectId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task DeleteByIdAsync(ObjectId id)
        {
            try
            {
                var builder = Builders<T>.Filter;
                var filter = builder.Eq("_id", id);
                await Collection.DeleteOneAsync(filter).ConfigureAwait(false);
            }
            catch (MongoConnectionException ex)
            {
                throw new MongoConnectionException(ex.ConnectionId, ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// Delete an instance by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task DeleteByIdAsync(string id)
        {
            try
            {
                await DeleteByIdAsync(new ObjectId(id)).ConfigureAwait(false);
            }
            catch (MongoConnectionException ex)
            {
                throw new MongoConnectionException(ex.ConnectionId, ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// Fill all instance
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<T>> FindAllAsync()
        {
            try
            {
                return await Collection.Find(t => true).ToListAsync().ConfigureAwait(false);
            }
            catch (MongoConnectionException ex)
            {
                throw new MongoConnectionException(ex.ConnectionId, ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// Find all intances by deleted flag
        /// </summary>
        /// <param name="deletedFlag"></param>
        /// <returns></returns>
        public virtual async Task<List<T>> FindAllAsync(bool deletedFlag)
        {
            try
            {
                var filter = Builders<T>.Filter.Eq("DeletedFlag", deletedFlag);
                return await Collection.Find(filter).ToListAsync().ConfigureAwait(false);
            }
            catch (MongoConnectionException ex)
            {
                throw new MongoConnectionException(ex.ConnectionId, ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// find all instance by filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual async Task<List<T>> FindAllAsync(FilterDefinition<T> filter)
        {
            try
            {
                return await Collection.Find(filter).ToListAsync().ConfigureAwait(false);
            }
            catch (MongoConnectionException ex)
            {
                throw new MongoConnectionException(ex.ConnectionId, ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public virtual async Task<List<T>> FindAllAsync(FilterDefinition<T> filter, SortDefinition<T> sort )
        {
            try
            {
                return await Collection.Find(filter).Sort(sort).ToListAsync().ConfigureAwait(false);
            }
            catch (MongoConnectionException ex)
            {
                throw new MongoConnectionException(ex.ConnectionId, ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public virtual async Task UpdateOneByFilterAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            try
            {
                await Collection.UpdateOneAsync(filter, update);
            }
            catch (MongoConnectionException ex)
            {
                throw new MongoConnectionException(ex.ConnectionId, ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }            
        }
        /// <summary>
        /// find all instances by basefilter and deletedflag
        /// </summary>
        /// <param name="baseFilter"></param>
        /// <param name="deletedFlag"></param>
        /// <returns></returns>
        public virtual async Task<List<T>> FindAllAsync(BaseFilter baseFilter, bool deletedFlag)
        {
            try
            {
                var filter = Builders<T>.Filter.Eq("DeletedFlag", deletedFlag);
                if (baseFilter.IsNeedPaging)
                {
                    return await
                        Collection.Find(filter)
                            .Skip(baseFilter.Skip)
                            .Limit(baseFilter.Limit)
                            .ToListAsync()
                            .ConfigureAwait(false);
                }
                else
                {
                    return await FindAllAsync(deletedFlag).ConfigureAwait(false);
                }
            }
            catch (MongoConnectionException ex)
            {
                throw new MongoConnectionException(ex.ConnectionId, ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        /// <summary>
        /// file all instance by basefilter and filter
        /// </summary>
        /// <param name="baseFilter"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual async Task<List<T>> FindAllAsync(BaseFilter baseFilter, FilterDefinition<T> filter)
        {
            try
            {

                if (baseFilter.IsNeedPaging)
                {
                    return await
                        Collection.Find(filter)
                            .Skip(baseFilter.Skip)
                            .Limit(baseFilter.Limit)
                            .ToListAsync()
                            .ConfigureAwait(false);
                }
                else
                {
                    return await FindAllAsync(filter).ConfigureAwait(false);
                }
            }
            catch (MongoConnectionException ex)
            {
                throw new MongoConnectionException(ex.ConnectionId, ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// Find all item async by basefilter, filter and sortable
        /// </summary>
        /// <param name="baseFilter"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public virtual async Task<List<T>> FindAllAsync(BaseFilter baseFilter, FilterDefinition<T> filter, SortDefinition<T> sort )
        {
            try
            {

                if (baseFilter.IsNeedPaging)
                {
                    return await
                        Collection.Find(filter)
                            .Skip(baseFilter.Skip)
                            .Limit(baseFilter.Limit)
                            .Sort(sort)
                            .ToListAsync()
                            .ConfigureAwait(false);
                }
                else
                {
                    return await FindAllAsync(filter,sort).ConfigureAwait(false);
                }
            }
            catch (MongoConnectionException ex)
            {
                throw new MongoConnectionException(ex.ConnectionId, ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }        
        public virtual async Task<List<T>> FindAllAsync(BaseFilter baseFilter)
        {
            
            if (baseFilter.IsNeedPaging)
            {
                return await FindAllAsync(baseFilter,false).ConfigureAwait(false);
            }
            else
            {
                return await FindAllAsync(false).ConfigureAwait(false);
            }
        }

        public virtual async Task<List<BsonDocument>> FullTextSearch(string query, BaseFilter filter,
            SortDefinition<T> sort)
        {
            var builder = Builders<T>.Filter;
            var conFilter = builder.Text(query) & builder.Eq("DeletedFlag", false) & builder.Eq("LockedFlag",false);
            var project = Builders<T>.Projection.MetaTextScore("TextMatchScore");
            if (filter.IsNeedPaging)
            {
                return
                    await
                        Collection.Find(conFilter)
                            .Skip(filter.Skip)
                            .Limit(filter.Limit)
                            .Project(project)
                            .Sort(sort)
                            .ToListAsync().ConfigureAwait(false);
            }
            else
            {
                return await Collection.Find(conFilter).Project(project).Sort(sort).ToListAsync().ConfigureAwait(false);
            }
        }
        public async Task<List<BsonDocument>> FullTextSearchAdminPost(string query, BaseFilter filter,
            SortDefinition<T> sort)
        {
            var builder = Builders<T>.Filter;
            var conFilter = builder.Text(query) & builder.Eq("DeletedFlag", false) & builder.Eq("LockedFlag", false) & builder.Eq("PostType", (int)PostTypeEnum.AdminPost);
            var project = Builders<T>.Projection.MetaTextScore("TextMatchScore");
            if (filter.IsNeedPaging)
            {
                return
                    await
                        Collection.Find(conFilter)
                            .Skip(filter.Skip)
                            .Limit(filter.Limit)
                            .Project(project)
                            .Sort(sort)
                            .ToListAsync().ConfigureAwait(false);
            }
            else
            {
                return await Collection.Find(conFilter).Project(project).Sort(sort).ToListAsync().ConfigureAwait(false);
            }
        }
        public void Dispose()
        {            
        }
    }
}