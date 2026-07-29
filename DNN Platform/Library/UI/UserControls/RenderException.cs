// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.UserControls;

using System;
using System.Runtime.Serialization;

/// <summary>An exception that's thrown when there's an error rendering a control.</summary>
[Serializable]
public class RenderException : Exception
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public RenderException()
    {
	Logger.LogDebug(">>> Entering RenderException()");
	Logger.LogDebug("<<< Exiting RenderException()");
    }

    /// <inheritdoc />
    public RenderException(string message)
        : base(message)
    {
	Logger.LogDebug(">>> Entering RenderException(message={Message})", message);
    }
	Logger.LogDebug("<<< Exiting RenderException(message={Message})", message);

    /// <inheritdoc />
    public RenderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected RenderException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
