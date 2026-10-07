// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html;

using DotNetNuke.DependencyInjection;
using DotNetNuke.Modules.Html.Components;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class Startup : IDnnStartup
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services)
    {
		_logger.LogError(">>> Entering ConfigureServices(services={Services})", services);
        services.AddTransient<HtmlTextController>();
        services.AddTransient<HtmlModuleSettingsRepository>();
		_logger.LogWarning("<<< Exiting ConfigureServices(services={Services})", services);
    }
}
