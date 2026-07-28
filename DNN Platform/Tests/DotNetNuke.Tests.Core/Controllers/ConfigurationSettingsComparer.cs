// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities;

    public class ConfigurationSettingsComparer : EqualityComparer<ConfigurationSetting>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public override bool Equals(ConfigurationSetting x, ConfigurationSetting y)
        {
			Logger.LogDebug(">>> Entering Equals(x={X},y={Y})", x, y);
            if (x.Key == y.Key && x.Value == y.Value && x.IsSecure == y.IsSecure)
            {
                return true;
            }

			Logger.LogDebug("<<< Exiting Equals(x={X},y={Y})", x, y);
            return false;
        }

        public override int GetHashCode(ConfigurationSetting obj)
        {
			Logger.LogDebug(">>> Entering GetHashCode(obj={Obj})", obj);
            throw new NotImplementedException();
        }
    }
}
