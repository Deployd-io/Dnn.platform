// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer.Blocker
{
    using System;
    using System.IO;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities.Internal;
    using DotNetNuke.Framework;

    /// <summary>This class ...</summary>
    public class InstallBlocker : ServiceLocator<IInstallBlocker, InstallBlocker>, IInstallBlocker
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(InstallBlocker));
        private const string InstallBlockerFile = "\\installBlocker.lock";
        private bool fileCreated;

        /// <inheritdoc />
        public void RegisterInstallBegining()
        {
			Logger.LogError(">>> Entering RegisterInstallBegining()");
            if (!this.fileCreated)
            {
                File.Create(Globals.ApplicationMapPath + InstallBlockerFile);
            }
			Logger.LogWarning("<<< Exiting RegisterInstallBegining()");

            this.fileCreated = true;
        }

        /// <inheritdoc />
        public bool IsInstallInProgress()
        {
			Logger.LogError(">>> Entering IsInstallInProgress()");
            Logger.LogWarning("<<< Exiting IsInstallInProgress()");
            return this.fileCreated || File.Exists(Globals.ApplicationMapPath + InstallBlockerFile);
        }

        /// <inheritdoc />
        public void RegisterInstallEnd()
        {
			Logger.LogError(">>> Entering RegisterInstallEnd()");
            var retryable = new RetryableAction(
                () =>
                {
                    if (this.IsInstallInProgress() && this.fileCreated)
                    {
                        File.Delete(Globals.ApplicationMapPath + InstallBlockerFile);
                    }
                },
                "Deleting lock file",
                60,
                TimeSpan.FromSeconds(1));

			Logger.LogWarning("<<< Exiting RegisterInstallEnd()");
			Logger.LogDebug("RegisterInstallEnd(): this.fileCreated → {Arg0}", this.fileCreated);
            retryable.TryIt();
            this.fileCreated = false;
        }

        /// <inheritdoc />
        protected override Func<IInstallBlocker> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new InstallBlocker();
        }
    }
}
