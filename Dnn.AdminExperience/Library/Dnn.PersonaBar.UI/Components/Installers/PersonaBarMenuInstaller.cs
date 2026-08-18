// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Components.Installers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml.XPath;

    using Dnn.PersonaBar.Library.Model;
    using Dnn.PersonaBar.Library.Permissions;
    using Dnn.PersonaBar.Library.Repository;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Installers;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Installer for persona bar menus.</summary>
    public class PersonaBarMenuInstaller(IHostSettings hostSettings, IPortalController portalController) : ComponentInstallerBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PersonaBarMenuInstaller));
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        private readonly List<MenuItem> menuItems = [];
        private readonly List<PersonaBarExtension> extensions = [];
        private readonly List<PermissionDefinition> permissionDefinitions = [];
        private readonly Dictionary<string, string> extensionMenus = new Dictionary<string, string>();
        private readonly Dictionary<string, string> menuRoles = new Dictionary<string, string>();
        private readonly Dictionary<string, string> parentMaps = new Dictionary<string, string>();

        /// <summary>Initializes a new instance of the <see cref="PersonaBarMenuInstaller"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public PersonaBarMenuInstaller()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering PersonaBarMenuInstaller()");
        }

        /// <summary>Initializes a new instance of the <see cref="PersonaBarMenuInstaller"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public PersonaBarMenuInstaller(IHostSettings hostSettings)
            : this(hostSettings, null)
        {
        }

        /// <inheritdoc />
        public override void Commit()
        {
		Logger.LogDebug(">>> Entering Commit()");
		Logger.LogDebug("<<< Exiting Commit()");
        }

        /// <inheritdoc />
        public override void Install()
        {
			Logger.LogDebug(">>> Entering Install()");
            try
            {
                this.SaveMenuItems();

                this.SaveMenuExtensions();

                this.SavePermissionDefinitions();

                if (this.menuItems.Count != 0)
                {
                    foreach (var menuItem in this.menuItems)
                    {
                        this.SaveMenuPermissions(menuItem);
                    }
                }

                this.Completed = true;
				Logger.LogDebug("Install(): this.Completed → {Arg0}", this.Completed);
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
			Logger.LogDebug("<<< Exiting Install()");
        }

        /// <inheritdoc />
        public override void ReadManifest(XPathNavigator manifestNav)
        {
			Logger.LogDebug(">>> Entering ReadManifest(manifestNav={ManifestNav})", manifestNav);
            foreach (XPathNavigator navigator in manifestNav.Select("menu"))
            {
                this.ReadMenuItemsFromManifest(navigator);
            }

            foreach (XPathNavigator navigator in manifestNav.Select("extension"))
            {
                this.ReadExtensionsFromManifest(navigator);
            }

            foreach (XPathNavigator navigator in manifestNav.Select("permission"))
            {
                this.ReadPermissionsFromManifest(navigator);
            }
			Logger.LogDebug("<<< Exiting ReadManifest(manifestNav={ManifestNav})", manifestNav);
        }

        /// <inheritdoc />
        public override void Rollback()
        {
			Logger.LogDebug(">>> Entering Rollback()");
            this.DeleteMenus();
			Logger.LogDebug("<<< Exiting Rollback()");
        }

        /// <inheritdoc />
        public override void UnInstall()
        {
			Logger.LogDebug(">>> Entering UnInstall()");
            this.DeleteMenus();
			Logger.LogDebug("<<< Exiting UnInstall()");
        }

        private static void SaveMenuPermission(IHostSettings hostSettings, IPortalController portalController, MenuItem menuItem, string roleName)
        {
            var portals = PortalController.Instance.GetPortals();
            foreach (IPortalInfo portal in portals)
            {
                var portalId = portal.PortalId;

                // when default permission already initialized, then package need to save default permission immediately.
                if (MenuPermissionController.PermissionAlreadyInitialized(portalId))
                {
                    MenuPermissionController.SaveMenuDefaultPermissions(hostSettings, portalController, portalId, menuItem, roleName);
                }
            }
			Logger.LogDebug("<<< Exiting SaveMenuPermission(hostSettings={HostSettings},portalController={PortalController},menuItem={MenuItem},roleName={RoleName})", hostSettings, portalController, menuItem, roleName);
        }

        private void SaveMenuItems()
        {
			Logger.LogDebug(">>> Entering SaveMenuItems()");
            foreach (var menuItem in this.menuItems.Where(x => !string.IsNullOrEmpty(x.Identifier) && !string.IsNullOrEmpty(x.ModuleName)))
            {
                if (this.parentMaps.TryGetValue(menuItem.Identifier, out var parentId))
                {
                    var parentItem = PersonaBarRepository.Instance.GetMenuItem(parentId);
					Logger.LogDebug("SaveMenuItems(): parentItem → {ParentItem}", parentItem);
                    if (parentItem != null)
                    {
                        menuItem.ParentId = parentItem.MenuId;
                    }
                }

                PersonaBarRepository.Instance.SaveMenuItem(menuItem);
            }
			Logger.LogDebug("<<< Exiting SaveMenuItems()");
        }

        private void SaveMenuExtensions()
        {
			Logger.LogDebug(">>> Entering SaveMenuExtensions()");
            foreach (var extension in this.extensions)
            {
                var menuIdentifier = this.extensionMenus[extension.Identifier];
                var menu = PersonaBarRepository.Instance.GetMenuItem(menuIdentifier);
				Logger.LogDebug("SaveMenuExtensions(): menu → {Menu}", menu);
                if (menu != null)
                {
                    extension.MenuId = menu.MenuId;

                    PersonaBarExtensionRepository.Instance.SaveExtension(extension);
                }
            }
			Logger.LogDebug("<<< Exiting SaveMenuExtensions()");
        }

        private void SavePermissionDefinitions()
        {
			Logger.LogDebug(">>> Entering SavePermissionDefinitions()");
            if (this.menuItems.Count == 0)
            {
                return;
            }

            foreach (var definition in this.permissionDefinitions)
            {
                var identifier = definition.Identifier;
                var menu = this.menuItems.FirstOrDefault(m => string.IsNullOrEmpty(identifier) || m.Identifier == identifier);
				Logger.LogDebug("SavePermissionDefinitions(): menu → {Menu}", menu);
                if (menu?.MenuId <= 0)
                {
                    menu = PersonaBarRepository.Instance.GetMenuItem(identifier);
                }

                if (menu != null)
                {
                    MenuPermissionController.SavePersonaBarPermission(menu.MenuId, definition.Key, definition.Name);
                }
            }
			Logger.LogDebug("<<< Exiting SavePermissionDefinitions()");
        }

        private void ReadMenuItemsFromManifest(XPathNavigator menuNavigator)
        {
			Logger.LogDebug(">>> Entering ReadMenuItemsFromManifest(menuNavigator={MenuNavigator})", menuNavigator);
            var menuItem = new MenuItem()
            {
                Identifier = Util.ReadElement(menuNavigator, "identifier"),
                ModuleName = Util.ReadElement(menuNavigator, "moduleName"),
                FolderName = Util.ReadElement(menuNavigator, "folderName"),
                Controller = Util.ReadElement(menuNavigator, "controller"),
                ResourceKey = Util.ReadElement(menuNavigator, "resourceKey"),
                Path = Util.ReadElement(menuNavigator, "path"),
                Link = Util.ReadElement(menuNavigator, "link"),
                CssClass = Util.ReadElement(menuNavigator, "css"),
                IconFile = Util.ReadElement(menuNavigator, "icon"),
                ParentId = Null.NullInteger,
                Order = Convert.ToInt32(Util.ReadElement(menuNavigator, "order", "0"), CultureInfo.InvariantCulture),
                AllowHost = Util.ReadElement(menuNavigator, "allowHost", "true").Equals("true", StringComparison.OrdinalIgnoreCase),
                Enabled = true,
            };

            var parent = Util.ReadElement(menuNavigator, "parent", string.Empty);
			Logger.LogDebug("ReadMenuItemsFromManifest(menuNavigator={MenuNavigator}): parent → {Parent}", menuNavigator, parent);
            if (!string.IsNullOrEmpty(parent))
            {
                this.parentMaps.Add(menuItem.Identifier, parent);
            }

            var defaultPermissions = Util.ReadElement(menuNavigator, "defaultPermissions", string.Empty);
			Logger.LogDebug("ReadMenuItemsFromManifest(menuNavigator={MenuNavigator}): defaultPermissions → {DefaultPermissions}", menuNavigator, defaultPermissions);
            if (!string.IsNullOrEmpty(defaultPermissions))
            {
                this.menuRoles.Add(menuItem.Identifier, defaultPermissions);
            }

            this.menuItems.Add(menuItem);
			Logger.LogDebug("<<< Exiting ReadMenuItemsFromManifest(menuNavigator={MenuNavigator})", menuNavigator);
        }

        private void ReadExtensionsFromManifest(XPathNavigator menuNavigator)
        {
			Logger.LogDebug(">>> Entering ReadExtensionsFromManifest(menuNavigator={MenuNavigator})", menuNavigator);
            var extension = new PersonaBarExtension()
            {
                Identifier = Util.ReadElement(menuNavigator, "identifier"),
                FolderName = Util.ReadElement(menuNavigator, "folderName"),
                Controller = Util.ReadElement(menuNavigator, "controller"),
                Container = Util.ReadElement(menuNavigator, "container"),
                Path = Util.ReadElement(menuNavigator, "path"),
                Order = Convert.ToInt32(Util.ReadElement(menuNavigator, "order", "0"), CultureInfo.InvariantCulture),
                Enabled = true,
            };

            this.extensions.Add(extension);
            this.extensionMenus.Add(extension.Identifier, Util.ReadElement(menuNavigator, "menu"));
			Logger.LogDebug("<<< Exiting ReadExtensionsFromManifest(menuNavigator={MenuNavigator})", menuNavigator);
        }

        private void ReadPermissionsFromManifest(XPathNavigator menuNavigator)
        {
			Logger.LogDebug(">>> Entering ReadPermissionsFromManifest(menuNavigator={MenuNavigator})", menuNavigator);
            var permission = new PermissionDefinition
            {
                Identifier = Util.ReadElement(menuNavigator, "identifier"),
                Key = Util.ReadElement(menuNavigator, "key"),
                Name = Util.ReadElement(menuNavigator, "name"),
            };

            this.permissionDefinitions.Add(permission);
			Logger.LogDebug("<<< Exiting ReadPermissionsFromManifest(menuNavigator={MenuNavigator})", menuNavigator);
        }

        private void SaveMenuPermissions(MenuItem menuItem)
        {
            if (!this.menuRoles.TryGetValue(menuItem.Identifier, out var role))
            {
                return;
            }

            var defaultPermissions = role.Split(',');
            if (menuItem.MenuId <= 0)
            {
                menuItem = PersonaBarRepository.Instance.GetMenuItem(menuItem.Identifier);
				Logger.LogDebug("SaveMenuPermissions(menuItem={MenuItem}): menuItem → {MenuItem1}", menuItem, menuItem);
            }

            PersonaBarRepository.Instance.GetMenuDefaultPermissions(menuItem.MenuId);
            PersonaBarRepository.Instance.SaveMenuDefaultPermissions(menuItem, this.menuRoles[menuItem.Identifier]);

            foreach (var roleName in defaultPermissions)
            {
                if (!string.IsNullOrEmpty(roleName.Trim()))
                {
                    SaveMenuPermission(this.hostSettings, this.portalController, menuItem, roleName.Trim());
                }
            }
			Logger.LogDebug("<<< Exiting SaveMenuPermissions(menuItem={MenuItem})", menuItem);
        }

        private void DeleteMenus()
        {
			Logger.LogDebug(">>> Entering DeleteMenus()");
            try
            {
                foreach (var menuItem in this.menuItems)
                {
                    PersonaBarRepository.Instance.DeleteMenuItem(menuItem.Identifier);
                }
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
			Logger.LogDebug("<<< Exiting DeleteMenus()");
                throw;
            }
        }

        private struct PermissionDefinition
        {
            public string Identifier { get; set; }

            public string Key { get; set; }

            public string Name { get; set; }
        }
		Logger.LogDebug("<<< Exiting PersonaBarMenuInstaller()");
    }
}
