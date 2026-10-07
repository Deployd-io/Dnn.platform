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
			Logger.LogError(">>> Entering GetLogs()");
            try
            {
                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        LogList = this.logController.GetLogFilesList(),
                        UpgradeLogList = this.logController.GetUpgradeLogList(),
                    },
                    TotalResults = 1,
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetLogs()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetLogFile(string fileName)
        {
			Logger.LogError(">>> Entering GetLogFile(fileName={FileName})", fileName);
            try
            {
                var cleanedFileName = Path.GetFileName(fileName);
				Logger.LogError("GetLogFile(fileName={FileName}): cleanedFileName → {CleanedFileName}", fileName, cleanedFileName);
                var logFilePath = Path.Combine(Globals.ApplicationMapPath, @"portals\_default\logs", cleanedFileName);
				Logger.LogError("GetLogFile(fileName={FileName}): logFilePath → {LogFilePath}", fileName, logFilePath);
                return this.CreateLogFileResponse(logFilePath);
            }
            catch (ArgumentException exc)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetLogFile(fileName={FileName})", fileName);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetUpgradeLogFile(string logName)
        {
			Logger.LogError(">>> Entering GetUpgradeLogFile(logName={LogName})", logName);
            try
            {
                var providerPath = DataProvider.Instance().GetProviderPath();
				Logger.LogError("GetUpgradeLogFile(logName={LogName}): providerPath → {ProviderPath}", logName, providerPath);
                var cleanedLogName = Path.GetFileName(logName);
				Logger.LogError("GetUpgradeLogFile(logName={LogName}): cleanedLogName → {CleanedLogName}", logName, cleanedLogName);
                var logFilePath = Path.Combine(providerPath, cleanedLogName);
				Logger.LogError("GetUpgradeLogFile(logName={LogName}): logFilePath → {LogFilePath}", logName, logFilePath);
                return this.CreateLogFileResponse(logFilePath);
            }
            catch (ArgumentException exc)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetUpgradeLogFile(logName={LogName})", logName);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [NonAction]
        private static void ValidateFilePath(string physicalPath)
        {
			Logger.LogError(">>> Entering ValidateFilePath(physicalPath={PhysicalPath})", physicalPath);
            var fileInfo = new FileInfo(physicalPath);
			Logger.LogError("ValidateFilePath(physicalPath={PhysicalPath}): fileInfo → {FileInfo}", physicalPath, fileInfo);
            if (!fileInfo.DirectoryName.StartsWith(Globals.ApplicationMapPath, StringComparison.InvariantCultureIgnoreCase))
            {
				Logger.LogError("Throwing ArgumentException from ValidateFilePath(physicalPath={PhysicalPath})", physicalPath);
                throw new ArgumentException("Invalid File Path");
            }
			Logger.LogWarning("<<< Exiting ValidateFilePath(physicalPath={PhysicalPath})", physicalPath);
        }

        [NonAction]
        private HttpResponseMessage CreateLogFileResponse(string logFilePath)
        {
			Logger.LogError(">>> Entering CreateLogFileResponse(logFilePath={LogFilePath})", logFilePath);
            ValidateFilePath(logFilePath);
            if (!File.Exists(logFilePath))
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }

            using var reader = File.OpenText(logFilePath);
			Logger.LogError("CreateLogFileResponse(logFilePath={LogFilePath}): reader → {Reader}", logFilePath, reader);
            var logText = reader.ReadToEnd();
			Logger.LogError("CreateLogFileResponse(logFilePath={LogFilePath}): logText → {LogText}", logFilePath, logText);
			Logger.LogWarning("<<< Exiting CreateLogFileResponse(logFilePath={LogFilePath})", logFilePath);
            return this.Request.CreateResponse(HttpStatusCode.OK, logText);
        }
    }
}
