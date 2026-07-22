// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.Console.Components
{
    using System;

    using DotNetNuke.Entities.Modules;

    /// <summary>Implements the module's business controller interface(s).</summary>
    public class BusinessController : IUpgradeable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public string UpgradeModule(string version)
        {
			Logger.LogDebug(">>> Entering UpgradeModule(version={Version})", version);
            try
            {
                switch (version)
                {
                    case "08.00.00":

                        break;
                }

                return "Success";
            }
            catch (Exception)
            {
				Logger.LogError("Exception in UpgradeModule(version={Version}): {Arg1}", version, ex.Message);
				Logger.LogDebug("<<< Exiting UpgradeModule(version={Version})", version);
                return "Failed";
            }
        }
    }
}
