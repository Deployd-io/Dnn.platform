// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Admin.Users
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.UI.WebControls;
    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>The ProfileDefinitions PortalModuleBase is used to manage the Profile Properties for a portal.</summary>
    public partial class ProfileDefinitions : PortalModuleBase, IActionable
    {
        private const int COLUMNREQUIRED = 11;
        private const int COLUMNVISIBLE = 12;
        private const int COLUMNMOVEDOWN = 2;
        private const int COLUMNMOVEUP = 3;

        private readonly INavigationManager navigationManager;
        private readonly IHostSettings hostSettings;
        private readonly IEventLogger eventLogger;
        private readonly IPortalController portalController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IPortalGroupController portalGroupController;
        private readonly ListController listController;

        private ProfilePropertyDefinitionCollection profileProperties;
        private bool requiredColumnHidden;

        /// <summary>Initializes a new instance of the <see cref="ProfileDefinitions"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public ProfileDefinitions()
            : this(null, null, null, null, null, null, null)
        {
		eventLogger.LogDebug(">>> Entering ProfileDefinitions()");
        }

		eventLogger.LogDebug("<<< Exiting ProfileDefinitions()");
        /// <summary>Initializes a new instance of the <see cref="ProfileDefinitions"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="hostSettings">The host settings.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public ProfileDefinitions(INavigationManager navigationManager, IHostSettings hostSettings)
            : this(navigationManager, hostSettings, null, null, null, null, null)
        {
		eventLogger.LogDebug(">>> Entering ProfileDefinitions(navigationManager={NavigationManager},hostSettings={HostSettings})", navigationManager, hostSettings);
		eventLogger.LogDebug("<<< Exiting ProfileDefinitions(navigationManager={NavigationManager},hostSettings={HostSettings})", navigationManager, hostSettings);
		eventLogger.LogDebug("ProfileDefinitions(navigationManager={NavigationManager},hostSettings={HostSettings},eventLogger={EventLogger},listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): hostSettings → {HostSettings1}", navigationManager, hostSettings, eventLogger, listController, portalController, appStatus, portalGroupController, hostSettings);
		eventLogger.LogDebug("ProfileDefinitions(navigationManager={NavigationManager},hostSettings={HostSettings},eventLogger={EventLogger},listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): eventLogger → {EventLogger1}", navigationManager, hostSettings, eventLogger, listController, portalController, appStatus, portalGroupController, eventLogger);
		eventLogger.LogDebug("ProfileDefinitions(navigationManager={NavigationManager},hostSettings={HostSettings},eventLogger={EventLogger},listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): listController → {ListController1}", navigationManager, hostSettings, eventLogger, listController, portalController, appStatus, portalGroupController, listController);
		eventLogger.LogDebug("ProfileDefinitions(navigationManager={NavigationManager},hostSettings={HostSettings},eventLogger={EventLogger},listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): portalController → {PortalController1}", navigationManager, hostSettings, eventLogger, listController, portalController, appStatus, portalGroupController, portalController);
		eventLogger.LogDebug("ProfileDefinitions(navigationManager={NavigationManager},hostSettings={HostSettings},eventLogger={EventLogger},listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): appStatus → {AppStatus1}", navigationManager, hostSettings, eventLogger, listController, portalController, appStatus, portalGroupController, appStatus);
		eventLogger.LogDebug("ProfileDefinitions(navigationManager={NavigationManager},hostSettings={HostSettings},eventLogger={EventLogger},listController={ListController},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): portalGroupController → {PortalGroupController1}", navigationManager, hostSettings, eventLogger, listController, portalController, appStatus, portalGroupController, portalGroupController);
        }

        /// <summary>Initializes a new instance of the <see cref="ProfileDefinitions"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="eventLogger">The event logger.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public ProfileDefinitions(INavigationManager navigationManager, IHostSettings hostSettings, IEventLogger eventLogger)
            : this(navigationManager, hostSettings, eventLogger, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ProfileDefinitions"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="listController">The list controller.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        public ProfileDefinitions(INavigationManager navigationManager, IHostSettings hostSettings, IEventLogger eventLogger, ListController listController, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController)
        {
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
            this.hostSettings = hostSettings ?? this.DependencyProvider.GetRequiredService<IHostSettings>();
            this.eventLogger = eventLogger ?? this.DependencyProvider.GetRequiredService<IEventLogger>();
            this.listController = listController ?? this.DependencyProvider.GetRequiredService<ListController>();
            this.portalController = portalController ?? this.DependencyProvider.GetRequiredService<IPortalController>();
            this.appStatus = appStatus ?? this.DependencyProvider.GetRequiredService<IApplicationStatusInfo>();
            this.portalGroupController = portalGroupController ?? this.DependencyProvider.GetRequiredService<IPortalGroupController>();
        }

        /// <summary>Gets the Return Url for the page.</summary>
        public string ReturnUrl
        {
            get
            {
                string returnURL;
                var filterParams = new string[string.IsNullOrEmpty(this.Request.QueryString["filterproperty"]) ? 1 : 2];

                if (string.IsNullOrEmpty(this.Request.QueryString["filterProperty"]))
                {
                    filterParams.SetValue("filter=" + this.Request.QueryString["filter"], 0);
                }
                else
                {
                    filterParams.SetValue("filter=" + this.Request.QueryString["filter"], 0);
                    filterParams.SetValue("filterProperty=" + this.Request.QueryString["filterProperty"], 1);
                }

                if (string.IsNullOrEmpty(this.Request.QueryString["filter"]))
                {
                    returnURL = this.navigationManager.NavigateURL(this.TabId);
                }
                else
                {
                    returnURL = this.navigationManager.NavigateURL(this.TabId, string.Empty, filterParams);
                }

                return returnURL;
            }
        }

        /// <inheritdoc />
        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();
                actions.Add(
                    this.GetNextActionID(),
                    Localization.GetString(ModuleActionType.AddContent, this.LocalResourceFile),
                    ModuleActionType.AddContent,
                    string.Empty,
                    "add.gif",
                    this.EditUrl("EditProfileProperty"),
                    false,
                    SecurityAccessLevel.Admin,
                    true,
                    false);
                actions.Add(
                    this.GetNextActionID(),
                    Localization.GetString("Cancel.Action", this.LocalResourceFile),
                    ModuleActionType.AddContent,
                    string.Empty,
                    "lt.gif",
                    this.ReturnUrl,
                    false,
                    SecurityAccessLevel.Admin,
                    true,
                    false);
                return actions;
            }
        }

        /// <summary>Gets a value indicating whether we are dealing with SuperUsers.</summary>
        protected bool IsSuperUser
        {
            get
            {
                return Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID);
            }
        }

        /// <summary>Gets the collection of Profile Properties.</summary>
        protected ProfilePropertyDefinitionCollection ProfileProperties
            => this.profileProperties ??= ProfileController.GetPropertyDefinitionsByPortal(this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, this.UsersPortalId, false, false);

        /// <summary>Gets the Portal ID whose Users we are managing.</summary>
        protected int UsersPortalId
        {
            get
            {
                int intPortalId = this.PortalId;
                if (this.IsSuperUser)
                {
                    intPortalId = Null.NullInteger;
                }

                return intPortalId;
            }
        }

        public string DisplayDataType(ProfilePropertyDefinition definition)
        {
			eventLogger.LogDebug(">>> Entering DisplayDataType(definition={Definition})", definition);
            string retValue = Null.NullString;
            ListEntryInfo definitionEntry = this.listController.GetListEntryInfo("DataType", definition.DataType);
			eventLogger.LogDebug("DisplayDataType(definition={Definition}): definitionEntry → {DefinitionEntry}", definition, definitionEntry);
			eventLogger.LogDebug("DisplayDataType(definition={Definition}): retValue → {RetValue}", definition, retValue);
            if (definitionEntry != null)
            {
                retValue = definitionEntry.Value;
            }

			eventLogger.LogDebug("<<< Exiting DisplayDataType(definition={Definition})", definition);
            return retValue;
        }

        public string DisplayDefaultVisibility(ProfilePropertyDefinition definition)
        {
			eventLogger.LogDebug(">>> Entering DisplayDefaultVisibility(definition={Definition})", definition);
            string retValue = Null.NullString;
            if (!string.IsNullOrEmpty(definition.DefaultVisibility.ToString()))
            {
                retValue = this.LocalizeText(definition.DefaultVisibility.ToString()) ?? definition.DefaultVisibility.ToString();
            }

			eventLogger.LogDebug("<<< Exiting DisplayDefaultVisibility(definition={Definition})", definition);
            return retValue;
        }

        public void Update()
        {
			eventLogger.LogDebug(">>> Entering Update()");
            try
            {
                this.UpdateProperties();

                // Redirect to upadte page
                this.Response.Redirect(this.Request.RawUrl, true);
            }
            catch (Exception exc)
            {
				eventLogger.LogError("Exception in Update(): {Arg0}", exc.getMessage());
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			eventLogger.LogDebug("<<< Exiting Update()");
        }

        /// <inheritdoc />
        protected override void LoadViewState(object savedState)
        {
			eventLogger.LogDebug(">>> Entering LoadViewState(savedState={SavedState})", savedState);
            if (savedState != null)
            {
                // Load State from the array of objects that was saved with SaveViewState.
                var myState = (object[])savedState;
				eventLogger.LogDebug("LoadViewState(savedState={SavedState}): myState → {MyState}", savedState, myState);

                // Load Base Controls ViewState
                if (myState[0] != null)
                {
                    base.LoadViewState(myState[0]);
                }

                // Load ModuleID
                if (myState[1] != null)
                {
                    this.profileProperties = (ProfilePropertyDefinitionCollection)myState[1];
					eventLogger.LogDebug("LoadViewState(savedState={SavedState}): profileProperties → {ProfileProperties}", savedState, profileProperties);
                }
            }
			eventLogger.LogDebug("<<< Exiting LoadViewState(savedState={SavedState})", savedState);
        }

        /// <inheritdoc />
        protected override object SaveViewState()
        {
			eventLogger.LogDebug(">>> Entering SaveViewState()");
            var allStates = new object[2];

            // Save the Base Controls ViewState
            allStates[0] = base.SaveViewState();
            allStates[1] = this.ProfileProperties;

			eventLogger.LogDebug("<<< Exiting SaveViewState()");
            return allStates;
        }

        /// <summary>Page_Init runs when the control is initialised.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInit(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            foreach (DataGridColumn column in this.grdProfileProperties.Columns)
            {
                if (ReferenceEquals(column.GetType(), typeof(CheckBoxColumn)))
                {
                    var checkBoxColumn = (CheckBoxColumn)column;
					eventLogger.LogDebug("OnInit(e={E}): checkBoxColumn → {CheckBoxColumn}", e, checkBoxColumn);
                    if (checkBoxColumn.DataField == "Required" && this.UsersPortalId == Null.NullInteger)
                    {
                        checkBoxColumn.Visible = false;
						eventLogger.LogDebug("OnInit(e={E}): checkBoxColumn.Visible → {Arg1}", e, checkBoxColumn.Visible);
                        this.requiredColumnHidden = true;
						eventLogger.LogDebug("OnInit(e={E}): this.requiredColumnHidden → {Arg1}", e, this.requiredColumnHidden);
                    }

                    if (this.SupportsRichClient() == false)
                    {
                        checkBoxColumn.CheckedChanged += this.GrdProfileProperties_ItemCheckedChanged;
                    }
                }
                else if (ReferenceEquals(column.GetType(), typeof(ImageCommandColumn)))
                {
                    // Manage Delete Confirm JS
                    var imageColumn = (ImageCommandColumn)column;
					eventLogger.LogDebug("OnInit(e={E}): imageColumn → {ImageColumn}", e, imageColumn);
                    switch (imageColumn.CommandName)
                    {
                        case "Delete":
                            imageColumn.OnClickJS = Localization.GetString("DeleteItem");
                            imageColumn.Text = Localization.GetString("Delete", this.LocalResourceFile);
                            break;
                        case "Edit":
                            // The Friendly URL parser does not like non-alphanumeric characters
                            // so first create the format string with a fake value and then
                            // replace the fake value with the FormatString place holder
                            string formatString = this.EditUrl("PropertyDefinitionID", "KEYFIELD", "EditProfileProperty");
							eventLogger.LogDebug("OnInit(e={E}): formatString → {FormatString}", e, formatString);
                            formatString = formatString.Replace("KEYFIELD", "{0}");
                            imageColumn.NavigateURLFormatString = formatString;
							eventLogger.LogDebug("OnInit(e={E}): imageColumn.NavigateURLFormatString → {Arg1}", e, imageColumn.NavigateURLFormatString);
                            imageColumn.Text = Localization.GetString("Edit", this.LocalResourceFile);
                            break;
                        case "MoveUp":
                            imageColumn.Text = Localization.GetString("MoveUp", this.LocalResourceFile);
                            break;
                        case "MoveDown":
                            imageColumn.Text = Localization.GetString("MoveDown", this.LocalResourceFile);
                            break;
                    }
                }
            }
			eventLogger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.cmdRefresh.Click += this.CmdRefresh_Click;
            this.grdProfileProperties.ItemCommand += this.GrdProfileProperties_ItemCommand;
            this.grdProfileProperties.ItemCreated += this.GrdProfileProperties_ItemCreated;
            this.grdProfileProperties.ItemDataBound += this.grdProfileProperties_ItemDataBound;

            this.cmdAdd.NavigateUrl = this.EditUrl("EditProfileProperty");

            try
            {
                if (!this.Page.IsPostBack)
                {
                    Localization.LocalizeDataGrid(ref this.grdProfileProperties, this.LocalResourceFile);
                    this.BindGrid();
                }
            }
            catch (Exception exc)
            {
				eventLogger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.getMessage());
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			eventLogger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        /// <summary>grdProfileProperties_ItemDataBound runs when a row in the grid is bound to its data source Grid.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        protected void grdProfileProperties_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
			eventLogger.LogDebug(">>> Entering grdProfileProperties_ItemDataBound(sender={Sender},e={E})", sender, e);
            DataGridItem item = e.Item;
            if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.SelectedItem)
            {
                Control imgColumnControl = item.Controls[1].Controls[0];
                if (imgColumnControl is ImageButton)
                {
                    var delImage = (ImageButton)imgColumnControl;
					eventLogger.LogDebug("grdProfileProperties_ItemDataBound(sender={Sender},e={E}): delImage → {DelImage}", sender, e, delImage);
                    var profProperty = (ProfilePropertyDefinition)item.DataItem;
					eventLogger.LogDebug("grdProfileProperties_ItemDataBound(sender={Sender},e={E}): profProperty → {ProfProperty}", sender, e, profProperty);

                    switch (profProperty.PropertyName.ToLowerInvariant())
                    {
                        case "lastname":
                        case "firstname":
                        case "preferredtimezone":
                        case "preferredlocale":
                            delImage.Visible = false;
							eventLogger.LogDebug("grdProfileProperties_ItemDataBound(sender={Sender},e={E}): delImage.Visible → {Arg2}", sender, e, delImage.Visible);
                            break;
                        default:
                            delImage.Visible = true;
                            break;
                    }
                }
            }
			eventLogger.LogDebug("<<< Exiting grdProfileProperties_ItemDataBound(sender={Sender},e={E})", sender, e);
        }

        /// <summary>Helper function that determines whether the client-side functionality is possible.</summary>
        private bool SupportsRichClient()
        {
			eventLogger.LogDebug(">>> Entering SupportsRichClient()");
			eventLogger.LogDebug("<<< Exiting SupportsRichClient()");
            return ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.DHTML);
        }

        /// <summary>Deletes a property.</summary>
        /// <param name="index">The index of the Property to delete.</param>
        private void DeleteProperty(int index)
        {
			eventLogger.LogDebug(">>> Entering DeleteProperty(index={Index})", index);
            ProfileController.DeletePropertyDefinition(this.eventLogger, this.portalController, this.appStatus, this.portalGroupController, this.ProfileProperties[index]);

            this.RefreshGrid();
			eventLogger.LogDebug("<<< Exiting DeleteProperty(index={Index})", index);
        }

        /// <summary>Moves a property.</summary>
        /// <param name="index">The index of the Property to move.</param>
        /// <param name="destIndex">The new index of the Property.</param>
        private void MoveProperty(int index, int destIndex)
        {
			eventLogger.LogDebug(">>> Entering MoveProperty(index={Index},destIndex={DestIndex})", index, destIndex);
            ProfilePropertyDefinition profileProperty = this.ProfileProperties[index];
            ProfilePropertyDefinition nextProfileProperty = this.ProfileProperties[destIndex];

            int currentOrder = profileProperty.ViewOrder;
            int nextOrder = nextProfileProperty.ViewOrder;

            // Swap ViewOrders
            profileProperty.ViewOrder = nextOrder;
			eventLogger.LogDebug("MoveProperty(index={Index},destIndex={DestIndex}): profileProperty.ViewOrder → {Arg2}", index, destIndex, profileProperty.ViewOrder);
			eventLogger.LogDebug("MoveProperty(index={Index},destIndex={DestIndex}): nextProfileProperty.ViewOrder → {Arg2}", index, destIndex, nextProfileProperty.ViewOrder);
            nextProfileProperty.ViewOrder = currentOrder;

            // Refresh Grid
            this.ProfileProperties.Sort();
            this.BindGrid();
			eventLogger.LogDebug("<<< Exiting MoveProperty(index={Index},destIndex={DestIndex})", index, destIndex);
        }

        /// <summary>Moves a property down in the ViewOrder.</summary>
        /// <param name="index">The index of the Property to move.</param>
        private void MovePropertyDown(int index)
        {
			eventLogger.LogDebug(">>> Entering MovePropertyDown(index={Index})", index);
            this.MoveProperty(index, index + 1);
			eventLogger.LogDebug("<<< Exiting MovePropertyDown(index={Index})", index);
        }

        /// <summary>Moves a property up in the ViewOrder.</summary>
        /// <param name="index">The index of the Property to move.</param>
        private void MovePropertyUp(int index)
        {
			eventLogger.LogDebug(">>> Entering MovePropertyUp(index={Index})", index);
            this.MoveProperty(index, index - 1);
			eventLogger.LogDebug("<<< Exiting MovePropertyUp(index={Index})", index);
        }

        /// <summary>Binds the Property Collection to the Grid.</summary>
        private void BindGrid()
        {
			eventLogger.LogDebug(">>> Entering BindGrid()");
            bool allRequired = true;
            bool allVisible = true;
			eventLogger.LogDebug("BindGrid(): allVisible → {AllVisible}", allVisible);
			eventLogger.LogDebug("BindGrid(): allRequired → {AllRequired}", allRequired);

            // Check whether the checkbox column headers are true or false
            foreach (ProfilePropertyDefinition profProperty in this.ProfileProperties)
            {
                if (profProperty.Required == false)
                {
                    allRequired = false;
                }

                if (profProperty.Visible == false)
                {
                    allVisible = false;
                }

                if (!allRequired && !allVisible)
                {
                    break;
                }
            }

            foreach (DataGridColumn column in this.grdProfileProperties.Columns)
            {
                if (ReferenceEquals(column.GetType(), typeof(CheckBoxColumn)))
                {
                    // Manage CheckBox column events
                    var checkBoxColumn = (CheckBoxColumn)column;
					eventLogger.LogDebug("BindGrid(): checkBoxColumn → {CheckBoxColumn}", checkBoxColumn);
                    if (checkBoxColumn.DataField == "Required")
                    {
                        checkBoxColumn.Checked = allRequired;
						eventLogger.LogDebug("BindGrid(): checkBoxColumn.Checked → {Arg0}", checkBoxColumn.Checked);
                    }

                    if (checkBoxColumn.DataField == "Visible")
                    {
                        checkBoxColumn.Checked = allVisible;
                    }
                }
            }

            this.grdProfileProperties.DataSource = this.ProfileProperties;
            this.grdProfileProperties.DataBind();
			eventLogger.LogDebug("<<< Exiting BindGrid()");
        }

        /// <summary>Refresh the Property Collection to the Grid.</summary>
        private void RefreshGrid()
        {
			eventLogger.LogDebug(">>> Entering RefreshGrid()");
            this.profileProperties = null;
            this.BindGrid();
			eventLogger.LogDebug("RefreshGrid(): this.profileProperties → {Arg0}", this.profileProperties);
			eventLogger.LogDebug("RefreshGrid(): profileProperties → {ProfileProperties}", profileProperties);
			eventLogger.LogDebug("<<< Exiting RefreshGrid()");
        }

        /// <summary>Updates any "dirty" properties.</summary>
        private void UpdateProperties()
        {
			eventLogger.LogDebug(">>> Entering UpdateProperties()");
            this.ProcessPostBack();
            foreach (ProfilePropertyDefinition property in this.ProfileProperties)
            {
                if (property.IsDirty)
                {
                    if (this.UsersPortalId == Null.NullInteger)
                    {
                        property.Required = false;
						eventLogger.LogDebug("UpdateProperties(): property.Required → {Arg0}", property.Required);
                    }

                    ProfileController.UpdatePropertyDefinition(this.eventLogger, this.portalController, this.appStatus, this.portalGroupController, property);
                }
            }
			eventLogger.LogDebug("<<< Exiting UpdateProperties()");
        }

        /// <summary>
        /// This method is responsible for taking in posted information from the grid and
        /// persisting it to the property definition collection.
        /// </summary>
        private void ProcessPostBack()
        {
			eventLogger.LogDebug(">>> Entering ProcessPostBack()");
            string[] newOrder = ClientAPI.GetClientSideReorder(this.grdProfileProperties.ClientID, this.Page);
            for (int i = 0; i <= this.grdProfileProperties.Items.Count - 1; i++)
            {
                DataGridItem dataGridItem = this.grdProfileProperties.Items[i];
				eventLogger.LogDebug("ProcessPostBack(): dataGridItem → {DataGridItem}", dataGridItem);
                ProfilePropertyDefinition profileProperty = this.ProfileProperties[i];
                CheckBox checkBox = (CheckBox)dataGridItem.Cells[COLUMNREQUIRED].Controls[0];
				eventLogger.LogDebug("ProcessPostBack(): checkBox → {CheckBox}", checkBox);
				eventLogger.LogDebug("ProcessPostBack(): profileProperty.Required → {Arg0}", profileProperty.Required);
                profileProperty.Required = checkBox.Checked;
                checkBox = (CheckBox)dataGridItem.Cells[COLUMNVISIBLE].Controls[0];
                profileProperty.Visible = checkBox.Checked;
				eventLogger.LogDebug("ProcessPostBack(): profileProperty.Visible → {Arg0}", profileProperty.Visible);
            }

            // assign vieworder
            for (int i = 0; i <= newOrder.Length - 1; i++)
            {
                this.ProfileProperties[Convert.ToInt32(newOrder[i])].ViewOrder = i;
            }

            this.ProfileProperties.Sort();
			eventLogger.LogDebug("<<< Exiting ProcessPostBack()");
        }

        private void CmdRefresh_Click(object sender, EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering CmdRefresh_Click(sender={Sender},e={E})", sender, e);
            this.RefreshGrid();
			eventLogger.LogDebug("<<< Exiting CmdRefresh_Click(sender={Sender},e={E})", sender, e);
        }

        private void GrdProfileProperties_ItemCheckedChanged(object sender, DNNDataGridCheckChangedEventArgs e)
        {
			eventLogger.LogDebug(">>> Entering GrdProfileProperties_ItemCheckedChanged(sender={Sender},e={E})", sender, e);
            string propertyName = e.Field;
            bool propertyValue = e.Checked;

            if (e.IsAll)
            {
                // Update All the properties
                foreach (ProfilePropertyDefinition profProperty in this.ProfileProperties)
                {
                    switch (propertyName)
                    {
                        case "Required":
                            profProperty.Required = propertyValue;
                            break;
                        case "Visible":
                            profProperty.Visible = propertyValue;
                            break;
                    }
                }
            }
            else
            {
                // Update the indexed property
                ProfilePropertyDefinition profileProperty = this.ProfileProperties[e.Item.ItemIndex];
                switch (propertyName)
                {
                    case "Required":
                        profileProperty.Required = propertyValue;
						eventLogger.LogDebug("GrdProfileProperties_ItemCheckedChanged(sender={Sender},e={E}): profileProperty.Required → {Arg2}", sender, e, profileProperty.Required);
                        break;
                    case "Visible":
                        profileProperty.Visible = propertyValue;
						eventLogger.LogDebug("GrdProfileProperties_ItemCheckedChanged(sender={Sender},e={E}): profileProperty.Visible → {Arg2}", sender, e, profileProperty.Visible);
                        break;
                }
            }

            this.BindGrid();
			eventLogger.LogDebug("<<< Exiting GrdProfileProperties_ItemCheckedChanged(sender={Sender},e={E})", sender, e);
        }

        private void GrdProfileProperties_ItemCommand(object source, DataGridCommandEventArgs e)
        {
			eventLogger.LogDebug(">>> Entering GrdProfileProperties_ItemCommand(source={Source},e={E})", source, e);
            int index = e.Item.ItemIndex;

            switch (e.CommandName)
            {
                case "Delete":
                    this.DeleteProperty(index);
                    break;
                case "MoveUp":
                    this.MovePropertyUp(index);
                    break;
                case "MoveDown":
                    this.MovePropertyDown(index);
                    break;
            }
			eventLogger.LogDebug("<<< Exiting GrdProfileProperties_ItemCommand(source={Source},e={E})", source, e);
        }

        /// <summary>
        /// When it is determined that the client supports a rich interactivity the grdProfileProperties_ItemCreated
        /// event is responsible for disabling all the unneeded AutoPostBacks, along with assigning the appropriate
        ///     client-side script for each event handler.
        /// </summary>
        private void GrdProfileProperties_ItemCreated(object sender, DataGridItemEventArgs e)
        {
			eventLogger.LogDebug(">>> Entering GrdProfileProperties_ItemCreated(sender={Sender},e={E})", sender, e);
            if (this.SupportsRichClient())
            {
                switch (e.Item.ItemType)
                {
                    case ListItemType.Header:
                        // we combined the header label and checkbox in same place, so it is control 1 instead of 0
                        ((WebControl)e.Item.Cells[COLUMNREQUIRED].Controls[1]).Attributes.Add("onclick", "dnn.util.checkallChecked(this," + COLUMNREQUIRED + ");");
                        ((CheckBox)e.Item.Cells[COLUMNREQUIRED].Controls[1]).AutoPostBack = false;

                        int column_visible = this.requiredColumnHidden ? COLUMNVISIBLE - 1 : COLUMNVISIBLE;
                        ((WebControl)e.Item.Cells[COLUMNVISIBLE].Controls[1]).Attributes.Add("onclick", "dnn.util.checkallChecked(this," + column_visible + ");");
                        ((CheckBox)e.Item.Cells[COLUMNVISIBLE].Controls[1]).AutoPostBack = false;
                        break;
                    case ListItemType.AlternatingItem:
                    case ListItemType.Item:
                        ((CheckBox)e.Item.Cells[COLUMNREQUIRED].Controls[0]).AutoPostBack = false;
                        ((CheckBox)e.Item.Cells[COLUMNVISIBLE].Controls[0]).AutoPostBack = false;
                        ClientAPI.EnableClientSideReorder(e.Item.Cells[COLUMNMOVEDOWN].Controls[0], this.Page, false, this.grdProfileProperties.ClientID);
                        ClientAPI.EnableClientSideReorder(e.Item.Cells[COLUMNMOVEUP].Controls[0], this.Page, true, this.grdProfileProperties.ClientID);
                        break;
                }
            }
			eventLogger.LogDebug("<<< Exiting GrdProfileProperties_ItemCreated(sender={Sender},e={E})", sender, e);
        }
    }
}
