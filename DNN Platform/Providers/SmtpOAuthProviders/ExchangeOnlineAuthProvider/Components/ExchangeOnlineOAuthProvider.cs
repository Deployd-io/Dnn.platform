// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExchangeOnlineAuthProvider.Components;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Abstractions.Security;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Common.Extensions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail.OAuth;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
public class ExchangeOnlineOAuthProvider : ISmtpOAuthProvider
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    private readonly IHostSettingsService hostSettingsService;
    private readonly IPortalAliasService portalAliasService;
    private readonly IPortalController portalController;
    private readonly IHostSettings hostSettings;
    private readonly ICryptographyProvider cryptographyProvider;

    /// <summary>Initializes a new instance of the <see cref="ExchangeOnlineOAuthProvider"/> class.</summary>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalAliasService">The portal alias service.</param>
    [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
    public ExchangeOnlineOAuthProvider(IHostSettingsService hostSettingsService, IPortalAliasService portalAliasService)
        : this(hostSettingsService, portalAliasService, null, null, null)
    {
	_logger.LogDebug(">>> Entering ExchangeOnlineOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService})", hostSettingsService, portalAliasService);
    }

	_logger.LogDebug("<<< Exiting ExchangeOnlineOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService})", hostSettingsService, portalAliasService);
    /// <summary>Initializes a new instance of the <see cref="ExchangeOnlineOAuthProvider"/> class.</summary>
	_logger.LogDebug("<<< Exiting ExchangeOnlineOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController})", hostSettingsService, portalAliasService, hostSettings, portalController);
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalAliasService">The portal alias service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with ICryptographyProvider. Scheduled for removal in v12.0.0.")]
    public ExchangeOnlineOAuthProvider(IHostSettingsService hostSettingsService, IPortalAliasService portalAliasService, IHostSettings hostSettings, IPortalController portalController)
        : this(hostSettingsService, portalAliasService, hostSettings, portalController, null)
    {
	_logger.LogDebug(">>> Entering ExchangeOnlineOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController})", hostSettingsService, portalAliasService, hostSettings, portalController);
	_logger.LogDebug("ExchangeOnlineOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController},cryptographyProvider={CryptographyProvider}): portalAliasService → {PortalAliasService1}", hostSettingsService, portalAliasService, hostSettings, portalController, cryptographyProvider, portalAliasService);
	_logger.LogDebug("ExchangeOnlineOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController},cryptographyProvider={CryptographyProvider}): hostSettings → {HostSettings1}", hostSettingsService, portalAliasService, hostSettings, portalController, cryptographyProvider, hostSettings);
    }

    /// <summary>Initializes a new instance of the <see cref="ExchangeOnlineOAuthProvider"/> class.</summary>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalAliasService">The portal alias service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="cryptographyProvider">The cryptography provider.</param>
    public ExchangeOnlineOAuthProvider(IHostSettingsService hostSettingsService, IPortalAliasService portalAliasService, IHostSettings hostSettings, IPortalController portalController, ICryptographyProvider cryptographyProvider)
    {
        this.hostSettingsService = hostSettingsService;
        this.portalAliasService = portalAliasService;
		_logger.LogDebug("ExchangeOnlineOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController},cryptographyProvider={CryptographyProvider}): this.hostSettingsService → {Arg5}", hostSettingsService, portalAliasService, hostSettings, portalController, cryptographyProvider, this.hostSettingsService);
		_logger.LogDebug("ExchangeOnlineOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController},cryptographyProvider={CryptographyProvider}): this.portalAliasService → {Arg5}", hostSettingsService, portalAliasService, hostSettings, portalController, cryptographyProvider, this.portalAliasService);
        this.hostSettings = hostSettings ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IHostSettings>() ?? new HostSettings(hostSettingsService);
        this.portalController = portalController ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IPortalController>();
        this.cryptographyProvider = cryptographyProvider ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<ICryptographyProvider>();
    }

    /// <inheritdoc />
    public string Name => Constants.Name;

    /// <inheritdoc />
    public string LocalizedName => Localization.GetSafeJSString(this.Name, Constants.LocalResourcesFile);

    /// <inheritdoc />
    public bool IsAuthorized(int portalId)
    {
		_logger.LogDebug(">>> Entering IsAuthorized(portalId={PortalId})", portalId);
        var clientApplication = CreateClientApplication(this, this.hostSettingsService, this.hostSettings, this.portalController, portalId);
        if (clientApplication == null)
		_logger.LogDebug("IsAuthorized(portalId={PortalId}): clientApplication → {ClientApplication}", portalId, clientApplication);
        {
            return false;
        }

        var accounts = clientApplication.GetAccountsAsync().Result;
		_logger.LogDebug("IsAuthorized(portalId={PortalId}): accounts → {Accounts}", portalId, accounts);

		_logger.LogDebug("<<< Exiting IsAuthorized(portalId={PortalId})", portalId);
        return accounts?.Any() ?? false;
    }

    /// <inheritdoc />
    public async Task<bool> IsAuthorizedAsync(int portalId, CancellationToken cancellationToken = default)
    {
		_logger.LogDebug(">>> Entering IsAuthorizedAsync(portalId={PortalId},cancellationToken=<redacted>)", portalId);
        var clientApplication = CreateClientApplication(this, this.hostSettingsService, this.hostSettings, this.portalController, portalId);
        if (clientApplication == null)
		_logger.LogDebug("IsAuthorizedAsync(portalId={PortalId},cancellationToken=<redacted>): clientApplication → {ClientApplication}", portalId, clientApplication);
        {
            return false;
        }

        var accounts = await clientApplication.GetAccountsAsync(cancellationToken);
		_logger.LogDebug("IsAuthorizedAsync(portalId={PortalId},cancellationToken=<redacted>): accounts → {Accounts}", portalId, accounts);
		_logger.LogDebug("<<< Exiting IsAuthorizedAsync(portalId={PortalId},cancellationToken=<redacted>)", portalId);
        return accounts?.Any() ?? false;
    }

    /// <inheritdoc />
    public string GetAuthorizeUrl(int portalId)
    {
		_logger.LogDebug(">>> Entering GetAuthorizeUrl(portalId={PortalId})", portalId);
        var clientId = this.GetSetting(portalId, Constants.ClientIdSettingName);
        var clientSecret = this.GetSetting(portalId, Constants.ClientSecretSettingName);
		_logger.LogDebug("GetAuthorizeUrl(portalId={PortalId}): clientSecret → {ClientSecret}", portalId, clientSecret);
		_logger.LogDebug("GetAuthorizeUrl(portalId={PortalId}): clientId → {ClientId}", portalId, clientId);

        if (string.IsNullOrWhiteSpace(clientId)
            || string.IsNullOrWhiteSpace(clientSecret))
        {
            return string.Empty;
        }

        var portalSettings = new PortalSettings(portalId == Null.NullInteger ? this.hostSettings.HostPortalId : portalId);
		_logger.LogDebug("GetAuthorizeUrl(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);
        var portalAlias = this.portalAliasService.GetPortalAliasesByPortalId(portalId == Null.NullInteger ? this.hostSettings.HostPortalId : portalId)
            .OrderByDescending(a => a.IsPrimary)
            .First();
        var sslEnabled = portalSettings.SSLEnabled && portalSettings.SSLSetup == SiteSslSetup.On;

        var siteUrl = $"{(sslEnabled ? "https" : "http")}://{portalAlias.HttpAlias}";
		_logger.LogDebug("GetAuthorizeUrl(portalId={PortalId}): siteUrl → {SiteUrl}", portalId, siteUrl);

		_logger.LogDebug("<<< Exiting GetAuthorizeUrl(portalId={PortalId})", portalId);
        return string.Format(Constants.CallbackUrl, siteUrl, portalId);
    }

    /// <inheritdoc />
    public IList<SmtpOAuthSetting> GetSettings(int portalId)
    {
		_logger.LogDebug(">>> Entering GetSettings(portalId={PortalId})", portalId);
		_logger.LogDebug("<<< Exiting GetSettings(portalId={PortalId})", portalId);
        return portalId > Null.NullInteger ? this.GetSettingsFromPortal(portalId) : this.GetSettingsFromHost();
    }

    /// <inheritdoc />
    public bool UpdateSettings(int portalId, IDictionary<string, string> settings, out IList<string> errorMessages)
    {
		_logger.LogDebug(">>> Entering UpdateSettings(portalId={PortalId},settings={Settings})", portalId, settings);
        errorMessages = new List<string>();
        bool changed;
		_logger.LogDebug("UpdateSettings(portalId={PortalId},settings={Settings}): errorMessages → {ErrorMessages}", portalId, settings, errorMessages);

        if (portalId == Null.NullInteger)
        {
            changed = this.UpdateHostSettings(settings);
        }
        else
        {
            changed = this.UpdatePortalSettings(portalId, settings);
        }

        if (changed)
        {
            this.DeleteSetting(portalId, Constants.AuthenticationSettingName);
        }

		_logger.LogDebug("<<< Exiting UpdateSettings(portalId={PortalId},settings={Settings})", portalId, settings);
        return changed;
    }

    /// <inheritdoc />
    public void Authorize(int portalId, IOAuth2SmtpClient smtpClient)
    {
		_logger.LogDebug(">>> Entering Authorize(portalId={PortalId},smtpClient={SmtpClient})", portalId, smtpClient);
        if (!this.IsAuthorized(portalId))
        {
            return;
        }

        var clientApplication = CreateClientApplication(this, this.hostSettingsService, this.hostSettings, this.portalController, portalId);
		_logger.LogDebug("Authorize(portalId={PortalId},smtpClient={SmtpClient}): clientApplication → {ClientApplication}", portalId, smtpClient, clientApplication);
        var account = clientApplication.GetAccountsAsync().Result.First();
		_logger.LogDebug("Authorize(portalId={PortalId},smtpClient={SmtpClient}): account → {Account}", portalId, smtpClient, account);
        var scopes = GetAuthenticationScopes();
		_logger.LogDebug("Authorize(portalId={PortalId},smtpClient={SmtpClient}): scopes → {Scopes}", portalId, smtpClient, scopes);
        var result = clientApplication.AcquireTokenSilent(scopes, account).ExecuteAsync().Result;
		_logger.LogDebug("Authorize(portalId={PortalId},smtpClient={SmtpClient}): result → {Result}", portalId, smtpClient, result);
        if (result != null)
        {
            var username = result.Account.Username;
			_logger.LogDebug("Authorize(portalId={PortalId},smtpClient={SmtpClient}): username → {Username}", portalId, smtpClient, username);
            var accessToken = result.AccessToken;

            smtpClient.Authenticate(username, accessToken);
        }
		_logger.LogDebug("<<< Exiting Authorize(portalId={PortalId},smtpClient={SmtpClient})", portalId, smtpClient);
    }

    /// <inheritdoc />
    public async Task AuthorizeAsync(int portalId, IOAuth2SmtpClient smtpClient, CancellationToken cancellationToken = default)
    {
		_logger.LogDebug(">>> Entering AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken=<redacted>)", portalId, smtpClient);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        if (!await this.IsAuthorizedAsync(portalId, cancellationToken))
        {
            return;
        }

        var clientApplication = CreateClientApplication(this, this.hostSettingsService, this.hostSettings, this.portalController, portalId);
		_logger.LogDebug("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken=<redacted>): clientApplication → {ClientApplication}", portalId, smtpClient, clientApplication);
        var accounts = await clientApplication.GetAccountsAsync(cancellationToken);
		_logger.LogDebug("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken=<redacted>): accounts → {Accounts}", portalId, smtpClient, accounts);
        var account = accounts.First();
		_logger.LogDebug("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken=<redacted>): account → {Account}", portalId, smtpClient, account);
        var scopes = GetAuthenticationScopes();
		_logger.LogDebug("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken=<redacted>): scopes → {Scopes}", portalId, smtpClient, scopes);
        var result = await clientApplication.AcquireTokenSilent(scopes, account).ExecuteAsync(cancellationToken);
		_logger.LogDebug("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken=<redacted>): result → {Result}", portalId, smtpClient, result);
        if (result != null)
        {
            var username = result.Account.Username;
			_logger.LogDebug("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken=<redacted>): username → {Username}", portalId, smtpClient, username);
            var accessToken = result.AccessToken;

            await smtpClient.AuthenticateAsync(username, accessToken, cancellationToken);
        }
		_logger.LogInformation("AuthorizeAsync(portalId,smtpClient,cancellationToken)={PortalId},{SmtpClient},{CancellationToken}: external service call took {Arg3} ms", portalId, smtpClient, cancellationToken, __logStart.ElapsedMilliseconds);
		_logger.LogDebug("<<< Exiting AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken=<redacted>)", portalId, smtpClient);
    }

    /// <summary>Create the authentication client application.</summary>
    /// <param name="smtpOAuthController">The SMTP OAuth controller.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The client application.</returns>
    internal static ConfidentialClientApplication CreateClientApplication(ISmtpOAuthController smtpOAuthController, IHostSettingsService hostSettingsService, IHostSettings hostSettings, IPortalController portalController, int portalId)
    {
		_logger.LogDebug(">>> Entering CreateClientApplication(smtpOAuthController={SmtpOAuthController},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController},portalId={PortalId})", smtpOAuthController, hostSettingsService, hostSettings, portalController, portalId);
		_logger.LogDebug("<<< Exiting CreateClientApplication(smtpOAuthController={SmtpOAuthController},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController},portalId={PortalId})", smtpOAuthController, hostSettingsService, hostSettings, portalController, portalId);
		_logger.LogDebug("<<< Exiting CreateClientApplication(authProvider={AuthProvider},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController},portalId={PortalId})", authProvider, hostSettingsService, hostSettings, portalController, portalId);
        return CreateClientApplication(smtpOAuthController.GetOAuthProvider(Constants.Name), hostSettingsService, hostSettings, portalController, portalId);
    }

    /// <summary>Get the authentication scopes list.</summary>
    /// <returns>The scopes.</returns>
    internal static IList<string> GetAuthenticationScopes() => ["https://outlook.office365.com/.default",];

    private static ConfidentialClientApplication CreateClientApplication(ISmtpOAuthProvider authProvider, IHostSettingsService hostSettingsService, IHostSettings hostSettings, IPortalController portalController, int portalId)
    {
        var settings = authProvider.GetSettings(portalId);
        var tenantId = settings.FirstOrDefault(i => i.Name == Constants.TenantIdSettingName)?.Value ?? string.Empty;
        var clientId = settings.FirstOrDefault(i => i.Name == Constants.ClientIdSettingName)?.Value ?? string.Empty;
        var clientSecret = settings.FirstOrDefault(i => i.Name == Constants.ClientSecretSettingName)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(clientSecret))
        {
            return null;
        }

        var redirectUrl = authProvider.GetAuthorizeUrl(portalId);
        var questionMarkIndex = redirectUrl.IndexOf('?');
        if (questionMarkIndex > -1)
        {
            redirectUrl = redirectUrl.Substring(0, questionMarkIndex);
        }

        var options = new ConfidentialClientApplicationOptions
        {
            TenantId = tenantId,
            ClientId = clientId,
            ClientSecret = clientSecret,
            RedirectUri = redirectUrl,
            Instance = Constants.AzureInstance,
        };
        var clientApplication = (ConfidentialClientApplication)ConfidentialClientApplicationBuilder
            .CreateWithApplicationOptions(options)
            .Build();

        var tokenCacheHelper = new TokenCacheHelper(portalId, hostSettingsService, hostSettings, portalController);
        tokenCacheHelper.EnableSerialization(clientApplication.UserTokenCache);

        return clientApplication;
    }

    private IList<SmtpOAuthSetting> GetSettingsFromPortal(int portalId)
    {
		_logger.LogDebug(">>> Entering GetSettingsFromPortal(portalId={PortalId})", portalId);
        var portalSettings = PortalController.Instance.GetPortalSettings(portalId);
        if (portalSettings == null)
		_logger.LogDebug("GetSettingsFromPortal(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);
        {
			_logger.LogWarning("Throwing ArgumentException from GetSettingsFromPortal(portalId={PortalId})", portalId);
            throw new ArgumentException("Invalid portal Id.");
        }

        var tenantId = portalSettings.GetValueOrDefault(Constants.TenantIdSettingName, string.Empty);
		_logger.LogDebug("GetSettingsFromPortal(portalId={PortalId}): tenantId → {TenantId}", portalId, tenantId);
        var clientId = portalSettings.GetValueOrDefault(Constants.ClientIdSettingName, string.Empty);
		_logger.LogDebug("GetSettingsFromPortal(portalId={PortalId}): clientId → {ClientId}", portalId, clientId);
        var clientSecret = portalSettings.GetValueOrDefault(Constants.ClientSecretSettingName, string.Empty);
		_logger.LogDebug("GetSettingsFromPortal(portalId={PortalId}): clientSecret → {ClientSecret}", portalId, clientSecret);
        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            var algorithmName = portalSettings.GetValueOrDefault(Constants.ClientSecretEncryptionAlgorithm, string.Empty);
			_logger.LogDebug("GetSettingsFromPortal(portalId={PortalId}): algorithmName → {AlgorithmName}", portalId, algorithmName);
            if (!string.IsNullOrWhiteSpace(algorithmName))
            {
                var initializationVector = portalSettings.GetValueOrDefault(Constants.ClientSecretInitializationVector, string.Empty);
				_logger.LogDebug("GetSettingsFromPortal(portalId={PortalId}): initializationVector → {InitializationVector}", portalId, initializationVector);
                clientSecret = this.cryptographyProvider.DecryptString(clientSecret, Config.GetDecryptionkey(), algorithmName, initializationVector);
            }
            else
		_logger.LogDebug("<<< Exiting GetSettingsFromPortal(portalId={PortalId})", portalId);
            {
#pragma warning disable CS0618 // Type or member is obsolete
                clientSecret = PortalSecurity.Instance.Decrypt(Config.GetDecryptionkey(), clientSecret);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        return
        [
            new SmtpOAuthSetting
            {
                Name = Constants.TenantIdSettingName,
                Value = tenantId,
                Label = Localization.GetString("TenantId", Constants.LocalResourcesFile),
                Help = Localization.GetString("TenantId.Help", Constants.LocalResourcesFile),
                IsRequired = true,
            },
            new SmtpOAuthSetting
            {
                Name = Constants.ClientIdSettingName,
                Value = clientId,
                Label = Localization.GetString("ClientId", Constants.LocalResourcesFile),
                Help = Localization.GetString("ClientId.Help", Constants.LocalResourcesFile),
                IsRequired = true,
            },
            new SmtpOAuthSetting
            {
                Name = Constants.ClientSecretSettingName,
                Value = clientSecret,
                Label = Localization.GetString("ClientSecret", Constants.LocalResourcesFile),
                Help = Localization.GetString("ClientSecret.Help", Constants.LocalResourcesFile),
                IsSecure = true,
                IsRequired = true,
            },
        ];
    }

    private IList<SmtpOAuthSetting> GetSettingsFromHost()
    {
		_logger.LogDebug(">>> Entering GetSettingsFromHost()");
        var tenantId = this.hostSettingsService.GetString(Constants.TenantIdSettingName, string.Empty);
        var clientId = this.hostSettingsService.GetString(Constants.ClientIdSettingName, string.Empty);
		_logger.LogDebug("GetSettingsFromHost(): clientId → {ClientId}", clientId);
        var clientSecret = this.hostSettingsService.GetString(Constants.ClientSecretSettingName, string.Empty);
		_logger.LogDebug("GetSettingsFromHost(): clientSecret → {ClientSecret}", clientSecret);
		_logger.LogDebug("GetSettingsFromHost(): tenantId → {TenantId}", tenantId);
        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            var algorithmName = this.hostSettingsService.GetString(Constants.ClientSecretEncryptionAlgorithm, string.Empty);
			_logger.LogDebug("GetSettingsFromHost(): algorithmName → {AlgorithmName}", algorithmName);
            if (!string.IsNullOrWhiteSpace(algorithmName))
            {
                var initializationVector = this.hostSettingsService.GetString(Constants.ClientSecretInitializationVector, string.Empty);
				_logger.LogDebug("GetSettingsFromHost(): initializationVector → {InitializationVector}", initializationVector);
                clientSecret = this.cryptographyProvider.DecryptString(clientSecret, Config.GetDecryptionkey(), algorithmName, initializationVector);
            }
            else
		_logger.LogDebug("<<< Exiting GetSettingsFromHost()");
            {
#pragma warning disable CS0618 // Type or member is obsolete
                clientSecret = PortalSecurity.Instance.Decrypt(Config.GetDecryptionkey(), clientSecret);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        return new List<SmtpOAuthSetting>
        {
            new SmtpOAuthSetting
            {
                Name = Constants.TenantIdSettingName,
                Value = tenantId,
                Label = Localization.GetString("TenantId", Constants.LocalResourcesFile),
                Help = Localization.GetString("TenantId.Help", Constants.LocalResourcesFile),
                IsRequired = true,
            },
            new SmtpOAuthSetting
            {
                Name = Constants.ClientIdSettingName,
                Value = clientId,
                Label = Localization.GetString("ClientId", Constants.LocalResourcesFile),
                Help = Localization.GetString("ClientId.Help", Constants.LocalResourcesFile),
                IsRequired = true,
            },
            new SmtpOAuthSetting
            {
                Name = Constants.ClientSecretSettingName,
                Value = clientSecret,
                Label = Localization.GetString("ClientSecret", Constants.LocalResourcesFile),
                Help = Localization.GetString("ClientSecret.Help", Constants.LocalResourcesFile),
                IsSecure = true,
                IsRequired = true,
            },
        };
    }

    private bool UpdateHostSettings(IDictionary<string, string> settings)
    {
		_logger.LogDebug(">>> Entering UpdateHostSettings(settings={Settings})", settings);
        var tenantId = this.GetSetting(Null.NullInteger, Constants.TenantIdSettingName);
        var clientId = this.GetSetting(Null.NullInteger, Constants.ClientIdSettingName);
		_logger.LogDebug("UpdateHostSettings(settings={Settings}): clientId → {ClientId}", settings, clientId);
        var clientSecret = this.GetSetting(Null.NullInteger, Constants.ClientSecretSettingName);
		_logger.LogDebug("UpdateHostSettings(settings={Settings}): clientSecret → {ClientSecret}", settings, clientSecret);
        var clientSecretEncryptionAlgorithm = this.GetSetting(Null.NullInteger, Constants.ClientSecretEncryptionAlgorithm);
		_logger.LogDebug("UpdateHostSettings(settings={Settings}): clientSecretEncryptionAlgorithm → {ClientSecretEncryptionAlgorithm}", settings, clientSecretEncryptionAlgorithm);
		_logger.LogDebug("UpdateHostSettings(settings={Settings}): tenantId → {TenantId}", settings, tenantId);

        var changed = false;
		_logger.LogDebug("UpdateHostSettings(settings={Settings}): changed → {Changed}", settings, changed);
        if (settings.ContainsKey(Constants.TenantIdSettingName) && settings[Constants.TenantIdSettingName] != tenantId)
        {
            this.hostSettingsService.Update(Constants.TenantIdSettingName, settings[Constants.TenantIdSettingName], false);
            changed = true;
        }

        if (settings.ContainsKey(Constants.ClientIdSettingName) && settings[Constants.ClientIdSettingName] != clientId)
        {
            this.hostSettingsService.Update(Constants.ClientIdSettingName, settings[Constants.ClientIdSettingName], false);
            changed = true;
        }

        if (settings.ContainsKey(Constants.ClientSecretSettingName))
        {
            if (settings[Constants.ClientSecretSettingName] != clientSecret || string.IsNullOrWhiteSpace(clientSecretEncryptionAlgorithm))
            {
                var (encryptedSecret, algorithmName, initializationVector) = this.cryptographyProvider.EncryptString(settings[Constants.ClientSecretSettingName], Config.GetDecryptionkey());
                this.hostSettingsService.Update(Constants.ClientSecretSettingName, encryptedSecret, false);
                this.hostSettingsService.Update(Constants.ClientSecretEncryptionAlgorithm, algorithmName, false);
                this.hostSettingsService.Update(Constants.ClientSecretInitializationVector, initializationVector, false);
                changed = true;
            }
        }

        if (changed)
        {
            DataCache.ClearCache();
        }

		_logger.LogDebug("<<< Exiting UpdateHostSettings(settings={Settings})", settings);
        return changed;
    }

    private bool UpdatePortalSettings(int portalId, IDictionary<string, string> settings)
    {
		_logger.LogDebug(">>> Entering UpdatePortalSettings(portalId={PortalId},settings={Settings})", portalId, settings);
        var tenantId = this.GetSetting(portalId, Constants.TenantIdSettingName);
        var clientId = this.GetSetting(portalId, Constants.ClientIdSettingName);
		_logger.LogDebug("UpdatePortalSettings(portalId={PortalId},settings={Settings}): clientId → {ClientId}", portalId, settings, clientId);
        var clientSecret = this.GetSetting(portalId, Constants.ClientSecretSettingName);
		_logger.LogDebug("UpdatePortalSettings(portalId={PortalId},settings={Settings}): clientSecret → {ClientSecret}", portalId, settings, clientSecret);
        var clientSecretEncryptionAlgorithm = this.GetSetting(portalId, Constants.ClientSecretEncryptionAlgorithm);
		_logger.LogDebug("UpdatePortalSettings(portalId={PortalId},settings={Settings}): clientSecretEncryptionAlgorithm → {ClientSecretEncryptionAlgorithm}", portalId, settings, clientSecretEncryptionAlgorithm);
		_logger.LogDebug("UpdatePortalSettings(portalId={PortalId},settings={Settings}): tenantId → {TenantId}", portalId, settings, tenantId);

        var changed = false;
		_logger.LogDebug("UpdatePortalSettings(portalId={PortalId},settings={Settings}): changed → {Changed}", portalId, settings, changed);
        if (settings.ContainsKey(Constants.TenantIdSettingName) && settings[Constants.TenantIdSettingName] != tenantId)
        {
            PortalController.UpdatePortalSetting(this.portalController, portalId, Constants.TenantIdSettingName, settings[Constants.TenantIdSettingName], false);
            changed = true;
        }

        if (settings.ContainsKey(Constants.ClientIdSettingName) && settings[Constants.ClientIdSettingName] != clientId)
        {
            PortalController.UpdatePortalSetting(this.portalController, portalId, Constants.ClientIdSettingName, settings[Constants.ClientIdSettingName], false);
            changed = true;
        }

        if (settings.ContainsKey(Constants.ClientSecretSettingName))
        {
            if (settings[Constants.ClientSecretSettingName] != clientSecret || string.IsNullOrWhiteSpace(clientSecretEncryptionAlgorithm))
            {
                var (encryptedSecret, algorithmName, initializationVector) = this.cryptographyProvider.EncryptString(settings[Constants.ClientSecretSettingName], Config.GetDecryptionkey());
                PortalController.UpdatePortalSetting(this.portalController, portalId, Constants.ClientSecretSettingName, encryptedSecret, false);
                PortalController.UpdatePortalSetting(this.portalController, portalId, Constants.ClientSecretEncryptionAlgorithm, algorithmName, false);
                PortalController.UpdatePortalSetting(this.portalController, portalId, Constants.ClientSecretInitializationVector, initializationVector, false);
                changed = true;
            }
        }

        if (changed)
        {
            DataCache.ClearPortalCache(portalId, false);
        }

		_logger.LogDebug("<<< Exiting UpdatePortalSettings(portalId={PortalId},settings={Settings})", portalId, settings);
        return changed;
    }

    private string GetSetting(int portalId, string settingName)
    {
		_logger.LogDebug(">>> Entering GetSetting(portalId={PortalId},settingName={SettingName})", portalId, settingName);
        var settings = this.GetSettings(portalId);
        var setting = settings.FirstOrDefault(i => i.Name == settingName);
		_logger.LogDebug("GetSetting(portalId={PortalId},settingName={SettingName}): setting → {Setting}", portalId, settingName, setting);
		_logger.LogDebug("GetSetting(portalId={PortalId},settingName={SettingName}): settings → {Settings}", portalId, settingName, settings);

		_logger.LogDebug("<<< Exiting GetSetting(portalId={PortalId},settingName={SettingName})", portalId, settingName);
        return setting?.Value ?? string.Empty;
    }

    private void DeleteSetting(int portalId, string settingName)
    {
		_logger.LogDebug(">>> Entering DeleteSetting(portalId={PortalId},settingName={SettingName})", portalId, settingName);
        if (portalId == Null.NullInteger)
        {
            this.hostSettingsService.Update(settingName, string.Empty, false);
        }
        else
        {
            PortalController.UpdatePortalSetting(this.portalController, portalId, settingName, string.Empty, false);
        }
		_logger.LogDebug("<<< Exiting DeleteSetting(portalId={PortalId},settingName={SettingName})", portalId, settingName);
    }
}
