// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.UI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Model;
    using Dnn.PersonaBar.Library.Repository;

    using DotNetNuke.Common;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An API controller for the menu of the Persona Bar.</summary>
    [MenuPermission(Scope = ServiceScope.Regular)]
    public class MenuExtensionsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MenuExtensionsController));
        private readonly IServiceProvider serviceProvider;

        /// <summary>Initializes a new instance of the <see cref="MenuExtensionsController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public MenuExtensionsController()
            : this(Globals.GetCurrentServiceProvider())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MenuExtensionsController"/> class.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        public MenuExtensionsController(IServiceProvider serviceProvider)
        {
			Logger.LogError(">>> Entering MenuExtensionsController(serviceProvider={ServiceProvider})", serviceProvider);
            this.serviceProvider = serviceProvider;
			Logger.LogDebug("MenuExtensionsController(serviceProvider={ServiceProvider}): serviceProvider → {ServiceProvider1}", serviceProvider, serviceProvider);
        Logger.LogWarning("<<< Exiting MenuExtensionsController(serviceProvider={ServiceProvider})", serviceProvider);
        }

        /// <summary>Retrieve a list of extensions for menu.</summary>
        /// <param name="menu">The menu identifier.</param>
        /// <returns>A response with a collection of extension info.</returns>
        [HttpGet]
        public HttpResponseMessage GetExtensions(string menu)
        {
			Logger.LogError(">>> Entering GetExtensions(menu={Menu})", menu);
            try
            {
                var menuItem = PersonaBarRepository.Instance.GetMenuItem(menu);
				Logger.LogError("GetExtensions(menu={Menu}): menuItem → {MenuItem}", menu, menuItem);
                if (menuItem != null)
                {
                    var extensions = PersonaBarExtensionRepository.Instance.GetExtensions(menuItem.MenuId)
                        .Where(this.IsVisible)
                        .Select(t => new
                        {
                            identifier = t.Identifier,
                            folderName = t.FolderName,
                            container = t.Container,
                            path = this.GetExtensionPathByController(t),
                            settings = this.GetExtensionSettings(t),
                        });

                    return this.Request.CreateResponse(HttpStatusCode.OK, extensions);
                }

                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in GetExtensions(menu={Menu}): {Arg1}", menu, ex.Message);
                Exceptions.LogException(ex);
				Logger.LogWarning("<<< Exiting GetExtensions(menu={Menu})", menu);
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }

        private string GetExtensionPathByController(PersonaBarExtension extension)
        {
			Logger.LogError(">>> Entering GetExtensionPathByController(extension={Extension})", extension);
            var menuItem = PersonaBarRepository.Instance.GetMenuItem(extension.MenuId);
			Logger.LogError("GetExtensionPathByController(extension={Extension}): menuItem → {MenuItem}", extension, menuItem);
            if (menuItem == null)
            {
                return extension.Path;
            }

            var extensionController = this.GetExtensionController(extension);
			Logger.LogError("GetExtensionPathByController(extension={Extension}): extensionController → {ExtensionController}", extension, extensionController);
            var path = extensionController?.GetPath(extension);
			Logger.LogError("GetExtensionPathByController(extension={Extension}): path → {Path}", extension, path);
			Logger.LogWarning("<<< Exiting GetExtensionPathByController(extension={Extension})", extension);
            return !string.IsNullOrEmpty(path) ? path : extension.Path;
        }

        private bool IsVisible(PersonaBarExtension extension)
        {
			Logger.LogError(">>> Entering IsVisible(extension={Extension})", extension);
            var extensionController = this.GetExtensionController(extension);
			Logger.LogError("IsVisible(extension={Extension}): extensionController → {ExtensionController}", extension, extensionController);
			Logger.LogWarning("<<< Exiting IsVisible(extension={Extension})", extension);
            return extensionController == null || extensionController.Visible(extension);
        }

        private IDictionary<string, object> GetExtensionSettings(PersonaBarExtension extension)
        {
            var extensionController = this.GetExtensionController(extension);
            return extensionController?.GetSettings(extension);
        }

        private IExtensionController GetExtensionController(PersonaBarExtension extension)
        {
			Logger.LogError(">>> Entering GetExtensionController(extension={Extension})", extension);
            var controllerTypeName = extension.Controller;
            if (string.IsNullOrEmpty(controllerTypeName))
            {
                return null;
            }

            try
            {
                var cacheKey = $"PersonaBarExtensionController_{extension.Identifier}";
				Logger.LogError("GetExtensionController(extension={Extension}): cacheKey → {CacheKey}", extension, cacheKey);
                var controllerType = Reflection.CreateType(controllerTypeName, cacheKey, useCache: true);
				Logger.LogError("GetExtensionController(extension={Extension}): controllerType → {ControllerType}", extension, controllerType);
                return ActivatorUtilities.GetServiceOrCreateInstance(this.serviceProvider, controllerType) as IExtensionController;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting GetExtensionController(extension={Extension})", extension);
                return null;
            }
        }
    }
}
