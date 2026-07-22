// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Prompt
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Prompt;
    using DotNetNuke.Abstractions.Users;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Prompt;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;

    /// <summary>This is a (Prompt) Console Command. You should not reference this class directly. It is to be used solely through Prompt.</summary>
    [ConsoleCommand("add-module", Constants.CommandCategoryKeys.Modules, "Prompt_AddModule_Description")]
    public class AddModule : ConsoleCommand
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AddModule));

        /// <inheritdoc />
        public override string LocalResourceFile => Constants.DefaultPromptResourceFile;

        [ConsoleCommandParameter("name", "Prompt_AddModule_FlagModuleName", true)]
        public string ModuleName { get; set; }

        [ConsoleCommandParameter("pageid", "Prompt_AddModule_FlagPageId", true)]
        public int PageId { get; set; } // the page on which to add the module

        [ConsoleCommandParameter("pane", "Prompt_AddModule_FlagPane", "ContentPane")]
        public string Pane { get; set; }

        [ConsoleCommandParameter("title", "Prompt_AddModule_FlagModuleTitle")]
        public string ModuleTitle { get; set; } // title for the new module. defaults to friendly name

        /// <inheritdoc />
        public override void Initialize(string[] args, IPortalSettings portalSettings, IUserInfo userInfo, int activeTabId)
        {
			Logger.LogDebug(">>> Entering Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            base.Initialize(args, portalSettings, userInfo, activeTabId);
            this.ParseParameters(this);
			Logger.LogDebug("<<< Exiting Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public override IConsoleResultModel Run()
        {
			Logger.LogDebug(">>> Entering Run()");
            try
            {
                var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(this.ModuleName, this.PortalId);
                if (desktopModule == null)
				Logger.LogDebug("Run(): desktopModule → {DesktopModule}", desktopModule);
                {
                    return new ConsoleErrorResultModel(string.Format(CultureInfo.CurrentCulture, this.LocalizeString("Prompt_DesktopModuleNotFound"), this.ModuleName));
                }

                var message = default(KeyValuePair<HttpStatusCode, string>);
                var page = TabController.Instance.GetTab(this.PageId, this.PortalSettings.PortalId);
				Logger.LogDebug("Run(): message → {Message}", message);
                if (page == null)
				Logger.LogDebug("Run(): page → {Page}", page);
                {
                    message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, string.Format(CultureInfo.CurrentCulture, Localization.GetString("Prompt_PageNotFound", this.LocalResourceFile), this.PageId));
                    return null;
                }

                if (!TabPermissionController.CanManagePage(page))
                {
                    message = new KeyValuePair<HttpStatusCode, string>(HttpStatusCode.NotFound, Localization.GetString("Prompt_InsufficientPermissions", this.LocalResourceFile));
                    return null;
                }

                var moduleList = new List<ModuleInfo>();

				Logger.LogDebug("Run(): moduleList → {ModuleList}", moduleList);
                foreach (var objModuleDefinition in ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModule.DesktopModuleID).Values)
                {
                    var objModule = new ModuleInfo();
                    objModule.Initialize(this.PortalSettings.PortalId);
					Logger.LogDebug("Run(): objModule → {ObjModule}", objModule);

                    objModule.PortalID = this.PortalSettings.PortalId;
					Logger.LogDebug("Run(): objModule.PortalID → {Arg0}", objModule.PortalID);
					Logger.LogDebug("Run(): objModule.TabID → {Arg0}", objModule.TabID);
					Logger.LogDebug("Run(): objModule.ModuleOrder → {Arg0}", objModule.ModuleOrder);
                    objModule.TabID = this.PageId;
                    objModule.ModuleOrder = 0;
                    objModule.ModuleTitle = string.IsNullOrEmpty(this.ModuleTitle) ? objModuleDefinition.FriendlyName : this.ModuleTitle;
                    objModule.PaneName = this.Pane;
					Logger.LogDebug("Run(): objModule.PaneName → {Arg0}", objModule.PaneName);
					Logger.LogDebug("Run(): objModule.ModuleDefID → {Arg0}", objModule.ModuleDefID);
                    objModule.ModuleDefID = objModuleDefinition.ModuleDefID;
                    if (objModuleDefinition.DefaultCacheTime > 0)
                    {
                        objModule.CacheTime = objModuleDefinition.DefaultCacheTime;
						Logger.LogDebug("Run(): objModule.CacheTime → {Arg0}", objModule.CacheTime);
                        if (this.PortalSettings.DefaultModuleId > Null.NullInteger &&
                            this.PortalSettings.DefaultTabId > Null.NullInteger)
                        {
                            var defaultModule = ModuleController.Instance.GetModule(
                                this.PortalSettings.DefaultModuleId,
                                this.PortalSettings.DefaultTabId,
                                true);
                            if (defaultModule != null)
                            {
                                objModule.CacheTime = defaultModule.CacheTime;
                            }
                        }
                    }

                    ModuleController.Instance.InitialModulePermission(objModule, objModule.TabID, 0);

                    if (this.PortalSettings.ContentLocalizationEnabled)
                    {
                        var defaultLocale = LocaleController.Instance.GetDefaultLocale(this.PortalSettings.PortalId);

						Logger.LogDebug("Run(): defaultLocale → {DefaultLocale}", defaultLocale);
                        // check whether original tab is exists, if true then set culture code to default language,
                        // otherwise set culture code to current.
                        objModule.CultureCode =
                            TabController.Instance.GetTabByCulture(objModule.TabID, this.PortalSettings.PortalId, defaultLocale) !=
                            null
                                ? defaultLocale.Code
                                : this.PortalSettings.CultureCode;
                    }
                    else
                    {
                        objModule.CultureCode = Null.NullString;
						Logger.LogDebug("Run(): objModule.CultureCode → {Arg0}", objModule.CultureCode);
                    }

                    objModule.AllTabs = false;
					Logger.LogDebug("Run(): objModule.AllTabs → {Arg0}", objModule.AllTabs);
					Logger.LogDebug("Run(): objModule.Alignment → {Arg0}", objModule.Alignment);
                    objModule.Alignment = null;

                    ModuleController.Instance.AddModule(objModule);
                    moduleList.Add(objModule);

                    // Set position so future additions to page can operate correctly
                    var position = ModuleController.Instance.GetTabModule(objModule.TabModuleID).ModuleOrder + 1;
                }
				Logger.LogDebug("Run(): position → {Position}", position);

                if (moduleList == null)
                {
                    return new ConsoleErrorResultModel(message.Value);
                }

                if (moduleList.Count == 0)
                {
                    return new ConsoleErrorResultModel(this.LocalizeString("Prompt_NoModulesAdded"));
                }

                var modules = moduleList.Select(newModule => ModuleController.Instance.GetTabModule(newModule.TabModuleID)).ToList();

				Logger.LogDebug("Run(): modules → {Modules}", modules);
                return new ConsoleResultModel(string.Format(CultureInfo.CurrentCulture, this.LocalizeString("Prompt_ModuleAdded"), modules.Count, moduleList.Count == 1 ? string.Empty : "s")) { Data = modules, Records = modules.Count };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting Run()");
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_AddModuleError"));
            }
        }
    }
}
