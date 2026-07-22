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

/// <inheritdoc />
public class GoogleMailOAuthProvider : ISmtpOAuthProvider
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
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
	Logger.LogDebug(">>> Entering GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService})", hostSettingsService, portalAliasService);
    }
	Logger.LogDebug("<<< Exiting GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService})", hostSettingsService, portalAliasService);

    /// <summary>Initializes a new instance of the <see cref="GoogleMailOAuthProvider"/> class.</summary>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalAliasService">The portal alias service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    public GoogleMailOAuthProvider(IHostSettingsService hostSettingsService, IPortalAliasService portalAliasService, IHostSettings hostSettings, IPortalController portalController)
        : this(hostSettingsService, portalAliasService, hostSettings, portalController, null)
    {
	Logger.LogDebug(">>> Entering GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController})", hostSettingsService, portalAliasService, hostSettings, portalController);
	Logger.LogDebug("<<< Exiting GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController})", hostSettingsService, portalAliasService, hostSettings, portalController);
	Logger.LogDebug("GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController},cryptographyProvider={CryptographyProvider}): portalAliasService → {PortalAliasService1}", hostSettingsService, portalAliasService, hostSettings, portalController, cryptographyProvider, portalAliasService);
    }

    /// <summary>Initializes a new instance of the <see cref="GoogleMailOAuthProvider"/> class.</summary>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalAliasService">The portal alias service.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="cryptographyProvider">The cryptography provider.</param>
    public GoogleMailOAuthProvider(IHostSettingsService hostSettingsService, IPortalAliasService portalAliasService, IHostSettings hostSettings, IPortalController portalController, ICryptographyProvider cryptographyProvider)
    {
        this.hostSettingsService = hostSettingsService;
        this.portalAliasService = portalAliasService;
		Logger.LogDebug("GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController},cryptographyProvider={CryptographyProvider}): this.hostSettingsService → {Arg5}", hostSettingsService, portalAliasService, hostSettings, portalController, cryptographyProvider, this.hostSettingsService);
		Logger.LogDebug("GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController},cryptographyProvider={CryptographyProvider}): this.portalAliasService → {Arg5}", hostSettingsService, portalAliasService, hostSettings, portalController, cryptographyProvider, this.portalAliasService);
		Logger.LogDebug("GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController},cryptographyProvider={CryptographyProvider}): portalController → {PortalController1}", hostSettingsService, portalAliasService, hostSettings, portalController, cryptographyProvider, portalController);
		Logger.LogDebug("GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController},cryptographyProvider={CryptographyProvider}): cryptographyProvider → {CryptographyProvider1}", hostSettingsService, portalAliasService, hostSettings, portalController, cryptographyProvider, cryptographyProvider);
		Logger.LogDebug("GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController},cryptographyProvider={CryptographyProvider}): hostSettingsService → {HostSettingsService1}", hostSettingsService, portalAliasService, hostSettings, portalController, cryptographyProvider, hostSettingsService);
        this.hostSettings = hostSettings ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IHostSettings>() ?? new HostSettings(hostSettingsService);
        this.portalController = portalController ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IPortalController>();
        this.cryptographyProvider = cryptographyProvider ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<ICryptographyProvider>();
		Logger.LogDebug("GoogleMailOAuthProvider(hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},hostSettings={HostSettings},portalController={PortalController},cryptographyProvider={CryptographyProvider}): hostSettings → {HostSettings1}", hostSettingsService, portalAliasService, hostSettings, portalController, cryptographyProvider, hostSettings);
    }

    /// <inheritdoc />
    public string Name => Constants.Name;

    /// <inheritdoc />
    public string LocalizedName => Localization.GetSafeJSString(this.Name, Constants.LocalResourcesFile);

    /// <inheritdoc />
    public bool IsAuthorized(int portalId)
    {
		Logger.LogDebug(">>> Entering IsAuthorized(portalId={PortalId})", portalId);
		Logger.LogDebug("<<< Exiting IsAuthorized(portalId={PortalId})", portalId);
        return Task.Run(() => this.IsAuthorizedAsync(portalId)).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<bool> IsAuthorizedAsync(int portalId, CancellationToken cancellationToken = default)
    {
		Logger.LogDebug(">>> Entering IsAuthorizedAsync(portalId={PortalId},cancellationToken={CancellationToken})", portalId, cancellationToken);
        var accountEmail = this.GetSetting(portalId, Constants.AccountEmailSettingName);
        if (string.IsNullOrWhiteSpace(accountEmail))
		Logger.LogDebug("IsAuthorizedAsync(portalId={PortalId},cancellationToken={CancellationToken}): accountEmail → {AccountEmail}", portalId, cancellationToken, accountEmail);
        {
            return false;
        }

        var credential = await new GoogleCredentialDataStore(portalId, this.hostSettingsService, this.hostSettings, this.portalController).GetAsync<TokenResponse>(accountEmail);

		Logger.LogDebug("IsAuthorizedAsync(portalId={PortalId},cancellationToken={CancellationToken}): credential → {Credential}", portalId, cancellationToken, credential);
		Logger.LogDebug("<<< Exiting IsAuthorizedAsync(portalId={PortalId},cancellationToken={CancellationToken})", portalId, cancellationToken);
        return !string.IsNullOrWhiteSpace(credential?.AccessToken);
    }

    /// <inheritdoc />
    public string GetAuthorizeUrl(int portalId)
    {
		Logger.LogDebug(">>> Entering GetAuthorizeUrl(portalId={PortalId})", portalId);
        var accountEmail = this.GetSetting(portalId, Constants.AccountEmailSettingName);
        var clientId = this.GetSetting(portalId, Constants.ClientIdSettingName);
		Logger.LogDebug("GetAuthorizeUrl(portalId={PortalId}): accountEmail → {AccountEmail}", portalId, accountEmail);
		Logger.LogDebug("GetAuthorizeUrl(portalId={PortalId}): clientId → {ClientId}", portalId, clientId);
        var clientSecret = this.GetSetting(portalId, Constants.ClientSecretSettingName);

		Logger.LogDebug("GetAuthorizeUrl(portalId={PortalId}): clientSecret → {ClientSecret}", portalId, clientSecret);
        if (string.IsNullOrWhiteSpace(accountEmail)
            || string.IsNullOrWhiteSpace(clientId)
            || string.IsNullOrWhiteSpace(clientSecret))
        {
            return string.Empty;
        }

        var portalSettings = new PortalSettings(portalId == Null.NullInteger ? this.hostSettings.HostPortalId : portalId);
        var portalAlias = this.portalAliasService.GetPortalAliasesByPortalId(portalId == Null.NullInteger ? this.hostSettings.HostPortalId : portalId)
            .OrderByDescending(a => a.IsPrimary)
            .First();
			Logger.LogDebug("GetAuthorizeUrl(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);
        var sslEnabled = portalSettings.SSLEnabled && portalSettings.SSLSetup == SiteSslSetup.On;

        var siteUrl = $"{(sslEnabled ? "https" : "http")}://{portalAlias.HttpAlias}";

		Logger.LogDebug("GetAuthorizeUrl(portalId={PortalId}): siteUrl → {SiteUrl}", portalId, siteUrl);
		Logger.LogDebug("<<< Exiting GetAuthorizeUrl(portalId={PortalId})", portalId);
        return string.Format(Constants.CallbackUrl, siteUrl, portalId);
    }

    /// <inheritdoc />
    public IList<SmtpOAuthSetting> GetSettings(int portalId)
    {
		Logger.LogDebug(">>> Entering GetSettings(portalId={PortalId})", portalId);
		Logger.LogDebug("<<< Exiting GetSettings(portalId={PortalId})", portalId);
        return portalId > Null.NullInteger ? this.GetSettingsFromPortal(portalId) : this.GetSettingsFromHost();
    }

    /// <inheritdoc />
    public bool UpdateSettings(int portalId, IDictionary<string, string> settings, out IList<string> errorMessages)
    {
        errorMessages = new List<string>();
        bool changed;

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
            var settingName = string.Format(Constants.DataStoreSettingName, portalId);
            this.DeleteSetting(portalId, settingName);
        }

        return changed;
    }

    /// <inheritdoc />
    public void Authorize(int portalId, IOAuth2SmtpClient smtpClient)
    {
		Logger.LogDebug(">>> Entering Authorize(portalId={PortalId},smtpClient={SmtpClient})", portalId, smtpClient);
        var settings = this.GetSettings(portalId);
        var accountEmail = settings.FirstOrDefault(i => i.Name == Constants.AccountEmailSettingName)?.Value ?? string.Empty;
		Logger.LogDebug("Authorize(portalId={PortalId},smtpClient={SmtpClient}): settings → {Settings}", portalId, smtpClient, settings);
		Logger.LogDebug("Authorize(portalId={PortalId},smtpClient={SmtpClient}): accountEmail → {AccountEmail}", portalId, smtpClient, accountEmail);
        var codeFlow = this.CreateAuthorizationCodeFlow(portalId);
        if (codeFlow == null)
		Logger.LogDebug("Authorize(portalId={PortalId},smtpClient={SmtpClient}): codeFlow → {CodeFlow}", portalId, smtpClient, codeFlow);
        {
            return;
        }

        var response = new GoogleCredentialDataStore(portalId, this.hostSettingsService, this.hostSettings, this.portalController).GetAsync<TokenResponse>(accountEmail).Result;
        var credential = new UserCredential(codeFlow, accountEmail, response);
		Logger.LogDebug("Authorize(portalId={PortalId},smtpClient={SmtpClient}): response → {Response}", portalId, smtpClient, response);
		Logger.LogDebug("Authorize(portalId={PortalId},smtpClient={SmtpClient}): credential → {Credential}", portalId, smtpClient, credential);
        if (credential.Token.IsStale)
        {
            _ = credential.RefreshTokenAsync(CancellationToken.None).Result;
        }
		Logger.LogDebug("Authorize(portalId={PortalId},smtpClient={SmtpClient}): _ → {_}", portalId, smtpClient, _);

        smtpClient.Authenticate(credential.UserId, credential.Token.AccessToken);
		Logger.LogDebug("<<< Exiting Authorize(portalId={PortalId},smtpClient={SmtpClient})", portalId, smtpClient);
    }

    /// <inheritdoc />
    public async Task AuthorizeAsync(int portalId, IOAuth2SmtpClient smtpClient, CancellationToken cancellationToken)
    {
		Logger.LogDebug(">>> Entering AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken={CancellationToken})", portalId, smtpClient, cancellationToken);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        var settings = this.GetSettings(portalId);
        var accountEmail = settings.FirstOrDefault(i => i.Name == Constants.AccountEmailSettingName)?.Value ?? string.Empty;
		Logger.LogDebug("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken={CancellationToken}): settings → {Settings}", portalId, smtpClient, cancellationToken, settings);
		Logger.LogDebug("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken={CancellationToken}): accountEmail → {AccountEmail}", portalId, smtpClient, cancellationToken, accountEmail);
        var codeFlow = this.CreateAuthorizationCodeFlow(portalId);
        if (codeFlow == null)
		Logger.LogDebug("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken={CancellationToken}): codeFlow → {CodeFlow}", portalId, smtpClient, cancellationToken, codeFlow);
        {
            return;
        }

        var response = await new GoogleCredentialDataStore(portalId, this.hostSettingsService, this.hostSettings, this.portalController).GetAsync<TokenResponse>(accountEmail);
        var credential = new UserCredential(codeFlow, accountEmail, response);
		Logger.LogDebug("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken={CancellationToken}): response → {Response}", portalId, smtpClient, cancellationToken, response);
		Logger.LogDebug("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken={CancellationToken}): credential → {Credential}", portalId, smtpClient, cancellationToken, credential);
        if (credential.Token.IsStale)
        {
            _ = await credential.RefreshTokenAsync(cancellationToken);
        }
		Logger.LogDebug("AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken={CancellationToken}): _ → {_}", portalId, smtpClient, cancellationToken, _);

        await smtpClient.AuthenticateAsync(credential.UserId, credential.Token.AccessToken, cancellationToken);
		Logger.LogInformation("AuthorizeAsync(portalId,smtpClient,cancellationToken)={PortalId},{SmtpClient},{CancellationToken}: external service call took {Arg3} ms", portalId, smtpClient, cancellationToken, __logStart.ElapsedMilliseconds);
		Logger.LogDebug("<<< Exiting AuthorizeAsync(portalId={PortalId},smtpClient={SmtpClient},cancellationToken={CancellationToken})", portalId, smtpClient, cancellationToken);
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
		Logger.LogDebug(">>> Entering CreateAuthorizationCodeFlow(smtpOAuthController={SmtpOAuthController},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController},portalId={PortalId})", smtpOAuthController, hostSettingsService, hostSettings, portalController, portalId);
		Logger.LogDebug("<<< Exiting CreateAuthorizationCodeFlow(smtpOAuthController={SmtpOAuthController},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController},portalId={PortalId})", smtpOAuthController, hostSettingsService, hostSettings, portalController, portalId);
		Logger.LogDebug("<<< Exiting CreateAuthorizationCodeFlow(portalId={PortalId})", portalId);
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

		Logger.LogDebug("<<< Exiting CreateAuthorizationCodeFlow(authProvider={AuthProvider},hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController},portalId={PortalId})", authProvider, hostSettingsService, hostSettings, portalController, portalId);
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
		Logger.LogDebug(">>> Entering GetSettingsFromPortal(portalId={PortalId})", portalId);
        var portalSettings = PortalController.Instance.GetPortalSettings(portalId);
        if (portalSettings == null)
		Logger.LogDebug("GetSettingsFromPortal(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);
        {
            throw new ArgumentException("Invalid portal Id.");
        }

        var accountEmail = portalSettings.GetValueOrDefault(Constants.AccountEmailSettingName, string.Empty);
        var clientId = portalSettings.GetValueOrDefault(Constants.ClientIdSettingName, string.Empty);
		Logger.LogDebug("GetSettingsFromPortal(portalId={PortalId}): accountEmail → {AccountEmail}", portalId, accountEmail);
		Logger.LogDebug("GetSettingsFromPortal(portalId={PortalId}): clientId → {ClientId}", portalId, clientId);
        var clientSecret = portalSettings.GetValueOrDefault(Constants.ClientSecretSettingName, string.Empty);
        if (!string.IsNullOrWhiteSpace(clientSecret))
		Logger.LogDebug("GetSettingsFromPortal(portalId={PortalId}): clientSecret → {ClientSecret}", portalId, clientSecret);
        {
            var algorithmName = portalSettings.GetValueOrDefault(Constants.ClientSecretEncryptionAlgorithm, string.Empty);
            if (!string.IsNullOrWhiteSpace(algorithmName))
			Logger.LogDebug("GetSettingsFromPortal(portalId={PortalId}): algorithmName → {AlgorithmName}", portalId, algorithmName);
            {
                var initializationVector = portalSettings.GetValueOrDefault(Constants.ClientSecretInitializationVector, string.Empty);
                clientSecret = this.cryptographyProvider.DecryptString(clientSecret, Config.GetDecryptionkey(), algorithmName, initializationVector);
				Logger.LogDebug("GetSettingsFromPortal(portalId={PortalId}): initializationVector → {InitializationVector}", portalId, initializationVector);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                clientSecret = PortalSecurity.Instance.Decrypt(Config.GetDecryptionkey(), clientSecret);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

		Logger.LogDebug("<<< Exiting GetSettingsFromPortal(portalId={PortalId})", portalId);
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
		Logger.LogDebug(">>> Entering GetSettingsFromHost()");
        var settingsDictionary = this.hostSettingsService.GetSettingsDictionary().ToDictionary(i => i.Key, i => i.Value);

		Logger.LogDebug("GetSettingsFromHost(): settingsDictionary → {SettingsDictionary}", settingsDictionary);
        var accountEmail = settingsDictionary.GetValueOrDefault(Constants.AccountEmailSettingName, string.Empty);
        var clientId = settingsDictionary.GetValueOrDefault(Constants.ClientIdSettingName, string.Empty);
		Logger.LogDebug("GetSettingsFromHost(): accountEmail → {AccountEmail}", accountEmail);
		Logger.LogDebug("GetSettingsFromHost(): clientId → {ClientId}", clientId);
        var clientSecret = settingsDictionary.GetValueOrDefault(Constants.ClientSecretSettingName, string.Empty);
        if (!string.IsNullOrWhiteSpace(clientSecret))
		Logger.LogDebug("GetSettingsFromHost(): clientSecret → {ClientSecret}", clientSecret);
        {
            var algorithmName = settingsDictionary.GetValueOrDefault(Constants.ClientSecretEncryptionAlgorithm, string.Empty);
            if (!string.IsNullOrWhiteSpace(algorithmName))
			Logger.LogDebug("GetSettingsFromHost(): algorithmName → {AlgorithmName}", algorithmName);
            {
                var initializationVector = settingsDictionary.GetValueOrDefault(Constants.ClientSecretInitializationVector, (string)null);
                clientSecret = this.cryptographyProvider.DecryptString(clientSecret, Config.GetDecryptionkey(), algorithmName, initializationVector);
				Logger.LogDebug("GetSettingsFromHost(): initializationVector → {InitializationVector}", initializationVector);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                clientSecret = PortalSecurity.Instance.Decrypt(Config.GetDecryptionkey(), clientSecret);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

		Logger.LogDebug("<<< Exiting GetSettingsFromHost()");
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
        var accountEmail = this.GetSetting(Null.NullInteger, Constants.AccountEmailSettingName);
        var clientId = this.GetSetting(Null.NullInteger, Constants.ClientIdSettingName);
        var clientSecret = this.GetSetting(Null.NullInteger, Constants.ClientSecretSettingName);
        var clientSecretEncryptionAlgorithm = this.GetSetting(Null.NullInteger, Constants.ClientSecretEncryptionAlgorithm);

        var changed = false;
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

        return changed;
    }

    private bool UpdatePortalSettings(int portalId, IDictionary<string, string> settings)
    {
        var accountEmail = this.GetSetting(portalId, Constants.AccountEmailSettingName);
        var clientId = this.GetSetting(portalId, Constants.ClientIdSettingName);
        var clientSecret = this.GetSetting(portalId, Constants.ClientSecretSettingName);
        var clientSecretEncryptionAlgorithm = this.GetSetting(portalId, Constants.ClientSecretEncryptionAlgorithm);

        var changed = false;
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

        return changed;
    }

    private string GetSetting(int portalId, string settingName)
    {
        var settings = this.GetSettings(portalId);
        var setting = settings.FirstOrDefault(i => i.Name == settingName);

        return setting?.Value ?? string.Empty;
    }

    private void DeleteSetting(int portalId, string settingName)
    {
        if (portalId == Null.NullInteger)
        {
            this.hostSettingsService.Update(settingName, string.Empty, false);
        }
        else
        {
            PortalController.UpdatePortalSetting(this.portalController, portalId, settingName, string.Empty, false);
        }
    }
}
