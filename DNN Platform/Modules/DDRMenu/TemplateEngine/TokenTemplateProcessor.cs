// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Xml;
    using System.Xml.Xsl;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Web.DDRMenu.DNNCommon;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An <see cref="ITemplateProcessor"/> for text files with token replacement.</summary>
    /// <param name="hostSettings">The host settings.</param>
    public class TokenTemplateProcessor(IHostSettings hostSettings)
        : ITemplateProcessor
    {
        private static readonly Dictionary<string, string> Aliases = new Dictionary<string, string>
        {
            { "page", "node" },
            { "name", "text" },
        };

        private static readonly Regex TemplatesRegex =
                new Regex(
                    @"(\[(?<directive>(\*|\*\>|\/\*|\>|\/\>|\?|\?!|\/\?|\=))(?<nodename>[A-Z]*)(-(?<modename>[0-9A-Z]*))?\])",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private XslCompiledTransform xsl;

        /// <summary>Initializes a new instance of the <see cref="TokenTemplateProcessor"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public TokenTemplateProcessor()
            : this(null)
        {
		eventLogger.LogDebug(">>> Entering TokenTemplateProcessor()");
        }
		eventLogger.LogDebug("<<< Exiting TokenTemplateProcessor()");

        /// <inheritdoc />
        public bool LoadDefinition(TemplateDefinition baseDefinition)
        {
			eventLogger.LogDebug(">>> Entering LoadDefinition(baseDefinition={BaseDefinition})", baseDefinition);
            if (!baseDefinition.TemplateVirtualPath.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            var templateText = Utilities.CachedFileContent(baseDefinition.TemplatePath);
			eventLogger.LogDebug("LoadDefinition(baseDefinition={BaseDefinition}): templateText → {TemplateText}", baseDefinition, templateText);
            var xml = new XmlDocument { XmlResolver = null };
            const string xmlNs = "http://www.w3.org/1999/XSL/Transform";
            const string xslXml = $"""
                                   	<xsl:stylesheet version='1.0' xmlns:xsl='{xmlNs}' xmlns:ddr='urn:ddrmenu'>
                                   		<xsl:output method='html'/>
                                   		<xsl:template match='/*'>
                                   			<xsl:apply-templates select='root' />
                                   		</xsl:template>
                                   		<xsl:template match='root'>
                                   		</xsl:template>
                                   	</xsl:stylesheet>
                                   """;
            using (var xmlReader = XmlReader.Create(new StringReader(xslXml), new XmlReaderSettings { XmlResolver = null, }))
            {
                xml.Load(xmlReader);
            }

            var validParams = baseDefinition.DefaultTemplateArguments.ConvertAll(a => a.Name.ToLowerInvariant());
			eventLogger.LogDebug("LoadDefinition(baseDefinition={BaseDefinition}): validParams → {ValidParams}", baseDefinition, validParams);
            validParams.AddRange(["controlid", "options", "dnnpath", "manifestpath", "portalpath", "skinpath",]);

            var docElt = xml.DocumentElement;
            var outputElt = (XmlElement)docElt.GetElementsByTagName("output", xmlNs)[0];
			eventLogger.LogDebug("LoadDefinition(baseDefinition={BaseDefinition}): outputElt → {OutputElt}", baseDefinition, outputElt);
            foreach (var param in validParams)
            {
                var elt = xml.CreateElement("param", xmlNs);
				eventLogger.LogDebug("LoadDefinition(baseDefinition={BaseDefinition}): elt → {Elt}", baseDefinition, elt);
                elt.SetAttribute("name", param);
                docElt.InsertAfter(elt, outputElt);
            }

            var current = (XmlElement)docElt.GetElementsByTagName("template", xmlNs)[1];
			eventLogger.LogDebug("LoadDefinition(baseDefinition={BaseDefinition}): current → {Current}", baseDefinition, current);
            var stack = new Stack<XmlElement>();
			eventLogger.LogDebug("LoadDefinition(baseDefinition={BaseDefinition}): stack → {Stack}", baseDefinition, stack);

            var index = 0;
            foreach (Match match in TemplatesRegex.Matches(templateText))
            {
                current.AppendChild(xml.CreateTextNode(templateText.Substring(index, match.Index - index)));

                var directive = match.Groups["directive"].Value;
				eventLogger.LogDebug("LoadDefinition(baseDefinition={BaseDefinition}): directive → {Directive}", baseDefinition, directive);
                var nodeName = match.Groups["nodename"].Value.ToLowerInvariant();
				eventLogger.LogDebug("LoadDefinition(baseDefinition={BaseDefinition}): nodeName → {NodeName}", baseDefinition, nodeName);
                var modeName = match.Groups["modename"].Value.ToLowerInvariant();
				eventLogger.LogDebug("LoadDefinition(baseDefinition={BaseDefinition}): modeName → {ModeName}", baseDefinition, modeName);

                string alias;
                if (Aliases.TryGetValue(nodeName, out alias))
                {
                    nodeName = alias;
                }

                if (directive == "=")
                {
                    var elt = xml.CreateElement("value-of", xmlNs);
                    if (validParams.Contains(nodeName))
                    {
                        elt.SetAttribute("select", "ddr:HtmlEncode($" + nodeName + ")");
                    }
                    else
                    {
                        elt.SetAttribute("select", "ddr:HtmlEncode(concat(" + nodeName + ", @" + nodeName + "))");
                    }

                    current.AppendChild(elt);
                }
                else if (directive == "*")
                {
                    var elt = xml.CreateElement("for-each", xmlNs);
                    elt.SetAttribute("select", nodeName);
                    current.AppendChild(elt);
                    stack.Push(current);
                    current = elt;
                }
                else if (directive == "*>")
                {
                    var elt = xml.CreateElement("apply-templates", xmlNs);
                    elt.SetAttribute("select", nodeName);
                    elt.SetAttribute("mode", "M" + modeName);
                    current.AppendChild(elt);
                }
                else if (directive == ">")
                {
                    var elt = xml.CreateElement("template", xmlNs);
                    elt.SetAttribute("match", nodeName);
                    elt.SetAttribute("mode", "M" + modeName);
                    xml.DocumentElement.AppendChild(elt);
                    stack.Push(current);
                    current = elt;
                }
                else if (directive[0] == '?')
                {
                    XmlElement elt;
                    if (nodeName != "else")
                    {
                        elt = xml.CreateElement("when", xmlNs);
                        var test = string.Format("{0} or (@{0}=1) or (@{0}!=0 and @{0}!=1 and @{0}!='')", nodeName);
						eventLogger.LogDebug("LoadDefinition(baseDefinition={BaseDefinition}): test → {Test}", baseDefinition, test);
                        if (directive == "?!")
                        {
                            test = string.Format("not({0})", test);
                        }

                        elt.SetAttribute("test", test);

                        var choose = xml.CreateElement("choose", xmlNs);
						eventLogger.LogDebug("LoadDefinition(baseDefinition={BaseDefinition}): choose → {Choose}", baseDefinition, choose);
                        current.AppendChild(choose);
                        choose.AppendChild(elt);
                        stack.Push(current);
                    }
                    else
                    {
                        elt = xml.CreateElement("otherwise", xmlNs);
                        current.ParentNode.AppendChild(elt);
                    }

                    current = elt;
                }
                else if (directive[0] == '/')
                {
                    current = stack.Pop();
                }

                index = match.Index + match.Length;
            }

            current.AppendChild(xml.CreateTextNode(templateText.Substring(index)));

            this.xsl = new XslCompiledTransform();
			eventLogger.LogDebug("LoadDefinition(baseDefinition={BaseDefinition}): xsl → {Xsl}", baseDefinition, xsl);
            this.xsl.Load(xml);
			eventLogger.LogDebug("<<< Exiting LoadDefinition(baseDefinition={BaseDefinition})", baseDefinition);
            return true;
        }

        /// <inheritdoc />
        public void Render(object source, HtmlTextWriter htmlWriter, TemplateDefinition liveDefinition)
        {
			eventLogger.LogDebug(">>> Entering Render(source={Source},htmlWriter={HtmlWriter},liveDefinition={LiveDefinition})", source, htmlWriter, liveDefinition);
            var resolver = new PathResolver(this.hostSettings, liveDefinition.Folder);
            var args = new XsltArgumentList();
			eventLogger.LogDebug("Render(source={Source},htmlWriter={HtmlWriter},liveDefinition={LiveDefinition}): args → {Args}", source, htmlWriter, liveDefinition, args);
			eventLogger.LogDebug("Render(source={Source},htmlWriter={HtmlWriter},liveDefinition={LiveDefinition}): resolver → {Resolver}", source, htmlWriter, liveDefinition, resolver);
            args.AddExtensionObject("urn:ddrmenu", new XsltFunctions());
            args.AddExtensionObject("urn:dnngarden", new XsltFunctions());
            args.AddParam("controlid", string.Empty, DNNContext.Current.HostControl.ClientID);
            args.AddParam("options", string.Empty, ConvertToJson(liveDefinition.ClientOptions));
            args.AddParam("dnnpath", string.Empty, resolver.Resolve("/", PathResolver.RelativeTo.Dnn));
            args.AddParam("manifestpath", string.Empty, resolver.Resolve("/", PathResolver.RelativeTo.Manifest));
            args.AddParam("portalpath", string.Empty, resolver.Resolve("/", PathResolver.RelativeTo.Portal));
            args.AddParam("skinpath", string.Empty, resolver.Resolve("/", PathResolver.RelativeTo.Skin));
            liveDefinition.TemplateArguments.ForEach(a => args.AddParam(a.Name.ToLowerInvariant(), string.Empty, a.Value));

            var sb = new StringBuilder();
			eventLogger.LogDebug("Render(source={Source},htmlWriter={HtmlWriter},liveDefinition={LiveDefinition}): sb → {Sb}", source, htmlWriter, liveDefinition, sb);

            using (var xmlStream = new MemoryStream())
            using (var outputWriter = new StringWriter(sb))
            {
                Utilities.SerialiserFor(source.GetType()).Serialize(xmlStream, source);
                xmlStream.Seek(0, SeekOrigin.Begin);
                using var xmlReader = XmlReader.Create(xmlStream);
                this.xsl.Transform(xmlReader, args, outputWriter);
            }

            htmlWriter.Write(HttpUtility.HtmlDecode(sb.ToString()));
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
