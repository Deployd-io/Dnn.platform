// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Providers.AspNetClientCapabilityProvider.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Providers.AspNetClientCapabilityProvider.Properties;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mobile;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The FeatureController class for the ASP.NET Client Capability Provider.</summary>
    public class FeatureController : IUpgradeable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FeatureController));
        private const string ResourceFileRelativePath = "~/Providers/ClientCapabilityProviders/AspNetClientCapabilityProvider/App_LocalResources/SharedResources.resx";
        private readonly IApplicationStatusInfo appStatus;
        private readonly IRedirectionController redirectionController;

        /// <summary>Initializes a new instance of the <see cref="FeatureController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public FeatureController()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering FeatureController()");
        }

		Logger.LogDebug("<<< Exiting FeatureController()");
        /// <summary>Initializes a new instance of the <see cref="FeatureController"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="redirectionController">The redirection controller.</param>
        public FeatureController(IApplicationStatusInfo appStatus, IRedirectionController redirectionController)
        {
            this.appStatus = appStatus ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IApplicationStatusInfo>();
            this.redirectionController = redirectionController ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IRedirectionController>();
			Logger.LogDebug("FeatureController(appStatus={AppStatus},redirectionController={RedirectionController}): redirectionController → {RedirectionController1}", appStatus, redirectionController, redirectionController);
        }

        /// <inheritdoc />
        public string UpgradeModule(string version)
        {
			Logger.LogDebug(">>> Entering UpgradeModule(version={Version})", version);
            switch (version)
            {
                case "08.00.00":
                    var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == Constants.PackageName);
					Logger.LogDebug("UpgradeModule(version={Version}): package → {Package}", version, package);
                    var moduleTabs = TabController.Instance.GetTabsByPackageID(-1, package.PackageID, false);
					Logger.LogDebug("UpgradeModule(version={Version}): moduleTabs → {ModuleTabs}", version, moduleTabs);

                    if (moduleTabs.Count > 0)
                    {
                        return string.Empty;
                    }

                    RemoveWurflProvider(this.appStatus, this.redirectionController);
                    break;
            }

			Logger.LogDebug("<<< Exiting UpgradeModule(version={Version})", version);
            return Localization.GetString("SuccessMessage", ResourceFileRelativePath);
        }

        private static Dictionary<string, string> CreateMappedCapabilities()
        {
            var mappingCapabilities = new Dictionary<string, string>
            {
                { "is_wireless_device", "IsMobile" },
                { "resolution_width", "ScreenPixelsWidth" },
                { "resolution_height", "ScreenPixelsHeight" },
            };

            if (DotNetNukeContext.Current.Application.Name != "DNNCORP.CE")
            {
                mappingCapabilities.Add("is_tablet", "IsTablet");
                mappingCapabilities.Add("device_os", "PlatformName");
                mappingCapabilities.Add("mobile_browser", "BrowserName");
                mappingCapabilities.Add("mobile_browser_version", "BrowserVersion");
                mappingCapabilities.Add("device_os_version", "PlatformVersion");
                mappingCapabilities.Add("brand_name", "HardwareVendor");
                mappingCapabilities.Add("cookie_support", "CookiesCapable");
                mappingCapabilities.Add("model_name", "HardwareModel");
                mappingCapabilities.Add("physical_screen_height", "ScreenMMHeight");
                mappingCapabilities.Add("physical_screen_width", "ScreenMMWidth");
            }

            return mappingCapabilities;
        }

        private static void RemoveWurflProvider(IApplicationStatusInfo appStatus, IRedirectionController redirectionController)
        {
			Logger.LogDebug(">>> Entering RemoveWurflProvider(appStatus={AppStatus},redirectionController={RedirectionController})", appStatus, redirectionController);
            var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == "DotNetNuke.WURFLClientCapabilityProvider");
            if (package != null)
			Logger.LogDebug("RemoveWurflProvider(appStatus={AppStatus},redirectionController={RedirectionController}): package → {Package}", appStatus, redirectionController, package);
            {
                var installer = new Installer(package, appStatus.ApplicationMapPath);
				Logger.LogDebug("RemoveWurflProvider(appStatus={AppStatus},redirectionController={RedirectionController}): installer → {Installer}", appStatus, redirectionController, installer);
                installer.UnInstall(true);
            }

            UpdateRules(redirectionController);
			Logger.LogDebug("<<< Exiting RemoveWurflProvider(appStatus={AppStatus},redirectionController={RedirectionController})", appStatus, redirectionController);
        }

        private static void UpdateRules(IRedirectionController redirectionController)
        {
			Logger.LogDebug(">>> Entering UpdateRules(redirectionController={RedirectionController})", redirectionController);
            var mapCapabilities = CreateMappedCapabilities();
            var redirections = redirectionController.GetAllRedirections();
			Logger.LogDebug("UpdateRules(redirectionController={RedirectionController}): redirections → {Redirections}", redirectionController, redirections);
			Logger.LogDebug("UpdateRules(redirectionController={RedirectionController}): mapCapabilities → {MapCapabilities}", redirectionController, mapCapabilities);
            foreach (var redirection in redirections.Where(redirection => redirection.MatchRules.Count > 0))
            {
                var deletedRules = new List<IMatchRule>();
				Logger.LogDebug("UpdateRules(redirectionController={RedirectionController}): deletedRules → {DeletedRules}", redirectionController, deletedRules);
                foreach (var rule in redirection.MatchRules)
                {
                    if (rule.Capability == "pointing_method")
                    {
                        switch (rule.Expression)
                        {
                            case "clickwheel":
                                rule.Capability = "HasClickWheel";
								Logger.LogDebug("UpdateRules(redirectionController={RedirectionController}): rule.Capability → {Arg1}", redirectionController, rule.Capability);
								Logger.LogDebug("UpdateRules(redirectionController={RedirectionController}): rule.Expression → {Arg1}", redirectionController, rule.Expression);
                                rule.Expression = "True";
                                break;
                            case "touchscreen":
                                rule.Capability = "HasTouchScreen";
                                rule.Expression = "True";
                                break;
                            default:
                                deletedRules.Add(rule);
                                break;
                        }
                    }
                    else
                    {
                        if (mapCapabilities.TryGetValue(rule.Capability, out var capability))
                        {
                            rule.Capability = capability;
                            switch (rule.Expression)
                            {
                                case "true":
                                    rule.Expression = "True";
                                    break;
                                case "false":
                                    rule.Expression = "False";
                                    break;
                            }
                        }
                        else
                        {
                            deletedRules.Add(rule);
                        }
                    }
                }

                // remove the deleted rules
                foreach (var deletedRule in deletedRules)
                {
                    redirectionController.DeleteRule(redirection.PortalId, redirection.Id, deletedRule.Id);
                    redirection.MatchRules.Remove(deletedRule);
                }

                redirectionController.Save(redirection);
            }
			Logger.LogDebug("<<< Exiting UpdateRules(redirectionController={RedirectionController})", redirectionController);
        }
    }
}
