using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MongoDB.Bson;
using MongoDB.Driver;
using Ninject.Activation;
using OneVietnam.Common;
using OneVietnam.DAL;
using OneVietnam.DTL;
using OneVietnam.Models;
using Tag = OneVietnam.DTL.Tag;

namespace OneVietnam.BLL
{
    public partial class PostManager : AbstractManager<Post>
    {

        public static PostManager Create(IdentityFactoryOptions<PostManager> options,
            IOwinContext context)
        {
            var manager =
                new PostManager(new PostStore(context.Get<ApplicationIdentityContext>().Posts));
            return manager;
        }   
           
        //OK
        public async Task<List<Post>> FindByUserId(string userId)
        {
            var filter = Builders<Post>.Filter.Eq("UserId", userId);
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
            return await Store.FindAllAsync(filter).ConfigureAwait(false);
        }

        public async Task<List<Post>> FindPostsByTypeAsync(BaseFilter baseFilter,int? type)
        {
            var builder = Builders<Post>.Filter;
            var filter = builder.Eq("PostType", type) & builder.Eq("LockedFlag", false) &
                         builder.Eq("DeletedFlag", false);
            var sort = Builders<Post>.Sort.Descending("CreatedDate");
            return await FindAllAsync(baseFilter, filter, sort).ConfigureAwait(false);
        }

        public async Task<List<Post>> FindPostByTypeAndUserIdAsync(BaseFilter baseFilter, string userId, int? type)
        {
            var builder = Builders<Post>.Filter;
            var filter = builder.Eq("PostType", type) & builder.Eq("LockedFlag", false) &
                         builder.Eq("DeletedFlag", false)& builder.Eq("UserId",userId);
            var sort = Builders<Post>.Sort.Descending("CreatedDate");
            return await FindAllAsync(baseFilter, filter, sort).ConfigureAwait(false);
        }

        public async Task<List<Post>> FindPostByTagsAsync(List<Tag> tags, string postId)
        {            
            var builder = Builders<Post>.Filter;
            var filter = builder.Ne(p => p.Id, postId);
            var lockFilter = builder.Eq("LockedFlag", false);
            var filterTag = builder.AnyEq("Tags",tags[0]);
            for (int i = 1; i < tags.Count; i++)
            {
                filterTag = filterTag | builder.AnyEq("Tags", tags[i]);
            }            
            filter = filter&lockFilter& filterTag;
            return await FindAllAsync(filter);            
        }
        //OK                                
        public async Task<List<BsonDocument>> FullTextSearch(string query, BaseFilter filter)
        {
            var sort = Builders<Post>.Sort.MetaTextScore("TextMatchScore").Ascending("CreatedDate");
            return await Store.FullTextSearch(query, filter, sort).ConfigureAwait(false);
        }
        public async Task<List<BsonDocument>> FullTextSearchAdminPosts(string query, BaseFilter filter)
        {
            var sort = Builders<Post>.Sort.MetaTextScore("TextMatchScore").Ascending("CreatedDate");
            return await Store.FullTextSearchAdminPost(query, filter, sort).ConfigureAwait(false);
        }
        public PostManager(PostStore store) : base(store)
        {
            Store = store;
        }

        public async Task<List<Post>> FindAllActiveAdminPostAsync()
        {
            var list = new List<Post>();
            var builder = Builders<Post>.Filter;
            var filter = builder.Eq("PostType", PostTypeEnum.AdminPost)&builder.Eq("DeletedFlag", false)& 
                            builder.Eq("LockedFlag", false)&builder.Eq("Status",true);
            var sort = Builders<Post>.Sort.Descending("CreatedDate");
            var baseFilter = new BaseFilter {IsNeedPaging = false};
            return await this.FindAllAsync(baseFilter,filter,sort);
        }
        public async Task<List<Illustration>> AzureUploadAsync(HttpFileCollectionBase pFiles, string pBlobContainerName)
        {
            try
            {
                int fileCount = pFiles.Count;
                if (fileCount > 0 && !string.IsNullOrEmpty(pFiles[0]?.FileName))
                {                    
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString"));                    
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();                    
                    CloudBlobContainer blobContainer = blobClient.GetContainerReference(pBlobContainerName);                    
                    await blobContainer.DeleteIfExistsAsync();                    
                    await blobContainer.CreateAsync();                    
                    await blobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });                    
                    for (int i = 0; i < fileCount; i++)
                    {                       
                        CloudBlockBlob blob = blobContainer.GetBlockBlobReference(GetRandomBlobName(pFiles[i].FileName));                        
                        await   blob.UploadFromStreamAsync(pFiles[i].InputStream);                         
                    }                    
                    var blobList = blobContainer.ListBlobs();
                    List<Illustration> illList = new List<Illustration>();
                    foreach (var blob in blobList)
                    {
                        Illustration newIll = new Illustration(blob.Uri.ToString());
                        illList.Add(newIll);
                    }
                    return illList;
                }
            }
            catch (Exception ex)
            {                
                return null;
            }
            return null;
        }
        public async Task AzureDeleteAsync(string name, string id)
        {
            
                Uri uri = new Uri(name);
                string filename = Path.GetFileName(uri.LocalPath);
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(id);

                var blob = blobContainer.GetBlockBlobReference(filename);
                await blob.DeleteIfExistsAsync();



           
        }
        private string GetRandomBlobName(string filename)
        {
            string ext = Path.GetExtension(filename);
            return string.Format("{0:10}_{1}{2}", DateTime.Now.Ticks, Guid.NewGuid(), ext);
        }

        public async Task<List<Tag>> AddAndGetAddedTags(HttpRequestBase pRequestBase, TagManager pTagManager, string pFormId)
        {
            if (pRequestBase.Form.Count > 0)
            {
                var addedTagValueList = pRequestBase.Form[pFormId];
                if (!string.IsNullOrEmpty(addedTagValueList))
                {
                    List<Tag> newList = new List<Tag>();
                    var tagsInDb = await pTagManager.GetTagsValueAsync();
                    int numberTags = 0;
                    if (tagsInDb != null)
                    {
                        numberTags = tagsInDb.Count;
                    }

                    foreach (var tag in addedTagValueList.Split(','))
                    {
                        if (tagsInDb == null | (tagsInDb != null && !tagsInDb.Contains(tag)))
                        {
                            Tag newTag = new Tag(string.Concat("Tag_", numberTags.ToString()), tag);
                            await pTagManager.CreateAsync(newTag);
                            numberTags = numberTags + 1;
                            newList.Add(newTag);
                        }
                        else
                        {
                            var existTag = await pTagManager.FindTagByValueAsync(tag);
                            newList.Add(existTag[0]);
                        }
                    }
                    return newList;
                }
                return null;
            }
            return null;
        }

        public async Task<List<Post>> FindAllDescenderAsync(BaseFilter basefilter)
        {
            var builder = Builders<Post>.Filter;
            var filter = builder.Eq("DeletedFlag", false) & builder.Eq("LockedFlag",false);
            var sort = Builders<Post>.Sort.Descending("CreatedDate");
            return await Store.FindAllAsync(basefilter, filter, sort);
        }

        public async Task<List<Post>> FindAllDescenderByIdAsync(BaseFilter baseFilter, string id)
        {
            var builder = Builders<Post>.Filter;
            var filter = builder.Eq("DeletedFlag", false) & builder.Eq("LockedFlag", false) & builder.Eq("UserId",id);
            var sort = Builders<Post>.Sort.Descending("CreatedDate");
            return await Store.FindAllAsync(baseFilter, filter, sort);
        }
        public async Task<List<Post>> SearchPostMultipleQuery(string title, DateTimeOffset? createdDateFrom, DateTimeOffset? createdDateTo, bool? status)
        {
            var builder = Builders<Post>.Filter;
            FilterDefinition<Post> filter = null;
            if (!string.IsNullOrWhiteSpace(title))
            {
                var textFilter = builder.Regex("Title", new BsonRegularExpression(title, "i"));
                filter = textFilter;
            }
            if (createdDateFrom != null)
            {
                var dateFrpm = builder.Gte("CreatedDate", createdDateFrom);
                if (filter == null)
                {
                    filter = dateFrpm;
                }
                else
                {
                    filter = filter & dateFrpm;
                }
            }

            if (createdDateTo != null)
            {
                var dateTo = builder.Lt("CreatedDate", createdDateTo);
                if (filter == null)
                {
                    filter = dateTo;
                }
                else
                {
                    filter = filter & dateTo;
                }
            }

            if (status != null)
            {
                var statusFilter = builder.Eq("Status", status);
                if (filter == null)
                {
                    filter = statusFilter;
                }
                else
                {
                    filter = filter & statusFilter;
                }
            }
            var adminPostFilter = builder.Eq("PostType", PostTypeEnum.AdminPost);
            if (filter == null)
            {
                filter = adminPostFilter;
            }
            else
            {
                filter = filter & adminPostFilter;
            }
            var baseFilter = new BaseFilter {IsNeedPaging = false};
            var sort = Builders<Post>.Sort.Descending("CreatedDate");
            return await Store.FindAllAsync(baseFilter,filter,sort);
        }

        public async Task<int> FindNumberOfPost(string id)
        {
            var builder = Builders<Post>.Filter;
            var filter = builder.Eq("DeletedFlag", false) & builder.Eq("LockedFlag", false) & builder.Eq("UserId", id);            
            var listPost = await Store.FindAllAsync(filter).ConfigureAwait(false);
            return listPost.Count;
        }

    }
}