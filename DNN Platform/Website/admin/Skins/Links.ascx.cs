// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Tabs;

    /// <summary>A skin/theme object which displays a list of page links.</summary>
    public partial class Links : SkinObjectBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly Regex SrcRegex = new Regex("src=[']?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private string alignment;
        private bool forceLinks = true;
        private bool includeActiveTab = true;
        private string level;

        public string Alignment
        {
            get
            {
                return this.alignment;
            }

            set
            {
                this.alignment = value.ToLowerInvariant();
            }
        }

        public string CssClass { get; set; }

        public string Level
        {
            get
            {
                return this.level;
            }

            set
            {
                this.level = value.ToLowerInvariant();
            }
        }

        public string Separator { get; set; }

        public bool ShowDisabled { get; set; }

        public bool ForceLinks
        {
            get
            {
                return this.forceLinks;
            }

            set
            {
                this.forceLinks = value;
            }
        }

        public bool IncludeActiveTab
        {
            get
            {
                return this.includeActiveTab;
            }

            set
            {
                this.includeActiveTab = value;
            }
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            string strCssClass;
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                strCssClass = this.CssClass;
				Logger.LogDebug("OnLoad(e={E}): strCssClass → {StrCssClass}", e, strCssClass);
            }
            else
            {
                strCssClass = "SkinObject";
            }

            string strSeparator = string.Empty;
            if (!string.IsNullOrEmpty(this.Separator))
            {
                if (this.Separator.IndexOf("src=", StringComparison.Ordinal) != -1)
                {
                    // Add the skinpath to image paths
                    this.Separator = SrcRegex.Replace(this.Separator, "$&" + this.PortalSettings.ActiveTab.SkinPath);
                }

                // Wrap in a span
                this.Separator = string.Format("<span class=\"{0}\">{1}</span>", strCssClass, this.Separator);
            }
            else
            {
                this.Separator = " ";
				Logger.LogDebug("OnLoad(e={E}): this.Separator → {Arg1}", e, this.Separator);
            }

            // build links
            string strLinks = string.Empty;

			Logger.LogDebug("OnLoad(e={E}): strLinks → {StrLinks}", e, strLinks);
            strLinks = this.BuildLinks(this.Level, strSeparator, strCssClass);

            // Render links, even if nothing is returned with the currently set level
            if (string.IsNullOrEmpty(strLinks) && this.ForceLinks)
            {
                strLinks = this.BuildLinks(string.Empty, strSeparator, strCssClass);
            }

            this.lblLinks.Text = strLinks;
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        private string BuildLinks(string strLevel, string strSeparator, string strCssClass)
        {
			Logger.LogDebug(">>> Entering BuildLinks(strLevel={StrLevel},strSeparator={StrSeparator},strCssClass={StrCssClass})", strLevel, strSeparator, strCssClass);
            var sbLinks = new StringBuilder();

			Logger.LogDebug("BuildLinks(strLevel={StrLevel},strSeparator={StrSeparator},strCssClass={StrCssClass}): sbLinks → {SbLinks}", strLevel, strSeparator, strCssClass, sbLinks);
            List<TabInfo> portalTabs = TabController.GetTabsBySortOrder(this.PortalSettings.PortalId);
            List<TabInfo> hostTabs = TabController.GetTabsBySortOrder(Null.NullInteger);
			Logger.LogDebug("BuildLinks(strLevel={StrLevel},strSeparator={StrSeparator},strCssClass={StrCssClass}): portalTabs → {PortalTabs}", strLevel, strSeparator, strCssClass, portalTabs);
			Logger.LogDebug("BuildLinks(strLevel={StrLevel},strSeparator={StrSeparator},strCssClass={StrCssClass}): hostTabs → {HostTabs}", strLevel, strSeparator, strCssClass, hostTabs);

            foreach (TabInfo objTab in portalTabs)
            {
                sbLinks.Append(this.ProcessLink(this.ProcessTab(objTab, strLevel, strCssClass), sbLinks.ToString().Length));
            }

            foreach (TabInfo objTab in hostTabs)
            {
                sbLinks.Append(this.ProcessLink(this.ProcessTab(objTab, strLevel, strCssClass), sbLinks.ToString().Length));
            }

			Logger.LogDebug("<<< Exiting BuildLinks(strLevel={StrLevel},strSeparator={StrSeparator},strCssClass={StrCssClass})", strLevel, strSeparator, strCssClass);
            return sbLinks.ToString();
        }

        private string ProcessTab(TabInfo objTab, string strLevel, string strCssClass)
        {
			Logger.LogDebug(">>> Entering ProcessTab(objTab={ObjTab},strLevel={StrLevel},strCssClass={StrCssClass})", objTab, strLevel, strCssClass);
            if (Navigation.CanShowTab(objTab, this.AdminMode, this.ShowDisabled))
            {
                switch (strLevel)
                {
                    case "same": // Render tabs on the same level as the current tab
                    case "":
                        if (objTab.ParentId == this.PortalSettings.ActiveTab.ParentId)
                        {
                            if (this.IncludeActiveTab || objTab.TabID != this.PortalSettings.ActiveTab.TabID)
                            {
                                return this.AddLink(objTab.TabName, objTab.FullUrl, strCssClass);
                            }
                        }

                        break;
                    case "child": // Render the current tabs child tabs
                        if (objTab.ParentId == this.PortalSettings.ActiveTab.TabID)
                        {
                            return this.AddLink(objTab.TabName, objTab.FullUrl, strCssClass);
                        }

                        break;
                    case "parent": // Render the current tabs parenttab
                        if (objTab.TabID == this.PortalSettings.ActiveTab.ParentId)
                        {
                            return this.AddLink(objTab.TabName, objTab.FullUrl, strCssClass);
                        }

                        break;
                    case "root": // Render Root tabs
                        if (objTab.Level == 0)
                        {
                            return this.AddLink(objTab.TabName, objTab.FullUrl, strCssClass);
                        }

                        break;
                }
            }

			Logger.LogDebug("<<< Exiting ProcessTab(objTab={ObjTab},strLevel={StrLevel},strCssClass={StrCssClass})", objTab, strLevel, strCssClass);
            return string.Empty;
        }

        private string ProcessLink(string sLink, int iLinksLength)
        {
			Logger.LogDebug(">>> Entering ProcessLink(sLink={SLink},iLinksLength={ILinksLength})", sLink, iLinksLength);
            // wrap in a div if set to vertical
            if (string.IsNullOrEmpty(sLink))
            {
                return string.Empty;
            }

            if (this.Alignment == "vertical")
            {
                sLink = string.Concat("<div>", this.Separator, sLink, "</div>");
            }
			Logger.LogDebug("ProcessLink(sLink={SLink},iLinksLength={ILinksLength}): sLink → {SLink1}", sLink, iLinksLength, sLink);
            else if (!string.IsNullOrEmpty(this.Separator) && iLinksLength > 0)
            {
                // If not vertical, then render the separator
                sLink = string.Concat(this.Separator, sLink);
            }

			Logger.LogDebug("<<< Exiting ProcessLink(sLink={SLink},iLinksLength={ILinksLength})", sLink, iLinksLength);
            return sLink;
        }

        private string AddLink(string strTabName, string strURL, string strCssClass)
        {
			Logger.LogDebug(">>> Entering AddLink(strTabName={StrTabName},strURL={StrURL},strCssClass={StrCssClass})", strTabName, strURL, strCssClass);
			Logger.LogDebug("<<< Exiting AddLink(strTabName={StrTabName},strURL={StrURL},strCssClass={StrCssClass})", strTabName, strURL, strCssClass);
            return string.Format("<a class=\"{0}\" href=\"{1}\">{2}</a>", strCssClass, strURL, strTabName);
        }
    }
}
