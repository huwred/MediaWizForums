using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace MediaWiz.Forums.Controllers
{
    public class ResetController : RenderController
    {
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ServiceContext _serviceContext;

        public ResetController(ILogger<ResetController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor, IVariationContextAccessor variationContextAccessor,ServiceContext context) : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _variationContextAccessor = variationContextAccessor;
            _serviceContext = context;
        }
        public override IActionResult Index()
        {
            VerifyViewModel pageViewModel = new VerifyViewModel(CurrentPage,
                new PublishedValueFallback(_serviceContext, _variationContextAccessor))
            {
                ValidatedMember = null
            };
            return CurrentTemplate(pageViewModel);
        }
        [HttpGet]
        public IActionResult Index([FromQuery(Name = "token")] string token,[FromQuery(Name = "id")] string memberid)
        {
            if (token != null)
            {
                VerifyViewModel ResetViewModel = new VerifyViewModel(CurrentPage,
                    new PublishedValueFallback(_serviceContext, _variationContextAccessor))
                {
                    ResetToken = token, MemberId = memberid
                };
                return CurrentTemplate(ResetViewModel);
            }

            VerifyViewModel viewModel = new VerifyViewModel(CurrentPage,
                new PublishedValueFallback(_serviceContext, _variationContextAccessor))
            {
                ValidatedMember = null
            };
            return CurrentTemplate(viewModel);
        }

    }


}