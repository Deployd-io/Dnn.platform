// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Compression
{
    using System.IO;
    using System.IO.Compression;

    /// <summary>This is a little filter to support HTTP compression using GZip.</summary>
    public class GZipFilter : CompressingFilter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(GZipFilter));
        private readonly GZipStream stream;

        /// <summary>Initializes a new instance of the <see cref="GZipFilter"/> class.</summary>
        /// <param name="baseStream">The base stream.</param>
        public GZipFilter(Stream baseStream)
            : base(baseStream)
        {
			Logger.LogDebug(">>> Entering GZipFilter(baseStream={BaseStream})", baseStream);
            this.stream = new GZipStream(baseStream, CompressionMode.Compress);
			Logger.LogDebug("GZipFilter(baseStream={BaseStream}): stream → {Stream}", baseStream, stream);
			Logger.LogDebug("<<< Exiting GZipFilter(baseStream={BaseStream})", baseStream);
        }

        /// <inheritdoc />
        public override string ContentEncoding
        {
            get
            {
                return "gzip";
            }
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
			Logger.LogDebug(">>> Entering Write(buffer={Buffer},offset={Offset},count={Count})", buffer, offset, count);
            if (!this.HasWrittenHeaders)
            {
                this.WriteHeaders();
            }

            this.stream.Write(buffer, offset, count);
			Logger.LogDebug("<<< Exiting Write(buffer={Buffer},offset={Offset},count={Count})", buffer, offset, count);
        }

        /// <inheritdoc />
        public override void Close()
        {
			Logger.LogDebug(">>> Entering Close()");
            this.stream.Close();
			Logger.LogDebug("<<< Exiting Close()");
        }

        /// <inheritdoc />
        public override void Flush()
        {
			Logger.LogDebug(">>> Entering Flush()");
            this.stream.Flush();
			Logger.LogDebug("<<< Exiting Flush()");
        }
    }
}
