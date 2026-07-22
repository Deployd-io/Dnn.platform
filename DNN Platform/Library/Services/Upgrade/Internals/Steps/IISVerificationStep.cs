// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Upgrade.Internals.Steps
{
    using System;
    using System.Globalization;
    using System.Web;

    using Microsoft.Win32;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>Performs verifications about the IIS environment.</summary>
    public class IISVerificationStep : BaseInstallationStep
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Executes verifications on the IIS environment.</summary>
        public override void Execute()
        {
			Logger.LogDebug(">>> Entering Execute()");
            this.Status = StepStatus.Running;

			Logger.LogDebug("Execute(): this.Status → {Arg0}", this.Status);
            this.Details = Localization.GetString("CheckingIIS", this.LocalInstallResourceFile);

            // Checks for integrated pipeline mode.
            if (!HttpRuntime.UsingIntegratedPipeline)
            {
                this.Errors.Add(Localization.GetString("IISVerificationFail", this.LocalInstallResourceFile));
                this.Status = StepStatus.Abort;
                return;
            }

            // Check for .Net Framework 4.8
            if (!IsDotNetVersionAtLeast(528040))
            {
                this.Errors.Add(Localization.GetString("DotNetVersion48Required", this.LocalInstallResourceFile));
                this.Status = StepStatus.Abort;
                return;
            }

            this.Status = StepStatus.Done;
			Logger.LogDebug("<<< Exiting Execute()");
        }

        private static bool IsDotNetVersionAtLeast(int version)
        {
			Logger.LogDebug(">>> Entering IsDotNetVersionAtLeast(version={Version})", version);
			Logger.LogDebug("<<< Exiting IsDotNetVersionAtLeast(version={Version})", version);
            return GetVersion() >= version;
        }

        private static int GetVersion()
        {
			Logger.LogDebug(">>> Entering GetVersion()");
            using var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\");
			Logger.LogWarning("GetVersion(): key → {Key}", key);
			Logger.LogDebug("<<< Exiting GetVersion()");
            return Convert.ToInt32(key.GetValue("Release"), CultureInfo.InvariantCulture);
        }
    }
}
