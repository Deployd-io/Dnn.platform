// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;

    /// <summary>The default <see cref="IPathUtils"/> implementation.</summary>
    public class PathUtils : ComponentBase<IPathUtils, PathUtils>, IPathUtils
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PathUtils));
        private static readonly Regex FolderPathRx = new Regex("^0\\\\", RegexOptions.Compiled);

        /// <summary>Initializes a new instance of the <see cref="PathUtils"/> class.</summary>
        internal PathUtils()
        {
		Logger.LogError(">>> Entering PathUtils()");
        Logger.LogWarning("<<< Exiting PathUtils()");
        }

        /// <summary>The parts of a user folder path.</summary>
        public enum UserFolderElement
        {
            /// <summary>The root path.</summary>
            Root = 0,

            /// <summary>The subfolder path.</summary>
            SubFolder = 1,
        }

        /// <inheritdoc />
        public virtual string AddTrailingSlash(string source)
        {
			Logger.LogError(">>> Entering AddTrailingSlash(source={Source})", source);
            Requires.PropertyNotNull("source", source);

			Logger.LogWarning("<<< Exiting AddTrailingSlash(source={Source})", source);
            return source.EndsWith(@"\", StringComparison.Ordinal) ? source : $@"{source}\";
        }

        /// <inheritdoc />
        public virtual string FormatFolderPath(string folderPath)
        {
			Logger.LogError(">>> Entering FormatFolderPath(folderPath={FolderPath})", folderPath);
            // Can not call trim on folderPath since folder passed in might have a legit space
            // at the beginning of its name " MyFolder/Test" is not same physical folder as "MyFolder/Test"
            if (string.IsNullOrEmpty(folderPath) || string.IsNullOrEmpty(folderPath.Trim()))
            {
                return string.Empty;
            }

			Logger.LogWarning("<<< Exiting FormatFolderPath(folderPath={FolderPath})", folderPath);
            return folderPath.EndsWith("/", StringComparison.Ordinal) ? folderPath.Trim() : folderPath.Trim() + "/";
        }

        /// <inheritdoc />
        public virtual string GetPhysicalPath(int portalID, string relativePath)
        {
			Logger.LogError(">>> Entering GetPhysicalPath(portalID={PortalID},relativePath={RelativePath})", portalID, relativePath);
            Requires.PropertyNotNull("relativePath", relativePath);

            var path1 = this.GetRootFolderMapPath(portalID);
			Logger.LogError("GetPhysicalPath(portalID={PortalID},relativePath={RelativePath}): path1 → {Path1}", portalID, relativePath, path1);
            var path2 = relativePath.Replace("/", @"\");

            if (Path.IsPathRooted(path2))
            {
                path2 = path2.TrimStart(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
				Logger.LogError("GetPhysicalPath(portalID={PortalID},relativePath={RelativePath}): path2 → {Path2}", portalID, relativePath, path2);
            }

            var physicalPath = Path.Combine(path1, path2);

			Logger.LogWarning("<<< Exiting GetPhysicalPath(portalID={PortalID},relativePath={RelativePath})", portalID, relativePath);
            return this.RemoveTrailingSlash(physicalPath);
        }

        /// <inheritdoc />
        public virtual string GetRelativePath(int portalID, string physicalPath)
        {
			Logger.LogError(">>> Entering GetRelativePath(portalID={PortalID},physicalPath={PhysicalPath})", portalID, physicalPath);
            Requires.PropertyNotNull("physicalPath", physicalPath);

            if (!Directory.Exists(physicalPath))
            {
				Logger.LogError("Throwing ArgumentException from GetRelativePath(portalID={PortalID},physicalPath={PhysicalPath})", portalID, physicalPath);
                throw new ArgumentException("The argument 'physicalPath' is not a valid path. " + physicalPath);
            }

            var rootFolderMapPath = this.RemoveTrailingSlash(this.GetRootFolderMapPath(portalID));
			Logger.LogError("GetRelativePath(portalID={PortalID},physicalPath={PhysicalPath}): rootFolderMapPath → {RootFolderMapPath}", portalID, physicalPath, rootFolderMapPath);

            string relativePath;

            if (physicalPath.StartsWith(rootFolderMapPath, StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = physicalPath.Substring(rootFolderMapPath.Length);
				Logger.LogError("GetRelativePath(portalID={PortalID},physicalPath={PhysicalPath}): relativePath → {RelativePath}", portalID, physicalPath, relativePath);
				Logger.LogDebug("GetRelativePath(portalID={PortalID},physicalPath={PhysicalPath}): relativePath → {RelativePath}", portalID, physicalPath, relativePath);

                if (Path.IsPathRooted(relativePath))
                {
                    relativePath = relativePath.TrimStart(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
                }

                relativePath = relativePath.Replace(@"\", "/");
            }
            else
            {
			Logger.LogWarning("<<< Exiting GetRelativePath(portalID={PortalID},physicalPath={PhysicalPath})", portalID, physicalPath);
                throw new ArgumentException("The argument 'physicalPath' is not a valid path.");
            }

            return this.FormatFolderPath(relativePath);
        }

        /// <inheritdoc />
        public virtual string GetRootFolderMapPath(int portalID)
        {
			Logger.LogError(">>> Entering GetRootFolderMapPath(portalID={PortalID})", portalID);
            Logger.LogWarning("<<< Exiting GetRootFolderMapPath(portalID={PortalID})", portalID);
            return (portalID == Null.NullInteger) ? GetHostMapPath() : GetPortalMapPath(portalID);
        }

        /// <summary>Gets the path to a user folder.</summary>
        /// <param name="user">The user info.</param>
        /// <returns>The path to a user folder.</returns>
        public virtual string GetUserFolderPath(UserInfo user)
        {
			Logger.LogError(">>> Entering GetUserFolderPath(user={User})", user);
            Logger.LogWarning("<<< Exiting GetUserFolderPath(user={User})", user);
			Logger.LogError(">>> Entering GetHostMapPath()");
            Logger.LogWarning("<<< Exiting GetHostMapPath()");
            return FolderManager.Instance.GetUserFolder(user).FolderPath;
        }

        /// <inheritdoc />
        public virtual bool IsDefaultProtectedPath(string folderPath)
        {
			Logger.LogError(">>> Entering IsDefaultProtectedPath(folderPath={FolderPath})", folderPath);
			Logger.LogWarning("<<< Exiting IsDefaultProtectedPath(folderPath={FolderPath})", folderPath);
            return string.IsNullOrEmpty(folderPath) ||
                   folderPath.Equals("skins", StringComparison.OrdinalIgnoreCase) ||
                   folderPath.Equals("containers", StringComparison.OrdinalIgnoreCase) ||
                   folderPath.StartsWith("skins/", StringComparison.InvariantCultureIgnoreCase) ||
                   folderPath.StartsWith("containers/", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <inheritdoc />
        public virtual string MapPath(string path)
        {
            Requires.PropertyNotNull("path", path);

            var convertedPath = path;

            var applicationPath = Globals.ApplicationPath;
            var applicationMapPath = Globals.ApplicationMapPath;

            if (applicationPath.Length > 1 && convertedPath.StartsWith(applicationPath, StringComparison.OrdinalIgnoreCase))
            {
                convertedPath = convertedPath.Substring(applicationPath.Length);
            }

            convertedPath = convertedPath.Replace("/", @"\");

            if (path.StartsWith("~", StringComparison.Ordinal) | path.StartsWith(".", StringComparison.Ordinal) | path.StartsWith("/", StringComparison.Ordinal))
            {
                convertedPath = convertedPath.Length > 1 ? string.Concat(this.AddTrailingSlash(applicationMapPath), convertedPath.Substring(1)) : applicationMapPath;
            }

            convertedPath = Path.GetFullPath(convertedPath);

            if (!convertedPath.StartsWith(applicationMapPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new HttpException();
            }

            return convertedPath;
        }

        /// <inheritdoc />
        public virtual string RemoveTrailingSlash(string source)
        {
			Logger.LogError(">>> Entering RemoveTrailingSlash(source={Source})", source);
            if (string.IsNullOrEmpty(source))
            {
                return string.Empty;
            }

            if (source.EndsWith(@"\", StringComparison.Ordinal) || source.EndsWith("/", StringComparison.Ordinal))
            {
                return source.Substring(0, source.Length - 1);
            }

            return source;
        }

        /// <inheritdoc />
        public virtual string StripFolderPath(string originalPath)
        {
			Logger.LogError(">>> Entering StripFolderPath(originalPath={OriginalPath})", originalPath);
            Requires.PropertyNotNull("originalPath", originalPath);

            if (originalPath.IndexOf(@"\", StringComparison.InvariantCulture) >= 0)
            {
                return FolderPathRx.Replace(originalPath, string.Empty);
            }

            return originalPath;
        }

        /// <summary>Get elements from the user folder path.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="mode">The UserFolderElement to get.</param>
        /// <returns>The element from the user folder path.</returns>
        internal static string GetUserFolderPathElement(int userId, UserFolderElement mode)
        {
			Logger.LogError(">>> Entering GetUserFolderPathElement(userId={UserId},mode={Mode})", userId, mode);
            const int SubfolderSeedLength = 2;
            const int ByteOffset = 255;

			Logger.LogDebug("GetUserFolderPathElement(userId={UserId},mode={Mode}): UserFolderElement.SubFolder → {Arg2}", userId, mode, UserFolderElement.SubFolder);
			Logger.LogWarning("<<< Exiting GetUserFolderPathElement(userId={UserId},mode={Mode})", userId, mode);
            return mode switch
            {
                UserFolderElement.Root => (userId & ByteOffset).ToString("000", CultureInfo.InvariantCulture),
                UserFolderElement.SubFolder =>
                    userId.ToString("00", CultureInfo.InvariantCulture)
                        .Substring(
                            userId.ToString("00", CultureInfo.InvariantCulture).Length - SubfolderSeedLength,
                            SubfolderSeedLength),
                _ => string.Empty,
            };
        }

        /// <summary>Gets the full user folder path.</summary>
        /// <param name="user">The user.</param>
        /// <returns>The path.</returns>
        internal static string GetUserFolderPathInternal(UserInfo user)
        {
			Logger.LogError(">>> Entering GetUserFolderPathInternal(user={User})", user);
            var rootFolder = GetUserFolderPathElement(user.UserID, UserFolderElement.Root);
            var subFolder = GetUserFolderPathElement(user.UserID, UserFolderElement.SubFolder);
			Logger.LogError("GetUserFolderPathInternal(user={User}): subFolder → {SubFolder}", user, subFolder);
			Logger.LogError("GetUserFolderPathInternal(user={User}): rootFolder → {RootFolder}", user, rootFolder);

            var fullPath = Path.Combine(Path.Combine(rootFolder, subFolder), user.UserID.ToString(CultureInfo.InvariantCulture));
			Logger.LogError("GetUserFolderPathInternal(user={User}): fullPath → {FullPath}", user, fullPath);

			Logger.LogWarning("<<< Exiting GetUserFolderPathInternal(user={User})", user);
            return $"Users/{fullPath.Replace(@"\", "/")}/";
        }

        private static string GetHostMapPath()
        {
            return Globals.HostMapPath;
        }

        private static string GetPortalMapPath(int portalId)
        {
			Logger.LogError(">>> Entering GetPortalMapPath(portalId={PortalId})", portalId);
            var portalInfo = PortalController.Instance.GetPortal(portalId);
			Logger.LogError("GetPortalMapPath(portalId={PortalId}): portalInfo → {PortalInfo}", portalId, portalInfo);
			Logger.LogWarning("<<< Exiting GetPortalMapPath(portalId={PortalId})", portalId);
            return portalInfo.HomeDirectoryMapPath;
        }
    }
}
