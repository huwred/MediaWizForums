using System;
using Examine;
using MediaWiz.Forums.Indexing;
using Microsoft.Extensions.Logging;
using Serilog;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Examine;

namespace MediaWiz.Forums.Events;

public class ForumPostDeletedHandler : INotificationHandler<ContentMovedToRecycleBinNotification>
{
    private readonly ILogger<ForumPostDeletedHandler> _logger;
    private readonly IExamineManager _examineManager;
    private readonly IIndexRebuilder _indexRebuilder;

    public ForumPostDeletedHandler(IIndexRebuilder indexRebuilder,IExamineManager examineManager,ILogger<ForumPostDeletedHandler> logger)
    {
        _examineManager = examineManager;
        _logger = logger;
        _indexRebuilder = indexRebuilder;
    }

    public void Handle(ContentMovedToRecycleBinNotification notification)
    {

        bool rebuild = false;
        foreach (var node in notification.MoveInfoCollection)
        {
            if (node.Entity.ContentType.Alias.Equals("forumPost"))
            {
                rebuild = true;
            }
        }

        if (rebuild)
        {
            _indexRebuilder.RebuildIndex("ForumIndex", null, true);
        }
    }
}