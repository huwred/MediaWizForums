using System;
using System.Linq;
using MediaWiz.Core.Interfaces;
using MediaWiz.Core.Models;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace MediaWiz.Core.Services
{
    public class ForumCacheService : IForumCacheService
    {
        private readonly IAppPolicyCache _runtimeCache;

        public ForumCacheService(AppCaches appCaches)
        {
            // Grap RuntimeCache from appCaches
            // and assign to our private field.
            _runtimeCache = appCaches.RuntimeCache;
        }
        public ForumCacheItem GetPost(IPublishedContent item, string cacheKey, TimeSpan? timeout = null)
        {
            // GetCacheItem will automatically insert the object
            // into cache if it doesn't exist.
            return _runtimeCache.GetCacheItem(cacheKey, () => GetPostInfo(item,_runtimeCache,cacheKey), timeout);
        }

        public static ForumCacheItem GetPostInfo(IPublishedContent item,IAppPolicyCache cache,string cacheKey = "")
        {

            var forumInfo = new ForumCacheItem();

            var posts = item.Descendants().Where(x => x.IsVisible() && x.IsDocumentType("forumPost")).ToList();

            forumInfo.Count = posts.Count();
            forumInfo.TopicCount = posts.Count(x=> x.Value<bool>("postType"));
            forumInfo.ReplyCount = posts.Count(x=> !x.Value<bool>("postType"));
            
            if (posts.Any())
            {
                var pagesize = Convert.ToInt32(item.Value("intPageSize"));
                var totalPages = (int)Math.Ceiling((double)posts.Count() / (double)pagesize);

                var lastPost = posts.OrderByDescending(x => x.CreateDate).FirstOrDefault();
                if (lastPost != null) forumInfo.latestPost = lastPost.CreateDate;
                if (lastPost != null) forumInfo.Id = Convert.ToInt32(lastPost.Id);
                if (lastPost != null) forumInfo.lastPostUrl = lastPost.Url();
                forumInfo.lastpostAuthor = lastPost.Value<string>(("postCreator"));
                var author = lastPost.Value<IPublishedContent>("postAuthor");
                if (author != null)
                {
                    forumInfo.lastpostAuthorId = lastPost.Value<IPublishedContent>("postAuthor").Id;
                }
                else
                {
                    forumInfo.lastpostAuthorId = -1;
                }
                

                for (int page = 1; page <= totalPages; page++)
                {
                    var post = posts.Skip((page-1) * pagesize).Take(pagesize)
                        .OrderByDescending(x => x.Value<DateTime>("createDate"));
                    if (post.Any(x => x.UrlSegment.Equals(lastPost.UrlSegment, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        forumInfo.Page = page;
                        break;
                    }
                }
            }

            return forumInfo;
        }
 
    }
}
