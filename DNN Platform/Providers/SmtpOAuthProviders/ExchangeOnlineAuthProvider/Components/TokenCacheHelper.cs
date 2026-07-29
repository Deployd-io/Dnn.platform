// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExchangeOnlineAuthProvider.Components;

using System;
using System.Security.Cryptography;
using System.Text;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Extensions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

/// <summary>The token cache helper.</summary>
public class TokenCacheHelper
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    private readonly IHostSettingsService hostSettingsService;
    private readonly IPortalController portalController;
    private readonly IHostSettings hostSettings;
    private readonly int portalId;

    /// <summary>Initializes a new instance of the <see cref="TokenCacheHelper"/> class.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
    public TokenCacheHelper(int portalId, IHostSettingsService hostSettingsService)
        : this(portalId, hostSettingsService, null, null)
    {
	Logger.LogDebug(">>> Entering TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService})", portalId, hostSettingsService);
	Logger.LogDebug(">>> Entering TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController})", portalId, hostSettingsService, hostSettings, portalController);
	Logger.LogDebug("TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController}): hostSettingsService → {HostSettingsService1}", portalId, hostSettingsService, hostSettings, portalController, hostSettingsService);
	Logger.LogDebug("<<< Exiting TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController})", portalId, hostSettingsService, hostSettings, portalController);
    }

    /// <summary>Initializes a new instance of the <see cref="TokenCacheHelper"/> class.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    public TokenCacheHelper(int portalId, IHostSettingsService hostSettingsService, IHostSettings hostSettings, IPortalController portalController)
    {
        this.portalId = portalId;
        this.hostSettingsService = hostSettingsService;
		Logger.LogDebug("TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController}): this.portalId → {Arg4}", portalId, hostSettingsService, hostSettings, portalController, this.portalId);
		Logger.LogDebug("TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController}): this.hostSettingsService → {Arg4}", portalId, hostSettingsService, hostSettings, portalController, this.hostSettingsService);
		Logger.LogDebug("TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController}): portalController → {PortalController1}", portalId, hostSettingsService, hostSettings, portalController, portalController);
		Logger.LogDebug("TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController}): portalId → {PortalId1}", portalId, hostSettingsService, hostSettings, portalController, portalId);
        this.hostSettings = hostSettings ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IHostSettings>() ?? new HostSettings(hostSettingsService);
        this.portalController = portalController ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IPortalController>();
		Logger.LogDebug("<<< Exiting TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController})", portalId, hostSettingsService, hostSettings, portalController);
		Logger.LogDebug("TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController}): hostSettings → {HostSettings1}", portalId, hostSettingsService, hostSettings, portalController, hostSettings);
    }

    /// <summary>Enable the token cache.</summary>
    /// <param name="tokenCache">The token cache instance.</param>
    public void EnableSerialization(ITokenCache tokenCache)
    {
        tokenCache.SetBeforeAccess(this.BeforeAccessNotification);
        tokenCache.SetAfterAccess(this.AfterAccessNotification);
		Logger.LogDebug("<<< Exiting EnableSerialization(tokenCache={TokenCache})", tokenCache);
    }

    private void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
		Logger.LogDebug(">>> Entering BeforeAccessNotification(args={Args})", args);
        var authentication = this.GetAuthenticationData();
        if (!string.IsNullOrEmpty(authentication))
		Logger.LogDebug("BeforeAccessNotification(args={Args}): authentication → {Authentication}", args, authentication);
        {
            args.TokenCache.DeserializeMsalV3(Encoding.UTF8.GetBytes(authentication));
        }
		Logger.LogDebug("<<< Exiting BeforeAccessNotification(args={Args})", args);
    }

    private void AfterAccessNotification(TokenCacheNotificationArgs args)
    {
		Logger.LogDebug(">>> Entering AfterAccessNotification(args={Args})", args);
        if (args.HasStateChanged)
        {
            byte[] data = args.TokenCache.SerializeMsalV3();
            this.UpdateAuthenticationData(data);
        }
		Logger.LogDebug("<<< Exiting AfterAccessNotification(args={Args})", args);
    }

    private string GetAuthenticationData()
    {
		Logger.LogDebug(">>> Entering GetAuthenticationData()");
        if (this.portalId == Null.NullInteger)
        {
            return this.hostSettingsService.GetEncryptedString(Constants.AuthenticationSettingName, Config.GetDecryptionkey());
        }

		Logger.LogDebug("<<< Exiting GetAuthenticationData()");
        return PortalController.GetEncryptedString(this.hostSettings, this.portalController, Constants.AuthenticationSettingName, this.portalId, Config.GetDecryptionkey());
    }

    private void UpdateAuthenticationData(byte[] data)
    {
		Logger.LogDebug(">>> Entering UpdateAuthenticationData(data={Data})", data);
        var settingValue = Encoding.UTF8.GetString(data);
        if (this.portalId == Null.NullInteger)
		Logger.LogDebug("UpdateAuthenticationData(data={Data}): settingValue → {SettingValue}", data, settingValue);
        {
            this.hostSettingsService.UpdateEncryptedString(Constants.AuthenticationSettingName, settingValue, Config.GetDecryptionkey());
        }
        else
        {
            PortalController.UpdateEncryptedString(this.hostSettings, this.portalController, HashAlgorithmName.SHA512, this.portalId, Constants.AuthenticationSettingName, settingValue, Config.GetDecryptionkey());
        }
		Logger.LogDebug("<<< Exiting UpdateAuthenticationData(data={Data})", data);
    }
}
