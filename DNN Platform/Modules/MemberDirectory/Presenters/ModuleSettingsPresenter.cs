// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.MemberDirectory.Presenters
{
    using System.Collections.Generic;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Modules.MemberDirectory.ViewModels;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Web.Mvp;

    /// <summary>The presenter for <see cref="Settings"/>.</summary>
    [DnnDeprecated(9, 2, 0, "Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
    public partial class ModuleSettingsPresenter : ModuleSettingsPresenter<ISettingsView<MemberDirectorySettingsModel>, MemberDirectorySettingsModel>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleSettingsPresenter));
        /// <summary>Initializes a new instance of the <see cref="ModuleSettingsPresenter"/> class.</summary>
        /// <param name="view">The settings view.</param>
        public ModuleSettingsPresenter(ISettingsView<MemberDirectorySettingsModel> view)
            : base(view)
        {
		Logger.LogDebug(">>> Entering ModuleSettingsPresenter(view={View})", view);
		Logger.LogDebug("<<< Exiting ModuleSettingsPresenter(view={View})", view);
        }

        /// <inheritdoc />
        protected override void OnLoad()
        {
			Logger.LogDebug(">>> Entering OnLoad()");
            base.OnLoad();

            this.View.Model.Groups = RoleController.Instance.GetRoles(this.PortalId, r => r.Status == RoleStatus.Approved);
            this.View.Model.Relationships = RelationshipController.Instance.GetRelationshipsByPortalId(this.PortalId);

            this.View.Model.ProfileProperties = new List<ProfilePropertyDefinition>();
            foreach (ProfilePropertyDefinition definition in ProfileController.GetPropertyDefinitionsByPortal(this.PortalId))
            {
                var controller = new ListController();
				Logger.LogDebug("OnLoad(): controller → {Controller}", controller);
                ListEntryInfo textType = controller.GetListEntryInfo("DataType", "Text");
				Logger.LogDebug("OnLoad(): textType → {TextType}", textType);
                ListEntryInfo regionType = controller.GetListEntryInfo("DataType", "Region");
				Logger.LogDebug("OnLoad(): regionType → {RegionType}", regionType);
                ListEntryInfo countryType = controller.GetListEntryInfo("DataType", "Country");
				Logger.LogDebug("OnLoad(): countryType → {CountryType}", countryType);
                if (definition.DataType == textType.EntryID || definition.DataType == regionType.EntryID || definition.DataType == countryType.EntryID)
                {
                    this.View.Model.ProfileProperties.Add(definition);
                }
            }
			Logger.LogDebug("<<< Exiting OnLoad()");
        }
    }
}
