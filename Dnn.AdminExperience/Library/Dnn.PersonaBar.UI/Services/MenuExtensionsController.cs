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
		Logger.LogDebug(">>> Entering MenuExtensionsController()");
		Logger.LogDebug("<<< Exiting MenuExtensionsController()");
		Logger.LogDebug(">>> Entering MenuExtensionsController(serviceProvider={ServiceProvider})", serviceProvider);
		Logger.LogDebug("<<< Exiting MenuExtensionsController(serviceProvider={ServiceProvider})", serviceProvider);
        }

        /// <summary>Initializes a new instance of the <see cref="MenuExtensionsController"/> class.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        public MenuExtensionsController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
			Logger.LogDebug("MenuExtensionsController(serviceProvider={ServiceProvider}): serviceProvider → {ServiceProvider1}", serviceProvider, serviceProvider);
			Logger.LogDebug("MenuExtensionsController(serviceProvider={ServiceProvider}): this.serviceProvider → {Arg1}", serviceProvider, this.serviceProvider);
        }

        /// <summary>Retrieve a list of extensions for menu.</summary>
        /// <param name="menu">The menu identifier.</param>
        /// <returns>A response with a collection of extension info.</returns>
        [HttpGet]
        public HttpResponseMessage GetExtensions(string menu)
        {
			Logger.LogDebug(">>> Entering GetExtensions(menu={Menu})", menu);
            try
            {
                var menuItem = PersonaBarRepository.Instance.GetMenuItem(menu);
                if (menuItem != null)
				Logger.LogDebug("GetExtensions(menu={Menu}): menuItem → {MenuItem}", menu, menuItem);
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
							Logger.LogDebug("GetExtensions(menu={Menu}): identifier → {Identifier}", menu, identifier);
							Logger.LogDebug("GetExtensions(menu={Menu}): folderName → {FolderName}", menu, folderName);
							Logger.LogDebug("GetExtensions(menu={Menu}): container → {Container}", menu, container);
							Logger.LogDebug("GetExtensions(menu={Menu}): path → {Path}", menu, path);
							Logger.LogDebug("GetExtensions(menu={Menu}): settings → {Settings}", menu, settings);

                    return this.Request.CreateResponse(HttpStatusCode.OK, extensions);
                }

                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in GetExtensions(menu={Menu}): {Arg1}", menu, ex.Message);
                Exceptions.LogException(ex);
				Logger.LogDebug("<<< Exiting GetExtensions(menu={Menu})", menu);
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }

        private string GetExtensionPathByController(PersonaBarExtension extension)
        {
			Logger.LogDebug(">>> Entering GetExtensionPathByController(extension={Extension})", extension);
            var menuItem = PersonaBarRepository.Instance.GetMenuItem(extension.MenuId);
            if (menuItem == null)
			Logger.LogDebug("GetExtensionPathByController(extension={Extension}): menuItem → {MenuItem}", extension, menuItem);
            {
                return extension.Path;
            }

            var extensionController = this.GetExtensionController(extension);
            var path = extensionController?.GetPath(extension);
			Logger.LogDebug("GetExtensionPathByController(extension={Extension}): extensionController → {ExtensionController}", extension, extensionController);
			Logger.LogDebug("GetExtensionPathByController(extension={Extension}): path → {Path}", extension, path);
			Logger.LogDebug("<<< Exiting GetExtensionPathByController(extension={Extension})", extension);
            return !string.IsNullOrEmpty(path) ? path : extension.Path;
        }

        private bool IsVisible(PersonaBarExtension extension)
        {
			Logger.LogDebug(">>> Entering IsVisible(extension={Extension})", extension);
            var extensionController = this.GetExtensionController(extension);
			Logger.LogDebug("IsVisible(extension={Extension}): extensionController → {ExtensionController}", extension, extensionController);
			Logger.LogDebug("<<< Exiting IsVisible(extension={Extension})", extension);
            return extensionController == null || extensionController.Visible(extension);
        }

        private IDictionary<string, object> GetExtensionSettings(PersonaBarExtension extension)
        {
            var extensionController = this.GetExtensionController(extension);
            return extensionController?.GetSettings(extension);
        }

        private IExtensionController GetExtensionController(PersonaBarExtension extension)
        {
			Logger.LogDebug(">>> Entering GetExtensionController(extension={Extension})", extension);
            var controllerTypeName = extension.Controller;
            if (string.IsNullOrEmpty(controllerTypeName))
            {
                return null;
            }

            try
            {
                var cacheKey = $"PersonaBarExtensionController_{extension.Identifier}";
                var controllerType = Reflection.CreateType(controllerTypeName, cacheKey, useCache: true);
				Logger.LogDebug("GetExtensionController(extension={Extension}): cacheKey → {CacheKey}", extension, cacheKey);
				Logger.LogDebug("GetExtensionController(extension={Extension}): controllerType → {ControllerType}", extension, controllerType);
                return ActivatorUtilities.GetServiceOrCreateInstance(this.serviceProvider, controllerType) as IExtensionController;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting GetExtensionController(extension={Extension})", extension);
                return null;
            }
        }
    }
}
