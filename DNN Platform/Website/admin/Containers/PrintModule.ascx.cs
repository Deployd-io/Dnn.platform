// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Personalization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Contains the attributes of an Icon.
    /// These are read into the PortalModuleBase collection as attributes for the icons within the module controls.
    /// </summary>
    public partial class PrintModule : ActionBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PrintModule.ascx));
        /// <summary>Initializes a new instance of the <see cref="PrintModule"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public PrintModule()
            : this(null)
        {
		Logger.LogDebug(">>> Entering PrintModule()");
		Logger.LogDebug("<<< Exiting PrintModule()");
        }

        /// <summary>Initializes a new instance of the <see cref="PrintModule"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        public PrintModule(IEventLogger eventLogger)
            : base(eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>())
        {
        }

        public string PrintIcon { get; set; }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            try
            {
                foreach (ModuleAction action in this.Actions)
                {
                    this.DisplayAction(action);
                }

                // set visibility
                if (this.Controls.Count > 0)
                {
                    this.Visible = true;
					Logger.LogDebug("OnLoad(e={E}): this.Visible → {Arg1}", e, this.Visible);
                }
                else
                {
                    this.Visible = false;
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.getMessage());
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        private void DisplayAction(ModuleAction action)
        {
			Logger.LogDebug(">>> Entering DisplayAction(action={Action})", action);
            if (action.CommandName == ModuleActionType.PrintModule)
            {
                if (action.Visible)
                {
                    if ((Personalization.GetUserMode() == PortalSettings.Mode.Edit) || (action.Secure == SecurityAccessLevel.Anonymous || action.Secure == SecurityAccessLevel.View))
                    {
                        if (this.ModuleContext.Configuration.DisplayPrint)
                        {
                            var moduleActionIcon = new ImageButton();
							Logger.LogDebug("DisplayAction(action={Action}): moduleActionIcon → {ModuleActionIcon}", action, moduleActionIcon);
                            if (!string.IsNullOrEmpty(this.PrintIcon))
                            {
                                moduleActionIcon.ImageUrl = this.ModuleContext.Configuration.ContainerPath.Substring(0, this.ModuleContext.Configuration.ContainerPath.LastIndexOf("/", StringComparison.Ordinal) + 1) + this.PrintIcon;
                            }
                            else
                            {
                                moduleActionIcon.ImageUrl = "~/images/" + action.Icon;
								Logger.LogDebug("DisplayAction(action={Action}): moduleActionIcon.ImageUrl → {Arg1}", action, moduleActionIcon.ImageUrl);
                            }

                            moduleActionIcon.ToolTip = action.Title;
							Logger.LogDebug("DisplayAction(action={Action}): moduleActionIcon.ToolTip → {Arg1}", action, moduleActionIcon.ToolTip);
							Logger.LogDebug("DisplayAction(action={Action}): moduleActionIcon.ID → {Arg1}", action, moduleActionIcon.ID);
							Logger.LogDebug("DisplayAction(action={Action}): moduleActionIcon.CausesValidation → {Arg1}", action, moduleActionIcon.CausesValidation);
                            moduleActionIcon.ID = "ico" + action.ID;
                            moduleActionIcon.CausesValidation = false;

                            moduleActionIcon.Click += this.IconAction_Click;

                            this.Controls.Add(moduleActionIcon);
                        }
                    }
                }
            }

            foreach (ModuleAction subAction in action.Actions)
            {
                this.DisplayAction(subAction);
            }
			Logger.LogDebug("<<< Exiting DisplayAction(action={Action})", action);
        }

        private void IconAction_Click(object sender, ImageClickEventArgs e)
        {
			Logger.LogDebug(">>> Entering IconAction_Click(sender={Sender},e={E})", sender, e);
            try
            {
                this.ProcessAction(((ImageButton)sender).ID.Substring(3));
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in IconAction_Click(sender={Sender},e={E}): {Arg2}", sender, e, exc.getMessage());
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting IconAction_Click(sender={Sender},e={E})", sender, e);
        }
    }
}
