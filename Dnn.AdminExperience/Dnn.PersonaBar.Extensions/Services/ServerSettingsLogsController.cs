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
						Logger.LogDebug("GetLogs(): LogList → {LogList}", LogList);
						Logger.LogDebug("GetLogs(): UpgradeLogList → {UpgradeLogList}", UpgradeLogList);
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
                var logFilePath = Path.Combine(Globals.ApplicationMapPath, @"portals\_default\logs", cleanedFileName);
				Logger.LogDebug("GetLogFile(fileName={FileName}): cleanedFileName → {CleanedFileName}", fileName, cleanedFileName);
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
                var cleanedLogName = Path.GetFileName(logName);
				Logger.LogDebug("GetUpgradeLogFile(logName={LogName}): providerPath → {ProviderPath}", logName, providerPath);
				Logger.LogDebug("GetUpgradeLogFile(logName={LogName}): cleanedLogName → {CleanedLogName}", logName, cleanedLogName);
                var logFilePath = Path.Combine(providerPath, cleanedLogName);
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
                throw new ArgumentException("Invalid File Path");
            }
			Logger.LogDebug("<<< Exiting ValidateFilePath(physicalPath={PhysicalPath})", physicalPath);
        }

        [NonAction]
        private HttpResponseMessage CreateLogFileResponse(string logFilePath)
        {
            ValidateFilePath(logFilePath);
            if (!File.Exists(logFilePath))
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }

            using var reader = File.OpenText(logFilePath);
            var logText = reader.ReadToEnd();
            return this.Request.CreateResponse(HttpStatusCode.OK, logText);
        }
    }
}
