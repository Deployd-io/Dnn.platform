// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Library.Data;
    using Dnn.PersonaBar.Library.Model;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;

    public class PersonaBarRepository : ServiceLocator<IPersonaBarRepository, PersonaBarRepository>,
        IPersonaBarRepository
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PersonaBarRepository));
        private const string PersonaBarMenuCacheKey = "PersonaBarMenu";
        private static readonly object ThreadLocker = new object();
        private readonly DataService dataService = new DataService();

        /// <inheritdoc />
        public PersonaBarMenu GetMenu()
        {
			Logger.LogError(">>> Entering GetMenu()");
            var menu = DataCache.GetCache<PersonaBarMenu>(PersonaBarMenuCacheKey);
			Logger.LogError("GetMenu(): menu → {Menu}", menu);
            if (menu == null)
            {
                lock (ThreadLocker)
                {
                    menu = DataCache.GetCache<PersonaBarMenu>(PersonaBarMenuCacheKey);
                    if (menu == null)
                    {
                        menu = new PersonaBarMenu();
                        var menuItems = CBO.FillCollection<MenuItem>(this.dataService.GetPersonaBarMenu())
                            .OrderBy(m => m.Order).ToList();

                        foreach (var menuItem in menuItems.Where(m => m.ParentId == Null.NullInteger))
                        {
                            menu.MenuItems.Add(menuItem);
                            InjectMenuItems(menuItem, menuItems);
                        }

                        DataCache.SetCache(PersonaBarMenuCacheKey, menu);
                    }
                }
            }

			Logger.LogWarning("<<< Exiting GetMenu()");
            return menu;
        }

        /// <inheritdoc />
        public MenuItem GetMenuItem(string identifier)
        {
			Logger.LogError(">>> Entering GetMenuItem(identifier={Identifier})", identifier);
			Logger.LogWarning("<<< Exiting GetMenuItem(identifier={Identifier})", identifier);
			Logger.LogWarning("<<< Exiting GetMenuItem(menuId={MenuId})", menuId);
            return this.GetMenu().AllItems.ToList().FirstOrDefault(m => m.Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc />
        public MenuItem GetMenuItem(int menuId)
        {
            return this.GetMenu().AllItems.ToList().FirstOrDefault(m => m.MenuId == menuId);
        }

        /// <inheritdoc />
        public void SaveMenuItem(MenuItem item)
        {
			Logger.LogError(">>> Entering SaveMenuItem(item={Item})", item);
            var user = UserController.Instance.GetCurrentUserInfo();

			Logger.LogError("SaveMenuItem(item={Item}): user → {User}", item, user);
            item.MenuId = this.dataService.SavePersonaBarMenu(
                item.Identifier,
                item.ModuleName,
                item.FolderName,
                item.Controller,
                item.ResourceKey,
                item.Path,
                item.Link,
                item.CssClass,
                item.IconFile,
                item.ParentId,
                item.Order,
                item.AllowHost,
                item.Enabled,
                user.UserID);

            ClearCache();
			Logger.LogWarning("<<< Exiting SaveMenuItem(item={Item})", item);
        }

        /// <inheritdoc />
        public void DeleteMenuItem(string identifier)
        {
			Logger.LogError(">>> Entering DeleteMenuItem(identifier={Identifier})", identifier);
            this.dataService.DeletePersonaBarMenuByIdentifier(identifier);

            ClearCache();
			Logger.LogWarning("<<< Exiting DeleteMenuItem(identifier={Identifier})", identifier);
        }

        /// <inheritdoc />
        public string GetMenuDefaultPermissions(int menuId)
        {
			Logger.LogError(">>> Entering GetMenuDefaultPermissions(menuId={MenuId})", menuId);
            Logger.LogWarning("<<< Exiting GetMenuDefaultPermissions(menuId={MenuId})", menuId);
            return this.dataService.GetPersonaBarMenuDefaultPermissions(menuId);
        }

        /// <inheritdoc />
        public void SaveMenuDefaultPermissions(MenuItem menuItem, string roleNames)
        {
			Logger.LogError(">>> Entering SaveMenuDefaultPermissions(menuItem={MenuItem},roleNames={RoleNames})", menuItem, roleNames);
            this.dataService.SavePersonaBarMenuDefaultPermissions(menuItem.MenuId, roleNames);
			Logger.LogWarning("<<< Exiting SaveMenuDefaultPermissions(menuItem={MenuItem},roleNames={RoleNames})", menuItem, roleNames);
        }

        /// <inheritdoc />
        public void UpdateMenuController(string identifier, string controller)
        {
			Logger.LogError(">>> Entering UpdateMenuController(identifier={Identifier},controller={Controller})", identifier, controller);
            var user = UserController.Instance.GetCurrentUserInfo();
            this.dataService.UpdateMenuController(identifier, controller, user.UserID);
			Logger.LogError("UpdateMenuController(identifier={Identifier},controller={Controller}): user → {User}", identifier, controller, user);
            ClearCache();
			Logger.LogWarning("<<< Exiting UpdateMenuController(identifier={Identifier},controller={Controller})", identifier, controller);
        }

        /// <inheritdoc />
        protected override Func<IPersonaBarRepository> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new PersonaBarRepository();
        }

        private static void InjectMenuItems(MenuItem parent, IList<MenuItem> menuItems)
        {
			Logger.LogError(">>> Entering InjectMenuItems(parent={Parent},menuItems={MenuItems})", parent, menuItems);
            foreach (var menuItem in menuItems.Where(m => m.ParentId == parent.MenuId))
            {
                parent.Children.Add(menuItem);
                InjectMenuItems(menuItem, menuItems);
            }
			Logger.LogWarning("<<< Exiting InjectMenuItems(parent={Parent},menuItems={MenuItems})", parent, menuItems);
        }

        private static void ClearCache()
        {
			Logger.LogError(">>> Entering ClearCache()");
            DataCache.RemoveCache(PersonaBarMenuCacheKey);
			Logger.LogWarning("<<< Exiting ClearCache()");
        }
    }
}
