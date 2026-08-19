// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
    using System;
    using System.Globalization;
    using System.IO;
	using Microsoft.Extensions.Logging;

    public static class FolderHelper
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private const int ByteOffset = 255;
        private const int SubfolderSeedLength = 2;

        public static string GetUserFolderPath(int userId)
        {
			_logger.LogDebug(">>> Entering GetUserFolderPath(userId={UserId})", userId);
            var rootFolder = GetUserFolderPathRoot(userId);
            var subFolder = GetUserFolderPathSubFolder(userId);
			_logger.LogDebug("GetUserFolderPath(userId={UserId}): subFolder → {SubFolder}", userId, subFolder);
			_logger.LogDebug("GetUserFolderPath(userId={UserId}): rootFolder → {RootFolder}", userId, rootFolder);

            var fullPath = Path.Combine(Path.Combine(rootFolder, subFolder), userId.ToString(CultureInfo.InvariantCulture));
			_logger.LogDebug("GetUserFolderPath(userId={UserId}): fullPath → {FullPath}", userId, fullPath);

            return string.Format("Users/{0}/", fullPath.Replace(@"\", "/"));
        }

        private static string GetUserFolderPathRoot(int userId)
        {
			_logger.LogDebug(">>> Entering GetUserFolderPathRoot(userId={UserId})", userId);
			_logger.LogDebug("<<< Exiting GetUserFolderPathRoot(userId={UserId})", userId);
            return (Convert.ToInt32(userId) & ByteOffset).ToString("000");
        }

        private static string GetUserFolderPathSubFolder(int userId)
        {
			_logger.LogDebug(">>> Entering GetUserFolderPathSubFolder(userId={UserId})", userId);
			_logger.LogDebug("<<< Exiting GetUserFolderPathSubFolder(userId={UserId})", userId);
            return userId.ToString("00").Substring(userId.ToString("00").Length - SubfolderSeedLength, SubfolderSeedLength);
        }
    }
}
