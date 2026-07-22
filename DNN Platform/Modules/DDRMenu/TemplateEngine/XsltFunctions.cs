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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public bool UserIsInRole(string roleName)
        {
			Logger.LogDebug(">>> Entering UserIsInRole(roleName={RoleName})", roleName);
			Logger.LogDebug("<<< Exiting UserIsInRole(roleName={RoleName})", roleName);
            return UserController.Instance.GetCurrentUserInfo().IsInRole(roleName);
        }

        public string GetLoginURL()
        {
			Logger.LogDebug(">>> Entering GetLoginURL()");
			Logger.LogDebug("<<< Exiting GetLoginURL()");
            return DNNAbstract.GetLoginUrl();
        }

        public string GetLoginText()
        {
			Logger.LogDebug(">>> Entering GetLoginText()");
            try
            {
                return Localization.GetString(
                    HttpContext.Current.Request.IsAuthenticated ? "Logout" : "Login",
                    DNNContext.Current.ResolveUrl("~/Admin/Skins/" + Localization.LocalResourceDirectory + "/Login.ascx"));
            }
            catch (Exception)
            {
				Logger.LogError("Exception in GetLoginText(): {Arg0}", ex.Message);
				Logger.LogDebug("<<< Exiting GetLoginText()");
                return "Login";
            }
        }

        public string GetUserURL()
        {
			Logger.LogDebug(">>> Entering GetUserURL()");
			Logger.LogDebug("<<< Exiting GetUserURL()");
            return DNNAbstract.GetUserUrl();
        }

        public string GetUserText()
        {
			Logger.LogDebug(">>> Entering GetUserText()");
            try
            {
                if (HttpContextSource.Current.Request.IsAuthenticated)
                {
                    return UserController.Instance.GetCurrentUserInfo().DisplayName;
                }
                else
                {
                    var hostControl = (Control)HttpContextSource.Current.Items["DDRMenuHostControl"];
                    var resourceFileRoot = hostControl.ResolveUrl($"~/Admin/Skins/{Localization.LocalResourceDirectory}/User.ascx");
					Logger.LogDebug("GetUserText(): hostControl → {HostControl}", hostControl);
					Logger.LogDebug("GetUserText(): resourceFileRoot → {ResourceFileRoot}", resourceFileRoot);
                    return Localization.GetString("Register", resourceFileRoot);
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in GetUserText(): {Arg0}", ex.Message);
				Logger.LogDebug("<<< Exiting GetUserText()");
                return exc.ToString();
            }
        }

        public string EscapeXML(string xml)
        {
			Logger.LogDebug(">>> Entering EscapeXML(xml={Xml})", xml);
			Logger.LogDebug("<<< Exiting EscapeXML(xml={Xml})", xml);
            return SecurityElement.Escape(xml);
        }

        public string HtmlEncode(string s)
        {
			Logger.LogDebug(">>> Entering HtmlEncode(s={S})", s);
			Logger.LogDebug("<<< Exiting HtmlEncode(s={S})", s);
            return HttpUtility.HtmlEncode(s);
        }

        public string GetString(string name, string resourceFile)
        {
			Logger.LogDebug(">>> Entering GetString(name={Name},resourceFile={ResourceFile})", name, resourceFile);
            var resolver = HttpContext.Current.Items["Resolver"] as PathResolver;
            var resolvedFile = string.Empty;
			Logger.LogDebug("GetString(name={Name},resourceFile={ResourceFile}): resolver → {Resolver}", name, resourceFile, resolver);
			Logger.LogDebug("GetString(name={Name},resourceFile={ResourceFile}): resolvedFile → {ResolvedFile}", name, resourceFile, resolvedFile);
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

			Logger.LogDebug("<<< Exiting GetString(name={Name},resourceFile={ResourceFile})", name, resourceFile);
            return Localization.GetString(name, resolvedFile);
        }

        public bool ActionCommandIs(string commandName, string constName)
        {
			Logger.LogDebug(">>> Entering ActionCommandIs(commandName={CommandName},constName={ConstName})", commandName, constName);
            try
            {
                return
                    commandName.Equals(
                        typeof(DotNetNuke.Entities.Modules.Actions.ModuleActionType).InvokeMember(
                            constName, BindingFlags.GetField, null, null, new object[0]));
            }
            catch (MissingFieldException)
            {
				Logger.LogError("Exception in ActionCommandIs(commandName={CommandName},constName={ConstName}): {Arg2}", commandName, constName, ex.Message);
				Logger.LogDebug("<<< Exiting ActionCommandIs(commandName={CommandName},constName={ConstName})", commandName, constName);
                return false;
            }
        }
    }
}
