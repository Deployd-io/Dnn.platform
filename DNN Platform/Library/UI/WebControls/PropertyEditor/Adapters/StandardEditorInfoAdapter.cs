// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;

    /// <summary>The StandardEditorInfoAdapter control provides an Adapter for standard datasources.</summary>
    public class StandardEditorInfoAdapter : IEditorInfoAdapter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(StandardEditorInfoAdapter));
        private readonly object dataSource;
        private readonly string fieldName;

        /// <summary>Initializes a new instance of the <see cref="StandardEditorInfoAdapter"/> class.</summary>
        /// <param name="dataSource">The data source object.</param>
        /// <param name="fieldName">The field/property name.</param>
        public StandardEditorInfoAdapter(object dataSource, string fieldName)
        {
			Logger.LogError(">>> Entering StandardEditorInfoAdapter(dataSource={DataSource},fieldName={FieldName})", dataSource, fieldName);
            this.dataSource = dataSource;
			Logger.LogDebug("StandardEditorInfoAdapter(dataSource={DataSource},fieldName={FieldName}): this.dataSource → {Arg2}", dataSource, fieldName, this.dataSource);
        Logger.LogWarning("<<< Exiting StandardEditorInfoAdapter(dataSource={DataSource},fieldName={FieldName})", dataSource, fieldName);
            this.fieldName = fieldName;
        }

        /// <inheritdoc />
        public EditorInfo CreateEditControl()
        {
			Logger.LogError(">>> Entering CreateEditControl()");
            EditorInfo editInfo = null;
            PropertyInfo objProperty = GetProperty(this.dataSource, this.fieldName);
			Logger.LogError("CreateEditControl(): editInfo → {EditInfo}", editInfo);
            if (objProperty != null)
            {
				Logger.LogError("CreateEditControl(): objProperty → {ObjProperty}", objProperty);
                editInfo = GetEditorInfo(this.dataSource, objProperty);
            }

			Logger.LogWarning("<<< Exiting CreateEditControl()");
            return editInfo;
        }

        /// <inheritdoc />
        public bool UpdateValue(PropertyEditorEventArgs e)
        {
			Logger.LogError(">>> Entering UpdateValue(e={E})", e);
            bool changed = e.Changed;
            object oldValue = e.OldValue;
            object newValue = e.Value;
            bool isDirty = Null.NullBoolean;
			Logger.LogError("UpdateValue(e={E}): isDirty → {IsDirty}", e, isDirty);
            if (this.dataSource != null)
            {
                PropertyInfo objProperty = this.dataSource.GetType().GetProperty(e.Name);
				Logger.LogError("UpdateValue(e={E}): objProperty → {ObjProperty}", e, objProperty);
                if (objProperty != null)
                {
                    if ((!ReferenceEquals(newValue, oldValue)) || changed)
                    {
                        objProperty.SetValue(this.dataSource, newValue, null);
                        isDirty = true;
                    }
                }
            }

			Logger.LogWarning("<<< Exiting UpdateValue(e={E})", e);
            return isDirty;
        }

        /// <inheritdoc />
        public bool UpdateVisibility(PropertyEditorEventArgs e)
        {
			Logger.LogError(">>> Entering UpdateVisibility(e={E})", e);
			Logger.LogWarning("<<< Exiting UpdateVisibility(e={E})", e);
            return false;
        }

        /// <summary>GetEditorInfo builds an EditorInfo object for a property.</summary>
        private static EditorInfo GetEditorInfo(object dataSource, PropertyInfo objProperty)
        {
			Logger.LogError(">>> Entering GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty})", dataSource, objProperty);
            var editInfo = new EditorInfo();

			Logger.LogError("GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty}): editInfo → {EditInfo}", dataSource, objProperty, editInfo);
            // Get the Name of the property
            editInfo.Name = objProperty.Name;
			Logger.LogDebug("GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty}): editInfo.Name → {Arg2}", dataSource, objProperty, editInfo.Name);

            // Get the value of the property
            editInfo.Value = objProperty.GetValue(dataSource, null);

            // Get the type of the property
            editInfo.Type = objProperty.PropertyType.AssemblyQualifiedName;
			Logger.LogDebug("GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty}): editInfo.Type → {Arg2}", dataSource, objProperty, editInfo.Type);

            // Get the Custom Attributes for the property
            editInfo.Attributes = objProperty.GetCustomAttributes(true);

            // Get Category Field
            editInfo.Category = string.Empty;
			Logger.LogDebug("GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty}): editInfo.Category → {Arg2}", dataSource, objProperty, editInfo.Category);
            object[] categoryAttributes = objProperty.GetCustomAttributes(typeof(CategoryAttribute), true);
            if (categoryAttributes.Length > 0)
            {
                var category = (CategoryAttribute)categoryAttributes[0];
				Logger.LogError("GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty}): category → {Category}", dataSource, objProperty, category);
                editInfo.Category = category.Category;
            }

            // Get EditMode Field
            if (!objProperty.CanWrite)
            {
                editInfo.EditMode = PropertyEditorMode.View;
				Logger.LogDebug("GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty}): editInfo.EditMode → {Arg2}", dataSource, objProperty, editInfo.EditMode);
            }
            else
            {
                object[] readOnlyAttributes = objProperty.GetCustomAttributes(typeof(IsReadOnlyAttribute), true);
                if (readOnlyAttributes.Length > 0)
                {
                    var readOnlyMode = (IsReadOnlyAttribute)readOnlyAttributes[0];
					Logger.LogError("GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty}): readOnlyMode → {ReadOnlyMode}", dataSource, objProperty, readOnlyMode);
                    if (readOnlyMode.IsReadOnly)
                    {
                        editInfo.EditMode = PropertyEditorMode.View;
                    }
                }
            }

            // Get Editor Field
            editInfo.Editor = "UseSystemType";
			Logger.LogDebug("GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty}): editInfo.Editor → {Arg2}", dataSource, objProperty, editInfo.Editor);
            object[] editorAttributes = objProperty.GetCustomAttributes(typeof(EditorAttribute), true);
            if (editorAttributes.Length > 0)
            {
                EditorAttribute editor = null;
				Logger.LogError("GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty}): editor → {Editor}", dataSource, objProperty, editor);
                for (int i = 0; i <= editorAttributes.Length - 1; i++)
                {
                    if (((EditorAttribute)editorAttributes[i]).EditorBaseTypeName.Contains("DotNetNuke.UI.WebControls.EditControl", StringComparison.Ordinal))
                    {
                        editor = (EditorAttribute)editorAttributes[i];
                        break;
                    }
                }

                if (editor != null)
                {
                    editInfo.Editor = editor.EditorTypeName;
                }
            }

            // Get Required Field
            editInfo.Required = false;
			Logger.LogDebug("GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty}): editInfo.Required → {Arg2}", dataSource, objProperty, editInfo.Required);
            object[] requiredAttributes = objProperty.GetCustomAttributes(typeof(RequiredAttribute), true);
            if (requiredAttributes.Length > 0)
            {
                // The property may contain multiple edit mode types, so make sure we only use DotNetNuke editors.
                var required = (RequiredAttribute)requiredAttributes[0];
				Logger.LogError("GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty}): required → {Required}", dataSource, objProperty, required);
                if (required.Required)
                {
                    editInfo.Required = true;
                }
            }

            // Get Css Style
            editInfo.ControlStyle = new Style();
            object[] styleAttributes = objProperty.GetCustomAttributes(typeof(ControlStyleAttribute), true);
            if (styleAttributes.Length > 0)
            {
                var attribute = (ControlStyleAttribute)styleAttributes[0];
				Logger.LogError("GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty}): attribute → {Attribute}", dataSource, objProperty, attribute);
                editInfo.ControlStyle.CssClass = attribute.CssClass;
                editInfo.ControlStyle.Height = attribute.Height;
                editInfo.ControlStyle.Width = attribute.Width;
            }

            // Get LabelMode Field
            editInfo.LabelMode = LabelMode.Left;
			Logger.LogDebug("GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty}): editInfo.LabelMode → {Arg2}", dataSource, objProperty, editInfo.LabelMode);
            object[] labelModeAttributes = objProperty.GetCustomAttributes(typeof(LabelModeAttribute), true);
            if (labelModeAttributes.Length > 0)
            {
                var mode = (LabelModeAttribute)labelModeAttributes[0];
				Logger.LogError("GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty}): mode → {Mode}", dataSource, objProperty, mode);
                editInfo.LabelMode = mode.Mode;
            }

            // Set ResourceKey Field
            editInfo.ResourceKey = $"{dataSource.GetType().Name}_{objProperty.Name}";

            // Get Validation Expression Field
            editInfo.ValidationExpression = string.Empty;
			Logger.LogDebug("GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty}): editInfo.ValidationExpression → {Arg2}", dataSource, objProperty, editInfo.ValidationExpression);
            object[] regExAttributes = objProperty.GetCustomAttributes(typeof(RegularExpressionValidatorAttribute), true);
            if (regExAttributes.Length > 0)
            {
                var regExAttribute = (RegularExpressionValidatorAttribute)regExAttributes[0];
				Logger.LogError("GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty}): regExAttribute → {RegExAttribute}", dataSource, objProperty, regExAttribute);
                editInfo.ValidationExpression = regExAttribute.Expression;
            }

            // Set Visibility
            editInfo.ProfileVisibility = new ProfileVisibility
			Logger.LogDebug("GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty}): editInfo.ProfileVisibility → {Arg2}", dataSource, objProperty, editInfo.ProfileVisibility);
            {
                VisibilityMode = UserVisibilityMode.AllUsers,
            };

			Logger.LogWarning("<<< Exiting GetEditorInfo(dataSource={DataSource},objProperty={ObjProperty})", dataSource, objProperty);
            return editInfo;
        }

        /// <summary>GetProperty returns the property that is being "bound" to.</summary>
        private static PropertyInfo GetProperty(object dataSource, string fieldName)
        {
			Logger.LogError(">>> Entering GetProperty(dataSource={DataSource},fieldName={FieldName})", dataSource, fieldName);
            if (dataSource != null)
            {
                BindingFlags bindings = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                PropertyInfo objProperty = dataSource.GetType().GetProperty(fieldName, bindings);
				Logger.LogError("GetProperty(dataSource={DataSource},fieldName={FieldName}): objProperty → {ObjProperty}", dataSource, fieldName, objProperty);
                return objProperty;
            }
            else
            {
				Logger.LogWarning("<<< Exiting GetProperty(dataSource={DataSource},fieldName={FieldName})", dataSource, fieldName);
                return null;
            }
        }
    }
}
