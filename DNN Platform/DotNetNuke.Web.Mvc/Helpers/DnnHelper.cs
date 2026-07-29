// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable ConvertPropertyToExpressionBody
namespace DotNetNuke.Web.Mvc.Helpers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.Client.ResourceManager;
    using DotNetNuke.Web.Mvc.Framework.Controllers;

    using Microsoft.Extensions.DependencyInjection;

    using FileOrder = DotNetNuke.Web.Client.FileOrder;

    public class DnnHelper
    {
        private readonly IClientResourceController clientResourceController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="DnnHelper"/> class.</summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="viewDataContainer">The ViewData container.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public DnnHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
            : this(null, null, null, viewContext, viewDataContainer, RouteTable.Routes)
        {
		eventLogger.LogDebug(">>> Entering DnnHelper(viewContext={ViewContext},viewDataContainer={ViewDataContainer})", viewContext, viewDataContainer);
        }
		eventLogger.LogDebug("<<< Exiting DnnHelper(viewContext={ViewContext},viewDataContainer={ViewDataContainer})", viewContext, viewDataContainer);

        /// <summary>Initializes a new instance of the <see cref="DnnHelper"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="viewContext">The view context.</param>
        /// <param name="viewDataContainer">The ViewData container.</param>
        public DnnHelper(IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, ViewContext viewContext, IViewDataContainer viewDataContainer)
            : this(clientResourceController, appStatus, eventLogger, viewContext, viewDataContainer, RouteTable.Routes)
        {
		eventLogger.LogDebug(">>> Entering DnnHelper(clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},viewContext={ViewContext},viewDataContainer={ViewDataContainer})", clientResourceController, appStatus, eventLogger, viewContext, viewDataContainer);
		eventLogger.LogDebug("<<< Exiting DnnHelper(clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},viewContext={ViewContext},viewDataContainer={ViewDataContainer})", clientResourceController, appStatus, eventLogger, viewContext, viewDataContainer);
		eventLogger.LogDebug(">>> Entering DnnHelper(viewContext={ViewContext},viewDataContainer={ViewDataContainer},routeCollection={RouteCollection})", viewContext, viewDataContainer, routeCollection);
		eventLogger.LogDebug("<<< Exiting DnnHelper(viewContext={ViewContext},viewDataContainer={ViewDataContainer},routeCollection={RouteCollection})", viewContext, viewDataContainer, routeCollection);
        }

        /// <summary>Initializes a new instance of the <see cref="DnnHelper"/> class.</summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="viewDataContainer">The ViewData container.</param>
        /// <param name="routeCollection">The route collection.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public DnnHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
            : this(null, null, null, new HtmlHelper(viewContext, viewDataContainer, routeCollection))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnHelper"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="viewContext">The view context.</param>
        /// <param name="viewDataContainer">The ViewData container.</param>
        /// <param name="routeCollection">The route collection.</param>
        public DnnHelper(IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
            : this(clientResourceController, appStatus, eventLogger, new HtmlHelper(viewContext, viewDataContainer, routeCollection))
        {
		eventLogger.LogDebug("DnnHelper(clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},htmlHelper={HtmlHelper}): appStatus → {AppStatus1}", clientResourceController, appStatus, eventLogger, htmlHelper, appStatus);
		eventLogger.LogDebug("DnnHelper(clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},htmlHelper={HtmlHelper}): eventLogger → {EventLogger1}", clientResourceController, appStatus, eventLogger, htmlHelper, eventLogger);
		eventLogger.LogDebug("DnnHelper(clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},htmlHelper={HtmlHelper}): clientResourceController → {ClientResourceController1}", clientResourceController, appStatus, eventLogger, htmlHelper, clientResourceController);
        }

        /// <summary>Initializes a new instance of the <see cref="DnnHelper"/> class.</summary>
        /// <param name="htmlHelper">The HtmlHelper instance to wrap.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        protected DnnHelper(HtmlHelper htmlHelper)
            : this(null, null, null, htmlHelper)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnHelper"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <exception cref="InvalidOperationException">The DnnHelper class can only be used in Views that inherit from DnnWebViewPage.</exception>
        protected DnnHelper(IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, HtmlHelper htmlHelper)
        {
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
            this.HtmlHelper = htmlHelper;
			eventLogger.LogDebug("DnnHelper(clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},htmlHelper={HtmlHelper}): this.HtmlHelper → {Arg4}", clientResourceController, appStatus, eventLogger, htmlHelper, this.HtmlHelper);

            if (htmlHelper.ViewContext.Controller is not IDnnController controller)
            {
                throw new InvalidOperationException("The DnnHelper class can only be used in Views that inherit from DnnWebViewPage");
            }

            this.DnnPage = controller.DnnPage;
			eventLogger.LogDebug("DnnHelper(clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},htmlHelper={HtmlHelper}): this.DnnPage → {Arg4}", clientResourceController, appStatus, eventLogger, htmlHelper, this.DnnPage);

            this.ModuleContext = controller.ModuleContext;
			eventLogger.LogDebug("DnnHelper(clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},htmlHelper={HtmlHelper}): this.ModuleContext → {Arg4}", clientResourceController, appStatus, eventLogger, htmlHelper, this.ModuleContext);
            this.LocalResourceFile = controller.LocalResourceFile;
			eventLogger.LogDebug("DnnHelper(clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},htmlHelper={HtmlHelper}): this.LocalResourceFile → {Arg4}", clientResourceController, appStatus, eventLogger, htmlHelper, this.LocalResourceFile);
        }

        public ModuleInfo ActiveModule => this.ModuleContext?.Configuration;

        public TabInfo ActivePage => this.PortalSettings?.ActiveTab;

        public PortalSettings PortalSettings => this.ModuleContext?.PortalSettings;

        public RouteCollection RouteCollection => this.HtmlHelper.RouteCollection;

        public UserInfo User => this.PortalSettings?.UserInfo;

        public dynamic ViewBag => this.HtmlHelper.ViewBag;

        public ViewContext ViewContext => this.HtmlHelper.ViewContext;

        public ViewDataDictionary ViewData => this.HtmlHelper.ViewData;

        public IViewDataContainer ViewDataContainer => this.HtmlHelper.ViewDataContainer;

        public Page DnnPage { get; set; }

        public string LocalResourceFile { get; set; }

        public ModuleInstanceContext ModuleContext { get; set; }

        internal HtmlHelper HtmlHelper { get; set; }

        public string LocalizeString(string key)
        {
			eventLogger.LogDebug(">>> Entering LocalizeString(key={Key})", key);
			eventLogger.LogDebug("<<< Exiting LocalizeString(key={Key})", key);
            return Localization.GetString(key, this.LocalResourceFile);
        }

        /// <summary>Requests that a CSS file be registered on the client browser.</summary>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        public void DnnCssInclude(string filePath)
        {
			eventLogger.LogDebug(">>> Entering DnnCssInclude(filePath={FilePath})", filePath);
            this.clientResourceController.RegisterStylesheet(filePath);
			eventLogger.LogDebug("<<< Exiting DnnCssInclude(filePath={FilePath})", filePath);
        }

        /// <summary>Requests that a CSS file be registered on the client browser. Defaults to rendering in the page header.</summary>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        public void DnnCssInclude(string filePath, int priority)
        {
            this.clientResourceController.RegisterStylesheet(filePath, (Abstractions.ClientResources.FileOrder.Css)priority);
			eventLogger.LogDebug(">>> Entering DnnCssInclude(filePath={FilePath},priority={Priority})", filePath, priority);
			eventLogger.LogDebug("<<< Exiting DnnCssInclude(filePath={FilePath},priority={Priority})", filePath, priority);
        }

        /// <summary>Requests that a CSS file be registered on the client browser. Defaults to rendering in the page header.</summary>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        public void DnnCssInclude(string filePath, FileOrder.Css priority)
        {
            this.clientResourceController.RegisterStylesheet(filePath, (Abstractions.ClientResources.FileOrder.Css)priority);
        }

        /// <summary>Requests that a CSS file be registered on the client browser. Allows for overriding the default provider.</summary>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The provider name to be used to render the CSS file on the page.</param>
        public void DnnCssInclude(string filePath, int priority, string provider)
        {
			eventLogger.LogDebug(">>> Entering DnnCssInclude(filePath={FilePath},priority={Priority},provider={Provider})", filePath, priority, provider);
            this.clientResourceController.CreateStylesheet(filePath)
                .SetPriority(priority)
                .SetProvider(provider)
                .Register();
				eventLogger.LogDebug("<<< Exiting DnnCssInclude(filePath={FilePath},priority={Priority},provider={Provider})", filePath, priority, provider);
        }

        /// <summary>Requests that a CSS file be registered on the client browser. Allows for overriding the default provider.</summary>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The provider name to be used to render the CSS file on the page.</param>
        /// <param name="name">Name of framework like Bootstrap, Angular, etc.</param>
        /// <param name="version">Version nr of framework.</param>
        public void DnnCssInclude(string filePath, int priority, string provider, string name, string version)
        {
			eventLogger.LogDebug(">>> Entering DnnCssInclude(filePath={FilePath},priority={Priority},provider={Provider},name={Name},version={Version})", filePath, priority, provider, name, version);
            this.clientResourceController.CreateStylesheet(filePath)
                .SetPriority(priority)
                .SetProvider(provider)
                .SetNameAndVersion(name, version, false)
                .Register();
        }

        /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        public void DnnJsInclude(string filePath)
        {
			eventLogger.LogDebug(">>> Entering DnnJsInclude(filePath={FilePath})", filePath);
            this.clientResourceController.RegisterScript(filePath);
			eventLogger.LogDebug("<<< Exiting DnnJsInclude(filePath={FilePath})", filePath);
        }

        /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        public void DnnJsInclude(string filePath, int priority)
        {
            this.clientResourceController.RegisterScript(filePath, (Abstractions.ClientResources.FileOrder.Js)priority);
			eventLogger.LogDebug(">>> Entering DnnJsInclude(filePath={FilePath},priority={Priority})", filePath, priority);
			eventLogger.LogDebug("<<< Exiting DnnJsInclude(filePath={FilePath},priority={Priority})", filePath, priority);
        }

        /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        public void DnnJsInclude(string filePath, FileOrder.Js priority)
        {
            this.clientResourceController.RegisterScript(filePath, (Abstractions.ClientResources.FileOrder.Js)priority);
        }

        /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
        public void DnnJsInclude(string filePath, int priority, string provider)
        {
			eventLogger.LogDebug(">>> Entering DnnJsInclude(filePath={FilePath},priority={Priority},provider={Provider})", filePath, priority, provider);
            this.clientResourceController.CreateScript(filePath)
                .SetPriority(priority)
                .SetProvider(provider)
                .Register();
				eventLogger.LogDebug("<<< Exiting DnnJsInclude(filePath={FilePath},priority={Priority},provider={Provider})", filePath, priority, provider);
        }

        /// <summary>Requests that a JavaScript file be registered on the client browser.</summary>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
        /// <param name="name">Name of framework like Bootstrap, Angular, etc.</param>
        /// <param name="version">Version nr of framework.</param>
        public void DnnJsInclude(string filePath, int priority, string provider, string name, string version)
        {
			eventLogger.LogDebug(">>> Entering DnnJsInclude(filePath={FilePath},priority={Priority},provider={Provider},name={Name},version={Version})", filePath, priority, provider, name, version);
            this.clientResourceController.CreateStylesheet(filePath)
                .SetPriority(priority)
                .SetProvider(provider)
                .SetNameAndVersion(name, version, false)
                .Register();
        }

        /// <summary>Requests that a script from a JavaScript Library be registered on the client browser.</summary>
        /// <param name="name">The name of the JavaScript Library.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void JavaScriptLibraryInclude(string name)
        {
			eventLogger.LogDebug(">>> Entering JavaScriptLibraryInclude(name={Name})", name);
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, name);
			eventLogger.LogDebug("<<< Exiting JavaScriptLibraryInclude(name={Name})", name);
        }

        /// <summary>Requests that a script from a JavaScript Library be registered on the client browser.</summary>
        /// <param name="name">The name of the JavaScript Library.</param>
        /// <param name="version">The library's version.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void JavaScriptLibraryInclude(string name, Version version)
        {
			eventLogger.LogDebug(">>> Entering JavaScriptLibraryInclude(name={Name},version={Version})", name, version);
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, name, version);
			eventLogger.LogDebug("<<< Exiting JavaScriptLibraryInclude(name={Name},version={Version})", name, version);
        }

        /// <summary>Requests that a script from a JavaScript Library be registered on the client browser.</summary>
        /// <param name="name">The name of the JavaScript Library.</param>
        /// <param name="version">The library's version.</param>
        /// <param name="specificVersion">
        /// How much of the <paramref name="version"/> to pay attention to.
        /// When <see cref="SpecificVersion.Latest"/> is passed, ignore the <paramref name="version"/>.
        /// When <see cref="SpecificVersion.LatestMajor"/> is passed, match the major version.
        /// When <see cref="SpecificVersion.LatestMinor"/> is passed, match the major and minor versions.
        /// When <see cref="SpecificVersion.Exact"/> is passed, match all parts of the version.
        /// </param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void JavaScriptLibraryInclude(string name, Version version, SpecificVersion specificVersion)
        {
			eventLogger.LogDebug(">>> Entering JavaScriptLibraryInclude(name={Name},version={Version},specificVersion={SpecificVersion})", name, version, specificVersion);
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, name, version, specificVersion);
			eventLogger.LogDebug("<<< Exiting JavaScriptLibraryInclude(name={Name},version={Version},specificVersion={SpecificVersion})", name, version, specificVersion);
        }
    }
}
