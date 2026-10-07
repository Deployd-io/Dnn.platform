// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Tokens;

using System.Globalization;
using System.Net;
using System.Web;
using System.Web.Util;

using DotNetNuke.Entities.Users;

/// <summary>An <see cref="IPropertyAccess"/> implementation that wraps an existing <see cref="IPropertyAccess"/>, JavaScript string encoding its result.</summary>
/// <param name="implementation">The <see cref="IPropertyAccess"/> implementation that is being wrapped.</param>
public class JavaScriptEncodingPropertyAccess(IPropertyAccess implementation) : IPropertyAccess
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JavaScriptEncodingPropertyAccess));
    /// <inheritdoc />
    public CacheLevel Cacheability
    {
        get => implementation.Cacheability;
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
		Logger.LogError(">>> Entering GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
        var result = implementation.GetProperty(
            propertyName,
            format,
            formatProvider,
            accessingUser,
            accessLevel,
            ref propertyNotFound);

		Logger.LogWarning("<<< Exiting GetProperty(propertyName={PropertyName},format={Format},formatProvider={FormatProvider},accessingUser={AccessingUser},accessLevel={AccessLevel},propertyNotFound={PropertyNotFound})", propertyName, format, formatProvider, accessingUser, accessLevel, propertyNotFound);
        return HttpUtility.JavaScriptStringEncode(result);
    }
}
