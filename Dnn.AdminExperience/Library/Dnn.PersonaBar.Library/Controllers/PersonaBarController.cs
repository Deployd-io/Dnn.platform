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
		Logger.LogDebug(">>> Entering PersonaBarController(serviceScopeFactory={ServiceScopeFactory},personaBarRepository={PersonaBarRepository},hostSettings={HostSettings},portalController={PortalController})", serviceScopeFactory, personaBarRepository, hostSettings, portalController);
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PersonaBarController));

		Logger.LogDebug("PersonaBarController(serviceScopeFactory={ServiceScopeFactory},personaBarRepository={PersonaBarRepository},hostSettings={HostSettings},portalController={PortalController}): Logger → {Logger}", serviceScopeFactory, personaBarRepository, hostSettings, portalController, Logger);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        private readonly IServiceScopeFactory serviceScopeFactory = serviceScopeFactory ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServiceScopeFactory>();
        private readonly IPersonaBarRepository personaBarRepository = personaBarRepository ?? PersonaBarRepository.Instance;
		Logger.LogDebug("PersonaBarController(serviceScopeFactory={ServiceScopeFactory},personaBarRepository={PersonaBarRepository},hostSettings={HostSettings},portalController={PortalController}): serviceScopeFactory → {ServiceScopeFactory1}", serviceScopeFactory, personaBarRepository, hostSettings, portalController, serviceScopeFactory);
		Logger.LogDebug("PersonaBarController(serviceScopeFactory={ServiceScopeFactory},personaBarRepository={PersonaBarRepository},hostSettings={HostSettings},portalController={PortalController}): personaBarRepository → {PersonaBarRepository1}", serviceScopeFactory, personaBarRepository, hostSettings, portalController, personaBarRepository);
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
		Logger.LogDebug("PersonaBarController(serviceScopeFactory={ServiceScopeFactory},personaBarRepository={PersonaBarRepository},hostSettings={HostSettings},portalController={PortalController}): hostSettings → {HostSettings1}", serviceScopeFactory, personaBarRepository, hostSettings, portalController, hostSettings);
		Logger.LogDebug("PersonaBarController(serviceScopeFactory={ServiceScopeFactory},personaBarRepository={PersonaBarRepository},hostSettings={HostSettings},portalController={PortalController}): portalController → {PortalController1}", serviceScopeFactory, personaBarRepository, hostSettings, portalController, portalController);

        /// <summary>Initializes a new instance of the <see cref="PersonaBarController"/> class.</summary>
		Logger.LogDebug("<<< Exiting PersonaBarController(serviceScopeFactory={ServiceScopeFactory},personaBarRepository={PersonaBarRepository},hostSettings={HostSettings},portalController={PortalController})", serviceScopeFactory, personaBarRepository, hostSettings, portalController);
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceScopeFactory. Scheduled removal in v12.0.0.")]
        public PersonaBarController()
            : this(null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PersonaBarController"/> class.</summary>
        /// <param name="serviceScopeFactory">The service scope factory.</param>
        /// <param name="personaBarRepository">The Persona Bar repository.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPersonaBarContainer. Scheduled removal in v12.0.0.")]
        public PersonaBarController(IServiceScopeFactory serviceScopeFactory, IPersonaBarRepository personaBarRepository)
            : this(serviceScopeFactory, personaBarRepository, null, null)
        {
		Logger.LogInformation("PersonaBarController(serviceScopeFactory,personaBarRepository,hostSettings,portalController)={ServiceScopeFactory},{PersonaBarRepository},{HostSettings},{PortalController}: find query executed in {Arg4} ms", serviceScopeFactory, personaBarRepository, hostSettings, portalController, __logStart.ElapsedMilliseconds);
		Logger.LogError("Exception in PersonaBarController(serviceScopeFactory={ServiceScopeFactory},personaBarRepository={PersonaBarRepository},hostSettings={HostSettings},portalController={PortalController}): {Arg4}", serviceScopeFactory, personaBarRepository, hostSettings, portalController, ex.Message);
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
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                using var scope = this.serviceScopeFactory.CreateScope();
                var personaBarContainer = scope.ServiceProvider.GetRequiredService<IPersonaBarContainer>();
                var personaBarMenu = this.personaBarRepository.GetMenu();
                var rootItems = personaBarMenu.MenuItems.Where(m => personaBarContainer.RootItems.Contains(m.Identifier)).ToList();

                var filteredMenu = new PersonaBarMenu();
                this.GetPersonaBarMenuWithPermissionCheck(portalSettings, user, filteredMenu.MenuItems, rootItems);

				Logger.LogInformation("PersonaBarController(serviceScopeFactory,personaBarRepository,hostSettings,portalController)={ServiceScopeFactory},{PersonaBarRepository},{HostSettings},{PortalController}: find query executed in {Arg4} ms", serviceScopeFactory, personaBarRepository, hostSettings, portalController, __logStart.ElapsedMilliseconds);
                personaBarContainer.FilterMenu(filteredMenu);
                return filteredMenu;
            }
            catch (Exception e)
            {
				Logger.LogError("Exception in PersonaBarController(serviceScopeFactory={ServiceScopeFactory},personaBarRepository={PersonaBarRepository},hostSettings={HostSettings},portalController={PortalController}): {Arg4}", serviceScopeFactory, personaBarRepository, hostSettings, portalController, ex.Message);
                DotNetNuke.Services.Exceptions.Exceptions.LogException(e);
                return new PersonaBarMenu();
            }
        }

        /// <inheritdoc />
        public bool IsVisible(PortalSettings portalSettings, UserInfo user, MenuItem menuItem)
        {
            var visible = menuItem.Enabled
                   && !(user.IsSuperUser && !menuItem.AllowHost)
                   && MenuPermissionController.CanView(this.hostSettings, this.portalController, portalSettings.PortalId, menuItem);

            if (visible)
            {
                try
                {
                    using var scope = this.serviceScopeFactory.CreateScope();
                    var menuController = GetMenuItemController(scope, menuItem);
                    visible = menuController == null || menuController.Visible(menuItem);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    visible = false;
                }
            }

            return visible;
        }

        /// <inheritdoc />
        protected override Func<IPersonaBarController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<IPersonaBarController>;
        }

        private static void AddPermissions(IHostSettings hostSettings, IPortalController portalController, MenuItem menuItem, IDictionary<string, object> settings)
        {
            var portalSettings = PortalSettings.Current;
            if (!settings.ContainsKey("permissions") && portalSettings != null)
            {
                var menuPermissions = MenuPermissionController.GetPermissions(hostSettings, menuItem.MenuId)
                    .Where(p => p.PermissionKey != "VIEW");
                var portalId = portalSettings.PortalId;
                var permissions = new Dictionary<string, bool>();
                foreach (var permission in menuPermissions)
                {
                    var key = permission.PermissionKey;
                    var hasPermission = MenuPermissionController.HasMenuPermission(hostSettings, portalController, portalId, menuItem, key);
                    permissions.Add(key, hasPermission);
                }

                settings.Add("permissions", permissions);
            }
        }

        private static IMenuItemController GetMenuItemController(IServiceScope scope, MenuItem menuItem)
        {
			Logger.LogDebug(">>> Entering GetMenuItemController(scope={Scope},menuItem={MenuItem})", scope, menuItem);
            var identifier = menuItem.Identifier;
            var controller = menuItem.Controller;

            if (string.IsNullOrEmpty(controller))
            {
                return null;
            }

            try
            {
                var cacheKey = $"PersonaBarMenuController_{identifier}";
                var controllerType = Reflection.CreateType(controller, cacheKey, useCache: true);
				Logger.LogDebug("GetMenuItemController(scope={Scope},menuItem={MenuItem}): cacheKey → {CacheKey}", scope, menuItem, cacheKey);
				Logger.LogDebug("GetMenuItemController(scope={Scope},menuItem={MenuItem}): controllerType → {ControllerType}", scope, menuItem, controllerType);
                return ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, controllerType) as IMenuItemController;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting GetMenuItemController(scope={Scope},menuItem={MenuItem})", scope, menuItem);
                return null;
            }
        }

        private bool GetPersonaBarMenuWithPermissionCheck(PortalSettings portalSettings, UserInfo user, IList<MenuItem> filterItems, IList<MenuItem> menuItems)
        {
            var menuFiltered = false;
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
                    if (!filtered || cloneItem.Children.Count > 0)
                    {
                        filterItems.Add(cloneItem);
                    }
                }
                catch (Exception e)
                {
					Logger.LogError("Exception in PersonaBarController(serviceScopeFactory={ServiceScopeFactory},personaBarRepository={PersonaBarRepository},hostSettings={HostSettings},portalController={PortalController}): {Arg4}", serviceScopeFactory, personaBarRepository, hostSettings, portalController, ex.Message);
                    // Ignore the failure and still load personaBar
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(e);
                }
            }

            return menuFiltered;
        }

        private void UpdateParameters(MenuItem menuItem)
        {
			Logger.LogDebug(">>> Entering UpdateParameters(menuItem={MenuItem})", menuItem);
            using var scope = this.serviceScopeFactory.CreateScope();
            var menuController = GetMenuItemController(scope, menuItem);
			Logger.LogDebug("UpdateParameters(menuItem={MenuItem}): scope → {Scope}", menuItem, scope);
			Logger.LogDebug("UpdateParameters(menuItem={MenuItem}): menuController → {MenuController}", menuItem, menuController);
            try
            {
                menuController?.UpdateParameters(menuItem);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
			Logger.LogDebug("<<< Exiting UpdateParameters(menuItem={MenuItem})", menuItem);
        }

        private string GetMenuSettings(MenuItem menuItem)
        {
			Logger.LogDebug(">>> Entering GetMenuSettings(menuItem={MenuItem})", menuItem);
            IDictionary<string, object> settings;
            try
            {
                using var scope = this.serviceScopeFactory.CreateScope();
                var menuController = GetMenuItemController(scope, menuItem);
				Logger.LogDebug("GetMenuSettings(menuItem={MenuItem}): scope → {Scope}", menuItem, scope);
				Logger.LogDebug("GetMenuSettings(menuItem={MenuItem}): menuController → {MenuController}", menuItem, menuController);
                settings = menuController?.GetSettings(menuItem) ?? new Dictionary<string, object>();

				Logger.LogDebug("GetMenuSettings(menuItem={MenuItem}): settings → {Settings}", menuItem, settings);
                AddPermissions(this.hostSettings, this.portalController, menuItem, settings);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                settings = new Dictionary<string, object>();
            }

			Logger.LogDebug("<<< Exiting GetMenuSettings(menuItem={MenuItem})", menuItem);
            return JsonConvert.SerializeObject(settings);
        }
    }
}
