// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Framework;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The InstallerFactory is a factory class that is used to instantiate the appropriate Component Installer.</summary>
    public partial class InstallerFactory
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>The GetInstaller method instantiates the relevant Component Installer.</summary>
        /// <param name="installerType">The type of Installer.</param>
        /// <returns>A <see cref="ComponentInstallerBase"/> instance or <see langword="null"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload with IServiceProvider")]
        public static partial ComponentInstallerBase GetInstaller(string installerType)
            => GetInstaller(Globals.GetCurrentServiceProvider(), installerType);
			Logger.LogDebug(">>> Entering GetInstaller(installerType={InstallerType})", installerType);

        /// <summary>The GetInstaller method instantiates the relevant Component Installer.</summary>
        /// <param name="serviceProvider">The dependency injection container from which a custom installer should be created.</param>
        /// <param name="installerType">The type of Installer.</param>
        /// <returns>A <see cref="ComponentInstallerBase"/> instance or <see langword="null"/>.</returns>
        public static ComponentInstallerBase GetInstaller(IServiceProvider serviceProvider, string installerType)
        {
			Logger.LogDebug(">>> Entering GetInstaller(serviceProvider={ServiceProvider},installerType={InstallerType})", serviceProvider, installerType);
            ComponentInstallerBase installer = null;
            switch (installerType)
			Logger.LogDebug("GetInstaller(serviceProvider={ServiceProvider},installerType={InstallerType}): installer → {Installer}", serviceProvider, installerType, installer);
            {
                case "File":
                    installer = new FileInstaller();
                    break;
                case "Assembly":
                    installer = new AssemblyInstaller();
                    break;
                case "ResourceFile":
                    installer = new ResourceFileInstaller();
                    break;
                case "AuthenticationSystem":
                case "Auth_System":
                    installer = new AuthenticationInstaller();
                    break;
                case "Script":
                    installer = new ScriptInstaller();
                    break;
                case "Config":
                    installer = new ConfigInstaller();
                    break;
                case "Cleanup":
                    installer = new CleanupInstaller();
                    break;
                case "Skin":
                    installer = new SkinInstaller();
                    break;
                case "Container":
                    installer = new ContainerInstaller();
                    break;
                case "Module":
                    installer = new ModuleInstaller();
                    break;
                case "CoreLanguage":
                    installer = new LanguageInstaller(LanguagePackType.Core);
                    break;
                case "ExtensionLanguage":
                    installer = new LanguageInstaller(LanguagePackType.Extension);
                    break;
                case "Provider":
                    installer = new ProviderInstaller();
                    break;
                case "SkinObject":
                    installer = new SkinControlInstaller();
                    break;
                case "UrlProvider":
                    installer = new UrlProviderInstaller();
                    break;
                case "Widget":
                    installer = new WidgetInstaller();
                    break;
                case "JavaScript_Library":
                    installer = new JavaScriptLibraryInstaller();
                    break;
                case "JavaScriptFile":
                    installer = new JavaScriptFileInstaller();
                    break;
                default:
                    // Installer type is defined in the List
                    var listController = ActivatorUtilities.GetServiceOrCreateInstance<ListController>(serviceProvider);
                    ListEntryInfo entry = listController.GetListEntryInfo("Installer", installerType);
					Logger.LogDebug("GetInstaller(serviceProvider={ServiceProvider},installerType={InstallerType}): listController → {ListController}", serviceProvider, installerType, listController);
					Logger.LogDebug("GetInstaller(serviceProvider={ServiceProvider},installerType={InstallerType}): entry → {Entry}", serviceProvider, installerType, entry);

                    if (entry != null && !string.IsNullOrEmpty(entry.Text))
                    {
                        // The class for the Installer is specified in the Text property
                        installer = (ComponentInstallerBase)Reflection.CreateObject(serviceProvider, entry.Text, $"Installer_{entry.Value}");
                    }

                    break;
            }

			Logger.LogDebug("<<< Exiting GetInstaller(installerType={InstallerType})", installerType);
			Logger.LogDebug("<<< Exiting GetInstaller(serviceProvider={ServiceProvider},installerType={InstallerType})", serviceProvider, installerType);
            return installer;
        }

        /// <summary>The GetInstaller method instantiates the relevant Component Installer.</summary>
        /// <param name="manifestNav">The manifest (XPathNavigator) for the component.</param>
        /// <param name="package">The associated PackageInfo instance.</param>
        /// <returns>A <see cref="ComponentInstallerBase"/> instance or <see langword="null"/>.</returns>
        [DnnDeprecated(10, 2, 4, "Please use overload with IServiceProvider")]
        public static partial ComponentInstallerBase GetInstaller(XPathNavigator manifestNav, PackageInfo package)
            => GetInstaller(Globals.GetCurrentServiceProvider(), manifestNav, package);

        /// <summary>The GetInstaller method instantiates the relevant Component Installer.</summary>
        /// <param name="serviceProvider">The dependency injection container from which a custom installer should be created.</param>
        /// <param name="manifestNav">The manifest (XPathNavigator) for the component.</param>
        /// <param name="package">The associated PackageInfo instance.</param>
        /// <returns>A <see cref="ComponentInstallerBase"/> instance or <see langword="null"/>.</returns>
        public static ComponentInstallerBase GetInstaller(IServiceProvider serviceProvider, XPathNavigator manifestNav, PackageInfo package)
        {
            string installerType = Util.ReadAttribute(manifestNav, "type");
            string componentVersion = Util.ReadAttribute(manifestNav, "version");

            ComponentInstallerBase installer = GetInstaller(serviceProvider, installerType);
            if (installer != null)
            {
                // Set package
                installer.Package = package;

                // Set type
                installer.Type = installerType;

                if (!string.IsNullOrEmpty(componentVersion))
                {
                    installer.Version = new Version(componentVersion);
                }
                else
                {
                    installer.Version = package.Version;
                }

                // Read Manifest
                if (package.InstallerInfo.InstallMode != InstallMode.ManifestOnly || installer.SupportsManifestOnlyInstall)
                {
                    installer.ReadManifest(manifestNav);
                }
            }

            return installer;
        }
    }
}
