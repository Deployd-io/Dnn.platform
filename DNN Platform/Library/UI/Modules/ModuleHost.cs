// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.ModuleCache;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.WebControls;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>ModuleHost hosts a Module Control (or its cached Content).</summary>
    public sealed class ModuleHost : Panel
    {
        private const string DefaultCssProvider = "DnnPageHeaderProvider";
        private const string DefaultJsProvider = "DnnBodyProvider";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleHost));

        private static readonly Regex CdfMatchRegex = new Regex(
            @"<\!--CDF\((?<type>JAVASCRIPT|CSS|JS-LIBRARY)\|(?<path>.+?)(\|(?<provider>.+?)\|(?<priority>\d+?))?\)-->",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly char[] ArgsSeparator = [',',];

        private readonly ModuleInfo moduleConfiguration;
        private readonly IModuleControlPipeline moduleControlPipeline = Globals.GetCurrentServiceProvider().GetRequiredService<IModuleControlPipeline>();
        private Control control;
        private bool isCached;

        /// <summary>Initializes a new instance of the <see cref="ModuleHost"/> class using the ModuleConfiguration for the Module.</summary>
        /// <param name="moduleConfiguration">The module info.</param>
        /// <param name="skin">The skin for the page.</param>
        /// <param name="container">The container for the module.</param>
        public ModuleHost(ModuleInfo moduleConfiguration, Skins.Skin skin, Containers.Container container)
        {
			Logger.LogDebug(">>> Entering ModuleHost(moduleConfiguration={ModuleConfiguration},skin={Skin},container={Container})", moduleConfiguration, skin, container);
            this.ID = "ModuleContent";
            this.Container = container;
			Logger.LogDebug("ModuleHost(moduleConfiguration={ModuleConfiguration},skin={Skin},container={Container}): this.ID → {Arg3}", moduleConfiguration, skin, container, this.ID);
			Logger.LogDebug("ModuleHost(moduleConfiguration={ModuleConfiguration},skin={Skin},container={Container}): this.Container → {Arg3}", moduleConfiguration, skin, container, this.Container);
            this.moduleConfiguration = moduleConfiguration;
			Logger.LogDebug("ModuleHost(moduleConfiguration={ModuleConfiguration},skin={Skin},container={Container}): moduleConfiguration → {ModuleConfiguration1}", moduleConfiguration, skin, container, moduleConfiguration);
            this.Skin = skin;
			Logger.LogDebug("<<< Exiting ModuleHost(moduleConfiguration={ModuleConfiguration},skin={Skin},container={Container})", moduleConfiguration, skin, container);
        }

        /// <summary>Gets the attached ModuleControl.</summary>
        /// <returns>An IModuleControl.</returns>
        public IModuleControl ModuleControl
        {
            get
            {
                // Make sure the Control tree has been created
                this.EnsureChildControls();
                return this.control as IModuleControl;
            }
        }

        /// <summary>Gets the current Portal Settings.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public PortalSettings PortalSettings => PortalSettings.Current;

        public Containers.Container Container { get; private set; }

        public Skins.Skin Skin { get; private set; }

        /// <summary>Gets a flag that indicates whether the Module is in View Mode.</summary>
        /// <param name="moduleInfo">The module information.</param>
        /// <param name="settings">The portal settings.</param>
        /// <returns>A Boolean.</returns>
        internal static bool IsViewMode(ModuleInfo moduleInfo, PortalSettings settings)
        {
			Logger.LogDebug(">>> Entering IsViewMode(moduleInfo={ModuleInfo},settings={Settings})", moduleInfo, settings);
            bool viewMode;

            if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.ViewPermissions, Null.NullString, moduleInfo))
            {
                viewMode = false;
            }
            else
            {
                viewMode = !ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, Null.NullString, moduleInfo);
            }

			Logger.LogDebug("<<< Exiting IsViewMode(moduleInfo={ModuleInfo},settings={Settings})", moduleInfo, settings);
            return viewMode || Personalization.GetUserMode() == PortalSettings.Mode.View;
        }

        /// <summary>CreateChildControls builds the control tree.</summary>
        protected override void CreateChildControls()
        {
			Logger.LogDebug(">>> Entering CreateChildControls()");
            this.Controls.Clear();

            // Load Module Control (or cached control)
            this.LoadModuleControl();

            // Optionally Inject AJAX Update Panel
            if (this.ModuleControl != null)
            {
                // if module is dynamically loaded and AJAX is installed and the control supports partial rendering (defined in ModuleControls table )
                if (!this.isCached && this.moduleConfiguration.ModuleControl.SupportsPartialRendering && AJAX.IsInstalled())
                {
                    this.LoadUpdatePanel();
                }
                else
                {
                    // inject a message placeholder for common module messaging - UI.Skins.Skin.AddModuleMessage
                    InjectMessageControl(this);

                    this.InjectVersionToTheModuleIfSupported();

                    // inject the module into the panel
                    this.InjectModuleContent(this.control);
                }
            }
			Logger.LogDebug("<<< Exiting CreateChildControls()");
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            if (Host.EnableCustomModuleCssClass)
            {
                string moduleName = this.ModuleControl.ModuleContext.Configuration.DesktopModule.ModuleName;
				Logger.LogDebug("OnPreRender(e={E}): moduleName → {ModuleName}", e, moduleName);
                if (moduleName != null)
                {
                    moduleName = Globals.CleanName(moduleName);
                }

                this.Attributes.Add("class", $"DNNModuleContent Mod{moduleName}C");
            }
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <inheritdoc />
        protected override void RenderContents(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderContents(writer={Writer})", writer);
            if (this.isCached)
            {
                // Render the cached control to the output stream
                base.RenderContents(writer);
            }
            else
            {
                if (this.SupportsCaching() && IsViewMode(this.moduleConfiguration, this.PortalSettings) && !Globals.IsAdminControl() && !IsVersionRequest())
                {
                    // Render to cache
                    var tempWriter = new StringWriter();
					Logger.LogDebug("RenderContents(writer={Writer}): tempWriter → {TempWriter}", writer, tempWriter);

                    this.control.RenderControl(new HtmlTextWriter(tempWriter));
                    string cachedOutput = tempWriter.ToString();

                    if (!string.IsNullOrEmpty(cachedOutput) && (!HttpContext.Current.Request.Browser.Crawler))
                    {
                        // Save content to cache
                        var moduleContent = Encoding.UTF8.GetBytes(cachedOutput);
						Logger.LogDebug("RenderContents(writer={Writer}): moduleContent → {ModuleContent}", writer, moduleContent);
                        var cache = ModuleCachingProvider.Instance(this.moduleConfiguration.GetEffectiveCacheMethod());
						Logger.LogDebug("RenderContents(writer={Writer}): cache → {Cache}", writer, cache);

                        var varyBy = new SortedDictionary<string, string> { { "locale", Thread.CurrentThread.CurrentUICulture.ToString() } };
						Logger.LogDebug("RenderContents(writer={Writer}): varyBy → {VaryBy}", writer, varyBy);

                        var cacheKey = cache.GenerateCacheKey(this.moduleConfiguration.TabModuleID, varyBy);
						Logger.LogDebug("RenderContents(writer={Writer}): cacheKey → {CacheKey}", writer, cacheKey);
                        cache.SetModule(this.moduleConfiguration.TabModuleID, cacheKey, new TimeSpan(0, 0, this.moduleConfiguration.CacheTime), moduleContent);
                    }

                    // Render the cached content to Response
                    writer.Write(cachedOutput);
                }
                else
                {
                    // Render the control to Response
                    base.RenderContents(writer);
                }
            }
			Logger.LogDebug("<<< Exiting RenderContents(writer={Writer})", writer);
        }

        private static void InjectMessageControl(Control container)
        {
			Logger.LogDebug(">>> Entering InjectMessageControl(container={Container})", container);
            // inject a message placeholder for common module messaging - UI.Skins.Skin.AddModuleMessage
            var messagePlaceholder = new PlaceHolder { ID = "MessagePlaceHolder", Visible = false };
			Logger.LogDebug("InjectMessageControl(container={Container}): messagePlaceholder → {MessagePlaceholder}", container, messagePlaceholder);
            container.Controls.Add(messagePlaceholder);
			Logger.LogDebug("<<< Exiting InjectMessageControl(container={Container})", container);
        }

        private static bool IsVersionRequest() => TabVersionUtils.TryGetUrlVersion(out _);

        private void InjectVersionToTheModuleIfSupported()
        {
			Logger.LogDebug(">>> Entering InjectVersionToTheModuleIfSupported()");
            if (this.control is not IVersionableControl versionableControl)
            {
                return;
            }

            if (this.moduleConfiguration.ModuleVersion != Null.NullInteger)
            {
                versionableControl.SetModuleVersion(this.moduleConfiguration.ModuleVersion);
            }
			Logger.LogDebug("<<< Exiting InjectVersionToTheModuleIfSupported()");
        }

        private void InjectModuleContent(Control content) => this.Controls.Add(content);

        /// <summary>Gets a flag that indicates whether the Module Content should be displayed.</summary>
        /// <returns>A Boolean.</returns>
        private bool DisplayContent()
        {
			Logger.LogDebug(">>> Entering DisplayContent()");
            // module content visibility options
            var content = Personalization.GetUserMode() != PortalSettings.Mode.Layout;
			Logger.LogDebug("DisplayContent(): content → {Content}", content);
            if (this.Page.Request.QueryString["content"] != null)
            {
                switch (this.Page.Request.QueryString["Content"].ToLowerInvariant())
                {
                    case "1":
                    case "true":
                        content = true;
                        break;
                    case "0":
                    case "false":
                        content = false;
                        break;
                }
            }

            if (Globals.IsAdminControl())
            {
                content = true;
            }

			Logger.LogDebug("<<< Exiting DisplayContent()");
            return content;
        }

        /// <summary>LoadModuleControl loads the ModuleControl (PortalModuelBase).</summary>
        private void LoadModuleControl()
        {
			Logger.LogDebug(">>> Entering LoadModuleControl()");
            try
            {
                if (this.DisplayContent())
                {
                    // if the module supports caching and caching is enabled for the instance and the user does not have Edit rights or is currently in View mode
                    if (this.SupportsCaching() && IsViewMode(this.moduleConfiguration, this.PortalSettings) && !IsVersionRequest())
                    {
                        // attempt to load the cached content
                        this.isCached = this.TryLoadCached();
                    }

                    if (!this.isCached)
                    {
                        // load the control dynamically
                        this.control = this.moduleControlPipeline.LoadModuleControl(this.Page, this.moduleConfiguration);
						Logger.LogDebug("LoadModuleControl(): control → {Control}", control);
                    }
                }
                else
                {
                    // content placeholder
                    this.control = this.moduleControlPipeline.CreateModuleControl(this.moduleConfiguration);
                }

                if (this.Skin != null)
                {
                    // check for IMC
                    this.Skin.Communicator.LoadCommunicator(this.control);
                }

                // add module settings
                this.ModuleControl.ModuleContext.Configuration = this.moduleConfiguration;
            }
            catch (ThreadAbortException exc)
            {
                Logger.Debug(exc);

                Thread.ResetAbort();
            }
            catch (Exception exc)
            {
                Logger.Error(exc);

                // add module settings
                this.control = this.moduleControlPipeline.CreateModuleControl(this.moduleConfiguration);
                this.ModuleControl.ModuleContext.Configuration = this.moduleConfiguration;
                if (TabPermissionController.CanAdminPage())
                {
                    // only display the error to page administrators
                    Exceptions.ProcessModuleLoadException(this.control, exc);
                }
                else
                {
                    // Otherwise just log the fact that an exception occurred
                    new ExceptionLogController().AddLog(exc);
                }
            }

            // Enable ViewState
            this.control.ViewStateMode = ViewStateMode.Enabled;
			Logger.LogDebug("<<< Exiting LoadModuleControl()");
        }

        /// <summary>LoadUpdatePanel optionally loads an AJAX Update Panel.</summary>
        private void LoadUpdatePanel()
        {
			Logger.LogDebug(">>> Entering LoadUpdatePanel()");
            // register AJAX
            AJAX.RegisterScriptManager();

            // enable Partial Rendering
            var scriptManager = AJAX.GetScriptManager(this.Page);
			Logger.LogDebug("LoadUpdatePanel(): scriptManager → {ScriptManager}", scriptManager);
            if (scriptManager != null)
            {
                scriptManager.EnablePartialRendering = true;
				Logger.LogDebug("LoadUpdatePanel(): scriptManager.EnablePartialRendering → {Arg0}", scriptManager.EnablePartialRendering);
            }

            // create update panel
            var updatePanel = new UpdatePanel
            {
                UpdateMode = UpdatePanelUpdateMode.Conditional,
                ID = this.control.ID + "_UP",
            };

            // get update panel content template
            var templateContainer = updatePanel.ContentTemplateContainer;

            // inject a message placeholder for common module messaging - UI.Skins.Skin.AddModuleMessage
            InjectMessageControl(templateContainer);

            // inject module into update panel content template
            templateContainer.Controls.Add(this.control);

            // inject the update panel into the panel
            this.InjectModuleContent(updatePanel);

            // create image for update progress control
            var progressTemplate = "<div class=\"dnnLoading dnnPanelLoading\"></div>";

            // inject updateprogress into the panel
            var updateProgress = new UpdateProgress
            {
                AssociatedUpdatePanelID = updatePanel.ID,
                ID = updatePanel.ID + "_Prog",

                ProgressTemplate = new LiteralTemplate(progressTemplate),
            };
            this.Controls.Add(updateProgress);
			Logger.LogDebug("<<< Exiting LoadUpdatePanel()");
        }

        /// <summary>Gets a flag that indicates whether the Module Instance supports Caching.</summary>
        /// <returns>A Boolean.</returns>
        private bool SupportsCaching()
        {
			Logger.LogDebug(">>> Entering SupportsCaching()");
			Logger.LogDebug("<<< Exiting SupportsCaching()");
            return this.moduleConfiguration.CacheTime > 0;
        }

        /// <summary>Trys to load previously cached Module Content.</summary>
        /// <returns>A Boolean that indicates whether the cahed content was loaded.</returns>
        private bool TryLoadCached()
        {
			Logger.LogDebug(">>> Entering TryLoadCached()");
            bool success = false;
            string cachedContent = string.Empty;
			Logger.LogDebug("TryLoadCached(): cachedContent → {CachedContent}", cachedContent);
			Logger.LogDebug("TryLoadCached(): success → {Success}", success);
            try
            {
                var cache = ModuleCachingProvider.Instance(this.moduleConfiguration.GetEffectiveCacheMethod());
				Logger.LogDebug("TryLoadCached(): cache → {Cache}", cache);
                var varyBy = new SortedDictionary<string, string> { { "locale", Thread.CurrentThread.CurrentUICulture.ToString() } };
				Logger.LogDebug("TryLoadCached(): varyBy → {VaryBy}", varyBy);

                string cacheKey = cache.GenerateCacheKey(this.moduleConfiguration.TabModuleID, varyBy);
				Logger.LogDebug("TryLoadCached(): cacheKey → {CacheKey}", cacheKey);
                byte[] cachedBytes = ModuleCachingProvider.Instance(this.moduleConfiguration.GetEffectiveCacheMethod()).GetModule(this.moduleConfiguration.TabModuleID, cacheKey);

                if (cachedBytes != null && cachedBytes.Length > 0)
                {
                    cachedContent = Encoding.UTF8.GetString(cachedBytes);
                    success = true;
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in TryLoadCached(): {Arg0}", ex.getMessage());
                cachedContent = string.Empty;
                Exceptions.LogException(ex);
                success = false;
            }

            if (success)
            {
                this.RestoreCachedClientResourceRegistrations(cachedContent);
                this.control = this.moduleControlPipeline.CreateCachedControl(cachedContent, this.moduleConfiguration);
				Logger.LogDebug("TryLoadCached(): control → {Control}", control);
                this.Controls.Add(this.control);
            }

			Logger.LogDebug("<<< Exiting TryLoadCached()");
            return success;
        }

        /// <summary>
        /// Restores client resource registrations from the <paramref name="cachedContent"/>.
        /// These are registrations that originated from <c>DnnJsInclude</c>, <c>DnnCssInclude</c>, and <c>JavaScriptLibraryInclude</c> controls.
        /// </summary>
        /// <param name="cachedContent">The HTML text of the cached module.</param>
        private void RestoreCachedClientResourceRegistrations(string cachedContent)
        {
			Logger.LogDebug(">>> Entering RestoreCachedClientResourceRegistrations(cachedContent={CachedContent})", cachedContent);
            // parse the registered CDF from content
            var matches = CdfMatchRegex.Matches(cachedContent);
			Logger.LogDebug("RestoreCachedClientResourceRegistrations(cachedContent={CachedContent}): matches → {Matches}", cachedContent, matches);
            if (matches.Count == 0)
            {
                return;
            }

            foreach (Match match in matches)
            {
                cachedContent = cachedContent.Replace(match.Value, string.Empty);
				Logger.LogDebug("RestoreCachedClientResourceRegistrations(cachedContent={CachedContent}): cachedContent → {CachedContent1}", cachedContent, cachedContent);
                var dependencyType = match.Groups["type"].Value.ToUpperInvariant();
				Logger.LogDebug("RestoreCachedClientResourceRegistrations(cachedContent={CachedContent}): dependencyType → {DependencyType}", cachedContent, dependencyType);
                var filePath = match.Groups["path"].Value;
				Logger.LogDebug("RestoreCachedClientResourceRegistrations(cachedContent={CachedContent}): filePath → {FilePath}", cachedContent, filePath);
                var forceProvider = string.Empty;
				Logger.LogDebug("RestoreCachedClientResourceRegistrations(cachedContent={CachedContent}): forceProvider → {ForceProvider}", cachedContent, forceProvider);
                var priority = Null.NullInteger;
				Logger.LogDebug("RestoreCachedClientResourceRegistrations(cachedContent={CachedContent}): priority → {Priority}", cachedContent, priority);

                if (match.Groups["provider"].Success)
                {
                    forceProvider = match.Groups["provider"].Value;
                }

                if (match.Groups["priority"].Success)
                {
                    priority = Convert.ToInt32(match.Groups["priority"].Value, CultureInfo.InvariantCulture);
                }

                switch (dependencyType)
                {
                    case "JAVASCRIPT":
                        if (string.IsNullOrEmpty(forceProvider))
                        {
                            forceProvider = DefaultJsProvider;
                        }

                        if (priority == Null.NullInteger)
                        {
                            priority = (int)FileOrder.Js.DefaultPriority;
                        }

                        ClientResourceManager.RegisterScript(this.Page, filePath, priority, forceProvider);
                        break;
                    case "CSS":
                        if (string.IsNullOrEmpty(forceProvider))
                        {
                            forceProvider = DefaultCssProvider;
                        }

                        if (priority == Null.NullInteger)
                        {
                            priority = (int)FileOrder.Css.DefaultPriority;
                        }

                        ClientResourceManager.RegisterStyleSheet(this.Page, filePath, priority, forceProvider);
                        break;
                    case "JS-LIBRARY":
                        var args = filePath.Split(ArgsSeparator, StringSplitOptions.None);
						Logger.LogDebug("RestoreCachedClientResourceRegistrations(cachedContent={CachedContent}): args → {Args}", cachedContent, args);
                        if (string.IsNullOrEmpty(args[1]))
                        {
                            JavaScript.RequestRegistration(args[0]);
                        }
                        else if (string.IsNullOrEmpty(args[2]))
                        {
                            JavaScript.RequestRegistration(args[0], new Version(args[1]));
                        }
                        else
                        {
                            JavaScript.RequestRegistration(args[0], new Version(args[1]), (SpecificVersion)Enum.Parse(typeof(SpecificVersion), args[2]));
                        }

                        break;
                }
            }
			Logger.LogDebug("<<< Exiting RestoreCachedClientResourceRegistrations(cachedContent={CachedContent})", cachedContent);
        }
    }
}
