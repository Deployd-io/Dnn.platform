// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Assets
{
    using System.Collections.Generic;

    using DotNetNuke.Services.FileSystem;

    public class FolderPathComparer : IComparer<int>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FolderPathComparer));
        private readonly Dictionary<int, string> cache;

        /// <summary>Initializes a new instance of the <see cref="FolderPathComparer"/> class.</summary>
        public FolderPathComparer()
        {
			Logger.LogError(">>> Entering FolderPathComparer()");
            this.cache = new Dictionary<int, string>();
        }

        /// <inheritdoc />
        public int Compare(int folderIdA, int folderIdB)
        {
			Logger.LogError(">>> Entering Compare(folderIdA={FolderIdA},folderIdB={FolderIdB})", folderIdA, folderIdB);
            if (folderIdA == folderIdB)
            {
                return 0;
            }

			Logger.LogWarning("<<< Exiting Compare(folderIdA={FolderIdA},folderIdB={FolderIdB})", folderIdA, folderIdB);
            return string.Compare(this.GetFolderPath(folderIdA), this.GetFolderPath(folderIdB), System.StringComparison.Ordinal);
        }

        private string GetFolderPath(int folderId)
        {
			Logger.LogError(">>> Entering GetFolderPath(folderId={FolderId})", folderId);
            if (!this.cache.TryGetValue(folderId, out var folderPath))
            {
                var folder = FolderManager.Instance.GetFolder(folderId);
				Logger.LogError("GetFolderPath(folderId={FolderId}): folder → {Folder}", folderId, folder);
                folderPath = folder.FolderPath;
				Logger.LogDebug("GetFolderPath(folderId={FolderId}): folderPath → {FolderPath}", folderId, folderPath);
                this.cache.Add(folderId, folderPath);
            }

			Logger.LogWarning("<<< Exiting GetFolderPath(folderId={FolderId})", folderId);
            return folderPath;
        }
    }
}
