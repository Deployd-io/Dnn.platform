// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Roles;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when a role is requested which is not approved.</summary>
[Serializable]
public class RoleNotApprovedException : Exception
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public RoleNotApprovedException()
    {
	Logger.LogDebug(">>> Entering RoleNotApprovedException()");
	Logger.LogDebug("<<< Exiting RoleNotApprovedException()");
    }

    /// <inheritdoc />
    public RoleNotApprovedException(string message)
        : base(message)
    {
	Logger.LogDebug(">>> Entering RoleNotApprovedException(message={Message})", message);
    }
	Logger.LogDebug("<<< Exiting RoleNotApprovedException(message={Message})", message);

    /// <inheritdoc />
    public RoleNotApprovedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected RoleNotApprovedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
