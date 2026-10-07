// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Packages
{
    using System;

    /// <summary>
    /// PackageCreatedEventArgs provides a custom EventArgs class for a
    /// Package Created Event.
    /// </summary>
    public class PackageCreatedEventArgs : EventArgs
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PackageCreatedEventArgs));
        private readonly PackageInfo package;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageCreatedEventArgs"/> class.
        /// Builds a new PackageCreatedEventArgs.
        /// </summary>
        /// <param name="package">The package associated with this event.</param>
        public PackageCreatedEventArgs(PackageInfo package)
        {
			Logger.LogError(">>> Entering PackageCreatedEventArgs(package={Package})", package);
            this.package = package;
			Logger.LogDebug("PackageCreatedEventArgs(package={Package}): package → {Package1}", package, package);
        Logger.LogDebug("PackageCreatedEventArgs(package={Package}): this.package → {Arg1}", package, this.package);
        Logger.LogWarning("<<< Exiting PackageCreatedEventArgs(package={Package})", package);
        }

        /// <summary>Gets the Package associated with this event.</summary>
        public PackageInfo Package
        {
            get
            {
                return this.package;
            }
        }
    }
}
