// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client;

using System;
using System.Runtime.Serialization;
using System.Web.UI;

/// <summary>An exception thrown from an operation that requires a <see cref="ScriptManager"/> when it is not present.</summary>
[Serializable]
public class ScriptManagerRequiredException : Exception
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public ScriptManagerRequiredException()
    {
	Logger.LogDebug(">>> Entering ScriptManagerRequiredException()");
	Logger.LogDebug("<<< Exiting ScriptManagerRequiredException()");
    }

    /// <inheritdoc />
    public ScriptManagerRequiredException(string message)
        : base(message)
    {
	Logger.LogDebug(">>> Entering ScriptManagerRequiredException(message={Message})", message);
    }
	Logger.LogDebug("<<< Exiting ScriptManagerRequiredException(message={Message})", message);

    /// <inheritdoc />
    public ScriptManagerRequiredException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected ScriptManagerRequiredException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
