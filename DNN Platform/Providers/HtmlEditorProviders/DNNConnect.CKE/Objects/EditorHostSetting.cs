// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Objects
{
    using System;

    /// <summary>A Editor Host Setting Item.</summary>
    [Serializable]
    public class EditorHostSetting
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="EditorHostSetting"/> class.</summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        public EditorHostSetting(string settingName, string settingValue)
        {
			Logger.LogDebug(">>> Entering EditorHostSetting(settingName={SettingName},settingValue={SettingValue})", settingName, settingValue);
            this.Name = settingName;
            this.Value = settingValue;
			Logger.LogDebug("<<< Exiting EditorHostSetting(settingName={SettingName},settingValue={SettingValue})", settingName, settingValue);
			Logger.LogDebug("EditorHostSetting(settingName={SettingName},settingValue={SettingValue}): this.Name → {Arg2}", settingName, settingValue, this.Name);
			Logger.LogDebug("EditorHostSetting(settingName={SettingName},settingValue={SettingValue}): this.Value → {Arg2}", settingName, settingValue, this.Value);
        }

        /// <summary>Gets or sets the setting name.</summary>
        /// <value>
        /// The setting name.
        /// </value>
        public string Name { get; set; }

        /// <summary>Gets or sets the setting value.</summary>
        /// <value>
        /// The setting value.
        /// </value>
        public string Value { get; set; }
    }
}
