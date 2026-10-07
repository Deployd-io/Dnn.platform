// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.DependencyInjection
{
    using System;
    using System.Threading.Tasks;
    using System.Web;

    using DotNetNuke.Common.Extensions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An HTTP module which creates dependency injection scopes for each request.</summary>
    public class ServiceRequestScopeModule : IHttpModule
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServiceRequestScopeModule));
        private static IServiceProvider serviceProvider;

        /// <summary>For internal use only. Allows setting the service provider used to create request scopes.</summary>
        /// <param name="serviceProvider">The service provider.</param>
        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
			Logger.LogError(">>> Entering SetServiceProvider(serviceProvider={ServiceProvider})", serviceProvider);
            ServiceRequestScopeModule.serviceProvider = serviceProvider;
			Logger.LogDebug("SetServiceProvider(serviceProvider={ServiceProvider}): ServiceRequestScopeModule.serviceProvider → {Arg1}", serviceProvider, ServiceRequestScopeModule.serviceProvider);
        Logger.LogWarning("<<< Exiting SetServiceProvider(serviceProvider={ServiceProvider})", serviceProvider);
        }

        /// <inheritdoc />
        public void Init(HttpApplication context)
        {
			Logger.LogError(">>> Entering Init(context={Context})", context);
            context.BeginRequest += Context_BeginRequest;

            var asyncHandler = new EventHandlerTaskAsyncHelper(Context_EndRequest);
			Logger.LogError("Init(context={Context}): asyncHandler → {AsyncHandler}", context, asyncHandler);
            context.AddOnEndRequestAsync(asyncHandler.BeginEventHandler, asyncHandler.EndEventHandler);
			Logger.LogWarning("<<< Exiting Init(context={Context})", context);
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
            // left empty by design
			Logger.LogWarning("<<< Exiting Dispose(disposing={Disposing})", disposing);
        }

        private static void Context_BeginRequest(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering Context_BeginRequest(sender={Sender},e={E})", sender, e);
            var context = ((HttpApplication)sender).Context;
			Logger.LogWarning("<<< Exiting Context_BeginRequest(sender={Sender},e={E})", sender, e);
			Logger.LogError("Context_BeginRequest(sender={Sender},e={E}): context → {Context}", sender, e, context);
            context.SetScope(serviceProvider.CreateScope());
        }

        private static async Task Context_EndRequest(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering Context_EndRequest(sender={Sender},e={E})", sender, e);
            var context = ((HttpApplication)sender).Context;
			Logger.LogError("Context_EndRequest(sender={Sender},e={E}): context → {Context}", sender, e, context);
            switch (context.GetScope())
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }

            context.ClearScope();
			Logger.LogWarning("<<< Exiting Context_EndRequest(sender={Sender},e={E})", sender, e);
        }
    }
}
