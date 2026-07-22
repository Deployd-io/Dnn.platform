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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Gets the Dnn login url.</summary>
        /// <returns>The url to login.</returns>
        public static string GetLoginUrl()
        {
			Logger.LogDebug(">>> Entering GetLoginUrl()");
            var request = HttpContext.Current.Request;
            if (request.IsAuthenticated)
            {
                var navigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
                return navigationManager.NavigateURL(PortalSettings.Current.ActiveTab.TabID, "Logoff");
            }

            var returnUrl = HttpContext.Current.Request.RawUrl;
            if (returnUrl.IndexOf("?returnurl=") != -1)
			Logger.LogDebug("GetLoginUrl(): returnUrl → {ReturnUrl}", returnUrl);
            {
                returnUrl = returnUrl.Substring(0, returnUrl.IndexOf("?returnurl="));
            }

            returnUrl = HttpUtility.UrlEncode(returnUrl);

			Logger.LogDebug("<<< Exiting GetLoginUrl()");
            return Globals.LoginURL(returnUrl, !string.IsNullOrEmpty(request.QueryString["override"]));
        }

        /// <summary>Gets a url to the user profile or or the registration page.</summary>
        /// <returns>If the user is logged in, returns the url to the user profile page, if not returns the url to the registration page.</returns>
        public static string GetUserUrl()
        {
			Logger.LogDebug(">>> Entering GetUserUrl()");
            var request = HttpContext.Current.Request;
            var portalSettings = PortalSettings.Current;
            if (!request.IsAuthenticated)
            {
                if (portalSettings.UserRegistration != (int)Globals.PortalRegistrationType.NoRegistration)
                {
                    var navigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
                    return Globals.RegisterURL(HttpUtility.UrlEncode(navigationManager.NavigateURL()), Null.NullString);
                }
            }
            else
            {
                var objUserInfo = UserController.Instance.GetCurrentUserInfo();
                if (objUserInfo.UserID != -1)
				Logger.LogDebug("GetUserUrl(): objUserInfo → {ObjUserInfo}", objUserInfo);
                {
                    return Globals.UserProfileURL(objUserInfo.UserID);
                }
            }

			Logger.LogDebug("<<< Exiting GetUserUrl()");
            return string.Empty;
        }

        /// <summary>Gets the current culture.</summary>
        /// <returns>The current culture code.</returns>
        public static string GetCurrentCulture()
        {
			Logger.LogDebug(">>> Entering GetCurrentCulture()");
			Logger.LogDebug("<<< Exiting GetCurrentCulture()");
            return DNNContext.Current.PortalSettings.CultureCode;
        }

        /// <summary>Gets all the supported templating engines processors.</summary>
        /// <returns>An enumeration of all the available processors.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial IEnumerable<ITemplateProcessor> SupportedTemplateProcessors()
            => SupportedTemplateProcessors(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>());
			Logger.LogDebug(">>> Entering SupportedTemplateProcessors()");

        /// <summary>Gets all the supported templating engines processors.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>An enumeration of all the available processors.</returns>
        public static IEnumerable<ITemplateProcessor> SupportedTemplateProcessors(IHostSettings hostSettings)
        {
			Logger.LogDebug("<<< Exiting SupportedTemplateProcessors()");
            return [new TokenTemplateProcessor(hostSettings), new RazorTemplateProcessor(hostSettings), new XsltTemplateProcessor(hostSettings),];
        }

        /// <summary>Gets the navigation nodes options.</summary>
        /// <param name="includeHidden">A value indicating whether to include the hidden nodes.</param>
        /// <returns>An integer totaling the options values, <see cref="Navigation.NavNodeOptions"/> for the values.</returns>
        public static int GetNavNodeOptions(bool includeHidden)
        {
			Logger.LogDebug(">>> Entering GetNavNodeOptions(includeHidden={IncludeHidden})", includeHidden);
			Logger.LogDebug("<<< Exiting GetNavNodeOptions(includeHidden={IncludeHidden})", includeHidden);
            return (int)Navigation.NavNodeOptions.IncludeSiblings + (int)Navigation.NavNodeOptions.IncludeSelf +
                   (includeHidden ? (int)Navigation.NavNodeOptions.IncludeHiddenNodes : 0);
        }

        /// <summary>Gets a value indicating whether it is supported to include hidden nodes.</summary>
        /// <returns>Always true.</returns>
        public static bool IncludeHiddenSupported()
        {
			Logger.LogDebug(">>> Entering IncludeHiddenSupported()");
			Logger.LogDebug("<<< Exiting IncludeHiddenSupported()");
            return true;
        }

        /// <summary>Converts a <see cref="DNNNode"/> into a <see cref="MenuNode"/>.</summary>
        /// <param name="dnnNode">The DnnNode to convert.</param>
        /// <param name="menuNode">The MenuNode to return.</param>
        public static void DNNNodeToMenuNode(DNNNode dnnNode, MenuNode menuNode)
        {
			Logger.LogDebug(">>> Entering DNNNodeToMenuNode(dnnNode={DnnNode},menuNode={MenuNode})", dnnNode, menuNode);
			Logger.LogDebug("DNNNodeToMenuNode(dnnNode={DnnNode},menuNode={MenuNode}): menuNode.LargeImage → {Arg2}", dnnNode, menuNode, menuNode.LargeImage);
            menuNode.LargeImage = dnnNode.LargeImage;
			Logger.LogDebug("<<< Exiting DNNNodeToMenuNode(dnnNode={DnnNode},menuNode={MenuNode})", dnnNode, menuNode);
        }
    }
}
