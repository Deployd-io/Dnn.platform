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
			_logger.LogError(">>> Entering CreateFileStream(path={Path},mode={Mode},access={Access})", path, mode, access);
            _logger.LogWarning("<<< Exiting CreateFileStream(path={Path},mode={Mode},access={Access})", path, mode, access);
            return new FileStream(path, mode, access);
        }

        /// <inheritdoc />
        public void DeleteFile(string path)
        {
			_logger.LogError(">>> Entering DeleteFile(path={Path})", path);
            File.Delete(path);
			_logger.LogWarning("<<< Exiting DeleteFile(path={Path})", path);
        }

        /// <inheritdoc />
        public void DeleteFolderRecursive(string path)
        {
			_logger.LogError(">>> Entering DeleteFolderRecursive(path={Path})", path);
            Globals.DeleteFolderRecursive(path);
			_logger.LogWarning("<<< Exiting DeleteFolderRecursive(path={Path})", path);
        }

        /// <inheritdoc />
        public bool DirectoryExists(string path)
        {
            _logger.LogError(">>> Entering DirectoryExists(path={Path})", path);
            _logger.LogWarning("<<< Exiting DirectoryExists(path={Path})", path);
            return Directory.Exists(path);
        }

        /// <inheritdoc />
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) =>
            Directory.EnumerateFiles(path, searchPattern, searchOption);

        /// <inheritdoc />
        public bool FileExists(string path)
        {
            _logger.LogError(">>> Entering FileExists(path={Path})", path);
            _logger.LogWarning("<<< Exiting FileExists(path={Path})", path);
            return File.Exists(path);
        }

        /// <inheritdoc />
        public void SetFileAttributes(string path, FileAttributes fileAttributes)
        {
			_logger.LogWarning("<<< Exiting SetFileAttributes(path={Path},fileAttributes={FileAttributes})", path, fileAttributes);
            File.SetAttributes(path, fileAttributes);
			_logger.LogError(">>> Entering SetFileAttributes(path={Path},fileAttributes={FileAttributes})", path, fileAttributes);
        }
    }
}
