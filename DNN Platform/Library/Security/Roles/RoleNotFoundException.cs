// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Roles;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when a role is requested which cannot be found.</summary>
[Serializable]
public class RoleNotFoundException : Exception
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RoleNotFoundException));
    /// <inheritdoc />
    public RoleNotFoundException()
    {
	Logger.LogDebug(">>> Entering RoleNotFoundException()");
	Logger.LogDebug("<<< Exiting RoleNotFoundException()");
    }

    /// <inheritdoc />
    public RoleNotFoundException(string message)
        : base(message)
    {
	Logger.LogDebug(">>> Entering RoleNotFoundException(message={Message})", message);
    }
	Logger.LogDebug("<<< Exiting RoleNotFoundException(message={Message})", message);

    /// <inheritdoc />
    public RoleNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected RoleNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
