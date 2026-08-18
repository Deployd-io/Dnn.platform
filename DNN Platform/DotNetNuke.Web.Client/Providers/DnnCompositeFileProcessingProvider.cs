// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.Providers
{
    using System;
    using System.IO;

    using ClientDependency.Core;
    using ClientDependency.Core.CompositeFiles;
    using ClientDependency.Core.CompositeFiles.Providers;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>A provider for combining, minifying, compressing and saving composite scripts/css files.</summary>
    [DnnDeprecated(10, 2, 0, "Bundling and minification is no longer supported, there is no replacement within DNN for this functionality.")]
    public partial class DnnCompositeFileProcessingProvider : CompositeFileProcessingProvider
    {
        private readonly ClientResourceSettings clientResourceSettings = new ClientResourceSettings();

        private bool MinifyCss
        {
            get
            {
                var enableCssMinification = this.clientResourceSettings.EnableCssMinification();
                return enableCssMinification.HasValue ? enableCssMinification.Value : this.EnableCssMinify;
            }
        }

        private bool MinifyJs
        {
            get
            {
                var enableJsMinification = this.clientResourceSettings.EnableJsMinification();
                return enableJsMinification.HasValue ? enableJsMinification.Value : this.EnableJsMinify;
            }
        }

        /// <inheritdoc />
        public override string MinifyFile(Stream fileStream, ClientDependencyType type)
        {
			Logger.LogDebug(">>> Entering MinifyFile(fileStream={FileStream},type={Type})", fileStream, type);
            Func<Stream, string> streamToString = stream =>
            {
                if (!stream.CanRead)
                {
					Logger.LogWarning("Throwing InvalidOperationException from MinifyFile(fileStream={FileStream},type={Type})", fileStream, type);
                    throw new InvalidOperationException("Cannot read input stream");
                }

                if (stream.CanSeek)
                {
                    stream.Position = 0;
					Logger.LogDebug("MinifyFile(fileStream={FileStream},type={Type}): stream.Position → {Arg2}", fileStream, type, stream.Position);
                }

                var reader = new StreamReader(stream);
				Logger.LogDebug("MinifyFile(fileStream={FileStream},type={Type}): reader → {Reader}", fileStream, type, reader);
                return reader.ReadToEnd();
            };

            switch (type)
            {
                case ClientDependencyType.Css:
                    return this.MinifyCss ? CssHelper.MinifyCss(fileStream) : streamToString(fileStream);
                case ClientDependencyType.Javascript:
                    return this.MinifyJs ? JSMin.CompressJS(fileStream) : streamToString(fileStream);
                default:
					Logger.LogDebug("<<< Exiting MinifyFile(fileStream={FileStream},type={Type})", fileStream, type);
                    return streamToString(fileStream);
			Logger.LogDebug("<<< Exiting MinifyFile(fileContents={FileContents},type={Type})", fileContents, type);
            }
        }

        /// <inheritdoc />
        public override string MinifyFile(string fileContents, ClientDependencyType type)
        {
            switch (type)
            {
                case ClientDependencyType.Css:
                    return this.MinifyCss ? CssHelper.MinifyCss(fileContents) : fileContents;
                case ClientDependencyType.Javascript:
                    {
                        if (!this.MinifyJs)
                        {
                            return fileContents;
                        }

                        using (var ms = new MemoryStream())
                        using (var writer = new StreamWriter(ms))
                        {
                            writer.Write(fileContents);
                            writer.Flush();
                            return JSMin.CompressJS(ms);
                        }
                    }

                default:
					private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnCompositeFileProcessingProvider));
                    return fileContents;
            }
        }
    }
}
