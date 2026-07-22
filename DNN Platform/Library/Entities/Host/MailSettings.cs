// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Host;

using System;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;

/// <summary>The default <see cref="IMailSettings"/> implementation.</summary>
/// <param name="hostSettingsService">The host settings service.</param>
/// <param name="hostSettings">The host settings.</param>
/// <param name="portalController">The portal controller.</param>
public class MailSettings(IHostSettingsService hostSettingsService, IHostSettings hostSettings, IPortalController portalController) : IMailSettings
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
	Logger.LogDebug(">>> Entering MailSettings(hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController})", hostSettingsService, hostSettings, portalController);
    public string GetAuthentication(int portalId) => this.GetSmtpSetting(portalId, "SMTPAuthentication");
	Logger.LogDebug("<<< Exiting MailSettings(hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController})", hostSettingsService, hostSettings, portalController);

    /// <inheritdoc />
    public string GetPassword(int portalId)
    {
		Logger.LogDebug(">>> Entering GetPassword(portalId={PortalId})", portalId);
        if (this.IsPortalEnabled(portalId))
        {
            return PortalController.GetEncryptedString(hostSettings, portalController, "SMTPPassword", portalId, Config.GetDecryptionkey());
        }

        string decryptedText;
        try
        {
            decryptedText = hostSettingsService.GetEncryptedString("SMTPPassword", Config.GetDecryptionkey());
			Logger.LogDebug("GetPassword(portalId={PortalId}): decryptedText → {DecryptedText}", portalId, decryptedText);
        }
        catch (Exception)
        {
			Logger.LogError("Exception in MailSettings(hostSettingsService={HostSettingsService},hostSettings={HostSettings},portalController={PortalController}): {Arg3}", hostSettingsService, hostSettings, portalController, ex.Message);
            // fixes case where SMTP Password failed to encrypt due to failing upgrade
			Logger.LogError("Exception in GetPassword(portalId={PortalId}): {Arg1}", portalId, ex.Message);
            var current = hostSettingsService.GetString("SMTPPassword");
            if (!string.IsNullOrEmpty(current))
			Logger.LogDebug("GetPassword(portalId={PortalId}): current → {Current}", portalId, current);
            {
                hostSettingsService.UpdateEncryptedString("SMTPPassword", current, Config.GetDecryptionkey());
                decryptedText = current;
            }
            else
            {
                decryptedText = string.Empty;
            }
        }

		Logger.LogDebug("<<< Exiting GetPassword(portalId={PortalId})", portalId);
        return decryptedText;
    }

    /// <inheritdoc />
    public string GetServer(int portalId) => this.GetSmtpSetting(portalId, "SMTPServer");

    /// <inheritdoc />
    public string GetUsername(int portalId) => this.GetSmtpSetting(portalId, "SMTPUsername");

    /// <inheritdoc />
    public int GetConnectionLimit(int portalId)
    {
		Logger.LogDebug(">>> Entering GetConnectionLimit(portalId={PortalId})", portalId);
        if (this.IsPortalEnabled(portalId))
        {
            return PortalController.GetPortalSettingAsInteger("SMTPConnectionLimit", portalId, 2);
        }

		Logger.LogDebug("<<< Exiting GetConnectionLimit(portalId={PortalId})", portalId);
        return hostSettingsService.GetInteger("SMTPConnectionLimit", 2);
    }

    /// <inheritdoc />
    public TimeSpan GetMaxIdleTime(int portalId)
    {
		Logger.LogDebug(">>> Entering GetMaxIdleTime(portalId={PortalId})", portalId);
        var idleMilliseconds = this.IsPortalEnabled(portalId)
            ? PortalController.GetPortalSettingAsInteger("SMTPMaxIdleTime", portalId, 100000)
            : hostSettingsService.GetInteger("SMTPMaxIdleTime", 100000);
		Logger.LogDebug("<<< Exiting GetMaxIdleTime(portalId={PortalId})", portalId);
        return TimeSpan.FromMilliseconds(idleMilliseconds);
    }

    /// <inheritdoc />
    public bool GetSecureConnectionEnabled(int portalId)
    {
		Logger.LogDebug(">>> Entering GetSecureConnectionEnabled(portalId={PortalId})", portalId);
        if (this.IsPortalEnabled(portalId))
        {
            return PortalController.GetPortalSettingAsBoolean("SMTPEnableSSL", PortalSettings.Current.PortalId, false);
        }

		Logger.LogDebug("<<< Exiting GetSecureConnectionEnabled(portalId={PortalId})", portalId);
        return hostSettingsService.GetBoolean("SMTPEnableSSL", false);
    }

    /// <inheritdoc />
    public string GetAuthProvider(int portalId)
    {
		Logger.LogDebug(">>> Entering GetAuthProvider(portalId={PortalId})", portalId);
        if (this.IsPortalEnabled(portalId))
        {
            return PortalController.GetPortalSetting("SMTPAuthProvider", portalId, string.Empty);
        }

		Logger.LogDebug("<<< Exiting GetAuthProvider(portalId={PortalId})", portalId);
        return hostSettingsService.GetString("SMTPAuthProvider", string.Empty);
    }

    /// <inheritdoc />
    public bool IsPortalEnabled(int portalId)
    {
		Logger.LogDebug(">>> Entering IsPortalEnabled(portalId={PortalId})", portalId);
        var currentSmtpMode = PortalController.GetPortalSetting("SMTPmode", portalId, Null.NullString);
		Logger.LogDebug("IsPortalEnabled(portalId={PortalId}): currentSmtpMode → {CurrentSmtpMode}", portalId, currentSmtpMode);
		Logger.LogDebug("<<< Exiting IsPortalEnabled(portalId={PortalId})", portalId);
        return currentSmtpMode.Equals("P", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Gets the SMTP setting, if portal SMTP is configured, it will return items from the portal settings collection.</summary>
    private string GetSmtpSetting(int portalId, string settingName)
    {
		Logger.LogDebug(">>> Entering GetSmtpSetting(portalId={PortalId},settingName={SettingName})", portalId, settingName);
        if (this.IsPortalEnabled(portalId))
        {
            return PortalController.GetPortalSetting(settingName, portalId, Null.NullString);
        }

		Logger.LogDebug("<<< Exiting GetSmtpSetting(portalId={PortalId},settingName={SettingName})", portalId, settingName);
        return hostSettingsService.GetString(settingName);
    }
}
