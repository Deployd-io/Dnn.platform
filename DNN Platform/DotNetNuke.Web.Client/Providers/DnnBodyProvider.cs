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
    using System.Web.UI.HtmlControls;

    using ClientDependency.Core;
    using ClientDependency.Core.Config;
    using ClientDependency.Core.FileRegistration.Providers;

    /// <summary>Registers resources at the top of the body on default.aspx.</summary>
    public class DnnBodyProvider : DnnFileRegistrationProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>The name of the provider.</summary>
        public const string DefaultName = "DnnBodyProvider";

        /// <summary>The name of the placeholder in which the controls will be rendered.</summary>
        public const string DnnBodyPlaceHolderName = "BodySCRIPTS";

        /// <summary>Initializes the provider.</summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="System.ArgumentNullException">The name of the provider is null.</exception>
        /// <exception cref="System.ArgumentException">The name of the provider has a length of zero.</exception>
        /// <exception cref="System.InvalidOperationException">An attempt is made to call <see cref="System.Configuration.Provider.ProviderBase.Initialize"/> on a provider after the provider has already been initialized.</exception>
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
			Logger.LogDebug(">>> Entering Initialize(name={Name},config={Config})", name, config);
            // Assign the provider a default name if it doesn't have one
            if (string.IsNullOrEmpty(name))
            {
                name = DefaultName;
            }
			Logger.LogDebug("Initialize(name={Name},config={Config}): name → {Name1}", name, config, name);

            base.Initialize(name, config);
			Logger.LogDebug("<<< Exiting Initialize(name={Name},config={Config})", name, config);
        }

        /// <inheritdoc />
        protected override string RenderJsDependencies(IEnumerable<IClientDependencyFile> jsDependencies, HttpContextBase http, IDictionary<string, string> htmlAttributes)
        {
            if (!jsDependencies.Any())
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

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
                foreach (var s in comp)
                {
                    sb.Append(this.RenderSingleJsFile(s, htmlAttributes));
                }
            }

            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string RenderSingleJsFile(string js, IDictionary<string, string> htmlAttributes)
        {
            return string.Format(CultureInfo.InvariantCulture, HtmlEmbedContants.ScriptEmbedWithSource, js, htmlAttributes.ToHtmlAttributes());
        }

        /// <inheritdoc />
        protected override string RenderCssDependencies(IEnumerable<IClientDependencyFile> cssDependencies, HttpContextBase http, IDictionary<string, string> htmlAttributes)
        {
            if (!cssDependencies.Any())
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

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
                foreach (var s in comp)
                {
                    sb.Append(this.RenderSingleCssFile(s, htmlAttributes));
                }
            }

            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string RenderSingleCssFile(string css, IDictionary<string, string> htmlAttributes)
        {
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
                throw new InvalidOperationException("The current HttpHandler in a WebFormsFileRegistrationProvider must be of type Page");
            }

            if (page.Header == null)
            {
                throw new InvalidOperationException("DnnBodyProvider requires a runat='server' tag in the page's header tag");
            }

            var jsScriptBlock = new LiteralControl(js.Replace("&", "&amp;"));
            var cssStyleBlock = new LiteralControl(css.Replace("&", "&amp;"));
			Logger.LogDebug("RegisterDependencies(http={Http},js={Js},css={Css}): jsScriptBlock → {JsScriptBlock}", http, js, css, jsScriptBlock);
			Logger.LogDebug("RegisterDependencies(http={Http},js={Js},css={Css}): cssStyleBlock → {CssStyleBlock}", http, js, css, cssStyleBlock);

            var holderControl = page.FindControl(DnnBodyPlaceHolderName);
            holderControl.Controls.Add(jsScriptBlock);
			Logger.LogDebug("RegisterDependencies(http={Http},js={Js},css={Css}): holderControl → {HolderControl}", http, js, css, holderControl);
            holderControl.Controls.Add(cssStyleBlock);

            var form = (HtmlForm)page.FindControl("Form");
            if (form != null)
			Logger.LogDebug("RegisterDependencies(http={Http},js={Js},css={Css}): form → {Form}", http, js, css, form);
            {
                form.Controls.Remove(holderControl);
                form.Controls.AddAt(0, holderControl);
            }

            var scriptManager = ScriptManager.GetCurrent(page);
            if (scriptManager is { IsInAsyncPostBack: true })
			Logger.LogDebug("RegisterDependencies(http={Http},js={Js},css={Css}): scriptManager → {ScriptManager}", http, js, css, scriptManager);
            {
                holderControl.ID = "$crm_" + holderControl.ID;
				Logger.LogDebug("RegisterDependencies(http={Http},js={Js},css={Css}): holderControl.ID → {Arg3}", http, js, css, holderControl.ID);
                scriptManager.RegisterDataItem(holderControl, $"{jsScriptBlock.Text}{cssStyleBlock.Text}");
            }
			Logger.LogDebug("<<< Exiting RegisterDependencies(http={Http},js={Js},css={Css})", http, js, css);
        }
    }
}
