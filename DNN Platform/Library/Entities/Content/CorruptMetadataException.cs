// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when a content item's metadata has become corrupt.</summary>
[Serializable]
public class CorruptMetadataException : ApplicationException
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CorruptMetadataException));
    /// <inheritdoc />
    public CorruptMetadataException()
    {
	Logger.LogDebug(">>> Entering CorruptMetadataException()");
	Logger.LogDebug("<<< Exiting CorruptMetadataException()");
    }

    /// <inheritdoc />
    public CorruptMetadataException(string message)
        : base(message)
    {
	Logger.LogDebug(">>> Entering CorruptMetadataException(message={Message})", message);
    }
	Logger.LogDebug("<<< Exiting CorruptMetadataException(message={Message})", message);

    /// <inheritdoc />
    public CorruptMetadataException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected CorruptMetadataException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
