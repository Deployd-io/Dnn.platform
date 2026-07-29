// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc.Helpers
{
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    /// <summary>Represents the functionality to render a partial view as an HTML-encoded string.</summary>
    public static class HtmlPartialExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Renders the specified partial view as an HTML-encoded string.</summary>
        ///
        /// <returns>
        /// The partial view that is rendered as an HTML-encoded string.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="partialViewName">The name of the partial view to render.</param>
        public static MvcHtmlString Partial(this DnnHtmlHelper html, string partialViewName)
        {
			Logger.LogDebug(">>> Entering Partial(html={Html},partialViewName={PartialViewName})", html, partialViewName);
			Logger.LogDebug("<<< Exiting Partial(html={Html},partialViewName={PartialViewName})", html, partialViewName);
            return html.HtmlHelper.Partial(partialViewName);
        }

        /// <summary>Renders the specified partial view as an HTML-encoded string.</summary>
        ///
        /// <returns>
        /// The partial view that is rendered as an HTML-encoded string.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="partialViewName">The name of the partial view to render.</param><param name="viewData">The view data dictionary for the partial view.</param>
        public static MvcHtmlString Partial(this DnnHtmlHelper html, string partialViewName, ViewDataDictionary viewData)
        {
			Logger.LogDebug("<<< Exiting Partial(html={Html},partialViewName={PartialViewName},viewData={ViewData})", html, partialViewName, viewData);
            return html.HtmlHelper.Partial(partialViewName, (object)null, viewData);
        }

        /// <summary>Renders the specified partial view as an HTML-encoded string.</summary>
        ///
        /// <returns>
        /// The partial view that is rendered as an HTML-encoded string.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="partialViewName">The name of the partial view to render.</param><param name="model">The model for the partial view.</param>
        public static MvcHtmlString Partial(this DnnHtmlHelper html, string partialViewName, object model)
        {
			Logger.LogDebug("<<< Exiting Partial(html={Html},partialViewName={PartialViewName},model={Model})", html, partialViewName, model);
            return html.HtmlHelper.Partial(partialViewName, model, html.HtmlHelper.ViewData);
        }

        /// <summary>Renders the specified partial view as an HTML-encoded string.</summary>
        ///
        /// <returns>
        /// The partial view that is rendered as an HTML-encoded string.
        /// </returns>
        /// <param name="html">The HTML helper instance that this method extends.</param><param name="partialViewName">The name of the partial view.</param><param name="model">The model for the partial view.</param><param name="viewData">The view data dictionary for the partial view.</param>
        public static MvcHtmlString Partial(this DnnHtmlHelper html, string partialViewName, object model, ViewDataDictionary viewData)
        {
			Logger.LogDebug("<<< Exiting Partial(html={Html},partialViewName={PartialViewName},model={Model},viewData={ViewData})", html, partialViewName, model, viewData);
            return html.HtmlHelper.Partial(partialViewName, model, viewData);
        }
    }
}
