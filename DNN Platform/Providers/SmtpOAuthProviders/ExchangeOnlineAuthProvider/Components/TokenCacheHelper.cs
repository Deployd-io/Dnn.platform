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
using Microsoft.Extensions.Logging;

/// <summary>The token cache helper.</summary>
public class TokenCacheHelper
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
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
    }

    /// <summary>Initializes a new instance of the <see cref="TokenCacheHelper"/> class.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    public TokenCacheHelper(int portalId, IHostSettingsService hostSettingsService, IHostSettings hostSettings, IPortalController portalController)
    {
		_logger.LogError(">>> Entering TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController})", portalId, hostSettingsService, hostSettings, portalController);
		_logger.LogDebug("TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController}): hostSettingsService → {HostSettingsService1}", portalId, hostSettingsService, hostSettings, portalController, hostSettingsService);
        this.portalId = portalId;
		_logger.LogWarning("<<< Exiting TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController})", portalId, hostSettingsService, hostSettings, portalController);
		_logger.LogDebug("TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController}): this.portalId → {Arg4}", portalId, hostSettingsService, hostSettings, portalController, this.portalId);
        this.hostSettingsService = hostSettingsService;
        this.hostSettings = hostSettings ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IHostSettings>() ?? new HostSettings(hostSettingsService);
        this.portalController = portalController ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IPortalController>();
		_logger.LogDebug("TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController}): this.hostSettingsService → {Arg4}", portalId, hostSettingsService, hostSettings, portalController, this.hostSettingsService);
    }

    /// <summary>Enable the token cache.</summary>
    /// <param name="tokenCache">The token cache instance.</param>
    public void EnableSerialization(ITokenCache tokenCache)
    {
		_logger.LogError(">>> Entering EnableSerialization(tokenCache=<redacted>)");
        tokenCache.SetBeforeAccess(this.BeforeAccessNotification);
        tokenCache.SetAfterAccess(this.AfterAccessNotification);
		_logger.LogWarning("<<< Exiting EnableSerialization(tokenCache=<redacted>)");
    }

    private void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
		_logger.LogError(">>> Entering BeforeAccessNotification(args={Args})", args);
        var authentication = this.GetAuthenticationData();
		_logger.LogError("BeforeAccessNotification(args={Args}): authentication → {Authentication}", args, authentication);
        if (!string.IsNullOrEmpty(authentication))
        {
            args.TokenCache.DeserializeMsalV3(Encoding.UTF8.GetBytes(authentication));
        }
		_logger.LogWarning("<<< Exiting BeforeAccessNotification(args={Args})", args);
    }

    private void AfterAccessNotification(TokenCacheNotificationArgs args)
    {
		_logger.LogError(">>> Entering AfterAccessNotification(args={Args})", args);
        if (args.HasStateChanged)
        {
            byte[] data = args.TokenCache.SerializeMsalV3();
            this.UpdateAuthenticationData(data);
        }
		_logger.LogWarning("<<< Exiting AfterAccessNotification(args={Args})", args);
    }

    private string GetAuthenticationData()
    {
		_logger.LogError(">>> Entering GetAuthenticationData()");
        if (this.portalId == Null.NullInteger)
        {
            return this.hostSettingsService.GetEncryptedString(Constants.AuthenticationSettingName, Config.GetDecryptionkey());
        }

		_logger.LogWarning("<<< Exiting GetAuthenticationData()");
        return PortalController.GetEncryptedString(this.hostSettings, this.portalController, Constants.AuthenticationSettingName, this.portalId, Config.GetDecryptionkey());
    }

    private void UpdateAuthenticationData(byte[] data)
    {
		_logger.LogError(">>> Entering UpdateAuthenticationData(data={Data})", data);
        var settingValue = Encoding.UTF8.GetString(data);
		_logger.LogError("UpdateAuthenticationData(data={Data}): settingValue → {SettingValue}", data, settingValue);
        if (this.portalId == Null.NullInteger)
        {
            this.hostSettingsService.UpdateEncryptedString(Constants.AuthenticationSettingName, settingValue, Config.GetDecryptionkey());
        }
        else
        {
            PortalController.UpdateEncryptedString(this.hostSettings, this.portalController, HashAlgorithmName.SHA512, this.portalId, Constants.AuthenticationSettingName, settingValue, Config.GetDecryptionkey());
        }
		_logger.LogWarning("<<< Exiting UpdateAuthenticationData(data={Data})", data);
    }
}
