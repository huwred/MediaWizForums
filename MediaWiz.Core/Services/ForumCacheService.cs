using System;
using System.Linq;
using MediaWiz.Forums.Interfaces;
using MediaWiz.Forums.Models;
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
            _runtimeCache = appCaches.RuntimeCache;
        }
        public ForumCacheItem GetPost(IPublishedContent item, string cacheKey, TimeSpan? timeout = null)
        {
            // GetCacheItem will automatically insert the object
            // into cache if it doesn't exist.
            var post = _runtimeCache.GetCacheItem(cacheKey, () => GetPostInfo(item,_runtimeCache,cacheKey), timeout);
            if (post != null && post.Id == 0)
            {
                //clearing the cachekey doesn't remove the cache item!! just sets it as blank so re-add if the id is 0
                _runtimeCache.InsertCacheItem<ForumCacheItem>(cacheKey,() => GetPostInfo(item,_runtimeCache,cacheKey), timeout);
                return _runtimeCache.GetCacheItem(cacheKey, () => GetPostInfo(item,_runtimeCache,cacheKey), timeout);
            }
            return post;
        }
        public TopicCacheItem GetLatestPosts(IPublishedContent item, TimeSpan? timeout = null)
        {
            // GetCacheItem will automatically insert the object
            // into cache if it doesn't exist.
            return _runtimeCache.GetCacheItem("LatestPosts", () => GetLatestPostInfo(item,_runtimeCache,"LatestPosts"), timeout);
        }
        public ForumCacheItem GetPostInfo(IPublishedContent item,IAppPolicyCache cache,string cacheKey = "")
        {
            var forumInfo = new ForumCacheItem();
            var posts = item.Children.Where(x => x.IsVisible() && x.IsDocumentType("forumPost")).ToList();

            forumInfo.Count = posts.Count();
            if (item.ContentType.Alias == "forum")
            {
                forumInfo.TopicCount = posts.Count(x=> x.Value<bool>("postType"));
                foreach (var post in posts)
                {
                    forumInfo.Count += post.Children.Count();
                }
            }
            else
            {
                forumInfo.TopicCount = posts.Count(x=> x.Value<bool>("postType"));
                forumInfo.ReplyCount = posts.Count(x=> !x.Value<bool>("postType"));
            }

            
            if (posts.Any())
            {
                var pagesize = Convert.ToInt32(item.Value("intPageSize"));
                var totalPages = (int)Math.Ceiling((double)posts.Count / (double)pagesize);
                var lastPost = posts.MaxBy(x => x.CreateDate);
                if (lastPost != null)
                {
                    forumInfo.latestPost = lastPost.CreateDate;
                    forumInfo.Id = Convert.ToInt32(lastPost.Id);
                    forumInfo.lastPostUrl = lastPost.Url();
                    forumInfo.lastpostAuthor = lastPost.Value<string>(("postCreator"));
                    var author = lastPost.Value<IPublishedContent>("postAuthor");
                    if (author != null)
                    {
                        forumInfo.lastpostAuthorId = lastPost.Value<IPublishedContent>("postAuthor").Id;
                    }
                }
                else
                {
                    forumInfo.lastpostAuthorId = -1;
                }

                //find the correct page to display the post
                for (int page = 1; page <= totalPages; page++)
                {
                    var post = posts.OrderByDescending(x => x.Value<DateTime>("createDate"))
                        .Skip((page-1) * pagesize)
                        .Take(pagesize);
                    if (post.Any(x => x.UrlSegment.Equals(lastPost?.UrlSegment, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        forumInfo.Page = page;
                        break;
                    }
                }
            }

            return forumInfo;
        }

        public TopicCacheItem GetLatestPostInfo(IPublishedContent item,IAppPolicyCache cache,string cacheKey = "")
        {
            
            var topicInfo = new TopicCacheItem();

            var posts = item.Children.Where(x => x.IsVisible() && x.IsDocumentType("forumPost")).ToList();

            topicInfo.ReplyCount = posts.Count(x=> !x.Value<bool>("postType"));
            
            if (posts.Any())
            {
                var lastPost = posts.OrderByDescending(x => x.CreateDate).FirstOrDefault();
                if (lastPost != null) topicInfo.latestPost = lastPost.CreateDate;
                //if (lastPost != null) forumInfo.Id = Convert.ToInt32(lastPost.Id);
                if (lastPost != null) topicInfo.lastPostUrl = lastPost.Url();
                topicInfo.lastpostAuthor = lastPost.Value<string>(("postCreator"));
                var author = lastPost.Value<IPublishedContent>("postAuthor");
                if (author != null)
                {
                    topicInfo.lastpostAuthorId = lastPost.Value<IPublishedContent>("postAuthor").Id;
                }
                else
                {
                    topicInfo.lastpostAuthorId = -1;
                }
                
            }

            return topicInfo;
        }
    }
}
