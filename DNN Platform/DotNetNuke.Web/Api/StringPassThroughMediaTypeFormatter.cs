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
		Logger.LogDebug(">>> Entering StringPassThroughMediaTypeFormatter()");
		Logger.LogDebug("<<< Exiting StringPassThroughMediaTypeFormatter()");
		Logger.LogDebug(">>> Entering StringPassThroughMediaTypeFormatter(mediaTypes={MediaTypes})", mediaTypes);
		Logger.LogDebug("<<< Exiting StringPassThroughMediaTypeFormatter(mediaTypes={MediaTypes})", mediaTypes);
        }

        /// <summary>Initializes a new instance of the <see cref="StringPassThroughMediaTypeFormatter"/> class that can handle the specified media types.</summary>
        /// <param name="mediaTypes">The supported media types.</param>
        public StringPassThroughMediaTypeFormatter(IEnumerable<string> mediaTypes)
        {
            foreach (var type in mediaTypes)
            {
                this.SupportedMediaTypes.Add(new MediaTypeHeaderValue(type));
            }
        }

        /// <inheritdoc />
        public override bool CanReadType(Type type)
        {
			Logger.LogDebug(">>> Entering CanReadType(type={Type})", type);
			Logger.LogDebug("CanReadType(type={Type}): type → {Type1}", type, type);
			Logger.LogDebug("<<< Exiting CanReadType(type={Type})", type);
            return type == typeof(string);
        }

        /// <inheritdoc />
        public override bool CanWriteType(Type type)
        {
			Logger.LogDebug(">>> Entering CanWriteType(type={Type})", type);
			Logger.LogDebug("CanWriteType(type={Type}): type → {Type1}", type, type);
			Logger.LogDebug("<<< Exiting CanWriteType(type={Type})", type);
            return type == typeof(string);
        }

        /// <inheritdoc />
        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
			Logger.LogDebug(">>> Entering ReadFromStreamAsync(type={Type},readStream={ReadStream},content={Content},formatterLogger={FormatterLogger})", type, readStream, content, formatterLogger);
            using (var reader = new StreamReader(readStream))
            {
                string value = reader.ReadToEnd();

                var completionSource = new TaskCompletionSource<object>();
                completionSource.SetResult(value);
				Logger.LogDebug("<<< Exiting ReadFromStreamAsync(type={Type},readStream={ReadStream},content={Content},formatterLogger={FormatterLogger})", type, readStream, content, formatterLogger);
                return completionSource.Task;
            }
        }

        /// <inheritdoc />
        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext)
        {
			Logger.LogDebug(">>> Entering WriteToStreamAsync(type={Type},value={Value},writeStream={WriteStream},content={Content},transportContext={TransportContext})", type, value, writeStream, content, transportContext);
            using (var writer = new StreamWriter(writeStream))
            {
                writer.Write((string)value);
                writer.Flush();
            }

            var completionSource = new TaskCompletionSource<object>();
			Logger.LogDebug("WriteToStreamAsync(type={Type},value={Value},writeStream={WriteStream},content={Content},transportContext={TransportContext}): completionSource → {CompletionSource}", type, value, writeStream, content, transportContext, completionSource);
            completionSource.SetResult(null);
			Logger.LogDebug("<<< Exiting WriteToStreamAsync(type={Type},value={Value},writeStream={WriteStream},content={Content},transportContext={TransportContext})", type, value, writeStream, content, transportContext);
            return completionSource.Task;
        }
    }
}
