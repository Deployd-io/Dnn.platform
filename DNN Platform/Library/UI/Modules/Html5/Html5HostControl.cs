// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Modules.Html5
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A WebForms control which outputs the content for a control using the HTML module pattern.</summary>
    /// <param name="html5File">The path to the HTML file.</param>
    /// <param name="businessControllerProvider">The business controller provider.</param>
    /// <param name="servicesFramework">The web API service framework.</param>
    /// <param name="hostSettings">The host settings.</param>
    public class Html5HostControl(string html5File, IBusinessControllerProvider businessControllerProvider, IServicesFramework servicesFramework, IHostSettings hostSettings)
        : ModuleControlBase, IActionable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Html5HostControl));
        private readonly string html5File = html5File;
        private readonly IBusinessControllerProvider businessControllerProvider = businessControllerProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<IBusinessControllerProvider>();
        private readonly IServicesFramework servicesFramework = servicesFramework ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServicesFramework>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private string fileContent;

        /// <summary>Initializes a new instance of the <see cref="Html5HostControl"/> class.</summary>
        /// <param name="html5File">The path to the HTML file.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public Html5HostControl(string html5File)
            : this(html5File, null, null)
        {
			Logger.LogDebug(">>> Entering Html5HostControl(html5File={Html5File})", html5File);
            this.html5File = html5File;
			Logger.LogDebug("<<< Exiting Html5HostControl(html5File={Html5File})", html5File);
			Logger.LogDebug("Html5HostControl(html5File={Html5File}): this.html5File → {Arg1}", html5File, this.html5File);
        }

        /// <summary>Initializes a new instance of the <see cref="Html5HostControl"/> class.</summary>
        /// <param name="html5File">The path to the HTML file.</param>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
        public Html5HostControl(string html5File, IBusinessControllerProvider businessControllerProvider)
            : this(html5File, businessControllerProvider, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Html5HostControl"/> class.</summary>
        /// <param name="html5File">The path to the HTML file.</param>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public Html5HostControl(string html5File, IBusinessControllerProvider businessControllerProvider, IServicesFramework servicesFramework)
            : this(html5File, businessControllerProvider, servicesFramework, null)
        {
        }

        /// <inheritdoc />
        public ModuleActionCollection ModuleActions { get; private set; }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            if (!string.IsNullOrEmpty(this.html5File))
            {
                // Check if css file exists
                var cssFile = Path.ChangeExtension(this.html5File, ".css");
				Logger.LogDebug("OnInit(e={E}): cssFile → {CssFile}", e, cssFile);
                if (this.FileExists(cssFile))
                {
                    ClientResourceManager.RegisterStyleSheet(this.Page, cssFile, FileOrder.Css.DefaultPriority);
                }

                // Check if js file exists
                var jsFile = Path.ChangeExtension(this.html5File, ".js");
				Logger.LogDebug("OnInit(e={E}): jsFile → {JsFile}", e, jsFile);
                if (this.FileExists(jsFile))
                {
                    ClientResourceManager.RegisterScript(this.Page, jsFile, FileOrder.Js.DefaultPriority);
                }

                this.fileContent = this.GetFileContent(this.html5File);

                this.ModuleActions = new ModuleActionCollection();
                var tokenReplace = new Html5ModuleTokenReplace(this.Page, this.businessControllerProvider, this.html5File, this.ModuleContext, this.ModuleActions);
				Logger.LogDebug("OnInit(e={E}): tokenReplace → {TokenReplace}", e, tokenReplace);
                this.fileContent = tokenReplace.ReplaceEnvironmentTokens(this.fileContent);
            }

            // Register for Services Framework
            this.servicesFramework.RequestAjaxScriptSupport();
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            if (!string.IsNullOrEmpty(this.html5File))
            {
                this.Controls.Add(new LiteralControl(HttpUtility.HtmlDecode(this.fileContent)));
            }
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        private static string GetFileContentInternal(string filepath)
        {
			Logger.LogDebug(">>> Entering GetFileContentInternal(filepath={Filepath})", filepath);
            using var reader = new StreamReader(filepath);
			Logger.LogDebug("GetFileContentInternal(filepath={Filepath}): reader → {Reader}", filepath, reader);
			Logger.LogDebug("<<< Exiting GetFileContentInternal(filepath={Filepath})", filepath);
            return reader.ReadToEnd();
        }

        private string GetFileContent(string filepath)
        {
			Logger.LogDebug(">>> Entering GetFileContent(filepath={Filepath})", filepath);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.SpaModulesContentHtmlFileCacheKey, filepath);
            var absoluteFilePath = this.Page.Server.MapPath(filepath);
			Logger.LogDebug("GetFileContent(filepath={Filepath}): absoluteFilePath → {AbsoluteFilePath}", filepath, absoluteFilePath);
			Logger.LogDebug("GetFileContent(filepath={Filepath}): cacheKey → {CacheKey}", filepath, cacheKey);
            var cacheItemArgs = new CacheItemArgs(cacheKey, DataCache.SpaModulesHtmlFileTimeOut, DataCache.SpaModulesHtmlFileCachePriority)
            {
                CacheDependency = new DNNCacheDependency(absoluteFilePath),
            };
			Logger.LogDebug("<<< Exiting GetFileContent(filepath={Filepath})", filepath);
            return CBO.GetCachedObject<string>(
                this.hostSettings,
                cacheItemArgs,
                _ => GetFileContentInternal(absoluteFilePath));
        }

        private bool FileExists(string filepath)
        {
			Logger.LogDebug(">>> Entering FileExists(filepath={Filepath})", filepath);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.SpaModulesFileExistsCacheKey, filepath);
			Logger.LogDebug("FileExists(filepath={Filepath}): cacheKey → {CacheKey}", filepath, cacheKey);
			Logger.LogDebug("<<< Exiting FileExists(filepath={Filepath})", filepath);
            return CBO.GetCachedObject<bool>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.SpaModulesHtmlFileTimeOut, DataCache.SpaModulesHtmlFileCachePriority),
                _ => File.Exists(this.Page.Server.MapPath(filepath)));
        }
    }
}
