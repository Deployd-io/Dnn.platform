// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System;
    using System.IO;
    using System.Xml;

    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.UI.Skins;

    /// <summary>The SkinPackageWriter class.</summary>
    public class SkinPackageWriter : PackageWriterBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SkinPackageWriter));
        private readonly SkinPackageInfo skinPackage;
        private readonly string subFolder;

        /// <summary>Initializes a new instance of the <see cref="SkinPackageWriter"/> class.</summary>
        /// <param name="package">The package info.</param>
        public SkinPackageWriter(PackageInfo package)
            : base(package)
        {
			Logger.LogError(">>> Entering SkinPackageWriter(package={Package})", package);
            this.skinPackage = SkinController.GetSkinByPackageID(package.PackageID);
			Logger.LogDebug("SkinPackageWriter(skinPackage={SkinPackage},package={Package}): skinPackage → {SkinPackage1}", skinPackage, package, skinPackage);
			Logger.LogDebug("SkinPackageWriter(package={Package}): skinPackage → {SkinPackage}", package, skinPackage);
        Logger.LogDebug("SkinPackageWriter(skinPackage={SkinPackage},package={Package},basePath={BasePath}): BasePath → {BasePath1}", skinPackage, package, basePath, BasePath);
        Logger.LogDebug("SkinPackageWriter(skinPackage={SkinPackage},package={Package},basePath={BasePath},subFolder={SubFolder}): this.subFolder → {Arg4}", skinPackage, package, basePath, subFolder, this.subFolder);
        Logger.LogWarning("<<< Exiting SkinPackageWriter(package={Package})", package);
            this.SetBasePath();
			Logger.LogError(">>> Entering SkinPackageWriter(skinPackage={SkinPackage},package={Package})", skinPackage, package);
			Logger.LogWarning("<<< Exiting SkinPackageWriter(skinPackage={SkinPackage},package={Package})", skinPackage, package);
			Logger.LogDebug("SkinPackageWriter(skinPackage={SkinPackage},package={Package}): this.skinPackage → {Arg2}", skinPackage, package, this.skinPackage);
			Logger.LogDebug("SkinPackageWriter(skinPackage={SkinPackage},package={Package},basePath={BasePath}): this.skinPackage → {Arg3}", skinPackage, package, basePath, this.skinPackage);
        }

        /// <summary>Initializes a new instance of the <see cref="SkinPackageWriter"/> class.</summary>
        /// <param name="skinPackage">The skin package info.</param>
        /// <param name="package">The package info.</param>
        public SkinPackageWriter(SkinPackageInfo skinPackage, PackageInfo package)
            : base(package)
        {
            this.skinPackage = skinPackage;
            this.SetBasePath();
        }

        /// <summary>Initializes a new instance of the <see cref="SkinPackageWriter"/> class.</summary>
        /// <param name="skinPackage">The skin package info.</param>
        /// <param name="package">The package info.</param>
        /// <param name="basePath">The base path for the skin.</param>
        public SkinPackageWriter(SkinPackageInfo skinPackage, PackageInfo package, string basePath)
            : base(package)
        {
            this.skinPackage = skinPackage;
            this.BasePath = basePath;
			Logger.LogDebug("SkinPackageWriter(skinPackage={SkinPackage},package={Package},basePath={BasePath},subFolder={SubFolder}): this.skinPackage → {Arg4}", skinPackage, package, basePath, subFolder, this.skinPackage);
        }

        /// <summary>Initializes a new instance of the <see cref="SkinPackageWriter"/> class.</summary>
        /// <param name="skinPackage">The skin package info.</param>
        /// <param name="package">The package info.</param>
        /// <param name="basePath">The base path for the skin.</param>
        /// <param name="subFolder">The folder under <paramref name="basePath"/> for this skin.</param>
        public SkinPackageWriter(SkinPackageInfo skinPackage, PackageInfo package, string basePath, string subFolder)
            : base(package)
        {
            this.skinPackage = skinPackage;
            this.subFolder = subFolder;
            this.BasePath = Path.Combine(basePath, subFolder);
        }

        /// <inheritdoc />
        public override bool IncludeAssemblies
        {
            get
            {
                return false;
            }
        }

        protected SkinPackageInfo SkinPackage
        {
            get
            {
                return this.skinPackage;
            }
        }

        public void SetBasePath()
        {
			Logger.LogError(">>> Entering SetBasePath()");
            if (this.skinPackage.SkinType == "Skin")
            {
                this.BasePath = Path.Combine("Portals\\_default\\Skins", this.SkinPackage.SkinName);
            }
            else
            {
                this.BasePath = Path.Combine("Portals\\_default\\Containers", this.SkinPackage.SkinName);
            }
			Logger.LogWarning("<<< Exiting SetBasePath()");
        }

        /// <inheritdoc />
        protected override void GetFiles(bool includeSource, bool includeAppCode)
        {
			Logger.LogError(">>> Entering GetFiles(includeSource={IncludeSource},includeAppCode={IncludeAppCode})", includeSource, includeAppCode);
            // Call base class method with includeAppCode = false
            base.GetFiles(includeSource, false);
			Logger.LogWarning("<<< Exiting GetFiles(includeSource={IncludeSource},includeAppCode={IncludeAppCode})", includeSource, includeAppCode);
        }

        /// <inheritdoc />
        protected override void ParseFiles(DirectoryInfo folder, string rootPath)
        {
			Logger.LogError(">>> Entering ParseFiles(folder={Folder},rootPath=<redacted>)", folder);
            // Add the Files in the Folder
            FileInfo[] files = folder.GetFiles();
            foreach (FileInfo file in files)
            {
                string filePath = folder.FullName.Replace(rootPath, string.Empty);
				Logger.LogError("ParseFiles(folder={Folder},rootPath=<redacted>): filePath → {FilePath}", folder, filePath);
                if (filePath.StartsWith(@"\", StringComparison.Ordinal))
                {
                    filePath = filePath.Substring(1);
                }

                if (!file.Extension.Equals(".dnn", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(this.subFolder))
                    {
                        this.AddFile(Path.Combine(filePath, file.Name));
                    }
                    else
                    {
                        filePath = Path.Combine(filePath, file.Name);
                        this.AddFile(filePath, Path.Combine(this.subFolder, filePath));
                    }
					Logger.LogWarning("<<< Exiting ParseFiles(folder={Folder},rootPath=<redacted>)", folder);
                }
            }
        }

        /// <inheritdoc />
        protected override void WriteFilesToManifest(XmlWriter writer)
        {
			Logger.LogError(">>> Entering WriteFilesToManifest(writer={Writer})", writer);
            var skinFileWriter = new SkinComponentWriter(this.SkinPackage.SkinName, this.BasePath, this.Files, this.Package);
			Logger.LogError("WriteFilesToManifest(writer={Writer}): skinFileWriter → {SkinFileWriter}", writer, skinFileWriter);
            if (this.SkinPackage.SkinType == "Skin")
            {
                skinFileWriter = new SkinComponentWriter(this.SkinPackage.SkinName, this.BasePath, this.Files, this.Package);
            }
            else
            {
                skinFileWriter = new ContainerComponentWriter(this.SkinPackage.SkinName, this.BasePath, this.Files, this.Package);
            }

            skinFileWriter.WriteManifest(writer);
			Logger.LogWarning("<<< Exiting WriteFilesToManifest(writer={Writer})", writer);
        }
    }
}
