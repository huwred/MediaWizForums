using System.Threading.Tasks;
using MediaWiz.Forums.Models;
using Microsoft.AspNetCore.Mvc;

namespace MediaWiz.Forums.ViewComponents
{
    public class ForumViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string Template, int ParentId)
        {
            switch (Template)
            {
                case "Create":
                    var Forum = new ForumsForumModel
                    {
                        ParentId = ParentId,
                        AllowPosts = true
                    };
                    return await Task.FromResult((IViewComponentResult)View("Create",Forum));

            }
            return await Task.FromResult((IViewComponentResult)View(Template));
        }
    }
}
