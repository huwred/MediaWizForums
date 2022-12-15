using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using MediaWiz.Forums.Helpers;
using MediaWiz.Forums.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mail;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;
using Umbraco.Extensions;

namespace MediaWiz.Core.Services
{
    public class ForumMailService : IForumMailService
    {
        private readonly string _fromEmail;
        private readonly HostingSettings _hostingSettings;
        private readonly ILogger _logger;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContext;
        private readonly ILocalizationService _localisation;
        private readonly IMemberService _memberService;
        public ForumMailService(IHttpContextAccessor httpContext, IOptions<GlobalSettings> globalSettings,IEmailSender emailSender, 
            ILocalizationService localisation,ILogger<MailMessage> logger,IMemberService memberService,IOptions<ContentSettings> contentSettings,
            IOptions<HostingSettings> hostingSettings)
        {

            _logger = logger;
            _emailSender = emailSender;
            _httpContext = httpContext;
            _localisation = localisation;
            _memberService = memberService;
            _hostingSettings = hostingSettings.Value;

            _fromEmail = globalSettings.Value.Smtp?.From != null ? globalSettings.Value.Smtp.From : contentSettings.Value.Notifications.Email;
        }

        public async Task<bool> SendVerifyAccount(UmbracoHelper umbraco, string email, string guid)
        {
            try
            {
                //logger.Info<ForumEmailHelper>("Send Verify: {0} {1}", email, guid);

                var test = ForumHelper.GetAbsoluteUri(_httpContext.HttpContext.Request);
                string baseURL = test.AbsoluteUri.Replace(test.AbsolutePath, string.Empty);
                var resetUrl = baseURL + umbraco.GetDictionaryValue("Forums.VerifyUrl","/verify").TrimEnd('/') + "/?verifyGUID=" + guid;

                var messageBody = umbraco.GetDictionaryValue("Forums.VerifyBody",$@"<h2>Verify your account</h2>
            <p>in order to use your account, you first need to verify your email address using the link below.</p>
            <p><a href='{resetUrl}'>Verify your account</a></p>");


                EmailMessage message = new EmailMessage(_fromEmail, email,
                    umbraco.GetDictionaryValue("Forums.VerifySubject", "Verifiy your account"), messageBody, true);


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

            var test = ForumHelper.GetAbsoluteUri(_httpContext.HttpContext.Request);
            string siteUrl = test.AbsoluteUri;
            string postUrl = test.AbsoluteUri;

            //Umbraco community: New comment in topic 'UmbracoApiController returning 404 error'

            // build the default body template
            var bodyTemplate = "<p>{{author}} has posted a comment to the '{{postTitle}}' topic</p>" +
                "<div style=\"border-left: 4px solid #444;padding:0.5em;font-size:1.3em\">{{body}}</div>" +
                "<p>you can view all the comments here: <a href=\"{{threadUrl}}\">{{threadUrl}}</a></p>" +
                "<p>You get this notification because you are subscribed to receive notifications.You can unsubscribe from your profile on</p>";

            var Subject = GetEmailTemplate("{{newOld}} comment in topic '{{postTitle}}'", "Forums.NotificationSubject",
                threadTitle, updateBody?.ToString(), authorName, postUrl, newPost);

            var Body = GetEmailTemplate(bodyTemplate, "Forums.NotificationBody",
                threadTitle, updateBody?.ToString(), authorName, postUrl, newPost);

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

        public async Task SendResetPassword(UmbracoHelper umbraco, string email, string guid)
        {
            try
            {
                var member = _memberService.GetByEmail(email);
                if (member != null)
                {
                    var test = ForumHelper.GetAbsoluteUri(_httpContext.HttpContext.Request);

                    string baseURL = test.AbsoluteUri.Replace(test.AbsolutePath, string.Empty);
                    var resetUrl = baseURL + "/forgotpassword/?id=" + member.Id + "&token=" + guid;

                    var messageBody = umbraco.GetDictionaryValue("Forums.ResetBody",$@"<p>Hi {member.Name},</p>
                    <p>Someone requested a password reset for your account on {_hostingSettings.SiteName}.</p>
                    <p>If this wasn't you then you can ignore this email, otherwise, please click the following password reset link to continue:</p>
                    <p>Please go to <a href='{resetUrl}'>here</a> to reset your password</p>
                    <p>&nnbsp;</p>
                    <p>Kind regards,<br/>The {_hostingSettings.SiteName} Team</p>");

                    EmailMessage message = new EmailMessage(_fromEmail,email,umbraco.GetDictionaryValue("Forums.ResetSubject", $@"Password reset requested for {_hostingSettings.SiteName}"),messageBody,true);

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

        public string GetEmailTemplate(string template, string dictionaryString, string postTitle, string body, string author, string threadUrl, bool newPost)
        {
            var dictionaryTemplate = _localisation.GetDictionaryItemByKey(dictionaryString);
            if (dictionaryTemplate != null && !string.IsNullOrWhiteSpace(dictionaryTemplate.GetDefaultValue()))
            {
                template = dictionaryTemplate.GetDefaultValue();
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                {"{{author}}", author},
                {"{{postTitle}}", postTitle},
                {"{{body}}", body},
                {"{{threadUrl}}", threadUrl},
                {"{{newOld}}", newPost ? "New" : "Updated"}
            };

            return template.ReplaceMany(parameters);
        }


        private void AllDone(object sender, AsyncCompletedEventArgs e)
        {
            _logger.LogInformation(e.Error.Message);
        }
    }

}