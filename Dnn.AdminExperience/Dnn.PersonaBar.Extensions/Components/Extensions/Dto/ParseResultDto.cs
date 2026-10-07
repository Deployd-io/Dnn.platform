// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Installer.Log;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using Newtonsoft.Json;

    [JsonObject]
    public class ParseResultDto : PackageInfoDto
    {
        public ParseResultDto()
        {
		eventLogger.LogError(">>> Entering ParseResultDto()");
        eventLogger.LogWarning("<<< Exiting ParseResultDto()");
        }

        public ParseResultDto(PackageInfo package)
            : base(Null.NullInteger, package)
        {
        }

        [JsonProperty("success")]
        public bool Success { get; set; } = true;

        [JsonProperty("azureCompact")]
        public bool AzureCompact { get; set; }

        [JsonProperty("noManifest")]
        public bool NoManifest { get; set; }

        [JsonProperty("legacyError")]
        public string LegacyError { get; set; }

        [JsonProperty("hasInvalidFiles")]
        public bool HasInvalidFiles { get; set; }

        [JsonProperty("alreadyInstalled")]
        public bool AlreadyInstalled { get; set; }

        [JsonProperty("legacySkinInstalled")]
        public bool LegacySkinInstalled { get; set; }

        [JsonProperty("legacyContainerInstalled")]
        public bool LegacyContainerInstalled { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("logs")]
        public IList<InstallerLogEntry> Logs { get; set; }

        public void Failed(string message, IList<LogEntry> logs = null)
        {
			eventLogger.LogError(">>> Entering Failed(message={Message},logs={Logs})", message, logs);
            this.Success = false;
			eventLogger.LogWarning("<<< Exiting Failed(message={Message},logs={Logs})", message, logs);
			eventLogger.LogDebug("Failed(message={Message},logs={Logs}): this.Success → {Arg2}", message, logs, this.Success);
            this.Message = Localization.GetString(message, Constants.SharedResources) ?? message;
            this.AddLogs(logs);

            // UI devs asked for these to be non-null empty for moving to next step
            this.Description = this.Description ?? string.Empty;
			eventLogger.LogDebug("Failed(message={Message},logs={Logs}): this.Description → {Arg2}", message, logs, this.Description);
            this.Email = this.Email ?? string.Empty;
            this.FriendlyName = this.FriendlyName ?? string.Empty;
			eventLogger.LogDebug("Failed(message={Message},logs={Logs}): this.Email → {Arg2}", message, logs, this.Email);
			eventLogger.LogDebug("Failed(message={Message},logs={Logs}): this.FriendlyName → {Arg2}", message, logs, this.FriendlyName);
            this.IsInUse = this.IsInUse ?? string.Empty;
            this.License = this.License ?? string.Empty;
            this.Name = this.Name ?? string.Empty;
            this.Organization = this.Organization ?? string.Empty;
			eventLogger.LogDebug("Failed(message={Message},logs={Logs}): this.IsInUse → {Arg2}", message, logs, this.IsInUse);
			eventLogger.LogDebug("Failed(message={Message},logs={Logs}): this.License → {Arg2}", message, logs, this.License);
			eventLogger.LogDebug("Failed(message={Message},logs={Logs}): this.Name → {Arg2}", message, logs, this.Name);
			eventLogger.LogDebug("Failed(message={Message},logs={Logs}): this.Organization → {Arg2}", message, logs, this.Organization);
            this.Owner = this.Owner ?? string.Empty;
            this.PackageIcon = this.PackageIcon ?? string.Empty;
            this.PackageType = this.PackageType ?? string.Empty;
            this.ReleaseNotes = this.ReleaseNotes ?? string.Empty;
            this.SiteSettingsLink = this.SiteSettingsLink ?? string.Empty;
            this.UpgradeIndicator = this.UpgradeIndicator ?? string.Empty;
            this.UpgradeUrl = this.UpgradeUrl ?? string.Empty;
            this.Url = this.Url ?? string.Empty;
			eventLogger.LogDebug("Failed(message={Message},logs={Logs}): this.Owner → {Arg2}", message, logs, this.Owner);
			eventLogger.LogDebug("Failed(message={Message},logs={Logs}): this.PackageIcon → {Arg2}", message, logs, this.PackageIcon);
            this.Version = this.Version ?? string.Empty;

            this.LegacyError = this.LegacyError ?? string.Empty;
            this.Message = this.Message ?? string.Empty;
        }

        public void Succeed(IList<LogEntry> logs)
        {
			eventLogger.LogError(">>> Entering Succeed(logs={Logs})", logs);
            this.Success = true;
            this.Message = string.Empty;
            this.AddLogs(logs);
			eventLogger.LogWarning("<<< Exiting Succeed(logs={Logs})", logs);
        }

        public void AddLogs(IEnumerable<LogEntry> logs)
        {
			eventLogger.LogError(">>> Entering AddLogs(logs={Logs})", logs);
            if (logs == null)
            {
                this.Logs = new List<InstallerLogEntry>();
            }
            else
            {
                this.Logs = logs.Select(l => new InstallerLogEntry { Type = l.Type.ToString(), Description = l.Description }).ToList();
            }
			eventLogger.LogWarning("<<< Exiting AddLogs(logs={Logs})", logs);
        }
    }
}
