// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Integration.Executers.Builders
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Tests.Integration.Executers.Dto;
    using TestStack.Dossier;

    public class PageSettingsBuilder : TestDataBuilder<PageSettings, PageSettingsBuilder>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public PageSettingsBuilder()
        {
			Logger.LogDebug(">>> Entering PageSettingsBuilder()");
            this.WithTabId(0);
            this.WithName("RB" + Guid.NewGuid().ToString().Replace("-", string.Empty));
            this.WithPageType(string.Empty);
            this.WithUrl(string.Empty);
            this.WithPermission(new TabPermissions());
			Logger.LogDebug("<<< Exiting PageSettingsBuilder()");
        }

        public PageSettingsBuilder WithTabId(int tabId)
        {
			Logger.LogDebug(">>> Entering WithTabId(tabId={TabId})", tabId);
            this.Set(x => x.tabId, tabId);
			Logger.LogDebug("<<< Exiting WithTabId(tabId={TabId})", tabId);
            return this;
        }

        public PageSettingsBuilder WithPageType(string pageType)
        {
			Logger.LogDebug(">>> Entering WithPageType(pageType={PageType})", pageType);
            this.Set(x => x.pageType, pageType);
			Logger.LogDebug("<<< Exiting WithPageType(pageType={PageType})", pageType);
            return this;
        }

        public PageSettingsBuilder WithName(string name)
        {
            this.Set(x => x.Name, name);
            return this;
        }

        public PageSettingsBuilder WithStartDate(DateTime startDate)
        {
			Logger.LogDebug(">>> Entering WithStartDate(startDate={StartDate})", startDate);
            this.Set(x => x.startDate, startDate);
			Logger.LogDebug("<<< Exiting WithStartDate(startDate={StartDate})", startDate);
            return this;
        }

        public PageSettingsBuilder WithEndDate(DateTime endDate)
        {
			Logger.LogDebug(">>> Entering WithEndDate(endDate={EndDate})", endDate);
            this.Set(x => x.endDate, endDate);
			Logger.LogDebug("<<< Exiting WithEndDate(endDate={EndDate})", endDate);
            return this;
        }

        public PageSettingsBuilder WithKeyWords(string keyWords)
        {
			Logger.LogDebug(">>> Entering WithKeyWords(keyWords={KeyWords})", keyWords);
            this.Set(x => x.keywords, keyWords);
			Logger.LogDebug("<<< Exiting WithKeyWords(keyWords={KeyWords})", keyWords);
            return this;
        }

        public PageSettingsBuilder WithDescription(string description)
        {
			Logger.LogDebug(">>> Entering WithDescription(description={Description})", description);
            this.Set(x => x.Description, description);
			Logger.LogDebug("<<< Exiting WithDescription(description={Description})", description);
            return this;
        }

        public PageSettingsBuilder WithUrl(string url)
        {
			Logger.LogDebug(">>> Entering WithUrl(url={Url})", url);
            this.Set(x => x.Url, url);
			Logger.LogDebug("<<< Exiting WithUrl(url={Url})", url);
            return this;
        }

        public PageSettingsBuilder WithPermission(TabPermissions permissions)
        {
			Logger.LogDebug(">>> Entering WithPermission(permissions={Permissions})", permissions);
            this.Set(x => x.permissions, permissions);
			Logger.LogDebug("<<< Exiting WithPermission(permissions={Permissions})", permissions);
            return this;
        }

        public PageSettingsBuilder WithTemplateTabId(int templateTabId)
        {
			Logger.LogDebug(">>> Entering WithTemplateTabId(templateTabId={TemplateTabId})", templateTabId);
            this.Set(x => x.templateTabId, templateTabId);
			Logger.LogDebug("<<< Exiting WithTemplateTabId(templateTabId={TemplateTabId})", templateTabId);
            return this;
        }

        public PageSettingsBuilder WithCopyModules(IList<CopyModuleItem> modules)
        {
			Logger.LogDebug(">>> Entering WithCopyModules(modules={Modules})", modules);
            this.Set(x => x.modules, modules);

			Logger.LogDebug("<<< Exiting WithCopyModules(modules={Modules})", modules);
            return this;
        }

        public PageSettingsBuilder WithSecure(bool secure)
        {
			Logger.LogDebug(">>> Entering WithSecure(secure={Secure})", secure);
            this.Set(x => x.isSecure, secure);
			Logger.LogDebug("<<< Exiting WithSecure(secure={Secure})", secure);
            return this;
        }

        protected override PageSettings BuildObject()
        {
			Logger.LogDebug(">>> Entering BuildObject()");
            var name = this.GetOrDefault(p => p.Name);
            var pageSettings = new PageSettings
			Logger.LogDebug("BuildObject(): name → {Name}", name);
            {
                ApplyWorkflowToChildren = false,
                created = string.Empty,
                customUrlEnabled = true,
                hasChild = false,
                Hierarchy = string.Empty,
                IncludeInMenu = true,
                isCopy = false,
                isWorkflowCompleted = true,
                isWorkflowPropagationAvailable = false,
                keywords = this.GetOrDefault(p => p.keywords),
                localizedName = string.Empty,
                Name = name,
                pageType = this.GetOrDefault(p => p.pageType),
                tabId = this.GetOrDefault(p => p.tabId),
                tags = string.Empty,
                thumbnail = string.Empty,
                title = string.Empty,
                trackLinks = false,
                type = 0,
                workflowId = this.GetOrDefault(p => p.workflowId),
                Url = this.GetOrDefault(p => p.Url),
                Description = this.GetOrDefault(p => p.Description),
                startDate = this.GetOrDefault(p => p.startDate),
                endDate = this.GetOrDefault(p => p.endDate),
                permissions = this.GetOrDefault(p => p.permissions),
                templateTabId = this.GetOrDefault(p => p.templateTabId),
                modules = this.GetOrDefault(p => p.modules),
                isSecure = this.GetOrDefault(p => p.isSecure),
            };
			Logger.LogDebug("<<< Exiting BuildObject()");
            return pageSettings;
        }
    }
}
