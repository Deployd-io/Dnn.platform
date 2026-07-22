// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.WebPages;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Web.DDRMenu.DNNCommon;
    using DotNetNuke.Web.Razor;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An <see cref="ITemplateProcessor"/> for Razor files.</summary>
    /// <param name="hostSettings">The host settings.</param>
    public class RazorTemplateProcessor(IHostSettings hostSettings) : ITemplateProcessor
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
		Logger.LogDebug(">>> Entering RazorTemplateProcessor(hostSettings={HostSettings})", hostSettings);
		Logger.LogDebug("RazorTemplateProcessor(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);

        /// <summary>Initializes a new instance of the <see cref="RazorTemplateProcessor"/> class.</summary>
		Logger.LogDebug("<<< Exiting RazorTemplateProcessor(hostSettings={HostSettings})", hostSettings);
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public RazorTemplateProcessor()
            : this(null)
        {
        }

        /// <inheritdoc />
        public bool LoadDefinition(TemplateDefinition baseDefinition)
        {
			Logger.LogDebug(">>> Entering LoadDefinition(baseDefinition={BaseDefinition})", baseDefinition);
            var virtualPath = baseDefinition.TemplateVirtualPath;

            if (!virtualPath.EndsWith(".cshtml", StringComparison.InvariantCultureIgnoreCase) &&
                !virtualPath.EndsWith(".vbhtml", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

			Logger.LogDebug("<<< Exiting LoadDefinition(baseDefinition={BaseDefinition})", baseDefinition);
            return true;
        }

        /// <inheritdoc />
        public void Render(object source, HtmlTextWriter htmlWriter, TemplateDefinition liveDefinition)
        {
			Logger.LogDebug(">>> Entering Render(source={Source},htmlWriter={HtmlWriter},liveDefinition={LiveDefinition})", source, htmlWriter, liveDefinition);
            if (!string.IsNullOrEmpty(liveDefinition.TemplateVirtualPath))
            {
                var resolver = new PathResolver(this.hostSettings, liveDefinition.Folder);
                dynamic model = new ExpandoObject();
				Logger.LogDebug("Render(source={Source},htmlWriter={HtmlWriter},liveDefinition={LiveDefinition}): resolver → {Resolver}", source, htmlWriter, liveDefinition, resolver);
				Logger.LogDebug("Render(source={Source},htmlWriter={HtmlWriter},liveDefinition={LiveDefinition}): model.Source → {Arg3}", source, htmlWriter, liveDefinition, model.Source);
				Logger.LogDebug("Render(source={Source},htmlWriter={HtmlWriter},liveDefinition={LiveDefinition}): model.ControlID → {Arg3}", source, htmlWriter, liveDefinition, model.ControlID);
                model.Source = source;
                model.ControlID = DNNContext.Current.HostControl.ClientID;
                model.Options = ConvertToJson(liveDefinition.ClientOptions);
                model.DNNPath = resolver.Resolve("/", PathResolver.RelativeTo.Dnn);
                model.ManifestPath = resolver.Resolve("/", PathResolver.RelativeTo.Manifest);
                model.PortalPath = resolver.Resolve("/", PathResolver.RelativeTo.Portal);
                model.SkinPath = resolver.Resolve("/", PathResolver.RelativeTo.Skin);
                var modelDictionary = model as IDictionary<string, object>;
                liveDefinition.TemplateArguments.ForEach(a => modelDictionary.Add(a.Name, a.Value));
                htmlWriter.Write(this.RenderTemplate(liveDefinition.TemplateVirtualPath, model));
            }
			Logger.LogDebug("<<< Exiting Render(source={Source},htmlWriter={HtmlWriter},liveDefinition={LiveDefinition})", source, htmlWriter, liveDefinition);
        }

        protected static string ConvertToJson(List<ClientOption> options)
        {
			Logger.LogDebug(">>> Entering ConvertToJson(options={Options})", options);
            var result = new StringBuilder();
            result.Append("{");
			Logger.LogDebug("ConvertToJson(options={Options}): result → {Result}", options, result);

            if (options != null)
            {
                foreach (var option in options)
                {
                    if (option is ClientNumber)
                    {
                        result.AppendFormat("{0}:{1},", option.Name, Utilities.ConvertToJs(Convert.ToDecimal(option.Value)));
                    }
                    else if (option is ClientString)
                    {
                        result.AppendFormat("{0}:{1},", option.Name, Utilities.ConvertToJs(option.Value));
                    }
                    else if (option is ClientBoolean)
                    {
                        result.AppendFormat(
                            "{0}:{1},", option.Name, Utilities.ConvertToJs(Convert.ToBoolean(option.Value.ToLowerInvariant())));
                    }
                    else
                    {
                        result.AppendFormat("{0}:{1},", option.Name, option.Value);
                    }
                }

                if (options.Count > 0)
                {
                    result.Remove(result.Length - 1, 1);
                }
            }

            result.Append("}");
			Logger.LogDebug("<<< Exiting ConvertToJson(options={Options})", options);
            return result.ToString();
        }

        private StringWriter RenderTemplate(string virtualPath, dynamic model)
        {
            var page = WebPageBase.CreateInstanceFromVirtualPath(virtualPath);
            var httpContext = new HttpContextWrapper(HttpContext.Current);
            var pageContext = new WebPageContext(httpContext, page, model);

            var writer = new StringWriter();

            if (page is WebPage)
            {
                page.ExecutePageHierarchy(pageContext, writer);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var razorEngine = new RazorEngine(virtualPath, null, null);
#pragma warning restore CS0618 // Type or member is obsolete
                razorEngine.Render<dynamic>(writer, model);
            }

            return writer;
        }
    }
}
