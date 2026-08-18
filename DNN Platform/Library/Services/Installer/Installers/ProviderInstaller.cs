// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System.Xml.XPath;

    /// <summary>The ProviderInstaller installs Provider Components to a DotNetNuke site.</summary>
    public class ProviderInstaller : ComponentInstallerBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ProviderInstaller));
        /// <summary>Gets a list of allowable file extensions (in addition to the Host's List).</summary>
        /// <value>A String.</value>
        public override string AllowableFiles
        {
            get
            {
                return "ashx, aspx, ascx, vb, cs, resx, css, js, resources, config, xml, htc, html, htm, text, vbproj, csproj, sln";
            }
        }

        /// <inheritdoc />
        public override void Commit()
        {
			Logger.LogDebug(">>> Entering Commit()");
            this.Completed = true;
			Logger.LogDebug("Commit(): this.Completed → {Arg0}", this.Completed);
			Logger.LogDebug("<<< Exiting Commit()");
        }

        /// <inheritdoc />
        public override void Install()
        {
			Logger.LogDebug(">>> Entering Install()");
            this.Completed = true;
			Logger.LogDebug("Install(): this.Completed → {Arg0}", this.Completed);
			Logger.LogDebug("<<< Exiting Install()");
        }

        /// <inheritdoc />
        public override void ReadManifest(XPathNavigator manifestNav)
        {
		Logger.LogDebug(">>> Entering ReadManifest(manifestNav={ManifestNav})", manifestNav);
		Logger.LogDebug("<<< Exiting ReadManifest(manifestNav={ManifestNav})", manifestNav);
        }

        /// <inheritdoc />
        public override void Rollback()
        {
			Logger.LogDebug(">>> Entering Rollback()");
            this.Completed = true;
			Logger.LogDebug("Rollback(): this.Completed → {Arg0}", this.Completed);
			Logger.LogDebug("<<< Exiting Rollback()");
        }

        /// <inheritdoc />
        public override void UnInstall()
        {
			Logger.LogDebug(">>> Entering UnInstall()");
            this.Completed = true;
			Logger.LogDebug("UnInstall(): this.Completed → {Arg0}", this.Completed);
			Logger.LogDebug("<<< Exiting UnInstall()");
        }
    }
}
