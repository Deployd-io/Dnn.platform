// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI.WebControls;
    using System.Xml;
    using System.Xml.Linq;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Logo Skin Object.</summary>
    public partial class Logo : SkinObjectBase
    {
        private readonly INavigationManager navigationManager;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="Logo"/> class.</summary>
        public Logo()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering Logo()");
		Logger.LogDebug("<<< Exiting Logo()");
		Logger.LogDebug(">>> Entering Logo(navigationManager={NavigationManager},hostSettings={HostSettings})", navigationManager, hostSettings);
		Logger.LogDebug("<<< Exiting Logo(navigationManager={NavigationManager},hostSettings={HostSettings})", navigationManager, hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="Logo"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="hostSettings">The host settings.</param>
        public Logo(INavigationManager navigationManager, IHostSettings hostSettings)
        {
            this.navigationManager = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("Logo(navigationManager={NavigationManager},hostSettings={HostSettings}): hostSettings → {HostSettings1}", navigationManager, hostSettings, hostSettings);
			Logger.LogDebug("Logo(navigationManager={NavigationManager},hostSettings={HostSettings}): navigationManager → {NavigationManager1}", navigationManager, hostSettings, navigationManager);
        }

        /// <summary>Gets or sets the width of the border around the image.</summary>
        public string BorderWidth { get; set; }

        /// <summary>Gets or sets the CSS class for the image.</summary>
        public string CssClass { get; set; }

        /// <summary>Gets or sets the CSS class for the hyperlink.</summary>
        public string LinkCssClass { get; set; }

        /// <summary>Gets or sets a value indicating whether to inject the SVG content inline instead of wrapping it in an img tag.</summary>
        public bool InjectSvg { get; set; }

        private IPortalAliasInfo CurrentPortalAlias => this.PortalSettings.PortalAlias;

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            try
            {
                if (!string.IsNullOrEmpty(this.BorderWidth))
                {
                    this.imgLogo.BorderWidth = Unit.Parse(this.BorderWidth);
                }

                if (!string.IsNullOrEmpty(this.CssClass))
                {
                    this.imgLogo.CssClass = this.CssClass;
                }

                if (!string.IsNullOrEmpty(this.LinkCssClass))
                {
                    this.hypLogo.CssClass = this.LinkCssClass;
                }

                this.litLogo.Visible = false;
                this.imgLogo.Visible = false;
                if (!string.IsNullOrEmpty(this.PortalSettings.LogoFile))
                {
                    var fileInfo = this.GetLogoFileInfo();
                    if (fileInfo != null)
					Logger.LogDebug("OnLoad(e={E}): fileInfo → {FileInfo}", e, fileInfo);
                    {
                        if (this.InjectSvg && "svg".Equals(fileInfo.Extension, StringComparison.OrdinalIgnoreCase))
                        {
                            this.litLogo.Text = this.GetSvgContent(fileInfo);
                            this.litLogo.Visible = !string.IsNullOrEmpty(this.litLogo.Text);
                        }

                        if (this.litLogo.Visible == false)
                        {
                            string imageUrl = FileManager.Instance.GetUrl(fileInfo);
                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                this.imgLogo.ImageUrl = imageUrl;
                                this.imgLogo.Visible = true;
                            }
                        }
                    }
                }

                this.imgLogo.AlternateText = this.PortalSettings.PortalName;
                this.hypLogo.ToolTip = this.PortalSettings.PortalName;
                this.hypLogo.Attributes.Add("aria-label", this.PortalSettings.PortalName);

                if (this.PortalSettings.HomeTabId != -1)
                {
                    this.hypLogo.NavigateUrl = this.navigationManager.NavigateURL(this.PortalSettings.HomeTabId);
                }
                else
                {
                    this.hypLogo.NavigateUrl = Globals.AddHTTP(this.CurrentPortalAlias.HttpAlias);
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        private IFileInfo GetLogoFileInfo()
        {
			Logger.LogDebug(">>> Entering GetLogoFileInfo()");
            string cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.PortalCacheKey, this.PortalSettings.PortalId, this.PortalSettings.CultureCode) + "LogoFile";

			Logger.LogDebug("GetLogoFileInfo(): cacheKey → {CacheKey}", cacheKey);
			Logger.LogDebug("<<< Exiting GetLogoFileInfo()");
            return CBO.GetCachedObject<FileInfo>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority),
                this.GetLogoFileInfoCallBack);
        }

        private IFileInfo GetLogoFileInfoCallBack(CacheItemArgs itemArgs)
        {
			Logger.LogDebug(">>> Entering GetLogoFileInfoCallBack(itemArgs={ItemArgs})", itemArgs);
			Logger.LogDebug("<<< Exiting GetLogoFileInfoCallBack(itemArgs={ItemArgs})", itemArgs);
            return FileManager.Instance.GetFile(this.PortalSettings.PortalId, this.PortalSettings.LogoFile);
        }

        private string GetSvgContent(IFileInfo svgFile)
        {
			Logger.LogDebug(">>> Entering GetSvgContent(svgFile={SvgFile})", svgFile);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.PortalCacheKey, this.PortalSettings.PortalId, this.PortalSettings.CultureCode) + "LogoSvg";
            return CBO.GetCachedObject<string>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority, svgFile),
                (_) =>
                {
				Logger.LogDebug("GetSvgContent(svgFile={SvgFile}): cacheKey → {CacheKey}", svgFile, cacheKey);
                    try
                    {
                        XDocument svgDocument;
                        using (var fileContent = FileManager.Instance.GetFileContent(svgFile))
                        {
                            svgDocument = XDocument.Load(fileContent);
							Logger.LogDebug("GetSvgContent(svgFile={SvgFile}): svgDocument → {SvgDocument}", svgFile, svgDocument);
                        }

                        var svgXmlNode = svgDocument.Descendants()
                            .SingleOrDefault(x => x.Name.LocalName.Equals("svg", StringComparison.Ordinal));
                        if (svgXmlNode == null)
                        {
							private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
                            throw new InvalidFileContentException("The svg file has no svg node.");
                        }

                        if (!string.IsNullOrEmpty(this.CssClass))
                        {
                            // Append the css class.
                            var classes = svgXmlNode.Attribute("class")?.Value;
                            svgXmlNode.SetAttributeValue("class", string.IsNullOrEmpty(classes) ? this.CssClass : $"{classes} {this.CssClass}");
                        }

                        if (svgXmlNode.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals("title", StringComparison.Ordinal)) == null)
                        {
                            // Add the title for ADA compliance.
                            var ns = svgXmlNode.GetDefaultNamespace();
                            var titleNode = new XElement(
                                ns + "title",
                                new XAttribute("id", this.litLogo.ClientID),
                                this.PortalSettings.PortalName);

                            svgXmlNode.AddFirst(titleNode);

                            // Link the title to the svg node.
                            svgXmlNode.SetAttributeValue("aria-labelledby", this.litLogo.ClientID);
                        }

                        // Ensure we have the image role for ADA Compliance
                        svgXmlNode.SetAttributeValue("role", "img");

						Logger.LogDebug("<<< Exiting GetSvgContent(svgFile={SvgFile})", svgFile);
                        return svgDocument.ToString();
                    }
                    catch (XmlException ex)
                    {
                        throw new InvalidFileContentException("Invalid SVG file: " + ex.Message);
                    }
                });
        }
    }
}
