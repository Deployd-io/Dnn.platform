// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common
{
    using System;
    using System.Web;

    using DotNetNuke.Common.Extensions;
    using Microsoft.Extensions.DependencyInjection;

    internal struct ServiceScopeContainer : IDisposable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServiceScopeContainer));
        /// <summary>Initializes a new instance of the <see cref="ServiceScopeContainer"/> struct.</summary>
        /// <param name="serviceScope">The <see cref="IServiceScope"/> instance to use.</param>
        /// <param name="shouldDispose">A value indicating whether to dispose the <see cref="ServiceScope"/> when this instance is disposed.</param>
        public ServiceScopeContainer(IServiceScope serviceScope, bool shouldDispose)
        {
			Logger.LogDebug(">>> Entering ServiceScopeContainer(serviceScope={ServiceScope},shouldDispose={ShouldDispose})", serviceScope, shouldDispose);
            this.ServiceScope = serviceScope;
            this.ShouldDispose = shouldDispose;
			Logger.LogDebug("ServiceScopeContainer(serviceScope={ServiceScope},shouldDispose={ShouldDispose}): this.ServiceScope → {Arg2}", serviceScope, shouldDispose, this.ServiceScope);
			Logger.LogDebug("<<< Exiting ServiceScopeContainer(serviceScope={ServiceScope},shouldDispose={ShouldDispose})", serviceScope, shouldDispose);
        }

        /// <summary>    Gets service scope that the container holds.</summary>
        public IServiceScope ServiceScope { get; }

        /// <summary>    Gets a value indicating whether true if the <see cref="ServiceScope"/> should be disposed.</summary>
        public bool ShouldDispose { get; }

        /// <summary>    Get the service scope from <see cref="HttpContext"/> or create a new scope from <see cref="Globals.DependencyProvider"/> when the scope doesn't exists.</summary>
        /// <returns>The service scope container.</returns>
        public static ServiceScopeContainer GetRequestOrCreateScope()
        {
			Logger.LogDebug(">>> Entering GetRequestOrCreateScope()");
            var requestScope = HttpContextSource.Current?.GetScope();

			Logger.LogDebug("GetRequestOrCreateScope(): requestScope → {RequestScope}", requestScope);
            if (requestScope != null)
            {
			Logger.LogDebug("<<< Exiting GetRequestOrCreateScope()");
                return new ServiceScopeContainer(requestScope, false);
            }

            return new ServiceScopeContainer(Globals.DependencyProvider.CreateScope(), true);
        }

        /// <inheritdoc />
        public void Dispose()
        {
			Logger.LogDebug(">>> Entering Dispose()");
            if (this.ShouldDispose)
            {
                this.ServiceScope.Dispose();
            }
			Logger.LogDebug("<<< Exiting Dispose()");
        }
    }
}
