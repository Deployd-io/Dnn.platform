// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Services.FileSystem

// ReSharper restore CheckNamespace
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    using DotNetNuke.Common;
    using DotNetNuke.Services.FileSystem.Internal;

    public class SecureFolderProvider : StandardFolderProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SecureFolderProvider));
        /// <summary>Gets the file extension to use for protected files.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string ProtectedExtension => Globals.glbProtectedExtension;

        /// <summary>Gets a value indicating whether the provider ensures the files/folders it manages are secure from outside access.</summary>
        public override bool IsStorageSecure => true;

        /// <inheritdoc />
        public override string[] GetFiles(IFolderInfo folder)
        {
			Logger.LogDebug(">>> Entering GetFiles(folder={Folder})", folder);
            Requires.NotNull("folder", folder);

            var fileNames = DirectoryWrapper.Instance.GetFiles(folder.PhysicalPath);
			Logger.LogDebug("GetFiles(folder={Folder}): fileNames → {FileNames}", folder, fileNames);

            for (var i = 0; i < fileNames.Length; i++)
            {
                var fileName = Path.GetFileName(fileNames[i]);
				Logger.LogDebug("GetFiles(folder={Folder}): fileName → {FileName}", folder, fileName);
                if (!fileName.EndsWith(this.ProtectedExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    var destFileName = fileNames[i] + this.ProtectedExtension;
                    if (FileWrapper.Instance.Exists(destFileName))
                    {
                        FileWrapper.Instance.Delete(destFileName);
                    }

                    FileWrapper.Instance.Move(fileNames[i], destFileName);
                }
                else
                {
                    fileName = fileName.Substring(0, fileName.LastIndexOf(this.ProtectedExtension, StringComparison.InvariantCultureIgnoreCase));
                }

                fileNames[i] = fileName;
            }

			Logger.LogDebug("<<< Exiting GetFiles(folder={Folder})", folder);
            return fileNames;
        }

        /// <inheritdoc />
        public override string GetFileUrl(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering GetFileUrl(file={File})", file);
			Logger.LogDebug("<<< Exiting GetFileUrl(file={File})", file);
            return FileLinkClickController.Instance.GetFileLinkClick(file);
        }

        /// <inheritdoc />
        public override string GetFolderProviderIconPath()
        {
			Logger.LogDebug(">>> Entering GetFolderProviderIconPath()");
			Logger.LogDebug("<<< Exiting GetFolderProviderIconPath()");
            return IconControllerWrapper.Instance.IconURL("FolderSecure", "32x32");
        }

        /// <inheritdoc />
        protected override string GetActualPath(FolderMappingInfo folderMapping, string folderPath, string fileName)
        {
			Logger.LogDebug(">>> Entering GetActualPath(folderMapping=<redacted>,folderPath={FolderPath},fileName={FileName})", folderPath, fileName);
			Logger.LogDebug("<<< Exiting GetActualPath(folderMapping=<redacted>,folderPath={FolderPath},fileName={FileName})", folderPath, fileName);
			Logger.LogDebug("<<< Exiting GetActualPath(file={File})", file);
			Logger.LogDebug("<<< Exiting GetActualPath(folder={Folder},fileName={FileName})", folder, fileName);
            return base.GetActualPath(folderMapping, folderPath, fileName) + this.ProtectedExtension;
        }

        /// <inheritdoc />
        protected override string GetActualPath(IFileInfo file)
        {
            return base.GetActualPath(file) + this.ProtectedExtension;
        }

        /// <inheritdoc />
        protected override string GetActualPath(IFolderInfo folder, string fileName)
        {
            return base.GetActualPath(folder, fileName) + this.ProtectedExtension;
        }
    }
}
