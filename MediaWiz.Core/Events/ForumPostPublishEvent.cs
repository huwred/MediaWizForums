using System;
using System.Collections.Generic;
using System.Linq;
using MediaWiz.Forums.Interfaces;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace MediaWiz.Forums.Events
{
    /// <summary>
    /// Post published event for Forum Posts,
    /// Updates members post count, clears caches and Handles Email notifications
    /// </summary>
    public class ForumPostPublishedEvent : INotificationHandler<ContentPublishedNotification>
    {
        private readonly AppCaches _appCaches;
        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentType;
        private readonly ILogger<ForumPostPublishedEvent> _logger;
        private readonly IUmbracoContextFactory  _context;
        private readonly IBackofficeUserAccessor _backofficeUserAccessor;
        private readonly IMemberManager _memberManager;
        private readonly IMemberService _memberService;
        private readonly IForumMailService _mailService;

        public ForumPostPublishedEvent(ILogger<ForumPostPublishedEvent> logger,AppCaches appCaches,IContentService contentService,IContentTypeService contentType,
            IForumMailService mailService,
            IUmbracoContextFactory context,IBackofficeUserAccessor backofficeUserAccessor, IMemberManager memberManager,IMemberService memberService)
        {
            _appCaches = appCaches;
            _contentService = contentService;
            _contentType = contentType;
            _context = context;
            _logger = logger;
            _memberManager = memberManager;
            _backofficeUserAccessor = backofficeUserAccessor;
            _memberService = memberService;
            _mailService = mailService;
        }
        public void Handle(ContentPublishedNotification notification)
        {

            foreach (var item in notification.PublishedEntities)
            {
                // is a forum post...
                if (item.ContentType.Alias.Equals("forumPost"))
                {

                    var currentUser = _memberManager.GetCurrentMemberAsync().Result;
                    var backofficeUser = _backofficeUserAccessor.BackofficeUser;
                    if (currentUser == null && backofficeUser != null && backofficeUser.IsAuthenticated)
                    {
                        return;
                    }
                    //Update post count if userlogged in to forum
                    if (currentUser != null)
                    {
                        var member = _memberService.GetByKey(currentUser.Key);
                        int posts = member.GetValue<int>("postCount");
                        posts += 1;
                        member.SetValue("postCount",posts);
                        _memberService.Save(member);

                        using var cref = _context.EnsureUmbracoContext();
                        {
                            var cache = cref.UmbracoContext.Content;
                            
                            var post = cache.GetById(item.Id);
                            if (post == null)
                                return;

                            var author = post.Value("postAuthor");
                            // work out the root of this post (top of the thread)
                            var postRoot = post;
                            var parent = _contentService.GetParent(post.Id);
                            if (parent.ContentType.Alias== "forumPost")
                            {
                                // if we have a parent post, then this is a reply 
                                postRoot = post.Parent;
                            }
                            _logger.LogInformation("Sending Notification for new post for {0}", postRoot.Name);

                            List<string> receipients = GetRecipients(postRoot);

                            // remove the author from the list.
                            var postAuthor = GetAuthorEmail(post);
                            if (receipients.Contains(postAuthor))
                                receipients.Remove(postAuthor);

                            if (receipients.Any())
                            {
                                _mailService.SendNotificationEmail(postRoot, post, author, receipients, true);
                            }
                        }
                    }

                }
            }
            

        }
        private List<string> AddParentForumCaches(IContent item, List<string> cacheList)
        {
            var parent = _contentService.GetParent(item);
            var forumType = _contentType.Get("forum");
            var postType = _contentType.Get("forumPost");

            if (parent != null && forumType != null)
            {
                parent.UpdateDate = DateTime.Now;
                _contentService.SaveAndPublish(parent);
                if (parent.ContentTypeId == forumType.Id || parent.ContentTypeId == forumType.Id)
                {
                    var cache = $"forum_{parent.Id}";
                    if (!cacheList.Contains(cache))
                        cacheList.Add(cache);
                    
                    cacheList = AddParentForumCaches(parent, cacheList);
                }
                else if (parent.ContentTypeId == postType.Id || parent.ContentTypeId == postType.Id)
                {
                    var cache = $"Topic_{parent.Id}";
                    if (!cacheList.Contains(cache))
                        cacheList.Add(cache);

                    cacheList = AddParentForumCaches(parent, cacheList);
                }
                else
                {
                    cacheList = AddParentForumCaches(parent, cacheList);
                }
            }

            return cacheList;
        }
        private List<string> GetRecipients(IPublishedContent item)
        {
            List<string> recipients = new List<string>();

            // get the orginal post author.
            var topicAuthorEmail = GetAuthorEmail(item);

            if (!string.IsNullOrWhiteSpace(topicAuthorEmail))
            {
                recipients.Add(topicAuthorEmail);
            }

            foreach (var childPost in item.Children().Where(x => x.IsVisible()))
            {
                var postAuthorEmail = GetAuthorEmail(childPost);
                if (!string.IsNullOrWhiteSpace(postAuthorEmail) && !recipients.Contains(postAuthorEmail))
                {
                    _logger.LogInformation("Adding: {0}", postAuthorEmail);
                    recipients.Add(postAuthorEmail);
                }
            }
            return recipients;
        }

        private string GetAuthorEmail(IPublishedContent post)
        {
            if (post == null)
                return string.Empty;
            if (post.HasValue("postAuthor"))
            {
                var author = post.Value<IPublishedContent>("PostAuthor");
                if (author != null && author.Value<int?>("receiveNotifications") != null)
                {
                    return author.Value<string>("Email");
                }
            }
            return string.Empty;
        }

    }
}