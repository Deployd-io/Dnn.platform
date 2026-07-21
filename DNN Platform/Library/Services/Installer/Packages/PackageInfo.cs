// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Packages
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Services.Installer.Log;
    using Newtonsoft.Json;

    /// <summary>The PackageInfo class represents a single Installer Package.</summary>
    [Serializable]
    public class PackageInfo : BaseEntityInfo
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private IList<PackageDependencyInfo> dependencies;

        /// <summary>Initializes a new instance of the <see cref="PackageInfo"/> class.</summary>
        /// <param name="info">The installer info.</param>
        public PackageInfo(InstallerInfo info)
            : this()
        {
			Logger.LogDebug(">>> Entering PackageInfo(info={Info})", info);
            this.AttachInstallerInfo(info);
			Logger.LogDebug("<<< Exiting PackageInfo(info={Info})", info);
			Logger.LogDebug(">>> Entering PackageInfo()");
			Logger.LogDebug("<<< Exiting PackageInfo()");
        }

        /// <summary>Initializes a new instance of the <see cref="PackageInfo"/> class.</summary>
        public PackageInfo()
        {
            this.PackageID = Null.NullInteger;
            this.PortalID = Null.NullInteger;
			Logger.LogDebug("PackageInfo(): this.PackageID → {Arg0}", this.PackageID);
			Logger.LogDebug("PackageInfo(): PortalID → {PortalID}", PortalID);
			Logger.LogDebug("PackageInfo(): this.PortalID → {Arg0}", this.PortalID);
			Logger.LogDebug("PackageInfo(): PackageID → {PackageID}", PackageID);
            this.Version = new Version(0, 0, 0);
            this.IsValid = true;
            this.InstalledVersion = new Version(0, 0, 0);
			Logger.LogDebug("PackageInfo(): Version → {Version}", Version);
			Logger.LogDebug("PackageInfo(): IsValid → {IsValid}", IsValid);
			Logger.LogDebug("PackageInfo(): this.IsValid → {Arg0}", this.IsValid);
			Logger.LogDebug("PackageInfo(): InstalledVersion → {InstalledVersion}", InstalledVersion);
        }

        /// <summary>Gets the direct dependencies of this package.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public IList<PackageDependencyInfo> Dependencies
        {
            get
            {
                return this.dependencies ?? (this.dependencies = (this.PackageID == -1)
                                        ? new List<PackageDependencyInfo>()
                                        : PackageController.Instance.GetPackageDependencies(p => p.PackageId == this.PackageID));
            }
        }

        /// <summary>Gets a Dictionary of Files that are included in the Package.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public Dictionary<string, InstallFile> Files
        {
            get
            {
                return this.InstallerInfo.Files;
            }
        }

        /// <summary>Gets the InstallMode.</summary>
        public InstallMode InstallMode
        {
            get
            {
                return this.InstallerInfo.InstallMode;
            }
        }

        /// <summary>Gets the Logger.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public Logger Log
        {
            get
            {
                return this.InstallerInfo.Log;
            }
        }

        /// <summary>Gets or sets the Email for this package.</summary>
        public string Email { get; set; }

        /// <summary>Gets or sets the Description of this package.</summary>
        public string Description { get; set; }

        /// <summary>Gets or sets the FileName of this package.</summary>
        public string FileName { get; set; }

        /// <summary>Gets or sets the name (path) of the folder where the package is installed.</summary>
        public string FolderName { get; set; }

        /// <summary>Gets or sets the Friendly Name of this package.</summary>
        public string FriendlyName { get; set; }

        /// <summary>Gets or sets the URL for the icon for the package.</summary>
        public string IconFile { get; set; }

        /// <summary>Gets or sets the Installed Version of the Package.</summary>
        public Version InstalledVersion { get; set; }

        /// <summary>Gets the associated InstallerInfo.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public InstallerInfo InstallerInfo { get; private set; }

        /// <summary>Gets or sets a value indicating whether this package is a "system" Package.</summary>
        public bool IsSystemPackage { get; set; }

        /// <summary>Gets a value indicating whether the Package is Valid.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public bool IsValid { get; private set; }

        /// <summary>Gets or sets the License of this package.</summary>
        public string License { get; set; }

        /// <summary>Gets or sets the Manifest of this package.</summary>
        public string Manifest { get; set; }

        /// <summary>Gets or sets the Name of this package.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the Organisation for this package.</summary>
        public string Organization { get; set; }

        /// <summary>Gets or sets the Owner of this package.</summary>
        public string Owner { get; set; }

        /// <summary>Gets or sets the ID of this package.</summary>
        public int PackageID { get; set; }

        /// <summary>Gets or sets the Type of this package.</summary>
        public string PackageType { get; set; }

        /// <summary>Gets or sets the ID of this portal.</summary>
        public int PortalID { get; set; }

        /// <summary>Gets or sets the ReleaseNotes of this package.</summary>
        public string ReleaseNotes { get; set; }

        /// <summary>Gets or sets the URL for this package.</summary>
        public string Url { get; set; }

        /// <summary>Gets or sets the Version of this package.</summary>
        public Version Version { get; set; }

        /// <summary>The AttachInstallerInfo method attaches an InstallerInfo instance to the Package.</summary>
        /// <param name="installer">The InstallerInfo instance to attach.</param>
        public void AttachInstallerInfo(InstallerInfo installer)
        {
			Logger.LogDebug(">>> Entering AttachInstallerInfo(installer={Installer})", installer);
            this.InstallerInfo = installer;
			Logger.LogDebug("<<< Exiting AttachInstallerInfo(installer={Installer})", installer);
			Logger.LogDebug("AttachInstallerInfo(installer={Installer}): InstallerInfo → {InstallerInfo}", installer, InstallerInfo);
			Logger.LogDebug("AttachInstallerInfo(installer={Installer}): this.InstallerInfo → {Arg1}", installer, this.InstallerInfo);
        }

        /// <summary>Clone current object.</summary>
        /// <returns>A new <see cref="PackageInfo"/> instance.</returns>
        public PackageInfo Clone()
        {
			Logger.LogDebug(">>> Entering Clone()");
			Logger.LogDebug("<<< Exiting Clone()");
            return new PackageInfo
            {
                PackageID = this.PackageID,
                PortalID = this.PortalID,
                PackageType = this.PackageType,
                InstallerInfo = this.InstallerInfo,
                Name = this.Name,
                FriendlyName = this.FriendlyName,
                Manifest = this.Manifest,
                Email = this.Email,
                Description = this.Description,
                FolderName = this.FolderName,
                FileName = this.FileName,
                IconFile = this.IconFile,
                IsSystemPackage = this.IsSystemPackage,
                IsValid = this.IsValid,
                Organization = this.Organization,
                Owner = this.Owner,
                License = this.License,
                ReleaseNotes = this.ReleaseNotes,
                Url = this.Url,
                Version = this.Version,
                InstalledVersion = this.InstalledVersion,
            };
        }
    }
}
