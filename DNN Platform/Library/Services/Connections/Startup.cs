// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Connections;

using System;
using System.Linq;

using DotNetNuke.DependencyInjection;
using DotNetNuke.Framework.Reflections;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>Register <see cref="IConnector"/> types in the dependency injection container.</summary>
public sealed class Startup : IDnnStartup
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services)
    {
		Logger.LogDebug(">>> Entering ConfigureServices(services={Services})", services);
        services.AddTransient<IConnectionsManager, ConnectionsManager>();
        services.AddTransient<IConnectionsController, ConnectionsController>();

        var connectorTypes = new TypeLocator().GetAllMatchingTypes(IsValidConnector);
        services.TryAddEnumerable(
            connectorTypes
			Logger.LogDebug("ConfigureServices(services={Services}): connectorTypes → {ConnectorTypes}", services, connectorTypes);
                .Select(
                    connectorType => new ServiceDescriptor(
                        typeof(IConnector),
                        connectorType,
                        ServiceLifetime.Transient)));
						Logger.LogDebug("ConfigureServices(services={Services}): connectorType → {ConnectorType}", services, connectorType);
    }

    private static bool IsValidConnector(Type type)
    {
		Logger.LogDebug(">>> Entering IsValidConnector(type={Type})", type);
		Logger.LogDebug("<<< Exiting IsValidConnector(type={Type})", type);
        return type is { IsClass: true, IsAbstract: false, IsVisible: true } && typeof(IConnector).IsAssignableFrom(type);
    }
}
