// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    /// <summary>The SkinControlInstaller installs SkinControl (SkinObject) Components to a DotNetNuke site.</summary>
    public class SkinControlInstaller : ComponentInstallerBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SkinControlInstaller));
        private SkinControlInfo installedSkinControl;
        private SkinControlInfo skinControl;

        /// <summary>Gets a list of allowable file extensions (in addition to the Host's List).</summary>
        /// <value>A String.</value>
        public override string AllowableFiles
        {
            get
            {
                return "ascx, vb, cs, js, resx, xml, vbproj, csproj, sln";
            }
        }

        /// <summary>The Commit method finalises the Install and commits any pending changes.</summary>
        /// <remarks>In the case of Modules this is not neccessary.</remarks>
        public override void Commit()
        {
		Logger.LogDebug(">>> Entering Commit()");
		Logger.LogDebug("<<< Exiting Commit()");
        }

        /// <summary>The Install method installs the Module component.</summary>
        public override void Install()
        {
			Logger.LogDebug(">>> Entering Install()");
            try
            {
                // Attempt to get the SkinControl
                this.installedSkinControl = SkinControlController.GetSkinControlByKey(this.skinControl.ControlKey);
				Logger.LogDebug("Install(): installedSkinControl → {InstalledSkinControl}", installedSkinControl);

                if (this.installedSkinControl != null)
                {
                    this.skinControl.SkinControlID = this.installedSkinControl.SkinControlID;
                }

                // Save SkinControl
                this.skinControl.PackageID = this.Package.PackageID;
                this.skinControl.SkinControlID = SkinControlController.SaveSkinControl(this.skinControl);

                this.Completed = true;
				Logger.LogDebug("Install(): this.Completed → {Arg0}", this.Completed);
                this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.MODULE_Registered, this.skinControl.ControlKey));
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
			Logger.LogDebug("<<< Exiting Install()");
        }

        /// <summary>The ReadManifest method reads the manifest file for the SkinControl component.</summary>
        /// <param name="manifestNav">The XPath navigator for the Skin Control section of the manifest.</param>
        public override void ReadManifest(XPathNavigator manifestNav)
        {
			Logger.LogDebug(">>> Entering ReadManifest(manifestNav={ManifestNav})", manifestNav);
            // Load the SkinControl from the manifest
            this.skinControl = CBO.DeserializeObject<SkinControlInfo>(new StringReader(manifestNav.InnerXml));
			Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): skinControl → {SkinControl}", manifestNav, skinControl);

            if (this.Log.Valid)
            {
                this.Log.AddInfo(Util.MODULE_ReadSuccess);
            }
			Logger.LogDebug("<<< Exiting ReadManifest(manifestNav={ManifestNav})", manifestNav);
        }

        /// <summary>
        /// The Rollback method undoes the installation of the component in the event
        /// that one of the other components fails.
        /// </summary>
        public override void Rollback()
        {
			Logger.LogDebug(">>> Entering Rollback()");
            // If Temp SkinControl exists then we need to update the DataStore with this
            if (this.installedSkinControl == null)
            {
                // No Temp SkinControl - Delete newly added SkinControl
                this.DeleteSkinControl();
            }
            else
            {
                // Temp SkinControl - Rollback to Temp
                SkinControlController.SaveSkinControl(this.installedSkinControl);
            }
			Logger.LogDebug("<<< Exiting Rollback()");
        }

        /// <summary>The UnInstall method uninstalls the SkinControl component.</summary>
        public override void UnInstall()
        {
			Logger.LogDebug(">>> Entering UnInstall()");
            this.DeleteSkinControl();
			Logger.LogDebug("<<< Exiting UnInstall()");
        }

        /// <summary>The DeleteSkinControl method deletes the SkinControl from the data Store.</summary>
        private void DeleteSkinControl()
        {
			Logger.LogDebug(">>> Entering DeleteSkinControl()");
            try
            {
                // Attempt to get the SkinControl
                SkinControlInfo skinControl = SkinControlController.GetSkinControlByPackageID(this.Package.PackageID);
				Logger.LogDebug("DeleteSkinControl(): skinControl → {SkinControl}", skinControl);
                if (skinControl != null)
                {
                    SkinControlController.DeleteSkinControl(skinControl);
                }

                this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.MODULE_UnRegistered, skinControl.ControlKey));
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
			Logger.LogDebug("<<< Exiting DeleteSkinControl()");
        }
    }
}
