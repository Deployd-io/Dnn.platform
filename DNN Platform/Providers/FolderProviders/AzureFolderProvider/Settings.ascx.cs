// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Providers.FolderProviders.AzureFolderProvider
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>Windows Azure Storage Settings Control.</summary>
    public partial class Settings : FolderMappingSettingsControlBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Settings));
        private readonly ICryptographyProvider cryptographyProvider;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="Settings"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Use overload with ICryptographyProvider. Scheduled for removal in v12.0.0.")]
        public Settings()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Settings"/> class.</summary>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        /// <param name="hostSettings">The host settings.</param>
        public Settings(ICryptographyProvider cryptographyProvider, IHostSettings hostSettings)
        {
			Logger.LogError(">>> Entering Settings(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings})", cryptographyProvider, hostSettings);
			Logger.LogDebug("Settings(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings}): hostSettings → {HostSettings1}", cryptographyProvider, hostSettings, hostSettings);
            this.cryptographyProvider = cryptographyProvider ?? this.DependencyProvider.GetRequiredService<ICryptographyProvider>();
			Logger.LogWarning("<<< Exiting Settings(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings})", cryptographyProvider, hostSettings);
            this.hostSettings = hostSettings ?? this.DependencyProvider.GetRequiredService<IHostSettings>();
        }

        /// <summary>Loads concrete settings.</summary>
        /// <param name="folderMappingSettings">The Hashtable containing the folder mapping settings.</param>
        public override void LoadSettings(Hashtable folderMappingSettings)
        {
			Logger.LogError(">>> Entering LoadSettings(folderMappingSettings=<redacted>)");
            if (folderMappingSettings.ContainsKey(Constants.AccountName))
            {
                this.tbAccountName.Text = FolderProvider.GetEncryptedSetting(this.cryptographyProvider, this.hostSettings, folderMappingSettings, Constants.AccountName);
            }

            if (folderMappingSettings.ContainsKey(Constants.AccountKey))
            {
                this.tbAccountKey.Text = FolderProvider.GetEncryptedSetting(this.cryptographyProvider, this.hostSettings, folderMappingSettings, Constants.AccountKey);
            }

            if (this.tbAccountName.Text.Length > 0 && this.tbAccountKey.Text.Length > 0)
            {
                var bucketName = string.Empty;
				Logger.LogError("LoadSettings(folderMappingSettings=<redacted>): bucketName → {BucketName}", bucketName);

                if (folderMappingSettings.ContainsKey(Constants.Container))
                {
                    bucketName = folderMappingSettings[Constants.Container].ToString();
                }

                this.LoadContainers();

                var bucket = this.ddlContainers.Items.FindByText(bucketName);
				Logger.LogError("LoadSettings(folderMappingSettings=<redacted>): bucket → {Bucket}", bucket);

                if (bucket != null)
                {
                    this.ddlContainers.SelectedIndex = this.ddlContainers.Items.IndexOf(bucket);
                }
            }

            if (folderMappingSettings.ContainsKey(Constants.UseHttps))
            {
                this.chkUseHttps.Checked = bool.Parse(folderMappingSettings[Constants.UseHttps].ToString());
            }

            this.chkDirectLink.Checked = !folderMappingSettings.ContainsKey(Constants.DirectLink) || folderMappingSettings[Constants.DirectLink].ToString().ToLowerInvariant() == "true";

            if (folderMappingSettings.ContainsKey(Constants.CustomDomain))
            {
                this.tbCustomDomain.Text = FolderProvider.GetEncryptedSetting(this.cryptographyProvider, this.hostSettings, folderMappingSettings, Constants.CustomDomain);
            }

            if (folderMappingSettings.ContainsKey(Constants.SyncBatchSize) && folderMappingSettings[Constants.SyncBatchSize] != null)
            {
                this.tbSyncBatchSize.Text = folderMappingSettings[Constants.SyncBatchSize].ToString();
            }
            else
            {
                this.tbSyncBatchSize.Text = Constants.DefaultSyncBatchSize.ToString();
            }
			Logger.LogWarning("<<< Exiting LoadSettings(folderMappingSettings=<redacted>)");
        }

        /// <summary>Updates concrete settings for the specified folder mapping.</summary>
        /// <param name="folderMappingID">The folder mapping identifier.</param>
        public override void UpdateSettings(int folderMappingID)
        {
			Logger.LogError(">>> Entering UpdateSettings(folderMappingID=<redacted>)");
            this.Page.Validate();

            if (!this.Page.IsValid)
            {
				Logger.LogError("Throwing Exception from UpdateSettings(folderMappingID=<redacted>)");
                throw new Exception();
            }

            var folderMappingController = FolderMappingController.Instance;
            var folderMapping = folderMappingController.GetFolderMapping(folderMappingID);
			Logger.LogError("UpdateSettings(folderMappingID=<redacted>): folderMapping → {FolderMapping}", folderMapping);

            var accountName = FolderProvider.EncryptValue(this.cryptographyProvider, this.hostSettings, this.tbAccountName.Text).EncryptedMessage;
			Logger.LogError("UpdateSettings(folderMappingID=<redacted>): accountName → {AccountName}", accountName);
            var container = this.GetContainer();
			Logger.LogError("UpdateSettings(folderMappingID=<redacted>): container → {Container}", container);

            if (AreThereFolderMappingsWithSameSettings(folderMapping, accountName, container))
            {
                this.valContainerName.ErrorMessage = Localization.GetString("MultipleFolderMappingsWithSameSettings.ErrorMessage", this.LocalResourceFile);
                this.valContainerName.IsValid = false;

                throw new Exception();
            }

            this.SetEncryptedSetting(folderMapping, Constants.AccountName, this.tbAccountName.Text);
            this.SetEncryptedSetting(folderMapping, Constants.AccountKey, this.tbAccountKey.Text);
            folderMapping.FolderMappingSettings[Constants.Container] = container;
            folderMapping.FolderMappingSettings[Constants.UseHttps] = this.GetUseHttps();
            folderMapping.FolderMappingSettings[Constants.DirectLink] = this.chkDirectLink.Checked;
            this.SetEncryptedSetting(folderMapping, Constants.CustomDomain, this.tbCustomDomain.Text);
            folderMapping.FolderMappingSettings[Constants.SyncBatchSize] = this.GetSynchBatchSize();

            folderMappingController.UpdateFolderMapping(folderMapping);
			Logger.LogWarning("<<< Exiting UpdateSettings(folderMappingID=<redacted>)");
        }

        /// <summary>Handles the <see cref="ListControl.SelectedIndexChanged"/> event of the Container drop-down list.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        protected void ddlContainers_SelectedIndexChanged(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering ddlContainers_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
            if (this.ddlContainers.SelectedIndex != 1)
            {
                return;
            }

            if (this.tbAccountName.Text.Trim().Length > 0 && this.tbAccountKey.Text.Trim().Length > 0)
            {
                this.ddlContainers.Items.Clear();

                this.ddlContainers.Items.Add(Localization.GetString("SelectContainer", this.LocalResourceFile));
                this.ddlContainers.Items.Add(Localization.GetString("RefreshContainerList", this.LocalResourceFile));

                this.LoadContainers();

                if (this.ddlContainers.Items.Count == 3)
                {
                    // If there is only one container, then select it
                    this.ddlContainers.SelectedValue = this.ddlContainers.Items[2].Value;
                }
            }
            else
            {
                this.valContainerName.ErrorMessage = Localization.GetString("CredentialsRequired.ErrorMessage", this.LocalResourceFile);
                this.valContainerName.IsValid = false;
            }
			Logger.LogWarning("<<< Exiting ddlContainers_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
        }

        /// <summary>Handles the click event for the New Container button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        protected void btnNewContainer_Click(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering btnNewContainer_Click(sender={Sender},e={E})", sender, e);
            this.SelectContainerPanel.Visible = false;
            this.CreateContainerPanel.Visible = true;
			Logger.LogWarning("<<< Exiting btnNewContainer_Click(sender={Sender},e={E})", sender, e);
        }

        /// <summary>Handles the click event for the Select Existing Container button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        protected void btnSelectExistingContainer_Click(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering btnSelectExistingContainer_Click(sender={Sender},e={E})", sender, e);
            this.SelectContainerPanel.Visible = true;
            this.CreateContainerPanel.Visible = false;
			Logger.LogWarning("<<< Exiting btnSelectExistingContainer_Click(sender={Sender},e={E})", sender, e);
        }

        /// <summary>Handles the <see cref="CustomValidator.ServerValidate"/> event of the Container Name validator.</summary>
        /// <param name="source">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        protected void valContainerName_ServerValidate(object source, ServerValidateEventArgs args)
        {
			Logger.LogError(">>> Entering valContainerName_ServerValidate(source={Source},args={Args})", source, args);
            if (this.SelectContainerPanel.Visible)
            {
                if (this.ddlContainers.SelectedIndex > 1)
                {
                    args.IsValid = true;
					Logger.LogDebug("valContainerName_ServerValidate(source={Source},args={Args}): args.IsValid → {Arg2}", source, args, args.IsValid);
                    return;
                }
            }
            else
            {
                if (this.tbContainerName.Text.Trim().Length > 0)
                {
                    args.IsValid = true;
                    return;
                }
            }

            this.valContainerName.ErrorMessage = Localization.GetString("valContainerName.ErrorMessage", this.LocalResourceFile);
            args.IsValid = false;
			Logger.LogWarning("<<< Exiting valContainerName_ServerValidate(source={Source},args={Args})", source, args);
        }

        private static bool AreThereFolderMappingsWithSameSettings(FolderMappingInfo folderMapping, string accountName, string container)
        {
			Logger.LogError(">>> Entering AreThereFolderMappingsWithSameSettings(folderMapping=<redacted>,accountName={AccountName},container={Container})", accountName, container);
            var folderMappingController = FolderMappingController.Instance;
            var folderMappings = folderMappingController.GetFolderMappings(folderMapping.PortalID);
			Logger.LogError("AreThereFolderMappingsWithSameSettings(folderMapping=<redacted>,accountName={AccountName},container={Container}): folderMappings → {FolderMappings}", accountName, container, folderMappings);

			Logger.LogWarning("<<< Exiting AreThereFolderMappingsWithSameSettings(folderMapping=<redacted>,accountName={AccountName},container={Container})", accountName, container);
            return folderMappings
                .Where(fm => fm.FolderMappingID != folderMapping.FolderMappingID && fm.FolderProviderType == folderMapping.FolderProviderType)
                .Any(fm => fm.FolderMappingSettings[Constants.AccountName].ToString().Equals(accountName, StringComparison.InvariantCulture) &&
                           fm.FolderMappingSettings[Constants.Container].ToString().Equals(container, StringComparison.InvariantCultureIgnoreCase));
        }

        private void SetEncryptedSetting(FolderMappingInfo folderMapping, string settingName, string settingValue)
        {
			Logger.LogError(">>> Entering SetEncryptedSetting(folderMapping=<redacted>,settingName={SettingName},settingValue={SettingValue})", settingName, settingValue);
            var (encryptedValue, algorithmName, initializationVector) = FolderProvider.EncryptValue(this.cryptographyProvider, this.hostSettings, settingValue);
            folderMapping.FolderMappingSettings[settingName] = encryptedValue;
            folderMapping.FolderMappingSettings[FolderProvider.GetAlgorithmSettingKey(settingName)] = algorithmName;
            folderMapping.FolderMappingSettings[FolderProvider.GetInitializationVectorSettingKey(settingName)] = initializationVector;
			Logger.LogWarning("<<< Exiting SetEncryptedSetting(folderMapping=<redacted>,settingName={SettingName},settingValue={SettingValue})", settingName, settingValue);
        }

        private string GetUseHttps()
        {
			Logger.LogError(">>> Entering GetUseHttps()");
            Logger.LogWarning("<<< Exiting GetUseHttps()");
            return this.chkUseHttps.Checked.ToString();
        }

        private string GetContainer()
        {
			Logger.LogError(">>> Entering GetContainer()");
            string container;

            if (this.SelectContainerPanel.Visible)
            {
                container = this.ddlContainers.SelectedValue;
				Logger.LogError("GetContainer(): container → {Container}", container);
            }
            else
            {
                container = this.tbContainerName.Text.Trim().ToLowerInvariant();
                if (!this.CreateContainer(container))
                {
					Logger.LogError("Throwing Exception from GetContainer()");
                    throw new Exception();
                }
            }

			Logger.LogWarning("<<< Exiting GetContainer()");
            return container;
        }

        private bool CreateContainer(string containerName)
        {
			Logger.LogError(">>> Entering CreateContainer(containerName={ContainerName})", containerName);
            var accountName = this.tbAccountName.Text.Trim();
            var accountKey = this.tbAccountKey.Text.Trim();
			Logger.LogError("CreateContainer(containerName={ContainerName}): accountKey → {AccountKey}", containerName, accountKey);
			Logger.LogError("CreateContainer(containerName={ContainerName}): accountName → {AccountName}", containerName, accountName);
            var useHttps = this.chkUseHttps.Checked;

            StorageCredentials sc;

            try
            {
                sc = new StorageCredentials(accountName, accountKey);
				Logger.LogError("CreateContainer(containerName={ContainerName}): sc → {Sc}", containerName, sc);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                this.valContainerName.ErrorMessage = Localization.GetString("AuthenticationFailure.ErrorMessage", this.LocalResourceFile);
                this.valContainerName.IsValid = false;

                return false;
            }

            var csa = new CloudStorageAccount(sc, useHttps);
			Logger.LogError("CreateContainer(containerName={ContainerName}): csa → {Csa}", containerName, csa);
            var blobClient = csa.CreateCloudBlobClient();
			Logger.LogError("CreateContainer(containerName={ContainerName}): blobClient → {BlobClient}", containerName, blobClient);
            var container = blobClient.GetContainerReference(containerName);
			Logger.LogError("CreateContainer(containerName={ContainerName}): container → {Container}", containerName, container);

            try
            {
                if (container.CreateIfNotExists())
                {
                    var permissions = container.GetPermissions();
					Logger.LogError("CreateContainer(containerName={ContainerName}): permissions → {Permissions}", containerName, permissions);
                    permissions.PublicAccess = BlobContainerPublicAccessType.Container;
					Logger.LogDebug("CreateContainer(containerName={ContainerName}): permissions.PublicAccess → {Arg1}", containerName, permissions.PublicAccess);
                    container.SetPermissions(permissions);
                }

                return true;
            }
            catch (StorageException ex)
            {
				Logger.LogError("Exception in CreateContainer(containerName={ContainerName}): {Arg1}", containerName, ex.Message);
                if (ex.RequestInformation.ExtendedErrorInformation != null)
                {
                    switch (ex.RequestInformation.ExtendedErrorInformation.ErrorCode)
                    {
                        case "AccountNotFound":
                            this.valContainerName.ErrorMessage = Localization.GetString(
                                "AccountNotFound.ErrorMessage",
                                this.LocalResourceFile);
                            break;
                        case "AuthenticationFailure":
                            this.valContainerName.ErrorMessage = Localization.GetString(
                                "AuthenticationFailure.ErrorMessage", this.LocalResourceFile);
                            break;
                        case "AccessDenied":
                            this.valContainerName.ErrorMessage = Localization.GetString(
                                "AccessDenied.ErrorMessage",
                                this.LocalResourceFile);
                            break;
                        case "ContainerAlreadyExists":
                            return true;
                        default:
                            Logger.Error(ex);
                            this.valContainerName.ErrorMessage = Localization.GetString(
                                "NewContainer.ErrorMessage",
                                this.LocalResourceFile);
                            break;
                    }
                }
                else
                {
                    this.valContainerName.ErrorMessage = ex.RequestInformation.HttpStatusMessage ?? ex.Message;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                this.valContainerName.ErrorMessage = Localization.GetString("NewContainer.ErrorMessage", this.LocalResourceFile);
            }

            this.valContainerName.IsValid = false;
			Logger.LogWarning("<<< Exiting CreateContainer(containerName={ContainerName})", containerName);
            return false;
        }

        private string GetSynchBatchSize()
        {
			Logger.LogError(">>> Entering GetSynchBatchSize()");
            Logger.LogWarning("<<< Exiting GetSynchBatchSize()");
            return this.tbSyncBatchSize.Text;
        }

        private void LoadContainers()
        {
			Logger.LogError(">>> Entering LoadContainers()");
            var accountName = this.tbAccountName.Text.Trim();
            var accountKey = this.tbAccountKey.Text.Trim();
			Logger.LogError("LoadContainers(): accountKey → {AccountKey}", accountKey);
			Logger.LogError("LoadContainers(): accountName → {AccountName}", accountName);
            var useHttps = this.chkUseHttps.Checked;

            StorageCredentials sc;

            try
            {
                sc = new StorageCredentials(accountName, accountKey);
				Logger.LogError("LoadContainers(): sc → {Sc}", sc);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                this.valContainerName.ErrorMessage = Localization.GetString("AuthenticationFailure.ErrorMessage", this.LocalResourceFile);
                this.valContainerName.IsValid = false;

                return;
            }

            var csa = new CloudStorageAccount(sc, useHttps);
			Logger.LogError("LoadContainers(): csa → {Csa}", csa);
            var blobClient = csa.CreateCloudBlobClient();
			Logger.LogError("LoadContainers(): blobClient → {BlobClient}", blobClient);

            try
            {
                foreach (var container in blobClient.ListContainers())
                {
                    this.ddlContainers.Items.Add(container.Name);
                }
            }
            catch (StorageException ex)
            {
				Logger.LogError("Exception in LoadContainers(): {Arg0}", ex.Message);
                if (ex.RequestInformation.ExtendedErrorInformation != null)
                {
                    switch (ex.RequestInformation.ExtendedErrorInformation.ErrorCode)
                    {
                        case "AuthenticationFailure":
                            this.valContainerName.ErrorMessage = Localization.GetString("AuthenticationFailure.ErrorMessage", this.LocalResourceFile);
                            break;
                        default:
                            Logger.Error(ex);
                            this.valContainerName.ErrorMessage = Localization.GetString("ListContainers.ErrorMessage", this.LocalResourceFile);
                            break;
                    }
                }
                else
                {
                    this.valContainerName.ErrorMessage = ex.RequestInformation.HttpStatusMessage ?? ex.Message;
                }

                this.valContainerName.IsValid = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                this.valContainerName.ErrorMessage = Localization.GetString("ListContainers.ErrorMessage", this.LocalResourceFile);
                this.valContainerName.IsValid = false;
            }
			Logger.LogWarning("<<< Exiting LoadContainers()");
        }
    }
}
