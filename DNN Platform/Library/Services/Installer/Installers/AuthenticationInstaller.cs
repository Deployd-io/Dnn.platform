// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Globalization;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Authentication;

    /// <summary>The AuthenticationInstaller installs Authentication Service Components to a DotNetNuke site.</summary>
    public class AuthenticationInstaller : ComponentInstallerBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private AuthenticationInfo authSystem;
        private AuthenticationInfo tempAuthSystem;

        /// <summary>Gets a list of allowable file extensions (in addition to the Host's List).</summary>
        /// <value>A String.</value>
        public override string AllowableFiles
        {
            get
            {
                return "ashx, aspx, ascx, vb, cs, resx, css, js, resources, config, vbproj, csproj, sln, htm, html";
            }
        }

        /// <summary>The Commit method finalises the Install and commits any pending changes.</summary>
        /// <remarks>In the case of Authentication systems this is not neccessary.</remarks>
        public override void Commit()
        {
		Logger.LogDebug(">>> Entering Commit()");
		Logger.LogDebug("<<< Exiting Commit()");
        }

        /// <summary>The Install method installs the authentication component.</summary>
        public override void Install()
        {
			Logger.LogDebug(">>> Entering Install()");
            bool bAdd = Null.NullBoolean;
            try
			Logger.LogDebug("Install(): bAdd → {BAdd}", bAdd);
            {
                // Attempt to get the Authentication Service
                this.tempAuthSystem = AuthenticationController.GetAuthenticationServiceByType(this.authSystem.AuthenticationType);
				Logger.LogDebug("Install(): tempAuthSystem → {TempAuthSystem}", tempAuthSystem);

                if (this.tempAuthSystem == null)
                {
                    // Enable by default
                    this.authSystem.IsEnabled = true;
                    bAdd = true;
                }
                else
                {
                    this.authSystem.AuthenticationID = this.tempAuthSystem.AuthenticationID;
                    this.authSystem.IsEnabled = this.tempAuthSystem.IsEnabled;
                }

                this.authSystem.PackageID = this.Package.PackageID;
                if (bAdd)
                {
                    // Add new service
                    AuthenticationController.AddAuthentication(this.authSystem);
                }
                else
                {
                    // Update service
                    AuthenticationController.UpdateAuthentication(this.authSystem);
                }

                this.Completed = true;
				Logger.LogDebug("Install(): this.Completed → {Arg0}", this.Completed);
                this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.AUTHENTICATION_Registered, this.authSystem.AuthenticationType));
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
			Logger.LogDebug("<<< Exiting Install()");
        }

        /// <summary>The ReadManifest method reads the manifest file for the Authentication component.</summary>
        /// <param name="manifestNav">The XPath navigator for the Auth System section of the manifest.</param>
        public override void ReadManifest(XPathNavigator manifestNav)
        {
			Logger.LogDebug(">>> Entering ReadManifest(manifestNav={ManifestNav})", manifestNav);
            this.authSystem = new AuthenticationInfo();

			Logger.LogDebug("ReadManifest(manifestNav={ManifestNav}): authSystem → {AuthSystem}", manifestNav, authSystem);
            // Get the type
            this.authSystem.AuthenticationType = Util.ReadElement(manifestNav, "authenticationService/type", this.Log, Util.AUTHENTICATION_TypeMissing);

            // Get the SettingsSrc
            this.authSystem.SettingsControlSrc = Util.ReadElement(manifestNav, "authenticationService/settingsControlSrc", this.Log, Util.AUTHENTICATION_SettingsSrcMissing);

            // Get the LoginSrc
            this.authSystem.LoginControlSrc = Util.ReadElement(manifestNav, "authenticationService/loginControlSrc", this.Log, Util.AUTHENTICATION_LoginSrcMissing);

            // Get the LogoffSrc
            this.authSystem.LogoffControlSrc = Util.ReadElement(manifestNav, "authenticationService/logoffControlSrc");

            if (this.Log.Valid)
            {
                this.Log.AddInfo(Util.AUTHENTICATION_ReadSuccess);
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
            // If Temp Auth System exists then we need to update the DataStore with this
            if (this.tempAuthSystem == null)
            {
                // No Temp Auth System - Delete newly added system
                this.DeleteAuthentication();
            }
            else
            {
                // Temp Auth System - Rollback to Temp
                AuthenticationController.UpdateAuthentication(this.tempAuthSystem);
            }
			Logger.LogDebug("<<< Exiting Rollback()");
        }

        /// <summary>The UnInstall method uninstalls the authentication component.</summary>
        public override void UnInstall()
        {
			Logger.LogDebug(">>> Entering UnInstall()");
            this.DeleteAuthentication();
			Logger.LogDebug("<<< Exiting UnInstall()");
        }

        /// <summary>
        /// The DeleteAuthentication method deletes the Authentication System
        /// from the data Store.
        /// </summary>
        private void DeleteAuthentication()
        {
			Logger.LogDebug(">>> Entering DeleteAuthentication()");
            try
            {
                AuthenticationInfo authSystem = AuthenticationController.GetAuthenticationServiceByPackageID(this.Package.PackageID);
                if (authSystem != null)
				Logger.LogDebug("DeleteAuthentication(): authSystem → {AuthSystem}", authSystem);
                {
                    AuthenticationController.DeleteAuthentication(authSystem);
                }

                this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.AUTHENTICATION_UnRegistered, authSystem.AuthenticationType));
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
			Logger.LogDebug("<<< Exiting DeleteAuthentication()");
        }
    }
}
