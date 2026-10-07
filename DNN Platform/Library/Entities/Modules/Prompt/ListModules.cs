// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Prompt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Prompt;
    using DotNetNuke.Abstractions.Users;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Prompt;
    using DotNetNuke.Security.Permissions;

    /// <summary>This is a (Prompt) Console Command. You should not reference this class directly. It is to be used solely through Prompt.</summary>
    [ConsoleCommand("list-modules", Constants.CommandCategoryKeys.Modules, "Prompt_ListModules_Description")]
    public class ListModules : ConsoleCommand
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ListModules));
        /// <inheritdoc />
        public override string LocalResourceFile => Constants.DefaultPromptResourceFile;

        [ConsoleCommandParameter("pageid", "Prompt_ListModules_FlagPageId")]
        public int? PageId { get; set; } = -1;

        [ConsoleCommandParameter("page", "Prompt_ListModules_FlagPage", "1")]
        public int Page { get; set; } = 1;

        [ConsoleCommandParameter("max", "Prompt_ListModules_FlagMax", "10")]
        public int Max { get; set; } = 10;

        [ConsoleCommandParameter("name", "Prompt_ListModules_FlagModuleName")]
        public string ModuleName { get; set; }

        [ConsoleCommandParameter("title", "Prompt_ListModules_FlagModuleTitle")]
        public string ModuleTitle { get; set; }

        [ConsoleCommandParameter("deleted", "Prompt_ListModules_FlagDeleted")]
        public bool? Deleted { get; set; }

        /// <inheritdoc />
        public override void Initialize(string[] args, IPortalSettings portalSettings, IUserInfo userInfo, int activeTabId)
        {
			Logger.LogError(">>> Entering Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            base.Initialize(args, portalSettings, userInfo, activeTabId);
            this.ParseParameters(this);
			Logger.LogWarning("<<< Exiting Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public override IConsoleResultModel Run()
        {
			Logger.LogError(">>> Entering Run()");
            var max = this.Max <= 0 ? 10 : (this.Max > 500 ? 500 : this.Max);

			Logger.LogError("Run(): max → {Max}", max);
            int total;
            var portalDesktopModules = DesktopModuleController.GetPortalDesktopModules(this.PortalId);
			Logger.LogError("Run(): portalDesktopModules → {PortalDesktopModules}", portalDesktopModules);

            var pageIndex = this.Page > 0 ? this.Page - 1 : 0;
            pageIndex = pageIndex < 0 ? 0 : pageIndex;
			Logger.LogError("Run(): pageIndex → {PageIndex}", pageIndex);
            var pageSize = this.Max;
            pageSize = pageSize > 0 && pageSize <= 100 ? pageSize : 10;
			Logger.LogError("Run(): pageSize → {PageSize}", pageSize);
            this.ModuleName = this.ModuleName?.Replace("*", string.Empty);
            this.ModuleTitle = this.ModuleTitle?.Replace("*", string.Empty);
            var modules = ModuleController.Instance.GetModules(this.PortalSettings.PortalId)
                    .Cast<ModuleInfo>().Where(ModulePermissionController.CanViewModule);
            if (!string.IsNullOrEmpty(this.ModuleName))
            {
                modules = modules.Where(module => module.DesktopModule.ModuleName.IndexOf(this.ModuleName, StringComparison.OrdinalIgnoreCase) >= 0);
				Logger.LogError("Run(): modules → {Modules}", modules);
            }

            if (!string.IsNullOrEmpty(this.ModuleTitle))
            {
                modules = modules.Where(module => module.ModuleTitle.IndexOf(this.ModuleTitle, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // Return only deleted modules with matching criteria.
            if (this.PageId.HasValue && this.PageId.Value > 0)
            {
                modules = modules.Where(x => x.TabID == this.PageId.Value);
            }

            if (this.Deleted.HasValue)
            {
                modules = modules.Where(module => module.IsDeleted == this.Deleted);
            }

            // Get distincts.
            modules = modules.GroupBy(x => x.ModuleID).Select(group => group.First()).OrderBy(x => x.ModuleID);
            var moduleInfos = modules as IList<ModuleInfo> ?? modules.ToList();
			Logger.LogError("Run(): moduleInfos → {ModuleInfos}", moduleInfos);
            total = moduleInfos.Count;
			Logger.LogError("Run(): total → {Total}", total);
            modules = moduleInfos.Skip(pageIndex * pageSize).Take(pageSize)
                .Where(m =>
                {
					Logger.LogDebug("Run(): total → {Total}", total);
                    var moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(m.ModuleDefID);
					Logger.LogError("Run(): moduleDefinition → {ModuleDefinition}", moduleDefinition);
                    return portalDesktopModules.Any(kvp =>
                        kvp.Value.DesktopModuleID == moduleDefinition?.DesktopModuleID);
                });

            var results = modules.Select(x => PromptModuleInfo.FromDnnModuleInfo(x, this.Deleted));
			Logger.LogError("Run(): results → {Results}", results);

            var totalPages = (total / max) + (total % max == 0 ? 0 : 1);
			Logger.LogError("Run(): totalPages → {TotalPages}", totalPages);
            var pageNo = this.Page > 0 ? this.Page : 1;
            var count = results.Count();
			Logger.LogError("Run(): count → {Count}", count);
			Logger.LogWarning("<<< Exiting Run()");
            return new ConsoleResultModel
            {
                Data = results,
                PagingInfo = new PagingInfo
                {
                    PageNo = pageNo,
                    TotalPages = totalPages,
                    TotalRecords = total,
                    PageSize = max,
                },
                Records = count,
                Output = count == 0 ? this.LocalizeString("Prompt_NoModules") : string.Empty,
            };
        }
    }
}
