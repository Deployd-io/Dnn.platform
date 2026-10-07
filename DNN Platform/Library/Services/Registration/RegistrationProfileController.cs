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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RegistrationProfileController));
        private readonly ListController listController = listController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ListController>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        private readonly IPortalGroupController portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();

        /// <summary>Initializes a new instance of the <see cref="RegistrationProfileController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public RegistrationProfileController()
            : this(null, null, null, null, null)
        {
		Logger.LogError(">>> Entering RegistrationProfileController()");
        Logger.LogWarning("<<< Exiting RegistrationProfileController()");
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public IEnumerable<string> Search(int portalId, string searchTerm)
        {
			Logger.LogError(">>> Entering Search(portalId={PortalId},searchTerm={SearchTerm})", portalId, searchTerm);
            ListEntryInfo imageType = this.listController.GetListEntryInfo("DataType", "Image");

			Logger.LogError("Search(portalId={PortalId},searchTerm={SearchTerm}): imageType → {ImageType}", portalId, searchTerm, imageType);
            List<string> results = [];
			Logger.LogError("Search(portalId={PortalId},searchTerm={SearchTerm}): results → {Results}", portalId, searchTerm, results);
            var properties = ProfileController.GetPropertyDefinitionsByPortal(this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, portalId);
			Logger.LogError("Search(portalId={PortalId},searchTerm={SearchTerm}): properties → {Properties}", portalId, searchTerm, properties);
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

			Logger.LogWarning("<<< Exiting Search(portalId={PortalId},searchTerm={SearchTerm})", portalId, searchTerm);
            return results;
        }

        /// <inheritdoc />
        protected override Func<IRegistrationProfileController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => Globals.DependencyProvider.GetRequiredService<IRegistrationProfileController>();
        }

        private static void AddProperty(List<string> results, string field, string searchTerm)
        {
			Logger.LogError(">>> Entering AddProperty(results={Results},field={Field},searchTerm={SearchTerm})", results, field, searchTerm);
            if (field.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant().Trim()))
            {
                results.Add(field);
            }
			Logger.LogWarning("<<< Exiting AddProperty(results={Results},field={Field},searchTerm={SearchTerm})", results, field, searchTerm);
        }
    }
}
