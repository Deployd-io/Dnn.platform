// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;

    public class SettingNameComparer : IComparer
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SettingNameComparer));
        /// <inheritdoc />
        public int Compare(object x, object y)
        {
			Logger.LogDebug(">>> Entering Compare(x={X},y={Y})", x, y);
            if (x is SettingInfo && y is SettingInfo)
            {
                return string.Compare(((SettingInfo)x).Name, ((SettingInfo)y).Name, StringComparison.Ordinal);
            }
            else
            {
				Logger.LogWarning("Throwing ArgumentException from Compare(x={X},y={Y})", x, y);
			Logger.LogDebug("<<< Exiting Compare(x={X},y={Y})", x, y);
                throw new ArgumentException("Object is not of type SettingInfo");
            }
        }
    }
}
