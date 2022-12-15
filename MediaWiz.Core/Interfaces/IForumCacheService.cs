using System;
using MediaWiz.Forums.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace MediaWiz.Forums.Interfaces
{
    public interface IForumCacheService
    {
        ForumCacheItem GetPost(
            IPublishedContent item,
            string cacheKey,
            TimeSpan? timeout = null);
    }
}