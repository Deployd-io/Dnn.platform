// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Localization;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when there's an error reading or writing to a resource file.</summary>
[Serializable]
public class ResourceFileException : Exception
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ResourceFileException));
    /// <inheritdoc />
    public ResourceFileException()
    {
	Logger.LogDebug(">>> Entering ResourceFileException()");
	Logger.LogDebug("<<< Exiting ResourceFileException()");
    }

    /// <inheritdoc />
    public ResourceFileException(string message)
        : base(message)
    {
	Logger.LogDebug(">>> Entering ResourceFileException(message={Message})", message);
    }
	Logger.LogDebug("<<< Exiting ResourceFileException(message={Message})", message);

    /// <inheritdoc />
    public ResourceFileException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected ResourceFileException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
