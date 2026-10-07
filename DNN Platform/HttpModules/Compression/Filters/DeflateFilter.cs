// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Compression
{
    using System.IO;
    using System.IO.Compression;

    /// <summary>A <see cref="CompressingFilter"/> using <see cref="DeflateStream"/> to compress the result.</summary>
    public class DeflateFilter : CompressingFilter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DeflateFilter));
        private readonly DeflateStream stream;

        /// <summary>Initializes a new instance of the <see cref="DeflateFilter"/> class.</summary>
        /// <param name="baseStream">The base stream.</param>
        public DeflateFilter(Stream baseStream)
            : base(baseStream)
        {
			Logger.LogError(">>> Entering DeflateFilter(baseStream={BaseStream})", baseStream);
            this.stream = new DeflateStream(baseStream, CompressionMode.Compress);
			Logger.LogDebug("DeflateFilter(baseStream={BaseStream}): stream → {Stream}", baseStream, stream);
        Logger.LogWarning("<<< Exiting DeflateFilter(baseStream={BaseStream})", baseStream);
        }

        /// <inheritdoc />
        public override string ContentEncoding
        {
            get
            {
                return "deflate";
            }
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
			Logger.LogError(">>> Entering Write(buffer={Buffer},offset={Offset},count={Count})", buffer, offset, count);
            if (!this.HasWrittenHeaders)
            {
                this.WriteHeaders();
            }

            this.stream.Write(buffer, offset, count);
			Logger.LogWarning("<<< Exiting Write(buffer={Buffer},offset={Offset},count={Count})", buffer, offset, count);
        }

        /// <inheritdoc />
        public override void Close()
        {
			Logger.LogError(">>> Entering Close()");
            this.stream.Close();
			Logger.LogWarning("<<< Exiting Close()");
        }

        /// <inheritdoc />
        public override void Flush()
        {
			Logger.LogError(">>> Entering Flush()");
            this.stream.Flush();
			Logger.LogWarning("<<< Exiting Flush()");
        }
    }
}
