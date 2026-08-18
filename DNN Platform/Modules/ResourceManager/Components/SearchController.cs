// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.Modules.ResourceManager.Exceptions;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Services.Assets;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;

    /// <summary>Provides search functionality.</summary>
    public class SearchController : ComponentBase<ISearchController, SearchController>, ISearchController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SearchController));
        private readonly IPermissionsManager permissionsManager;

        /// <summary>Initializes a new instance of the <see cref="SearchController"/> class.</summary>
        public SearchController()
        {
			Logger.LogDebug(">>> Entering SearchController()");
            this.permissionsManager = PermissionsManager.Instance;
			Logger.LogDebug("SearchController(): permissionsManager → {PermissionsManager}", permissionsManager);
			Logger.LogDebug("SearchController(): this.permissionsManager → {Arg0}", this.permissionsManager);
			Logger.LogDebug("<<< Exiting SearchController()");
        }

        /// <inheritdoc />
        public IList<IFileInfo> SearchFolderContent(int moduleId, IFolderInfo folder, bool recursive, string search, int pageIndex, int pageSize, string sorting, int moduleMode, out int totalCount)
        {
			Logger.LogDebug(">>> Entering SearchFolderContent(moduleId={ModuleId},folder={Folder},recursive={Recursive},search={Search},pageIndex={PageIndex},pageSize={PageSize},sorting={Sorting},moduleMode={ModuleMode})", moduleId, folder, recursive, search, pageIndex, pageSize, sorting, moduleMode);
            var noPermissionMessage = Localization.GetExceptionMessage(
                "UserHasNoPermissionToBrowseFolder",
                Constants.UserHasNoPermissionToBrowseFolderDefaultMessage);

            if (!this.permissionsManager.HasFolderContentPermission(folder.FolderID, moduleMode))
            {
				Logger.LogWarning("Throwing FolderPermissionNotMetException from SearchFolderContent(moduleId={ModuleId},folder={Folder},recursive={Recursive},search={Search},pageIndex={PageIndex},pageSize={PageSize},sorting={Sorting},moduleMode={ModuleMode})", moduleId, folder, recursive, search, pageIndex, pageSize, sorting, moduleMode);
                throw new FolderPermissionNotMetException(noPermissionMessage);
            }

            search = (search ?? string.Empty).Trim();
			Logger.LogDebug("SearchFolderContent(moduleId={ModuleId},folder={Folder},recursive={Recursive},search={Search},pageIndex={PageIndex},pageSize={PageSize},sorting={Sorting},moduleMode={ModuleMode}): search → {Search1}", moduleId, folder, recursive, search, pageIndex, pageSize, sorting, moduleMode, search);

            var files = FolderManager.Instance.SearchFiles(folder, search, recursive);
			Logger.LogDebug("SearchFolderContent(moduleId={ModuleId},folder={Folder},recursive={Recursive},search={Search},pageIndex={PageIndex},pageSize={PageSize},sorting={Sorting},moduleMode={ModuleMode}): files → {Files}", moduleId, folder, recursive, search, pageIndex, pageSize, sorting, moduleMode, files);
            var sortProperties = SortProperties.Parse(sorting);
			Logger.LogDebug("SearchFolderContent(moduleId={ModuleId},folder={Folder},recursive={Recursive},search={Search},pageIndex={PageIndex},pageSize={PageSize},sorting={Sorting},moduleMode={ModuleMode}): sortProperties → {SortProperties}", moduleId, folder, recursive, search, pageIndex, pageSize, sorting, moduleMode, sortProperties);
            var sortedFiles = SortFiles(files, sortProperties).ToList();
			Logger.LogDebug("SearchFolderContent(moduleId={ModuleId},folder={Folder},recursive={Recursive},search={Search},pageIndex={PageIndex},pageSize={PageSize},sorting={Sorting},moduleMode={ModuleMode}): sortedFiles → {SortedFiles}", moduleId, folder, recursive, search, pageIndex, pageSize, sorting, moduleMode, sortedFiles);
            totalCount = sortedFiles.Count;
			Logger.LogDebug("SearchFolderContent(moduleId={ModuleId},folder={Folder},recursive={Recursive},search={Search},pageIndex={PageIndex},pageSize={PageSize},sorting={Sorting},moduleMode={ModuleMode}): totalCount → {TotalCount}", moduleId, folder, recursive, search, pageIndex, pageSize, sorting, moduleMode, totalCount);

			Logger.LogDebug("<<< Exiting SearchFolderContent(moduleId={ModuleId},folder={Folder},recursive={Recursive},search={Search},pageIndex={PageIndex},pageSize={PageSize},sorting={Sorting},moduleMode={ModuleMode})", moduleId, folder, recursive, search, pageIndex, pageSize, sorting, moduleMode);
            return sortedFiles.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
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
			Logger.LogDebug(">>> Entering OrderBy(source={Source},keySelector={KeySelector},ascending={Ascending})", source, keySelector, ascending);
			Logger.LogDebug("<<< Exiting OrderBy(source={Source},keySelector={KeySelector},ascending={Ascending})", source, keySelector, ascending);
			Logger.LogDebug("<<< Exiting OrderBy(source={Source},keySelector={KeySelector},comparer={Comparer},ascending={Ascending})", source, keySelector, comparer, ascending);
            return ascending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
        }

        private static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer, bool ascending)
        {
            return ascending ? source.OrderBy(keySelector, comparer) : source.OrderByDescending(keySelector, comparer);
        }
    }
}
