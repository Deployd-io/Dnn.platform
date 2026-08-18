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
        public override bool Equals(ConfigurationSetting x, ConfigurationSetting y)
        {
			mockEventLogger.LogDebug(">>> Entering Equals(x={X},y={Y})", x, y);
            if (x.Key == y.Key && x.Value == y.Value && x.IsSecure == y.IsSecure)
            {
			mockEventLogger.LogDebug("<<< Exiting Equals(x={X},y={Y})", x, y);
                return true;
            }

            return false;
        }

        public override int GetHashCode(ConfigurationSetting obj)
        {
			mockEventLogger.LogDebug(">>> Entering GetHashCode(obj={Obj})", obj);
			mockEventLogger.LogWarning("Throwing NotImplementedException from GetHashCode(obj={Obj})", obj);
			mockEventLogger.LogDebug("<<< Exiting GetHashCode(obj={Obj})", obj);
            throw new NotImplementedException();
        }
    }
}
