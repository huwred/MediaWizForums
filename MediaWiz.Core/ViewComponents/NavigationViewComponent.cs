using System.Threading.Tasks;
using MediaWiz.Forums.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MediaWiz.Forums.ViewComponents
{
    public class NavigationViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string Template, SearchViewModel Model)
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
