// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html
{
    using System;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Exceptions;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>  MyWork allows a user to view any outstanding workflow items.</summary>
    public partial class MyWork : PortalModuleBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly INavigationManager navigationManager;

        /// <summary>Initializes a new instance of the <see cref="MyWork"/> class.</summary>
        public MyWork()
        {
			Logger.LogDebug(">>> Entering MyWork()");
            this.navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
			Logger.LogDebug("<<< Exiting MyWork()");
			Logger.LogDebug("MyWork(): navigationManager → {NavigationManager}", navigationManager);
        }

        public string FormatURL(object dataItem)
        {
			Logger.LogDebug(">>> Entering FormatURL(dataItem={DataItem})", dataItem);
            var objHtmlTextUser = (HtmlTextUserInfo)dataItem;
			Logger.LogDebug("FormatURL(dataItem={DataItem}): objHtmlTextUser → {ObjHtmlTextUser}", dataItem, objHtmlTextUser);
			Logger.LogDebug("<<< Exiting FormatURL(dataItem={DataItem})", dataItem);
            return "<a href=\"" + this.navigationManager.NavigateURL(objHtmlTextUser.TabID) + "#" + objHtmlTextUser.ModuleID + "\">" + objHtmlTextUser.ModuleTitle + " ( " + objHtmlTextUser.StateName + " )</a>";
        }

        /// <summary>Page_Load runs when the control is loaded.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            this.hlCancel.NavigateUrl = this.navigationManager.NavigateURL();

            try
            {
                if (!this.Page.IsPostBack)
                {
                    var objHtmlTextUsers = new HtmlTextUserController();
                    this.dgTabs.DataSource = objHtmlTextUsers.GetHtmlTextUser(this.UserInfo.UserID);
					Logger.LogDebug("OnLoad(e={E}): objHtmlTextUsers → {ObjHtmlTextUsers}", e, objHtmlTextUsers);
                    this.dgTabs.DataBind();
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }
    }
}
