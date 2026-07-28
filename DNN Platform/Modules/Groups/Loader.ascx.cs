// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Modules.Groups.Components;

    /// <summary>Display the configured group view.</summary>
    public partial class Loader : GroupsModuleBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        protected void Page_Load(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering Page_Load(sender={Sender},e={E})", sender, e);
            string path = Constants.ModulePath;
            switch (this.LoadView)
            {
                case GroupMode.Setup:
                    path += "Setup.ascx";
                    break;
                case GroupMode.List:
                    path += "List.ascx";
                    break;
                case GroupMode.View:
                    path += "GroupView.ascx";
                    break;
            }

            GroupsModuleBase ctl = new GroupsModuleBase();
            ctl = (GroupsModuleBase)this.LoadControl(path);
			Logger.LogDebug("Page_Load(sender={Sender},e={E}): ctl → {Ctl}", sender, e, ctl);
			Logger.LogDebug("Page_Load(sender={Sender},e={E}): ctl.ModuleConfiguration → {Arg2}", sender, e, ctl.ModuleConfiguration);
            ctl.ModuleConfiguration = this.ModuleConfiguration;
            this.plhContent.Controls.Add(ctl);
			Logger.LogDebug("<<< Exiting Page_Load(sender={Sender},e={E})", sender, e);
        }
    }
}
