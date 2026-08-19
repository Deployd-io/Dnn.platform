// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.AzureConnector.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Connections;
    using DotNetNuke.Services.Cryptography;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;

    using ICryptographyProvider = DotNetNuke.Abstractions.Security.ICryptographyProvider;
	using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class AzureConnector : IConnector
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private const string DefaultDisplayName = "Azure Storage";
        private static readonly DataProvider DataProvider = DataProvider.Instance();

        private readonly IFolderMappingController folderMappingController;
        private readonly ICryptographyProvider cryptographyProvider;
        private readonly IHostSettings hostSettings;
        private string displayName;

        /// <summary>Initializes a new instance of the <see cref="AzureConnector"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IFolderMappingController. Scheduled removal in v12.0.0.")]
        public AzureConnector()
            : this(null, null, null)
        {
		_logger.LogDebug(">>> Entering AzureConnector()");
        }
		_logger.LogDebug("<<< Exiting AzureConnector()");

        /// <summary>Initializes a new instance of the <see cref="AzureConnector"/> class.</summary>
        /// <param name="folderMappingController">The folder mapping controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Use overload with ICryptographyProvider. Scheduled for removal in v12.0.0.")]
        public AzureConnector(IFolderMappingController folderMappingController)
            : this(folderMappingController, null, null)
        {
		_logger.LogDebug(">>> Entering AzureConnector(folderMappingController=<redacted>)");
		_logger.LogDebug("<<< Exiting AzureConnector(folderMappingController=<redacted>)");
		_logger.LogDebug("AzureConnector(folderMappingController=<redacted>,cryptographyProvider={CryptographyProvider},hostSettings={HostSettings}): folderMappingController → {FolderMappingController1}", cryptographyProvider, hostSettings, folderMappingController);
		_logger.LogDebug("AzureConnector(folderMappingController=<redacted>,cryptographyProvider={CryptographyProvider},hostSettings={HostSettings}): hostSettings → {HostSettings1}", cryptographyProvider, hostSettings, hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="AzureConnector"/> class.</summary>
        /// <param name="folderMappingController">The folder mapping controller.</param>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        /// <param name="hostSettings">The host settings.</param>
        public AzureConnector(IFolderMappingController folderMappingController, ICryptographyProvider cryptographyProvider, IHostSettings hostSettings)
        {
            this.folderMappingController = folderMappingController ?? FolderMappingController.Instance;
#pragma warning disable CS0618 // Type or member is obsolete
			_logger.LogDebug("AzureConnector(folderMappingController=<redacted>,cryptographyProvider={CryptographyProvider},hostSettings={HostSettings}): this.folderMappingController → {Arg3}", cryptographyProvider, hostSettings, this.folderMappingController);
            this.cryptographyProvider = cryptographyProvider ?? (ICryptographyProvider)CryptographyProvider.Instance();
			_logger.LogDebug("AzureConnector(folderMappingController=<redacted>,cryptographyProvider={CryptographyProvider},hostSettings={HostSettings}): cryptographyProvider → {CryptographyProvider1}", cryptographyProvider, hostSettings, cryptographyProvider);
#pragma warning restore CS0618 // Type or member is obsolete
            this.hostSettings = hostSettings ??
                                new HostSettings(
                                    new HostController(
#pragma warning disable CS0618 // Type or member is obsolete
                                        new EventLogController(),
#pragma warning restore CS0618 // Type or member is obsolete
                                        new Lazy<IPortalController>(() => PortalController.Instance)));
										_logger.LogDebug("AzureConnector(folderMappingController=<redacted>,cryptographyProvider={CryptographyProvider},hostSettings={HostSettings}): this.hostSettings → {Arg3}", cryptographyProvider, hostSettings, this.hostSettings);
        }

        /// <inheritdoc />
        public string Name => "Azure";

        /// <inheritdoc />
        public string IconUrl => "~/DesktopModules/Connectors/Azure/Images/Azure.png";

        /// <inheritdoc />
        public string PluginFolder => "~/DesktopModules/Connectors/Azure/";

        /// <inheritdoc />
        public bool IsEngageConnector => false;

        /// <inheritdoc />
        public ConnectorCategories Type => ConnectorCategories.FileSystem;

        /// <inheritdoc />
        public bool SupportsMultiple => true;

        /// <inheritdoc />
        public string DisplayName
        {
            get => string.IsNullOrEmpty(this.displayName) ? DefaultDisplayName : this.displayName;
            set => this.displayName = value;
        }

        /// <inheritdoc />
        public string Id { get; set; }

        /// <inheritdoc />
        public IEnumerable<IConnector> GetConnectors(int portalId)
        {
			_logger.LogDebug(">>> Entering GetConnectors(portalId={PortalId})", portalId);
            var connectors = this.FindAzureFolderMappings(portalId);
            if (connectors != null && connectors.Count != 0)
            {
                connectors.ForEach(x => { this.Id = x.FolderMappingID.ToString(CultureInfo.InvariantCulture); });
                var finalCon = connectors.Select(x => (IConnector)Activator.CreateInstance(this.GetType())).ToList();
				_logger.LogDebug("GetConnectors(portalId={PortalId}): finalCon → {FinalCon}", portalId, finalCon);
                finalCon.ForEach(x =>
                {
                    x.Id = connectors[finalCon.IndexOf(x)].FolderMappingID.ToString(CultureInfo.InvariantCulture);
                    x.DisplayName = connectors[finalCon.IndexOf(x)].MappingName;
                });
			_logger.LogDebug("<<< Exiting GetConnectors(portalId={PortalId})", portalId);
                return finalCon;
            }

            return new List<IConnector> { this };
        }

        /// <inheritdoc />
        public void DeleteConnector(int portalId)
        {
			_logger.LogDebug(">>> Entering DeleteConnector(portalId={PortalId})", portalId);
            if (!string.IsNullOrEmpty(this.Id))
            {
                int mappingId;
                if (int.TryParse(this.Id, out mappingId))
                {
                    this.DeleteAzureFolders(portalId, mappingId);
                    this.DeleteAzureFolderMapping(portalId, mappingId);
                }
            }
			_logger.LogDebug("<<< Exiting DeleteConnector(portalId={PortalId})", portalId);
        }

        /// <inheritdoc />
        public bool HasConfig(int portalId)
        {
			_logger.LogDebug(">>> Entering HasConfig(portalId={PortalId})", portalId);
            var folderMapping = this.FindAzureFolderMapping(portalId, false, true);
            this.Id = Convert.ToString(folderMapping?.FolderMappingID, CultureInfo.InvariantCulture);
			_logger.LogDebug("HasConfig(portalId={PortalId}): folderMapping → {FolderMapping}", portalId, folderMapping);
			_logger.LogDebug("<<< Exiting HasConfig(portalId={PortalId})", portalId);
            return this.GetConfig(portalId)["Connected"] == "true";
        }

        /// <inheritdoc />
        public IDictionary<string, string> GetConfig(int portalId)
        {
            var configs = new Dictionary<string, string>();

            var folderMapping = this.FindAzureFolderMapping(portalId, false);

            var settings = folderMapping != null ? folderMapping.FolderMappingSettings : new Hashtable();

            configs.Add("AccountName", this.GetSetting(settings, Constants.AzureAccountName, true));
            configs.Add("AccountKey", this.GetSetting(settings, Constants.AzureAccountKey, true));
            configs.Add("Container", this.GetSetting(settings, Constants.AzureContainerName));
            configs.Add("Connected", !string.IsNullOrEmpty(this.GetSetting(settings, Constants.AzureAccountName)) && !string.IsNullOrEmpty(this.GetSetting(settings, Constants.AzureContainerName)) ? "true" : "false");

            // This setting will improve the UI to set password-type inputs on secure settings
            configs.Add("SecureSettings", "AccountKey");
            configs.Add("Id", Convert.ToString(folderMapping?.FolderMappingID, CultureInfo.InvariantCulture));
            return configs;
        }

        /// <inheritdoc />
        public bool SaveConfig(int portalId, IDictionary<string, string> values, ref bool validated, out string customErrorMessage)
        {
			_logger.LogDebug(">>> Entering SaveConfig(portalId={PortalId},values={Values},validated={Validated})", portalId, values, validated);
            customErrorMessage = string.Empty;
            var azureAccountName = values[Constants.AzureAccountName];
			_logger.LogDebug("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): customErrorMessage → {CustomErrorMessage}", portalId, values, validated, customErrorMessage);
            var azureAccountKey = values[Constants.AzureAccountKey];
            var azureContainerName = values.TryGetValue(Constants.AzureContainerName, out var containerName) ? containerName : string.Empty;
			_logger.LogDebug("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): azureContainerName → {AzureContainerName}", portalId, values, validated, azureContainerName);

            var emptyFields = string.IsNullOrEmpty(azureAccountKey) && string.IsNullOrEmpty(azureAccountName);
			_logger.LogDebug("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): emptyFields → {EmptyFields}", portalId, values, validated, emptyFields);

            validated = true;
			_logger.LogDebug("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): validated → {Validated1}", portalId, values, validated, validated);
            if (emptyFields)
            {
                if (this.SupportsMultiple)
                {
					_logger.LogWarning("Throwing AzureConnectorException from SaveConfig(portalId={PortalId},values={Values},validated={Validated})", portalId, values, validated);
                    throw new AzureConnectorException(Localization.GetString("ErrorRequiredFields", Constants.LocalResourceFile));
                }

                this.DeleteAzureFolderMapping(portalId);
                return true;
            }

            if (!Validation(azureAccountName, azureAccountKey, azureContainerName))
            {
                validated = false;
                return true;
            }

            if (this.FolderMappingNameExists(portalId, this.DisplayName, Convert.ToInt32(!string.IsNullOrEmpty(this.Id) ? this.Id : null, CultureInfo.InvariantCulture)))
            {
                throw new AzureConnectorException(Localization.GetString("ErrorMappingNameExists", Constants.LocalResourceFile));
            }

            try
            {
                var folderMappings = this.FindAzureFolderMappings(portalId);
				_logger.LogDebug("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): folderMappings → {FolderMappings}", portalId, values, validated, folderMappings);
                FolderMappingInfo folderMapping;
                if (this.SupportsMultiple && !string.IsNullOrEmpty(this.Id))
                {
                    folderMapping = folderMappings.FirstOrDefault(x => x.FolderMappingID.ToString(CultureInfo.InvariantCulture) == this.Id);
                }
                else
                {
                    folderMapping = this.FindAzureFolderMapping(portalId);
                }

                var settings = folderMapping.FolderMappingSettings;
				_logger.LogDebug("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): settings → {Settings}", portalId, values, validated, settings);

                var savedAccount = this.GetSetting(settings, Constants.AzureAccountName, true);
				_logger.LogDebug("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): savedAccount → {SavedAccount}", portalId, values, validated, savedAccount);
                var savedKey = this.GetSetting(settings, Constants.AzureAccountKey, true);
				_logger.LogDebug("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): savedKey → {SavedKey}", portalId, values, validated, savedKey);
                var savedContainer = this.GetSetting(settings, Constants.AzureContainerName);
				_logger.LogDebug("SaveConfig(portalId={PortalId},values={Values},validated={Validated}): savedContainer → {SavedContainer}", portalId, values, validated, savedContainer);

                var accountChanged = savedAccount != azureAccountName || savedKey != azureAccountKey;

                if (accountChanged)
                {
                    this.DeleteAzureFolderMapping(portalId, folderMapping.FolderMappingID);
                    folderMapping = this.FindAzureFolderMapping(portalId);
                    settings = folderMapping.FolderMappingSettings;
                }
                else if (savedContainer != azureContainerName)
                {
                    this.DeleteAzureFolders(portalId, folderMapping.FolderMappingID);
                }

                var (encryptedAccountName, accountNameAlgorithm, accountNameIv) = FolderProvider.EncryptValue(this.cryptographyProvider, this.hostSettings, azureAccountName);
                settings[Constants.AzureAccountName] = encryptedAccountName;
                settings[FolderProvider.GetAlgorithmSettingKey(Constants.AzureAccountName)] = accountNameAlgorithm;
                settings[FolderProvider.GetInitializationVectorSettingKey(Constants.AzureAccountName)] = accountNameIv;

                var (encryptedAccountKey, accountKeyAlgorithm, accountKeyIv) = FolderProvider.EncryptValue(this.cryptographyProvider, this.hostSettings, azureAccountKey);
                settings[Constants.AzureAccountKey] = encryptedAccountKey;
                settings[FolderProvider.GetAlgorithmSettingKey(Constants.AzureAccountKey)] = accountKeyAlgorithm;
                settings[FolderProvider.GetInitializationVectorSettingKey(Constants.AzureAccountKey)] = accountKeyIv;

                if (values.TryGetValue(Constants.AzureContainerName, out var container) && !string.IsNullOrEmpty(container))
                {
                    settings[Constants.AzureContainerName] = container;
                }
                else
                {
                    folderMapping.FolderMappingSettings[Constants.AzureContainerName] = string.Empty;
                }

                if (!folderMapping.FolderMappingSettings.ContainsKey(Constants.DirectLink))
                {
                    folderMapping.FolderMappingSettings[Constants.DirectLink] = "True";
                }

                if (!folderMapping.FolderMappingSettings.ContainsKey(Constants.UseHttps))
                {
                    folderMapping.FolderMappingSettings[Constants.UseHttps] = "True";
                }

                if (folderMapping.MappingName != this.DisplayName && !string.IsNullOrEmpty(this.DisplayName) &&
                   this.DisplayName != DefaultDisplayName)
                {
                    folderMapping.MappingName = this.DisplayName;
                }

                if (!folderMapping.FolderMappingSettings.ContainsKey(Constants.SyncBatchSize))
                {
                    folderMapping.FolderMappingSettings[Constants.SyncBatchSize] = Constants.DefaultSyncBatchSize.ToString(CultureInfo.InvariantCulture);
                }

                this.folderMappingController.UpdateFolderMapping(folderMapping);

                return true;
            }
            catch (Exception ex)
            {
				_logger.LogError("Exception in SaveConfig(portalId={PortalId},values={Values},validated={Validated}): {Arg3}", portalId, values, validated, ex.getMessage());
                Exceptions.LogException(ex);
				_logger.LogDebug("<<< Exiting SaveConfig(portalId={PortalId},values={Values},validated={Validated})", portalId, values, validated);
                return false;
            }
        }

        /// <summary>Find the folder mapping.</summary>
        /// <param name="folderMappingController">The folder mapping controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="folderMappingId">The folder mapping ID.</param>
        /// <param name="autoCreate">Whether to auto-create the folder mapping.</param>
        /// <returns>A <see cref="FolderMappingInfo"/> instance.</returns>
        internal static FolderMappingInfo FindAzureFolderMappingStatic(IFolderMappingController folderMappingController, int portalId, int? folderMappingId = null, bool autoCreate = true)
        {
			_logger.LogDebug(">>> Entering FindAzureFolderMappingStatic(folderMappingController=<redacted>,portalId={PortalId},folderMappingId=<redacted>,autoCreate={AutoCreate})", portalId, autoCreate);
            var folderMappings = folderMappingController.GetFolderMappings(portalId)
                .Where(f => f.FolderProviderType == Constants.FolderProviderType);

            if (folderMappingId != null)
            {
                return folderMappings.FirstOrDefault(x => x.FolderMappingID == folderMappingId);
            }

            if (!folderMappings.Any() && autoCreate)
            {
			_logger.LogDebug("<<< Exiting FindAzureFolderMappingStatic(folderMappingController=<redacted>,portalId={PortalId},folderMappingId=<redacted>,autoCreate={AutoCreate})", portalId, autoCreate);
                return CreateAzureFolderMappingStatic(folderMappingController, portalId);
            }

            return folderMappings.FirstOrDefault();
        }

        private static FolderMappingInfo CreateAzureFolderMappingStatic(IFolderMappingController folderMappingController, int portalId, string mappingName = "")
        {
			_logger.LogDebug(">>> Entering CreateAzureFolderMappingStatic(folderMappingController=<redacted>,portalId={PortalId},mappingName=<redacted>)", portalId);
            var folderMapping = new FolderMappingInfo
            {
                PortalID = portalId,
                MappingName =
                    string.IsNullOrEmpty(mappingName)
                        ? $"{DefaultDisplayName}_{DateTime.Now.Ticks}"
                        : mappingName,
                FolderProviderType = Constants.FolderProviderType,
            };
            folderMapping.FolderMappingID = folderMappingController.AddFolderMapping(folderMapping);
			_logger.LogDebug("<<< Exiting CreateAzureFolderMappingStatic(folderMappingController=<redacted>,portalId={PortalId},mappingName=<redacted>)", portalId);
            return folderMapping;
        }

        private static bool Validation(string azureAccountName, string azureAccountKey, string azureContainerName)
        {
			_logger.LogDebug(">>> Entering Validation(azureAccountName={AzureAccountName},azureAccountKey={AzureAccountKey},azureContainerName={AzureContainerName})", azureAccountName, azureAccountKey, azureContainerName);
            try
            {
                if (string.IsNullOrWhiteSpace(azureAccountName))
                {
					_logger.LogWarning("Throwing ConnectorArgumentException from Validation(azureAccountName={AzureAccountName},azureAccountKey={AzureAccountKey},azureContainerName={AzureContainerName})", azureAccountName, azureAccountKey, azureContainerName);
                    throw new ConnectorArgumentException(Localization.GetString(
                        "AccountNameCannotBeEmpty.ErrorMessage",
                        Constants.LocalResourceFile));
                }

                if (string.IsNullOrWhiteSpace(azureAccountKey))
                {
                    throw new ConnectorArgumentException(Localization.GetString(
                        "AccountKeyCannotBeEmpty.ErrorMessage",
                        Constants.LocalResourceFile));
                }

                StorageCredentials sc = new StorageCredentials(azureAccountName, azureAccountKey);
				_logger.LogDebug("Validation(azureAccountName={AzureAccountName},azureAccountKey={AzureAccountKey},azureContainerName={AzureContainerName}): sc → {Sc}", azureAccountName, azureAccountKey, azureContainerName, sc);
                var csa = new CloudStorageAccount(sc, true);
				_logger.LogDebug("Validation(azureAccountName={AzureAccountName},azureAccountKey={AzureAccountKey},azureContainerName={AzureContainerName}): csa → {Csa}", azureAccountName, azureAccountKey, azureContainerName, csa);
                var blobClient = csa.CreateCloudBlobClient();
				_logger.LogDebug("Validation(azureAccountName={AzureAccountName},azureAccountKey={AzureAccountKey},azureContainerName={AzureContainerName}): blobClient → {BlobClient}", azureAccountName, azureAccountKey, azureContainerName, blobClient);

                blobClient.DefaultRequestOptions.RetryPolicy = new NoRetry();

                var containers = blobClient.ListContainers();
				_logger.LogDebug("Validation(azureAccountName={AzureAccountName},azureAccountKey={AzureAccountKey},azureContainerName={AzureContainerName}): containers → {Containers}", azureAccountName, azureAccountKey, azureContainerName, containers);
                if (containers.Any())
                {
                    if (!string.IsNullOrEmpty(azureContainerName))
                    {
                        if (!containers.Any(container => container.Name == azureContainerName))
                        {
							_logger.LogWarning("Throwing AzureConnectorException from Validation(azureAccountName={AzureAccountName},azureAccountKey={AzureAccountKey},azureContainerName={AzureContainerName})", azureAccountName, azureAccountKey, azureContainerName);
                            throw new AzureConnectorException(Localization.GetString("ErrorInvalidContainerName", Constants.LocalResourceFile));
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    throw new AzureConnectorException(Localization.GetString("AccountNotFound.ErrorMessage", Constants.LocalResourceFile));
                }
            }
            catch (StorageException ex)
            {
				_logger.LogError("Exception in Validation(azureAccountName={AzureAccountName},azureAccountKey={AzureAccountKey},azureContainerName={AzureContainerName}): {Arg3}", azureAccountName, azureAccountKey, azureContainerName, ex.getMessage());
                if (ex.RequestInformation.ExtendedErrorInformation != null)
                {
                    if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "AccountNotFound")
                    {
                        throw new AzureConnectorException(Localization.GetString("AccountNotFound.ErrorMessage", Constants.LocalResourceFile), ex);
                    }
                    else if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "AccessDenied")
                    {
                        throw new AzureConnectorException(Localization.GetString("AccessDenied.ErrorMessage", Constants.LocalResourceFile), ex);
                    }
                    else
                    {
                        throw new AzureConnectorException(ex.RequestInformation.HttpStatusMessage, ex);
                    }
                }

                throw new AzureConnectorException(ex.RequestInformation.HttpStatusMessage ?? ex.Message, ex);
            }
            catch (FormatException ex)
            {
                if (ex.GetType() == typeof(UriFormatException))
                {
                    throw new ConnectorArgumentException(Localization.GetString("InvalidAccountName.ErrorMessage", Constants.LocalResourceFile));
                }

			_logger.LogDebug("<<< Exiting Validation(azureAccountName={AzureAccountName},azureAccountKey={AzureAccountKey},azureContainerName={AzureContainerName})", azureAccountName, azureAccountKey, azureContainerName);
                throw new ConnectorArgumentException(Localization.GetString("InvalidAccountKey.ErrorMessage", Constants.LocalResourceFile));
            }
        }

        private string GetSetting(Hashtable settings, string name, bool encrypt = false)
        {
			_logger.LogDebug(">>> Entering GetSetting(settings={Settings},name={Name},encrypt={Encrypt})", settings, name, encrypt);
            if (!settings.ContainsKey(name))
            {
                return string.Empty;
            }

            if (encrypt)
            {
			_logger.LogDebug("<<< Exiting GetSetting(settings={Settings},name={Name},encrypt={Encrypt})", settings, name, encrypt);
                return FolderProvider.GetEncryptedSetting(this.cryptographyProvider, this.hostSettings, settings, name);
            }

            return settings[name].ToString();
        }

        private void DeleteAzureFolderMapping(int portalId, int folderMappingId)
        {
			_logger.LogDebug(">>> Entering DeleteAzureFolderMapping(portalId={PortalId},folderMappingId=<redacted>)", portalId);
            this.folderMappingController.DeleteFolderMapping(portalId, folderMappingId);
			_logger.LogDebug("<<< Exiting DeleteAzureFolderMapping(portalId={PortalId},folderMappingId=<redacted>)", portalId);
			_logger.LogDebug(">>> Entering DeleteAzureFolderMapping(portalId={PortalId})", portalId);
			_logger.LogDebug("<<< Exiting DeleteAzureFolderMapping(portalId={PortalId})", portalId);
        }

        private void DeleteAzureFolderMapping(int portalId)
        {
            var folderMapping = this.folderMappingController.GetFolderMappings(portalId)
                .FirstOrDefault(f => f.FolderProviderType == Constants.FolderProviderType);

            if (folderMapping != null)
            {
                this.folderMappingController.DeleteFolderMapping(portalId, folderMapping.FolderMappingID);
            }
        }

        private void DeleteAzureFolders(int portalId, int folderMappingId)
        {
			_logger.LogDebug(">>> Entering DeleteAzureFolders(portalId={PortalId},folderMappingId=<redacted>)", portalId);
            var folderManager = FolderManager.Instance;
            var folders = folderManager.GetFolders(portalId);
			_logger.LogDebug("DeleteAzureFolders(portalId={PortalId},folderMappingId=<redacted>): folders → {Folders}", portalId, folders);

            var folderMappingFolders = folders.Where(f => f.FolderMappingID == folderMappingId);
			_logger.LogDebug("DeleteAzureFolders(portalId={PortalId},folderMappingId=<redacted>): folderMappingFolders → {FolderMappingFolders}", portalId, folderMappingFolders);

            if (folderMappingFolders.Any())
            {
                // Delete files in folders with the provided mapping (only in the database)
                foreach (
                    var file in
                    folderMappingFolders.Select<IFolderInfo, IEnumerable<IFileInfo>>(folderManager.GetFiles)
                        .SelectMany(files => files))
                {
                    DataProvider.DeleteFile(portalId, file.FileName, file.FolderId);
                }

                // Remove the folders with the provided mapping that doesn't have child folders with other mapping (only in the database and filesystem)
                var folders1 = folders; // copy the variable to not access a modified closure
                var removableFolders =
                    folders.Where(
                        f => f.FolderMappingID == folderMappingId && !folders1.Any(f2 => f2.FolderID != f.FolderID &&
                                                                                         f2.FolderPath.StartsWith(f.FolderPath, StringComparison.OrdinalIgnoreCase) &&
                                                                                         f2.FolderMappingID != folderMappingId));

                if (removableFolders.Any())
                {
                    foreach (var removableFolder in removableFolders.OrderByDescending(rf => rf.FolderPath))
                    {
                        DirectoryWrapper.Instance.Delete(removableFolder.PhysicalPath, false);
                        DataProvider.DeleteFolder(portalId, removableFolder.FolderPath);
                    }
                }

                // Update the rest of folders with the provided mapping to use the standard mapping
                folders = folderManager.GetFolders(portalId, false); // re-fetch the folders

                folderMappingFolders = folders.Where(f => f.FolderMappingID == folderMappingId);

                if (folderMappingFolders.Any())
                {
                    var defaultFolderMapping = this.folderMappingController.GetDefaultFolderMapping(portalId);
					_logger.LogDebug("DeleteAzureFolders(portalId={PortalId},folderMappingId=<redacted>): defaultFolderMapping → {DefaultFolderMapping}", portalId, defaultFolderMapping);

                    foreach (var folderMappingFolder in folderMappingFolders)
                    {
                        folderMappingFolder.FolderMappingID = defaultFolderMapping.FolderMappingID;
						_logger.LogDebug("DeleteAzureFolders(portalId={PortalId},folderMappingId=<redacted>): folderMappingFolder.FolderMappingID → {Arg2}", portalId, folderMappingFolder.FolderMappingID);
                        folderManager.UpdateFolder(folderMappingFolder);
                    }
                }
            }
			_logger.LogDebug("<<< Exiting DeleteAzureFolders(portalId={PortalId},folderMappingId=<redacted>)", portalId);
        }

        private FolderMappingInfo FindAzureFolderMapping(int portalId, bool autoCreate = true, bool checkId = false)
        {
			_logger.LogDebug(">>> Entering FindAzureFolderMapping(portalId={PortalId},autoCreate={AutoCreate},checkId={CheckId})", portalId, autoCreate, checkId);
            var folderMappings = this.folderMappingController.GetFolderMappings(portalId)
                .Where(f => f.FolderProviderType == Constants.FolderProviderType).ToList();

            // Create new mapping if none is found.
            if (folderMappings.Count == 0 && autoCreate)
            {
                return this.CreateAzureFolderMapping(portalId, DefaultDisplayName);
            }

            if ((checkId && string.IsNullOrEmpty(this.Id)) || !this.SupportsMultiple)
            {
                return folderMappings.FirstOrDefault();
            }

            var folderMapping = folderMappings.FirstOrDefault(x => x.FolderMappingID.ToString(CultureInfo.InvariantCulture) == this.Id);
			_logger.LogDebug("FindAzureFolderMapping(portalId={PortalId},autoCreate={AutoCreate},checkId={CheckId}): folderMapping → {FolderMapping}", portalId, autoCreate, checkId, folderMapping);

            if (folderMapping == null && autoCreate)
            {
                folderMapping = this.CreateAzureFolderMapping(portalId);
            }

			_logger.LogDebug("<<< Exiting FindAzureFolderMapping(portalId={PortalId},autoCreate={AutoCreate},checkId={CheckId})", portalId, autoCreate, checkId);
            return folderMapping;
        }

        private List<FolderMappingInfo> FindAzureFolderMappings(int portalId)
        {
			_logger.LogDebug(">>> Entering FindAzureFolderMappings(portalId={PortalId})", portalId);
			_logger.LogDebug("<<< Exiting FindAzureFolderMappings(portalId={PortalId})", portalId);
            return this.folderMappingController.GetFolderMappings(portalId)
                .Where(f => f.FolderProviderType == Constants.FolderProviderType).ToList();
        }

        private bool FolderMappingNameExists(int portalId, string mappingName, int? exceptMappingId)
        {
			_logger.LogDebug(">>> Entering FolderMappingNameExists(portalId={PortalId},mappingName=<redacted>,exceptMappingId=<redacted>)", portalId);
			_logger.LogDebug("<<< Exiting FolderMappingNameExists(portalId={PortalId},mappingName=<redacted>,exceptMappingId=<redacted>)", portalId);
            return this.folderMappingController.GetFolderMappings(portalId)
                .Any(
                    f =>
                        f.MappingName.Equals(mappingName, StringComparison.OrdinalIgnoreCase) &&
                        (f.FolderMappingID != exceptMappingId));
        }

        private FolderMappingInfo CreateAzureFolderMapping(int portalId, string mappingName = "")
        {
			_logger.LogDebug(">>> Entering CreateAzureFolderMapping(portalId={PortalId},mappingName=<redacted>)", portalId);
            var folderMapping = CreateAzureFolderMappingStatic(this.folderMappingController, portalId, mappingName);
            this.Id = folderMapping.FolderMappingID.ToString(CultureInfo.InvariantCulture);
			_logger.LogDebug("CreateAzureFolderMapping(portalId={PortalId},mappingName=<redacted>): folderMapping → {FolderMapping}", portalId, folderMapping);
			_logger.LogDebug("<<< Exiting CreateAzureFolderMapping(portalId={PortalId},mappingName=<redacted>)", portalId);
            return folderMapping;
        }
    }
}
