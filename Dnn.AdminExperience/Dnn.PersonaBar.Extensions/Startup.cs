// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions;

using Dnn.PersonaBar.ConfigConsole.Components;
using Dnn.PersonaBar.Pages.Components;
using Dnn.PersonaBar.Prompt.Components.Repositories;
using Dnn.PersonaBar.Themes.Components;

using DotNetNuke.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

/// <summary>Registers services for the Persona Bar Extensions.</summary>
public class Startup : IDnnStartup
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services)
    {
	Logger.LogDebug(">>> Entering ConfigureServices(services={Services})", services);
#pragma warning disable CS0618
        services.AddTransient<ICommandRepository, CommandRepository>();
#pragma warning restore CS0618

        services.AddTransient<ITemplateController, TemplateController>();
        services.AddTransient<IPagesController, PagesControllerImpl>();
        services.AddTransient<IBulkPagesController, BulkPagesController>();
        services.AddTransient<IThemesController, ThemesController>();
        services.AddTransient<IDefaultPortalThemeController, DefaultPortalThemeController>();
        services.AddTransient<ConfigConsoleController>();
		Logger.LogDebug("<<< Exiting ConfigureServices(services={Services})", services);
    }
}
