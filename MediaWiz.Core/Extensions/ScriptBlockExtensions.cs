using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Umbraco.Extensions;

namespace MediaWiz.Forums.Extensions
{
    /// <summary>
    /// Helper extensions add Javascript and CSS blocks in partial views
    /// Blocks will be rendered in the master template using RenderPartialViewXXXXBlocks
    /// </summary>
    public static class ScriptBlockExtensions
    {
        #region PartialView Scriptblocks
        private const string SCRIPTBLOCK_BUILDER = "ScriptBlockBuilder";
        private const string CSSBLOCK_BUILDER = "CssBlockBuilder";

        /// <summary>
        /// Defines a javascript block for renderring in Partial views into the main template body
        /// 
        /// DO NOT USE THIS FOR CSS FILES
        /// </summary>
        /// <param name="helper">HtmlHelper</param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static ScriptBlock PartialViewScriptBlock(this IHtmlHelper helper,Func<dynamic, HelperResult> template)
        {
            var scriptBuilder = helper.ViewContext.HttpContext.Items[SCRIPTBLOCK_BUILDER] as StringBuilder ?? new StringBuilder();
            scriptBuilder.Append(template(null).ToHtmlString());
            helper.ViewContext.HttpContext.Items[SCRIPTBLOCK_BUILDER] = scriptBuilder;
            return new ScriptBlock(helper.ViewContext);
        }

        /// <summary>
        /// Defines a CSS block for renderring in Partial views into the main template body
        /// </summary>
        /// <param name="helper">HtmlHelper</param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static CssBlock PartialViewCSSBlock(this IHtmlHelper helper,Func<dynamic, HelperResult> template)
        {
            var scriptBuilder = helper.ViewContext.HttpContext.Items[CSSBLOCK_BUILDER] as StringBuilder ?? new StringBuilder();
            scriptBuilder.Append(template(null).ToHtmlString());
            helper.ViewContext.HttpContext.Items[CSSBLOCK_BUILDER] = scriptBuilder;
            return new CssBlock(helper.ViewContext);
        }

        /// <summary>
        /// Extension to Render Scriptblocks from child views
        /// </summary>
        /// <param name="webPage"></param>
        /// <returns>MvcHtmlString</returns>
        public static IHtmlContent RenderScriptBlocks(this IHtmlHelper helper)
        {
            if (helper.ViewContext.HttpContext.Items.TryGetValue(SCRIPTBLOCK_BUILDER, out var scriptsData) && scriptsData is StringBuilder)
            {
                var scripts = new List<object>();
                scripts.Add(scriptsData);
                return new HtmlString(scriptsData.ToString());
            }

            return HtmlString.Empty;
        }
        /// <summary>
        /// Extension to Render CSS blocks from child views
        /// </summary>
        /// <param name="webPage"></param>
        /// <returns>MvcHtmlString</returns>
        public static IHtmlContent RenderCSSBlocks(this IHtmlHelper helper)
        {
            //if (helper.ViewContext.HttpContext.Items.TryGetValue(CSSBLOCK_BUILDER, out var scriptsData) && scriptsData is List<object> scripts)
            //    return new HtmlContentBuilder(scripts);
            if (helper.ViewContext.HttpContext.Items.TryGetValue(CSSBLOCK_BUILDER, out var scriptsData) && scriptsData is StringBuilder)
            {
                var scripts = new List<object>();
                scripts.Add(scriptsData);
                return new HtmlString(scriptsData.ToString());
            }

            return HtmlString.Empty;
        }
        #endregion

        public class CssBlock : IDisposable
        {
            private ViewContext _viewContext;
            private TextWriter _originalWriter;
            private StringWriter _scriptWriter;
            private bool _disposed;

            public CssBlock(ViewContext viewContext)
            {
                _viewContext = viewContext;
                _originalWriter = viewContext.Writer;

                // replace writer
                viewContext.Writer = _scriptWriter = new StringWriter();
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                try
                {
                    List<object> scripts = null;
                    if (_viewContext.HttpContext.Items.TryGetValue(CSSBLOCK_BUILDER, out var scriptsData))
                        scripts = scriptsData as List<object>;
                    if (scripts == null)
                        _viewContext.HttpContext.Items[CSSBLOCK_BUILDER] = scripts = new List<object>();

                    scripts.Add(new HtmlString(_scriptWriter.ToString()));
                }
                finally
                {
                    // restore the original writer
                    _viewContext.Writer = _originalWriter;
                    _disposed = true;
                }
            }
        }
        public class ScriptBlock : IDisposable
        {
            private ViewContext _viewContext;
            private TextWriter _originalWriter;
            private StringWriter _scriptWriter;
            private bool _disposed;

            public ScriptBlock(ViewContext viewContext)
            {
                _viewContext = viewContext;
                _originalWriter = viewContext.Writer;

                // replace writer
                viewContext.Writer = _scriptWriter = new StringWriter();
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                try
                {
                    List<object> scripts = null;
                    if (_viewContext.HttpContext.Items.TryGetValue(SCRIPTBLOCK_BUILDER, out var scriptsData))
                        scripts = scriptsData as List<object>;
                    if (scripts == null)
                        _viewContext.HttpContext.Items[SCRIPTBLOCK_BUILDER] = scripts = new List<object>();

                    scripts.Add(new HtmlString(_scriptWriter.ToString()));
                }
                finally
                {
                    // restore the original writer
                    _viewContext.Writer = _originalWriter;
                    _disposed = true;
                }
            }
        }
    }


}