// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls;

using System;
using System.Runtime.Serialization;

/// <summary>An exception throw when a portal is requested that could not be found.</summary>
[Serializable]
public class PortalNotFoundException : Exception
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public PortalNotFoundException()
    {
	Logger.LogDebug(">>> Entering PortalNotFoundException()");
	Logger.LogDebug("<<< Exiting PortalNotFoundException()");
    }

    /// <inheritdoc />
    public PortalNotFoundException(string message)
        : base(message)
    {
	Logger.LogDebug(">>> Entering PortalNotFoundException(message={Message})", message);
    }
	Logger.LogDebug("<<< Exiting PortalNotFoundException(message={Message})", message);

    /// <inheritdoc />
    public PortalNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected PortalNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
