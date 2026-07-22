// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidMetadataValuesException : Exception
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="InvalidMetadataValuesException"/> class.</summary>
        public InvalidMetadataValuesException()
        {
		Logger.LogDebug(">>> Entering InvalidMetadataValuesException()");
		Logger.LogDebug("<<< Exiting InvalidMetadataValuesException()");
        }

        /// <summary>Initializes a new instance of the <see cref="InvalidMetadataValuesException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidMetadataValuesException(string message)
            : base(message)
        {
		Logger.LogDebug(">>> Entering InvalidMetadataValuesException(message={Message})", message);
        }
		Logger.LogDebug("<<< Exiting InvalidMetadataValuesException(message={Message})", message);

        /// <summary>Initializes a new instance of the <see cref="InvalidMetadataValuesException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner"/> is not a <see langword="null" /> reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public InvalidMetadataValuesException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="InvalidMetadataValuesException"/> class.</summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        public InvalidMetadataValuesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
