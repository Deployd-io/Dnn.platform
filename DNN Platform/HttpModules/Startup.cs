// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.HttpModules;

using DotNetNuke.DependencyInjection;
using DotNetNuke.Entities.Urls;

using Microsoft.Extensions.DependencyInjection;

/// <summary>The <see cref="IDnnStartup"/> for <c>DotNetNuke.HttpModules</c>.</summary>
public class Startup : IDnnStartup
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services)
    {
		Logger.LogDebug(">>> Entering ConfigureServices(services={Services})", services);
        services.AddScoped<UrlRewriterBase>(serviceProvider => UrlRewriteModule.GetUrlRewriterInstance(serviceProvider));
		Logger.LogDebug("<<< Exiting ConfigureServices(services={Services})", services);
    }
}
