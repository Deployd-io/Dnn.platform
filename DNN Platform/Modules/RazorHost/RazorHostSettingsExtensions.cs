// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.RazorHost
{
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security;

    /// <summary>Extension methods for the razor host settings.</summary>
    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    public static partial class RazorHostSettingsExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RazorHostSettingsExtensions));
        /// <summary>Loads the razor host settings control.</summary>
        /// <param name="parent">The parent user control.</param>
        /// <param name="configuration">The module configuration, <see cref="ModuleInfo"/>.</param>
        /// <param name="localResourceFile">The local resource file for localization strings.</param>
        /// <returns>Razor Host Settings, <see cref="Settings"/>.</returns>
        public static Settings LoadRazorSettingsControl(this UserControl parent, ModuleInfo configuration, string localResourceFile)
        {
			Logger.LogDebug(">>> Entering LoadRazorSettingsControl(parent={Parent},configuration={Configuration},localResourceFile={LocalResourceFile})", parent, configuration, localResourceFile);
            var control = (Settings)parent.LoadControl("~/DesktopModules/RazorModules/RazorHost/Settings.ascx");
            control.ModuleConfiguration = configuration;
			Logger.LogDebug("LoadRazorSettingsControl(parent={Parent},configuration={Configuration},localResourceFile={LocalResourceFile}): control.ModuleConfiguration → {Arg3}", parent, configuration, localResourceFile, control.ModuleConfiguration);
			Logger.LogDebug("LoadRazorSettingsControl(parent={Parent},configuration={Configuration},localResourceFile={LocalResourceFile}): control.LocalResourceFile → {Arg3}", parent, configuration, localResourceFile, control.LocalResourceFile);
			Logger.LogDebug("LoadRazorSettingsControl(parent={Parent},configuration={Configuration},localResourceFile={LocalResourceFile}): control → {Control}", parent, configuration, localResourceFile, control);
            control.LocalResourceFile = localResourceFile;
            EnsureEditScriptControlIsRegistered(configuration.ModuleDefID);
			Logger.LogDebug("<<< Exiting LoadRazorSettingsControl(parent={Parent},configuration={Configuration},localResourceFile={LocalResourceFile})", parent, configuration, localResourceFile);
            return control;
        }

        private static void EnsureEditScriptControlIsRegistered(int moduleDefId)
        {
			Logger.LogDebug(">>> Entering EnsureEditScriptControlIsRegistered(moduleDefId={ModuleDefId})", moduleDefId);
            if (ModuleControlController.GetModuleControlByControlKey("EditRazorScript", moduleDefId) != null)
            {
                return;
            }

            var m = new ModuleControlInfo
            {
                ControlKey = "EditRazorScript",
                ControlSrc = "DesktopModules/RazorModules/RazorHost/EditScript.ascx",
                ControlTitle = "Edit Script",
                ControlType = SecurityAccessLevel.Host,
                ModuleDefID = moduleDefId,
            };
            ModuleControlController.UpdateModuleControl(m);
			Logger.LogDebug("<<< Exiting EnsureEditScriptControlIsRegistered(moduleDefId={ModuleDefId})", moduleDefId);
        }
    }
}
