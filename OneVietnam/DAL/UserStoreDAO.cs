﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AspNet.Identity.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
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

        public Task<List<ApplicationUser>> AllUsersAsync()
        {
            return _users.Find(u => true).ToListAsync();
        }

        public async Task<List<ApplicationUser>> FindUsersByRoleAsync(IdentityRole role)
        {
            return await _users.Find(u => u.Roles.Contains(role.Name)).ToListAsync();
        }

        public Task AddPostAsync(ApplicationUser pUser, Post pPost)
        {
            pUser.AddPost(pPost);
            return Task.FromResult(0);
        }



        public Task UpdatePostAsync(ApplicationUser pUser, Post pPost)
        {
            pUser.UpdatePost(pPost);
            return Task.FromResult(0);
        }

        public Task DeletePostAsync(ApplicationUser pUser, Post pPost)
        {
            pUser.DeletePost(pPost);
            return Task.FromResult(0);
        }

        public List<Post> GetPostsAsync(ApplicationUser user)
        {
            return user.Posts;
        }

        public Task<List<ApplicationUser>> FindUserByPostIdAsync(string pPostId)
        {
            return  _users.Find(u => u.Posts.Any(t => t.Id == pPostId)).ToListAsync();
        }


        //DEMO
        public Task AddLocationAsync(ApplicationUser user, Location location)
        {
            user.AddLocation(location);
            return Task.FromResult(0);
        }

        public List<AddLocationViewModel> GetInfoForInitMap(List<ApplicationUser> userList)
        {
            List<AddLocationViewModel> infoForInitMap = new List<AddLocationViewModel>();
            AddLocationViewModel viewModel;
            foreach (ApplicationUser user in userList)
            {

                viewModel = new AddLocationViewModel(user.Location, user.Id, user.Gender, user.Posts);
                infoForInitMap.Add(viewModel);
            }

            return infoForInitMap;
        }

        public List<Location> GetLocationListAsync(List<ApplicationUser> userList)
        {
            return userList.Select(user => user.Location).ToList();
        }

        public List<List<Post>> GetPostListAsync(List<ApplicationUser> userList)
        {
            return userList.Select(user => user.Posts).ToList();
        }

        public async Task<List<ApplicationUser>> TextSearchByUserName(string query)
        {
            var filter = new BsonDocument {{"UserName", new BsonDocument {{"$regex", query}, {"$options", "i"}}}};

            var result = await _users.Find(filter).ToListAsync();
            return result;
        }
    }
}