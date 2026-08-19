// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider
{
    using System;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The options page.</summary>
    public partial class Options : PageBase
    {
        private readonly IEventLogger eventLogger;
        private readonly IClientResourceController clientResourceController;
        private readonly IHostSettingsService hostSettingsService;
        private readonly IPortalAliasService portalAliasService;
        private readonly IModuleController moduleController;

        /// <summary>The request.</summary>
        private readonly HttpRequest request = HttpContext.Current.Request;

        /// <summary>The portal settings.</summary>
        private IPortalSettings curPortalSettings;

        /// <summary>Initializes a new instance of the <see cref="Options"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IUserController. Scheduled removal in v12.0.0.")]
        public Options()
            : this(null, null, null, null, null, null, null, null, null)
        {
		eventLogger.LogDebug(">>> Entering Options()");
        }

		eventLogger.LogDebug("<<< Exiting Options()");
        /// <summary>Initializes a new instance of the <see cref="Options"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IUserController. Scheduled removal in v12.0.0.")]
        public Options(IPortalController portalController, IApplicationStatusInfo appStatus, IHostSettings hostSettings)
            : this(portalController, appStatus, hostSettings, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Options"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        /// <param name="moduleController">The module controller.</param>
        public Options(IPortalController portalController, IApplicationStatusInfo appStatus, IHostSettings hostSettings, IUserController userController, IEventLogger eventLogger, IClientResourceController clientResourceController, IHostSettingsService hostSettingsService, IPortalAliasService portalAliasService, IModuleController moduleController)
            : base(portalController, appStatus, hostSettings, userController)
        {
            this.eventLogger = eventLogger ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IEventLogger>();
            this.clientResourceController = clientResourceController ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IClientResourceController>();
			eventLogger.LogDebug("Options(portalController={PortalController},appStatus={AppStatus},hostSettings={HostSettings},userController={UserController},eventLogger={EventLogger},clientResourceController={ClientResourceController},hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},moduleController={ModuleController}): clientResourceController → {ClientResourceController1}", portalController, appStatus, hostSettings, userController, eventLogger, clientResourceController, hostSettingsService, portalAliasService, moduleController, clientResourceController);
			eventLogger.LogDebug("Options(portalController={PortalController},appStatus={AppStatus},hostSettings={HostSettings},userController={UserController},eventLogger={EventLogger},clientResourceController={ClientResourceController},hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},moduleController={ModuleController}): portalAliasService → {PortalAliasService1}", portalController, appStatus, hostSettings, userController, eventLogger, clientResourceController, hostSettingsService, portalAliasService, moduleController, portalAliasService);
			eventLogger.LogDebug("Options(portalController={PortalController},appStatus={AppStatus},hostSettings={HostSettings},userController={UserController},eventLogger={EventLogger},clientResourceController={ClientResourceController},hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},moduleController={ModuleController}): moduleController → {ModuleController1}", portalController, appStatus, hostSettings, userController, eventLogger, clientResourceController, hostSettingsService, portalAliasService, moduleController, moduleController);
            this.hostSettingsService = hostSettingsService ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IHostSettingsService>();
			eventLogger.LogDebug("Options(portalController={PortalController},appStatus={AppStatus},hostSettings={HostSettings},userController={UserController},eventLogger={EventLogger},clientResourceController={ClientResourceController},hostSettingsService={HostSettingsService},portalAliasService={PortalAliasService},moduleController={ModuleController}): hostSettingsService → {HostSettingsService1}", portalController, appStatus, hostSettings, userController, eventLogger, clientResourceController, hostSettingsService, portalAliasService, moduleController, hostSettingsService);
            this.portalAliasService = portalAliasService ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IPortalAliasService>();
            this.moduleController = moduleController ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IModuleController>();
        }

        /// <summary>  Gets Current Language from Url.</summary>
        protected string LangCode => this.request.QueryString["langCode"];

        /// <summary>  Gets the Name for the Current Resource file name.</summary>
        protected string ResXFile
        {
            get
            {
                string[] page = this.Request.ServerVariables["SCRIPT_NAME"].Split('/');

                return $"{this.TemplateSourceDirectory}/{Localization.LocalResourceDirectory}/{page[page.GetUpperBound(0)]}.resx";
            }
        }

        /// <summary>Register the JavaScripts and CSS.</summary>
        /// <param name="e">The Event Args.</param>
        protected override void OnPreRender(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            JavaScript.RequestRegistration(this.AppStatus, this.eventLogger, this.PortalSettings, CommonJs.jQuery);
            JavaScript.RequestRegistration(this.AppStatus, this.eventLogger, this.PortalSettings, CommonJs.jQueryUI);
            this.clientResourceController.CreateScript(this.ResolveUrl("js/jquery.notification.js")).Register();
            this.clientResourceController.CreateScript(this.ResolveUrl("js/Options.js")).Register();
            this.clientResourceController.CreateStylesheet("https://ajax.googleapis.com/ajax/libs/jqueryui/1/themes/blitzer/jquery-ui.css").Register();
            this.clientResourceController.CreateStylesheet(this.ResolveUrl("css/jquery.notification.css")).Register();
            this.clientResourceController.CreateStylesheet(this.ResolveUrl("css/Options.css")).Register();

            base.OnPreRender(e);
			eventLogger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <summary>Raises the <see cref="E:Init" /> event.</summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override void OnInit(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnInit(e={E})", e);
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            this.InitializeComponent();
            base.OnInit(e);

            // Favicon
            this.LoadFavIcon();
			eventLogger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <summary>Handles the Load event of the Page control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering Page_Load(sender={Sender},e={E})", sender, e);
            ModuleInfo modInfo = null;

			eventLogger.LogDebug("Page_Load(sender={Sender},e={E}): modInfo → {ModInfo}", sender, e, modInfo);
            try
            {
                // Get ModuleID from Url
                int moduleId;
                if (!int.TryParse(this.request.QueryString["mid"], NumberStyles.Integer, CultureInfo.InvariantCulture, out moduleId))
                {
                    moduleId = -1;
					eventLogger.LogDebug("Page_Load(sender={Sender},e={E}): moduleId → {ModuleId}", sender, e, moduleId);
                }

                // Get TabId from Url
                int tabId;
                if (!int.TryParse(this.request.QueryString["tid"], NumberStyles.Integer, CultureInfo.InvariantCulture, out tabId))
                {
                    tabId = -1;
					eventLogger.LogDebug("Page_Load(sender={Sender},e={E}): tabId → {TabId}", sender, e, tabId);
                }

                if (moduleId != -1 && tabId != -1)
                {
                    modInfo = this.moduleController.GetModule(moduleId, tabId, false);
                }
                else
                {
                    this.ClosePage();
                }
            }
            catch (Exception exception)
            {
				eventLogger.LogError("Exception in Page_Load(sender={Sender},e={E}): {Arg2}", sender, e, exception.Message);
                Exceptions.ProcessPageLoadException(exception);

                this.ClosePage();
            }

            try
            {
                // Get ModuleID from Url
                var oEditorOptions = (CKEditorOptions)this.Page.LoadControl("CKEditorOptions.ascx");
				eventLogger.LogDebug("Page_Load(sender={Sender},e={E}): oEditorOptions → {OEditorOptions}", sender, e, oEditorOptions);

                oEditorOptions.ID = "CKEditor_Options";
				eventLogger.LogDebug("Page_Load(sender={Sender},e={E}): oEditorOptions.ID → {Arg2}", sender, e, oEditorOptions.ID);
				eventLogger.LogDebug("Page_Load(sender={Sender},e={E}): oEditorOptions.ModuleConfiguration → {Arg2}", sender, e, oEditorOptions.ModuleConfiguration);
                oEditorOptions.ModuleConfiguration = modInfo;

                this.phControls.Controls.Add(oEditorOptions);
            }
            catch (Exception exception)
            {
                Exceptions.ProcessPageLoadException(exception);

                this.ClosePage();
            }
			eventLogger.LogDebug("<<< Exiting Page_Load(sender={Sender},e={E})", sender, e);
        }

        /// <summary>Closes the page.</summary>
        private void ClosePage()
        {
			eventLogger.LogDebug(">>> Entering ClosePage()");
            this.Page.ClientScript.RegisterStartupScript(
                this.GetType(), "closeScript", "javascript:self.close();", true);
				eventLogger.LogDebug("<<< Exiting ClosePage()");
        }

        /// <summary>Gets the portal settings.</summary>
        /// <returns>The Portal Settings.</returns>
        private IPortalSettings GetPortalSettings()
        {
			eventLogger.LogDebug(">>> Entering GetPortalSettings()");
            int iTabId = 0, iPortalId = 0;

            IPortalSettings portalSettings;
			eventLogger.LogDebug("GetPortalSettings(): iTabId → {ITabId}", iTabId);

            try
            {
                if (this.request.QueryString["tabid"] != null)
                {
                    iTabId = int.Parse(this.request.QueryString["tabid"]);
                }

                if (this.request.QueryString["PortalID"] != null)
                {
                    iPortalId = int.Parse(this.request.QueryString["PortalID"]);
                }
				eventLogger.LogDebug("GetPortalSettings(): iPortalId → {IPortalId}", iPortalId);

                string sDomainName = Globals.GetDomainName(this.Request, true);

                string sPortalAlias = this.portalAliasService.GetPortalAliasByPortal(iPortalId, sDomainName);

                var objPortalAliasInfo = this.portalAliasService.GetPortalAlias(sPortalAlias);
				eventLogger.LogDebug("GetPortalSettings(): objPortalAliasInfo → {ObjPortalAliasInfo}", objPortalAliasInfo);
                portalSettings = PortalSettings.Create(iTabId, objPortalAliasInfo);
            }
            catch (Exception)
            {
				eventLogger.LogError("Exception in GetPortalSettings(): {Arg0}", Exception.Message);
                portalSettings = (IPortalSettings)HttpContext.Current.Items["PortalSettings"];
            }

			eventLogger.LogDebug("<<< Exiting GetPortalSettings()");
            return portalSettings;
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        ///   the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			eventLogger.LogDebug(">>> Entering InitializeComponent()");
            this.curPortalSettings = this.GetPortalSettings();
			eventLogger.LogDebug("InitializeComponent(): curPortalSettings → {CurPortalSettings}", curPortalSettings);
			eventLogger.LogDebug("<<< Exiting InitializeComponent()");
        }

        /// <summary>Load Favicon from Current Portal Home Directory.</summary>
        private void LoadFavIcon()
        {
			eventLogger.LogDebug(">>> Entering LoadFavIcon()");
            this.favicon.Controls.Add(new LiteralControl(DotNetNuke.UI.Internals.FavIcon.GetHeaderLink(this.HostSettings, this.curPortalSettings.PortalId)));
			eventLogger.LogDebug("<<< Exiting LoadFavIcon()");
        }
    }
}
