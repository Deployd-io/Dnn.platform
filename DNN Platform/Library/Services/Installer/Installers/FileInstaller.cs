// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;

    /// <summary>The FileInstaller installs File Components to a DotNetNuke site.</summary>
    public class FileInstaller : ComponentInstallerBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileInstaller));
        /// <summary>Gets a value indicating whether the Installer supports Manifest only installs.</summary>
        public override bool SupportsManifestOnlyInstall => Null.NullBoolean;

        /// <summary>Gets or sets a value indicating whether the Packages files are deleted when uninstalling the package.</summary>
        public bool DeleteFiles { get; set; } = Null.NullBoolean;

        /// <summary>Gets the name of the Collection Node (<c>files</c>).</summary>
        protected virtual string CollectionNodeName => "files";

        /// <summary>Gets a Dictionary of Files that are included in this component.</summary>
        protected List<InstallFile> Files { get; } = [];

        /// <summary>Gets the default Path for the file - if not present in the manifest.</summary>
        protected virtual string DefaultPath { get; } = Null.NullString;

        /// <summary>Gets the name of the Item Node (<c>file</c>).</summary>
        protected virtual string ItemNodeName => "file";

        /// <summary>Gets the PhysicalBasePath for the files.</summary>
        protected virtual string PhysicalBasePath
        {
            get
            {
                string physicalBasePath = this.PhysicalSitePath + @"\" + this.BasePath;
                if (!physicalBasePath.EndsWith(@"\", StringComparison.Ordinal))
                {
                    physicalBasePath += @"\";
                }

                return physicalBasePath.Replace("/", @"\");
            }
        }

        /// <summary>Gets or sets the BasePath for the files.</summary>
        /// <remarks>The Base Path is relative to the WebRoot.</remarks>
        protected string BasePath { get; set; }

        /// <summary>The Commit method finalizes the Install and commits any pending changes.</summary>
        /// <remarks>In the case of Files this is not necessary.</remarks>
        public override void Commit()
        {
			Logger.LogDebug(">>> Entering Commit()");
            try
            {
                foreach (InstallFile file in this.Files)
                {
                    this.CommitFile(file);
                }

                this.Completed = true;
				Logger.LogDebug("Commit(): this.Completed → {Arg0}", this.Completed);
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(Util.EXCEPTION + " - " + ex.Message);
            }
			Logger.LogDebug("<<< Exiting Commit()");
        }

        /// <summary>The Install method installs the file component.</summary>
        public override void Install()
        {
			Logger.LogDebug(">>> Entering Install()");
            try
            {
                bool bSuccess = true;
				Logger.LogDebug("Install(): bSuccess → {BSuccess}", bSuccess);
                foreach (InstallFile file in this.Files)
                {
                    bSuccess = this.InstallFile(file);
                    if (!bSuccess)
                    {
                        break;
                    }
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

        /// <summary>The ReadManifest method reads the manifest file for the file component.</summary>
        /// <param name="manifestNav">The XPath navigator for the File section of the manifest.</param>
        public override void ReadManifest(XPathNavigator manifestNav)
        {
			Logger.LogDebug(">>> Entering ReadManifest(manifestNav={ManifestNav})", manifestNav);
            XPathNavigator rootNav = manifestNav.SelectSingleNode(this.CollectionNodeName);
            if (rootNav != null)
            {
                XPathNavigator baseNav = rootNav.SelectSingleNode("basePath");
				Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): baseNav → {BaseNav}", manifestNav, baseNav);
                if (baseNav != null)
                {
                    this.BasePath = baseNav.Value;
					Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): this.BasePath → {Arg1}", manifestNav, this.BasePath);
                }

                this.ReadCustomManifest(rootNav);
                foreach (XPathNavigator nav in rootNav.Select(this.ItemNodeName))
                {
                    this.ProcessFile(this.ReadManifestItem(nav, true), nav);
                }
            }
			Logger.LogDebug("<<< Exiting ReadManifest(manifestNav={ManifestNav})", manifestNav);
        }

        /// <summary>The Rollback method undoes the installation of the file component in the event that one of the other components fails.</summary>
        public override void Rollback()
        {
			Logger.LogDebug(">>> Entering Rollback()");
            try
            {
                foreach (InstallFile file in this.Files)
                {
                    this.RollbackFile(file);
                }

                this.Completed = true;
				Logger.LogDebug("Rollback(): this.Completed → {Arg0}", this.Completed);
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(Util.EXCEPTION + " - " + ex.Message);
            }
			Logger.LogDebug("<<< Exiting Rollback()");
        }

        /// <summary>The UnInstall method uninstalls the file component.</summary>
        public override void UnInstall()
        {
			Logger.LogDebug(">>> Entering UnInstall()");
            try
            {
                foreach (InstallFile file in this.Files)
                {
                    this.UnInstallFile(file);
                }

                this.Completed = true;
				Logger.LogDebug("UnInstall(): this.Completed → {Arg0}", this.Completed);
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(Util.EXCEPTION + " - " + ex.Message);
            }
			Logger.LogDebug("<<< Exiting UnInstall()");
        }

        /// <summary>The CommitFile method commits a single file.</summary>
        /// <param name="insFile">The InstallFile to commit.</param>
        protected virtual void CommitFile(InstallFile insFile)
        {
		Logger.LogDebug(">>> Entering CommitFile(insFile={InsFile})", insFile);
		Logger.LogDebug("<<< Exiting CommitFile(insFile={InsFile})", insFile);
        }

        /// <summary>The DeleteFile method deletes a single file.</summary>
        /// <param name="insFile">The InstallFile to delete.</param>
        protected virtual void DeleteFile(InstallFile insFile)
        {
			Logger.LogDebug(">>> Entering DeleteFile(insFile={InsFile})", insFile);
            if (this.DeleteFiles)
            {
                Util.DeleteFile(insFile, this.PhysicalBasePath, this.Log);
            }
			Logger.LogDebug("<<< Exiting DeleteFile(insFile={InsFile})", insFile);
        }

        /// <summary>The InstallFile method installs a single file.</summary>
        /// <param name="insFile">The InstallFile to install.</param>
        /// <returns><see langword="true"/> if the installation was successful, otherwise <see langword="false"/>.</returns>
        protected virtual bool InstallFile(InstallFile insFile)
        {
			Logger.LogDebug(">>> Entering InstallFile(insFile={InsFile})", insFile);
            try
            {
                // Check the White Lists
                if (this.Package.InstallerInfo.IgnoreWhiteList || Util.IsFileValid(insFile, this.Package.InstallerInfo.AllowableFiles))
                {
                    // Install File
                    if (File.Exists(this.PhysicalBasePath + insFile.FullName))
                    {
                        Util.BackupFile(insFile, this.PhysicalBasePath, this.Log);
                    }

                    // Copy file from temp location
                    Util.CopyFile(insFile, this.PhysicalBasePath, this.Log);
                    return true;
                }
                else
                {
                    this.Log.AddFailure(string.Format(CultureInfo.InvariantCulture, Util.FILE_NotAllowed, insFile.FullName));
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
				Logger.LogDebug("<<< Exiting InstallFile(insFile={InsFile})", insFile);
                return false;
            }
        }

        /// <summary>Gets a flag that determines what type of file this installer supports.</summary>
        /// <param name="type">The type of file being processed.</param>
        /// <returns><see langword="true"/> if this is the correct <paramref name="type"/>, otherwise <see langword="false"/>.</returns>
        protected virtual bool IsCorrectType(InstallFileType type)
        {
			Logger.LogDebug(">>> Entering IsCorrectType(type={Type})", type);
			Logger.LogDebug("<<< Exiting IsCorrectType(type={Type})", type);
            return true;
        }

        /// <summary>The ProcessFile method determines what to do with parsed "file" node.</summary>
        /// <param name="file">The file represented by the node.</param>
        /// <param name="nav">The XPathNavigator representing the node.</param>
        protected virtual void ProcessFile(InstallFile file, XPathNavigator nav)
        {
			Logger.LogDebug(">>> Entering ProcessFile(file={File},nav={Nav})", file, nav);
            if (file != null && this.IsCorrectType(file.Type))
            {
                this.Files.Add(file);

                // Add to the
                this.Package.InstallerInfo.Files[file.FullName.ToLowerInvariant()] = file;
            }
			Logger.LogDebug("<<< Exiting ProcessFile(file={File},nav={Nav})", file, nav);
        }

        /// <summary>The ReadCustomManifest method reads the custom manifest items (that subclasses of FileInstaller may need).</summary>
        /// <param name="nav">The XPathNavigator representing the node.</param>
        protected virtual void ReadCustomManifest(XPathNavigator nav)
        {
		Logger.LogDebug(">>> Entering ReadCustomManifest(nav={Nav})", nav);
		Logger.LogDebug("<<< Exiting ReadCustomManifest(nav={Nav})", nav);
        }

        /// <summary>The ReadManifestItem method reads a single node.</summary>
        /// <param name="nav">The XPathNavigator representing the node.</param>
        /// <param name="checkFileExists">Flag that determines whether a check should be made.</param>
        /// <returns>A new <see cref="Services.Installer.InstallFile"/> instance.</returns>
        protected virtual InstallFile ReadManifestItem(XPathNavigator nav, bool checkFileExists)
        {
			Logger.LogDebug(">>> Entering ReadManifestItem(nav={Nav},checkFileExists={CheckFileExists})", nav, checkFileExists);
            string fileName = Null.NullString;

			Logger.LogDebug("ReadManifestItem(nav={Nav},checkFileExists={CheckFileExists}): fileName → {FileName}", nav, checkFileExists, fileName);
            // Get the path
            XPathNavigator pathNav = nav.SelectSingleNode("path");
			Logger.LogDebug("ReadManifestItem(nav={Nav},checkFileExists={CheckFileExists}): pathNav → {PathNav}", nav, checkFileExists, pathNav);
            if (pathNav == null)
            {
                fileName = this.DefaultPath;
            }
            else
            {
                fileName = pathNav.Value + @"\";
            }

            // Get the name
            XPathNavigator nameNav = nav.SelectSingleNode("name");
			Logger.LogDebug("ReadManifestItem(nav={Nav},checkFileExists={CheckFileExists}): nameNav → {NameNav}", nav, checkFileExists, nameNav);
            if (nameNav != null)
            {
                fileName += nameNav.Value;
            }

            // Get the sourceFileName
            string sourceFileName = Util.ReadElement(nav, "sourceFileName");
            var file = new InstallFile(fileName, sourceFileName, this.Package.InstallerInfo);
			Logger.LogDebug("ReadManifestItem(nav={Nav},checkFileExists={CheckFileExists}): file → {File}", nav, checkFileExists, file);
            if ((!string.IsNullOrEmpty(this.BasePath)) && (this.BasePath.StartsWith("app_code", StringComparison.InvariantCultureIgnoreCase) && file.Type == InstallFileType.Other))
            {
                file.Type = InstallFileType.AppCode;
				Logger.LogDebug("ReadManifestItem(nav={Nav},checkFileExists={CheckFileExists}): file.Type → {Arg2}", nav, checkFileExists, file.Type);
            }

            if (file != null)
            {
                // Set the Version
                string strVersion = XmlUtils.GetNodeValue(nav, "version");
                if (!string.IsNullOrEmpty(strVersion))
                {
                    file.SetVersion(new Version(strVersion));
                }
                else
                {
                    file.SetVersion(this.Package.Version);
                }

                // Set the Action
                string strAction = XmlUtils.GetAttributeValue(nav, "action");
                if (!string.IsNullOrEmpty(strAction))
                {
                    file.Action = strAction;
					Logger.LogDebug("ReadManifestItem(nav={Nav},checkFileExists={CheckFileExists}): file.Action → {Arg2}", nav, checkFileExists, file.Action);
                }

                if (this.InstallMode == InstallMode.Install && checkFileExists && file.Action != "UnRegister")
                {
                    if (File.Exists(file.TempFileName))
                    {
                        this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.FILE_Found, file.Path, file.Name));
                    }
                    else
                    {
                        this.Log.AddFailure(Util.FILE_NotFound + " - " + file.TempFileName);
                    }
                }
            }

			Logger.LogDebug("<<< Exiting ReadManifestItem(nav={Nav},checkFileExists={CheckFileExists})", nav, checkFileExists);
            return file;
        }

        /// <summary>The RollbackFile method rolls back the install of a single file.</summary>
        /// <remarks>For new installs this removes the added file.  For upgrades it restores the backup file created during install.</remarks>
        /// <param name="installFile">The InstallFile to commit.</param>
        protected virtual void RollbackFile(InstallFile installFile)
        {
			Logger.LogDebug(">>> Entering RollbackFile(installFile={InstallFile})", installFile);
            if (File.Exists(installFile.BackupFileName))
            {
                Util.RestoreFile(installFile, this.PhysicalBasePath, this.Log);
            }
            else
            {
                this.DeleteFile(installFile);
            }
			Logger.LogDebug("<<< Exiting RollbackFile(installFile={InstallFile})", installFile);
        }

        /// <summary>The UnInstallFile method unInstalls a single file.</summary>
        /// <param name="unInstallFile">The InstallFile to unInstall.</param>
        protected virtual void UnInstallFile(InstallFile unInstallFile)
        {
			Logger.LogDebug(">>> Entering UnInstallFile(unInstallFile={UnInstallFile})", unInstallFile);
            this.DeleteFile(unInstallFile);
			Logger.LogDebug("<<< Exiting UnInstallFile(unInstallFile={UnInstallFile})", unInstallFile);
        }
    }
}
