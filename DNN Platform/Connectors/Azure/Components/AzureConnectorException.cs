// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AzureConnector.Components;

using System;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;

/// <summary>An exception thrown when there's an issue in the Azure connector.</summary>
[Serializable]
public class AzureConnectorException : Exception
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    /// <inheritdoc />
    public AzureConnectorException()
    {
	_logger.LogError(">>> Entering AzureConnectorException()");
    _logger.LogWarning("<<< Exiting AzureConnectorException()");
    }

    /// <inheritdoc />
    public AzureConnectorException(string message)
        : base(message)
    {
	_logger.LogError(">>> Entering AzureConnectorException(message={Message})", message);
    _logger.LogWarning("<<< Exiting AzureConnectorException(message={Message})", message);
    }

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
