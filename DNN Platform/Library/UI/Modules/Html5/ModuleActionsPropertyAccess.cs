// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Modules.Html5
{
    using System;

    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Tokens;

    public class ModuleActionsPropertyAccess : JsonPropertyAccess<ModuleActionDto>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly ModuleActionCollection moduleActions;
        private readonly ModuleInstanceContext moduleContext;

        /// <summary>Initializes a new instance of the <see cref="ModuleActionsPropertyAccess"/> class.</summary>
        /// <param name="moduleContext">The module context.</param>
        /// <param name="moduleActions">The module actions.</param>
        public ModuleActionsPropertyAccess(ModuleInstanceContext moduleContext, ModuleActionCollection moduleActions)
        {
			Logger.LogDebug(">>> Entering ModuleActionsPropertyAccess(moduleContext={ModuleContext},moduleActions={ModuleActions})", moduleContext, moduleActions);
            this.moduleContext = moduleContext;
            this.moduleActions = moduleActions;
			Logger.LogDebug("<<< Exiting ModuleActionsPropertyAccess(moduleContext={ModuleContext},moduleActions={ModuleActions})", moduleContext, moduleActions);
			Logger.LogDebug("ModuleActionsPropertyAccess(moduleContext={ModuleContext},moduleActions={ModuleActions}): this.moduleContext → {Arg2}", moduleContext, moduleActions, this.moduleContext);
			Logger.LogDebug("ModuleActionsPropertyAccess(moduleContext={ModuleContext},moduleActions={ModuleActions}): moduleActions → {ModuleActions1}", moduleContext, moduleActions, moduleActions);
			Logger.LogDebug("ModuleActionsPropertyAccess(moduleContext={ModuleContext},moduleActions={ModuleActions}): this.moduleActions → {Arg2}", moduleContext, moduleActions, this.moduleActions);
			Logger.LogDebug("ModuleActionsPropertyAccess(moduleContext={ModuleContext},moduleActions={ModuleActions}): moduleContext → {ModuleContext1}", moduleContext, moduleActions, moduleContext);
        }

        /// <inheritdoc />
        protected override string ProcessToken(ModuleActionDto model, UserInfo accessingUser, Scope accessLevel)
        {
			Logger.LogDebug(">>> Entering ProcessToken(model={Model},accessingUser={AccessingUser},accessLevel={AccessLevel})", model, accessingUser, accessLevel);
            var title = (!string.IsNullOrEmpty(model.TitleKey) && !string.IsNullOrEmpty(model.LocalResourceFile))
                                ? Localization.GetString(model.TitleKey, model.LocalResourceFile)
                                : model.Title;

            SecurityAccessLevel securityAccessLevel = SecurityAccessLevel.View;

			Logger.LogDebug("ProcessToken(model={Model},accessingUser={AccessingUser},accessLevel={AccessLevel}): securityAccessLevel → {SecurityAccessLevel}", model, accessingUser, accessLevel, securityAccessLevel);
            if (!string.IsNullOrEmpty(model.SecurityAccessLevel))
            {
                switch (model.SecurityAccessLevel)
                {
                    case "Edit":
                        securityAccessLevel = SecurityAccessLevel.Edit;
                        break;
                    case "Admin":
                        securityAccessLevel = SecurityAccessLevel.Admin;
                        break;
                    case "Host":
                        securityAccessLevel = SecurityAccessLevel.Host;
                        break;
                    default:
                        securityAccessLevel = SecurityAccessLevel.View;
                        break;
                }
            }

            var moduleAction = new ModuleAction(this.moduleContext.GetNextActionID())
            {
                Title = title,
                Icon = model.Icon,
                Secure = securityAccessLevel,
            };

            if (string.IsNullOrEmpty(model.Script))
            {
                moduleAction.Url = this.moduleContext.EditUrl(model.ControlKey);
            }
            else
            {
                moduleAction.Url =
                    model.Script.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
                        ? model.Script
                        : $"javascript:{model.Script}";
            }

            this.moduleActions.Add(moduleAction);

			Logger.LogDebug("<<< Exiting ProcessToken(model={Model},accessingUser={AccessingUser},accessLevel={AccessLevel})", model, accessingUser, accessLevel);
            return string.Empty;
        }
    }
}
