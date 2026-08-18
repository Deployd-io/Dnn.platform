// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.Console
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI.WebControls;

    using Dnn.Modules.Console.Components;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Common;
    using DotNetNuke.Web.UI.WebControls.Internal;

    /// <summary>Loads and saves the module settings.</summary>
    public partial class Settings : ModuleSettingsBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Settings));

        /// <inheritdoc />
        public override void LoadSettings()
        {
			Logger.LogDebug(">>> Entering LoadSettings()");
            try
            {
                if (this.Page.IsPostBack == false)
                {
                    if (this.Settings.ContainsKey("ParentTabId") && !string.IsNullOrEmpty(Convert.ToString(this.Settings["ParentTabId"])))
                    {
                        var tabId = Convert.ToInt32(this.Settings["ParentTabId"]);
						Logger.LogDebug("LoadSettings(): tabId → {TabId}", tabId);
                        this.ParentTab.SelectedPage = TabController.Instance.GetTab(tabId, this.PortalId);
                    }

                    foreach (string val in ConsoleController.GetSizeValues())
                    {
                        this.DefaultSize.AddItem(Localization.GetString(val, this.LocalResourceFile), val);
                    }

                    this.SelectDropDownListItem(ref this.DefaultSize, "DefaultSize");

                    this.SelectDropDownListItem(ref this.modeList, "Mode");

                    if (this.Settings.ContainsKey("AllowSizeChange"))
                    {
                        this.AllowResize.Checked = Convert.ToBoolean(this.Settings["AllowSizeChange"]);
                    }

                    foreach (var val in ConsoleController.GetViewValues())
                    {
                        this.DefaultView.AddItem(Localization.GetString(val, this.LocalResourceFile), val);
                    }

                    this.SelectDropDownListItem(ref this.DefaultView, "DefaultView");
                    if (this.Settings.ContainsKey("IncludeParent"))
                    {
                        this.IncludeParent.Checked = Convert.ToBoolean(this.Settings["IncludeParent"]);
                    }

                    if (this.Settings.ContainsKey("AllowViewChange"))
                    {
                        this.AllowViewChange.Checked = Convert.ToBoolean(this.Settings["AllowViewChange"]);
                    }

                    if (this.Settings.ContainsKey("ShowTooltip"))
                    {
                        this.ShowTooltip.Checked = Convert.ToBoolean(this.Settings["ShowTooltip"]);
                    }

                    if (this.Settings.ContainsKey("OrderTabsByHierarchy"))
                    {
                        this.OrderTabsByHierarchy.Checked = Convert.ToBoolean(this.Settings["OrderTabsByHierarchy"]);
                    }

                    if (this.Settings.ContainsKey("IncludeHiddenPages"))
                    {
                        this.IncludeHiddenPages.Checked = Convert.ToBoolean(this.Settings["IncludeHiddenPages"]);
                    }

                    if (this.Settings.ContainsKey("ConsoleWidth"))
                    {
                        this.ConsoleWidth.Text = Convert.ToString(this.Settings["ConsoleWidth"]);
                    }

                    this.SwitchMode();
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in LoadSettings(): {Arg0}", ex.getMessage());
                Exceptions.ProcessModuleLoadException(this, ex);
            }
			Logger.LogDebug("<<< Exiting LoadSettings()");
        }

        /// <inheritdoc />
        public override void UpdateSettings()
        {
			Logger.LogDebug(">>> Entering UpdateSettings()");
            try
            {
                // validate console width value
                var width = string.Empty;
				Logger.LogDebug("UpdateSettings(): width → {Width}", width);
                if (this.ConsoleWidth.Text.Trim().Length > 0)
                {
                    try
                    {
                        width = Unit.Parse(this.ConsoleWidth.Text.Trim()).ToString();
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);

                        throw new Exception("ConsoleWidth value is invalid. Value must be numeric.");
                    }
                }

                if (this.ParentTab.SelectedItemValueAsInt == Null.NullInteger)
                {
                    ModuleController.Instance.DeleteModuleSetting(this.ModuleId, "ParentTabID");
                }
                else
                {
                    ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ParentTabID", this.ParentTab.SelectedItem.Value);
                }

                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "Mode", this.modeList.SelectedValue);
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "DefaultSize", this.DefaultSize.SelectedValue);
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "AllowSizeChange", this.AllowResize.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "DefaultView", this.DefaultView.SelectedValue);
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "AllowViewChange", this.AllowViewChange.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ShowTooltip", this.ShowTooltip.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "OrderTabsByHierarchy", this.OrderTabsByHierarchy.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "IncludeHiddenPages", this.IncludeHiddenPages.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "IncludeParent", this.IncludeParent.Checked.ToString(CultureInfo.InvariantCulture));
                ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "ConsoleWidth", width);

                foreach (RepeaterItem item in this.tabs.Items)
                {
                    if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
                    {
                        var tabPath = (item.FindControl("tabPath") as HiddenField).Value;
						Logger.LogDebug("UpdateSettings(): tabPath → {TabPath}", tabPath);
                        var visibility = (item.FindControl("tabVisibility") as DnnComboBox).SelectedValue;
						Logger.LogDebug("UpdateSettings(): visibility → {Visibility}", visibility);

                        var key = string.Format("TabVisibility{0}", tabPath.Replace("//", "-"));
						Logger.LogWarning("UpdateSettings(): key → {Key}", key);
                        ModuleController.Instance.UpdateModuleSetting(this.ModuleId, key, visibility);
                    }
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in UpdateSettings(): {Arg0}", exc.getMessage());
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting UpdateSettings()");
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.tabs.ItemDataBound += this.TabsItemDataBound;
            this.modeList.SelectedIndexChanged += this.ModeListSelectedIndexChanged;

            this.ParentTab.UndefinedItem = new ListItem(DynamicSharedConstants.Unspecified, string.Empty);
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        private void BindTabs(int tabId, bool includeParent)
        {
			Logger.LogDebug(">>> Entering BindTabs(tabId={TabId},includeParent={IncludeParent})", tabId, includeParent);
            List<TabInfo> tempTabs = TabController.GetTabsBySortOrder(this.PortalId).OrderBy(t => t.Level).ThenBy(t => t.HasChildren).ToList();

			Logger.LogDebug("BindTabs(tabId={TabId},includeParent={IncludeParent}): tempTabs → {TempTabs}", tabId, includeParent, tempTabs);
            IList<TabInfo> tabList = new List<TabInfo>();
			Logger.LogDebug("BindTabs(tabId={TabId},includeParent={IncludeParent}): tabList → {TabList}", tabId, includeParent, tabList);

            IList<int> tabIdList = new List<int>();
			Logger.LogDebug("BindTabs(tabId={TabId},includeParent={IncludeParent}): tabIdList → {TabIdList}", tabId, includeParent, tabIdList);
            tabIdList.Add(tabId);

            if (includeParent)
            {
                TabInfo consoleTab = TabController.Instance.GetTab(tabId, this.PortalId);
				Logger.LogDebug("BindTabs(tabId={TabId},includeParent={IncludeParent}): consoleTab → {ConsoleTab}", tabId, includeParent, consoleTab);
                if (consoleTab != null)
                {
                    tabList.Add(consoleTab);
                }
            }

            foreach (TabInfo tab in tempTabs)
            {
                bool canShowTab = TabPermissionController.CanViewPage(tab) &&
                                  !tab.IsDeleted &&
                                  (tab.StartDate < DateTime.Now || tab.StartDate == Null.NullDate);

                if (!canShowTab)
                {
                    continue;
                }

                if (tabIdList.Contains(tab.ParentId))
                {
                    if (!tabIdList.Contains(tab.TabID))
                    {
                        tabIdList.Add(tab.TabID);
                    }

                    tabList.Add(tab);
                }
            }

            this.tabs.DataSource = tabList;
            this.tabs.DataBind();
			Logger.LogDebug("<<< Exiting BindTabs(tabId={TabId},includeParent={IncludeParent})", tabId, includeParent);
        }

        private void SwitchMode()
        {
			Logger.LogDebug(">>> Entering SwitchMode()");
            int parentTabId = -1;
            if (this.Settings.ContainsKey("ParentTabID") && !string.IsNullOrEmpty(Convert.ToString(this.Settings["ParentTabID"])))
			Logger.LogDebug("SwitchMode(): parentTabId → {ParentTabId}", parentTabId);
            {
                parentTabId = Convert.ToInt32(this.Settings["ParentTabID"]);
            }

            switch (this.modeList.SelectedValue)
            {
                case "Normal":
                    this.parentTabRow.Visible = true;
                    this.includeParentRow.Visible = true;
                    this.tabVisibilityRow.Visible = false;
                    break;
                case "Profile":
                    this.parentTabRow.Visible = false;
                    this.includeParentRow.Visible = false;
                    this.tabVisibilityRow.Visible = true;
                    parentTabId = this.PortalSettings.UserTabId;
                    break;
                case "Group":
                    this.parentTabRow.Visible = true;
                    this.includeParentRow.Visible = true;
                    this.tabVisibilityRow.Visible = true;
                    break;
            }

            this.ParentTab.SelectedPage = TabController.Instance.GetTab(parentTabId, this.PortalId);
            this.BindTabs(parentTabId, this.IncludeParent.Checked);
			Logger.LogDebug("<<< Exiting SwitchMode()");
        }

        private void ModeListSelectedIndexChanged(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering ModeListSelectedIndexChanged(sender={Sender},e={E})", sender, e);
            this.SwitchMode();
			Logger.LogDebug("<<< Exiting ModeListSelectedIndexChanged(sender={Sender},e={E})", sender, e);
        }

        private void TabsItemDataBound(object sender, RepeaterItemEventArgs e)
        {
			Logger.LogDebug(">>> Entering TabsItemDataBound(sender={Sender},e={E})", sender, e);
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var tab = (TabInfo)e.Item.DataItem;
				Logger.LogDebug("TabsItemDataBound(sender={Sender},e={E}): tab → {Tab}", sender, e, tab);
                DnnComboBox visibilityDropDown = (DnnComboBox)e.Item.FindControl("tabVisibility");
				Logger.LogDebug("TabsItemDataBound(sender={Sender},e={E}): visibilityDropDown → {VisibilityDropDown}", sender, e, visibilityDropDown);

                var tabLabel = (Label)e.Item.FindControl("tabLabel");
				Logger.LogDebug("TabsItemDataBound(sender={Sender},e={E}): tabLabel → {TabLabel}", sender, e, tabLabel);
                var tabPathField = (HiddenField)e.Item.FindControl("tabPath");
				Logger.LogDebug("TabsItemDataBound(sender={Sender},e={E}): tabPathField → {TabPathField}", sender, e, tabPathField);

                visibilityDropDown.Items.Clear();

                visibilityDropDown.AddItem(this.LocalizeText("AllUsers"), "AllUsers");
                if (this.modeList.SelectedValue == "Profile")
                {
                    visibilityDropDown.AddItem(this.LocalizeText("Friends"), "Friends");
                    visibilityDropDown.AddItem(this.LocalizeText("User"), "User");
                }
                else
                {
                    visibilityDropDown.AddItem(this.LocalizeText("Owner"), "Owner");
                    visibilityDropDown.AddItem(this.LocalizeText("Members"), "Members");
                }

                tabLabel.Text = tab.TabName;
				Logger.LogDebug("TabsItemDataBound(sender={Sender},e={E}): tabLabel.Text → {Arg2}", sender, e, tabLabel.Text);
				Logger.LogDebug("TabsItemDataBound(sender={Sender},e={E}): tabPathField.Value → {Arg2}", sender, e, tabPathField.Value);
                tabPathField.Value = tab.TabPath;

                var key = string.Format("TabVisibility{0}", tab.TabPath.Replace("//", "-"));
				Logger.LogWarning("TabsItemDataBound(sender={Sender},e={E}): key → {Key}", sender, e, key);
                this.SelectDropDownListItem(ref visibilityDropDown, key);
            }
			Logger.LogDebug("<<< Exiting TabsItemDataBound(sender={Sender},e={E})", sender, e);
        }

        private void SelectDropDownListItem(ref DnnComboBox ddl, string key)
        {
			Logger.LogDebug(">>> Entering SelectDropDownListItem(ddl={Ddl},key={Key})", ddl, key);
            if (this.Settings.ContainsKey(key))
            {
                ddl.ClearSelection();
                var selItem = ddl.FindItemByValue(Convert.ToString(this.Settings[key]));
				Logger.LogDebug("SelectDropDownListItem(ddl={Ddl},key={Key}): selItem → {SelItem}", ddl, key, selItem);
                if (selItem != null)
                {
                    selItem.Selected = true;
					Logger.LogDebug("SelectDropDownListItem(ddl={Ddl},key={Key}): selItem.Selected → {Arg2}", ddl, key, selItem.Selected);
                }
            }
			Logger.LogDebug("<<< Exiting SelectDropDownListItem(ddl={Ddl},key={Key})", ddl, key);
        }
    }
}
