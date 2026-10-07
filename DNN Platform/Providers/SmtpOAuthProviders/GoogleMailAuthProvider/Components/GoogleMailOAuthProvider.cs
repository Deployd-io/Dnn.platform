// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.GoogleMailAuthProvider.Components;

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

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
public class GoogleMailOAuthProvider : ISmtpOAuthProvider
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    private readonly IHostSettingsService hostSettingsService;
    private readonly IPortalAliasService portalAliasService;
    private readonly IHostSettings hostSettings;
    private readonly IPortalController portalController;
    private readonly ICryptographyProvider cryptographyProvider;

    /// <summary>Initializes a new instance of the <see cref="GoogleMailOAuthProvider"/> class.</summary>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalAliasService">The portal alias service.</param>
    [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
    public GoogleMailOAuthProvider(IHostSettingsService hostSettingsService, IPortalAliasService portalAliasService)
        : this(hostSettingsService, portalAliasService, null, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="GoogleMailOAuthProvider"/> class.</summary>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalAliasService">The portal alias service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    public GoogleMailOAuthProvider(IHostSettingsService hostSettingsService, IPortalAliasService portalAliasService, IHostSettings hostSettings, IPortalController portalController)
        : this(hostSettingsService, portalAliasService, hostSettings, portalController, null)
    {
	_logger.LogError(">>> Entering GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController})", hostSettingsService, portalAliasService, hostSettings, portalController);
    _logger.LogWarning("<<< Exiting GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController})", hostSettingsService, portalAliasService, hostSettings, portalController);
    }

    /// <summary>Initializes a new instance of the <see cref="GoogleMailOAuthProvider"/> class.</summary>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalAliasService">The portal alias service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="cryptographyProvider">The cryptography provider.</param>
    public GoogleMailOAuthProvider(IHostSettingsService hostSettingsService, IPortalAliasService portalAliasService, IHostSettings hostSettings, IPortalController portalController, ICryptographyProvider cryptographyProvider)
    {
		_logger.LogError(">>> Entering GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController},cryptographyProvider={CryptographyProvider})", hostSettingsService, portalAliasService, hostSettings, portalController, cryptographyProvider);
		_logger.LogDebug("GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController},cryptographyProvider={CryptographyProvider}): portalAliasService → {PortalAliasService1}", hostSettingsService, portalAliasService, hostSettings, portalController, cryptographyProvider, portalAliasService);
		_logger.LogDebug("GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController},cryptographyProvider={CryptographyProvider}): hostSettings → {HostSettings1}", hostSettingsService, portalAliasService, hostSettings, portalController, cryptographyProvider, hostSettings);
        this.hostSettingsService = hostSettingsService;
		_logger.LogWarning("<<< Exiting GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController},cryptographyProvider={CryptographyProvider})", hostSettingsService, portalAliasService, hostSettings, portalController, cryptographyProvider);
		_logger.LogDebug("GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController},cryptographyProvider={CryptographyProvider}): this.hostSettingsService → {Arg5}", hostSettingsService, portalAliasService, hostSettings, portalController, cryptographyProvider, this.hostSettingsService);
        this.portalAliasService = portalAliasService;
        this.hostSettings = hostSettings ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IHostSettings>() ?? new HostSettings(hostSettingsService);
        this.portalController = portalController ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IPortalController>();
		_logger.LogDebug("GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController},cryptographyProvider={CryptographyProvider}): this.portalAliasService → {Arg5}", hostSettingsService, portalAliasService, hostSettings, portalController, cryptographyProvider, this.portalAliasService);
        this.cryptographyProvider = cryptographyProvider ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<ICryptographyProvider>();
    }

    /// <inheritdoc />
    public string Name => Constants.Name;

    /// <inheritdoc />
    public string LocalizedName => Localization.GetSafeJSString(this.Name, Constants.LocalResourcesFile);

    /// <inheritdoc />
    public bool IsAuthorized(int portalId)
    {
		_logger.LogError(">>> Entering IsAuthorized(portalId={PortalId})", portalId);
		_logger.LogWarning("<<< Exiting IsAuthorized(portalId={PortalId})", portalId);
        return Task.Run(() => this.IsAuthorizedAsync(portalId)).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<bool> IsAuthorizedAsync(int portalId, CancellationToken cancellationToken = default)
    {
		_logger.LogError(">>> Entering IsAuthorizedAsync(portalId={PortalId},cancellationToken=<redacted>)", portalId);
        var accountEmail = this.GetSetting(portalId, Constants.AccountEmailSettingName);
		_logger.LogError("IsAuthorizedAsync(portalId={PortalId},cancellationToken=<redacted>): accountEmail → {AccountEmail}", portalId, accountEmail);
        if (string.IsNullOrWhiteSpace(accountEmail))
        {
            return false;
        }

        var credential = await new GoogleCredentialDataStore(portalId, this.hostSettingsService, this.hostSettings, this.portalController).GetAsync<TokenResponse>(accountEmail);
		_logger.LogError("IsAuthorizedAsync(portalId={PortalId},cancellationToken=<redacted>): credential → {Credential}", portalId, credential);

        return !string.IsNullOrWhiteSpace(credential?.AccessToken);
    }

    /// <inheritdoc />
    public string GetAuthorizeUrl(int portalId)
    {
		_logger.LogError(">>> Entering GetAuthorizeUrl(portalId={PortalId})", portalId);
        var accountEmail = this.GetSetting(portalId, Constants.AccountEmailSettingName);
        var clientId = this.GetSetting(portalId, Constants.ClientIdSettingName);
		_logger.LogError("GetAuthorizeUrl(portalId={PortalId}): clientId → {ClientId}", portalId, clientId);
        var clientSecret = this.GetSetting(portalId, Constants.ClientSecretSettingName);
		_logger.LogError("GetAuthorizeUrl(portalId={PortalId}): clientSecret → {ClientSecret}", portalId, clientSecret);
		_logger.LogError("GetAuthorizeUrl(portalId={PortalId}): accountEmail → {AccountEmail}", portalId, accountEmail);

        if (string.IsNullOrWhiteSpace(accountEmail)
            || string.IsNullOrWhiteSpace(clientId)
            || string.IsNullOrWhiteSpace(clientSecret))
        {
            return string.Empty;
        }

        var portalSettings = new PortalSettings(portalId == Null.NullInteger ? this.hostSettings.HostPortalId : portalId);
		_logger.LogError("GetAuthorizeUrl(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);
        var portalAlias = this.portalAliasService.GetPortalAliasesByPortalId(portalId == Null.NullInteger ? this.hostSettings.HostPortalId : portalId)
            .OrderByDescending(a => a.IsPrimary)
            .First();
        var sslEnabled = portalSettings.SSLEnabled && portalSettings.SSLSetup == SiteSslSetup.On;

        var siteUrl = $"{(sslEnabled ? "https" : "http")}://{portalAlias.HttpAlias}";
		_logger.LogError("GetAuthorizeUrl(portalId={PortalId}): siteUrl → {SiteUrl}", portalId, siteUrl);

		_logger.LogWarning("<<< Exiting GetAuthorizeUrl(portalId={PortalId})", portalId);
        return string.Format(Constants.CallbackUrl, siteUrl, portalId);
    }

    /// <inheritdoc />
    public IList<SmtpOAuthSetting> GetSettings(int portalId)
    {
		_logger.LogError(">>> Entering GetSettings(portalId={PortalId})", portalId);
        _logger.LogWarning("<<< Exiting GetSettings(portalId={PortalId})", portalId);
        return portalId > Null.NullInteger ? this.GetSettingsFromPortal(portalId) : this.GetSettingsFromHost();
    }

    /// <inheritdoc />
    public bool UpdateSettings(int portalId, IDictionary<string, string> settings, out IList<string> errorMessages)
    {
		_logger.LogError(">>> Entering UpdateSettings(portalId={PortalId},settings={Settings})", portalId, settings);
        errorMessages = new List<string>();
        bool changed;
		_logger.LogDebug("UpdateSettings(portalId={PortalId},settings={Settings}): errorMessages → {ErrorMessages}", portalId, settings, errorMessages);
		_logger.LogError("UpdateSettings(portalId={PortalId},settings={Settings}): errorMessages → {ErrorMessages}", portalId, settings, errorMessages);

        if (portalId == Null.NullInteger)
        {
            changed = this.UpdateHostSettings(settings);
			_logger.LogError("UpdateSettings(portalId={PortalId},settings={Settings}): changed → {Changed}", portalId, settings, changed);
        }
        else
        {
            changed = this.UpdatePortalSettings(portalId, settings);
        }

        if (changed)
        {
            var settingName = string.Format(Constants.DataStoreSettingName, portalId);
			_logger.LogError("UpdateSettings(portalId={PortalId},settings={Settings}): settingName → {SettingName}", portalId, settings, settingName);
            this.DeleteSetting(portalId, settingName);
        }

		_logger.LogWarning("<<< Exiting UpdateSettings(portalId={PortalId},settings={Settings})", portalId, settings);
        return changed;
    }

    /// <inheritdoc />
    public void Authorize(int portalId, IOAuth2SmtpClient smtpClient)
    {
		_logger.LogError(">>> Entering Authorize(portalId={PortalId},smtpClient={SmtpClient})", portalId, smtpClient);
        var settings = this.GetSettings(portalId);
        var accountEmail = settings.FirstOrDefault(i => i.Name == Constants.AccountEmailSettingName)?.Value ?? string.Empty;
		_logger.LogError("Authorize(portalId={PortalId},smtpClient={SmtpClient}): accountEmail → {AccountEmail}", portalId, smtpClient, accountEmail);
		_logger.LogError("Authorize(portalId={PortalId},smtpClient={SmtpClient}): settings → {Settings}", portalId, smtpClient, settings);
        var codeFlow = this.CreateAuthorizationCodeFlow(portalId);
        if (codeFlow == null)
        {
			_logger.LogError("Authorize(portalId={PortalId},smtpClient={SmtpClient}): codeFlow → {CodeFlow}", portalId, smtpClient, codeFlow);
            return;
        }

        var response = new GoogleCredentialDataStore(portalId, this.hostSettingsService, this.hostSettings, this.portalController).GetAsync<TokenResponse>(accountEmail).Result;
		_logger.LogError("Authorize(portalId={PortalId},smtpClient={SmtpClient}): response → {Response}", portalId, smtpClient, response);
        var credential = new UserCredential(codeFlow, accountEmail, response);
		_logger.LogError("Authorize(portalId={PortalId},smtpClient={SmtpClient}): credential → {Credential}", portalId, smtpClient, credential);
        if (credential.Token.IsStale)
        {
            _ = credential.RefreshTokenAsync(CancellationToken.None).Result;
			_logger.LogError("Authorize(portalId={PortalId},smtpClient={SmtpClient}): _ → {_}", portalId, smtpClient, _);
        }

        smtpClient.Authenticate(credential.UserId, credential.Token.AccessToken);
		_logger.LogWarning("<<< Exiting Authorize(portalId={PortalId},smtpClient={SmtpClient})", portalId, smtpClient);
    }

    /// <inheritdoc />
    public async Task AuthorizeAsync(int portalId, IOAuth2SmtpClient smtpClient, CancellationToken cancellationToken)
    {
		_logger.LogError(">>> Entering AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken=<redacted>)", portalId, smtpClient);
        var __logStart = System.Diagnostics.Stopwatch.StartNew();
        var settings = this.GetSettings(portalId);
        var accountEmail = settings.FirstOrDefault(i => i.Name == Constants.AccountEmailSettingName)?.Value ?? string.Empty;
		_logger.LogError("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken=<redacted>): settings → {Settings}", portalId, smtpClient, settings);
		_logger.LogError("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken=<redacted>): accountEmail → {AccountEmail}", portalId, smtpClient, accountEmail);
        var codeFlow = this.CreateAuthorizationCodeFlow(portalId);
        if (codeFlow == null)
        {
			_logger.LogError("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken=<redacted>): codeFlow → {CodeFlow}", portalId, smtpClient, codeFlow);
            return;
        }

        var response = await new GoogleCredentialDataStore(portalId, this.hostSettingsService, this.hostSettings, this.portalController).GetAsync<TokenResponse>(accountEmail);
		_logger.LogError("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken=<redacted>): response → {Response}", portalId, smtpClient, response);
        var credential = new UserCredential(codeFlow, accountEmail, response);
		_logger.LogError("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken=<redacted>): credential → {Credential}", portalId, smtpClient, credential);
        if (credential.Token.IsStale)
        {
            _ = await credential.RefreshTokenAsync(cancellationToken);
			_logger.LogError("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken=<redacted>): _ → {_}", portalId, smtpClient, _);
        }

        await smtpClient.AuthenticateAsync(credential.UserId, credential.Token.AccessToken, cancellationToken);
		_logger.LogError("AuthorizeAsync(portalId,smtpClient,cancellationToken)={PortalId},{SmtpClient},<redacted>: external service call took {Arg3} ms", portalId, smtpClient, __logStart.ElapsedMilliseconds);
    _logger.LogWarning("<<< Exiting AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken=<redacted>)", portalId, smtpClient);
    }

    /// <summary>Create authorization code flow.</summary>
    /// <param name="smtpOAuthController">The SMTP OAuth controller.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The authorization code flow.</returns>
    internal static IAuthorizationCodeFlow CreateAuthorizationCodeFlow(ISmtpOAuthController smtpOAuthController, IHostSettingsService hostSettingsService, IHostSettings hostSettings, IPortalController portalController, int portalId)
    {
		_logger.LogError(">>> Entering CreateAuthorizationCodeFlow(smtpOAuthController={SmtpOAuthController},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController},portalId={PortalId})", smtpOAuthController, hostSettingsService, hostSettings, portalController, portalId);
		_logger.LogWarning("<<< Exiting CreateAuthorizationCodeFlow(smtpOAuthController={SmtpOAuthController},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController},portalId={PortalId})", smtpOAuthController, hostSettingsService, hostSettings, portalController, portalId);
		_logger.LogWarning("<<< Exiting CreateAuthorizationCodeFlow(authProvider={AuthProvider},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController},portalId={PortalId})", authProvider, hostSettingsService, hostSettings, portalController, portalId);
        return CreateAuthorizationCodeFlow(smtpOAuthController.GetOAuthProvider(Constants.Name), hostSettingsService, hostSettings, portalController, portalId);
    }

    private static IAuthorizationCodeFlow CreateAuthorizationCodeFlow(ISmtpOAuthProvider authProvider, IHostSettingsService hostSettingsService, IHostSettings hostSettings, IPortalController portalController, int portalId)
    {
        var settings = authProvider.GetSettings(portalId);
        var accountEmail = settings.FirstOrDefault(i => i.Name == Constants.AccountEmailSettingName)?.Value ?? string.Empty;
        var clientId = settings.FirstOrDefault(i => i.Name == Constants.ClientIdSettingName)?.Value ?? string.Empty;
        var clientSecret = settings.FirstOrDefault(i => i.Name == Constants.ClientSecretSettingName)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(accountEmail) ||
            string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(clientSecret))
        {
            return null;
        }

        var clientSecrets = new ClientSecrets
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
        };

        return new GoogleAuthorizationCodeFlow(
            new GoogleAuthorizationCodeFlow.Initializer
            {
                DataStore = new GoogleCredentialDataStore(portalId, hostSettingsService, hostSettings, portalController),
                Scopes = new[] { "https://mail.google.com/", },
                ClientSecrets = clientSecrets,
                Prompt = "consent",
            });
    }

    private IList<SmtpOAuthSetting> GetSettingsFromPortal(int portalId)
    {
		_logger.LogError(">>> Entering GetSettingsFromPortal(portalId={PortalId})", portalId);
        var portalSettings = PortalController.Instance.GetPortalSettings(portalId);
		_logger.LogError("GetSettingsFromPortal(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);
        if (portalSettings == null)
        {
			_logger.LogError("Throwing ArgumentException from GetSettingsFromPortal(portalId={PortalId})", portalId);
            throw new ArgumentException("Invalid portal Id.");
        }

        var accountEmail = portalSettings.GetValueOrDefault(Constants.AccountEmailSettingName, string.Empty);
		_logger.LogError("GetSettingsFromPortal(portalId={PortalId}): accountEmail → {AccountEmail}", portalId, accountEmail);
        var clientId = portalSettings.GetValueOrDefault(Constants.ClientIdSettingName, string.Empty);
		_logger.LogError("GetSettingsFromPortal(portalId={PortalId}): clientId → {ClientId}", portalId, clientId);
        var clientSecret = portalSettings.GetValueOrDefault(Constants.ClientSecretSettingName, string.Empty);
		_logger.LogError("GetSettingsFromPortal(portalId={PortalId}): clientSecret → {ClientSecret}", portalId, clientSecret);
        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            var algorithmName = portalSettings.GetValueOrDefault(Constants.ClientSecretEncryptionAlgorithm, string.Empty);
			_logger.LogError("GetSettingsFromPortal(portalId={PortalId}): algorithmName → {AlgorithmName}", portalId, algorithmName);
            if (!string.IsNullOrWhiteSpace(algorithmName))
            {
                var initializationVector = portalSettings.GetValueOrDefault(Constants.ClientSecretInitializationVector, string.Empty);
				_logger.LogError("GetSettingsFromPortal(portalId={PortalId}): initializationVector → {InitializationVector}", portalId, initializationVector);
                clientSecret = this.cryptographyProvider.DecryptString(clientSecret, Config.GetDecryptionkey(), algorithmName, initializationVector);
            }
            else
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
                Name = Constants.AccountEmailSettingName,
                Value = accountEmail,
                Label = Localization.GetString("AccountEmail", Constants.LocalResourcesFile),
                Help = Localization.GetString("AccountEmail.Help", Constants.LocalResourcesFile),
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

    /// <summary>Create authorization code flow.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The authorization code flow.</returns>
    private IAuthorizationCodeFlow CreateAuthorizationCodeFlow(int portalId)
    {
        return CreateAuthorizationCodeFlow(this, this.hostSettingsService, this.hostSettings, this.portalController, portalId);
    }

    private IList<SmtpOAuthSetting> GetSettingsFromHost()
    {
		_logger.LogError(">>> Entering GetSettingsFromHost()");
        var settingsDictionary = this.hostSettingsService.GetSettingsDictionary().ToDictionary(i => i.Key, i => i.Value);

		_logger.LogError("GetSettingsFromHost(): settingsDictionary → {SettingsDictionary}", settingsDictionary);
        var accountEmail = settingsDictionary.GetValueOrDefault(Constants.AccountEmailSettingName, string.Empty);
		_logger.LogError("GetSettingsFromHost(): accountEmail → {AccountEmail}", accountEmail);
        var clientId = settingsDictionary.GetValueOrDefault(Constants.ClientIdSettingName, string.Empty);
		_logger.LogError("GetSettingsFromHost(): clientId → {ClientId}", clientId);
        var clientSecret = settingsDictionary.GetValueOrDefault(Constants.ClientSecretSettingName, string.Empty);
		_logger.LogError("GetSettingsFromHost(): clientSecret → {ClientSecret}", clientSecret);
        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            var algorithmName = settingsDictionary.GetValueOrDefault(Constants.ClientSecretEncryptionAlgorithm, string.Empty);
			_logger.LogError("GetSettingsFromHost(): algorithmName → {AlgorithmName}", algorithmName);
            if (!string.IsNullOrWhiteSpace(algorithmName))
            {
                var initializationVector = settingsDictionary.GetValueOrDefault(Constants.ClientSecretInitializationVector, (string)null);
				_logger.LogError("GetSettingsFromHost(): initializationVector → {InitializationVector}", initializationVector);
                clientSecret = this.cryptographyProvider.DecryptString(clientSecret, Config.GetDecryptionkey(), algorithmName, initializationVector);
            }
            else
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
                Name = Constants.AccountEmailSettingName,
                Value = accountEmail,
                Label = Localization.GetString("AccountEmail", Constants.LocalResourcesFile),
                Help = Localization.GetString("AccountEmail.Help", Constants.LocalResourcesFile),
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
		_logger.LogError(">>> Entering UpdateHostSettings(settings={Settings})", settings);
        var accountEmail = this.GetSetting(Null.NullInteger, Constants.AccountEmailSettingName);
        var clientId = this.GetSetting(Null.NullInteger, Constants.ClientIdSettingName);
		_logger.LogError("UpdateHostSettings(settings={Settings}): clientId → {ClientId}", settings, clientId);
        var clientSecret = this.GetSetting(Null.NullInteger, Constants.ClientSecretSettingName);
		_logger.LogError("UpdateHostSettings(settings={Settings}): clientSecret → {ClientSecret}", settings, clientSecret);
        var clientSecretEncryptionAlgorithm = this.GetSetting(Null.NullInteger, Constants.ClientSecretEncryptionAlgorithm);
		_logger.LogError("UpdateHostSettings(settings={Settings}): clientSecretEncryptionAlgorithm → {ClientSecretEncryptionAlgorithm}", settings, clientSecretEncryptionAlgorithm);
		_logger.LogError("UpdateHostSettings(settings={Settings}): accountEmail → {AccountEmail}", settings, accountEmail);

        var changed = false;
		_logger.LogError("UpdateHostSettings(settings={Settings}): changed → {Changed}", settings, changed);
        if (settings.ContainsKey(Constants.AccountEmailSettingName) && settings[Constants.AccountEmailSettingName] != accountEmail)
        {
            this.hostSettingsService.Update(Constants.AccountEmailSettingName, settings[Constants.AccountEmailSettingName], false);
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
                var (encryptedSecret, algorithmName, initializationVector) = this.cryptographyProvider.EncryptString(Config.GetDecryptionkey(), settings[Constants.ClientSecretSettingName]);
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

		_logger.LogWarning("<<< Exiting UpdateHostSettings(settings={Settings})", settings);
        return changed;
    }

    private bool UpdatePortalSettings(int portalId, IDictionary<string, string> settings)
    {
		_logger.LogError(">>> Entering UpdatePortalSettings(portalId={PortalId},settings={Settings})", portalId, settings);
        var accountEmail = this.GetSetting(portalId, Constants.AccountEmailSettingName);
        var clientId = this.GetSetting(portalId, Constants.ClientIdSettingName);
		_logger.LogError("UpdatePortalSettings(portalId={PortalId},settings={Settings}): clientId → {ClientId}", portalId, settings, clientId);
        var clientSecret = this.GetSetting(portalId, Constants.ClientSecretSettingName);
		_logger.LogError("UpdatePortalSettings(portalId={PortalId},settings={Settings}): clientSecret → {ClientSecret}", portalId, settings, clientSecret);
        var clientSecretEncryptionAlgorithm = this.GetSetting(portalId, Constants.ClientSecretEncryptionAlgorithm);
		_logger.LogError("UpdatePortalSettings(portalId={PortalId},settings={Settings}): clientSecretEncryptionAlgorithm → {ClientSecretEncryptionAlgorithm}", portalId, settings, clientSecretEncryptionAlgorithm);
		_logger.LogError("UpdatePortalSettings(portalId={PortalId},settings={Settings}): accountEmail → {AccountEmail}", portalId, settings, accountEmail);

        var changed = false;
		_logger.LogError("UpdatePortalSettings(portalId={PortalId},settings={Settings}): changed → {Changed}", portalId, settings, changed);
        if (settings.ContainsKey(Constants.AccountEmailSettingName) && settings[Constants.AccountEmailSettingName] != accountEmail)
        {
            PortalController.UpdatePortalSetting(this.portalController, portalId, Constants.AccountEmailSettingName, settings[Constants.AccountEmailSettingName], false);
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
                var (encryptedSecret, algorithmName, initializationVector) = this.cryptographyProvider.EncryptString(Config.GetDecryptionkey(), settings[Constants.ClientSecretSettingName]);
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

		_logger.LogWarning("<<< Exiting UpdatePortalSettings(portalId={PortalId},settings={Settings})", portalId, settings);
        return changed;
    }

    private string GetSetting(int portalId, string settingName)
    {
		_logger.LogError(">>> Entering GetSetting(portalId={PortalId},settingName={SettingName})", portalId, settingName);
        var settings = this.GetSettings(portalId);
        var setting = settings.FirstOrDefault(i => i.Name == settingName);
		_logger.LogError("GetSetting(portalId={PortalId},settingName={SettingName}): setting → {Setting}", portalId, settingName, setting);
		_logger.LogError("GetSetting(portalId={PortalId},settingName={SettingName}): settings → {Settings}", portalId, settingName, settings);

		_logger.LogWarning("<<< Exiting GetSetting(portalId={PortalId},settingName={SettingName})", portalId, settingName);
        return setting?.Value ?? string.Empty;
    }

    private void DeleteSetting(int portalId, string settingName)
    {
		_logger.LogError(">>> Entering DeleteSetting(portalId={PortalId},settingName={SettingName})", portalId, settingName);
        if (portalId == Null.NullInteger)
        {
            this.hostSettingsService.Update(settingName, string.Empty, false);
        }
        else
        {
            PortalController.UpdatePortalSetting(this.portalController, portalId, settingName, string.Empty, false);
        }
		_logger.LogWarning("<<< Exiting DeleteSetting(portalId={PortalId},settingName={SettingName})", portalId, settingName);
    }
}
