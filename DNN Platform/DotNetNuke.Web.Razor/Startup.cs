// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Razor
{
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.UI.Modules;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
	using Microsoft.Extensions.Logging;

    public class Startup : IDnnStartup
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
#pragma warning disable CS0618 // Type or member is obsolete
			_logger.LogError(">>> Entering ConfigureServices(services={Services})", services);
			_logger.LogWarning("<<< Exiting ConfigureServices(services={Services})", services);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IModuleControlFactory, RazorModuleControlFactory>());
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
