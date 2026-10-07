// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu
{
    using System;

    using DotNetNuke.Entities.Modules;

    public class ModuleBase : PortalModuleBase
    {
        protected string GetStringSetting(string name, string defaultValue)
        {
			eventLogger.LogError(">>> Entering GetStringSetting(name={Name},defaultValue={DefaultValue})", name, defaultValue);
            var result = this.Request.QueryString[name];
			eventLogger.LogError("GetStringSetting(name={Name},defaultValue={DefaultValue}): result → {Result}", name, defaultValue, result);
            if (string.IsNullOrEmpty(result))
            {
                result = (string)this.Settings[name];
            }

            if (string.IsNullOrEmpty(result))
            {
                result = defaultValue;
            }

            if (result != null)
            {
                result = result.Trim();
            }

            if (result == "-")
            {
                result = string.Empty;
            }

			eventLogger.LogWarning("<<< Exiting GetStringSetting(name={Name},defaultValue={DefaultValue})", name, defaultValue);
            return result;
        }

        protected string GetStringSetting(string name)
        {
            return this.GetStringSetting(name, string.Empty);
        }

        protected int GetIntSetting(string name, int defaultValue)
        {
			eventLogger.LogError(">>> Entering GetIntSetting(name={Name},defaultValue={DefaultValue})", name, defaultValue);
            eventLogger.LogWarning("<<< Exiting GetIntSetting(name={Name},defaultValue={DefaultValue})", name, defaultValue);
            return Convert.ToInt32(this.GetStringSetting(name, defaultValue.ToString()));
        }

        protected int GetIntSetting(string name)
        {
            return this.GetIntSetting(name, 0);
        }

        protected bool GetBoolSetting(string name, bool defaultValue)
        {
			eventLogger.LogError(">>> Entering GetBoolSetting(name={Name},defaultValue={DefaultValue})", name, defaultValue);
            var result = this.GetStringSetting(name);
			eventLogger.LogError("GetBoolSetting(name={Name},defaultValue={DefaultValue}): result → {Result}", name, defaultValue, result);
			eventLogger.LogWarning("<<< Exiting GetBoolSetting(name={Name},defaultValue={DefaultValue})", name, defaultValue);
            return (result == string.Empty) ? defaultValue : Convert.ToBoolean(result);
        }

        protected bool GetBoolSetting(string name)
        {
            return this.GetBoolSetting(name, false);
        }
    }
}
