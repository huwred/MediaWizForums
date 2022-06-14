using System;
using MediaWiz.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace MediaWiz.Core.Interfaces
{
    public interface IForumCacheService
    {
        ForumCacheItem GetPost(
            IPublishedContent item,
            string cacheKey,
            TimeSpan? timeout = null);
    }
}