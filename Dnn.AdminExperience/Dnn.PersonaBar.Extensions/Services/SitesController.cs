// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Sites.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Sites.Services.Dto;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class SitesController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SitesController));
        private readonly Components.SitesController controller = new Components.SitesController();

        /// <summary>Gets list of portals.</summary>
        /// <param name="portalGroupId">The portal group ID or <see cref="Null.NullInteger"/> for no group.</param>
        /// <param name="filter">THe portal name filter.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>List of portals.</returns>
        /// <example>
        /// <code>
        /// GET <c>/api/personabar/sites/GetPortals?portalGroupId=-1&amp;filter=mysite&amp;pageIndex=0&amp;pageSize=10</c>
        /// </code>
        /// </example>
        [HttpGet]
        public HttpResponseMessage GetPortals(int portalGroupId, string filter, int pageIndex, int pageSize)
        {
			Logger.LogDebug(">>> Entering GetPortals(portalGroupId={PortalGroupId},filter={Filter},pageIndex={PageIndex},pageSize={PageSize})", portalGroupId, filter, pageIndex, pageSize);
            try
            {
                var totalRecords = 0;
                IEnumerable<PortalInfo> portals;
				Logger.LogDebug("GetPortals(portalGroupId={PortalGroupId},filter={Filter},pageIndex={PageIndex},pageSize={PageSize}): totalRecords → {TotalRecords}", portalGroupId, filter, pageIndex, pageSize, totalRecords);

                if (portalGroupId > Null.NullInteger)
                {
                    portals = PortalGroupController.Instance.GetPortalsByGroup(portalGroupId)
                                .Where(p => string.IsNullOrEmpty(filter)
                                                || p.PortalName.ToLowerInvariant().Contains(filter.ToLowerInvariant()))
                                .ToList();
								Logger.LogDebug("GetPortals(portalGroupId={PortalGroupId},filter={Filter},pageIndex={PageIndex},pageSize={PageSize}): portals → {Portals}", portalGroupId, filter, pageIndex, pageSize, portals);
                    totalRecords = portals.Count();
                    portals = portals.Skip(pageIndex * pageSize).Take(pageSize);
                }
                else
                {
                    portals = PortalController.GetPortalsByName(
                            $"%{filter}%",
                            pageIndex,
                            pageSize,
                            ref totalRecords)
                        .Cast<PortalInfo>();
                }

                var response = new
                {
                    Results = portals.Select(this.GetPortalDto).ToList(),
                    TotalResults = totalRecords,
                };
					Logger.LogDebug("GetPortals(portalGroupId={PortalGroupId},filter={Filter},pageIndex={PageIndex},pageSize={PageSize}): Results → {Results}", portalGroupId, filter, pageIndex, pageSize, Results);
					Logger.LogDebug("GetPortals(portalGroupId={PortalGroupId},filter={Filter},pageIndex={PageIndex},pageSize={PageSize}): TotalResults → {TotalResults}", portalGroupId, filter, pageIndex, pageSize, TotalResults);

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetPortals(portalGroupId={PortalGroupId},filter={Filter},pageIndex={PageIndex},pageSize={PageSize})", portalGroupId, filter, pageIndex, pageSize);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Sites/CreatePortal
        /// <summary>Adds a portal.</summary>
        /// <param name="request">The creation request.</param>
        /// <returns>A response with portal info and error messages.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage CreatePortal(CreatePortalRequest request)
        {
			Logger.LogDebug(">>> Entering CreatePortal(request={Request})", request);
            try
            {
                var errors = new List<string>();
                var portalId = this.controller.CreatePortal(
                    errors,
                    this.GetDomainName(),
                    this.GetAbsoluteServerPath(),
                    request.SiteTemplate,
                    request.SiteName,
                    request.SiteAlias,
                    request.SiteDescription,
                    request.SiteKeywords,
                    request.IsChildSite,
                    request.HomeDirectory,
                    request.SiteGroupId,
                    request.UseCurrentUserAsAdmin,
                    request.Firstname,
                    request.Lastname,
                    request.Username,
                    request.Email,
                    request.Password,
                    request.PasswordConfirm,
                    request.Question,
                    request.Answer);
                if (portalId < 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Join("<br/>", errors));
                }

                var portal = PortalController.Instance.GetPortal(portalId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
				Logger.LogDebug("CreatePortal(request={Request}): portal → {Portal}", request, portal);
                    Portal = this.GetPortalDto(portal),
                    ErrorMessage = errors,
                });
					Logger.LogDebug("CreatePortal(request={Request}): Portal → {Portal}", request, Portal);
					Logger.LogDebug("CreatePortal(request={Request}): ErrorMessage → {ErrorMessage}", request, ErrorMessage);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting CreatePortal(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Sites/DeletePortal
        /// <summary>Deletes a portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeletePortal(int portalId)
        {
            try
            {
                var portal = PortalController.Instance.GetPortal(portalId);
                if (portal != null)
                {
                    if (portal.PortalID != this.PortalSettings.PortalId &&
                        !PortalController.IsMemberOfPortalGroup(portal.PortalID))
                    {
                        var strMessage = PortalController.DeletePortal(portal, this.GetAbsoluteServerPath());
                        if (string.IsNullOrEmpty(strMessage))
                        {
                            return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
                        }

                        return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, strMessage);
                    }

                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.Unauthorized,
                        Localization.GetString("PortalDeletionDenied", Components.Constants.LocalResourcesFile));
                }

                return this.Request.CreateErrorResponse(
                    HttpStatusCode.NotFound,
                    Localization.GetString("PortalNotFound", Components.Constants.LocalResourcesFile));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Sites/ExportPortalTemplate
        /// <summary>Exports portal template.</summary>
        /// <param name="request">The export request.</param>
        /// <returns>A response with a message and template info.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ExportPortalTemplate(ExportTemplateRequest request)
        {
			Logger.LogDebug(">>> Entering ExportPortalTemplate(request={Request})", request);
            try
            {
                bool success;
                var message = this.controller.ExportPortalTemplate(request, this.UserInfo, out success);

				Logger.LogDebug("ExportPortalTemplate(request={Request}): message → {Message}", request, message);
                if (!success)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
                }

                var template = this.controller.GetPortalTemplates().First(x => x.Name == request.FileName);
                var templateItem = this.controller.CreateListItem(template);
				Logger.LogDebug("ExportPortalTemplate(request={Request}): template → {Template}", request, template);
				Logger.LogDebug("ExportPortalTemplate(request={Request}): templateItem → {TemplateItem}", request, templateItem);
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Message = message,
                    Template = new
					Logger.LogDebug("ExportPortalTemplate(request={Request}): Message → {Message}", request, Message);
					Logger.LogDebug("ExportPortalTemplate(request={Request}): Template → {Template}", request, Template);
                    {
                        Name = templateItem.Text,
                        templateItem.Value,
                    },
                });
						Logger.LogDebug("ExportPortalTemplate(request={Request}): Name → {Name}", request, Name);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting ExportPortalTemplate(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Sites/GetPortalLocales
        /// <summary>Gets list of portal locales.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>List of portal locales.</returns>
        [HttpGet]
        public HttpResponseMessage GetPortalLocales(int portalId)
        {
            try
            {
                var locales = LocaleController.Instance.GetLocales(portalId).Values;
                var response = new
                {
                    Success = true,
                    Results = locales.Select(l => new
                    {
                        l.Code,
                        l.EnglishName,
                        l.LanguageId,
                    }),
                    TotalResults = locales.Count,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Sites/DeleteExpiredPortals
        /// <summary>Deletes expired portals.</summary>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteExpiredPortals()
        {
			Logger.LogDebug(">>> Entering DeleteExpiredPortals()");
            try
            {
                PortalController.DeleteExpiredPortals(this.GetAbsoluteServerPath());
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting DeleteExpiredPortals()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Sites/GetPortalTemplates
        /// <summary>Gets list of portal templates.</summary>
        /// <returns>List of portal templates.</returns>
        [HttpGet]
        public HttpResponseMessage GetPortalTemplates()
        {
			Logger.LogDebug(">>> Entering GetPortalTemplates()");
            try
            {
                var defaultTemplate = this.controller.GetDefaultTemplate();
                var temps = new List<Tuple<string, string>>();
				Logger.LogDebug("GetPortalTemplates(): defaultTemplate → {DefaultTemplate}", defaultTemplate);
				Logger.LogDebug("GetPortalTemplates(): temps → {Temps}", temps);
                var templates = this.controller.GetPortalTemplates();
                foreach (var template in templates)
                {
				Logger.LogDebug("GetPortalTemplates(): templates → {Templates}", templates);
                    var item = this.controller.CreateListItem(template);
                    temps.Add(new Tuple<string, string>(item.Text, item.Value));
					Logger.LogDebug("GetPortalTemplates(): item → {Item}", item);
                    if (item.Value.StartsWith(defaultTemplate, StringComparison.OrdinalIgnoreCase))
                    {
                        defaultTemplate = item.Value;
                    }
                }

                var response = new
                {
                    Success = true,
                    Results = new
					Logger.LogDebug("GetPortalTemplates(): Success → {Success}", Success);
					Logger.LogDebug("GetPortalTemplates(): Results → {Results}", Results);
                    {
                        Templates = temps.Select(t => new
                        {
                            Name = t.Item1,
                            Value = t.Item2,
                        }),
                        DefaultTemplate = defaultTemplate,
                    },
                    TotalResults = temps.Count,
                };
							Logger.LogDebug("GetPortalTemplates(): Name → {Name}", Name);
							Logger.LogDebug("GetPortalTemplates(): Value → {Value}", Value);

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetPortalTemplates()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Sites/RequiresQuestionAndAnswer
        /// <summary>Gets whether a Question/Answer is required for Password retrieval.</summary>
        /// <returns>A response with a <c>RequiresQuestionAndAnswer</c> field.</returns>
        [HttpGet]
        public HttpResponseMessage RequiresQuestionAndAnswer()
        {
			Logger.LogDebug(">>> Entering RequiresQuestionAndAnswer()");
            try
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    MembershipProviderConfig.RequiresQuestionAndAnswer,
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting RequiresQuestionAndAnswer()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private string GetAbsoluteServerPath()
        {
			Logger.LogDebug(">>> Entering GetAbsoluteServerPath()");
            var httpContext = this.Request.Properties["MS_HttpContext"] as HttpContextWrapper;
            var strServerPath = string.Empty;
			Logger.LogDebug("GetAbsoluteServerPath(): httpContext → {HttpContext}", httpContext);
			Logger.LogDebug("GetAbsoluteServerPath(): strServerPath → {StrServerPath}", strServerPath);
            if (httpContext != null)
            {
                strServerPath = httpContext.Request.MapPath(httpContext.Request.ApplicationPath);
            }

            if (!strServerPath.EndsWith(@"\", StringComparison.OrdinalIgnoreCase))
            {
                strServerPath += @"\";
            }

			Logger.LogDebug("<<< Exiting GetAbsoluteServerPath()");
            return strServerPath;
        }

        private string GetDomainName()
        {
			Logger.LogDebug(">>> Entering GetDomainName()");
			Logger.LogDebug("<<< Exiting GetDomainName()");
            return this.Request.Properties["MS_HttpContext"] is HttpContextWrapper httpContext
                ? Globals.GetDomainName(httpContext.Request, true)
                : string.Empty;
        }

        private object GetPortalDto(PortalInfo portal)
        {
            string contentLocalizable;
            var portalDto = new
            {
                portal.PortalID,
                portal.PortalName,
                PortalAliases = this.controller.FormatPortalAliases(portal.PortalID),
                portal.Users,
                portal.Pages,
                portal.HostSpace,
                portal.HostFee,
                portal.DefaultLanguage,
                ExpiryDate = this.controller.FormatExpiryDate(portal.ExpiryDate),
                portal.LastModifiedOnDate,
                contentLocalizable =
                    PortalController.Instance.GetPortalSettings(portal.PortalID)
                        .TryGetValue("ContentLocalizationEnabled", out contentLocalizable) &&
                    Convert.ToBoolean(contentLocalizable),
                allowDelete =
                    portal.PortalID != this.PortalSettings.PortalId &&
                     !PortalController.IsMemberOfPortalGroup(portal.PortalID),
            };
            return portalDto;
        }
    }
}
