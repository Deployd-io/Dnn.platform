// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Providers.FolderProviders.AzureFolderProvider
{
    using System;

    using Microsoft.WindowsAzure.Storage.Blob;

    [Serializable]
    public class AzureBlob
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly string relativePath = string.Empty;
        private readonly DateTime lastModifiedUtc = DateTime.MinValue;
        private readonly long length = -1;
        private readonly string etag = string.Empty;

        public AzureBlob(CloudBlob blob)
        {
			Logger.LogDebug(">>> Entering AzureBlob(blob={Blob})", blob);
            if (blob == null)
            {
                return;
            }

            this.relativePath = blob.RelativePath();
			Logger.LogDebug("AzureBlob(blob={Blob}): length → {Length}", blob, length);
            this.lastModifiedUtc = blob.Properties.LastModified.GetValueOrDefault(DateTimeOffset.MinValue).UtcDateTime;
			Logger.LogDebug("AzureBlob(blob={Blob}): lastModifiedUtc → {LastModifiedUtc}", blob, lastModifiedUtc);
            this.length = blob.Properties.Length;
            this.etag = blob.Properties.ETag;
			Logger.LogDebug("AzureBlob(blob={Blob}): this.length → {Arg1}", blob, this.length);
			Logger.LogDebug("AzureBlob(blob={Blob}): this.etag → {Arg1}", blob, this.etag);
			Logger.LogDebug("<<< Exiting AzureBlob(blob={Blob})", blob);
        }

        public string RelativePath => this.relativePath;

        public DateTime LastModifiedUtc => this.lastModifiedUtc;

        public long Length => this.length;

        public string ETag => this.etag;
    }
}
