using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MediaWiz.Forums.ViewComponents
{
    public class TopicsViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string Template)
        {

            return await Task.FromResult((IViewComponentResult)View(Template));
        }
    }
}
