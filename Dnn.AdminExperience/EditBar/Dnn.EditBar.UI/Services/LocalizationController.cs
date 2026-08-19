// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.EditBar.UI.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web;
    using System.Web.Http;

    using Dnn.EditBar.Library;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Web.Api;
    using Newtonsoft.Json;

    [DnnAuthorize]
    public class LocalizationController : DnnApiController
    {
        private static object threadLocker = new object();

        /// <summary>Retrieve a list of CMX related Localization Keys with its values for the current culture.</summary>
        /// <param name="culture">The culture name.</param>
        /// <returns>A response with a collection of resources.</returns>
        [HttpGet]
        public HttpResponseMessage GetTable(string culture)
        {
			eventLogger.LogDebug(">>> Entering GetTable(culture={Culture})", culture);
            try
            {
                var resources = GetResourcesFromFile(culture);
				eventLogger.LogDebug("GetTable(culture={Culture}): resources → {Resources}", culture, resources);
                if (resources == null)
                {
                    lock (threadLocker)
                    {
                        resources = GetResourcesFromFile(culture);
                        if (resources == null)
                        {
                            resources = GenerateJsonFile(culture);
                        }
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, resources);
            }
            catch (Exception ex)
            {
				eventLogger.LogError("Exception in GetTable(culture={Culture}): {Arg1}", culture, ex.getMessage());
                Exceptions.LogException(ex);
				eventLogger.LogDebug("<<< Exiting GetTable(culture={Culture})", culture);
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }

        private static IDictionary<string, IDictionary<string, string>> GetResourcesFromFile(string culture)
        {
            if (!Expired(culture))
            {
                var jsonFileContent = GetJsonFileContent(culture);
                return jsonFileContent != null
                    ? JsonConvert.DeserializeObject<IDictionary<string, IDictionary<string, string>>>(jsonFileContent)
                    : null;
            }

            return null;
        }

        private static bool Expired(string culture)
        {
			eventLogger.LogDebug(">>> Entering Expired(culture={Culture})", culture);
            var cacheKey = $"EditBarResources{culture}";
            if (DataCache.GetCache(cacheKey) != null)
            {
                return false;
            }

            var jsonFilePath = GetResourcesJsonFilePath(culture);
			eventLogger.LogDebug("Expired(culture={Culture}): jsonFilePath → {JsonFilePath}", culture, jsonFilePath);
            var jsonFile = new FileInfo(jsonFilePath);
			eventLogger.LogDebug("Expired(culture={Culture}): jsonFile → {JsonFile}", culture, jsonFile);
            if (!jsonFile.Exists)
            {
                return true;
            }

            var lastModifiedTime = jsonFile.LastWriteTime;
            var resourceFiles = GetAllResourceFiles(culture);
			eventLogger.LogDebug("Expired(culture={Culture}): resourceFiles → {ResourceFiles}", culture, resourceFiles);

            var expired = resourceFiles.Select(file => new FileInfo(file))
                .Any(resourceFile => resourceFile.LastWriteTime > lastModifiedTime);
            if (!expired)
            {
                DataCache.SetCache(cacheKey, new CacheDto(), TimeSpan.FromMinutes(5));
            }

			eventLogger.LogDebug("<<< Exiting Expired(culture={Culture})", culture);
            return expired;
        }

        private static string GetJsonFileContent(string culture)
        {
			eventLogger.LogDebug(">>> Entering GetJsonFileContent(culture={Culture})", culture);
            var path = GetResourcesJsonFilePath(culture);
			eventLogger.LogDebug("GetJsonFileContent(culture={Culture}): path → {Path}", culture, path);
			eventLogger.LogDebug("<<< Exiting GetJsonFileContent(culture={Culture})", culture);
            return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : null;
        }

        private static Dictionary<string, IDictionary<string, string>> GenerateJsonFile(string culture)
        {
            var resources = new Dictionary<string, IDictionary<string, string>>();
            var resourceFiles = GetAllResourceFiles(culture);
            var editBarResourcesPath = Path.Combine(Constants.EditBarRelativePath, "App_LocalResources");
            foreach (var resourcesFile in resourceFiles)
            {
                var key = Path.GetFileNameWithoutExtension(resourcesFile);
                var filename = Path.GetFileName(resourcesFile);
                var relativePath = Path.Combine(editBarResourcesPath, filename);
                var keys = EditBar.UI.Controllers.LocalizationController.Instance.GetLocalizedDictionary(relativePath, culture);
                resources.Add(key, keys);
            }

            var content = JsonConvert.SerializeObject(resources);
            var filePath = GetResourcesJsonFilePath(culture);
            var folderPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            File.WriteAllText(filePath, content, Encoding.UTF8);

            return resources;
        }

        private static string GetResourcesJsonFilePath(string culture)
        {
			eventLogger.LogDebug(">>> Entering GetResourcesJsonFilePath(culture={Culture})", culture);
            var path = Path.Combine(Constants.EditBarRelativePath, "Resources", $"LocalResources.{culture}.resources");
			eventLogger.LogDebug("GetResourcesJsonFilePath(culture={Culture}): path → {Path}", culture, path);
			eventLogger.LogDebug("<<< Exiting GetResourcesJsonFilePath(culture={Culture})", culture);
            return HttpContext.Current.Server.MapPath(path);
        }

        private static string[] GetAllResourceFiles(string culture)
        {
			eventLogger.LogDebug(">>> Entering GetAllResourceFiles(culture={Culture})", culture);
            var editBarResourcesPath = Path.Combine(Constants.EditBarRelativePath, "App_LocalResources");
            var physicalPath = HttpContext.Current.Server.MapPath(editBarResourcesPath);
			eventLogger.LogDebug("GetAllResourceFiles(culture={Culture}): physicalPath → {PhysicalPath}", culture, physicalPath);
			eventLogger.LogDebug("GetAllResourceFiles(culture={Culture}): editBarResourcesPath → {EditBarResourcesPath}", culture, editBarResourcesPath);
			eventLogger.LogDebug("<<< Exiting GetAllResourceFiles(culture={Culture})", culture);
            return Directory.GetFiles(physicalPath, "*.resx");
        }
    }
}
