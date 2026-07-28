// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Dnn.ContactList.Api
{
    public class Startup : IDnnStartup
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public void ConfigureServices(IServiceCollection services)
        {
			Logger.LogDebug(">>> Entering ConfigureServices(services={Services})", services);
            // Register ContactService as a singleton
            services.AddScoped(x => ContactRepository.Instance);
			Logger.LogDebug("<<< Exiting ConfigureServices(services={Services})", services);
        }
    }
}
