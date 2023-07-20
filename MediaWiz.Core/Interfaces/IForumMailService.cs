using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace MediaWiz.Forums.Interfaces
{
    public interface IForumMailService
    {

        Task SendResetPassword(string email, string token);
        Task<bool> SendVerifyAccount(string email, string guid);

        void SendNotificationEmail(IPublishedContent root, IPublishedContent post, object author,List<string> recipients, bool newPost);

        string GetEmailTemplate(string template, string dictionaryString, Dictionary<string,string> parameters);
    }
}