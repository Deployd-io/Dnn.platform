// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc.Helpers
{
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    public static class HtmlRenderPartialExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Renders the specified partial view by using the specified HTML helper.</summary>
        /// <param name="html">The HTML helper.</param><param name="partialViewName">The name of the partial view.</param>
        public static void RenderPartial(this DnnHtmlHelper html, string partialViewName)
        {
			Logger.LogDebug(">>> Entering RenderPartial(html={Html},partialViewName={PartialViewName})", html, partialViewName);
            html.HtmlHelper.RenderPartial(partialViewName, html.HtmlHelper.ViewData);
			Logger.LogDebug("<<< Exiting RenderPartial(html={Html},partialViewName={PartialViewName})", html, partialViewName);
        }

        /// <summary>Renders the specified partial view, replacing its ViewData property with the specified <see cref="System.Web.Mvc.ViewDataDictionary"/> object.</summary>
        /// <param name="html">The HTML helper.</param><param name="partialViewName">The name of the partial view.</param><param name="viewData">The view data.</param>
        public static void RenderPartial(this DnnHtmlHelper html, string partialViewName, ViewDataDictionary viewData)
        {
			Logger.LogDebug(">>> Entering RenderPartial(html={Html},partialViewName={PartialViewName},viewData={ViewData})", html, partialViewName, viewData);
            html.HtmlHelper.RenderPartial(partialViewName, viewData);
			Logger.LogDebug("<<< Exiting RenderPartial(html={Html},partialViewName={PartialViewName},viewData={ViewData})", html, partialViewName, viewData);
        }

        /// <summary>Renders the specified partial view, passing it a copy of the current <see cref="System.Web.Mvc.ViewDataDictionary"/> object, but with the Model property set to the specified model.</summary>
        /// <param name="html">The HTML helper.</param><param name="model">The model.</param><param name="partialViewName">The name of the partial view.</param>
        public static void RenderPartial(this DnnHtmlHelper html, object model, string partialViewName)
        {
			Logger.LogDebug(">>> Entering RenderPartial(html={Html},model={Model},partialViewName={PartialViewName})", html, model, partialViewName);
            html.HtmlHelper.RenderPartial(partialViewName, model, html.HtmlHelper.ViewData);
			Logger.LogDebug("<<< Exiting RenderPartial(html={Html},model={Model},partialViewName={PartialViewName})", html, model, partialViewName);
        }

        /// <summary>Renders the specified partial view, replacing the partial view's ViewData property with the specified <see cref="System.Web.Mvc.ViewDataDictionary"/> object and setting the Model property of the view data to the specified model.</summary>
        /// <param name="html">The HTML helper.</param><param name="partialViewName">The name of the partial view.</param><param name="model">The model for the partial view.</param><param name="viewData">The view data for the partial view.</param>
        public static void RenderPartial(this DnnHtmlHelper html, string partialViewName, object model, ViewDataDictionary viewData)
        {
			Logger.LogDebug(">>> Entering RenderPartial(html={Html},partialViewName={PartialViewName},model={Model},viewData={ViewData})", html, partialViewName, model, viewData);
            html.HtmlHelper.RenderPartial(partialViewName, model, viewData);
			Logger.LogDebug("<<< Exiting RenderPartial(html={Html},partialViewName={PartialViewName},model={Model},viewData={ViewData})", html, partialViewName, model, viewData);
        }
    }
}
