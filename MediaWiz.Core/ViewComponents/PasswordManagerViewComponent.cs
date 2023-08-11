using System.Threading.Tasks;
using MediaWiz.Forums.Models;
using Microsoft.AspNetCore.Mvc;

namespace MediaWiz.Forums.ViewComponents
{
    public class PasswordManagerViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string Template,ForumForgotPasswordModel Model)
        {

            switch (Template)
            {
                case "ChangePassword" :
                    return await Task.FromResult((IViewComponentResult)View(Template,new Umbraco.Cms.Core.Models.ChangingPasswordModel()));

                case "ForgotPassword" :
                    return await Task.FromResult((IViewComponentResult)View(Template, Model ??= new ForumForgotPasswordModel()));
                case "ResetPassword" :
                    return await Task.FromResult((IViewComponentResult)View(Template));

            }
            return await Task.FromResult((IViewComponentResult)View(Template));
        }
    }


}
