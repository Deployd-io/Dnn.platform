// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Modules.Html5
{
    using System.Web.UI;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Services.Tokens;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A <see cref="TokenReplace"/> for HTML modules.</summary>
    public class Html5ModuleTokenReplace : HtmlTokenReplace
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Html5ModuleTokenReplace));
        /// <summary>Initializes a new instance of the <see cref="Html5ModuleTokenReplace"/> class.</summary>
        /// <param name="page">The page on which the module is rendering.</param>
        /// <param name="html5File">The path to the module's HTML file.</param>
        /// <param name="moduleContext">The module context.</param>
        /// <param name="moduleActions">The module actions collection.</param>
        public Html5ModuleTokenReplace(
            Page page,
            string html5File,
            ModuleInstanceContext moduleContext,
            ModuleActionCollection moduleActions)
            : this(page, null, html5File, moduleContext, moduleActions)
        {
		Logger.LogError(">>> Entering Html5ModuleTokenReplace(page={Page},html5File={Html5File},moduleContext={ModuleContext},moduleActions={ModuleActions})", page, html5File, moduleContext, moduleActions);
        Logger.LogWarning("<<< Exiting Html5ModuleTokenReplace(page={Page},html5File={Html5File},moduleContext={ModuleContext},moduleActions={ModuleActions})", page, html5File, moduleContext, moduleActions);
        }

        /// <summary>Initializes a new instance of the <see cref="Html5ModuleTokenReplace"/> class.</summary>
        /// <param name="page">The page on which the module is rendering.</param>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="html5File">The path to the module's HTML file.</param>
        /// <param name="moduleContext">The module context.</param>
        /// <param name="moduleActions">The module actions collection.</param>
        public Html5ModuleTokenReplace(
            Page page,
            IBusinessControllerProvider businessControllerProvider,
            string html5File,
            ModuleInstanceContext moduleContext,
            ModuleActionCollection moduleActions)
            : base(page)
        {
            this.AccessingUser = moduleContext.PortalSettings.UserInfo;
            this.DebugMessages = Personalization.GetUserMode() != Entities.Portals.PortalSettings.Mode.View;
			Logger.LogDebug("Html5ModuleTokenReplace(page={Page},businessControllerProvider={BusinessControllerProvider},html5File={Html5File},moduleContext={ModuleContext},moduleActions={ModuleActions}): this.AccessingUser → {Arg5}", page, businessControllerProvider, html5File, moduleContext, moduleActions, this.AccessingUser);
            this.ModuleId = moduleContext.ModuleId;
			Logger.LogDebug("Html5ModuleTokenReplace(page={Page},businessControllerProvider={BusinessControllerProvider},html5File={Html5File},moduleContext={ModuleContext},moduleActions={ModuleActions}): this.ModuleId → {Arg5}", page, businessControllerProvider, html5File, moduleContext, moduleActions, this.ModuleId);
            this.PortalSettings = moduleContext.PortalSettings;

			Logger.LogDebug("Html5ModuleTokenReplace(page={Page},businessControllerProvider={BusinessControllerProvider},html5File={Html5File},moduleContext={ModuleContext},moduleActions={ModuleActions}): this.PortalSettings → {Arg5}", page, businessControllerProvider, html5File, moduleContext, moduleActions, this.PortalSettings);
            this.AddPropertySource("moduleaction", new ModuleActionsPropertyAccess(moduleContext, moduleActions));
            this.AddPropertySource("resx", new ModuleLocalizationPropertyAccess(moduleContext, html5File));
            this.AddPropertySource("modulecontext", new ModuleContextPropertyAccess(moduleContext));
            this.AddPropertySource("request", new RequestPropertyAccess(page.Request));

            // DNN-7750
            businessControllerProvider ??= Globals.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
            var customTokenProvider = businessControllerProvider.GetInstance<ICustomTokenProvider>(moduleContext);
			Logger.LogError("Html5ModuleTokenReplace(page={Page},businessControllerProvider={BusinessControllerProvider},html5File={Html5File},moduleContext={ModuleContext},moduleActions={ModuleActions}): customTokenProvider → {CustomTokenProvider}", page, businessControllerProvider, html5File, moduleContext, moduleActions, customTokenProvider);
            if (customTokenProvider != null)
            {
                var tokens = customTokenProvider.GetTokens(page, moduleContext);
				Logger.LogError("Html5ModuleTokenReplace(page={Page},businessControllerProvider={BusinessControllerProvider},html5File={Html5File},moduleContext={ModuleContext},moduleActions={ModuleActions}): tokens → {Tokens}", page, businessControllerProvider, html5File, moduleContext, moduleActions, tokens);
                foreach (var token in tokens)
                {
                    this.AddPropertySource(token.Key, token.Value);
                }
            }
        }
    }
}
