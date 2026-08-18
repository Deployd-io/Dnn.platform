// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.ControlPanels;
    using DotNetNuke.UI.Modules;

    public class UIUtilities
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UIUtilities));
        public static ModuleInfo GetSlaveModule(int tabId)
        {
			Logger.LogDebug(">>> Entering GetSlaveModule(tabId={TabId})", tabId);
            var key = GetControlKey();
            var moduleId = GetModuleId(key);
			Logger.LogDebug("GetSlaveModule(tabId={TabId}): moduleId → {ModuleId}", tabId, moduleId);
			Logger.LogWarning("GetSlaveModule(tabId={TabId}): key → {Key}", tabId, key);

            ModuleInfo slaveModule = GetSlaveModule(moduleId, key, tabId);
			Logger.LogDebug("GetSlaveModule(tabId={TabId}): slaveModule → {SlaveModule}", tabId, slaveModule);
            if (slaveModule != null)
            {
                var moduleControl = ModuleControlController.GetModuleControlByControlKey(key, slaveModule.ModuleDefID) ??
                                    ModuleControlController.GetModuleControlByControlKey(key, Null.NullInteger);
                if (moduleControl != null)
                {
                    slaveModule.ModuleControlId = moduleControl.ModuleControlID;
					Logger.LogDebug("GetSlaveModule(tabId={TabId}): slaveModule.ModuleControlId → {Arg1}", tabId, slaveModule.ModuleControlId);
                }
            }

			Logger.LogDebug("<<< Exiting GetSlaveModule(tabId={TabId})", tabId);
			Logger.LogDebug("<<< Exiting GetSlaveModule(moduleId={ModuleId},key={Key},tabId={TabId})", moduleId, key, tabId);
            return slaveModule;
        }

        public static bool IsLegacyUI(int moduleId, string key, int portalId)
        {
			Logger.LogDebug(">>> Entering IsLegacyUI(moduleId={ModuleId},key={Key},portalId={PortalId})", moduleId, key, portalId);
            var request = HttpContext.Current.Request;
            var isLegacyUi = true;
			Logger.LogDebug("IsLegacyUI(moduleId={ModuleId},key={Key},portalId={PortalId}): isLegacyUi → {IsLegacyUi}", moduleId, key, portalId, isLegacyUi);
            var currentPage = TabController.CurrentPage;
            var settings = PortalController.Instance.GetCurrentSettings();
			Logger.LogDebug("IsLegacyUI(moduleId={ModuleId},key={Key},portalId={PortalId}): settings → {Settings}", moduleId, key, portalId, settings);
            if (settings != null)
            {
                isLegacyUi = !(settings.EnablePopUps && !request.Browser.Crawler && request.Browser.EcmaScriptVersion >= new Version(1, 0));

                if (!isLegacyUi && !string.IsNullOrEmpty(key))
                {
                    var slaveModule = GetSlaveModule(moduleId, key, currentPage.TabID);
					Logger.LogDebug("IsLegacyUI(moduleId={ModuleId},key={Key},portalId={PortalId}): slaveModule → {SlaveModule}", moduleId, key, portalId, slaveModule);
                    if (slaveModule != null)
                    {
                        var moduleControl = ModuleControlController.GetModuleControlByControlKey(key, slaveModule.ModuleDefID) ??
                                            ModuleControlController.GetModuleControlByControlKey(key, Null.NullInteger);
                        if (moduleControl != null)
                        {
                            isLegacyUi = !moduleControl.SupportsPopUps;
                        }
                    }
                }
            }

			Logger.LogDebug("<<< Exiting IsLegacyUI(moduleId={ModuleId},key={Key},portalId={PortalId})", moduleId, key, portalId);
			Logger.LogDebug("<<< Exiting IsLegacyUI(portalId={PortalId})", portalId);
            return isLegacyUi;
        }

        public static bool IsLegacyUI(int portalId)
        {
            var key = GetControlKey();
            var moduleId = GetModuleId(key);

            return IsLegacyUI(moduleId, key, portalId);
        }

        internal static string GetControlKey()
        {
			Logger.LogDebug(">>> Entering GetControlKey()");
            HttpRequest request = HttpContext.Current.Request;

            string key = string.Empty;
			Logger.LogWarning("GetControlKey(): key → {Key}", key);
            if (request.QueryString["ctl"] != null)
            {
                key = request.QueryString["ctl"];
            }

			Logger.LogDebug("<<< Exiting GetControlKey()");
            return key;
        }

        internal static int GetModuleId(string key)
        {
			Logger.LogDebug(">>> Entering GetModuleId(key={Key})", key);
            HttpRequest request = HttpContext.Current.Request;

            int moduleId = -1;
			Logger.LogDebug("GetModuleId(key={Key}): moduleId → {ModuleId}", key, moduleId);
            if (request.QueryString["mid"] != null)
            {
                if (!int.TryParse(request.QueryString["mid"], out moduleId))
                {
                    moduleId = -1;
                }
            }

            if (request.QueryString["moduleid"] != null && (key.Equals("module", StringComparison.OrdinalIgnoreCase) || key.Equals("help", StringComparison.OrdinalIgnoreCase)))
            {
                if (!int.TryParse(request.QueryString["moduleid"], out moduleId))
                {
                    moduleId = -1;
                }
            }

			Logger.LogDebug("<<< Exiting GetModuleId(key={Key})", key);
            return moduleId;
        }

        internal static string GetRenderMode()
        {
			Logger.LogDebug(">>> Entering GetRenderMode()");
            HttpRequest request = HttpContext.Current.Request;

            var renderMode = string.Empty;
			Logger.LogDebug("GetRenderMode(): renderMode → {RenderMode}", renderMode);
            if (request.QueryString["render"] != null)
            {
                renderMode = request.QueryString["render"];
            }

			Logger.LogDebug("<<< Exiting GetRenderMode()");
            return renderMode;
        }

        internal static ModuleInfo GetSlaveModule(int moduleId, string key, int tabId)
        {
            HttpRequest request = HttpContext.Current.Request;

            ModuleInfo slaveModule = null;
            if (moduleId != -1)
            {
                ModuleInfo module = ModuleController.Instance.GetModule(moduleId, tabId, false);
                if (module != null)
                {
                    slaveModule = module.Clone();
                }
            }

            if (slaveModule == null)
            {
                slaveModule = new ModuleInfo { ModuleID = moduleId, ModuleDefID = -1, TabID = tabId, InheritViewPermissions = true };
            }

            if (request.QueryString["moduleid"] != null && (key.Equals("module", StringComparison.OrdinalIgnoreCase) || key.Equals("help", StringComparison.OrdinalIgnoreCase)))
            {
                slaveModule.ModuleDefID = -1;
            }

            if (request.QueryString["dnnprintmode"] != "true")
            {
                slaveModule.ModuleTitle = string.Empty;
            }

            slaveModule.Header = string.Empty;
            slaveModule.Footer = string.Empty;
            slaveModule.StartDate = DateTime.MinValue;
            slaveModule.EndDate = DateTime.MaxValue;
            slaveModule.Visibility = VisibilityState.None;
            slaveModule.Color = string.Empty;
            slaveModule.Border = string.Empty;
            slaveModule.DisplayTitle = true;
            slaveModule.DisplayPrint = false;
            slaveModule.DisplaySyndicate = false;

            return slaveModule;
        }

        /// <summary>Gets the path to the local resource file associated to the control.</summary>
        /// <param name="ctrl">The control.</param>
        /// <returns>The path to the resource file.</returns>
        internal static string GetLocalResourceFile(Control ctrl)
        {
			Logger.LogDebug(">>> Entering GetLocalResourceFile(ctrl={Ctrl})", ctrl);
            string resourceFileName = Null.NullString;

			Logger.LogDebug("GetLocalResourceFile(ctrl={Ctrl}): resourceFileName → {ResourceFileName}", ctrl, resourceFileName);
            while (ctrl != null)
            {
                if (ctrl is UserControl)
                {
                    resourceFileName = $"{ctrl.TemplateSourceDirectory}/{Localization.LocalResourceDirectory}/{ctrl.GetType().BaseType.Name}.ascx.resx";
                    if (File.Exists(ctrl.Page.Server.MapPath(resourceFileName)))
                    {
                        break;
                    }
                }

                if (ctrl is IModuleControl moduleControl)
                {
                    resourceFileName = moduleControl.LocalResourceFile;
                    break;
                }

                if (ctrl is ControlPanelBase controlPanel)
                {
                    resourceFileName = controlPanel.LocalResourceFile;
                    break;
                }

                if (ctrl is Page)
                {
                    resourceFileName = $"{ctrl.TemplateSourceDirectory}/{Localization.LocalResourceDirectory}/{ctrl.GetType().BaseType.Name}.aspx.resx";
                }

                ctrl = ctrl.Parent;
            }

			Logger.LogDebug("<<< Exiting GetLocalResourceFile(ctrl={Ctrl})", ctrl);
            return resourceFileName;
        }
    }
}
