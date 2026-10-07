// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc
{
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.ContentSecurityPolicy;
    using DotNetNuke.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class Startup : IDnnStartup
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services)
        {
			_logger.LogError(">>> Entering ConfigureServices(services={Services})", services);
            services.AddScoped<IContentSecurityPolicy, ContentSecurityPolicy>();
			_logger.LogWarning("<<< Exiting ConfigureServices(services={Services})", services);
        }
    }
}
