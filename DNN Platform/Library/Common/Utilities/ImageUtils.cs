// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;

    using DotNetNuke.Services.FileSystem.Internal;

    public class ImageUtils
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ImageUtils));
        /// <summary>return height of image.</summary>
        /// <param name="sPath">file path of image.</param>
        /// <returns>The image size in pixels.</returns>
        public static Size GetSize(string sPath)
        {
			Logger.LogError(">>> Entering GetSize(sPath={SPath})", sPath);
            Image g = Image.FromFile(sPath);
            Size s = g.Size;
			Logger.LogError("GetSize(sPath={SPath}): g → {G}", sPath, g);
            g.Dispose();
			Logger.LogWarning("<<< Exiting GetSize(sPath={SPath})", sPath);
            return s;
        }

        /// <summary>return height of image.</summary>
        /// <param name="sPath">file path of image.</param>
        /// <returns>The image height in pixels.</returns>
        public static int GetHeight(string sPath)
        {
			Logger.LogError(">>> Entering GetHeight(sPath={SPath})", sPath);
            Image g = Image.FromFile(sPath);
            int h = g.Height;
			Logger.LogError("GetHeight(sPath={SPath}): g → {G}", sPath, g);
            g.Dispose();
			Logger.LogWarning("<<< Exiting GetHeight(sPath={SPath})", sPath);
            return h;
        }

        /// <summary>return width of image.</summary>
        /// <param name="sPath">file path of image.</param>
        /// <returns>The image width in pixels.</returns>
        public static int GetWidth(string sPath)
        {
			Logger.LogError(">>> Entering GetWidth(sPath={SPath})", sPath);
            Image g = Image.FromFile(sPath);
            int w = g.Width;
			Logger.LogError("GetWidth(sPath={SPath}): g → {G}", sPath, g);
            g.Dispose();
			Logger.LogWarning("<<< Exiting GetWidth(sPath={SPath})", sPath);
            return w;
        }

        /// <summary>return height of image.</summary>
        /// <param name="sFile">Stream of image.</param>
        /// <returns>The image height in pixels.</returns>
        public static int GetHeightFromStream(Stream sFile)
        {
			Logger.LogError(">>> Entering GetHeightFromStream(sFile={SFile})", sFile);
            Image g = Image.FromStream(sFile, true);
            int h = g.Height;
			Logger.LogError("GetHeightFromStream(sFile={SFile}): g → {G}", sFile, g);
            g.Dispose();
			Logger.LogWarning("<<< Exiting GetHeightFromStream(sFile={SFile})", sFile);
            return h;
        }

        /// <summary>width of image.</summary>
        /// <param name="sFile">Steam of image.</param>
        /// <returns>The image width in pixels.</returns>
        public static int GetWidthFromStream(Stream sFile)
        {
			Logger.LogError(">>> Entering GetWidthFromStream(sFile={SFile})", sFile);
            Image g = Image.FromStream(sFile, true);
            int w = g.Width;
			Logger.LogError("GetWidthFromStream(sFile={SFile}): g → {G}", sFile, g);
            g.Dispose();
			Logger.LogWarning("<<< Exiting GetWidthFromStream(sFile={SFile})", sFile);
            return w;
        }

        /// <summary>create an image.</summary>
        /// <param name="sFile">path of load image file - will be resized according to height and width set.</param>
        /// <returns>The path of the created image.</returns>
        public static string CreateImage(string sFile)
        {
			Logger.LogError(">>> Entering CreateImage(sFile={SFile})", sFile);
            Image g = Image.FromFile(sFile);
            int h = g.Height;
			Logger.LogError("CreateImage(sFile={SFile}): g → {G}", sFile, g);
            int w = g.Width;
            g.Dispose();
			Logger.LogWarning("<<< Exiting CreateImage(sFile={SFile})", sFile);
			Logger.LogWarning("<<< Exiting CreateImage(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth})", sFile, intHeight, intWidth);
			Logger.LogWarning("<<< Exiting CreateImage(stream={Stream},intHeight={IntHeight},intWidth={IntWidth},extension={Extension})", stream, intHeight, intWidth, extension);
            return CreateImage(sFile, h, w);
        }

        /// <summary>create an image.</summary>
        /// <param name="sFile">path of image file.</param>
        /// <param name="intHeight">height.</param>
        /// <param name="intWidth">width.</param>
        /// <returns>The path of the created image.</returns>
        public static string CreateImage(string sFile, int intHeight, int intWidth)
        {
            var fi = new FileInfo(sFile);
            string tmp = fi.FullName.Replace(fi.Extension, "_TEMP" + fi.Extension);
            if (FileWrapper.Instance.Exists(tmp))
            {
                FileWrapper.Instance.SetAttributes(tmp, FileAttributes.Normal);
                FileWrapper.Instance.Delete(tmp);
            }

            File.Copy(sFile, tmp);

            using (var fileContent = File.OpenRead(tmp))
            using (var content = CreateImage(fileContent, intHeight, intWidth, fi.Extension))
            {
                string sFileExt = fi.Extension;
                string sFileNoExtension = Path.GetFileNameWithoutExtension(sFile);

                sFile += sFileNoExtension + sFileExt;
                if (FileWrapper.Instance.Exists(sFile))
                {
                    FileWrapper.Instance.SetAttributes(sFile, FileAttributes.Normal);
                    FileWrapper.Instance.Delete(sFile);
                }

                var arrData = new byte[2048];
                using (Stream outStream = FileWrapper.Instance.Create(sFile))
                {
                    long originalPosition = content.Position;
                    content.Position = 0;

                    try
                    {
                        int intLength = content.Read(arrData, 0, arrData.Length);

                        while (intLength > 0)
                        {
                            outStream.Write(arrData, 0, intLength);
                            intLength = content.Read(arrData, 0, arrData.Length);
                        }
                    }
                    finally
                    {
                        content.Position = originalPosition;
                    }
                }
            }

            if (FileWrapper.Instance.Exists(tmp))
            {
                FileWrapper.Instance.SetAttributes(tmp, FileAttributes.Normal);
                FileWrapper.Instance.Delete(tmp);
            }

            return sFile;
        }

        public static Stream CreateImage(Stream stream, int intHeight, int intWidth, string extension)
        {
            using (var original = new Bitmap(stream))
            {
                int imgHeight, imgWidth;
                PixelFormat format = original.PixelFormat;
                if (format.ToString().Contains("Indexed"))
                {
                    format = PixelFormat.Format24bppRgb;
                }

                int newHeight = intHeight;
                int newWidth = intWidth;
                Size imgSize;
                if (original.Width > newWidth || original.Height > newHeight)
                {
                    imgSize = NewImageSize(original.Width, original.Height, newWidth, newHeight);
                    imgHeight = imgSize.Height;
                    imgWidth = imgSize.Width;
                }
                else
                {
                    imgHeight = original.Height;
                    imgWidth = original.Width;
                }

                if (imgWidth < 1)
                {
                    imgWidth = 1;
                }

                if (imgHeight < 1)
                {
                    imgHeight = 1;
                }

                imgSize = new Size(imgWidth, imgHeight);

                using (var newImg = new Bitmap(imgWidth, imgHeight, format))
                {
                    newImg.SetResolution(original.HorizontalResolution, original.VerticalResolution);

                    using (Graphics canvas = Graphics.FromImage(newImg))
                    {
                        canvas.SmoothingMode = SmoothingMode.None;
                        canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        canvas.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        if (!extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
                        {
                            canvas.Clear(Color.White);
                            canvas.FillRectangle(Brushes.White, 0, 0, imgSize.Width, imgSize.Height);
                        }

                        canvas.DrawImage(original, 0, 0, imgSize.Width, imgSize.Height);

                        ////newImg.Save
                        ImageFormat imgFormat = ImageFormat.Bmp;
                        if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
                        {
                            imgFormat = ImageFormat.Png;
                        }
                        else if (extension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
                        {
                            imgFormat = ImageFormat.Gif;
                        }
                        else if (extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase))
                        {
                            imgFormat = ImageFormat.Jpeg;
                        }

                        var content = new MemoryStream();
                        newImg.Save(content, imgFormat);
                        return content;
                    }
                }
            }
        }

        /// <summary>create a JPG image.</summary>
        /// <param name="sFile">name of image.</param>
        /// <param name="img">bitmap of image.</param>
        /// <param name="compressionLevel">image quality.</param>
        /// <returns>The path of the created image.</returns>
        public static string CreateJPG(string sFile, Bitmap img, int compressionLevel)
        {
			Logger.LogError(">>> Entering CreateJPG(sFile={SFile},img={Img},compressionLevel={CompressionLevel})", sFile, img, compressionLevel);
            Graphics bmpOutput = Graphics.FromImage(img);
            bmpOutput.InterpolationMode = InterpolationMode.HighQualityBicubic;
			Logger.LogDebug("CreateJPG(sFile={SFile},img={Img},compressionLevel={CompressionLevel}): bmpOutput.InterpolationMode → {Arg3}", sFile, img, compressionLevel, bmpOutput.InterpolationMode);
            bmpOutput.SmoothingMode = SmoothingMode.HighQuality;
			Logger.LogDebug("CreateJPG(sFile={SFile},img={Img},compressionLevel={CompressionLevel}): bmpOutput.SmoothingMode → {Arg3}", sFile, img, compressionLevel, bmpOutput.SmoothingMode);
            var compressionRectange = new Rectangle(0, 0, img.Width, img.Height);
			Logger.LogError("CreateJPG(sFile={SFile},img={Img},compressionLevel={CompressionLevel}): compressionRectange → {CompressionRectange}", sFile, img, compressionLevel, compressionRectange);
			Logger.LogError("CreateJPG(sFile={SFile},img={Img},compressionLevel={CompressionLevel}): bmpOutput → {BmpOutput}", sFile, img, compressionLevel, bmpOutput);

            bmpOutput.DrawImage(img, compressionRectange);

            ImageCodecInfo myImageCodecInfo = GetEncoderInfo("image/jpeg");
			Logger.LogError("CreateJPG(sFile={SFile},img={Img},compressionLevel={CompressionLevel}): myImageCodecInfo → {MyImageCodecInfo}", sFile, img, compressionLevel, myImageCodecInfo);
            Encoder myEncoder = Encoder.Quality;
            var myEncoderParameters = new EncoderParameters(1);
			Logger.LogError("CreateJPG(sFile={SFile},img={Img},compressionLevel={CompressionLevel}): myEncoderParameters → {MyEncoderParameters}", sFile, img, compressionLevel, myEncoderParameters);
            var myEncoderParameter = new EncoderParameter(myEncoder, compressionLevel);
			Logger.LogError("CreateJPG(sFile={SFile},img={Img},compressionLevel={CompressionLevel}): myEncoderParameter → {MyEncoderParameter}", sFile, img, compressionLevel, myEncoderParameter);
            myEncoderParameters.Param[0] = myEncoderParameter;
            if (File.Exists(sFile))
            {
                File.Delete(sFile);
            }

            try
            {
                img.Save(sFile, myImageCodecInfo, myEncoderParameters);
            }
            catch (Exception)
            {
				Logger.LogError("Exception in CreateJPG(sFile={SFile},img={Img},compressionLevel={CompressionLevel}): {Arg3}", sFile, img, compressionLevel, Exception.Message);
                // suppress unexpected exceptions
            }

            img.Dispose();
            bmpOutput.Dispose();
			Logger.LogWarning("<<< Exiting CreateJPG(sFile={SFile},img={Img},compressionLevel={CompressionLevel})", sFile, img, compressionLevel);
            return sFile;
        }

        /// <summary>create an image based on a stream (read from a database).</summary>
        /// <param name="sFile">image name.</param>
        /// <param name="intHeight">height.</param>
        /// <param name="intWidth">width.</param>
        /// <returns>steam.</returns>
        public static MemoryStream CreateImageForDB(Stream sFile, int intHeight, int intWidth)
        {
			Logger.LogError(">>> Entering CreateImageForDB(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth})", sFile, intHeight, intWidth);
            var newStream = new MemoryStream();
            Image g = Image.FromStream(sFile);
			Logger.LogError("CreateImageForDB(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth}): g → {G}", sFile, intHeight, intWidth, g);
			Logger.LogError("CreateImageForDB(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth}): newStream → {NewStream}", sFile, intHeight, intWidth, newStream);
            int imgHeight, imgWidth;

            if (intHeight > 0 & intWidth > 0)
            {
                int newHeight = intHeight;
                int newWidth = intWidth;
                if (g.Width > newWidth | g.Height > newHeight)
                {
                    Size imgSize = NewImageSize(g.Width, g.Height, newWidth, newHeight);
					Logger.LogError("CreateImageForDB(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth}): imgSize → {ImgSize}", sFile, intHeight, intWidth, imgSize);
					Logger.LogDebug("CreateImageForDB(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth}): imgWidth → {ImgWidth}", sFile, intHeight, intWidth, imgWidth);
                    imgHeight = imgSize.Height;
					Logger.LogError("CreateImageForDB(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth}): imgHeight → {ImgHeight}", sFile, intHeight, intWidth, imgHeight);
                    imgWidth = imgSize.Width;
                }
                else
                {
					Logger.LogError("CreateImageForDB(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth}): imgWidth → {ImgWidth}", sFile, intHeight, intWidth, imgWidth);
                    imgHeight = g.Height;
					Logger.LogDebug("CreateImageForDB(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth}): imgHeight → {ImgHeight}", sFile, intHeight, intWidth, imgHeight);
                    imgWidth = g.Width;
                }
            }
            else
            {
                imgWidth = g.Width;
                imgHeight = g.Height;
            }

            var imgOutput1 = new Bitmap(g, imgWidth, imgHeight);
            Graphics bmpOutput = Graphics.FromImage(imgOutput1);
			Logger.LogError("CreateImageForDB(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth}): imgOutput1 → {ImgOutput1}", sFile, intHeight, intWidth, imgOutput1);
			Logger.LogError("CreateImageForDB(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth}): bmpOutput → {BmpOutput}", sFile, intHeight, intWidth, bmpOutput);
			Logger.LogDebug("CreateImageForDB(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth}): bmpOutput.SmoothingMode → {Arg3}", sFile, intHeight, intWidth, bmpOutput.SmoothingMode);
            bmpOutput.InterpolationMode = InterpolationMode.HighQualityBicubic;
			Logger.LogDebug("CreateImageForDB(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth}): bmpOutput.InterpolationMode → {Arg3}", sFile, intHeight, intWidth, bmpOutput.InterpolationMode);
            bmpOutput.SmoothingMode = SmoothingMode.HighQuality;
            var compressionRectange = new Rectangle(0, 0, imgWidth, imgHeight);
            bmpOutput.DrawImage(g, compressionRectange);
			Logger.LogError("CreateImageForDB(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth}): compressionRectange → {CompressionRectange}", sFile, intHeight, intWidth, compressionRectange);
            ImageCodecInfo myImageCodecInfo = GetEncoderInfo("image/jpeg");
			Logger.LogError("CreateImageForDB(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth}): myImageCodecInfo → {MyImageCodecInfo}", sFile, intHeight, intWidth, myImageCodecInfo);
            Encoder myEncoder = Encoder.Quality;
            var myEncoderParameters = new EncoderParameters(1);
			Logger.LogError("CreateImageForDB(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth}): myEncoderParameters → {MyEncoderParameters}", sFile, intHeight, intWidth, myEncoderParameters);
            var myEncoderParameter = new EncoderParameter(myEncoder, 90);
			Logger.LogError("CreateImageForDB(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth}): myEncoderParameter → {MyEncoderParameter}", sFile, intHeight, intWidth, myEncoderParameter);
            myEncoderParameters.Param[0] = myEncoderParameter;
            imgOutput1.Save(newStream, myImageCodecInfo, myEncoderParameters);
            g.Dispose();
            imgOutput1.Dispose();
            bmpOutput.Dispose();
			Logger.LogWarning("<<< Exiting CreateImageForDB(sFile={SFile},intHeight={IntHeight},intWidth={IntWidth})", sFile, intHeight, intWidth);
            return newStream;
        }

        /// <summary>return the appropriate encoded for the mime-type of the image being created.</summary>
        /// <param name="myMimeType">mime type (e.g jpg/png).</param>
        /// <returns>The <see cref="ImageCodecInfo"/> or <see langword="null"/>.</returns>
        public static ImageCodecInfo GetEncoderInfo(string myMimeType)
        {
			Logger.LogError(">>> Entering GetEncoderInfo(myMimeType={MyMimeType})", myMimeType);
            try
            {
                int i;
                ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
                for (i = 0; i <= (encoders.Length - 1); i++)
                {
                    if (encoders[i].MimeType == myMimeType)
                    {
                        return encoders[i];
                    }
                }

                return null;
            }
            catch
            {
				Logger.LogError("Exception in GetEncoderInfo(myMimeType={MyMimeType}): {Arg1}", myMimeType, Exception.Message);
				Logger.LogWarning("<<< Exiting GetEncoderInfo(myMimeType={MyMimeType})", myMimeType);
                return null;
            }
        }

        /// <summary>scale an image based on existing dimensions and updated requirement.</summary>
        /// <param name="currentWidth">current width.</param>
        /// <param name="currentHeight">current height.</param>
        /// <param name="newWidth">new width.</param>
        /// <param name="newHeight">new height.</param>
        /// <returns>updated calculated height/width dimensions.</returns>
        public static Size NewImageSize(int currentWidth, int currentHeight, int newWidth, int newHeight)
        {
			Logger.LogError(">>> Entering NewImageSize(currentWidth={CurrentWidth},currentHeight={CurrentHeight},newWidth={NewWidth},newHeight={NewHeight})", currentWidth, currentHeight, newWidth, newHeight);
            decimal decScale = ((decimal)currentWidth / (decimal)newWidth) > ((decimal)currentHeight / (decimal)newHeight) ? Convert.ToDecimal((decimal)currentWidth / (decimal)newWidth) : Convert.ToDecimal((decimal)currentHeight / (decimal)newHeight);
            newWidth = Convert.ToInt32(Math.Floor((decimal)currentWidth / decScale));
			Logger.LogError("NewImageSize(currentWidth={CurrentWidth},currentHeight={CurrentHeight},newWidth={NewWidth},newHeight={NewHeight}): newWidth → {NewWidth1}", currentWidth, currentHeight, newWidth, newHeight, newWidth);
            newHeight = Convert.ToInt32(Math.Floor((decimal)currentHeight / decScale));
			Logger.LogError("NewImageSize(currentWidth={CurrentWidth},currentHeight={CurrentHeight},newWidth={NewWidth},newHeight={NewHeight}): newHeight → {NewHeight1}", currentWidth, currentHeight, newWidth, newHeight, newHeight);

            var newSize = new Size(newWidth, newHeight);
			Logger.LogError("NewImageSize(currentWidth={CurrentWidth},currentHeight={CurrentHeight},newWidth={NewWidth},newHeight={NewHeight}): newSize → {NewSize}", currentWidth, currentHeight, newWidth, newHeight, newSize);

			Logger.LogWarning("<<< Exiting NewImageSize(currentWidth={CurrentWidth},currentHeight={CurrentHeight},newWidth={NewWidth},newHeight={NewHeight})", currentWidth, currentHeight, newWidth, newHeight);
            return newSize;
        }
    }
}
