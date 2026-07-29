// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>An exception with a <see cref="DotNetNukeErrorCode"/>.</summary>
    public class DotNetNukeException : Exception
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly DotNetNukeErrorCode errorCode = DotNetNukeErrorCode.NotSet;

        /// <summary>Initializes a new instance of the <see cref="DotNetNukeException"/> class.</summary>
        public DotNetNukeException()
        {
		Logger.LogDebug(">>> Entering DotNetNukeException()");
		Logger.LogDebug("<<< Exiting DotNetNukeException()");
        }

        /// <summary>Initializes a new instance of the <see cref="DotNetNukeException"/> class.</summary>
        /// <param name="message">The message.</param>
        public DotNetNukeException(string message)
            : base(message)
        {
		Logger.LogDebug(">>> Entering DotNetNukeException(message={Message})", message);
        }
		Logger.LogDebug("<<< Exiting DotNetNukeException(message={Message})", message);

        /// <summary>Initializes a new instance of the <see cref="DotNetNukeException"/> class.</summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DotNetNukeException(string message, Exception innerException)
            : base(message, innerException)
        {
		Logger.LogDebug(">>> Entering DotNetNukeException(message={Message},innerException={InnerException})", message, innerException);
        }

        /// <summary>Initializes a new instance of the <see cref="DotNetNukeException"/> class.</summary>
        /// <param name="message">The message.</param>
        /// <param name="errorCode">The error code.</param>
        public DotNetNukeException(string message, DotNetNukeErrorCode errorCode)
            : base(message)
        {
            this.errorCode = errorCode;
			Logger.LogDebug("<<< Exiting DotNetNukeException(message={Message},errorCode={ErrorCode})", message, errorCode);
			Logger.LogDebug("DotNetNukeException(message={Message},errorCode={ErrorCode}): errorCode → {ErrorCode1}", message, errorCode, errorCode);
			Logger.LogDebug("DotNetNukeException(message={Message},errorCode={ErrorCode}): this.errorCode → {Arg2}", message, errorCode, this.errorCode);
        }

        /// <summary>Initializes a new instance of the <see cref="DotNetNukeException"/> class.</summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="errorCode">The error code.</param>
        public DotNetNukeException(string message, Exception innerException, DotNetNukeErrorCode errorCode)
            : base(message, innerException)
        {
            this.errorCode = errorCode;
			Logger.LogDebug("DotNetNukeException(message={Message},innerException={InnerException},errorCode={ErrorCode}): errorCode → {ErrorCode1}", message, innerException, errorCode, errorCode);
			Logger.LogDebug("DotNetNukeException(message={Message},innerException={InnerException},errorCode={ErrorCode}): this.errorCode → {Arg3}", message, innerException, errorCode, this.errorCode);
        }

        /// <summary>Initializes a new instance of the <see cref="DotNetNukeException"/> class.</summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        public DotNetNukeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>Gets the error code associated with this exception.</summary>
        public DotNetNukeErrorCode ErrorCode
        {
            get
            {
                return this.errorCode;
            }
        }
    }
}
