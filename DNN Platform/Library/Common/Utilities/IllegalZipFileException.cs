// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when a zip file has an illegal entry.</summary>
[Serializable]
public class IllegalZipFileException : Exception
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(IllegalZipFileException));
    /// <inheritdoc />
    public IllegalZipFileException()
    {
	Logger.LogDebug(">>> Entering IllegalZipFileException()");
	Logger.LogDebug("<<< Exiting IllegalZipFileException()");
    }

    /// <inheritdoc />
    public IllegalZipFileException(string message)
        : base(message)
    {
	Logger.LogDebug(">>> Entering IllegalZipFileException(message={Message})", message);
    }
	Logger.LogDebug("<<< Exiting IllegalZipFileException(message={Message})", message);

    /// <inheritdoc />
    public IllegalZipFileException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected IllegalZipFileException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
