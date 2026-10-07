// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Providers.FolderProviders.AzureFolderProvider
{
    using System;

    using Microsoft.WindowsAzure.Storage.Blob;

    internal static class BlobItemExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(BlobItemExtensions));
        public static string RelativePath(this IListBlobItem item)
        {
			Logger.LogError(">>> Entering RelativePath(item={Item})", item);
            var filterUrl = item.Container.Uri.AbsoluteUri;
			Logger.LogError("RelativePath(item={Item}): filterUrl → {FilterUrl}", item, filterUrl);
            if (!filterUrl.EndsWith("/", StringComparison.Ordinal))
            {
                filterUrl = filterUrl + "/";
            }

			Logger.LogWarning("<<< Exiting RelativePath(item={Item})", item);
            return item.Uri.AbsoluteUri.Replace(filterUrl, string.Empty);
        }
    }
}
