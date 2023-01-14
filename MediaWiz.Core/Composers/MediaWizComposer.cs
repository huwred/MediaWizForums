using MediaWiz.Core.Services;
using MediaWiz.Forums.Events;
using MediaWiz.Forums.Helpers;
using MediaWiz.Forums.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;



namespace MediaWiz.Forums.Composers
{
    public class MediaWizComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.ManifestFilters().Append<EmailValidationManifestFilter>();

            builder.Services.AddScoped<IBackofficeUserAccessor, BackofficeUserAccessor>();
            builder.Services.AddScoped<IForumCacheService, ForumCacheService>();
            builder.Services.AddScoped<IViewCounterService, ForumViewCounterService>();
            builder.Services.AddScoped<IForumMailService, ForumMailService>();

            builder.AddNotificationHandler<ContentUnpublishedNotification, ForumPostUnPublishEvent>();
            builder.AddNotificationHandler<ContentSavingNotification, ForumPostSavingEvent>();
            builder.AddNotificationHandler<ContentPublishedNotification, ForumPostPublishedEvent>();
            builder.AddNotificationHandler<MenuRenderingNotification, MemberTreeNotificationHandler>();

            builder.Services
                .AddOptions<ForumConfigOptions>()
                .Bind(builder.Config.GetSection(ForumConfigOptions.MediaWizOptions));
        }
    }
}