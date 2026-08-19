// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Pages.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;

    using Dnn.PersonaBar.Library.Dto;
    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Pages.Components.Dto;
    using Dnn.PersonaBar.Pages.Components.Exceptions;
    using Dnn.PersonaBar.Pages.Services.Dto;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Repositories;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;

    using Microsoft.Extensions.DependencyInjection;

    using PermissionsNotMetException = DotNetNuke.Entities.Tabs.PermissionsNotMetException;

    [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
    public class PagesControllerImpl : IPagesController
    {
        public const string PageTagsVocabulary = "PageTags";

        private static readonly IList<string> TabSettingKeys = new List<string> { "CustomStylesheet" };
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly ITabController tabController;
        private readonly IModuleController moduleController;
        private readonly IPageUrlsController pageUrlsController;
        private readonly ITemplateController templateController;
        private readonly IDefaultPortalThemeController defaultPortalThemeController;
        private readonly ICloneModuleExecutionContext cloneModuleExecutionContext;
        private readonly IUrlRewriterUtilsWrapper urlRewriterUtilsWrapper;
        private readonly IFriendlyUrlWrapper friendlyUrlWrapper;
        private readonly IContentVerifier contentVerifier;
        private readonly IPortalController portalController;
        private readonly PersonalizationController personalizationController;
        private readonly IPermissionDefinitionService permissionDefinitionService;
        private readonly IPortalAliasService portalAliasService;
        private readonly IHostSettings hostSettings;
        private readonly IHostSettingsService hostSettingsService;

        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public PagesControllerImpl(IBusinessControllerProvider businessControllerProvider, ITemplateController templateController)
            : this(
                  businessControllerProvider,
                  TabController.Instance,
                  ModuleController.Instance,
                  PageUrlsController.Instance,
                  templateController,
                  DefaultPortalThemeController.Instance,
                  CloneModuleExecutionContext.Instance,
                  new UrlRewriterUtilsWrapper(),
                  new FriendlyUrlWrapper(),
                  new ContentVerifier(),
                  PortalController.Instance)
        {
		eventLogger.LogDebug(">>> Entering PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},templateController={TemplateController})", businessControllerProvider, templateController);
        }

        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public PagesControllerImpl(
            IBusinessControllerProvider businessControllerProvider,
            ITabController tabController,
            IModuleController moduleController,
            IPageUrlsController pageUrlsController,
            ITemplateController templateController,
            IDefaultPortalThemeController defaultPortalThemeController,
            ICloneModuleExecutionContext cloneModuleExecutionContext,
            IUrlRewriterUtilsWrapper urlRewriterUtilsWrapper,
            IFriendlyUrlWrapper friendlyUrlWrapper,
            IContentVerifier contentVerifier,
            IPortalController portalController)
            : this(
                businessControllerProvider,
                tabController,
                moduleController,
                pageUrlsController,
                templateController,
                defaultPortalThemeController,
                cloneModuleExecutionContext,
                urlRewriterUtilsWrapper,
                friendlyUrlWrapper,
                contentVerifier,
                portalController,
                null)
        {
        }
		eventLogger.LogDebug("<<< Exiting PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},templateController={TemplateController})", businessControllerProvider, templateController);

        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public PagesControllerImpl(
            IBusinessControllerProvider businessControllerProvider,
            ITabController tabController,
            IModuleController moduleController,
            IPageUrlsController pageUrlsController,
            ITemplateController templateController,
            IDefaultPortalThemeController defaultPortalThemeController,
            ICloneModuleExecutionContext cloneModuleExecutionContext,
            IUrlRewriterUtilsWrapper urlRewriterUtilsWrapper,
            IFriendlyUrlWrapper friendlyUrlWrapper,
            IContentVerifier contentVerifier,
            IPortalController portalController,
            PersonalizationController personalizationController)
            : this(businessControllerProvider, tabController, moduleController, pageUrlsController, templateController, defaultPortalThemeController, cloneModuleExecutionContext, urlRewriterUtilsWrapper, friendlyUrlWrapper, contentVerifier, portalController, personalizationController, null, null, null, null)
        {
        }

        public PagesControllerImpl(
            IBusinessControllerProvider businessControllerProvider,
            ITabController tabController,
            IModuleController moduleController,
            IPageUrlsController pageUrlsController,
            ITemplateController templateController,
            IDefaultPortalThemeController defaultPortalThemeController,
            ICloneModuleExecutionContext cloneModuleExecutionContext,
            IUrlRewriterUtilsWrapper urlRewriterUtilsWrapper,
            IFriendlyUrlWrapper friendlyUrlWrapper,
            IContentVerifier contentVerifier,
            IPortalController portalController,
            PersonalizationController personalizationController,
            IPermissionDefinitionService permissionDefinitionService,
            IPortalAliasService portalAliasService,
            IHostSettings hostSettings,
            IHostSettingsService hostSettingsService)
        {
			eventLogger.LogDebug("PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},tabController={TabController},moduleController={ModuleController},pageUrlsController={PageUrlsController},templateController={TemplateController},defaultPortalThemeController={DefaultPortalThemeController},cloneModuleExecutionContext={CloneModuleExecutionContext},urlRewriterUtilsWrapper={UrlRewriterUtilsWrapper},friendlyUrlWrapper={FriendlyUrlWrapper},contentVerifier={ContentVerifier},portalController={PortalController},personalizationController={PersonalizationController},permissionDefinitionService={PermissionDefinitionService},portalAliasService={PortalAliasService},hostSettings={HostSettings},hostSettingsService={HostSettingsService}): templateController → {TemplateController1}", businessControllerProvider, tabController, moduleController, pageUrlsController, templateController, defaultPortalThemeController, cloneModuleExecutionContext, urlRewriterUtilsWrapper, friendlyUrlWrapper, contentVerifier, portalController, personalizationController, permissionDefinitionService, portalAliasService, hostSettings, hostSettingsService, templateController);
			eventLogger.LogDebug("PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},tabController={TabController},moduleController={ModuleController},pageUrlsController={PageUrlsController},templateController={TemplateController},defaultPortalThemeController={DefaultPortalThemeController},cloneModuleExecutionContext={CloneModuleExecutionContext},urlRewriterUtilsWrapper={UrlRewriterUtilsWrapper},friendlyUrlWrapper={FriendlyUrlWrapper},contentVerifier={ContentVerifier},portalController={PortalController},personalizationController={PersonalizationController},permissionDefinitionService={PermissionDefinitionService},portalAliasService={PortalAliasService},hostSettings={HostSettings},hostSettingsService={HostSettingsService}): defaultPortalThemeController → {DefaultPortalThemeController1}", businessControllerProvider, tabController, moduleController, pageUrlsController, templateController, defaultPortalThemeController, cloneModuleExecutionContext, urlRewriterUtilsWrapper, friendlyUrlWrapper, contentVerifier, portalController, personalizationController, permissionDefinitionService, portalAliasService, hostSettings, hostSettingsService, defaultPortalThemeController);
			eventLogger.LogDebug("PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},tabController={TabController},moduleController={ModuleController},pageUrlsController={PageUrlsController},templateController={TemplateController},defaultPortalThemeController={DefaultPortalThemeController},cloneModuleExecutionContext={CloneModuleExecutionContext},urlRewriterUtilsWrapper={UrlRewriterUtilsWrapper},friendlyUrlWrapper={FriendlyUrlWrapper},contentVerifier={ContentVerifier},portalController={PortalController},personalizationController={PersonalizationController},permissionDefinitionService={PermissionDefinitionService},portalAliasService={PortalAliasService},hostSettings={HostSettings},hostSettingsService={HostSettingsService}): cloneModuleExecutionContext → {CloneModuleExecutionContext1}", businessControllerProvider, tabController, moduleController, pageUrlsController, templateController, defaultPortalThemeController, cloneModuleExecutionContext, urlRewriterUtilsWrapper, friendlyUrlWrapper, contentVerifier, portalController, personalizationController, permissionDefinitionService, portalAliasService, hostSettings, hostSettingsService, cloneModuleExecutionContext);
			eventLogger.LogDebug("PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},tabController={TabController},moduleController={ModuleController},pageUrlsController={PageUrlsController},templateController={TemplateController},defaultPortalThemeController={DefaultPortalThemeController},cloneModuleExecutionContext={CloneModuleExecutionContext},urlRewriterUtilsWrapper={UrlRewriterUtilsWrapper},friendlyUrlWrapper={FriendlyUrlWrapper},contentVerifier={ContentVerifier},portalController={PortalController},personalizationController={PersonalizationController},permissionDefinitionService={PermissionDefinitionService},portalAliasService={PortalAliasService},hostSettings={HostSettings},hostSettingsService={HostSettingsService}): urlRewriterUtilsWrapper → {UrlRewriterUtilsWrapper1}", businessControllerProvider, tabController, moduleController, pageUrlsController, templateController, defaultPortalThemeController, cloneModuleExecutionContext, urlRewriterUtilsWrapper, friendlyUrlWrapper, contentVerifier, portalController, personalizationController, permissionDefinitionService, portalAliasService, hostSettings, hostSettingsService, urlRewriterUtilsWrapper);
            this.businessControllerProvider = businessControllerProvider;
			eventLogger.LogDebug("PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},tabController={TabController},moduleController={ModuleController},pageUrlsController={PageUrlsController},templateController={TemplateController},defaultPortalThemeController={DefaultPortalThemeController},cloneModuleExecutionContext={CloneModuleExecutionContext},urlRewriterUtilsWrapper={UrlRewriterUtilsWrapper},friendlyUrlWrapper={FriendlyUrlWrapper},contentVerifier={ContentVerifier},portalController={PortalController},personalizationController={PersonalizationController},permissionDefinitionService={PermissionDefinitionService},portalAliasService={PortalAliasService},hostSettings={HostSettings},hostSettingsService={HostSettingsService}): businessControllerProvider → {BusinessControllerProvider1}", businessControllerProvider, tabController, moduleController, pageUrlsController, templateController, defaultPortalThemeController, cloneModuleExecutionContext, urlRewriterUtilsWrapper, friendlyUrlWrapper, contentVerifier, portalController, personalizationController, permissionDefinitionService, portalAliasService, hostSettings, hostSettingsService, businessControllerProvider);
			eventLogger.LogDebug("PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},tabController={TabController},moduleController={ModuleController},pageUrlsController={PageUrlsController},templateController={TemplateController},defaultPortalThemeController={DefaultPortalThemeController},cloneModuleExecutionContext={CloneModuleExecutionContext},urlRewriterUtilsWrapper={UrlRewriterUtilsWrapper},friendlyUrlWrapper={FriendlyUrlWrapper},contentVerifier={ContentVerifier},portalController={PortalController},personalizationController={PersonalizationController},permissionDefinitionService={PermissionDefinitionService},portalAliasService={PortalAliasService},hostSettings={HostSettings},hostSettingsService={HostSettingsService}): this.businessControllerProvider → {Arg16}", businessControllerProvider, tabController, moduleController, pageUrlsController, templateController, defaultPortalThemeController, cloneModuleExecutionContext, urlRewriterUtilsWrapper, friendlyUrlWrapper, contentVerifier, portalController, personalizationController, permissionDefinitionService, portalAliasService, hostSettings, hostSettingsService, this.businessControllerProvider);
			eventLogger.LogDebug("PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},tabController={TabController},moduleController={ModuleController},pageUrlsController={PageUrlsController},templateController={TemplateController},defaultPortalThemeController={DefaultPortalThemeController},cloneModuleExecutionContext={CloneModuleExecutionContext},urlRewriterUtilsWrapper={UrlRewriterUtilsWrapper},friendlyUrlWrapper={FriendlyUrlWrapper},contentVerifier={ContentVerifier},portalController={PortalController},personalizationController={PersonalizationController},permissionDefinitionService={PermissionDefinitionService},portalAliasService={PortalAliasService},hostSettings={HostSettings},hostSettingsService={HostSettingsService}): moduleController → {ModuleController1}", businessControllerProvider, tabController, moduleController, pageUrlsController, templateController, defaultPortalThemeController, cloneModuleExecutionContext, urlRewriterUtilsWrapper, friendlyUrlWrapper, contentVerifier, portalController, personalizationController, permissionDefinitionService, portalAliasService, hostSettings, hostSettingsService, moduleController);
			eventLogger.LogDebug("PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},tabController={TabController},moduleController={ModuleController},pageUrlsController={PageUrlsController},templateController={TemplateController},defaultPortalThemeController={DefaultPortalThemeController},cloneModuleExecutionContext={CloneModuleExecutionContext},urlRewriterUtilsWrapper={UrlRewriterUtilsWrapper},friendlyUrlWrapper={FriendlyUrlWrapper},contentVerifier={ContentVerifier},portalController={PortalController},personalizationController={PersonalizationController},permissionDefinitionService={PermissionDefinitionService},portalAliasService={PortalAliasService},hostSettings={HostSettings},hostSettingsService={HostSettingsService}): pageUrlsController → {PageUrlsController1}", businessControllerProvider, tabController, moduleController, pageUrlsController, templateController, defaultPortalThemeController, cloneModuleExecutionContext, urlRewriterUtilsWrapper, friendlyUrlWrapper, contentVerifier, portalController, personalizationController, permissionDefinitionService, portalAliasService, hostSettings, hostSettingsService, pageUrlsController);
            this.tabController = tabController;
            this.moduleController = moduleController;
            this.pageUrlsController = pageUrlsController;
			eventLogger.LogDebug("PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},tabController={TabController},moduleController={ModuleController},pageUrlsController={PageUrlsController},templateController={TemplateController},defaultPortalThemeController={DefaultPortalThemeController},cloneModuleExecutionContext={CloneModuleExecutionContext},urlRewriterUtilsWrapper={UrlRewriterUtilsWrapper},friendlyUrlWrapper={FriendlyUrlWrapper},contentVerifier={ContentVerifier},portalController={PortalController},personalizationController={PersonalizationController},permissionDefinitionService={PermissionDefinitionService},portalAliasService={PortalAliasService},hostSettings={HostSettings},hostSettingsService={HostSettingsService}): tabController → {TabController1}", businessControllerProvider, tabController, moduleController, pageUrlsController, templateController, defaultPortalThemeController, cloneModuleExecutionContext, urlRewriterUtilsWrapper, friendlyUrlWrapper, contentVerifier, portalController, personalizationController, permissionDefinitionService, portalAliasService, hostSettings, hostSettingsService, tabController);
			eventLogger.LogDebug("PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},tabController={TabController},moduleController={ModuleController},pageUrlsController={PageUrlsController},templateController={TemplateController},defaultPortalThemeController={DefaultPortalThemeController},cloneModuleExecutionContext={CloneModuleExecutionContext},urlRewriterUtilsWrapper={UrlRewriterUtilsWrapper},friendlyUrlWrapper={FriendlyUrlWrapper},contentVerifier={ContentVerifier},portalController={PortalController},personalizationController={PersonalizationController},permissionDefinitionService={PermissionDefinitionService},portalAliasService={PortalAliasService},hostSettings={HostSettings},hostSettingsService={HostSettingsService}): this.tabController → {Arg16}", businessControllerProvider, tabController, moduleController, pageUrlsController, templateController, defaultPortalThemeController, cloneModuleExecutionContext, urlRewriterUtilsWrapper, friendlyUrlWrapper, contentVerifier, portalController, personalizationController, permissionDefinitionService, portalAliasService, hostSettings, hostSettingsService, this.tabController);
            this.templateController = templateController;
            this.defaultPortalThemeController = defaultPortalThemeController;
            this.cloneModuleExecutionContext = cloneModuleExecutionContext;
			eventLogger.LogDebug("PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},tabController={TabController},moduleController={ModuleController},pageUrlsController={PageUrlsController},templateController={TemplateController},defaultPortalThemeController={DefaultPortalThemeController},cloneModuleExecutionContext={CloneModuleExecutionContext},urlRewriterUtilsWrapper={UrlRewriterUtilsWrapper},friendlyUrlWrapper={FriendlyUrlWrapper},contentVerifier={ContentVerifier},portalController={PortalController},personalizationController={PersonalizationController},permissionDefinitionService={PermissionDefinitionService},portalAliasService={PortalAliasService},hostSettings={HostSettings},hostSettingsService={HostSettingsService}): this.moduleController → {Arg16}", businessControllerProvider, tabController, moduleController, pageUrlsController, templateController, defaultPortalThemeController, cloneModuleExecutionContext, urlRewriterUtilsWrapper, friendlyUrlWrapper, contentVerifier, portalController, personalizationController, permissionDefinitionService, portalAliasService, hostSettings, hostSettingsService, this.moduleController);
            this.urlRewriterUtilsWrapper = urlRewriterUtilsWrapper;
            this.friendlyUrlWrapper = friendlyUrlWrapper;
			eventLogger.LogDebug("PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},tabController={TabController},moduleController={ModuleController},pageUrlsController={PageUrlsController},templateController={TemplateController},defaultPortalThemeController={DefaultPortalThemeController},cloneModuleExecutionContext={CloneModuleExecutionContext},urlRewriterUtilsWrapper={UrlRewriterUtilsWrapper},friendlyUrlWrapper={FriendlyUrlWrapper},contentVerifier={ContentVerifier},portalController={PortalController},personalizationController={PersonalizationController},permissionDefinitionService={PermissionDefinitionService},portalAliasService={PortalAliasService},hostSettings={HostSettings},hostSettingsService={HostSettingsService}): this.pageUrlsController → {Arg16}", businessControllerProvider, tabController, moduleController, pageUrlsController, templateController, defaultPortalThemeController, cloneModuleExecutionContext, urlRewriterUtilsWrapper, friendlyUrlWrapper, contentVerifier, portalController, personalizationController, permissionDefinitionService, portalAliasService, hostSettings, hostSettingsService, this.pageUrlsController);
            this.contentVerifier = contentVerifier;
            this.portalController = portalController;
			eventLogger.LogDebug("PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},tabController={TabController},moduleController={ModuleController},pageUrlsController={PageUrlsController},templateController={TemplateController},defaultPortalThemeController={DefaultPortalThemeController},cloneModuleExecutionContext={CloneModuleExecutionContext},urlRewriterUtilsWrapper={UrlRewriterUtilsWrapper},friendlyUrlWrapper={FriendlyUrlWrapper},contentVerifier={ContentVerifier},portalController={PortalController},personalizationController={PersonalizationController},permissionDefinitionService={PermissionDefinitionService},portalAliasService={PortalAliasService},hostSettings={HostSettings},hostSettingsService={HostSettingsService}): this.templateController → {Arg16}", businessControllerProvider, tabController, moduleController, pageUrlsController, templateController, defaultPortalThemeController, cloneModuleExecutionContext, urlRewriterUtilsWrapper, friendlyUrlWrapper, contentVerifier, portalController, personalizationController, permissionDefinitionService, portalAliasService, hostSettings, hostSettingsService, this.templateController);
            this.personalizationController = personalizationController ?? Globals.GetCurrentServiceProvider().GetRequiredService<PersonalizationController>();
            this.permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
			eventLogger.LogDebug("PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},tabController={TabController},moduleController={ModuleController},pageUrlsController={PageUrlsController},templateController={TemplateController},defaultPortalThemeController={DefaultPortalThemeController},cloneModuleExecutionContext={CloneModuleExecutionContext},urlRewriterUtilsWrapper={UrlRewriterUtilsWrapper},friendlyUrlWrapper={FriendlyUrlWrapper},contentVerifier={ContentVerifier},portalController={PortalController},personalizationController={PersonalizationController},permissionDefinitionService={PermissionDefinitionService},portalAliasService={PortalAliasService},hostSettings={HostSettings},hostSettingsService={HostSettingsService}): this.defaultPortalThemeController → {Arg16}", businessControllerProvider, tabController, moduleController, pageUrlsController, templateController, defaultPortalThemeController, cloneModuleExecutionContext, urlRewriterUtilsWrapper, friendlyUrlWrapper, contentVerifier, portalController, personalizationController, permissionDefinitionService, portalAliasService, hostSettings, hostSettingsService, this.defaultPortalThemeController);
            this.portalAliasService = portalAliasService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			eventLogger.LogDebug("PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},tabController={TabController},moduleController={ModuleController},pageUrlsController={PageUrlsController},templateController={TemplateController},defaultPortalThemeController={DefaultPortalThemeController},cloneModuleExecutionContext={CloneModuleExecutionContext},urlRewriterUtilsWrapper={UrlRewriterUtilsWrapper},friendlyUrlWrapper={FriendlyUrlWrapper},contentVerifier={ContentVerifier},portalController={PortalController},personalizationController={PersonalizationController},permissionDefinitionService={PermissionDefinitionService},portalAliasService={PortalAliasService},hostSettings={HostSettings},hostSettingsService={HostSettingsService}): this.cloneModuleExecutionContext → {Arg16}", businessControllerProvider, tabController, moduleController, pageUrlsController, templateController, defaultPortalThemeController, cloneModuleExecutionContext, urlRewriterUtilsWrapper, friendlyUrlWrapper, contentVerifier, portalController, personalizationController, permissionDefinitionService, portalAliasService, hostSettings, hostSettingsService, this.cloneModuleExecutionContext);
            this.hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
        }
		eventLogger.LogDebug("PagesControllerImpl(businessControllerProvider={BusinessControllerProvider},tabController={TabController},moduleController={ModuleController},pageUrlsController={PageUrlsController},templateController={TemplateController},defaultPortalThemeController={DefaultPortalThemeController},cloneModuleExecutionContext={CloneModuleExecutionContext},urlRewriterUtilsWrapper={UrlRewriterUtilsWrapper},friendlyUrlWrapper={FriendlyUrlWrapper},contentVerifier={ContentVerifier},portalController={PortalController},personalizationController={PersonalizationController},permissionDefinitionService={PermissionDefinitionService},portalAliasService={PortalAliasService},hostSettings={HostSettings},hostSettingsService={HostSettingsService}): this.urlRewriterUtilsWrapper → {Arg16}", businessControllerProvider, tabController, moduleController, pageUrlsController, templateController, defaultPortalThemeController, cloneModuleExecutionContext, urlRewriterUtilsWrapper, friendlyUrlWrapper, contentVerifier, portalController, personalizationController, permissionDefinitionService, portalAliasService, hostSettings, hostSettingsService, this.urlRewriterUtilsWrapper);

        private PortalSettings PortalSettings { get; set; }

        /// <inheritdoc />
        public bool IsValidTabPath(TabInfo tab, string newTabPath, string newTabName, out string errorMessage)
        {
			eventLogger.LogDebug(">>> Entering IsValidTabPath(tab={Tab},newTabPath={NewTabPath},newTabName={NewTabName})", tab, newTabPath, newTabName);
            var portalSettings = this.PortalSettings ?? PortalSettings.Current;
            var valid = true;
			eventLogger.LogDebug("IsValidTabPath(tab={Tab},newTabPath={NewTabPath},newTabName={NewTabName}): valid → {Valid}", tab, newTabPath, newTabName, valid);
            errorMessage = string.Empty;

            // get default culture if the tab's culture is null
            var cultureCode = tab != null ? tab.CultureCode : string.Empty;
			eventLogger.LogDebug("IsValidTabPath(tab={Tab},newTabPath={NewTabPath},newTabName={NewTabName}): cultureCode → {CultureCode}", tab, newTabPath, newTabName, cultureCode);
            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = portalSettings.DefaultLanguage;
            }

			eventLogger.LogDebug("IsValidTabPath(tab={Tab},newTabPath={NewTabPath},newTabName={NewTabName}): errorMessage → {ErrorMessage}", tab, newTabPath, newTabName, errorMessage);
            // Validate Tab Path
            var tabId = TabController.GetTabByTabPath(portalSettings.PortalId, newTabPath, cultureCode);
			eventLogger.LogDebug("IsValidTabPath(tab={Tab},newTabPath={NewTabPath},newTabName={NewTabName}): tabId → {TabId}", tab, newTabPath, newTabName, tabId);
            if (tabId != Null.NullInteger && (tab == null || tabId != tab.TabID))
            {
                var existingTab = this.tabController.GetTab(tabId, portalSettings.PortalId, false);
				eventLogger.LogDebug("IsValidTabPath(tab={Tab},newTabPath={NewTabPath},newTabName={NewTabName}): existingTab → {ExistingTab}", tab, newTabPath, newTabName, existingTab);
                if (existingTab is { IsDeleted: true })
                {
                    errorMessage = Localization.GetString("TabRecycled");
                }
                else
                {
                    errorMessage = Localization.GetString("TabExists");
                }

                valid = false;
            }

            // check whether there is a conflict between tab path and portal alias.
            if (valid && TabController.IsDuplicateWithPortalAlias(portalSettings.PortalId, newTabPath))
            {
                errorMessage = string.Format(CultureInfo.CurrentCulture, Localization.GetString("PathDuplicateWithAlias"), newTabName, newTabPath);
                valid = false;
            }

            if (valid)
            {
                FriendlyUrlController.ValidateUrl(newTabPath.TrimStart('/'), tab?.TabID ?? Null.NullInteger, portalSettings, out var modified);
                if (modified)
                {
                    errorMessage = string.Format(CultureInfo.CurrentCulture, Localization.GetString("PathDuplicateWithPage"), newTabPath);
                    valid = false;
                }
            }

			eventLogger.LogDebug("<<< Exiting IsValidTabPath(tab={Tab},newTabPath={NewTabPath},newTabName={NewTabName})", tab, newTabPath, newTabName);
            return valid;
        }

        /// <inheritdoc />
        public List<int> GetPageHierarchy(int pageId)
        {
			eventLogger.LogDebug(">>> Entering GetPageHierarchy(pageId={PageId})", pageId);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var tab = TabController.Instance.GetTab(pageId, portalSettings.PortalId);
			eventLogger.LogDebug("GetPageHierarchy(pageId={PageId}): tab → {Tab}", pageId, tab);
			eventLogger.LogDebug("GetPageHierarchy(pageId={PageId}): portalSettings → {PortalSettings}", pageId, portalSettings);
            if (tab == null)
            {
				eventLogger.LogWarning("Throwing PageNotFoundException from GetPageHierarchy(pageId={PageId})", pageId);
                throw new PageNotFoundException();
            }

            var paths = new List<int> { tab.TabID, };
            while (tab.ParentId != Null.NullInteger)
            {
                tab = TabController.Instance.GetTab(tab.ParentId, portalSettings.PortalId);
                if (tab != null)
                {
                    paths.Insert(0, tab.TabID);
                }
            }

			eventLogger.LogDebug("<<< Exiting GetPageHierarchy(pageId={PageId})", pageId);
            return paths;
        }

        /// <inheritdoc />
        public TabInfo MovePage(PageMoveRequest request)
        {
			eventLogger.LogDebug(">>> Entering MovePage(request={Request})", request);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var tab = TabController.Instance.GetTab(request.PageId, portalSettings.PortalId);
			eventLogger.LogDebug("MovePage(request={Request}): tab → {Tab}", request, tab);
			eventLogger.LogDebug("MovePage(request={Request}): portalSettings → {PortalSettings}", request, portalSettings);
            if (tab == null)
            {
				eventLogger.LogWarning("Throwing PageNotFoundException from MovePage(request={Request})", request);
                throw new PageNotFoundException();
            }

            if (request.Action == "parent" && tab.ParentId != request.ParentId)
            {
                if (!this.IsValidTabPath(tab, Globals.GenerateTabPath(request.ParentId, tab.TabName), tab.TabName, out var errorMessage))
                {
					eventLogger.LogWarning("Throwing PageException from MovePage(request={Request})", request);
                    throw new PageException(errorMessage);
                }
            }
            else if (request.Action is "before" or "after")
            {
                var relatedTab = TabController.Instance.GetTab(request.RelatedPageId, portalSettings.PortalId);
				eventLogger.LogDebug("MovePage(request={Request}): relatedTab → {RelatedTab}", request, relatedTab);
                if (relatedTab == null)
                {
                    throw new PageNotFoundException();
                }

                if (tab.ParentId != relatedTab.ParentId && !this.IsValidTabPath(tab, Globals.GenerateTabPath(relatedTab.ParentId, tab.TabName), tab.TabName, out var errorMessage))
                {
                    throw new PageException(errorMessage);
                }
            }

            switch (request.Action)
            {
                case "before":
                    TabController.Instance.MoveTabBefore(tab, request.RelatedPageId);
                    break;
                case "after":
                    TabController.Instance.MoveTabAfter(tab, request.RelatedPageId);
                    break;
                case "parent":
                    // avoid move tab into its child page
                    if (IsChild(portalSettings.PortalId, tab.TabID, request.ParentId))
                    {
                        throw new PageException("DragInvalid");
                    }

                    TabController.Instance.MoveTabToParent(tab, request.ParentId);
                    break;
            }

            // as tab's parent may have changed, url need refresh.
			eventLogger.LogDebug("<<< Exiting MovePage(request={Request})", request);
            return TabController.Instance.GetTab(request.PageId, portalSettings.PortalId);
        }

        /// <inheritdoc />
        public void DeletePage(PageItem page, PortalSettings portalSettings = null)
        {
			eventLogger.LogDebug(">>> Entering DeletePage(page={Page},portalSettings={PortalSettings})", page, portalSettings);
            this.DeletePage(page, false, portalSettings);
			eventLogger.LogDebug("<<< Exiting DeletePage(page={Page},portalSettings={PortalSettings})", page, portalSettings);
        }

        /// <inheritdoc />
        public void DeletePage(PageItem page, bool hardDelete, PortalSettings portalSettings = null)
        {
			eventLogger.LogDebug(">>> Entering DeletePage(page={Page},hardDelete={HardDelete},portalSettings={PortalSettings})", page, hardDelete, portalSettings);
            var currentPortal = portalSettings ?? PortalSettings.Current;
            var tab = TabController.Instance.GetTab(page.Id, currentPortal.PortalId);
			eventLogger.LogDebug("DeletePage(page={Page},hardDelete={HardDelete},portalSettings={PortalSettings}): tab → {Tab}", page, hardDelete, portalSettings, tab);
            if (tab == null)
            {
				eventLogger.LogWarning("Throwing PageNotFoundException from DeletePage(page={Page},hardDelete={HardDelete},portalSettings={PortalSettings})", page, hardDelete, portalSettings);
                throw new PageNotFoundException();
            }

            if (TabPermissionController.CanDeletePage(tab))
            {
                if (TabController.IsSpecialTab(tab.TabID, currentPortal.PortalId))
                {
					eventLogger.LogWarning("Throwing PageException from DeletePage(page={Page},hardDelete={HardDelete},portalSettings={PortalSettings})", page, hardDelete, portalSettings);
                    throw new PageException(Localization.GetString("CannotDeleteSpecialPage"));
                }
                else
                {
                    if (this.contentVerifier.IsContentExistsForRequestedPortal(tab.PortalID, currentPortal))
                    {
                        if (hardDelete)
                        {
                            TabController.Instance.DeleteTab(tab.TabID, currentPortal.PortalId);
                        }
                        else
                        {
                            TabController.Instance.SoftDeleteTab(tab.TabID, currentPortal);
                        }
                    }
                    else
                    {
                        throw new PageNotFoundException();
                    }
                }
            }
            else
            {
			eventLogger.LogDebug("<<< Exiting DeletePage(page={Page},hardDelete={HardDelete},portalSettings={PortalSettings})", page, hardDelete, portalSettings);
                throw new PageException(Localization.GetString("NoPermissionDeletePage"));
            }
        }

        /// <inheritdoc />
        public void EditModeForPage(int pageId, int userId)
        {
			eventLogger.LogDebug(">>> Entering EditModeForPage(pageId={PageId},userId={UserId})", pageId, userId);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var newCookie = new HttpCookie("LastPageId", $"{portalSettings.PortalId}:{pageId}")
            {
			eventLogger.LogDebug("EditModeForPage(pageId={PageId},userId={UserId}): portalSettings → {PortalSettings}", pageId, userId, portalSettings);
                Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
            };
            HttpContext.Current.Response.Cookies.Add(newCookie);

            if (Personalization.GetUserMode() != PortalSettings.Mode.Edit)
            {
                var personalization = this.personalizationController.LoadProfile(userId, portalSettings.PortalId);
				eventLogger.LogDebug("EditModeForPage(pageId={PageId},userId={UserId}): personalization → {Personalization}", pageId, userId, personalization);
                personalization.Profile["Usability:UserMode" + portalSettings.PortalId] = "EDIT";
                personalization.IsModified = true;
				eventLogger.LogDebug("EditModeForPage(pageId={PageId},userId={UserId}): personalization.IsModified → {Arg2}", pageId, userId, personalization.IsModified);
                this.personalizationController.SaveProfile(personalization);
            }
			eventLogger.LogDebug("<<< Exiting EditModeForPage(pageId={PageId},userId={UserId})", pageId, userId);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public TabInfo SavePageDetails(PortalSettings settings, PageSettings pageSettings)
        {
			eventLogger.LogDebug(">>> Entering SavePageDetails(settings={Settings},pageSettings={PageSettings})", settings, pageSettings);
            this.PortalSettings = settings ?? PortalSettings.Current;
            TabInfo tab = null;
			eventLogger.LogDebug("SavePageDetails(settings={Settings},pageSettings={PageSettings}): tab → {Tab}", settings, pageSettings, tab);
			eventLogger.LogDebug("SavePageDetails(settings={Settings},pageSettings={PageSettings}): this.PortalSettings → {Arg2}", settings, pageSettings, this.PortalSettings);
            if (pageSettings.TabId > 0)
            {
                tab = TabController.Instance.GetTab(pageSettings.TabId, this.PortalSettings.PortalId);
                if (tab == null)
                {
					eventLogger.LogWarning("Throwing PageNotFoundException from SavePageDetails(settings={Settings},pageSettings={PageSettings})", settings, pageSettings);
                    throw new PageNotFoundException();
                }
            }

            string errorMessage;
            string field;
            if (!this.ValidatePageSettingsData(this.PortalSettings, pageSettings, tab, out field, out errorMessage))
            {
				eventLogger.LogWarning("Throwing PageValidationException from SavePageDetails(settings={Settings},pageSettings={PageSettings})", settings, pageSettings);
                throw new PageValidationException(field, errorMessage);
            }

            var tabId = pageSettings.TabId <= 0
                ? this.AddTab(this.PortalSettings, pageSettings)
                : this.UpdateTab(tab, pageSettings);

			eventLogger.LogDebug("<<< Exiting SavePageDetails(settings={Settings},pageSettings={PageSettings})", settings, pageSettings);
            return TabController.Instance.GetTab(tabId, this.PortalSettings.PortalId);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public IEnumerable<TabInfo> GetPageList(PortalSettings settings, int parentId = -1, string searchKey = "", bool includeHidden = true, bool includeDeleted = false, bool includeSubpages = false)
        {
			eventLogger.LogDebug(">>> Entering GetPageList(settings={Settings},parentId={ParentId},searchKey={SearchKey},includeHidden={IncludeHidden},includeDeleted={IncludeDeleted},includeSubpages={IncludeSubpages})", settings, parentId, searchKey, includeHidden, includeDeleted, includeSubpages);
            var portalSettings = settings ?? PortalController.Instance.GetCurrentSettings();
            var adminTabId = portalSettings.AdminTabId;
			eventLogger.LogDebug("GetPageList(settings={Settings},parentId={ParentId},searchKey={SearchKey},includeHidden={IncludeHidden},includeDeleted={IncludeDeleted},includeSubpages={IncludeSubpages}): portalSettings → {PortalSettings}", settings, parentId, searchKey, includeHidden, includeDeleted, includeSubpages, portalSettings);

            var tabs = TabController.GetPortalTabs(portalSettings.PortalId, adminTabId, false, includeHidden, includeDeleted, true);
			eventLogger.LogDebug("GetPageList(settings={Settings},parentId={ParentId},searchKey={SearchKey},includeHidden={IncludeHidden},includeDeleted={IncludeDeleted},includeSubpages={IncludeSubpages}): tabs → {Tabs}", settings, parentId, searchKey, includeHidden, includeDeleted, includeSubpages, tabs);
            var pages = from t in tabs
                        where (t.ParentId != adminTabId || t.ParentId == Null.NullInteger) &&
                                !t.IsSystem &&
                                    ((string.IsNullOrEmpty(searchKey) && (includeSubpages || t.ParentId == parentId))
                                        || (!string.IsNullOrEmpty(searchKey) &&
                                                (t.TabName.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) > Null.NullInteger
                                                    || t.LocalizedTabName.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) > Null.NullInteger)))
                        select t;

            return includeSubpages ? pages.OrderBy(x => x.ParentId > -1 ? x.ParentId : x.TabID).ThenBy(x => x.TabID) : pages;
        }

        /// <inheritdoc />
        public IEnumerable<TabInfo> GetPageList(PortalSettings portalSettings, bool? deleted, string tabName, string tabTitle, string tabPath, string tabSkin, bool? visible, int parentId, out int total, string searchKey = "", int pageIndex = -1, int pageSize = 10, bool includeSubpages = false)
        {
            pageIndex = pageIndex <= 0 ? 0 : pageIndex;
            pageSize = pageSize > 0 && pageSize <= 100 ? pageSize : 10;
            var tabs = this.GetPageList(portalSettings, parentId, searchKey, true, deleted ?? false, includeSubpages);
            var finalList = new List<TabInfo>();
            if (deleted.HasValue)
            {
                tabs = tabs.Where(tab => tab.IsDeleted == deleted);
            }

            if (visible.HasValue)
            {
                tabs = tabs.Where(tab => tab.IsVisible == visible);
            }

            if (!string.IsNullOrEmpty(tabTitle) || !string.IsNullOrEmpty(tabName) || !string.IsNullOrEmpty(tabPath) ||
                !string.IsNullOrEmpty(tabSkin))
            {
                foreach (var tab in tabs)
                {
                    var bIsMatch = true;
                    if (!string.IsNullOrEmpty(tabTitle))
                    {
                        var tabTitleRegex = RegexUtils.GetCachedRegex(tabTitle.Replace("*", ".*"), RegexOptions.IgnoreCase);
                        bIsMatch &= tabTitleRegex.IsMatch(tab.Title);
                    }

                    if (!string.IsNullOrEmpty(tabName))
                    {
                        var tabNameRegex = RegexUtils.GetCachedRegex(tabName.Replace("*", ".*"), RegexOptions.IgnoreCase);
                        bIsMatch &= tabNameRegex.IsMatch(tab.TabName);
                    }

                    if (!string.IsNullOrEmpty(tabPath))
                    {
                        var tabPathRegex = RegexUtils.GetCachedRegex(tabPath.Replace("*", ".*"), RegexOptions.IgnoreCase);
                        bIsMatch &= tabPathRegex.IsMatch(tab.TabPath);
                    }

                    if (!string.IsNullOrEmpty(tabSkin))
                    {
                        var escapeRegex = RegexUtils.GetCachedRegex("([^\\w^\\*\\s]+)+", RegexOptions.Compiled | RegexOptions.ECMAScript | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        var escapedString = escapeRegex.Replace(tabSkin, @"\$1");
                        var skinSrcRegex = RegexUtils.GetCachedRegex(escapedString.Replace("*", ".*"), RegexOptions.IgnoreCase);
                        bIsMatch &= skinSrcRegex.IsMatch(tab.SkinSrc);
                    }

                    if (bIsMatch)
                    {
                        finalList.Add(tab);
                    }
                }
            }
            else
            {
                finalList.AddRange(tabs);
            }

            total = finalList.Count;
            return finalList.Skip(pageIndex * pageSize).Take(pageSize);
        }

        /// <inheritdoc />
        public IEnumerable<TabInfo> SearchPages(out int totalRecords, string searchKey = "", string pageType = "", string tags = "", string publishStatus = "", string publishDateStart = "", string publishDateEnd = "", int workflowId = -1, int pageIndex = -1, int pageSize = -1)
        {
			eventLogger.LogDebug(">>> Entering SearchPages(searchKey={SearchKey},pageType={PageType},tags={Tags},publishStatus={PublishStatus},publishDateStart={PublishDateStart},publishDateEnd={PublishDateEnd},workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize})", searchKey, pageType, tags, publishStatus, publishDateStart, publishDateEnd, workflowId, pageIndex, pageSize);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var adminTabId = portalSettings.AdminTabId;
			eventLogger.LogDebug("SearchPages(searchKey={SearchKey},pageType={PageType},tags={Tags},publishStatus={PublishStatus},publishDateStart={PublishDateStart},publishDateEnd={PublishDateEnd},workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize}): portalSettings → {PortalSettings}", searchKey, pageType, tags, publishStatus, publishDateStart, publishDateEnd, workflowId, pageIndex, pageSize, portalSettings);

            var tabs = TabController.GetPortalTabs(portalSettings.PortalId, adminTabId, false, true, false, true);
			eventLogger.LogDebug("SearchPages(searchKey={SearchKey},pageType={PageType},tags={Tags},publishStatus={PublishStatus},publishDateStart={PublishDateStart},publishDateEnd={PublishDateEnd},workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize}): tabs → {Tabs}", searchKey, pageType, tags, publishStatus, publishDateStart, publishDateEnd, workflowId, pageIndex, pageSize, tabs);
            var pages = from t in tabs
						eventLogger.LogDebug("SearchPages(searchKey={SearchKey},pageType={PageType},tags={Tags},publishStatus={PublishStatus},publishDateStart={PublishDateStart},publishDateEnd={PublishDateEnd},workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize}): pages → {Pages}", searchKey, pageType, tags, publishStatus, publishDateStart, publishDateEnd, workflowId, pageIndex, pageSize, pages);
                        where (t.ParentId != adminTabId || t.ParentId == Null.NullInteger) &&
                                !t.IsSystem &&
                                    (string.IsNullOrEmpty(searchKey)
                                        || (!string.IsNullOrEmpty(searchKey) &&
                                                (t.TabName.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) > Null.NullInteger
                                                    || t.LocalizedTabName.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) > Null.NullInteger)))
                        select t;

            if (!string.IsNullOrEmpty(pageType))
            {
                pages = pages.Where(p => string.Equals(Globals.GetURLType(p.Url).ToString(), pageType, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(tags))
            {
                pages = pages.Where(p => HasTags(tags, p.Terms));
            }

            PublishStatus status;
            if (Enum.TryParse(publishStatus, true, out status))
            {
                switch (status)
                {
                    case PublishStatus.Published:
                        pages = pages.Where(tab => tab.HasBeenPublished && WorkflowHelper.IsWorkflowCompleted(tab));
                        break;
                    case PublishStatus.Draft:
                        pages = pages.Where(tab => !tab.HasBeenPublished || !WorkflowHelper.IsWorkflowCompleted(tab));
                        break;
                    case PublishStatus.All:
                        break;
                    default:
                        break;
                }
				eventLogger.LogDebug(">>> Entering HasTags(tags={Tags},terms={Terms})", tags, terms);
				eventLogger.LogDebug("<<< Exiting HasTags(tags={Tags},terms={Terms})", tags, terms);
            }

            DateTime startDate;
            if (!string.IsNullOrEmpty(publishDateStart))
            {
                startDate = DateTime.ParseExact(publishDateStart, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                pages = pages.Where(p => WorkflowHelper.GetTabLastPublishedOn(p) >= startDate);
            }
			eventLogger.LogDebug("SearchPages(searchKey={SearchKey},pageType={PageType},tags={Tags},publishStatus={PublishStatus},publishDateStart={PublishDateStart},publishDateEnd={PublishDateEnd},workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize}): startDate → {StartDate}", searchKey, pageType, tags, publishStatus, publishDateStart, publishDateEnd, workflowId, pageIndex, pageSize, startDate);

            DateTime endDate;
            if (!string.IsNullOrEmpty(publishDateEnd))
            {
                endDate = DateTime.ParseExact(publishDateEnd, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                pages = pages.Where(p => WorkflowHelper.GetTabLastPublishedOn(p) <= endDate);
            }
			eventLogger.LogDebug("SearchPages(searchKey={SearchKey},pageType={PageType},tags={Tags},publishStatus={PublishStatus},publishDateStart={PublishDateStart},publishDateEnd={PublishDateEnd},workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize}): endDate → {EndDate}", searchKey, pageType, tags, publishStatus, publishDateStart, publishDateEnd, workflowId, pageIndex, pageSize, endDate);

            if (workflowId != -1)
            {
                pages = pages.Where(p => WorkflowHelper.GetTabWorkflowId(p) == workflowId);
            }

            totalRecords = pages.Count();
			eventLogger.LogDebug("SearchPages(searchKey={SearchKey},pageType={PageType},tags={Tags},publishStatus={PublishStatus},publishDateStart={PublishDateStart},publishDateEnd={PublishDateEnd},workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize}): pageIndex → {PageIndex1}", searchKey, pageType, tags, publishStatus, publishDateStart, publishDateEnd, workflowId, pageIndex, pageSize, pageIndex);
			eventLogger.LogDebug("SearchPages(searchKey={SearchKey},pageType={PageType},tags={Tags},publishStatus={PublishStatus},publishDateStart={PublishDateStart},publishDateEnd={PublishDateEnd},workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize}): totalRecords → {TotalRecords}", searchKey, pageType, tags, publishStatus, publishDateStart, publishDateEnd, workflowId, pageIndex, pageSize, totalRecords);
			eventLogger.LogDebug("<<< Exiting SearchPages(searchKey={SearchKey},pageType={PageType},tags={Tags},publishStatus={PublishStatus},publishDateStart={PublishDateStart},publishDateEnd={PublishDateEnd},workflowId={WorkflowId},pageIndex={PageIndex},pageSize={PageSize})", searchKey, pageType, tags, publishStatus, publishDateStart, publishDateEnd, workflowId, pageIndex, pageSize);
            return pageIndex == -1 || pageSize == -1 ? pages : pages.Skip(pageIndex * pageSize).Take(pageSize);
        }

        /// <inheritdoc />
        public IEnumerable<ModuleInfo> GetModules(int pageId)
        {
			eventLogger.LogDebug(">>> Entering GetModules(pageId={PageId})", pageId);
            var tabModules = this.moduleController.GetTabModules(pageId);
			eventLogger.LogDebug("GetModules(pageId={PageId}): tabModules → {TabModules}", pageId, tabModules);
			eventLogger.LogDebug("<<< Exiting GetModules(pageId={PageId})", pageId);
            return tabModules.Values.Where(m => !m.IsDeleted);
        }

        public bool ValidatePageUrlSettings(PortalSettings portalSettings, PageSettings pageSettings, TabInfo tab, ref string invalidField, ref string errorMessage)
        {
			eventLogger.LogDebug(">>> Entering ValidatePageUrlSettings(portalSettings={PortalSettings},pageSettings={PageSettings},tab={Tab},invalidField={InvalidField},errorMessage={ErrorMessage})", portalSettings, pageSettings, tab, invalidField, errorMessage);
            var urlPath = pageSettings.Url;

			eventLogger.LogDebug("ValidatePageUrlSettings(portalSettings={PortalSettings},pageSettings={PageSettings},tab={Tab},invalidField={InvalidField},errorMessage={ErrorMessage}): urlPath → {UrlPath}", portalSettings, pageSettings, tab, invalidField, errorMessage, urlPath);
            if (string.IsNullOrEmpty(urlPath))
            {
                return true;
            }

            bool modified;

            // Clean Url
            var options = this.urlRewriterUtilsWrapper.GetExtendOptionsForURLs(portalSettings.PortalId);
			eventLogger.LogDebug("ValidatePageUrlSettings(portalSettings={PortalSettings},pageSettings={PageSettings},tab={Tab},invalidField={InvalidField},errorMessage={ErrorMessage}): options → {Options}", portalSettings, pageSettings, tab, invalidField, errorMessage, options);
            urlPath = GetLocalPath(urlPath);
            urlPath = this.friendlyUrlWrapper.CleanNameForUrl(urlPath, options, out modified);
            if (modified)
            {
                errorMessage = Localization.GetString("UrlPathCleaned");
				eventLogger.LogDebug("ValidatePageUrlSettings(portalSettings={PortalSettings},pageSettings={PageSettings},tab={Tab},invalidField={InvalidField},errorMessage={ErrorMessage}): errorMessage → {ErrorMessage1}", portalSettings, pageSettings, tab, invalidField, errorMessage, errorMessage);
                invalidField = "url";
				eventLogger.LogDebug("ValidatePageUrlSettings(portalSettings={PortalSettings},pageSettings={PageSettings},tab={Tab},invalidField={InvalidField},errorMessage={ErrorMessage}): invalidField → {InvalidField1}", portalSettings, pageSettings, tab, invalidField, errorMessage, invalidField);
                return false;
            }

            // Validate for uniqueness
            this.friendlyUrlWrapper.ValidateUrl(urlPath, tab?.TabID ?? Null.NullInteger, portalSettings, out modified);
            if (modified)
            {
                errorMessage = Localization.GetString("UrlPathNotUnique");
                invalidField = "url";
			eventLogger.LogDebug("<<< Exiting ValidatePageUrlSettings(portalSettings={PortalSettings},pageSettings={PageSettings},tab={Tab},invalidField={InvalidField},errorMessage={ErrorMessage})", portalSettings, pageSettings, tab, invalidField, errorMessage);
                return false;
            }

            return true;
        }

        public virtual int AddTab(PortalSettings settings, PageSettings pageSettings)
        {
			eventLogger.LogDebug(">>> Entering AddTab(settings={Settings},pageSettings={PageSettings})", settings, pageSettings);
            var portalSettings = settings ?? PortalController.Instance.GetCurrentSettings();
            var portalId = portalSettings.PortalId;
			eventLogger.LogDebug("AddTab(settings={Settings},pageSettings={PageSettings}): portalSettings → {PortalSettings}", settings, pageSettings, portalSettings);
            var tab = new TabInfo { PortalID = portalId, ParentId = pageSettings.ParentId ?? Null.NullInteger };
			eventLogger.LogDebug("AddTab(settings={Settings},pageSettings={PageSettings}): tab → {Tab}", settings, pageSettings, tab);
            this.UpdateTabInfoFromPageSettings(tab, pageSettings);

            if (portalSettings.ContentLocalizationEnabled)
            {
                tab.CultureCode = portalSettings.CultureCode;
				eventLogger.LogDebug("AddTab(settings={Settings},pageSettings={PageSettings}): tab.CultureCode → {Arg2}", settings, pageSettings, tab.CultureCode);
            }

            this.SavePagePermissions(tab, pageSettings.Permissions);

            var tabId = this.tabController.AddTab(tab);
			eventLogger.LogDebug("AddTab(settings={Settings},pageSettings={PageSettings}): tabId → {TabId}", settings, pageSettings, tabId);

            this.CreateOrUpdateContentItem(tab);

            this.tabController.UpdateTab(tab);

            tab = this.tabController.GetTab(tabId, portalId);

            this.UpdateTabWorkflowFromPageSettings(tab, pageSettings);

            if (pageSettings.TemplateTabId > 0)
            {
                this.CopyContentFromSourceTab(tab, pageSettings.TemplateTabId, pageSettings.Modules);
            }

            if (pageSettings.TemplateId > 0)
            {
                try
                {
                    this.templateController.CreatePageFromTemplate(pageSettings.TemplateId, tab, portalId);
                }
                catch (PageException)
                {
					eventLogger.LogError("Exception in AddTab(settings={Settings},pageSettings={PageSettings}): {Arg2}", settings, pageSettings, PageException.getMessage());
                    this.tabController.DeleteTab(tab.TabID, portalId);
                    throw;
                }
            }

            this.SaveTabUrl(tab, pageSettings);

            this.MovePageIfNeeded(pageSettings, tab);

            this.tabController.ClearCache(portalId);
			eventLogger.LogDebug("<<< Exiting AddTab(settings={Settings},pageSettings={PageSettings})", settings, pageSettings);
            return tab.TabID;
        }

        public void SaveTabUrl(TabInfo tab, PageSettings pageSettings)
        {
			eventLogger.LogDebug(">>> Entering SaveTabUrl(tab={Tab},pageSettings={PageSettings})", tab, pageSettings);
            if (!pageSettings.CustomUrlEnabled)
            {
                return;
            }

            if (tab.IsSuperTab)
            {
                return;
            }

            var url = pageSettings.Url;
			eventLogger.LogDebug("SaveTabUrl(tab={Tab},pageSettings={PageSettings}): url → {Url}", tab, pageSettings, url);
            var tabUrl = tab.TabUrls.SingleOrDefault(t => t.IsSystem
                                                          && t.HttpStatus == "200"
                                                          && t.SeqNum == 0);

            var portalSettings = PortalSettings.Current;
            if (!string.IsNullOrEmpty(url) && url != "/")
            {
                url = this.CleanTabUrl(url);

                string currentUrl = string.Empty;
				eventLogger.LogDebug("SaveTabUrl(tab={Tab},pageSettings={PageSettings}): currentUrl → {CurrentUrl}", tab, pageSettings, currentUrl);
                var friendlyUrlSettings = new FriendlyUrlSettings(this.portalController, this.hostSettings, this.hostSettingsService, portalSettings.PortalId);
				eventLogger.LogDebug("SaveTabUrl(tab={Tab},pageSettings={PageSettings}): friendlyUrlSettings → {FriendlyUrlSettings}", tab, pageSettings, friendlyUrlSettings);
                if (tab.TabID > -1)
                {
                    IPortalAliasInfo alias = portalSettings.PortalAlias;
                    var baseUrl = Globals.AddHTTP(alias.HttpAlias) + "/Default.aspx?TabId=" + tab.TabID;
					eventLogger.LogDebug("SaveTabUrl(tab={Tab},pageSettings={PageSettings}): baseUrl → {BaseUrl}", tab, pageSettings, baseUrl);
                    var path = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(
                        tab,
                        baseUrl,
                        Globals.glbDefaultPage,
                        alias.HttpAlias,
                        false,
                        friendlyUrlSettings,
                        Guid.Empty);

                    currentUrl = path.Replace(Globals.AddHTTP(alias.HttpAlias), string.Empty);
                }

                if (url == currentUrl)
                {
                    return;
                }

                if (tabUrl == null)
                {
                    // Add new custom url
                    tabUrl = new TabUrlInfo
                    {
                        TabId = tab.TabID,
                        SeqNum = 0,
                        PortalAliasId = -1,
                        PortalAliasUsage = PortalAliasUsageType.Default,
                        QueryString = string.Empty,
                        Url = url,
                        HttpStatus = "200",
                        CultureCode = string.Empty,
                        IsSystem = true,
                    };

                    // Save url
                    this.tabController.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                }
                else
                {
                    // Change the original 200 url to a redirect
                    tabUrl.HttpStatus = "301";
					eventLogger.LogDebug("SaveTabUrl(tab={Tab},pageSettings={PageSettings}): tabUrl.HttpStatus → {Arg2}", tab, pageSettings, tabUrl.HttpStatus);
                    tabUrl.SeqNum = tab.TabUrls.Max(t => t.SeqNum) + 1;
                    this.tabController.SaveTabUrl(tabUrl, portalSettings.PortalId, true);

                    // Add new custom url
                    tabUrl.Url = url;
					eventLogger.LogDebug("SaveTabUrl(tab={Tab},pageSettings={PageSettings}): tabUrl.Url → {Arg2}", tab, pageSettings, tabUrl.Url);
                    tabUrl.HttpStatus = "200";
                    tabUrl.SeqNum = 0;
					eventLogger.LogDebug("SaveTabUrl(tab={Tab},pageSettings={PageSettings}): tabUrl.SeqNum → {Arg2}", tab, pageSettings, tabUrl.SeqNum);
                    this.tabController.SaveTabUrl(tabUrl, portalSettings.PortalId, true);
                }

                // Delete any redirects to the same url
                foreach (var redirecturl in this.tabController.GetTabUrls(tab.TabID, tab.PortalID))
                {
                    if (redirecturl.Url == url && redirecturl.HttpStatus != "200")
                    {
                        this.tabController.DeleteTabUrl(redirecturl, tab.PortalID, true);
                    }
                }
            }
            else
            {
                if (url == "/" && tabUrl != null)
                {
                    this.tabController.DeleteTabUrl(tabUrl, portalSettings.PortalId, true);
                }
            }
			eventLogger.LogDebug("<<< Exiting SaveTabUrl(tab={Tab},pageSettings={PageSettings})", tab, pageSettings);
        }

        /// <inheritdoc />
        public string CleanTabUrl(string url)
        {
			eventLogger.LogDebug(">>> Entering CleanTabUrl(url={Url})", url);
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            var urlPath = url.TrimStart('/');
			eventLogger.LogDebug("CleanTabUrl(url={Url}): urlPath → {UrlPath}", url, urlPath);

            var portalSettings = PortalController.Instance.GetCurrentSettings();
			eventLogger.LogDebug("CleanTabUrl(url={Url}): portalSettings → {PortalSettings}", url, portalSettings);
            var friendlyUrlSettings = new FriendlyUrlSettings(this.portalController, this.hostSettings, this.hostSettingsService, portalSettings.PortalId);
			eventLogger.LogDebug("CleanTabUrl(url={Url}): friendlyUrlSettings → {FriendlyUrlSettings}", url, friendlyUrlSettings);
            urlPath = UrlRewriterUtils.CleanExtension(urlPath, friendlyUrlSettings, string.Empty);

            // Clean Url
            var options = UrlRewriterUtils.ExtendOptionsForCustomURLs(UrlRewriterUtils.GetOptionsFromSettings(friendlyUrlSettings));
			eventLogger.LogDebug("CleanTabUrl(url={Url}): options → {Options}", url, options);
            urlPath = FriendlyUrlController.CleanNameForUrl(urlPath, options, out _);

			eventLogger.LogDebug("<<< Exiting CleanTabUrl(url={Url})", url);
            return '/' + urlPath;
        }

        /// <inheritdoc />
        public void CopyThemeToDescendantPages(int pageId, Theme theme)
        {
			eventLogger.LogDebug(">>> Entering CopyThemeToDescendantPages(pageId={PageId},theme={Theme})", pageId, theme);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var portalId = portalSettings.PortalId;
			eventLogger.LogDebug("CopyThemeToDescendantPages(pageId={PageId},theme={Theme}): portalSettings → {PortalSettings}", pageId, theme, portalSettings);
            var tab = this.tabController.GetTab(pageId, portalId, false);
			eventLogger.LogDebug("CopyThemeToDescendantPages(pageId={PageId},theme={Theme}): tab → {Tab}", pageId, theme, tab);
            if (tab == null)
            {
				eventLogger.LogWarning("Throwing PageNotFoundException from CopyThemeToDescendantPages(pageId={PageId},theme={Theme})", pageId, theme);
                throw new PageNotFoundException();
            }

            TabController.CopyDesignToChildren(tab, theme.SkinSrc, theme.ContainerSrc);
			eventLogger.LogDebug("<<< Exiting CopyThemeToDescendantPages(pageId={PageId},theme={Theme})", pageId, theme);
        }

        /// <inheritdoc />
        public void CopyPermissionsToDescendantPages(int pageId)
        {
			eventLogger.LogDebug(">>> Entering CopyPermissionsToDescendantPages(pageId={PageId})", pageId);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var portalId = portalSettings.PortalId;
			eventLogger.LogDebug("CopyPermissionsToDescendantPages(pageId={PageId}): portalSettings → {PortalSettings}", pageId, portalSettings);
            var tab = this.tabController.GetTab(pageId, portalId, false);
			eventLogger.LogDebug("CopyPermissionsToDescendantPages(pageId={PageId}): tab → {Tab}", pageId, tab);
            if (tab == null)
            {
				eventLogger.LogWarning("Throwing PageNotFoundException from CopyPermissionsToDescendantPages(pageId={PageId})", pageId);
                throw new PageNotFoundException();
            }

            if (!TabPermissionController.CanManagePage(tab) || tab.IsSuperTab)
            {
				eventLogger.LogWarning("Throwing PermissionsNotMetException from CopyPermissionsToDescendantPages(pageId={PageId})", pageId);
                throw new PermissionsNotMetException(tab.TabID, Localization.GetString("CannotCopyPermissionsToDescendantPages"));
            }

            TabController.CopyPermissionsToChildren(tab, tab.TabPermissions);
			eventLogger.LogDebug("<<< Exiting CopyPermissionsToDescendantPages(pageId={PageId})", pageId);
        }

        /// <inheritdoc />
        public IEnumerable<Url> GetPageUrls(int tabId)
        {
			eventLogger.LogDebug(">>> Entering GetPageUrls(tabId={TabId})", tabId);
            var tab = this.GetPageDetails(tabId);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
			eventLogger.LogDebug("GetPageUrls(tabId={TabId}): portalSettings → {PortalSettings}", tabId, portalSettings);
			eventLogger.LogDebug("GetPageUrls(tabId={TabId}): tab → {Tab}", tabId, tab);
            var portalId = portalSettings.PortalId;
			eventLogger.LogDebug("<<< Exiting GetPageUrls(tabId={TabId})", tabId);
            return this.pageUrlsController.GetPageUrls(tab, portalId);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public PageSettings GetPageSettings(int pageId, PortalSettings requestPortalSettings = null)
        {
			eventLogger.LogDebug(">>> Entering GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings})", pageId, requestPortalSettings);
            var tab = this.GetPageDetails(pageId);

			eventLogger.LogDebug("GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings}): tab → {Tab}", pageId, requestPortalSettings, tab);
            var portalSettings = requestPortalSettings ?? this.portalController.GetCurrentPortalSettings();
			eventLogger.LogDebug("GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings}): portalSettings → {PortalSettings}", pageId, requestPortalSettings, portalSettings);

            if (!this.contentVerifier.IsContentExistsForRequestedPortal(tab.PortalID, portalSettings))
            {
				eventLogger.LogWarning("Throwing PageNotFoundException from GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings})", pageId, requestPortalSettings);
                throw new PageNotFoundException();
            }

            var page = Converters.ConvertToPageSettings<PageSettings>(tab);
			eventLogger.LogDebug("GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings}): page → {Page}", pageId, requestPortalSettings, page);
            page.Modules = this.GetModules(page.TabId).Select(Converters.ConvertToModuleItem);
            page.PageUrls = this.GetPageUrls(page.TabId);
            page.Permissions = this.GetPermissionsData(pageId);
            page.SiteAliases = this.GetSiteAliases(portalSettings.PortalId);
            page.PrimaryAliasId = this.GetPrimaryAliasId(portalSettings.PortalId, portalSettings.CultureCode);
            page.Locales = this.GetLocales(portalSettings.PortalId);
            page.HasParent = tab.ParentId > -1;
			eventLogger.LogDebug("GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings}): page.HasParent → {Arg2}", pageId, requestPortalSettings, page.HasParent);

            // icons
            var iconFile = string.IsNullOrEmpty(tab.IconFile) ? null : FileManager.Instance.GetFile(tab.PortalID, tab.IconFileRaw);
			eventLogger.LogDebug("GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings}): iconFile → {IconFile}", pageId, requestPortalSettings, iconFile);
            if (iconFile != null)
            {
                page.IconFile = new FileDto
				eventLogger.LogDebug("GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings}): page.IconFile → {Arg2}", pageId, requestPortalSettings, page.IconFile);
                {
                    fileId = iconFile.FileId,
                    fileName = iconFile.FileName,
                    folderId = iconFile.FolderId,
                    folderPath = iconFile.Folder,
                };
            }

            var iconFileLarge = string.IsNullOrEmpty(tab.IconFileLarge) ? null : FileManager.Instance.GetFile(tab.PortalID, tab.IconFileLargeRaw);
			eventLogger.LogDebug("GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings}): iconFileLarge → {IconFileLarge}", pageId, requestPortalSettings, iconFileLarge);
            if (iconFileLarge != null)
            {
                page.IconFileLarge = new FileDto
				eventLogger.LogDebug("GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings}): page.IconFileLarge → {Arg2}", pageId, requestPortalSettings, page.IconFileLarge);
                {
                    fileId = iconFileLarge.FileId,
                    fileName = iconFileLarge.FileName,
                    folderId = iconFileLarge.FolderId,
                    folderPath = iconFileLarge.Folder,
                };
            }

            page.EnabledVersioning = TabVersionSettings.Instance.IsVersioningEnabled(portalSettings.PortalId, pageId);
            page.WorkflowEnabled = TabWorkflowSettings.Instance.IsWorkflowEnabled(portalSettings.PortalId, pageId);
            page.WorkflowId = WorkflowHelper.GetTabWorkflowId(tab);

            var workflow = WorkflowManager.Instance.GetWorkflow(page.WorkflowId);
			eventLogger.LogDebug("GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings}): workflow → {Workflow}", pageId, requestPortalSettings, workflow);
            page.WorkflowName = workflow?.WorkflowName ?? WorkflowHelper.GetTabWorkflowName(tab);

            var isWorkflowCompleted = WorkflowHelper.IsWorkflowCompleted(tab);
			eventLogger.LogDebug("GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings}): isWorkflowCompleted → {IsWorkflowCompleted}", pageId, requestPortalSettings, isWorkflowCompleted);
            var isWorkflowOnDraft = WorkflowEngine.Instance.IsWorkflowOnDraft(tab);
			eventLogger.LogDebug("GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings}): isWorkflowOnDraft → {IsWorkflowOnDraft}", pageId, requestPortalSettings, isWorkflowOnDraft);

            page.StateId = tab.StateID;
			eventLogger.LogDebug("GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings}): page.StateId → {Arg2}", pageId, requestPortalSettings, page.StateId);
            page.StateName = tab.StateID != Null.NullInteger
                ? WorkflowStateManager.Instance.GetWorkflowState(tab.StateID)?.StateName
                : workflow == null
                    ? null
                    : isWorkflowCompleted
                        ? workflow.LastState?.StateName
                        : workflow.FirstState?.StateName;

            page.HasAVisibleVersion = tab.HasAVisibleVersion;
			eventLogger.LogDebug("GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings}): page.HasAVisibleVersion → {Arg2}", pageId, requestPortalSettings, page.HasAVisibleVersion);
			eventLogger.LogDebug("GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings}): page.HasBeenPublished → {Arg2}", pageId, requestPortalSettings, page.HasBeenPublished);
			eventLogger.LogDebug("GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings}): page.IsWorkflowCompleted → {Arg2}", pageId, requestPortalSettings, page.IsWorkflowCompleted);
			eventLogger.LogDebug("GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings}): page.IsWorkflowOnDraft → {Arg2}", pageId, requestPortalSettings, page.IsWorkflowOnDraft);
			eventLogger.LogDebug("GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings}): page.PublishStatus → {Arg2}", pageId, requestPortalSettings, page.PublishStatus);
            page.HasBeenPublished = tab.HasBeenPublished;
            page.IsWorkflowCompleted = isWorkflowCompleted;
            page.IsWorkflowOnDraft = isWorkflowOnDraft;
            page.PublishStatus = tab.HasBeenPublished && isWorkflowCompleted ? "Published" : "Draft";

			eventLogger.LogDebug("<<< Exiting GetPageSettings(pageId={PageId},requestPortalSettings={RequestPortalSettings})", pageId, requestPortalSettings);
            return page;
        }

        /// <inheritdoc />
        public PageUrlResult CreateCustomUrl(SeoUrl dto)
        {
			eventLogger.LogDebug(">>> Entering CreateCustomUrl(dto={Dto})", dto);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
			eventLogger.LogDebug("CreateCustomUrl(dto={Dto}): portalSettings → {PortalSettings}", dto, portalSettings);
			eventLogger.LogDebug("<<< Exiting CreateCustomUrl(dto={Dto})", dto);
            return this.pageUrlsController.CreateCustomUrl(dto.SaveUrl, this.tabController.GetTab(dto.TabId, portalSettings.PortalId, false));
        }

        /// <inheritdoc />
        public PageUrlResult UpdateCustomUrl(SeoUrl dto)
        {
			eventLogger.LogDebug(">>> Entering UpdateCustomUrl(dto={Dto})", dto);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
			eventLogger.LogDebug("UpdateCustomUrl(dto={Dto}): portalSettings → {PortalSettings}", dto, portalSettings);
			eventLogger.LogDebug("<<< Exiting UpdateCustomUrl(dto={Dto})", dto);
            return this.pageUrlsController.UpdateCustomUrl(dto.SaveUrl, this.tabController.GetTab(dto.TabId, portalSettings.PortalId, false));
        }

        /// <inheritdoc />
        public PageUrlResult DeleteCustomUrl(UrlIdDto dto)
        {
			eventLogger.LogDebug(">>> Entering DeleteCustomUrl(dto={Dto})", dto);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
			eventLogger.LogDebug("DeleteCustomUrl(dto={Dto}): portalSettings → {PortalSettings}", dto, portalSettings);
			eventLogger.LogDebug("<<< Exiting DeleteCustomUrl(dto={Dto})", dto);
            return this.pageUrlsController.DeleteCustomUrl(dto.Id, this.tabController.GetTab(dto.TabId, portalSettings.PortalId, false));
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void CreateOrUpdateContentItem(TabInfo tab)
        {
			eventLogger.LogDebug(">>> Entering CreateOrUpdateContentItem(tab={Tab})", tab);
            var contentController = Util.GetContentController();
            tab.Content = string.IsNullOrEmpty(tab.Title) ? tab.TabName : tab.Title;
			eventLogger.LogDebug("CreateOrUpdateContentItem(tab={Tab}): contentController → {ContentController}", tab, contentController);
			eventLogger.LogDebug("CreateOrUpdateContentItem(tab={Tab}): tab.Indexed → {Arg1}", tab, tab.Indexed);
            tab.Indexed = false;

            if (tab.ContentItemId != Null.NullInteger)
            {
                contentController.UpdateContentItem(tab);
                return;
            }

            var typeController = new ContentTypeController();
			eventLogger.LogDebug("CreateOrUpdateContentItem(tab={Tab}): typeController → {TypeController}", tab, typeController);
            var contentType =
                (from t in typeController.GetContentTypes()
                 where t.ContentType == "Tab"
                 select t).SingleOrDefault();

            if (contentType != null)
            {
                tab.ContentTypeId = contentType.ContentTypeId;
				eventLogger.LogDebug("CreateOrUpdateContentItem(tab={Tab}): tab.ContentTypeId → {Arg1}", tab, tab.ContentTypeId);
            }

            contentController.AddContentItem(tab);
			eventLogger.LogDebug("<<< Exiting CreateOrUpdateContentItem(tab={Tab})", tab);
        }

        public int UpdateTab(TabInfo tab, PageSettings pageSettings)
        {
			eventLogger.LogDebug(">>> Entering UpdateTab(tab={Tab},pageSettings={PageSettings})", tab, pageSettings);
            this.UpdateTabInfoFromPageSettings(tab, pageSettings);

            this.UpdateTabWorkflowFromPageSettings(tab, pageSettings);

            this.SavePagePermissions(tab, pageSettings.Permissions);

            this.tabController.UpdateTab(tab);

            this.CreateOrUpdateContentItem(tab);

            this.SaveTabUrl(tab, pageSettings);

            this.MovePageIfNeeded(pageSettings, tab);

			eventLogger.LogDebug("<<< Exiting UpdateTab(tab={Tab},pageSettings={PageSettings})", tab, pageSettings);
            return tab.TabID;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void SavePagePermissions(TabInfo tab, PagePermissions permissions)
        {
			eventLogger.LogDebug(">>> Entering SavePagePermissions(tab={Tab},permissions={Permissions})", tab, permissions);
            var portalSettings = PortalController.Instance.GetCurrentSettings();

			eventLogger.LogDebug("SavePagePermissions(tab={Tab},permissions={Permissions}): portalSettings → {PortalSettings}", tab, permissions, portalSettings);
            tab.TabPermissions.Clear();

            // add default permissions for administrators if needed
            if (!HasAdminPermissions(permissions))
            {
                // add default permissions
                foreach (var editPermission in this.permissionDefinitionService.GetDefinitionsByTab())
                {
                    var permission = new TabPermissionInfo(editPermission)
                    {
                        AllowAccess = true,
                        RoleName = portalSettings.AdministratorRoleName,
                    };
                    ((IPermissionInfo)permission).RoleId = portalSettings.AdministratorRoleId;
                    tab.TabPermissions.Add(permission);
                }
            }

            // add role permissions
            if (permissions.RolePermissions != null)
            {
                foreach (var rolePermission in permissions.RolePermissions.Where(NoLocked()))
                {
                    if (rolePermission.RoleId.ToString(CultureInfo.InvariantCulture) == Globals.glbRoleAllUsers
                        || rolePermission.RoleId.ToString(CultureInfo.InvariantCulture) == Globals.glbRoleUnauthUser
                        || RoleController.Instance.GetRoleById(portalSettings.PortalId, rolePermission.RoleId) != null)
                    {
                        foreach (var permission in rolePermission.Permissions)
                        {
                            var info = new TabPermissionInfo
                            {
                                AllowAccess = permission.AllowAccess,
                            };
                            ((IPermissionInfo)info).PermissionId = permission.PermissionId;
                            ((IPermissionInfo)info).RoleId = rolePermission.RoleId;
                            ((IPermissionInfo)info).UserId = Null.NullInteger;
                            tab.TabPermissions.Add(info);
                        }
                    }
                }
            }

            // add user permissions
            if (permissions.UserPermissions != null)
            {
                foreach (var userPermission in permissions.UserPermissions)
                {
                    var user = UserController.Instance.GetUserById(portalSettings.PortalId, userPermission.UserId);
					eventLogger.LogDebug("SavePagePermissions(tab={Tab},permissions={Permissions}): user → {User}", tab, permissions, user);
                    if (user != null)
                    {
                        if (!int.TryParse(Globals.glbRoleNothing, out var roleId))
                        {
                            roleId = -4;
							eventLogger.LogDebug("SavePagePermissions(tab={Tab},permissions={Permissions}): roleId → {RoleId}", tab, permissions, roleId);
                        }

                        foreach (var permission in userPermission.Permissions)
                        {
                            var info = new TabPermissionInfo
                            {
                                AllowAccess = permission.AllowAccess,
                            };
                            ((IPermissionInfo)info).PermissionId = permission.PermissionId;
                            ((IPermissionInfo)info).RoleId = roleId;
                            ((IPermissionInfo)info).UserId = userPermission.UserId;
                            tab.TabPermissions.Add(info);
                        }
                    }
                }
            }
			eventLogger.LogDebug("<<< Exiting SavePagePermissions(tab={Tab},permissions={Permissions})", tab, permissions);
        }

        /// <inheritdoc />
        public virtual PageSettings GetDefaultSettings(int pageId = 0)
        {
			eventLogger.LogDebug(">>> Entering GetDefaultSettings(pageId={PageId})", pageId);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var pageSettings = new PageSettings
			eventLogger.LogDebug("GetDefaultSettings(pageId={PageId}): portalSettings → {PortalSettings}", pageId, portalSettings);
            {
                Templates = this.templateController.GetTemplates(),
                Permissions = pageId == 0 ? this.GetPermissionsData(portalSettings.HomeTabId) : this.GetPermissionsData(pageId),
            };

            pageSettings.TemplateId = this.templateController.GetDefaultTemplateId(pageSettings.Templates);

            if (portalSettings.SSLSetup == DotNetNuke.Abstractions.Security.SiteSslSetup.On || (portalSettings.SSLEnabled && portalSettings.SSLEnforced))
            {
                pageSettings.IsSecure = true;
				eventLogger.LogDebug("GetDefaultSettings(pageId={PageId}): pageSettings.IsSecure → {Arg1}", pageId, pageSettings.IsSecure);
            }

            var tabVersionSettings = TabVersionSettings.Instance;
            var tabWorkflowSettings = TabWorkflowSettings.Instance;
            pageSettings.EnabledVersioning = tabVersionSettings.IsVersioningEnabled(portalSettings.PortalId);
            pageSettings.WorkflowEnabled = tabWorkflowSettings.IsWorkflowEnabled(portalSettings.PortalId);
            pageSettings.WorkflowId = tabWorkflowSettings.GetDefaultTabWorkflowId(portalSettings.PortalId);

			eventLogger.LogDebug("<<< Exiting GetDefaultSettings(pageId={PageId})", pageId);
            return pageSettings;
        }

        /// <inheritdoc />
        public PagePermissions GetPermissionsData(int pageId)
        {
			eventLogger.LogDebug(">>> Entering GetPermissionsData(pageId={PageId})", pageId);
            var permissions = new PagePermissions(this.permissionDefinitionService, true);
            if (pageId > 0)
            {
                var portalSettings = PortalController.Instance.GetCurrentSettings();
				eventLogger.LogDebug("GetPermissionsData(pageId={PageId}): portalSettings → {PortalSettings}", pageId, portalSettings);
                var tab = TabController.Instance.GetTab(pageId, portalSettings.PortalId);
				eventLogger.LogDebug("GetPermissionsData(pageId={PageId}): tab → {Tab}", pageId, tab);
                if (tab != null)
                {
                    foreach (IPermissionInfo permission in tab.TabPermissions)
                    {
                        if (permission.UserId != Null.NullInteger)
                        {
                            permissions.AddUserPermission(permission);
                        }
                        else
                        {
                            permissions.AddRolePermission(permission);
                        }
                    }

                    permissions.RolePermissions =
                        permissions.RolePermissions
                            .Select(
                                p =>
                                {
                                    p.RoleName = DotNetNuke.Services.Localization.Localization.LocalizeRole(p.RoleName);
                                    return p;
                                })
                            .OrderByDescending(p => p.Locked)
                            .ThenByDescending(p => p.IsDefault)
                            .ThenBy(p => p.RoleName)
                            .ToList();
                    permissions.UserPermissions = permissions.UserPermissions.OrderBy(p => p.DisplayName).ToList();
                }
            }

			eventLogger.LogDebug("<<< Exiting GetPermissionsData(pageId={PageId})", pageId);
            return permissions;
        }

        /// <inheritdoc />
        public void DeleteTabModule(int pageId, int moduleId)
        {
			eventLogger.LogDebug(">>> Entering DeleteTabModule(pageId={PageId},moduleId={ModuleId})", pageId, moduleId);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var tab = this.tabController.GetTab(pageId, portalSettings.PortalId);
			eventLogger.LogDebug("DeleteTabModule(pageId={PageId},moduleId={ModuleId}): tab → {Tab}", pageId, moduleId, tab);
			eventLogger.LogDebug("DeleteTabModule(pageId={PageId},moduleId={ModuleId}): portalSettings → {PortalSettings}", pageId, moduleId, portalSettings);
            if (tab == null)
            {
				eventLogger.LogWarning("Throwing PageModuleNotFoundException from DeleteTabModule(pageId={PageId},moduleId={ModuleId})", pageId, moduleId);
                throw new PageModuleNotFoundException();
            }

            var tabModule = this.moduleController.GetModule(moduleId, pageId, false);
			eventLogger.LogDebug("DeleteTabModule(pageId={PageId},moduleId={ModuleId}): tabModule → {TabModule}", pageId, moduleId, tabModule);
            if (tabModule == null)
            {
                throw new PageModuleNotFoundException();
            }

            if (!TabPermissionController.CanAddContentToPage(tab))
            {
				eventLogger.LogWarning("Throwing SecurityException from DeleteTabModule(pageId={PageId},moduleId={ModuleId})", pageId, moduleId);
                throw new SecurityException("You do not have permission to delete module on this page");
            }

            this.moduleController.DeleteTabModule(pageId, moduleId, true);
            this.moduleController.ClearCache(pageId);
			eventLogger.LogDebug("<<< Exiting DeleteTabModule(pageId={PageId},moduleId={ModuleId})", pageId, moduleId);
        }

        public void CopyContentFromSourceTab(TabInfo tab, int sourceTabId, IEnumerable<ModuleItem> includedModules)
        {
			eventLogger.LogDebug(">>> Entering CopyContentFromSourceTab(tab={Tab},sourceTabId={SourceTabId},includedModules={IncludedModules})", tab, sourceTabId, includedModules);
            var sourceTab = this.tabController.GetTab(sourceTabId, tab.PortalID);
            if (sourceTab == null || sourceTab.IsDeleted)
            {
                return;
            }

            // Copy Properties
            this.CopySourceTabProperties(tab, sourceTab);

            // Copy Modules
            this.CopyModulesFromSourceTab(tab, sourceTab, includedModules);
			eventLogger.LogDebug("<<< Exiting CopyContentFromSourceTab(tab={Tab},sourceTabId={SourceTabId},includedModules={IncludedModules})", tab, sourceTabId, includedModules);
        }

        protected virtual bool ValidatePageSettingsData(PortalSettings portalSettings, PageSettings pageSettings, TabInfo tab, out string invalidField, out string errorMessage)
        {
			eventLogger.LogDebug(">>> Entering ValidatePageSettingsData(portalSettings={PortalSettings},pageSettings={PageSettings},tab={Tab})", portalSettings, pageSettings, tab);
            errorMessage = string.Empty;
            invalidField = string.Empty;

			eventLogger.LogDebug("ValidatePageSettingsData(portalSettings={PortalSettings},pageSettings={PageSettings},tab={Tab}): errorMessage → {ErrorMessage}", portalSettings, pageSettings, tab, errorMessage);
			eventLogger.LogDebug("ValidatePageSettingsData(portalSettings={PortalSettings},pageSettings={PageSettings},tab={Tab}): invalidField → {InvalidField}", portalSettings, pageSettings, tab, invalidField);
            var isValid = !string.IsNullOrEmpty(pageSettings.Name) && TabController.IsValidTabName(pageSettings.Name, out errorMessage);
			eventLogger.LogDebug("ValidatePageSettingsData(portalSettings={PortalSettings},pageSettings={PageSettings},tab={Tab}): isValid → {IsValid}", portalSettings, pageSettings, tab, isValid);
            if (!isValid)
            {
                invalidField = pageSettings.PageType == "template" ? "templateName" : "name";
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = Localization.GetString("EmptyTabName");
                }
                else if (errorMessage.Equals("InvalidTabName", StringComparison.OrdinalIgnoreCase))
                {
                    errorMessage = string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidTabName"), pageSettings.Name);
                }
                else
                {
                    errorMessage = Localization.GetString(errorMessage);
                }

                return false;
            }

            var parentId = pageSettings.ParentId ?? tab?.ParentId ?? Null.NullInteger;
			eventLogger.LogDebug("ValidatePageSettingsData(portalSettings={PortalSettings},pageSettings={PageSettings},tab={Tab}): parentId → {ParentId}", portalSettings, pageSettings, tab, parentId);

            if (pageSettings.PageType == "template")
            {
                parentId = this.GetTemplateParentId(tab?.PortalID ?? portalSettings.PortalId);
            }

            isValid = this.IsValidTabPath(tab, Globals.GenerateTabPath(parentId, pageSettings.Name), pageSettings.Name, out errorMessage);
            if (!isValid)
            {
                invalidField = pageSettings.PageType == "template" ? "templateName" : "name";
                errorMessage = (pageSettings.PageType == "template" ? "templates_" : string.Empty) + errorMessage;
                return false;
            }

            if (pageSettings.StartDate.HasValue && pageSettings.EndDate.HasValue && pageSettings.StartDate > pageSettings.EndDate)
            {
                errorMessage = Localization.GetString("StartDateAfterEndDate");
                invalidField = "endDate";
                return false;
            }

            switch (pageSettings.PageType)
            {
                case "tab":
                    if (!int.TryParse(pageSettings.ExistingTabRedirection, out var existingTabRedirectionId) || existingTabRedirectionId <= 0)
                    {
                        errorMessage = Localization.GetString("TabToRedirectIsRequired");
                        invalidField = "ExistingTabRedirection";
                        return false;
                    }

                    if (!TabPermissionController.CanViewPage(TabController.Instance.GetTab(
                        existingTabRedirectionId,
                        portalSettings.PortalId)))
                    {
                        errorMessage = Localization.GetString("NoPermissionViewRedirectPage");
                        invalidField = "ExistingTabRedirection";
                        return false;
                    }

                    break;
                case "url":
                    if (string.IsNullOrEmpty(pageSettings.ExternalRedirection))
                    {
                        errorMessage = Localization.GetString("ExternalRedirectionUrlRequired");
                        invalidField = "ExternalRedirection";
                        return false;
                    }

                    break;
                case "file":
                    var fileIdRedirectionId = pageSettings.FileIdRedirection ?? 0;
                    var file = pageSettings.FileIdRedirection != null ? FileManager.Instance.GetFile(pageSettings.FileIdRedirection.Value) : null;
					eventLogger.LogDebug("ValidatePageSettingsData(portalSettings={PortalSettings},pageSettings={PageSettings},tab={Tab}): file → {File}", portalSettings, pageSettings, tab, file);
                    if (fileIdRedirectionId <= 0 || file == null)
                    {
                        errorMessage = Localization.GetString("ValidFileIsRequired");
                        invalidField = "FileIdRedirection";
                        return false;
                    }

                    break;
            }

			eventLogger.LogDebug("<<< Exiting ValidatePageSettingsData(portalSettings={PortalSettings},pageSettings={PageSettings},tab={Tab})", portalSettings, pageSettings, tab);
            return this.ValidatePageUrlSettings(portalSettings, pageSettings, tab, ref invalidField, ref errorMessage);
        }

        protected virtual int GetTemplateParentId(int portalId)
        {
			eventLogger.LogDebug(">>> Entering GetTemplateParentId(portalId={PortalId})", portalId);
			eventLogger.LogDebug("<<< Exiting GetTemplateParentId(portalId={PortalId})", portalId);
            return Null.NullInteger;
        }

        protected virtual void UpdateTabInfoFromPageSettings(TabInfo tab, PageSettings pageSettings)
        {
			eventLogger.LogDebug(">>> Entering UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings})", tab, pageSettings);
            tab.TabName = pageSettings.Name;
            tab.TabPath = Globals.GenerateTabPath(tab.ParentId, tab.TabName);
			eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): tab.TabName → {Arg2}", tab, pageSettings, tab.TabName);
			eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): tab.Title → {Arg2}", tab, pageSettings, tab.Title);
            tab.Title = pageSettings.Title;
            tab.Description = GetTabDescription(pageSettings);
            tab.KeyWords = GetKeyWords(pageSettings);
            tab.IsVisible = pageSettings.IncludeInMenu;
			eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): tab.IsVisible → {Arg2}", tab, pageSettings, tab.IsVisible);
			eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): tab.DisableLink → {Arg2}", tab, pageSettings, tab.DisableLink);
            tab.DisableLink = pageSettings.DisableLink;

            tab.StartDate = pageSettings.StartDate ?? Null.NullDate;
			eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): tab.StartDate → {Arg2}", tab, pageSettings, tab.StartDate);
			eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): tab.EndDate → {Arg2}", tab, pageSettings, tab.EndDate);
            tab.EndDate = pageSettings.EndDate ?? Null.NullDate;

            tab.IsSecure = pageSettings.IsSecure;
			eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): tab.IsSecure → {Arg2}", tab, pageSettings, tab.IsSecure);
            tab.TabSettings["AllowIndex"] = pageSettings.AllowIndex;

            tab.SiteMapPriority = pageSettings.SiteMapPriority;
			eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): tab.SiteMapPriority → {Arg2}", tab, pageSettings, tab.SiteMapPriority);
			eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): tab.PageHeadText → {Arg2}", tab, pageSettings, tab.PageHeadText);
            tab.PageHeadText = pageSettings.PageHeadText;

            tab.PermanentRedirect = pageSettings.PermanentRedirect;
			eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): tab.PermanentRedirect → {Arg2}", tab, pageSettings, tab.PermanentRedirect);
            tab.Url = GetInternalUrl(pageSettings);

            tab.TabSettings["CacheProvider"] = pageSettings.CacheProvider;
            if (pageSettings.CacheProvider != null)
            {
                tab.TabSettings["CacheDuration"] = pageSettings.CacheDuration;
                if (pageSettings.CacheIncludeExclude.HasValue)
                {
                    if (pageSettings.CacheIncludeExclude.Value)
                    {
                        tab.TabSettings["CacheIncludeExclude"] = "1";
                        tab.TabSettings["IncludeVaryBy"] = null;
                        tab.TabSettings["ExcludeVaryBy"] = pageSettings.CacheExcludeVaryBy;
                    }
                    else
                    {
                        tab.TabSettings["CacheIncludeExclude"] = "0";
                        tab.TabSettings["IncludeVaryBy"] = pageSettings.CacheIncludeVaryBy;
                        tab.TabSettings["ExcludeVaryBy"] = null;
                    }

                    tab.TabSettings["MaxVaryByCount"] = pageSettings.CacheMaxVaryByCount;
                }
            }
            else
            {
                tab.TabSettings["CacheDuration"] = null;
                tab.TabSettings["CacheIncludeExclude"] = null;
                tab.TabSettings["IncludeVaryBy"] = null;
                tab.TabSettings["ExcludeVaryBy"] = null;
                tab.TabSettings["MaxVaryByCount"] = null;
            }

            tab.TabSettings["LinkNewWindow"] = pageSettings.LinkNewWindow.ToString();
            tab.TabSettings["CustomStylesheet"] = pageSettings.PageStyleSheet;

            // Tab Skin
            tab.SkinSrc = this.GetSkinSrc(pageSettings);
            tab.ContainerSrc = this.GetContainerSrc(pageSettings);

            if (pageSettings.PageType == "template")
            {
                tab.ParentId = this.GetTemplateParentId(tab.PortalID);
                tab.IsSystem = true;
				eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): tab.IsSystem → {Arg2}", tab, pageSettings, tab.IsSystem);
            }

            tab.Terms.Clear();
            if (!string.IsNullOrEmpty(pageSettings.Tags))
            {
                tab.Terms.Clear();
                var termController = new TermController();
				eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): termController → {TermController}", tab, pageSettings, termController);
                var vocabularyController = Util.GetVocabularyController();
				eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): vocabularyController → {VocabularyController}", tab, pageSettings, vocabularyController);
                var vocabulary =
                    vocabularyController.GetVocabularies()
                        .Cast<Vocabulary>()
                        .Where(v => v.Name == PageTagsVocabulary && v.ScopeId == tab.PortalID)
                        .SingleOrDefault();

                int vocabularyId;
                if (vocabulary == null)
                {
                    var scopeType = Util.GetScopeTypeController().GetScopeTypes().SingleOrDefault(s => s.ScopeType == "Portal");
					eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): scopeType → {ScopeType}", tab, pageSettings, scopeType);
                    if (scopeType == null)
                    {
						eventLogger.LogWarning("Throwing ScopeNotFoundException from UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings})", tab, pageSettings);
                        throw new ScopeNotFoundException("Can't create default vocabulary as scope type 'Portal' can't be found.");
                    }

                    vocabularyId = vocabularyController.AddVocabulary(
                        new Vocabulary(PageTagsVocabulary, string.Empty, VocabularyType.Simple)
                        {
							eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): vocabularyId → {VocabularyId}", tab, pageSettings, vocabularyId);
                            ScopeTypeId = scopeType.ScopeTypeId,
                            ScopeId = tab.PortalID,
                        });
                }
                else
                {
                    vocabularyId = vocabulary.VocabularyId;
                }

                // get all terms info
                var allTerms = new List<Term>();
				eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): allTerms → {AllTerms}", tab, pageSettings, allTerms);
                var vocabularies = from v in vocabularyController.GetVocabularies()
                                   where v.ScopeType.ScopeType == "Portal" && v.ScopeId == tab.PortalID && !v.Name.Equals("Tags", StringComparison.OrdinalIgnoreCase)
                                   select v;
                foreach (var v in vocabularies)
                {
                    allTerms.AddRange(termController.GetTermsByVocabulary(v.VocabularyId));
                }

                foreach (var tag in pageSettings.Tags.Trim().Split(','))
                {
                    if (!string.IsNullOrEmpty(tag) && tab.Terms.All(t => !t.Name.Equals(tag, StringComparison.OrdinalIgnoreCase)))
                    {
                        var term = allTerms.FirstOrDefault(t => t.Name.Equals(tag, StringComparison.OrdinalIgnoreCase));
						eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): term → {Term}", tab, pageSettings, term);
                        if (term == null)
                        {
                            var termId = termController.AddTerm(new Term(tag, string.Empty, vocabularyId));
							eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): termId → {TermId}", tab, pageSettings, termId);
                            term = termController.GetTerm(termId);
                        }

                        tab.Terms.Add(term);
                    }
                }
            }

            // icons
            if (pageSettings.IconFile != null && pageSettings.IconFile.fileId > 0)
            {
                tab.IconFile = FileManager.Instance.GetFile(pageSettings.IconFile.fileId).RelativePath;
            }
            else
            {
                tab.IconFile = null;
				eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): tab.IconFile → {Arg2}", tab, pageSettings, tab.IconFile);
            }

            if (pageSettings.IconFileLarge is { fileId: > 0 })
            {
                tab.IconFileLarge = FileManager.Instance.GetFile(pageSettings.IconFileLarge.fileId).RelativePath;
            }
            else
            {
                tab.IconFileLarge = null;
				eventLogger.LogDebug("UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings}): tab.IconFileLarge → {Arg2}", tab, pageSettings, tab.IconFileLarge);
            }
			eventLogger.LogDebug("<<< Exiting UpdateTabInfoFromPageSettings(tab={Tab},pageSettings={PageSettings})", tab, pageSettings);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected void UpdateTabWorkflowFromPageSettings(TabInfo tab, PageSettings pageSettings)
        {
			eventLogger.LogDebug(">>> Entering UpdateTabWorkflowFromPageSettings(tab={Tab},pageSettings={PageSettings})", tab, pageSettings);
            var tabVersionSettings = TabVersionSettings.Instance;
            var tabWorkflowSettings = TabWorkflowSettings.Instance;

            if (pageSettings.EnabledVersioning.HasValue && tabVersionSettings.IsVersioningEnabled(tab.PortalID))
            {
                tabVersionSettings.SetEnabledVersioningForTab(tab.TabID, pageSettings.EnabledVersioning.Value);
            }

            if (pageSettings.WorkflowEnabled.HasValue && tabWorkflowSettings.IsWorkflowEnabled(tab.PortalID))
            {
                tabWorkflowSettings.SetWorkflowEnabled(tab.PortalID, tab.TabID, pageSettings.WorkflowEnabled.Value);
            }

            if (tabVersionSettings.IsVersioningEnabled(tab.PortalID, tab.TabID)
                && tabWorkflowSettings.IsWorkflowEnabled(tab.PortalID, tab.TabID))
            {
                ChangeContentWorkflow(tab, pageSettings);
            }
            else
            {
                tab.StateID = Null.NullInteger;
				eventLogger.LogDebug("UpdateTabWorkflowFromPageSettings(tab={Tab},pageSettings={PageSettings}): tab.StateID → {Arg2}", tab, pageSettings, tab.StateID);
            }
			eventLogger.LogDebug("<<< Exiting UpdateTabWorkflowFromPageSettings(tab={Tab},pageSettings={PageSettings})", tab, pageSettings);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected IOrderedEnumerable<KeyValuePair<int, string>> GetLocales(int portalId)
        {
            var locales = new Lazy<Dictionary<string, Locale>>(() => LocaleController.Instance.GetLocales(portalId));
            return locales.Value.Values.Select(local => new KeyValuePair<int, string>(local.KeyID, local.EnglishName)).OrderBy(x => x.Value);
        }

        protected IEnumerable<KeyValuePair<int, string>> GetSiteAliases(int portalId)
        {
            var aliases = this.portalAliasService.GetPortalAliasesByPortalId(portalId);
            return aliases.Select(alias => new KeyValuePair<int, string>(alias.PortalAliasId, alias.HttpAlias)).OrderBy(x => x.Value);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected int? GetPrimaryAliasId(int portalId, string cultureCode)
        {
			eventLogger.LogDebug(">>> Entering GetPrimaryAliasId(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
			eventLogger.LogDebug("<<< Exiting GetPrimaryAliasId(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            return this.portalAliasService.GetPortalAliasesByPortalId(portalId)
                .Where(a => a.IsPrimary && (a.CultureCode == cultureCode || string.IsNullOrEmpty(a.CultureCode)))
                .OrderByDescending(a => a.CultureCode)
                .Select(a => (int?)a.PortalAliasId)
                .FirstOrDefault();
        }

        private static string GetExternalUrlRedirection(string url)
        {
			eventLogger.LogDebug(">>> Entering GetExternalUrlRedirection(url={Url})", url);
            if (url == null)
            {
                return null;
            }

            if (url.Equals("http://", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            if (url.StartsWith("//", StringComparison.Ordinal))
            {
                return url;
            }

            if (url.IndexOf("://", StringComparison.Ordinal) != -1)
            {
			eventLogger.LogDebug("<<< Exiting GetExternalUrlRedirection(url={Url})", url);
                return Globals.AddHTTP(url);
            }

            return url;
        }

        private static Func<RolePermission, bool> NoLocked()
        {
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            return r => !(r.Locked && r.RoleId != portalSettings.AdministratorRoleId);
        }

        private static bool HasAdminPermissions(PagePermissions permissions)
        {
			eventLogger.LogDebug(">>> Entering HasAdminPermissions(permissions={Permissions})", permissions);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
			eventLogger.LogDebug("HasAdminPermissions(permissions={Permissions}): portalSettings → {PortalSettings}", permissions, portalSettings);
			eventLogger.LogDebug("<<< Exiting HasAdminPermissions(permissions={Permissions})", permissions);
            return permissions.RolePermissions != null && (bool)permissions.RolePermissions?.Any(permission =>
                permission.RoleId == portalSettings.AdministratorRoleId &&
                permission.Permissions.Count != 0);
        }

        private static bool HasTags(string tags, IEnumerable<Term> terms)
        {
			eventLogger.LogDebug(">>> Entering HasTags(tags={Tags},terms={Terms})", tags, terms);
			eventLogger.LogDebug("<<< Exiting HasTags(tags={Tags},terms={Terms})", tags, terms);
            return tags.Split(',').All(tag => terms.Any(t => string.Equals(t.Name, tag, StringComparison.OrdinalIgnoreCase)));
        }

        private static void ChangeContentWorkflow(TabInfo tab, PageSettings pageSettings)
        {
			eventLogger.LogDebug(">>> Entering ChangeContentWorkflow(tab={Tab},pageSettings={PageSettings})", tab, pageSettings);
            var currentState = WorkflowStateRepository.Instance.GetWorkflowStateByID(tab.StateID);
            if (pageSettings.WorkflowId == currentState?.WorkflowID)
            {
                return;
            }

            var newWorkflow = WorkflowManager.Instance.GetWorkflow(pageSettings.WorkflowId);
			eventLogger.LogDebug("ChangeContentWorkflow(tab={Tab},pageSettings={PageSettings}): newWorkflow → {NewWorkflow}", tab, pageSettings, newWorkflow);

            // Can't find workflow. This is not expected.
            if (newWorkflow == null)
            {
                return;
            }

            // Change to new workflow
            tab.StateID = WorkflowEngine.Instance.IsWorkflowCompleted(tab.ContentItemId)
                ? newWorkflow.LastState.StateID // Workflow is completed, just change to last state ("Published") of new workflow
                : newWorkflow.FirstState.StateID; // Workflow bot completed, just change to first state ("Draft") of new workflow
				eventLogger.LogDebug("<<< Exiting ChangeContentWorkflow(tab={Tab},pageSettings={PageSettings})", tab, pageSettings);
        }

        private static bool IsChild(int portalId, int tabId, int parentId)
        {
			eventLogger.LogDebug(">>> Entering IsChild(portalId={PortalId},tabId={TabId},parentId={ParentId})", portalId, tabId, parentId);
            if (parentId == Null.NullInteger)
            {
                return false;
            }

            if (tabId == parentId)
            {
                return true;
            }

            var tab = TabController.Instance.GetTab(parentId, portalId);
			eventLogger.LogDebug("IsChild(portalId={PortalId},tabId={TabId},parentId={ParentId}): tab → {Tab}", portalId, tabId, parentId, tab);
            while (tab != null && tab.ParentId != Null.NullInteger)
            {
                if (tab.ParentId == tabId)
                {
                    return true;
                }

                tab = TabController.Instance.GetTab(tab.ParentId, portalId);
            }

			eventLogger.LogDebug("<<< Exiting IsChild(portalId={PortalId},tabId={TabId},parentId={ParentId})", portalId, tabId, parentId);
            return false;
        }

        private static string GetInternalUrl(PageSettings pageSettings)
        {
			eventLogger.LogDebug(">>> Entering GetInternalUrl(pageSettings={PageSettings})", pageSettings);
            switch (pageSettings.PageType)
            {
                case "tab":
                    return pageSettings.ExistingTabRedirection;
                case "url":
                    return GetExternalUrlRedirection(pageSettings.ExternalRedirection);
                case "file":
                    return pageSettings.FileIdRedirection.HasValue ? "FileId=" + pageSettings.FileIdRedirection : null;
                default:
					eventLogger.LogDebug("<<< Exiting GetInternalUrl(pageSettings={PageSettings})", pageSettings);
                    return null;
            }
        }

        /// <summary>
        /// If the tab description is equal to the portal description
        /// we store null so the system will serve the portal description instead.
        /// </summary>
        /// <param name="pageSettings">The page settings.</param>
        /// <returns>Tab Description value to be stored.</returns>
        private static string GetTabDescription(PageSettings pageSettings)
        {
			eventLogger.LogDebug(">>> Entering GetTabDescription(pageSettings={PageSettings})", pageSettings);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
			eventLogger.LogDebug("GetTabDescription(pageSettings={PageSettings}): portalSettings → {PortalSettings}", pageSettings, portalSettings);
			eventLogger.LogDebug("<<< Exiting GetTabDescription(pageSettings={PageSettings})", pageSettings);
            return pageSettings.Description != portalSettings.Description
                ? pageSettings.Description : null;
        }

        /// <summary>
        /// If the tab keywords is equal to the portal keywords
        /// we store null so the system will serve the portal keywords instead.
        /// </summary>
        /// <param name="pageSettings">The page settings.</param>
        /// <returns>Tab Keywords value to be stored.</returns>
        private static string GetKeyWords(PageSettings pageSettings)
        {
			eventLogger.LogDebug(">>> Entering GetKeyWords(pageSettings={PageSettings})", pageSettings);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
			eventLogger.LogDebug("GetKeyWords(pageSettings={PageSettings}): portalSettings → {PortalSettings}", pageSettings, portalSettings);
			eventLogger.LogDebug("<<< Exiting GetKeyWords(pageSettings={PageSettings})", pageSettings);
            return pageSettings.Keywords != portalSettings.KeyWords
                ? pageSettings.Keywords : null;
        }

        private static string GetLocalPath(string url)
        {
			eventLogger.LogDebug(">>> Entering GetLocalPath(url={Url})", url);
            url = url.TrimEnd('/');
            if (url.Length > 1 && url.IndexOf('/') > -1)
            {
                url = url.Remove(0, url.LastIndexOf('/'));
            }

			eventLogger.LogDebug("<<< Exiting GetLocalPath(url={Url})", url);
            return url;
        }

        private TabInfo GetPageDetails(int pageId)
        {
			eventLogger.LogDebug(">>> Entering GetPageDetails(pageId={PageId})", pageId);
            var portalSettings = this.portalController.GetCurrentSettings();
            var tab = this.tabController.GetTab(pageId, portalSettings.PortalId);
			eventLogger.LogDebug("GetPageDetails(pageId={PageId}): tab → {Tab}", pageId, tab);
			eventLogger.LogDebug("GetPageDetails(pageId={PageId}): portalSettings → {PortalSettings}", pageId, portalSettings);
            if (tab == null)
            {
				eventLogger.LogWarning("Throwing PageNotFoundException from GetPageDetails(pageId={PageId})", pageId);
			eventLogger.LogDebug("<<< Exiting GetPageDetails(pageId={PageId})", pageId);
                throw new PageNotFoundException();
            }

            return tab;
        }

        private void MovePageIfNeeded(PageSettings pageSettings, TabInfo tab)
        {
			eventLogger.LogDebug(">>> Entering MovePageIfNeeded(pageSettings={PageSettings},tab={Tab})", pageSettings, tab);
            if (pageSettings.ParentId.HasValue && pageSettings.ParentId.Value != tab.ParentId)
            {
                var request = new PageMoveRequest
                {
                    Action = "parent",
                    PageId = tab.TabID,
                    ParentId = pageSettings.ParentId.Value,
                };

                this.MovePage(request);
            }
			eventLogger.LogDebug("<<< Exiting MovePageIfNeeded(pageSettings={PageSettings},tab={Tab})", pageSettings, tab);
        }

        private string GetContainerSrc(PageSettings pageSettings)
        {
			eventLogger.LogDebug(">>> Entering GetContainerSrc(pageSettings={PageSettings})", pageSettings);
            var defaultContainer = this.defaultPortalThemeController.GetDefaultPortalContainer();
            if (pageSettings.ContainerSrc != null &&
                pageSettings.ContainerSrc.Equals(
                    defaultContainer,
                    StringComparison.OrdinalIgnoreCase))
            {
					eventLogger.LogDebug("GetContainerSrc(pageSettings={PageSettings}): defaultContainer → {DefaultContainer}", pageSettings, defaultContainer);
			eventLogger.LogDebug("<<< Exiting GetContainerSrc(pageSettings={PageSettings})", pageSettings);
                return null;
            }

            return pageSettings.ContainerSrc;
        }

        private string GetSkinSrc(PageSettings pageSettings)
        {
			eventLogger.LogDebug(">>> Entering GetSkinSrc(pageSettings={PageSettings})", pageSettings);
            var defaultSkin = this.defaultPortalThemeController.GetDefaultPortalLayout();
            if (pageSettings.SkinSrc != null &&
                pageSettings.SkinSrc.Equals(
                    defaultSkin,
                    StringComparison.OrdinalIgnoreCase))
            {
					eventLogger.LogDebug("GetSkinSrc(pageSettings={PageSettings}): defaultSkin → {DefaultSkin}", pageSettings, defaultSkin);
			eventLogger.LogDebug("<<< Exiting GetSkinSrc(pageSettings={PageSettings})", pageSettings);
                return null;
            }

            return pageSettings.SkinSrc;
        }

        private void CopySourceTabProperties(TabInfo tab, TabInfo sourceTab)
        {
			eventLogger.LogDebug(">>> Entering CopySourceTabProperties(tab={Tab},sourceTab={SourceTab})", tab, sourceTab);
            tab.IconFile = sourceTab.IconFile;
            tab.IconFileLarge = sourceTab.IconFileLarge;
			eventLogger.LogDebug("CopySourceTabProperties(tab={Tab},sourceTab={SourceTab}): tab.IconFileLarge → {Arg2}", tab, sourceTab, tab.IconFileLarge);
			eventLogger.LogDebug("CopySourceTabProperties(tab={Tab},sourceTab={SourceTab}): tab.PageHeadText → {Arg2}", tab, sourceTab, tab.PageHeadText);
			eventLogger.LogDebug("CopySourceTabProperties(tab={Tab},sourceTab={SourceTab}): tab.RefreshInterval → {Arg2}", tab, sourceTab, tab.RefreshInterval);
			eventLogger.LogDebug("CopySourceTabProperties(tab={Tab},sourceTab={SourceTab}): tab.IconFile → {Arg2}", tab, sourceTab, tab.IconFile);
            tab.PageHeadText = sourceTab.PageHeadText;
            tab.RefreshInterval = sourceTab.RefreshInterval;
            this.tabController.UpdateTab(tab);

            // update need tab settings.
            foreach (var key in TabSettingKeys)
            {
                if (sourceTab.TabSettings.ContainsKey(key))
                {
                    this.tabController.UpdateTabSetting(tab.TabID, key, Convert.ToString(sourceTab.TabSettings[key], CultureInfo.InvariantCulture));
                }
            }
			eventLogger.LogDebug("<<< Exiting CopySourceTabProperties(tab={Tab},sourceTab={SourceTab})", tab, sourceTab);
        }

        private void CopyModulesFromSourceTab(TabInfo tab, TabInfo sourceTab, IEnumerable<ModuleItem> includedModules)
        {
			eventLogger.LogDebug(">>> Entering CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules})", tab, sourceTab, includedModules);
            includedModules = includedModules ?? sourceTab.ChildModules.Values.Select(Converters.ConvertToModuleItem);
            foreach (var module in includedModules)
            {
			eventLogger.LogDebug("CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules}): includedModules → {IncludedModules1}", tab, sourceTab, includedModules, includedModules);
                var includedInCopy = module.IncludedInCopy ?? true;
                if (!includedInCopy || !sourceTab.ChildModules.Values.Any(m => !m.IsDeleted && !m.AllTabs && m.ModuleID == module.Id))
                {
                    continue;
                }

                var copyType = module.CopyType ?? ModuleCopyType.Copy;
                var objModule = ModuleController.Instance.GetModule(module.Id, sourceTab.TabID, false);
				eventLogger.LogDebug("CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules}): objModule → {ObjModule}", tab, sourceTab, includedModules, objModule);
                ModuleInfo newModule = null;
				eventLogger.LogDebug("CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules}): newModule → {NewModule}", tab, sourceTab, includedModules, newModule);
                if (objModule != null)
                {
                    // Clone module as it exists in the cache and changes we make will update the cached object
                    newModule = objModule.Clone();
                    newModule.TabID = tab.TabID;
					eventLogger.LogDebug("CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules}): newModule.TabID → {Arg3}", tab, sourceTab, includedModules, newModule.TabID);
					eventLogger.LogDebug("CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules}): newModule.DefaultLanguageGuid → {Arg3}", tab, sourceTab, includedModules, newModule.DefaultLanguageGuid);
					eventLogger.LogDebug("CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules}): newModule.CultureCode → {Arg3}", tab, sourceTab, includedModules, newModule.CultureCode);
					eventLogger.LogDebug("CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules}): newModule.ModuleTitle → {Arg3}", tab, sourceTab, includedModules, newModule.ModuleTitle);
                    newModule.DefaultLanguageGuid = Null.NullGuid;
                    newModule.CultureCode = tab.CultureCode;
                    newModule.ModuleTitle = module.Title;

                    if (copyType != ModuleCopyType.Reference)
                    {
                        newModule.ModuleID = Null.NullInteger;
						eventLogger.LogDebug("CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules}): newModule.ModuleID → {Arg3}", tab, sourceTab, includedModules, newModule.ModuleID);
                        ModuleController.Instance.InitialModulePermission(newModule, newModule.TabID, 0);
                        newModule.InheritViewPermissions = objModule.InheritViewPermissions;
						eventLogger.LogDebug("CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules}): newModule.InheritViewPermissions → {Arg3}", tab, sourceTab, includedModules, newModule.InheritViewPermissions);
                    }

                    newModule.ModuleID = ModuleController.Instance.AddModule(newModule);

                    // copy permissions from source module
                    foreach (ModulePermissionInfo existingPermission in objModule.ModulePermissions)
                    {
                        var newPermission = new ModulePermissionInfo
                            {
                                ModuleID = newModule.ModuleID,
                                PermissionKey = existingPermission.PermissionKey,
                                AllowAccess = existingPermission.AllowAccess,
                            };
                        ((IPermissionInfo)newPermission).PermissionId = ((IPermissionInfo)existingPermission).PermissionId;
                        ((IPermissionInfo)newPermission).RoleId = ((IPermissionInfo)existingPermission).RoleId;
                        ((IPermissionInfo)newPermission).UserId = ((IPermissionInfo)existingPermission).UserId;
                        newModule.ModulePermissions.Add(newPermission, true);
                    }

                    ModulePermissionController.SaveModulePermissions(newModule);

                    if (copyType == ModuleCopyType.Copy)
                    {
                        var controller = this.businessControllerProvider.GetInstance<IPortable>(newModule);
						eventLogger.LogDebug("CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules}): controller → {Controller}", tab, sourceTab, includedModules, controller);
                        if (controller is not null)
                        {
                            try
                            {
                                this.cloneModuleExecutionContext.SetCloneModuleContext(true);
                                var content = controller.ExportModule(module.Id);
								eventLogger.LogDebug("CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules}): content → {Content}", tab, sourceTab, includedModules, content);
                                if (!string.IsNullOrEmpty(content))
                                {
                                    controller.ImportModule(
                                        newModule.ModuleID,
                                        content,
                                        newModule.DesktopModule.Version,
                                        UserController.Instance.GetCurrentUserInfo().UserID);
                                }
                            }
                            finally
                            {
                                this.cloneModuleExecutionContext.SetCloneModuleContext(false);
                            }
                        }
                    }
                }

                if (copyType != ModuleCopyType.Reference && objModule != null)
                {
                    // Make reference copies on secondary language
                    foreach (var m in objModule.LocalizedModules.Values)
                    {
                        if (tab.LocalizedTabs.TryGetValue(m.CultureCode, out var localizedTab))
                        {
                            var newLocalizedModule = m.Clone();
							eventLogger.LogDebug("CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules}): newLocalizedModule → {NewLocalizedModule}", tab, sourceTab, includedModules, newLocalizedModule);
							eventLogger.LogDebug("CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules}): newLocalizedModule.TabID → {Arg3}", tab, sourceTab, includedModules, newLocalizedModule.TabID);
							eventLogger.LogDebug("CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules}): newLocalizedModule.CultureCode → {Arg3}", tab, sourceTab, includedModules, newLocalizedModule.CultureCode);
							eventLogger.LogDebug("CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules}): newLocalizedModule.ModuleTitle → {Arg3}", tab, sourceTab, includedModules, newLocalizedModule.ModuleTitle);
							eventLogger.LogDebug("CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules}): newLocalizedModule.DefaultLanguageGuid → {Arg3}", tab, sourceTab, includedModules, newLocalizedModule.DefaultLanguageGuid);
                            newLocalizedModule.TabID = localizedTab.TabID;
                            newLocalizedModule.CultureCode = localizedTab.CultureCode;
                            newLocalizedModule.ModuleTitle = module.Title;
                            newLocalizedModule.DefaultLanguageGuid = newModule.UniqueId;
                            newLocalizedModule.ModuleID = ModuleController.Instance.AddModule(newLocalizedModule);
                        }
                    }
                }
            }
			eventLogger.LogDebug("<<< Exiting CopyModulesFromSourceTab(tab={Tab},sourceTab={SourceTab},includedModules={IncludedModules})", tab, sourceTab, includedModules);
        }
    }
}
