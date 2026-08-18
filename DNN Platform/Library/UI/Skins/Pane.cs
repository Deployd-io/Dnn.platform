// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.Containers;
    using DotNetNuke.UI.Utilities;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>The Pane class represents a Pane within the Skin.</summary>
    public class Pane : IDisposable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Pane));
        private const string CPaneOutline = "paneOutline";
        private HtmlGenericControl containerWrapperControl;
        private Dictionary<string, Containers.Container> containers;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pane"/> class.
        /// Constructs a new Pane object from the Control in the Skin.
        /// </summary>
        /// <param name="pane">The HtmlContainerControl in the Skin.</param>
        public Pane(HtmlContainerControl pane)
        {
			Logger.LogDebug(">>> Entering Pane(pane={Pane})", pane);
            this.PaneControl = pane;

			Logger.LogDebug("Pane(pane={Pane}): this.PaneControl → {Arg1}", pane, this.PaneControl);
            // Disable ViewState (we enable it later in the process)
            this.PaneControl.ViewStateMode = ViewStateMode.Disabled;
            this.Name = pane.ID;
			Logger.LogDebug("Pane(pane={Pane}): this.Name → {Arg1}", pane, this.Name);
			Logger.LogDebug("<<< Exiting Pane(pane={Pane})", pane);
        }

        /// <summary>
		Logger.LogDebug("<<< Exiting Pane(pane={Pane})", pane);
        /// Initializes a new instance of the <see cref="Pane"/> class.
        /// Constructs a new Pane object from the Control in the Skin.
        /// </summary>
        /// <param name="name">The name (ID) of the HtmlContainerControl.</param>
        /// <param name="pane">The HtmlContainerControl in the Skin.</param>
        public Pane(string name, HtmlContainerControl pane)
        {
			Logger.LogDebug(">>> Entering Pane(name={Name},pane={Pane})", name, pane);
            this.PaneControl = pane;
            this.Name = name;
			Logger.LogDebug("Pane(name={Name},pane={Pane}): this.PaneControl → {Arg2}", name, pane, this.PaneControl);
        }

        /// <summary>Gets a Dictionary of Containers.</summary>
        protected Dictionary<string, Containers.Container> Containers => this.containers ??= new Dictionary<string, Containers.Container>();

        /// <summary>Gets the PortalSettings of the Portal.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected PortalSettings PortalSettings => PortalSettings.Current;

        /// <summary>Gets or sets the name (ID) of the Pane.</summary>
        protected string Name { get; set; }

        /// <summary>Gets or sets the HtmlContainerControl.</summary>
        protected HtmlContainerControl PaneControl { get; set; }

        /// <summary>InjectModule injects a Module (and its container) into the Pane.</summary>
        /// <param name="module">The Module.</param>
        public void InjectModule(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering InjectModule(module={Module})", module);
            this.containerWrapperControl = new HtmlGenericControl("div");
            this.PaneControl.Controls.Add(this.containerWrapperControl);
			Logger.LogDebug("InjectModule(module={Module}): containerWrapperControl → {ContainerWrapperControl}", module, containerWrapperControl);

            // inject module classes
            string classFormatString = "DnnModule DnnModule-{0} DnnModule-{1}";
            string sanitizedModuleName = Null.NullString;
			Logger.LogDebug("InjectModule(module={Module}): sanitizedModuleName → {SanitizedModuleName}", module, sanitizedModuleName);

            if (!string.IsNullOrEmpty(module.DesktopModule.ModuleName))
            {
                sanitizedModuleName = Globals.CreateValidClass(module.DesktopModule.ModuleName, false);
            }

            if (IsVersionableModule(module))
            {
                classFormatString += " DnnVersionableControl";
            }

            this.containerWrapperControl.Attributes["class"] = string.Format(CultureInfo.InvariantCulture, classFormatString, sanitizedModuleName, module.ModuleID);

            try
            {
                if (!Globals.IsAdminControl() && (this.PortalSettings.InjectModuleHyperLink || Personalization.GetUserMode() != PortalSettings.Mode.View))
                {
                    this.containerWrapperControl.Controls.Add(new LiteralControl($"<a name=\"{module.ModuleID}\"></a>"));
                }

                // Load container control
                Containers.Container container = this.LoadModuleContainer(module);
				Logger.LogDebug("InjectModule(module={Module}): container → {Container}", module, container);

                // Add Container to Dictionary
                this.Containers.Add(container.ID, container);

                // hide anything of type ActionsMenu - as we're injecting our own menu now.
                container.InjectActionMenu = !container.Controls.OfType<ActionBase>().Any();
                if (!container.InjectActionMenu)
                {
                    foreach (var actionControl in container.Controls.OfType<IActionControl>())
                    {
                        if (actionControl is ActionsMenu and Control control)
                        {
                            control.Visible = false;
							Logger.LogDebug("InjectModule(module={Module}): control.Visible → {Arg1}", module, control.Visible);
							Logger.LogDebug("InjectModule(module={Module}): container.InjectActionMenu → {Arg1}", module, container.InjectActionMenu);
                            container.InjectActionMenu = true;
                        }
                    }
                }

                if (Globals.IsLayoutMode() && !Globals.IsAdminControl())
                {
                    // provide Drag-N-Drop capabilities
                    var dragDropContainer = new Panel();
					Logger.LogDebug("InjectModule(module={Module}): dragDropContainer → {DragDropContainer}", module, dragDropContainer);
                    Control title = container.FindControl("dnnTitle");
					Logger.LogDebug("InjectModule(module={Module}): title → {Title}", module, title);

                    // Assume that the title control is named dnnTitle.  If this becomes an issue we could loop through the controls looking for the title type of skin object
                    dragDropContainer.ID = container.ID + "_DD";
					Logger.LogDebug("InjectModule(module={Module}): dragDropContainer.ID → {Arg1}", module, dragDropContainer.ID);
                    this.containerWrapperControl.Controls.Add(dragDropContainer);

                    // inject the container into the page pane - this triggers the Pre_Init() event for the user control
                    dragDropContainer.Controls.Add(container);

                    if (title != null)
                    {
                        if (title.Controls.Count > 0)
                        {
                            title = title.Controls[0];
                        }
                    }

                    // enable drag and drop
                    if (title != null)
                    {
                        // The title ID is actually the first child so we need to make sure at least one child exists
                        DNNClientAPI.EnableContainerDragAndDrop(title, dragDropContainer, module.ModuleID);
                        ClientAPI.RegisterPostBackEventHandler(this.PaneControl, "MoveToPane", this.ModuleMoveToPanePostBack, false);
                    }
                }
                else
                {
                    this.containerWrapperControl.Controls.Add(container);
                    if (Globals.IsAdminControl())
                    {
                        this.containerWrapperControl.Attributes["class"] += " DnnModule-Admin";
                    }
                }

                // Attach Module to Container
                container.SetModuleConfiguration(module);

                // display collapsible page panes
                if (!this.PaneControl.Visible)
                {
                    this.PaneControl.Visible = true;
                }
            }
            catch (ThreadAbortException)
            {
				Logger.LogError("Exception in InjectModule(module={Module}): {Arg1}", module, ThreadAbortException.getMessage());
                // Response.Redirect may be called in module control's OnInit method, so it will cause ThreadAbortException, no need any action here.
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in InjectModule(module={Module}): {Arg1}", module, exc.getMessage());
                var lex = new ModuleLoadException(string.Format(CultureInfo.InvariantCulture, Skin.MODULEADD_ERROR, this.PaneControl.ID), exc);
				Logger.LogDebug("InjectModule(module={Module}): lex → {Lex}", module, lex);
                if (TabPermissionController.CanAdminPage())
                {
                    // only display the error to administrators
                    this.containerWrapperControl.Controls.Add(new ErrorContainer(this.PortalSettings, Skin.MODULELOAD_ERROR, lex).Container);
                }

                Exceptions.LogException(exc);
				Logger.LogWarning("Throwing lex from InjectModule(module={Module})", module);
			Logger.LogDebug("<<< Exiting InjectModule(module={Module})", module);
                throw lex;
            }
        }

        /// <summary>ProcessPane processes the Attributes for the PaneControl.</summary>
        public void ProcessPane()
        {
			Logger.LogDebug(">>> Entering ProcessPane()");
            if (this.PaneControl != null)
            {
                // remove excess skin non-validating attributes
                this.PaneControl.Attributes.Remove("ContainerType");
                this.PaneControl.Attributes.Remove("ContainerName");
                this.PaneControl.Attributes.Remove("ContainerSrc");

                if (Globals.IsLayoutMode())
                {
                    this.PaneControl.Visible = true;

                    // display pane border
                    string cssclass = this.PaneControl.Attributes["class"];
                    if (string.IsNullOrEmpty(cssclass))
                    {
                        this.PaneControl.Attributes["class"] = CPaneOutline;
                    }
                    else
                    {
                        this.PaneControl.Attributes["class"] = cssclass.Replace(CPaneOutline, string.Empty).Trim().Replace("  ", " ") + " " + CPaneOutline;
                    }

                    // display pane name
                    var ctlLabel = new Label { Text = "<center>" + this.Name + "</center><br />", CssClass = "SubHead" };
					Logger.LogDebug("ProcessPane(): ctlLabel → {CtlLabel}", ctlLabel);
                    this.PaneControl.Controls.AddAt(0, ctlLabel);
                }
                else
                {
                    if (this.PaneControl.Visible == false && TabPermissionController.CanAddContentToPage())
                    {
                        this.PaneControl.Visible = true;
                    }

                    if (this.CanCollapsePane())
                    {
                        // This pane has no controls so set the width to 0
                        if (this.PaneControl.Attributes["style"] != null)
                        {
                            this.PaneControl.Attributes.Remove("style");
                        }

                        if (this.PaneControl.Attributes["class"] != null)
                        {
                            this.PaneControl.Attributes["class"] = this.PaneControl.Attributes["class"] + " DNNEmptyPane";
                        }
                        else
                        {
                            this.PaneControl.Attributes["class"] = "DNNEmptyPane";
                        }
                    }

                    // Add support for drag and drop
                    if (Globals.IsEditMode())
                    {
                        // this call also checks for permission
                        if (this.PaneControl.Attributes["class"] != null)
                        {
                            this.PaneControl.Attributes["class"] = this.PaneControl.Attributes["class"] + " dnnSortable";
                        }
                        else
                        {
                            this.PaneControl.Attributes["class"] = "dnnSortable";
                        }
                    }
                }
            }
			Logger.LogDebug("<<< Exiting ProcessPane()");
        }

        /// <inheritdoc />
        public void Dispose()
        {
			Logger.LogDebug(">>> Entering Dispose()");
            this.Dispose(true);
            GC.SuppressFinalize(this);
			Logger.LogDebug("<<< Exiting Dispose()");
        }

        protected virtual void Dispose(bool disposing)
        {
			Logger.LogDebug(">>> Entering Dispose(disposing={Disposing})", disposing);
            if (disposing)
            {
                this.containerWrapperControl?.Dispose();
                this.PaneControl?.Dispose();
            }
			Logger.LogDebug("<<< Exiting Dispose(disposing={Disposing})", disposing);
        }

        private static bool IsVersionableModule(ModuleInfo moduleInfo)
        {
			Logger.LogDebug(">>> Entering IsVersionableModule(moduleInfo={ModuleInfo})", moduleInfo);
            if (string.IsNullOrEmpty(moduleInfo.DesktopModule.BusinessControllerClass))
            {
                return false;
            }

            var controllerType = Framework.Reflection.CreateType(moduleInfo.DesktopModule.BusinessControllerClass);
			Logger.LogDebug("IsVersionableModule(moduleInfo={ModuleInfo}): controllerType → {ControllerType}", moduleInfo, controllerType);
			Logger.LogDebug("<<< Exiting IsVersionableModule(moduleInfo={ModuleInfo})", moduleInfo);
            return typeof(IVersionable).IsAssignableFrom(controllerType);
        }

        private bool CanCollapsePane()
        {
			Logger.LogDebug(">>> Entering CanCollapsePane()");
            // This section sets the width to "0" on panes that have no modules.
            // This preserves the integrity of the HTML syntax so we don't have to set
            // the visiblity of a pane to false. Setting the visibility of a pane to
            // false where there are colspans and rowspans can render the skin incorrectly.
            bool canCollapsePane = true;
			Logger.LogDebug("CanCollapsePane(): canCollapsePane → {CanCollapsePane}", canCollapsePane);
            if (this.Containers.Count > 0)
            {
                canCollapsePane = false;
            }
            else if (this.PaneControl.Controls.Count == 1)
            {
                // Pane contains 1 control
                canCollapsePane = false;
                var literal = this.PaneControl.Controls[0] as LiteralControl;
                if (literal != null)
                {
                    // Check  if the literal control is just whitespace - if so we can collapse panes
                    if (string.IsNullOrEmpty(HtmlUtils.StripWhiteSpace(literal.Text, false)))
                    {
                        canCollapsePane = true;
                    }
                }
            }
            else if (this.PaneControl.Controls.Count > 1)
            {
                // Pane contains more than 1 control
                canCollapsePane = false;
            }

			Logger.LogDebug("<<< Exiting CanCollapsePane()");
            return canCollapsePane;
        }

        /// <summary>LoadContainerByPath gets the Container from its Url(Path).</summary>
        /// <param name="containerPath">The Url to the Container control.</param>
        /// <returns>A Container.</returns>
        private Containers.Container LoadContainerByPath(string containerPath)
        {
			Logger.LogDebug(">>> Entering LoadContainerByPath(containerPath={ContainerPath})", containerPath);
            if (containerPath.IndexOf("/skins/", StringComparison.InvariantCultureIgnoreCase) != -1
                || containerPath.IndexOf(@"/skins\", StringComparison.InvariantCultureIgnoreCase) != -1
                || containerPath.IndexOf(@"\skins\", StringComparison.InvariantCultureIgnoreCase) != -1
                || containerPath.IndexOf(@"\skins/", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
				Logger.LogWarning("Throwing InvalidContainerPathException from LoadContainerByPath(containerPath={ContainerPath})", containerPath);
                throw new InvalidContainerPathException($"Invalid container path: {containerPath}");
            }

            Containers.Container container = null;

            try
            {
				Logger.LogDebug("LoadContainerByPath(containerPath={ContainerPath}): container → {Container}", containerPath, container);
                string containerSrc = containerPath;
                if (containerPath.IndexOf(Globals.ApplicationPath, StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    containerPath = containerPath.Remove(0, Globals.ApplicationPath.Length);
					Logger.LogDebug("LoadContainerByPath(containerPath={ContainerPath}): containerPath → {ContainerPath1}", containerPath, containerPath);
                }

                container = ControlUtilities.LoadControl<Containers.Container>(this.PaneControl.Page, containerPath);
                container.ContainerSrc = containerSrc;
				Logger.LogDebug("LoadContainerByPath(containerPath={ContainerPath}): container.ContainerSrc → {Arg1}", containerPath, container.ContainerSrc);

                // call databind so that any server logic in the container is executed
                container.DataBind();
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in LoadContainerByPath(containerPath={ContainerPath}): {Arg1}", containerPath, exc.getMessage());
                // could not load user control
                var lex = new ModuleLoadException(Skin.MODULELOAD_ERROR, exc);
				Logger.LogDebug("LoadContainerByPath(containerPath={ContainerPath}): lex → {Lex}", containerPath, lex);
                if (TabPermissionController.CanAdminPage())
                {
                    // only display the error to administrators
                    this.containerWrapperControl.Controls.Add(new ErrorContainer(this.PortalSettings, string.Format(CultureInfo.InvariantCulture, Skin.CONTAINERLOAD_ERROR, containerPath), lex).Container);
                }

                Exceptions.LogException(lex);
            }

            return container;
        }

        /// <summary>LoadModuleContainer gets the Container for cookie.</summary>
        /// <param name="request">Current Http Request.</param>
        /// <returns>A Container.</returns>
        private Containers.Container LoadContainerFromCookie(HttpRequest request)
        {
            Containers.Container container = null;
            HttpCookie cookie = request.Cookies["_ContainerSrc" + this.PortalSettings.PortalId];
            if (cookie != null)
            {
                if (!string.IsNullOrEmpty(cookie.Value))
                {
                    container = this.LoadContainerByPath(SkinController.FormatSkinSrc(cookie.Value + ".ascx", this.PortalSettings));
                }
            }

            return container;
        }

        private Containers.Container LoadContainerFromPane()
        {
			Logger.LogDebug(">>> Entering LoadContainerFromPane()");
            Containers.Container container = null;
            string containerSrc;
			Logger.LogDebug("LoadContainerFromPane(): container → {Container}", container);
            var validSrc = false;
			Logger.LogDebug("LoadContainerFromPane(): validSrc → {ValidSrc}", validSrc);

            if ((this.PaneControl.Attributes["ContainerType"] != null) && (this.PaneControl.Attributes["ContainerName"] != null))
            {
                containerSrc = "[" + this.PaneControl.Attributes["ContainerType"] + "]" + SkinController.RootContainer + "/" + this.PaneControl.Attributes["ContainerName"] + "/" +
                               this.PaneControl.Attributes["ContainerSrc"];
							Logger.LogDebug("LoadContainerFromPane(): containerSrc → {ContainerSrc}", containerSrc);
                validSrc = true;
            }
            else
            {
                containerSrc = this.PaneControl.Attributes["ContainerSrc"];
                if (containerSrc.Contains("/") && !(containerSrc.StartsWith("[g]", StringComparison.InvariantCultureIgnoreCase) || containerSrc.StartsWith("[l]", StringComparison.InvariantCultureIgnoreCase)))
                {
                    containerSrc = string.Format(CultureInfo.InvariantCulture, SkinController.IsGlobalSkin(this.PortalSettings.ActiveTab.SkinSrc) ? "[G]containers/{0}" : "[L]containers/{0}", containerSrc.TrimStart('/'));
                    validSrc = true;
                }
            }

            if (validSrc)
            {
                containerSrc = SkinController.FormatSkinSrc(containerSrc, this.PortalSettings);
                container = this.LoadContainerByPath(containerSrc);
            }

			Logger.LogDebug("<<< Exiting LoadContainerFromPane()");
            return container;
        }

        private Containers.Container LoadContainerFromQueryString(ModuleInfo module, HttpRequest request)
        {
            Containers.Container container = null;
            int previewModuleId = -1;
            if (request.QueryString["ModuleId"] != null)
            {
                if (!int.TryParse(request.QueryString["ModuleId"], out previewModuleId))
                {
                    previewModuleId = -1;
                }
            }

            // load user container ( based on cookie )
            if (request.QueryString["ContainerSrc"] != null && (module.ModuleID == previewModuleId || previewModuleId == -1))
            {
                string containerSrc = SkinController.FormatSkinSrc(Globals.QueryStringDecode(request.QueryString["ContainerSrc"]) + ".ascx", this.PortalSettings);
                container = this.LoadContainerByPath(containerSrc);
            }

            return container;
        }

        private Containers.Container LoadNoContainer(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering LoadNoContainer(module={Module})", module);
            string noContainerSrc = "[G]" + SkinController.RootContainer + "/_default/No Container.ascx";
            Containers.Container container = null;
			Logger.LogDebug("LoadNoContainer(module={Module}): container → {Container}", module, container);

            // if the module specifies that no container should be used
            if (module.DisplayTitle == false)
            {
                // always display container if the current user is the administrator or the module is being used in an admin case
                bool displayTitle = ModulePermissionController.CanEditModuleContent(module) || Globals.IsAdminSkin();

                // unless the administrator is in view mode
                if (displayTitle)
                {
                    displayTitle = Personalization.GetUserMode() != PortalSettings.Mode.View;
                }

                if (displayTitle == false)
                {
                    container = this.LoadContainerByPath(SkinController.FormatSkinSrc(noContainerSrc, this.PortalSettings));
                }
            }

			Logger.LogDebug("<<< Exiting LoadNoContainer(module={Module})", module);
            return container;
        }

        private Containers.Container LoadModuleContainer(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering LoadModuleContainer(module={Module})", module);
            var containerSrc = Null.NullString;
            var request = this.PaneControl.Page.Request;
			Logger.LogDebug("LoadModuleContainer(module={Module}): containerSrc → {ContainerSrc}", module, containerSrc);
            Containers.Container container = null;
			Logger.LogDebug("LoadModuleContainer(module={Module}): container → {Container}", module, container);

            if (this.PortalSettings.EnablePopUps && UrlUtils.InPopUp())
            {
                containerSrc = module.ContainerPath + "popUpContainer.ascx";

                // Check Skin for a popup Container
                if (module.ContainerSrc == this.PortalSettings.ActiveTab.ContainerSrc)
                {
                    if (File.Exists(HttpContext.Current.Server.MapPath(containerSrc)))
                    {
                        container = this.LoadContainerByPath(containerSrc);
                    }
                }

                // error loading container - load default popup container
                if (container == null)
                {
                    containerSrc = Globals.HostPath + "Containers/_default/popUpContainer.ascx";
                    container = this.LoadContainerByPath(containerSrc);
                }
            }
            else
            {
                container = (this.LoadContainerFromQueryString(module, request) ?? this.LoadContainerFromCookie(request)) ?? this.LoadNoContainer(module);
                if (container == null)
                {
                    // Check Skin for Container
                    var masterModules = this.PortalSettings.ActiveTab.ChildModules;
                    if (masterModules.TryGetValue(module.ModuleID, out var masterModule) && string.IsNullOrEmpty(masterModule.ContainerSrc))
                    {
                        // look for a container specification in the skin pane
                        if (this.PaneControl?.Attributes["ContainerSrc"] != null)
                        {
                            container = this.LoadContainerFromPane();
                        }
                    }
                }

                // else load assigned container
                if (container == null)
                {
                    containerSrc = module.ContainerSrc;
                    if (!string.IsNullOrEmpty(containerSrc))
                    {
                        containerSrc = SkinController.FormatSkinSrc(containerSrc, this.PortalSettings);
                        container = this.LoadContainerByPath(containerSrc);
                    }
                }

                // error loading container - load from tab
                if (container == null)
                {
                    containerSrc = this.PortalSettings.ActiveTab.ContainerSrc;
                    if (!string.IsNullOrEmpty(containerSrc))
                    {
                        containerSrc = SkinController.FormatSkinSrc(containerSrc, this.PortalSettings);
                        container = this.LoadContainerByPath(containerSrc);
                    }
                }

                // error loading container - load default
                if (container == null)
                {
                    containerSrc = SkinController.FormatSkinSrc(SkinController.GetDefaultPortalContainer(), this.PortalSettings);
                    container = this.LoadContainerByPath(containerSrc);
                }
            }

            // Set container path
            module.ContainerPath = SkinController.FormatSkinPath(containerSrc);

            // set container id to an explicit short name to reduce page payload
            container.ID = "ctr";
			Logger.LogDebug("LoadModuleContainer(module={Module}): container.ID → {Arg1}", module, container.ID);

            // make the container id unique for the page
            if (module.ModuleID > -1)
            {
                container.ID += module.ModuleID.ToString(CultureInfo.InvariantCulture);
            }

			Logger.LogDebug("<<< Exiting LoadModuleContainer(module={Module})", module);
            return container;
        }

        /// <summary>ModuleMoveToPanePostBack executes when a module is moved by Drag-and-Drop.</summary>
        /// <param name="args">A ClientAPIPostBackEventArgs object.</param>
        private void ModuleMoveToPanePostBack(ClientAPIPostBackEventArgs args)
        {
			Logger.LogDebug(">>> Entering ModuleMoveToPanePostBack(args={Args})", args);
            var portalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
            if (TabPermissionController.CanAdminPage())
			Logger.LogDebug("ModuleMoveToPanePostBack(args={Args}): portalSettings → {PortalSettings}", args, portalSettings);
            {
                var moduleId = Convert.ToInt32(args.EventArguments["moduleid"], CultureInfo.InvariantCulture);
				Logger.LogDebug("ModuleMoveToPanePostBack(args={Args}): moduleId → {ModuleId}", args, moduleId);
                var paneName = Convert.ToString(args.EventArguments["pane"], CultureInfo.InvariantCulture);
				Logger.LogDebug("ModuleMoveToPanePostBack(args={Args}): paneName → {PaneName}", args, paneName);
                var moduleOrder = Convert.ToInt32(args.EventArguments["order"], CultureInfo.InvariantCulture);
				Logger.LogDebug("ModuleMoveToPanePostBack(args={Args}): moduleOrder → {ModuleOrder}", args, moduleOrder);

                ModuleController.Instance.UpdateModuleOrder(portalSettings.ActiveTab.TabID, moduleId, moduleOrder, paneName);
                ModuleController.Instance.UpdateTabModuleOrder(portalSettings.ActiveTab.TabID);

                // Redirect to the same page to pick up changes
                this.PaneControl.Page.Response.Redirect(this.PaneControl.Page.Request.RawUrl, true);
            }
			Logger.LogDebug("<<< Exiting ModuleMoveToPanePostBack(args={Args})", args);
        }
    }
}
