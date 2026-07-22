// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.ExportImport.Services
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Controllers;
    using Dnn.ExportImport.Components.Dto;

    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;

    /// <summary>REST APIs to manage import/export jobs.</summary>
    [RequireHost]
    public class ExportImportController : DnnApiController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly ExportController exportController;
        private readonly ImportController importController;
        private readonly BaseController baseController;

        /// <summary>Initializes a new instance of the <see cref="ExportImportController"/> class.</summary>
        /// <param name="exportController">The export controller.</param>
        /// <param name="importController">The import controller.</param>
        /// <param name="baseController">The base controller.</param>
        public ExportImportController(ExportController exportController, ImportController importController, BaseController baseController)
        {
			Logger.LogDebug(">>> Entering ExportImportController(exportController={ExportController},importController={ImportController},baseController={BaseController})", exportController, importController, baseController);
            this.exportController = exportController;
            this.importController = importController;
			Logger.LogDebug("ExportImportController(exportController={ExportController},importController={ImportController},baseController={BaseController}): this.exportController → {Arg3}", exportController, importController, baseController, this.exportController);
			Logger.LogDebug("ExportImportController(exportController={ExportController},importController={ImportController},baseController={BaseController}): importController → {ImportController1}", exportController, importController, baseController, importController);
			Logger.LogDebug("ExportImportController(exportController={ExportController},importController={ImportController},baseController={BaseController}): this.importController → {Arg3}", exportController, importController, baseController, this.importController);
			Logger.LogDebug("ExportImportController(exportController={ExportController},importController={ImportController},baseController={BaseController}): exportController → {ExportController1}", exportController, importController, baseController, exportController);
            this.baseController = baseController;
			Logger.LogDebug("<<< Exiting ExportImportController(exportController={ExportController},importController={ImportController},baseController={BaseController})", exportController, importController, baseController);
			Logger.LogDebug("ExportImportController(exportController={ExportController},importController={ImportController},baseController={BaseController}): baseController → {BaseController1}", exportController, importController, baseController, baseController);
			Logger.LogDebug("ExportImportController(exportController={ExportController},importController={ImportController},baseController={BaseController}): this.baseController → {Arg3}", exportController, importController, baseController, this.baseController);
        }

        /// <summary>Exports the specified site.</summary>
        /// <param name="exportDto">The details needed to export a site.</param>
        /// <returns>OK.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Export(ExportDto exportDto)
        {
			Logger.LogDebug(">>> Entering Export(exportDto={ExportDto})", exportDto);
            var jobId = this.exportController.QueueOperation(this.PortalSettings.UserId, exportDto);

			Logger.LogDebug("Export(exportDto={ExportDto}): jobId → {JobId}", exportDto, jobId);
			Logger.LogDebug("<<< Exiting Export(exportDto={ExportDto})", exportDto);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { jobId });
        }

        /// <summary>Imports a site.</summary>
        /// <param name="importDto">The details required to import a site.</param>
        /// <returns>OK or BadRequest with a message.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Import(ImportDto importDto)
        {
			Logger.LogDebug(">>> Entering Import(importDto={ImportDto})", importDto);
            if (this.importController.VerifyImportPackage(importDto.PackageId, null, out var message))
            {
                var jobId = this.importController.QueueOperation(this.PortalSettings.UserId, importDto);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { jobId });
            }

			Logger.LogDebug("<<< Exiting Import(importDto={ImportDto})", importDto);
            return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
        }

        /// <summary>Verifies an import package validity.</summary>
        /// <param name="packageId">The ID of the package to validate.</param>
        /// <returns>OK with a summary or BadRequest with an error message.</returns>
        [HttpGet]
        public HttpResponseMessage VerifyImportPackage(string packageId)
        {
			Logger.LogDebug(">>> Entering VerifyImportPackage(packageId={PackageId})", packageId);
            var summary = new ImportExportSummary();
            var isValid = this.importController.VerifyImportPackage(packageId, summary, out var message);
			Logger.LogDebug("VerifyImportPackage(packageId={PackageId}): isValid → {IsValid}", packageId, isValid);
            summary.ConvertToLocal(this.UserInfo);
			Logger.LogDebug("<<< Exiting VerifyImportPackage(packageId={PackageId})", packageId);
            return isValid
                ? this.Request.CreateResponse(HttpStatusCode.OK, summary)
                : this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
        }

        /// <summary>Get list of packages to import.</summary>
        /// <param name="keyword">Keyword to search the import package. This will look into the package name and description.</param>
        /// <param name="order">Order by which the packages list should be sorted. Allowed values: newest, oldest, name.</param>
        /// <param name="pageIndex">Page index to get.</param>
        /// <param name="pageSize">Page size. Should not be more than 100.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>.</returns>
        [HttpGet]
        public HttpResponseMessage GetImportPackages(string keyword = "", string order = "newest", int pageIndex = 0, int pageSize = 10)
        {
			Logger.LogDebug(">>> Entering GetImportPackages(keyword={Keyword},order={Order},pageIndex={PageIndex},pageSize={PageSize})", keyword, order, pageIndex, pageSize);
            var packages = this.importController.GetImportPackages(out var total, keyword, order, pageIndex, pageSize).ToList();
            packages.ForEach(package => package.ConvertToLocal(this.UserInfo));
			Logger.LogDebug("GetImportPackages(keyword={Keyword},order={Order},pageIndex={PageIndex},pageSize={PageSize}): packages → {Packages}", keyword, order, pageIndex, pageSize, packages);
			Logger.LogDebug("<<< Exiting GetImportPackages(keyword={Keyword},order={Order},pageIndex={PageIndex},pageSize={PageSize})", keyword, order, pageIndex, pageSize);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { packages, total });
        }

        /// <summary>
        /// Cancels a job.
        /// </summary>
        /// <param name="jobId">The ID of the job to cancel.</param>
        /// <remarks>This is a POST so users can't cancel using a simple browser link.</remarks>
        /// <returns>OK or a BadRequest with a cancel status value.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CancelProcess([FromUri] int jobId)
        {
            var cancelStatus = this.baseController.CancelJob(this.UserInfo.IsSuperUser ? -1 : this.PortalSettings.PortalId, jobId);
            return this.Request.CreateResponse(
                cancelStatus ? HttpStatusCode.OK : HttpStatusCode.BadRequest, new { success = cancelStatus });
        }

        /// <summary>
        /// Removes a job.
        /// </summary>
        /// <param name="jobId">The ID of the job to remove.</param>
        /// <remarks>This is a POST so users can't remove a job using a browser link.</remarks>
        /// <returns>OK or a BadRequest with a status value.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RemoveJob([FromUri] int jobId)
        {
            var cancelStatus = this.baseController.RemoveJob(this.UserInfo.IsSuperUser ? -1 : this.PortalSettings.PortalId, jobId);
            return this.Request.CreateResponse(
                cancelStatus ? HttpStatusCode.OK : HttpStatusCode.BadRequest, new { success = cancelStatus });
        }

        /// <summary>
        /// Gets the last job time.
        /// </summary>
        /// <param name="portal">The ID of the portal to check.</param>
        /// <param name="jobType">Type of the job.</param>
        /// <returns>OK with the last time or a BadRequest with a message.</returns>
        [HttpGet]
        public HttpResponseMessage LastJobTime(int portal, JobType jobType)
        {
			Logger.LogDebug(">>> Entering LastJobTime(portal={Portal},jobType={JobType})", portal, jobType);
            if (!this.UserInfo.IsSuperUser && portal != this.PortalSettings.PortalId)
            {
                var error = Localization.GetString("NotPortalAdmin", Constants.SharedResources);
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }

            if (portal < 0)
            {
                return this.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    Localization.GetString("InvalidPortal", Constants.SharedResources));
            }

            var lastTime = this.baseController.GetLastJobTime(portal, jobType);
			Logger.LogDebug("LastJobTime(portal={Portal},jobType={JobType}): lastTime → {LastTime}", portal, jobType, lastTime);
			Logger.LogDebug("<<< Exiting LastJobTime(portal={Portal},jobType={JobType})", portal, jobType);
            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new { lastTime = Util.GetDateTimeString(lastTime) });
        }

        /// <summary>
        /// Gets all the jobs.
        /// </summary>
        /// <param name="portal">The ID of the portal to check.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="jobType">Type of the job.</param>
        /// <param name="keywords">The keywords to search for.</param>
        /// <returns>OK with a list of jobs or a BadRequest with an error message.</returns>
        [HttpGet]
        public HttpResponseMessage AllJobs(int portal, int? pageSize = 10, int? pageIndex = 0, int? jobType = null, string keywords = null)
        {
			Logger.LogDebug(">>> Entering AllJobs(portal={Portal},pageSize={PageSize},pageIndex={PageIndex},jobType={JobType},keywords={Keywords})", portal, pageSize, pageIndex, jobType, keywords);
            if (!this.UserInfo.IsSuperUser && portal != this.PortalSettings.PortalId)
            {
                var error = Localization.GetString("NotPortalAdmin", Constants.SharedResources);
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }

            var jobs = this.baseController.GetAllJobs(portal, this.PortalSettings.PortalId, pageSize, pageIndex, jobType, keywords);
            jobs?.ConvertToLocal(this.UserInfo);
			Logger.LogDebug("AllJobs(portal={Portal},pageSize={PageSize},pageIndex={PageIndex},jobType={JobType},keywords={Keywords}): jobs → {Jobs}", portal, pageSize, pageIndex, jobType, keywords, jobs);
			Logger.LogDebug("<<< Exiting AllJobs(portal={Portal},pageSize={PageSize},pageIndex={PageIndex},jobType={JobType},keywords={Keywords})", portal, pageSize, pageIndex, jobType, keywords);
            return this.Request.CreateResponse(HttpStatusCode.OK, jobs);
        }

        /// <summary>
        /// Jobs the details about a single job.
        /// </summary>
        /// <param name="jobId">The ID of the job to get.</param>
        /// <returns>OK with the job details or BadRequest with an error message.</returns>
        [HttpGet]
        public HttpResponseMessage JobDetails(int jobId)
        {
            var job = this.baseController.GetJobDetails(this.UserInfo.IsSuperUser ? -1 : this.PortalSettings.PortalId, jobId);
            job?.ConvertToLocal(this.UserInfo);
            return job != null
                ? this.Request.CreateResponse(HttpStatusCode.OK, job)
                : this.Request.CreateResponse(
                    HttpStatusCode.BadRequest,
                    new { message = Localization.GetString("JobNotExist", Constants.SharedResources) });
        }
    }
}
