// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components.Common
{
    using System;
    using System.ComponentModel;

    using DotNetNuke.Services.FileSystem;

    /// <summary>General utilities for the Resource Manager.</summary>
    public class Utils
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Utils));
        /// <summary>
        /// Obtains a human friendly description from an enum value using the
        /// <see cref="System.ComponentModel.DescriptionAttribute" /> attribute to get a proper name.
        /// </summary>
        /// <param name="enumValue">The enum value to lookup.</param>
        /// <returns>The specified description attribute name or the value of the enum as a string.</returns>
        public static string GetEnumDescription(Enum enumValue)
        {
			Logger.LogDebug(">>> Entering GetEnumDescription(enumValue={EnumValue})", enumValue);
            var fi = enumValue.GetType().GetField(enumValue.ToString());
            var descriptionAttributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
			Logger.LogDebug("GetEnumDescription(enumValue={EnumValue}): descriptionAttributes → {DescriptionAttributes}", enumValue, descriptionAttributes);
			Logger.LogDebug("GetEnumDescription(enumValue={EnumValue}): fi → {Fi}", enumValue, fi);

            if (descriptionAttributes.Length > 0)
            {
			Logger.LogDebug("<<< Exiting GetEnumDescription(enumValue={EnumValue})", enumValue);
                return descriptionAttributes[0].Description;
            }

            return enumValue.ToString();
        }

        /// <summary>Gets the id of the group folder.</summary>
        /// <param name="folderId">The id of the folder.</param>
        /// <returns>An integer representing the root folder id for the group.</returns>
        public static int GetFolderGroupId(int folderId)
        {
			Logger.LogDebug(">>> Entering GetFolderGroupId(folderId={FolderId})", folderId);
            var folder = FolderManager.Instance.GetFolder(folderId);
            if (folder is null)
			Logger.LogDebug("GetFolderGroupId(folderId={FolderId}): folder → {Folder}", folderId, folder);
            {
                return -1;
            }

            var folderPath = folder.DisplayPath;

            if (!folderPath.StartsWith(Constants.GroupFolderPathStart))
            {
                return -1;
            }

            var prefixLength = Constants.GroupFolderPathStart.Length;
            var folderGroupIdString = folderPath.Substring(prefixLength - 1);
			Logger.LogDebug("GetFolderGroupId(folderId={FolderId}): folderGroupIdString → {FolderGroupIdString}", folderId, folderGroupIdString);
            folderGroupIdString = folderGroupIdString.Substring(0, folderGroupIdString.IndexOf("/", StringComparison.Ordinal));

            if (!int.TryParse(folderGroupIdString, out var folderGroupId))
            {
			Logger.LogDebug("<<< Exiting GetFolderGroupId(folderId={FolderId})", folderId);
                return -1;
            }

            return folderGroupId;
        }
    }
}
