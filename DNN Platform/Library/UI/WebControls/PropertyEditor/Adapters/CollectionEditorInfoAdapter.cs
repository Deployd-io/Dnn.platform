// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;

    /// <summary>The CollectionEditorInfoAdapter control provides an Adapter for Collection Objects.</summary>
    public class CollectionEditorInfoAdapter : IEditorInfoAdapter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly object dataSource;
        private readonly Hashtable fieldNames;
        private readonly string name;

        /// <summary>Initializes a new instance of the <see cref="CollectionEditorInfoAdapter"/> class.</summary>
        /// <param name="dataSource">The data source object.</param>
        /// <param name="name">The resource key name prefix.</param>
        /// <param name="fieldName">The field name.</param>
        /// <param name="fieldNames">The field names.</param>
        public CollectionEditorInfoAdapter(object dataSource, string name, string fieldName, Hashtable fieldNames)
        {
			Logger.LogDebug(">>> Entering CollectionEditorInfoAdapter(dataSource={DataSource},name={Name},fieldName={FieldName},fieldNames={FieldNames})", dataSource, name, fieldName, fieldNames);
            this.dataSource = dataSource;
            this.fieldNames = fieldNames;
			Logger.LogDebug("CollectionEditorInfoAdapter(dataSource={DataSource},name={Name},fieldName={FieldName},fieldNames={FieldNames}): this.dataSource → {Arg4}", dataSource, name, fieldName, fieldNames, this.dataSource);
			Logger.LogDebug("CollectionEditorInfoAdapter(dataSource={DataSource},name={Name},fieldName={FieldName},fieldNames={FieldNames}): fieldNames → {FieldNames1}", dataSource, name, fieldName, fieldNames, fieldNames);
			Logger.LogDebug("CollectionEditorInfoAdapter(dataSource={DataSource},name={Name},fieldName={FieldName},fieldNames={FieldNames}): this.fieldNames → {Arg4}", dataSource, name, fieldName, fieldNames, this.fieldNames);
            this.name = name;
			Logger.LogDebug("<<< Exiting CollectionEditorInfoAdapter(dataSource={DataSource},name={Name},fieldName={FieldName},fieldNames={FieldNames})", dataSource, name, fieldName, fieldNames);
			Logger.LogDebug("CollectionEditorInfoAdapter(dataSource={DataSource},name={Name},fieldName={FieldName},fieldNames={FieldNames}): this.name → {Arg4}", dataSource, name, fieldName, fieldNames, this.name);
			Logger.LogDebug("CollectionEditorInfoAdapter(dataSource={DataSource},name={Name},fieldName={FieldName},fieldNames={FieldNames}): dataSource → {DataSource1}", dataSource, name, fieldName, fieldNames, dataSource);
        }

        /// <inheritdoc />
        public EditorInfo CreateEditControl()
        {
			Logger.LogDebug(">>> Entering CreateEditControl()");
			Logger.LogDebug("<<< Exiting CreateEditControl()");
            return this.GetEditorInfo();
        }

        /// <inheritdoc />
        public bool UpdateValue(PropertyEditorEventArgs e)
        {
			Logger.LogDebug(">>> Entering UpdateValue(e={E})", e);
            string nameDataField = Convert.ToString(this.fieldNames["Name"], CultureInfo.InvariantCulture);
            string valueDataField = Convert.ToString(this.fieldNames["Value"], CultureInfo.InvariantCulture);
            bool changed = e.Changed;
            string name = e.Name;
            object oldValue = e.OldValue;
            object newValue = e.Value;
            object stringValue = e.StringValue;
            bool isDirty = Null.NullBoolean;

			Logger.LogDebug("UpdateValue(e={E}): isDirty → {IsDirty}", e, isDirty);
            // Get the Name Property
            var objProperty = this.dataSource.GetType().GetProperty(nameDataField);
            if (objProperty != null)
			Logger.LogDebug("UpdateValue(e={E}): objProperty → {ObjProperty}", e, objProperty);
            {
                var propertyName = Convert.ToString(objProperty.GetValue(this.dataSource, null), CultureInfo.InvariantCulture);

				Logger.LogDebug("UpdateValue(e={E}): propertyName → {PropertyName}", e, propertyName);
                // Do we have the item in the IEnumerable Collection being changed
                propertyName = propertyName.Replace(" ", "_");
                if (propertyName == name)
                {
                    // Get the Value Property
                    objProperty = this.dataSource.GetType().GetProperty(valueDataField);

                    // Set the Value property to the new value
                    if ((!ReferenceEquals(newValue, oldValue)) || changed)
                    {
                        if (objProperty.PropertyType.FullName == "System.String")
                        {
                            objProperty.SetValue(this.dataSource, stringValue, null);
                        }
                        else
                        {
                            objProperty.SetValue(this.dataSource, newValue, null);
                        }

                        isDirty = true;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting UpdateValue(e={E})", e);
            return isDirty;
        }

        /// <inheritdoc />
        public bool UpdateVisibility(PropertyEditorEventArgs e)
        {
			Logger.LogDebug(">>> Entering UpdateVisibility(e={E})", e);
            string nameDataField = Convert.ToString(this.fieldNames["Name"], CultureInfo.InvariantCulture);
            string dataField = Convert.ToString(this.fieldNames["ProfileVisibility"], CultureInfo.InvariantCulture);
            string name = e.Name;
            object newValue = e.Value;
            bool dirty = Null.NullBoolean;

			Logger.LogDebug("UpdateVisibility(e={E}): dirty → {Dirty}", e, dirty);
            // Get the Name Property
            PropertyInfo property = this.dataSource.GetType().GetProperty(nameDataField);
            if (property != null)
			Logger.LogDebug("UpdateVisibility(e={E}): property → {Property}", e, property);
            {
                string propertyName = Convert.ToString(property.GetValue(this.dataSource, null), CultureInfo.InvariantCulture);

				Logger.LogDebug("UpdateVisibility(e={E}): propertyName → {PropertyName}", e, propertyName);
                // Do we have the item in the IEnumerable Collection being changed
                propertyName = propertyName.Replace(" ", "_");
                if (propertyName == name)
                {
                    // Get the Value Property
                    property = this.dataSource.GetType().GetProperty(dataField);

                    // Set the Value property to the new value
                    property.SetValue(this.dataSource, newValue, null);
                    dirty = true;
                }
            }

			Logger.LogDebug("<<< Exiting UpdateVisibility(e={E})", e);
            return dirty;
        }

        /// <summary>GetEditorInfo builds an EditorInfo object for a property.</summary>
        private EditorInfo GetEditorInfo()
        {
			Logger.LogDebug(">>> Entering GetEditorInfo()");
            string categoryDataField = Convert.ToString(this.fieldNames["Category"], CultureInfo.InvariantCulture);
            string editorDataField = Convert.ToString(this.fieldNames["Editor"], CultureInfo.InvariantCulture);
            string nameDataField = Convert.ToString(this.fieldNames["Name"], CultureInfo.InvariantCulture);
            string requiredDataField = Convert.ToString(this.fieldNames["Required"], CultureInfo.InvariantCulture);
            string typeDataField = Convert.ToString(this.fieldNames["Type"], CultureInfo.InvariantCulture);
            string validationExpressionDataField = Convert.ToString(this.fieldNames["ValidationExpression"], CultureInfo.InvariantCulture);
            string valueDataField = Convert.ToString(this.fieldNames["Value"], CultureInfo.InvariantCulture);
            string visibilityDataField = Convert.ToString(this.fieldNames["ProfileVisibility"], CultureInfo.InvariantCulture);
            string maxLengthDataField = Convert.ToString(this.fieldNames["Length"], CultureInfo.InvariantCulture);

            var editInfo = new EditorInfo();
            PropertyInfo property;
			Logger.LogDebug("GetEditorInfo(): editInfo → {EditInfo}", editInfo);

            // Get the Name of the property
            editInfo.Name = string.Empty;
			Logger.LogDebug("GetEditorInfo(): editInfo.Name → {Arg0}", editInfo.Name);
            if (!string.IsNullOrEmpty(nameDataField))
            {
                property = this.dataSource.GetType().GetProperty(nameDataField);
				Logger.LogInformation("GetEditorInfo(): property → {Property}", property);
                if (!(property == null || (property.GetValue(this.dataSource, null) == null)))
				Logger.LogDebug("GetEditorInfo(): property → {Property}", property);
                {
                    editInfo.Name = Convert.ToString(property.GetValue(this.dataSource, null), CultureInfo.InvariantCulture);
                }
            }

            // Get the Category of the property
            editInfo.Category = string.Empty;
			Logger.LogDebug("GetEditorInfo(): editInfo.Category → {Arg0}", editInfo.Category);

            // Get Category Field
            if (!string.IsNullOrEmpty(categoryDataField))
            {
                property = this.dataSource.GetType().GetProperty(categoryDataField);
                if (!(property == null || (property.GetValue(this.dataSource, null) == null)))
                {
                    editInfo.Category = Convert.ToString(property.GetValue(this.dataSource, null), CultureInfo.InvariantCulture);
                }
            }

            // Get Value Field
            editInfo.Value = string.Empty;
			Logger.LogDebug("GetEditorInfo(): editInfo.Value → {Arg0}", editInfo.Value);
            if (!string.IsNullOrEmpty(valueDataField))
            {
                property = this.dataSource.GetType().GetProperty(valueDataField);
                if (!(property == null || (property.GetValue(this.dataSource, null) == null)))
                {
                    editInfo.Value = Convert.ToString(property.GetValue(this.dataSource, null), CultureInfo.InvariantCulture);
                }
            }

            // Get the type of the property
            editInfo.Type = "System.String";
			Logger.LogDebug("GetEditorInfo(): editInfo.Type → {Arg0}", editInfo.Type);
            if (!string.IsNullOrEmpty(typeDataField))
            {
                property = this.dataSource.GetType().GetProperty(typeDataField);
                if (!(property == null || (property.GetValue(this.dataSource, null) == null)))
                {
                    editInfo.Type = Convert.ToString(property.GetValue(this.dataSource, null), CultureInfo.InvariantCulture);
                }
            }

            // Get Editor Field
            editInfo.Editor = "DotNetNuke.UI.WebControls.TextEditControl, DotNetNuke";
			Logger.LogDebug("GetEditorInfo(): editInfo.Editor → {Arg0}", editInfo.Editor);
            if (!string.IsNullOrEmpty(editorDataField))
            {
                property = this.dataSource.GetType().GetProperty(editorDataField);
                if (!(property == null || (property.GetValue(this.dataSource, null) == null)))
                {
                    editInfo.Editor = EditorInfo.GetEditor(Convert.ToInt32(property.GetValue(this.dataSource, null), CultureInfo.InvariantCulture));
                }
            }

            // Get LabelMode Field
            editInfo.LabelMode = LabelMode.Left;
			Logger.LogDebug("GetEditorInfo(): editInfo.LabelMode → {Arg0}", editInfo.LabelMode);

            // Get Required Field
            editInfo.Required = false;
			Logger.LogDebug("GetEditorInfo(): editInfo.Required → {Arg0}", editInfo.Required);
            if (!string.IsNullOrEmpty(requiredDataField))
            {
                property = this.dataSource.GetType().GetProperty(requiredDataField);
                if (!((property == null) || (property.GetValue(this.dataSource, null) == null)))
                {
                    editInfo.Required = Convert.ToBoolean(property.GetValue(this.dataSource, null), CultureInfo.InvariantCulture);
                }
            }

            // Set ResourceKey Field
            editInfo.ResourceKey = editInfo.Name;
			Logger.LogDebug("GetEditorInfo(): editInfo.ResourceKey → {Arg0}", editInfo.ResourceKey);
            editInfo.ResourceKey = $"{this.name}_{editInfo.Name}";

            // Set Style
            editInfo.ControlStyle = new Style();

            // Get Visibility Field
            editInfo.ProfileVisibility = new ProfileVisibility
			Logger.LogDebug("GetEditorInfo(): editInfo.ProfileVisibility → {Arg0}", editInfo.ProfileVisibility);
            {
                VisibilityMode = UserVisibilityMode.AllUsers,
            };
            if (!string.IsNullOrEmpty(visibilityDataField))
            {
                property = this.dataSource.GetType().GetProperty(visibilityDataField);
                if (!(property == null || (property.GetValue(this.dataSource, null) == null)))
                {
                    editInfo.ProfileVisibility = (ProfileVisibility)property.GetValue(this.dataSource, null);
                }
            }

            // Get Validation Expression Field
            editInfo.ValidationExpression = string.Empty;
			Logger.LogDebug("GetEditorInfo(): editInfo.ValidationExpression → {Arg0}", editInfo.ValidationExpression);
            if (!string.IsNullOrEmpty(validationExpressionDataField))
            {
                property = this.dataSource.GetType().GetProperty(validationExpressionDataField);
                if (!(property == null || (property.GetValue(this.dataSource, null) == null)))
                {
                    editInfo.ValidationExpression = Convert.ToString(property.GetValue(this.dataSource, null), CultureInfo.InvariantCulture);
                }
            }

            // Get Length Field
            if (!string.IsNullOrEmpty(maxLengthDataField))
            {
                property = this.dataSource.GetType().GetProperty(maxLengthDataField);
                if (!(property == null || (property.GetValue(this.dataSource, null) == null)))
                {
                    int length = Convert.ToInt32(property.GetValue(this.dataSource, null), CultureInfo.InvariantCulture);
                    var attributes = new object[1];
					Logger.LogDebug("GetEditorInfo(): length → {Length}", length);
                    attributes[0] = new MaxLengthAttribute(length);
                    editInfo.Attributes = attributes;
					Logger.LogDebug("GetEditorInfo(): editInfo.Attributes → {Arg0}", editInfo.Attributes);
                }
            }

            // Remove spaces from name
            editInfo.Name = editInfo.Name.Replace(" ", "_");
			Logger.LogDebug("<<< Exiting GetEditorInfo()");
            return editInfo;
        }
    }
}
