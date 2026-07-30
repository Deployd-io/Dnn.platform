// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html
{
    using System;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Modules.Html.Components;
    using DotNetNuke.Services.Exceptions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The Settings ModuleSettingsBase is used to manage the settings for the HTML Module.</summary>
    public partial class Settings : ModuleSettingsBase
    {
        private readonly HtmlModuleSettingsRepository settingsRepository;
        private HtmlModuleSettings moduleSettings;

        /// <summary>Initializes a new instance of the <see cref="Settings"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with HtmlModuleSettingsRepository. Scheduled removal in v12.0.0.")]
        public Settings()
            : this(null)
        {
		Logger.LogDebug(">>> Entering Settings()");
        }
		Logger.LogDebug("<<< Exiting Settings()");

        /// <summary>Initializes a new instance of the <see cref="Settings"/> class.</summary>
        /// <param name="settingsRepository">The settings repository.</param>
        public Settings(HtmlModuleSettingsRepository settingsRepository)
        {
            this.settingsRepository = settingsRepository ?? this.DependencyProvider.GetRequiredService<HtmlModuleSettingsRepository>();
			Logger.LogDebug("Settings(settingsRepository={SettingsRepository}): settingsRepository → {SettingsRepository1}", settingsRepository, settingsRepository);
        }

        private new HtmlModuleSettings ModuleSettings => this.moduleSettings ??= this.settingsRepository.GetSettings(this.ModuleConfiguration);

        /// <summary>LoadSettings loads the settings from the Database and displays them.</summary>
        public override void LoadSettings()
        {
			Logger.LogDebug(">>> Entering LoadSettings()");
            try
            {
                if (!this.Page.IsPostBack)
                {
                    this.chkReplaceTokens.Checked = this.ModuleSettings.ReplaceTokens;
                    this.cbDecorate.Checked = this.ModuleSettings.UseDecorate;
                    this.txtSearchDescLength.Text = this.ModuleSettings.SearchDescLength.ToString();
                }

                // Module failed to load
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in LoadSettings(): {Arg0}", ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting LoadSettings()");
        }

        /// <summary>UpdateSettings saves the modified settings to the Database.</summary>
        public override void UpdateSettings()
        {
			Logger.LogDebug(">>> Entering UpdateSettings()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                // update replace token setting
                this.ModuleSettings.ReplaceTokens = this.chkReplaceTokens.Checked;
                this.ModuleSettings.UseDecorate = this.cbDecorate.Checked;
                this.ModuleSettings.SearchDescLength = int.Parse(this.txtSearchDescLength.Text);
                this.settingsRepository.SaveSettings(this.ModuleConfiguration, this.ModuleSettings);
				Logger.LogInformation("UpdateSettings(): save query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

                // disable module caching if token replace is enabled
                if (this.chkReplaceTokens.Checked)
                {
                    var module = ModuleController.Instance.GetModule(this.ModuleId, this.TabId, false);
                    if (module.CacheTime > 0)
					Logger.LogDebug("UpdateSettings(): module → {Module}", module);
                    {
                        module.CacheTime = 0;
						Logger.LogDebug("UpdateSettings(): module.CacheTime → {Arg0}", module.CacheTime);
                        ModuleController.Instance.UpdateModule(module);
                    }
                }

                // Module failed to load
            }
            catch (Exception exc)
            {
				private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
                Exceptions.ProcessModuleLoadException(this, exc);
				Logger.LogError("Exception in UpdateSettings(): {Arg0}", ex.Message);
            }
			Logger.LogDebug("<<< Exiting UpdateSettings()");
        }
    }
}
