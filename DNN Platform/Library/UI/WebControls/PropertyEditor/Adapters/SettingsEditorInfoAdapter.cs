// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;

    /// <summary>
    /// The SettingsEditorInfoAdapter control provides a factory for creating the
    /// appropriate EditInfo object.
    /// </summary>
    public class SettingsEditorInfoAdapter : IEditorInfoAdapter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SettingsEditorInfoAdapter));
        private readonly object dataMember;
        private readonly object dataSource;
        private string fieldName;

        /// <summary>Initializes a new instance of the <see cref="SettingsEditorInfoAdapter"/> class.</summary>
        /// <param name="dataSource">The data source <see cref="Hashtable"/>.</param>
        /// <param name="dataMember">The <see cref="SettingInfo"/> instance.</param>
        /// <param name="fieldName">The field name.</param>
        public SettingsEditorInfoAdapter(object dataSource, object dataMember, string fieldName)
        {
			Logger.LogDebug(">>> Entering SettingsEditorInfoAdapter(dataSource={DataSource},dataMember={DataMember},fieldName={FieldName})", dataSource, dataMember, fieldName);
            this.dataMember = dataMember;
            this.dataSource = dataSource;
			Logger.LogDebug("SettingsEditorInfoAdapter(dataSource={DataSource},dataMember={DataMember},fieldName={FieldName}): this.dataMember → {Arg3}", dataSource, dataMember, fieldName, this.dataMember);
			Logger.LogDebug("SettingsEditorInfoAdapter(dataSource={DataSource},dataMember={DataMember},fieldName={FieldName}): this.dataSource → {Arg3}", dataSource, dataMember, fieldName, this.dataSource);
			Logger.LogDebug("SettingsEditorInfoAdapter(dataSource={DataSource},dataMember={DataMember},fieldName={FieldName}): dataSource → {DataSource1}", dataSource, dataMember, fieldName, dataSource);
            this.fieldName = fieldName;
			Logger.LogDebug("<<< Exiting SettingsEditorInfoAdapter(dataSource={DataSource},dataMember={DataMember},fieldName={FieldName})", dataSource, dataMember, fieldName);
        }

        /// <inheritdoc />
        public EditorInfo CreateEditControl()
        {
			Logger.LogDebug(">>> Entering CreateEditControl()");
            var info = (SettingInfo)this.dataMember;
            var editInfo = new EditorInfo();
			Logger.LogDebug("CreateEditControl(): editInfo → {EditInfo}", editInfo);
			Logger.LogDebug("CreateEditControl(): info → {Info}", info);

            // Get the Name of the property
            editInfo.Name = info.Name;
			Logger.LogDebug("CreateEditControl(): editInfo.Name → {Arg0}", editInfo.Name);

            editInfo.Category = string.Empty;
			Logger.LogDebug("CreateEditControl(): editInfo.Category → {Arg0}", editInfo.Category);

            // Get Value Field
            editInfo.Value = info.Value;
			Logger.LogDebug("CreateEditControl(): editInfo.Value → {Arg0}", editInfo.Value);

            // Get the type of the property
            editInfo.Type = info.Type.AssemblyQualifiedName;
			Logger.LogDebug("CreateEditControl(): editInfo.Type → {Arg0}", editInfo.Type);

            // Get Editor Field
            editInfo.Editor = info.Editor;
			Logger.LogDebug("CreateEditControl(): editInfo.Editor → {Arg0}", editInfo.Editor);

            // Get LabelMode Field
            editInfo.LabelMode = LabelMode.Left;
			Logger.LogDebug("CreateEditControl(): editInfo.LabelMode → {Arg0}", editInfo.LabelMode);

            // Get Required Field
            editInfo.Required = false;
			Logger.LogDebug("CreateEditControl(): editInfo.Required → {Arg0}", editInfo.Required);

            // Set ResourceKey Field
            editInfo.ResourceKey = editInfo.Name;
			Logger.LogDebug("CreateEditControl(): editInfo.ResourceKey → {Arg0}", editInfo.ResourceKey);

            // Get Style
            editInfo.ControlStyle = new Style();

            // Get Validation Expression Field
            editInfo.ValidationExpression = string.Empty;
			Logger.LogDebug("CreateEditControl(): editInfo.ValidationExpression → {Arg0}", editInfo.ValidationExpression);

			Logger.LogDebug("<<< Exiting CreateEditControl()");
            return editInfo;
        }

        /// <inheritdoc />
        public bool UpdateValue(PropertyEditorEventArgs e)
        {
			Logger.LogDebug(">>> Entering UpdateValue(e={E})", e);
            string name = e.Name;
            bool changed = e.Changed;
            object oldValue = e.OldValue;
            object newValue = e.Value;
            bool isDirty = Null.NullBoolean;
			Logger.LogDebug("UpdateValue(e={E}): isDirty → {IsDirty}", e, isDirty);

            var settings = (Hashtable)this.dataSource;
			Logger.LogDebug("UpdateValue(e={E}): settings → {Settings}", e, settings);
            var settingsEnumerator = settings.GetEnumerator();
			Logger.LogDebug("UpdateValue(e={E}): settingsEnumerator → {SettingsEnumerator}", e, settingsEnumerator);
            while (settingsEnumerator.MoveNext())
            {
                var key = Convert.ToString(settingsEnumerator.Key, CultureInfo.InvariantCulture);
				Logger.LogWarning("UpdateValue(e={E}): key → {Key}", e, key);

                // Do we have the item in the Hashtable being changed
                if (key == name)
                {
                    // Set the Value property to the new value
                    if ((!ReferenceEquals(newValue, oldValue)) || changed)
                    {
                        settings[key] = newValue;
                        isDirty = true;
                        break;
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
			Logger.LogDebug("<<< Exiting UpdateVisibility(e={E})", e);
            return false;
        }
    }
}
