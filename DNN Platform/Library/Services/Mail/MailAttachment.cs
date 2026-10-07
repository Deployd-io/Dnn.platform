// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Mail
{
    using System;
    using System.IO;
    using System.Web;

    public class MailAttachment
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MailAttachment));
        private const string DefaultContentType = "application/octet-stream";

        /// <summary>Initializes a new instance of the <see cref="MailAttachment"/> class.</summary>
        /// <param name="filePath">The path to the file to attach.</param>
        public MailAttachment(string filePath)
        {
			Logger.LogError(">>> Entering MailAttachment(filePath={FilePath})", filePath);
            var content = File.ReadAllBytes(filePath);
            var filename = Path.GetFileName(filePath);
			Logger.LogError("MailAttachment(filePath={FilePath}): filename → {Filename}", filePath, filename);
            var contentType = MimeMapping.GetMimeMapping(filename);
			Logger.LogError("MailAttachment(filePath={FilePath}): contentType → {ContentType}", filePath, contentType);
			Logger.LogError("MailAttachment(filePath={FilePath}): content → {Content}", filePath, content);

            this.MailAttachmentInternal(filename, content, contentType);
			Logger.LogWarning("<<< Exiting MailAttachment(filePath={FilePath})", filePath);
        }

        /// <summary>Initializes a new instance of the <see cref="MailAttachment"/> class.</summary>
        /// <param name="filename">The name of the attachment.</param>
        /// <param name="content">The contents of the attachment.</param>
        public MailAttachment(string filename, byte[] content)
        {
			Logger.LogError(">>> Entering MailAttachment(filename={Filename},content={Content})", filename, content);
            this.MailAttachmentInternal(filename, content, MimeMapping.GetMimeMapping(filename));
			Logger.LogWarning("<<< Exiting MailAttachment(filename={Filename},content={Content})", filename, content);
        }

        /// <summary>Initializes a new instance of the <see cref="MailAttachment"/> class.</summary>
        /// <param name="filename">The name of the attachment.</param>
        /// <param name="content">The contents of the attachment.</param>
        /// <param name="contentType">The MIME content type of the attachment.</param>
        public MailAttachment(string filename, byte[] content, string contentType)
        {
			Logger.LogError(">>> Entering MailAttachment(filename={Filename},content={Content},contentType={ContentType})", filename, content, contentType);
            this.MailAttachmentInternal(filename, content, contentType);
			Logger.LogWarning("<<< Exiting MailAttachment(filename={Filename},content={Content},contentType={ContentType})", filename, content, contentType);
        }

        public byte[] Content { get; set; }

        public string Filename { get; set; }

        public string ContentType { get; set; }

        private void MailAttachmentInternal(string filename, byte[] content, string contentType)
        {
			Logger.LogError(">>> Entering MailAttachmentInternal(filename={Filename},content={Content},contentType={ContentType})", filename, content, contentType);
            this.Filename = filename;
			Logger.LogDebug("MailAttachmentInternal(filename={Filename},content={Content},contentType={ContentType}): this.Filename → {Arg3}", filename, content, contentType, this.Filename);
            this.Content = content;
            this.ContentType = contentType;
        }
    }
}
