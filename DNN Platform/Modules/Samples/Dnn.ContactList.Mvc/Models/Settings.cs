// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.ComponentModel;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Dnn.ContactList.Mvc.Models
{
    /// <summary>
    /// Settings class manages the settings for the module instance.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Settings
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>
        /// Settings constructor
        /// </summary>
        public Settings()
        {
			_logger.LogError(">>> Entering Settings()");
            PageSize = 10;
			_logger.LogWarning("<<< Exiting Settings()");
			_logger.LogDebug("Settings(): PageSize → {PageSize}", PageSize);
            AllowContactCreation = true;
			_logger.LogDebug("Settings(): AllowContactCreation → {AllowContactCreation}", AllowContactCreation);
            
        }

        /// <summary>
        /// Number of contacts to show per page.
        /// </summary>
        [DisplayName("Page Size")]
        [JsonProperty("PageSize")]
        public int PageSize { get; set; }

        /// <summary>
        /// Allow users add/edit the contact
        /// </summary>
        [DisplayName("Allow contact creation.")]
        [JsonProperty("AllowContactCreation")]
        public bool AllowContactCreation { get; set; }
    }
}
