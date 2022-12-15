using System;
using MediaWiz.Forums.Interfaces;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;

namespace MediaWiz.Forums.Events
{
    public class ForumPostSavingEvent : INotificationHandler<ContentSavingNotification>
    {
        private readonly IBackofficeUserAccessor _backofficeUserAccessor;
        private readonly IMemberManager _memberManager;
        private readonly ILogger<ForumPostSavingEvent> _logger;
        public ForumPostSavingEvent(IBackofficeUserAccessor backofficeUserAccessor, IMemberManager memberManager,ILogger<ForumPostSavingEvent> logger)
        {
            _backofficeUserAccessor = backofficeUserAccessor;
            _memberManager = memberManager;
            _logger = logger;
        }
        public void Handle(ContentSavingNotification notification)
        {
            foreach (var node in notification.SavedEntities)
            {
                if ( node.ContentType.Alias.Equals("forumPost"))
                {
                    var currentUser = _memberManager.GetCurrentMemberAsync().Result;
                    
                    var backofficeUser = _backofficeUserAccessor.BackofficeUser;
                    _logger.LogInformation("Dirty? {dirtyProperties}", String.Join(',',node.GetDirtyProperties()));

                    if (currentUser == null && backofficeUser != null && backofficeUser.IsAuthenticated)
                    {
                        //if (node.IsPropertyDirty("postBody"))
                        //{
                        //    notification.CancelOperation(new EventMessage("Forum infringement",
                        //        "Forum postBody can not be edited in the back office!",
                        //        EventMessageType.Error));
                        //}
                    }
                }
            }
        }

    }
}