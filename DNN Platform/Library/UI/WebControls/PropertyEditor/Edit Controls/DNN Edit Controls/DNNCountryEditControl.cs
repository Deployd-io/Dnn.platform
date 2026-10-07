// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;

    using DotNetNuke.Common.Lists;

    /// <summary>The DNNCountryEditControl control provides a standard UI component for editing Countries.</summary>
    [ToolboxData("<{0}:DNNCountryEditControl runat=server></{0}:DNNCountryEditControl>")]
    public class DNNCountryEditControl : DNNListEditControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DNNCountryEditControl));
        private readonly ListController listController;

        /// <summary>Initializes a new instance of the <see cref="DNNCountryEditControl"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public DNNCountryEditControl()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DNNCountryEditControl"/> class.</summary>
        /// <param name="listController">The list controller.</param>
        public DNNCountryEditControl(ListController listController)
        {
			Logger.LogError(">>> Entering DNNCountryEditControl(listController={ListController})", listController);
            this.listController = listController;
			Logger.LogWarning("<<< Exiting DNNCountryEditControl(listController={ListController})", listController);
			Logger.LogDebug("DNNCountryEditControl(listController={ListController}): this.listController → {Arg1}", listController, this.listController);
            this.AutoPostBack = true;
            this.ListName = "Country";
			Logger.LogDebug("DNNCountryEditControl(listController={ListController}): this.AutoPostBack → {Arg1}", listController, this.AutoPostBack);
			Logger.LogDebug("DNNCountryEditControl(listController={ListController}): this.ListName → {Arg1}", listController, this.ListName);
            this.ParentKey = string.Empty;
            this.TextField = ListBoundField.Text;
            this.ValueField = ListBoundField.Id;
            this.ItemChanged += this.OnItemChanged;
			Logger.LogDebug("DNNCountryEditControl(listController={ListController}): this.ParentKey → {Arg1}", listController, this.ParentKey);
            this.SortAlphabetically = true;
        }

        private void OnItemChanged(object sender, PropertyEditorEventArgs e)
        {
			Logger.LogError(">>> Entering OnItemChanged(sender={Sender},e={E})", sender, e);
            var regionContainer = ControlUtilities.FindControl<Control>(this.Parent, "Region", true);
			Logger.LogError("OnItemChanged(sender={Sender},e={E}): regionContainer → {RegionContainer}", sender, e, regionContainer);
            if (regionContainer != null)
            {
                var regionControl = ControlUtilities.FindFirstDescendent<DNNRegionEditControl>(regionContainer);
				Logger.LogError("OnItemChanged(sender={Sender},e={E}): regionControl → {RegionControl}", sender, e, regionControl);
                if (regionControl != null)
                {
                    var countries = this.listController.GetListEntryInfoItems("Country");
					Logger.LogError("OnItemChanged(sender={Sender},e={E}): countries → {Countries}", sender, e, countries);
                    foreach (var checkCountry in countries)
                    {
                        if (checkCountry.EntryID.ToString(CultureInfo.InvariantCulture) == e.StringValue)
                        {
                            var attributes = new object[1];
                            attributes[0] = new ListAttribute("Region", "Country." + checkCountry.Value, ListBoundField.Id, ListBoundField.Text);
                            regionControl.CustomAttributes = attributes;
							Logger.LogDebug("OnItemChanged(sender={Sender},e={E}): regionControl.CustomAttributes → {Arg2}", sender, e, regionControl.CustomAttributes);
                            break;
                        }
                    }
                }
            }
			Logger.LogWarning("<<< Exiting OnItemChanged(sender={Sender},e={E})", sender, e);
        }
    }
}
