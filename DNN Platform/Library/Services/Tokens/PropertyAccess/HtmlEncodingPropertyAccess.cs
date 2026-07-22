// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Tokens;

using System.Globalization;
using System.Net;

using DotNetNuke.Entities.Users;

/// <summary>An <see cref="IPropertyAccess"/> implementation that wraps an existing <see cref="IPropertyAccess"/>, HTML encoding its result.</summary>
/// <param name="implementation">The <see cref="IPropertyAccess"/> implementation that is being wrapped.</param>
public class HtmlEncodingPropertyAccess(IPropertyAccess implementation) : IPropertyAccess
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
	Logger.LogDebug(">>> Entering HtmlEncodingPropertyAccess(implementation={Implementation})", implementation);
    public CacheLevel Cacheability
    {
        get => implementation.Cacheability;
		Logger.LogDebug("HtmlEncodingPropertyAccess(implementation={Implementation}): get → {Get}", implementation, get);
    }

    /// <inheritdoc />
    public string GetProperty(
        string propertyName,
        string format,
        CultureInfo formatProvider,
        UserInfo accessingUser,
        Scope accessLevel,
        ref bool propertyNotFound)
    {
        var result = implementation.GetProperty(
            propertyName,
            format,
            formatProvider,
            accessingUser,
            accessLevel,
            ref propertyNotFound);

		Logger.LogDebug("<<< Exiting HtmlEncodingPropertyAccess(implementation={Implementation})", implementation);
        return WebUtility.HtmlEncode(result);
    }
}
