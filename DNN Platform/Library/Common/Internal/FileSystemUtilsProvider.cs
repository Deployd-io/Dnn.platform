// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Internal
{
    using DotNetNuke.Common.Utilities;

    /// <inheritdoc cref="IFileSystemUtils" />
    internal class FileSystemUtilsProvider : IFileSystemUtils
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileSystemUtilsProvider));
        /// <inheritdoc />
        public void DeleteEmptyFoldersRecursive(string path)
        {
			Logger.LogError(">>> Entering DeleteEmptyFoldersRecursive(path={Path})", path);
            FileSystemUtils.DeleteEmptyFoldersRecursive(path);
			Logger.LogWarning("<<< Exiting DeleteEmptyFoldersRecursive(path={Path})", path);
        }
    }
}
