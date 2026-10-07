// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.CssEditor.Services
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.CssEditor.Services.Dto;
    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    [MenuPermission(Scope = ServiceScope.Admin)]
    public class CssEditorController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CssEditorController));

        /// GET: api/CssEditor/GetStyleSheet
        /// <summary>Gets portal.css of specific portal.</summary>
        /// <param name="portalId">Id of portal.</param>
        /// <returns>Content of portal.css.</returns>
        [HttpGet]
        public HttpResponseMessage GetStyleSheet(int portalId)
        {
			Logger.LogError(">>> Entering GetStyleSheet(portalId={PortalId})", portalId);
            try
            {
                if (!PortalSettings.Current.UserInfo.IsSuperUser && PortalSettings.Current.UserInfo.PortalID != portalId)
                {
					Logger.LogError("Throwing SecurityException from GetStyleSheet(portalId={PortalId})", portalId);
                    throw new SecurityException("No Permission");
                }
                else
                {
                    var activeLanguage = LocaleController.Instance.GetDefaultLocale(portalId).Code;
					Logger.LogError("GetStyleSheet(portalId={PortalId}): activeLanguage → {ActiveLanguage}", portalId, activeLanguage);
                    var portal = PortalController.Instance.GetPortal(portalId, activeLanguage);
					Logger.LogError("GetStyleSheet(portalId={PortalId}): portal → {Portal}", portalId, portal);

                    string uploadDirectory = string.Empty;
					Logger.LogError("GetStyleSheet(portalId={PortalId}): uploadDirectory → {UploadDirectory}", portalId, uploadDirectory);
                    string styleSheetContent = string.Empty;
					Logger.LogError("GetStyleSheet(portalId={PortalId}): styleSheetContent → {StyleSheetContent}", portalId, styleSheetContent);
                    if (portal != null)
                    {
                        uploadDirectory = portal.HomeDirectoryMapPath;
                    }

                    // read CSS file
                    if (File.Exists(uploadDirectory + "portal.css"))
                    {
                        using (var text = File.OpenText(uploadDirectory + "portal.css"))
                        {
                            styleSheetContent = text.ReadToEnd();
                        }
                    }

                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Content = styleSheetContent });
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetStyleSheet(portalId={PortalId})", portalId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/CssEditor/UpdateStyleSheet
        /// <summary>Updates portal.css of specific portal.</summary>
        /// <param name="request">Content of portal css.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateStyleSheet(UpdateCssRequest request)
        {
			Logger.LogError(">>> Entering UpdateStyleSheet(request={Request})", request);
            if (!PortalSettings.Current.UserInfo.IsSuperUser && PortalSettings.Current.UserInfo.PortalID != request.PortalId)
            {
				Logger.LogError("Throwing SecurityException from UpdateStyleSheet(request={Request})", request);
                throw new SecurityException("No Permission");
            }
            else
            {
                try
                {
                    string strUploadDirectory = string.Empty;
					Logger.LogError("UpdateStyleSheet(request={Request}): strUploadDirectory → {StrUploadDirectory}", request, strUploadDirectory);
                    var relativePath = string.Empty;
					Logger.LogError("UpdateStyleSheet(request={Request}): relativePath → {RelativePath}", request, relativePath);

                    PortalInfo objPortal = PortalController.Instance.GetPortal(request.PortalId);
					Logger.LogError("UpdateStyleSheet(request={Request}): objPortal → {ObjPortal}", request, objPortal);
                    if (objPortal != null)
                    {
                        strUploadDirectory = objPortal.HomeDirectoryMapPath;
                        relativePath = $"{Globals.ApplicationPath}/{objPortal.HomeDirectory}/portal.css";
                    }

                    // reset attributes
                    if (File.Exists(strUploadDirectory + "portal.css"))
                    {
                        File.SetAttributes(strUploadDirectory + "portal.css", FileAttributes.Normal);
                    }

                    // write CSS file
                    using (var writer = File.CreateText(strUploadDirectory + "portal.css"))
                    {
                        writer.WriteLine(request.StyleSheetContent);
                    }

                    // Clear client resource cache
                    var overrideSetting =
                        PortalController.GetPortalSetting(
                            ClientResourceSettings.OverrideDefaultSettingsKey,
                            request.PortalId,
                            "False");
                    bool overridePortal;
                    if (bool.TryParse(overrideSetting, out overridePortal))
                    {
                        if (overridePortal)
                        {
                            // increment this portal version only
                            PortalController.IncrementCrmVersion(request.PortalId);
                        }
                        else
                        {
                            // increment host version, do not increment other portal versions though.
                            HostController.Instance.IncrementCrmVersion(false);
                        }
                    }

                    ClientResourceManager.ClearFileExistsCache(relativePath);

                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
					Logger.LogWarning("<<< Exiting UpdateStyleSheet(request={Request})", request);
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
                }
            }
        }

        /// POST: api/CssEditor/RestoreStyleSheet
        /// <summary>Restores portal.css of specific portal.</summary>
        /// <param name="request">Id of portal.</param>
        /// <returns>Content of portal.css.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RestoreStyleSheet(RestoreCssRequest request)
        {
			Logger.LogError(">>> Entering RestoreStyleSheet(request={Request})", request);
            if (!PortalSettings.Current.UserInfo.IsSuperUser &&
                PortalSettings.Current.UserInfo.PortalID != request.PortalId)
            {
				Logger.LogError("Throwing SecurityException from RestoreStyleSheet(request={Request})", request);
                throw new SecurityException("No Permission");
            }
            else
            {
                try
                {
                    PortalInfo portal = PortalController.Instance.GetPortal(request.PortalId);
					Logger.LogError("RestoreStyleSheet(request={Request}): portal → {Portal}", request, portal);
                    if (portal != null)
                    {
                        if (File.Exists(portal.HomeDirectoryMapPath + "portal.css"))
                        {
                            // delete existing style sheet
                            File.Delete(portal.HomeDirectoryMapPath + "portal.css");
                        }

                        // copy file from Host
                        if (File.Exists(Globals.HostMapPath + "portal.css"))
                        {
                            File.Copy(Globals.HostMapPath + "portal.css", portal.HomeDirectoryMapPath + "portal.css");
                        }

                        ClientResourceManager.ClearFileExistsCache($"{Globals.ApplicationPath}/{portal.HomeDirectory}/portal.css");
                    }

                    var content = LoadStyleSheet(request.PortalId);
					Logger.LogError("RestoreStyleSheet(request={Request}): content → {Content}", request, content);

                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, StyleSheetContent = content });
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
					Logger.LogWarning("<<< Exiting RestoreStyleSheet(request={Request})", request);
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
                }
            }
        }

        private static string LoadStyleSheet(int portalId)
        {
			Logger.LogError(">>> Entering LoadStyleSheet(portalId={PortalId})", portalId);
            var activeLanguage = LocaleController.Instance.GetDefaultLocale(portalId).Code;
            var portal = PortalController.Instance.GetPortal(portalId, activeLanguage);
			Logger.LogError("LoadStyleSheet(portalId={PortalId}): portal → {Portal}", portalId, portal);
			Logger.LogError("LoadStyleSheet(portalId={PortalId}): activeLanguage → {ActiveLanguage}", portalId, activeLanguage);

            string uploadDirectory = string.Empty;
			Logger.LogError("LoadStyleSheet(portalId={PortalId}): uploadDirectory → {UploadDirectory}", portalId, uploadDirectory);
            string styleSheetContent = string.Empty;
			Logger.LogError("LoadStyleSheet(portalId={PortalId}): styleSheetContent → {StyleSheetContent}", portalId, styleSheetContent);
            if (portal != null)
            {
                uploadDirectory = portal.HomeDirectoryMapPath;
            }

            // read CSS file
            if (File.Exists(uploadDirectory + "portal.css"))
            {
                using (var text = File.OpenText(uploadDirectory + "portal.css"))
                {
                    styleSheetContent = text.ReadToEnd();
                }
            }

			Logger.LogWarning("<<< Exiting LoadStyleSheet(portalId={PortalId})", portalId);
            return styleSheetContent;
        }
    }
}
