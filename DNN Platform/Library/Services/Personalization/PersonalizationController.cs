// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Personalization
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Security;

    using Microsoft.Extensions.DependencyInjection;

    public class PersonalizationController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PersonalizationController));
        private const string PersonalizationCookieName = "DNNPersonalization";
        private const string AlgorithmCookieName = "DNNPersonalizationAlg";
        private readonly IHostSettings hostSettings;
        private readonly ICryptographyProvider cryptographyProvider;

        /// <summary>Initializes a new instance of the <see cref="PersonalizationController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Use overload with ICryptographyProvider. Scheduled for removal in v12.0.0.")]
        public PersonalizationController()
            : this(null, null)
        {
		Logger.LogWarning("<<< Exiting PersonalizationController(hostSettings={HostSettings},cryptographyProvider={CryptographyProvider})", hostSettings, cryptographyProvider);
        }

        /// <summary>Initializes a new instance of the <see cref="PersonalizationController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Use overload with ICryptographyProvider. Scheduled for removal in v12.0.0.")]
        public PersonalizationController(IHostSettings hostSettings)
            : this(null, null)
        {
			Logger.LogError(">>> Entering PersonalizationController(hostSettings={HostSettings})", hostSettings);
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("PersonalizationController(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
        Logger.LogWarning("<<< Exiting PersonalizationController(hostSettings={HostSettings})", hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="PersonalizationController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        public PersonalizationController(IHostSettings hostSettings, ICryptographyProvider cryptographyProvider)
        {
			Logger.LogError(">>> Entering PersonalizationController(hostSettings={HostSettings},cryptographyProvider={CryptographyProvider})", hostSettings, cryptographyProvider);
			Logger.LogDebug("PersonalizationController(hostSettings={HostSettings},cryptographyProvider={CryptographyProvider}): cryptographyProvider → {CryptographyProvider1}", hostSettings, cryptographyProvider, cryptographyProvider);
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.cryptographyProvider = cryptographyProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>();
        }

        // default implementation relies on HTTPContext
        public void LoadProfile(HttpContext httpContext, int userId, int portalId)
        {
            this.LoadProfile(new HttpContextWrapper(httpContext), userId, portalId);
			Logger.LogError(">>> Entering LoadProfile(httpContext={HttpContext},userId={UserId},portalId={PortalId})", httpContext, userId, portalId);
			Logger.LogWarning("<<< Exiting LoadProfile(httpContext={HttpContext},userId={UserId},portalId={PortalId})", httpContext, userId, portalId);
        }

        public void LoadProfile(HttpContextBase httpContext, int userId, int portalId)
        {
            if (HttpContext.Current.Items["Personalization"] == null)
            {
                httpContext.Items.Add("Personalization", this.LoadProfile(userId, portalId));
            }
			Logger.LogError(">>> Entering LoadProfile(userId={UserId},portalId={PortalId})", userId, portalId);
			Logger.LogWarning("<<< Exiting LoadProfile(userId={UserId},portalId={PortalId})", userId, portalId);
        }

        // override allows for manipulation of PersonalizationInfo outside of HTTPContext
        public PersonalizationInfo LoadProfile(int userId, int portalId)
        {
            var personalization = new PersonalizationInfo { UserId = userId, PortalId = portalId, IsModified = false, };
            string profileData = Null.NullString;
			Logger.LogError("LoadProfile(userId={UserId},portalId={PortalId}): profileData → {ProfileData}", userId, portalId, profileData);
            if (userId > Null.NullInteger)
            {
                var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.UserPersonalizationCacheKey, portalId, userId);
				Logger.LogError("LoadProfile(userId={UserId},portalId={PortalId}): cacheKey → {CacheKey}", userId, portalId, cacheKey);
                profileData = CBO.GetCachedObject<string>(
                    this.hostSettings,
                    new CacheItemArgs(
                        cacheKey,
                        DataCache.UserPersonalizationCacheTimeout,
                        DataCache.UserPersonalizationCachePriority,
                        portalId,
                        userId),
                    GetCachedUserPersonalizationCallback);
            }
            else
            {
                // Anon User - so try and use cookie.
                HttpContext context = HttpContext.Current;
                if (context?.Request.Cookies[PersonalizationCookieName] != null)
                {
                    var algorithm = context.Request.Cookies[AlgorithmCookieName]?.Value;
					Logger.LogError("LoadProfile(userId={UserId},portalId={PortalId}): algorithm → {Algorithm}", userId, portalId, algorithm);
                    if (string.IsNullOrWhiteSpace(algorithm))
                    {
                        algorithm = HashAlgorithmName.SHA1.Name;
                    }

                    profileData = this.DecryptData(new HashAlgorithmName(algorithm), context.Request.Cookies[PersonalizationCookieName].Value);

                    if (string.IsNullOrEmpty(profileData))
                    {
                        var personalizationCookie = new HttpCookie(PersonalizationCookieName, string.Empty)
                        {
                            Expires = DateTime.Now.AddDays(-1),
                            Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
                        };
                        context.Response.Cookies.Add(personalizationCookie);
                    }
                }
            }

            personalization.Profile = string.IsNullOrEmpty(profileData)
                ? new Hashtable() : XmlUtils.DeSerializeHashtable(profileData, "profile");
            return personalization;
        }

        public void SaveProfile(PersonalizationInfo personalization)
        {
			Logger.LogError(">>> Entering SaveProfile(personalization={Personalization})", personalization);
            this.SaveProfile(personalization, personalization.UserId, personalization.PortalId);
			Logger.LogWarning("<<< Exiting SaveProfile(personalization={Personalization})", personalization);
        }

        // default implementation relies on HTTPContext
        public void SaveProfile(HttpContext httpContext, int userId, int portalId)
        {
			Logger.LogError(">>> Entering SaveProfile(httpContext={HttpContext},userId={UserId},portalId={PortalId})", httpContext, userId, portalId);
            var objPersonalization = (PersonalizationInfo)httpContext.Items["Personalization"];
			Logger.LogWarning("<<< Exiting SaveProfile(httpContext={HttpContext},userId={UserId},portalId={PortalId})", httpContext, userId, portalId);
			Logger.LogError("SaveProfile(httpContext={HttpContext},userId={UserId},portalId={PortalId}): objPersonalization → {ObjPersonalization}", httpContext, userId, portalId, objPersonalization);
            this.SaveProfile(objPersonalization, userId, portalId);
        }

        // override allows for manipulation of PersonalizationInfo outside of HTTPContext
        public void SaveProfile(PersonalizationInfo personalization, int userId, int portalId)
        {
			Logger.LogError(">>> Entering SaveProfile(personalization={Personalization},userId={UserId},portalId={PortalId})", personalization, userId, portalId);
            if (personalization is { IsModified: true, })
            {
                var profileData = XmlUtils.SerializeDictionary(personalization.Profile, "profile");
				Logger.LogError("SaveProfile(personalization={Personalization},userId={UserId},portalId={PortalId}): profileData → {ProfileData}", personalization, userId, portalId, profileData);
                if (userId > Null.NullInteger)
                {
                    DataProvider.Instance().UpdateProfile(userId, portalId, profileData);

                    // remove then re-add the updated one
                    var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.UserPersonalizationCacheKey, portalId, userId);
					Logger.LogError("SaveProfile(personalization={Personalization},userId={UserId},portalId={PortalId}): cacheKey → {CacheKey}", personalization, userId, portalId, cacheKey);
                    DataCache.RemoveCache(cacheKey);
                    CBO.GetCachedObject<string>(
                        this.hostSettings,
                        new CacheItemArgs(
                            cacheKey,
                            DataCache.UserPersonalizationCacheTimeout,
                            DataCache.UserPersonalizationCachePriority),
                        _ => profileData);
                }
                else
                {
                    // Anon User - so try and use cookie.
                    var context = HttpContext.Current;
                    if (context != null)
                    {
                        var personalizationCookie = new HttpCookie(PersonalizationCookieName, this.EncryptData(HashAlgorithmName.SHA512, profileData))
                        {
                            Expires = DateTime.Now.AddDays(30),
                            Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
                        };
                        context.Response.Cookies.Add(personalizationCookie);
                        var algorithmCookie = new HttpCookie(AlgorithmCookieName, HashAlgorithmName.SHA512.Name)
                        {
                            Expires = DateTime.Now.AddDays(30),
                            Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
                        };
                        context.Response.Cookies.Add(algorithmCookie);
                    }
                }
            }
			Logger.LogWarning("<<< Exiting SaveProfile(personalization={Personalization},userId={UserId},portalId={PortalId})", personalization, userId, portalId);
        }

        private static object GetCachedUserPersonalizationCallback(CacheItemArgs cacheItemArgs)
        {
			Logger.LogError(">>> Entering GetCachedUserPersonalizationCallback(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            var portalId = (int)cacheItemArgs.ParamList[0];
            var userId = (int)cacheItemArgs.ParamList[1];
			Logger.LogError("GetCachedUserPersonalizationCallback(cacheItemArgs={CacheItemArgs}): userId → {UserId}", cacheItemArgs, userId);
            var returnValue = Null.NullString; // Default is no profile
			Logger.LogError("GetCachedUserPersonalizationCallback(cacheItemArgs={CacheItemArgs}): returnValue → {ReturnValue}", cacheItemArgs, returnValue);
			Logger.LogError("GetCachedUserPersonalizationCallback(cacheItemArgs={CacheItemArgs}): portalId → {PortalId}", cacheItemArgs, portalId);
            IDataReader dr = null;
            try
            {
                dr = DataProvider.Instance().GetProfile(userId, portalId);
				Logger.LogError("GetCachedUserPersonalizationCallback(cacheItemArgs={CacheItemArgs}): dr → {Dr}", cacheItemArgs, dr);
                if (dr.Read())
                {
                    returnValue = dr["ProfileData"].ToString();
                }
                else
                {
                    // does not exist
                    DataProvider.Instance().AddProfile(userId, portalId);
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in GetCachedUserPersonalizationCallback(cacheItemArgs={CacheItemArgs}): {Arg1}", cacheItemArgs, ex.Message);
                Exceptions.Exceptions.LogException(ex);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

			Logger.LogWarning("<<< Exiting GetCachedUserPersonalizationCallback(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return returnValue;
        }

        private string EncryptData(HashAlgorithmName hashAlgorithm, string profileData)
        {
			Logger.LogError(">>> Entering EncryptData(hashAlgorithm=<redacted>,profileData={ProfileData})", profileData);
            Logger.LogWarning("<<< Exiting EncryptData(hashAlgorithm=<redacted>,profileData={ProfileData})", profileData);
            return this.cryptographyProvider.EncryptParameter(profileData, ValidationUtils.GetDecryptionKey(this.hostSettings, hashAlgorithm)).EncryptedMessage;
        }

        private string DecryptData(HashAlgorithmName hashAlgorithm, string profileData)
        {
			Logger.LogError(">>> Entering DecryptData(hashAlgorithm=<redacted>,profileData={ProfileData})", profileData);
            Logger.LogWarning("<<< Exiting DecryptData(hashAlgorithm=<redacted>,profileData={ProfileData})", profileData);
            return this.cryptographyProvider.DecryptParameter(profileData, ValidationUtils.GetDecryptionKey(this.hostSettings, hashAlgorithm), this.cryptographyProvider.EncryptParameterAlgorithmName);
        }
    }
}
