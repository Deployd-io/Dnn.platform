// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.DependencyInjection;

using DotNetNuke.Common;
using DotNetNuke.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;

/// <summary>An <see cref="IScopeAccessor"/> implementation using <see cref="HttpContextSource.Current"/>.</summary>
public class ScopeAccessor : IScopeAccessor
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ScopeAccessor));
    /// <inheritdoc />
    public IServiceScope GetScope()
    {
		Logger.LogDebug(">>> Entering GetScope()");
		Logger.LogDebug("<<< Exiting GetScope()");
        return HttpContextSource.Current.GetScope();
    }
}
