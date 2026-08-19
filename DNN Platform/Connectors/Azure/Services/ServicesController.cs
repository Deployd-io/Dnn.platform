// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AzureConnector.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Providers.FolderProviders.AzureFolderProvider;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.WindowsAzure.Storage;
	using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    [DnnAuthorize]
    public class ServicesController : DnnApiController
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly IFolderMappingController folderMappingController;

        /// <summary>Initializes a new instance of the <see cref="ServicesController"/> class.</summary>
        /// <param name="folderMappingController">The folder mapping controller.</param>
        public ServicesController(IFolderMappingController folderMappingController)
        {
			_logger.LogDebug(">>> Entering ServicesController(folderMappingController=<redacted>)");
            this.folderMappingController = folderMappingController;
			_logger.LogDebug("ServicesController(folderMappingController=<redacted>): folderMappingController → {FolderMappingController1}", folderMappingController);
			_logger.LogDebug("ServicesController(folderMappingController=<redacted>): this.folderMappingController → {Arg1}", this.folderMappingController);
			_logger.LogDebug("<<< Exiting ServicesController(folderMappingController=<redacted>)");
        }

        /// <summary>Gets all containers.</summary>
        /// <param name="id">The folder mapping ID.</param>
        /// <returns>An <see cref="HttpResponseMessage"/> wrapping a <see cref="List{T}"/> of <see cref="string"/>.</returns>
        [HttpGet]
        public HttpResponseMessage GetAllContainers(int id)
        {
			_logger.LogDebug(">>> Entering GetAllContainers(id={Id})", id);
            try
            {
                var containers = new List<string>();
				_logger.LogDebug("GetAllContainers(id={Id}): containers → {Containers}", id, containers);
                var folderProvider = ActivatorUtilities.GetServiceOrCreateInstance<AzureFolderProvider>(HttpContextSource.Current.GetScope().ServiceProvider);
				_logger.LogDebug("GetAllContainers(id={Id}): folderProvider → {FolderProvider}", id, folderProvider);
                var folderMapping = Components.AzureConnector.FindAzureFolderMappingStatic(this.folderMappingController, this.PortalSettings.PortalId, id, false);
				_logger.LogDebug("GetAllContainers(id={Id}): folderMapping → {FolderMapping}", id, folderMapping);
                if (folderMapping != null)
                {
                    containers = folderProvider.GetAllContainers(folderMapping);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, containers);
            }
            catch (StorageException ex)
            {
				_logger.LogError("Exception in GetAllContainers(id={Id}): {Arg1}", id, ex.getMessage());
                Exceptions.LogException(ex);
                var message = ex.RequestInformation.HttpStatusMessage ?? ex.Message;
				_logger.LogDebug("GetAllContainers(id={Id}): message → {Message}", id, message);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = message });
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                const string message = "An error has occurred connecting to the Azure account.";
				_logger.LogDebug("<<< Exiting GetAllContainers(id={Id})", id);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = message });
            }
        }

        /// <summary>Gets the folder mapping ID.</summary>
        /// <returns>An <see cref="HttpResponseMessage"/> wrapping the folder mapping ID.</returns>
        [HttpGet]
        public HttpResponseMessage GetFolderMappingId()
        {
			_logger.LogDebug(">>> Entering GetFolderMappingId()");
			_logger.LogDebug("<<< Exiting GetFolderMappingId()");
            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                Components.AzureConnector.FindAzureFolderMappingStatic(this.folderMappingController, this.PortalSettings.PortalId).FolderMappingID);
        }
    }
}
