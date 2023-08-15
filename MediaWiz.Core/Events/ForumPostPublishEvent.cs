using System.Collections.Generic;
using System.Linq;
using Examine;
using MediaWiz.Forums.Indexing;
using MediaWiz.Forums.Interfaces;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace MediaWiz.Forums.Events
{
    /// <summary>
    /// Post published event for Forum Posts,
    /// Updates members post count, clears caches and Handles Email notifications
    /// </summary>
    public class ForumPostPublishedEvent : INotificationHandler<ContentPublishedNotification>
    {
        private readonly IExamineManager _examineManager;
        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentType;
        private readonly ILogger<ForumPostPublishedEvent> _logger;
        private readonly IUmbracoContextFactory  _context;
        private readonly IBackofficeUserAccessor _backofficeUserAccessor;
        private readonly IMemberManager _memberManager;
        private readonly IMemberService _memberService;
        private readonly IForumMailService _mailService;
        private readonly IIndexRebuilder _indexRebuilder;
        private readonly IAppPolicyCache _runtimeCache;
        private readonly ForumIndexValueSetBuilder _forumIndexValueSetBuilder;

        public ForumPostPublishedEvent(
        ILogger<ForumPostPublishedEvent> logger,AppCaches appCaches,IContentService contentService,IContentTypeService contentType,
        IForumMailService mailService, IUmbracoContextFactory context,IBackofficeUserAccessor backofficeUserAccessor, 
        IMemberManager memberManager,IMemberService memberService,IIndexRebuilder indexRebuilder, IExamineManager examineManager,
        ForumIndexValueSetBuilder forumIndexValueSetBuilder)
        {
            _examineManager = examineManager;
            _contentService = contentService;
            _contentType = contentType;
            _context = context;
            _logger = logger;
            _memberManager = memberManager;
            _backofficeUserAccessor = backofficeUserAccessor;
            _memberService = memberService;
            _mailService = mailService;
            _indexRebuilder = indexRebuilder;

            _runtimeCache = appCaches.RuntimeCache;
            _forumIndexValueSetBuilder = forumIndexValueSetBuilder;

        }
        public void Handle(ContentPublishedNotification notification)
        {
            List<string> invalidCacheList = new List<string>();


            foreach (var item in notification.PublishedEntities)
            {
                // is a forum post...
                if (item.ContentType.Alias.Equals("forumPost"))
                {
                    var index = _examineManager.Indexes.FirstOrDefault(s => s.Name == "ForumIndex");

                    var indexData = _forumIndexValueSetBuilder.GetValueSets(item);
                    index.IndexItems(indexData);    
                    
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
                                invalidCacheList.Add($"Topic_{parent.Id}");
                            }
                            else
                            {
                                invalidCacheList.Add($"forum_{parent.Id}");
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
            foreach (var cache in invalidCacheList)
            {
                _runtimeCache.ClearByKey(cache);
            }

            if (invalidCacheList.Any())
            {
                RebuildIndex();
            }
            
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
        private void RebuildIndex()
        {

            bool validated = ValidateIndex("ForumIndex", out var index);
            if (!validated)
                return;

            validated = ValidatePopulator(index);
            if (!validated)
                return;

            _indexRebuilder.RebuildIndex(index.Name);
        }
        private bool ValidateIndex(string indexName, out IIndex index)
        {
            if (!_examineManager.TryGetIndex(indexName, out index))
            {
                _logger.LogError("ForumIndexRefreshComponent | | Message: {0}", $"No index found by name < ForumIndex >");
                return false;
            }

            return true;
        }

        private bool ValidatePopulator(IIndex index)
        {
            if (_indexRebuilder.CanRebuild(index.Name))
                return true;

            _logger.LogError( $"The index {index.Name} cannot be rebuilt because it does not have an associated {typeof(IIndexPopulator)}");
            return false;
        }

    }
}