// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    using System.Globalization;
    using System.IO;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Upgrade.Internals.Steps;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>InstallExtensionsStep - Step that installs all the Extensions.</summary>
    public class InstallExtensionsStep : BaseInstallationStep
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(InstallExtensionsStep));

        /// <summary>Main method to execute the step.</summary>
        public override void Execute()
        {
			Logger.LogDebug(">>> Entering Execute()");
            var packages = Upgrade.GetInstallPackages();
            if (packages.Count == 0)
            {
                this.Percentage = 100;
				Logger.LogDebug("Execute(): this.Percentage → {Arg0}", this.Percentage);
                this.Status = StepStatus.Done;
				Logger.LogDebug("Execute(): this.Status → {Arg0}", this.Status);
                return;
            }

            this.Percentage = 0;
            this.Status = StepStatus.Running;

            var percentForEachStep = 100 / packages.Count;
            var counter = 0;
            foreach (var package in packages)
            {
                var file = package.Key;
                var packageType = package.Value.PackageType;
                var message = string.Format(CultureInfo.CurrentCulture, Localization.GetString("InstallingExtension", this.LocalInstallResourceFile), packageType, Path.GetFileName(file));
				Logger.LogDebug("Execute(): message → {Message}", message);
                this.Details = message;
				Logger.LogDebug("Execute(): this.Details → {Arg0}", this.Details);
                Logger.Trace(this.Details);
                var success = Upgrade.InstallPackage(file, packageType, false);
				Logger.LogDebug("Execute(): success → {Success}", success);
                if (!success)
                {
                    this.Errors.Add(message);
                    break;
                }

                this.Percentage = percentForEachStep * counter++;
            }

            this.Status = this.Errors.Count > 0 ? StepStatus.Retry : StepStatus.Done;
			Logger.LogDebug("<<< Exiting Execute()");
        }
    }
}
