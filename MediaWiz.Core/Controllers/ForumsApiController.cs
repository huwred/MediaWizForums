using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaWiz.Forums.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NPoco.fastJSON;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Security;

namespace MediaWiz.Forums.Controllers
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
        private readonly IForumMailService _mailService;
        private readonly UmbracoHelper _umbracoHelper;

        public object TempData { get; private set; }

        public ForumsApiController(MediaFileManager mediaFileManager, ILogger<ForumsApiController> logger,
            IHttpContextAccessor httpContextAccessor,  IContentService contentservice,MemberManager memberManager, 
            ILocalizationService localizationService, IMemberService memberService,IForumMailService forumMailService,
            IUmbracoHelperAccessor umbracoHelper)
        {
            _memberManager = memberManager;
            _localizationService = localizationService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _mediaFileManager = mediaFileManager;
            _contentService = contentservice;
            _memberService = memberService;
            _mailService = forumMailService;
            umbracoHelper.TryGetUmbracoHelper(out _umbracoHelper);
        }

        /// <summary>
        /// used by the front end to delete posts via ajax.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("deletepost/{id?}")]
        //[Authorize(Roles = "ForumAdministrator")]
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
        [Route("captchacheck/{id?}")]
        public bool CaptchaCheck(int? id)
        {
            if (id != null)
            {
                var session = _httpContextAccessor.HttpContext.Session;

                if (session.Keys.Contains("Captcha") && session.GetString("Captcha") != id.Value.ToString())
                {
                    ModelState.AddModelError("Captcha", "Wrong value of sum, please try again.");
                    return false;
                }
                //empty the captcha variable
                session.Remove("Captcha");
                return true;
            }

            return false;
        }

        [Route("markanswer/{id?}")]
        [HttpGet]
        public bool MarkAsAnswer(int? id)
        {
            if (id != null)
            {
                var post = _contentService.GetById(id.Value);

                if (post != null)
                {
                    var author = post.GetValue<string>("postAuthor");
                    var currentMember =  _memberManager.GetCurrentMemberAsync().Result;

                    if (currentMember != null)
                    {
                        if (post.HasProperty("answer"))
                        {
                            post.SetValue("answer", true);
                            _contentService.SaveAndPublish(post);
                            var parentTopic = _contentService.GetById(post.ParentId);
                            if (parentTopic != null)
                            {
                                parentTopic.SetValue("answer", true);
                                _contentService.SaveAndPublish(parentTopic);
                            }
                            return true;
                        }


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

                }

                return true;
            }

            return false;
        }    
        #region Installation



        [Route("sendvalidation")]
        [HttpPost]
        public void ResendValidation(JObject jobj)
        {
            var id = jobj["id"].Value<int?>();

            if (id == null)
            {
                return;
            }
            var member = _memberService.GetById(id.Value);
            
            var result =  _mailService.SendVerifyAccount(member.Email,member.GetValue<string>("resetGuid")).Result;
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
            var currentMember =  _memberManager.GetCurrentMemberAsync().Result;
            path = path + "/" + currentMember.Id;

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
                throw new InvalidOperationException("File size (8MB) limit exceeded.");
            }            
            var _fileSystem = _mediaFileManager.FileSystem;

            if (!_fileSystem.DirectoryExists(_fileSystem.GetFullPath(targetFolder)))
            {
                Directory.CreateDirectory(_fileSystem.GetFullPath(targetFolder));
            }

            var fileName = Guid.NewGuid() + extension;
            var path = Path.Combine(_fileSystem.GetFullPath(targetFolder), file.FileName);
            await using (Stream fileStream = new FileStream(path, FileMode.Create)) {
                await file.CopyToAsync(fileStream);
            }


            return Combine(targetFolder, file.FileName);
        }
        public static string Combine(string uri1, string uri2)
        {
            uri1 = uri1.TrimEnd('/');
            uri2 = uri2.TrimStart('/');
            return $"{uri1}/{uri2}";
        }

    }
}