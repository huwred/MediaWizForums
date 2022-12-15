using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace MediaWiz.Forums.Composers
{
    //[RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class ViewCounterComposer : ComponentComposer<PostViewsComponent>
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);
        }
    }
}