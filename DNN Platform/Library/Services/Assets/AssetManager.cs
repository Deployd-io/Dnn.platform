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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AssetManager));
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
			Logger.LogError(">>> Entering ApplyOrder(source={Source},propertyName={PropertyName},asc={Asc})", source, propertyName, asc);
            var methodName = asc ? "OrderBy" : "OrderByDescending";
            var arg = Expression.Parameter(typeof(T), "x");
			Logger.LogError("ApplyOrder(source={Source},propertyName={PropertyName},asc={Asc}): arg → {Arg}", source, propertyName, asc, arg);
			Logger.LogError("ApplyOrder(source={Source},propertyName={PropertyName},asc={Asc}): methodName → {MethodName}", source, propertyName, asc, methodName);

            // Use reflection to mirror LINQ
            var property = typeof(T).GetProperty(propertyName);
			Logger.LogError("ApplyOrder(source={Source},propertyName={PropertyName},asc={Asc}): property → {Property}", source, propertyName, asc, property);

            // If property is undefined returns the original source
            if (property == null)
            {
                return (IOrderedQueryable<T>)source;
            }

            Expression expr = Expression.Property(arg, property);
			Logger.LogError("ApplyOrder(source={Source},propertyName={PropertyName},asc={Asc}): expr → {Expr}", source, propertyName, asc, expr);

            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), property.PropertyType);
			Logger.LogError("ApplyOrder(source={Source},propertyName={PropertyName},asc={Asc}): delegateType → {DelegateType}", source, propertyName, asc, delegateType);
            var lambda = Expression.Lambda(delegateType, expr, arg);
			Logger.LogError("ApplyOrder(source={Source},propertyName={PropertyName},asc={Asc}): lambda → {Lambda}", source, propertyName, asc, lambda);

            var result = typeof(Queryable).GetMethods().Single(
                    method => method.Name == methodName
                            && method.IsGenericMethodDefinition
                            && method.GetGenericArguments().Length == 2
                            && method.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), property.PropertyType)
                    .Invoke(null, new object[] { source, lambda });
			Logger.LogWarning("<<< Exiting ApplyOrder(source={Source},propertyName={PropertyName},asc={Asc})", source, propertyName, asc);
            return (IOrderedQueryable<T>)result;
        }

        public static bool HasPermission(IFolderInfo folder, string permissionKey)
        {
			Logger.LogError(">>> Entering HasPermission(folder={Folder},permissionKey={PermissionKey})", folder, permissionKey);
            var hasPermision = PortalSettings.Current.UserInfo.IsSuperUser;

			Logger.LogError("HasPermission(folder={Folder},permissionKey={PermissionKey}): hasPermision → {HasPermision}", folder, permissionKey, hasPermision);
            if (!hasPermision && folder != null)
            {
                hasPermision = FolderPermissionController.HasFolderPermission(folder.FolderPermissions, permissionKey);
            }

			Logger.LogWarning("<<< Exiting HasPermission(folder={Folder},permissionKey={PermissionKey})", folder, permissionKey);
            return hasPermision;
        }

        /// <inheritdoc />
        public ContentPage GetFolderContent(int folderId, int startIndex, int numItems, string sortExpression = null, SubfolderFilter subfolderFilter = SubfolderFilter.IncludeSubfoldersFolderStructure)
        {
			Logger.LogError(">>> Entering GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter})", folderId, startIndex, numItems, sortExpression, subfolderFilter);
            var folder = GetFolderInfo(folderId);

			Logger.LogError("GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): folder → {Folder}", folderId, startIndex, numItems, sortExpression, subfolderFilter, folder);
            if (!FolderPermissionController.CanBrowseFolder((FolderInfo)folder))
            {
				Logger.LogError("Throwing AssetManagerException from GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter})", folderId, startIndex, numItems, sortExpression, subfolderFilter);
                throw new AssetManagerException(Localization.GetExceptionMessage("UserHasNoPermissionToBrowseFolder", UserHasNoPermissionToBrowseFolderDefaultMessage));
            }

            var sortProperties = SortProperties.Parse(sortExpression);
			Logger.LogError("GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): sortProperties → {SortProperties}", folderId, startIndex, numItems, sortExpression, subfolderFilter, sortProperties);

            List<IFolderInfo> folders;

            if (subfolderFilter != SubfolderFilter.IncludeSubfoldersFolderStructure)
            {
                folders = new List<IFolderInfo>();
				Logger.LogError("GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): folders → {Folders}", folderId, startIndex, numItems, sortExpression, subfolderFilter, folders);
            }
            else
            {
                folders = this.GetFolders(folder, sortProperties.Column == "ItemName" ? "FolderName" : sortProperties.Column, sortProperties.Ascending).ToList();
            }

            var recursive = subfolderFilter == SubfolderFilter.IncludeSubfoldersFilesOnly;
            var files = GetFiles(folder, sortProperties, startIndex, recursive).ToList();
			Logger.LogError("GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): files → {Files}", folderId, startIndex, numItems, sortExpression, subfolderFilter, files);

            IEnumerable<object> content;
            if (startIndex + numItems <= folders.Count)
            {
                content = folders.Skip(startIndex).Take(numItems);
				Logger.LogError("GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): content → {Content}", folderId, startIndex, numItems, sortExpression, subfolderFilter, content);
            }
            else if (startIndex >= folders.Count)
            {
                content = files.Skip(startIndex - folders.Count).Take(numItems);
            }
            else
            {
                var numFiles = numItems - (folders.Count - startIndex);
				Logger.LogError("GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): numFiles → {NumFiles}", folderId, startIndex, numItems, sortExpression, subfolderFilter, numFiles);
                content = folders.Skip(startIndex);
                content = content.Union(files.Take(numFiles));
            }

			Logger.LogWarning("<<< Exiting GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter})", folderId, startIndex, numItems, sortExpression, subfolderFilter);
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
			Logger.LogError(">>> Entering SearchFolderContent(folderId={FolderId},pattern={Pattern},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter})", folderId, pattern, startIndex, numItems, sortExpression, subfolderFilter);
            var recursive = subfolderFilter != SubfolderFilter.ExcludeSubfolders;
            var folder = GetFolderInfo(folderId);
			Logger.LogError("SearchFolderContent(folderId={FolderId},pattern={Pattern},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): folder → {Folder}", folderId, pattern, startIndex, numItems, sortExpression, subfolderFilter, folder);

            var files = FolderManager.Instance.SearchFiles(folder, pattern, recursive);
			Logger.LogError("SearchFolderContent(folderId={FolderId},pattern={Pattern},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): files → {Files}", folderId, pattern, startIndex, numItems, sortExpression, subfolderFilter, files);
            var sortProperties = SortProperties.Parse(sortExpression);
			Logger.LogError("SearchFolderContent(folderId={FolderId},pattern={Pattern},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): sortProperties → {SortProperties}", folderId, pattern, startIndex, numItems, sortExpression, subfolderFilter, sortProperties);
            var sortedFiles = SortFiles(files, sortProperties).ToList();
			Logger.LogError("SearchFolderContent(folderId={FolderId},pattern={Pattern},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): sortedFiles → {SortedFiles}", folderId, pattern, startIndex, numItems, sortExpression, subfolderFilter, sortedFiles);

            IEnumerable<object> content = sortedFiles.Skip(startIndex).Take(numItems);
			Logger.LogError("SearchFolderContent(folderId={FolderId},pattern={Pattern},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter}): content → {Content}", folderId, pattern, startIndex, numItems, sortExpression, subfolderFilter, content);

			Logger.LogWarning("<<< Exiting SearchFolderContent(folderId={FolderId},pattern={Pattern},startIndex={StartIndex},numItems={NumItems},sortExpression={SortExpression},subfolderFilter={SubfolderFilter})", folderId, pattern, startIndex, numItems, sortExpression, subfolderFilter);
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
			Logger.LogError(">>> Entering GetFolders(parentFolder={ParentFolder},orderingField={OrderingField},asc={Asc})", parentFolder, orderingField, asc);
            Requires.NotNull("parentFolder", parentFolder);

            var folders = FolderManager.Instance.GetFolders(parentFolder).Where(f => HasPermission(f, "BROWSE") || HasPermission(f, "READ"));
			Logger.LogError("GetFolders(parentFolder={ParentFolder},orderingField={OrderingField},asc={Asc}): folders → {Folders}", parentFolder, orderingField, asc, folders);

            // Set default sorting values
            var field = string.IsNullOrEmpty(orderingField) ? "FolderName" : orderingField;
			Logger.LogError("GetFolders(parentFolder={ParentFolder},orderingField={OrderingField},asc={Asc}): field → {Field}", parentFolder, orderingField, asc, field);

			Logger.LogWarning("<<< Exiting GetFolders(parentFolder={ParentFolder},orderingField={OrderingField},asc={Asc})", parentFolder, orderingField, asc);
            return ApplyOrder(folders.AsQueryable(), field, asc);
        }

        /// <inheritdoc />
        public IFileInfo RenameFile(int fileId, string newFileName)
        {
			Logger.LogError(">>> Entering RenameFile(fileId={FileId},newFileName={NewFileName})", fileId, newFileName);
            Requires.NotNullOrEmpty("newFileName", newFileName);

            var filteredName = CleanDotsAtTheEndOfTheName(newFileName);
			Logger.LogError("RenameFile(fileId={FileId},newFileName={NewFileName}): filteredName → {FilteredName}", fileId, newFileName, filteredName);

            if (string.IsNullOrEmpty(filteredName))
            {
				Logger.LogError("Throwing AssetManagerException from RenameFile(fileId={FileId},newFileName={NewFileName})", fileId, newFileName);
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
			Logger.LogError("RenameFile(fileId={FileId},newFileName={NewFileName}): file → {File}", fileId, newFileName, file);

            // Check if the name has not changed
            if (file.FileName == newFileName)
            {
                return file;
            }

            // Check if user has appropiate permissions
            var folder = FolderManager.Instance.GetFolder(file.FolderId);
			Logger.LogError("RenameFile(fileId={FileId},newFileName={NewFileName}): folder → {Folder}", fileId, newFileName, folder);
            if (!HasPermission(folder, "MANAGE"))
            {
                throw new AssetManagerException(Localization.GetExceptionMessage("UserHasNoPermissionToEditFile", UserHasNoPermissionToEditFileDefaultMessage));
            }

			Logger.LogWarning("<<< Exiting RenameFile(fileId={FileId},newFileName={NewFileName})", fileId, newFileName);
            return FileManager.Instance.RenameFile(file, newFileName);
        }

        /// <inheritdoc />
        public IFolderInfo RenameFolder(int folderId, string newFolderName)
        {
			Logger.LogError(">>> Entering RenameFolder(folderId={FolderId},newFolderName={NewFolderName})", folderId, newFolderName);
            Requires.NotNullOrEmpty("newFolderName", newFolderName);

            newFolderName = CleanDotsAtTheEndOfTheName(newFolderName);
			Logger.LogError("RenameFolder(folderId={FolderId},newFolderName={NewFolderName}): newFolderName → {NewFolderName1}", folderId, newFolderName, newFolderName);

            // Check if the new name has invalid chars
            if (IsInvalidName(newFolderName))
            {
				Logger.LogError("Throwing AssetManagerException from RenameFolder(folderId={FolderId},newFolderName={NewFolderName})", folderId, newFolderName);
                throw new AssetManagerException(GetInvalidCharsErrorText());
            }

            // Check if the name is reserved
            if (IsReservedName(newFolderName))
            {
                throw new AssetManagerException(Localization.GetExceptionMessage("FolderFileNameIsReserved", FolderFileNameIsReservedDefaultMessage));
            }

            var folder = GetFolderInfo(folderId);
			Logger.LogError("RenameFolder(folderId={FolderId},newFolderName={NewFolderName}): folder → {Folder}", folderId, newFolderName, folder);

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
			Logger.LogError("RenameFolder(folderId={FolderId},newFolderName={NewFolderName}): newFolderPath → {NewFolderPath}", folderId, newFolderName, newFolderPath);

            // Check if the new folder already exists
            if (FolderManager.Instance.FolderExists(folder.PortalID, newFolderPath))
            {
                throw new AssetManagerException(Localization.GetExceptionMessage("FolderAlreadyExists", FolderAlreadyExistsDefultMessage, newFolderName));
            }

            FolderManager.Instance.RenameFolder(folder, newFolderName);
			Logger.LogWarning("<<< Exiting RenameFolder(folderId={FolderId},newFolderName={NewFolderName})", folderId, newFolderName);
            return folder;
        }

        /// <inheritdoc />
        public IFolderInfo CreateFolder(string folderName, int folderParentId, int folderMappingId, string mappedPath)
        {
			Logger.LogError(">>> Entering CreateFolder(folderName={FolderName},folderParentId={FolderParentId},folderMappingId=<redacted>,mappedPath={MappedPath})", folderName, folderParentId, mappedPath);
            Requires.NotNullOrEmpty("folderName", folderName);

            var filterFolderName = CleanDotsAtTheEndOfTheName(folderName);
			Logger.LogError("CreateFolder(folderName={FolderName},folderParentId={FolderParentId},folderMappingId=<redacted>,mappedPath={MappedPath}): filterFolderName → {FilterFolderName}", folderName, folderParentId, mappedPath, filterFolderName);

            if (IsInvalidName(filterFolderName))
            {
				Logger.LogError("Throwing AssetManagerException from CreateFolder(folderName={FolderName},folderParentId={FolderParentId},folderMappingId=<redacted>,mappedPath={MappedPath})", folderName, folderParentId, mappedPath);
                throw new AssetManagerException(GetInvalidCharsErrorText());
            }

            // Check if the new name is a reserved name
            if (IsReservedName(filterFolderName))
            {
                throw new AssetManagerException(Localization.GetExceptionMessage("FolderFileNameIsReserved", FolderFileNameIsReservedDefaultMessage));
            }

            var parentFolder = GetFolderInfo(folderParentId);
			Logger.LogError("CreateFolder(folderName={FolderName},folderParentId={FolderParentId},folderMappingId=<redacted>,mappedPath={MappedPath}): parentFolder → {ParentFolder}", folderName, folderParentId, mappedPath, parentFolder);

            if (!HasPermission(parentFolder, "ADD"))
            {
                throw new AssetManagerException(Localization.GetExceptionMessage("UserHasNoPermissionToAdd", UserHasNoPermissionToAddDefaultMessage));
            }

            var folderPath = PathUtils.Instance.FormatFolderPath(
                PathUtils.Instance.FormatFolderPath(
                PathUtils.Instance.StripFolderPath(parentFolder.FolderPath).Replace(@"\", "/")) + filterFolderName);

            mappedPath = PathUtils.Instance.FormatFolderPath(mappedPath);

            if (!MappedPathRegex.IsMatch(mappedPath))
            {
				Logger.LogError("CreateFolder(folderName={FolderName},folderParentId={FolderParentId},folderMappingId=<redacted>,mappedPath={MappedPath}): mappedPath → {MappedPath1}", folderName, folderParentId, mappedPath, mappedPath);
                throw new AssetManagerException(Localization.GetExceptionMessage("InvalidMappedPath", InvalidMappedPathDefaultMessage));
            }

            try
            {
                var folderMapping = FolderMappingController.Instance.GetFolderMapping(parentFolder.PortalID, folderMappingId);
				Logger.LogError("CreateFolder(folderName={FolderName},folderParentId={FolderParentId},folderMappingId=<redacted>,mappedPath={MappedPath}): folderMapping → {FolderMapping}", folderName, folderParentId, mappedPath, folderMapping);
				Logger.LogWarning("<<< Exiting CreateFolder(folderName={FolderName},folderParentId={FolderParentId},folderMappingId=<redacted>,mappedPath={MappedPath})", folderName, folderParentId, mappedPath);
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
			Logger.LogError(">>> Entering DeleteFolder(folderId={FolderId},onlyUnlink={OnlyUnlink},nonDeletedSubfolders={NonDeletedSubfolders})", folderId, onlyUnlink, nonDeletedSubfolders);
            var folder = FolderManager.Instance.GetFolder(folderId);
			Logger.LogError("DeleteFolder(folderId={FolderId},onlyUnlink={OnlyUnlink},nonDeletedSubfolders={NonDeletedSubfolders}): folder → {Folder}", folderId, onlyUnlink, nonDeletedSubfolders, folder);
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

			Logger.LogWarning("<<< Exiting DeleteFolder(folderId={FolderId},onlyUnlink={OnlyUnlink},nonDeletedSubfolders={NonDeletedSubfolders})", folderId, onlyUnlink, nonDeletedSubfolders);
            return true;
        }

        /// <inheritdoc />
        public bool DeleteFile(int fileId)
        {
			Logger.LogError(">>> Entering DeleteFile(fileId={FileId})", fileId);
            var fileInfo = FileManager.Instance.GetFile(fileId, true);
			Logger.LogError("DeleteFile(fileId={FileId}): fileInfo → {FileInfo}", fileId, fileInfo);
            if (fileInfo == null)
            {
                return false;
            }

            var folder = FolderManager.Instance.GetFolder(fileInfo.FolderId);
			Logger.LogError("DeleteFile(fileId={FileId}): folder → {Folder}", fileId, folder);

            if (!HasPermission(folder, "DELETE"))
            {
                return false;
            }

            FileManager.Instance.DeleteFile(fileInfo);
			Logger.LogWarning("<<< Exiting DeleteFile(fileId={FileId})", fileId);
            return true;
        }

        private static IEnumerable<IFileInfo> SortFiles(IEnumerable<IFileInfo> files, SortProperties sortProperties)
        {
			Logger.LogError(">>> Entering SortFiles(files={Files},sortProperties={SortProperties})", files, sortProperties);
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
					Logger.LogWarning("<<< Exiting SortFiles(files={Files},sortProperties={SortProperties})", files, sortProperties);
                    return files;
            }
        }

        private static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, bool ascending)
        {
			Logger.LogError(">>> Entering OrderBy(source={Source},keySelector={KeySelector},ascending={Ascending})", source, keySelector, ascending);
            Logger.LogWarning("<<< Exiting OrderBy(source={Source},keySelector={KeySelector},ascending={Ascending})", source, keySelector, ascending);
            return ascending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
        }

        private static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer, bool ascending)
        {
            return ascending ? source.OrderBy(keySelector, comparer) : source.OrderByDescending(keySelector, comparer);
        }

        private static string CleanDotsAtTheEndOfTheName(string name)
        {
			Logger.LogError(">>> Entering CleanDotsAtTheEndOfTheName(name={Name})", name);
            Logger.LogWarning("<<< Exiting CleanDotsAtTheEndOfTheName(name={Name})", name);
            return name.Trim().TrimEnd('.', ' ');
        }

        private static IEnumerable<IFileInfo> GetFiles(IFolderInfo folder, SortProperties sortProperties, int startIndex, bool recursive)
        {
			Logger.LogError(">>> Entering GetFiles(folder={Folder},sortProperties={SortProperties},startIndex={StartIndex},recursive={Recursive})", folder, sortProperties, startIndex, recursive);
            Requires.NotNull("folder", folder);

            if (Host.EnableFileAutoSync && startIndex == 0)
            {
                FolderManager.Instance.Synchronize(folder.PortalID, folder.FolderPath, false, true);
            }

			Logger.LogWarning("<<< Exiting GetFiles(folder={Folder},sortProperties={SortProperties},startIndex={StartIndex},recursive={Recursive})", folder, sortProperties, startIndex, recursive);
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
        }

        private static bool IsInvalidName(string itemName)
        {
			Logger.LogError(">>> Entering IsInvalidName(itemName={ItemName})", itemName);
            var invalidFilenameChars = RegexUtils.GetCachedRegex("[" + Regex.Escape(GetInvalidChars()) + "]");

			Logger.LogError("IsInvalidName(itemName={ItemName}): invalidFilenameChars → {InvalidFilenameChars}", itemName, invalidFilenameChars);
			Logger.LogWarning("<<< Exiting IsInvalidName(itemName={ItemName})", itemName);
            return invalidFilenameChars.IsMatch(itemName);
        }

        private static string GetInvalidChars()
        {
			Logger.LogError(">>> Entering GetInvalidChars()");
            var invalidChars = new string(Path.GetInvalidFileNameChars());

			Logger.LogError("GetInvalidChars(): invalidChars → {InvalidChars}", invalidChars);
            foreach (var ch in Path.GetInvalidPathChars())
            {
                if (invalidChars.IndexOf(ch) == -1)
                {
                    // The ch does not exist
                    invalidChars += ch;
                }
            }

			Logger.LogWarning("<<< Exiting GetInvalidChars()");
            return invalidChars;
        }

        private static bool IsReservedName(string name)
        {
			Logger.LogError(">>> Entering IsReservedName(name={Name})", name);
            var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9", "CLOCK$" };
			Logger.LogError("IsReservedName(name={Name}): reservedNames → {ReservedNames}", name, reservedNames);
			Logger.LogWarning("<<< Exiting IsReservedName(name={Name})", name);
            return reservedNames.Contains(Path.GetFileNameWithoutExtension(name.ToUpperInvariant()));
        }

        private static string GetInvalidCharsErrorText()
        {
			Logger.LogError(">>> Entering GetInvalidCharsErrorText()");
					Logger.LogError("Throwing AssetManagerException from GetInvalidCharsErrorText()");
        Logger.LogWarning("<<< Exiting GetInvalidCharsErrorText()");
            throw new AssetManagerException(
                Localization.GetExceptionMessage(
                    "FolderFileNameHasInvalidcharacters",
                    FolderFileNameHasInvalidcharactersDefaultMessage,
                    "\\:/*?\"<>|"));
        }

        private static IFolderInfo GetFolderInfo(int folderId)
        {
			Logger.LogError(">>> Entering GetFolderInfo(folderId={FolderId})", folderId);
            var folder = FolderManager.Instance.GetFolder(folderId);
			Logger.LogError("GetFolderInfo(folderId={FolderId}): folder → {Folder}", folderId, folder);
            if (folder == null)
            {
				Logger.LogError("Throwing AssetManagerException from GetFolderInfo(folderId={FolderId})", folderId);
                throw new AssetManagerException(Localization.GetExceptionMessage("FolderDoesNotExists", DefaultMessageDefaultMessage));
            }

			Logger.LogWarning("<<< Exiting GetFolderInfo(folderId={FolderId})", folderId);
            return folder;
        }

        private static string ReplaceFolderName(string path, string folderName, string newFolderName)
        {
			Logger.LogError(">>> Entering ReplaceFolderName(path={Path},folderName={FolderName},newFolderName={NewFolderName})", path, folderName, newFolderName);
            var newPath = PathUtils.Instance.RemoveTrailingSlash(path);
			Logger.LogError("ReplaceFolderName(path={Path},folderName={FolderName},newFolderName={NewFolderName}): newPath → {NewPath}", path, folderName, newFolderName, newPath);
            if (string.IsNullOrEmpty(newPath))
            {
                return path;
            }

            var nameIndex = newPath.LastIndexOf(folderName, StringComparison.Ordinal);
			Logger.LogError("ReplaceFolderName(path={Path},folderName={FolderName},newFolderName={NewFolderName}): nameIndex → {NameIndex}", path, folderName, newFolderName, nameIndex);
            if (nameIndex == -1)
            {
                return path;
            }

			Logger.LogWarning("<<< Exiting ReplaceFolderName(path={Path},folderName={FolderName},newFolderName={NewFolderName})", path, folderName, newFolderName);
            return newPath.Substring(0, nameIndex) + newPath.Substring(nameIndex).Replace(folderName, newFolderName);
        }

        private static string GetNewFolderPath(string newFolderName, IFolderInfo folder)
        {
			Logger.LogError(">>> Entering GetNewFolderPath(newFolderName={NewFolderName},folder={Folder})", newFolderName, folder);
            if (folder.FolderName.Equals(newFolderName, StringComparison.OrdinalIgnoreCase))
            {
                return folder.FolderPath;
            }

            var oldFolderPath = folder.FolderPath;
			Logger.LogError("GetNewFolderPath(newFolderName={NewFolderName},folder={Folder}): oldFolderPath → {OldFolderPath}", newFolderName, folder, oldFolderPath);
            if (oldFolderPath.Length > 0)
            {
                oldFolderPath = oldFolderPath.Substring(0, oldFolderPath.LastIndexOf(folder.FolderName, StringComparison.Ordinal));
            }

			Logger.LogWarning("<<< Exiting GetNewFolderPath(newFolderName={NewFolderName},folder={Folder})", newFolderName, folder);
            return PathUtils.Instance.FormatFolderPath(oldFolderPath + newFolderName);
        }
    }
}
