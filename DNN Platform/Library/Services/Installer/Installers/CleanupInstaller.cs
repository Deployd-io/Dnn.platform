// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileSystemGlobbing;

    /// <summary>The CleanupInstaller cleans up (removes) files from previous versions.</summary>
    public class CleanupInstaller : FileInstaller
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CleanupInstaller));

        private readonly IList<string> folders = new List<string>();
        private readonly IApplicationStatusInfo applicationStatusInfo;
        private readonly IFileSystemUtils fileSystemUtils;

        private string fileName;
        private string glob;

        /// <summary>Initializes a new instance of the <see cref="CleanupInstaller"/> class.</summary>
        public CleanupInstaller()
            : this(
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IFileSystemUtils>())
        {
		Logger.LogDebug(">>> Entering CleanupInstaller()");
		Logger.LogDebug("<<< Exiting CleanupInstaller()");
		Logger.LogDebug(">>> Entering CleanupInstaller(applicationStatusInfo={ApplicationStatusInfo},fileSystemUtils={FileSystemUtils})", applicationStatusInfo, fileSystemUtils);
		Logger.LogDebug("CleanupInstaller(applicationStatusInfo={ApplicationStatusInfo},fileSystemUtils={FileSystemUtils}): applicationStatusInfo → {ApplicationStatusInfo1}", applicationStatusInfo, fileSystemUtils, applicationStatusInfo);
        }

        /// <summary>Initializes a new instance of the <see cref="CleanupInstaller"/> class.</summary>
        /// <param name="applicationStatusInfo">An instance of <see cref="IApplicationStatusInfo"/>.</param>
        /// <param name="fileSystemUtils">An instance of <see cref="IFileSystemUtils"/>.</param>
        internal CleanupInstaller(
            IApplicationStatusInfo applicationStatusInfo,
            IFileSystemUtils fileSystemUtils)
        {
            this.applicationStatusInfo = applicationStatusInfo
                ?? throw new ArgumentNullException(nameof(applicationStatusInfo));
				Logger.LogDebug("CleanupInstaller(applicationStatusInfo={ApplicationStatusInfo},fileSystemUtils={FileSystemUtils}): this.applicationStatusInfo → {Arg2}", applicationStatusInfo, fileSystemUtils, this.applicationStatusInfo);

            this.fileSystemUtils = fileSystemUtils
                ?? throw new ArgumentNullException(nameof(fileSystemUtils));
				Logger.LogDebug("CleanupInstaller(applicationStatusInfo={ApplicationStatusInfo},fileSystemUtils={FileSystemUtils}): this.fileSystemUtils → {Arg2}", applicationStatusInfo, fileSystemUtils, this.fileSystemUtils);
				Logger.LogDebug("CleanupInstaller(applicationStatusInfo={ApplicationStatusInfo},fileSystemUtils={FileSystemUtils}): fileSystemUtils → {FileSystemUtils1}", applicationStatusInfo, fileSystemUtils, fileSystemUtils);
				Logger.LogDebug("<<< Exiting CleanupInstaller(applicationStatusInfo={ApplicationStatusInfo},fileSystemUtils={FileSystemUtils})", applicationStatusInfo, fileSystemUtils);
        }

        /// <inheritdoc />
        public override string AllowableFiles => "*";

        /// <summary>Gets the list of folders to clean up.</summary>
        protected IList<string> Folders => this.folders;

        /// <remarks>In the case of Clenup this is not neccessary.</remarks>
        /// <inheritdoc />
        public override void Commit()
        {
			Logger.LogDebug(">>> Entering Commit()");
            // Do nothing
            base.Commit();
			Logger.LogDebug("<<< Exiting Commit()");
        }

        /// <remarks>The Install method cleans up the files.</remarks>
        /// <inheritdoc />
        public override void Install()
        {
			Logger.LogDebug(">>> Entering Install()");
            try
            {
                bool bSuccess = true;
				Logger.LogDebug("Install(): bSuccess → {BSuccess}", bSuccess);
                if (string.IsNullOrEmpty(this.fileName) && string.IsNullOrEmpty(this.glob))
                {
                    // No attribute: use the xml files definition.
                    foreach (InstallFile file in this.Files)
                    {
                        bSuccess = this.CleanupFile(file);
                        if (!bSuccess)
                        {
                            break;
                        }
                    }

                    foreach (string folder in this.Folders)
                    {
                        this.CleanupFolder(folder);
                    }
                }
                else if (!string.IsNullOrEmpty(this.fileName))
                {
                    // Cleanup file provided: clean each file in the cleanup text file line one by one.
                    bSuccess = this.ProcessCleanupFile();
                }
                else if (!string.IsNullOrEmpty(this.glob))
                {
                    // A globbing pattern was provided, use it to find the files and delete what matches.
                    bSuccess = this.ProcessGlob();
                }

                this.Completed = bSuccess;
				Logger.LogDebug("Install(): this.Completed → {Arg0}", this.Completed);
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(Util.EXCEPTION + " - " + ex.Message);
            }
			Logger.LogDebug("<<< Exiting Install()");
        }

        /// <inheritdoc />
        public override void ReadManifest(XPathNavigator manifestNav)
        {
			Logger.LogDebug(">>> Entering ReadManifest(manifestNav={ManifestNav})", manifestNav);
            this.fileName = Util.ReadAttribute(manifestNav, "fileName");
            this.glob = Util.ReadAttribute(manifestNav, "glob");

            foreach (XPathNavigator nav in manifestNav.Select("folder"))
            {
                this.ProcessFolder(nav);
            }

            base.ReadManifest(manifestNav);
			Logger.LogDebug("<<< Exiting ReadManifest(manifestNav={ManifestNav})", manifestNav);
        }

        /// <remarks>There is no uninstall for this component.</remarks>
        /// <inheritdoc />
        public override void UnInstall()
        {
		Logger.LogDebug(">>> Entering UnInstall()");
		Logger.LogDebug("<<< Exiting UnInstall()");
        }

        /// <summary>Adds a folder path to the list.</summary>
        /// <param name="path">The folder path.</param>
        internal void ProcessFolder(string path)
        {
			Logger.LogDebug(">>> Entering ProcessFolder(path={Path})", path);
            this.Folders.Add(path);
			Logger.LogDebug("<<< Exiting ProcessFolder(path={Path})", path);
        }

        /// <summary>Validates a folder path for cleanup.</summary>
        /// <param name="path">The folder path to validate.</param>
        /// <param name="validPath">The sanitized absolute folder path after validation.</param>
        /// <returns>Whether or not the folder path is valid.</returns>
        internal bool IsValidFolderPath(string path, out string validPath)
        {
			Logger.LogDebug(">>> Entering IsValidFolderPath(path={Path})", path);
            validPath = null;

			Logger.LogDebug("IsValidFolderPath(path={Path}): validPath → {ValidPath}", path, validPath);
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            if (Path.IsPathRooted(path))
            {
                return false; // no rooted paths
            }

            if (path.StartsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                return false; // no absolute paths
            }

            if (path.Contains("..", StringComparison.Ordinal))
            {
                return false; // no relative paths outside the app root
            }

            path = path.Trim();
			Logger.LogDebug("IsValidFolderPath(path={Path}): path → {Path1}", path, path);

            // normalize slashes
            var appPath = Path.GetFullPath(this.applicationStatusInfo.ApplicationMapPath);
			Logger.LogDebug("IsValidFolderPath(path={Path}): appPath → {AppPath}", path, appPath);

            // ensure trailing slash
            appPath = appPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            try
            {
                validPath = Path.GetFullPath(Path.Combine(appPath, path));
            }
            catch
            {
				Logger.LogError("Exception in IsValidFolderPath(path={Path}): {Arg1}", path, ex.Message);
                return false; // no malformed paths
            }

            validPath = validPath.TrimEnd(Path.DirectorySeparatorChar);

            if (!validPath.StartsWith(appPath, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(validPath, appPath, StringComparison.OrdinalIgnoreCase))
            {
                validPath = null;
			Logger.LogDebug("<<< Exiting IsValidFolderPath(path={Path})", path);
                return false; // not the app root or paths outside the app root
            }

            return true;
        }

        /// <summary>The CleanupFile method cleans up a single file.</summary>
        /// <param name="insFile">The InstallFile to clean up.</param>
        /// <returns><see langword="true"/> if the file was deleted, otherwise <see langword="false"/>.</returns>
        protected bool CleanupFile(InstallFile insFile)
        {
			Logger.LogDebug(">>> Entering CleanupFile(insFile={InsFile})", insFile);
            try
            {
                // Backup File
                if (File.Exists(this.PhysicalBasePath + insFile.FullName))
                {
                    Util.BackupFile(insFile, this.PhysicalBasePath, this.Log);
                }

                // Delete file
                Util.DeleteFile(insFile, this.PhysicalBasePath, this.Log);
                return true;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);

				Logger.LogDebug("<<< Exiting CleanupFile(insFile={InsFile})", insFile);
                return false;
            }
        }

        /// <summary>Deletes all empty folders beneath a given root folder and the root folder itself as well if empty.</summary>
        /// <param name="path">The root folder path.</param>
        protected virtual void CleanupFolder(string path)
        {
			Logger.LogDebug(">>> Entering CleanupFolder(path={Path})", path);
            try
            {
                if (this.IsValidFolderPath(path, out string validPath))
                {
                    this.fileSystemUtils.DeleteEmptyFoldersRecursive(validPath);
                }
                else
                {
                    Logger.Warn($"Ignoring invalid cleanup folder path '{path}' in package '{this.Package?.Name}'.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
			Logger.LogDebug("<<< Exiting CleanupFolder(path={Path})", path);
        }

        /// <summary>The ProcessFile method determines what to do with parsed "file" node.</summary>
        /// <param name="file">The file represented by the node.</param>
        /// <param name="nav">The XPathNavigator representing the node.</param>
        protected override void ProcessFile(InstallFile file, XPathNavigator nav)
        {
			Logger.LogDebug(">>> Entering ProcessFile(file={File},nav={Nav})", file, nav);
            if (file != null)
            {
                this.Files.Add(file);
            }
			Logger.LogDebug("<<< Exiting ProcessFile(file={File},nav={Nav})", file, nav);
        }

        /// <summary>Determines what to do with the parsed "folder" node.</summary>
        /// <param name="nav">The XPathNavigator representing the node.</param>
        protected virtual void ProcessFolder(XPathNavigator nav)
        {
			Logger.LogDebug(">>> Entering ProcessFolder(nav={Nav})", nav);
            var pathNav = nav.SelectSingleNode("path");

			Logger.LogDebug("ProcessFolder(nav={Nav}): pathNav → {PathNav}", nav, pathNav);
            if (pathNav != null)
            {
                this.ProcessFolder(pathNav.Value);
            }
			Logger.LogDebug("<<< Exiting ProcessFolder(nav={Nav})", nav);
        }

        /// <inheritdoc />
        protected override InstallFile ReadManifestItem(XPathNavigator nav, bool checkFileExists)
        {
			Logger.LogDebug(">>> Entering ReadManifestItem(nav={Nav},checkFileExists={CheckFileExists})", nav, checkFileExists);
			Logger.LogDebug("<<< Exiting ReadManifestItem(nav={Nav},checkFileExists={CheckFileExists})", nav, checkFileExists);
            return base.ReadManifestItem(nav, false);
        }

        /// <inheritdoc />
        protected override void RollbackFile(InstallFile installFile)
        {
			Logger.LogDebug(">>> Entering RollbackFile(installFile={InstallFile})", installFile);
            if (File.Exists(installFile.BackupFileName))
            {
                Util.RestoreFile(installFile, this.PhysicalBasePath, this.Log);
            }
			Logger.LogDebug("<<< Exiting RollbackFile(installFile={InstallFile})", installFile);
        }

        private bool ProcessCleanupFile()
        {
			Logger.LogDebug(">>> Entering ProcessCleanupFile()");
            this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.CLEANUP_Processing, this.Version.ToString(3)));
            try
            {
                var strListFile = Path.Combine(this.Package.InstallerInfo.TempInstallFolder, this.fileName);
				Logger.LogDebug("ProcessCleanupFile(): strListFile → {StrListFile}", strListFile);
                if (File.Exists(strListFile))
                {
                    FileSystemUtils.DeleteFiles(File.ReadAllLines(strListFile));
                }
            }
            catch (Exception ex)
            {
                this.Log.AddWarning(string.Format(CultureInfo.InvariantCulture, Util.CLEANUP_ProcessError, ex.Message));

                // DNN-9202: MUST NOT fail installation when cleanup files deletion fails
                // return false;
            }

            this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.CLEANUP_ProcessComplete, this.Version.ToString(3)));
			Logger.LogDebug("<<< Exiting ProcessCleanupFile()");
            return true;
        }

        private bool ProcessGlob()
        {
			Logger.LogDebug(">>> Entering ProcessGlob()");
            this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.CLEANUP_Processing, this.Version.ToString(3)));
            try
            {
                if (this.glob.Contains(".."))
                {
                    this.Log.AddWarning(Util.EXCEPTION + " - " + Util.EXCEPTION_GlobDotDotNotSupportedInCleanup);
                }
                else
                {
                    var globs = new Matcher(StringComparison.InvariantCultureIgnoreCase);
					Logger.LogDebug("ProcessGlob(): globs → {Globs}", globs);
                    globs.AddIncludePatterns(this.glob.Split(';'));
                    var files = globs.GetResultsInFullPath(Globals.ApplicationMapPath).ToArray();
					Logger.LogDebug("ProcessGlob(): files → {Files}", files);
                    FileSystemUtils.DeleteFiles(files);
                }
            }
            catch (Exception ex)
            {
                this.Log.AddWarning(string.Format(CultureInfo.InvariantCulture, Util.CLEANUP_ProcessError, ex.Message));
            }

            this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.CLEANUP_ProcessComplete, this.Version.ToString(3)));
			Logger.LogDebug("<<< Exiting ProcessGlob()");
            return true;
        }
    }
}
