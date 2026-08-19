// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Services.FileSystem

// ReSharper restore CheckNamespace
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.FileSystem.Internal;

    public class StandardFolderProvider : FolderProvider
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(StandardFolderProvider));

        private static readonly char[] InvalidFileUrlChars = new char[] { '%', ';', '?', ':', '@', '&', '=', '+', '$', ',' };

        /// <summary>Gets a value indicating whether the provider requires network connectivity to do its tasks.</summary>
        public override bool RequiresNetworkConnectivity
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override bool SupportsMoveFile
        {
            get
            {
                return true;
            }
        }

        /// <inheritdoc />
        public override bool SupportsMoveFolder
        {
            get { return true; }
        }

        /// <inheritdoc />
        public override void CopyFile(string folderPath, string fileName, string newFolderPath, FolderMappingInfo folderMapping)
        {
			Logger.LogDebug(">>> Entering CopyFile(folderPath={FolderPath},fileName={FileName},newFolderPath={NewFolderPath},folderMapping=<redacted>)", folderPath, fileName, newFolderPath);
            Requires.PropertyNotNull("folderPath", folderPath);
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.PropertyNotNull("newFolderPath", newFolderPath);
            Requires.NotNull("folderMapping", folderMapping);

            if (folderPath == newFolderPath)
            {
                return;
            }

            var filePath = this.GetActualPath(folderMapping, folderPath, fileName);
			Logger.LogDebug("CopyFile(folderPath={FolderPath},fileName={FileName},newFolderPath={NewFolderPath},folderMapping=<redacted>): filePath → {FilePath}", folderPath, fileName, newFolderPath, filePath);
            var newFilePath = this.GetActualPath(folderMapping, newFolderPath, fileName);
			Logger.LogDebug("CopyFile(folderPath={FolderPath},fileName={FileName},newFolderPath={NewFolderPath},folderMapping=<redacted>): newFilePath → {NewFilePath}", folderPath, fileName, newFolderPath, newFilePath);

            if (FileWrapper.Instance.Exists(filePath))
            {
                FileWrapper.Instance.Copy(filePath, newFilePath, true);
            }
			Logger.LogDebug("<<< Exiting CopyFile(folderPath={FolderPath},fileName={FileName},newFolderPath={NewFolderPath},folderMapping=<redacted>)", folderPath, fileName, newFolderPath);
        }

        /// <inheritdoc />
        public override void AddFile(IFolderInfo folder, string fileName, Stream content)
        {
			Logger.LogDebug(">>> Entering AddFile(folder={Folder},fileName={FileName},content={Content})", folder, fileName, content);
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.NotNull("content", content);

            this.UpdateFile(folder, fileName, content);
			Logger.LogDebug("<<< Exiting AddFile(folder={Folder},fileName={FileName},content={Content})", folder, fileName, content);
        }

        /// <inheritdoc />
        public override void AddFolder(string folderPath, FolderMappingInfo folderMapping)
        {
		Logger.LogDebug(">>> Entering AddFolder(folderPath={FolderPath},folderMapping=<redacted>)", folderPath);
		Logger.LogDebug("<<< Exiting AddFolder(folderPath={FolderPath},folderMapping=<redacted>)", folderPath);
        }

        /// <inheritdoc />
        public override void DeleteFile(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering DeleteFile(file={File})", file);
            Requires.NotNull("file", file);

            var path = this.GetActualPath(file);
			Logger.LogDebug("DeleteFile(file={File}): path → {Path}", file, path);

            if (FileWrapper.Instance.Exists(path))
            {
                FileWrapper.Instance.SetAttributes(path, FileAttributes.Normal);
                FileWrapper.Instance.Delete(path);
            }
			Logger.LogDebug("<<< Exiting DeleteFile(file={File})", file);
        }

        /// <inheritdoc />
        public override void DeleteFolder(IFolderInfo folder)
        {
		Logger.LogDebug(">>> Entering DeleteFolder(folder={Folder})", folder);
		Logger.LogDebug("<<< Exiting DeleteFolder(folder={Folder})", folder);
        }

        /// <inheritdoc />
        public override bool FileExists(IFolderInfo folder, string fileName)
        {
			Logger.LogDebug(">>> Entering FileExists(folder={Folder},fileName={FileName})", folder, fileName);
            Requires.NotNull("folder", folder);
            Requires.PropertyNotNull("fileName", fileName);

			Logger.LogDebug("<<< Exiting FileExists(folder={Folder},fileName={FileName})", folder, fileName);
            return FileWrapper.Instance.Exists(this.GetActualPath(folder, fileName));
        }

        /// <inheritdoc />
        public override bool FolderExists(string folderPath, FolderMappingInfo folderMapping)
        {
			Logger.LogDebug(">>> Entering FolderExists(folderPath={FolderPath},folderMapping=<redacted>)", folderPath);
            Requires.PropertyNotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

			Logger.LogDebug("<<< Exiting FolderExists(folderPath={FolderPath},folderMapping=<redacted>)", folderPath);
            return DirectoryWrapper.Instance.Exists(this.GetActualPath(folderMapping, folderPath));
        }

        /// <inheritdoc />
        public override FileAttributes? GetFileAttributes(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering GetFileAttributes(file={File})", file);
            Requires.NotNull("file", file);

            FileAttributes? fileAttributes = null;

            try
            {
                fileAttributes = FileWrapper.Instance.GetAttributes(this.GetActualPath(file));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

			Logger.LogDebug("<<< Exiting GetFileAttributes(file={File})", file);
            return fileAttributes;
        }

        /// <inheritdoc />
        public override string[] GetFiles(IFolderInfo folder)
        {
			Logger.LogDebug(">>> Entering GetFiles(folder={Folder})", folder);
            Requires.NotNull("folder", folder);

            var fileNames = DirectoryWrapper.Instance.GetFiles(this.GetActualPath(folder));
			Logger.LogDebug("GetFiles(folder={Folder}): fileNames → {FileNames}", folder, fileNames);

            for (var i = 0; i < fileNames.Length; i++)
            {
                fileNames[i] = Path.GetFileName(fileNames[i]);
            }

			Logger.LogDebug("<<< Exiting GetFiles(folder={Folder})", folder);
            return fileNames;
        }

        /// <inheritdoc />
        public override long GetFileSize(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering GetFileSize(file={File})", file);
            Requires.NotNull("file", file);

            var physicalFile = new System.IO.FileInfo(this.GetActualPath(file));
			Logger.LogDebug("GetFileSize(file={File}): physicalFile → {PhysicalFile}", file, physicalFile);

			Logger.LogDebug("<<< Exiting GetFileSize(file={File})", file);
            return physicalFile.Length;
        }

        /// <inheritdoc />
        public override Stream GetFileStream(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering GetFileStream(file={File})", file);
            Requires.NotNull("file", file);

			Logger.LogDebug("<<< Exiting GetFileStream(file={File})", file);
			Logger.LogDebug("<<< Exiting GetFileStream(folder={Folder},fileName={FileName})", folder, fileName);
            return this.GetFileStreamInternal(this.GetActualPath(file));
        }

        /// <inheritdoc />
        public override Stream GetFileStream(IFolderInfo folder, string fileName)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);
            return this.GetFileStreamInternal(this.GetActualPath(folder, fileName));
        }

        /// <inheritdoc />
        public override string GetFileUrl(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering GetFileUrl(file={File})", file);
            Requires.NotNull("file", file);

            var portalSettings = file.PortalId == PortalSettings.Current?.PortalId ?
                PortalSettings.Current :
                this.GetPortalSettings(file.PortalId);

            var rootFolder = file.PortalId == Null.NullInteger ? Globals.HostPath : portalSettings.HomeDirectory;

            var fullPath = rootFolder + file.Folder + file.FileName;

            // check if a filename has a character that is not valid for urls
            if (fullPath.IndexOfAny(InvalidFileUrlChars) >= 0)
            {
                return Globals.LinkClick(
                    $"fileid={file.FileId}",
                    Null.NullInteger,
                    Null.NullInteger,
                    true,
                    false,
                    portalSettings.PortalId,
                    portalSettings.EnableUrlLanguage,
                    portalSettings.GUID.ToString());
            }

            // Does site management want the cache-buster parameter?
            if (portalSettings.AddCachebusterToResourceUris)
            {
                var cacheBusterToken = UrlUtils.EncryptParameter(
                    file.LastModificationTime.GetHashCode().ToString(CultureInfo.InvariantCulture),
                    portalSettings.GUID.ToString());

			Logger.LogDebug("<<< Exiting GetFileUrl(file={File})", file);
                return TestableGlobals.Instance.ResolveUrl(fullPath + "?ver=" + cacheBusterToken);
            }

            return TestableGlobals.Instance.ResolveUrl(fullPath);
        }

        /// <inheritdoc />
        public override string GetFolderProviderIconPath()
        {
			Logger.LogDebug(">>> Entering GetFolderProviderIconPath()");
			Logger.LogDebug("<<< Exiting GetFolderProviderIconPath()");
            return IconControllerWrapper.Instance.IconURL("FolderStandard", "32x32");
        }

        /// <inheritdoc />
        public override DateTime GetLastModificationTime(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering GetLastModificationTime(file={File})", file);
            Requires.NotNull("file", file);

            var lastModificationTime = Null.NullDate;
			Logger.LogDebug("GetLastModificationTime(file={File}): lastModificationTime → {LastModificationTime}", file, lastModificationTime);

            try
            {
                lastModificationTime = FileWrapper.Instance.GetLastWriteTime(this.GetActualPath(file));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

			Logger.LogDebug("<<< Exiting GetLastModificationTime(file={File})", file);
            return lastModificationTime;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetSubFolders(string folderPath, FolderMappingInfo folderMapping)
        {
			Logger.LogDebug(">>> Entering GetSubFolders(folderPath={FolderPath},folderMapping=<redacted>)", folderPath);
            Requires.PropertyNotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

			Logger.LogDebug("<<< Exiting GetSubFolders(folderPath={FolderPath},folderMapping=<redacted>)", folderPath);
            return DirectoryWrapper.Instance.GetDirectories(this.GetActualPath(folderMapping, folderPath))
                .Select(directory => this.GetRelativePath(folderMapping, directory));
        }

        /// <inheritdoc />
        public override bool IsInSync(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering IsInSync(file={File})", file);
            Requires.NotNull("file", file);

			Logger.LogDebug("<<< Exiting IsInSync(file={File})", file);
            return Convert.ToInt32((file.LastModificationTime - this.GetLastModificationTime(file)).TotalSeconds) == 0;
        }

        /// <inheritdoc />
        public override void MoveFile(IFileInfo file, IFolderInfo destinationFolder)
        {
			Logger.LogDebug(">>> Entering MoveFile(file={File},destinationFolder={DestinationFolder})", file, destinationFolder);
            Requires.NotNull("file", file);
            Requires.NotNull("destinationFolder", destinationFolder);

            if (file.FolderId != destinationFolder.FolderID)
            {
                string oldName = this.GetActualPath(file);
                string newName = this.GetActualPath(destinationFolder, file.FileName);
                FileWrapper.Instance.Move(oldName, newName);
            }
			Logger.LogDebug("<<< Exiting MoveFile(file={File},destinationFolder={DestinationFolder})", file, destinationFolder);
        }

        /// <inheritdoc />
        public override void MoveFolder(string folderPath, string newFolderPath, FolderMappingInfo folderMapping)
        {
			Logger.LogDebug(">>> Entering MoveFolder(folderPath={FolderPath},newFolderPath={NewFolderPath},folderMapping=<redacted>)", folderPath, newFolderPath);
            // The folder has already been moved in filesystem
			Logger.LogDebug("<<< Exiting MoveFolder(folderPath={FolderPath},newFolderPath={NewFolderPath},folderMapping=<redacted>)", folderPath, newFolderPath);
        }

        /// <inheritdoc />
        public override void RenameFile(IFileInfo file, string newFileName)
        {
			Logger.LogDebug(">>> Entering RenameFile(file={File},newFileName={NewFileName})", file, newFileName);
            Requires.NotNull("file", file);
            Requires.NotNullOrEmpty("newFileName", newFileName);

            if (file.FileName != newFileName)
            {
                IFolderInfo folder = FolderManager.Instance.GetFolder(file.FolderId);
				Logger.LogDebug("RenameFile(file={File},newFileName={NewFileName}): folder → {Folder}", file, newFileName, folder);
                string oldName = this.GetActualPath(file);
                string newName = this.GetActualPath(folder, newFileName);
                FileWrapper.Instance.Move(oldName, newName);
            }
			Logger.LogDebug("<<< Exiting RenameFile(file={File},newFileName={NewFileName})", file, newFileName);
        }

        /// <inheritdoc />
        public override void RenameFolder(IFolderInfo folder, string newFolderName)
        {
			Logger.LogDebug(">>> Entering RenameFolder(folder={Folder},newFolderName={NewFolderName})", folder, newFolderName);
            // The folder has already been moved in filesystem
			Logger.LogDebug("<<< Exiting RenameFolder(folder={Folder},newFolderName={NewFolderName})", folder, newFolderName);
        }

        /// <inheritdoc />
        public override void SetFileAttributes(IFileInfo file, FileAttributes fileAttributes)
        {
			Logger.LogDebug(">>> Entering SetFileAttributes(file={File},fileAttributes={FileAttributes})", file, fileAttributes);
            Requires.NotNull("file", file);

            FileWrapper.Instance.SetAttributes(this.GetActualPath(file), fileAttributes);
			Logger.LogDebug("<<< Exiting SetFileAttributes(file={File},fileAttributes={FileAttributes})", file, fileAttributes);
        }

        /// <inheritdoc />
        public override bool SupportsFileAttributes()
        {
			Logger.LogDebug(">>> Entering SupportsFileAttributes()");
			Logger.LogDebug("<<< Exiting SupportsFileAttributes()");
            return true;
        }

        /// <inheritdoc />
        public override void UpdateFile(IFileInfo file, Stream content)
        {
			Logger.LogDebug(">>> Entering UpdateFile(file={File},content={Content})", file, content);
            Requires.NotNull("file", file);
            Requires.NotNull("content", content);

            this.UpdateFile(FolderManager.Instance.GetFolder(file.FolderId), file.FileName, content);
			Logger.LogDebug("<<< Exiting UpdateFile(file={File},content={Content})", file, content);
        }

        /// <inheritdoc />
        public override void UpdateFile(IFolderInfo folder, string fileName, Stream content)
        {
			Logger.LogDebug(">>> Entering UpdateFile(folder={Folder},fileName={FileName},content={Content})", folder, fileName, content);
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.NotNull("content", content);

            var arrData = new byte[2048];
            var actualPath = this.GetActualPath(folder, fileName);
			Logger.LogDebug("UpdateFile(folder={Folder},fileName={FileName},content={Content}): actualPath → {ActualPath}", folder, fileName, content, actualPath);

            if (FileWrapper.Instance.Exists(actualPath))
            {
                FileWrapper.Instance.SetAttributes(actualPath, FileAttributes.Normal);
                FileWrapper.Instance.Delete(actualPath);
            }

            using (var outStream = FileWrapper.Instance.Create(actualPath))
            {
                var originalPosition = content.Position;
                content.Position = 0;
				Logger.LogDebug("UpdateFile(folder={Folder},fileName={FileName},content={Content}): content.Position → {Arg3}", folder, fileName, content, content.Position);

                try
                {
                    var intLength = content.Read(arrData, 0, arrData.Length);
					Logger.LogDebug("UpdateFile(folder={Folder},fileName={FileName},content={Content}): intLength → {IntLength}", folder, fileName, content, intLength);

                    while (intLength > 0)
                    {
                        outStream.Write(arrData, 0, intLength);
                        intLength = content.Read(arrData, 0, arrData.Length);
                    }
                }
                finally
                {
                    content.Position = originalPosition;
                }
            }
			Logger.LogDebug("<<< Exiting UpdateFile(folder={Folder},fileName={FileName},content={Content})", folder, fileName, content);
        }

        internal virtual string GetHash(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering GetHash(file={File})", file);
            var fileManager = new FileManager();
			Logger.LogDebug("GetHash(file={File}): fileManager → {FileManager}", file, fileManager);
			Logger.LogDebug("<<< Exiting GetHash(file={File})", file);
            return fileManager.GetHash(file);
        }

        internal virtual PortalSettings GetPortalSettings(int portalId)
        {
			Logger.LogDebug(">>> Entering GetPortalSettings(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetPortalSettings(portalId={PortalId})", portalId);
            return new PortalSettings(portalId);
        }

        /// <summary>Get actual path to a file.</summary>
        /// <param name="folderMapping">Folder Mapping of the folder.</param>
        /// <param name="folderPath">Folder Path where the file is contained.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>A windows supported path to the file.</returns>
        protected virtual string GetActualPath(FolderMappingInfo folderMapping, string folderPath, string fileName)
        {
			Logger.LogDebug(">>> Entering GetActualPath(folderMapping=<redacted>,folderPath={FolderPath},fileName={FileName})", folderPath, fileName);
            var actualFolderPath = this.GetActualPath(folderMapping, folderPath);
			Logger.LogDebug("GetActualPath(folderMapping=<redacted>,folderPath={FolderPath},fileName={FileName}): actualFolderPath → {ActualFolderPath}", folderPath, fileName, actualFolderPath);
			Logger.LogDebug("<<< Exiting GetActualPath(folderMapping=<redacted>,folderPath={FolderPath},fileName={FileName})", folderPath, fileName);
			Logger.LogDebug("<<< Exiting GetActualPath(folderMapping=<redacted>,folderPath={FolderPath})", folderPath);
			Logger.LogDebug("<<< Exiting GetActualPath(file={File})", file);
			Logger.LogDebug("<<< Exiting GetActualPath(folder={Folder},fileName={FileName})", folder, fileName);
			Logger.LogDebug("<<< Exiting GetActualPath(folder={Folder})", folder);
            return Path.Combine(actualFolderPath, fileName);
        }

        /// <summary>Get actual path to an IFileInfo.</summary>
        /// <param name="file">The file.</param>
        /// <returns>A windows supported path to the file.</returns>
        protected virtual string GetActualPath(IFileInfo file)
        {
            return file.PhysicalPath;
        }

        /// <summary>Get actual path to a file in specified folder.</summary>
        /// <param name="folder">The folder that contains the file.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>A windows supported path to the file.</returns>
        protected virtual string GetActualPath(IFolderInfo folder, string fileName)
        {
            return Path.Combine(folder.PhysicalPath, fileName);
        }

        /// <summary>Get actual path to a folder in the specified folder mapping.</summary>
        /// <param name="folderMapping">The folder mapping.</param>
        /// <param name="folderPath">The folder path.</param>
        /// <returns>A windows supported path to the folder.</returns>
        protected virtual string GetActualPath(FolderMappingInfo folderMapping, string folderPath)
        {
            return PathUtils.Instance.GetPhysicalPath(folderMapping.PortalID, folderPath);
        }

        /// <summary>Get actual path to a folder.</summary>
        /// <param name="folder">The folder.</param>
        /// <returns>A windows supported path to the folder.</returns>
        protected virtual string GetActualPath(IFolderInfo folder)
        {
            return folder.PhysicalPath;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected Stream GetFileStreamInternal(string filePath)
        {
			Logger.LogDebug(">>> Entering GetFileStreamInternal(filePath={FilePath})", filePath);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            Stream stream = null;

			Logger.LogDebug("GetFileStreamInternal(filePath={FilePath}): stream → {Stream}", filePath, stream);
            try
            {
                stream = FileWrapper.Instance.OpenRead(filePath);
            }
            catch (IOException iex)
            {
                Logger.Warn(iex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

			Logger.LogInformation("GetFileStreamInternal(filePath)={FilePath}: external service call took {Arg1} ms", filePath, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting GetFileStreamInternal(filePath={FilePath})", filePath);
            return stream;
        }

        /// <summary>Get the path relative to the root of the FolderMapping.</summary>
        /// <param name="folderMapping">Path is relative to this.</param>
        /// <param name="path">The path.</param>
        /// <returns>A relative path.</returns>
        protected virtual string GetRelativePath(FolderMappingInfo folderMapping, string path)
        {
			Logger.LogDebug(">>> Entering GetRelativePath(folderMapping=<redacted>,path={Path})", path);
			Logger.LogDebug("<<< Exiting GetRelativePath(folderMapping=<redacted>,path={Path})", path);
            return PathUtils.Instance.GetRelativePath(folderMapping.PortalID, path);
        }
    }
}
