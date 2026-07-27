// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager
{
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc />
    public class Startup : IDnnStartup
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
			Logger.LogDebug(">>> Entering ConfigureServices(services={Services})", services);
            services.AddScoped<IClientResourceController, ClientResourceController>();
			Logger.LogDebug("<<< Exiting ConfigureServices(services={Services})", services);
        }
    }
}
