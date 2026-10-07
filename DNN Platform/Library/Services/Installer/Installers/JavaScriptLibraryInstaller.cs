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
    using DotNetNuke.Framework.JavaScriptLibraries;

    public class JavaScriptLibraryInstaller : ComponentInstallerBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JavaScriptLibraryInstaller));
        private JavaScriptLibrary library;
        private JavaScriptLibrary installedLibrary;

        /// <inheritdoc />
        public override void Commit()
        {
		Logger.LogError(">>> Entering Commit()");
        Logger.LogWarning("<<< Exiting Commit()");
        }

        /// <inheritdoc />
        public override void Install()
        {
			Logger.LogError(">>> Entering Install()");
            try
            {
                // Attempt to get the JavaScript Library
                this.installedLibrary = JavaScriptLibraryController.Instance.GetLibrary(l => l.LibraryName == this.library.LibraryName && l.Version == this.library.Version);

                if (this.installedLibrary != null)
                {
                    this.library.JavaScriptLibraryID = this.installedLibrary.JavaScriptLibraryID;
                }

                // Save JavaScript Library  to database
                this.library.PackageID = this.Package.PackageID;
                JavaScriptLibraryController.Instance.SaveLibrary(this.library);

                this.Completed = true;
				Logger.LogDebug("Install(): this.Completed → {Arg0}", this.Completed);
                this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.LIBRARY_Registered, this.library.LibraryName));
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
			Logger.LogWarning("<<< Exiting Install()");
        }

        /// <inheritdoc />
        public override void ReadManifest(XPathNavigator manifestNav)
        {
			Logger.LogError(">>> Entering ReadManifest(manifestNav={ManifestNav})", manifestNav);
            // Load the JavaScript Library from the manifest
            this.library = CBO.DeserializeObject<JavaScriptLibrary>(new StringReader(manifestNav.InnerXml));
			Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): library → {Library}", manifestNav, library);
            this.library.Version = this.Package.Version;

            if (this.Log.Valid)
            {
                this.Log.AddInfo(Util.LIBRARY_ReadSuccess);
            }
			Logger.LogWarning("<<< Exiting ReadManifest(manifestNav={ManifestNav})", manifestNav);
        }

        /// <inheritdoc />
        public override void Rollback()
        {
			Logger.LogError(">>> Entering Rollback()");
            // If Temp Library exists then we need to update the DataStore with this
            if (this.installedLibrary == null)
            {
                // No Temp Library - Delete newly added library
                this.DeleteLibrary();
            }
            else
            {
                // Temp Library - Rollback to Temp
                JavaScriptLibraryController.Instance.SaveLibrary(this.installedLibrary);
            }
			Logger.LogWarning("<<< Exiting Rollback()");
        }

        /// <inheritdoc />
        public override void UnInstall()
        {
			Logger.LogError(">>> Entering UnInstall()");
            this.DeleteLibrary();
			Logger.LogWarning("<<< Exiting UnInstall()");
        }

        private void DeleteLibrary()
        {
			Logger.LogError(">>> Entering DeleteLibrary()");
            try
            {
                // Attempt to get the Library
                var library = JavaScriptLibraryController.Instance.GetLibrary(l => l.PackageID == this.Package.PackageID);
				Logger.LogError("DeleteLibrary(): library → {Library}", library);

                if (library != null)
                {
                    JavaScriptLibraryController.Instance.DeleteLibrary(library);

                    this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.LIBRARY_UnRegistered, library.LibraryName));
                }
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
			Logger.LogWarning("<<< Exiting DeleteLibrary()");
        }
    }
}
