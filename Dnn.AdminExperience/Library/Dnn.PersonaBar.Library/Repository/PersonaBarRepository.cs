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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string PersonaBarMenuCacheKey = "PersonaBarMenu";
        private static readonly object ThreadLocker = new object();
        private readonly DataService dataService = new DataService();

        /// <inheritdoc />
        public PersonaBarMenu GetMenu()
        {
			Logger.LogDebug(">>> Entering GetMenu()");
            var menu = DataCache.GetCache<PersonaBarMenu>(PersonaBarMenuCacheKey);
            if (menu == null)
			Logger.LogDebug("GetMenu(): menu → {Menu}", menu);
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

			Logger.LogDebug("<<< Exiting GetMenu()");
            return menu;
        }

        /// <inheritdoc />
        public MenuItem GetMenuItem(string identifier)
        {
			Logger.LogDebug(">>> Entering GetMenuItem(identifier={Identifier})", identifier);
			Logger.LogDebug("<<< Exiting GetMenuItem(identifier={Identifier})", identifier);
            return this.GetMenu().AllItems.ToList().FirstOrDefault(m => m.Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc />
        public MenuItem GetMenuItem(int menuId)
        {
			Logger.LogDebug("<<< Exiting GetMenuItem(menuId={MenuId})", menuId);
            return this.GetMenu().AllItems.ToList().FirstOrDefault(m => m.MenuId == menuId);
        }

        /// <inheritdoc />
        public void SaveMenuItem(MenuItem item)
        {
			Logger.LogDebug(">>> Entering SaveMenuItem(item={Item})", item);
            var user = UserController.Instance.GetCurrentUserInfo();

			Logger.LogDebug("SaveMenuItem(item={Item}): user → {User}", item, user);
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
			Logger.LogDebug("<<< Exiting SaveMenuItem(item={Item})", item);
        }

        /// <inheritdoc />
        public void DeleteMenuItem(string identifier)
        {
			Logger.LogDebug(">>> Entering DeleteMenuItem(identifier={Identifier})", identifier);
            this.dataService.DeletePersonaBarMenuByIdentifier(identifier);

            ClearCache();
			Logger.LogDebug("<<< Exiting DeleteMenuItem(identifier={Identifier})", identifier);
        }

        /// <inheritdoc />
        public string GetMenuDefaultPermissions(int menuId)
        {
			Logger.LogDebug(">>> Entering GetMenuDefaultPermissions(menuId={MenuId})", menuId);
			Logger.LogDebug("<<< Exiting GetMenuDefaultPermissions(menuId={MenuId})", menuId);
            return this.dataService.GetPersonaBarMenuDefaultPermissions(menuId);
        }

        /// <inheritdoc />
        public void SaveMenuDefaultPermissions(MenuItem menuItem, string roleNames)
        {
			Logger.LogDebug(">>> Entering SaveMenuDefaultPermissions(menuItem={MenuItem},roleNames={RoleNames})", menuItem, roleNames);
            this.dataService.SavePersonaBarMenuDefaultPermissions(menuItem.MenuId, roleNames);
			Logger.LogDebug("<<< Exiting SaveMenuDefaultPermissions(menuItem={MenuItem},roleNames={RoleNames})", menuItem, roleNames);
        }

        /// <inheritdoc />
        public void UpdateMenuController(string identifier, string controller)
        {
			Logger.LogDebug(">>> Entering UpdateMenuController(identifier={Identifier},controller={Controller})", identifier, controller);
            var user = UserController.Instance.GetCurrentUserInfo();
            this.dataService.UpdateMenuController(identifier, controller, user.UserID);
            ClearCache();
			Logger.LogDebug("<<< Exiting UpdateMenuController(identifier={Identifier},controller={Controller})", identifier, controller);
        }

        /// <inheritdoc />
        protected override Func<IPersonaBarRepository> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new PersonaBarRepository();
        }

        private static void InjectMenuItems(MenuItem parent, IList<MenuItem> menuItems)
        {
            foreach (var menuItem in menuItems.Where(m => m.ParentId == parent.MenuId))
            {
                parent.Children.Add(menuItem);
                InjectMenuItems(menuItem, menuItems);
            }
        }

        private static void ClearCache()
        {
			Logger.LogDebug(">>> Entering ClearCache()");
            DataCache.RemoveCache(PersonaBarMenuCacheKey);
			Logger.LogDebug("<<< Exiting ClearCache()");
        }
    }
}
