// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Integration.Executers.Builders
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Tests.Integration.Executers.Dto;
    using TestStack.Dossier;
	using Microsoft.Extensions.Logging;

    public class PageSettingsBuilder : TestDataBuilder<PageSettings, PageSettingsBuilder>
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public PageSettingsBuilder()
        {
			_logger.LogDebug(">>> Entering PageSettingsBuilder()");
            this.WithTabId(0);
            this.WithName("RB" + Guid.NewGuid().ToString().Replace("-", string.Empty));
            this.WithPageType(string.Empty);
            this.WithUrl(string.Empty);
            this.WithPermission(new TabPermissions());
			_logger.LogDebug("<<< Exiting PageSettingsBuilder()");
        }

        public PageSettingsBuilder WithTabId(int tabId)
        {
			_logger.LogDebug(">>> Entering WithTabId(tabId={TabId})", tabId);
            this.Set(x => x.tabId, tabId);
			_logger.LogDebug("<<< Exiting WithTabId(tabId={TabId})", tabId);
            return this;
        }

        public PageSettingsBuilder WithPageType(string pageType)
        {
			_logger.LogDebug(">>> Entering WithPageType(pageType={PageType})", pageType);
            this.Set(x => x.pageType, pageType);
			_logger.LogDebug("<<< Exiting WithPageType(pageType={PageType})", pageType);
            return this;
        }

        public PageSettingsBuilder WithName(string name)
        {
            this.Set(x => x.Name, name);
            return this;
        }

        public PageSettingsBuilder WithStartDate(DateTime startDate)
        {
			_logger.LogDebug(">>> Entering WithStartDate(startDate={StartDate})", startDate);
            this.Set(x => x.startDate, startDate);
			_logger.LogDebug("<<< Exiting WithStartDate(startDate={StartDate})", startDate);
            return this;
        }

        public PageSettingsBuilder WithEndDate(DateTime endDate)
        {
			_logger.LogDebug(">>> Entering WithEndDate(endDate={EndDate})", endDate);
            this.Set(x => x.endDate, endDate);
			_logger.LogDebug("<<< Exiting WithEndDate(endDate={EndDate})", endDate);
            return this;
        }

        public PageSettingsBuilder WithKeyWords(string keyWords)
        {
			_logger.LogDebug(">>> Entering WithKeyWords(keyWords={KeyWords})", keyWords);
            this.Set(x => x.keywords, keyWords);
			_logger.LogDebug("<<< Exiting WithKeyWords(keyWords={KeyWords})", keyWords);
            return this;
        }

        public PageSettingsBuilder WithDescription(string description)
        {
			_logger.LogDebug(">>> Entering WithDescription(description={Description})", description);
            this.Set(x => x.Description, description);
			_logger.LogDebug("<<< Exiting WithDescription(description={Description})", description);
            return this;
        }

        public PageSettingsBuilder WithUrl(string url)
        {
			_logger.LogDebug(">>> Entering WithUrl(url={Url})", url);
            this.Set(x => x.Url, url);
			_logger.LogDebug("<<< Exiting WithUrl(url={Url})", url);
            return this;
        }

        public PageSettingsBuilder WithPermission(TabPermissions permissions)
        {
			_logger.LogDebug(">>> Entering WithPermission(permissions={Permissions})", permissions);
            this.Set(x => x.permissions, permissions);
			_logger.LogDebug("<<< Exiting WithPermission(permissions={Permissions})", permissions);
            return this;
        }

        public PageSettingsBuilder WithTemplateTabId(int templateTabId)
        {
			_logger.LogDebug(">>> Entering WithTemplateTabId(templateTabId={TemplateTabId})", templateTabId);
            this.Set(x => x.templateTabId, templateTabId);
			_logger.LogDebug("<<< Exiting WithTemplateTabId(templateTabId={TemplateTabId})", templateTabId);
            return this;
        }

        public PageSettingsBuilder WithCopyModules(IList<CopyModuleItem> modules)
        {
			_logger.LogDebug(">>> Entering WithCopyModules(modules={Modules})", modules);
            this.Set(x => x.modules, modules);

			_logger.LogDebug("<<< Exiting WithCopyModules(modules={Modules})", modules);
            return this;
        }

        public PageSettingsBuilder WithSecure(bool secure)
        {
			_logger.LogDebug(">>> Entering WithSecure(secure={Secure})", secure);
            this.Set(x => x.isSecure, secure);
			_logger.LogDebug("<<< Exiting WithSecure(secure={Secure})", secure);
            return this;
        }

        protected override PageSettings BuildObject()
        {
			_logger.LogDebug(">>> Entering BuildObject()");
            var name = this.GetOrDefault(p => p.Name);
            var pageSettings = new PageSettings
			_logger.LogDebug("BuildObject(): name → {Name}", name);
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
			_logger.LogDebug("<<< Exiting BuildObject()");
            return pageSettings;
        }
    }
}
