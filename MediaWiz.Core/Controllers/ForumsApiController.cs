using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using MediaWiz.Forums.Extensions;
using MediaWiz.Forums.Helpers;
using MediaWiz.Forums.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace MediaWiz.Forums.Controllers
{
    public class Upload
    {
        public string location;
    }

    /// <summary>
    /// Summary description for ForumsApiController
    /// </summary>
    public class ForumsApiController : UmbracoApiController
    {
        private const string uploadFolder = "forumuploads";

        private readonly IContentService _contentService;
        private readonly MemberManager _memberManager;
        private readonly ILocalizationService _localizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ForumsApiController> _logger;
        private readonly MediaFileManager _mediaFileManager;
        private readonly IMemberService _memberService;
        private readonly IForumMailService _mailService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IOptions<ForumConfigOptions> _forumOptions;
        private string[] AllowedFiles => _forumOptions.Value.AllowedFiles;
        private int MaxFileSize => _forumOptions.Value.MaxFileSize;
        private bool UniqueFilenames => _forumOptions.Value.UniqueFilenames;

        public object TempData { get; private set; }


        public ForumsApiController(MediaFileManager mediaFileManager, ILogger<ForumsApiController> logger,
            IHttpContextAccessor httpContextAccessor,  IContentService contentservice,MemberManager memberManager, 
            ILocalizationService localizationService, IMemberService memberService,IForumMailService forumMailService,
            IOptions<ForumConfigOptions> forumOptions,
            IWebHostEnvironment hostingEnvironment)
        {
            _memberManager = memberManager;
            _localizationService = localizationService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _mediaFileManager = mediaFileManager;
            _contentService = contentservice;
            _memberService = memberService;
            _mailService = forumMailService;
            _hostingEnvironment = hostingEnvironment;
            _forumOptions = forumOptions;

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
                    if(!roles.Contains("ForumAdministrator") && !roles.Contains("ForumModerator") && !(author != "0" && author == currentMember.Id))
                    {
                        return false;
                    }

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
                    
                    ModelState.AddModelError("Captcha", _localizationService.GetOrCreateDictionaryValue("Forums.Error.CaptchaFail","Wrong value of sum, please try again."));
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
        [Route("approve/{id?}")]
        public bool ApprovePost(int? id)
        {
            var currentMember =  _memberManager.GetUserAsync(_httpContextAccessor.HttpContext?.User!).Result;
            var roles =  _memberManager.GetRolesAsync(currentMember).Result;
            if (!roles.Contains("ForumAdministrator") && !roles.Contains("ForumModerator"))
            {
                return false;
            }
            if (id != null)
            {
                var post = _contentService.GetById(id.Value);

                if (post != null)
                {

                    if (roles.Contains("ForumAdministrator") || roles.Contains("ForumModerator"))
                    {
                        if (post.HasProperty("approved"))
                        {
                            var currentState = post.GetValue<bool>("approved");
                            post.SetValue("approved", !currentState);
                            post.SetValue("umbracoNaviHide",currentState);
                        }
                        var parent = _contentService.GetParent(post);
                        if (parent.HasProperty("unapprovedReplies"))
                        {
                             var counter = parent.GetValue<int>("unapprovedReplies");
                             if(counter > 0)
                            {
                                counter -= 1;
                            }
                             parent.SetValue("unapprovedReplies", counter);

                            _contentService.SaveAndPublish(parent, new string[] { "*" });
                        }

                        _contentService.SaveAndPublish(post, new string[] { "*" });
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

        [Route("sendvalidation/{id?}")]
        public void ResendValidation(Guid? id)
        {
            if (id == null)
            {
                return;
            }
            var member = _memberService.GetByKey(id.Value);
                string resetGuid = ForumHelper.GenerateUniqueCode(16);
                member.SetValue("resetGuid", resetGuid);    
            _memberService.Save(member);

            var result =  _mailService.SendVerifyAccount(member.Email,resetGuid).Result;
        }
        #endregion

        /// <summary>
        /// File upload handler for TinyMCE
        /// </summary>
        /// <returns></returns>
        [Route("forumupload/{id?}")]
        public async Task<IActionResult> TinyMceUpload(int? id)
        {
            var currentMember =  _memberManager.GetCurrentMemberAsync().Result;
            var path = "/" + Combine(uploadFolder, currentMember.Id); 

            var file = _httpContextAccessor.HttpContext.Request.Form.Files;
            var loc = await SaveFileAsync(path, file[0]);
            var data = new Upload()
            {
                location = "/media" + loc + "?width=800"
            };
            var test = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            return Content(test);
            
        }

        [Route("memberfiles/{id?}")]
        public async Task<IActionResult> GetMemberFiles(int? id)
        {
            string wwwroot = _hostingEnvironment.MapPathWebRoot("~/");
            string folderPath = _hostingEnvironment.MapPathWebRoot($"~/media/{uploadFolder}/" + id);
            string[] files = Directory.GetFiles(folderPath);
            var content = _localizationService.GetOrCreateDictionaryValue("Forums.Profile.NoFiles", "No files uploaded");
            if (files.Any())
            {
                
                content = @"<ul class=""image-gallery list-unstyled"">";
                foreach (var file in files)
                {
                    var imgpath = file.Replace(wwwroot, "") + "?width=120";
                    content += @$"<li class=""d-flex p-2"" data-id=""{id}""><img src=""{imgpath}"" width=""120"" /><i title=""Delete file"" class=""fs-4 bi-trash delete-member-file"" data-id=""{HttpUtility.HtmlEncode(imgpath)}""></i></li>";
                }

                content += "</ul>";
            }
            return Content(content);
        }

        [Route("deletefile/{id?}")]
        public IActionResult DeleteFile(string id)
        {
            try
            {
                var file = _hostingEnvironment.MapPathWebRoot("~" + id.Split('?')[0]);
                System.IO.File.Delete(file);
                var memberidMatch = Regex.Match(id, @"(\\[0-9]+\\)");
                if (memberidMatch.Success)
                {
                    return Content(memberidMatch.Value.Replace("\\",""));
                }                
                return Content("");
            }
            catch (Exception e)
            {
                _logger.LogError(e,"Member File delete error {0}",id);
                return Content(e.Message);
            }
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
            string[] extensions =  { ".gif", ".jpg", ".png", ".svg", ".webp" };
            if (AllowedFiles.Any())
            {
                extensions = AllowedFiles;
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                throw new InvalidOperationException(_localizationService.GetOrCreateDictionaryValue("Forums.Error.MimeType","MIME type is not an Image."));
            }
            var extension = Path.GetExtension(file.FileName.ToLowerInvariant());
            if (!extensions.Contains(extension))
            {
                throw new InvalidOperationException(_localizationService.GetOrCreateDictionaryValue("Forums.Error.FileExt","Invalid file extension."));
            }            
            if (file.Length > (MaxFileSize * megabyte))
            {
                
                throw new InvalidOperationException($"{_localizationService.GetOrCreateDictionaryValue("Forums.Error.FileSize","File size limit exceeded.")} ({MaxFileSize}MB)");
            }            
            var _fileSystem = _mediaFileManager.FileSystem;

            if (!_fileSystem.DirectoryExists(_fileSystem.GetFullPath(targetFolder)))
            {
                Directory.CreateDirectory(_fileSystem.GetFullPath(targetFolder));
            }

            var fileName = file.FileName;
            if (UniqueFilenames)
            {
                fileName = Guid.NewGuid() + extension;
            }
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