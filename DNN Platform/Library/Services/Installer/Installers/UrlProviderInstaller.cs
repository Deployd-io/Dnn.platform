// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Urls;

    internal class UrlProviderInstaller : ComponentInstallerBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UrlProviderInstaller));
        private ExtensionUrlProviderInfo extensionUrlProvider;
        private ExtensionUrlProviderInfo installedExtensionUrlProvider;
        private string desktopModuleName;

        /// <summary>Gets a list of allowable file extensions (in addition to the Host's List).</summary>
        /// <value>A String.</value>
        public override string AllowableFiles
        {
            get
            {
                return "ashx, aspx, ascx, vb, cs, resx, css, js, resources, config, vbproj, csproj, sln, htm, html";
            }
        }

        /// <summary>The Commit method finalises the Install and commits any pending changes.</summary>
        public override void Commit()
        {
		Logger.LogDebug(">>> Entering Commit()");
		Logger.LogDebug("<<< Exiting Commit()");
        }

        /// <summary>The Install method installs the UrlProvider component.</summary>
        public override void Install()
        {
			Logger.LogDebug(">>> Entering Install()");
            try
            {
                // Ensure DesktopModule Cache is cleared
                DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.DesktopModuleCacheKey, Null.NullInteger));

                var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(this.desktopModuleName, Null.NullInteger);
				Logger.LogDebug("Install(): desktopModule → {DesktopModule}", desktopModule);
                if (desktopModule != null)
                {
                    this.extensionUrlProvider.DesktopModuleId = desktopModule.DesktopModuleID;
                }

                // Attempt to get the Desktop Module
                this.installedExtensionUrlProvider = ExtensionUrlProviderController.GetProviders(Null.NullInteger)
                                            .SingleOrDefault(p => p.ProviderType == this.extensionUrlProvider.ProviderType);
											Logger.LogDebug("Install(): installedExtensionUrlProvider → {InstalledExtensionUrlProvider}", installedExtensionUrlProvider);

                if (this.installedExtensionUrlProvider != null)
                {
                    this.extensionUrlProvider.ExtensionUrlProviderId = this.installedExtensionUrlProvider.ExtensionUrlProviderId;
                }

                ExtensionUrlProviderController.SaveProvider(this.extensionUrlProvider);

                this.Completed = true;
				Logger.LogDebug("Install(): this.Completed → {Arg0}", this.Completed);
                this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.URLPROVIDER_Registered, this.extensionUrlProvider.ProviderName));
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
			Logger.LogDebug("<<< Exiting Install()");
        }

        /// <summary>The ReadManifest method reads the manifest file for the component.</summary>
        /// <param name="manifestNav">The XPath navigator for the URL Provider section of the manifest.</param>
        public override void ReadManifest(XPathNavigator manifestNav)
        {
			Logger.LogDebug(">>> Entering ReadManifest(manifestNav={ManifestNav})", manifestNav);
            this.extensionUrlProvider = new ExtensionUrlProviderInfo
            {
				Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): this.extensionUrlProvider → {Arg1}", manifestNav, this.extensionUrlProvider);
                ProviderName = Util.ReadElement(manifestNav, "urlProvider/name", this.Log, Util.URLPROVIDER_NameMissing),
                ProviderType = Util.ReadElement(manifestNav, "urlProvider/type", this.Log, Util.URLPROVIDER_TypeMissing),
                SettingsControlSrc = Util.ReadElement(manifestNav, "urlProvider/settingsControlSrc"),
                IsActive = true,
                RedirectAllUrls = Convert.ToBoolean(Util.ReadElement(manifestNav, "urlProvider/redirectAllUrls", "false")),
                ReplaceAllUrls = Convert.ToBoolean(Util.ReadElement(manifestNav, "urlProvider/replaceAllUrls", "false")),
                RewriteAllUrls = Convert.ToBoolean(Util.ReadElement(manifestNav, "urlProvider/rewriteAllUrls", "false")),
            };
			Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): extensionUrlProvider → {ExtensionUrlProvider}", manifestNav, extensionUrlProvider);

            this.desktopModuleName = Util.ReadElement(manifestNav, "urlProvider/desktopModule");
            if (this.Log.Valid)
            {
                this.Log.AddInfo(Util.URLPROVIDER_ReadSuccess);
            }
			Logger.LogDebug("<<< Exiting ReadManifest(manifestNav={ManifestNav})", manifestNav);
        }

        /// <summary>
        /// The Rollback method undoes the installation of the component in the event
        /// that one of the other components fails.
        /// </summary>
        public override void Rollback()
        {
			Logger.LogDebug(">>> Entering Rollback()");
            // If Temp Provider exists then we need to update the DataStore with this
            if (this.installedExtensionUrlProvider == null)
            {
                // No Temp Provider - Delete newly added module
                this.DeleteProvider();
            }
            else
            {
                // Temp Provider - Rollback to Temp
                ExtensionUrlProviderController.SaveProvider(this.installedExtensionUrlProvider);
            }
			Logger.LogDebug("<<< Exiting Rollback()");
        }

        /// <summary>The UnInstall method uninstalls the component.</summary>
        public override void UnInstall()
        {
			Logger.LogDebug(">>> Entering UnInstall()");
            this.DeleteProvider();
			Logger.LogDebug("<<< Exiting UnInstall()");
        }

        private void DeleteProvider()
        {
			Logger.LogDebug(">>> Entering DeleteProvider()");
            try
            {
                ExtensionUrlProviderInfo tempUrlProvider = ExtensionUrlProviderController.GetProviders(Null.NullInteger).Where(p => p.ProviderName == this.extensionUrlProvider.ProviderName && p.ProviderType == this.extensionUrlProvider.ProviderType).FirstOrDefault();
				Logger.LogDebug("DeleteProvider(): tempUrlProvider → {TempUrlProvider}", tempUrlProvider);
                if (tempUrlProvider != null)
                {
                    ExtensionUrlProviderController.DeleteProvider(tempUrlProvider);

                    this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.URLPROVIDER_UnRegistered, tempUrlProvider.ProviderName));
                }
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
			Logger.LogDebug("<<< Exiting DeleteProvider()");
        }
    }
}
