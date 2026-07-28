// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls;

using System;
using System.Runtime.Serialization;

/// <summary>An exception throw when the <c>GeoIP.dat</c> file could not be found.</summary>
[Serializable]
public class GeoIPFileNotFoundException : Exception
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public GeoIPFileNotFoundException()
    {
	Logger.LogDebug(">>> Entering GeoIPFileNotFoundException()");
	Logger.LogDebug("<<< Exiting GeoIPFileNotFoundException()");
    }

    /// <inheritdoc />
    public GeoIPFileNotFoundException(string message)
        : base(message)
    {
	Logger.LogDebug(">>> Entering GeoIPFileNotFoundException(message={Message})", message);
    }
	Logger.LogDebug("<<< Exiting GeoIPFileNotFoundException(message={Message})", message);

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
