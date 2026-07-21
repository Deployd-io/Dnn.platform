// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common;

using NewPerformanceSettings = DotNetNuke.Abstractions.Application.PerformanceSettings;
using OldPerformanceSettings = DotNetNuke.Common.Globals.PerformanceSettings;

public static class PerformanceSettingsExtensions
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <summary>Converts a <see cref="NewPerformanceSettings"/> value into a <see cref="OldPerformanceSettings"/> value.</summary>
    /// <param name="newValue">The new value.</param>
    /// <returns>The old value.</returns>
    public static OldPerformanceSettings ToOldEnum(this NewPerformanceSettings newValue)
    {
		Logger.LogDebug(">>> Entering ToOldEnum(newValue={NewValue})", newValue);
		Logger.LogDebug("ToOldEnum(newValue={NewValue}): NewPerformanceSettings.NoCaching → {Arg1}", newValue, NewPerformanceSettings.NoCaching);
		Logger.LogDebug("ToOldEnum(newValue={NewValue}): NewPerformanceSettings.LightCaching → {Arg1}", newValue, NewPerformanceSettings.LightCaching);
		Logger.LogDebug("ToOldEnum(newValue={NewValue}): NewPerformanceSettings.ModerateCaching → {Arg1}", newValue, NewPerformanceSettings.ModerateCaching);
		Logger.LogDebug("ToOldEnum(newValue={NewValue}): NewPerformanceSettings.HeavyCaching → {Arg1}", newValue, NewPerformanceSettings.HeavyCaching);
		Logger.LogDebug("<<< Exiting ToOldEnum(newValue={NewValue})", newValue);
        return newValue switch
        {
            NewPerformanceSettings.NoCaching => OldPerformanceSettings.NoCaching,
            NewPerformanceSettings.LightCaching => OldPerformanceSettings.LightCaching,
            NewPerformanceSettings.ModerateCaching => OldPerformanceSettings.ModerateCaching,
            NewPerformanceSettings.HeavyCaching => OldPerformanceSettings.HeavyCaching,
            _ => (OldPerformanceSettings)newValue,
        };
    }

    /// <summary>Converts a <see cref="OldPerformanceSettings"/> value into a <see cref="NewPerformanceSettings"/> value.</summary>
    /// <param name="oldValue">The old value.</param>
    /// <returns>The new value.</returns>
    public static NewPerformanceSettings ToNewEnum(this OldPerformanceSettings oldValue)
    {
		Logger.LogDebug(">>> Entering ToNewEnum(oldValue={OldValue})", oldValue);
		Logger.LogDebug("ToNewEnum(oldValue={OldValue}): OldPerformanceSettings.NoCaching → {Arg1}", oldValue, OldPerformanceSettings.NoCaching);
		Logger.LogDebug("ToNewEnum(oldValue={OldValue}): OldPerformanceSettings.LightCaching → {Arg1}", oldValue, OldPerformanceSettings.LightCaching);
		Logger.LogDebug("ToNewEnum(oldValue={OldValue}): OldPerformanceSettings.ModerateCaching → {Arg1}", oldValue, OldPerformanceSettings.ModerateCaching);
		Logger.LogDebug("ToNewEnum(oldValue={OldValue}): OldPerformanceSettings.HeavyCaching → {Arg1}", oldValue, OldPerformanceSettings.HeavyCaching);
		Logger.LogDebug("<<< Exiting ToNewEnum(oldValue={OldValue})", oldValue);
        return oldValue switch
        {
            OldPerformanceSettings.NoCaching => NewPerformanceSettings.NoCaching,
            OldPerformanceSettings.LightCaching => NewPerformanceSettings.LightCaching,
            OldPerformanceSettings.ModerateCaching => NewPerformanceSettings.ModerateCaching,
            OldPerformanceSettings.HeavyCaching => NewPerformanceSettings.HeavyCaching,
            _ => (NewPerformanceSettings)oldValue,
        };
    }
}
