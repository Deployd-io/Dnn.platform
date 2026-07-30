// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    public class RegistrationProfileController(ListController listController, IHostSettings hostSettings, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController)
        : ServiceLocator<IRegistrationProfileController, RegistrationProfileController>, IRegistrationProfileController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly ListController listController = listController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ListController>();
		Logger.LogDebug(">>> Entering RegistrationProfileController(listController={ListController},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController})", listController, hostSettings, portalController, appStatus, portalGroupController);
		Logger.LogDebug("RegistrationProfileController(listController={ListController},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): listController → {ListController1}", listController, hostSettings, portalController, appStatus, portalGroupController, listController);
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
		Logger.LogDebug("RegistrationProfileController(listController={ListController},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): hostSettings → {HostSettings1}", listController, hostSettings, portalController, appStatus, portalGroupController, hostSettings);
		Logger.LogDebug("RegistrationProfileController(listController={ListController},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): portalController → {PortalController1}", listController, hostSettings, portalController, appStatus, portalGroupController, portalController);
        private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        private readonly IPortalGroupController portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();
		Logger.LogDebug("RegistrationProfileController(listController={ListController},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): appStatus → {AppStatus1}", listController, hostSettings, portalController, appStatus, portalGroupController, appStatus);
		Logger.LogDebug("RegistrationProfileController(listController={ListController},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): portalGroupController → {PortalGroupController1}", listController, hostSettings, portalController, appStatus, portalGroupController, portalGroupController);

        /// <summary>Initializes a new instance of the <see cref="RegistrationProfileController"/> class.</summary>
		Logger.LogDebug("<<< Exiting RegistrationProfileController(listController={ListController},hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController})", listController, hostSettings, portalController, appStatus, portalGroupController);
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public RegistrationProfileController()
            : this(null, null, null, null, null)
        {
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public IEnumerable<string> Search(int portalId, string searchTerm)
        {
			Logger.LogDebug(">>> Entering Search(portalId={PortalId},searchTerm={SearchTerm})", portalId, searchTerm);
            ListEntryInfo imageType = this.listController.GetListEntryInfo("DataType", "Image");

			Logger.LogDebug("Search(portalId={PortalId},searchTerm={SearchTerm}): imageType → {ImageType}", portalId, searchTerm, imageType);
            List<string> results = [];
            var properties = ProfileController.GetPropertyDefinitionsByPortal(this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, portalId);
			Logger.LogDebug("Search(portalId={PortalId},searchTerm={SearchTerm}): results → {Results}", portalId, searchTerm, results);
			Logger.LogDebug("Search(portalId={PortalId},searchTerm={SearchTerm}): properties → {Properties}", portalId, searchTerm, properties);
            foreach (var definition in properties.Where(definition => definition.DataType != imageType.EntryID))
            {
                AddProperty(results, definition.PropertyName, searchTerm);
            }

            AddProperty(results, "Email", searchTerm);
            AddProperty(results, "DisplayName", searchTerm);
            AddProperty(results, "Username", searchTerm);
            AddProperty(results, "Password", searchTerm);
            AddProperty(results, "PasswordConfirm", searchTerm);
            AddProperty(results, "PasswordQuestion", searchTerm);
            AddProperty(results, "PasswordAnswer", searchTerm);

			Logger.LogDebug("<<< Exiting Search(portalId={PortalId},searchTerm={SearchTerm})", portalId, searchTerm);
            return results;
        }

        /// <inheritdoc />
        protected override Func<IRegistrationProfileController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => Globals.DependencyProvider.GetRequiredService<IRegistrationProfileController>();
        }

        private static void AddProperty(List<string> results, string field, string searchTerm)
        {
            if (field.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant().Trim()))
            {
                results.Add(field);
            }
        }
    }
}
