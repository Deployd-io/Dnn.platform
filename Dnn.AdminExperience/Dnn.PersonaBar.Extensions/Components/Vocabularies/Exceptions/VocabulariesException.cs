// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Vocabularies.Exceptions
{
    using System;

    public class VocabulariesException : ApplicationException
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="VocabulariesException"/> class.</summary>
        public VocabulariesException()
        {
		Logger.LogDebug(">>> Entering VocabulariesException()");
		Logger.LogDebug("<<< Exiting VocabulariesException()");
        }

        /// <summary>Initializes a new instance of the <see cref="VocabulariesException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        public VocabulariesException(string message)
            : base(message)
        {
		Logger.LogDebug(">>> Entering VocabulariesException(message={Message})", message);
        }
		Logger.LogDebug("<<< Exiting VocabulariesException(message={Message})", message);

        /// <summary>Initializes a new instance of the <see cref="VocabulariesException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException"/> is not a <see langword="null" /> reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public VocabulariesException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
