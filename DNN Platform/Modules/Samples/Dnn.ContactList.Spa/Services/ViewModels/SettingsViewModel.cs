// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Dnn.ContactList.Spa.Services.ViewModels
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SettingsViewModel
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public SettingsViewModel()
        {
			_logger.LogError(">>> Entering SettingsViewModel()");
            
			_logger.LogWarning("<<< Exiting SettingsViewModel()");
        }

        [JsonProperty("isFormEnabled")]
        public bool IsFormEnabled { get; set; }
    }
}
