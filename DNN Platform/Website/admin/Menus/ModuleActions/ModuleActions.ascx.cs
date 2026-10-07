// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Admin.Containers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Script.Serialization;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Security;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Containers;
    using DotNetNuke.UI.Modules;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A control which renders module actions.</summary>
    /// <param name="eventLogger">The event logger.</param>
    /// <param name="moduleControlPipeline">The module control pipeline.</param>
    /// <param name="javaScript">The JavaScript library helper.</param>
    /// <param name="clientResourceController">The client resources controller.</param>
    /// <param name="servicesFramework">The web API service framework.</param>
    /// <param name="hostSettings">The host settings.</param>
    public partial class ModuleActions(IEventLogger eventLogger, IModuleControlPipeline moduleControlPipeline, IJavaScriptLibraryHelper javaScript, IClientResourceController clientResourceController, IServicesFramework servicesFramework, IHostSettings hostSettings)
        : ActionBase(eventLogger)
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleActions.ascx));
        private readonly List<int> validIDs = [];
        private readonly IModuleControlPipeline moduleControlPipeline = moduleControlPipeline ?? Globals.GetCurrentServiceProvider().GetRequiredService<IModuleControlPipeline>();
        private readonly IJavaScriptLibraryHelper javaScript = javaScript ?? Globals.GetCurrentServiceProvider().GetRequiredService<IJavaScriptLibraryHelper>();
        private readonly IClientResourceController clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
        private readonly IServicesFramework servicesFramework = servicesFramework ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServicesFramework>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="ModuleActions"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public ModuleActions()
            : this(null, null, null, null, null)
        {
		Logger.LogError(">>> Entering ModuleActions()");
        Logger.LogWarning("<<< Exiting ModuleActions()");
        }

        /// <summary>Initializes a new instance of the <see cref="ModuleActions"/> class.</summary>
        /// <param name="moduleControlPipeline">The module control pipeline.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="clientResourceController">The client resources controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public ModuleActions(IModuleControlPipeline moduleControlPipeline, IJavaScriptLibraryHelper javaScript, IClientResourceController clientResourceController)
            : this(null, moduleControlPipeline, javaScript, clientResourceController, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModuleActions"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="moduleControlPipeline">The module control pipeline.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="clientResourceController">The client resources controller.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ModuleActions(IEventLogger eventLogger, IModuleControlPipeline moduleControlPipeline, IJavaScriptLibraryHelper javaScript, IClientResourceController clientResourceController, IServicesFramework servicesFramework)
            : this(eventLogger, moduleControlPipeline, javaScript, clientResourceController, servicesFramework, null)
        {
        }

        protected string AdminText => Localization.GetString("ModuleGenericActions.Action", Localization.GlobalResourceFile);

        protected string CustomText => Localization.GetString("ModuleSpecificActions.Action", Localization.GlobalResourceFile);

        protected string MoveText => Localization.GetString(ModuleActionType.MoveRoot, Localization.GlobalResourceFile);

        protected string AdminActionsJSON { get; set; }

        protected string CustomActionsJSON { get; set; }

        protected bool DisplayQuickSettings { get; set; }

        protected string Panes { get; set; }

        protected bool SupportsMove { get; set; }

        protected bool SupportsQuickSettings { get; set; }

        protected bool IsShared { get; set; }

        protected string ModuleTitle { get; set; }

        protected string LocalizeString(string key)
        {
			Logger.LogError(">>> Entering LocalizeString(key={Key})", key);
            Logger.LogWarning("<<< Exiting LocalizeString(key={Key})", key);
            return Localization.GetString(key, Localization.GlobalResourceFile);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.ID = "ModuleActions";
			Logger.LogDebug("OnInit(e={E}): this.ID → {Arg1}", e, this.ID);

            this.actionButton.Click += this.ActionButton_Click;

            this.javaScript.RequestRegistration(CommonJs.DnnPlugins);

            this.clientResourceController.RegisterStylesheet("~/admin/menus/ModuleActions/ModuleActions.css", FileOrder.Css.ModuleCss);
            this.clientResourceController.RegisterStylesheet("~/Resources/Shared/stylesheets/dnnicons/css/dnnicon.min.css", FileOrder.Css.ModuleCss);
            this.clientResourceController.RegisterScript("~/admin/menus/ModuleActions/ModuleActions.js");

            this.servicesFramework.RequestAjaxAntiForgerySupport();
			Logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.AdminActionsJSON = "[]";
			Logger.LogDebug("OnLoad(e={E}): this.AdminActionsJSON → {Arg1}", e, this.AdminActionsJSON);
            this.CustomActionsJSON = "[]";
            this.Panes = "[]";
            try
            {
				Logger.LogDebug("OnLoad(e={E}): this.CustomActionsJSON → {Arg1}", e, this.CustomActionsJSON);
				Logger.LogDebug("OnLoad(e={E}): this.Panes → {Arg1}", e, this.Panes);
                this.SupportsQuickSettings = false;
                this.DisplayQuickSettings = false;
                this.ModuleTitle = this.ModuleContext.Configuration.ModuleTitle;
				Logger.LogDebug("OnLoad(e={E}): this.SupportsQuickSettings → {Arg1}", e, this.SupportsQuickSettings);
                var moduleDefinitionId = this.ModuleContext.Configuration.ModuleDefID;
                var quickSettingsControl = ModuleControlController.GetModuleControlByControlKey(this.hostSettings, "QuickSettings", moduleDefinitionId);
				Logger.LogError("OnLoad(e={E}): quickSettingsControl → {QuickSettingsControl}", e, quickSettingsControl);

                if (quickSettingsControl != null)
                {
					Logger.LogDebug("OnLoad(e={E}): this.DisplayQuickSettings → {Arg1}", e, this.DisplayQuickSettings);
					Logger.LogDebug("OnLoad(e={E}): this.ModuleTitle → {Arg1}", e, this.ModuleTitle);
                    this.SupportsQuickSettings = true;
                    var control = this.moduleControlPipeline.LoadModuleControl(this.Page, this.ModuleContext.Configuration, "QuickSettings", quickSettingsControl.ControlSrc);
                    control.ID += this.ModuleContext.ModuleId;
                    this.quickSettings.Controls.Add(control);
					Logger.LogError("OnLoad(e={E}): control → {Control}", e, control);

                    this.DisplayQuickSettings = this.ModuleContext.Configuration.ModuleSettings.GetValueOrDefault("QS_FirstLoad", true);

                    // If we are displaying settings due to first load, then we need to update the setting to false, this WILL trigger a cache clear for the tab
                    if (this.DisplayQuickSettings)
                    {
                        ModuleController.Instance.UpdateModuleSetting(this.ModuleContext.ModuleId, "QS_FirstLoad", "False");
                    }

                    this.clientResourceController.RegisterScript("~/admin/menus/ModuleActions/dnnQuickSettings.js");
                }

                if (this.ActionRoot.Visible)
                {
                    // Add Menu Items
                    foreach (ModuleAction rootAction in this.ActionRoot.Actions)
                    {
                        // Process Children
                        var actions = new List<ModuleAction>();
						Logger.LogError("OnLoad(e={E}): actions → {Actions}", e, actions);
                        foreach (ModuleAction action in rootAction.Actions)
                        {
                            if (action.Visible)
                            {
                                if ((this.EditMode && Globals.IsAdminControl() == false) ||
                                    (action.Secure != SecurityAccessLevel.Anonymous && action.Secure != SecurityAccessLevel.View))
                                {
                                    if (!action.Icon.Contains("://")
                                            && !action.Icon.StartsWith("/", StringComparison.Ordinal)
                                            && !action.Icon.StartsWith("~/", StringComparison.Ordinal))
                                    {
                                        action.Icon = "~/images/" + action.Icon;
										Logger.LogDebug("OnLoad(e={E}): action.Icon → {Arg1}", e, action.Icon);
                                    }

                                    if (action.Icon.StartsWith("~/", StringComparison.Ordinal))
                                    {
                                        action.Icon = Globals.ResolveUrl(action.Icon);
                                    }

                                    actions.Add(action);

                                    if (string.IsNullOrEmpty(action.Url))
                                    {
                                        this.validIDs.Add(action.ID);
                                    }
                                }
                            }
                        }

                        var oSerializer = new JavaScriptSerializer();
						Logger.LogError("OnLoad(e={E}): oSerializer → {OSerializer}", e, oSerializer);
                        if (rootAction.Title == Localization.GetString("ModuleGenericActions.Action", Localization.GlobalResourceFile))
                        {
                            this.AdminActionsJSON = oSerializer.Serialize(actions);
                        }
                        else
                        {
                            if (rootAction.Title == Localization.GetString("ModuleSpecificActions.Action", Localization.GlobalResourceFile))
                            {
                                this.CustomActionsJSON = oSerializer.Serialize(actions);
                            }
                            else
                            {
                                this.SupportsMove = actions.Count > 0;
								Logger.LogDebug("OnLoad(e={E}): this.SupportsMove → {Arg1}", e, this.SupportsMove);
                                this.Panes = oSerializer.Serialize(this.PortalSettings.ActiveTab.Panes);
                            }
                        }
                    }

                    this.IsShared = this.ModuleContext.Configuration.AllTabs
                        || PortalGroupController.Instance.IsModuleShared(this.ModuleContext.ModuleId, PortalController.Instance.GetPortal(this.PortalSettings.PortalId))
                        || TabController.Instance.GetTabsByModuleID(this.ModuleContext.ModuleId).Count > 1;
						Logger.LogDebug("OnLoad(e={E}): this.IsShared → {Arg1}", e, this.IsShared);
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering Render(writer={Writer})", writer);
            base.Render(writer);

            foreach (int id in this.validIDs)
            {
                this.Page.ClientScript.RegisterForEventValidation(this.actionButton.UniqueID, id.ToString());
            }
			Logger.LogWarning("<<< Exiting Render(writer={Writer})", writer);
        }

        private void ActionButton_Click(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering ActionButton_Click(sender={Sender},e={E})", sender, e);
            this.ProcessAction(this.Request.Params["__EVENTARGUMENT"]);
			Logger.LogWarning("<<< Exiting ActionButton_Click(sender={Sender},e={E})", sender, e);
        }
    }
}
