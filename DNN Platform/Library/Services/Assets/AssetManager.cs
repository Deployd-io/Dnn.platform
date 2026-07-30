// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Assets
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem;

    using Localization = DotNetNuke.Services.Localization.Localization;

    public class AssetManager : ComponentBase<IAssetManager, AssetManager>, IAssetManager
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string UserHasNoPermissionToBrowseFolderDefaultMessage = "The user has no permission to browse this folder";
        private const string FileNameInvalidDefaultMessage = "The specified name ({0}) is not valid";
        private const string FolderFileNameIsReservedDefaultMessage = "The name is reserved. Try a different name";
        private const string UserHasNoPermissionToEditFileDefaultMessage = "The user has no permission to edit this file";
        private const string UserHasNoPermissionToEditFolderDefaultMessage = "The user has no permission to edit this folder";
        private const string FolderAlreadyExistsDefultMessage = "Cannot create folder ({0}), folder already exists in this location";
        private const string UserHasNoPermissionToAddDefaultMessage = "The user has no permission to add folders on this location";
        private const string InvalidMappedPathDefaultMessage = "The Mapped Path is invalid";
        private const string FolderAlreadyExistsDefaultMessage = "Cannot create folder ({0}), folder already exists in this location";
        private const string FolderFileNameHasInvalidcharactersDefaultMessage = "The name contains invalid character(s). Please specify a name without {0}";
        private const string DefaultMessageDefaultMessage = "The folder does not exist";
        private static readonly Regex MappedPathRegex = new Regex(@"^(?!\s*[\\/]).*$", RegexOptions.Compiled);

        public static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string propertyName, bool asc)
        {
			Logger.LogDebug(">>> Entering ApplyOrder(source={Source},propertyName={PropertyName},asc={Asc})", source, propertyName, asc);
            var methodName = asc ? "OrderBy" : "OrderByDescending";
            var arg = Expression.Parameter(typeof(T), "x");
			Logger.LogDebug("ApplyOrder(source={Source},propertyName={PropertyName},asc={Asc}): methodName → {MethodName}", source, propertyName, asc, methodName);
			Logger.LogDebug("ApplyOrder(source={Source},propertyName={PropertyName},asc={Asc}): arg → {Arg}", source, propertyName, asc, arg);

            // Use reflection to mirror LINQ
            var property = typeof(T).GetProperty(propertyName);

			Logger.LogDebug("ApplyOrder(source={Source},propertyName={PropertyName},asc={Asc}): property → {Property}", source, propertyName, asc, property);
            // If property is undefined returns the original source
            if (property == null)
            {
                return (IOrderedQueryable<T>)source;
            }

            Expression expr = Expression.Property(arg, property);

			Logger.LogDebug("ApplyOrder(source={Source},propertyName={PropertyName},asc={Asc}): expr → {Expr}", source, propertyName, asc, expr);
            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), property.PropertyType);
            var lambda = Expression.Lambda(delegateType, expr, arg);
			Logger.LogDebug("ApplyOrder(source={Source},propertyName={PropertyName},asc={Asc}): delegateType → {DelegateType}", source, propertyName, asc, delegateType);
			Logger.LogDebug("ApplyOrder(source={Source},propertyName={PropertyName},asc={Asc}): lambda → {Lambda}", source, propertyName, asc, lambda);

            var result = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), property.PropertyType)
                    .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }

        public static bool HasPermission(IFolderInfo folder, string permissionKey)
        {
            var hasPermision = PortalSettings.Current.UserInfo.IsSuperUser;

            if (!hasPermision && folder != null)
            {
                hasPermision = FolderPermissionController.HasFolderPermission(folder.FolderPermissions, permissionKey);
            }

            return hasPermision;
        }

        /// <inheritdoc />
        public ContentPage GetFolderContent(int folderId, int startIndex, int numItems, string sortExpression = null, SubfolderFilter subfolderFilter = SubfolderFilter.IncludeSubfoldersFolderStructure)
        {
			Logger.LogDebug(">>> Entering GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter})", folderId, startIndex, numItems, sortExpression, subfolderFilter);
            var folder = GetFolderInfo(folderId);

			Logger.LogDebug("GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): folder → {Folder}", folderId, startIndex, numItems, sortExpression, subfolderFilter, folder);
            if (!FolderPermissionController.CanBrowseFolder((FolderInfo)folder))
            {
                throw new AssetManagerException(Localization.GetExceptionMessage("UserHasNoPermissionToBrowseFolder", UserHasNoPermissionToBrowseFolderDefaultMessage));
            }

            var sortProperties = SortProperties.Parse(sortExpression);

			Logger.LogDebug("GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): sortProperties → {SortProperties}", folderId, startIndex, numItems, sortExpression, subfolderFilter, sortProperties);
            List<IFolderInfo> folders;

            if (subfolderFilter != SubfolderFilter.IncludeSubfoldersFolderStructure)
            {
                folders = new List<IFolderInfo>();
				Logger.LogDebug("GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): folders → {Folders}", folderId, startIndex, numItems, sortExpression, subfolderFilter, folders);
            }
            else
            {
                folders = this.GetFolders(folder, sortProperties.Column == "ItemName" ? "FolderName" : sortProperties.Column, sortProperties.Ascending).ToList();
            }

            var recursive = subfolderFilter == SubfolderFilter.IncludeSubfoldersFilesOnly;
            var files = GetFiles(folder, sortProperties, startIndex, recursive).ToList();

			Logger.LogDebug("GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): files → {Files}", folderId, startIndex, numItems, sortExpression, subfolderFilter, files);
            IEnumerable<object> content;
            if (startIndex + numItems <= folders.Count)
            {
                content = folders.Skip(startIndex).Take(numItems);
				Logger.LogDebug("GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): content → {Content}", folderId, startIndex, numItems, sortExpression, subfolderFilter, content);
            }
            else if (startIndex >= folders.Count)
            {
                content = files.Skip(startIndex - folders.Count).Take(numItems);
            }
            else
            {
                var numFiles = numItems - (folders.Count - startIndex);
                content = folders.Skip(startIndex);
				Logger.LogDebug("GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): numFiles → {NumFiles}", folderId, startIndex, numItems, sortExpression, subfolderFilter, numFiles);
                content = content.Union(files.Take(numFiles));
            }

			Logger.LogDebug("<<< Exiting GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter})", folderId, startIndex, numItems, sortExpression, subfolderFilter);
            return new ContentPage
            {
                Folder = folder,
                Items = content.ToList(),
                TotalCount = folders.Count + files.Count,
            };
        }

        /// <inheritdoc />
        public ContentPage SearchFolderContent(int folderId, string pattern, int startIndex, int numItems, string sortExpression = null, SubfolderFilter subfolderFilter = SubfolderFilter.IncludeSubfoldersFolderStructure)
        {
			Logger.LogDebug(">>> Entering SearchFolderContent(folderId={FolderId},pattern={Pattern},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter})", folderId, pattern, startIndex, numItems, sortExpression, subfolderFilter);
            var recursive = subfolderFilter != SubfolderFilter.ExcludeSubfolders;
            var folder = GetFolderInfo(folderId);

			Logger.LogDebug("SearchFolderContent(folderId={FolderId},pattern={Pattern},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): folder → {Folder}", folderId, pattern, startIndex, numItems, sortExpression, subfolderFilter, folder);
            var files = FolderManager.Instance.SearchFiles(folder, pattern, recursive);
            var sortProperties = SortProperties.Parse(sortExpression);
			Logger.LogDebug("SearchFolderContent(folderId={FolderId},pattern={Pattern},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): files → {Files}", folderId, pattern, startIndex, numItems, sortExpression, subfolderFilter, files);
			Logger.LogDebug("SearchFolderContent(folderId={FolderId},pattern={Pattern},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): sortProperties → {SortProperties}", folderId, pattern, startIndex, numItems, sortExpression, subfolderFilter, sortProperties);
            var sortedFiles = SortFiles(files, sortProperties).ToList();

			Logger.LogDebug("SearchFolderContent(folderId={FolderId},pattern={Pattern},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): sortedFiles → {SortedFiles}", folderId, pattern, startIndex, numItems, sortExpression, subfolderFilter, sortedFiles);
            IEnumerable<object> content = sortedFiles.Skip(startIndex).Take(numItems);

			Logger.LogDebug("SearchFolderContent(folderId={FolderId},pattern={Pattern},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): content → {Content}", folderId, pattern, startIndex, numItems, sortExpression, subfolderFilter, content);
			Logger.LogDebug("<<< Exiting SearchFolderContent(folderId={FolderId},pattern={Pattern},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter})", folderId, pattern, startIndex, numItems, sortExpression, subfolderFilter);
            return new ContentPage
            {
                Folder = folder,
                Items = content.ToList(),
                TotalCount = sortedFiles.Count,
            };
        }

        /// <inheritdoc />
        public IEnumerable<IFolderInfo> GetFolders(IFolderInfo parentFolder, string orderingField, bool asc)
        {
            Requires.NotNull("parentFolder", parentFolder);

            var folders = FolderManager.Instance.GetFolders(parentFolder).Where(f => HasPermission(f, "BROWSE") || HasPermission(f, "READ"));

            // Set default sorting values
            var field = string.IsNullOrEmpty(orderingField) ? "FolderName" : orderingField;

            return ApplyOrder(folders.AsQueryable(), field, asc);
        }

        /// <inheritdoc />
        public IFileInfo RenameFile(int fileId, string newFileName)
        {
			Logger.LogDebug(">>> Entering RenameFile(fileId={FileId},newFileName={NewFileName})", fileId, newFileName);
            Requires.NotNullOrEmpty("newFileName", newFileName);

            var filteredName = CleanDotsAtTheEndOfTheName(newFileName);

			Logger.LogDebug("RenameFile(fileId={FileId},newFileName={NewFileName}): filteredName → {FilteredName}", fileId, newFileName, filteredName);
            if (string.IsNullOrEmpty(filteredName))
            {
                throw new AssetManagerException(Localization.GetExceptionMessage("FileNameInvalid", FileNameInvalidDefaultMessage, newFileName));
            }

            // Chech if the new name has invalid chars
            if (IsInvalidName(filteredName))
            {
                throw new AssetManagerException(GetInvalidCharsErrorText());
            }

            // Check if the new name is a reserved name
            if (IsReservedName(filteredName))
            {
                throw new AssetManagerException(Localization.GetExceptionMessage("FolderFileNameIsReserved", FolderFileNameIsReservedDefaultMessage));
            }

            var file = FileManager.Instance.GetFile(fileId, true);

			Logger.LogDebug("RenameFile(fileId={FileId},newFileName={NewFileName}): file → {File}", fileId, newFileName, file);
            // Check if the name has not changed
            if (file.FileName == newFileName)
            {
                return file;
            }

            // Check if user has appropiate permissions
            var folder = FolderManager.Instance.GetFolder(file.FolderId);
            if (!HasPermission(folder, "MANAGE"))
			Logger.LogDebug("RenameFile(fileId={FileId},newFileName={NewFileName}): folder → {Folder}", fileId, newFileName, folder);
            {
                throw new AssetManagerException(Localization.GetExceptionMessage("UserHasNoPermissionToEditFile", UserHasNoPermissionToEditFileDefaultMessage));
            }

			Logger.LogDebug("<<< Exiting RenameFile(fileId={FileId},newFileName={NewFileName})", fileId, newFileName);
            return FileManager.Instance.RenameFile(file, newFileName);
        }

        /// <inheritdoc />
        public IFolderInfo RenameFolder(int folderId, string newFolderName)
        {
			Logger.LogDebug(">>> Entering RenameFolder(folderId={FolderId},newFolderName={NewFolderName})", folderId, newFolderName);
            Requires.NotNullOrEmpty("newFolderName", newFolderName);

            newFolderName = CleanDotsAtTheEndOfTheName(newFolderName);

			Logger.LogDebug("RenameFolder(folderId={FolderId},newFolderName={NewFolderName}): newFolderName → {NewFolderName1}", folderId, newFolderName, newFolderName);
            // Check if the new name has invalid chars
            if (IsInvalidName(newFolderName))
            {
                throw new AssetManagerException(GetInvalidCharsErrorText());
            }

            // Check if the name is reserved
            if (IsReservedName(newFolderName))
            {
                throw new AssetManagerException(Localization.GetExceptionMessage("FolderFileNameIsReserved", FolderFileNameIsReservedDefaultMessage));
            }

            var folder = GetFolderInfo(folderId);

			Logger.LogDebug("RenameFolder(folderId={FolderId},newFolderName={NewFolderName}): folder → {Folder}", folderId, newFolderName, folder);
            // Check if user has appropiate permissions
            if (!HasPermission(folder, "MANAGE"))
            {
                throw new AssetManagerException(Localization.GetExceptionMessage("UserHasNoPermissionToEditFolder", UserHasNoPermissionToEditFolderDefaultMessage));
            }

            // check if the name has not changed
            if (folder.FolderName == newFolderName)
            {
                return folder;
            }

            if (folder.FolderName.Equals(newFolderName, StringComparison.OrdinalIgnoreCase))
            {
                folder.FolderPath = ReplaceFolderName(folder.FolderPath, folder.FolderName, newFolderName);
                return FolderManager.Instance.UpdateFolder(folder);
            }

            var newFolderPath = GetNewFolderPath(newFolderName, folder);

			Logger.LogDebug("RenameFolder(folderId={FolderId},newFolderName={NewFolderName}): newFolderPath → {NewFolderPath}", folderId, newFolderName, newFolderPath);
            // Check if the new folder already exists
            if (FolderManager.Instance.FolderExists(folder.PortalID, newFolderPath))
            {
                throw new AssetManagerException(Localization.GetExceptionMessage("FolderAlreadyExists", FolderAlreadyExistsDefultMessage, newFolderName));
            }

            FolderManager.Instance.RenameFolder(folder, newFolderName);
			Logger.LogDebug("<<< Exiting RenameFolder(folderId={FolderId},newFolderName={NewFolderName})", folderId, newFolderName);
            return folder;
        }

        /// <inheritdoc />
        public IFolderInfo CreateFolder(string folderName, int folderParentId, int folderMappingId, string mappedPath)
        {
			Logger.LogDebug(">>> Entering CreateFolder(folderName={FolderName},folderParentId={FolderParentId},folderMappingId={FolderMappingId},mappedPath={MappedPath})", folderName, folderParentId, folderMappingId, mappedPath);
            Requires.NotNullOrEmpty("folderName", folderName);

            var filterFolderName = CleanDotsAtTheEndOfTheName(folderName);

			Logger.LogDebug("CreateFolder(folderName={FolderName},folderParentId={FolderParentId},folderMappingId={FolderMappingId},mappedPath={MappedPath}): filterFolderName → {FilterFolderName}", folderName, folderParentId, folderMappingId, mappedPath, filterFolderName);
            if (IsInvalidName(filterFolderName))
            {
                throw new AssetManagerException(GetInvalidCharsErrorText());
            }

            // Check if the new name is a reserved name
            if (IsReservedName(filterFolderName))
            {
                throw new AssetManagerException(Localization.GetExceptionMessage("FolderFileNameIsReserved", FolderFileNameIsReservedDefaultMessage));
            }

            var parentFolder = GetFolderInfo(folderParentId);

			Logger.LogDebug("CreateFolder(folderName={FolderName},folderParentId={FolderParentId},folderMappingId={FolderMappingId},mappedPath={MappedPath}): parentFolder → {ParentFolder}", folderName, folderParentId, folderMappingId, mappedPath, parentFolder);
            if (!HasPermission(parentFolder, "ADD"))
            {
                throw new AssetManagerException(Localization.GetExceptionMessage("UserHasNoPermissionToAdd", UserHasNoPermissionToAddDefaultMessage));
            }

            var folderPath = PathUtils.Instance.FormatFolderPath(
                PathUtils.Instance.FormatFolderPath(
                PathUtils.Instance.StripFolderPath(parentFolder.FolderPath).Replace(@"\", "/")) + filterFolderName);

            mappedPath = PathUtils.Instance.FormatFolderPath(mappedPath);

			Logger.LogDebug("CreateFolder(folderName={FolderName},folderParentId={FolderParentId},folderMappingId={FolderMappingId},mappedPath={MappedPath}): mappedPath → {MappedPath1}", folderName, folderParentId, folderMappingId, mappedPath, mappedPath);
            if (!MappedPathRegex.IsMatch(mappedPath))
            {
                throw new AssetManagerException(Localization.GetExceptionMessage("InvalidMappedPath", InvalidMappedPathDefaultMessage));
            }

            try
            {
                var folderMapping = FolderMappingController.Instance.GetFolderMapping(parentFolder.PortalID, folderMappingId);
				Logger.LogDebug("CreateFolder(folderName={FolderName},folderParentId={FolderParentId},folderMappingId={FolderMappingId},mappedPath={MappedPath}): folderMapping → {FolderMapping}", folderName, folderParentId, folderMappingId, mappedPath, folderMapping);
				Logger.LogDebug("<<< Exiting CreateFolder(folderName={FolderName},folderParentId={FolderParentId},folderMappingId={FolderMappingId},mappedPath={MappedPath})", folderName, folderParentId, folderMappingId, mappedPath);
                return FolderManager.Instance.AddFolder(folderMapping, folderPath, mappedPath.Replace(@"\", "/"));
            }
            catch (FolderAlreadyExistsException)
            {
                throw new AssetManagerException(Localization.GetExceptionMessage("FolderAlreadyExists", FolderAlreadyExistsDefaultMessage, filterFolderName));
            }
        }

        /// <inheritdoc />
        public bool DeleteFolder(int folderId, bool onlyUnlink, ICollection<IFolderInfo> nonDeletedSubfolders)
        {
            var folder = FolderManager.Instance.GetFolder(folderId);
            if (folder == null)
            {
                return false;
            }

            if (!HasPermission(folder, "DELETE"))
            {
                nonDeletedSubfolders.Add(folder);
            }
            else
            {
                if (onlyUnlink)
                {
                    FolderManager.Instance.UnlinkFolder(folder);
                }
                else
                {
                    DeleteFolder(folder, nonDeletedSubfolders);
                }
            }

            return true;
        }

        /// <inheritdoc />
        public bool DeleteFile(int fileId)
        {
			Logger.LogDebug(">>> Entering DeleteFile(fileId={FileId})", fileId);
            var fileInfo = FileManager.Instance.GetFile(fileId, true);
            if (fileInfo == null)
			Logger.LogDebug("DeleteFile(fileId={FileId}): fileInfo → {FileInfo}", fileId, fileInfo);
            {
                return false;
            }

            var folder = FolderManager.Instance.GetFolder(fileInfo.FolderId);

			Logger.LogDebug("DeleteFile(fileId={FileId}): folder → {Folder}", fileId, folder);
            if (!HasPermission(folder, "DELETE"))
            {
                return false;
            }

            FileManager.Instance.DeleteFile(fileInfo);
			Logger.LogDebug("<<< Exiting DeleteFile(fileId={FileId})", fileId);
            return true;
        }

        private static IEnumerable<IFileInfo> SortFiles(IEnumerable<IFileInfo> files, SortProperties sortProperties)
        {
            switch (sortProperties.Column)
            {
                case "ItemName":
                    return OrderBy(files, f => f.FileName, sortProperties.Ascending);
                case "LastModifiedOnDate":
                    return OrderBy(files, f => f.LastModifiedOnDate, sortProperties.Ascending);
                case "Size":
                    return OrderBy(files, f => f.Size, sortProperties.Ascending);
                case "ParentFolder":
                    return OrderBy(files, f => f.FolderId, new FolderPathComparer(), sortProperties.Ascending);
                case "CreatedOnDate":
                    return OrderBy(files, f => f.CreatedOnDate, sortProperties.Ascending);
                default:
                    return files;
            }
        }

        private static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, bool ascending)
        {
            return ascending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
        }

        private static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer, bool ascending)
        {
            return ascending ? source.OrderBy(keySelector, comparer) : source.OrderByDescending(keySelector, comparer);
        }

        private static string CleanDotsAtTheEndOfTheName(string name)
        {
			Logger.LogDebug(">>> Entering CleanDotsAtTheEndOfTheName(name={Name})", name);
			Logger.LogDebug("<<< Exiting CleanDotsAtTheEndOfTheName(name={Name})", name);
            return name.Trim().TrimEnd('.', ' ');
        }

        private static IEnumerable<IFileInfo> GetFiles(IFolderInfo folder, SortProperties sortProperties, int startIndex, bool recursive)
        {
            Requires.NotNull("folder", folder);

            if (Host.EnableFileAutoSync && startIndex == 0)
            {
                FolderManager.Instance.Synchronize(folder.PortalID, folder.FolderPath, false, true);
            }

            return SortFiles(FolderManager.Instance.GetFiles(folder, recursive, true), sortProperties);
        }

        private static void DeleteFolder(IFolderInfo folder, ICollection<IFolderInfo> nonDeletedItems)
        {
            var nonDeletedSubfolders = new List<IFolderInfo>();
            FolderManager.Instance.DeleteFolder(folder, nonDeletedSubfolders);
            if (nonDeletedSubfolders.Count == 0)
            {
                return;
            }

            foreach (var nonDeletedSubfolder in nonDeletedSubfolders)
            {
                nonDeletedItems.Add(nonDeletedSubfolder);
            }
			Logger.LogDebug("<<< Exiting DeleteFolder(folderId={FolderId},onlyUnlink={OnlyUnlink},nonDeletedSubfolders={NonDeletedSubfolders})", folderId, onlyUnlink, nonDeletedSubfolders);
        }

        private static bool IsInvalidName(string itemName)
        {
			Logger.LogDebug(">>> Entering IsInvalidName(itemName={ItemName})", itemName);
            var invalidFilenameChars = RegexUtils.GetCachedRegex("[" + Regex.Escape(GetInvalidChars()) + "]");

			Logger.LogDebug("IsInvalidName(itemName={ItemName}): invalidFilenameChars → {InvalidFilenameChars}", itemName, invalidFilenameChars);
			Logger.LogDebug("<<< Exiting IsInvalidName(itemName={ItemName})", itemName);
            return invalidFilenameChars.IsMatch(itemName);
        }

        private static string GetInvalidChars()
        {
			Logger.LogDebug(">>> Entering GetInvalidChars()");
            var invalidChars = new string(Path.GetInvalidFileNameChars());

			Logger.LogDebug("GetInvalidChars(): invalidChars → {InvalidChars}", invalidChars);
            foreach (var ch in Path.GetInvalidPathChars())
            {
                if (invalidChars.IndexOf(ch) == -1)
                {
                    // The ch does not exist
                    invalidChars += ch;
                }
            }

			Logger.LogDebug("<<< Exiting GetInvalidChars()");
            return invalidChars;
        }

        private static bool IsReservedName(string name)
        {
			Logger.LogDebug(">>> Entering IsReservedName(name={Name})", name);
            var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9", "CLOCK$" };
			Logger.LogDebug("IsReservedName(name={Name}): reservedNames → {ReservedNames}", name, reservedNames);
			Logger.LogDebug("<<< Exiting IsReservedName(name={Name})", name);
            return reservedNames.Contains(Path.GetFileNameWithoutExtension(name.ToUpperInvariant()));
        }

        private static string GetInvalidCharsErrorText()
        {
			Logger.LogDebug(">>> Entering GetInvalidCharsErrorText()");
            throw new AssetManagerException(
                Localization.GetExceptionMessage(
                    "FolderFileNameHasInvalidcharacters",
                    FolderFileNameHasInvalidcharactersDefaultMessage,
                    "\\:/*?\"<>|"));
				Logger.LogDebug("<<< Exiting GetInvalidCharsErrorText()");
        }

        private static IFolderInfo GetFolderInfo(int folderId)
        {
			Logger.LogDebug(">>> Entering GetFolderInfo(folderId={FolderId})", folderId);
            var folder = FolderManager.Instance.GetFolder(folderId);
            if (folder == null)
			Logger.LogDebug("GetFolderInfo(folderId={FolderId}): folder → {Folder}", folderId, folder);
            {
                throw new AssetManagerException(Localization.GetExceptionMessage("FolderDoesNotExists", DefaultMessageDefaultMessage));
            }

			Logger.LogDebug("<<< Exiting GetFolderInfo(folderId={FolderId})", folderId);
            return folder;
        }

        private static string ReplaceFolderName(string path, string folderName, string newFolderName)
        {
			Logger.LogDebug(">>> Entering ReplaceFolderName(path={Path},folderName={FolderName},newFolderName={NewFolderName})", path, folderName, newFolderName);
            var newPath = PathUtils.Instance.RemoveTrailingSlash(path);
            if (string.IsNullOrEmpty(newPath))
			Logger.LogDebug("ReplaceFolderName(path={Path},folderName={FolderName},newFolderName={NewFolderName}): newPath → {NewPath}", path, folderName, newFolderName, newPath);
            {
                return path;
            }

            var nameIndex = newPath.LastIndexOf(folderName, StringComparison.Ordinal);
            if (nameIndex == -1)
			Logger.LogDebug("ReplaceFolderName(path={Path},folderName={FolderName},newFolderName={NewFolderName}): nameIndex → {NameIndex}", path, folderName, newFolderName, nameIndex);
            {
                return path;
            }

			Logger.LogDebug("<<< Exiting ReplaceFolderName(path={Path},folderName={FolderName},newFolderName={NewFolderName})", path, folderName, newFolderName);
            return newPath.Substring(0, nameIndex) + newPath.Substring(nameIndex).Replace(folderName, newFolderName);
        }

        private static string GetNewFolderPath(string newFolderName, IFolderInfo folder)
        {
            if (folder.FolderName.Equals(newFolderName, StringComparison.OrdinalIgnoreCase))
            {
                return folder.FolderPath;
            }

            var oldFolderPath = folder.FolderPath;
            if (oldFolderPath.Length > 0)
            {
                oldFolderPath = oldFolderPath.Substring(0, oldFolderPath.LastIndexOf(folder.FolderName, StringComparison.Ordinal));
            }

            return PathUtils.Instance.FormatFolderPath(oldFolderPath + newFolderName);
        }
    }
}
