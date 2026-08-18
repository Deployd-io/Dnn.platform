// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Xml;
    using System.Xml.Xsl;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Web.DDRMenu.DNNCommon;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An <see cref="ITemplateProcessor"/> for XSLT files.</summary>
    /// <param name="hostSettings">The host settings.</param>
    public class XsltTemplateProcessor(IHostSettings hostSettings) : ITemplateProcessor
    {
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private XslCompiledTransform xsl;

        /// <summary>Initializes a new instance of the <see cref="XsltTemplateProcessor"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public XsltTemplateProcessor()
            : this(null)
        {
		eventLogger.LogDebug(">>> Entering XsltTemplateProcessor()");
        }
		eventLogger.LogDebug("<<< Exiting XsltTemplateProcessor()");

        /// <inheritdoc />
        public bool LoadDefinition(TemplateDefinition baseDefinition)
        {
			eventLogger.LogDebug(">>> Entering LoadDefinition(baseDefinition={BaseDefinition})", baseDefinition);
            try
            {
                var virtualPath = baseDefinition.TemplateVirtualPath;
                if (!virtualPath.EndsWith(".xsl", StringComparison.InvariantCultureIgnoreCase) &&
                    !virtualPath.EndsWith(".xslt", StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                this.xsl = Utilities.CachedXslt(baseDefinition.TemplatePath);
				eventLogger.LogDebug("LoadDefinition(baseDefinition={BaseDefinition}): xsl → {Xsl}", baseDefinition, xsl);
                return true;
            }
            catch (Exception)
            {
				eventLogger.LogError("Exception in LoadDefinition(baseDefinition={BaseDefinition}): {Arg1}", baseDefinition, Exception.Message);
				eventLogger.LogDebug("<<< Exiting LoadDefinition(baseDefinition={BaseDefinition})", baseDefinition);
                return false;
            }
        }

        /// <inheritdoc />
        public void Render(object source, HtmlTextWriter htmlWriter, TemplateDefinition liveDefinition)
        {
			eventLogger.LogDebug(">>> Entering Render(source={Source},htmlWriter={HtmlWriter},liveDefinition={LiveDefinition})", source, htmlWriter, liveDefinition);
            var resolver = new PathResolver(this.hostSettings, liveDefinition.Folder);
            var hostPage = DNNContext.Current.Page;
			eventLogger.LogDebug("Render(source={Source},htmlWriter={HtmlWriter},liveDefinition={LiveDefinition}): resolver → {Resolver}", source, htmlWriter, liveDefinition, resolver);

            var args = new XsltArgumentList();
			eventLogger.LogDebug("Render(source={Source},htmlWriter={HtmlWriter},liveDefinition={LiveDefinition}): args → {Args}", source, htmlWriter, liveDefinition, args);
            args.AddExtensionObject("urn:ddrmenu", new XsltFunctions());
            args.AddExtensionObject("urn:dnngarden", new XsltFunctions());
            args.AddParam("ControlID", string.Empty, DNNContext.Current.HostControl.ClientID);
            args.AddParam("Options", string.Empty, ConvertToJson(liveDefinition.ClientOptions));
            args.AddParam("DNNPath", string.Empty, hostPage.ResolveUrl(resolver.Resolve("/", PathResolver.RelativeTo.Dnn)));
            args.AddParam("ManifestPath", string.Empty, hostPage.ResolveUrl(resolver.Resolve("/", PathResolver.RelativeTo.Manifest)));
            args.AddParam("PortalPath", string.Empty, hostPage.ResolveUrl(resolver.Resolve("/", PathResolver.RelativeTo.Portal)));
            args.AddParam("SkinPath", string.Empty, hostPage.ResolveUrl(resolver.Resolve("/", PathResolver.RelativeTo.Skin)));
            liveDefinition.TemplateArguments.ForEach(a => args.AddParam(a.Name, string.Empty, a.Value));

            HttpContext.Current.Items["Resolver"] = resolver;

            using var xmlStream = new MemoryStream();
			eventLogger.LogDebug("Render(source={Source},htmlWriter={HtmlWriter},liveDefinition={LiveDefinition}): xmlStream → {XmlStream}", source, htmlWriter, liveDefinition, xmlStream);
            Utilities.SerialiserFor(source.GetType()).Serialize(xmlStream, source);
            xmlStream.Seek(0, SeekOrigin.Begin);
            using var xmlReader = XmlReader.Create(xmlStream);
			eventLogger.LogDebug("Render(source={Source},htmlWriter={HtmlWriter},liveDefinition={LiveDefinition}): xmlReader → {XmlReader}", source, htmlWriter, liveDefinition, xmlReader);
            this.xsl.Transform(xmlReader, args, htmlWriter);
			eventLogger.LogDebug("<<< Exiting Render(source={Source},htmlWriter={HtmlWriter},liveDefinition={LiveDefinition})", source, htmlWriter, liveDefinition);
        }

        protected static string ConvertToJson(List<ClientOption> options)
        {
			eventLogger.LogDebug(">>> Entering ConvertToJson(options={Options})", options);
            var result = new StringBuilder();
            result.Append("{");
			eventLogger.LogDebug("ConvertToJson(options={Options}): result → {Result}", options, result);

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
			eventLogger.LogDebug("<<< Exiting ConvertToJson(options={Options})", options);
            return result.ToString();
        }
    }
}
