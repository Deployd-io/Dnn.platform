// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Providers.FolderProviders.AzureFolderProvider
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Web.Caching;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Providers.FolderProviders.Components;
    using DotNetNuke.Services.FileSystem;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>Windows Azure Storage Folder Provider.</summary>
    /// <inheritdoc />
    public class AzureFolderProvider : BaseRemoteStorageProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AzureFolderProvider));
        internal const string ProviderName = "AzureFolderProvider";
        internal const string ProviderType = "folder";

        private readonly ProviderConfiguration providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
        private readonly ICryptographyProvider cryptographyProvider;
        private readonly string cacheControl = string.Empty;

        /// <summary>Initializes a new instance of the <see cref="AzureFolderProvider"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public AzureFolderProvider()
            : this(null, null)
        {
		Logger.LogError(">>> Entering AzureFolderProvider()");
        }

        /// <summary>Initializes a new instance of the <see cref="AzureFolderProvider"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        public AzureFolderProvider(IHostSettings hostSettings, ICryptographyProvider cryptographyProvider)
            : base(hostSettings)
        {
            this.cryptographyProvider = cryptographyProvider ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<ICryptographyProvider>();

			Logger.LogDebug("AzureFolderProvider(hostSettings={HostSettings},cryptographyProvider={CryptographyProvider}): cryptographyProvider → {CryptographyProvider1}", hostSettings, cryptographyProvider, cryptographyProvider);
			Logger.LogDebug("AzureFolderProvider(hostSettings={HostSettings},cryptographyProvider={CryptographyProvider}): ServicePointManager.UseNagleAlgorithm → {Arg2}", hostSettings, cryptographyProvider, ServicePointManager.UseNagleAlgorithm);
			Logger.LogDebug("AzureFolderProvider(hostSettings={HostSettings},cryptographyProvider={CryptographyProvider}): ServicePointManager.Expect100Continue → {Arg2}", hostSettings, cryptographyProvider, ServicePointManager.Expect100Continue);
            ServicePointManager.DefaultConnectionLimit = 100;
			Logger.LogDebug("AzureFolderProvider(hostSettings={HostSettings},cryptographyProvider={CryptographyProvider}): ServicePointManager.DefaultConnectionLimit → {Arg2}", hostSettings, cryptographyProvider, ServicePointManager.DefaultConnectionLimit);
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.Expect100Continue = false;

			Logger.LogWarning("<<< Exiting AzureFolderProvider()");
            // Read the configuration specific information for this provider
            var objProvider = (Provider)this.providerConfiguration.Providers[ProviderName];
			Logger.LogError("AzureFolderProvider(hostSettings={HostSettings},cryptographyProvider={CryptographyProvider}): objProvider → {ObjProvider}", hostSettings, cryptographyProvider, objProvider);

            if (!string.IsNullOrEmpty(objProvider.Attributes["cacheControl"]))
            {
                this.cacheControl = objProvider.Attributes["cacheControl"].ToLowerInvariant();
            }
        }

        /// <inheritdoc />
        protected override string FileNotFoundMessage => "Azure File Not Found";

        /// <inheritdoc />
        protected override string ObjectCacheKey => "Azure_Object_{0}_{1}";

        /// <inheritdoc />
        protected override string ListObjectsCacheKey => "Azure_ListObjects_{0}";

        /// <remarks>
        /// Azure Storage doesn't support folders, so we create a file in order for the folder to not be deleted during future synchronizations.
        /// The file has an extension not allowed by host. This way the file won't be added during synchronizations.
        /// </remarks>
        /// <inheritdoc />
        public override void AddFolder(string folderPath, FolderMappingInfo folderMapping, string mappedPath)
        {
			Logger.LogError(">>> Entering AddFolder(folderPath={FolderPath},folderMapping=<redacted>,mappedPath={MappedPath})", folderPath, mappedPath);
            Requires.NotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

            this.UpdateFileInternal(new MemoryStream(), folderMapping, mappedPath + Constants.PlaceHolderFileName);
			Logger.LogWarning("<<< Exiting AddFolder(folderPath={FolderPath},folderMapping=<redacted>,mappedPath={MappedPath})", folderPath, mappedPath);
        }

        /// <inheritdoc />
        public override string GetFileUrl(IFileInfo file)
        {
			Logger.LogError(">>> Entering GetFileUrl(file={File})", file);
            Requires.NotNull("file", file);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
			Logger.LogError("GetFileUrl(file={File}): folderMapping → {FolderMapping}", file, folderMapping);
            var directLink = string.IsNullOrEmpty(GetSetting(folderMapping, Constants.DirectLink)) || GetSetting(folderMapping, Constants.DirectLink).ToLowerInvariant() == "true";
			Logger.LogError("GetFileUrl(file={File}): directLink → {DirectLink}", file, directLink);

            if (directLink)
            {
                var folder = FolderManager.Instance.GetFolder(file.FolderId);
				Logger.LogError("GetFileUrl(file={File}): folder → {Folder}", file, folder);
                var uri = folder.MappedPath + file.FileName;

                var container = this.GetContainer(folderMapping);
				Logger.LogError("GetFileUrl(file={File}): container → {Container}", file, container);
                var blob = container.GetBlobReference(uri);
				Logger.LogError("GetFileUrl(file={File}): blob → {Blob}", file, blob);
                var absuri = blob.Uri.AbsoluteUri;
                var customDomain = GetEncryptedSetting(this.cryptographyProvider, this.HostSettings, folderMapping.FolderMappingSettings, Constants.CustomDomain);
				Logger.LogError("GetFileUrl(file={File}): customDomain → {CustomDomain}", file, customDomain);

                if (!string.IsNullOrEmpty(customDomain))
                {
                    var customUri = new UriBuilder(customDomain).Uri;
                    absuri =
                        new UriBuilder(blob.Uri.AbsoluteUri) { Host = customUri.Host, Scheme = customUri.Scheme, Port = customUri.Port }
                            .Uri.AbsoluteUri;
                }

                const string groupPolicyIdentifier = "DNNFileManagerPolicy";

                var permissions = container.GetPermissions();

                SharedAccessBlobPolicy policy;

                permissions.SharedAccessPolicies.TryGetValue(groupPolicyIdentifier, out policy);

                if (policy == null)
                {
                    policy = new SharedAccessBlobPolicy { Permissions = SharedAccessBlobPermissions.Read, SharedAccessExpiryTime = DateTime.UtcNow.AddYears(100) };

                    permissions.SharedAccessPolicies.Add(groupPolicyIdentifier, policy);
                }
                else
                {
                    policy.Permissions = SharedAccessBlobPermissions.Read;
                    policy.SharedAccessExpiryTime = DateTime.UtcNow.AddYears(100);
                }

                /*
                 * Workaround for CONTENT-3662
                 * The Azure Client Storage api has issue when used with Italian Thread.Culture or eventually other cultures
                 * (see this article for further information https://connect.microsoft.com/VisualStudio/feedback/details/760974/windows-azure-sdk-cloudblobcontainer-setpermissions-permissions-as-microsoft-windowsazure-storageclient-blobcontainerpermissions-error).
                 * This code changes the thread culture to en-US
                 */
                var currentCulture = Thread.CurrentThread.CurrentCulture;
                if (currentCulture.Name != "en-US")
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
                }

                container.SetPermissions(permissions);

                var signature = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy(), groupPolicyIdentifier);

                // Reset original Thread Culture
                if (currentCulture.Name != "en-US")
                {
                    Thread.CurrentThread.CurrentCulture = currentCulture;
                }

                return absuri + signature;
            }

            return FileLinkClickController.Instance.GetFileLinkClick(file);
        }

        /// <inheritdoc />
        public override string GetFolderProviderIconPath()
        {
			Logger.LogError(">>> Entering GetFolderProviderIconPath()");
            Logger.LogWarning("<<< Exiting GetFolderProviderIconPath()");
            return Globals.ResolveUrl("~/Providers/FolderProviders/AzureFolderProvider/images/FolderAzure_32x32_Standard.png");
        }

        public List<string> GetAllContainers(FolderMappingInfo folderMapping)
        {
			Logger.LogError(">>> Entering GetAllContainers(folderMapping=<redacted>)");
            List<string> containers = [];
            var accountName = GetEncryptedSetting(this.cryptographyProvider, this.HostSettings, folderMapping.FolderMappingSettings, Constants.AccountName);
			Logger.LogError("GetAllContainers(folderMapping=<redacted>): accountName → {AccountName}", accountName);
            var accountKey = GetEncryptedSetting(this.cryptographyProvider, this.HostSettings, folderMapping.FolderMappingSettings, Constants.AccountKey);
			Logger.LogError("GetAllContainers(folderMapping=<redacted>): accountKey → {AccountKey}", accountKey);
            var useHttps = GetBooleanSetting(folderMapping, Constants.UseHttps);
			Logger.LogError("GetAllContainers(folderMapping=<redacted>): useHttps → {UseHttps}", useHttps);

            var sc = new StorageCredentials(accountName, accountKey);
			Logger.LogError("GetAllContainers(folderMapping=<redacted>): sc → {Sc}", sc);
            var csa = new CloudStorageAccount(sc, useHttps);
			Logger.LogError("GetAllContainers(folderMapping=<redacted>): csa → {Csa}", csa);
            var blobClient = csa.CreateCloudBlobClient();
			Logger.LogError("GetAllContainers(folderMapping=<redacted>): blobClient → {BlobClient}", blobClient);
            blobClient.ListContainers().ToList().ForEach(x => containers.Add(x.Name));
			Logger.LogWarning("<<< Exiting GetAllContainers(folderMapping=<redacted>)");
            return containers;
        }

        /// <summary>Updates a file in Azure folder provider.</summary>
        /// <remarks>
        /// Azure is case sensitive.  If you update dnninternals.pdf with DNNINTERNALS.pdf, to DNN it's the same
        /// so it just re-uploads it, causing both files to exist in Azure. This azure specific method deletes the
        /// old existing duplicate that only differs in case as part of the update.
        /// </remarks>
        /// <inheritdoc />
        public override void UpdateFile(IFolderInfo folder, string fileName, Stream content)
        {
			Logger.LogError(">>> Entering UpdateFile(folder={Folder},fileName={FileName},content={Content})", folder, fileName, content);
            IFileInfo originalFile = FileManager.Instance.GetFile(folder, fileName);

			Logger.LogError("UpdateFile(folder={Folder},fileName={FileName},content={Content}): originalFile → {OriginalFile}", folder, fileName, content, originalFile);
            base.UpdateFile(folder, fileName, content);

            if (originalFile != null && originalFile.FileName != fileName)
            {
                FolderMappingInfo folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.FolderMappingID);
				Logger.LogError("UpdateFile(folder={Folder},fileName={FileName},content={Content}): folderMapping → {FolderMapping}", folder, fileName, content, folderMapping);
                this.DeleteFileInternal(folderMapping, folder.MappedPath + originalFile.FileName);
            }
			Logger.LogWarning("<<< Exiting UpdateFile(folder={Folder},fileName={FileName},content={Content})", folder, fileName, content);
        }

        /// <inheritdoc />
        protected override void CopyFileInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri)
        {
			Logger.LogError(">>> Entering CopyFileInternal(folderMapping=<redacted>,sourceUri={SourceUri},newUri={NewUri})", sourceUri, newUri);
            var container = this.GetContainer(folderMapping);

			Logger.LogError("CopyFileInternal(folderMapping=<redacted>,sourceUri={SourceUri},newUri={NewUri}): container → {Container}", sourceUri, newUri, container);
            var sourceBlob = container.GetBlobReference(sourceUri);
			Logger.LogError("CopyFileInternal(folderMapping=<redacted>,sourceUri={SourceUri},newUri={NewUri}): sourceBlob → {SourceBlob}", sourceUri, newUri, sourceBlob);
            var newBlob = container.GetBlobReference(newUri);
			Logger.LogError("CopyFileInternal(folderMapping=<redacted>,sourceUri={SourceUri},newUri={NewUri}): newBlob → {NewBlob}", sourceUri, newUri, newBlob);

            newBlob.StartCopy(sourceBlob.Uri);

            this.ClearCache(folderMapping.FolderMappingID);
			Logger.LogWarning("<<< Exiting CopyFileInternal(folderMapping=<redacted>,sourceUri={SourceUri},newUri={NewUri})", sourceUri, newUri);
        }

        /// <inheritdoc />
        protected override void DeleteFileInternal(FolderMappingInfo folderMapping, string uri)
        {
			Logger.LogError(">>> Entering DeleteFileInternal(folderMapping=<redacted>,uri={Uri})", uri);
            var container = this.GetContainer(folderMapping);
            var blob = container.GetBlobReference(uri);
			Logger.LogError("DeleteFileInternal(folderMapping=<redacted>,uri={Uri}): blob → {Blob}", uri, blob);
			Logger.LogError("DeleteFileInternal(folderMapping=<redacted>,uri={Uri}): container → {Container}", uri, container);

            blob.DeleteIfExists();

            this.ClearCache(folderMapping.FolderMappingID);
			Logger.LogWarning("<<< Exiting DeleteFileInternal(folderMapping=<redacted>,uri={Uri})", uri);
        }

        /// <inheritdoc />
        protected override void DeleteFolderInternal(FolderMappingInfo folderMapping, IFolderInfo folder)
        {
			Logger.LogError(">>> Entering DeleteFolderInternal(folderMapping=<redacted>,folder={Folder})", folder);
            this.DeleteFileInternal(folderMapping, folder.MappedPath + Constants.PlaceHolderFileName);
			Logger.LogWarning("<<< Exiting DeleteFolderInternal(folderMapping=<redacted>,folder={Folder})", folder);
        }

        /// <inheritdoc />
        protected override Stream GetFileStreamInternal(FolderMappingInfo folderMapping, string uri)
        {
			Logger.LogError(">>> Entering GetFileStreamInternal(folderMapping=<redacted>,uri={Uri})", uri);
            var container = this.GetContainer(folderMapping);
            var blob = container.GetBlockBlobReference(uri);
			Logger.LogError("GetFileStreamInternal(folderMapping=<redacted>,uri={Uri}): blob → {Blob}", uri, blob);
			Logger.LogError("GetFileStreamInternal(folderMapping=<redacted>,uri={Uri}): container → {Container}", uri, container);

            var memoryStream = new MemoryStream();
			Logger.LogError("GetFileStreamInternal(folderMapping=<redacted>,uri={Uri}): memoryStream → {MemoryStream}", uri, memoryStream);
            blob.DownloadToStream(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

			Logger.LogWarning("<<< Exiting GetFileStreamInternal(folderMapping=<redacted>,uri={Uri})", uri);
            return memoryStream;
        }

        /// <inheritdoc />
        protected override IList<IRemoteStorageItem> GetObjectList(FolderMappingInfo folderMapping)
        {
			Logger.LogError(">>> Entering GetObjectList(folderMapping=<redacted>)");
            var cacheKey = string.Format(this.ListObjectsCacheKey, folderMapping.FolderMappingID);

			Logger.LogError("GetObjectList(folderMapping=<redacted>): cacheKey → {CacheKey}", cacheKey);
            return CBO.GetCachedObject<IList<IRemoteStorageItem>>(
                this.HostSettings,
                new CacheItemArgs(
                    cacheKey,
                    this.ListObjectsCacheTimeout,
                    CacheItemPriority.Default,
                    folderMapping.FolderMappingID),
                _ =>
                {
                    var container = this.GetContainer(folderMapping);
                    var synchBatchSize = GetIntegerSetting(
                        folderMapping,
                        Constants.SyncBatchSize,
                        Constants.DefaultSyncBatchSize);

                    BlobContinuationToken continuationToken = null;

                    var list = new List<IRemoteStorageItem>();
                    do
                    {
                        // This overload allows control of the page size. You can return all remaining results by passing null for the maxResults parameter,
                        // or by calling a different overload.
                        var resultSegment = container.ListBlobsSegmented(
                            string.Empty,
                            true,
                            BlobListingDetails.All,
                            synchBatchSize,
                            continuationToken,
                            null,
                            null);
                        foreach (var blobItem in resultSegment.Results)
                        {
                            list.Add(new AzureRemoteStorageItem { Blob = new AzureBlob(blobItem as CloudBlob), });
                        }

                        // Get the continuation token.
                        continuationToken = resultSegment.ContinuationToken;
                    }
                    while (continuationToken != null);

				Logger.LogWarning("<<< Exiting GetObjectList(folderMapping=<redacted>)");
                    return list;
                });
        }

        /// <inheritdoc />
        protected override void MoveFileInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri)
        {
			Logger.LogError(">>> Entering MoveFileInternal(folderMapping=<redacted>,sourceUri={SourceUri},newUri={NewUri})", sourceUri, newUri);
            var container = this.GetContainer(folderMapping);

			Logger.LogError("MoveFileInternal(folderMapping=<redacted>,sourceUri={SourceUri},newUri={NewUri}): container → {Container}", sourceUri, newUri, container);
            var sourceBlob = container.GetBlobReference(sourceUri);
			Logger.LogError("MoveFileInternal(folderMapping=<redacted>,sourceUri={SourceUri},newUri={NewUri}): sourceBlob → {SourceBlob}", sourceUri, newUri, sourceBlob);
            var newBlob = container.GetBlobReference(newUri);
			Logger.LogError("MoveFileInternal(folderMapping=<redacted>,sourceUri={SourceUri},newUri={NewUri}): newBlob → {NewBlob}", sourceUri, newUri, newBlob);

            newBlob.StartCopy(sourceBlob.Uri);
            sourceBlob.Delete();

            this.ClearCache(folderMapping.FolderMappingID);
			Logger.LogWarning("<<< Exiting MoveFileInternal(folderMapping=<redacted>,sourceUri={SourceUri},newUri={NewUri})", sourceUri, newUri);
        }

        /// <inheritdoc />
        protected override void MoveFolderInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri)
        {
			Logger.LogError(">>> Entering MoveFolderInternal(folderMapping=<redacted>,sourceUri={SourceUri},newUri={NewUri})", sourceUri, newUri);
            var container = this.GetContainer(folderMapping);
            var directory = container.GetDirectoryReference(sourceUri);
			Logger.LogError("MoveFolderInternal(folderMapping=<redacted>,sourceUri={SourceUri},newUri={NewUri}): directory → {Directory}", sourceUri, newUri, directory);
            var blobs = directory.ListBlobs(true);
			Logger.LogError("MoveFolderInternal(folderMapping=<redacted>,sourceUri={SourceUri},newUri={NewUri}): blobs → {Blobs}", sourceUri, newUri, blobs);
			Logger.LogError("MoveFolderInternal(folderMapping=<redacted>,sourceUri={SourceUri},newUri={NewUri}): container → {Container}", sourceUri, newUri, container);

            foreach (var blobItem in blobs)
            {
                var blob = (CloudBlob)blobItem;
				Logger.LogError("MoveFolderInternal(folderMapping=<redacted>,sourceUri={SourceUri},newUri={NewUri}): blob → {Blob}", sourceUri, newUri, blob);
                var newBlob = container.GetBlobReference(newUri + blobItem.Uri.LocalPath.Substring(directory.Uri.LocalPath.Length));
				Logger.LogError("MoveFolderInternal(folderMapping=<redacted>,sourceUri={SourceUri},newUri={NewUri}): newBlob → {NewBlob}", sourceUri, newUri, newBlob);
                newBlob.StartCopy(blob.Uri);
                blob.Delete();
            }

            this.ClearCache(folderMapping.FolderMappingID);
			Logger.LogWarning("<<< Exiting MoveFolderInternal(folderMapping=<redacted>,sourceUri={SourceUri},newUri={NewUri})", sourceUri, newUri);
        }

        /// <inheritdoc />
        protected override void UpdateFileInternal(Stream stream, FolderMappingInfo folderMapping, string uri)
        {
			Logger.LogError(">>> Entering UpdateFileInternal(stream={Stream},folderMapping=<redacted>,uri={Uri})", stream, uri);
            var container = this.GetContainer(folderMapping);
            var blob = container.GetBlockBlobReference(uri);
			Logger.LogError("UpdateFileInternal(stream={Stream},folderMapping=<redacted>,uri={Uri}): blob → {Blob}", stream, uri, blob);
			Logger.LogError("UpdateFileInternal(stream={Stream},folderMapping=<redacted>,uri={Uri}): container → {Container}", stream, uri, container);

            stream.Seek(0, SeekOrigin.Begin);
            blob.UploadFromStream(stream);

            // Set the content type
            blob.Properties.ContentType = FileContentTypeManager.Instance.GetContentType(Path.GetExtension(uri));

            // Set cache control
            if (!string.IsNullOrEmpty(this.cacheControl))
            {
                blob.Properties.CacheControl = this.cacheControl;
            }

            blob.SetProperties();

            this.ClearCache(folderMapping.FolderMappingID);
			Logger.LogWarning("<<< Exiting UpdateFileInternal(stream={Stream},folderMapping=<redacted>,uri={Uri})", stream, uri);
        }

        private static void CheckSettings(FolderMappingInfo folderMapping)
        {
			Logger.LogError(">>> Entering CheckSettings(folderMapping=<redacted>)");
            var settings = folderMapping.FolderMappingSettings;

            if (string.IsNullOrEmpty((string)settings[Constants.AccountName]) ||
                string.IsNullOrEmpty((string)settings[Constants.AccountKey]) ||
                string.IsNullOrEmpty((string)settings[Constants.Container]) ||
                string.IsNullOrEmpty((string)settings[Constants.UseHttps]))
            {
				Logger.LogError("Throwing Exception from CheckSettings(folderMapping=<redacted>)");
			Logger.LogWarning("<<< Exiting CheckSettings(folderMapping=<redacted>)");
                throw new Exception("Settings cannot be found.");
            }
        }

        private CloudBlobContainer GetContainer(FolderMappingInfo folderMapping)
        {
			Logger.LogError(">>> Entering GetContainer(folderMapping=<redacted>)");
            CheckSettings(folderMapping);

            var accountName = GetEncryptedSetting(this.cryptographyProvider, this.HostSettings, folderMapping.FolderMappingSettings, Constants.AccountName);
			Logger.LogError("GetContainer(folderMapping=<redacted>): accountName → {AccountName}", accountName);
            var accountKey = GetEncryptedSetting(this.cryptographyProvider, this.HostSettings, folderMapping.FolderMappingSettings, Constants.AccountKey);
			Logger.LogError("GetContainer(folderMapping=<redacted>): accountKey → {AccountKey}", accountKey);
            var container = GetSetting(folderMapping, Constants.Container);
			Logger.LogError("GetContainer(folderMapping=<redacted>): container → {Container}", container);
            var useHttps = GetBooleanSetting(folderMapping, Constants.UseHttps);
			Logger.LogError("GetContainer(folderMapping=<redacted>): useHttps → {UseHttps}", useHttps);

            var sc = new StorageCredentials(accountName, accountKey);
			Logger.LogError("GetContainer(folderMapping=<redacted>): sc → {Sc}", sc);
            var csa = new CloudStorageAccount(sc, useHttps);
			Logger.LogError("GetContainer(folderMapping=<redacted>): csa → {Csa}", csa);
            var blobClient = csa.CreateCloudBlobClient();
			Logger.LogError("GetContainer(folderMapping=<redacted>): blobClient → {BlobClient}", blobClient);
			Logger.LogWarning("<<< Exiting GetContainer(folderMapping=<redacted>)");
            return blobClient.GetContainerReference(container);
        }
    }
}
