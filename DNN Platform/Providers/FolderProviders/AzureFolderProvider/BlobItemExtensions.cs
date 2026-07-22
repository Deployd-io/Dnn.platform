// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Providers.FolderProviders.AzureFolderProvider
{
    using System;

    using Microsoft.WindowsAzure.Storage.Blob;

    internal static class BlobItemExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static string RelativePath(this IListBlobItem item)
        {
			Logger.LogDebug(">>> Entering RelativePath(item={Item})", item);
            var filterUrl = item.Container.Uri.AbsoluteUri;
            if (!filterUrl.EndsWith("/", StringComparison.Ordinal))
			Logger.LogDebug("RelativePath(item={Item}): filterUrl → {FilterUrl}", item, filterUrl);
            {
                filterUrl = filterUrl + "/";
            }

			Logger.LogDebug("<<< Exiting RelativePath(item={Item})", item);
            return item.Uri.AbsoluteUri.Replace(filterUrl, string.Empty);
        }
    }
}
