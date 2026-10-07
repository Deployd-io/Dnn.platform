// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals.Templates
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals.Templates;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Portals.Internal;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc cref="IPortalTemplateController"/>
    /// <param name="permissionDefinitionService">The permission definition service.</param>
    /// <param name="businessControllerProvider">The business controller provider.</param>
    /// <param name="listController">The list controller.</param>
    /// <param name="eventLogger">The event logger.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="appStatus">The application status.</param>
    /// <param name="portalGroupController">The portal group controller.</param>
    /// <param name="userController">The user controller.</param>
    /// <param name="fileContentTypeManager">The file content type manager.</param>
    /// <param name="roleProvider">The role provider.</param>
    /// <param name="roleController">The role controller.</param>
    public class PortalTemplateController(IPermissionDefinitionService permissionDefinitionService, IBusinessControllerProvider businessControllerProvider, ListController listController, IEventLogger eventLogger, IHostSettings hostSettings, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, IUserController userController, IFileContentTypeManager fileContentTypeManager, RoleProvider roleProvider, IRoleController roleController)
        : ServiceLocator<IPortalTemplateController, PortalTemplateController>, IPortalTemplateController
    {
        private readonly IPermissionDefinitionService permissionDefinitionService = permissionDefinitionService ?? Globals.DependencyProvider.GetRequiredService<IPermissionDefinitionService>();
        private readonly IBusinessControllerProvider businessControllerProvider = businessControllerProvider ?? Globals.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
        private readonly ListController listController = listController ?? Globals.DependencyProvider.GetRequiredService<ListController>();
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.DependencyProvider.GetRequiredService<IEventLogger>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.DependencyProvider.GetRequiredService<IHostSettings>();
        private readonly IPortalController portalController = portalController ?? Globals.DependencyProvider.GetRequiredService<IPortalController>();
        private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.DependencyProvider.GetRequiredService<IApplicationStatusInfo>();
        private readonly IPortalGroupController portalGroupController = portalGroupController ?? Globals.DependencyProvider.GetRequiredService<IPortalGroupController>();
        private readonly IUserController userController = userController ?? Globals.DependencyProvider.GetRequiredService<IUserController>();
        private readonly IFileContentTypeManager fileContentTypeManager = fileContentTypeManager ?? Globals.DependencyProvider.GetRequiredService<IFileContentTypeManager>();
        private readonly RoleProvider roleProvider = roleProvider ?? Globals.DependencyProvider.GetRequiredService<RoleProvider>();
        private readonly IRoleController roleController = roleController ?? Globals.DependencyProvider.GetRequiredService<IRoleController>();

        /// <summary>Initializes a new instance of the <see cref="PortalTemplateController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public PortalTemplateController()
            : this(null, null, null, null, null, null, null, null, null, null, null, null)
        {
		eventLogger.LogError(">>> Entering PortalTemplateController()");
        eventLogger.LogWarning("<<< Exiting PortalTemplateController()");
        }

        /// <summary>Initializes a new instance of the <see cref="PortalTemplateController"/> class.</summary>
        /// <param name="businessControllerProvider">The DI container.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public PortalTemplateController(IBusinessControllerProvider businessControllerProvider)
            : this(null, businessControllerProvider, null, null, null, null, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PortalTemplateController"/> class.</summary>
        /// <param name="businessControllerProvider">The DI container.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public PortalTemplateController(IBusinessControllerProvider businessControllerProvider, IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService)
            : this(permissionDefinitionService, businessControllerProvider, null, eventLogger, null, null, null, null, null, null, null, null)
        {
        }

        /// <inheritdoc />
        public void ApplyPortalTemplate(int portalId, IPortalTemplateInfo template, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal)
        {
			eventLogger.LogError(">>> Entering ApplyPortalTemplate(portalId={PortalId},template={Template},administratorId={AdministratorId},mergeTabs={MergeTabs},isNewPortal={IsNewPortal})", portalId, template, administratorId, mergeTabs, isNewPortal);
            var importer = new PortalTemplateImporter(this.permissionDefinitionService, this.businessControllerProvider, this.listController, this.eventLogger, this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, this.userController, this.fileContentTypeManager, this.roleProvider, this.roleController, template);
			eventLogger.LogWarning("<<< Exiting ApplyPortalTemplate(portalId={PortalId},template={Template},administratorId={AdministratorId},mergeTabs={MergeTabs},isNewPortal={IsNewPortal})", portalId, template, administratorId, mergeTabs, isNewPortal);
			eventLogger.LogError("ApplyPortalTemplate(portalId={PortalId},template={Template},administratorId={AdministratorId},mergeTabs={MergeTabs},isNewPortal={IsNewPortal}): importer → {Importer}", portalId, template, administratorId, mergeTabs, isNewPortal, importer);
            importer.ParseTemplate(portalId, administratorId, mergeTabs, isNewPortal);
        }

        /// <inheritdoc />
        public (bool Success, string Message) ExportPortalTemplate(int portalId, string fileName, string description, bool isMultiLanguage, IEnumerable<string> locales, string localizationCulture, IEnumerable<int> exportTabIds, bool includeContent, bool includeFiles, bool includeModules, bool includeProfile, bool includeRoles)
        {
			eventLogger.LogError(">>> Entering ExportPortalTemplate(portalId={PortalId},fileName={FileName},description={Description},isMultiLanguage={IsMultiLanguage},locales={Locales},localizationCulture={LocalizationCulture},exportTabIds={ExportTabIds},includeContent={IncludeContent},includeFiles={IncludeFiles},includeModules={IncludeModules},includeProfile={IncludeProfile},includeRoles={IncludeRoles})", portalId, fileName, description, isMultiLanguage, locales, localizationCulture, exportTabIds, includeContent, includeFiles, includeModules, includeProfile, includeRoles);
            var exporter = new PortalTemplateExporter(this.businessControllerProvider, this.listController, this.portalController, this.hostSettings, this.roleProvider, this.appStatus, this.portalGroupController);
			eventLogger.LogError("ExportPortalTemplate(portalId={PortalId},fileName={FileName},description={Description},isMultiLanguage={IsMultiLanguage},locales={Locales},localizationCulture={LocalizationCulture},exportTabIds={ExportTabIds},includeContent={IncludeContent},includeFiles={IncludeFiles},includeModules={IncludeModules},includeProfile={IncludeProfile},includeRoles={IncludeRoles}): exporter → {Exporter}", portalId, fileName, description, isMultiLanguage, locales, localizationCulture, exportTabIds, includeContent, includeFiles, includeModules, includeProfile, includeRoles, exporter);
			eventLogger.LogWarning("<<< Exiting ExportPortalTemplate(portalId={PortalId},fileName={FileName},description={Description},isMultiLanguage={IsMultiLanguage},locales={Locales},localizationCulture={LocalizationCulture},exportTabIds={ExportTabIds},includeContent={IncludeContent},includeFiles={IncludeFiles},includeModules={IncludeModules},includeProfile={IncludeProfile},includeRoles={IncludeRoles})", portalId, fileName, description, isMultiLanguage, locales, localizationCulture, exportTabIds, includeContent, includeFiles, includeModules, includeProfile, includeRoles);
            return exporter.ExportPortalTemplate(portalId, fileName, description, isMultiLanguage, locales, localizationCulture, exportTabIds, includeContent, includeFiles, includeModules, includeProfile, includeRoles);
        }

        /// <inheritdoc />
        public IPortalTemplateInfo GetPortalTemplate(string templatePath, string cultureCode)
        {
			eventLogger.LogError(">>> Entering GetPortalTemplate(templatePath={TemplatePath},cultureCode={CultureCode})", templatePath, cultureCode);
            var template = new PortalTemplateInfo(templatePath, cultureCode);

			eventLogger.LogError("GetPortalTemplate(templatePath={TemplatePath},cultureCode={CultureCode}): template → {Template}", templatePath, cultureCode, template);
            if (!string.IsNullOrEmpty(cultureCode) && template.CultureCode != cultureCode)
            {
                return null;
            }

			eventLogger.LogWarning("<<< Exiting GetPortalTemplate(templatePath={TemplatePath},cultureCode={CultureCode})", templatePath, cultureCode);
            return template;
        }

        public IList<IPortalTemplateInfo> GetPortalTemplates()
        {
			eventLogger.LogError(">>> Entering GetPortalTemplates()");
            var list = new List<IPortalTemplateInfo>();

			eventLogger.LogError("GetPortalTemplates(): list → {List}", list);
            var templateFilePaths = PortalTemplateIO.Instance.EnumerateTemplates();
			eventLogger.LogError("GetPortalTemplates(): templateFilePaths → {TemplateFilePaths}", templateFilePaths);
            var languageFileNames = PortalTemplateIO.Instance.EnumerateLanguageFiles().Select(Path.GetFileName).ToList();
			eventLogger.LogError("GetPortalTemplates(): languageFileNames → {LanguageFileNames}", languageFileNames);

            foreach (string templateFilePath in templateFilePaths)
            {
                var currentFileName = Path.GetFileName(templateFilePath);
				eventLogger.LogError("GetPortalTemplates(): currentFileName → {CurrentFileName}", currentFileName);
                var langs = languageFileNames.Where(x => GetTemplateName(x).Equals(currentFileName, StringComparison.OrdinalIgnoreCase)).Select(x => GetCultureCode(x)).Distinct().ToList();
				eventLogger.LogError("GetPortalTemplates(): langs → {Langs}", langs);

                if (langs.Count != 0)
                {
                    langs.ForEach(x => list.Add(new PortalTemplateInfo(templateFilePath, x)));
                }
                else
                {
                    list.Add(new PortalTemplateInfo(templateFilePath, string.Empty));
                }
            }

			eventLogger.LogWarning("<<< Exiting GetPortalTemplates()");
            return list;
        }

        /// <summary>Instantiates a new instance of the PortalTemplateController.</summary>
        /// <returns>An instance of IPortalTemplateController.</returns>
        protected override Func<IPortalTemplateController> GetFactory()
        {
			eventLogger.LogError(">>> Entering GetFactory()");
			eventLogger.LogWarning("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<IPortalTemplateController>;
        }

        private static string GetTemplateName(string languageFileName)
        {
			eventLogger.LogError(">>> Entering GetTemplateName(languageFileName={LanguageFileName})", languageFileName);
            // e.g. "default template.template.en-US.resx"
			eventLogger.LogWarning("<<< Exiting GetTemplateName(languageFileName={LanguageFileName})", languageFileName);
            return languageFileName.GetFileNameFromLocalizedResxFile();
        }

        private static string GetCultureCode(string languageFileName)
        {
			eventLogger.LogError(">>> Entering GetCultureCode(languageFileName={LanguageFileName})", languageFileName);
            // e.g. "default template.template.en-US.resx"
			eventLogger.LogWarning("<<< Exiting GetCultureCode(languageFileName={LanguageFileName})", languageFileName);
            return languageFileName.GetLocaleCodeFromFileName();
        }
    }
}
