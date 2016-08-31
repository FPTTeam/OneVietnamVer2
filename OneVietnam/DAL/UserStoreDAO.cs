using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AspNet.Identity.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using OneVietnam.DTL;
using OneVietnam.Models;

namespace OneVietnam.DAL
{
    public class UserStore : UserStore<ApplicationUser>
    {
        private readonly IMongoCollection<ApplicationUser> _users;
        public UserStore(IMongoCollection<ApplicationUser> users) : base(users)
        {
            _users = users;
        }

        public async Task<List<ApplicationUser>> AllUsersAsync()
        {
            return await _users.Find(u => true).ToListAsync();
        }

        public async Task<List<ApplicationUser>> FindUsersByRoleAsync(IdentityRole role)
        {
            return await _users.Find(u => u.Roles.Contains(role.Name)).ToListAsync();
        }

        public async Task<List<ApplicationUser>> TextSearchUsers(string query)
        {
            var builder = Builders<ApplicationUser>.Filter;
            var filter = builder.Eq("LockedFlag", false) & builder.Eq("DeletedFlag", false) &
                         (builder.Regex("UserName", new BsonRegularExpression(query, "i")) | builder.Regex("Email", new BsonRegularExpression(query, "i")));
            return await _users.Find(filter).ToListAsync();
        }

        public async Task<List<ApplicationUser>> TextSearchUsers(BaseFilter baseFilter, string query)
        {
            if (baseFilter.IsNeedPaging)
            {
                var builder = Builders<ApplicationUser>.Filter;
                var filter = builder.Eq("LockedFlag", false) & builder.Eq("DeletedFlag", false) &
                             (builder.Regex("UserName", new BsonRegularExpression(query, "i")) |
                              builder.Regex("Email", new BsonRegularExpression(query, "i")));
                return await _users.Find(filter).Skip(baseFilter.Skip).Limit(baseFilter.Limit).ToListAsync().ConfigureAwait(false);
            }
            else
            {
                return await TextSearchUsers(query);
            }
        }

        //public async Task<List<ApplicationUser>> TextSearchUsers(string query, BaseFilter baseFilter)
        //{
        //    if (baseFilter.IsNeedPaging)
        //    {
        //        var builder = Builders<ApplicationUser>.Filter;
        //        var filter = builder.Regex("UserName", new BsonRegularExpression(query, "i")) | builder.Regex("Email", new BsonRegularExpression(query, "i"));
        //        return
        //            await
        //                _users.Find(filter)
        //                    .Skip(baseFilter.Skip)
        //                    .Limit(baseFilter.Limit)
        //                    .ToListAsync()
        //                    .ConfigureAwait(false);
        //    }
        //    else
        //    {
        //        return await TextSearchUsers(query).ConfigureAwait(false);
        //    }
        //}

        public async Task<List<ApplicationUser>> TextSearchMultipleQuery(FilterDefinition<ApplicationUser> filter)
        {
            if (filter != null)
            {
                return
                    await
                        _users.Find(filter)
                            .ToListAsync()
                            .ConfigureAwait(false);
            }
            else
            {
                return await _users.Find(u => true).ToListAsync();
            }

        }
        public async Task UpdateOneByFilterAsync(FilterDefinition<ApplicationUser> filter, UpdateDefinition<ApplicationUser> update)
        {
            try
            {
                await _users.UpdateOneAsync(filter, update);
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

        public async Task PushAdminNotificationToAllUsersAsync(Notification notification)
        {
            var filter = new BsonDocument();
            using (var cursor = await _users.FindAsync(filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        if (document.Notifications == null)
                        {
                            document.Notifications = new SortedList<string, Notification>();
                        }
                        document.Notifications.Add(notification.Id,notification);                        
                        await UpdateAsync(document);
                    }
                }
            }
        }
    }
}