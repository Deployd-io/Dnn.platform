// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client;

using System;
using System.Reflection;

using DotNetNuke.Instrumentation;

using Microsoft.Extensions.DependencyInjection;

/// <summary>A helper for dependency injection.</summary>
/// <remarks>This class uses reflection to access the DI container since it can't directly reference the main DotNetNuke.Library project.</remarks>
internal static class DependencyInjection
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DependencyInjection));
    private static readonly Type CommonGlobalsType;

    static DependencyInjection()
    {
		Logger.LogError(">>> Entering DependencyInjection()");
        try
        {
            // all these types are part of the same library, so we don't need a separate catch for each one
            CommonGlobalsType = Type.GetType("DotNetNuke.Common.Globals, DotNetNuke");
			Logger.LogError("DependencyInjection(): CommonGlobalsType → {CommonGlobalsType}", CommonGlobalsType);
        }
        catch (Exception exception)
        {
			Logger.LogDebug("DependencyInjection(): CommonGlobalsType → {CommonGlobalsType}", CommonGlobalsType);
            LoggerSource.Instance.GetLogger(typeof(ClientResourceSettings)).Warn("Failed to get get types for reflection", exception);
        }
		Logger.LogWarning("<<< Exiting DependencyInjection()");
    }

    /// <summary>Gets the current service scope, or creates a service scope if there isn't one already created. The scope must be disposed by the caller.</summary>
    /// <returns>A service scope.</returns>
    /// <exception cref="InvalidOperationException">The expected method in <c>DotNetNuke.Common.Globals</c> does not exist.</exception>
    public static IServiceScope GetOrCreateServiceScope()
    {
		Logger.LogError(">>> Entering GetOrCreateServiceScope()");
        var getOrCreateServiceScopeMethod = CommonGlobalsType.GetMethod("GetOrCreateServiceScope", BindingFlags.NonPublic | BindingFlags.Static, null, [], null) ?? throw new InvalidOperationException("Unable to retrieve Globals.GetOrCreateServiceScope method via reflection");
        var serviceScope = getOrCreateServiceScopeMethod.Invoke(null, []);
		Logger.LogError("GetOrCreateServiceScope(): serviceScope → {ServiceScope}", serviceScope);
		Logger.LogError("GetOrCreateServiceScope(): getOrCreateServiceScopeMethod → {GetOrCreateServiceScopeMethod}", getOrCreateServiceScopeMethod);
		Logger.LogWarning("<<< Exiting GetOrCreateServiceScope()");
        return (IServiceScope)serviceScope;
    }

    /// <summary>Gets the current service provider.</summary>
    /// <returns>A service provider.</returns>
    /// <exception cref="InvalidOperationException">The expected method in <c>DotNetNuke.Common.Globals</c> does not exist.</exception>
    public static IServiceProvider GetCurrentServiceProvider()
    {
		Logger.LogError(">>> Entering GetCurrentServiceProvider()");
        var getCurrentServiceProviderMethod = CommonGlobalsType.GetMethod("GetCurrentServiceProvider", BindingFlags.NonPublic | BindingFlags.Static, null, [], null) ?? throw new InvalidOperationException("Unable to retrieve Globals.GetCurrentServiceProvider method via reflection");
        var serviceProvider = getCurrentServiceProviderMethod.Invoke(null, []);
		Logger.LogError("GetCurrentServiceProvider(): serviceProvider → {ServiceProvider}", serviceProvider);
		Logger.LogError("GetCurrentServiceProvider(): getCurrentServiceProviderMethod → {GetCurrentServiceProviderMethod}", getCurrentServiceProviderMethod);
		Logger.LogWarning("<<< Exiting GetCurrentServiceProvider()");
        return (IServiceProvider)serviceProvider;
    }
}
