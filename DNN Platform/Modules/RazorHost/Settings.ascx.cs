// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.RazorHost
{
    using System;
    using System.IO;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>Implements the Settings view logic.</summary>
    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    public partial class Settings : ModuleSettingsBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private string razorScriptFolder = "~/DesktopModules/RazorModules/RazorHost/Scripts/";

        /// <inheritdoc />
        public override void LoadSettings()
        {
			Logger.LogDebug(">>> Entering LoadSettings()");
            string basePath = this.Server.MapPath(this.razorScriptFolder);
            var scriptFileSetting = this.Settings["ScriptFile"] as string;
			Logger.LogDebug("LoadSettings(): scriptFileSetting → {ScriptFileSetting}", scriptFileSetting);

            foreach (string script in Directory.GetFiles(this.Server.MapPath(this.razorScriptFolder), "*.??html"))
            {
                string scriptPath = script.Replace(basePath, string.Empty);
                var item = new ListItem(scriptPath, scriptPath);
				Logger.LogDebug("LoadSettings(): item → {Item}", item);
                if (!string.IsNullOrEmpty(scriptFileSetting) && scriptPath.ToLowerInvariant() == scriptFileSetting.ToLowerInvariant())
                {
                    item.Selected = true;
					Logger.LogDebug("LoadSettings(): item.Selected → {Arg0}", item.Selected);
                }

                this.scriptList.Items.Add(item);
            }

            base.LoadSettings();
			Logger.LogDebug("<<< Exiting LoadSettings()");
        }

        /// <inheritdoc />
        public override void UpdateSettings()
        {
			Logger.LogDebug(">>> Entering UpdateSettings()");
            ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ScriptFile", this.scriptList.SelectedValue);
			Logger.LogDebug("<<< Exiting UpdateSettings()");
        }
    }
}
