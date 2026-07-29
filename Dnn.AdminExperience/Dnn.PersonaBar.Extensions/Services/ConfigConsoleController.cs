// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.ConfigConsole.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Dnn.PersonaBar.ConfigConsole.Services.Dto;
    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;

    using DotNetNuke.Common;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The web API controller for the Config Console component in the Persona Bar.</summary>
    [MenuPermission(Scope = ServiceScope.Host)]
    public class ConfigConsoleController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ConfigConsoleController));
        private Components.ConfigConsoleController controller;

        /// <summary>Initializes a new instance of the <see cref="ConfigConsoleController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public ConfigConsoleController()
            : this(null)
        {
		Logger.LogDebug(">>> Entering ConfigConsoleController()");
		Logger.LogDebug("<<< Exiting ConfigConsoleController()");
		Logger.LogDebug(">>> Entering ConfigConsoleController(controller={Controller})", controller);
		Logger.LogDebug("<<< Exiting ConfigConsoleController(controller={Controller})", controller);
        }

        /// <summary>Initializes a new instance of the <see cref="ConfigConsoleController"/> class.</summary>
        /// <param name="controller">The controller.</param>
        public ConfigConsoleController(Components.ConfigConsoleController controller)
        {
            this.controller = controller ?? Globals.GetCurrentServiceProvider().GetRequiredService<Components.ConfigConsoleController>();
			Logger.LogDebug("ConfigConsoleController(controller={Controller}): controller → {Controller1}", controller, controller);
        }

        /// GET: api/ConfigConsole/GetConfigFilesList
        /// <summary>Gets list of config files.A response indicating success.</summary>
        /// <returns>List of config files.</returns>
        [HttpGet]
        public async Task<HttpResponseMessage> GetConfigFilesList()
        {
			Logger.LogDebug(">>> Entering GetConfigFilesList()");
            try
            {
                var configFileList = (await this.controller.GetConfigFilesListAsync()).ToList();

				Logger.LogDebug("GetConfigFilesList(): configFileList → {ConfigFileList}", configFileList);
                var response = new
                {
                    Success = true,
                    Results = configFileList,
                    TotalResults = configFileList.Count,
                };
					Logger.LogDebug("GetConfigFilesList(): Success → {Success}", Success);
					Logger.LogDebug("GetConfigFilesList(): Results → {Results}", Results);
					Logger.LogDebug("GetConfigFilesList(): TotalResults → {TotalResults}", TotalResults);

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetConfigFilesList()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/ConfigConsole/GetConfigFile
        /// <summary>Gets content of a specific config file.</summary>
        /// <param name="fileName">Name of a config file.</param>
        /// <returns>Content of a config file.</returns>
        [HttpGet]
        public async Task<HttpResponseMessage> GetConfigFile(string fileName)
        {
			Logger.LogDebug(">>> Entering GetConfigFile(fileName={FileName})", fileName);
            try
            {
                var response = new
                {
                    FileName = fileName,
                    FileContent = await this.controller.GetConfigFileAsync(fileName),
                };
					Logger.LogDebug("GetConfigFile(fileName={FileName}): FileName → {FileName1}", fileName, FileName);
					Logger.LogDebug("GetConfigFile(fileName={FileName}): FileContent → {FileContent}", fileName, FileContent);

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (ArgumentException exc)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetConfigFile(fileName={FileName})", fileName);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/ConfigConsole/ValidateConfigFile
        /// <summary>Validates a config file against a well known schema.</summary>
        /// <param name="configFileDto">Content of config file.</param>
        /// <returns>A list of validation errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> ValidateConfigFile(ConfigFileDto configFileDto)
        {
			Logger.LogDebug(">>> Entering ValidateConfigFile(configFileDto={ConfigFileDto})", configFileDto);
            try
            {
                var response = new
                {
                    ValidationErrors = await this.controller.ValidateConfigFileAsync(configFileDto.FileName, configFileDto.FileContent),
                };
					Logger.LogDebug("ValidateConfigFile(configFileDto={ConfigFileDto}): ValidationErrors → {ValidationErrors}", configFileDto, ValidationErrors);
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (ArgumentException exc)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting ValidateConfigFile(configFileDto={ConfigFileDto})", configFileDto);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/ConfigConsole/UpdateConfigFile
        /// <summary>Updates a config file.</summary>
        /// <param name="configFileDto">Content of config file.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> UpdateConfigFile(ConfigFileDto configFileDto)
        {
			Logger.LogDebug(">>> Entering UpdateConfigFile(configFileDto={ConfigFileDto})", configFileDto);
            try
            {
                await this.controller.UpdateConfigFileAsync(configFileDto.FileName, configFileDto.FileContent);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (ArgumentException exc)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateConfigFile(configFileDto={ConfigFileDto})", configFileDto);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/ConfigConsole/MergeConfigFile
        /// <summary>Merges config files.</summary>
        /// <param name="configFileDto">Content of config file.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MergeConfigFile(ConfigFileDto configFileDto)
        {
			Logger.LogDebug(">>> Entering MergeConfigFile(configFileDto={ConfigFileDto})", configFileDto);
            try
            {
                this.controller.MergeConfigFile(configFileDto.FileContent);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting MergeConfigFile(configFileDto={ConfigFileDto})", configFileDto);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}
