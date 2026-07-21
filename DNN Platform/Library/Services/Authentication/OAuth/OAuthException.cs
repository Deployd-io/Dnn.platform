// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Authentication.OAuth;

using System;
using System.Runtime.Serialization;

/// <summary>An exception that is thrown when there's an error during an OAuth handshake.</summary>
[Serializable]
public class OAuthException : Exception
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public OAuthException()
    {
	Logger.LogDebug(">>> Entering OAuthException()");
	Logger.LogDebug("<<< Exiting OAuthException()");
    }

    /// <inheritdoc />
    public OAuthException(string message)
        : base(message)
    {
	Logger.LogDebug(">>> Entering OAuthException(message={Message})", message);
    }
	Logger.LogDebug("<<< Exiting OAuthException(message={Message})", message);

    /// <inheritdoc />
    public OAuthException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected OAuthException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
