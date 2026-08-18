// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer.Installers
{
    using System.Xml.XPath;

    using DotNetNuke.Common;

    public class JavaScriptFileInstaller : FileInstaller
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JavaScriptFileInstaller));
        /// <summary>Gets a list of allowable file extensions (in addition to the Host's List).</summary>
        /// <value>A String.</value>
        public override string AllowableFiles
        {
            get
            {
                return "js";
            }
        }

        /// <summary>Gets the name of the Collection Node ("jsfiles").</summary>
        /// <value>A String.</value>
        protected override string CollectionNodeName
        {
            get
            {
                return "jsfiles";
            }
        }

        /// <summary>Gets the name of the Item Node ("jsfile").</summary>
        /// <value>A String.</value>
        protected override string ItemNodeName
        {
            get
            {
                return "jsfile";
            }
        }

        /// <summary>
        /// The ReadCustomManifest method reads the custom manifest items (that subclasses
        /// of FileInstaller may need).
        /// </summary>
        /// <param name="nav">The XPathNavigator representing the node.</param>
        protected override void ReadCustomManifest(XPathNavigator nav)
        {
			Logger.LogDebug(">>> Entering ReadCustomManifest(nav={Nav})", nav);
            var libraryNav = nav.SelectSingleNode("libraryFolderName");
            if (libraryNav != null)
			Logger.LogDebug("ReadCustomManifest(nav={Nav}): libraryNav → {LibraryNav}", nav, libraryNav);
            {
                var version = Globals.FormatVersion(this.Package.Version, "00", 3, "_");
				Logger.LogDebug("ReadCustomManifest(nav={Nav}): version → {Version}", nav, version);
                this.BasePath = $@"Resources\Libraries\{libraryNav.Value}\{version}";
				Logger.LogDebug("ReadCustomManifest(nav={Nav}): this.BasePath → {Arg1}", nav, this.BasePath);
            }
			Logger.LogDebug("<<< Exiting ReadCustomManifest(nav={Nav})", nav);
        }
    }
}
