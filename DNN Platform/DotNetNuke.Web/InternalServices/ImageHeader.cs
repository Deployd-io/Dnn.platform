// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    /// <summary>
    ///     Taken from http://stackoverflow.com/questions/111345/getting-image-dimensions-without-reading-the-entire-file/111349
    ///     Minor improvements including supporting unsigned 16-bit integers when decoding Jfif and added logic
    ///     to load the image using new Bitmap if reading the headers fails.
    /// </summary>
    public static class ImageHeader
    {
        private const string ErrorMessage = "Could not recognize image format.";

        private static readonly Dictionary<byte[], Func<BinaryReader, Size>> ImageFormatDecoders = new Dictionary
            <byte[], Func<BinaryReader, Size>>
            {
                { new byte[] { 0x42, 0x4D }, DecodeBitmap },
                { new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, DecodeGif },
                { new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, DecodeGif },
                { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, DecodePng },
                { new byte[] { 0xff, 0xd8 }, DecodeJfif },
            };

        /// <summary>    Gets the dimensions of an image.</summary>
        /// <param name="path">The path of the image to get the dimensions of.</param>
        /// <returns>The dimensions of the specified image.</returns>
        /// <exception cref="ArgumentException">The image was of an unrecognised format.</exception>
        public static Size GetDimensions(string path)
        {
			Logger.LogDebug(">>> Entering GetDimensions(path={Path})", path);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            Size size;
            try
            {
                using (var binaryReader = new BinaryReader(File.OpenRead(path)))
                {
                    try
                    {
                        size = GetDimensions(binaryReader);
                    }
                    catch (ArgumentException e)
                    {
						Logger.LogError("Exception in GetDimensions(path={Path}): {Arg1}", path, ex.Message);
                        throw new ArgumentException($"{ErrorMessage} file: '{path}' ", nameof(path), e);
                    }
                }
            }
            catch (ArgumentException)
            {
				Logger.LogError("Exception in GetDimensions(path={Path}): {Arg1}", path, ex.Message);
                // do it the old fashioned way
                using (var b = new Bitmap(path))
                {
                    size = b.Size;
                }
            }

			Logger.LogInformation("GetDimensions(path)={Path}: external service call took {Arg1} ms", path, __logStart.ElapsedMilliseconds);
            return size;
        }

        /// <summary>    Gets the dimensions of an image.</summary>
        /// <param name="binaryReader">The path of the image to get the dimensions of.</param>
        /// <returns>The dimensions of the specified image.</returns>
        /// <exception cref="ArgumentException">The image was of an unrecognised format.</exception>
        public static Size GetDimensions(BinaryReader binaryReader)
        {
			private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
            int maxMagicBytesLength = ImageFormatDecoders.Keys.OrderByDescending(x => x.Length).First().Length;
            var magicBytes = new byte[maxMagicBytesLength];
            for (var i = 0; i < maxMagicBytesLength; i += 1)
            {
                magicBytes[i] = binaryReader.ReadByte();
                foreach (var pair in ImageFormatDecoders)
                {
                    if (StartsWith(magicBytes, pair.Key))
                    {
                        return pair.Value(binaryReader);
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetDimensions(path={Path})", path);
            throw new ArgumentException(ErrorMessage, nameof(binaryReader));
        }

        private static bool StartsWith(byte[] thisBytes, byte[] thatBytes)
        {
			Logger.LogDebug(">>> Entering StartsWith(thisBytes={ThisBytes},thatBytes={ThatBytes})", thisBytes, thatBytes);
            for (var i = 0; i < thatBytes.Length; i += 1)
            {
                if (thisBytes[i] != thatBytes[i])
                {
                    return false;
                }
            }

			Logger.LogDebug("<<< Exiting StartsWith(thisBytes={ThisBytes},thatBytes={ThatBytes})", thisBytes, thatBytes);
            return true;
        }

        private static short ReadLittleEndianInt16(BinaryReader binaryReader)
        {
			Logger.LogDebug(">>> Entering ReadLittleEndianInt16(binaryReader={BinaryReader})", binaryReader);
            var bytes = new byte[sizeof(short)];
            for (var i = 0; i < sizeof(short); i += 1)
			Logger.LogDebug("ReadLittleEndianInt16(binaryReader={BinaryReader}): bytes → {Bytes}", binaryReader, bytes);
            {
                bytes[sizeof(short) - 1 - i] = binaryReader.ReadByte();
            }

			Logger.LogDebug("<<< Exiting ReadLittleEndianInt16(binaryReader={BinaryReader})", binaryReader);
            return BitConverter.ToInt16(bytes, 0);
        }

        private static ushort ReadLittleEndianUInt16(BinaryReader binaryReader)
        {
			Logger.LogDebug(">>> Entering ReadLittleEndianUInt16(binaryReader={BinaryReader})", binaryReader);
            var bytes = new byte[sizeof(ushort)];
            for (var i = 0; i < sizeof(ushort); i += 1)
			Logger.LogDebug("ReadLittleEndianUInt16(binaryReader={BinaryReader}): bytes → {Bytes}", binaryReader, bytes);
            {
                bytes[sizeof(ushort) - 1 - i] = binaryReader.ReadByte();
            }

			Logger.LogDebug("<<< Exiting ReadLittleEndianUInt16(binaryReader={BinaryReader})", binaryReader);
            return BitConverter.ToUInt16(bytes, 0);
        }

        private static int ReadLittleEndianInt32(BinaryReader binaryReader)
        {
			Logger.LogDebug(">>> Entering ReadLittleEndianInt32(binaryReader={BinaryReader})", binaryReader);
            var bytes = new byte[sizeof(int)];
            for (var i = 0; i < sizeof(int); i += 1)
			Logger.LogDebug("ReadLittleEndianInt32(binaryReader={BinaryReader}): bytes → {Bytes}", binaryReader, bytes);
            {
                bytes[sizeof(int) - 1 - i] = binaryReader.ReadByte();
            }

			Logger.LogDebug("<<< Exiting ReadLittleEndianInt32(binaryReader={BinaryReader})", binaryReader);
            return BitConverter.ToInt32(bytes, 0);
        }

        private static Size DecodeBitmap(BinaryReader binaryReader)
        {
			Logger.LogDebug(">>> Entering DecodeBitmap(binaryReader={BinaryReader})", binaryReader);
            binaryReader.ReadBytes(16);
            var width = binaryReader.ReadInt32();
            var height = binaryReader.ReadInt32();
			Logger.LogDebug("DecodeBitmap(binaryReader={BinaryReader}): width → {Width}", binaryReader, width);
			Logger.LogDebug("DecodeBitmap(binaryReader={BinaryReader}): height → {Height}", binaryReader, height);
			Logger.LogDebug("<<< Exiting DecodeBitmap(binaryReader={BinaryReader})", binaryReader);
            return new Size(width, height);
        }

        private static Size DecodeGif(BinaryReader binaryReader)
        {
			Logger.LogDebug(">>> Entering DecodeGif(binaryReader={BinaryReader})", binaryReader);
            int width = binaryReader.ReadInt16();
            int height = binaryReader.ReadInt16();
			Logger.LogDebug("DecodeGif(binaryReader={BinaryReader}): width → {Width}", binaryReader, width);
			Logger.LogDebug("DecodeGif(binaryReader={BinaryReader}): height → {Height}", binaryReader, height);
			Logger.LogDebug("<<< Exiting DecodeGif(binaryReader={BinaryReader})", binaryReader);
            return new Size(width, height);
        }

        private static Size DecodePng(BinaryReader binaryReader)
        {
			Logger.LogDebug(">>> Entering DecodePng(binaryReader={BinaryReader})", binaryReader);
            binaryReader.ReadBytes(8);
            var width = ReadLittleEndianInt32(binaryReader);
            var height = ReadLittleEndianInt32(binaryReader);
			Logger.LogDebug("DecodePng(binaryReader={BinaryReader}): width → {Width}", binaryReader, width);
			Logger.LogDebug("DecodePng(binaryReader={BinaryReader}): height → {Height}", binaryReader, height);
			Logger.LogDebug("<<< Exiting DecodePng(binaryReader={BinaryReader})", binaryReader);
            return new Size(width, height);
        }

        private static Size DecodeJfif(BinaryReader binaryReader)
        {
			Logger.LogDebug(">>> Entering DecodeJfif(binaryReader={BinaryReader})", binaryReader);
            while (binaryReader.ReadByte() == 0xff)
            {
                byte marker = binaryReader.ReadByte();
                short chunkLength = ReadLittleEndianInt16(binaryReader);
				Logger.LogDebug("DecodeJfif(binaryReader={BinaryReader}): marker → {Marker}", binaryReader, marker);
				Logger.LogDebug("DecodeJfif(binaryReader={BinaryReader}): chunkLength → {ChunkLength}", binaryReader, chunkLength);
                if (marker == 0xc0 || marker == 0xc2)
                {
                    binaryReader.ReadByte();
                    int height = ReadLittleEndianInt16(binaryReader);
                    int width = ReadLittleEndianInt16(binaryReader);
					Logger.LogDebug("DecodeJfif(binaryReader={BinaryReader}): height → {Height}", binaryReader, height);
					Logger.LogDebug("DecodeJfif(binaryReader={BinaryReader}): width → {Width}", binaryReader, width);
                    return new Size(width, height);
                }

                if (chunkLength < 0)
                {
                    var uchunkLength = (ushort)chunkLength;
                    binaryReader.ReadBytes(uchunkLength - 2);
					Logger.LogDebug("DecodeJfif(binaryReader={BinaryReader}): uchunkLength → {UchunkLength}", binaryReader, uchunkLength);
                }
                else
                {
                    binaryReader.ReadBytes(chunkLength - 2);
                }
            }

			Logger.LogDebug("<<< Exiting DecodeJfif(binaryReader={BinaryReader})", binaryReader);
            throw new ArgumentException(ErrorMessage);
        }
    }
}
