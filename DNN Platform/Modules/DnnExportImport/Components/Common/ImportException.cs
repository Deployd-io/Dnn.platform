// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Common;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when issues are encountered importing.</summary>
[Serializable]
public class ImportException : Exception
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ImportException));
    /// <inheritdoc />
    public ImportException()
    {
	Logger.LogDebug(">>> Entering ImportException()");
	Logger.LogDebug("<<< Exiting ImportException()");
    }

    /// <inheritdoc />
    public ImportException(string message)
        : base(message)
    {
	Logger.LogDebug(">>> Entering ImportException(message={Message})", message);
    }
	Logger.LogDebug("<<< Exiting ImportException(message={Message})", message);

    /// <inheritdoc />
    public ImportException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected ImportException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
