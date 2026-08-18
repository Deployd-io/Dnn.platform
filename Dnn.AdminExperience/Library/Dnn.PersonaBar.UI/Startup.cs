// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI;

using Dnn.PersonaBar.UI.Components.Controllers;
using DotNetNuke.DependencyInjection;
using DotNetNuke.Services.FileSystem.Internal;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>The <see cref="IDnnStartup"/> instance for the Persona Bar UI project.</summary>
public class Startup : IDnnStartup
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Startup));
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services)
    {
		Logger.LogDebug(">>> Entering ConfigureServices(services={Services})", services);
        services.TryAddTransient<IDirectory, DirectoryWrapper>();
        services.AddTransient<IAdminMenuController, AdminMenuController>();
		Logger.LogDebug("<<< Exiting ConfigureServices(services={Services})", services);
    }
}
