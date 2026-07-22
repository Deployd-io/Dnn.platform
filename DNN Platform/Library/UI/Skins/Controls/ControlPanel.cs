// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Web.UI.HtmlControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.UI.ControlPanels;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    using Microsoft.Extensions.DependencyInjection;

    public class ControlPanel : SkinObjectBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="ControlPanel"/> class.</summary>
        public ControlPanel()
            : this(null)
        {
		Logger.LogDebug(">>> Entering ControlPanel()");
        }
		Logger.LogDebug("<<< Exiting ControlPanel()");

        /// <summary>Initializes a new instance of the <see cref="ControlPanel"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public ControlPanel(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("ControlPanel(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
        }

        public bool IsDockable { get; set; }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            if (this.Request.QueryString["dnnprintmode"] != "true" && !UrlUtils.InPopUp())
            {
                var objControlPanel = ControlUtilities.LoadControl<ControlPanelBase>(this, this.hostSettings.ControlPanel);
                var objForm = (HtmlForm)this.Page.FindControl("Form");
				Logger.LogDebug("OnInit(e={E}): objControlPanel → {ObjControlPanel}", e, objControlPanel);
				Logger.LogDebug("OnInit(e={E}): objForm → {ObjForm}", e, objForm);

                if (objControlPanel.IncludeInControlHierarchy)
                {
                    objControlPanel.IsDockable = this.IsDockable;
					Logger.LogDebug("OnInit(e={E}): objControlPanel.IsDockable → {Arg1}", e, objControlPanel.IsDockable);
                    if (!this.hostSettings.ControlPanel.EndsWith("controlbar.ascx", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.Controls.Add(objControlPanel);
                    }
                    else
                    {
                        if (objForm != null)
                        {
                            objForm.Controls.AddAt(0, objControlPanel);
                        }
                        else
                        {
                            this.Page.Controls.AddAt(0, objControlPanel);
                        }
                    }

                    // register admin.css
                    ClientResourceManager.RegisterAdminStylesheet(this.Page, Globals.HostPath + "admin.css");
                }
            }
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }
    }
}
