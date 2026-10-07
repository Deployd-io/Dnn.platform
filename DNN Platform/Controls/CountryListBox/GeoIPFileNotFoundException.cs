// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls;

using System;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;

/// <summary>An exception throw when the <c>GeoIP.dat</c> file could not be found.</summary>
[Serializable]
public class GeoIPFileNotFoundException : Exception
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    /// <inheritdoc />
    public GeoIPFileNotFoundException()
    {
	_logger.LogError(">>> Entering GeoIPFileNotFoundException()");
    _logger.LogWarning("<<< Exiting GeoIPFileNotFoundException()");
    }

    /// <inheritdoc />
    public GeoIPFileNotFoundException(string message)
        : base(message)
    {
	_logger.LogError(">>> Entering GeoIPFileNotFoundException(message={Message})", message);
    _logger.LogWarning("<<< Exiting GeoIPFileNotFoundException(message={Message})", message);
    }

    /// <inheritdoc />
    public GeoIPFileNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected GeoIPFileNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
