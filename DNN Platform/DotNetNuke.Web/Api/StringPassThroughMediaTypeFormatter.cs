// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    /// <summary>A MediaTypeFormatter that simply allows strings to pass through WebAPI and be associated with the specified MIME type.</summary>
    public class StringPassThroughMediaTypeFormatter : MediaTypeFormatter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(StringPassThroughMediaTypeFormatter));
        private static readonly string[] DefaultMediaTypes = ["text/plain", "text/html",];

        /// <summary>Initializes a new instance of the <see cref="StringPassThroughMediaTypeFormatter"/> class that can handle text/plain and text/html.</summary>
        public StringPassThroughMediaTypeFormatter()
            : this(DefaultMediaTypes)
        {
		Logger.LogError(">>> Entering StringPassThroughMediaTypeFormatter()");
        Logger.LogWarning("<<< Exiting StringPassThroughMediaTypeFormatter()");
        }

        /// <summary>Initializes a new instance of the <see cref="StringPassThroughMediaTypeFormatter"/> class that can handle the specified media types.</summary>
        /// <param name="mediaTypes">The supported media types.</param>
        public StringPassThroughMediaTypeFormatter(IEnumerable<string> mediaTypes)
        {
			Logger.LogError(">>> Entering StringPassThroughMediaTypeFormatter(mediaTypes={MediaTypes})", mediaTypes);
            foreach (var type in mediaTypes)
            {
                this.SupportedMediaTypes.Add(new MediaTypeHeaderValue(type));
            }
			Logger.LogWarning("<<< Exiting StringPassThroughMediaTypeFormatter(mediaTypes={MediaTypes})", mediaTypes);
        }

        /// <inheritdoc />
        public override bool CanReadType(Type type)
        {
			Logger.LogError(">>> Entering CanReadType(type={Type})", type);
            Logger.LogWarning("<<< Exiting CanReadType(type={Type})", type);
            return type == typeof(string);
        }

        /// <inheritdoc />
        public override bool CanWriteType(Type type)
        {
			Logger.LogError(">>> Entering CanWriteType(type={Type})", type);
            Logger.LogWarning("<<< Exiting CanWriteType(type={Type})", type);
            return type == typeof(string);
        }

        /// <inheritdoc />
        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
			Logger.LogError(">>> Entering ReadFromStreamAsync(type={Type},readStream={ReadStream},content={Content},formatterLogger={FormatterLogger})", type, readStream, content, formatterLogger);
            using (var reader = new StreamReader(readStream))
            {
                string value = reader.ReadToEnd();

                var completionSource = new TaskCompletionSource<object>();
                completionSource.SetResult(value);
				Logger.LogWarning("<<< Exiting ReadFromStreamAsync(type={Type},readStream={ReadStream},content={Content},formatterLogger={FormatterLogger})", type, readStream, content, formatterLogger);
                return completionSource.Task;
            }
        }

        /// <inheritdoc />
        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext)
        {
			Logger.LogError(">>> Entering WriteToStreamAsync(type={Type},value={Value},writeStream={WriteStream},content={Content},transportContext={TransportContext})", type, value, writeStream, content, transportContext);
            using (var writer = new StreamWriter(writeStream))
            {
                writer.Write((string)value);
                writer.Flush();
            }

            var completionSource = new TaskCompletionSource<object>();
			Logger.LogError("WriteToStreamAsync(type={Type},value={Value},writeStream={WriteStream},content={Content},transportContext={TransportContext}): completionSource → {CompletionSource}", type, value, writeStream, content, transportContext, completionSource);
            completionSource.SetResult(null);
			Logger.LogWarning("<<< Exiting WriteToStreamAsync(type={Type},value={Value},writeStream={WriteStream},content={Content},transportContext={TransportContext})", type, value, writeStream, content, transportContext);
            return completionSource.Task;
        }
    }
}
