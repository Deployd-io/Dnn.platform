// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Definitions;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when a module definition version is required but unknown.</summary>
[Serializable]
public class UnknownModuleDefinitionVersionException : Exception
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public UnknownModuleDefinitionVersionException()
    {
	Logger.LogDebug(">>> Entering UnknownModuleDefinitionVersionException()");
	Logger.LogDebug("<<< Exiting UnknownModuleDefinitionVersionException()");
    }

    /// <inheritdoc />
    public UnknownModuleDefinitionVersionException(string message)
        : base(message)
    {
	Logger.LogDebug(">>> Entering UnknownModuleDefinitionVersionException(message={Message})", message);
    }
	Logger.LogDebug("<<< Exiting UnknownModuleDefinitionVersionException(message={Message})", message);

    /// <inheritdoc />
    public UnknownModuleDefinitionVersionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected UnknownModuleDefinitionVersionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
