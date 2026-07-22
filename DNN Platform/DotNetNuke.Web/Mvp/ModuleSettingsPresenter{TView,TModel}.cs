// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvp
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>Represents a class that is a presenter for a settings controlin a Web Forms Model-View-Presenter application.</summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    [DnnDeprecated(9, 2, 0, "Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
    public abstract partial class ModuleSettingsPresenter<TView, TModel> : ModulePresenterBase<TView>
        where TView : class, ISettingsView<TModel>
        where TModel : SettingsModel, new()
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="ModuleSettingsPresenter{TView, TModel}"/> class.</summary>
        /// <param name="view">The view.</param>
        protected ModuleSettingsPresenter(TView view)
            : base(view)
        {
			Logger.LogDebug(">>> Entering ModuleSettingsPresenter(view={View})", view);
            view.OnLoadSettings += this.OnLoadSettingsInternal;
            view.OnSaveSettings += this.OnSaveSettingsInternal;
			Logger.LogDebug("<<< Exiting ModuleSettingsPresenter(view={View})", view);
        }

        /// <inheritdoc />
        protected override void OnLoad()
        {
			Logger.LogDebug(">>> Entering OnLoad()");
            base.OnLoad();

            if (this.IsPostBack)
            {
                // Initialize dictionaries as LoadSettings is not called on Postback
                this.View.Model.ModuleSettings = new Dictionary<string, string>();
                this.View.Model.TabModuleSettings = new Dictionary<string, string>();
            }
			Logger.LogDebug("<<< Exiting OnLoad()");
        }

        /// <summary>Loads the module settings.</summary>
        protected virtual void LoadSettings()
        {
			Logger.LogDebug(">>> Entering LoadSettings()");
            this.View.Model.ModuleSettings = new Dictionary<string, string>(
                                            this.ModuleContext.Configuration.ModuleSettings
                                            .Cast<DictionaryEntry>()
                                            .ToDictionary(kvp => (string)kvp.Key, kvp => (string)kvp.Value));

            this.View.Model.TabModuleSettings = new Dictionary<string, string>(
                                            this.ModuleContext.Configuration.TabModuleSettings
                                            .Cast<DictionaryEntry>()
                                            .ToDictionary(kvp => (string)kvp.Key, kvp => (string)kvp.Value));
											Logger.LogDebug("<<< Exiting LoadSettings()");
        }

        /// <summary>Updates the module settings.</summary>
        protected virtual void SaveSettings()
        {
			Logger.LogDebug(">>> Entering SaveSettings()");
            foreach (var setting in this.View.Model.ModuleSettings)
            {
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, setting.Key, setting.Value);
            }

            foreach (var setting in this.View.Model.TabModuleSettings)
            {
                ModuleController.Instance.UpdateTabModuleSetting(this.ModuleContext.TabModuleId, setting.Key, setting.Value);
            }
			Logger.LogDebug("<<< Exiting SaveSettings()");
        }

        private void OnLoadSettingsInternal(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoadSettingsInternal(sender={Sender},e={E})", sender, e);
            this.LoadSettings();
			Logger.LogDebug("<<< Exiting OnLoadSettingsInternal(sender={Sender},e={E})", sender, e);
        }

        private void OnSaveSettingsInternal(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnSaveSettingsInternal(sender={Sender},e={E})", sender, e);
            this.SaveSettings();
			Logger.LogDebug("<<< Exiting OnSaveSettingsInternal(sender={Sender},e={E})", sender, e);
        }
    }
}
