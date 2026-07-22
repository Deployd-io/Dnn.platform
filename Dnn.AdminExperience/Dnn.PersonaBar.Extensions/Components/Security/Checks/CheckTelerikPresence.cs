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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly ITelerikUtils telerikUtils;

        /// <summary>Initializes a new instance of the <see cref="CheckTelerikPresence"/> class.</summary>
        public CheckTelerikPresence()
            : this(Globals.GetCurrentServiceProvider().GetRequiredService<ITelerikUtils>())
        {
		Logger.LogDebug(">>> Entering CheckTelerikPresence()");
		Logger.LogDebug("<<< Exiting CheckTelerikPresence()");
        }

        /// <summary>Initializes a new instance of the <see cref="CheckTelerikPresence"/> class.</summary>
        /// <param name="telerikUtils">
        /// An instance of the <see cref="ITelerikUtils"/> interface.
        /// </param>
        internal CheckTelerikPresence(ITelerikUtils telerikUtils)
            : base()
        {
            this.telerikUtils = telerikUtils ??
				Logger.LogDebug("CheckTelerikPresence(telerikUtils={TelerikUtils}): telerikUtils → {TelerikUtils1}", telerikUtils, telerikUtils);
				Logger.LogDebug("CheckTelerikPresence(telerikUtils={TelerikUtils}): this.telerikUtils → {Arg1}", telerikUtils, this.telerikUtils);
                throw new ArgumentNullException(nameof(telerikUtils));
        }

        /// <inheritdoc />
        protected override CheckResult ExecuteInternal()
        {
			Logger.LogDebug(">>> Entering ExecuteInternal()");
            if (this.telerikUtils.TelerikIsInstalled())
            {
                var files = this.telerikUtils.GetAssembliesThatDependOnTelerik();
                var fileList = files as IList<string> ?? files.ToList();
				Logger.LogDebug("ExecuteInternal(): files → {Files}", files);
				Logger.LogDebug("ExecuteInternal(): fileList → {FileList}", fileList);

                if (fileList.Any())
                {
                    return this.InstalledAndUsed(fileList);
                }

                return this.InstalledButNotUsed();
            }

			Logger.LogDebug("<<< Exiting ExecuteInternal()");
            return this.NotInstalled();
        }

        private CheckResult InstalledButNotUsed()
        {
			Logger.LogDebug(">>> Entering InstalledButNotUsed()");
            var note = this.GetLocalizedString("InstalledButNotUsed");

			Logger.LogDebug("InstalledButNotUsed(): note → {Note}", note);
			Logger.LogDebug("<<< Exiting InstalledButNotUsed()");
            return new CheckResult(SeverityEnum.Failure, this.Id)
            {
                Notes = { note },
            };
        }

        private CheckResult InstalledAndUsed(IEnumerable<string> files)
        {
            var caption = this.GetLocalizedString("InstalledAndUsed");
            var relativeFiles = files.Select(path => path.Substring(this.telerikUtils.BinPath.Length + 1));
            var fileList = string.Join("<br/>", relativeFiles.Select(path => $"* {path}"));
            var note = string.Join("<br/>", new[] { caption, string.Empty, fileList });

            return new CheckResult(SeverityEnum.Failure, this.Id)
            {
                Notes = { note },
            };
        }

        private CheckResult NotInstalled()
        {
			Logger.LogDebug(">>> Entering NotInstalled()");
			Logger.LogDebug("<<< Exiting NotInstalled()");
            return new CheckResult(SeverityEnum.Pass, this.Id);
        }
    }
}
