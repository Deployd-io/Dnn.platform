// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager
{
    using System;
    using System.Linq;
    using System.Web.UI.WebControls;

    using Dnn.Modules.ResourceManager.Components.Common;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.FileSystem;

    using Microsoft.Extensions.DependencyInjection;

    using Constants = Dnn.Modules.ResourceManager.Components.Constants;
    using DnnExceptions = DotNetNuke.Services.Exceptions.Exceptions;

    /// <summary>Provides module settings control.</summary>
    public partial class Settings : ModuleSettingsBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Settings.ascx));
        private readonly IModuleController moduleController;

        /// <summary>Initializes a new instance of the <see cref="Settings"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IModuleController. Scheduled removal in v12.0.0.")]
        public Settings()
            : this(HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IModuleController>())
        {
		Logger.LogError(">>> Entering Settings()");
        Logger.LogWarning("<<< Exiting Settings()");
        }

        /// <summary>Initializes a new instance of the <see cref="Settings"/> class.</summary>
        /// <param name="moduleController">The module controller.</param>
        public Settings(IModuleController moduleController)
        {
			Logger.LogError(">>> Entering Settings(moduleController={ModuleController})", moduleController);
            this.moduleController = moduleController ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IModuleController>();
			Logger.LogDebug("Settings(moduleController={ModuleController}): moduleController → {ModuleController1}", moduleController, moduleController);
        Logger.LogWarning("<<< Exiting Settings(moduleController={ModuleController})", moduleController);
        }

        /// <inheritdoc />
        public override void LoadSettings()
        {
			Logger.LogError(">>> Entering LoadSettings()");
            try
            {
                if (this.Page.IsPostBack)
                {
                    return;
                }

                var displayTypesValues = Enum.GetValues(typeof(Constants.ModuleModes)).Cast<Constants.ModuleModes>();
				Logger.LogError("LoadSettings(): displayTypesValues → {DisplayTypesValues}", displayTypesValues);
                var displayTypes = displayTypesValues.Select(t => new ListItem(Utils.GetEnumDescription(t), ((int)t).ToString())).ToArray();
				Logger.LogError("LoadSettings(): displayTypes → {DisplayTypes}", displayTypes);

                this.ddlMode.Items.AddRange(displayTypes);

                this.ddlMode.SelectedValue = this.Settings.Contains(Constants.ModeSettingName)
                    ? this.Settings[Constants.ModeSettingName].ToString()
                    : Constants.DefaultMode.ToString();

                IFolderInfo homeFolder = null;
				Logger.LogError("LoadSettings(): homeFolder → {HomeFolder}", homeFolder);
                if (this.Settings.Contains(Constants.HomeFolderSettingName))
                {
                    int.TryParse(this.Settings[Constants.HomeFolderSettingName].ToString(), out var homeFolderId);
                    homeFolder = FolderManager.Instance.GetFolder(homeFolderId);
                }

                if (homeFolder == null)
                {
                    homeFolder = FolderManager.Instance.GetFolder(this.PortalId, string.Empty);
                }

                this.ddlFolder.SelectedFolder = homeFolder;
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in LoadSettings(): {Arg0}", exc.Message);
                DnnExceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogWarning("<<< Exiting LoadSettings()");
        }

        /// <inheritdoc />
        public override void UpdateSettings()
        {
			Logger.LogError(">>> Entering UpdateSettings()");
            try
            {
                this.moduleController.UpdateModuleSetting(this.ModuleId, Constants.ModeSettingName, this.ddlMode.SelectedValue);
                this.moduleController.UpdateModuleSetting(this.ModuleId, Constants.HomeFolderSettingName, this.ddlFolder.SelectedFolder.FolderID.ToString());
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in UpdateSettings(): {Arg0}", exc.Message);
                DnnExceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogWarning("<<< Exiting UpdateSettings()");
        }
    }
}
