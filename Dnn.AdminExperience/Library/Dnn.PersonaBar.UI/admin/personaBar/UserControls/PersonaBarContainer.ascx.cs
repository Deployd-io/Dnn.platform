// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable once CheckNamespace
namespace Dnn.PersonaBar.UI.UserControls
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Net;

    using Dnn.PersonaBar.Library.Containers;
    using Dnn.PersonaBar.Library.Controllers;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.UI.ControlPanels;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>The control containing the Persona Bar.</summary>
    public partial class PersonaBarContainer : ControlPanelBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IPersonaBarContainer personaBarContainer;
        private readonly IPersonaBarController personaBarController;
        private readonly IHostSettings hostSettings;
        private readonly IJavaScriptLibraryHelper javaScript;
        private readonly IClientResourceController clientResourceController;
        private readonly IServicesFramework servicesFramework;

        /// <summary>Initializes a new instance of the <see cref="PersonaBarContainer"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public PersonaBarContainer()
            : this(null, null, null, null, null, null, null, null)
        {
		Logger.LogDebug(">>> Entering PersonaBarContainer()");
        }
		Logger.LogDebug("<<< Exiting PersonaBarContainer()");

        /// <summary>Initializes a new instance of the <see cref="PersonaBarContainer"/> class.</summary>
        /// <param name="personaBarContainer">The Persona Bar container.</param>
        /// <param name="personaBarController">The Persona Bar controller.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="clientResourceController">The client resources controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public PersonaBarContainer(IPersonaBarContainer personaBarContainer, IPersonaBarController personaBarController, IHostSettings hostSettings, IJavaScriptLibraryHelper javaScript, IClientResourceController clientResourceController)
            : this(null, null, personaBarContainer, personaBarController, hostSettings, javaScript, clientResourceController, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PersonaBarContainer"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="personaBarContainer">The Persona Bar container.</param>
        /// <param name="personaBarController">The Persona Bar controller.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="clientResourceController">The client resources controller.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        public PersonaBarContainer(IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService, IPersonaBarContainer personaBarContainer, IPersonaBarController personaBarController, IHostSettings hostSettings, IJavaScriptLibraryHelper javaScript, IClientResourceController clientResourceController, IServicesFramework servicesFramework)
            : base(eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>())
        {
            this.personaBarContainer = personaBarContainer ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPersonaBarContainer>();
            this.personaBarController = personaBarController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPersonaBarController>();
			Logger.LogDebug("PersonaBarContainer(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},personaBarContainer={PersonaBarContainer},personaBarController={PersonaBarController},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController},servicesFramework={ServicesFramework}): personaBarController → {PersonaBarController1}", eventLogger, permissionDefinitionService, personaBarContainer, personaBarController, hostSettings, javaScript, clientResourceController, servicesFramework, personaBarController);
			Logger.LogDebug("PersonaBarContainer(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},personaBarContainer={PersonaBarContainer},personaBarController={PersonaBarController},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController},servicesFramework={ServicesFramework}): javaScript → {JavaScript1}", eventLogger, permissionDefinitionService, personaBarContainer, personaBarController, hostSettings, javaScript, clientResourceController, servicesFramework, javaScript);
			Logger.LogDebug("PersonaBarContainer(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},personaBarContainer={PersonaBarContainer},personaBarController={PersonaBarController},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController},servicesFramework={ServicesFramework}): clientResourceController → {ClientResourceController1}", eventLogger, permissionDefinitionService, personaBarContainer, personaBarController, hostSettings, javaScript, clientResourceController, servicesFramework, clientResourceController);
			Logger.LogDebug("PersonaBarContainer(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},personaBarContainer={PersonaBarContainer},personaBarController={PersonaBarController},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController},servicesFramework={ServicesFramework}): servicesFramework → {ServicesFramework1}", eventLogger, permissionDefinitionService, personaBarContainer, personaBarController, hostSettings, javaScript, clientResourceController, servicesFramework, servicesFramework);
			Logger.LogDebug("PersonaBarContainer(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},personaBarContainer={PersonaBarContainer},personaBarController={PersonaBarController},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController},servicesFramework={ServicesFramework}): personaBarContainer → {PersonaBarContainer1}", eventLogger, permissionDefinitionService, personaBarContainer, personaBarController, hostSettings, javaScript, clientResourceController, servicesFramework, personaBarContainer);
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("PersonaBarContainer(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},personaBarContainer={PersonaBarContainer},personaBarController={PersonaBarController},hostSettings={HostSettings},javaScript={JavaScript},clientResourceController={ClientResourceController},servicesFramework={ServicesFramework}): hostSettings → {HostSettings1}", eventLogger, permissionDefinitionService, personaBarContainer, personaBarController, hostSettings, javaScript, clientResourceController, servicesFramework, hostSettings);
            this.javaScript = javaScript ?? Globals.GetCurrentServiceProvider().GetRequiredService<IJavaScriptLibraryHelper>();
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
            this.servicesFramework = servicesFramework ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServicesFramework>();
        }

        /// <summary>Gets a JSON representation of <see cref="IPersonaBarContainer.GetConfiguration"/>.</summary>
        public string PersonaBarSettings => JsonConvert.SerializeObject(this.personaBarContainer.GetConfiguration());

        /// <summary>Gets the site's virtual application root path.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string AppPath => Globals.ApplicationPath;

        /// <summary>Gets the client resource version number.</summary>
        public string BuildNumber => this.hostSettings.CrmVersion.ToString(CultureInfo.InvariantCulture);

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            if (this.personaBarContainer.Visible)
            {
                this.personaBarContainer.Initialize(this);
            }
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            this.PersonaBarPanel.Visible = this.InjectPersonaBar();

            if (!this.PersonaBarPanel.Visible)
            {
                this.RemovedAdminStyleSheet();
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            this.Visible = this.Response.StatusCode == (int)HttpStatusCode.OK;

            base.OnPreRender(e);
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        private bool InjectPersonaBar()
        {
			Logger.LogDebug(">>> Entering InjectPersonaBar()");
            if (!this.personaBarContainer.Visible)
            {
                return false;
            }

            // copied this logic from DotNetNuke.UI.Skins.Skin.InjectControlPanel
            if (this.Request.QueryString["dnnprintmode"] == "true" || this.Request.QueryString["popUp"] == "true")
            {
                return false;
            }

            var menuStructure = this.personaBarController.GetMenu(this.PortalSettings, UserController.Instance.GetCurrentUserInfo());
            if (menuStructure.MenuItems == null || !menuStructure.MenuItems.Any())
			Logger.LogDebug("InjectPersonaBar(): menuStructure → {MenuStructure}", menuStructure);
            {
                return false;
            }

            this.RegisterPersonaBarStyleSheet();

            JavaScript.RegisterClientReference(this.Page, ClientAPI.ClientNamespaceReferences.dnn);
            this.javaScript.RequestRegistration(CommonJs.DnnPlugins); // We need to add the Dnn JQuery plugins because the Edit Bar removes the Control Panel from the page
            this.javaScript.RequestRegistration(CommonJs.KnockoutMapping);

            this.servicesFramework.RequestAjaxAntiForgerySupport();

            this.clientResourceController.RegisterScript("~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js");
            this.clientResourceController.RegisterStylesheet("~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css");

			Logger.LogDebug("<<< Exiting InjectPersonaBar()");
            return true;
        }

        private void RegisterPersonaBarStyleSheet()
        {
			Logger.LogDebug(">>> Entering RegisterPersonaBarStyleSheet()");
            this.clientResourceController.RegisterStylesheet("~/DesktopModules/admin/Dnn.PersonaBar/css/personaBarContainer.css");
			Logger.LogDebug("<<< Exiting RegisterPersonaBarStyleSheet()");
        }

        private void RemovedAdminStyleSheet()
        {
			Logger.LogDebug(">>> Entering RemovedAdminStyleSheet()");
            var loader = this.Page.FindControl("ClientResourceIncludes");
            if (loader != null)
			Logger.LogDebug("RemovedAdminStyleSheet(): loader → {Loader}", loader);
            {
                for (var i = 0; i < loader.Controls.Count; i++)
                {
                    if (loader.Controls[i] is DnnCssInclude cssInclude)
                    {
                        if (cssInclude.FilePath == (Globals.HostPath + "admin.css"))
                        {
                            loader.Controls.Remove(cssInclude);
                            break;
                        }
                    }
                }
            }
			Logger.LogDebug("<<< Exiting RemovedAdminStyleSheet()");
        }
    }
}
