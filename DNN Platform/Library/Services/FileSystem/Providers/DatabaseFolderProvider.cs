// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Services.FileSystem

// ReSharper restore CheckNamespace
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Services.FileSystem.Internal;

    public class DatabaseFolderProvider : SecureFolderProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DatabaseFolderProvider));
        /// <summary>Clears the content of the file in the database.</summary>
        /// <param name="fileId">The file identifier.</param>
        public static void ClearFileContent(int fileId)
        {
			Logger.LogError(">>> Entering ClearFileContent(fileId={FileId})", fileId);
            DataProvider.Instance().ClearFileContent(fileId);
            DataProvider.Instance().UpdateFileVersion(fileId, Guid.NewGuid());
			Logger.LogWarning("<<< Exiting ClearFileContent(fileId={FileId})", fileId);
        }

        /// <summary>Updates the content of the file in the database.</summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="content">The new content.</param>
        public static void UpdateFileContent(int fileId, Stream content)
        {
			Logger.LogError(">>> Entering UpdateFileContent(fileId={FileId},content={Content})", fileId, content);
            if (content != null)
            {
                byte[] fileContent;
                var buffer = new byte[16 * 1024];
                using (var ms = new MemoryStream())
                {
                    int read;
                    while ((read = content.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }

                    fileContent = ms.ToArray();
                }

                UpdateFileContent(fileId, fileContent);
            }
            else
            {
                ClearFileContent(fileId);
            }

            DataProvider.Instance().UpdateFileVersion(fileId, Guid.NewGuid());
			Logger.LogWarning("<<< Exiting UpdateFileContent(fileId={FileId},content={Content})", fileId, content);
        }

        /// <summary>Updates the content of the file in the database.</summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="content">The new content.</param>
        public static void UpdateFileContent(int fileId, byte[] content)
        {
            if (content != null)
            {
                DataProvider.Instance().UpdateFileContent(fileId, content);
                DataProvider.Instance().UpdateFileVersion(fileId, Guid.NewGuid());
            }
            else
            {
                ClearFileContent(fileId);
            }
        }

        /// <inheritdoc />
        public override void CopyFile(string folderPath, string fileName, string newFolderPath, FolderMappingInfo folderMapping)
        {
			Logger.LogError(">>> Entering CopyFile(folderPath={FolderPath},fileName={FileName},newFolderPath={NewFolderPath},folderMapping=<redacted>)", folderPath, fileName, newFolderPath);
            Requires.PropertyNotNull("folderPath", folderPath);
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.PropertyNotNull("newFolderPath", newFolderPath);
            Requires.NotNull("folderMapping", folderMapping);

            if (folderPath == newFolderPath)
            {
                return;
            }

            var sourceFolder = FolderManager.Instance.GetFolder(folderMapping.PortalID, folderPath);
			Logger.LogError("CopyFile(folderPath={FolderPath},fileName={FileName},newFolderPath={NewFolderPath},folderMapping=<redacted>): sourceFolder → {SourceFolder}", folderPath, fileName, newFolderPath, sourceFolder);
            var destinationFolder = FolderManager.Instance.GetFolder(folderMapping.PortalID, newFolderPath);
			Logger.LogError("CopyFile(folderPath={FolderPath},fileName={FileName},newFolderPath={NewFolderPath},folderMapping=<redacted>): destinationFolder → {DestinationFolder}", folderPath, fileName, newFolderPath, destinationFolder);

            Requires.NotNull("sourceFolder", sourceFolder);
            Requires.NotNull("destinationFolder", destinationFolder);

            using (var fileContent = this.GetFileStream(sourceFolder, fileName))
            {
                if (!fileContent.CanSeek)
                {
                    using (var seekableStream = FileManager.Instance.GetSeekableStream(fileContent))
                    {
                        this.AddFile(destinationFolder, fileName, seekableStream);
                    }
                }
                else
                {
                    this.AddFile(destinationFolder, fileName, fileContent);
                }
            }
			Logger.LogWarning("<<< Exiting CopyFile(folderPath={FolderPath},fileName={FileName},newFolderPath={NewFolderPath},folderMapping=<redacted>)", folderPath, fileName, newFolderPath);
        }

        /// <inheritdoc />
        public override void AddFile(IFolderInfo folder, string fileName, Stream content)
        {
			Logger.LogError(">>> Entering AddFile(folder={Folder},fileName={FileName},content={Content})", folder, fileName, content);
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);

            this.UpdateFile(folder, fileName, content);
			Logger.LogWarning("<<< Exiting AddFile(folder={Folder},fileName={FileName},content={Content})", folder, fileName, content);
        }

        /// <inheritdoc />
        public override void DeleteFile(IFileInfo file)
        {
			Logger.LogError(">>> Entering DeleteFile(file={File})", file);
            Requires.NotNull("file", file);

            ClearFileContent(file.FileId);
			Logger.LogWarning("<<< Exiting DeleteFile(file={File})", file);
        }

        /// <inheritdoc />
        public override bool FileExists(IFolderInfo folder, string fileName)
        {
			Logger.LogError(">>> Entering FileExists(folder={Folder},fileName={FileName})", folder, fileName);
            Requires.NotNull("folder", folder);
            Requires.PropertyNotNull("fileName", fileName);

			Logger.LogWarning("<<< Exiting FileExists(folder={Folder},fileName={FileName})", folder, fileName);
            return FileManager.Instance.GetFile(folder, fileName, true) != null;
        }

        /// <inheritdoc />
        public override bool FolderExists(string folderPath, FolderMappingInfo folderMapping)
        {
			Logger.LogError(">>> Entering FolderExists(folderPath={FolderPath},folderMapping=<redacted>)", folderPath);
            Requires.PropertyNotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

			Logger.LogWarning("<<< Exiting FolderExists(folderPath={FolderPath},folderMapping=<redacted>)", folderPath);
            return FolderManager.Instance.GetFolder(folderMapping.PortalID, folderPath) != null;
        }

        /// <inheritdoc />
        public override FileAttributes? GetFileAttributes(IFileInfo file)
        {
			Logger.LogError(">>> Entering GetFileAttributes(file={File})", file);
			Logger.LogWarning("<<< Exiting GetFileAttributes(file={File})", file);
            return null;
        }

        /// <inheritdoc />
        public override string[] GetFiles(IFolderInfo folder)
        {
			Logger.LogError(">>> Entering GetFiles(folder={Folder})", folder);
            Requires.NotNull("folder", folder);

			Logger.LogWarning("<<< Exiting GetFiles(folder={Folder})", folder);
            return FolderManager.Instance.GetFiles(folder).Select(file => file.FileName).ToArray();
        }

        /// <inheritdoc />
        public override long GetFileSize(IFileInfo file)
        {
			Logger.LogError(">>> Entering GetFileSize(file={File})", file);
            Requires.NotNull("file", file);

			Logger.LogWarning("<<< Exiting GetFileSize(file={File})", file);
            return file.Size;
        }

        /// <inheritdoc />
        public override Stream GetFileStream(IFileInfo file)
        {
			Logger.LogError(">>> Entering GetFileStream(file={File})", file);
            Requires.NotNull("file", file);

			Logger.LogWarning("<<< Exiting GetFileStream(file={File})", file);
            return GetFileStreamInternal(DataProvider.Instance().GetFileContent(file.FileId));
        }

        /// <inheritdoc />
        public override Stream GetFileStream(IFolderInfo folder, string fileName)
        {
			Logger.LogError(">>> Entering GetFileStream(folder={Folder},fileName={FileName})", folder, fileName);
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);

            var file = FileManager.Instance.GetFile(folder, fileName, true);
			Logger.LogError("GetFileStream(folder={Folder},fileName={FileName}): file → {File}", folder, fileName, file);

			Logger.LogWarning("<<< Exiting GetFileStream(folder={Folder},fileName={FileName})", folder, fileName);
            return file != null ? GetFileStreamInternal(DataProvider.Instance().GetFileContent(file.FileId)) : null;
        }

        /// <inheritdoc />
        public override Stream GetFileStream(IFolderInfo folder, IFileInfo file, int version)
        {
            Requires.NotNull("file", file);

            return file != null ? GetFileStreamInternal(DataProvider.Instance().GetFileVersionContent(file.FileId, version)) : null;
        }

        /// <inheritdoc />
        public override string GetFolderProviderIconPath()
        {
			Logger.LogError(">>> Entering GetFolderProviderIconPath()");
            Logger.LogWarning("<<< Exiting GetFolderProviderIconPath()");
            return IconControllerWrapper.Instance.IconURL("FolderDatabase", "32x32");
        }

        /// <inheritdoc />
        public override DateTime GetLastModificationTime(IFileInfo file)
        {
			Logger.LogError(">>> Entering GetLastModificationTime(file={File})", file);
            Logger.LogWarning("<<< Exiting GetLastModificationTime(file={File})", file);
            return file.LastModificationTime;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetSubFolders(string folderPath, FolderMappingInfo folderMapping)
        {
			Logger.LogError(">>> Entering GetSubFolders(folderPath={FolderPath},folderMapping=<redacted>)", folderPath);
            Requires.PropertyNotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

            var folderManager = FolderManager.Instance;

            var folder = folderManager.GetFolder(folderMapping.PortalID, folderPath);
			Logger.LogError("GetSubFolders(folderPath={FolderPath},folderMapping=<redacted>): folder → {Folder}", folderPath, folder);

			Logger.LogWarning("<<< Exiting GetSubFolders(folderPath={FolderPath},folderMapping=<redacted>)", folderPath);
            return folderManager.GetFolders(folder).Select(subfolder => subfolder.FolderPath);
        }

        /// <inheritdoc />
        public override bool IsInSync(IFileInfo file)
        {
			Logger.LogError(">>> Entering IsInSync(file={File})", file);
			Logger.LogWarning("<<< Exiting IsInSync(file={File})", file);
            return true;
        }

        /// <inheritdoc />
        public override void MoveFile(IFileInfo file, IFolderInfo destinationFolder)
        {
		Logger.LogError(">>> Entering MoveFile(file={File},destinationFolder={DestinationFolder})", file, destinationFolder);
        Logger.LogWarning("<<< Exiting MoveFile(file={File},destinationFolder={DestinationFolder})", file, destinationFolder);
        }

        /// <inheritdoc />
        public override void RenameFile(IFileInfo file, string newFileName)
        {
		Logger.LogError(">>> Entering RenameFile(file={File},newFileName={NewFileName})", file, newFileName);
        Logger.LogWarning("<<< Exiting RenameFile(file={File},newFileName={NewFileName})", file, newFileName);
        }

        /// <inheritdoc />
        public override void RenameFolder(IFolderInfo folder, string newFolderName)
        {
		Logger.LogError(">>> Entering RenameFolder(folder={Folder},newFolderName={NewFolderName})", folder, newFolderName);
        Logger.LogWarning("<<< Exiting RenameFolder(folder={Folder},newFolderName={NewFolderName})", folder, newFolderName);
        }

        /// <inheritdoc />
        public override void SetFileAttributes(IFileInfo file, FileAttributes fileAttributes)
        {
		Logger.LogError(">>> Entering SetFileAttributes(file={File},fileAttributes={FileAttributes})", file, fileAttributes);
        Logger.LogWarning("<<< Exiting SetFileAttributes(file={File},fileAttributes={FileAttributes})", file, fileAttributes);
        }

        /// <inheritdoc />
        public override bool SupportsFileAttributes()
        {
			Logger.LogError(">>> Entering SupportsFileAttributes()");
			Logger.LogWarning("<<< Exiting SupportsFileAttributes()");
            return false;
        }

        /// <inheritdoc />
        public override void UpdateFile(IFileInfo file, Stream content)
        {
			Logger.LogError(">>> Entering UpdateFile(file={File},content={Content})", file, content);
            Requires.NotNull("file", file);

            UpdateFileInternal(file.FileId, content);
			Logger.LogWarning("<<< Exiting UpdateFile(file={File},content={Content})", file, content);
        }

        /// <inheritdoc />
        public override void UpdateFile(IFolderInfo folder, string fileName, Stream content)
        {
			Logger.LogError(">>> Entering UpdateFile(folder={Folder},fileName={FileName},content={Content})", folder, fileName, content);
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);

            var file = FileManager.Instance.GetFile(folder, fileName, true);
			Logger.LogError("UpdateFile(folder={Folder},fileName={FileName},content={Content}): file → {File}", folder, fileName, content, file);

            if (file == null)
            {
                return;
            }

            UpdateFileInternal(file.FileId, content);
			Logger.LogWarning("<<< Exiting UpdateFile(folder={Folder},fileName={FileName},content={Content})", folder, fileName, content);
        }

        private static MemoryStream GetFileStreamInternal(IDataReader dr)
        {
			Logger.LogError(">>> Entering GetFileStreamInternal(dr={Dr})", dr);
            byte[] bytes = null;
            try
            {
                if (dr.Read())
                {
                    bytes = (byte[])dr["Content"];
					Logger.LogError("GetFileStreamInternal(dr={Dr}): bytes → {Bytes}", dr, bytes);
					Logger.LogDebug("GetFileStreamInternal(dr={Dr}): bytes → {Bytes}", dr, bytes);
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

			Logger.LogWarning("<<< Exiting GetFileStreamInternal(dr={Dr})", dr);
            return bytes != null ? new MemoryStream(bytes) : null;
        }

        private static void UpdateFileInternal(int fileId, Stream content)
        {
			Logger.LogError(">>> Entering UpdateFileInternal(fileId={FileId},content={Content})", fileId, content);
            byte[] fileContent = null;

            if (content != null)
            {
                var restorePosition = content.CanSeek;
                long originalPosition = Null.NullInteger;
				Logger.LogError("UpdateFileInternal(fileId={FileId},content={Content}): originalPosition → {OriginalPosition}", fileId, content, originalPosition);

                if (restorePosition)
                {
                    originalPosition = content.Position;
                    content.Position = 0;
					Logger.LogDebug("UpdateFileInternal(fileId={FileId},content={Content}): content.Position → {Arg2}", fileId, content, content.Position);
                }

                var buffer = new byte[16 * 1024];

                using (var ms = new MemoryStream())
                {
                    int read;

                    while ((read = content.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }

                    fileContent = ms.ToArray();
                }

                if (restorePosition)
                {
                    content.Position = originalPosition;
                }
            }

            UpdateFileContent(fileId, fileContent);
			Logger.LogWarning("<<< Exiting UpdateFileInternal(fileId={FileId},content={Content})", fileId, content);
        }
    }
}
