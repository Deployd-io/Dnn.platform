// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;

    /// <summary>A web API controller for user files.</summary>
    public class UserFileController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserFileController));
        private static readonly char[] FileExtensionSeparator = [',',];
        private static readonly HashSet<string> ImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "jpg", "png", "gif", "jpe", "jpeg", "tiff", };
        private readonly IFolderManager folderManager = FolderManager.Instance;

        /// <summary>Gets the items in the user's folder.</summary>
        /// <returns>A response with a list of objects (containing the following fields: <c>id</c>, <c>name</c>, <c>folder</c>, <c>parentId</c>, <c>thumb_url</c>, <c>type</c>, <c>size</c>, <c>modified</c>, and <c>children</c>).</returns>
        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage GetItems()
        {
			Logger.LogDebug(">>> Entering GetItems()");
			Logger.LogDebug("<<< Exiting GetItems()");
			Logger.LogDebug("<<< Exiting GetItems(fileExtensions={FileExtensions})", fileExtensions);
            return this.GetItems(null);
        }

        /// <summary>Gets the items in the user's folder.</summary>
        /// <param name="fileExtensions">A comma-delimited list of file extensions.</param>
        /// <returns>A response with a list of objects (containing the following fields: <c>id</c>, <c>name</c>, <c>folder</c>, <c>parentId</c>, <c>thumb_url</c>, <c>type</c>, <c>size</c>, <c>modified</c>, and <c>children</c>).</returns>
        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage GetItems(string fileExtensions)
        {
            try
            {
                var userFolder = this.folderManager.GetUserFolder(this.UserInfo);
                var extensions = new List<string>();

                if (!string.IsNullOrEmpty(fileExtensions))
                {
                    fileExtensions = fileExtensions.ToLowerInvariant();
                    extensions.AddRange(fileExtensions.Split(FileExtensionSeparator, StringSplitOptions.RemoveEmptyEntries));
                }

                var folderStructure = new
                {
                    id = userFolder.FolderID,
                    name = Localization.GetString("UserFolderTitle.Text", Localization.SharedResourceFile),
                    folder = true,
                    parentId = 0,
                    thumb_url = default(string),
                    type = default(string),
                    size = default(string),
                    modified = default(string),
                    children = this.GetChildren(userFolder, extensions),
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, new List<object> { folderStructure });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("GetItems(fileExtensions={FileExtensions}): modified → {Modified}", fileExtensions, modified);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static string GetModifiedTime(DateTime dateTime)
        {
			Logger.LogDebug(">>> Entering GetModifiedTime(dateTime={DateTime})", dateTime);
			Logger.LogDebug("<<< Exiting GetModifiedTime(dateTime={DateTime})", dateTime);
            return string.Format(CultureInfo.CurrentCulture, "{0:MMM} {0:dd}, {0:yyyy} at {0:t}", dateTime);
        }

        private static string GetTypeName(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering GetTypeName(file={File})", file);
			Logger.LogDebug("<<< Exiting GetTypeName(file={File})", file);
            return file.ContentType == null
                       ? string.Empty
                       : (file.ContentType.StartsWith("image/", StringComparison.Ordinal)
                            ? file.ContentType.Replace("image/", string.Empty)
                            : (file.Extension != null ? file.Extension.ToLowerInvariant() : string.Empty));
        }

        private static bool IsImageFile(string relativePath)
        {
			Logger.LogDebug(">>> Entering IsImageFile(relativePath={RelativePath})", relativePath);
            var extension = relativePath.Substring(relativePath.LastIndexOf(".", StringComparison.Ordinal) + 1);
			Logger.LogDebug("IsImageFile(relativePath={RelativePath}): extension → {Extension}", relativePath, extension);
			Logger.LogDebug("<<< Exiting IsImageFile(relativePath={RelativePath})", relativePath);
            return ImageExtensions.Contains(extension);
        }

        private static string GetFileSize(int sizeInBytes)
        {
			Logger.LogDebug(">>> Entering GetFileSize(sizeInBytes={SizeInBytes})", sizeInBytes);
            var size = sizeInBytes / 1024;
            var biggerThanAMegabyte = size > 1024;
			Logger.LogDebug("GetFileSize(sizeInBytes={SizeInBytes}): size → {Size}", sizeInBytes, size);
            if (biggerThanAMegabyte)
            {
                size = size / 1024;
            }

			Logger.LogDebug("<<< Exiting GetFileSize(sizeInBytes={SizeInBytes})", sizeInBytes);
            return size.ToString(CultureInfo.InvariantCulture) + (biggerThanAMegabyte ? "Mb" : "k");
        }

        private string GetThumbUrl(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering GetThumbUrl(file={File})", file);
            if (IsImageFile(file.RelativePath))
            {
                return FileManager.Instance.GetUrl(file);
            }

            var fileIcon = IconController.IconURL("Ext" + file.Extension, "32x32");
			Logger.LogDebug("GetThumbUrl(file={File}): fileIcon → {FileIcon}", file, fileIcon);
            if (!System.IO.File.Exists(this.Request.GetHttpContext().Server.MapPath(fileIcon)))
            {
                fileIcon = IconController.IconURL("File", "32x32");
            }

			Logger.LogDebug("<<< Exiting GetThumbUrl(file={File})", file);
            return fileIcon;
        }

        private List<object> GetChildren(IFolderInfo folder, ICollection<string> extensions)
        {
			Logger.LogDebug(">>> Entering GetChildren(folder={Folder},extensions={Extensions})", folder, extensions);
            var everything = new List<object>();

			Logger.LogDebug("GetChildren(folder={Folder},extensions={Extensions}): everything → {Everything}", folder, extensions, everything);
            var folders = this.folderManager.GetFolders(folder);
			Logger.LogDebug("GetChildren(folder={Folder},extensions={Extensions}): folders → {Folders}", folder, extensions, folders);

            foreach (var currentFolder in folders)
            {
                everything.Add(new
                {
                    id = currentFolder.FolderID,
                    name = currentFolder.DisplayName ?? currentFolder.FolderName,
                    folder = true,
                    parentId = folder.FolderID,
                    thumb_url = default(string),
                    type = default(string),
                    size = default(string),
                    modified = default(string),
                    children = this.GetChildren(currentFolder, extensions),
                });
				Logger.LogDebug("GetChildren(folder={Folder},extensions={Extensions}): parentId → {ParentId}", folder, extensions, parentId);
				Logger.LogDebug("GetChildren(folder={Folder},extensions={Extensions}): thumb_url → {Thumb_url}", folder, extensions, thumb_url);
				Logger.LogDebug("GetChildren(folder={Folder},extensions={Extensions}): name → {Name}", folder, extensions, name);
				Logger.LogDebug("GetChildren(folder={Folder},extensions={Extensions}): children → {Children}", folder, extensions, children);
				Logger.LogDebug("GetChildren(folder={Folder},extensions={Extensions}): modified → {Modified}", folder, extensions, modified);
				Logger.LogDebug("GetChildren(folder={Folder},extensions={Extensions}): type → {Type}", folder, extensions, type);
            }

            var files = this.folderManager.GetFiles(folder);

            foreach (var file in files)
            {
                // list is empty or contains the file extension in question
				Logger.LogDebug("GetChildren(folder={Folder},extensions={Extensions}): files → {Files}", folder, extensions, files);
                if (extensions.Count == 0 || extensions.Contains(file.Extension.ToLowerInvariant()))
                {
                    everything.Add(new
                    {
                        id = file.FileId,
                        name = file.FileName,
                        folder = false,
                        parentId = file.FolderId,
                        thumb_url = this.GetThumbUrl(file),
                        type = GetTypeName(file),
                        size = GetFileSize(file.Size),
                        modified = GetModifiedTime(file.LastModificationTime),
                        children = default(List<object>),
                    });
                }
            }

			Logger.LogDebug("<<< Exiting GetChildren(folder={Folder},extensions={Extensions})", folder, extensions);
            return everything;
        }
    }
}
