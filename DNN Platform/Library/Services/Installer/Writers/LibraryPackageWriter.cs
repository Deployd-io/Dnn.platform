// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using DotNetNuke.Services.Installer.Packages;

    /// <summary>The LibraryPackageWriter class.</summary>
    public class LibraryPackageWriter : PackageWriterBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LibraryPackageWriter));
        /// <summary>Initializes a new instance of the <see cref="LibraryPackageWriter"/> class.</summary>
        /// <param name="package">The package info.</param>
        public LibraryPackageWriter(PackageInfo package)
            : base(package)
        {
			Logger.LogDebug(">>> Entering LibraryPackageWriter(package={Package})", package);
            this.BasePath = "DesktopModules\\Libraries";
            this.AssemblyPath = "bin";
			Logger.LogDebug("LibraryPackageWriter(package={Package}): this.BasePath → {Arg1}", package, this.BasePath);
			Logger.LogDebug("<<< Exiting LibraryPackageWriter(package={Package})", package);
        }

        /// <inheritdoc />
        protected override void GetFiles(bool includeSource, bool includeAppCode)
        {
			Logger.LogDebug(">>> Entering GetFiles(includeSource={IncludeSource},includeAppCode={IncludeAppCode})", includeSource, includeAppCode);
            base.GetFiles(includeSource, false);
			Logger.LogDebug("<<< Exiting GetFiles(includeSource={IncludeSource},includeAppCode={IncludeAppCode})", includeSource, includeAppCode);
        }
    }
}
