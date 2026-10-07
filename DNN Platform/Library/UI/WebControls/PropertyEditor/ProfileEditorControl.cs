// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Security;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The ProfileEditorControl control provides a Control to display Profile Properties.</summary>
    /// <param name="serviceProvider">The DI container.</param>
    /// <param name="listController">The list controller.</param>
    [ToolboxData("<{0}:ProfileEditorControl runat=server></{0}:ProfileEditorControl>")]
    public class ProfileEditorControl(IServiceProvider serviceProvider, ListController listController)
        : CollectionEditorControl(serviceProvider)
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ProfileEditorControl));
        private readonly ListController listController = listController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ListController>();

        /// <summary>Initializes a new instance of the <see cref="ProfileEditorControl"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public ProfileEditorControl()
            : this(Globals.GetCurrentServiceProvider())
        {
		Logger.LogError(">>> Entering ProfileEditorControl()");
        Logger.LogWarning("<<< Exiting ProfileEditorControl()");
        }

        /// <summary>Initializes a new instance of the <see cref="ProfileEditorControl"/> class.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public ProfileEditorControl(IServiceProvider serviceProvider)
            : this(serviceProvider, null)
        {
        }

        /// <summary>CreateEditor creates the control collection.</summary>
        protected override void CreateEditor()
        {
			Logger.LogError(">>> Entering CreateEditor()");
            this.CategoryDataField = "PropertyCategory";
            this.EditorDataField = "DataType";
			Logger.LogDebug("CreateEditor(): this.CategoryDataField → {Arg0}", this.CategoryDataField);
			Logger.LogDebug("CreateEditor(): this.EditorDataField → {Arg0}", this.EditorDataField);
            this.NameDataField = "PropertyName";
            this.RequiredDataField = "Required";
            this.ValidationExpressionDataField = "ValidationExpression";
			Logger.LogDebug("CreateEditor(): this.NameDataField → {Arg0}", this.NameDataField);
			Logger.LogDebug("CreateEditor(): this.RequiredDataField → {Arg0}", this.RequiredDataField);
			Logger.LogDebug("CreateEditor(): this.ValidationExpressionDataField → {Arg0}", this.ValidationExpressionDataField);
            this.ValueDataField = "PropertyValue";
            this.VisibleDataField = "Visible";
            this.VisibilityDataField = "ProfileVisibility";
            this.LengthDataField = "Length";

            base.CreateEditor();
			Logger.LogDebug("CreateEditor(): this.ValueDataField → {Arg0}", this.ValueDataField);

            foreach (FieldEditorControl editor in this.Fields)
            {
				Logger.LogDebug("CreateEditor(): this.VisibleDataField → {Arg0}", this.VisibleDataField);
				Logger.LogDebug("CreateEditor(): this.VisibilityDataField → {Arg0}", this.VisibilityDataField);
				Logger.LogDebug("CreateEditor(): this.LengthDataField → {Arg0}", this.LengthDataField);
                // Check whether Field is readonly
                string fieldName = editor.Editor.Name;
                ProfilePropertyDefinitionCollection definitions = editor.DataSource as ProfilePropertyDefinitionCollection;
                ProfilePropertyDefinition definition = definitions[fieldName];

                if (definition is { ReadOnly: true, } && editor.Editor.EditMode == PropertyEditorMode.Edit)
                {
                    var ps = PortalController.Instance.GetCurrentSettings();
					Logger.LogError("CreateEditor(): ps → {Ps}", ps);
                    if (!PortalSecurity.IsInRole(ps.AdministratorRoleName))
                    {
                        editor.Editor.EditMode = PropertyEditorMode.View;
                    }
                }

                // We need to wire up the RegionControl to the CountryControl
                if (editor.Editor is DNNRegionEditControl)
                {
                    string country = null;
					Logger.LogError("CreateEditor(): country → {Country}", country);

                    foreach (FieldEditorControl checkEditor in this.Fields)
                    {
                        if (checkEditor.Editor is DNNCountryEditControl countryEdit)
                        {
                            if (editor.Editor.Category == countryEdit.Category)
                            {
                                country = Convert.ToString(countryEdit.Value, CultureInfo.InvariantCulture);
                            }
                        }
                    }

                    // Create a ListAttribute for the Region
                    string countryKey = "Unknown";
					Logger.LogError("CreateEditor(): countryKey → {CountryKey}", countryKey);
                    if (int.TryParse(country, out var entryId))
                    {
                        ListEntryInfo item = this.listController.GetListEntryInfo(entryId);
						Logger.LogError("CreateEditor(): item → {Item}", item);
                        if (item != null)
                        {
                            countryKey = item.Value;
                        }
                    }

                    countryKey = "Country." + countryKey;
                    var attributes = new object[1];
                    attributes[0] = new ListAttribute("Region", countryKey, ListBoundField.Id, ListBoundField.Text);
                    editor.Editor.CustomAttributes = attributes;
                }
            }
			Logger.LogWarning("<<< Exiting CreateEditor()");
        }
    }
}
