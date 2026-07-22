// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Services
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Servers.Components.DatabaseServer;
    using DotNetNuke.Instrumentation;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class SystemInfoDatabaseController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SystemInfoDatabaseController));
        private readonly DatabaseController databaseController = new DatabaseController();

        [HttpGet]
        public HttpResponseMessage GetDatabaseServerInfo()
        {
			Logger.LogDebug(">>> Entering GetDatabaseServerInfo()");
            try
            {
                var dbInfo = this.databaseController.GetDbInfo();
                var dbBackups = this.databaseController.GetDbBackups().Select(b => new
                {
				Logger.LogDebug("GetDatabaseServerInfo(): dbInfo → {DbInfo}", dbInfo);
                    name = b.Name,
                    startDate = b.StartDate,
                    finishDate = b.FinishDate,
                    size = b.Size,
                    backupType = b.BackupType,
                });
					Logger.LogDebug("GetDatabaseServerInfo(): name → {Name}", name);
					Logger.LogDebug("GetDatabaseServerInfo(): startDate → {StartDate}", startDate);
					Logger.LogDebug("GetDatabaseServerInfo(): finishDate → {FinishDate}", finishDate);
					Logger.LogDebug("GetDatabaseServerInfo(): size → {Size}", size);
					Logger.LogDebug("GetDatabaseServerInfo(): backupType → {BackupType}", backupType);
                var dbFileInfo = this.databaseController.GetDbFileInfo().Select(f => new
                {
                    name = f.Name,
                    size = f.Megabytes,
                    fileType = f.FileType,
                    fileName = f.ShortFileName,
                });
					Logger.LogDebug("GetDatabaseServerInfo(): fileType → {FileType}", fileType);
					Logger.LogDebug("GetDatabaseServerInfo(): fileName → {FileName}", fileName);

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    productVersion = dbInfo.ProductVersion,
                    servicePack = dbInfo.ServicePack,
                    productEdition = dbInfo.ProductEdition,
                    softwarePlatform = dbInfo.SoftwarePlatform,
                    backups = dbBackups,
                    files = dbFileInfo,
                });
					Logger.LogDebug("GetDatabaseServerInfo(): productVersion → {ProductVersion}", productVersion);
					Logger.LogDebug("GetDatabaseServerInfo(): servicePack → {ServicePack}", servicePack);
					Logger.LogDebug("GetDatabaseServerInfo(): productEdition → {ProductEdition}", productEdition);
					Logger.LogDebug("GetDatabaseServerInfo(): softwarePlatform → {SoftwarePlatform}", softwarePlatform);
					Logger.LogDebug("GetDatabaseServerInfo(): backups → {Backups}", backups);
					Logger.LogDebug("GetDatabaseServerInfo(): files → {Files}", files);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetDatabaseServerInfo()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}
