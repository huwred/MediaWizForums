using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaWiz.Forums.Models;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace MediaWiz.Forums.ViewComponents
{
    public class ForumViewComponent : ViewComponent
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IMemberManager _memberManager;
        private readonly IMemberGroupService _groupService;
        public ForumViewComponent(IUmbracoContextAccessor umbracoContextAccessor, IMemberManager memberManager,IMemberGroupService groupService)
        {
            _groupService = groupService;
            _memberManager = memberManager;
            _umbracoContextAccessor = umbracoContextAccessor;
            
        }
        public async Task<IViewComponentResult> InvokeAsync(string Template, int ParentId)
        {
            var currentpage = _umbracoContextAccessor?.GetRequiredUmbracoContext()?.PublishedRequest?.PublishedContent;

            switch (Template)
            {
                case "Create":
                    var Forum = new ForumsForumModel
                    {
                        ParentId = ParentId,
                        AllowPosts = true
                    };
                    return await Task.FromResult((IViewComponentResult)View("Create",Forum));
                case "List":
                    var user = _memberManager.GetCurrentMemberAsync().Result;
                    IList<string> roles = new List<string>();
                    if (user != null)
                    {
                        roles = _memberManager.GetRolesAsync(user).Result;
                    }

                    var model = new ForumListViewModel(_memberManager,_groupService)
                    {
                        Content = currentpage,
                        Forums = currentpage.Children.Where(x => x.IsDocumentType("Forum") && x.IsVisible() && x.Value<bool>("isActive") && (x.Value<int?>("membersOnly") != 1 || (x.Value<int?>("membersOnly") == 1 && _memberManager.IsLoggedIn()))).ToList(),
                        Roles = roles,
                        IsLoggedIn = _memberManager.IsLoggedIn(),
                        User = user
                    };
                    return await Task.FromResult((IViewComponentResult)View(Template,model));

            }
            return await Task.FromResult((IViewComponentResult)View(Template));
        }
    }

    public class ForumListViewModel
    {
        public IList<string> Roles { get; set; }
        public MemberIdentityUser User { get; set; }
        public bool IsLoggedIn { get; set; }
        public IPublishedContent Content { get; set; }
        public List<IPublishedContent> Forums { get; set; }

        public ForumListViewModel(IMemberManager memberManager, IMemberGroupService groupService)
        {
            _groupService = groupService;
            _memberManager = memberManager;
        }
        private readonly IMemberManager _memberManager;
        private readonly IMemberGroupService _groupService;
        public bool CanView(IPublishedContent model)
        {

            var canViewGroups = model.Value<string>("canViewGroups");
            //all members allowed
            if (String.IsNullOrWhiteSpace(canViewGroups))
                return true;

            var allowedGroupList = new List<string>();
            foreach (var memberGroupStr in canViewGroups.Split(','))
            {
                var memberGroup = _groupService.GetById(Convert.ToInt32(memberGroupStr));
                if (memberGroup != null)
                {
                    allowedGroupList.Add(memberGroup.Name);
                }
            }
            //check if member is one of the allowed groups

            return _memberManager.IsMemberAuthorizedAsync(null, allowedGroupList).Result;
        }
        public string PostRestriction(IPublishedContent item)
        {

            var canPostGroups = item.Value<string>("canPostGroup");

            // default(blank list) is anyone can post
            if (string.IsNullOrWhiteSpace(canPostGroups))
                return "";

            // is the user a member of a group
            // is the user in any of those groups ?
            var allowedGroupList = new List<string>();
            foreach (var memberGroupStr in canPostGroups.Split(','))
            {
                var memberGroup = _groupService.GetById(Convert.ToInt32(memberGroupStr));
                if (memberGroup != null)
                {
                    allowedGroupList.Add(memberGroup.Name);
                }
            }
            return string.Join(", ", allowedGroupList);
        }
    }
}
