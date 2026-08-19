// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.DDRMenu.DNNCommon;

    public class XsltFunctions
    {
        public bool UserIsInRole(string roleName)
        {
			eventLogger.LogDebug(">>> Entering UserIsInRole(roleName={RoleName})", roleName);
			eventLogger.LogDebug("<<< Exiting UserIsInRole(roleName={RoleName})", roleName);
            return UserController.Instance.GetCurrentUserInfo().IsInRole(roleName);
        }

        public string GetLoginURL()
        {
			eventLogger.LogDebug(">>> Entering GetLoginURL()");
			eventLogger.LogDebug("<<< Exiting GetLoginURL()");
            return DNNAbstract.GetLoginUrl();
        }

        public string GetLoginText()
        {
			eventLogger.LogDebug(">>> Entering GetLoginText()");
            try
            {
                return Localization.GetString(
                    HttpContext.Current.Request.IsAuthenticated ? "Logout" : "Login",
                    DNNContext.Current.ResolveUrl("~/Admin/Skins/" + Localization.LocalResourceDirectory + "/Login.ascx"));
            }
            catch (Exception)
            {
				eventLogger.LogError("Exception in GetLoginText(): {Arg0}", Exception.Message);
				eventLogger.LogDebug("<<< Exiting GetLoginText()");
                return "Login";
            }
        }

        public string GetUserURL()
        {
			eventLogger.LogDebug(">>> Entering GetUserURL()");
			eventLogger.LogDebug("<<< Exiting GetUserURL()");
            return DNNAbstract.GetUserUrl();
        }

        public string GetUserText()
        {
			eventLogger.LogDebug(">>> Entering GetUserText()");
            try
            {
                if (HttpContextSource.Current.Request.IsAuthenticated)
                {
                    return UserController.Instance.GetCurrentUserInfo().DisplayName;
                }
                else
                {
                    var hostControl = (Control)HttpContextSource.Current.Items["DDRMenuHostControl"];
					eventLogger.LogDebug("GetUserText(): hostControl → {HostControl}", hostControl);
                    var resourceFileRoot = hostControl.ResolveUrl($"~/Admin/Skins/{Localization.LocalResourceDirectory}/User.ascx");
					eventLogger.LogDebug("GetUserText(): resourceFileRoot → {ResourceFileRoot}", resourceFileRoot);
                    return Localization.GetString("Register", resourceFileRoot);
                }
            }
            catch (Exception exc)
            {
				eventLogger.LogError("Exception in GetUserText(): {Arg0}", exc.getMessage());
				eventLogger.LogDebug("<<< Exiting GetUserText()");
                return exc.ToString();
            }
        }

        public string EscapeXML(string xml)
        {
			eventLogger.LogDebug(">>> Entering EscapeXML(xml={Xml})", xml);
			eventLogger.LogDebug("<<< Exiting EscapeXML(xml={Xml})", xml);
            return SecurityElement.Escape(xml);
        }

        public string HtmlEncode(string s)
        {
			eventLogger.LogDebug(">>> Entering HtmlEncode(s={S})", s);
			eventLogger.LogDebug("<<< Exiting HtmlEncode(s={S})", s);
            return HttpUtility.HtmlEncode(s);
        }

        public string GetString(string name, string resourceFile)
        {
			eventLogger.LogDebug(">>> Entering GetString(name={Name},resourceFile={ResourceFile})", name, resourceFile);
            var resolver = HttpContext.Current.Items["Resolver"] as PathResolver;
            var resolvedFile = string.Empty;
			eventLogger.LogDebug("GetString(name={Name},resourceFile={ResourceFile}): resolvedFile → {ResolvedFile}", name, resourceFile, resolvedFile);
			eventLogger.LogDebug("GetString(name={Name},resourceFile={ResourceFile}): resolver → {Resolver}", name, resourceFile, resolver);
            if (resolver != null)
            {
                var localFile = resolver.Resolve(
                    resourceFile,
                    PathResolver.RelativeTo.Manifest,
                    PathResolver.RelativeTo.Skin,
                    PathResolver.RelativeTo.Module,
                    PathResolver.RelativeTo.Dnn);
                if (File.Exists(HttpContext.Current.Server.MapPath(localFile)))
                {
                    resolvedFile = localFile;
                }
            }

            if (string.IsNullOrEmpty(resolvedFile))
            {
                resolvedFile = "~/" + resourceFile;
            }

			eventLogger.LogDebug("<<< Exiting GetString(name={Name},resourceFile={ResourceFile})", name, resourceFile);
            return Localization.GetString(name, resolvedFile);
        }

        public bool ActionCommandIs(string commandName, string constName)
        {
			eventLogger.LogDebug(">>> Entering ActionCommandIs(commandName={CommandName},constName={ConstName})", commandName, constName);
            try
            {
                return
                    commandName.Equals(
                        typeof(DotNetNuke.Entities.Modules.Actions.ModuleActionType).InvokeMember(
                            constName, BindingFlags.GetField, null, null, new object[0]));
            }
            catch (MissingFieldException)
            {
				eventLogger.LogError("Exception in ActionCommandIs(commandName={CommandName},constName={ConstName}): {Arg2}", commandName, constName, MissingFieldException.getMessage());
				eventLogger.LogDebug("<<< Exiting ActionCommandIs(commandName={CommandName},constName={ConstName})", commandName, constName);
                return false;
            }
        }
    }
}
