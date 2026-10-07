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
			eventLogger.LogError(">>> Entering UserIsInRole(roleName={RoleName})", roleName);
            eventLogger.LogWarning("<<< Exiting UserIsInRole(roleName={RoleName})", roleName);
            return UserController.Instance.GetCurrentUserInfo().IsInRole(roleName);
        }

        public string GetLoginURL()
        {
			eventLogger.LogError(">>> Entering GetLoginURL()");
            eventLogger.LogWarning("<<< Exiting GetLoginURL()");
            return DNNAbstract.GetLoginUrl();
        }

        public string GetLoginText()
        {
			eventLogger.LogError(">>> Entering GetLoginText()");
            try
            {
                return Localization.GetString(
                    HttpContext.Current.Request.IsAuthenticated ? "Logout" : "Login",
                    DNNContext.Current.ResolveUrl("~/Admin/Skins/" + Localization.LocalResourceDirectory + "/Login.ascx"));
            }
            catch (Exception)
            {
				eventLogger.LogError("Exception in GetLoginText(): {Arg0}", Exception.Message);
				eventLogger.LogWarning("<<< Exiting GetLoginText()");
                return "Login";
            }
        }

        public string GetUserURL()
        {
			eventLogger.LogError(">>> Entering GetUserURL()");
            eventLogger.LogWarning("<<< Exiting GetUserURL()");
            return DNNAbstract.GetUserUrl();
        }

        public string GetUserText()
        {
			eventLogger.LogError(">>> Entering GetUserText()");
            try
            {
                if (HttpContextSource.Current.Request.IsAuthenticated)
                {
                    return UserController.Instance.GetCurrentUserInfo().DisplayName;
                }
                else
                {
                    var hostControl = (Control)HttpContextSource.Current.Items["DDRMenuHostControl"];
					eventLogger.LogError("GetUserText(): hostControl → {HostControl}", hostControl);
                    var resourceFileRoot = hostControl.ResolveUrl($"~/Admin/Skins/{Localization.LocalResourceDirectory}/User.ascx");
					eventLogger.LogError("GetUserText(): resourceFileRoot → {ResourceFileRoot}", resourceFileRoot);
                    return Localization.GetString("Register", resourceFileRoot);
                }
            }
            catch (Exception exc)
            {
				eventLogger.LogError("Exception in GetUserText(): {Arg0}", exc.Message);
				eventLogger.LogWarning("<<< Exiting GetUserText()");
                return exc.ToString();
            }
        }

        public string EscapeXML(string xml)
        {
			eventLogger.LogError(">>> Entering EscapeXML(xml={Xml})", xml);
            eventLogger.LogWarning("<<< Exiting EscapeXML(xml={Xml})", xml);
            return SecurityElement.Escape(xml);
        }

        public string HtmlEncode(string s)
        {
			eventLogger.LogError(">>> Entering HtmlEncode(s={S})", s);
            eventLogger.LogWarning("<<< Exiting HtmlEncode(s={S})", s);
            return HttpUtility.HtmlEncode(s);
        }

        public string GetString(string name, string resourceFile)
        {
			eventLogger.LogError(">>> Entering GetString(name={Name},resourceFile={ResourceFile})", name, resourceFile);
            var resolver = HttpContext.Current.Items["Resolver"] as PathResolver;
            var resolvedFile = string.Empty;
			eventLogger.LogError("GetString(name={Name},resourceFile={ResourceFile}): resolver → {Resolver}", name, resourceFile, resolver);
            if (resolver != null)
            {
				eventLogger.LogError("GetString(name={Name},resourceFile={ResourceFile}): resolvedFile → {ResolvedFile}", name, resourceFile, resolvedFile);
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

			eventLogger.LogWarning("<<< Exiting GetString(name={Name},resourceFile={ResourceFile})", name, resourceFile);
            return Localization.GetString(name, resolvedFile);
        }

        public bool ActionCommandIs(string commandName, string constName)
        {
			eventLogger.LogError(">>> Entering ActionCommandIs(commandName={CommandName},constName={ConstName})", commandName, constName);
            try
            {
                return
                    commandName.Equals(
                        typeof(DotNetNuke.Entities.Modules.Actions.ModuleActionType).InvokeMember(
                            constName, BindingFlags.GetField, null, null, new object[0]));
            }
            catch (MissingFieldException)
            {
				eventLogger.LogError("Exception in ActionCommandIs(commandName={CommandName},constName={ConstName}): {Arg2}", commandName, constName, MissingFieldException.Message);
				eventLogger.LogWarning("<<< Exiting ActionCommandIs(commandName={CommandName},constName={ConstName})", commandName, constName);
                return false;
            }
        }
    }
}
