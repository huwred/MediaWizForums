using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using MediaWiz.Forums.Extensions;
using MediaWiz.Forums.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace MediaWiz.Core.Services
{
    public class ForumMailService : IForumMailService
    {
        private readonly string _fromEmail;
        private readonly HostingSettings _hostingSettings;
        private readonly ILogger _logger;
        private readonly IEmailSender _emailSender;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILocalizationService _localisation;
        private readonly IMemberService _memberService;


        public ForumMailService(IHostingEnvironment hostingEnvironment, IOptions<GlobalSettings> globalSettings,IEmailSender emailSender, 
            ILocalizationService localisation,ILogger<MailMessage> logger,IMemberService memberService,IOptions<ContentSettings> contentSettings,
            IOptions<HostingSettings> hostingSettings)
        {

            _logger = logger;
            _emailSender = emailSender;
            _hostingEnvironment = hostingEnvironment;
            _localisation = localisation;
            _memberService = memberService;
            _hostingSettings = hostingSettings.Value;

            _fromEmail = globalSettings.Value.Smtp?.From != null ? globalSettings.Value.Smtp.From : contentSettings.Value.Notifications.Email;
        }

        /// <summary>
        /// Sends an email for new members to verify their account
        /// </summary>
        /// <param name="umbraco"></param>
        /// <param name="email"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<bool> SendVerifyAccount(string email, string guid)
        {
            try
            {

                string baseURL = _hostingEnvironment.ApplicationMainUrl.AbsoluteUri;
                var resetUrl = baseURL + _localisation.GetOrCreateDictionaryValue("Forums.VerifyUrl","/verify").TrimEnd('/') + "/?verifyGUID=" + guid;
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    {"{resetUrl}", resetUrl}
                };
                var messageTemplate = _localisation.GetOrCreateDictionaryValue("Forums.VerifyBody",@"<h2>Verify your account</h2>
            <p>in order to use your account, you first need to verify your email address using the link below.</p>
            <p><a href='{resetUrl}'>Verify your account</a></p>");

                var messageBody = GetEmailTemplate(messageTemplate, "Forums.NotificationBody", parameters);
                EmailMessage message = new EmailMessage(_fromEmail, email,
                    _localisation.GetOrCreateDictionaryValue("Forums.VerifySubject", "Verifiy your account"), messageBody, true);


                await _emailSender.SendAsync(message, emailType: "Contact");
                return true;
 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Problems sending verify email: {0}", ex.ToString());
                return false;
            }
                
        }

        public async void SendNotificationEmail(IPublishedContent root, IPublishedContent post, object author, List<string> recipients, bool newPost)
        {

            var threadTitle = root.Value<string>("postTitle");
            var updateBody = post.Value<string>("postBody");

            var authorName = author != null ? ((IPublishedContent)author).Name : post.Value<string>("postCreator");

            string postUrl = _hostingEnvironment.ApplicationMainUrl.AbsoluteUri;

            // build the default body template
            var bodyTemplate = "<p>{author} has posted a comment to the '{postTitle}' topic</p>" +
                "<div style=\"border-left: 4px solid #444;padding:0.5em;font-size:1.3em\">{body}</div>" +
                "<p>you can view all the comments here: <a href=\"{threadUrl}\">{threadUrl}</a></p>" +
                "<p>You get this notification because you are subscribed to receive notifications.You can unsubscribe from your profile on</p>";

            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"{postTitle}", postUrl},
                {"{newOld}", newPost ? "New" : "Updated"}
            };
            var Subject = GetEmailTemplate("{newOld} comment in topic '{postTitle}'", "Forums.NotificationSubject", parameters);

            parameters = new Dictionary<string, string>
            {
                {"{author}", authorName},
                {"{postTitle}", threadTitle},
                {"{body}", updateBody},
                {"{threadUrl}", postUrl}
            };
            var Body = GetEmailTemplate(bodyTemplate, "Forums.NotificationBody", parameters);

            EmailMessage message = new EmailMessage(_fromEmail, new string[]{recipients.First()}, null, recipients.ToArray(), new[] { _fromEmail },
                Subject,
                Body,
                true, null);

            try
            {
                _logger.LogInformation("Sending Email {0} to {1} people", threadTitle, recipients.Count);
                // smtp (assuming you've set all this up)
                await _emailSender.SendAsync(message, emailType: "Contact");
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Failed to send the email - probably because email isn't configured for this site\n {0}", ex.ToString());
            }
        }

        public async Task SendResetPassword(string email, string token)
        {
            try
            {
                var member = _memberService.GetByEmail(email);
                if (member != null)
                {

                    string baseURL = _hostingEnvironment.ApplicationMainUrl.AbsoluteUri;
                    var resetUrl = baseURL + "/forgotpassword/?id=" + member.Id + "&token=" + token;

                    var subjectTemplate = @"Password reset requested for {_hostingSettings.SiteName}";
                    Dictionary<string, string> parameters = new Dictionary<string, string>
                    {
                        {"{_hostingSettings.SiteName}", _hostingSettings.SiteName},
                    };
                    var subject = GetEmailTemplate(subjectTemplate, "Forums.RestSubject", parameters);
                    var messageTemplate = @"<p>Hi {member.Name},</p>
                    <p>Someone requested a password reset for your account on {_hostingSettings.SiteName}.</p>
                    <p>If this wasn't you then you can ignore this email, otherwise, please click the following password reset link to continue:</p>
                    <p>Please go to <a href='{resetUrl}'>here</a> to reset your password</p>
                    <p>&nnbsp;</p>
                    <p>Kind regards,<br/>The {_hostingSettings.SiteName} Team</p>";

                    parameters = new Dictionary<string, string>
                    {
                        {"{member.Name}", member.Name},
                        {"{_hostingSettings.SiteName}", _hostingSettings.SiteName},
                        {"{resetUrl}",resetUrl}
                    };
                    var body = GetEmailTemplate(messageTemplate, "Forums.RestBody", parameters);

                    EmailMessage message = new EmailMessage(_fromEmail,email,subject,body,true);

                    try
                    {
                        // smtp (assuming you've set all this up)
                        await _emailSender.SendAsync(message, emailType: "Contact");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation("Failed to send the email - probably because email isn't configured for this site\n {0}", ex.ToString());
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error {0}",ex.Message);
            }
        }

        public string GetEmailTemplate(string template, string dictionaryString, Dictionary<string,string> parameters)
        {
            var dictionaryTemplate = _localisation.GetDictionaryItemByKey(dictionaryString);
            if (dictionaryTemplate != null && !string.IsNullOrWhiteSpace(dictionaryTemplate.GetDefaultValue()))
            {
                template = dictionaryTemplate.GetDefaultValue();
            }


            return template?.ReplaceMany(parameters);
        }
        private void AllDone(object sender, AsyncCompletedEventArgs e)
        {
            _logger.LogInformation(e.Error.Message);
        }
    }

}