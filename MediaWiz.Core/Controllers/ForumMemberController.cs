using System;
using System.Linq;
using System.Threading.Tasks;
using MediaWiz.Forums.Extensions;
using MediaWiz.Forums.Helpers;
using MediaWiz.Forums.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Models;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Cms.Web.Website.Models;

namespace MediaWiz.Forums.Controllers
{
    public class ForumMemberController : UmbRegisterController
    {

        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IForumMailService _mailService;
        private readonly IMemberService _memberService;
        private readonly IMemberManager _memberManager;
        private readonly IMemberSignInManager _memberSignInManager;
        private readonly ILocalizationService _localizationService;

        public ForumMemberController(IMemberManager memberManager, IMemberService memberService, IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, IMemberSignInManager memberSignInManager, IScopeProvider scopeProvider,ILogger<ForumMemberController> logger,IHttpContextAccessor httpContextAccessor,IForumMailService mailService,ILocalizationService localizationService) 
            : base(memberManager, memberService, umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider, memberSignInManager, scopeProvider)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mailService = mailService;
            _memberService = memberService;
            _memberManager = memberManager;
            _memberSignInManager = memberSignInManager;
            _localizationService = localizationService;
        }

        [HttpPost]
        public async Task<IActionResult> HandleLoginAsync(LoginModel login)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            var validate = await _memberManager.ValidateCredentialsAsync(login.Username, login.Password);
            if (validate)
            {
                var result = await _memberSignInManager.PasswordSignInAsync(login.Username, login.Password, login.RememberMe, true);
                if (result.Succeeded)
                {
                    if (Url.IsLocalUrl(login.RedirectUrl))
                    {
                        return Redirect(login.RedirectUrl);
                    }

                    return RedirectToCurrentUmbracoPage();
                }

            }
            else
            {
                ModelState.AddModelError(string.Empty,_localizationService.GetOrCreateDictionaryValue("Forums.Error.InvalidCredentials","The username or password provided is incorrect.") );
            }
            // If there is a specified path to redirect to then use it.

            return CurrentUmbracoPage();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterMeAsync([Bind(Prefix = "registerModel")] Umbraco.Cms.Web.Website.Models.RegisterModel newmember)
        {

            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }
            var usernamecheck = _memberService.GetByUsername(newmember.Username);
            if (usernamecheck != null)
            {
                ModelState.AddModelError("Registration",_localizationService.GetOrCreateDictionaryValue("Forums.Error.DuplicateUsername","The username is already in use, please use another") );
                return CurrentUmbracoPage();
            }

            var identityUser = MemberIdentityUser.CreateNew(newmember.Username, newmember.Email, "forumMember", isApproved: false, newmember.Name);
            IdentityResult identityResult = await _memberManager.CreateAsync(
                identityUser,
                newmember.Password);
            var member = _memberService.GetByEmail(identityUser.Email);

            string resetGuid = null;
            if (member != null)
            {
                resetGuid = ForumHelper.GenerateUniqueCode(16);
                member.SetValue("resetGuid", resetGuid);
                //member.IsApproved = false;
                foreach (MemberPropertyModel property in newmember.MemberProperties.Where(p => p.Value != null)
                    .Where(property => member.Properties.Contains(property.Alias)))
                {
                    member.Properties[property.Alias]?.SetValue(property.Value);
                }
            }

            _memberService.Save(member);
            try
            {
                var umbracoHelper = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<UmbracoHelper>();

                TempData["FormSuccess"] = await _mailService.SendVerifyAccount(member.Email, resetGuid); ;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Problem sending Validation email");
                throw;
            }


            // If there is a specified path to redirect to then use it.
            if (string.IsNullOrWhiteSpace(newmember.RedirectUrl) == false)
            {
                return Redirect(newmember.RedirectUrl!);
            }
            return CurrentUmbracoPage();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword([Bind(Prefix = "registerModel")] RegisterModel changePassword, int id, string token)
        {
            //do the passwords match
            if (changePassword.Password != changePassword.ConfirmPassword)
            {
                TempData["ValidationError"] = _localizationService.GetOrCreateDictionaryValue("Forums.Error.PasswordMismatch","Passwords do not match!");
                return CurrentUmbracoPage();
            }
            try
            {

                var member = _memberService.GetById(id);
                if (member == null)
                {
                    TempData["ValidationError"] = _localizationService.GetOrCreateDictionaryValue("Forums.Error.NoMember","Invalid member");
                }
                else
                {
                    if (member.HasProperty("resetGuid"))
                    {
                        if (member.GetValue<string>("resetGuid") != token)
                        {
                            TempData["ValidationError"] = _localizationService.GetOrCreateDictionaryValue("Forums.Error.InvalidToken","Token not found");
                            return CurrentUmbracoPage();
                        }
                    }

                    var user = _memberManager.FindByIdAsync(member.Id.ToString()).Result;

                    var changePasswordResult =
                        await _memberManager.ChangePasswordWithResetAsync(user.Id, token, changePassword.Password);
                    if (changePasswordResult.Succeeded)
                    {
                        TempData["ValidationSuccess"] =  "success";
                    }
                    else
                    {
                        foreach (var identityError in changePasswordResult.Errors)
                        {
                            TempData["ValidationError"] += identityError.Description;
                        }
                    }
                }


            }
            catch (Exception e)
            {
                TempData["ValidationError"] = e.Message;
            }


            return CurrentUmbracoPage();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangingPasswordModel model)
        {
            var passwordvalid = _memberManager.ValidatePasswordAsync(model.NewPassword).Result;
            if (passwordvalid.Succeeded)
            {
                try
                {
                    var member = _memberManager.GetCurrentMemberAsync().Result;
                    var changePasswordResult =
                        await _memberManager.ChangePasswordAsync(member, model.OldPassword, model.NewPassword);
                    if (changePasswordResult.Succeeded)
                    {
                        TempData["ValidationSuccess"] = "success";
                    }
                    else
                    {
                        foreach (var identityError in changePasswordResult.Errors)
                        {
                            TempData["ValidationError"] += identityError.Description;
                        }
                    }
                    
                }
                catch (Exception e)
                {
                    TempData["ValidationError"] = e.Message;
                }
            }
            else
            {
                TempData["ValidationError"] = passwordvalid.Errors.ToString();
            }

            return CurrentUmbracoPage();
        }
    }
}
