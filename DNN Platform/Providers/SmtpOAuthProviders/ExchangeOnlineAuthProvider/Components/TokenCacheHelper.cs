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
	_logger.LogDebug(">>> Entering TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService})", portalId, hostSettingsService);
	_logger.LogDebug("<<< Exiting TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService})", portalId, hostSettingsService);
	_logger.LogDebug(">>> Entering TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController})", portalId, hostSettingsService, hostSettings, portalController);
	_logger.LogDebug("TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController}): hostSettingsService → {HostSettingsService1}", portalId, hostSettingsService, hostSettings, portalController, hostSettingsService);
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
		_logger.LogDebug("TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController}): this.portalId → {Arg4}", portalId, hostSettingsService, hostSettings, portalController, this.portalId);
		_logger.LogDebug("TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController}): this.hostSettingsService → {Arg4}", portalId, hostSettingsService, hostSettings, portalController, this.hostSettingsService);
        this.hostSettings = hostSettings ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IHostSettings>() ?? new HostSettings(hostSettingsService);
        this.portalController = portalController ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IPortalController>();
		_logger.LogDebug("<<< Exiting TokenCacheHelper(portalId={PortalId},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController})", portalId, hostSettingsService, hostSettings, portalController);
    }

    /// <summary>Enable the token cache.</summary>
    /// <param name="tokenCache">The token cache instance.</param>
    public void EnableSerialization(ITokenCache tokenCache)
    {
		_logger.LogDebug(">>> Entering EnableSerialization(tokenCache=<redacted>)");
        tokenCache.SetBeforeAccess(this.BeforeAccessNotification);
        tokenCache.SetAfterAccess(this.AfterAccessNotification);
		_logger.LogDebug("<<< Exiting EnableSerialization(tokenCache=<redacted>)");
    }

    private void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
		_logger.LogDebug(">>> Entering BeforeAccessNotification(args={Args})", args);
        var authentication = this.GetAuthenticationData();
        if (!string.IsNullOrEmpty(authentication))
        {
            args.TokenCache.DeserializeMsalV3(Encoding.UTF8.GetBytes(authentication));
        }
		_logger.LogDebug("<<< Exiting BeforeAccessNotification(args={Args})", args);
    }

    private void AfterAccessNotification(TokenCacheNotificationArgs args)
    {
		_logger.LogDebug(">>> Entering AfterAccessNotification(args={Args})", args);
        if (args.HasStateChanged)
        {
            byte[] data = args.TokenCache.SerializeMsalV3();
            this.UpdateAuthenticationData(data);
        }
		_logger.LogDebug("<<< Exiting AfterAccessNotification(args={Args})", args);
    }

    private string GetAuthenticationData()
    {
		_logger.LogDebug(">>> Entering GetAuthenticationData()");
        if (this.portalId == Null.NullInteger)
        {
		_logger.LogDebug("<<< Exiting GetAuthenticationData()");
            return this.hostSettingsService.GetEncryptedString(Constants.AuthenticationSettingName, Config.GetDecryptionkey());
        }

        return PortalController.GetEncryptedString(this.hostSettings, this.portalController, Constants.AuthenticationSettingName, this.portalId, Config.GetDecryptionkey());
    }

    private void UpdateAuthenticationData(byte[] data)
    {
		_logger.LogDebug(">>> Entering UpdateAuthenticationData(data={Data})", data);
        var settingValue = Encoding.UTF8.GetString(data);
        if (this.portalId == Null.NullInteger)
        {
            this.hostSettingsService.UpdateEncryptedString(Constants.AuthenticationSettingName, settingValue, Config.GetDecryptionkey());
        }
        else
        {
            PortalController.UpdateEncryptedString(this.hostSettings, this.portalController, HashAlgorithmName.SHA512, this.portalId, Constants.AuthenticationSettingName, settingValue, Config.GetDecryptionkey());
        }
		_logger.LogDebug("<<< Exiting UpdateAuthenticationData(data={Data})", data);
    }
}
