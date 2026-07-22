// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Profile
{
    using System;
    using System.Globalization;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The DNNProfileProvider overrides the default ProfileProvider to provide a purely DotNetNuke implementation.</summary>
    public class DNNProfileProvider : ProfileProvider
    {
        private readonly IEventLogger eventLogger;
        private readonly DataProvider dataProvider;

        /// <summary>Initializes a new instance of the <see cref="DNNProfileProvider"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public DNNProfileProvider()
            : this(null, null)
        {
		eventLogger.LogDebug(">>> Entering DNNProfileProvider()");
		eventLogger.LogDebug("<<< Exiting DNNProfileProvider()");
		eventLogger.LogDebug(">>> Entering DNNProfileProvider(eventLogger={EventLogger},dataProvider={DataProvider})", eventLogger, dataProvider);
		eventLogger.LogDebug("<<< Exiting DNNProfileProvider(eventLogger={EventLogger},dataProvider={DataProvider})", eventLogger, dataProvider);
        }

        /// <summary>Initializes a new instance of the <see cref="DNNProfileProvider"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="dataProvider">The data provider.</param>
        public DNNProfileProvider(IEventLogger eventLogger, DataProvider dataProvider)
        {
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
            this.dataProvider = dataProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>();
			eventLogger.LogDebug("DNNProfileProvider(eventLogger={EventLogger},dataProvider={DataProvider}): dataProvider → {DataProvider1}", eventLogger, dataProvider, dataProvider);
			eventLogger.LogDebug("DNNProfileProvider(eventLogger={EventLogger},dataProvider={DataProvider}): eventLogger → {EventLogger1}", eventLogger, dataProvider, eventLogger);
        }

        /// <summary>Gets a value indicating whether the Provider Properties can be edited.</summary>
        public override bool CanEditProviderProperties => true;

        /// <summary>GetUserProfile retrieves the UserProfile information from the Data Store.</summary>
        /// <param name="user">The user whose Profile information we are retrieving.</param>
        public override void GetUserProfile(ref UserInfo user)
        {
			eventLogger.LogDebug(">>> Entering GetUserProfile(user={User})", user);
            ProfilePropertyDefinition profProperty;

            int portalId = user.IsSuperUser ? Globals.glbSuperUserAppName : user.PortalID;
            var properties = ProfileController.GetPropertyDefinitionsByPortal(portalId, true, false);

			eventLogger.LogDebug("GetUserProfile(user={User}): properties → {Properties}", user, properties);
            // Load the Profile properties
            if (user.UserID > Null.NullInteger)
            {
                var key = GetProfileCacheKey(user);
                var cachedProperties = (ProfilePropertyDefinitionCollection)DataCache.GetCache(key);
				eventLogger.LogWarning("GetUserProfile(user={User}): key → {Key}", user, key);
				eventLogger.LogDebug("GetUserProfile(user={User}): cachedProperties → {CachedProperties}", user, cachedProperties);
                if (cachedProperties != null)
                {
                    properties = cachedProperties;
                }
                else
                {
                    using (var dr = this.dataProvider.GetUserProfile(user.UserID))
                    {
                        while (dr.Read())
                        {
                            // Ensure the data reader returned is valid
                            if (!string.Equals(dr.GetName(0), "ProfileID", StringComparison.OrdinalIgnoreCase))
                            {
                                break;
                            }

                            int definitionId = Convert.ToInt32(dr["PropertyDefinitionId"], CultureInfo.InvariantCulture);
                            profProperty = properties.GetById(definitionId);
							eventLogger.LogDebug("GetUserProfile(user={User}): definitionId → {DefinitionId}", user, definitionId);
							eventLogger.LogDebug("GetUserProfile(user={User}): profProperty → {ProfProperty}", user, profProperty);
                            if (profProperty != null)
                            {
                                profProperty.PropertyValue = Convert.ToString(dr["PropertyValue"], CultureInfo.InvariantCulture);
                                var extendedVisibility = string.Empty;
                                var schemaTable = dr.GetSchemaTable();
								eventLogger.LogDebug("GetUserProfile(user={User}): extendedVisibility → {ExtendedVisibility}", user, extendedVisibility);
								eventLogger.LogDebug("GetUserProfile(user={User}): schemaTable → {SchemaTable}", user, schemaTable);
                                if (schemaTable != null && schemaTable.Select("ColumnName = 'ExtendedVisibility'").Length > 0)
                                {
                                    extendedVisibility = Convert.ToString(dr["ExtendedVisibility"], CultureInfo.InvariantCulture);
                                }

                                profProperty.ProfileVisibility = new ProfileVisibility(portalId, extendedVisibility)
                                {
                                    VisibilityMode = (UserVisibilityMode)dr["Visibility"],
                                };
                            }
                        }

                        if (properties.Count > 0)
                        {
                            DataCache.SetCache(key, properties, TimeSpan.FromMinutes(DataCache.UserProfileCacheTimeOut));
                        }
                    }
                }
            }

            // Clear the profile
            user.Profile.ProfileProperties.Clear();

            // Add the properties to the profile
            foreach (ProfilePropertyDefinition property in properties)
            {
                profProperty = property;
                if (string.IsNullOrEmpty(profProperty.PropertyValue) && !string.IsNullOrEmpty(profProperty.DefaultValue))
                {
                    profProperty.PropertyValue = profProperty.DefaultValue;
					eventLogger.LogDebug("GetUserProfile(user={User}): profProperty.PropertyValue → {Arg1}", user, profProperty.PropertyValue);
                }

                user.Profile.ProfileProperties.Add(profProperty);
            }

            // Clear IsDirty Flag
            user.Profile.ClearIsDirty();

            // Ensure old and new TimeZone properties are in synch
            this.UpdateTimeZoneInfo(user, properties);
			eventLogger.LogDebug("<<< Exiting GetUserProfile(user={User})", user);
        }

        /// <summary>UpdateUserProfile persists a user's Profile to the Data Store.</summary>
        /// <param name="user">The user to persist to the Data Store.</param>
        public override void UpdateUserProfile(UserInfo user)
        {
			eventLogger.LogDebug(">>> Entering UpdateUserProfile(user={User})", user);
            var key = GetProfileCacheKey(user);
            DataCache.ClearCache(key);
			eventLogger.LogWarning("UpdateUserProfile(user={User}): key → {Key}", user, key);

            ProfilePropertyDefinitionCollection properties = user.Profile.ProfileProperties;

            // Ensure old and new TimeZone properties are in synch
            var newTimeZone = properties["PreferredTimeZone"];
            var oldTimeZone = properties["TimeZone"];
			eventLogger.LogDebug("UpdateUserProfile(user={User}): newTimeZone → {NewTimeZone}", user, newTimeZone);
			eventLogger.LogDebug("UpdateUserProfile(user={User}): oldTimeZone → {OldTimeZone}", user, oldTimeZone);
            if (oldTimeZone != null && newTimeZone != null)
            { // preference given to new property, if new is changed then old should be updated as well.
                if (newTimeZone.IsDirty && !string.IsNullOrEmpty(newTimeZone.PropertyValue))
                {
                    var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(newTimeZone.PropertyValue);
                    if (timeZoneInfo != null)
					eventLogger.LogDebug("UpdateUserProfile(user={User}): timeZoneInfo → {TimeZoneInfo}", user, timeZoneInfo);
                    {
                        oldTimeZone.PropertyValue = timeZoneInfo.BaseUtcOffset.TotalMinutes.ToString(CultureInfo.InvariantCulture);
                    }
                }

                // however if old is changed, we need to update new as well
                else if (oldTimeZone.IsDirty)
                {
                    if (int.TryParse(oldTimeZone.PropertyValue, out var oldOffset))
                    {
                        newTimeZone.PropertyValue = Localization.ConvertLegacyTimeZoneOffsetToTimeZoneInfo(oldOffset).Id;
                    }
                }
            }

            foreach (ProfilePropertyDefinition profProperty in properties)
            {
                if (profProperty.PropertyValue != null && profProperty.IsDirty)
                {
                    var objSecurity = PortalSecurity.Instance;
#pragma warning disable CS0618 // Type or member is obsolete
                    string propertyValue = objSecurity.InputFilter(profProperty.PropertyValue, PortalSecurity.FilterFlag.NoScripting);
#pragma warning restore CS0618 // Type or member is obsolete
                    this.dataProvider.UpdateProfileProperty(
                        Null.NullInteger,
                        user.UserID,
                        profProperty.PropertyDefinitionId,
                        propertyValue,
                        (int)profProperty.ProfileVisibility.VisibilityMode,
                        profProperty.ProfileVisibility.ExtendedVisibilityString(),
                        DateTime.Now);
                    this.eventLogger.AddLog(user, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, "USERPROFILE_UPDATED");
                }
            }
			eventLogger.LogDebug("<<< Exiting UpdateUserProfile(user={User})", user);
        }

        private static string GetProfileCacheKey(UserInfo user)
        {
			eventLogger.LogDebug(">>> Entering GetProfileCacheKey(user={User})", user);
			eventLogger.LogDebug("<<< Exiting GetProfileCacheKey(user={User})", user);
            return string.Format(CultureInfo.InvariantCulture, DataCache.UserProfileCacheKey, user.PortalID, user.Username);
        }

        private void UpdateTimeZoneInfo(UserInfo user, ProfilePropertyDefinitionCollection properties)
        {
            ProfilePropertyDefinition newTimeZone = properties["PreferredTimeZone"];
            ProfilePropertyDefinition oldTimeZone = properties["TimeZone"];
            if (newTimeZone != null && oldTimeZone != null)
            {
                // Old timezone is present but new is not...we will set that up.
                if (!string.IsNullOrEmpty(oldTimeZone.PropertyValue) && string.IsNullOrEmpty(newTimeZone.PropertyValue))
                {
                    if (int.TryParse(oldTimeZone.PropertyValue, out var oldOffset))
                    {
                        TimeZoneInfo timeZoneInfo = Localization.ConvertLegacyTimeZoneOffsetToTimeZoneInfo(oldOffset);
                        newTimeZone.PropertyValue = timeZoneInfo.Id;
                        this.UpdateUserProfile(user);
                    }
                }

                // It's also possible that the new value is set but not the old value. We need to make them backwards compatible
                else if (!string.IsNullOrEmpty(newTimeZone.PropertyValue) && string.IsNullOrEmpty(oldTimeZone.PropertyValue))
                {
                    TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(newTimeZone.PropertyValue);
                    if (timeZoneInfo != null)
                    {
                        oldTimeZone.PropertyValue = timeZoneInfo.BaseUtcOffset.TotalMinutes.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
        }
    }
}
