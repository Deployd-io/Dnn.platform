// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Web.Client.Cdf;
    using DotNetNuke.Web.Client.ResourceManager;

    /// <summary>Allows for registration of CSS and JavaScript resources.</summary>
    public class DnnHtmlInclude : Literal
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>The pattern that matches a tag.</summary>
        public const string TagPattern = @"<{0}((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>";

        /// <summary>The pattern that matches an attribute.</summary>
        public const string AttributePattern = @"{0}(\s*=\s*(?:""(?<val>.*?)""|'(?<val>.*?)'|(?<val>[^'"">\s]+)))";

        private const string MatchAllAttributes = @"(\S+)=[""']?((?:.(?![""']?\s+(?:\S+)=|[>""']))+.)[""']?";

        private static readonly Regex LinkTagRegex = new Regex(string.Format(CultureInfo.InvariantCulture, TagPattern, "link"), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex ScriptTagRegex = new Regex(string.Format(CultureInfo.InvariantCulture, TagPattern, "script"), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private readonly IClientResourceController clientResourceController;

        /// <summary>Initializes a new instance of the <see cref="DnnHtmlInclude"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        public DnnHtmlInclude(IClientResourceController clientResourceController)
        {
			Logger.LogDebug(">>> Entering DnnHtmlInclude(clientResourceController={ClientResourceController})", clientResourceController);
            this.clientResourceController = clientResourceController;
			Logger.LogDebug("<<< Exiting DnnHtmlInclude(clientResourceController={ClientResourceController})", clientResourceController);
			Logger.LogDebug("DnnHtmlInclude(clientResourceController={ClientResourceController}): clientResourceController → {ClientResourceController1}", clientResourceController, clientResourceController);
			Logger.LogDebug("DnnHtmlInclude(clientResourceController={ClientResourceController}): this.clientResourceController → {Arg1}", clientResourceController, this.clientResourceController);
        }

        /// <summary>Gets or sets the provider to force for this resource include.</summary>
        public string ForceProvider { get; set; }

        /// <summary>Gets or sets the priority for this resource include.</summary>
        public int Priority { get; set; } = 100;

        /// <summary>Gets or sets the group for this resource include.</summary>
        public int Group { get; set; } = 100;

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            this.RegisterIncludes(this.Text);
            this.Text = string.Empty;
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
			Logger.LogDebug("OnLoad(e={E}): this.Text → {Arg1}", e, this.Text);
        }

        private void RegisterIncludes(string innerHtml)
        {
			Logger.LogDebug(">>> Entering RegisterIncludes(innerHtml={InnerHtml})", innerHtml);
            this.RegisterIncludes(this.GetIncludes(innerHtml, ClientDependencyType.Css), ClientDependencyType.Css);
            this.RegisterIncludes(this.GetIncludes(innerHtml, ClientDependencyType.Javascript), ClientDependencyType.Javascript);
			Logger.LogDebug("<<< Exiting RegisterIncludes(innerHtml={InnerHtml})", innerHtml);
        }

        private void RegisterIncludes(IEnumerable<BasicFile> files, ClientDependencyType dependencyType)
        {
            foreach (var file in files)
            {
                switch (dependencyType)
                {
                    case ClientDependencyType.Css:
                        var styleSheet = this.clientResourceController.CreateStylesheet(file.FilePath)
                                                        .SetProvider(file.ForceProvider)
                                                        .SetPriority(file.Priority);
                        foreach (var a in file.HtmlAttributes)
                        {
                            styleSheet.AddAttribute(a.Key, a.Value);
                        }

                        styleSheet.Register();
                        break;
                    case ClientDependencyType.Javascript:
                        var script = this.clientResourceController.CreateScript(file.FilePath)
                                                        .SetProvider(file.ForceProvider)
                                                        .SetPriority(file.Priority);
                        foreach (var a in file.HtmlAttributes)
                        {
                            script.AddAttribute(a.Key, a.Value);
                        }

                        script.Register();
                        break;
                }
            }
        }

        private List<BasicFile> GetIncludes(string innerHtml, ClientDependencyType dependencyType)
        {
			Logger.LogDebug(">>> Entering GetIncludes(innerHtml={InnerHtml},dependencyType={DependencyType})", innerHtml, dependencyType);
            Regex tagRegex;
            string sourceAttribute, mime;
            if (dependencyType == ClientDependencyType.Css)
            {
                tagRegex = LinkTagRegex;
				Logger.LogDebug("GetIncludes(innerHtml={InnerHtml},dependencyType={DependencyType}): tagRegex → {TagRegex}", innerHtml, dependencyType, tagRegex);
                sourceAttribute = "href";
                mime = "text/css";
				Logger.LogDebug("GetIncludes(innerHtml={InnerHtml},dependencyType={DependencyType}): sourceAttribute → {SourceAttribute}", innerHtml, dependencyType, sourceAttribute);
				Logger.LogDebug("GetIncludes(innerHtml={InnerHtml},dependencyType={DependencyType}): mime → {Mime}", innerHtml, dependencyType, mime);
            }
            else
            {
                tagRegex = ScriptTagRegex;
                sourceAttribute = "src";
                mime = null;
            }

            var files = new List<BasicFile>();
			Logger.LogDebug("GetIncludes(innerHtml={InnerHtml},dependencyType={DependencyType}): files → {Files}", innerHtml, dependencyType, files);
            foreach (Match match in tagRegex.Matches(innerHtml))
            {
                var allAttributes = Regex.Matches(match.Value, MatchAllAttributes, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
                                         .Cast<Match>()
                                         .ToArray();

                var type = allAttributes.FirstOrDefault(x =>
                {
                    if (x.Groups.Count < 3)
                    {
                        return false;
                    }

                    return x.Groups[1].Value == "type";
                });

                var href = allAttributes.FirstOrDefault(x =>
                {
                    if (x.Groups.Count < 3)
                    {
                        return false;
                    }

                    return x.Groups[1].Value == sourceAttribute;
                });

                if (href == null || (type == null && mime != null) || (type != null && mime != null && type.Groups[2].Value != mime))
                {
                    continue;
                }

                var attributes = allAttributes.Where(x =>
                {
                    if (x.Groups.Count < 3)
                    {
                        return false;
                    }

                    return x.Groups[1].Value != sourceAttribute && x.Groups[1].Value != "type";
                }).ToDictionary(x => x.Groups[1].Value, x => x.Groups[2].Value);

                var file = new BasicFile(dependencyType)
                {
                    FilePath = href.Groups[2].Value,
                    Group = this.Group,
                    Priority = this.Priority,
                    ForceProvider = this.ForceProvider,
                };

                foreach (var a in attributes)
                {
                    file.HtmlAttributes.Add(a.Key, a.Value);
                }

                files.Add(file);
            }

			Logger.LogDebug("<<< Exiting GetIncludes(innerHtml={InnerHtml},dependencyType={DependencyType})", innerHtml, dependencyType);
            return files;
        }
    }
}
