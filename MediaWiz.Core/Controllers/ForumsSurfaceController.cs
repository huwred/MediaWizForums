using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using MediaWiz.Core.Interfaces;
using MediaWiz.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;

namespace MediaWiz.Core.Controllers
{
    /// <summary>
    /// Summary description for ForumsSurfaceController
    /// </summary>
    //[PluginController("MediaWiz.Core.Controllers")]
    public class ForumsSurfaceController : SurfaceController
    {
        private readonly IMemberService _memberService;
        private readonly IPublishedContentQuery _publishedContentQuery;
        private readonly IMemberSignInManager _signInManager;
        private readonly IMemberManager _memberManager;

        private readonly IContentService _contentService;
        private readonly ILogger _logger;
        private readonly IForumMailService _mailService;
        private readonly UmbracoHelper _umbracoHelper;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILocalizationService _localizationService;
        
        public ForumsSurfaceController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider,
            IMemberService memberService,
            IMemberSignInManager signInManager,
            IPublishedContentQuery publishedContentQuery,
            IMemberManager memberManager,
            IContentService contentService,
            ILogger<ForumsSurfaceController> logger,
            IForumMailService mailService,IUmbracoHelperAccessor umbracoHelper,IHttpContextAccessor httpContextAccessor,ILocalizationService localizationService) 
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberService = memberService;
            _memberManager = memberManager;
            _signInManager = signInManager;
            _publishedContentQuery = publishedContentQuery;
            _contentService = contentService;
            _logger = logger;
            _mailService = mailService;
            umbracoHelper.TryGetUmbracoHelper(out _umbracoHelper);
            _contextAccessor = httpContextAccessor;
            _localizationService = localizationService;
        }
        [HttpGet]
        public PartialViewResult EditPost(int id)
        {
            
            var post = _contentService.GetById(id);
            var model = new ForumsPostModel();
            model.Id = id;
            model.ParentId = post.ParentId;
            model.Title = post.GetValue<string>("postTitle");
            model.Body = post.GetValue<string>("postBody");
            model.AuthorId = post.GetValue<int>("postAuthor");
            model.IsTopic = post.GetValue<bool>("postType");
            //string referer = Request.Headers["Referer"].ToString();
            model.returnPath = _contextAccessor.HttpContext.Request.Headers["Referer"].ToString();
            return PartialView("Forums/_EditPostForm",model);
        }

        [HttpPost]
        public async Task<IActionResult> PostReply([Bind(Prefix="Post")]ForumsPostModel model)
        {
            IEnumerable<ILanguage> languages = _localizationService.GetAllLanguages();

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Reply", "Error posting (invalid model)");
                return  CurrentUmbracoPage();
            }

            if (await CanPost(model) == false)
            {
                ModelState.AddModelError("Reply", "You do not have permissions to post here");
                return CurrentUmbracoPage();
            }

            var posttype = model.IsTopic ? "topic" : "reply";

            var postName =
                $"reply_{DateTime.UtcNow:yyyyMMddhhmmss}";

            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                postName = model.Title;
            }


            var parent = _contentService.GetById(model.ParentId);
            bool newPost = false;
            if (parent != null)
            {
                IContent post = null;
                if (model.Id > 0)
                    post = _contentService.GetById(model.Id);

                if (post == null)
                {
                    post = _contentService.Create(postName, parent, "forumPost");
                    if (post.AvailableCultures.Any())
                    {
                        foreach (var language in languages)
                        {
                            post.SetCultureName(postName,language.IsoCode);
                        }
                    }
                    newPost = true;
                }

                // unlikely but possible we still don't have a node.
                if (post != null )
                {
                    post.SetValue("postTitle", model.Title);
                    post.SetValue("postBody", model.Body);

                    var author = _memberService.GetById(model.AuthorId);
                    if (author != null)
                    {
                        post.SetValue("postCreator", author.Name);
                        post.SetValue("postAuthor", author.Id);
                    }

                    if (parent.ContentType.Alias != "Forum")
                    {
                        // posts that are in a forum, are allowed replies 
                        // thats how the threads work.
                        post.SetValue("allowReplies", true);
                    }

                    post.SetValue("postType", model.IsTopic);
                    if (model.IsTopic)
                    {
                        post.SetValue("intPageSize",parent.GetValue<int>("intPageSize"));
                    }

                    if (!newPost)
                    {
                        post.SetValue("editDate",DateTime.UtcNow);
                    }
                    var result = _contentService.SaveAndPublish(post);

                    return RedirectToCurrentUmbracoPage();
                }
            }
            ModelState.AddModelError("Post", "Error creating the post");
            return RedirectToCurrentUmbracoPage();
        }
        [HttpPost]
        public async Task<IActionResult> EditPost([Bind(Prefix="Post")]ForumsPostModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("PostEdit", "Error editing post (invalid model)");
                return  CurrentUmbracoPage();
            }

            if (await CanPost(model) == false)
            {
                ModelState.AddModelError("PostEdit", "You do not have permissions to edit posts");
                return CurrentUmbracoPage();
            }
            var parent = _contentService.GetById(model.ParentId);

            if (parent != null)
            {
                IContent post = null;
                if (model.Id > 0)
                    post = _contentService.GetById(model.Id);
                // unlikely but possible we still don't have a node.
                if (post != null )
                {
                    if (!string.IsNullOrWhiteSpace(model.Title))
                    {
                        post.SetValue("postTitle", model.Title);
                    }
                    
                    post.SetValue("postBody", model.Body);
                    post.SetValue("editDate",DateTime.UtcNow);
                    var result = _contentService.SaveAndPublish(post);

                    return Redirect(model.returnPath);
                }
            }
            ModelState.AddModelError("PostEdit", "Error editing the post");
            return RedirectToCurrentUmbracoPage();
        }
        [HttpPost]
        [Authorize(Roles = "ForumAdministrator")]
        public IActionResult CreateForum([Bind(Prefix="Forum")]ForumsForumModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Forum", "Error creating Forum (invalid model)");
                return  CurrentUmbracoPage();
            }

            var parent = _contentService.GetById(model.ParentId);
            var forum = _contentService.CreateContent(model.Title, parent.GetUdi(), "forum");
            forum.SetValue("forumTitle",model.Title);
            forum.SetValue("forumDescription",model.Introduction);
            forum.SetValue("postAtRoot", model.AllowPosts);
            forum.SetValue("isActive",true);
            forum.SetValue("allowImages",model.AllowImages);
            var result = _contentService.SaveAndPublish(forum);
            TempData["ForumSaveResult"] = result;
            return CurrentUmbracoPage();
        }
 
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            TempData.Clear();
            await _signInManager.SignOutAsync();
            var forumRoot = _publishedContentQuery.ContentAtRoot().DescendantsOrSelfOfType("forum").FirstOrDefault(f=>f.Parent == null || f.Parent.ContentType.Alias != "forum");

            return Redirect(forumRoot?.Url());
        }
        [HttpGet]
        public IActionResult Reset(string resetGuid)
        {

            TempData["ResetForm"] = true;
            return View("Reset");
        }
        [HttpPost]
        public async Task<IActionResult> PasswordReset(ForumForgotPasswordModel model)
        {
            TempData["ResetSent"] = false;
            if (!ModelState.IsValid)
            {
                return PartialView(_umbracoHelper.GetDictionaryValue("Forums.ForgotPasswordView","Forums/_ForgotPassword"), model);
            }

            var member = _memberService.GetByEmail(model.EmailAddress);
            if (member != null)
            {
                var memberIdentity = await _memberManager.FindByIdAsync(member.Id.ToString());
                // we found a user with that email ....
                var token = await _memberManager.GeneratePasswordResetTokenAsync(memberIdentity);
                var encodedToken = !string.IsNullOrEmpty(token) ? HttpUtility.UrlEncode(token) : string.Empty;
                member.SetValue("resetGuid", token);
                _memberService.Save(member);

                // send email, do not wait as we want it to run in background....
                _mailService.SendResetPassword(_umbracoHelper,member.Email,encodedToken);

                TempData["ResetSent"] = true;
            }
            else
            {
                ModelState.AddModelError("ForgotPasswordForm", 
                    _umbracoHelper.GetDictionaryValue("Forums.Error.NoUser", "No user found"));
                return PartialView(_umbracoHelper.GetDictionaryValue("Forums.ForgotPasswordView","Forums/_ForgotPassword"));
            }

            return CurrentUmbracoPage();
        }
        // double check the current user can post to this forum...
        private async Task<bool> CanPost(ForumsPostModel model)
        {
            if (!_memberManager.IsLoggedIn())
                return false;
            
            if ( model.ParentId > 0 ) 
            {
                var parent = _publishedContentQuery.Content(model.ParentId);
                if ( parent != null )
                {
                    var canPostGroups = parent.Value<string>("canPostGroups");

                    // default is any one logged on...
                    if (string.IsNullOrWhiteSpace(canPostGroups))
                        return true;

                    // is the user in any of those groups ?
                    var allowedGroupList = new List<string>();
                    foreach (string memberGroupStr in canPostGroups.Split(','))
                    {
                        var memberGroup = Services.MemberGroupService.GetById(Convert.ToInt32(memberGroupStr));
                        if (memberGroup != null)
                        {
                            allowedGroupList.Add(memberGroup.Name);
                        }
                    }
                    return await _memberManager.IsMemberAuthorizedAsync(allowGroups: allowedGroupList);
                }
            }

            return false;
        }

        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost]
        public IActionResult Sort([FromBody] Object sort)
        {
            var data = JsonConvert.DeserializeObject<dynamic>(sort.ToString());
            var returnpath = _contextAccessor.HttpContext.Request.Headers["Referer"].ToString();

            returnpath = Regex.Replace(returnpath, @"\?sortdir=[A-Z]{3,4}", "");
            if (returnpath.Contains("?"))
            {
                returnpath += "&sortdir=" + data.sort.Value;
            }
            else
            {
                returnpath += "?sortdir=" + data.sort.Value;
            }

            return Json(new { success = true, message = returnpath });;
        }

        #region Captcha Image
        /// <summary>
        /// Checks the result of the Captcha check matches the Captcha stored in Session
        /// </summary>
        /// <param name="captcha">Result of Captcha equation</param>
        /// <returns></returns>
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        //[HttpPost]
        public IActionResult CaptchaCheck(int captcha)
        {
            var data = JsonConvert.DeserializeObject<dynamic>(captcha.ToString());
            var session = _contextAccessor.HttpContext.Session;

            if (session.Keys.Contains("Captcha") && session.GetString("Captcha") != data.captcha.Value)
            {
                ModelState.AddModelError("Captcha", "Wrong value of sum, please try again.");
                return
                    Json(
                        new
                        {
                            success = false,
                            message = "Wrong value of sum, please try again."
                        });
            }
            //empty the captcha variable
            session.Remove("Captcha");
            return Json(new { success = true });
        }

        /// <summary>
        /// Renders a Captcha Image with a simple arithmetic question
        /// </summary>
        /// <returns>data:image/jpg;base64</returns>
        [HttpGet]
        public IActionResult CaptchaImage()
        {
            var session = _contextAccessor.HttpContext.Session;
            string prefix = "";
            session.Remove("Captcha");
            var rand = new Random((int)DateTime.Now.Ticks);
            var allowed = new List<string>() { "plus", "minus" /*,"multiply"*/ };
            Random random = new Random();
            int item = random.Next(allowed.Count);
            string randomBar = allowed[item];

            try
            {
                //generate new question
                int a = rand.Next(10, 99);
                int b = rand.Next(0, 9);
                string captcha;
                switch (randomBar)
                {
                    case "plus":
                        session.SetString("Captcha" + prefix,(a+b).ToString());
                        captcha = $"{a} + {b} = ?";
                        break;
                    case "minus":
                        session.SetString("Captcha" + prefix,(a-b).ToString());
                        captcha = $"{a} - {b} = ?";
                        break;
                    case "multiply":
                        session.SetString("Captcha" + prefix,(a*b).ToString());
                        captcha = $"{a} x {b} = ?";
                        break;
                    default:
                        session.SetString("Captcha" + prefix,(a+b).ToString());
                        captcha = $"{a} + {b} = ?";
                        break;
                }

                using var mem = new MemoryStream();
                using var bmp = new Bitmap(240, 60);
                using (var gfx = Graphics.FromImage(bmp))
                {
                    gfx.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    gfx.SmoothingMode = SmoothingMode.AntiAlias;
                    gfx.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));

                    //add noise
                    int i;
                    var pen = new Pen(Color.LightYellow);
                    for (i = 1; i < 10; i++)
                    {
                        pen.Color = Color.FromArgb(
                            (rand.Next(0, 255)),
                            (rand.Next(0, 255)),
                            (rand.Next(0, 255)));

                        int r = rand.Next(0, (240 / 3));
                        int x = rand.Next(0, 240);
                        int y = rand.Next(0, 60);

                        x -= r;
                        y -= r;
                        gfx.DrawEllipse(pen, x, y, r, r);
                    }

                    //add question
                    gfx.DrawString(captcha, new Font("Tahoma", 28), Brushes.OrangeRed, 10, 10);

                    //render as Jpeg
                    bmp.Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);
                    //img = File(mem.GetBuffer(), "image/Jpeg");

                    byte[] imageBytes = mem.ToArray();
                    return File(imageBytes,"image/jpg");
                }

            }
            catch (Exception)
            {
                return Content("");
                //throw new HttpException(404, "Captcha image Not found");
            }

        }
        #endregion
    }
}