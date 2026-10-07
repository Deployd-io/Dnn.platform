// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.UI.Skins;

    /// <summary>The SkinInstaller installs Skin Components to a DotNetNuke site.</summary>
    public class SkinInstaller : FileInstaller
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SkinInstaller));
        private static readonly string[] MessageSeparator = ["<br />",];
        private readonly ArrayList skinFiles = new ArrayList();

        private SkinPackageInfo skinPackage;
        private SkinPackageInfo tempSkinPackage;
        private string skinName = Null.NullString;

        /// <summary>Gets a list of allowable file extensions (in addition to the Host's List).</summary>
        /// <value>A String.</value>
        public override string AllowableFiles
        {
            get
            {
                return "ascx, html, htm, css, xml, js, resx, jpg, jpeg, gif, png";
            }
        }

        /// <summary>Gets the name of the Collection Node ("skinFiles").</summary>
        /// <value>A String.</value>
        protected override string CollectionNodeName
        {
            get
            {
                return "skinFiles";
            }
        }

        /// <summary>Gets the name of the Item Node ("skinFile").</summary>
        /// <value>A String.</value>
        protected override string ItemNodeName
        {
            get
            {
                return "skinFile";
            }
        }

        /// <summary>Gets the PhysicalBasePath for the skin files.</summary>
        /// <value>A String.</value>
        protected override string PhysicalBasePath
        {
            get
            {
                string physicalBasePath = this.RootPath + this.SkinRoot + @"\" + this.skinPackage.SkinName;
                if (!physicalBasePath.EndsWith(@"\", StringComparison.Ordinal))
                {
                    physicalBasePath += @"\";
                }

                return physicalBasePath.Replace("/", @"\");
            }
        }

        /// <summary>Gets the root folder for the Skin.</summary>
        /// <value>A String.</value>
        protected string RootPath
        {
            get
            {
                string rootPath = Null.NullString;
                if (this.Package.InstallerInfo.PortalID == Null.NullInteger && this.Package.PortalID == Null.NullInteger)
                {
                    rootPath = Globals.HostMapPath;
                }
                else
                {
                    rootPath = PortalController.Instance.GetCurrentSettings().HomeSystemDirectoryMapPath;
                }

                return rootPath;
            }
        }

        /// <summary>Gets the collection of Skin Files.</summary>
        /// <value>A List(Of InstallFile).</value>
        protected ArrayList SkinFiles
        {
            get
            {
                return this.skinFiles;
            }
        }

        /// <summary>Gets the name of the SkinName Node ("skinName").</summary>
        /// <value>A String.</value>
        protected virtual string SkinNameNodeName
        {
            get
            {
                return "skinName";
            }
        }

        /// <summary>Gets the RootName of the Skin.</summary>
        /// <value>A String.</value>
        protected virtual string SkinRoot
        {
            get
            {
                return SkinController.RootSkin;
            }
        }

        /// <summary>Gets the Type of the Skin.</summary>
        /// <value>A String.</value>
        protected virtual string SkinType
        {
            get
            {
                return "Skin";
            }
        }

        /// <summary>The Install method installs the skin component.</summary>
        public override void Install()
        {
			Logger.LogError(">>> Entering Install()");
            bool bAdd = Null.NullBoolean;
			Logger.LogError("Install(): bAdd → {BAdd}", bAdd);
            try
            {
                // Attempt to get the Skin Package
                this.tempSkinPackage = SkinController.GetSkinPackage(this.skinPackage.PortalID, this.skinPackage.SkinName, this.SkinType);
				Logger.LogDebug("Install(): tempSkinPackage → {TempSkinPackage}", tempSkinPackage);
                if (this.tempSkinPackage == null)
                {
                    bAdd = true;
                    this.skinPackage.PackageID = this.Package.PackageID;
                }
                else
                {
                    this.skinPackage.SkinPackageID = this.tempSkinPackage.SkinPackageID;
                    if (this.tempSkinPackage.PackageID != this.Package.PackageID)
                    {
                        this.Completed = false;
						Logger.LogDebug("Install(): this.Completed → {Arg0}", this.Completed);
                        this.Log.AddFailure(Util.SKIN_Installed);
                        return;
                    }
                    else
                    {
                        this.skinPackage.PackageID = this.tempSkinPackage.PackageID;
                    }
                }

                this.skinPackage.SkinType = this.SkinType;
                if (bAdd)
                {
                    // Add new skin package
                    this.skinPackage.SkinPackageID = SkinController.AddSkinPackage(this.skinPackage);
                }
                else
                {
                    // Update skin package
                    SkinController.UpdateSkinPackage(this.skinPackage);
                }

                this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.SKIN_Registered, this.skinPackage.SkinName));

                // install (copy the files) by calling the base class
                base.Install();

                // process the list of skin files
                if (this.SkinFiles.Count > 0)
                {
                    this.Log.StartJob(Util.SKIN_BeginProcessing);
                    string strMessage = Null.NullString;
                    var newSkin = new SkinFileProcessor(this.RootPath, this.SkinRoot, this.skinPackage.SkinName);
					Logger.LogError("Install(): newSkin → {NewSkin}", newSkin);
                    foreach (string skinFile in this.SkinFiles)
                    {
                        strMessage += newSkin.ProcessFile(skinFile, SkinParser.Portable);
                        var skinSrc = skinFile.Replace($@"{Globals.HostMapPath}\", "[G]");
                        switch (Path.GetExtension(skinFile))
                        {
                            case ".htm":
								Logger.LogError("Install(): skinSrc → {SkinSrc}", skinSrc);
                                SkinController.AddSkin(this.skinPackage.SkinPackageID, skinSrc.Replace("htm", "ascx"));
                                break;
                            case ".html":
                                SkinController.AddSkin(this.skinPackage.SkinPackageID, skinSrc.Replace("html", "ascx"));
                                break;
                            case ".ascx":
                                SkinController.AddSkin(this.skinPackage.SkinPackageID, skinSrc);
                                break;
                        }
                    }

                    Array arrMessage = strMessage.Split(MessageSeparator, StringSplitOptions.None);
					Logger.LogError("Install(): arrMessage → {ArrMessage}", arrMessage);
                    foreach (string strRow in arrMessage)
                    {
                        this.Log.AddInfo(HtmlUtils.StripTags(strRow, true));
                    }

                    this.Log.EndJob(Util.SKIN_EndProcessing);
                }

                this.Completed = true;
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
        }

        /// <summary>
        /// The Rollback method undoes the installation of the component in the event
        /// that one of the other components fails.
        /// </summary>
        public override void Rollback()
        {
			Logger.LogError(">>> Entering Rollback()");
            // If Temp Skin exists then we need to update the DataStore with this
            if (this.tempSkinPackage == null)
            {
                // No Temp Skin - Delete newly added Skin
                this.DeleteSkinPackage();
            }
            else
            {
                // Temp Skin - Rollback to Temp
                SkinController.UpdateSkinPackage(this.tempSkinPackage);
            }

            // Call base class to prcoess files
            base.Rollback();
			Logger.LogWarning("<<< Exiting Rollback()");
        }

        /// <summary>The UnInstall method uninstalls the skin component.</summary>
        public override void UnInstall()
        {
			Logger.LogError(">>> Entering UnInstall()");
            this.DeleteSkinPackage();

            // Call base class to prcoess files
            base.UnInstall();
			Logger.LogWarning("<<< Exiting UnInstall()");
        }

        /// <summary>The ProcessFile method determines what to do with parsed "file" node.</summary>
        /// <param name="file">The file represented by the node.</param>
        /// <param name="nav">The XPathNavigator representing the node.</param>
        protected override void ProcessFile(InstallFile file, XPathNavigator nav)
        {
			Logger.LogError(">>> Entering ProcessFile(file={File},nav={Nav})", file, nav);
            switch (file.Extension)
            {
                case "htm":
                case "html":
                case "ascx":
                case "css":
                    if (file.Path.IndexOf(Globals.glbAboutPage, StringComparison.InvariantCultureIgnoreCase) < 0)
                    {
                        this.SkinFiles.Add(this.PhysicalBasePath + file.FullName);
                    }

                    break;
            }

            // Call base method to set up for file processing
            base.ProcessFile(file, nav);
			Logger.LogWarning("<<< Exiting ProcessFile(file={File},nav={Nav})", file, nav);
        }

        /// <summary>The ReadCustomManifest method reads the custom manifest items.</summary>
        /// <param name="nav">The XPathNavigator representing the node.</param>
        protected override void ReadCustomManifest(XPathNavigator nav)
        {
			Logger.LogError(">>> Entering ReadCustomManifest(nav={Nav})", nav);
            this.skinPackage = new SkinPackageInfo();
            this.skinPackage.PortalID = this.Package.PortalID;
			Logger.LogDebug("ReadCustomManifest(nav={Nav}): skinPackage → {SkinPackage}", nav, skinPackage);

            // Get the Skin name
            this.skinPackage.SkinName = Util.ReadElement(nav, this.SkinNameNodeName);

            // Call base class
            base.ReadCustomManifest(nav);
			Logger.LogWarning("<<< Exiting ReadCustomManifest(nav={Nav})", nav);
        }

        /// <summary>The UnInstallFile method unInstalls a single file.</summary>
        /// <param name="unInstallFile">The InstallFile to unInstall.</param>
        protected override void UnInstallFile(InstallFile unInstallFile)
        {
			Logger.LogError(">>> Entering UnInstallFile(unInstallFile={UnInstallFile})", unInstallFile);
            // Uninstall file
            base.UnInstallFile(unInstallFile);

            if (unInstallFile.Extension == "htm" || unInstallFile.Extension == "html")
            {
                // Try to remove "processed file"
                string fileName = unInstallFile.FullName;
                fileName = fileName.Replace(Path.GetExtension(fileName), ".ascx");
				Logger.LogError("UnInstallFile(unInstallFile={UnInstallFile}): fileName → {FileName}", unInstallFile, fileName);
                Util.DeleteFile(fileName, this.PhysicalBasePath, this.Log);
            }
			Logger.LogWarning("<<< Exiting UnInstallFile(unInstallFile={UnInstallFile})", unInstallFile);
        }

        /// <summary>
        /// The DeleteSkinPackage method deletes the Skin Package
        /// from the data Store.
        /// </summary>
        private void DeleteSkinPackage()
        {
			Logger.LogError(">>> Entering DeleteSkinPackage()");
            try
            {
                // Attempt to get the Authentication Service
                SkinPackageInfo skinPackage = SkinController.GetSkinByPackageID(this.Package.PackageID);
				Logger.LogError("DeleteSkinPackage(): skinPackage → {SkinPackage}", skinPackage);
                if (skinPackage != null)
                {
                    SkinController.DeleteSkinPackage(skinPackage);
                }

                this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.SKIN_UnRegistered, skinPackage.SkinName));
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
			Logger.LogWarning("<<< Exiting DeleteSkinPackage()");
        }
    }
}
