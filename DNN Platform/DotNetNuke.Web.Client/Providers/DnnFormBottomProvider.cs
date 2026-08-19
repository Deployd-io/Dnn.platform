// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.UI;

    using ClientDependency.Core;
    using ClientDependency.Core.Config;
    using ClientDependency.Core.FileRegistration.Providers;

    /// <summary>Registers resources at the top of the body on default.aspx.</summary>
    public class DnnFormBottomProvider : DnnFileRegistrationProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnFormBottomProvider));
        /// <summary>The default name of the provider.</summary>
        public const string DefaultName = "DnnFormBottomProvider";

        /// <summary>The name of the placeholder in which the controls will be rendered.</summary>
        public const string DnnFormBottomPlaceHolderName = "ClientResourcesFormBottom";

        /// <inheritdoc />
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
			Logger.LogDebug(">>> Entering Initialize(name={Name},config={Config})", name, config);
            // Assign the provider a default name if it doesn't have one
            if (string.IsNullOrEmpty(name))
            {
                name = DefaultName;
				Logger.LogDebug("Initialize(name={Name},config={Config}): name → {Name1}", name, config, name);
            }

            base.Initialize(name, config);
			Logger.LogDebug("<<< Exiting Initialize(name={Name},config={Config})", name, config);
        }

        /// <inheritdoc />
        protected override string RenderJsDependencies(IEnumerable<IClientDependencyFile> jsDependencies, HttpContextBase http, IDictionary<string, string> htmlAttributes)
        {
			Logger.LogDebug(">>> Entering RenderJsDependencies(jsDependencies={JsDependencies},http={Http},htmlAttributes={HtmlAttributes})", jsDependencies, http, htmlAttributes);
            if (!jsDependencies.Any())
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
			Logger.LogDebug("RenderJsDependencies(jsDependencies={JsDependencies},http={Http},htmlAttributes={HtmlAttributes}): sb → {Sb}", jsDependencies, http, htmlAttributes, sb);

            if (http.IsDebuggingEnabled || !this.EnableCompositeFiles)
            {
                foreach (var dependency in jsDependencies)
                {
                    sb.Append(this.RenderSingleJsFile(dependency.FilePath, htmlAttributes));
                }
            }
            else
            {
                var comp = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.ProcessCompositeList(jsDependencies, ClientDependencyType.Javascript, http);
				Logger.LogDebug("RenderJsDependencies(jsDependencies={JsDependencies},http={Http},htmlAttributes={HtmlAttributes}): comp → {Comp}", jsDependencies, http, htmlAttributes, comp);
                foreach (var s in comp)
                {
                    sb.Append(this.RenderSingleJsFile(s, htmlAttributes));
                }
            }

			Logger.LogDebug("<<< Exiting RenderJsDependencies(jsDependencies={JsDependencies},http={Http},htmlAttributes={HtmlAttributes})", jsDependencies, http, htmlAttributes);
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string RenderSingleJsFile(string js, IDictionary<string, string> htmlAttributes)
        {
			Logger.LogDebug(">>> Entering RenderSingleJsFile(js={Js},htmlAttributes={HtmlAttributes})", js, htmlAttributes);
			Logger.LogDebug("<<< Exiting RenderSingleJsFile(js={Js},htmlAttributes={HtmlAttributes})", js, htmlAttributes);
            return string.Format(CultureInfo.InvariantCulture, HtmlEmbedContants.ScriptEmbedWithSource, js, htmlAttributes.ToHtmlAttributes());
        }

        /// <inheritdoc />
        protected override string RenderCssDependencies(IEnumerable<IClientDependencyFile> cssDependencies, HttpContextBase http, IDictionary<string, string> htmlAttributes)
        {
			Logger.LogDebug(">>> Entering RenderCssDependencies(cssDependencies={CssDependencies},http={Http},htmlAttributes={HtmlAttributes})", cssDependencies, http, htmlAttributes);
            if (!cssDependencies.Any())
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
			Logger.LogDebug("RenderCssDependencies(cssDependencies={CssDependencies},http={Http},htmlAttributes={HtmlAttributes}): sb → {Sb}", cssDependencies, http, htmlAttributes, sb);

            if (http.IsDebuggingEnabled || !this.EnableCompositeFiles)
            {
                foreach (var dependency in cssDependencies)
                {
                    sb.Append(this.RenderSingleCssFile(dependency.FilePath, htmlAttributes));
                }
            }
            else
            {
                var comp = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.ProcessCompositeList(cssDependencies, ClientDependencyType.Css, http);
				Logger.LogDebug("RenderCssDependencies(cssDependencies={CssDependencies},http={Http},htmlAttributes={HtmlAttributes}): comp → {Comp}", cssDependencies, http, htmlAttributes, comp);
                foreach (var s in comp)
                {
                    sb.Append(this.RenderSingleCssFile(s, htmlAttributes));
                }
            }

			Logger.LogDebug("<<< Exiting RenderCssDependencies(cssDependencies={CssDependencies},http={Http},htmlAttributes={HtmlAttributes})", cssDependencies, http, htmlAttributes);
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string RenderSingleCssFile(string css, IDictionary<string, string> htmlAttributes)
        {
			Logger.LogDebug(">>> Entering RenderSingleCssFile(css={Css},htmlAttributes={HtmlAttributes})", css, htmlAttributes);
			Logger.LogDebug("<<< Exiting RenderSingleCssFile(css={Css},htmlAttributes={HtmlAttributes})", css, htmlAttributes);
            return string.Format(CultureInfo.InvariantCulture, HtmlEmbedContants.CssEmbedWithSource, css, htmlAttributes.ToHtmlAttributes());
        }

        /// <summary>Registers the dependencies in the body of default.aspx.</summary>
        /// <param name="http">The HTTP context.</param>
        /// <param name="js">The HTML markup to request JavaScript dependencies.</param>
        /// <param name="css">The HTML markup to request CSS dependencies.</param>
        /// <remarks>
        /// For some reason ampersands that aren't html escaped are not compliant to HTML standards when they exist in 'link' or 'script' tags in URLs,
        /// we need to replace the ampersands with &amp; . This is only required for this one w3c compliancy, the URL itself is a valid URL.
        ///
        /// </remarks>
        protected override void RegisterDependencies(HttpContextBase http, string js, string css)
        {
			Logger.LogDebug(">>> Entering RegisterDependencies(http={Http},js={Js},css={Css})", http, js, css);
            if (http.CurrentHandler is not Page page)
            {
				Logger.LogWarning("Throwing InvalidOperationException from RegisterDependencies(http={Http},js={Js},css={Css})", http, js, css);
                throw new InvalidOperationException("The current HttpHandler in a WebFormsFileRegistrationProvider must be of type Page");
            }

            if (page.Header == null)
            {
                throw new InvalidOperationException("DnnFormBottomProvider requires a runat='server' tag in the page's header tag");
            }

            var jsScriptBlock = new LiteralControl(js.Replace("&", "&amp;"));
			Logger.LogDebug("RegisterDependencies(http={Http},js={Js},css={Css}): jsScriptBlock → {JsScriptBlock}", http, js, css, jsScriptBlock);
            var cssStyleBlock = new LiteralControl(css.Replace("&", "&amp;"));
			Logger.LogDebug("RegisterDependencies(http={Http},js={Js},css={Css}): cssStyleBlock → {CssStyleBlock}", http, js, css, cssStyleBlock);
            page.FindControl(DnnFormBottomPlaceHolderName).Controls.Add(jsScriptBlock);
            page.FindControl(DnnFormBottomPlaceHolderName).Controls.Add(cssStyleBlock);
			Logger.LogDebug("<<< Exiting RegisterDependencies(http={Http},js={Js},css={Css})", http, js, css);
        }
    }
}
