// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>The PropertyEditorControl control provides a way to display and edit any properties of any Info class.</summary>
    public class PropertyEditorControl : WebControl, INamingContainer
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PropertyEditorControl));
        private readonly IServiceProvider serviceProvider;
        private bool itemChanged;
        private Hashtable sections;

        /// <summary>Initializes a new instance of the <see cref="PropertyEditorControl"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public PropertyEditorControl()
            : this(Globals.GetCurrentServiceProvider())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PropertyEditorControl"/> class.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        public PropertyEditorControl(IServiceProvider serviceProvider)
        {
			Logger.LogError(">>> Entering PropertyEditorControl(serviceProvider={ServiceProvider})", serviceProvider);
            this.serviceProvider = serviceProvider;
			Logger.LogWarning("<<< Exiting PropertyEditorControl(serviceProvider={ServiceProvider})", serviceProvider);
			Logger.LogDebug("PropertyEditorControl(serviceProvider={ServiceProvider}): this.serviceProvider → {Arg1}", serviceProvider, this.serviceProvider);
            this.VisibilityStyle = new Style();
            this.ItemStyle = new Style();
            this.LabelStyle = new Style();
            this.HelpStyle = new Style();
            this.GroupHeaderStyle = new Style();
            this.ErrorStyle = new Style();
            this.EditControlStyle = new Style();
            this.Fields = new ArrayList();
            this.ShowRequired = true;
			Logger.LogDebug("PropertyEditorControl(serviceProvider={ServiceProvider}): this.ShowRequired → {Arg1}", serviceProvider, this.ShowRequired);
            this.LabelMode = LabelMode.Left;
            this.HelpDisplayMode = HelpDisplayMode.Always;
            this.Groups = Null.NullString;
			Logger.LogDebug("PropertyEditorControl(serviceProvider={ServiceProvider}): LabelMode → {LabelMode}", serviceProvider, LabelMode);
			Logger.LogDebug("PropertyEditorControl(serviceProvider={ServiceProvider}): this.LabelMode → {Arg1}", serviceProvider, this.LabelMode);
            this.AutoGenerate = true;
        }

        public event PropertyChangedEventHandler ItemAdded;

        public event EditorCreatedEventHandler ItemCreated;

        public event PropertyChangedEventHandler ItemDeleted;

        /// <summary>Gets a value indicating whether any of the properties have been changed.</summary>
        [Browsable(false)]
        public bool IsDirty
        {
            get
            {
                return this.Fields.Cast<FieldEditorControl>().Any(editor => editor.Visible && editor.IsDirty);
            }
        }

        /// <summary>Gets a value indicating whether all of the properties are Valid.</summary>
        [Browsable(false)]
        public bool IsValid
        {
            get
            {
                return this.Fields.Cast<FieldEditorControl>().All(editor => !editor.Visible || editor.IsValid);
            }
        }

        /// <summary>Gets or sets a value indicating whether the editor Autogenerates its editors.</summary>
        [Category("Behavior")]
        public bool AutoGenerate { get; set; }

        /// <summary>Gets or sets the DataSource that is bound to this control.</summary>
        [Browsable(false)]
        [Category("Data")]
        public object DataSource { get; set; }

        /// <summary>Gets or sets the Edit Mode of the Editor.</summary>
        [Category("Appearance")]
        public PropertyEditorMode EditMode { get; set; }

        public EditorDisplayMode DisplayMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Validators should use client-side
        /// validation.
        /// </summary>
        [Category("Behavior")]
        public bool EnableClientValidation { get; set; }

        /// <summary>Gets or sets the grouping mode.</summary>
        [Category("Appearance")]
        public GroupByMode GroupByMode { get; set; }

        /// <summary>Gets or sets the grouping order.</summary>
        /// <value>A comma-delimited list of categories/groups.</value>
        [Category("Appearance")]
        public string Groups { get; set; }

        /// <summary>Gets or sets whether the control displays Help.</summary>
        public HelpDisplayMode HelpDisplayMode { get; set; }

        public LabelMode LabelMode { get; set; }

        /// <summary>Gets or sets the Local Resource File for the Control.</summary>
        public string LocalResourceFile { get; set; }

        /// <summary>Gets or sets the Url of the Required Image.</summary>
        public string RequiredUrl { get; set; }

        /// <summary>Gets or sets a value indicating whether the Required icon is used.</summary>
        public bool ShowRequired { get; set; }

        /// <summary>Gets or sets a value indicating whether the Visibility control is used.</summary>
        [Category("Appearance")]
        public bool ShowVisibility { get; set; }

        /// <summary>Gets or sets whether to sort properties.</summary>
        /// <value>The Sort Mode of the editor.</value>
        /// <remarks>
        /// By default all properties will be sorted.
        /// </remarks>
        [Category("Appearance")]
        public PropertySortType SortMode { get; set; }

        public UserInfo User { get; set; }

        /// <summary>
        /// Gets a collection of fields to display if AutoGenerate is false. Or the
        /// collection of fields generated if AutoGenerate is true.
        /// </summary>
        /// <value>A collection of FieldEditorControl objects.</value>
        [Category("Behavior")]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ArrayList Fields { get; private set; }

        /// <summary>Gets the value of the Field Style.</summary>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Set the Style for the Edit Control.")]
        public Style EditControlStyle { get; private set; }

        /// <summary>Gets or sets the width of the Edit Control Column.</summary>
        [Browsable(true)]
        [Category("Appearance")]
        [Description("Set the Width for the Edit Control.")]
        public Unit EditControlWidth { get; set; }

        /// <summary>Gets the value of the Error Style.</summary>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Set the Style for the Error Text.")]
        public Style ErrorStyle { get; private set; }

        /// <summary>Gets the value of the Group Header Style.</summary>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Set the Style for the Group Header Control.")]
        public Style GroupHeaderStyle { get; private set; }

        /// <summary>Gets or sets a value indicating whether to add a &lt;hr&gt; to the Group Header.</summary>
        [Browsable(true)]
        [Category("Appearance")]
        [Description("Set whether to include a rule <hr> in the Group Header.")]
        public bool GroupHeaderIncludeRule { get; set; }

        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Set the Style for the Help Text.")]
        public Style HelpStyle { get; private set; }

        /// <summary>Gets the value of the Label Style.</summary>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Set the Style for the Label Text")]
        public Style ItemStyle { get; private set; }

        /// <summary>Gets the value of the Label Style.</summary>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Set the Style for the Label Text")]
        public Style LabelStyle { get; private set; }

        /// <summary>Gets or sets the width of the Label Column.</summary>
        [Browsable(true)]
        [Category("Appearance")]
        [Description("Set the Width for the Label Control.")]
        public Unit LabelWidth { get; set; }

        /// <summary>Gets the value of the Visibility Style.</summary>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [Description("Set the Style for the Visibility Control")]
        public Style VisibilityStyle { get; private set; }

        /// <inheritdoc />
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        /// <summary>Gets the Underlying DataSource.</summary>
        /// <value>An IEnumerable Boolean.</value>
        protected virtual IEnumerable UnderlyingDataSource
        {
            get { return this.GetProperties(); }
        }

        /// <summary>Binds the controls to the DataSource.</summary>
        public override void DataBind()
        {
			Logger.LogError(">>> Entering DataBind()");
            // Invoke OnDataBinding so DataBinding Event is raised
            this.OnDataBinding(EventArgs.Empty);

            // Clear Existing Controls
            this.Controls.Clear();

            // Clear Child View State as controls will be loaded from DataSource
            this.ClearChildViewState();

            // Start Tracking ViewState
            this.TrackViewState();

            // Create the Editor
            this.CreateEditor();

			Logger.LogWarning("<<< Exiting DataBind()");
            // Set flag so CreateChildControls should not be invoked later in control's lifecycle
            this.ChildControlsCreated = true;
        }

        /// <summary>
        /// AddEditorRow builds a single editor row and adds it to the Table, using the
        /// specified adapter.
        /// </summary>
        /// <param name="table">The Table Control to add the row to.</param>
        /// <param name="name">The name of property being added.</param>
        /// <param name="adapter">An IEditorInfoAdapter.</param>
        protected void AddEditorRow(Table table, string name, IEditorInfoAdapter adapter)
        {
			Logger.LogError(">>> Entering AddEditorRow(table={Table},name={Name},adapter={Adapter})", table, name, adapter);
            var row = new TableRow();
            table.Rows.Add(row);
			Logger.LogError("AddEditorRow(table={Table},name={Name},adapter={Adapter}): row → {Row}", table, name, adapter, row);

            var cell = new TableCell();
			Logger.LogError("AddEditorRow(table={Table},name={Name},adapter={Adapter}): cell → {Cell}", table, name, adapter, cell);
            row.Cells.Add(cell);

            // Create a FieldEditor for this Row
            var editor = new FieldEditorControl(this.serviceProvider)
            {
                DataSource = this.DataSource,
                EditorInfoAdapter = adapter,
                DataField = name,
                EditorDisplayMode = this.DisplayMode,
                EnableClientValidation = this.EnableClientValidation,
                EditMode = this.EditMode,
                HelpDisplayMode = this.HelpDisplayMode,
                LabelMode = this.LabelMode,
                LabelWidth = this.LabelWidth,
            };
            this.AddEditorRow(editor, cell);

            this.Fields.Add(editor);
			Logger.LogWarning("<<< Exiting AddEditorRow(container={Container},obj={Obj})", container, obj);
        }

        protected void AddEditorRow(WebControl container, string name, IEditorInfoAdapter adapter)
        {
			Logger.LogError(">>> Entering AddEditorRow(container={Container},name={Name},adapter={Adapter})", container, name, adapter);
            var editor = new FieldEditorControl(this.serviceProvider)
            {
				Logger.LogWarning("<<< Exiting AddEditorRow(table={Table},name={Name},adapter={Adapter})", table, name, adapter);
                DataSource = this.DataSource,
                EditorInfoAdapter = adapter,
                DataField = name,
                EditorDisplayMode = this.DisplayMode,
                EnableClientValidation = this.EnableClientValidation,
                EditMode = this.EditMode,
                HelpDisplayMode = this.HelpDisplayMode,
                LabelMode = this.LabelMode,
                LabelWidth = this.LabelWidth,
            };
            this.AddEditorRow(editor, container);

            this.Fields.Add(editor);
			Logger.LogWarning("<<< Exiting AddEditorRow(container={Container},name={Name},adapter={Adapter})", container, name, adapter);
        }

        /// <summary>AddEditorRow builds a sigle editor row and adds it to the Table.</summary>
        /// <remarks>This method is protected so that classes that inherit from
        /// PropertyEditor can modify how the Row is displayed.</remarks>
        /// <param name="table">The Table Control to add the row to.</param>
        /// <param name="obj">Row Data Info.</param>
        protected virtual void AddEditorRow(Table table, object obj)
        {
			Logger.LogError(">>> Entering AddEditorRow(table={Table},obj={Obj})", table, obj);
            var objProperty = (PropertyInfo)obj;
			Logger.LogWarning("<<< Exiting AddEditorRow(table={Table},obj={Obj})", table, obj);
			Logger.LogError("AddEditorRow(table={Table},obj={Obj}): objProperty → {ObjProperty}", table, obj, objProperty);
            this.AddEditorRow(table, objProperty.Name, new StandardEditorInfoAdapter(this.DataSource, objProperty.Name));
        }

        protected virtual void AddEditorRow(Panel container, object obj)
        {
			Logger.LogError(">>> Entering AddEditorRow(container={Container},obj={Obj})", container, obj);
            var objProperty = (PropertyInfo)obj;
            this.AddEditorRow(container, objProperty.Name, new StandardEditorInfoAdapter(this.DataSource, objProperty.Name));
			Logger.LogError("AddEditorRow(container={Container},obj={Obj}): objProperty → {ObjProperty}", container, obj, objProperty);
			Logger.LogError(">>> Entering AddEditorRow(obj={Obj})", obj);
			Logger.LogWarning("<<< Exiting AddEditorRow(obj={Obj})", obj);
			Logger.LogError("AddEditorRow(obj={Obj}): objProperty → {ObjProperty}", obj, objProperty);
        }

        protected virtual void AddEditorRow(object obj)
        {
            var objProperty = (PropertyInfo)obj;
            this.AddEditorRow(this, objProperty.Name, new StandardEditorInfoAdapter(this.DataSource, objProperty.Name));
        }

        protected virtual void AddFields()
        {
			Logger.LogError(">>> Entering AddFields()");
            foreach (FieldEditorControl editor in this.Fields)
            {
                editor.DataSource = this.DataSource;
                editor.EditorInfoAdapter = new StandardEditorInfoAdapter(this.DataSource, editor.DataField);
                editor.EditorDisplayMode = this.DisplayMode;
                editor.EnableClientValidation = this.EnableClientValidation;
                if (editor.EditMode != PropertyEditorMode.View)
                {
                    editor.EditMode = this.EditMode;
                }

                editor.HelpDisplayMode = this.HelpDisplayMode;
                if (editor.LabelMode == LabelMode.None)
                {
                    editor.LabelMode = this.LabelMode;
                }

                if (editor.LabelWidth == Unit.Empty)
                {
                    editor.LabelWidth = this.LabelWidth;
                }

                this.AddEditorRow(editor, this);
            }
			Logger.LogWarning("<<< Exiting AddFields()");
        }

        /// <summary>AddFields adds the fields that have beend defined in design mode (Autogenerate=false).</summary>
        /// <param name="tbl">The Table Control to add the row to.</param>
        protected virtual void AddFields(Table tbl)
        {
			Logger.LogError(">>> Entering AddFields(tbl={Tbl})", tbl);
            foreach (FieldEditorControl editor in this.Fields)
            {
				Logger.LogWarning("<<< Exiting AddFields()");
                var row = new TableRow();
				Logger.LogError("AddFields(tbl={Tbl}): row → {Row}", tbl, row);
                tbl.Rows.Add(row);
                var cell = new TableCell();
				Logger.LogError("AddFields(tbl={Tbl}): cell → {Cell}", tbl, cell);
                row.Cells.Add(cell);

                editor.DataSource = this.DataSource;
                editor.EditorInfoAdapter = new StandardEditorInfoAdapter(this.DataSource, editor.DataField);
                editor.EditorDisplayMode = this.DisplayMode;
                editor.EnableClientValidation = this.EnableClientValidation;
                if (editor.EditMode != PropertyEditorMode.View)
                {
                    editor.EditMode = this.EditMode;
                }

                editor.HelpDisplayMode = this.HelpDisplayMode;
                if (editor.LabelMode == LabelMode.None)
                {
                    editor.LabelMode = this.LabelMode;
                }

                if (editor.LabelWidth == Unit.Empty)
                {
                    editor.LabelWidth = this.LabelWidth;
                }

                this.AddEditorRow(editor, cell);
            }
        }

        /// <summary>AddHeader builds a group header.</summary>
        /// <remarks>This method is protected so that classes that inherit from
        /// PropertyEditor can modify how the Header is displayed.</remarks>
        /// <param name="tbl">The Table Control that contains the group.</param>
        /// <param name="header">Table Header.</param>
        protected virtual void AddHeader(Table tbl, string header)
        {
			Logger.LogError(">>> Entering AddHeader(tbl={Tbl},header={Header})", tbl, header);
            var panel = new Panel();
            var icon = new Image { ID = "ico" + header, EnableViewState = false };
			Logger.LogError("AddHeader(tbl={Tbl},header={Header}): icon → {Icon}", tbl, header, icon);
			Logger.LogError("AddHeader(tbl={Tbl},header={Header}): panel → {Panel}", tbl, header, panel);

            var spacer = new Literal { Text = " ", EnableViewState = false };
			Logger.LogError("AddHeader(tbl={Tbl},header={Header}): spacer → {Spacer}", tbl, header, spacer);

            var label = new Label { ID = "lbl" + header };
			Logger.LogError("AddHeader(tbl={Tbl},header={Header}): label → {Label}", tbl, header, label);
            label.Attributes["resourcekey"] = this.ID + "_" + header + ".Header";
            label.Text = header;
			Logger.LogDebug("AddHeader(tbl={Tbl},header={Header}): label.Text → {Arg2}", tbl, header, label.Text);
            label.EnableViewState = false;
			Logger.LogDebug("AddHeader(tbl={Tbl},header={Header}): label.EnableViewState → {Arg2}", tbl, header, label.EnableViewState);
            label.ControlStyle.CopyFrom(this.GroupHeaderStyle);

            panel.Controls.Add(icon);
            panel.Controls.Add(spacer);
            panel.Controls.Add(label);

            if (this.GroupHeaderIncludeRule)
            {
                panel.Controls.Add(new LiteralControl("<hr noshade=\"noshade\" size=\"1\"/>"));
            }

            this.Controls.Add(panel);

            // Get the Hashtable
            if (this.sections == null)
            {
                this.sections = new Hashtable();
				Logger.LogDebug("AddHeader(tbl={Tbl},header={Header}): sections → {Sections}", tbl, header, sections);
            }

            this.sections[icon] = tbl;
			Logger.LogWarning("<<< Exiting AddHeader(tbl={Tbl},header={Header})", tbl, header);
        }

        /// <summary>CreateEditor creates the control collection.</summary>
        protected virtual void CreateEditor()
        {
			Logger.LogError(">>> Entering CreateEditor()");
            Table table;
            string[] arrGroups = null;

            this.Controls.Clear();
            if (!string.IsNullOrEmpty(this.Groups))
            {
                arrGroups = this.Groups.Split(',');
				Logger.LogError("CreateEditor(): arrGroups → {ArrGroups}", arrGroups);
            }
            else if (this.GroupByMode != GroupByMode.None)
            {
                arrGroups = this.GetGroups(this.UnderlyingDataSource);
				Logger.LogDebug("CreateEditor(): arrGroups → {ArrGroups}", arrGroups);
            }

            if (!this.AutoGenerate)
            {
                // Create a new table
                if (this.DisplayMode == EditorDisplayMode.Div)
                {
                    this.AddFields();
                }
                else
                {
                    // Add the Table to the Controls Collection
                    table = new Table { ID = "tbl" };
					Logger.LogError("CreateEditor(): table → {Table}", table);
					Logger.LogDebug("CreateEditor(): table → {Table}", table);
                    this.AddFields(table);
                    this.Controls.Add(table);
                }
            }
            else
            {
                this.Fields.Clear();
                if (arrGroups is { Length: > 0 })
                {
                    foreach (string strGroup in arrGroups)
                    {
                        if (this.GroupByMode == GroupByMode.Section)
                        {
                            if (this.DisplayMode == EditorDisplayMode.Div)
                            {
                                var groupData = this.UnderlyingDataSource.Cast<object>().Where(obj => this.GetCategory(obj) == strGroup.Trim() && this.GetRowVisibility(obj));
								Logger.LogError("CreateEditor(): groupData → {GroupData}", groupData);
                                if (groupData.Any())
                                {
                                    // Add header
                                    var header = new HtmlGenericControl("h2");
									Logger.LogError("CreateEditor(): header → {Header}", header);
                                    header.Attributes.Add("class", "dnnFormSectionHead");
                                    header.Attributes.Add("id", strGroup);
                                    this.Controls.Add(header);

                                    var localizedGroupName = Localization.GetString("ProfileProperties_" + strGroup + ".Header", this.LocalResourceFile);
									Logger.LogError("CreateEditor(): localizedGroupName → {LocalizedGroupName}", localizedGroupName);
                                    if (string.IsNullOrEmpty(localizedGroupName))
                                    {
                                        localizedGroupName = strGroup;
                                    }

                                    var link = new HyperLink() { Text = localizedGroupName, NavigateUrl = "#" };
									Logger.LogError("CreateEditor(): link → {Link}", link);
                                    header.Controls.Add(link);

                                    // fieldset to hold properties in group
                                    var fieldset = new HtmlGenericControl("fieldset");
									Logger.LogError("CreateEditor(): fieldset → {Fieldset}", fieldset);
                                    var container = new Panel();
									Logger.LogError("CreateEditor(): container → {Container}", container);
                                    fieldset.Controls.Add(container);

                                    foreach (object obj in groupData)
                                    {
                                        this.AddEditorRow(container, obj);
                                    }

                                    this.Controls.Add(fieldset);
                                }
                            }
                            else
                            {
                                // Create a new table
                                table = new Table { ID = "tbl" + strGroup };
                                foreach (object obj in this.UnderlyingDataSource)
                                {
                                    if (this.GetCategory(obj) == strGroup.Trim())
                                    {
                                        // Add the Editor Row to the Table
                                        if (this.GetRowVisibility(obj))
                                        {
                                            if (table.Rows.Count == 0)
                                            {
                                                // Add a Header
                                                this.AddHeader(table, strGroup);
                                            }

                                            this.AddEditorRow(table, obj);
                                        }
                                    }
                                }

                                // Add the Table to the Controls Collection (if it has any rows)
                                if (table.Rows.Count > 0)
                                {
                                    this.Controls.Add(table);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Create a new table
                    if (this.DisplayMode == EditorDisplayMode.Div)
                    {
                        foreach (object obj in this.UnderlyingDataSource)
                        {
                            // Add the Editor Row to the Table
                            if (this.GetRowVisibility(obj))
                            {
                                this.AddEditorRow(obj);
                            }
                        }
                    }
                    else
                    {
                        table = new Table { ID = "tbl" };
                        foreach (object obj in this.UnderlyingDataSource)
                        {
                            if (this.GetRowVisibility(obj))
                            {
                                this.AddEditorRow(table, obj);
                            }
                        }

                        // Add the Table to the Controls Collection
                        this.Controls.Add(table);
                    }
                }
            }
			Logger.LogWarning("<<< Exiting CreateEditor()");
        }

        /// <summary>GetCategory gets the Category of an object.</summary>
        /// <param name="obj">The <see cref="PropertyInfo"/>.</param>
        /// <returns>The category name, or <see cref="Null.NullString"/>.</returns>
        protected virtual string GetCategory(object obj)
        {
			Logger.LogError(">>> Entering GetCategory(obj={Obj})", obj);
            var objProperty = (PropertyInfo)obj;
            var categoryString = Null.NullString;
			Logger.LogError("GetCategory(obj={Obj}): categoryString → {CategoryString}", obj, categoryString);
			Logger.LogError("GetCategory(obj={Obj}): objProperty → {ObjProperty}", obj, objProperty);

            // Get Category Field
            var categoryAttributes = objProperty.GetCustomAttributes(typeof(CategoryAttribute), true);
			Logger.LogError("GetCategory(obj={Obj}): categoryAttributes → {CategoryAttributes}", obj, categoryAttributes);
            if (categoryAttributes.Length > 0)
            {
                var category = (CategoryAttribute)categoryAttributes[0];
				Logger.LogError("GetCategory(obj={Obj}): category → {Category}", obj, category);
                categoryString = category.Category;
            }

			Logger.LogWarning("<<< Exiting GetCategory(obj={Obj})", obj);
            return categoryString;
        }

        /// <summary>GetGroups gets an array of Groups/Categories from the DataSource.</summary>
        /// <param name="arrObjects">The <see cref="PropertyInfo"/> instances.</param>
        /// <returns>An array of group/category names.</returns>
        protected virtual string[] GetGroups(IEnumerable arrObjects)
        {
			Logger.LogError(">>> Entering GetGroups(arrObjects={ArrObjects})", arrObjects);
            var arrGroups = new ArrayList();

			Logger.LogError("GetGroups(arrObjects={ArrObjects}): arrGroups → {ArrGroups}", arrObjects, arrGroups);
            foreach (PropertyInfo objProperty in arrObjects)
            {
                var categoryAttributes = objProperty.GetCustomAttributes(typeof(CategoryAttribute), true);
				Logger.LogError("GetGroups(arrObjects={ArrObjects}): categoryAttributes → {CategoryAttributes}", arrObjects, categoryAttributes);
                if (categoryAttributes.Length > 0)
                {
                    var category = (CategoryAttribute)categoryAttributes[0];
					Logger.LogError("GetGroups(arrObjects={ArrObjects}): category → {Category}", arrObjects, category);

                    if (!arrGroups.Contains(category.Category))
                    {
                        arrGroups.Add(category.Category);
                    }
                }
            }

            var strGroups = new string[arrGroups.Count];
            for (int i = 0; i <= arrGroups.Count - 1; i++)
            {
                strGroups[i] = Convert.ToString(arrGroups[i], CultureInfo.InvariantCulture);
            }

			Logger.LogWarning("<<< Exiting GetGroups(arrObjects={ArrObjects})", arrObjects);
            return strGroups;
        }

        /// <summary>GetRowVisibility determines the Visibility of a row in the table.</summary>
        /// <param name="obj">The property.</param>
        /// <returns><see langword="true"/> if the row is visible, otherwise <see langword="false"/>.</returns>
        protected virtual bool GetRowVisibility(object obj)
        {
			Logger.LogError(">>> Entering GetRowVisibility(obj={Obj})", obj);
            var objProperty = (PropertyInfo)obj;

			Logger.LogError("GetRowVisibility(obj={Obj}): objProperty → {ObjProperty}", obj, objProperty);
            bool isVisible = true;
			Logger.LogError("GetRowVisibility(obj={Obj}): isVisible → {IsVisible}", obj, isVisible);
            object[] browsableAttributes = objProperty.GetCustomAttributes(typeof(BrowsableAttribute), true);
            if (browsableAttributes.Length > 0)
            {
                var browsable = (BrowsableAttribute)browsableAttributes[0];
				Logger.LogError("GetRowVisibility(obj={Obj}): browsable → {Browsable}", obj, browsable);
                if (!browsable.Browsable)
                {
                    isVisible = false;
                }
            }

            if (!isVisible && this.EditMode == PropertyEditorMode.Edit)
            {
                // Check if property is required - as this will need to override visibility
                object[] requiredAttributes = objProperty.GetCustomAttributes(typeof(RequiredAttribute), true);
                if (requiredAttributes.Length > 0)
                {
                    var required = (RequiredAttribute)requiredAttributes[0];
					Logger.LogError("GetRowVisibility(obj={Obj}): required → {Required}", obj, required);
                    if (required.Required)
                    {
                        isVisible = true;
                    }
                }
            }

			Logger.LogWarning("<<< Exiting GetRowVisibility(obj={Obj})", obj);
            return isVisible;
        }

        /// <summary>Runs when an item is added to a collection type property.</summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnItemAdded(PropertyEditorEventArgs e)
        {
			Logger.LogError(">>> Entering OnItemAdded(e={E})", e);
            if (this.ItemAdded != null)
            {
                this.ItemAdded(this, e);
            }
			Logger.LogWarning("<<< Exiting OnItemAdded(e={E})", e);
        }

        /// <summary>Runs when an Editor is Created.</summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnItemCreated(PropertyEditorItemEventArgs e)
        {
			Logger.LogError(">>> Entering OnItemCreated(e={E})", e);
            if (this.ItemCreated != null)
            {
                this.ItemCreated(this, e);
            }
			Logger.LogWarning("<<< Exiting OnItemCreated(e={E})", e);
        }

        /// <summary>Runs when an item is removed from a collection type property.</summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnItemDeleted(PropertyEditorEventArgs e)
        {
			Logger.LogError(">>> Entering OnItemDeleted(e={E})", e);
            if (this.ItemDeleted != null)
            {
                this.ItemDeleted(this, e);
            }
			Logger.LogWarning("<<< Exiting OnItemDeleted(e={E})", e);
        }

        /// <summary>Runs just before the control is rendered.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogError(">>> Entering OnPreRender(e={E})", e);
            if (this.itemChanged)
            {
                // Rebind the control to the DataSource to make sure that the dependent
                // editors are updated
                this.DataBind();
            }

            if (string.IsNullOrEmpty(this.CssClass))
            {
                this.CssClass = "dnnForm";
				Logger.LogDebug("OnPreRender(e={E}): this.CssClass → {Arg1}", e, this.CssClass);
            }

            // Find the Min/Max buttons
            if (this.GroupByMode == GroupByMode.Section && (this.sections != null))
            {
                foreach (DictionaryEntry key in this.sections)
                {
                    var tbl = (Table)key.Value;
					Logger.LogError("OnPreRender(e={E}): tbl → {Tbl}", e, tbl);
                    var icon = (Image)key.Key;
					Logger.LogError("OnPreRender(e={E}): icon → {Icon}", e, icon);
                    DNNClientAPI.EnableMinMax(icon, tbl, false, IconController.IconURL("Minus", "12X15"), IconController.IconURL("Plus", "12X15"), DNNClientAPI.MinMaxPersistanceType.Page);
                }
            }

            base.OnPreRender(e);
			Logger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <summary>Runs when an item is added to a collection type property.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        protected virtual void CollectionItemAdded(object sender, PropertyEditorEventArgs e)
        {
			Logger.LogError(">>> Entering CollectionItemAdded(sender={Sender},e={E})", sender, e);
            this.OnItemAdded(e);
			Logger.LogWarning("<<< Exiting CollectionItemAdded(sender={Sender},e={E})", sender, e);
        }

        /// <summary>Runs when an item is removed from a collection type property.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        protected virtual void CollectionItemDeleted(object sender, PropertyEditorEventArgs e)
        {
			Logger.LogError(">>> Entering CollectionItemDeleted(sender={Sender},e={E})", sender, e);
            this.OnItemDeleted(e);
			Logger.LogWarning("<<< Exiting CollectionItemDeleted(sender={Sender},e={E})", sender, e);
        }

        /// <summary>Runs when an Editor Is Created.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        protected virtual void EditorItemCreated(object sender, PropertyEditorItemEventArgs e)
        {
			Logger.LogError(">>> Entering EditorItemCreated(sender={Sender},e={E})", sender, e);
            this.OnItemCreated(e);
			Logger.LogWarning("<<< Exiting EditorItemCreated(sender={Sender},e={E})", sender, e);
        }

        /// <summary>Runs when an Item in the List Is Changed.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        protected virtual void ListItemChanged(object sender, PropertyEditorEventArgs e)
        {
			Logger.LogError(">>> Entering ListItemChanged(sender={Sender},e={E})", sender, e);
            this.itemChanged = true;
        }

        /// <summary>GetProperties returns an array of <see cref="System.Reflection.PropertyInfo">PropertyInfo</see>.</summary>
        /// <returns>An array of <see cref="System.Reflection.PropertyInfo">PropertyInfo</see> objects
        /// for the current DataSource object.</returns>
        /// <remarks>
        /// GetProperties will return an array of public properties for the current DataSource
        /// object.  The properties will be sorted according to the SortMode property.
        /// </remarks>
        private PropertyInfo[] GetProperties()
        {
			Logger.LogError(">>> Entering GetProperties()");
            if (this.DataSource != null)
            {
                // TODO:  We need to add code to support using the cache in the future
                const BindingFlags bindings = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

                var properties = this.DataSource.GetType().GetProperties(bindings);
				Logger.LogError("GetProperties(): properties → {Properties}", properties);

                // Apply sort method
                switch (this.SortMode)
                {
                    case PropertySortType.Alphabetical:
                        Array.Sort(properties, new PropertyNameComparer());
                        break;
                    case PropertySortType.Category:
                        Array.Sort(properties, new PropertyCategoryComparer());
                        break;
                    case PropertySortType.SortOrderAttribute:
                        Array.Sort(properties, new PropertySortOrderComparer());
                        break;
                }

                return properties;
            }

			Logger.LogWarning("<<< Exiting GetProperties()");
            return null;
        }

        private void AddEditorRow(FieldEditorControl editor, WebControl container)
        {
			Logger.LogError(">>> Entering AddEditorRow(editor={Editor},container={Container})", editor, container);
            editor.ControlStyle.CopyFrom(this.ItemStyle);
            editor.LabelStyle.CopyFrom(this.LabelStyle);
            editor.HelpStyle.CopyFrom(this.HelpStyle);
            editor.ErrorStyle.CopyFrom(this.ErrorStyle);
            editor.VisibilityStyle.CopyFrom(this.VisibilityStyle);
            editor.EditControlStyle.CopyFrom(this.EditControlStyle);
            if (editor.EditControlWidth == Unit.Empty)
            {
                editor.EditControlWidth = this.EditControlWidth;
				Logger.LogDebug("AddEditorRow(editor={Editor},container={Container}): editor.EditControlWidth → {Arg2}", editor, container, editor.EditControlWidth);
            }

            editor.LocalResourceFile = this.LocalResourceFile;
			Logger.LogDebug("AddEditorRow(editor={Editor},container={Container}): editor.LocalResourceFile → {Arg2}", editor, container, editor.LocalResourceFile);
			Logger.LogDebug("AddEditorRow(editor={Editor},container={Container}): editor.ShowRequired → {Arg2}", editor, container, editor.ShowRequired);
			Logger.LogDebug("AddEditorRow(editor={Editor},container={Container}): editor.ShowVisibility → {Arg2}", editor, container, editor.ShowVisibility);
			Logger.LogDebug("AddEditorRow(editor={Editor},container={Container}): editor.User → {Arg2}", editor, container, editor.User);
			Logger.LogDebug("AddEditorRow(editor={Editor},container={Container}): editor.Width → {Arg2}", editor, container, editor.Width);
            editor.RequiredUrl = this.RequiredUrl;
			Logger.LogDebug("AddEditorRow(editor={Editor},container={Container}): editor.RequiredUrl → {Arg2}", editor, container, editor.RequiredUrl);
            editor.ShowRequired = this.ShowRequired;
            editor.ShowVisibility = this.ShowVisibility;
            editor.User = this.User;
            editor.Width = this.Width;
            editor.ItemAdded += this.CollectionItemAdded;
            editor.ItemChanged += this.ListItemChanged;
            editor.ItemCreated += this.EditorItemCreated;
            editor.ItemDeleted += this.CollectionItemDeleted;

            editor.DataBind();
            container.Controls.Add(editor);
			Logger.LogWarning("<<< Exiting AddEditorRow(editor={Editor},container={Container})", editor, container);
        }
    }
}
