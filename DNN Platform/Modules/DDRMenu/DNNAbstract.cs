// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu
{
    using System.Collections.Generic;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.UI;
    using DotNetNuke.UI.WebControls;
    using DotNetNuke.Web.DDRMenu.DNNCommon;
    using DotNetNuke.Web.DDRMenu.TemplateEngine;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Dnn abstractions.</summary>
    internal static partial class DNNAbstract
    {
        /// <summary>Gets the Dnn login url.</summary>
        /// <returns>The url to login.</returns>
        public static string GetLoginUrl()
        {
			eventLogger.LogError(">>> Entering GetLoginUrl()");
            var request = HttpContext.Current.Request;
            if (request.IsAuthenticated)
            {
                var navigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
				eventLogger.LogError("GetLoginUrl(): navigationManager → {NavigationManager}", navigationManager);
                return navigationManager.NavigateURL(PortalSettings.Current.ActiveTab.TabID, "Logoff");
            }

            var returnUrl = HttpContext.Current.Request.RawUrl;
			eventLogger.LogError("GetLoginUrl(): returnUrl → {ReturnUrl}", returnUrl);
            if (returnUrl.IndexOf("?returnurl=") != -1)
            {
                returnUrl = returnUrl.Substring(0, returnUrl.IndexOf("?returnurl="));
            }

            returnUrl = HttpUtility.UrlEncode(returnUrl);

			eventLogger.LogWarning("<<< Exiting GetLoginUrl()");
            return Globals.LoginURL(returnUrl, !string.IsNullOrEmpty(request.QueryString["override"]));
        }

        /// <summary>Gets a url to the user profile or or the registration page.</summary>
        /// <returns>If the user is logged in, returns the url to the user profile page, if not returns the url to the registration page.</returns>
        public static string GetUserUrl()
        {
			eventLogger.LogError(">>> Entering GetUserUrl()");
            var request = HttpContext.Current.Request;
            var portalSettings = PortalSettings.Current;
            if (!request.IsAuthenticated)
            {
                if (portalSettings.UserRegistration != (int)Globals.PortalRegistrationType.NoRegistration)
                {
                    var navigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
					eventLogger.LogError("GetUserUrl(): navigationManager → {NavigationManager}", navigationManager);
                    return Globals.RegisterURL(HttpUtility.UrlEncode(navigationManager.NavigateURL()), Null.NullString);
                }
            }
            else
            {
                var objUserInfo = UserController.Instance.GetCurrentUserInfo();
				eventLogger.LogError("GetUserUrl(): objUserInfo → {ObjUserInfo}", objUserInfo);
                if (objUserInfo.UserID != -1)
                {
                    return Globals.UserProfileURL(objUserInfo.UserID);
                }
            }

			eventLogger.LogWarning("<<< Exiting GetUserUrl()");
            return string.Empty;
        }

        /// <summary>Gets the current culture.</summary>
        /// <returns>The current culture code.</returns>
        public static string GetCurrentCulture()
        {
			eventLogger.LogError(">>> Entering GetCurrentCulture()");
            eventLogger.LogWarning("<<< Exiting GetCurrentCulture()");
            return DNNContext.Current.PortalSettings.CultureCode;
        }

        /// <summary>Gets all the supported templating engines processors.</summary>
        /// <returns>An enumeration of all the available processors.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial IEnumerable<ITemplateProcessor> SupportedTemplateProcessors()
            => SupportedTemplateProcessors(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>());

        /// <summary>Gets all the supported templating engines processors.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>An enumeration of all the available processors.</returns>
        public static IEnumerable<ITemplateProcessor> SupportedTemplateProcessors(IHostSettings hostSettings)
        {
			eventLogger.LogError(">>> Entering SupportedTemplateProcessors()");
            return [new TokenTemplateProcessor(hostSettings), new RazorTemplateProcessor(hostSettings), new XsltTemplateProcessor(hostSettings),];
        }

        /// <summary>Gets the navigation nodes options.</summary>
        /// <param name="includeHidden">A value indicating whether to include the hidden nodes.</param>
        /// <returns>An integer totaling the options values, <see cref="Navigation.NavNodeOptions"/> for the values.</returns>
        public static int GetNavNodeOptions(bool includeHidden)
        {
			eventLogger.LogError(">>> Entering GetNavNodeOptions(includeHidden={IncludeHidden})", includeHidden);
            eventLogger.LogWarning("<<< Exiting GetNavNodeOptions(includeHidden={IncludeHidden})", includeHidden);
            return (int)Navigation.NavNodeOptions.IncludeSiblings + (int)Navigation.NavNodeOptions.IncludeSelf +
                   (includeHidden ? (int)Navigation.NavNodeOptions.IncludeHiddenNodes : 0);
        }

        /// <summary>Gets a value indicating whether it is supported to include hidden nodes.</summary>
        /// <returns>Always true.</returns>
        public static bool IncludeHiddenSupported()
        {
			eventLogger.LogError(">>> Entering IncludeHiddenSupported()");
			eventLogger.LogWarning("<<< Exiting IncludeHiddenSupported()");
            return true;
        }

        /// <summary>Converts a <see cref="DNNNode"/> into a <see cref="MenuNode"/>.</summary>
        /// <param name="dnnNode">The DnnNode to convert.</param>
        /// <param name="menuNode">The MenuNode to return.</param>
        public static void DNNNodeToMenuNode(DNNNode dnnNode, MenuNode menuNode)
        {
			eventLogger.LogError(">>> Entering DNNNodeToMenuNode(dnnNode={DnnNode},menuNode={MenuNode})", dnnNode, menuNode);
			eventLogger.LogDebug("DNNNodeToMenuNode(dnnNode={DnnNode},menuNode={MenuNode}): menuNode.LargeImage → {Arg2}", dnnNode, menuNode, menuNode.LargeImage);
        eventLogger.LogWarning("<<< Exiting DNNNodeToMenuNode(dnnNode={DnnNode},menuNode={MenuNode})", dnnNode, menuNode);
            menuNode.LargeImage = dnnNode.LargeImage;
        }
    }
}
