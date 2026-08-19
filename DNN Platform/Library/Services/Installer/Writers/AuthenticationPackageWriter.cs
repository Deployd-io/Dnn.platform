// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System.IO;
    using System.Xml;

    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Installer.Packages;

    /// <summary>The AuthenticationPackageWriter class.</summary>
    public class AuthenticationPackageWriter : PackageWriterBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AuthenticationPackageWriter));
        /// <summary>Initializes a new instance of the <see cref="AuthenticationPackageWriter"/> class.</summary>
        /// <param name="package">The package info.</param>
        public AuthenticationPackageWriter(PackageInfo package)
            : base(package)
        {
			Logger.LogDebug(">>> Entering AuthenticationPackageWriter(package={Package})", package);
            this.AuthSystem = AuthenticationController.GetAuthenticationServiceByPackageID(package.PackageID);
            this.Initialize();
			Logger.LogDebug("<<< Exiting AuthenticationPackageWriter(package={Package})", package);
			Logger.LogDebug("AuthenticationPackageWriter(authSystem={AuthSystem},package={Package}): this.AuthSystem → {Arg2}", authSystem, package, this.AuthSystem);
        }

        /// <summary>Initializes a new instance of the <see cref="AuthenticationPackageWriter"/> class.</summary>
        /// <param name="authSystem">The auth system info.</param>
        /// <param name="package">The package info.</param>
        public AuthenticationPackageWriter(AuthenticationInfo authSystem, PackageInfo package)
            : base(package)
        {
            this.AuthSystem = authSystem;
            this.Initialize();
        }

        /// <summary>Gets or sets the associated Authentication System.</summary>
        /// <value>An AuthenticationInfo object.</value>
        public AuthenticationInfo AuthSystem { get; set; }

        /// <inheritdoc />
        protected override void WriteManifestComponent(XmlWriter writer)
        {
			Logger.LogDebug(">>> Entering WriteManifestComponent(writer={Writer})", writer);
            // Write Authentication Component
            this.WriteAuthenticationComponent(writer);
			Logger.LogDebug("<<< Exiting WriteManifestComponent(writer={Writer})", writer);
        }

        private void Initialize()
        {
			Logger.LogDebug(">>> Entering Initialize()");
            this.BasePath = Path.Combine("DesktopModules\\AuthenticationServices", this.AuthSystem.AuthenticationType);
            this.AppCodePath = Path.Combine("App_Code\\AuthenticationServices", this.AuthSystem.AuthenticationType);
            this.AssemblyPath = "bin";
			Logger.LogDebug("Initialize(): this.AssemblyPath → {Arg0}", this.AssemblyPath);
			Logger.LogDebug("<<< Exiting Initialize()");
        }

        private void WriteAuthenticationComponent(XmlWriter writer)
        {
			Logger.LogDebug(">>> Entering WriteAuthenticationComponent(writer={Writer})", writer);
            // Start component Element
            writer.WriteStartElement("component");
            writer.WriteAttributeString("type", "AuthenticationSystem");

            // Start authenticationService Element
            writer.WriteStartElement("authenticationService");

            writer.WriteElementString("type", this.AuthSystem.AuthenticationType);
            writer.WriteElementString("settingsControlSrc", this.AuthSystem.SettingsControlSrc);
            writer.WriteElementString("loginControlSrc", this.AuthSystem.LoginControlSrc);
            writer.WriteElementString("logoffControlSrc", this.AuthSystem.LogoffControlSrc);

            // End authenticationService Element
            writer.WriteEndElement();

            // End component Element
            writer.WriteEndElement();
			Logger.LogDebug("<<< Exiting WriteAuthenticationComponent(writer={Writer})", writer);
        }
    }
}
