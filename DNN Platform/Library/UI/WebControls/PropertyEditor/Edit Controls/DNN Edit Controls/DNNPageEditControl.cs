// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;

    /// <summary>
    /// The DNNPageEditControl control provides a standard UI component for selecting
    /// a DNN Page.
    /// </summary>
    [ToolboxData("<{0}:DNNPageEditControl runat=server></{0}:DNNPageEditControl>")]
    public class DNNPageEditControl : IntegerEditControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>RenderEditMode renders the Edit mode of the control.</summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderEditMode(writer={Writer})", writer);
            PortalSettings portalSettings = Globals.GetPortalSettings();

			Logger.LogDebug("RenderEditMode(writer={Writer}): portalSettings → {PortalSettings}", writer, portalSettings);
            // Get the Pages
            List<TabInfo> listTabs = TabController.GetPortalTabs(portalSettings.PortalId, Null.NullInteger, true, "<" + Localization.GetString("None_Specified") + ">", true, false, true, true, false);

			Logger.LogDebug("RenderEditMode(writer={Writer}): listTabs → {ListTabs}", writer, listTabs);
            // Render the Select Tag
            this.ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.RenderBeginTag(HtmlTextWriterTag.Select);

            for (int tabIndex = 0; tabIndex <= listTabs.Count - 1; tabIndex++)
            {
                TabInfo tab = listTabs[tabIndex];

                // Add the Value Attribute
                writer.AddAttribute(HtmlTextWriterAttribute.Value, tab.TabID.ToString(CultureInfo.InvariantCulture));

                if (tab.TabID == this.IntegerValue)
                {
                    // Add the Selected Attribute
                    writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
                }

                // Render Option Tag
                writer.RenderBeginTag(HtmlTextWriterTag.Option);
                writer.Write(tab.IndentedTabName);
                writer.RenderEndTag();
            }

            // Close Select Tag
            writer.RenderEndTag();
			Logger.LogDebug("<<< Exiting RenderEditMode(writer={Writer})", writer);
        }

        /// <summary>RenderViewMode renders the View mode of the control.</summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderViewMode(writer={Writer})", writer);
            TabInfo linkedTabInfo = TabController.Instance.GetTab(this.IntegerValue, Globals.GetPortalSettings().PortalId, false);

			Logger.LogDebug("RenderViewMode(writer={Writer}): linkedTabInfo → {LinkedTabInfo}", writer, linkedTabInfo);
            // don't render anything if we didn't find the tab
            if (linkedTabInfo == null)
            {
                return;
            }

            // Not really sure how to get a good TabID and ModuleID, but it's only for tracking so not to concerned
            if (!int.TryParse(this.Page.Request.QueryString["tabid"], out var tabId))
            {
                tabId = Null.NullInteger;
				Logger.LogDebug("RenderViewMode(writer={Writer}): tabId → {TabId}", writer, tabId);
            }

            if (!int.TryParse(this.Page.Request.QueryString["mid"], out var moduleId))
            {
                moduleId = Null.NullInteger;
				Logger.LogDebug("RenderViewMode(writer={Writer}): moduleId → {ModuleId}", writer, moduleId);
            }

            string url = Globals.LinkClick(this.StringValue, tabId, moduleId, true);
            this.ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Href, url);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "Normal");
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.Write(linkedTabInfo.LocalizedTabName);
            writer.RenderEndTag();
			Logger.LogDebug("<<< Exiting RenderViewMode(writer={Writer})", writer);
        }
    }
}
