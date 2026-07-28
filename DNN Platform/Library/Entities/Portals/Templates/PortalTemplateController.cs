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
		eventLogger.LogDebug(">>> Entering PortalTemplateController(permissionDefinitionService={PermissionDefinitionService},businessControllerProvider={BusinessControllerProvider},listController={ListController},eventLogger={EventLogger},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},userController={UserController},fileContentTypeManager={FileContentTypeManager},roleProvider={RoleProvider},roleController={RoleController})", permissionDefinitionService, businessControllerProvider, listController, eventLogger, hostSettings, portalController, appStatus, portalGroupController, userController, fileContentTypeManager, roleProvider, roleController);
        private readonly IPermissionDefinitionService permissionDefinitionService = permissionDefinitionService ?? Globals.DependencyProvider.GetRequiredService<IPermissionDefinitionService>();
        private readonly IBusinessControllerProvider businessControllerProvider = businessControllerProvider ?? Globals.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
		eventLogger.LogDebug("PortalTemplateController(permissionDefinitionService={PermissionDefinitionService},businessControllerProvider={BusinessControllerProvider},listController={ListController},eventLogger={EventLogger},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},userController={UserController},fileContentTypeManager={FileContentTypeManager},roleProvider={RoleProvider},roleController={RoleController}): permissionDefinitionService → {PermissionDefinitionService1}", permissionDefinitionService, businessControllerProvider, listController, eventLogger, hostSettings, portalController, appStatus, portalGroupController, userController, fileContentTypeManager, roleProvider, roleController, permissionDefinitionService);
		eventLogger.LogDebug("PortalTemplateController(permissionDefinitionService={PermissionDefinitionService},businessControllerProvider={BusinessControllerProvider},listController={ListController},eventLogger={EventLogger},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},userController={UserController},fileContentTypeManager={FileContentTypeManager},roleProvider={RoleProvider},roleController={RoleController}): businessControllerProvider → {BusinessControllerProvider1}", permissionDefinitionService, businessControllerProvider, listController, eventLogger, hostSettings, portalController, appStatus, portalGroupController, userController, fileContentTypeManager, roleProvider, roleController, businessControllerProvider);
        private readonly ListController listController = listController ?? Globals.DependencyProvider.GetRequiredService<ListController>();
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.DependencyProvider.GetRequiredService<IEventLogger>();
		eventLogger.LogDebug("PortalTemplateController(permissionDefinitionService={PermissionDefinitionService},businessControllerProvider={BusinessControllerProvider},listController={ListController},eventLogger={EventLogger},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},userController={UserController},fileContentTypeManager={FileContentTypeManager},roleProvider={RoleProvider},roleController={RoleController}): listController → {ListController1}", permissionDefinitionService, businessControllerProvider, listController, eventLogger, hostSettings, portalController, appStatus, portalGroupController, userController, fileContentTypeManager, roleProvider, roleController, listController);
		eventLogger.LogDebug("PortalTemplateController(permissionDefinitionService={PermissionDefinitionService},businessControllerProvider={BusinessControllerProvider},listController={ListController},eventLogger={EventLogger},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},userController={UserController},fileContentTypeManager={FileContentTypeManager},roleProvider={RoleProvider},roleController={RoleController}): eventLogger → {EventLogger1}", permissionDefinitionService, businessControllerProvider, listController, eventLogger, hostSettings, portalController, appStatus, portalGroupController, userController, fileContentTypeManager, roleProvider, roleController, eventLogger);
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.DependencyProvider.GetRequiredService<IHostSettings>();
        private readonly IPortalController portalController = portalController ?? Globals.DependencyProvider.GetRequiredService<IPortalController>();
		eventLogger.LogDebug("PortalTemplateController(permissionDefinitionService={PermissionDefinitionService},businessControllerProvider={BusinessControllerProvider},listController={ListController},eventLogger={EventLogger},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},userController={UserController},fileContentTypeManager={FileContentTypeManager},roleProvider={RoleProvider},roleController={RoleController}): hostSettings → {HostSettings1}", permissionDefinitionService, businessControllerProvider, listController, eventLogger, hostSettings, portalController, appStatus, portalGroupController, userController, fileContentTypeManager, roleProvider, roleController, hostSettings);
		eventLogger.LogDebug("PortalTemplateController(permissionDefinitionService={PermissionDefinitionService},businessControllerProvider={BusinessControllerProvider},listController={ListController},eventLogger={EventLogger},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},userController={UserController},fileContentTypeManager={FileContentTypeManager},roleProvider={RoleProvider},roleController={RoleController}): portalController → {PortalController1}", permissionDefinitionService, businessControllerProvider, listController, eventLogger, hostSettings, portalController, appStatus, portalGroupController, userController, fileContentTypeManager, roleProvider, roleController, portalController);
        private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.DependencyProvider.GetRequiredService<IApplicationStatusInfo>();
        private readonly IPortalGroupController portalGroupController = portalGroupController ?? Globals.DependencyProvider.GetRequiredService<IPortalGroupController>();
		eventLogger.LogDebug("PortalTemplateController(permissionDefinitionService={PermissionDefinitionService},businessControllerProvider={BusinessControllerProvider},listController={ListController},eventLogger={EventLogger},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},userController={UserController},fileContentTypeManager={FileContentTypeManager},roleProvider={RoleProvider},roleController={RoleController}): appStatus → {AppStatus1}", permissionDefinitionService, businessControllerProvider, listController, eventLogger, hostSettings, portalController, appStatus, portalGroupController, userController, fileContentTypeManager, roleProvider, roleController, appStatus);
		eventLogger.LogDebug("PortalTemplateController(permissionDefinitionService={PermissionDefinitionService},businessControllerProvider={BusinessControllerProvider},listController={ListController},eventLogger={EventLogger},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},userController={UserController},fileContentTypeManager={FileContentTypeManager},roleProvider={RoleProvider},roleController={RoleController}): portalGroupController → {PortalGroupController1}", permissionDefinitionService, businessControllerProvider, listController, eventLogger, hostSettings, portalController, appStatus, portalGroupController, userController, fileContentTypeManager, roleProvider, roleController, portalGroupController);
        private readonly IUserController userController = userController ?? Globals.DependencyProvider.GetRequiredService<IUserController>();
        private readonly IFileContentTypeManager fileContentTypeManager = fileContentTypeManager ?? Globals.DependencyProvider.GetRequiredService<IFileContentTypeManager>();
		eventLogger.LogDebug("PortalTemplateController(permissionDefinitionService={PermissionDefinitionService},businessControllerProvider={BusinessControllerProvider},listController={ListController},eventLogger={EventLogger},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},userController={UserController},fileContentTypeManager={FileContentTypeManager},roleProvider={RoleProvider},roleController={RoleController}): userController → {UserController1}", permissionDefinitionService, businessControllerProvider, listController, eventLogger, hostSettings, portalController, appStatus, portalGroupController, userController, fileContentTypeManager, roleProvider, roleController, userController);
		eventLogger.LogDebug("PortalTemplateController(permissionDefinitionService={PermissionDefinitionService},businessControllerProvider={BusinessControllerProvider},listController={ListController},eventLogger={EventLogger},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},userController={UserController},fileContentTypeManager={FileContentTypeManager},roleProvider={RoleProvider},roleController={RoleController}): fileContentTypeManager → {FileContentTypeManager1}", permissionDefinitionService, businessControllerProvider, listController, eventLogger, hostSettings, portalController, appStatus, portalGroupController, userController, fileContentTypeManager, roleProvider, roleController, fileContentTypeManager);
        private readonly RoleProvider roleProvider = roleProvider ?? Globals.DependencyProvider.GetRequiredService<RoleProvider>();
        private readonly IRoleController roleController = roleController ?? Globals.DependencyProvider.GetRequiredService<IRoleController>();
		eventLogger.LogDebug("PortalTemplateController(permissionDefinitionService={PermissionDefinitionService},businessControllerProvider={BusinessControllerProvider},listController={ListController},eventLogger={EventLogger},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},userController={UserController},fileContentTypeManager={FileContentTypeManager},roleProvider={RoleProvider},roleController={RoleController}): roleProvider → {RoleProvider1}", permissionDefinitionService, businessControllerProvider, listController, eventLogger, hostSettings, portalController, appStatus, portalGroupController, userController, fileContentTypeManager, roleProvider, roleController, roleProvider);
		eventLogger.LogDebug("PortalTemplateController(permissionDefinitionService={PermissionDefinitionService},businessControllerProvider={BusinessControllerProvider},listController={ListController},eventLogger={EventLogger},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},userController={UserController},fileContentTypeManager={FileContentTypeManager},roleProvider={RoleProvider},roleController={RoleController}): roleController → {RoleController1}", permissionDefinitionService, businessControllerProvider, listController, eventLogger, hostSettings, portalController, appStatus, portalGroupController, userController, fileContentTypeManager, roleProvider, roleController, roleController);

        /// <summary>Initializes a new instance of the <see cref="PortalTemplateController"/> class.</summary>
		eventLogger.LogDebug("<<< Exiting PortalTemplateController(permissionDefinitionService={PermissionDefinitionService},businessControllerProvider={BusinessControllerProvider},listController={ListController},eventLogger={EventLogger},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},userController={UserController},fileContentTypeManager={FileContentTypeManager},roleProvider={RoleProvider},roleController={RoleController})", permissionDefinitionService, businessControllerProvider, listController, eventLogger, hostSettings, portalController, appStatus, portalGroupController, userController, fileContentTypeManager, roleProvider, roleController);
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public PortalTemplateController()
            : this(null, null, null, null, null, null, null, null, null, null, null, null)
        {
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
            var importer = new PortalTemplateImporter(this.permissionDefinitionService, this.businessControllerProvider, this.listController, this.eventLogger, this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, this.userController, this.fileContentTypeManager, this.roleProvider, this.roleController, template);
            importer.ParseTemplate(portalId, administratorId, mergeTabs, isNewPortal);
        }

        /// <inheritdoc />
        public (bool Success, string Message) ExportPortalTemplate(int portalId, string fileName, string description, bool isMultiLanguage, IEnumerable<string> locales, string localizationCulture, IEnumerable<int> exportTabIds, bool includeContent, bool includeFiles, bool includeModules, bool includeProfile, bool includeRoles)
        {
			eventLogger.LogDebug(">>> Entering ExportPortalTemplate(Success={Success},Message={Message})", Success, Message);
            var exporter = new PortalTemplateExporter(this.businessControllerProvider, this.listController, this.portalController, this.hostSettings, this.roleProvider, this.appStatus, this.portalGroupController);
			eventLogger.LogDebug("ExportPortalTemplate(Success={Success},Message={Message}): exporter → {Exporter}", Success, Message, exporter);
			eventLogger.LogDebug("<<< Exiting ExportPortalTemplate(Success={Success},Message={Message})", Success, Message);
            return exporter.ExportPortalTemplate(portalId, fileName, description, isMultiLanguage, locales, localizationCulture, exportTabIds, includeContent, includeFiles, includeModules, includeProfile, includeRoles);
        }

        /// <inheritdoc />
        public IPortalTemplateInfo GetPortalTemplate(string templatePath, string cultureCode)
        {
			eventLogger.LogDebug(">>> Entering GetPortalTemplate(templatePath={TemplatePath},cultureCode={CultureCode})", templatePath, cultureCode);
            var template = new PortalTemplateInfo(templatePath, cultureCode);

			eventLogger.LogDebug("GetPortalTemplate(templatePath={TemplatePath},cultureCode={CultureCode}): template → {Template}", templatePath, cultureCode, template);
            if (!string.IsNullOrEmpty(cultureCode) && template.CultureCode != cultureCode)
            {
                return null;
            }

			eventLogger.LogDebug("<<< Exiting GetPortalTemplate(templatePath={TemplatePath},cultureCode={CultureCode})", templatePath, cultureCode);
            return template;
        }

        public IList<IPortalTemplateInfo> GetPortalTemplates()
        {
			eventLogger.LogDebug(">>> Entering GetPortalTemplates()");
            var list = new List<IPortalTemplateInfo>();

			eventLogger.LogDebug("GetPortalTemplates(): list → {List}", list);
            var templateFilePaths = PortalTemplateIO.Instance.EnumerateTemplates();
            var languageFileNames = PortalTemplateIO.Instance.EnumerateLanguageFiles().Select(Path.GetFileName).ToList();
			eventLogger.LogDebug("GetPortalTemplates(): templateFilePaths → {TemplateFilePaths}", templateFilePaths);
			eventLogger.LogDebug("GetPortalTemplates(): languageFileNames → {LanguageFileNames}", languageFileNames);

            foreach (string templateFilePath in templateFilePaths)
            {
                var currentFileName = Path.GetFileName(templateFilePath);
                var langs = languageFileNames.Where(x => GetTemplateName(x).Equals(currentFileName, StringComparison.OrdinalIgnoreCase)).Select(x => GetCultureCode(x)).Distinct().ToList();
				eventLogger.LogDebug("GetPortalTemplates(): currentFileName → {CurrentFileName}", currentFileName);
				eventLogger.LogDebug("GetPortalTemplates(): langs → {Langs}", langs);

                if (langs.Count != 0)
                {
                    langs.ForEach(x => list.Add(new PortalTemplateInfo(templateFilePath, x)));
                }
                else
                {
                    list.Add(new PortalTemplateInfo(templateFilePath, string.Empty));
                }
            }

			eventLogger.LogDebug("<<< Exiting GetPortalTemplates()");
            return list;
        }

        /// <summary>Instantiates a new instance of the PortalTemplateController.</summary>
        /// <returns>An instance of IPortalTemplateController.</returns>
        protected override Func<IPortalTemplateController> GetFactory()
        {
			eventLogger.LogDebug(">>> Entering GetFactory()");
			eventLogger.LogDebug("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<IPortalTemplateController>;
        }

        private static string GetTemplateName(string languageFileName)
        {
			eventLogger.LogDebug(">>> Entering GetTemplateName(languageFileName={LanguageFileName})", languageFileName);
            // e.g. "default template.template.en-US.resx"
			eventLogger.LogDebug("<<< Exiting GetTemplateName(languageFileName={LanguageFileName})", languageFileName);
            return languageFileName.GetFileNameFromLocalizedResxFile();
        }

        private static string GetCultureCode(string languageFileName)
        {
			eventLogger.LogDebug(">>> Entering GetCultureCode(languageFileName={LanguageFileName})", languageFileName);
            // e.g. "default template.template.en-US.resx"
			eventLogger.LogDebug("<<< Exiting GetCultureCode(languageFileName={LanguageFileName})", languageFileName);
            return languageFileName.GetLocaleCodeFromFileName();
        }
    }
}
