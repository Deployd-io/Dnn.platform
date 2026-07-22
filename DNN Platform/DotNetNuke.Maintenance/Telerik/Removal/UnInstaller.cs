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

    /// <summary>Base class for uninstalling Telerik and related components.</summary>
    internal class UnInstaller
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IServiceProvider serviceProvider;
        private readonly ILocalizer localizer;

        /// <summary>Initializes a new instance of the <see cref="UnInstaller"/> class.</summary>
        /// <param name="serviceProvider">Instance of <see cref="IServiceProvider"/>.</param>
        /// <param name="localizer">Instance of <see cref="ILocalizer"/>.</param>
        public UnInstaller(IServiceProvider serviceProvider, ILocalizer localizer)
        {
			Logger.LogDebug(">>> Entering UnInstaller(serviceProvider={ServiceProvider},localizer={Localizer})", serviceProvider, localizer);
            this.serviceProvider = serviceProvider ??
                throw new ArgumentNullException(nameof(serviceProvider));

            this.localizer = localizer ??
				Logger.LogDebug("<<< Exiting UnInstaller(serviceProvider={ServiceProvider},localizer={Localizer})", serviceProvider, localizer);
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
            var step = this.GetService<IReplaceTabModuleStep>();
            step.OldModuleName = oldModuleName;
            step.NewModuleName = newModuleName;
            step.MigrateSettings = migrateSettings;
            return step;
        }

        /// <summary>Returns a step to remove a package from the DNN installation.</summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="deleteFiles">Whether to delete installed files.</param>
        /// <returns><see cref="IStep"/> of the package remover.</returns>
        private protected IStep RemoveExtension(string packageName, bool deleteFiles = true)
        {
			Logger.LogDebug(">>> Entering RemoveExtension(packageName={PackageName},deleteFiles={DeleteFiles})", packageName, deleteFiles);
            var step = this.GetService<IRemoveExtensionStep>();
            step.DeleteFiles = deleteFiles;
			Logger.LogDebug("RemoveExtension(packageName={PackageName},deleteFiles={DeleteFiles}): step → {Step}", packageName, deleteFiles, step);
			Logger.LogDebug("RemoveExtension(packageName={PackageName},deleteFiles={DeleteFiles}): step.DeleteFiles → {Arg2}", packageName, deleteFiles, step.DeleteFiles);
            step.PackageName = packageName;
			Logger.LogDebug("RemoveExtension(packageName={PackageName},deleteFiles={DeleteFiles}): step.PackageName → {Arg2}", packageName, deleteFiles, step.PackageName);
			Logger.LogDebug("<<< Exiting RemoveExtension(packageName={PackageName},deleteFiles={DeleteFiles})", packageName, deleteFiles);
            return step;
        }

        /// <summary>Returns a step to remove a file from the DNN installation.</summary>
        /// <param name="relativePath">Path to the directory to search, relative to the application root path.</param>
        /// <param name="searchPattern">Search string to match against the names of files in path.</param>
        /// <returns><see cref="IStep"/> of the file remover.</returns>
        private protected IStep RemoveFile(string relativePath, string searchPattern)
        {
			Logger.LogDebug(">>> Entering RemoveFile(relativePath={RelativePath},searchPattern={SearchPattern})", relativePath, searchPattern);
            var step = this.GetService<IDeleteFilesStep>();
            step.RelativePath = relativePath;
			Logger.LogDebug("RemoveFile(relativePath={RelativePath},searchPattern={SearchPattern}): step → {Step}", relativePath, searchPattern, step);
			Logger.LogDebug("RemoveFile(relativePath={RelativePath},searchPattern={SearchPattern}): step.RelativePath → {Arg2}", relativePath, searchPattern, step.RelativePath);
			Logger.LogDebug("RemoveFile(relativePath={RelativePath},searchPattern={SearchPattern}): step.SearchPattern → {Arg2}", relativePath, searchPattern, step.SearchPattern);
            step.SearchPattern = searchPattern;
            step.SearchOption = SearchOption.TopDirectoryOnly;
			Logger.LogDebug("RemoveFile(relativePath={RelativePath},searchPattern={SearchPattern}): step.SearchOption → {Arg2}", relativePath, searchPattern, step.SearchOption);
			Logger.LogDebug("<<< Exiting RemoveFile(relativePath={RelativePath},searchPattern={SearchPattern})", relativePath, searchPattern);
            return step;
        }

        /// <summary>Returns a step to replace a data type's editor.</summary>
        /// <param name="value">Data type's name.</param>
        /// <returns><see cref="IStep"/> of the data type updater.</returns>
        private protected IStep UpdateDataTypeList(string value)
        {
			Logger.LogDebug(">>> Entering UpdateDataTypeList(value={Value})", value);
            var step = this.GetService<IExecuteSqlStep>();
            step.Name = this.localizer.LocalizeFormat("UninstallStepUpdateDataTypeListFormat", value);
			Logger.LogDebug("UpdateDataTypeList(value={Value}): step → {Step}", value, step);
			Logger.LogDebug("UpdateDataTypeList(value={Value}): step.CommandText → {Arg1}", value, step.CommandText);
            step.CommandText = $$"""
                                 UPDATE {databaseOwner}[{objectQualifier}Lists]
                                 SET Text = 'DotNetNuke.Web.UI.WebControls.Internal.PropertyEditorControls.{{value}}EditControl, DotNetNuke.Web'
                                 WHERE ListName = 'DataType' AND Value = '{{value}}'
                                 """;
			Logger.LogDebug("<<< Exiting UpdateDataTypeList(value={Value})", value);
            return step;
        }

        /// <summary>Returns a step to remove Telerik rewriter rules in the siteurls.config.</summary>
        /// <returns><see cref="IStep"/> of the Telerik rewriter remover.</returns>
        private protected IStep UpdateSiteUrlsConfig()
        {
			Logger.LogDebug(">>> Entering UpdateSiteUrlsConfig()");
			Logger.LogDebug("<<< Exiting UpdateSiteUrlsConfig()");
            return this.GetService<IRemoveTelerikRewriterRulesStep>();
        }

        /// <summary>Returns step to remove items from the web.config.</summary>
        /// <param name="collectionPath">Collection path.</param>
        /// <param name="attributeNames">Attribute names.</param>
        /// <returns><see cref="IStep"/> of the web.config editor.</returns>
        private protected IStep UpdateWebConfig(string collectionPath, string attributeNames)
        {
			Logger.LogDebug(">>> Entering UpdateWebConfig(collectionPath={CollectionPath},attributeNames={AttributeNames})", collectionPath, attributeNames);
            var step = this.GetService<IRemoveItemFromCollectionStep>();
            step.Name = this.localizer.LocalizeFormat("UninstallStepUpdateWebConfigFormat", collectionPath);
			Logger.LogDebug("UpdateWebConfig(collectionPath={CollectionPath},attributeNames={AttributeNames}): step → {Step}", collectionPath, attributeNames, step);
			Logger.LogDebug("UpdateWebConfig(collectionPath={CollectionPath},attributeNames={AttributeNames}): step.RelativeFilePath → {Arg2}", collectionPath, attributeNames, step.RelativeFilePath);
			Logger.LogDebug("UpdateWebConfig(collectionPath={CollectionPath},attributeNames={AttributeNames}): step.CollectionPath → {Arg2}", collectionPath, attributeNames, step.CollectionPath);
			Logger.LogDebug("UpdateWebConfig(collectionPath={CollectionPath},attributeNames={AttributeNames}): step.AttributeNamesToIncludeInSearch → {Arg2}", collectionPath, attributeNames, step.AttributeNamesToIncludeInSearch);
			Logger.LogDebug("UpdateWebConfig(collectionPath={CollectionPath},attributeNames={AttributeNames}): step.SearchTerm → {Arg2}", collectionPath, attributeNames, step.SearchTerm);
            step.RelativeFilePath = "Web.config";
            step.CollectionPath = collectionPath;
            step.AttributeNamesToIncludeInSearch = attributeNames;
            step.SearchTerm = "telerik";
			Logger.LogDebug("<<< Exiting UpdateWebConfig(collectionPath={CollectionPath},attributeNames={AttributeNames})", collectionPath, attributeNames);
            return step;
        }

        /// <summary>Returns a step to remove Telerik binding redirects from the web.config.</summary>
        /// <returns><see cref="IStep"/> of the Telerik binding redirect remover.</returns>
        private protected IStep RemoveTelerikBindingRedirects()
        {
			Logger.LogDebug(">>> Entering RemoveTelerikBindingRedirects()");
			Logger.LogDebug("<<< Exiting RemoveTelerikBindingRedirects()");
            return this.GetService<IRemoveTelerikBindingRedirectsStep>();
        }

        /// <summary>Returns a step to remove uninstalled extensions from the install folder so they can't get installed by mistake.</summary>
        /// <param name="relativePath">Path to where the extension is.</param>
        /// <param name="searchPattern">Pattern to get the right files.</param>
        /// <param name="recurse">Whether to recurse folders.</param>
        /// <returns><see cref="IStep"/> of the uninstalled extension remover.</returns>
        private protected IStep RemoveUninstalledExtensionFiles(string relativePath, string searchPattern, bool recurse = false)
        {
			Logger.LogDebug(">>> Entering RemoveUninstalledExtensionFiles(relativePath={RelativePath},searchPattern={SearchPattern},recurse={Recurse})", relativePath, searchPattern, recurse);
            var step = this.GetService<IDeleteFilesStep>();
            step.Name = this.localizer.LocalizeFormat(
                "UninstallStepCleanupExtensionFilesFormat", $"{relativePath}/{searchPattern}");
				Logger.LogDebug("RemoveUninstalledExtensionFiles(relativePath={RelativePath},searchPattern={SearchPattern},recurse={Recurse}): step → {Step}", relativePath, searchPattern, recurse, step);
            step.RelativePath = relativePath;
			Logger.LogDebug("RemoveUninstalledExtensionFiles(relativePath={RelativePath},searchPattern={SearchPattern},recurse={Recurse}): step.RelativePath → {Arg3}", relativePath, searchPattern, recurse, step.RelativePath);
			Logger.LogDebug("RemoveUninstalledExtensionFiles(relativePath={RelativePath},searchPattern={SearchPattern},recurse={Recurse}): step.SearchPattern → {Arg3}", relativePath, searchPattern, recurse, step.SearchPattern);
			Logger.LogDebug("RemoveUninstalledExtensionFiles(relativePath={RelativePath},searchPattern={SearchPattern},recurse={Recurse}): step.SearchOption → {Arg3}", relativePath, searchPattern, recurse, step.SearchOption);
            step.SearchPattern = searchPattern;
            step.SearchOption = recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			Logger.LogDebug("<<< Exiting RemoveUninstalledExtensionFiles(relativePath={RelativePath},searchPattern={SearchPattern},recurse={Recurse})", relativePath, searchPattern, recurse);
            return step;
        }

        /// <summary>Returns a service dependency.</summary>
        /// <typeparam name="T">Dependency type.</typeparam>
        /// <returns>Requested Service.</returns>
        private protected T GetService<T>()
            where T : class
        {
			Logger.LogDebug(">>> Entering GetService()");
            return (T)this.serviceProvider.GetService(typeof(T));
        }
    }
}
