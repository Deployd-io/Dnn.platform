// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Removal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    using DotNetNuke.Maintenance.Telerik.Steps;
	using Microsoft.Extensions.Logging;

    /// <summary>Base class for uninstalling Telerik and related components.</summary>
    internal class UnInstaller
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly IServiceProvider serviceProvider;
        private readonly ILocalizer localizer;

        /// <summary>Initializes a new instance of the <see cref="UnInstaller"/> class.</summary>
        /// <param name="serviceProvider">Instance of <see cref="IServiceProvider"/>.</param>
        /// <param name="localizer">Instance of <see cref="ILocalizer"/>.</param>
        public UnInstaller(IServiceProvider serviceProvider, ILocalizer localizer)
        {
			_logger.LogDebug(">>> Entering UnInstaller(serviceProvider={ServiceProvider},localizer={Localizer})", serviceProvider, localizer);
            this.serviceProvider = serviceProvider ??
				_logger.LogDebug("UnInstaller(serviceProvider={ServiceProvider},localizer={Localizer}): serviceProvider → {ServiceProvider1}", serviceProvider, localizer, serviceProvider);
				_logger.LogDebug("UnInstaller(serviceProvider={ServiceProvider},localizer={Localizer}): this.serviceProvider → {Arg2}", serviceProvider, localizer, this.serviceProvider);
				_logger.LogDebug("UnInstaller(serviceProvider={ServiceProvider},localizer={Localizer}): localizer → {Localizer1}", serviceProvider, localizer, localizer);
                throw new ArgumentNullException(nameof(serviceProvider));

            this.localizer = localizer ??
				_logger.LogDebug("<<< Exiting UnInstaller(serviceProvider={ServiceProvider},localizer={Localizer})", serviceProvider, localizer);
				_logger.LogDebug("UnInstaller(serviceProvider={ServiceProvider},localizer={Localizer}): this.localizer → {Arg2}", serviceProvider, localizer, this.localizer);
				_logger.LogWarning("Throwing ArgumentNullException from UnInstaller(serviceProvider={ServiceProvider},localizer={Localizer})", serviceProvider, localizer);
                throw new ArgumentNullException(nameof(localizer));

            this.ProgressInternal = new List<UninstallSummaryItem>();
        }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> of type <see cref="UninstallSummaryItem"/>
        /// containing the overall execution progress.
        /// </summary>
        public IEnumerable<UninstallSummaryItem> Progress => this.ProgressInternal;

        /// <summary>
        /// Gets or sets an <see cref="List{T}"/> of type <see cref="UninstallSummaryItem"/>
        /// containing the overall execution progress.
        /// </summary>
        private protected List<UninstallSummaryItem> ProgressInternal { get; set; }

        /// <summary>Returns a step to replace one module by another.</summary>
        /// <param name="oldModuleName">Modulename of the module to be replaced.</param>
        /// <param name="newModuleName">Modulename of the module to be put in its place.</param>
        /// <param name="migrateSettings">Callback function to migrate settings.</param>
        /// <returns><see cref="IStep"/> of the module replacer.</returns>
        private protected IStep ReplaceModule(string oldModuleName, string newModuleName, Func<Hashtable, Hashtable> migrateSettings)
        {
			_logger.LogDebug(">>> Entering ReplaceModule(oldModuleName={OldModuleName},newModuleName={NewModuleName},migrateSettings={MigrateSettings})", oldModuleName, newModuleName, migrateSettings);
            var step = this.GetService<IReplaceTabModuleStep>();
            step.OldModuleName = oldModuleName;
			_logger.LogDebug("ReplaceModule(oldModuleName={OldModuleName},newModuleName={NewModuleName},migrateSettings={MigrateSettings}): step.OldModuleName → {Arg3}", oldModuleName, newModuleName, migrateSettings, step.OldModuleName);
			_logger.LogDebug("ReplaceModule(oldModuleName={OldModuleName},newModuleName={NewModuleName},migrateSettings={MigrateSettings}): step.NewModuleName → {Arg3}", oldModuleName, newModuleName, migrateSettings, step.NewModuleName);
			_logger.LogDebug("ReplaceModule(oldModuleName={OldModuleName},newModuleName={NewModuleName},migrateSettings={MigrateSettings}): step.MigrateSettings → {Arg3}", oldModuleName, newModuleName, migrateSettings, step.MigrateSettings);
			_logger.LogDebug("ReplaceModule(oldModuleName={OldModuleName},newModuleName={NewModuleName},migrateSettings={MigrateSettings}): step → {Step}", oldModuleName, newModuleName, migrateSettings, step);
            step.NewModuleName = newModuleName;
            step.MigrateSettings = migrateSettings;
			_logger.LogDebug("<<< Exiting ReplaceModule(oldModuleName={OldModuleName},newModuleName={NewModuleName},migrateSettings={MigrateSettings})", oldModuleName, newModuleName, migrateSettings);
            return step;
        }

        /// <summary>Returns a step to remove a package from the DNN installation.</summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="deleteFiles">Whether to delete installed files.</param>
        /// <returns><see cref="IStep"/> of the package remover.</returns>
        private protected IStep RemoveExtension(string packageName, bool deleteFiles = true)
        {
			_logger.LogDebug(">>> Entering RemoveExtension(packageName={PackageName},deleteFiles={DeleteFiles})", packageName, deleteFiles);
            var step = this.GetService<IRemoveExtensionStep>();
            step.DeleteFiles = deleteFiles;
			_logger.LogDebug("RemoveExtension(packageName={PackageName},deleteFiles={DeleteFiles}): step.DeleteFiles → {Arg2}", packageName, deleteFiles, step.DeleteFiles);
			_logger.LogDebug("RemoveExtension(packageName={PackageName},deleteFiles={DeleteFiles}): step.PackageName → {Arg2}", packageName, deleteFiles, step.PackageName);
			_logger.LogDebug("RemoveExtension(packageName={PackageName},deleteFiles={DeleteFiles}): step → {Step}", packageName, deleteFiles, step);
            step.PackageName = packageName;
			_logger.LogDebug("<<< Exiting RemoveExtension(packageName={PackageName},deleteFiles={DeleteFiles})", packageName, deleteFiles);
            return step;
        }

        /// <summary>Returns a step to remove a file from the DNN installation.</summary>
        /// <param name="relativePath">Path to the directory to search, relative to the application root path.</param>
        /// <param name="searchPattern">Search string to match against the names of files in path.</param>
        /// <returns><see cref="IStep"/> of the file remover.</returns>
        private protected IStep RemoveFile(string relativePath, string searchPattern)
        {
			_logger.LogDebug(">>> Entering RemoveFile(relativePath={RelativePath},searchPattern={SearchPattern})", relativePath, searchPattern);
            var step = this.GetService<IDeleteFilesStep>();
            step.RelativePath = relativePath;
			_logger.LogDebug("RemoveFile(relativePath={RelativePath},searchPattern={SearchPattern}): step.RelativePath → {Arg2}", relativePath, searchPattern, step.RelativePath);
			_logger.LogDebug("RemoveFile(relativePath={RelativePath},searchPattern={SearchPattern}): step.SearchPattern → {Arg2}", relativePath, searchPattern, step.SearchPattern);
			_logger.LogDebug("RemoveFile(relativePath={RelativePath},searchPattern={SearchPattern}): step.SearchOption → {Arg2}", relativePath, searchPattern, step.SearchOption);
			_logger.LogDebug("RemoveFile(relativePath={RelativePath},searchPattern={SearchPattern}): step → {Step}", relativePath, searchPattern, step);
            step.SearchPattern = searchPattern;
            step.SearchOption = SearchOption.TopDirectoryOnly;
			_logger.LogDebug("<<< Exiting RemoveFile(relativePath={RelativePath},searchPattern={SearchPattern})", relativePath, searchPattern);
            return step;
        }

        /// <summary>Returns a step to replace a data type's editor.</summary>
        /// <param name="value">Data type's name.</param>
        /// <returns><see cref="IStep"/> of the data type updater.</returns>
        private protected IStep UpdateDataTypeList(string value)
        {
			_logger.LogDebug(">>> Entering UpdateDataTypeList(value={Value})", value);
            var step = this.GetService<IExecuteSqlStep>();
            step.Name = this.localizer.LocalizeFormat("UninstallStepUpdateDataTypeListFormat", value);
			_logger.LogDebug("UpdateDataTypeList(value={Value}): step → {Step}", value, step);
            step.CommandText = $$"""
                                 UPDATE {databaseOwner}[{objectQualifier}Lists]
                                 SET Text = 'DotNetNuke.Web.UI.WebControls.Internal.PropertyEditorControls.{{value}}EditControl, DotNetNuke.Web'
                                 WHERE ListName = 'DataType' AND Value = '{{value}}'
                                 """;
								_logger.LogDebug("UpdateDataTypeList(value={Value}): step.CommandText → {Arg1}", value, step.CommandText);
			_logger.LogDebug("<<< Exiting UpdateDataTypeList(value={Value})", value);
            return step;
        }

        /// <summary>Returns a step to remove Telerik rewriter rules in the siteurls.config.</summary>
        /// <returns><see cref="IStep"/> of the Telerik rewriter remover.</returns>
        private protected IStep UpdateSiteUrlsConfig()
        {
			_logger.LogDebug(">>> Entering UpdateSiteUrlsConfig()");
			_logger.LogDebug("<<< Exiting UpdateSiteUrlsConfig()");
            return this.GetService<IRemoveTelerikRewriterRulesStep>();
        }

        /// <summary>Returns step to remove items from the web.config.</summary>
        /// <param name="collectionPath">Collection path.</param>
        /// <param name="attributeNames">Attribute names.</param>
        /// <returns><see cref="IStep"/> of the web.config editor.</returns>
        private protected IStep UpdateWebConfig(string collectionPath, string attributeNames)
        {
			_logger.LogDebug(">>> Entering UpdateWebConfig(collectionPath={CollectionPath},attributeNames={AttributeNames})", collectionPath, attributeNames);
            var step = this.GetService<IRemoveItemFromCollectionStep>();
            step.Name = this.localizer.LocalizeFormat("UninstallStepUpdateWebConfigFormat", collectionPath);
			_logger.LogDebug("UpdateWebConfig(collectionPath={CollectionPath},attributeNames={AttributeNames}): step → {Step}", collectionPath, attributeNames, step);
			_logger.LogDebug("UpdateWebConfig(collectionPath={CollectionPath},attributeNames={AttributeNames}): step.RelativeFilePath → {Arg2}", collectionPath, attributeNames, step.RelativeFilePath);
			_logger.LogDebug("UpdateWebConfig(collectionPath={CollectionPath},attributeNames={AttributeNames}): step.CollectionPath → {Arg2}", collectionPath, attributeNames, step.CollectionPath);
			_logger.LogDebug("UpdateWebConfig(collectionPath={CollectionPath},attributeNames={AttributeNames}): step.AttributeNamesToIncludeInSearch → {Arg2}", collectionPath, attributeNames, step.AttributeNamesToIncludeInSearch);
			_logger.LogDebug("UpdateWebConfig(collectionPath={CollectionPath},attributeNames={AttributeNames}): step.SearchTerm → {Arg2}", collectionPath, attributeNames, step.SearchTerm);
            step.RelativeFilePath = "Web.config";
            step.CollectionPath = collectionPath;
            step.AttributeNamesToIncludeInSearch = attributeNames;
            step.SearchTerm = "telerik";
			_logger.LogDebug("<<< Exiting UpdateWebConfig(collectionPath={CollectionPath},attributeNames={AttributeNames})", collectionPath, attributeNames);
            return step;
        }

        /// <summary>Returns a step to remove Telerik binding redirects from the web.config.</summary>
        /// <returns><see cref="IStep"/> of the Telerik binding redirect remover.</returns>
        private protected IStep RemoveTelerikBindingRedirects()
        {
			_logger.LogDebug(">>> Entering RemoveTelerikBindingRedirects()");
			_logger.LogDebug("<<< Exiting RemoveTelerikBindingRedirects()");
            return this.GetService<IRemoveTelerikBindingRedirectsStep>();
        }

        /// <summary>Returns a step to remove uninstalled extensions from the install folder so they can't get installed by mistake.</summary>
        /// <param name="relativePath">Path to where the extension is.</param>
        /// <param name="searchPattern">Pattern to get the right files.</param>
        /// <param name="recurse">Whether to recurse folders.</param>
        /// <returns><see cref="IStep"/> of the uninstalled extension remover.</returns>
        private protected IStep RemoveUninstalledExtensionFiles(string relativePath, string searchPattern, bool recurse = false)
        {
			_logger.LogDebug(">>> Entering RemoveUninstalledExtensionFiles(relativePath={RelativePath},searchPattern={SearchPattern},recurse={Recurse})", relativePath, searchPattern, recurse);
            var step = this.GetService<IDeleteFilesStep>();
            step.Name = this.localizer.LocalizeFormat(
                "UninstallStepCleanupExtensionFilesFormat", $"{relativePath}/{searchPattern}");
				_logger.LogDebug("RemoveUninstalledExtensionFiles(relativePath={RelativePath},searchPattern={SearchPattern},recurse={Recurse}): step → {Step}", relativePath, searchPattern, recurse, step);
				_logger.LogDebug("RemoveUninstalledExtensionFiles(relativePath={RelativePath},searchPattern={SearchPattern},recurse={Recurse}): step.RelativePath → {Arg3}", relativePath, searchPattern, recurse, step.RelativePath);
				_logger.LogDebug("RemoveUninstalledExtensionFiles(relativePath={RelativePath},searchPattern={SearchPattern},recurse={Recurse}): step.SearchPattern → {Arg3}", relativePath, searchPattern, recurse, step.SearchPattern);
				_logger.LogDebug("RemoveUninstalledExtensionFiles(relativePath={RelativePath},searchPattern={SearchPattern},recurse={Recurse}): step.SearchOption → {Arg3}", relativePath, searchPattern, recurse, step.SearchOption);
            step.RelativePath = relativePath;
            step.SearchPattern = searchPattern;
            step.SearchOption = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			_logger.LogDebug("<<< Exiting RemoveUninstalledExtensionFiles(relativePath={RelativePath},searchPattern={SearchPattern},recurse={Recurse})", relativePath, searchPattern, recurse);
            return step;
        }

        /// <summary>Returns a service dependency.</summary>
        /// <typeparam name="T">Dependency type.</typeparam>
        /// <returns>Requested Service.</returns>
        private protected T GetService<T>()
            where T : class
        {
			_logger.LogDebug(">>> Entering GetService()");
			_logger.LogDebug("<<< Exiting GetService()");
            return (T)this.serviceProvider.GetService(typeof(T));
        }
    }
}
