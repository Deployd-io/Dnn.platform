// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Pages.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml;

    using Dnn.PersonaBar.Pages.Components.Exceptions;
    using Dnn.PersonaBar.Pages.Services.Dto;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.UI;

    using Microsoft.Extensions.DependencyInjection;

    using TermHelper = DotNetNuke.Entities.Content.Taxonomy.TermHelper;

    public class BulkPagesController : ServiceLocator<IBulkPagesController, BulkPagesController>, IBulkPagesController
    {
        private const string DefaultPageTemplate = "Default.page.template";
        private static readonly Regex TabNameRegex = new Regex(">*(.*)", RegexOptions.Compiled);
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly IPermissionDefinitionService permissionDefinitionService;

        /// <summary>Initializes a new instance of the <see cref="BulkPagesController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public BulkPagesController()
            : this(null, null)
        {
		eventLogger.LogDebug(">>> Entering BulkPagesController()");
        }

		eventLogger.LogDebug("<<< Exiting BulkPagesController()");
        /// <summary>Initializes a new instance of the <see cref="BulkPagesController"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public BulkPagesController(IBusinessControllerProvider businessControllerProvider)
            : this(businessControllerProvider, null)
        {
		eventLogger.LogDebug(">>> Entering BulkPagesController(businessControllerProvider={BusinessControllerProvider})", businessControllerProvider);
		eventLogger.LogDebug("<<< Exiting BulkPagesController(businessControllerProvider={BusinessControllerProvider})", businessControllerProvider);
        }

        /// <summary>Initializes a new instance of the <see cref="BulkPagesController"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        public BulkPagesController(IBusinessControllerProvider businessControllerProvider, IPermissionDefinitionService permissionDefinitionService)
        {
            this.businessControllerProvider = businessControllerProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<IBusinessControllerProvider>();
            this.permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
			eventLogger.LogDebug("BulkPagesController(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService}): permissionDefinitionService → {PermissionDefinitionService1}", businessControllerProvider, permissionDefinitionService, permissionDefinitionService);
        }

        /// <inheritdoc />
        public BulkPageResponse AddBulkPages(BulkPage page, bool validateOnly)
        {
			eventLogger.LogDebug(">>> Entering AddBulkPages(page={Page},validateOnly={ValidateOnly})", page, validateOnly);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var portalId = portalSettings.PortalId;
			eventLogger.LogDebug("AddBulkPages(page={Page},validateOnly={ValidateOnly}): portalSettings → {PortalSettings}", page, validateOnly, portalSettings);
            var response = new BulkPageResponse();
			eventLogger.LogDebug("AddBulkPages(page={Page},validateOnly={ValidateOnly}): response → {Response}", page, validateOnly, response);
            var parentId = page.ParentId;
            var rootTab = TabController.Instance.GetTab(parentId, portalId, true);
			eventLogger.LogDebug("AddBulkPages(page={Page},validateOnly={ValidateOnly}): rootTab → {RootTab}", page, validateOnly, rootTab);

            var strValue = page.BulkPages;
			eventLogger.LogDebug("AddBulkPages(page={Page},validateOnly={ValidateOnly}): strValue → {StrValue}", page, validateOnly, strValue);
            strValue = strValue.Replace("\r", "\n").Replace("\n\n", "\n").Trim();

            if (!TabController.IsValidTabName(strValue, out var invalidType))
            {
				eventLogger.LogWarning("Throwing BulkPagesException from AddBulkPages(page={Page},validateOnly={ValidateOnly})", page, validateOnly);
                throw new BulkPagesException("bulkPages", string.Format(CultureInfo.CurrentCulture, Localization.GetString(invalidType), strValue));
            }

            if (page.StartDate.HasValue && page.EndDate.HasValue && page.StartDate > page.EndDate)
            {
                throw new BulkPagesException("endDate", Localization.GetString("StartDateAfterEndDate"));
            }

            var pages = strValue.Split('\n');
			eventLogger.LogDebug("AddBulkPages(page={Page},validateOnly={ValidateOnly}): pages → {Pages}", page, validateOnly, pages);
            var tabs = new List<TabInfo>();
			eventLogger.LogDebug("AddBulkPages(page={Page},validateOnly={ValidateOnly}): tabs → {Tabs}", page, validateOnly, tabs);

            foreach (var strLine in pages)
            {
                var tab = new TabInfo
                {
                    TabName = TabNameRegex.Replace(strLine, "${1}"),
                    Level = strLine.LastIndexOf(">", StringComparison.Ordinal) + 1,
                    KeyWords = page.Keywords,
                    StartDate = page.StartDate ?? Null.NullDate,
                    EndDate = page.EndDate ?? Null.NullDate,
                    IsVisible = page.IncludeInMenu,
                };
                tab.Terms.AddRange(TermHelper.ToTabTerms(page.Tags, portalId));
                tabs.Add(tab);
            }

            var currentIndex = -1;
            var bulkPageItems = new List<BulkPageResponseItem>();
			eventLogger.LogDebug("AddBulkPages(page={Page},validateOnly={ValidateOnly}): bulkPageItems → {BulkPageItems}", page, validateOnly, bulkPageItems);
            foreach (var oTab in tabs)
            {
                currentIndex += 1;

                try
                {
                    string errorMessage = null;
					eventLogger.LogDebug("AddBulkPages(page={Page},validateOnly={ValidateOnly}): errorMessage → {ErrorMessage}", page, validateOnly, errorMessage);

                    if (currentIndex == 0 && oTab.Level > 0)
                    {
                        errorMessage = Localization.GetString("NoParentTabName");
                    }
                    else if (DuplicateExists(currentIndex, pages))
                    {
                        errorMessage = Localization.GetString("TabExists");
                    }
                    else if (oTab.Level == 0)
                    {
                        oTab.TabID = this.CreateTabFromParent(portalSettings, rootTab, oTab, parentId, validateOnly, out errorMessage);
                    }
                    else if (validateOnly)
                    {
                        if (string.IsNullOrWhiteSpace(oTab.TabName))
                        {
                            errorMessage = string.Format(CultureInfo.CurrentCulture, Localization.GetString("EmptyTabName"), oTab.TabName);
                        }
                        else
                        {
                            errorMessage = string.Empty;
                        }
                    }
                    else
                    {
                        var parentTabId = GetParentTabId(tabs, currentIndex, oTab.Level - 1);
						eventLogger.LogDebug("AddBulkPages(page={Page},validateOnly={ValidateOnly}): parentTabId → {ParentTabId}", page, validateOnly, parentTabId);
                        if (parentTabId != Null.NullInteger)
                        {
                            oTab.TabID = this.CreateTabFromParent(portalSettings, rootTab, oTab, parentTabId, validateOnly, out errorMessage);
                        }
                    }

                    bulkPageItems.Add(ToBulkPageResponseItem(oTab, errorMessage));
                }
                catch (Exception ex)
                {
					eventLogger.LogError("Exception in AddBulkPages(page={Page},validateOnly={ValidateOnly}): {Arg2}", page, validateOnly, ex.getMessage());
                    throw new DotNetNukeException("Unable to process page.", ex, DotNetNukeErrorCode.DeserializePanesFailed);
                }
            }

            response.Pages = bulkPageItems;
			eventLogger.LogDebug("AddBulkPages(page={Page},validateOnly={ValidateOnly}): response.Pages → {Arg2}", page, validateOnly, response.Pages);

			eventLogger.LogDebug("<<< Exiting AddBulkPages(page={Page},validateOnly={ValidateOnly})", page, validateOnly);
            return response;
        }

        /// <inheritdoc />
        protected override Func<IBulkPagesController> GetFactory()
        {
			eventLogger.LogDebug(">>> Entering GetFactory()");
			eventLogger.LogDebug("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<IBulkPagesController>;
        }

        /// <summary>Checks whether there is a duplicate tab exists.</summary>
        /// <param name="pageIndex">Page index to check the duplicate for.</param>
        /// <param name="pages">Page list to check the duplicates from.</param>
        /// <returns>True in case there is a tab with the same name and path and with the index of greater than the first occurrence; false otherwise.</returns>
        private static bool DuplicateExists(int pageIndex, string[] pages)
        {
			eventLogger.LogDebug(">>> Entering DuplicateExists(pageIndex={PageIndex},pages={Pages})", pageIndex, pages);
            var pageName = pages[pageIndex];
            var pageDirectory = GetPageDirectory(pageIndex, pages);
			eventLogger.LogDebug("DuplicateExists(pageIndex={PageIndex},pages={Pages}): pageDirectory → {PageDirectory}", pageIndex, pages, pageDirectory);

            for (var index = 0; index < pageIndex; index++)
            {
                var loopedDirectory = GetPageDirectory(index, pages);
				eventLogger.LogDebug("DuplicateExists(pageIndex={PageIndex},pages={Pages}): loopedDirectory → {LoopedDirectory}", pageIndex, pages, loopedDirectory);
                if (loopedDirectory != pageDirectory)
                {
                    continue;
                }
                else if (string.Equals(pageName, pages[index], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

			eventLogger.LogDebug("<<< Exiting DuplicateExists(pageIndex={PageIndex},pages={Pages})", pageIndex, pages);
            return false;
        }

        private static string GetPageDirectory(int pageIndex, string[] pages)
        {
			eventLogger.LogDebug(">>> Entering GetPageDirectory(pageIndex={PageIndex},pages={Pages})", pageIndex, pages);
            var fullPath = string.Empty;
            var pageLevel = GetPageLevel(pages[pageIndex]);
			eventLogger.LogDebug("GetPageDirectory(pageIndex={PageIndex},pages={Pages}): pageLevel → {PageLevel}", pageIndex, pages, pageLevel);
            var currentLevel = pageLevel;
			eventLogger.LogDebug("GetPageDirectory(pageIndex={PageIndex},pages={Pages}): currentLevel → {CurrentLevel}", pageIndex, pages, currentLevel);
			eventLogger.LogDebug("GetPageDirectory(pageIndex={PageIndex},pages={Pages}): fullPath → {FullPath}", pageIndex, pages, fullPath);
            for (var index = pageIndex; index > -1; index--)
            {
                var currentPage = pages[index];
                var level = GetPageLevel(currentPage);
				eventLogger.LogDebug("GetPageDirectory(pageIndex={PageIndex},pages={Pages}): level → {Level}", pageIndex, pages, level);
                if (level < currentLevel)
                {
                    currentLevel = level;
                    fullPath = $"{currentPage}>{fullPath}";
                }
            }

			eventLogger.LogDebug("<<< Exiting GetPageDirectory(pageIndex={PageIndex},pages={Pages})", pageIndex, pages);
            return fullPath;
        }

        private static int GetPageLevel(string pageName)
        {
			eventLogger.LogDebug(">>> Entering GetPageLevel(pageName={PageName})", pageName);
			eventLogger.LogDebug("<<< Exiting GetPageLevel(pageName={PageName})", pageName);
            return pageName
                .Replace(" ", string.Empty)
                .TakeWhile(t => t == '>')
                .Count();
        }

        private static BulkPageResponseItem ToBulkPageResponseItem(TabInfo tab, string error)
        {
			eventLogger.LogDebug(">>> Entering ToBulkPageResponseItem(tab={Tab},error={Error})", tab, error);
			eventLogger.LogDebug("<<< Exiting ToBulkPageResponseItem(tab={Tab},error={Error})", tab, error);
            return new BulkPageResponseItem
            {
                TabId = tab.TabID,
                ErrorMessage = error,
                Status = (error == null && tab.TabID > 0) ? 0 : 1,
                PageName = tab.TabName,
            };
        }

        private static int GetParentTabId(List<TabInfo> lstTabs, int currentIndex, int parentLevel)
        {
			eventLogger.LogDebug(">>> Entering GetParentTabId(lstTabs={LstTabs},currentIndex={CurrentIndex},parentLevel={ParentLevel})", lstTabs, currentIndex, parentLevel);
            var oParent = lstTabs[0];

			eventLogger.LogDebug("GetParentTabId(lstTabs={LstTabs},currentIndex={CurrentIndex},parentLevel={ParentLevel}): oParent → {OParent}", lstTabs, currentIndex, parentLevel, oParent);
            for (var i = 0; i < lstTabs.Count; i++)
            {
                if (i == currentIndex)
                {
                    return oParent.TabID;
                }

                if (lstTabs[i].Level == parentLevel)
                {
                    oParent = lstTabs[i];
                }
            }

			eventLogger.LogDebug("<<< Exiting GetParentTabId(lstTabs={LstTabs},currentIndex={CurrentIndex},parentLevel={ParentLevel})", lstTabs, currentIndex, parentLevel);
            return Null.NullInteger;
        }

        private static bool IsValidTabPath(TabInfo tab, string newTabPath, out string errorMessage)
        {
			eventLogger.LogDebug(">>> Entering IsValidTabPath(tab={Tab},newTabPath={NewTabPath})", tab, newTabPath);
            var valid = true;
            errorMessage = null;

			eventLogger.LogDebug("IsValidTabPath(tab={Tab},newTabPath={NewTabPath}): errorMessage → {ErrorMessage}", tab, newTabPath, errorMessage);
			eventLogger.LogDebug("IsValidTabPath(tab={Tab},newTabPath={NewTabPath}): valid → {Valid}", tab, newTabPath, valid);
            // get default culture if the tab's culture is null
            var cultureCode = tab.CultureCode;
			eventLogger.LogDebug("IsValidTabPath(tab={Tab},newTabPath={NewTabPath}): cultureCode → {CultureCode}", tab, newTabPath, cultureCode);
            if (string.IsNullOrEmpty(cultureCode))
            {
                var portalSettings = PortalController.Instance.GetCurrentSettings();
				eventLogger.LogDebug("IsValidTabPath(tab={Tab},newTabPath={NewTabPath}): portalSettings → {PortalSettings}", tab, newTabPath, portalSettings);
                cultureCode = portalSettings.DefaultLanguage;
            }

            // Validate Tab Path
            var tabId = TabController.GetTabByTabPath(tab.PortalID, newTabPath, cultureCode);
			eventLogger.LogDebug("IsValidTabPath(tab={Tab},newTabPath={NewTabPath}): tabId → {TabId}", tab, newTabPath, tabId);
            if (tabId != Null.NullInteger && tabId != tab.TabID)
            {
                var existingTab = TabController.Instance.GetTab(tabId, tab.PortalID, false);
				eventLogger.LogDebug("IsValidTabPath(tab={Tab},newTabPath={NewTabPath}): existingTab → {ExistingTab}", tab, newTabPath, existingTab);
                if (existingTab is { IsDeleted: true, })
                {
                    errorMessage = Localization.GetString("TabRecycled");
                }
                else
                {
                    errorMessage = Localization.GetString("TabExists");
                }

                valid = false;
            }

            // check whether there are conflicts between tab path and portal alias.
            if (TabController.IsDuplicateWithPortalAlias(tab.PortalID, newTabPath))
            {
                errorMessage = string.Format(CultureInfo.CurrentCulture, Localization.GetString("PathDuplicateWithAlias"), tab.TabName, newTabPath);
                valid = false;
            }

			eventLogger.LogDebug("<<< Exiting IsValidTabPath(tab={Tab},newTabPath={NewTabPath})", tab, newTabPath);
            return valid;
        }

        private int CreateTabFromParent(IPortalSettings portalSettings, TabInfo objRoot, TabInfo oTab, int parentId, bool validateOnly, out string errorMessage)
        {
			eventLogger.LogDebug(">>> Entering CreateTabFromParent(portalSettings={PortalSettings},objRoot={ObjRoot},oTab={OTab},parentId={ParentId},validateOnly={ValidateOnly})", portalSettings, objRoot, oTab, parentId, validateOnly);
            var tab = new TabInfo
            {
                PortalID = portalSettings.PortalId,
                TabName = oTab.TabName,
                ParentId = parentId,
                Title = string.Empty,
                Description = string.Empty,
                KeyWords = oTab.KeyWords,
                IsVisible = oTab.IsVisible,
                DisableLink = false,
                IconFile = string.Empty,
                IconFileLarge = string.Empty,
                IsDeleted = false,
                Url = string.Empty,
                SkinSrc = string.Empty,
                ContainerSrc = string.Empty,
                CultureCode = Null.NullString,
                StartDate = oTab.StartDate,
                EndDate = oTab.EndDate,
            };

            tab.Terms.AddRange(oTab.Terms);

            if (objRoot != null)
            {
                // TODO: To be retrieved once the parent tab  is selected?
                ////tab.IsVisible = objRoot.IsVisible;
                tab.DisableLink = objRoot.DisableLink;
				eventLogger.LogDebug("CreateTabFromParent(portalSettings={PortalSettings},objRoot={ObjRoot},oTab={OTab},parentId={ParentId},validateOnly={ValidateOnly}): tab.DisableLink → {Arg5}", portalSettings, objRoot, oTab, parentId, validateOnly, tab.DisableLink);
				eventLogger.LogDebug("CreateTabFromParent(portalSettings={PortalSettings},objRoot={ObjRoot},oTab={OTab},parentId={ParentId},validateOnly={ValidateOnly}): tab.SkinSrc → {Arg5}", portalSettings, objRoot, oTab, parentId, validateOnly, tab.SkinSrc);
				eventLogger.LogDebug("CreateTabFromParent(portalSettings={PortalSettings},objRoot={ObjRoot},oTab={OTab},parentId={ParentId},validateOnly={ValidateOnly}): tab.ContainerSrc → {Arg5}", portalSettings, objRoot, oTab, parentId, validateOnly, tab.ContainerSrc);
                tab.SkinSrc = objRoot.SkinSrc;
                tab.ContainerSrc = objRoot.ContainerSrc;
            }

            if (portalSettings.ContentLocalizationEnabled)
            {
                tab.CultureCode = LocaleController.Instance.GetDefaultLocale(tab.PortalID).Code;
            }

            var parentTab = TabController.Instance.GetTab(parentId, -1, false);
			eventLogger.LogDebug("CreateTabFromParent(portalSettings={PortalSettings},objRoot={ObjRoot},oTab={OTab},parentId={ParentId},validateOnly={ValidateOnly}): parentTab → {ParentTab}", portalSettings, objRoot, oTab, parentId, validateOnly, parentTab);

            if (parentTab != null)
            {
                tab.PortalID = parentTab.PortalID;
				eventLogger.LogDebug("CreateTabFromParent(portalSettings={PortalSettings},objRoot={ObjRoot},oTab={OTab},parentId={ParentId},validateOnly={ValidateOnly}): tab.PortalID → {Arg5}", portalSettings, objRoot, oTab, parentId, validateOnly, tab.PortalID);
				eventLogger.LogDebug("CreateTabFromParent(portalSettings={PortalSettings},objRoot={ObjRoot},oTab={OTab},parentId={ParentId},validateOnly={ValidateOnly}): tab.ParentId → {Arg5}", portalSettings, objRoot, oTab, parentId, validateOnly, tab.ParentId);
                tab.ParentId = parentTab.TabID;
            }
            else
            {
                ////return Null.NullInteger;
                tab.PortalID = portalSettings.PortalId;
                tab.ParentId = Null.NullInteger;
            }

            tab.TabPath = Globals.GenerateTabPath(tab.ParentId, tab.TabName);

            // Check for invalid
            if (!TabController.IsValidTabName(tab.TabName, out var invalidType))
            {
                errorMessage = string.Format(CultureInfo.CurrentCulture, Localization.GetString(invalidType), tab.TabName);
				eventLogger.LogDebug("CreateTabFromParent(portalSettings={PortalSettings},objRoot={ObjRoot},oTab={OTab},parentId={ParentId},validateOnly={ValidateOnly}): errorMessage → {ErrorMessage}", portalSettings, objRoot, oTab, parentId, validateOnly, errorMessage);
                return Null.NullInteger;
            }

            // Validate Tab Path
            if (!IsValidTabPath(tab, tab.TabPath, out errorMessage))
            {
                return Null.NullInteger;
            }

            // Inherit permissions from parent
            tab.TabPermissions.Clear();
            if (tab.PortalID != Null.NullInteger && objRoot != null)
            {
                tab.TabPermissions.AddRange(objRoot.TabPermissions);
            }
            else if (tab.PortalID != Null.NullInteger)
            {
                // Give admin full permission
                foreach (var permission in this.permissionDefinitionService.GetDefinitionsByTab())
                {
                    var newTabPermission = new TabPermissionInfo
                    {
                        PermissionKey = permission.PermissionKey,
                        PermissionName = permission.PermissionName,
                        AllowAccess = true,
                    };
                    ((IPermissionInfo)newTabPermission).PermissionId = permission.PermissionId;
                    ((IPermissionInfo)newTabPermission).RoleId = portalSettings.AdministratorRoleId;
                    tab.TabPermissions.Add(newTabPermission);
                }
            }

            // Inherit other information from Parent
            if (objRoot != null)
            {
                // TODO: To be retrieved once the parent tab  is selected?
                ////tab.Terms.Clear();
                ////tab.StartDate = objRoot.StartDate;
                ////tab.EndDate = objRoot.EndDate;
                tab.RefreshInterval = objRoot.RefreshInterval;
				eventLogger.LogDebug("CreateTabFromParent(portalSettings={PortalSettings},objRoot={ObjRoot},oTab={OTab},parentId={ParentId},validateOnly={ValidateOnly}): tab.RefreshInterval → {Arg5}", portalSettings, objRoot, oTab, parentId, validateOnly, tab.RefreshInterval);
				eventLogger.LogDebug("CreateTabFromParent(portalSettings={PortalSettings},objRoot={ObjRoot},oTab={OTab},parentId={ParentId},validateOnly={ValidateOnly}): tab.SiteMapPriority → {Arg5}", portalSettings, objRoot, oTab, parentId, validateOnly, tab.SiteMapPriority);
				eventLogger.LogDebug("CreateTabFromParent(portalSettings={PortalSettings},objRoot={ObjRoot},oTab={OTab},parentId={ParentId},validateOnly={ValidateOnly}): tab.PageHeadText → {Arg5}", portalSettings, objRoot, oTab, parentId, validateOnly, tab.PageHeadText);
				eventLogger.LogDebug("CreateTabFromParent(portalSettings={PortalSettings},objRoot={ObjRoot},oTab={OTab},parentId={ParentId},validateOnly={ValidateOnly}): tab.IsSecure → {Arg5}", portalSettings, objRoot, oTab, parentId, validateOnly, tab.IsSecure);
				eventLogger.LogDebug("CreateTabFromParent(portalSettings={PortalSettings},objRoot={ObjRoot},oTab={OTab},parentId={ParentId},validateOnly={ValidateOnly}): tab.PermanentRedirect → {Arg5}", portalSettings, objRoot, oTab, parentId, validateOnly, tab.PermanentRedirect);
                tab.SiteMapPriority = objRoot.SiteMapPriority;
                tab.PageHeadText = objRoot.PageHeadText;
                tab.IsSecure = objRoot.IsSecure;
                tab.PermanentRedirect = objRoot.PermanentRedirect;
            }

            if (validateOnly)
            {
                return -1;
            }

            tab.TabID = TabController.Instance.AddTab(tab);
            this.ApplyDefaultTabTemplate(tab);

            // create localized tabs if content localization is enabled
            if (portalSettings.ContentLocalizationEnabled)
            {
                TabController.Instance.CreateLocalizedCopies(tab);
            }

			eventLogger.LogDebug("<<< Exiting CreateTabFromParent(portalSettings={PortalSettings},objRoot={ObjRoot},oTab={OTab},parentId={ParentId},validateOnly={ValidateOnly})", portalSettings, objRoot, oTab, parentId, validateOnly);
            return tab.TabID;
        }

        private void ApplyDefaultTabTemplate(TabInfo tab)
        {
			eventLogger.LogDebug(">>> Entering ApplyDefaultTabTemplate(tab={Tab})", tab);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var templateFile = Path.Combine(portalSettings.HomeDirectoryMapPath, $@"Templates\{DefaultPageTemplate}");
			eventLogger.LogDebug("ApplyDefaultTabTemplate(tab={Tab}): templateFile → {TemplateFile}", tab, templateFile);
			eventLogger.LogDebug("ApplyDefaultTabTemplate(tab={Tab}): portalSettings → {PortalSettings}", tab, portalSettings);

            if (!File.Exists(templateFile))
            {
                return;
            }

            var xmlDoc = new XmlDocument { XmlResolver = null };
            try
            {
                using (var xmlReader = XmlReader.Create(templateFile, new XmlReaderSettings { XmlResolver = null, }))
                {
                    xmlDoc.Load(xmlReader);
                }

                TabController.DeserializePanes(this.businessControllerProvider, xmlDoc.SelectSingleNode("//portal/tabs/tab/panes"), tab.PortalID, tab.TabID, PortalTemplateModuleAction.Ignore, new Hashtable());
            }
            catch (Exception ex)
            {
				eventLogger.LogError("Exception in ApplyDefaultTabTemplate(tab={Tab}): {Arg1}", tab, ex.getMessage());
			eventLogger.LogDebug("<<< Exiting ApplyDefaultTabTemplate(tab={Tab})", tab);
                throw new DotNetNukeException("Unable to process page template.", ex, DotNetNukeErrorCode.DeserializePanesFailed);
            }
        }
    }
}
