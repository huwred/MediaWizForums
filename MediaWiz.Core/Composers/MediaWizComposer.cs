using MediaWiz.Core.Events;
using MediaWiz.Core.Helpers;
using MediaWiz.Core.Interfaces;
using MediaWiz.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace MediaWiz.Core.Composers
{
    public class MediaWizComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddScoped<IBackofficeUserAccessor, BackofficeUserAccessor>();
            builder.Services.AddScoped<IForumCacheService, ForumCacheService>();
            builder.Services.AddScoped<IViewCounterService, ForumViewCounterService>();
            builder.Services.AddScoped<IForumMailService, ForumMailService>();
            //builder.Services.AddOutputCaching();
            builder.AddNotificationHandler<ContentUnpublishedNotification, ForumPostUnPublishEvent>();
            builder.AddNotificationHandler<ContentSavingNotification, ForumPostSavingEvent>();
            builder.AddNotificationHandler<ContentPublishedNotification, ForumPostPublishedEvent>();
        }
    }
}