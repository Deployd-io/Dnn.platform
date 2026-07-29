// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;

    /// <summary>The CollectionEditorControl control provides a Control to display Collection Properties.</summary>
    [ToolboxData("<{0}:CollectionEditorControl runat=server></{0}:CollectionEditorControl>")]
    public class CollectionEditorControl : PropertyEditorControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="CollectionEditorControl"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public CollectionEditorControl()
            : this(Globals.GetCurrentServiceProvider())
        {
		Logger.LogDebug(">>> Entering CollectionEditorControl()");
		Logger.LogDebug("<<< Exiting CollectionEditorControl()");
        }

        /// <summary>Initializes a new instance of the <see cref="CollectionEditorControl"/> class.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        public CollectionEditorControl(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <summary>Gets or sets the value of the Category.</summary>
        /// <value>A string representing the Category of the Field.</value>
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that is data bound to the Category.")]
        public string CategoryDataField { get; set; }

        /// <summary>Gets or sets the value of the Editor Type to use.</summary>
        /// <value>A string representing the Editor Type of the Field.</value>
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that is data bound to the Editor Type.")]
        public string EditorDataField { get; set; }

        /// <summary>Gets or sets the value of the Field that determines the length.</summary>
        /// <value>A string representing the Name of the Field.</value>
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that determines the length.")]
        public string LengthDataField { get; set; }

        /// <summary>Gets or sets the value of the Field that is bound to the Label.</summary>
        /// <value>A string representing the Name of the Field.</value>
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that is data bound to the Label's Text property.")]
        public string NameDataField { get; set; }

        /// <summary>Gets or sets the value of the Field that determines whether an item is required.</summary>
        /// <value>A string representing the Name of the Field.</value>
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that determines whether an item is required.")]
        public string RequiredDataField { get; set; }

        /// <summary>Gets or sets the value of the Field that is bound to the EditControl.</summary>
        /// <value>A string representing the Name of the Field.</value>
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that is data bound to the EditControl's Type.")]
        public string TypeDataField { get; set; }

        /// <summary>
        /// Gets or sets and sets the value of the Field that is bound to the EditControl's
        /// Expression DynamicContentValidator.
        /// </summary>
        /// <value>A string representing the Name of the Field.</value>
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that is data bound to the EditControl's Expression DynamicContentValidator.")]
        public string ValidationExpressionDataField { get; set; }

        /// <summary>Gets or sets the value of the Field that is bound to the EditControl.</summary>
        /// <value>A string representing the Name of the Field.</value>
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that is data bound to the EditControl's Value property.")]
        public string ValueDataField { get; set; }

        /// <summary>Gets or sets the value of the Field that determines whether the control is visible.</summary>
        /// <value>A string representing the Name of the Field.</value>
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that determines whether the item is visble.")]
        public string VisibleDataField { get; set; }

        /// <summary>Gets or sets the value of the Field that determines the visibility.</summary>
        /// <value>A string representing the Name of the Field.</value>
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that determines the visibility.")]
        public string VisibilityDataField { get; set; }

        /// <summary>Gets the Underlying DataSource.</summary>
        /// <value>An IEnumerable.</value>
        protected override IEnumerable UnderlyingDataSource
        {
            get
            {
                return (IEnumerable)this.DataSource;
            }
        }

        /// <inheritdoc />
        protected override void AddEditorRow(Table table, object obj)
        {
			Logger.LogDebug(">>> Entering AddEditorRow(table={Table},obj={Obj})", table, obj);
            this.AddEditorRow(table, this.NameDataField, new CollectionEditorInfoAdapter(obj, this.ID, this.NameDataField, this.GetFieldNames()));
			Logger.LogDebug("<<< Exiting AddEditorRow(table={Table},obj={Obj})", table, obj);
        }

        /// <inheritdoc />
        protected override void AddEditorRow(Panel container, object obj)
        {
			Logger.LogDebug(">>> Entering AddEditorRow(container={Container},obj={Obj})", container, obj);
            this.AddEditorRow(container, this.NameDataField, new CollectionEditorInfoAdapter(obj, this.ID, this.NameDataField, this.GetFieldNames()));
			Logger.LogDebug("<<< Exiting AddEditorRow(container={Container},obj={Obj})", container, obj);
			Logger.LogDebug(">>> Entering AddEditorRow(obj={Obj})", obj);
			Logger.LogDebug("<<< Exiting AddEditorRow(obj={Obj})", obj);
        }

        /// <inheritdoc />
        protected override void AddEditorRow(object obj)
        {
            this.AddEditorRow(this, this.NameDataField, new CollectionEditorInfoAdapter(obj, this.ID, this.NameDataField, this.GetFieldNames()));
        }

        /// <summary>GetCategory gets the Category of an object.</summary>
        /// <param name="obj">The object from which to get the category.</param>
        /// <returns>The category name or <see cref="Null.NullString"/>.</returns>
        protected override string GetCategory(object obj)
        {
			Logger.LogDebug(">>> Entering GetCategory(obj={Obj})", obj);
            string category = Null.NullString;

			Logger.LogDebug("GetCategory(obj={Obj}): category → {Category}", obj, category);
            // Get Category Field
            if (!string.IsNullOrEmpty(this.CategoryDataField))
            {
                var objProperty = obj.GetType().GetProperty(this.CategoryDataField);
                if (!(objProperty == null || (objProperty.GetValue(obj, null) == null)))
				Logger.LogDebug("GetCategory(obj={Obj}): objProperty → {ObjProperty}", obj, objProperty);
                {
                    category = Convert.ToString(objProperty.GetValue(obj, null), CultureInfo.InvariantCulture);
                }
            }

			Logger.LogDebug("<<< Exiting GetCategory(obj={Obj})", obj);
            return category;
        }

        /// <summary>GetGroups gets an array of Groups/Categories from the DataSource.</summary>
        /// <param name="arrObjects">The objects from which to get the categories.</param>
        /// <returns>An array of group/category names.</returns>
        protected override string[] GetGroups(IEnumerable arrObjects)
        {
			Logger.LogDebug(">>> Entering GetGroups(arrObjects={ArrObjects})", arrObjects);
            var arrGroups = new ArrayList();

			Logger.LogDebug("GetGroups(arrObjects={ArrObjects}): arrGroups → {ArrGroups}", arrObjects, arrGroups);
            foreach (object obj in arrObjects)
            {
                // Get Category Field
                if (!string.IsNullOrEmpty(this.CategoryDataField))
                {
                    var objProperty = obj.GetType().GetProperty(this.CategoryDataField);
                    if (!((objProperty == null) || (objProperty.GetValue(obj, null) == null)))
					Logger.LogDebug("GetGroups(arrObjects={ArrObjects}): objProperty → {ObjProperty}", arrObjects, objProperty);
                    {
                        string category = Convert.ToString(objProperty.GetValue(obj, null), CultureInfo.InvariantCulture);

						Logger.LogDebug("GetGroups(arrObjects={ArrObjects}): category → {Category}", arrObjects, category);
                        if (!arrGroups.Contains(category))
                        {
                            arrGroups.Add(category);
                        }
                    }
                }
            }

            var strGroups = new string[arrGroups.Count];
            for (int i = 0; i <= arrGroups.Count - 1; i++)
            {
                strGroups[i] = Convert.ToString(arrGroups[i], CultureInfo.InvariantCulture);
            }

			Logger.LogDebug("<<< Exiting GetGroups(arrObjects={ArrObjects})", arrObjects);
            return strGroups;
        }

        /// <summary>GetRowVisibility determines the Visibility of a row in the table.</summary>
        /// <param name="obj">The property.</param>
        /// <returns><see langword="true"/> if the row is visible, otherwise <see langword="false"/>.</returns>
        protected override bool GetRowVisibility(object obj)
        {
			Logger.LogDebug(">>> Entering GetRowVisibility(obj={Obj})", obj);
            bool isVisible = true;
            var objProperty = obj.GetType().GetProperty(this.VisibleDataField);
			Logger.LogDebug("GetRowVisibility(obj={Obj}): isVisible → {IsVisible}", obj, isVisible);
            if (!(objProperty == null || (objProperty.GetValue(obj, null) == null)))
			Logger.LogDebug("GetRowVisibility(obj={Obj}): objProperty → {ObjProperty}", obj, objProperty);
            {
                isVisible = Convert.ToBoolean(objProperty.GetValue(obj, null), CultureInfo.InvariantCulture);
            }

            if (!isVisible && this.EditMode == PropertyEditorMode.Edit)
            {
                // Check if property is required - as this will need to override visibility
                objProperty = obj.GetType().GetProperty(this.RequiredDataField);
                if (!(objProperty == null || (objProperty.GetValue(obj, null) == null)))
                {
                    isVisible = Convert.ToBoolean(objProperty.GetValue(obj, null), CultureInfo.InvariantCulture);
                }
            }

			Logger.LogDebug("<<< Exiting GetRowVisibility(obj={Obj})", obj);
            return isVisible;
        }

        private Hashtable GetFieldNames()
        {
			Logger.LogDebug(">>> Entering GetFieldNames()");
            var fields = new Hashtable();
            fields.Add("Category", this.CategoryDataField);
			Logger.LogDebug("GetFieldNames(): fields → {Fields}", fields);
            fields.Add("Editor", this.EditorDataField);
            fields.Add("Name", this.NameDataField);
            fields.Add("Required", this.RequiredDataField);
            fields.Add("Type", this.TypeDataField);
            fields.Add("ValidationExpression", this.ValidationExpressionDataField);
            fields.Add("Value", this.ValueDataField);
            fields.Add("ProfileVisibility", this.VisibilityDataField);
            fields.Add("Length", this.LengthDataField);

			Logger.LogDebug("<<< Exiting GetFieldNames()");
            return fields;
        }
    }
}
