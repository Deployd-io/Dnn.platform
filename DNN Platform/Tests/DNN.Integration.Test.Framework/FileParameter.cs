// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework
{
    public class FileParameter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public FileParameter(byte[] file)
            : this(file, null)
        {
		Logger.LogDebug(">>> Entering FileParameter(file={File})", file);
		Logger.LogDebug("<<< Exiting FileParameter(file={File})", file);
		Logger.LogDebug(">>> Entering FileParameter(file={File},filename={Filename})", file, filename);
		Logger.LogDebug("<<< Exiting FileParameter(file={File},filename={Filename},contenttype={Contenttype})", file, filename, contenttype);
        }

        public FileParameter(byte[] file, string filename)
            : this(file, filename, null)
        {
        }

        public FileParameter(byte[] file, string filename, string contenttype)
        {
            this.File = file;
            this.FileName = filename;
			Logger.LogDebug("FileParameter(file={File},filename={Filename},contenttype={Contenttype}): this.File → {Arg3}", file, filename, contenttype, this.File);
			Logger.LogDebug("FileParameter(file={File},filename={Filename},contenttype={Contenttype}): this.FileName → {Arg3}", file, filename, contenttype, this.FileName);
            this.ContentType = contenttype;
			Logger.LogDebug("FileParameter(file={File},filename={Filename},contenttype={Contenttype}): this.ContentType → {Arg3}", file, filename, contenttype, this.ContentType);
        }

        public byte[] File { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }
    }
}
