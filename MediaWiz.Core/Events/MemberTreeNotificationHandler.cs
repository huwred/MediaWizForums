using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace MediaWiz.Forums.Events
{
    /// <summary>
    /// Used to add a menu item when the tree renders
    /// </summary>
    public class MemberTreeNotificationHandler : INotificationHandler<MenuRenderingNotification>
    {
        /// <summary>
        /// Adds a translate action to the Dictionary menu
        /// </summary>
        /// <param name="notification">The notification details</param>
        public void Handle(MenuRenderingNotification notification)
        {
            if (notification.TreeAlias.Equals(Constants.Trees.Members))
            {
                var menuItem = new Umbraco.Cms.Core.Models.Trees.MenuItem("sendValidation", "SendValidation");
                menuItem.AdditionalData.Add("actionView", "/App_Plugins/MediaWizards/resendValidation.html");
                menuItem.Icon = "shuffle";
                notification.Menu.Items.Add(menuItem);
            }
        }
    }
}
