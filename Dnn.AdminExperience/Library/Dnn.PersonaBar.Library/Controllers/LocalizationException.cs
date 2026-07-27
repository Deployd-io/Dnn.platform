// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Controllers;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when issues are encountered with localization.</summary>
[Serializable]
public class LocalizationException : ApplicationException
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public LocalizationException()
    {
	Logger.LogDebug(">>> Entering LocalizationException()");
	Logger.LogDebug("<<< Exiting LocalizationException()");
    }

    /// <inheritdoc />
    public LocalizationException(string message)
        : base(message)
    {
	Logger.LogDebug(">>> Entering LocalizationException(message={Message})", message);
    }
	Logger.LogDebug("<<< Exiting LocalizationException(message={Message})", message);

    /// <inheritdoc />
    public LocalizationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected LocalizationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
