// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc
{
    using System.Web.Mvc;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Common;
    using DotNetNuke.Web.Mvc.Extensions;
    using DotNetNuke.Web.Mvc.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public class Startup : IDnnStartup
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
			Logger.LogDebug(">>> Entering ConfigureServices(services={Services})", services);
            services.AddTransient<IDesktopModuleController, DesktopModuleControllerAdapter>();
            services.AddTransient<IModuleControlController, ModuleControlControllerAdapter>();

            services.TryAddSingleton<IControllerFactory, DefaultControllerFactory>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IModuleControlFactory, MvcModuleControlFactory>());
            services.TryAddEnumerable(new ServiceDescriptor(typeof(IRoutingManager), typeof(MvcRoutingManager), ServiceLifetime.Singleton));

            services.AddMvcControllers();

            DependencyResolver.SetResolver(new DnnMvcDependencyResolver(Globals.DependencyProvider));
			Logger.LogDebug("<<< Exiting ConfigureServices(services={Services})", services);
        }
    }
}
