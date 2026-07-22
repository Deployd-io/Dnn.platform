// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when there is a failure registering a new user.</summary>
[Serializable]
public class RegistrationException : Exception
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public RegistrationException()
    {
	Logger.LogDebug(">>> Entering RegistrationException()");
	Logger.LogDebug("<<< Exiting RegistrationException()");
    }

    /// <inheritdoc />
    public RegistrationException(string message)
        : base(message)
    {
	Logger.LogDebug(">>> Entering RegistrationException(message={Message})", message);
    }
	Logger.LogDebug("<<< Exiting RegistrationException(message={Message})", message);

    /// <inheritdoc />
    public RegistrationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected RegistrationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
