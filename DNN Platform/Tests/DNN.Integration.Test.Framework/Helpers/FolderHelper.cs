// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
    using System;
    using System.Globalization;
    using System.IO;

    public static class FolderHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const int ByteOffset = 255;
        private const int SubfolderSeedLength = 2;

        public static string GetUserFolderPath(int userId)
        {
			Logger.LogDebug(">>> Entering GetUserFolderPath(userId={UserId})", userId);
            var rootFolder = GetUserFolderPathRoot(userId);
            var subFolder = GetUserFolderPathSubFolder(userId);
			Logger.LogDebug("GetUserFolderPath(userId={UserId}): rootFolder → {RootFolder}", userId, rootFolder);
			Logger.LogDebug("GetUserFolderPath(userId={UserId}): subFolder → {SubFolder}", userId, subFolder);

            var fullPath = Path.Combine(Path.Combine(rootFolder, subFolder), userId.ToString(CultureInfo.InvariantCulture));

			Logger.LogDebug("GetUserFolderPath(userId={UserId}): fullPath → {FullPath}", userId, fullPath);
			Logger.LogDebug("<<< Exiting GetUserFolderPath(userId={UserId})", userId);
            return string.Format("Users/{0}/", fullPath.Replace(@"\", "/"));
        }

        private static string GetUserFolderPathRoot(int userId)
        {
			Logger.LogDebug(">>> Entering GetUserFolderPathRoot(userId={UserId})", userId);
			Logger.LogDebug("<<< Exiting GetUserFolderPathRoot(userId={UserId})", userId);
            return (Convert.ToInt32(userId) & ByteOffset).ToString("000");
        }

        private static string GetUserFolderPathSubFolder(int userId)
        {
			Logger.LogDebug(">>> Entering GetUserFolderPathSubFolder(userId={UserId})", userId);
			Logger.LogDebug("<<< Exiting GetUserFolderPathSubFolder(userId={UserId})", userId);
            return userId.ToString("00").Substring(userId.ToString("00").Length - SubfolderSeedLength, SubfolderSeedLength);
        }
    }
}
