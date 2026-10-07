// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Components.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Model;
    using Dnn.PersonaBar.Library.Repository;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The default <see cref="IAdminMenuController"/> implementation.</summary>
    /// <param name="appStatus">The application status.</param>
    /// <param name="hostSettings">The host settings.</param>
    public class AdminMenuController(IApplicationStatusInfo appStatus, IHostSettings hostSettings)
        : ServiceLocator<IAdminMenuController, AdminMenuController>, IAdminMenuController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AdminMenuController));
        private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private Dictionary<string, IList<string>> knownPages;

        /// <summary>Initializes a new instance of the <see cref="AdminMenuController"/> class.</summary>
        [Obsolete("Deprecated in IApplicationStatusInfo 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public AdminMenuController()
            : this(null)
        {
		Logger.LogError(">>> Entering AdminMenuController()");
        Logger.LogWarning("<<< Exiting AdminMenuController()");
        }

        /// <summary>Initializes a new instance of the <see cref="AdminMenuController"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public AdminMenuController(IApplicationStatusInfo appStatus)
            : this(appStatus, null)
        {
        }

        /// <inheritdoc />
        public void CreateLinkMenu(TabInfo tab)
        {
			Logger.LogError(">>> Entering CreateLinkMenu(tab={Tab})", tab);
            if (!this.ValidateTab(tab))
            {
                return;
            }

            var portalId = tab.PortalID;
            var tabId = tab.TabID;

            var identifier = $"Link_{portalId}_{tabId}";
			Logger.LogError("CreateLinkMenu(tab={Tab}): identifier → {Identifier}", tab, identifier);
            if (PersonaBarRepository.Instance.GetMenuItem(identifier) != null)
            {
                return;
            }

            var menuItem = new MenuItem
            {
                Identifier = identifier,
                Path = $"LinkMenu?portalId={portalId}&tabId={tabId}",
                ModuleName = "LinkMenu",
                Controller = "Dnn.PersonaBar.UI.MenuControllers.LinkMenuController, Dnn.PersonaBar.UI",
                ResourceKey = tab.LocalizedTabName,
                ParentId = PersonaBarRepository.Instance.GetMenuItem("Manage").MenuId,
                Order = tab.IsSuperTab ? 300 : 200, // show host menus after admin menus
                AllowHost = true,
                Enabled = true,
            };

            PersonaBarRepository.Instance.SaveMenuItem(menuItem);

            if (!tab.IsSuperTab)
            {
                var portal = PortalController.Instance.GetPortal(portalId);
				Logger.LogError("CreateLinkMenu(tab={Tab}): portal → {Portal}", tab, portal);
                PersonaBarRepository.Instance.SaveMenuDefaultPermissions(menuItem, portal.AdministratorRoleName);
            }
			Logger.LogWarning("<<< Exiting CreateLinkMenu(tab={Tab})", tab);
        }

        /// <inheritdoc />
        public void DeleteLinkMenu(TabInfo tab)
        {
			Logger.LogError(">>> Entering DeleteLinkMenu(tab={Tab})", tab);
            var portalId = tab.PortalID;
            var tabId = tab.TabID;

            var identifier = $"Link_{portalId}_{tabId}";
			Logger.LogError("DeleteLinkMenu(tab={Tab}): identifier → {Identifier}", tab, identifier);

            PersonaBarRepository.Instance.DeleteMenuItem(identifier);
			Logger.LogWarning("<<< Exiting DeleteLinkMenu(tab={Tab})", tab);
        }

        /// <inheritdoc />
        protected override Func<IAdminMenuController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => Globals.DependencyProvider.GetRequiredService<IAdminMenuController>();
        }

        private bool ValidateTab(TabInfo tab)
        {
			Logger.LogError(">>> Entering ValidateTab(tab={Tab})", tab);
            if (tab.IsDeleted || tab.DisableLink || !tab.IsVisible)
            {
                return false;
            }

            var type = tab.IsSuperTab ? "host" : "admin";
			Logger.LogError("ValidateTab(tab={Tab}): type → {Type}", tab, type);
            var portalId = tab.PortalID;
            var tabName = tab.TabName;

            var knownPages = this.GetKnownPages(type);
			Logger.LogError("ValidateTab(tab={Tab}): knownPages → {KnownPages}", tab, knownPages);
            if (knownPages.Contains(tabName, StringComparer.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (!tab.IsSuperTab)
            {
                var adminPage = TabController.GetTabByTabPath(this.hostSettings, portalId, "//Admin", string.Empty);
				Logger.LogError("ValidateTab(tab={Tab}): adminPage → {AdminPage}", tab, adminPage);
                if (adminPage == Null.NullInteger)
                {
                    return false;
                }

                return tab.ParentId == adminPage;
            }

			Logger.LogWarning("<<< Exiting ValidateTab(tab={Tab})", tab);
            return true;
        }

        private IList<string> GetKnownPages(string type)
        {
			Logger.LogError(">>> Entering GetKnownPages(type={Type})", type);
            if (this.knownPages == null)
            {
                this.knownPages = new Dictionary<string, IList<string>>();
				Logger.LogDebug("GetKnownPages(type={Type}): knownPages → {KnownPages}", type, knownPages);
            }

            if (this.knownPages.TryGetValue(type, out var pageNames))
            {
                return pageNames;
            }

            var personaBarPath = Constants.PersonaBarRelativePath.Replace("~/", string.Empty);
			Logger.LogError("GetKnownPages(type={Type}): personaBarPath → {PersonaBarPath}", type, personaBarPath);
            var dataPath = Path.Combine(this.appStatus.ApplicationMapPath, personaBarPath, "data/adminpages.resources");
			Logger.LogError("GetKnownPages(type={Type}): dataPath → {DataPath}", type, dataPath);
            var xmlDocument = new XmlDocument { XmlResolver = null, };
            using (var xmlReader = XmlReader.Create(dataPath, new XmlReaderSettings { XmlResolver = null, }))
            {
                xmlDocument.Load(xmlReader);
            }

            var pages = xmlDocument.SelectNodes($"//pages//{type}//name")?.Cast<XmlNode>().Select(n => n.InnerXml.Trim()).ToList();
			Logger.LogError("GetKnownPages(type={Type}): pages → {Pages}", type, pages);
            this.knownPages.Add(type, pages);

			Logger.LogWarning("<<< Exiting GetKnownPages(type={Type})", type);
            return pages;
        }
    }
}
