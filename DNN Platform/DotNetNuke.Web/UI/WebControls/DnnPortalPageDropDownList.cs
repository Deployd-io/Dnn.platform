// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.UI.WebControls.Extensions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A portal page dropdown control.</summary>
    [ToolboxData("<{0}:DnnPortalPageDropDownList runat='server'></{0}:DnnPortalPageDropDownList>")]
    public class DnnPortalPageDropDownList : DnnDropDownList
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnPortalPageDropDownList));
        private readonly Lazy<int> portalId = new Lazy<int>(() => PortalSettings.Current.ActiveTab.IsSuperTab ? -1 : PortalSettings.Current.PortalId);

        /// <summary>Initializes a new instance of the <see cref="DnnPortalPageDropDownList"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public DnnPortalPageDropDownList()
            : this(Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>(), Globals.GetCurrentServiceProvider().GetRequiredService<IServicesFramework>())
        {
		Logger.LogError(">>> Entering DnnPortalPageDropDownList()");
        Logger.LogWarning("<<< Exiting DnnPortalPageDropDownList()");
        }

        /// <summary>Initializes a new instance of the <see cref="DnnPortalPageDropDownList"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        public DnnPortalPageDropDownList(IClientResourceController clientResourceController, IServicesFramework servicesFramework)
            : base(clientResourceController, servicesFramework)
        {
        }

        /// <summary>Gets or sets the selected Page in the control, or selects the Page in the control.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TabInfo SelectedPage
        {
            get
            {
                var pageId = this.SelectedItemValueAsInt;
                return (pageId == Null.NullInteger) ? null : TabController.Instance.GetTab(pageId, this.portalId.Value, false);
            }

            set
            {
                this.SelectedItem = (value != null) ? new ListItem() { Text = value.IndentedTabName, Value = value.TabID.ToString(CultureInfo.InvariantCulture) } : null;
            }
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.SelectItemDefaultText = Localization.GetString("DropDownList.SelectWebPageDefaultText", Localization.SharedResourceFile);
            this.Services.GetTreeMethod = "ItemListService/GetPagesInPortalGroup";
            this.Services.GetNodeDescendantsMethod = "ItemListService/GetPageDescendantsInPortalGroup";
            this.Services.SearchTreeMethod = "ItemListService/SearchPagesInPortalGroup";
            this.Services.GetTreeWithNodeMethod = "ItemListService/GetTreePathForPageInPortalGroup";
            this.Services.SortTreeMethod = "ItemListService/SortPagesInPortalGroup";
            this.Services.ServiceRoot = "InternalServices";
			Logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogError(">>> Entering OnPreRender(e={E})", e);
            this.AddCssClass("page");
            base.OnPreRender(e);
			Logger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
        }
    }
}
