using System.Collections.Generic;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace MediaWiz.Forums.Events
{
    public class ForumPostUnPublishEvent : INotificationHandler<ContentUnpublishedNotification>
    {
        private readonly AppCaches _appCaches;
        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentType;
        public ForumPostUnPublishEvent(AppCaches appCaches,IContentService contentService,IContentTypeService contentType)
        {
            _appCaches = appCaches;
            _contentService = contentService;
            _contentType = contentType;
        }
        public void Handle(ContentUnpublishedNotification notification)
        {
            List<string> invalidCacheList = new List<string>();

            foreach (var item in notification.UnpublishedEntities)
            {
                // is a forum post...
                if (item.ContentType.Alias.Equals("forumPost"))
                {
                    // get parent Forum.
                    invalidCacheList = AddParentForumCaches(item, invalidCacheList);
                }
            }

            // clear the cache for any forums that have had child pages published...
            foreach (var cache in invalidCacheList)
            {
                //Logger.Info<ForumCacheHandler>("Clearing Forum Info Cache: {0}",  cache);
                _appCaches.RuntimeCache.ClearByKey(cache);
            }
        }

        private List<string> AddParentForumCaches(IContent item, List<string> cacheList)
        {
            var parent = _contentService.GetParent(item);
            var forumType = _contentType.Get("forum");

            if (parent != null && forumType != null)
            {
                if (parent.ContentTypeId == forumType.Id || parent.ContentTypeId == forumType.Id)
                {
                    var cache = $"forum_{parent.Id}";
                    if (!cacheList.Contains(cache))
                        cacheList.Add(cache);

                    cacheList = AddParentForumCaches(parent, cacheList);
                }
            }

            return cacheList;
        }
    }
}