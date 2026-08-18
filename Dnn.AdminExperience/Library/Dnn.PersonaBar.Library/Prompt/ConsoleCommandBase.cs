// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Prompt
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;

    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Localization;

    /// <summary>A Prompt console command.</summary>
    [DnnDeprecated(9, 7, 0, "Moved to DotNetNuke.Prompt in the core library project")]
    public abstract partial class ConsoleCommandBase : IConsoleCommand
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ConsoleCommandBase));
        /// <inheritdoc />
        public abstract string LocalResourceFile { get; }

        /// <summary>Gets resource key for the result html.</summary>
        public virtual string ResultHtml => this.LocalizeString($"Prompt_{this.GetType().Name}_ResultHtml");

        /// <inheritdoc />
        public string ValidationMessage { get; private set; }

        protected PortalSettings PortalSettings { get; private set; }

        protected UserInfo User { get; private set; }

        protected int PortalId { get; private set; }

        protected int TabId { get; private set; }

        protected string[] Args { get; private set; }

        protected Hashtable Flags { get; private set; }

        /// <summary>Get the flag value.</summary>
        /// <typeparam name="T">Type of the output expected.</typeparam>
        /// <param name="flag">Flag name to look.</param>
        /// <param name="fieldName">Filed name to show in message.</param>
        /// <param name="defaultVal">Default value of the flag, if any.</param>
        /// <param name="required">Is this a required flag or not.</param>
        /// <param name="checkmain">Try to find the flag value in first args or not.</param>
        /// <param name="checkpositive">This would be applicable only if the output is of type int or double and value should be positive.</param>
        /// <returns>The flag value or <paramref name="defaultVal"/>.</returns>
        public virtual T GetFlagValue<T>(string flag, string fieldName, T defaultVal, bool required = false, bool checkmain = false, bool checkpositive = false)
        {
			Logger.LogDebug(">>> Entering GetFlagValue(flag={Flag},fieldName={FieldName},defaultVal={DefaultVal},required={Required},checkmain={Checkmain},checkpositive={Checkpositive})", flag, fieldName, defaultVal, required, checkmain, checkpositive);
            const string resourceFile = "~/DesktopModules/admin/Dnn.PersonaBar/App_LocalResources/SharedResources.resx";
            dynamic value = null;
			Logger.LogDebug("GetFlagValue(flag={Flag},fieldName={FieldName},defaultVal={DefaultVal},required={Required},checkmain={Checkmain},checkpositive={Checkpositive}): value → {Value}", flag, fieldName, defaultVal, required, checkmain, checkpositive, value);
            try
            {
                if (this.HasFlag(flag))
                {
                    if (this.IsBoolean<T>())
                    {
                        value = this.Flag<bool>(flag, true);
                    }
                    else
                    {
                        value = this.Flag<T>(flag, defaultVal);
                    }
                }
                else
                {
                    if (checkmain && this.Args.Length >= 2 && !this.IsFlag(this.Args[1]))
                    {
                        var tc = TypeDescriptor.GetConverter(typeof(T));
						Logger.LogDebug("GetFlagValue(flag={Flag},fieldName={FieldName},defaultVal={DefaultVal},required={Required},checkmain={Checkmain},checkpositive={Checkpositive}): tc → {Tc}", flag, fieldName, defaultVal, required, checkmain, checkpositive, tc);
                        value = tc.ConvertFrom(this.Args[1]);
                    }
                    else if (!required)
                    {
                        value = defaultVal;
                    }
                    else
                    {
                        this.ValidationMessage += Localization.GetString(
                            checkmain ? "Promp_MainFlagIsRequired" : "Prompt_FlagIsRequired",
                            resourceFile)?.Replace("[0]", fieldName).Replace("[1]", flag);
                    }
                }
            }
            catch (Exception)
            {
				Logger.LogError("Exception in GetFlagValue(flag={Flag},fieldName={FieldName},defaultVal={DefaultVal},required={Required},checkmain={Checkmain},checkpositive={Checkpositive}): {Arg6}", flag, fieldName, defaultVal, required, checkmain, checkpositive, Exception.Message);
                this.ValidationMessage +=
                    Localization.GetString("Prompt_InvalidType", resourceFile)?
                        .Replace("[0]", fieldName)
                        .Replace("[1]", GetTypeName(typeof(T)));
            }

            if (checkpositive &&
                (typeof(T) == typeof(int) || typeof(T) == typeof(long) || typeof(T) == typeof(int?) ||
                 typeof(T) == typeof(long?)) && value != null && Convert.ToInt32(value) <= 0)
            {
                this.ValidationMessage += Localization.GetString("Promp_PositiveValueRequired", resourceFile)?.Replace("[0]", fieldName);
            }

			Logger.LogDebug("<<< Exiting GetFlagValue(flag={Flag},fieldName={FieldName},defaultVal={DefaultVal},required={Required},checkmain={Checkmain},checkpositive={Checkpositive})", flag, fieldName, defaultVal, required, checkmain, checkpositive);
            return value ?? defaultVal;
        }

        public virtual void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
		Logger.LogDebug(">>> Entering Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
		Logger.LogDebug("<<< Exiting Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public void Initialize(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
			Logger.LogDebug(">>> Entering Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            this.Args = args;
            this.PortalSettings = portalSettings;
			Logger.LogDebug("Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId}): this.Args → {Arg4}", args, portalSettings, userInfo, activeTabId, this.Args);
			Logger.LogDebug("Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId}): this.PortalSettings → {Arg4}", args, portalSettings, userInfo, activeTabId, this.PortalSettings);
            this.User = userInfo;
            this.PortalId = portalSettings.PortalId;
            this.TabId = activeTabId;
			Logger.LogDebug("Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId}): this.User → {Arg4}", args, portalSettings, userInfo, activeTabId, this.User);
			Logger.LogDebug("Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId}): this.PortalId → {Arg4}", args, portalSettings, userInfo, activeTabId, this.PortalId);
            this.ValidationMessage = string.Empty;
            this.ParseFlags();
            this.Init(args, portalSettings, userInfo, activeTabId);
			Logger.LogDebug("<<< Exiting Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public abstract ConsoleResultModel Run();

        /// <inheritdoc />
        public virtual bool IsValid()
        {
			Logger.LogDebug(">>> Entering IsValid()");
			Logger.LogDebug("<<< Exiting IsValid()");
            return string.IsNullOrEmpty(this.ValidationMessage);
        }

        protected bool HasFlag(string flagName)
        {
			Logger.LogDebug(">>> Entering HasFlag(flagName={FlagName})", flagName);
            flagName = NormalizeFlagName(flagName);
			Logger.LogDebug(">>> Entering Flag(flagName={FlagName},defValue={DefValue})", flagName, defValue);
			Logger.LogDebug("Flag(flagName={FlagName},defValue={DefValue}): flagName → {FlagName1}", flagName, defValue, flagName);
			Logger.LogDebug("<<< Exiting Flag(flagName={FlagName},defValue={DefValue})", flagName, defValue);
			Logger.LogDebug("<<< Exiting HasFlag(flagName={FlagName})", flagName);
            return this.Flags.ContainsKey(flagName);
        }

        protected bool IsFlag(object input)
        {
			Logger.LogDebug(">>> Entering IsFlag(input={Input})", input);
            var inputVal = Convert.ToString(input, CultureInfo.InvariantCulture);
			Logger.LogDebug("<<< Exiting IsFlag(input={Input})", input);
            return !string.IsNullOrEmpty(inputVal) && inputVal.StartsWith("--", StringComparison.Ordinal);
        }

        protected string LocalizeString(string key)
        {
			Logger.LogDebug(">>> Entering LocalizeString(key={Key})", key);
            var localizedText = Localization.GetString(key, this.LocalResourceFile);
			Logger.LogDebug("LocalizeString(key={Key}): localizedText → {LocalizedText}", key, localizedText);
			Logger.LogDebug("<<< Exiting LocalizeString(key={Key})", key);
            return string.IsNullOrEmpty(localizedText) ? key : localizedText;
        }

        protected void AddMessage(string message)
        {
			Logger.LogDebug(">>> Entering AddMessage(message={Message})", message);
            this.ValidationMessage += message;
			Logger.LogDebug("<<< Exiting AddMessage(message={Message})", message);
        }

        private static string GetTypeName(Type type)
        {
			Logger.LogDebug(">>> Entering GetTypeName(type={Type})", type);
            if (type.FullName.ToLowerInvariant().Contains("int"))
            {
                return "Integer";
            }

            if (type.FullName.ToLowerInvariant().Contains("double"))
            {
                return "Double";
            }

            if (type.FullName.ToLowerInvariant().Contains("bool"))
            {
                return "Boolean";
            }

            if (type.FullName.ToLowerInvariant().Contains("datetime"))
            {
			Logger.LogDebug("<<< Exiting GetTypeName(type={Type})", type);
                return "DateTime";
            }

            return string.Empty;
        }

        private static string NormalizeFlagName(string flagName)
        {
			Logger.LogDebug(">>> Entering NormalizeFlagName(flagName={FlagName})", flagName);
            if (flagName == null)
            {
                return string.Empty;
            }

            if (flagName.StartsWith("--", StringComparison.Ordinal))
            {
                flagName = flagName.Substring(2);
				Logger.LogDebug("NormalizeFlagName(flagName={FlagName}): flagName → {FlagName1}", flagName, flagName);
            }

			Logger.LogDebug("<<< Exiting NormalizeFlagName(flagName={FlagName})", flagName);
            return flagName.ToLowerInvariant().Trim();
        }

        private void ParseFlags()
        {
			Logger.LogDebug(">>> Entering ParseFlags()");
            this.Flags = new Hashtable();

            // loop through arguments, skipping the first one (the command)
            for (var i = 1; i <= this.Args.Length - 1; i++)
            {
                if (!this.Args[i].StartsWith("--", StringComparison.Ordinal))
                {
                    continue;
                }

                // found a flag
                var flagName = NormalizeFlagName(this.Args[i]);
				Logger.LogDebug("ParseFlags(): flagName → {FlagName}", flagName);
                var flagValue = string.Empty;
				Logger.LogDebug("ParseFlags(): flagValue → {FlagValue}", flagValue);
                if (i < this.Args.Length - 1)
                {
                    if (!string.IsNullOrEmpty(this.Args[i + 1]))
                    {
                        if (this.Args[i + 1].StartsWith("--", StringComparison.Ordinal))
                        {
                            // next value is another flag, so this flag has no value
                            flagValue = string.Empty;
                        }
                        else
                        {
                            flagValue = this.Args[i + 1];
                        }
                    }
                    else
                    {
                        flagValue = string.Empty;
                    }
                }

                this.Flags.Add(flagName.ToLowerInvariant(), flagValue);
            }
			Logger.LogDebug("<<< Exiting ParseFlags()");
        }

        private object Flag<T>(string flagName, T defValue)
        {
            flagName = NormalizeFlagName(flagName);
            if (!this.Flags.ContainsKey(flagName))
            {
                return defValue;
            }

            var retVal = this.Flags[flagName];
            if (retVal == null || (string)retVal == string.Empty)
            {
                return defValue;
            }

            var tc = TypeDescriptor.GetConverter(typeof(T));
            return tc.ConvertFrom(retVal);
        }

        private bool IsBoolean<T>()
        {
			Logger.LogDebug(">>> Entering IsBoolean()");
			Logger.LogDebug("<<< Exiting IsBoolean()");
            return typeof(T) == typeof(bool?);
        }
    }
}
