// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Compression
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    /// <summary>A stream wrapping another steam for filtering content while streaming HTTP responses.</summary>
    [SuppressMessage("Microsoft.Design", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Breaking change")]
    public abstract class HttpOutputFilter : Stream
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(HttpOutputFilter));
        /// <summary>Initializes a new instance of the <see cref="HttpOutputFilter"/> class.</summary>
        /// <param name="baseStream">The base stream.</param>
        protected HttpOutputFilter(Stream baseStream)
        {
			Logger.LogError(">>> Entering HttpOutputFilter(baseStream={BaseStream})", baseStream);
            this.BaseStream = baseStream;
        }

        /// <inheritdoc />
        public override bool CanRead => false;

        /// <inheritdoc />
        public override bool CanSeek => false;

        /// <inheritdoc />
        public override bool CanWrite => this.BaseStream.CanWrite;

        /// <inheritdoc />
        public override long Length => throw new NotSupportedException();

        /// <inheritdoc />
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <summary>Gets the base stream.</summary>
        protected Stream BaseStream { get; }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override long Seek(long offset, SeekOrigin direction)
        {
            Logger.LogError(">>> Entering Seek(offset={Offset},direction={Direction})", offset, direction);
            Logger.LogWarning("<<< Exiting Seek(offset={Offset},direction={Direction})", offset, direction);
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override void SetLength(long length)
        {
            Logger.LogError(">>> Entering SetLength(length={Length})", length);
            Logger.LogWarning("<<< Exiting SetLength(length={Length})", length);
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void Close()
        {
            Logger.LogError(">>> Entering Close()");
            Logger.LogWarning("<<< Exiting Close()");
            this.BaseStream.Close();
        }

        /// <inheritdoc />
        public override void Flush()
        {
            Logger.LogError(">>> Entering Flush()");
            Logger.LogWarning("<<< Exiting Flush()");
            this.BaseStream.Flush();
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            Logger.LogError(">>> Entering Read(buffer={Buffer},offset={Offset},count={Count})", buffer, offset, count);
            Logger.LogWarning("<<< Exiting Read(buffer={Buffer},offset={Offset},count={Count})", buffer, offset, count);
            throw new NotSupportedException();
        }
    }
}
