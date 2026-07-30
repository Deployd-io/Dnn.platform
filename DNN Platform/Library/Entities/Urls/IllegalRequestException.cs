// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when processing a request that is illegal (e.g. it matches multiple pages).</summary>
[Serializable]
public class IllegalRequestException : Exception
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public IllegalRequestException()
    {
	Logger.LogDebug(">>> Entering IllegalRequestException()");
	Logger.LogDebug("<<< Exiting IllegalRequestException()");
    }

    /// <inheritdoc />
    public IllegalRequestException(string message)
        : base(message)
    {
	Logger.LogDebug(">>> Entering IllegalRequestException(message={Message})", message);
    }
	Logger.LogDebug("<<< Exiting IllegalRequestException(message={Message})", message);

    /// <inheritdoc />
    public IllegalRequestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected IllegalRequestException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
