// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Web.Http.Dependencies;

    using DotNetNuke.Services.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The <see cref="IDependencyResolver"/> implementation used in the
    /// Web API Modules of DNN.
    /// </summary>
    internal class DnnDependencyResolver : IDependencyResolver
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnDependencyResolver));
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnnDependencyResolver"/> class.
        /// Instantiate a new instance of the <see cref="DnnDependencyResolver"/>.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> to be used in the <see cref="DnnDependencyResolver"/>.
        /// </param>
        public DnnDependencyResolver(IServiceProvider serviceProvider)
        {
			Logger.LogError(">>> Entering DnnDependencyResolver(serviceProvider={ServiceProvider})", serviceProvider);
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			Logger.LogDebug("DnnDependencyResolver(serviceProvider={ServiceProvider}): serviceProvider → {ServiceProvider1}", serviceProvider, serviceProvider);
        Logger.LogWarning("<<< Exiting DnnDependencyResolver(serviceProvider={ServiceProvider})", serviceProvider);
        }

        /// <summary>Starts a new resolution scope.</summary>
        /// <returns>
        /// The dependency scope.
        /// </returns>
        public IDependencyScope BeginScope()
        {
			Logger.LogError(">>> Entering BeginScope()");
            var accessor = this.serviceProvider.GetRequiredService<IScopeAccessor>();
            var scope = accessor.GetScope();
			Logger.LogError("BeginScope(): scope → {Scope}", scope);
			Logger.LogError("BeginScope(): accessor → {Accessor}", accessor);
			Logger.LogWarning("<<< Exiting BeginScope()");
            return new DnnDependencyResolver(scope.ServiceProvider);
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
			Logger.LogError(">>> Entering GetService(serviceType={ServiceType})", serviceType);
            Logger.LogWarning("<<< Exiting GetService(serviceType={ServiceType})", serviceType);
            return this.serviceProvider.GetService(serviceType);
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
			Logger.LogError(">>> Entering GetServices(serviceType={ServiceType})", serviceType);
            Logger.LogWarning("<<< Exiting GetServices(serviceType={ServiceType})", serviceType);
            return this.serviceProvider.GetServices(serviceType);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
			Logger.LogError(">>> Entering Dispose()");
            this.Dispose(true);
			Logger.LogWarning("<<< Exiting Dispose()");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// true if the object is currently disposing.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
			Logger.LogError(">>> Entering Dispose(disposing={Disposing})", disposing);
            // left empty by design, since the DependencyResolver shouldn't dispose the ServiceProvider
			Logger.LogWarning("<<< Exiting Dispose(disposing={Disposing})", disposing);
        }
    }
}
