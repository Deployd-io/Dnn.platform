// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dnn.ContactList.Api
{
    public class Startup : IDnnStartup
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public void ConfigureServices(IServiceCollection services)
        {
			_logger.LogDebug(">>> Entering ConfigureServices(services={Services})", services);
            // Register ContactService as a singleton
            services.AddScoped(x => ContactRepository.Instance);
			_logger.LogDebug("<<< Exiting ConfigureServices(services={Services})", services);
        }
    }
}
