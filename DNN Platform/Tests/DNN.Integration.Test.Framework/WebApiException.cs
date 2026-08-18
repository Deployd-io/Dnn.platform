// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework
{
    using System;
    using System.Net.Http;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
	using Microsoft.Extensions.Logging;

    /// <summary>Represents a non successful response while executing a WebApi call.</summary>
    public class WebApiException : Exception
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiException"/> class, with the specified inner exception and response.
        /// message that caused the exception.
        /// </summary>
        /// <param name="innerException">The original exception.</param>
        /// <param name="result">The result of the request.</param>
        public WebApiException(Exception innerException, HttpResponseMessage result)
            : base(innerException.Message, innerException)
        {
			_logger.LogDebug(">>> Entering WebApiException(innerException={InnerException},result={Result})", innerException, result);
            this.Result = result;
			_logger.LogDebug("<<< Exiting WebApiException(innerException={InnerException},result={Result})", innerException, result);
			_logger.LogDebug("WebApiException(innerException={InnerException},result={Result}): this.Result → {Arg2}", innerException, result, this.Result);
        }

        public WebApiException(Exception innerException, HttpResponseMessage result, string body)
            : this(innerException, result)
        {
            this.Body = body;
        }

        /// <summary>Gets the result of the request. Can be used to retrieve additional info like HTTP status code.</summary>
        public HttpResponseMessage Result { get; private set; }

        /// <summary>Gets body from the Get Response. Available when exception is thrown as well.</summary>
        public string Body { get; private set; }

        public dynamic BodyAsJson()
        {
			_logger.LogDebug(">>> Entering BodyAsJson()");
			_logger.LogDebug("<<< Exiting BodyAsJson()");
            return JsonConvert.DeserializeObject<JContainer>(this.Body);
        }
    }
}
