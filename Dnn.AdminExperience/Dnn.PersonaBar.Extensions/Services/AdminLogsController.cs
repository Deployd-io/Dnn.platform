// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.AdminLogs.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.AdminLogs.Services.Dto;
    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Web.Api;
    using Newtonsoft.Json.Linq;

    [MenuPermission(MenuName = Components.Constants.MenuName)]
    public class AdminLogsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AdminLogsController));
        private readonly Components.AdminLogsController controller = new Components.AdminLogsController();

        /// GET: api/AdminLogs/GetLogTypes
        /// <summary>Gets log types.</summary>
        /// <returns>List of log types.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.AdminLogsView + "," + Components.Constants.LogSettingsEdit)]
        public HttpResponseMessage GetLogTypes()
        {
			Logger.LogDebug(">>> Entering GetLogTypes()");
            try
            {
                var logTypes =
                    LogController.Instance.GetLogTypeInfoDictionary()
                        .Values.OrderBy(t => t.LogTypeFriendlyName)
                        .ToList();

                logTypes.Insert(0, new LogTypeInfo
                {
                    LogTypeFriendlyName = Localization.GetString("AllTypes", Components.Constants.LocalResourcesFile),
                    LogTypeKey = "*",
                });

                var types = logTypes.Select(v => new
                {
                    v.LogTypeFriendlyName,
                    v.LogTypeKey,
                }).ToList();

                var response = new
                {
                    Success = true,
                    Results = types,
                    TotalResults = types.Count,
                };
					Logger.LogDebug("GetLogTypes(): Success → {Success}", Success);
					Logger.LogDebug("GetLogTypes(): Results → {Results}", Results);
					Logger.LogDebug("GetLogTypes(): TotalResults → {TotalResults}", TotalResults);

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetLogTypes()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/AdminLogs/GetLogItems
        /// <summary>Gets log items.</summary>
        /// <param name="logType">The log type.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <returns>List of log items.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.AdminLogsView)]
        public HttpResponseMessage GetLogItems(string logType, int pageSize, int pageIndex)
        {
			Logger.LogDebug(">>> Entering GetLogItems(logType={LogType},pageSize={PageSize},pageIndex={PageIndex})", logType, pageSize, pageIndex);
            try
            {
                var portalId = this.UserInfo.IsSuperUser ? this.Request.GetQueryStringAsInteger("portalid") : this.PortalId;
                var totalRecords = 0;
				Logger.LogDebug("GetLogItems(logType={LogType},pageSize={PageSize},pageIndex={PageIndex}): portalId → {PortalId}", logType, pageSize, pageIndex, portalId);
                var logItems = LogController.Instance.GetLogs(
                    portalId,
                    logType == "*" ? string.Empty : logType,
                    pageSize,
                    pageIndex,
                    ref totalRecords);

                var items = logItems.Select(v => new
                {
                    v.LogGUID,
                    v.LogFileID,
                    this.controller.GetMyLogType(v.LogTypeKey).LogTypeCSSClass,
                    this.controller.GetMyLogType(v.LogTypeKey).LogTypeFriendlyName,
                    v.LogUserName,
                    v.LogPortalName,
                    LogCreateDate = v.LogCreateDate.ToString("G", CultureInfo.InvariantCulture),
                    v.LogProperties.Summary,
                    LogProperties = this.controller.GetPropertiesText(v),
                });
					Logger.LogDebug("GetLogItems(logType={LogType},pageSize={PageSize},pageIndex={PageIndex}): LogCreateDate → {LogCreateDate}", logType, pageSize, pageIndex, LogCreateDate);

                var response = new
                {
                    Success = true,
                    Results = items,
                    TotalResults = totalRecords,
                };
					Logger.LogDebug("GetLogItems(logType={LogType},pageSize={PageSize},pageIndex={PageIndex}): Success → {Success}", logType, pageSize, pageIndex, Success);
					Logger.LogDebug("GetLogItems(logType={LogType},pageSize={PageSize},pageIndex={PageIndex}): Results → {Results}", logType, pageSize, pageIndex, Results);
					Logger.LogDebug("GetLogItems(logType={LogType},pageSize={PageSize},pageIndex={PageIndex}): TotalResults → {TotalResults}", logType, pageSize, pageIndex, TotalResults);

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetLogItems(logType={LogType},pageSize={PageSize},pageIndex={PageIndex})", logType, pageSize, pageIndex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/DeleteLogItems
        /// <summary>Deletes log items.</summary>
        /// <param name="logIds">List of log items to be deleted.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage DeleteLogItems(IEnumerable<string> logIds)
        {
			Logger.LogDebug(">>> Entering DeleteLogItems(logIds={LogIds})", logIds);
            try
            {
                foreach (var logId in logIds)
                {
                    var objLogInfo = new LogInfo { LogGUID = logId };
                    LogController.Instance.DeleteLog(objLogInfo);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting DeleteLogItems(logIds={LogIds})", logIds);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/EmailLogItems
        /// <summary>Sends log items via email.</summary>
        /// <param name="request">send email request.</param>
        /// <returns>A response with result details.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.AdminLogsView + "&" + Components.Constants.AdminLogsEdit)]
        public HttpResponseMessage EmailLogItems(EmailLogItemsRequest request)
        {
			Logger.LogDebug(">>> Entering EmailLogItems(request={Request})", request);
            try
            {
                if (!this.UserInfo.IsSuperUser && request.LogIds.Any(logGuid => (LogController.Instance.GetLog(logGuid)?.LogPortalId != this.PortalId)))
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.Unauthorized,
                        Localization.GetString("UnAuthorizedToSendLog", Components.Constants.LocalResourcesFile));
                }

                string error;
                var subject = request.Subject;
                var strFromEmailAddress = !string.IsNullOrEmpty(this.UserInfo.Email) ? this.UserInfo.Email : this.PortalSettings.Email;
				Logger.LogDebug("EmailLogItems(request={Request}): subject → {Subject}", request, subject);
				Logger.LogDebug("EmailLogItems(request={Request}): strFromEmailAddress → {StrFromEmailAddress}", request, strFromEmailAddress);

                if (string.IsNullOrEmpty(subject))
                {
                    subject = this.PortalSettings.PortalName + @" Exceptions";
                }

                string returnMsg = this.controller.EmailLogItems(subject, strFromEmailAddress, request.Email, request.Message, request.LogIds, out error);
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = string.IsNullOrEmpty(returnMsg) ? true : false,
                    ErrorMessage = error,
                    ReturnMessage = returnMsg,
                });
					Logger.LogDebug("EmailLogItems(request={Request}): Success → {Success}", request, Success);
					Logger.LogDebug("EmailLogItems(request={Request}): ErrorMessage → {ErrorMessage}", request, ErrorMessage);
					Logger.LogDebug("EmailLogItems(request={Request}): ReturnMessage → {ReturnMessage}", request, ReturnMessage);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting EmailLogItems(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/ClearLog
        /// <summary>Clears all log items.</summary>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage ClearLog()
        {
			Logger.LogDebug(">>> Entering ClearLog()");
            try
            {
                this.controller.ClearLog();
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting ClearLog()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/AdminLogs/GetKeepMostRecentOptions
        /// <summary>Gets options of Keep Most Recent.</summary>
        /// <returns>Option list of Keep Most Recent.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView, CheckPermissionForAdmin = true)]
        public HttpResponseMessage GetKeepMostRecentOptions()
        {
			Logger.LogDebug(">>> Entering GetKeepMostRecentOptions()");
            try
            {
                var options = this.controller.GetKeepMostRecentOptions().ToList();

				Logger.LogDebug("GetKeepMostRecentOptions(): options → {Options}", options);
                var response = new
                {
                    Success = true,
                    Results = options,
                    TotalResults = options.Count,
                };
					Logger.LogDebug("GetKeepMostRecentOptions(): Success → {Success}", Success);
					Logger.LogDebug("GetKeepMostRecentOptions(): Results → {Results}", Results);
					Logger.LogDebug("GetKeepMostRecentOptions(): TotalResults → {TotalResults}", TotalResults);

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetKeepMostRecentOptions()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/AdminLogs/GetOccurrenceOptions
        /// <summary>Gets options of occurrence threshold.</summary>
        /// <returns>Option list of occurrence threshold.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView, CheckPermissionForAdmin = true)]
        public HttpResponseMessage GetOccurrenceOptions()
        {
			Logger.LogDebug(">>> Entering GetOccurrenceOptions()");
            try
            {
                var response = new
                {
                    Success = true,
                    Results = new
					Logger.LogDebug("GetOccurrenceOptions(): Success → {Success}", Success);
					Logger.LogDebug("GetOccurrenceOptions(): Results → {Results}", Results);
                    {
                        Thresholds = this.controller.GetOccurenceThresholds().ToList(),
                        NotificationTimes = this.controller.GetOccurenceThresholdNotificationTimes().ToList(),
                        NotificationTimeTypes = this.controller.GetOccurenceThresholdNotificationTimeTypes().ToList(),
                    },
                    TotalResults = 1,
                };
						Logger.LogDebug("GetOccurrenceOptions(): Thresholds → {Thresholds}", Thresholds);
						Logger.LogDebug("GetOccurrenceOptions(): NotificationTimes → {NotificationTimes}", NotificationTimes);
						Logger.LogDebug("GetOccurrenceOptions(): NotificationTimeTypes → {NotificationTimeTypes}", NotificationTimeTypes);

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetOccurrenceOptions()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/AdminLogs/GetLogSettings
        /// <summary>Gets log settings.</summary>
        /// <returns>List of log settings.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView, CheckPermissionForAdmin = true)]
        public HttpResponseMessage GetLogSettings()
        {
			Logger.LogDebug(">>> Entering GetLogSettings()");
            try
            {
                var logTypes = LogController.Instance.GetLogTypeConfigInfo().Cast<LogTypeConfigInfo>();

				Logger.LogDebug("GetLogSettings(): logTypes → {LogTypes}", logTypes);
                int portalId;
                var types = logTypes
                    .Where(x => this.UserInfo.IsSuperUser || (int.TryParse(x.LogTypePortalID, out portalId) && portalId == this.PortalId))
                    .Select(v => new
                    {
                        v.LogTypeFriendlyName,
                        v.LogTypeKey,
                        v.LogTypePortalID,
                        LogTypePortalName =
                            int.TryParse(v.LogTypePortalID, out portalId)
                                ? PortalController.Instance.GetPortal(portalId).PortalName
                                : "*",
                        v.LoggingIsActive,
                        v.LogFileName,
                        v.ID,
                    }).ToList();

                var response = new
                {
                    Success = true,
                    Results = types,
                    TotalResults = types.Count,
                };
					Logger.LogDebug("GetLogSettings(): Success → {Success}", Success);
					Logger.LogDebug("GetLogSettings(): Results → {Results}", Results);
					Logger.LogDebug("GetLogSettings(): TotalResults → {TotalResults}", TotalResults);

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetLogSettings()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/AdminLogs/GetLogSettings
        /// <summary>Gets log type settings.</summary>
        /// <param name="logTypeConfigId">The log type config ID.</param>
        /// <returns>A response with the log setting info.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView, CheckPermissionForAdmin = true)]
        public HttpResponseMessage GetLogSetting(string logTypeConfigId)
        {
			Logger.LogDebug(">>> Entering GetLogSetting(logTypeConfigId={LogTypeConfigId})", logTypeConfigId);
            try
            {
                var configInfo = this.controller.GetLogTypeConfig(logTypeConfigId);
                int portalId;
				Logger.LogDebug("GetLogSetting(logTypeConfigId={LogTypeConfigId}): configInfo → {ConfigInfo}", logTypeConfigId, configInfo);
                if (!this.UserInfo.IsSuperUser && (!int.TryParse(configInfo.LogTypePortalID, out portalId) || portalId != this.PortalId))
                {
                    return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    configInfo.ID,
                    configInfo.LoggingIsActive,
                    configInfo.LogTypeFriendlyName,
                    configInfo.LogTypeKey,
                    LogTypePortalID =
                            int.TryParse(configInfo.LogTypePortalID, out portalId) ? portalId.ToString(CultureInfo.InvariantCulture) : "*",
                    LogTypePortalName =
                            int.TryParse(configInfo.LogTypePortalID, out portalId)
                                ? PortalController.Instance.GetPortal(portalId).PortalName
                                : "*",
                    configInfo.KeepMostRecent,
                    configInfo.EmailNotificationIsActive,
                    configInfo.NotificationThreshold,
                    configInfo.NotificationThresholdTime,
                    configInfo.NotificationThresholdTimeType,
                    configInfo.MailFromAddress,
                    configInfo.MailToAddress,
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetLogSetting(logTypeConfigId={LogTypeConfigId})", logTypeConfigId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/AddLogSettings
        /// <summary>Adds a new log settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response with the log setting info.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView + "&" + Components.Constants.LogSettingsEdit, CheckPermissionForAdmin = true)]
        public HttpResponseMessage AddLogSetting([FromBody] UpdateLogSettingsRequest request)
        {
			Logger.LogDebug(">>> Entering AddLogSetting(request={Request})", request);
            try
            {
                if (!this.UserInfo.IsSuperUser)
                {
                    return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                var logTypeConfigInfo = JObject.FromObject(request).ToObject<LogTypeConfigInfo>();
                this.controller.AddLogTypeConfig(logTypeConfigInfo);
				Logger.LogDebug("AddLogSetting(request={Request}): logTypeConfigInfo → {LogTypeConfigInfo}", request, logTypeConfigInfo);
                return this.GetLatestLogSetting();
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting AddLogSetting(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/UpdateLogSettings
        /// <summary>Updates an existing log settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response with the log setting info.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView + "&" + Components.Constants.LogSettingsEdit, CheckPermissionForAdmin = true)]
        public HttpResponseMessage UpdateLogSetting([FromBody] UpdateLogSettingsRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateLogSetting(request={Request})", request);
            try
            {
                request.LogTypePortalID = this.UserInfo.IsSuperUser ? request.LogTypePortalID : this.PortalId.ToString(CultureInfo.InvariantCulture);

                int requestPortalId;
                int settingPortalId;
                var configInfo = this.controller.GetLogTypeConfig(request.ID);
                if (!this.UserInfo.IsSuperUser &&
					Logger.LogDebug("UpdateLogSetting(request={Request}): configInfo → {ConfigInfo}", request, configInfo);
                    (!int.TryParse(configInfo.LogTypePortalID, out settingPortalId) ||
                     !int.TryParse(request.LogTypePortalID, out requestPortalId) || requestPortalId != settingPortalId))
                {
                    return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                var logTypeConfigInfo = JObject.FromObject(request).ToObject<LogTypeConfigInfo>();
                this.controller.UpdateLogTypeConfig(logTypeConfigInfo);
				Logger.LogDebug("UpdateLogSetting(request={Request}): logTypeConfigInfo → {LogTypeConfigInfo}", request, logTypeConfigInfo);
                return this.GetLogSetting(request.ID);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateLogSetting(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/DeleteLogSettings
        /// <summary>Deletes an existing log settings.</summary>
        /// <param name="request">The delete request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView + "&" + Components.Constants.LogSettingsEdit, CheckPermissionForAdmin = true)]
        public HttpResponseMessage DeleteLogSetting(DeleteLogSettingsRequest request)
        {
			Logger.LogDebug(">>> Entering DeleteLogSetting(request={Request})", request);
            try
            {
                var configInfo = this.controller.GetLogTypeConfig(request.LogTypeConfigId);
                int portalId;
				Logger.LogDebug("DeleteLogSetting(request={Request}): configInfo → {ConfigInfo}", request, configInfo);
                if (!this.UserInfo.IsSuperUser && (!int.TryParse(configInfo.LogTypePortalID, out portalId) || portalId != this.PortalId))
                {
                    return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                this.controller.DeleteLogTypeConfig(request.LogTypeConfigId);
                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    new { Success = true, LogSettingId = request.LogTypeConfigId });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting DeleteLogSetting(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/AdminLogs/GetLatestLogSetting
        /// <summary>Gets latest log setting.</summary>
        /// <returns>Log Setting.</returns>
        private HttpResponseMessage GetLatestLogSetting()
        {
			Logger.LogDebug(">>> Entering GetLatestLogSetting()");
            try
            {
                var logTypes = LogController.Instance.GetLogTypeConfigInfo().Cast<LogTypeConfigInfo>()
                    .OrderByDescending(l => Convert.ToInt32(l.ID, CultureInfo.InvariantCulture)).ToList();
                var configInfo = logTypes.FirstOrDefault();

				Logger.LogDebug("GetLatestLogSetting(): configInfo → {ConfigInfo}", configInfo);
                if (configInfo != null)
                {
                    int portalId;
                    return this.Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        configInfo.ID,
                        configInfo.LoggingIsActive,
                        configInfo.LogTypeFriendlyName,
                        configInfo.LogTypeKey,
                        LogTypePortalID =
                            int.TryParse(configInfo.LogTypePortalID, out portalId) ? portalId.ToString(CultureInfo.InvariantCulture) : "*",
                        LogTypePortalName =
                            int.TryParse(configInfo.LogTypePortalID, out portalId)
                                ? PortalController.Instance.GetPortal(portalId).PortalName
                                : "*",
                        configInfo.KeepMostRecent,
                        configInfo.EmailNotificationIsActive,
                        configInfo.NotificationThreshold,
                        configInfo.NotificationThresholdTime,
                        configInfo.NotificationThresholdTimeType,
                        configInfo.MailFromAddress,
                        configInfo.MailToAddress,
                    });
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetLatestLogSetting()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}
