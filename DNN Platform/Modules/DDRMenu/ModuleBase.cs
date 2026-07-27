// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu
{
    using System;

    using DotNetNuke.Entities.Modules;

    public class ModuleBase : PortalModuleBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        protected string GetStringSetting(string name, string defaultValue)
        {
			Logger.LogDebug(">>> Entering GetStringSetting(name={Name},defaultValue={DefaultValue})", name, defaultValue);
            var result = this.Request.QueryString[name];
            if (string.IsNullOrEmpty(result))
			Logger.LogDebug("GetStringSetting(name={Name},defaultValue={DefaultValue}): result → {Result}", name, defaultValue, result);
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

			Logger.LogDebug("<<< Exiting GetStringSetting(name={Name},defaultValue={DefaultValue})", name, defaultValue);
			Logger.LogDebug("<<< Exiting GetStringSetting(name={Name})", name);
            return result;
        }

        protected string GetStringSetting(string name)
        {
            return this.GetStringSetting(name, string.Empty);
        }

        protected int GetIntSetting(string name, int defaultValue)
        {
			Logger.LogDebug(">>> Entering GetIntSetting(name={Name},defaultValue={DefaultValue})", name, defaultValue);
			Logger.LogDebug("<<< Exiting GetIntSetting(name={Name},defaultValue={DefaultValue})", name, defaultValue);
			Logger.LogDebug("<<< Exiting GetIntSetting(name={Name})", name);
            return Convert.ToInt32(this.GetStringSetting(name, defaultValue.ToString()));
        }

        protected int GetIntSetting(string name)
        {
            return this.GetIntSetting(name, 0);
        }

        protected bool GetBoolSetting(string name, bool defaultValue)
        {
			Logger.LogDebug(">>> Entering GetBoolSetting(name={Name},defaultValue={DefaultValue})", name, defaultValue);
            var result = this.GetStringSetting(name);
			Logger.LogDebug("GetBoolSetting(name={Name},defaultValue={DefaultValue}): result → {Result}", name, defaultValue, result);
			Logger.LogDebug("<<< Exiting GetBoolSetting(name={Name},defaultValue={DefaultValue})", name, defaultValue);
			Logger.LogDebug("<<< Exiting GetBoolSetting(name={Name})", name);
            return (result == string.Empty) ? defaultValue : Convert.ToBoolean(result);
        }

        protected bool GetBoolSetting(string name)
        {
            return this.GetBoolSetting(name, false);
        }
    }
}
