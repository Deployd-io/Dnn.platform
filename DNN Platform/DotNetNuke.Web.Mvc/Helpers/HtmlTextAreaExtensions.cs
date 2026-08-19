// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    public static class HtmlTextAreaExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(HtmlTextAreaExtensions));
        private const int TextAreaRows = 2;
        private const int TextAreaColumns = 20;

        public static MvcHtmlString TextArea(this DnnHtmlHelper html, string name)
        {
			Logger.LogDebug(">>> Entering TextArea(html={Html},name={Name})", html, name);
            var htmlHelper = html.HtmlHelper;
			Logger.LogDebug("<<< Exiting TextArea(html={Html},name={Name})", html, name);
			Logger.LogDebug("<<< Exiting TextArea(html={Html},name={Name},htmlAttributes={HtmlAttributes})", html, name, htmlAttributes);
			Logger.LogDebug("<<< Exiting TextArea(html={Html},name={Name},value={Value})", html, name, value);
			Logger.LogDebug("<<< Exiting TextArea(html={Html},name={Name},value={Value},htmlAttributes={HtmlAttributes})", html, name, value, htmlAttributes);
			Logger.LogDebug("<<< Exiting TextArea(html={Html},name={Name},value={Value},rows={Rows},columns={Columns},htmlAttributes={HtmlAttributes})", html, name, value, rows, columns, htmlAttributes);
            return htmlHelper.TextArea(name);
        }

        public static MvcHtmlString TextArea(this DnnHtmlHelper html, string name, object htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper;
            return htmlHelper.TextArea(name, htmlAttributes);
        }

        public static MvcHtmlString TextArea(this DnnHtmlHelper html, string name, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper;
            return htmlHelper.TextArea(name, htmlAttributes);
        }

        public static MvcHtmlString TextArea(this DnnHtmlHelper html, string name, string value)
        {
            var htmlHelper = html.HtmlHelper;
            return htmlHelper.TextArea(name, value);
        }

        public static MvcHtmlString TextArea(this DnnHtmlHelper html, string name, string value, object htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper;
            return htmlHelper.TextArea(name, value, htmlAttributes);
        }

        public static MvcHtmlString TextArea(this DnnHtmlHelper html, string name, string value, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper;
            return htmlHelper.TextArea(name, value, htmlAttributes);
        }

        public static MvcHtmlString TextArea(this DnnHtmlHelper html, string name, string value, int rows, int columns, object htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper;
            return htmlHelper.TextArea(name, value, rows, columns, htmlAttributes);
        }

        public static MvcHtmlString TextArea(this DnnHtmlHelper html, string name, string value, int rows, int columns, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper;
            return htmlHelper.TextArea(name, value, rows, columns, htmlAttributes);
        }

        public static MvcHtmlString TextAreaFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
			Logger.LogDebug(">>> Entering TextAreaFor(html={Html},expression={Expression})", html, expression);
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
			Logger.LogDebug("<<< Exiting TextAreaFor(html={Html},expression={Expression})", html, expression);
			Logger.LogDebug("<<< Exiting TextAreaFor(html={Html},expression={Expression},htmlAttributes={HtmlAttributes})", html, expression, htmlAttributes);
			Logger.LogDebug("<<< Exiting TextAreaFor(html={Html},expression={Expression},rows={Rows},columns={Columns},htmlAttributes={HtmlAttributes})", html, expression, rows, columns, htmlAttributes);
            return htmlHelper.TextAreaFor(expression, TextAreaRows, TextAreaColumns, null);
        }

        public static MvcHtmlString TextAreaFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.TextAreaFor(expression, TextAreaRows, TextAreaColumns, htmlAttributes);
        }

        public static MvcHtmlString TextAreaFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.TextAreaFor(expression, TextAreaRows, TextAreaColumns, htmlAttributes);
        }

        public static MvcHtmlString TextAreaFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, int rows, int columns, object htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.TextAreaFor(expression, rows, columns, htmlAttributes);
        }

        public static MvcHtmlString TextAreaFor<TModel, TProperty>(this DnnHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, int rows, int columns, IDictionary<string, object> htmlAttributes)
        {
            var htmlHelper = html.HtmlHelper as HtmlHelper<TModel>;
            return htmlHelper.TextAreaFor(expression, rows, columns, htmlAttributes);
        }
    }
}
