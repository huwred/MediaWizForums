using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;

namespace MediaWiz.Core.Interfaces
{
    public interface IForumMailService
    {

        Task SendResetPassword(UmbracoHelper umbraco, string email, string guid);
        Task<bool> SendVerifyAccount(UmbracoHelper umbraco, string email, string guid);

        void SendNotificationEmail(IPublishedContent root, IPublishedContent post, object author,List<string> recipients, bool newPost);

        string GetEmailTemplate(string template, string dictionaryString, string postTitle, string body, string author,string threadUrl, bool newPost);
    }
}