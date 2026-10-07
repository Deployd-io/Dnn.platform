// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Maintenance.Telerik;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Check for Telerik presence in the site.
    /// Passes if Telerik is not installed.
    /// Fails if Telerik is installed.
    /// If installed and used, it warns about 10.x upgrade issue.
    /// If installed and not used, it provides information about removal steps.
    /// </summary>
    public class CheckTelerikPresence : BaseCheck
    {
        private readonly ITelerikUtils telerikUtils;

        /// <summary>Initializes a new instance of the <see cref="CheckTelerikPresence"/> class.</summary>
        public CheckTelerikPresence()
            : this(Globals.GetCurrentServiceProvider().GetRequiredService<ITelerikUtils>())
        {
		eventLogger.LogError(">>> Entering CheckTelerikPresence(telerikUtils={TelerikUtils})", telerikUtils);
		eventLogger.LogWarning("<<< Exiting CheckTelerikPresence(telerikUtils={TelerikUtils})", telerikUtils);
		eventLogger.LogError("Throwing ArgumentNullException from CheckTelerikPresence(telerikUtils={TelerikUtils})", telerikUtils);
        eventLogger.LogDebug("CheckTelerikPresence(telerikUtils={TelerikUtils}): telerikUtils → {TelerikUtils1}", telerikUtils, telerikUtils);
        }

        /// <summary>Initializes a new instance of the <see cref="CheckTelerikPresence"/> class.</summary>
        /// <param name="telerikUtils">
        /// An instance of the <see cref="ITelerikUtils"/> interface.
        /// </param>
        internal CheckTelerikPresence(ITelerikUtils telerikUtils)
            : base()
        {
            this.telerikUtils = telerikUtils ??
                throw new ArgumentNullException(nameof(telerikUtils));
        }

        /// <inheritdoc />
        protected override CheckResult ExecuteInternal()
        {
			eventLogger.LogError(">>> Entering ExecuteInternal()");
            if (this.telerikUtils.TelerikIsInstalled())
            {
                var files = this.telerikUtils.GetAssembliesThatDependOnTelerik();
				eventLogger.LogError("ExecuteInternal(): files → {Files}", files);
                var fileList = files as IList<string> ?? files.ToList();
				eventLogger.LogError("ExecuteInternal(): fileList → {FileList}", fileList);

                if (fileList.Any())
                {
                    return this.InstalledAndUsed(fileList);
                }

                return this.InstalledButNotUsed();
            }

			eventLogger.LogWarning("<<< Exiting ExecuteInternal()");
            return this.NotInstalled();
        }

        private CheckResult InstalledButNotUsed()
        {
			eventLogger.LogError(">>> Entering InstalledButNotUsed()");
            var note = this.GetLocalizedString("InstalledButNotUsed");

			eventLogger.LogError("InstalledButNotUsed(): note → {Note}", note);
			eventLogger.LogWarning("<<< Exiting InstalledButNotUsed()");
            return new CheckResult(SeverityEnum.Failure, this.Id)
            {
                Notes = { note },
            };
        }

        private CheckResult InstalledAndUsed(IEnumerable<string> files)
        {
			eventLogger.LogError(">>> Entering InstalledAndUsed(files={Files})", files);
            var caption = this.GetLocalizedString("InstalledAndUsed");
            var relativeFiles = files.Select(path => path.Substring(this.telerikUtils.BinPath.Length + 1));
			eventLogger.LogError("InstalledAndUsed(files={Files}): relativeFiles → {RelativeFiles}", files, relativeFiles);
            var fileList = string.Join("<br/>", relativeFiles.Select(path => $"* {path}"));
			eventLogger.LogError("InstalledAndUsed(files={Files}): fileList → {FileList}", files, fileList);
            var note = string.Join("<br/>", new[] { caption, string.Empty, fileList });
			eventLogger.LogError("InstalledAndUsed(files={Files}): note → {Note}", files, note);
			eventLogger.LogError("InstalledAndUsed(files={Files}): caption → {Caption}", files, caption);

            return new CheckResult(SeverityEnum.Failure, this.Id)
            {
                Notes = { note },
            };
        }

        private CheckResult NotInstalled()
        {
			eventLogger.LogError(">>> Entering NotInstalled()");
            eventLogger.LogWarning("<<< Exiting NotInstalled()");
            return new CheckResult(SeverityEnum.Pass, this.Id);
        }
    }
}
