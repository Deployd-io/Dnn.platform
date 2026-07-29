// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using Newtonsoft.Json;

namespace Dnn.ContactList.Spa.Services.ViewModels
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SettingsViewModel
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public SettingsViewModel()
        {
			Logger.LogDebug(">>> Entering SettingsViewModel()");
            
			Logger.LogDebug("<<< Exiting SettingsViewModel()");
        }

        [JsonProperty("isFormEnabled")]
        public bool IsFormEnabled { get; set; }
    }
}
