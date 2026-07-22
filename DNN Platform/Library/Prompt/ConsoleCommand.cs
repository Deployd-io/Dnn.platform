// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Prompt
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Prompt;
    using DotNetNuke.Abstractions.Users;
    using DotNetNuke.Collections;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The implementation of a Prompt command.</summary>
    public abstract class ConsoleCommand : IConsoleCommand
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public abstract string LocalResourceFile { get; }

        /// <inheritdoc />
        public string ValidationMessage { get; private set; }

        /// <summary>Gets the result html.</summary>
        public virtual string ResultHtml => this.LocalizeString($"Prompt_{this.GetType().Name}_ResultHtml");

        /// <summary>Gets the portal settings.</summary>
        protected IPortalSettings PortalSettings { get; private set; }

        /// <summary>Gets the current user.</summary>
        protected IUserInfo User { get; private set; }

        /// <summary>Gets the portal ID.</summary>
        protected int PortalId { get; private set; }

        /// <summary>Gets the tab ID.</summary>
        protected int TabId { get; private set; }

        /// <summary>Gets the raw arguments.</summary>
        protected string[] Args { get; private set; }

        /// <summary>Gets the flag values parsed from <see cref="Args"/>.</summary>
        protected IDictionary<string, string> Flags { get; private set; }

        private static ISerializationManager SerializationManager =>
            Common.Globals.GetCurrentServiceProvider().GetRequiredService<ISerializationManager>();

        /// <inheritdoc />
        public virtual void Initialize(string[] args, IPortalSettings portalSettings, IUserInfo userInfo, int activeTabId)
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
			Logger.LogDebug("<<< Exiting Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
			Logger.LogDebug("Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId}): this.TabId → {Arg4}", args, portalSettings, userInfo, activeTabId, this.TabId);
			Logger.LogDebug("Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId}): this.ValidationMessage → {Arg4}", args, portalSettings, userInfo, activeTabId, this.ValidationMessage);
        }

        /// <inheritdoc />
        public abstract IConsoleResultModel Run();

        /// <inheritdoc />
        public virtual bool IsValid()
        {
			Logger.LogDebug(">>> Entering IsValid()");
			Logger.LogDebug("<<< Exiting IsValid()");
            return string.IsNullOrEmpty(this.ValidationMessage);
        }

        /// <summary>Gets the localized string corresponding to the <paramref name="key"/>.</summary>
        /// <param name="key">The resource key to find.</param>
        /// <returns>The localized Text.</returns>
        protected string LocalizeString(string key)
        {
			Logger.LogDebug(">>> Entering LocalizeString(key={Key})", key);
            var localizedText = Localization.GetString(key, this.LocalResourceFile);
			Logger.LogDebug("LocalizeString(key={Key}): localizedText → {LocalizedText}", key, localizedText);
			Logger.LogDebug("<<< Exiting LocalizeString(key={Key})", key);
            return string.IsNullOrEmpty(localizedText) ? key : localizedText;
        }

        /// <summary>Adds a validation message.</summary>
        /// <param name="message">The message to add.</param>
        protected void AddMessage(string message)
        {
			Logger.LogDebug(">>> Entering AddMessage(message={Message})", message);
            this.ValidationMessage += message;
			Logger.LogDebug("<<< Exiting AddMessage(message={Message})", message);
        }

        /// <summary>Sets the properties from the values in <see cref="Flags"/>.</summary>
        /// <param name="myCommand">This command.</param>
        /// <typeparam name="T">The type of this command.</typeparam>
        protected void ParseParameters<T>(T myCommand)
            where T : class, new()
        {
			Logger.LogDebug(">>> Entering ParseParameters(myCommand={MyCommand})", myCommand);
            ////LoadMapping();
            var mpg = this.CreateMapping();
            mpg.ForEach(mapping =>
            {
			Logger.LogDebug("ParseParameters(myCommand={MyCommand}): mpg → {Mpg}", myCommand, mpg);
                var attribute = mapping.Attribute;
                var property = mapping.Property;
                var settingValue = this.Flags.TryGetValue(attribute.Name, out var flag) ? flag : null;
				Logger.LogDebug("ParseParameters(myCommand={MyCommand}): property → {Property}", myCommand, property);
				Logger.LogDebug("ParseParameters(myCommand={MyCommand}): settingValue → {SettingValue}", myCommand, settingValue);
                if (settingValue != null && property.CanWrite)
                {
                    SerializationManager.DeserializeProperty(myCommand, property, settingValue);
                }
            });
			Logger.LogDebug("<<< Exiting ParseParameters(myCommand={MyCommand})", myCommand);
        }

        /// <summary>Create a mapping of the command parameters to their properties.</summary>
        /// <returns>A list of parameters mapped to their corresponding attributes.</returns>
        protected virtual IList<ParameterMapping> CreateMapping()
        {
			Logger.LogDebug(">>> Entering CreateMapping()");
            var mapping = new List<ParameterMapping>();
            this.GetType().GetProperties().ForEach(property =>
            {
			Logger.LogDebug("CreateMapping(): mapping → {Mapping}", mapping);
                var attributes = property.GetCustomAttributes<ConsoleCommandParameterAttribute>(true);
                attributes.ForEach(attribute => mapping.Add(new ParameterMapping() { Attribute = attribute, Property = property }));
				Logger.LogDebug("CreateMapping(): attributes → {Attributes}", attributes);
            });
			Logger.LogDebug("<<< Exiting CreateMapping()");
            return mapping;
        }

        private static string NormalizeFlagName(string flagName)
        {
            if (flagName == null)
            {
                return string.Empty;
            }

            if (flagName.StartsWith("--", StringComparison.Ordinal))
            {
                flagName = flagName.Substring(2);
            }
			Logger.LogDebug("NormalizeFlagName(flagName={FlagName}): flagName → {FlagName1}", flagName, flagName);

			Logger.LogDebug("<<< Exiting NormalizeFlagName(flagName={FlagName})", flagName);
            return flagName.ToLowerInvariant().Trim();
        }

        private void ParseFlags()
        {
			Logger.LogDebug(">>> Entering ParseFlags()");
            this.Flags = new Dictionary<string, string>();

            // loop through arguments, skipping the first one (the command)
            for (var i = 1; i <= this.Args.Length - 1; i++)
            {
                if (!this.Args[i].StartsWith("--", StringComparison.Ordinal))
                {
                    continue;
                }

                // found a flag
                var flagName = NormalizeFlagName(this.Args[i]);
                var flagValue = string.Empty;
				Logger.LogDebug("ParseFlags(): flagName → {FlagName}", flagName);
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

        /// <summary>A mapping between a <see cref="ConsoleCommandParameterAttribute"/> and its corresponding <see cref="PropertyInfo"/>.</summary>
        public struct ParameterMapping
        {
            /// <summary>Gets or sets the attribute.</summary>
            public ConsoleCommandParameterAttribute Attribute { get; set; }

            /// <summary>Gets or sets the property.</summary>
            public PropertyInfo Property { get; set; }
        }
    }
}
