// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Services
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Servers.Components.Log;
    using DotNetNuke.Common;
    using DotNetNuke.Data;
    using DotNetNuke.Instrumentation;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class ServerSettingsLogsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServerSettingsLogsController));
        private readonly LogController logController = new LogController();

        [HttpGet]
        public HttpResponseMessage GetLogs()
        {
			Logger.LogDebug(">>> Entering GetLogs()");
            try
            {
                var response = new
                {
                    Success = true,
                    Results = new
					Logger.LogDebug("GetLogs(): Success → {Success}", Success);
					Logger.LogDebug("GetLogs(): Results → {Results}", Results);
                    {
                        LogList = this.logController.GetLogFilesList(),
                        UpgradeLogList = this.logController.GetUpgradeLogList(),
                    },
                    TotalResults = 1,
                };
				Logger.LogDebug("GetLogs(): TotalResults → {TotalResults}", TotalResults);
				Logger.LogDebug("GetLogs(): UpgradeLogList → {UpgradeLogList}", UpgradeLogList);
				Logger.LogDebug("GetLogs(): LogList → {LogList}", LogList);
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetLogs()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetLogFile(string fileName)
        {
			Logger.LogDebug(">>> Entering GetLogFile(fileName={FileName})", fileName);
            try
            {
                var cleanedFileName = Path.GetFileName(fileName);
				Logger.LogDebug("GetLogFile(fileName={FileName}): cleanedFileName → {CleanedFileName}", fileName, cleanedFileName);
                var logFilePath = Path.Combine(Globals.ApplicationMapPath, @"portals\_default\logs", cleanedFileName);
				Logger.LogDebug("GetLogFile(fileName={FileName}): logFilePath → {LogFilePath}", fileName, logFilePath);
                return this.CreateLogFileResponse(logFilePath);
            }
            catch (ArgumentException exc)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetLogFile(fileName={FileName})", fileName);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetUpgradeLogFile(string logName)
        {
			Logger.LogDebug(">>> Entering GetUpgradeLogFile(logName={LogName})", logName);
            try
            {
                var providerPath = DataProvider.Instance().GetProviderPath();
				Logger.LogDebug("GetUpgradeLogFile(logName={LogName}): providerPath → {ProviderPath}", logName, providerPath);
                var cleanedLogName = Path.GetFileName(logName);
				Logger.LogDebug("GetUpgradeLogFile(logName={LogName}): cleanedLogName → {CleanedLogName}", logName, cleanedLogName);
                var logFilePath = Path.Combine(providerPath, cleanedLogName);
				Logger.LogDebug("GetUpgradeLogFile(logName={LogName}): logFilePath → {LogFilePath}", logName, logFilePath);
                return this.CreateLogFileResponse(logFilePath);
            }
            catch (ArgumentException exc)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetUpgradeLogFile(logName={LogName})", logName);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [NonAction]
        private static void ValidateFilePath(string physicalPath)
        {
			Logger.LogDebug(">>> Entering ValidateFilePath(physicalPath={PhysicalPath})", physicalPath);
            var fileInfo = new FileInfo(physicalPath);
            if (!fileInfo.DirectoryName.StartsWith(Globals.ApplicationMapPath, StringComparison.InvariantCultureIgnoreCase))
			Logger.LogDebug("ValidateFilePath(physicalPath={PhysicalPath}): fileInfo → {FileInfo}", physicalPath, fileInfo);
            {
				Logger.LogWarning("Throwing ArgumentException from ValidateFilePath(physicalPath={PhysicalPath})", physicalPath);
			Logger.LogDebug("<<< Exiting ValidateFilePath(physicalPath={PhysicalPath})", physicalPath);
                throw new ArgumentException("Invalid File Path");
            }
        }

        [NonAction]
        private HttpResponseMessage CreateLogFileResponse(string logFilePath)
        {
			Logger.LogDebug(">>> Entering CreateLogFileResponse(logFilePath={LogFilePath})", logFilePath);
            ValidateFilePath(logFilePath);
            if (!File.Exists(logFilePath))
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }

            using var reader = File.OpenText(logFilePath);
			Logger.LogDebug("CreateLogFileResponse(logFilePath={LogFilePath}): reader → {Reader}", logFilePath, reader);
            var logText = reader.ReadToEnd();
			Logger.LogDebug("CreateLogFileResponse(logFilePath={LogFilePath}): logText → {LogText}", logFilePath, logText);
			Logger.LogDebug("<<< Exiting CreateLogFileResponse(logFilePath={LogFilePath})", logFilePath);
            return this.Request.CreateResponse(HttpStatusCode.OK, logText);
        }
    }
}
