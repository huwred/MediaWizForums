using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Security;

namespace MediaWiz.Core.Controllers
{
    public class upload
    {
        public string location;
    }

    /// <summary>
    /// Summary description for ForumsApiController
    /// </summary>
    public class ForumsApiController : UmbracoApiController
    {
        
        private readonly IContentService _contentService;
        private readonly MemberManager _memberManager;
        private readonly ILocalizationService _localizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ForumsApiController> _logger;
        private readonly MediaFileManager _mediaFileManager;
        private readonly IMemberService _memberService;

        public ForumsApiController(MediaFileManager mediaFileManager, ILogger<ForumsApiController> logger,
            IHttpContextAccessor httpContextAccessor,  IContentService contentservice,MemberManager memberManager, 
            ILocalizationService localizationService, IMemberService memberService)
        {
            _memberManager = memberManager;
            _localizationService = localizationService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _mediaFileManager = mediaFileManager;
            _contentService = contentservice;
            _memberService = memberService;
        }

        /// <summary>
        /// used by the front end to delete posts via ajax.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("deletepost/{id?}")]
        [Authorize(Roles = "ForumAdministrator")]
        public bool DeletePost(int? id)
        {
            if (id != null)
            {
                var post = _contentService.GetById(id.Value);


                if (post != null)
                {
                    var author = post.GetValue<string>("postAuthor");
                    var currentMember = _memberManager.GetCurrentMemberAsync().Result;
                    var roles =  _memberManager.GetRolesAsync(currentMember).Result;

                    if (author != "0" && author == currentMember.Id)
                    {
                        //Logger.LogInformation<ForumsApiController>("Deleting post {0}", id);
                        if ( post.HasProperty("umbracoNaviHide")) 
                            post.SetValue("umbracoNaviHide", true);

                        if ( post.HasProperty("deletedByAuthor"))
                            post.SetValue("deletedByAuthor", true);

                        _contentService.SaveAndPublish(post);
                        //Logger.LogInformation<ForumsApiController>("Deleting post {0}", id);
                        return true;
                    }
                    else if(roles.Contains("ForumAdministrator") ||roles.Contains("ForumModerator"))
                    {
                        //Logger.LogInformation<ForumsApiController>("Deleting post {0}", id);
                        if ( post.HasProperty("umbracoNaviHide")) 
                            post.SetValue("umbracoNaviHide", true);

                        if ( post.HasProperty("deletedByAuthor"))
                            post.SetValue("deletedByAuthor", false);

                        _contentService.SaveAndPublish(post);
                        //Logger.LogInformation<ForumsApiController>("Deleting post {0}", id);
                        return true;
                    }
                }
            }

            return false;
        }

        [Route("lockpost/{id?}")]
        public bool LockPost(int? id)
        {
            if (id != null)
            {
                var post = _contentService.GetById(id.Value);

                if (post != null)
                {
                    var author = post.GetValue<string>("postAuthor");
                    var currentMember =  _memberManager.GetCurrentMemberAsync().Result;
                    var roles =  _memberManager.GetRolesAsync(currentMember).Result;

                    if ((author != "0" && author == currentMember.Id) || (roles.Contains("ForumAdministrator") || roles.Contains("ForumModerator")))
                    {
                        if (post.HasProperty("allowReplies"))
                        {
                            var currentState = post.GetValue<bool>("allowReplies");
                            post.SetValue("allowReplies", !currentState);
                        }
                        
                        _contentService.SaveAndPublish(post);
                        return true;
                    }

                }
            }

            return false;
        }    
 
        [Route("lockuser/{id?}")]
        public bool LockUser(int? id)
        {
            if (id != null)
            {
 
                var user = _memberManager.FindByIdAsync(id.Value.ToString()).Result;
                var member = _memberService.GetById(id.Value);

                var querystring = _httpContextAccessor.HttpContext.Request.QueryString;
                var query = QueryHelpers.ParseQuery(querystring.Value);
                var mode = query["mode"].ToString();
                switch (mode)
                {
                    case "lock":
                        member.IsLockedOut = true;
                        _memberService.Save(member);
                        break;
                    case "unlock":
                        member.IsLockedOut = false;
                        _memberService.Save(member);
                        break;
                    default:
                        return false;
                        break;
                }

                return true;
            }

            return false;
        }    
        #region Installation

        private void AddDictionaryItems()
        {
            try
            {
                var defLang = _localizationService.GetDefaultLanguageId();
                ILanguage lang = _localizationService.GetLanguageById(defLang.Value);
                _logger.LogDebug("Executing AddDictionaryItems" + defLang);
                var newitem = _localizationService.GetDictionaryItemByKey("Forums.ForgotPasswordView") ?? new DictionaryItem("Forums.ForgotPasswordView");
                _localizationService.AddOrUpdateDictionaryValue(newitem,lang, "/reset" );
                _localizationService.Save(newitem);
                newitem = _localizationService.GetDictionaryItemByKey("Forums.LoginView") ?? new DictionaryItem("Forums.LoginView");
                _localizationService.AddOrUpdateDictionaryValue(newitem,lang,"/Login" );
                _localizationService.Save(newitem);
                newitem = _localizationService.GetDictionaryItemByKey("Forums.ResetPasswordView") ?? new DictionaryItem("Forums.ResetPasswordView");
                _localizationService.AddOrUpdateDictionaryValue(newitem,lang,"Member/ForumAuth.ResetPassword" );
                _localizationService.Save(newitem);
                newitem = _localizationService.GetDictionaryItemByKey("Forums.RegisterView") ?? new DictionaryItem("Forums.RegisterView");
                _localizationService.AddOrUpdateDictionaryValue(newitem,lang,"Member/ForumAuth.Register" );
                _localizationService.Save(newitem);
                newitem = _localizationService.GetDictionaryItemByKey("Forums.ProfileView") ?? new DictionaryItem("Forums.ProfileView");
                _localizationService.AddOrUpdateDictionaryValue(newitem,lang,"Member/ForumAuth.ViewProfile" );
                _localizationService.Save(newitem);
                newitem = _localizationService.GetDictionaryItemByKey("Forums.ProfileEditView") ?? new DictionaryItem("Forums.ProfileEditView");
                _localizationService.AddOrUpdateDictionaryValue(newitem,lang,"Member/ForumAuth.EditProfile" );
                _localizationService.Save(newitem);
                newitem = _localizationService.GetDictionaryItemByKey("ForumAuths.LoginUrl") ?? new DictionaryItem("Forums.LoginUrl");
                _localizationService.AddOrUpdateDictionaryValue(newitem,lang,"/login" );
                _localizationService.Save(newitem);
                _localizationService.Save(newitem);
                newitem = _localizationService.GetDictionaryItemByKey("Forums.RegisterUrl") ?? new DictionaryItem("Forums.RegisterUrl");
                _localizationService.AddOrUpdateDictionaryValue(newitem,lang,"/register" );
                _localizationService.Save(newitem);
                newitem = _localizationService.GetDictionaryItemByKey("Forums.VerifyUrl") ?? new DictionaryItem("Forums.VerifyUrl");
                _localizationService.AddOrUpdateDictionaryValue(newitem,lang,"/verify" );
                _localizationService.Save(newitem);
                _localizationService.Save(lang);
            }
            catch (Exception e)
            {
                _logger.LogError( e, "Executing AddDictionaryItems");

            }

        }

        #endregion

        /// <summary>
        /// File upload handler for TinyMCE
        /// </summary>
        /// <returns></returns>
        [Route("forumupload/{id?}")]
        public async Task<IActionResult> TinyMceUpload(int? id)
        {
            var path = "/forumuploads";

            var file = _httpContextAccessor.HttpContext.Request.Form.Files;
            var loc = await SaveFileAsync(path, file[0]);
            var data = new upload()
            {
                location = "/media" + loc
            };
            var test = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            return Content(test);
            
        }

        /// <summary>
        /// Saves the contents of an uploaded image file.
        /// </summary>
        /// <param name="targetFolder">Location where to save the image file.</param>
        /// <param name="file">The uploaded image file.</param>
        /// <exception cref="InvalidOperationException">Invalid MIME content type.</exception>
        /// <exception cref="InvalidOperationException">Invalid file extension.</exception>
        /// <exception cref="InvalidOperationException">File size limit exceeded.</exception>
        /// <returns>The relative path where the file is stored.</returns>
        private async Task<string> SaveFileAsync(string targetFolder, IFormFile file)
        {
            const int megabyte = 1024 * 1024;
            string[] extensions = { ".gif", ".jpg", ".png", ".svg", ".webp" };
            //var fs = new PhysicalFileSystem("~" + targetFolder);

            var _fileSystem = _mediaFileManager.FileSystem;

            if (!_fileSystem.DirectoryExists(_fileSystem.GetFullPath(targetFolder)))
            {
                Directory.CreateDirectory(_fileSystem.GetFullPath(targetFolder));
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                throw new InvalidOperationException("Invalid MIME content type.");
            }

            var extension = Path.GetExtension(file.FileName.ToLowerInvariant());
            if (!extensions.Contains(extension))
            {
                throw new InvalidOperationException("Invalid file extension.");
            }
            
            if (file.Length > (8 * megabyte))
            {
                throw new InvalidOperationException("File size limit exceeded.");
            }

            var fileName = Guid.NewGuid() + extension;
            var path = Path.Combine(_fileSystem.GetFullPath(targetFolder), fileName);
            await using (Stream fileStream = new FileStream(path, FileMode.Create)) {
                await file.CopyToAsync(fileStream);
            }


            return Combine(targetFolder, fileName);
        }
        public static string Combine(string uri1, string uri2)
        {
            uri1 = uri1.TrimEnd('/');
            uri2 = uri2.TrimStart('/');
            return $"{uri1}/{uri2}";
        }

    }
}