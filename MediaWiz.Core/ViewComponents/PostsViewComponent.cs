using System.Threading.Tasks;
using MediaWiz.Forums.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace MediaWiz.Forums.ViewComponents
{
    public class PostsViewComponent : ViewComponent
    {
        private readonly IContentService _contentService;
        private readonly IHttpContextAccessor _contextAccessor;

        public PostsViewComponent(IContentService contentService,IHttpContextAccessor contextAccessor)
        {
            _contentService = contentService;
            _contextAccessor = contextAccessor;

        }
        public async Task<IViewComponentResult> InvokeAsync(string Template, int Id,IPublishedContent Model, bool ShowTitle)
        {
            if (Template == "EditPostForm")
            {
            
                var post = _contentService.GetById(Id);
                var model = new ForumsPostModel
                {
                    Id = Id,
                    ParentId = post.ParentId,
                    Title = post.GetValue<string>("postTitle"),
                    Body = post.GetValue<string>("postBody"),
                    AuthorId = post.GetValue<int>("postAuthor"),
                    IsTopic = post.GetValue<bool>("postType"),
                    //string referer = Request.Headers["Referer"].ToString();
                    returnPath = _contextAccessor.HttpContext.Request.Headers["Referer"].ToString()
                };

                return await Task.FromResult((IViewComponentResult)View(Template,model));
            }else if (Template == "PostForm")
            {
                TempData["showTitle"] = ShowTitle;
                return await Task.FromResult((IViewComponentResult)View(Template,Model));
            }
            return await Task.FromResult((IViewComponentResult)View(Template));
        }
    }
}
