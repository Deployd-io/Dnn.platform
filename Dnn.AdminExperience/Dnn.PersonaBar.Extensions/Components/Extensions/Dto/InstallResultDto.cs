// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Services.Installer.Log;
    using Newtonsoft.Json;

    [JsonObject]
    public class InstallResultDto
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [JsonProperty("newPackageId")]
        public int NewPackageId { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; } = true;

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("logs")]
        public IList<InstallerLogEntry> Logs { get; set; } = new List<InstallerLogEntry>();

        public void Failed(string message)
        {
			Logger.LogDebug(">>> Entering Failed(message={Message})", message);
            this.Success = false;
            this.Message = message;
			Logger.LogDebug("<<< Exiting Failed(message={Message})", message);
			Logger.LogDebug("Failed(message={Message}): this.Success → {Arg1}", message, this.Success);
			Logger.LogDebug("Failed(message={Message}): this.Message → {Arg1}", message, this.Message);
        }

        public void Succeed()
        {
			Logger.LogDebug(">>> Entering Succeed()");
            this.Success = true;
            this.Message = string.Empty;
			Logger.LogDebug("<<< Exiting Succeed()");
			Logger.LogDebug("Succeed(): this.Success → {Arg0}", this.Success);
			Logger.LogDebug("Succeed(): this.Message → {Arg0}", this.Message);
        }

        public void AddLogs(IEnumerable<LogEntry> logs)
        {
			Logger.LogDebug(">>> Entering AddLogs(logs={Logs})", logs);
            this.Logs = logs?.Select(
                l => new InstallerLogEntry
                {
                    Type = l.Type.ToString(),
                    Description = l.Description,
                }).ToList() ?? new List<InstallerLogEntry>();
				Logger.LogDebug("<<< Exiting AddLogs(logs={Logs})", logs);
        }
    }
}
