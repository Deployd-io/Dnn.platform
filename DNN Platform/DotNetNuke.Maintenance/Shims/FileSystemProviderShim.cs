// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using System.Collections.Generic;
    using System.IO;

    using DotNetNuke.Common;

    /// <summary>Abstraction on file system utilities to enable unit testing.</summary>
    internal sealed class FileSystemProviderShim : IFileSystemProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public Stream CreateFileStream(string path, FileMode mode, FileAccess access)
        {
			Logger.LogDebug(">>> Entering CreateFileStream(path={Path},mode={Mode},access={Access})", path, mode, access);
			Logger.LogDebug("<<< Exiting CreateFileStream(path={Path},mode={Mode},access={Access})", path, mode, access);
            return new FileStream(path, mode, access);
        }

        /// <inheritdoc />
        public void DeleteFile(string path)
        {
			Logger.LogDebug(">>> Entering DeleteFile(path={Path})", path);
            File.Delete(path);
			Logger.LogDebug("<<< Exiting DeleteFile(path={Path})", path);
        }

        /// <inheritdoc />
        public void DeleteFolderRecursive(string path)
        {
			Logger.LogDebug(">>> Entering DeleteFolderRecursive(path={Path})", path);
            Globals.DeleteFolderRecursive(path);
			Logger.LogDebug("<<< Exiting DeleteFolderRecursive(path={Path})", path);
        }

        /// <inheritdoc />
        public bool DirectoryExists(string path) => Directory.Exists(path);

        /// <inheritdoc />
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) =>
            Directory.EnumerateFiles(path, searchPattern, searchOption);
			Logger.LogDebug(">>> Entering EnumerateFiles(path={Path},searchPattern={SearchPattern},searchOption={SearchOption})", path, searchPattern, searchOption);

		Logger.LogDebug("<<< Exiting EnumerateFiles(path={Path},searchPattern={SearchPattern},searchOption={SearchOption})", path, searchPattern, searchOption);
        /// <inheritdoc />
        public bool FileExists(string path) => File.Exists(path);

        /// <inheritdoc />
        public void SetFileAttributes(string path, FileAttributes fileAttributes)
        {
			Logger.LogDebug(">>> Entering SetFileAttributes(path={Path},fileAttributes={FileAttributes})", path, fileAttributes);
            File.SetAttributes(path, fileAttributes);
			Logger.LogDebug("<<< Exiting SetFileAttributes(path={Path},fileAttributes={FileAttributes})", path, fileAttributes);
        }
    }
}
