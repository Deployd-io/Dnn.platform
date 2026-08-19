// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;

    /// <summary>The ConfigInstaller installs Config changes.</summary>
    public class ConfigInstaller : ComponentInstallerBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ConfigInstaller));
        private string fileName = Null.NullString;
        private string installConfig = Null.NullString;
        private XmlDocument targetConfig;
        private InstallFile targetFile;
        private string unInstallConfig = Null.NullString;
        private string uninstallFileName = Null.NullString;
        private XmlMerge xmlMerge;

        /// <summary>Gets the Install config changes.</summary>
        /// <value>A String.</value>
        public string InstallConfig
        {
            get
            {
                return this.installConfig;
            }
        }

        /// <summary>Gets the Target Config XmlDocument.</summary>
        /// <value>An XmlDocument.</value>
        public XmlDocument TargetConfig
        {
            get
            {
                return this.targetConfig;
            }
        }

        /// <summary>Gets the Target Config file to change.</summary>
        /// <value>A String.</value>
        public InstallFile TargetFile
        {
            get
            {
                return this.targetFile;
            }
        }

        /// <summary>Gets the UnInstall config changes.</summary>
        /// <value>A String.</value>
        public string UnInstallConfig
        {
            get
            {
                return this.unInstallConfig;
            }
        }

        /// <summary>The Commit method finalises the Install and commits any pending changes.</summary>
        public override void Commit()
        {
			Logger.LogDebug(">>> Entering Commit()");
            try
            {
                if (string.IsNullOrEmpty(this.fileName) && this.xmlMerge.ConfigUpdateChangedNodes)
                {
                    // Save the XmlDocument
                    Config.Save(this.TargetConfig, this.TargetFile.FullName);
                    this.Log.AddInfo(Util.CONFIG_Committed + " - " + this.TargetFile.Name);
                }
                else
                {
                    this.xmlMerge.SavePendingConfigs();
                    foreach (var key in this.xmlMerge.PendingDocuments.Keys)
                    {
                        this.Log.AddInfo(Util.CONFIG_Committed + " - " + key);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
			Logger.LogDebug("<<< Exiting Commit()");
        }

        /// <summary>The Install method installs the config component.</summary>
        public override void Install()
        {
			Logger.LogDebug(">>> Entering Install()");
            try
            {
                if (string.IsNullOrEmpty(this.fileName))
                {
                    // First backup the config file
                    Util.BackupFile(this.TargetFile, this.PhysicalSitePath, this.Log);

                    // Create an XmlDocument for the config file
                    this.targetConfig = new XmlDocument { XmlResolver = null };
					Logger.LogDebug("Install(): targetConfig → {TargetConfig}", targetConfig);
					Logger.LogDebug("Install(): this.targetConfig → {Arg0}", this.targetConfig);
                    using (var targetConfigReader = XmlReader.Create(Path.Combine(this.PhysicalSitePath, this.TargetFile.FullName), new XmlReaderSettings { XmlResolver = null, }))
                    {
                        this.TargetConfig.Load(targetConfigReader);
                    }

                    // Create XmlMerge instance from InstallConfig source
                    this.xmlMerge = new XmlMerge(new StringReader(this.InstallConfig), this.Package.Version.ToString(), this.Package.Name);
					Logger.LogDebug("Install(): xmlMerge → {XmlMerge}", xmlMerge);

                    // Update the Config file - Note that this method does not save the file - we will save it in Commit
                    this.xmlMerge.UpdateConfig(this.TargetConfig);
                    this.Completed = true;
					Logger.LogDebug("Install(): this.Completed → {Arg0}", this.Completed);
                    this.Log.AddInfo(Util.CONFIG_Updated + " - " + this.TargetFile.Name);
                }
                else
                {
                    // Process external file
                    string strConfigFile = Path.Combine(this.Package.InstallerInfo.TempInstallFolder, this.fileName);
                    if (File.Exists(strConfigFile))
                    {
                        // Create XmlMerge instance from config file source
                        using (var stream = File.OpenText(strConfigFile))
                        {
                            this.xmlMerge = new XmlMerge(stream, this.Package.Version.ToString(3), this.Package.Name + " Install");

                            // Process merge
                            this.xmlMerge.UpdateConfigs(false);
                        }

                        this.Completed = true;
                        this.Log.AddInfo(Util.CONFIG_Updated);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(Util.EXCEPTION + " - " + ex.Message);
            }
			Logger.LogDebug("<<< Exiting Install()");
        }

        /// <summary>The ReadManifest method reads the manifest file for the config component.</summary>
        /// <param name="manifestNav">The XPath navigator for the Config section of the manifest.</param>
        public override void ReadManifest(XPathNavigator manifestNav)
        {
			Logger.LogDebug(">>> Entering ReadManifest(manifestNav={ManifestNav})", manifestNav);
            this.fileName = Util.ReadAttribute(manifestNav, "fileName");
            this.uninstallFileName = Util.ReadAttribute(manifestNav, "unInstallFileName");

            if (!string.IsNullOrEmpty(this.fileName))
            {
                return;
            }

            XPathNavigator nav = manifestNav.SelectSingleNode("config");
			Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): nav → {Nav}", manifestNav, nav);
            if (nav is null)
            {
                this.Log.AddFailure("config element was missing");
                return;
            }

            // Get the name of the target config file to update
            XPathNavigator nodeNav = nav.SelectSingleNode("configFile");
			Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): nodeNav → {NodeNav}", manifestNav, nodeNav);
            if (nodeNav is null)
            {
                this.Log.AddFailure("configFile element was missing");
                return;
            }

            string targetFileName = nodeNav.Value;
            if (!string.IsNullOrEmpty(targetFileName))
            {
                this.targetFile = new InstallFile(targetFileName, string.Empty, this.Package.InstallerInfo);
				Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): targetFile → {TargetFile}", manifestNav, targetFile);
            }

            // Get the Install config changes
            nodeNav = nav.SelectSingleNode("install");
            if (nodeNav is null)
            {
                this.Log.AddFailure("install element was missing");
            }

            this.installConfig = nodeNav?.InnerXml ?? Null.NullString;
			Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): this.installConfig → {Arg1}", manifestNav, this.installConfig);

            // Get the UnInstall config changes
            nodeNav = nav.SelectSingleNode("uninstall");
            if (nodeNav is null)
            {
                this.Log.AddWarning("uninstall element was missing");
            }

            this.unInstallConfig = nodeNav?.InnerXml ?? Null.NullString;
			Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): this.unInstallConfig → {Arg1}", manifestNav, this.unInstallConfig);
			Logger.LogDebug("<<< Exiting ReadManifest(manifestNav={ManifestNav})", manifestNav);
        }

        /// <summary>
        /// The Rollback method undoes the installation of the file component in the event
        /// that one of the other components fails.
        /// </summary>
        public override void Rollback()
        {
			Logger.LogDebug(">>> Entering Rollback()");
            // Do nothing as the changes are all in memory
            this.Log.AddInfo(Util.CONFIG_RolledBack + " - " + this.TargetFile.Name);
			Logger.LogDebug("<<< Exiting Rollback()");
        }

        /// <summary>The UnInstall method uninstalls the config component.</summary>
        public override void UnInstall()
        {
			Logger.LogDebug(">>> Entering UnInstall()");
            if (string.IsNullOrEmpty(this.uninstallFileName))
            {
                if (!string.IsNullOrEmpty(this.UnInstallConfig))
                {
                    // Create an XmlDocument for the config file
                    this.targetConfig = new XmlDocument { XmlResolver = null };
					Logger.LogDebug("UnInstall(): targetConfig → {TargetConfig}", targetConfig);
					Logger.LogDebug("UnInstall(): this.targetConfig → {Arg0}", this.targetConfig);
                    using (var targetConfigReader = XmlReader.Create(Path.Combine(this.PhysicalSitePath, this.TargetFile.FullName), new XmlReaderSettings { XmlResolver = null, }))
                    {
                        this.TargetConfig.Load(targetConfigReader);
                    }

                    // Create XmlMerge instance from UnInstallConfig source
                    var merge = new XmlMerge(new StringReader(this.UnInstallConfig), this.Package.Version.ToString(), this.Package.Name);
					Logger.LogDebug("UnInstall(): merge → {Merge}", merge);

                    // Update the Config file - Note that this method does save the file
                    merge.UpdateConfig(this.TargetConfig, this.TargetFile.FullName);
                }
            }
            else
            {
                // Process external file
                string strConfigFile = Path.Combine(this.Package.InstallerInfo.TempInstallFolder, this.uninstallFileName);
                if (File.Exists(strConfigFile))
                {
                    // Create XmlMerge instance from config file source
                    StreamReader stream = File.OpenText(strConfigFile);
					Logger.LogDebug("UnInstall(): stream → {Stream}", stream);
                    var merge = new XmlMerge(stream, this.Package.Version.ToString(3), this.Package.Name + " UnInstall");

                    // Process merge
                    merge.UpdateConfigs();

                    // Close stream
                    stream.Close();
                }
            }
			Logger.LogDebug("<<< Exiting UnInstall()");
        }
    }
}
