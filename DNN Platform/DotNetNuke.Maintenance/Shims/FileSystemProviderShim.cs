// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using System.Collections.Generic;
    using System.IO;

    using DotNetNuke.Common;
	using Microsoft.Extensions.Logging;

    /// <summary>Abstraction on file system utilities to enable unit testing.</summary>
    internal sealed class FileSystemProviderShim : IFileSystemProvider
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <inheritdoc />
        public Stream CreateFileStream(string path, FileMode mode, FileAccess access)
        {
			_logger.LogDebug(">>> Entering CreateFileStream(path={Path},mode={Mode},access={Access})", path, mode, access);
			_logger.LogDebug("<<< Exiting CreateFileStream(path={Path},mode={Mode},access={Access})", path, mode, access);
            return new FileStream(path, mode, access);
        }

        /// <inheritdoc />
        public void DeleteFile(string path)
        {
			_logger.LogDebug(">>> Entering DeleteFile(path={Path})", path);
            File.Delete(path);
			_logger.LogDebug("<<< Exiting DeleteFile(path={Path})", path);
        }

        /// <inheritdoc />
        public void DeleteFolderRecursive(string path)
        {
			_logger.LogDebug(">>> Entering DeleteFolderRecursive(path={Path})", path);
            Globals.DeleteFolderRecursive(path);
			_logger.LogDebug("<<< Exiting DeleteFolderRecursive(path={Path})", path);
        }

        /// <inheritdoc />
        public bool DirectoryExists(string path) => Directory.Exists(path);

        /// <inheritdoc />
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) =>
            Directory.EnumerateFiles(path, searchPattern, searchOption);

        /// <inheritdoc />
        public bool FileExists(string path) => File.Exists(path);

        /// <inheritdoc />
        public void SetFileAttributes(string path, FileAttributes fileAttributes)
        {
			_logger.LogDebug(">>> Entering SetFileAttributes(path={Path},fileAttributes={FileAttributes})", path, fileAttributes);
            File.SetAttributes(path, fileAttributes);
			_logger.LogDebug("<<< Exiting SetFileAttributes(path={Path},fileAttributes={FileAttributes})", path, fileAttributes);
        }
    }
}
