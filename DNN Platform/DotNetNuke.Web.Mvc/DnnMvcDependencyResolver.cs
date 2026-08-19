// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    using DotNetNuke.Services.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The <see cref="IDependencyResolver"/> implementation used in the
    /// MVC Modules of DNN.
    /// </summary>
    internal class DnnMvcDependencyResolver : IDependencyResolver
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnMvcDependencyResolver));
        private readonly IServiceProvider serviceProvider;

        /// <summary>Initializes a new instance of the <see cref="DnnMvcDependencyResolver"/> class.</summary>
        /// <param name="serviceProvider">The service provider.</param>
        public DnnMvcDependencyResolver(IServiceProvider serviceProvider)
        {
			Logger.LogDebug(">>> Entering DnnMvcDependencyResolver(serviceProvider={ServiceProvider})", serviceProvider);
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			Logger.LogDebug("DnnMvcDependencyResolver(serviceProvider={ServiceProvider}): serviceProvider → {ServiceProvider1}", serviceProvider, serviceProvider);
			Logger.LogDebug("<<< Exiting DnnMvcDependencyResolver(serviceProvider={ServiceProvider})", serviceProvider);
        }

        /// <summary>Returns the specified service from the scope.</summary>
        /// <param name="serviceType">
        /// The service to be retrieved.
        /// </param>
        /// <returns>
        /// The retrieved service.
        /// </returns>
        public object GetService(Type serviceType)
        {
			Logger.LogDebug(">>> Entering GetService(serviceType={ServiceType})", serviceType);
            var accessor = this.serviceProvider.GetRequiredService<IScopeAccessor>();
            var scope = accessor.GetScope();
			Logger.LogDebug("GetService(serviceType={ServiceType}): scope → {Scope}", serviceType, scope);
			Logger.LogDebug("GetService(serviceType={ServiceType}): accessor → {Accessor}", serviceType, accessor);
            if (scope != null)
            {
			Logger.LogWarning("Throwing InvalidOperationException from GetService(serviceType={ServiceType})", serviceType);
                return scope.ServiceProvider.GetService(serviceType);
            }

			Logger.LogDebug("<<< Exiting GetService(serviceType={ServiceType})", serviceType);
            throw new InvalidOperationException("IServiceScope not provided");
        }

        /// <summary>Returns the specified services from the scope.</summary>
        /// <param name="serviceType">
        /// The service to be retrieved.
        /// </param>
        /// <returns>
        /// The retrieved service.
        /// </returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
			Logger.LogDebug(">>> Entering GetServices(serviceType={ServiceType})", serviceType);
            var accessor = this.serviceProvider.GetRequiredService<IScopeAccessor>();
            var scope = accessor.GetScope();
			Logger.LogDebug("GetServices(serviceType={ServiceType}): scope → {Scope}", serviceType, scope);
			Logger.LogDebug("GetServices(serviceType={ServiceType}): accessor → {Accessor}", serviceType, accessor);
            if (scope != null)
            {
			Logger.LogWarning("Throwing InvalidOperationException from GetServices(serviceType={ServiceType})", serviceType);
                return scope.ServiceProvider.GetServices(serviceType);
            }

			Logger.LogDebug("<<< Exiting GetServices(serviceType={ServiceType})", serviceType);
            throw new InvalidOperationException("IServiceScope not provided");
        }
    }
}
