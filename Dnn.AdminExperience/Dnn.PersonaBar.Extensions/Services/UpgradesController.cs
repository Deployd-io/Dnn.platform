// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Services
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Dnn.PersonaBar.Extensions.Services.Dto;
    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api.Internal;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class UpgradesController : PersonaBarApiController
    {
        private const string ResourceFile = "~/DesktopModules/Admin/Dnn.PersonaBar/Modules/Dnn.Servers/App_LocalResources/Servers.resx";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UpgradesController));
        private readonly IApplicationStatusInfo applicationStatusInfo;
        private readonly ILocalUpgradeService localUpgradeService;

        /// <summary>Initializes a new instance of the <see cref="UpgradesController"/> class.</summary>
        /// <param name="applicationStatusInfo">The application status.</param>
        /// <param name="localUpgradeService">The local upgrade service.</param>
        public UpgradesController(IApplicationStatusInfo applicationStatusInfo, ILocalUpgradeService localUpgradeService)
        {
			Logger.LogDebug(">>> Entering UpgradesController(applicationStatusInfo={ApplicationStatusInfo},localUpgradeService={LocalUpgradeService})", applicationStatusInfo, localUpgradeService);
            this.applicationStatusInfo = applicationStatusInfo;
            this.localUpgradeService = localUpgradeService;
			Logger.LogDebug("UpgradesController(applicationStatusInfo={ApplicationStatusInfo},localUpgradeService={LocalUpgradeService}): localUpgradeService → {LocalUpgradeService1}", applicationStatusInfo, localUpgradeService, localUpgradeService);
			Logger.LogDebug("UpgradesController(applicationStatusInfo={ApplicationStatusInfo},localUpgradeService={LocalUpgradeService}): this.applicationStatusInfo → {Arg2}", applicationStatusInfo, localUpgradeService, this.applicationStatusInfo);
			Logger.LogDebug("<<< Exiting UpgradesController(applicationStatusInfo={ApplicationStatusInfo},localUpgradeService={LocalUpgradeService})", applicationStatusInfo, localUpgradeService);
        }

        /// <summary>Retrieves upgrade settings.</summary>
        /// <returns>A <see cref="HttpResponseMessage"/> containing the upgrade settings with a status code
        /// of <see cref="HttpStatusCode.OK"/> if successful, or an error message with a status code of
        /// <see cref="HttpStatusCode.InternalServerError"/> if an exception occurs.</returns>
        [HttpGet]
        public HttpResponseMessage GetSettings()
        {
			Logger.LogDebug(">>> Entering GetSettings()");
            try
            {
                if (!bool.TryParse(DotNetNuke.Common.Utilities.Config.GetSetting("AllowDnnUpgradeUpload"), out var allowDnnUpgradeUpload))
                {
                    allowDnnUpgradeUpload = false;
					Logger.LogDebug("GetSettings(): allowDnnUpgradeUpload → {AllowDnnUpgradeUpload}", allowDnnUpgradeUpload);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    AllowDnnUpgradeUpload = allowDnnUpgradeUpload,
                });
				Logger.LogDebug("GetSettings(): AllowDnnUpgradeUpload → {AllowDnnUpgradeUpload}", AllowDnnUpgradeUpload);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in GetSettings(): {Arg0}", ex.getMessage());
				Logger.LogDebug("<<< Exiting GetSettings()");
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message, });
            }
        }

        /// <summary>Retrieves a list of local upgrades.</summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="HttpResponseMessage"/> containing the list of local upgrades with a status code
        /// of <see cref="HttpStatusCode.OK"/> if successful, or an error message with a status code of
        /// <see cref="HttpStatusCode.InternalServerError"/> if an exception occurs.</returns>
        [HttpGet]
        public async Task<HttpResponseMessage> List(CancellationToken cancellationToken)
        {
			Logger.LogDebug(">>> Entering List(cancellationToken=<redacted>)");
            try
            {
                var upgrades = await this.localUpgradeService.GetLocalUpgrades(cancellationToken);
				Logger.LogDebug("List(cancellationToken=<redacted>): upgrades → {Upgrades}", upgrades);

                return this.Request.CreateResponse(HttpStatusCode.OK, upgrades);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in List(cancellationToken=<redacted>): {Arg1}", ex.getMessage());
				Logger.LogDebug("<<< Exiting List(cancellationToken=<redacted>)");
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        /// <summary>Starts a local upgrade.</summary>
        /// <param name="data">The DTO containing the packageName.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An empty success response.</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> StartUpgrade(UpgradePackageRequestDto data, CancellationToken cancellationToken)
        {
			Logger.LogDebug(">>> Entering StartUpgrade(data={Data},cancellationToken=<redacted>)", data);
            var upgrades = await this.localUpgradeService.GetLocalUpgrades(cancellationToken);
            if (!upgrades.Any(u => u.IsValid && !u.IsOutdated))
			Logger.LogDebug("StartUpgrade(data={Data},cancellationToken=<redacted>): upgrades → {Upgrades}", data, upgrades);
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = LocalizeString("Upgrade_NoUpgrade"), });
            }

            var upgrade = upgrades.FirstOrDefault(u => u.PackageName.Equals(data.PackageName, StringComparison.OrdinalIgnoreCase));
			Logger.LogDebug("StartUpgrade(data={Data},cancellationToken=<redacted>): upgrade → {Upgrade}", data, upgrade);
            if (upgrade is not { CanInstall: true, })
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = LocalizeString($"Upgrade_NoValidUpgrade", data.PackageName), });
            }

            await this.localUpgradeService.StartLocalUpgrade(upgrade, cancellationToken);

            await this.localUpgradeService.DeleteLocalUpgrade(data.PackageName, cancellationToken);

			Logger.LogDebug("<<< Exiting StartUpgrade(data={Data},cancellationToken=<redacted>)", data);
            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }

        /// <summary>Starts a local upgrade.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An empty success response.</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> StartFirstUpgrade(CancellationToken cancellationToken)
        {
			Logger.LogDebug(">>> Entering StartFirstUpgrade(cancellationToken=<redacted>)");
            var upgrades = await this.localUpgradeService.GetLocalUpgrades(cancellationToken);
            if (!upgrades.Any(u => u.IsValid && !u.IsOutdated))
			Logger.LogDebug("StartFirstUpgrade(cancellationToken=<redacted>): upgrades → {Upgrades}", upgrades);
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = LocalizeString("Upgrade_NoUpgrade"), });
            }

            await this.localUpgradeService.StartLocalUpgrade(upgrades, cancellationToken);

			Logger.LogDebug("<<< Exiting StartFirstUpgrade(cancellationToken=<redacted>)");
            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }

        /// <summary>Deletes a local upgrade package.</summary>
        /// <param name="data">The DTO containing the packageName.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An empty success response.</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> Delete(UpgradePackageRequestDto data, CancellationToken cancellationToken)
        {
			Logger.LogDebug(">>> Entering Delete(data={Data},cancellationToken=<redacted>)", data);
            try
            {
                await this.localUpgradeService.DeleteLocalUpgrade(data.PackageName, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
			Logger.LogDebug("<<< Exiting Delete(data={Data},cancellationToken=<redacted>)", data);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { message = ex.Message, });
            }

            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }

        /// <summary>Uploads a DNN package for local upgrade.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Either a package info or an error message.</returns>
        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        public Task<HttpResponseMessage> Upload(CancellationToken cancellationToken)
        {
			Logger.LogDebug(">>> Entering Upload(cancellationToken=<redacted>)");
            try
            {
                if (!bool.TryParse(DotNetNuke.Common.Utilities.Config.GetSetting("AllowDnnUpgradeUpload"), out var allowDnnUpgradeUpload))
                {
                    allowDnnUpgradeUpload = false;
					Logger.LogDebug("Upload(cancellationToken=<redacted>): allowDnnUpgradeUpload → {AllowDnnUpgradeUpload}", allowDnnUpgradeUpload);
                }

                if (!allowDnnUpgradeUpload)
                {
                    return Task.FromResult(this.Request.CreateResponse(HttpStatusCode.Forbidden, new { message = LocalizeString("Upgrade_UploadNotAllowed"), }));
                }

                return this.UploadFileAction(cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting Upload(cancellationToken=<redacted>)");
                return Task.FromResult(this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { message = ex.Message, }));
            }
        }

        /// <summary>
        /// Completes the uploads of a DNN package for local upgrade.
        /// </summary>
        /// <param name="data">The upload complete data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Either a package info or an error message.</returns>
        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> UploadComplete(UpgradeUploadCompleteDto data, CancellationToken cancellationToken)
        {
			Logger.LogDebug(">>> Entering UploadComplete(data={Data},cancellationToken=<redacted>)", data);
            var fileName = Path.GetFileName(data.FileName);
            var fileId = Path.GetFileName(data.FileId);
			Logger.LogDebug("UploadComplete(data={Data},cancellationToken=<redacted>): fileId → {FileId}", data, fileId);
			Logger.LogDebug("UploadComplete(data={Data},cancellationToken=<redacted>): fileName → {FileName}", data, fileName);

            if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(fileId))
            {
                var writtenFile = Path.Combine(this.applicationStatusInfo.ApplicationMapPath, "App_Data", "Upgrade", fileId + ".resources");
				Logger.LogDebug("UploadComplete(data={Data},cancellationToken=<redacted>): writtenFile → {WrittenFile}", data, writtenFile);
                if (!File.Exists(writtenFile))
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = LocalizeString($"Upgrade_InvalidPackage", fileName), });
                }

                var info = await this.localUpgradeService.GetLocalUpgradeInfo(writtenFile, cancellationToken);
				Logger.LogDebug("UploadComplete(data={Data},cancellationToken=<redacted>): info → {Info}", data, info);
                if (!info.IsValid)
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = LocalizeString($"Upgrade_InvalidPackage", fileName), });
                }
                else if (info.IsOutdated)
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = LocalizeString($"Upgrade_OutdatedPackage", fileName), });
                }
                else
                {
                    var upgradePath = Path.Combine(this.applicationStatusInfo.ApplicationMapPath, "App_Data", "Upgrade", Path.GetFileNameWithoutExtension(fileName) + ".zip");
					Logger.LogDebug("UploadComplete(data={Data},cancellationToken=<redacted>): upgradePath → {UpgradePath}", data, upgradePath);
                    File.Move(writtenFile, upgradePath);
                    return this.Request.CreateResponse(HttpStatusCode.OK, fileName);
                }
            }

			Logger.LogDebug("<<< Exiting UploadComplete(data={Data},cancellationToken=<redacted>)", data);
            return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = LocalizeString($"Upgrade_InvalidPackage", fileName), });
        }

        private static string LocalizeString(string key, params object[] args)
        {
			Logger.LogDebug(">>> Entering LocalizeString(key={Key},args={Args})", key, args);
			Logger.LogDebug("<<< Exiting LocalizeString(key={Key},args={Args})", key, args);
            return string.Format(CultureInfo.CurrentCulture, Localization.GetString(key, ResourceFile), args);
        }

        private async Task<HttpResponseMessage> UploadFileAction(CancellationToken cancellationToken)
        {
			Logger.LogDebug(">>> Entering UploadFileAction(cancellationToken=<redacted>)");
            var request = this.Request;
            if (!request.Content.IsMimeMultipartContent())
            {
				Logger.LogWarning("Throwing HttpResponseException from UploadFileAction(cancellationToken=<redacted>)");
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            // Ensure content is buffered (helps if something read headers but not the body).
            // Note: if a filter already consumed the body, buffering here won't restore it —
            // the filter must use LoadIntoBufferAsync or otherwise avoid consuming the stream.
            await request.Content.LoadIntoBufferAsync().ConfigureAwait(false);

            var provider = new MultipartMemoryStreamProvider();
			Logger.LogDebug("UploadFileAction(cancellationToken=<redacted>): provider → {Provider}", provider);

            // read multipart parts
            await request.Content.ReadAsMultipartAsync(provider, cancellationToken).ConfigureAwait(false);

            long startPosition = 0;
			Logger.LogDebug("UploadFileAction(cancellationToken=<redacted>): startPosition → {StartPosition}", startPosition);
            long totalSize = 0;
			Logger.LogDebug("UploadFileAction(cancellationToken=<redacted>): totalSize → {TotalSize}", totalSize);
            string fileId = string.Empty;
			Logger.LogDebug("UploadFileAction(cancellationToken=<redacted>): fileId → {FileId}", fileId);
            Stream stream = null;
			Logger.LogDebug("UploadFileAction(cancellationToken=<redacted>): stream → {Stream}", stream);

            foreach (var item in provider.Contents)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var name = item.Headers.ContentDisposition.Name;
				Logger.LogDebug("UploadFileAction(cancellationToken=<redacted>): name → {Name}", name);
                switch (name.ToUpperInvariant())
                {
                    case "\"CHUNK\"":
                        stream = await item.ReadAsStreamAsync();
                        break;
                    case "\"START\"":
                        if (!long.TryParse(await item.ReadAsStringAsync(), out startPosition))
                        {
                            startPosition = 0;
                        }

                        break;
                    case "\"TOTALSIZE\"":
                        if (!long.TryParse(await item.ReadAsStringAsync(), out totalSize))
                        {
                            totalSize = 0;
                        }

                        break;
                    case "\"FILEID\"":
                        fileId = await item.ReadAsStringAsync();
                        break;
                }
            }

            if (stream != null && totalSize != 0 && !string.IsNullOrEmpty(fileId))
            {
                var fileToWriteTo = Path.Combine(this.applicationStatusInfo.ApplicationMapPath, "App_Data", "Upgrade", fileId + ".resources");
				Logger.LogDebug("UploadFileAction(cancellationToken=<redacted>): fileToWriteTo → {FileToWriteTo}", fileToWriteTo);

                // use the file stream and start position to write the chunk to the file
                using var fileStream = new FileStream(fileToWriteTo, FileMode.OpenOrCreate, FileAccess.Write);
				Logger.LogDebug("UploadFileAction(cancellationToken=<redacted>): fileStream → {FileStream}", fileStream);
				Logger.LogDebug("UploadFileAction(cancellationToken=<redacted>): fileStream.Position → {Arg1}", fileStream.Position);
                fileStream.Position = startPosition;
                await stream.CopyToAsync(fileStream).ConfigureAwait(false);
            }

			Logger.LogDebug("<<< Exiting UploadFileAction(cancellationToken=<redacted>)");
            return request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
