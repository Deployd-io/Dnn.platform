// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Personalization
{
    using System;
    using System.Globalization;
    using System.Web;

    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;

    using Microsoft.Extensions.DependencyInjection;

    using static DotNetNuke.Entities.Portals.PortalSettings;

    /// <summary>Provides access to user personalization.</summary>
    public partial class Personalization
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Personalization));
        /// <summary>load users profile and extract value base on naming container and key.</summary>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <returns>The profile as an object.</returns>
        public static object GetProfile(string namingContainer, string key)
        {
			Logger.LogError(">>> Entering GetProfile(namingContainer={NamingContainer},key={Key})", namingContainer, key);
            Logger.LogWarning("<<< Exiting GetProfile(namingContainer={NamingContainer},key={Key})", namingContainer, key);
            return GetProfile(personalizationController: null, namingContainer, key);
        }

        /// <summary>load users profile and extract value base on naming container and key.</summary>
        /// <param name="personalizationController">The personalization controller.</param>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <returns>The profile as an object.</returns>
        public static object GetProfile(PersonalizationController personalizationController, string namingContainer, string key)
        {
            return GetProfile(LoadProfile(personalizationController), namingContainer, key);
        }

        /// <summary>extract value base on naming container and key from PersonalizationInfo object.</summary>
        /// <param name="personalization">Object containing user personalization info.</param>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <returns>The profile as an object.</returns>
        public static object GetProfile(PersonalizationInfo personalization, string namingContainer, string key)
        {
            return personalization != null ? personalization.Profile[namingContainer + ":" + key] : string.Empty;
        }

        /// <summary>load users profile and extract secure value base on naming container and key.</summary>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <returns>The profile as an object.</returns>
        public static object GetSecureProfile(string namingContainer, string key)
        {
			Logger.LogError(">>> Entering GetSecureProfile(namingContainer={NamingContainer},key={Key})", namingContainer, key);
            Logger.LogWarning("<<< Exiting GetSecureProfile(namingContainer={NamingContainer},key={Key})", namingContainer, key);
			Logger.LogWarning("<<< Exiting GetSecureProfile(cryptographyProvider={CryptographyProvider},personalization={Personalization},namingContainer={NamingContainer},key={Key})", cryptographyProvider, personalization, namingContainer, key);
            return GetSecureProfile(personalizationController: null, namingContainer, key);
        }

        /// <summary>load users profile and extract secure value base on naming container and key.</summary>
        /// <param name="personalizationController">The personalization controller.</param>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <returns>The profile as an object.</returns>
        public static object GetSecureProfile(PersonalizationController personalizationController, string namingContainer, string key)
        {
            return GetSecureProfile(LoadProfile(personalizationController), namingContainer, key);
        }

        /// <summary>
        /// extract value base on naming container and key from PersonalizationInfo object
        /// function will automatically decrypt value to plaintext.
        /// </summary>
        /// <param name="personalization">Object containing user personalization info.</param>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <returns>The profile as an object.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking ICryptographyProvider")]
        public static partial object GetSecureProfile(PersonalizationInfo personalization, string namingContainer, string key)
            => GetSecureProfile(Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>(), personalization, namingContainer, key);

        /// <summary>
        /// extract value base on naming container and key from PersonalizationInfo object
        /// function will automatically decrypt value to plaintext.
        /// </summary>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        /// <param name="personalization">Object containing user personalization info.</param>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <returns>The profile as an object.</returns>
        public static object GetSecureProfile(ICryptographyProvider cryptographyProvider, PersonalizationInfo personalization, string namingContainer, string key)
        {
            if (personalization != null)
            {
                return cryptographyProvider.DecryptString(
                    personalization.Profile[$"{namingContainer}:{key}"].ToString(),
                    Config.GetDecryptionkey(),
                    personalization.Profile[$"{namingContainer}:{key}:algorithmName"] as string,
                    personalization.Profile[$"{namingContainer}:{key}:initializationVector"] as string);
            }

            return string.Empty;
        }

        /// <summary>
        /// remove value from profile
        /// uses <paramref name="namingContainer"/> and <paramref name="key"/> to locate appropriate value.
        /// </summary>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        public static void RemoveProfile(string namingContainer, string key)
        {
			Logger.LogError(">>> Entering RemoveProfile(namingContainer={NamingContainer},key={Key})", namingContainer, key);
            RemoveProfile(personalizationController: null, namingContainer, key);
			Logger.LogWarning("<<< Exiting RemoveProfile(namingContainer={NamingContainer},key={Key})", namingContainer, key);
        }

        /// <summary>
        /// remove value from profile
        /// uses <paramref name="namingContainer"/> and <paramref name="key"/> to locate appropriate value.
        /// </summary>
        /// <param name="personalizationController">The personalization controller.</param>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        public static void RemoveProfile(PersonalizationController personalizationController, string namingContainer, string key)
        {
			Logger.LogError(">>> Entering RemoveProfile(personalizationController={PersonalizationController},namingContainer={NamingContainer},key={Key})", personalizationController, namingContainer, key);
            RemoveProfile(LoadProfile(personalizationController), namingContainer, key);
			Logger.LogWarning("<<< Exiting RemoveProfile(personalizationController={PersonalizationController},namingContainer={NamingContainer},key={Key})", personalizationController, namingContainer, key);
        }

        /// <summary>
        /// remove value from users PersonalizationInfo object (if it exists)
        /// uses namingcontainer and key to locate approriate value.
        /// </summary>
        /// <param name="personalization">Object containing user personalization info.</param>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        public static void RemoveProfile(PersonalizationInfo personalization, string namingContainer, string key)
        {
			Logger.LogError(">>> Entering RemoveProfile(personalization={Personalization},namingContainer={NamingContainer},key={Key})", personalization, namingContainer, key);
            if (personalization != null)
            {
                personalization.Profile.Remove(namingContainer + ":" + key);
                personalization.IsModified = true;
				Logger.LogDebug("RemoveProfile(personalization={Personalization},namingContainer={NamingContainer},key={Key}): personalization.IsModified → {Arg3}", personalization, namingContainer, key, personalization.IsModified);
            }
			Logger.LogWarning("<<< Exiting RemoveProfile(personalization={Personalization},namingContainer={NamingContainer},key={Key})", personalization, namingContainer, key);
        }

        /// <summary>persist profile value -use naming container and key to organize.</summary>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <param name="value">Individual profile value.</param>
        public static void SetProfile(string namingContainer, string key, object value)
        {
			Logger.LogError(">>> Entering SetProfile(namingContainer={NamingContainer},key={Key},value={Value})", namingContainer, key, value);
            SetProfile(personalizationController: null, namingContainer, key, value);
			Logger.LogWarning("<<< Exiting SetProfile(namingContainer={NamingContainer},key={Key},value={Value})", namingContainer, key, value);
        }

        /// <summary>persist profile value -use naming container and key to organize.</summary>
        /// <param name="personalizationController">The personalization controller.</param>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <param name="value">Individual profile value.</param>
        public static void SetProfile(PersonalizationController personalizationController, string namingContainer, string key, object value)
        {
			Logger.LogError(">>> Entering SetProfile(personalizationController={PersonalizationController},namingContainer={NamingContainer},key={Key},value={Value})", personalizationController, namingContainer, key, value);
            SetProfile(LoadProfile(personalizationController), namingContainer, key, value);
			Logger.LogWarning("<<< Exiting SetProfile(personalizationController={PersonalizationController},namingContainer={NamingContainer},key={Key},value={Value})", personalizationController, namingContainer, key, value);
        }

        /// <summary>persist value stored in PersonalizationInfo obhect - use naming container and key to organize.</summary>
        /// <param name="personalization">Object containing user personalization info.</param>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <param name="value">Individual profile value.</param>
        public static void SetProfile(PersonalizationInfo personalization, string namingContainer, string key, object value)
        {
			Logger.LogError(">>> Entering SetProfile(personalization={Personalization},namingContainer={NamingContainer},key={Key},value={Value})", personalization, namingContainer, key, value);
            if (personalization != null)
            {
                personalization.Profile[namingContainer + ":" + key] = value;
                personalization.IsModified = true;
				Logger.LogDebug("SetProfile(personalization={Personalization},namingContainer={NamingContainer},key={Key},value={Value}): personalization.IsModified → {Arg4}", personalization, namingContainer, key, value, personalization.IsModified);
            }
			Logger.LogWarning("<<< Exiting SetProfile(personalization={Personalization},namingContainer={NamingContainer},key={Key},value={Value})", personalization, namingContainer, key, value);
        }

        /// <summary>
        /// persist profile value -use naming container and key to organize
        /// function calls an overload which automatically encrypts the value.
        /// </summary>
        /// <param name="namingContainer">Object containing user personalization info.</param>
        /// <param name="key">Individual profile key.</param>
        /// <param name="value">Individual profile value.</param>
        public static void SetSecureProfile(string namingContainer, string key, object value)
        {
			Logger.LogError(">>> Entering SetSecureProfile(namingContainer={NamingContainer},key={Key},value={Value})", namingContainer, key, value);
            SetSecureProfile(personalizationController: null, namingContainer, key, value);
			Logger.LogWarning("<<< Exiting SetSecureProfile(namingContainer={NamingContainer},key={Key},value={Value})", namingContainer, key, value);
        }

        /// <summary>
        /// persist profile value -use naming container and key to organize
        /// function calls an overload which automatically encrypts the value.
        /// </summary>
        /// <param name="personalizationController">The personalization controller.</param>
        /// <param name="namingContainer">Object containing user personalization info.</param>
        /// <param name="key">Individual profile key.</param>
        /// <param name="value">Individual profile value.</param>
        public static void SetSecureProfile(PersonalizationController personalizationController, string namingContainer, string key, object value)
        {
			Logger.LogError(">>> Entering SetSecureProfile(personalizationController={PersonalizationController},namingContainer={NamingContainer},key={Key},value={Value})", personalizationController, namingContainer, key, value);
            SetSecureProfile(LoadProfile(personalizationController), namingContainer, key, value);
			Logger.LogWarning("<<< Exiting SetSecureProfile(personalizationController={PersonalizationController},namingContainer={NamingContainer},key={Key},value={Value})", personalizationController, namingContainer, key, value);
        }

        /// <summary>
        /// persist profile value from PersonalizationInfo object, using naming container and key to organize
        /// function will automatically encrypt the value to plaintext.
        /// </summary>
        /// <param name="personalization">Object containing user personalization info.</param>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <param name="value">Individual profile value.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking ICryptographyProvider")]
        public static partial void SetSecureProfile(PersonalizationInfo personalization, string namingContainer, string key, object value)
            => SetSecureProfile(Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>(), personalization, namingContainer, key, value);

        /// <summary>
        /// persist profile value from PersonalizationInfo object, using naming container and key to organize
        /// function will automatically encrypt the value to plaintext.
        /// </summary>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        /// <param name="personalization">Object containing user personalization info.</param>
        /// <param name="namingContainer">Container for related set of values.</param>
        /// <param name="key">Individual profile key.</param>
        /// <param name="value">Individual profile value.</param>
        public static void SetSecureProfile(ICryptographyProvider cryptographyProvider, PersonalizationInfo personalization, string namingContainer, string key, object value)
        {
			Logger.LogError(">>> Entering SetSecureProfile(personalization={Personalization},namingContainer={NamingContainer},key={Key},value={Value})", personalization, namingContainer, key, value);
            if (personalization != null)
            {
                var (encryptedValue, algorithmName, initializationVector) = cryptographyProvider.EncryptString(value.ToString(), Config.GetDecryptionkey());
                personalization.Profile[$"{namingContainer}:{key}"] = encryptedValue;
                personalization.Profile[$"{namingContainer}:{key}:algorithmName"] = algorithmName;
                personalization.Profile[$"{namingContainer}:{key}:initializationVector"] = initializationVector;
                personalization.IsModified = true;
				Logger.LogDebug("SetSecureProfile(cryptographyProvider={CryptographyProvider},personalization={Personalization},namingContainer={NamingContainer},key={Key},value={Value}): personalization.IsModified → {Arg5}", cryptographyProvider, personalization, namingContainer, key, value, personalization.IsModified);
            }
			Logger.LogWarning("<<< Exiting SetSecureProfile(cryptographyProvider={CryptographyProvider},personalization={Personalization},namingContainer={NamingContainer},key={Key},value={Value})", cryptographyProvider, personalization, namingContainer, key, value);
        }

        /// <summary>Gets the mode the user is viewing the page in.</summary>
        /// <returns><see cref="Mode"/>.</returns>
        public static Mode GetUserMode()
        {
			Logger.LogError(">>> Entering GetUserMode()");
            Mode mode;
            if (HttpContextSource.Current?.Request.IsAuthenticated == true)
            {
                mode = PortalSettings.Current.DefaultControlPanelMode;
				Logger.LogError("GetUserMode(): mode → {Mode}", mode);
				Logger.LogDebug("GetUserMode(): mode → {Mode}", mode);
                string setting = Convert.ToString(Personalization.GetProfile("Usability", "UserMode" + PortalController.Instance.GetCurrentSettings().PortalId), CultureInfo.InvariantCulture);
                switch (setting.ToUpperInvariant())
                {
                    case "VIEW":
                        mode = Mode.View;
                        break;
                    case "EDIT":
                        mode = Mode.Edit;
                        break;
                    case "LAYOUT":
                        mode = Mode.Layout;
                        break;
                }
            }
            else
            {
                mode = Mode.View;
            }

			Logger.LogWarning("<<< Exiting GetUserMode()");
            return mode;
        }

        private static PersonalizationInfo LoadProfile(PersonalizationController personalizationController)
        {
			Logger.LogError(">>> Entering LoadProfile(personalizationController={PersonalizationController})", personalizationController);
            HttpContext context = HttpContext.Current;

            // First try and load Personalization object from the Context
            var personalization = (PersonalizationInfo)context.Items["Personalization"];
			Logger.LogError("LoadProfile(personalizationController={PersonalizationController}): personalization → {Personalization}", personalizationController, personalization);

            // If the Personalization object is nothing load it and store it in the context for future calls
            if (personalization == null)
            {
                var portalSettings = (PortalSettings)context.Items["PortalSettings"];
				Logger.LogError("LoadProfile(personalizationController={PersonalizationController}): portalSettings → {PortalSettings}", personalizationController, portalSettings);

                // load the user info object
                UserInfo userInfo = UserController.Instance.GetCurrentUserInfo();
				Logger.LogError("LoadProfile(personalizationController={PersonalizationController}): userInfo → {UserInfo}", personalizationController, userInfo);

                // get the personalization object
                personalizationController ??= Globals.GetCurrentServiceProvider().GetRequiredService<PersonalizationController>();
                personalization = personalizationController.LoadProfile(userInfo.UserID, portalSettings.PortalId);

                // store it in the context
                context.Items.Add("Personalization", personalization);
            }

			Logger.LogWarning("<<< Exiting LoadProfile(personalizationController={PersonalizationController})", personalizationController);
            return personalization;
        }
    }
}
