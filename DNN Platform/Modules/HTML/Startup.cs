// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html;

using DotNetNuke.DependencyInjection;
using DotNetNuke.Modules.Html.Components;

using Microsoft.Extensions.DependencyInjection;

public class Startup : IDnnStartup
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services)
    {
		Logger.LogDebug(">>> Entering ConfigureServices(services={Services})", services);
        services.AddTransient<HtmlTextController>();
        services.AddTransient<HtmlModuleSettingsRepository>();
		Logger.LogDebug("<<< Exiting ConfigureServices(services={Services})", services);
    }
}
