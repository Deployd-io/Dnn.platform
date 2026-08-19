// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls;

using System;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;

/// <summary>An exception throw when there's an error calling <see cref="CountryLookup.SeekCountry"/>.</summary>
[Serializable]
public class SeekCountryException : Exception
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    /// <inheritdoc />
    public SeekCountryException()
    {
	_logger.LogDebug(">>> Entering SeekCountryException()");
	_logger.LogDebug("<<< Exiting SeekCountryException()");
    }

    /// <inheritdoc />
    public SeekCountryException(string message)
        : base(message)
    {
	_logger.LogDebug(">>> Entering SeekCountryException(message={Message})", message);
    }
	_logger.LogDebug("<<< Exiting SeekCountryException(message={Message})", message);

    /// <inheritdoc />
    public SeekCountryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected SeekCountryException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
