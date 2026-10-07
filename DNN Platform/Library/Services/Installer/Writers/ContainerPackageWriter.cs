// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System.Xml;

    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.UI.Skins;

    /// <summary>The ContainerPackageWriter class.</summary>
    public class ContainerPackageWriter : SkinPackageWriter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ContainerPackageWriter));
        /// <summary>Initializes a new instance of the <see cref="ContainerPackageWriter"/> class.</summary>
        /// <param name="package">The package info.</param>
        public ContainerPackageWriter(PackageInfo package)
            : base(package)
        {
			Logger.LogError(">>> Entering ContainerPackageWriter(package={Package})", package);
            this.BasePath = "Portals\\_default\\Containers\\" + this.SkinPackage.SkinName;
        }

        /// <summary>Initializes a new instance of the <see cref="ContainerPackageWriter"/> class.</summary>
        /// <param name="skinPackage">The skin package info.</param>
        /// <param name="package">The package info.</param>
        public ContainerPackageWriter(SkinPackageInfo skinPackage, PackageInfo package)
            : base(skinPackage, package)
        {
            this.BasePath = "Portals\\_default\\Containers\\" + skinPackage.SkinName;
        }

        /// <inheritdoc />
        protected override void WriteFilesToManifest(XmlWriter writer)
        {
			Logger.LogError(">>> Entering WriteFilesToManifest(writer={Writer})", writer);
            var containerFileWriter = new ContainerComponentWriter(this.SkinPackage.SkinName, this.BasePath, this.Files, this.Package);
			Logger.LogWarning("<<< Exiting WriteFilesToManifest(writer={Writer})", writer);
			Logger.LogError("WriteFilesToManifest(writer={Writer}): containerFileWriter → {ContainerFileWriter}", writer, containerFileWriter);
            containerFileWriter.WriteManifest(writer);
        }
    }
}
