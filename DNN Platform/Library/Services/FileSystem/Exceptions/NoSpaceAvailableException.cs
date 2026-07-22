// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class NoSpaceAvailableException : Exception
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="NoSpaceAvailableException"/> class.</summary>
        public NoSpaceAvailableException()
        {
		Logger.LogDebug(">>> Entering NoSpaceAvailableException()");
		Logger.LogDebug("<<< Exiting NoSpaceAvailableException()");
        }

        /// <summary>Initializes a new instance of the <see cref="NoSpaceAvailableException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        public NoSpaceAvailableException(string message)
            : base(message)
        {
		Logger.LogDebug(">>> Entering NoSpaceAvailableException(message={Message})", message);
        }
		Logger.LogDebug("<<< Exiting NoSpaceAvailableException(message={Message})", message);

        /// <summary>Initializes a new instance of the <see cref="NoSpaceAvailableException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner"/> is not a <see langword="null" /> reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public NoSpaceAvailableException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="NoSpaceAvailableException"/> class.</summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        public NoSpaceAvailableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
