// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.SearchResults
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Search.Internals;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A settings control for the search results module.</summary>
    public partial class ResultsSettings : ModuleSettingsBase
    {
        private readonly IPortalController portalController;

        /// <summary>Initializes a new instance of the <see cref="ResultsSettings"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public ResultsSettings()
            : this(null)
        {
		Logger.LogDebug(">>> Entering ResultsSettings()");
		Logger.LogDebug("<<< Exiting ResultsSettings()");
		Logger.LogDebug(">>> Entering ResultsSettings(portalController={PortalController})", portalController);
		Logger.LogDebug("ResultsSettings(portalController={PortalController}): portalController → {PortalController1}", portalController, portalController);
        }

        /// <summary>Initializes a new instance of the <see cref="ResultsSettings"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        public ResultsSettings(IPortalController portalController)
        {
            this.portalController = portalController ?? this.DependencyProvider.GetRequiredService<IPortalController>();
			Logger.LogDebug("<<< Exiting ResultsSettings(portalController={PortalController})", portalController);
        }

        /// <inheritdoc />
        public override void LoadSettings()
        {
			Logger.LogDebug(">>> Entering LoadSettings()");
            try
            {
                if (this.Page.IsPostBack == false)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(this.Settings["LinkTarget"])))
                    {
                        this.comboBoxLinkTarget.SelectedValue = Convert.ToString(this.Settings["LinkTarget"]);
                    }

                    if (!string.IsNullOrEmpty(Convert.ToString(this.Settings["ScopeForPortals"])))
                    {
                        var list = Convert.ToString(this.Settings["ScopeForPortals"]).Split('|').ToList();
						Logger.LogDebug("LoadSettings(): list → {List}", list);
                        var portalList = this.LoadPortalsList().ToList();
						Logger.LogDebug("LoadSettings(): portalList → {PortalList}", portalList);
                        if (portalList.Any())
                        {
                            foreach (var portal in portalList)
                            {
                                var item = new ListItem(portal[0], portal[1]) { Selected = list.Contains(portal[1]) };
								Logger.LogDebug("LoadSettings(): item → {Item}", item);
                                this.comboBoxPortals.Items.Add(item);
                            }
                        }
                        else
                        {
                            this.divPortalGroup.Visible = false;
                        }
                    }
                    else
                    {
                        var portalList = this.LoadPortalsList().ToList();
                        if (portalList.Any())
                        {
                            foreach (var portal in portalList)
                            {
                                var item = new ListItem(portal[0], portal[1]) { Selected = this.PortalId.ToString() == portal[1] };
                                this.comboBoxPortals.Items.Add(item);
                            }
                        }
                        else
                        {
                            this.divPortalGroup.Visible = false;
                        }
                    }

                    if (!string.IsNullOrEmpty(Convert.ToString(this.Settings["ScopeForFilters"])))
                    {
                        var list = Convert.ToString(this.Settings["ScopeForFilters"]).Split('|').ToList();
                        var filterList = this.LoadSeachContentSourcesList();
						Logger.LogDebug("LoadSettings(): filterList → {FilterList}", filterList);
                        foreach (var filter in filterList)
                        {
                            var item = new ListItem(filter, filter) { Selected = list.Contains(filter) };
                            this.comboBoxFilters.Items.Add(item);
                        }
                    }
                    else
                    {
                        var filterList = this.LoadSeachContentSourcesList();
                        foreach (var filter in filterList)
                        {
                            var item = new ListItem(filter, filter) { Selected = true };
                            this.comboBoxFilters.Items.Add(item);
                        }
                    }

                    var scopeForRoles =
                        PortalController.GetPortalSetting(this.portalController, "SearchResult_ScopeForRoles", this.PortalId, string.Empty)
                        .Split([','], StringSplitOptions.RemoveEmptyEntries);
                    var roles = RoleController.Instance.GetRoles(this.PortalId, r => !r.IsSystemRole || r.RoleName == "Registered Users");
					Logger.LogDebug("LoadSettings(): roles → {Roles}", roles);
                    roles.Insert(0, new RoleInfo() { RoleName = "Superusers" });

                    foreach (var role in roles)
                    {
                        var item = new ListItem(role.RoleName, role.RoleName) { Selected = scopeForRoles.Length == 0 || scopeForRoles.Contains(role.RoleName) };
                        this.comboBoxRoles.Items.Add(item);
                    }

                    this.chkEnableWildSearch.Checked = this.GetBooleanSetting("EnableWildSearch", true);
                    this.chkShowDescription.Checked = this.GetBooleanSetting("ShowDescription", true);
                    this.chkShowFriendlyTitle.Checked = this.GetBooleanSetting("ShowFriendlyTitle", true);
                    this.chkShowSnippet.Checked = this.GetBooleanSetting("ShowSnippet", true);
                    this.chkShowLastUpdated.Checked = this.GetBooleanSetting("ShowLastUpdated", true);
                    this.chkShowSource.Checked = this.GetBooleanSetting("ShowSource", true);
                    this.chkShowTags.Checked = this.GetBooleanSetting("ShowTags", true);

                    this.txtMaxDescriptionLength.Text = this.GetStringSetting("MaxDescriptionLength", "100");
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in LoadSettings(): {Arg0}", exc.getMessage());
                // Module failed to load
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting LoadSettings()");
        }

        /// <inheritdoc />
        public override void UpdateSettings()
        {
			Logger.LogDebug(">>> Entering UpdateSettings()");
            try
            {
                if (this.Page.IsValid)
                {
                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "LinkTarget", this.comboBoxLinkTarget.SelectedValue);

                    var selectedPortals = this.comboBoxPortals.Value.Replace(",", "|");
					Logger.LogDebug("UpdateSettings(): selectedPortals → {SelectedPortals}", selectedPortals);

                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ScopeForPortals", selectedPortals);

                    var selectedFilters = this.comboBoxFilters.Value.Replace(",", "|");
					Logger.LogDebug("UpdateSettings(): selectedFilters → {SelectedFilters}", selectedFilters);

                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ScopeForFilters", selectedFilters.ToString());

                    var selectedRoles = this.comboBoxRoles.Value;
                    PortalController.UpdatePortalSetting(this.portalController, this.PortalId, "SearchResult_ScopeForRoles", selectedRoles);

                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "EnableWildSearch", this.chkEnableWildSearch.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ShowDescription", this.chkShowDescription.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ShowFriendlyTitle", this.chkShowFriendlyTitle.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ShowSnippet", this.chkShowSnippet.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ShowLastUpdated", this.chkShowLastUpdated.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ShowSource", this.chkShowSource.Checked.ToString());
                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ShowTags", this.chkShowTags.Checked.ToString());

                    var maxDescriptionLength = this.txtMaxDescriptionLength.Text;
					Logger.LogDebug("UpdateSettings(): maxDescriptionLength → {MaxDescriptionLength}", maxDescriptionLength);
                    if (string.IsNullOrEmpty(maxDescriptionLength) || !Regex.IsMatch(maxDescriptionLength, "^\\d+$"))
                    {
                        maxDescriptionLength = "100";
                    }

                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "MaxDescriptionLength", maxDescriptionLength);
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in UpdateSettings(): {Arg0}", exc.getMessage());
                Exceptions.ProcessModuleLoadException(this, exc);
            }

            DataCache.RemoveCache(string.Format("ModuleInfos{0}", this.PortalSettings.PortalId));
			Logger.LogDebug("<<< Exiting UpdateSettings()");
        }

        protected IEnumerable<string[]> LoadPortalsList()
        {
			Logger.LogDebug(">>> Entering LoadPortalsList()");
            var groups = PortalGroupController.Instance.GetPortalGroups().ToArray();
            var myGroup = (
                from @group in groups
                select PortalGroupController.Instance.GetPortalsByGroup(@group.PortalGroupId) into portals
                where portals.Any((IPortalInfo x) => x.PortalId == PortalSettings.Current.PortalId)
                select portals.ToArray())
                .FirstOrDefault();
				Logger.LogDebug("LoadPortalsList(): groups → {Groups}", groups);

            var result = new List<string[]>();
			Logger.LogDebug("LoadPortalsList(): result → {Result}", result);
            if (myGroup != null && myGroup.Any())
            {
				private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ResultsSettings.ascx));
                result.AddRange(myGroup.Select(
                    (IPortalInfo pi) => new[] { pi.PortalName, pi.PortalId.ToString(CultureInfo.InvariantCulture) }));
            }

			Logger.LogDebug("<<< Exiting LoadPortalsList()");
            return result;
        }

        protected IEnumerable<string> LoadSeachContentSourcesList()
        {
			Logger.LogDebug(">>> Entering LoadSeachContentSourcesList()");
            var portals = PortalController.Instance.GetPortals();

			Logger.LogDebug("LoadSeachContentSourcesList(): portals → {Portals}", portals);
            var result = new List<string>();
			Logger.LogDebug("LoadSeachContentSourcesList(): result → {Result}", result);
            foreach (var portal in portals)
            {
                var pi = portal as IPortalInfo;
                if (pi != null)
                {
                    var list = InternalSearchController.Instance.GetSearchContentSourceList(pi.PortalId);
					Logger.LogDebug("LoadSeachContentSourcesList(): list → {List}", list);
                    foreach (var src in list)
                    {
                        if (!src.IsPrivate && !result.Contains(src.LocalizedName))
                        {
                            result.Add(src.LocalizedName);
                        }
                    }
                }
            }

			Logger.LogDebug("<<< Exiting LoadSeachContentSourcesList()");
            return result;
        }

        private bool GetBooleanSetting(string settingName, bool defaultValue)
        {
			Logger.LogDebug(">>> Entering GetBooleanSetting(settingName={SettingName},defaultValue={DefaultValue})", settingName, defaultValue);
            if (!string.IsNullOrEmpty(Convert.ToString(this.Settings[settingName])))
            {
			Logger.LogDebug("<<< Exiting GetBooleanSetting(settingName={SettingName},defaultValue={DefaultValue})", settingName, defaultValue);
                return Convert.ToBoolean(this.Settings[settingName]);
            }

            return defaultValue;
        }

        private string GetStringSetting(string settingName, string defaultValue)
        {
			Logger.LogDebug(">>> Entering GetStringSetting(settingName={SettingName},defaultValue={DefaultValue})", settingName, defaultValue);
            if (!string.IsNullOrEmpty(Convert.ToString(this.Settings[settingName])))
            {
			Logger.LogDebug("<<< Exiting GetStringSetting(settingName={SettingName},defaultValue={DefaultValue})", settingName, defaultValue);
                return Convert.ToString(this.Settings[settingName]);
            }

            return defaultValue;
        }
    }
}
