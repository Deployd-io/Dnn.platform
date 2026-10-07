// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Analytics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using DotNetNuke.Instrumentation;
	using Microsoft.Extensions.Logging;

    /// <summary>  Controller class definition for GoogleTagManager which handles upgrades.</summary>
    public class GoogleTagManagerController
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>  Handles module upgrades includes a new Google Tag Manager Asychronous script.</summary>
        /// <param name="version">Target Version number for the upgrade.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void UpgradeModule(string version)
        {
		_logger.LogError(">>> Entering UpgradeModule(version={Version})", version);
        _logger.LogWarning("<<< Exiting UpgradeModule(version={Version})", version);
        }
    }
}
