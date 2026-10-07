// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components
{
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Web;
    using System.Web.UI;

    using Dnn.PersonaBar.Security.Services.Dto;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Authentication.OAuth;
    using DotNetNuke.Services.Localization;

    public class SecurityController
    {
        private static IPortalSettings PortalSettings => PortalController.Instance.GetCurrentSettings();

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IEnumerable<string> GetAuthenticationProviders()
        {
			eventLogger.LogError(">>> Entering GetAuthenticationProviders()");
            var authSystems = AuthenticationController.GetEnabledAuthenticationServices();
            UserControl uc = new UserControl();
			eventLogger.LogError("GetAuthenticationProviders(): uc → {Uc}", uc);
            var authProviders = from authProvider in authSystems
                                 let authLoginControl = (AuthenticationLoginBase)uc.LoadControl("~/" + authProvider.LoginControlSrc)
                                 let oAuthLoginControl = authLoginControl as OAuthLoginBase
								eventLogger.LogError("GetAuthenticationProviders(): authSystems → {AuthSystems}", authSystems);
                                 where oAuthLoginControl == null && authLoginControl.Enabled
                                 select authProvider.AuthenticationType;

			eventLogger.LogWarning("<<< Exiting GetAuthenticationProviders()");
            return authProviders;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IList<UserRoleInfo> GetAdminUsers(int portalId, string cultureCode = "")
        {
			eventLogger.LogError(">>> Entering GetAdminUsers(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            var activeLanguage = string.IsNullOrEmpty(cultureCode)
                ? LocaleController.Instance.GetDefaultLocale(portalId).Code
                : cultureCode;
            var portal = PortalController.Instance.GetPortal(portalId, activeLanguage);
			eventLogger.LogError("GetAdminUsers(portalId={PortalId},cultureCode={CultureCode}): portal → {Portal}", portalId, cultureCode, portal);
            var adminUsers = RoleController.Instance.GetUserRoles(portalId, null, portal.AdministratorRoleName);
			eventLogger.LogError("GetAdminUsers(portalId={PortalId},cultureCode={CultureCode}): adminUsers → {AdminUsers}", portalId, cultureCode, adminUsers);

            return adminUsers;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public TabDto GetPortalTabs(int portalId, string cultureCode, bool isMultiLanguage)
        {
			eventLogger.LogError(">>> Entering GetPortalTabs(portalId={PortalId},cultureCode={CultureCode},isMultiLanguage={IsMultiLanguage})", portalId, cultureCode, isMultiLanguage);
            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = LocaleController.Instance.GetDefaultLocale(portalId).Code;
				eventLogger.LogError("GetPortalTabs(portalId={PortalId},cultureCode={CultureCode},isMultiLanguage={IsMultiLanguage}): cultureCode → {CultureCode1}", portalId, cultureCode, isMultiLanguage, cultureCode);
            }

            var rootNode = new TabDto
            {
                Name = PortalSettings.PortalName,
                TabId = Null.NullInteger.ToString(CultureInfo.InvariantCulture),
                ParentTabId = Null.NullInteger,
                ChildTabs = new List<TabDto>(),
            };

            var portalInfo = PortalController.Instance.GetPortal(portalId);
			eventLogger.LogError("GetPortalTabs(portalId={PortalId},cultureCode={CultureCode},isMultiLanguage={IsMultiLanguage}): portalInfo → {PortalInfo}", portalId, cultureCode, isMultiLanguage, portalInfo);
            var tabs = TabController.GetPortalTabs(isMultiLanguage ? TabController.GetTabsBySortOrder(portalId, portalInfo.DefaultLanguage, true) : TabController.GetTabsBySortOrder(portalId, cultureCode, true), Null.NullInteger, false, "<" + Localization.GetString("None_Specified") + ">", true, false, true, false, false).Where(t => !t.IsSystem).ToList();
			eventLogger.LogError("GetPortalTabs(portalId={PortalId},cultureCode={CultureCode},isMultiLanguage={IsMultiLanguage}): tabs → {Tabs}", portalId, cultureCode, isMultiLanguage, tabs);
            foreach (var tab in tabs)
            {
                if (tab.Level == 0 && tab.TabID != portalInfo.AdminTabId)
                {
                    var node = new TabDto
                    {
                        Name = tab.TabName,
                        TabId = tab.TabID.ToString(CultureInfo.InvariantCulture),
                        ParentTabId = tab.ParentId,
                        ChildTabs = new List<TabDto>(),
                    };

                    AddChildNodes(node, portalInfo, cultureCode);
                    rootNode.ChildTabs.Add(node);
                }
            }

			eventLogger.LogWarning("<<< Exiting GetPortalTabs(portalId={PortalId},cultureCode={CultureCode},isMultiLanguage={IsMultiLanguage})", portalId, cultureCode, isMultiLanguage);
            return rootNode;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public List<DataTable> GetModifiedSettings()
        {
			eventLogger.LogError(">>> Entering GetModifiedSettings()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var tables = new List<DataTable>();
            var reader = DataProvider.Instance().ExecuteReader("SecurityAnalyzer_GetModifiedSettings");
			eventLogger.LogError("GetModifiedSettings(): tables → {Tables}", tables);
            if (reader != null)
            {
				eventLogger.LogError("GetModifiedSettings(): reader → {Reader}", reader);
                do
                {
                    var table = new DataTable { Locale = CultureInfo.CurrentCulture };
                    table.Load(reader);
                    tables.Add(table);
                }
                while (!reader.IsClosed);
            }
			eventLogger.LogError("GetModifiedSettings(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

			eventLogger.LogWarning("<<< Exiting GetModifiedSettings()");
            return tables;
        }

        private static void AddChildNodes(TabDto parentNode, IPortalInfo portal, string cultureCode)
        {
			eventLogger.LogError(">>> Entering AddChildNodes(parentNode={ParentNode},portal={Portal},cultureCode={CultureCode})", parentNode, portal, cultureCode);
            if (parentNode.ChildTabs != null)
            {
                parentNode.ChildTabs.Clear();
                if (!int.TryParse(parentNode.TabId, out var parentId))
                {
                    parentId = 0;
					eventLogger.LogDebug("AddChildNodes(parentNode={ParentNode},portal={Portal},cultureCode={CultureCode}): parentId → {ParentId}", parentNode, portal, cultureCode, parentId);
                }

                var tabs = GetFilteredTabs(TabController.Instance.GetTabsByPortal(portal.PortalId).WithCulture(cultureCode, true)).WithParentId(parentId);
				eventLogger.LogError("AddChildNodes(parentNode={ParentNode},portal={Portal},cultureCode={CultureCode}): tabs → {Tabs}", parentNode, portal, cultureCode, tabs);

                foreach (var tab in tabs)
                {
                    if (tab.ParentId == parentId)
                    {
                        var node = new TabDto
                        {
                            Name = tab.TabName,
                            TabId = tab.TabID.ToString(CultureInfo.InvariantCulture),
                            ParentTabId = tab.ParentId,
                        };
                        AddChildNodes(node, portal, cultureCode);
                        parentNode.ChildTabs.Add(node);
                    }
                }
            }
			eventLogger.LogWarning("<<< Exiting AddChildNodes(parentNode={ParentNode},portal={Portal},cultureCode={CultureCode})", parentNode, portal, cultureCode);
        }

        private static TabCollection GetFilteredTabs(TabCollection tabs)
        {
			eventLogger.LogError(">>> Entering GetFilteredTabs(tabs={Tabs})", tabs);
            var filteredTabs = tabs.Where(kvp => !kvp.Value.IsSystem && !kvp.Value.IsDeleted && !kvp.Value.DisableLink).Select(kvp => kvp.Value);
			eventLogger.LogError("GetFilteredTabs(tabs={Tabs}): filteredTabs → {FilteredTabs}", tabs, filteredTabs);
			eventLogger.LogWarning("<<< Exiting GetFilteredTabs(tabs={Tabs})", tabs);
            return new TabCollection(filteredTabs);
        }
    }
}
