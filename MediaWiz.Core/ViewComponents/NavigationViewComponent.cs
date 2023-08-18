using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace MediaWiz.Forums.ViewComponents
{
    public class NavigationViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string Template, IPublishedContent Model)
        {
            if(Model != null)
                return await Task.FromResult((IViewComponentResult)View(Template, Model));
            else
            {
                return await Task.FromResult((IViewComponentResult)View(Template));
            }
        }

    }
}
