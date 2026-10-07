// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Dnn.PersonaBar.Library.Containers;
    using Dnn.PersonaBar.Library.Permissions;
    using Dnn.PersonaBar.Library.Repository;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;

    using MenuItem = Dnn.PersonaBar.Library.Model.MenuItem;
    using PersonaBarMenu = Dnn.PersonaBar.Library.Model.PersonaBarMenu;

    public class PersonaBarController(IServiceScopeFactory serviceScopeFactory, IPersonaBarRepository personaBarRepository, IHostSettings hostSettings, IPortalController portalController)
        : ServiceLocator<IPersonaBarController, PersonaBarController>, IPersonaBarController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PersonaBarController));

        private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServiceScopeFactory>();
        private readonly IPersonaBarRepository personaBarRepository = personaBarRepository ?? PersonaBarRepository.Instance;
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();

        /// <summary>Initializes a new instance of the <see cref="PersonaBarController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceScopeFactory. Scheduled removal in v12.0.0.")]
        public PersonaBarController()
            : this(null, null, null, null)
        {
		Logger.LogError(">>> Entering PersonaBarController()");
        Logger.LogWarning("<<< Exiting PersonaBarController()");
        }

        /// <summary>Initializes a new instance of the <see cref="PersonaBarController"/> class.</summary>
        /// <param name="serviceScopeFactory">The service scope factory.</param>
        /// <param name="personaBarRepository">The Persona Bar repository.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPersonaBarContainer. Scheduled removal in v12.0.0.")]
        public PersonaBarController(IServiceScopeFactory serviceScopeFactory, IPersonaBarRepository personaBarRepository)
            : this(serviceScopeFactory, personaBarRepository, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PersonaBarController"/> class.</summary>
        /// <param name="serviceScopeFactory">The service scope factory.</param>
        /// <param name="personaBarRepository">The Persona Bar repository.</param>
        /// <param name="hostSettings">The host settings.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public PersonaBarController(IServiceScopeFactory serviceScopeFactory, IPersonaBarRepository personaBarRepository, IHostSettings hostSettings)
            : this(serviceScopeFactory, personaBarRepository, hostSettings, null)
        {
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public PersonaBarMenu GetMenu(PortalSettings portalSettings, UserInfo user)
        {
			Logger.LogError(">>> Entering GetMenu(portalSettings={PortalSettings},user={User})", portalSettings, user);
            try
            {
                using var scope = this.serviceScopeFactory.CreateScope();
				Logger.LogError("GetMenu(portalSettings={PortalSettings},user={User}): scope → {Scope}", portalSettings, user, scope);
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
                var personaBarContainer = scope.ServiceProvider.GetRequiredService<IPersonaBarContainer>();
                var personaBarMenu = this.personaBarRepository.GetMenu();
                var rootItems = personaBarMenu.MenuItems.Where(m => personaBarContainer.RootItems.Contains(m.Identifier)).ToList();
				Logger.LogError("GetMenu(portalSettings={PortalSettings},user={User}): personaBarMenu → {PersonaBarMenu}", portalSettings, user, personaBarMenu);
				Logger.LogError("GetMenu(portalSettings={PortalSettings},user={User}): rootItems → {RootItems}", portalSettings, user, rootItems);
				Logger.LogError("GetMenu(portalSettings,user)={PortalSettings},{User}: find query executed in {Arg2} ms", portalSettings, user, __logStart.ElapsedMilliseconds);

                var filteredMenu = new PersonaBarMenu();
                this.GetPersonaBarMenuWithPermissionCheck(portalSettings, user, filteredMenu.MenuItems, rootItems);

				Logger.LogError("GetMenu(portalSettings={PortalSettings},user={User}): filteredMenu → {FilteredMenu}", portalSettings, user, filteredMenu);
                personaBarContainer.FilterMenu(filteredMenu);
                return filteredMenu;
            }
            catch (Exception e)
            {
				Logger.LogError("Exception in GetMenu(portalSettings={PortalSettings},user={User}): {Arg2}", portalSettings, user, e.Message);
                DotNetNuke.Services.Exceptions.Exceptions.LogException(e);
				Logger.LogWarning("<<< Exiting GetMenu(portalSettings={PortalSettings},user={User})", portalSettings, user);
                return new PersonaBarMenu();
            }
        }

        /// <inheritdoc />
        public bool IsVisible(PortalSettings portalSettings, UserInfo user, MenuItem menuItem)
        {
			Logger.LogError(">>> Entering IsVisible(portalSettings={PortalSettings},user={User},menuItem={MenuItem})", portalSettings, user, menuItem);
            var visible = menuItem.Enabled
                   && !(user.IsSuperUser && !menuItem.AllowHost)
                   && MenuPermissionController.CanView(this.hostSettings, this.portalController, portalSettings.PortalId, menuItem);

            if (visible)
            {
                try
                {
                    using var scope = this.serviceScopeFactory.CreateScope();
					Logger.LogError("IsVisible(portalSettings={PortalSettings},user={User},menuItem={MenuItem}): scope → {Scope}", portalSettings, user, menuItem, scope);
                    var menuController = GetMenuItemController(scope, menuItem);
					Logger.LogError("IsVisible(portalSettings={PortalSettings},user={User},menuItem={MenuItem}): menuController → {MenuController}", portalSettings, user, menuItem, menuController);
                    visible = menuController == null || menuController.Visible(menuItem);
					Logger.LogError("IsVisible(portalSettings={PortalSettings},user={User},menuItem={MenuItem}): visible → {Visible}", portalSettings, user, menuItem, visible);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    visible = false;
                }
            }

			Logger.LogWarning("<<< Exiting IsVisible(portalSettings={PortalSettings},user={User},menuItem={MenuItem})", portalSettings, user, menuItem);
            return visible;
        }

        /// <inheritdoc />
        protected override Func<IPersonaBarController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<IPersonaBarController>;
        }

        private static void AddPermissions(IHostSettings hostSettings, IPortalController portalController, MenuItem menuItem, IDictionary<string, object> settings)
        {
			Logger.LogError(">>> Entering AddPermissions(hostSettings={HostSettings},portalController={PortalController},menuItem={MenuItem},settings={Settings})", hostSettings, portalController, menuItem, settings);
            var portalSettings = PortalSettings.Current;
            if (!settings.ContainsKey("permissions") && portalSettings != null)
            {
                var menuPermissions = MenuPermissionController.GetPermissions(hostSettings, menuItem.MenuId)
                    .Where(p => p.PermissionKey != "VIEW");
                var portalId = portalSettings.PortalId;
                var permissions = new Dictionary<string, bool>();
				Logger.LogError("AddPermissions(hostSettings={HostSettings},portalController={PortalController},menuItem={MenuItem},settings={Settings}): permissions → {Permissions}", hostSettings, portalController, menuItem, settings, permissions);
                foreach (var permission in menuPermissions)
                {
                    var key = permission.PermissionKey;
					Logger.LogError("AddPermissions(hostSettings={HostSettings},portalController={PortalController},menuItem={MenuItem},settings={Settings}): key → {Key}", hostSettings, portalController, menuItem, settings, key);
                    var hasPermission = MenuPermissionController.HasMenuPermission(hostSettings, portalController, portalId, menuItem, key);
					Logger.LogError("AddPermissions(hostSettings={HostSettings},portalController={PortalController},menuItem={MenuItem},settings={Settings}): hasPermission → {HasPermission}", hostSettings, portalController, menuItem, settings, hasPermission);
                    permissions.Add(key, hasPermission);
                }

                settings.Add("permissions", permissions);
            }
			Logger.LogWarning("<<< Exiting AddPermissions(hostSettings={HostSettings},portalController={PortalController},menuItem={MenuItem},settings={Settings})", hostSettings, portalController, menuItem, settings);
        }

        private static IMenuItemController GetMenuItemController(IServiceScope scope, MenuItem menuItem)
        {
			Logger.LogError(">>> Entering GetMenuItemController(scope={Scope},menuItem={MenuItem})", scope, menuItem);
            var identifier = menuItem.Identifier;
            var controller = menuItem.Controller;

            if (string.IsNullOrEmpty(controller))
            {
                return null;
            }

            try
            {
                var cacheKey = $"PersonaBarMenuController_{identifier}";
				Logger.LogError("GetMenuItemController(scope={Scope},menuItem={MenuItem}): cacheKey → {CacheKey}", scope, menuItem, cacheKey);
                var controllerType = Reflection.CreateType(controller, cacheKey, useCache: true);
				Logger.LogError("GetMenuItemController(scope={Scope},menuItem={MenuItem}): controllerType → {ControllerType}", scope, menuItem, controllerType);
                return ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, controllerType) as IMenuItemController;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting GetMenuItemController(scope={Scope},menuItem={MenuItem})", scope, menuItem);
                return null;
            }
        }

        private bool GetPersonaBarMenuWithPermissionCheck(PortalSettings portalSettings, UserInfo user, IList<MenuItem> filterItems, IList<MenuItem> menuItems)
        {
			Logger.LogError(">>> Entering GetPersonaBarMenuWithPermissionCheck(portalSettings={PortalSettings},user={User},filterItems={FilterItems},menuItems={MenuItems})", portalSettings, user, filterItems, menuItems);
            var menuFiltered = false;
			Logger.LogError("GetPersonaBarMenuWithPermissionCheck(portalSettings={PortalSettings},user={User},filterItems={FilterItems},menuItems={MenuItems}): menuFiltered → {MenuFiltered}", portalSettings, user, filterItems, menuItems, menuFiltered);
            foreach (var menuItem in menuItems)
            {
                try
                {
                    if (!this.IsVisible(portalSettings, user, menuItem))
                    {
                        menuFiltered = true;
                        continue;
                    }

                    var cloneItem = new MenuItem()
                    {
                        MenuId = menuItem.MenuId,
                        Identifier = menuItem.Identifier,
                        ModuleName = menuItem.ModuleName,
                        FolderName = menuItem.FolderName,
                        Controller = menuItem.Controller,
                        ResourceKey = menuItem.ResourceKey,
                        Path = menuItem.Path,
                        Link = menuItem.Link,
                        CssClass = menuItem.CssClass,
                        IconFile = menuItem.IconFile,
                        AllowHost = menuItem.AllowHost,
                        Order = menuItem.Order,
                        ParentId = menuItem.ParentId,
                    };

                    this.UpdateParameters(cloneItem);
                    cloneItem.Settings = this.GetMenuSettings(menuItem);

                    var filtered = this.GetPersonaBarMenuWithPermissionCheck(portalSettings, user, cloneItem.Children, menuItem.Children);
					Logger.LogError("GetPersonaBarMenuWithPermissionCheck(portalSettings={PortalSettings},user={User},filterItems={FilterItems},menuItems={MenuItems}): filtered → {Filtered}", portalSettings, user, filterItems, menuItems, filtered);
                    if (!filtered || cloneItem.Children.Count > 0)
                    {
                        filterItems.Add(cloneItem);
                    }
                }
                catch (Exception e)
                {
					Logger.LogError("Exception in GetPersonaBarMenuWithPermissionCheck(portalSettings={PortalSettings},user={User},filterItems={FilterItems},menuItems={MenuItems}): {Arg4}", portalSettings, user, filterItems, menuItems, e.Message);
                    // Ignore the failure and still load personaBar
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(e);
                }
            }

            return menuFiltered;
        }

        private void UpdateParameters(MenuItem menuItem)
        {
			Logger.LogError(">>> Entering UpdateParameters(menuItem={MenuItem})", menuItem);
            using var scope = this.serviceScopeFactory.CreateScope();
            var menuController = GetMenuItemController(scope, menuItem);
			Logger.LogError("UpdateParameters(menuItem={MenuItem}): scope → {Scope}", menuItem, scope);
            try
            {
				Logger.LogError("UpdateParameters(menuItem={MenuItem}): menuController → {MenuController}", menuItem, menuController);
                menuController?.UpdateParameters(menuItem);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
			Logger.LogWarning("<<< Exiting UpdateParameters(menuItem={MenuItem})", menuItem);
        }

        private string GetMenuSettings(MenuItem menuItem)
        {
			Logger.LogError(">>> Entering GetMenuSettings(menuItem={MenuItem})", menuItem);
            IDictionary<string, object> settings;
            try
            {
                using var scope = this.serviceScopeFactory.CreateScope();
				Logger.LogError("GetMenuSettings(menuItem={MenuItem}): scope → {Scope}", menuItem, scope);
				Logger.LogDebug("GetMenuSettings(menuItem={MenuItem}): settings → {Settings}", menuItem, settings);
                var menuController = GetMenuItemController(scope, menuItem);
				Logger.LogError("GetMenuSettings(menuItem={MenuItem}): menuController → {MenuController}", menuItem, menuController);
                settings = menuController?.GetSettings(menuItem) ?? new Dictionary<string, object>();
				Logger.LogError("GetMenuSettings(menuItem={MenuItem}): settings → {Settings}", menuItem, settings);

                AddPermissions(this.hostSettings, this.portalController, menuItem, settings);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                settings = new Dictionary<string, object>();
            }

			Logger.LogWarning("<<< Exiting GetMenuSettings(menuItem={MenuItem})", menuItem);
            return JsonConvert.SerializeObject(settings);
        }
    }
}
