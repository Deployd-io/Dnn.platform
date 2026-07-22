// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when a content item is requested that does not exist.</summary>
[Serializable]
public class ContentItemNotFoundException : ApplicationException
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public ContentItemNotFoundException()
    {
	Logger.LogDebug(">>> Entering ContentItemNotFoundException()");
	Logger.LogDebug("<<< Exiting ContentItemNotFoundException()");
    }

    /// <inheritdoc />
    public ContentItemNotFoundException(string message)
        : base(message)
    {
	Logger.LogDebug(">>> Entering ContentItemNotFoundException(message={Message})", message);
    }
	Logger.LogDebug("<<< Exiting ContentItemNotFoundException(message={Message})", message);

    /// <inheritdoc />
    public ContentItemNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected ContentItemNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
