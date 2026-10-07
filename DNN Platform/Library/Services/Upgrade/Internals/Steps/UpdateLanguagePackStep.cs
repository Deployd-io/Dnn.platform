// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    using System;
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Upgrade.Internals;
    using DotNetNuke.Services.Upgrade.Internals.Steps;

    /// <summary>UpdateLanguagePackStep - Step that downloads and installs language pack.</summary>
    public class UpdateLanguagePackStep : BaseInstallationStep
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UpdateLanguagePackStep));

        /// <summary>Main method to execute the step.</summary>
        public override void Execute()
        {
			Logger.LogError(">>> Entering Execute()");
            this.Percentage = 0;
            this.Status = StepStatus.Running;
			Logger.LogDebug("Execute(): this.Percentage → {Arg0}", this.Percentage);
			Logger.LogDebug("Execute(): this.Status → {Arg0}", this.Status);

            var installConfig = InstallController.Instance.GetInstallConfig();
			Logger.LogError("Execute(): installConfig → {InstallConfig}", installConfig);
            string culture = installConfig.InstallCulture;

            if (!culture.Equals("en-us", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    // need apply the Licensing module after packages installed, so that we can know what's the edition of install instance. CE/PE/EE
                    var document = Config.Load();
					Logger.LogError("Execute(): document → {Document}", document);
                    var licensingNode = document.SelectSingleNode("/configuration/system.webServer/modules/add[@name='Licensing']");
					Logger.LogError("Execute(): licensingNode → {LicensingNode}", licensingNode);
                    if (licensingNode != null)
                    {
                        var type = licensingNode.Attributes["type"].Value;
						Logger.LogError("Execute(): type → {Type}", type);
                        var module = Reflection.CreateObject(type, null, false) as IHttpModule;
						Logger.LogError("Execute(): module → {Module}", module);
                        module.Init(HttpContext.Current.ApplicationInstance);
                    }

                    InstallController.Instance.IsAvailableLanguagePack(culture);
                }
                catch (Exception ex)
                {
                    // we shouldn't break the install process when LP download failed, for admin user can install the LP after website created.
                    // so we logged what's wrong here, and user can check it later.
                    Logger.Error(ex);
                }
            }
			Logger.LogWarning("<<< Exiting Execute()");

            this.Status = StepStatus.Done;
        }
    }
}
