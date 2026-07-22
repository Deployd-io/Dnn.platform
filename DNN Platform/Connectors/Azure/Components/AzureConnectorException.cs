// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AzureConnector.Components;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when there's an issue in the Azure connector.</summary>
[Serializable]
public class AzureConnectorException : Exception
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public AzureConnectorException()
    {
	Logger.LogDebug(">>> Entering AzureConnectorException()");
	Logger.LogDebug("<<< Exiting AzureConnectorException()");
    }

    /// <inheritdoc />
    public AzureConnectorException(string message)
        : base(message)
    {
	Logger.LogDebug(">>> Entering AzureConnectorException(message={Message})", message);
    }
	Logger.LogDebug("<<< Exiting AzureConnectorException(message={Message})", message);

    /// <inheritdoc />
    public AzureConnectorException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected AzureConnectorException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
