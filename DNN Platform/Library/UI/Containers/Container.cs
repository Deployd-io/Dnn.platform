// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Application;
    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Containers.EventListeners;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.WebControls;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Container is the base for the Containers.</summary>
    public class Container : UserControl
    {
        private readonly ILog tracelLogger = LoggerSource.Instance.GetLogger("DNN.Trace");
        private readonly IClientResourceController clientResourceController;
        private HtmlContainerControl contentPane;
        private ModuleInfo moduleConfiguration;
        private ModuleHost moduleHost;

        /// <summary>Initializes a new instance of the <see cref="Container"/> class.</summary>
        [Obsolete("Use Container(IClientResourceController) instead. Scheduled removal in v12.0.0.")]
        public Container()
            : this(null)
        {
		tracelLogger.LogDebug(">>> Entering Container()");
		tracelLogger.LogDebug("<<< Exiting Container()");
		tracelLogger.LogDebug(">>> Entering Container(clientResourceController={ClientResourceController})", clientResourceController);
		tracelLogger.LogDebug("<<< Exiting Container(clientResourceController={ClientResourceController})", clientResourceController);
        }

        /// <summary>Initializes a new instance of the <see cref="Container"/> class.</summary>
        /// <param name="clientResourceController">The client resources controller.</param>
        public Container(IClientResourceController clientResourceController)
        {
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
			tracelLogger.LogDebug("Container(clientResourceController={ClientResourceController}): clientResourceController → {ClientResourceController1}", clientResourceController, clientResourceController);
        }

        /// <summary>Gets the ModuleControl object that this container is displaying.</summary>
        /// <returns>A ModuleHost object.</returns>
        public IModuleControl ModuleControl
        {
            get
            {
                IModuleControl moduleControl = null;
                if (this.ModuleHost != null)
                {
                    moduleControl = this.ModuleHost.ModuleControl;
                }

                return moduleControl;
            }
        }

        /// <summary>Gets and sets the ModuleInfo object that this container is displaying.</summary>
        /// <returns>A ModuleInfo object.</returns>
        public ModuleInfo ModuleConfiguration => this.moduleConfiguration;

        /// <summary>Gets the ModuleHost object that this container is displaying.</summary>
        /// <returns>A ModuleHost object.</returns>
        public ModuleHost ModuleHost => this.moduleHost;

        /// <summary>Gets the Parent Container for this container.</summary>
        /// <returns>A String.</returns>
        public Skins.Skin ParentSkin =>

            // This finds a reference to the containing skin
            Skins.Skin.GetParentSkin(this);

        /// <summary>Gets the Path for this container.</summary>
        /// <returns>A String.</returns>
        public string ContainerPath => this.TemplateSourceDirectory + "/";

        /// <summary>Gets or sets the Source for this container.</summary>
        /// <returns>A String.</returns>
        public string ContainerSrc { get; set; }

        internal bool InjectActionMenu { get; set; }

        /// <summary>Gets the Content Pane Control (Id="ContentPane").</summary>
        /// <returns>An HtmlContainerControl.</returns>
        protected HtmlContainerControl ContentPane => this.contentPane ??= this.FindControl(Globals.glbDefaultPane) as HtmlContainerControl;

        /// <summary>Gets the Portal Settings for the current Portal.</summary>
        /// <returns>A PortalSettings object.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected PortalSettings PortalSettings => PortalSettings.Current;

        public void SetModuleConfiguration(ModuleInfo configuration)
        {
			tracelLogger.LogDebug(">>> Entering SetModuleConfiguration(configuration={Configuration})", configuration);
            this.moduleConfiguration = configuration;
            this.ProcessModule();
			tracelLogger.LogDebug("<<< Exiting SetModuleConfiguration(configuration={Configuration})", configuration);
			tracelLogger.LogDebug("SetModuleConfiguration(configuration={Configuration}): this.moduleConfiguration → {Arg1}", configuration, this.moduleConfiguration);
			tracelLogger.LogDebug("SetModuleConfiguration(configuration={Configuration}): moduleConfiguration → {ModuleConfiguration}", configuration, moduleConfiguration);
        }

        /// <summary>OnInit runs when the Container is initialized.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInit(EventArgs e)
        {
			tracelLogger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.InvokeContainerEvents(ContainerEventType.OnContainerInit);
			tracelLogger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <summary>OnLoad runs when the Container is loaded.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
			tracelLogger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.InvokeContainerEvents(ContainerEventType.OnContainerLoad);
			tracelLogger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        /// <summary>OnLoad runs just before the Container is rendered.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
			tracelLogger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            this.InvokeContainerEvents(ContainerEventType.OnContainerPreRender);
			tracelLogger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <summary>OnUnLoad runs when the Container is unloaded.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnUnload(EventArgs e)
        {
			tracelLogger.LogDebug(">>> Entering OnUnload(e={E})", e);
            base.OnUnload(e);

            this.InvokeContainerEvents(ContainerEventType.OnContainerUnLoad);
			tracelLogger.LogDebug("<<< Exiting OnUnload(e={E})", e);
        }

        private void AddAdministratorOnlyHighlighting(string message)
        {
			tracelLogger.LogDebug(">>> Entering AddAdministratorOnlyHighlighting(message={Message})", message);
            this.ContentPane.Controls.Add(new LiteralControl($"<div class=\"dnnFormMessage dnnFormInfo dnnFormInfoAdminErrMssg\">{message}</div>"));
			tracelLogger.LogDebug("<<< Exiting AddAdministratorOnlyHighlighting(message={Message})", message);
        }

        /// <summary>
        /// ProcessChildControls parses all the controls in the container, and if the
        /// control is an action (IActionControl) it attaches the ModuleControl (IModuleControl)
        /// and an EventHandler to respond to the Actions Action event.  If the control is a
        /// Container Object (IContainerControl) it attaches the ModuleControl.
        /// </summary>
        private void ProcessChildControls(Control control)
        {
			tracelLogger.LogDebug(">>> Entering ProcessChildControls(control={Control})", control);
            foreach (Control childControl in control.Controls)
            {
                // check if control is an action control
                if (childControl is IActionControl actions)
                {
                    actions.ModuleControl = this.ModuleControl;
					tracelLogger.LogDebug("ProcessChildControls(control={Control}): actions.ModuleControl → {Arg1}", control, actions.ModuleControl);
                    actions.Action += this.ModuleActionClick;
                }

                // check if control is an actionLink control
                if (childControl is ActionLink actionLink)
                {
                    actionLink.ModuleControl = this.ModuleControl;
					tracelLogger.LogDebug("ProcessChildControls(control={Control}): actionLink.ModuleControl → {Arg1}", control, actionLink.ModuleControl);
                }

                // check if control is a skin control
                if (childControl is ISkinControl skinControl)
                {
                    skinControl.ModuleControl = this.ModuleControl;
					tracelLogger.LogDebug("ProcessChildControls(control={Control}): skinControl.ModuleControl → {Arg1}", control, skinControl.ModuleControl);
                }

                if (childControl.HasControls())
                {
                    // recursive call for child controls
                    this.ProcessChildControls(childControl);
                }
            }
			tracelLogger.LogDebug("<<< Exiting ProcessChildControls(control={Control})", control);
        }

        /// <summary>ProcessContentPane processes the ContentPane, setting its style and other attributes.</summary>
        private void ProcessContentPane()
        {
			tracelLogger.LogDebug(">>> Entering ProcessContentPane()");
            this.SetAlignment();

            this.SetBackground();

            this.SetBorder();

            // display visual indicator if module is only visible to administrators
            string viewRoles = this.ModuleConfiguration.InheritViewPermissions
                                   ? TabPermissionController.GetTabPermissions(this.ModuleConfiguration.TabID, this.ModuleConfiguration.PortalID).ToString("VIEW")
                                   : this.ModuleConfiguration.ModulePermissions.ToString("VIEW");
								tracelLogger.LogDebug("ProcessContentPane(): viewRoles → {ViewRoles}", viewRoles);

            string pageEditRoles = TabPermissionController.GetTabPermissions(this.ModuleConfiguration.TabID, this.ModuleConfiguration.PortalID).ToString("EDIT");
            string moduleEditRoles = this.ModuleConfiguration.ModulePermissions.ToString("EDIT");
			tracelLogger.LogDebug("ProcessContentPane(): pageEditRoles → {PageEditRoles}", pageEditRoles);
			tracelLogger.LogDebug("ProcessContentPane(): moduleEditRoles → {ModuleEditRoles}", moduleEditRoles);

            viewRoles = viewRoles.Replace(";", string.Empty).Trim().ToLowerInvariant();
            pageEditRoles = pageEditRoles.Replace(";", string.Empty).Trim().ToLowerInvariant();
            moduleEditRoles = moduleEditRoles.Replace(";", string.Empty).Trim().ToLowerInvariant();

            var showMessage = false;
            var adminMessage = Null.NullString;
			tracelLogger.LogDebug("ProcessContentPane(): showMessage → {ShowMessage}", showMessage);
			tracelLogger.LogDebug("ProcessContentPane(): adminMessage → {AdminMessage}", adminMessage);
            if (viewRoles.Equals(this.PortalSettings.AdministratorRoleName, StringComparison.OrdinalIgnoreCase)
                            && (moduleEditRoles.Equals(this.PortalSettings.AdministratorRoleName, StringComparison.OrdinalIgnoreCase)
                                    || string.IsNullOrEmpty(moduleEditRoles))
                            && pageEditRoles.Equals(this.PortalSettings.AdministratorRoleName, StringComparison.OrdinalIgnoreCase))
            {
                adminMessage = Localization.GetString("ModuleVisibleAdministrator.Text");
                showMessage = !this.ModuleConfiguration.HideAdminBorder && !Globals.IsAdminControl();
            }

            if (this.ModuleConfiguration.StartDate >= DateTime.Now)
            {
                adminMessage = string.Format(CultureInfo.CurrentCulture, Localization.GetString("ModuleEffective.Text"), this.ModuleConfiguration.StartDate);
                showMessage = !Globals.IsAdminControl();
            }

            if (this.ModuleConfiguration.EndDate <= DateTime.Now)
            {
                adminMessage = string.Format(CultureInfo.CurrentCulture, Localization.GetString("ModuleExpired.Text"), this.ModuleConfiguration.EndDate);
                showMessage = !Globals.IsAdminControl();
            }

            if (showMessage)
            {
                this.AddAdministratorOnlyHighlighting(adminMessage);
            }
			tracelLogger.LogDebug("<<< Exiting ProcessContentPane()");
        }

        /// <summary>ProcessFooter adds an optional footer (and an End_Module comment)..</summary>
        private void ProcessFooter()
        {
			tracelLogger.LogDebug(">>> Entering ProcessFooter()");
            // inject the footer
            if (!string.IsNullOrEmpty(this.ModuleConfiguration.Footer))
            {
                var footer = new Literal { Text = this.ModuleConfiguration.Footer };
                this.ContentPane.Controls.Add(footer);
            }

            // inject an end comment around the module content
            if (!Globals.IsAdminControl())
            {
                this.ContentPane.Controls.Add(new LiteralControl("<!-- End_Module_" + this.ModuleConfiguration.ModuleID + " -->"));
            }
			tracelLogger.LogDebug("<<< Exiting ProcessFooter()");
        }

        /// <summary>ProcessHeader adds an optional header (and a Start_Module_ comment)..</summary>
        private void ProcessHeader()
        {
			tracelLogger.LogDebug(">>> Entering ProcessHeader()");
            if (!Globals.IsAdminControl())
            {
                // inject a start comment around the module content
                this.ContentPane.Controls.Add(new LiteralControl("<!-- Start_Module_" + this.ModuleConfiguration.ModuleID + " -->"));
            }

            // inject the header
            if (!string.IsNullOrEmpty(this.ModuleConfiguration.Header))
            {
                var header = new Literal { Text = this.ModuleConfiguration.Header };
                this.ContentPane.Controls.Add(header);
            }
			tracelLogger.LogDebug("<<< Exiting ProcessHeader()");
        }

        /// <summary>ProcessModule processes the module which is attached to this container.</summary>
        private void ProcessModule()
        {
			tracelLogger.LogDebug(">>> Entering ProcessModule()");
            if (this.tracelLogger.IsDebugEnabled)
            {
                this.tracelLogger.Debug($"Container.ProcessModule Start (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleID: {this.ModuleConfiguration.ModuleDefinition.DesktopModuleID}): Module FriendlyName: '{this.ModuleConfiguration.ModuleDefinition.FriendlyName}')");
            }

            if (this.ContentPane != null)
            {
                // Process Content Pane Attributes
                this.ProcessContentPane();

                // always add the actions menu as the first item in the content pane.
                if (this.InjectActionMenu && !ModuleHost.IsViewMode(this.ModuleConfiguration, this.PortalSettings) && this.Request.QueryString["dnnprintmode"] != "true")
                {
                    JavaScript.RequestRegistration(CommonJs.DnnPlugins);
                    this.ContentPane.Controls.Add(this.LoadControl(this.PortalSettings.DefaultModuleActionMenu));

                    // register admin.css
                    this.clientResourceController.RegisterStylesheet(Globals.HostPath + "admin.css", FileOrder.Css.AdminCss, true);
                }

                // Process Module Header
                this.ProcessHeader();

                // Try to load the module control
                this.moduleHost = new ModuleHost(this.ModuleConfiguration, this.ParentSkin, this);
				tracelLogger.LogDebug("ProcessModule(): moduleHost → {ModuleHost}", moduleHost);
                if (this.tracelLogger.IsDebugEnabled)
                {
                    this.tracelLogger.Debug($"Container.ProcessModule Info (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleID: {this.ModuleConfiguration.ModuleDefinition.DesktopModuleID}): ControlPane.Controls.Add(ModuleHost:{this.moduleHost.ID})");
                }

                this.ContentPane.Controls.Add(this.ModuleHost);

                // Process Module Footer
                this.ProcessFooter();

                // Process the Action Controls
                if (this.ModuleHost != null && this.ModuleControl != null)
                {
                    this.ProcessChildControls(this);
                }

                // Add Module Stylesheets
                this.ProcessStylesheets(this.ModuleHost != null);
            }

            if (this.tracelLogger.IsDebugEnabled)
            {
                this.tracelLogger.Debug($"Container.ProcessModule End (TabId:{this.PortalSettings.ActiveTab.TabID},ModuleID: {this.ModuleConfiguration.ModuleDefinition.DesktopModuleID}): Module FriendlyName: '{this.ModuleConfiguration.ModuleDefinition.FriendlyName}')");
            }
			tracelLogger.LogDebug("<<< Exiting ProcessModule()");
        }

        /// <summary>
        /// ProcessStylesheets processes the Module and Container stylesheets and adds
        /// them to the Page.
        /// </summary>
        private void ProcessStylesheets(bool includeModuleCss)
        {
			tracelLogger.LogDebug(">>> Entering ProcessStylesheets(includeModuleCss={IncludeModuleCss})", includeModuleCss);
            this.clientResourceController.RegisterStylesheet(this.ContainerPath + "container.css", FileOrder.Css.ContainerCss, true);
            this.clientResourceController.RegisterStylesheet(this.ContainerSrc.Replace(".ascx", ".css"), FileOrder.Css.SpecificContainerCss, true);

            // process the base class module properties
            if (includeModuleCss)
            {
                string controlSrc = this.ModuleConfiguration.ModuleControl.ControlSrc;
                string folderName = this.ModuleConfiguration.DesktopModule.FolderName;

                string stylesheet = string.Empty;
                if (!string.IsNullOrEmpty(folderName))
				tracelLogger.LogDebug("ProcessStylesheets(includeModuleCss={IncludeModuleCss}): stylesheet → {Stylesheet}", includeModuleCss, stylesheet);
                {
                    if (controlSrc.EndsWith(".mvc", StringComparison.OrdinalIgnoreCase))
                    {
                        stylesheet = $"{Globals.ApplicationPath}/DesktopModules/MVC/{folderName.Replace(@"\", "/")}/module.css";
                    }
                    else
                    {
                        stylesheet = $"{Globals.ApplicationPath}/DesktopModules/{folderName.Replace(@"\", "/")}/module.css";
                    }

                    this.clientResourceController.RegisterStylesheet(stylesheet, FileOrder.Css.ModuleCss, true);
                }

                var ix = controlSrc.LastIndexOf("/", StringComparison.Ordinal);
                if (ix >= 0)
				tracelLogger.LogDebug("ProcessStylesheets(includeModuleCss={IncludeModuleCss}): ix → {Ix}", includeModuleCss, ix);
                {
                    stylesheet = $"{Globals.ApplicationPath}/{controlSrc.Substring(0, ix + 1)}module.css";
                    this.clientResourceController.RegisterStylesheet(stylesheet, FileOrder.Css.ModuleCss, true);
                }
            }
			tracelLogger.LogDebug("<<< Exiting ProcessStylesheets(includeModuleCss={IncludeModuleCss})", includeModuleCss);
        }

        private void SetAlignment()
        {
			tracelLogger.LogDebug(">>> Entering SetAlignment()");
            if (!string.IsNullOrEmpty(this.ModuleConfiguration.Alignment))
            {
                if (this.ContentPane.Attributes["class"] != null)
                {
                    this.ContentPane.Attributes["class"] = this.ContentPane.Attributes["class"] + " DNNAlign" + this.ModuleConfiguration.Alignment.ToLowerInvariant();
                }
                else
                {
                    this.ContentPane.Attributes["class"] = "DNNAlign" + this.ModuleConfiguration.Alignment.ToLowerInvariant();
                }
            }
			tracelLogger.LogDebug("<<< Exiting SetAlignment()");
        }

        private void SetBackground()
        {
			tracelLogger.LogDebug(">>> Entering SetBackground()");
            if (!string.IsNullOrEmpty(this.ModuleConfiguration.Color))
            {
                this.ContentPane.Style["background-color"] = this.ModuleConfiguration.Color;
            }
			tracelLogger.LogDebug("<<< Exiting SetBackground()");
        }

        private void SetBorder()
        {
			tracelLogger.LogDebug(">>> Entering SetBorder()");
            if (!string.IsNullOrEmpty(this.ModuleConfiguration.Border))
            {
                this.ContentPane.Style["border-top"] = string.Format(CultureInfo.InvariantCulture, "{0}px #000000 solid", this.ModuleConfiguration.Border);
                this.ContentPane.Style["border-bottom"] = string.Format(CultureInfo.InvariantCulture, "{0}px #000000 solid", this.ModuleConfiguration.Border);
                this.ContentPane.Style["border-right"] = string.Format(CultureInfo.InvariantCulture, "{0}px #000000 solid", this.ModuleConfiguration.Border);
                this.ContentPane.Style["border-left"] = string.Format(CultureInfo.InvariantCulture, "{0}px #000000 solid", this.ModuleConfiguration.Border);
            }
			tracelLogger.LogDebug("<<< Exiting SetBorder()");
        }

        private void InvokeContainerEvents(ContainerEventType containerEventType)
        {
			tracelLogger.LogDebug(">>> Entering InvokeContainerEvents(containerEventType={ContainerEventType})", containerEventType);
            SharedList<ContainerEventListener> list = ((NaiveLockingList<ContainerEventListener>)DotNetNukeContext.Current.ContainerEventListeners).SharedList;

			tracelLogger.LogDebug("InvokeContainerEvents(containerEventType={ContainerEventType}): list → {List}", containerEventType, list);
            using (list.GetReadLock())
            {
                foreach (var listener in list.Where(x => x.EventType == containerEventType))
                {
                    listener.ContainerEvent.Invoke(this, new ContainerEventArgs(this));
                }
            }
			tracelLogger.LogDebug("<<< Exiting InvokeContainerEvents(containerEventType={ContainerEventType})", containerEventType);
        }

        /// <summary>ModuleAction_Click runs when a ModuleAction is clicked.</summary>
        /// <remarks>The Module Action must be configured to fire an event (it may be configured
        /// to redirect to a new url).  The event handler finds the Parent Container and invokes each
        /// registered ModuleActionEventListener delegate.
        ///
        /// Note: with the refactoring of this to the Container, this could be handled at the container level.
        /// However, for legacy purposes this is left this way, as many modules would have registered their
        /// listeners on the Container directly, rather than through the helper method in PortalModuleBase.</remarks>
        private void ModuleActionClick(object sender, ActionEventArgs e)
        {
			tracelLogger.LogDebug(">>> Entering ModuleActionClick(sender={Sender},e={E})", sender, e);
            // Search through the listeners
            foreach (ModuleActionEventListener listener in this.ParentSkin.ActionEventListeners)
            {
                // If the associated module has registered a listener
                if (e.ModuleConfiguration.ModuleID == listener.ModuleID)
                {
                    // Invoke the listener to handle the ModuleAction_Click event
                    listener.ActionEvent.Invoke(sender, e);
                }
            }
			tracelLogger.LogDebug("<<< Exiting ModuleActionClick(sender={Sender},e={E})", sender, e);
        }
    }
}
