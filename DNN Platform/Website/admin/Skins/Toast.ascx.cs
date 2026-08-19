// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A skin/theme object which allows the display of toast messages.</summary>
    public partial class Toast : SkinObjectBase
    {
        private const string MyFileName = "Toast.ascx";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Toast));
        private static readonly string ToastCacheKey = "DNN_Toast_Config";
        private readonly INavigationManager navigationManager;
        private readonly IJavaScriptLibraryHelper javaScript;
        private readonly IClientResourceController clientResourceController;
        private readonly IServicesFramework servicesFramework;

        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
        public Toast()
            : this(null, null, null)
        {
		Logger.LogDebug(">>> Entering Toast()");
        }

		Logger.LogDebug("<<< Exiting Toast(navigationManager={NavigationManager},javaScript={JavaScript},clientResourceController={ClientResourceController})", navigationManager, javaScript, clientResourceController);
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
		Logger.LogDebug("<<< Exiting Toast()");
        public Toast(INavigationManager navigationManager, IJavaScriptLibraryHelper javaScript, IClientResourceController clientResourceController)
            : this(navigationManager, javaScript, clientResourceController, null)
        {
		Logger.LogDebug(">>> Entering Toast(navigationManager={NavigationManager},javaScript={JavaScript},clientResourceController={ClientResourceController})", navigationManager, javaScript, clientResourceController);
		Logger.LogDebug("Toast(navigationManager={NavigationManager},javaScript={JavaScript},clientResourceController={ClientResourceController},servicesFramework={ServicesFramework}): javaScript → {JavaScript1}", navigationManager, javaScript, clientResourceController, servicesFramework, javaScript);
		Logger.LogDebug("Toast(navigationManager={NavigationManager},javaScript={JavaScript},clientResourceController={ClientResourceController},servicesFramework={ServicesFramework}): clientResourceController → {ClientResourceController1}", navigationManager, javaScript, clientResourceController, servicesFramework, clientResourceController);
		Logger.LogDebug("Toast(navigationManager={NavigationManager},javaScript={JavaScript},clientResourceController={ClientResourceController},servicesFramework={ServicesFramework}): servicesFramework → {ServicesFramework1}", navigationManager, javaScript, clientResourceController, servicesFramework, servicesFramework);
        }

        public Toast(INavigationManager navigationManager, IJavaScriptLibraryHelper javaScript, IClientResourceController clientResourceController, IServicesFramework servicesFramework)
        {
            this.navigationManager = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
            this.javaScript = javaScript ?? Globals.GetCurrentServiceProvider().GetRequiredService<IJavaScriptLibraryHelper>();
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
            this.servicesFramework = servicesFramework ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServicesFramework>();
        }

        protected string ServiceModuleName { get; private set; }

        protected string ServiceAction { get; private set; }

        public bool IsOnline()
        {
			Logger.LogDebug(">>> Entering IsOnline()");
            var userInfo = UserController.Instance.GetCurrentUserInfo();
			Logger.LogDebug("IsOnline(): userInfo → {UserInfo}", userInfo);
			Logger.LogDebug("<<< Exiting IsOnline()");
            return userInfo.UserID != -1;
        }

        public string GetNotificationLink()
        {
			Logger.LogDebug(">>> Entering GetNotificationLink()");
			Logger.LogDebug("<<< Exiting GetNotificationLink()");
            return this.GetMessageLink() + "?view=notifications&action=notifications";
        }

        public string GetMessageLink()
        {
			Logger.LogDebug(">>> Entering GetMessageLink()");
			Logger.LogDebug("<<< Exiting GetMessageLink()");
            return this.navigationManager.NavigateURL(this.GetMessageTab(), string.Empty, string.Format("userId={0}", this.PortalSettings.UserId));
        }

        public string GetMessageLabel()
        {
			Logger.LogDebug(">>> Entering GetMessageLabel()");
			Logger.LogDebug("<<< Exiting GetMessageLabel()");
            return Localization.GetString("SeeAllMessage", Localization.GetResourceFile(this, MyFileName));
        }

        public string GetNotificationLabel()
        {
			Logger.LogDebug(">>> Entering GetNotificationLabel()");
			Logger.LogDebug("<<< Exiting GetNotificationLabel()");
            return Localization.GetString("SeeAllNotification", Localization.GetResourceFile(this, MyFileName));
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.javaScript.RequestRegistration(CommonJs.jQueryUI);
            this.servicesFramework.RequestAjaxAntiForgerySupport();

            this.clientResourceController.RegisterScript("~/Resources/Shared/components/Toast/jquery.toastmessage.js", FileOrder.Js.jQuery);
            this.clientResourceController.RegisterStylesheet("~/Resources/Shared/components/Toast/jquery.toastmessage.css", FileOrder.Css.DefaultCss);

            this.InitializeConfig();
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        // This method is copied from user skin object
        private int GetMessageTab()
        {
			Logger.LogDebug(">>> Entering GetMessageTab()");
            var cacheKey = string.Format("MessageCenterTab:{0}:{1}", this.PortalSettings.PortalId, this.PortalSettings.CultureCode);
            var messageTabId = DataCache.GetCache<int>(cacheKey);
			Logger.LogDebug("GetMessageTab(): messageTabId → {MessageTabId}", messageTabId);
			Logger.LogDebug("GetMessageTab(): cacheKey → {CacheKey}", cacheKey);
            if (messageTabId > 0)
            {
                return messageTabId;
            }

            // Find the Message Tab
            messageTabId = this.FindMessageTab();

            // save in cache
            // NOTE - This cache is not being cleared. There is no easy way to clear this, except Tools->Clear Cache
            DataCache.SetCache(cacheKey, messageTabId, TimeSpan.FromMinutes(20));

			Logger.LogDebug("<<< Exiting GetMessageTab()");
            return messageTabId;
        }

        // This method is copied from user skin object
        private int FindMessageTab()
        {
			Logger.LogDebug(">>> Entering FindMessageTab()");
            // On brand new install the new Message Center Module is on the child page of User Profile Page
            // On Upgrade to 6.2.0, the Message Center module is on the User Profile Page
            var profileTab = TabController.Instance.GetTab(this.PortalSettings.UserTabId, this.PortalSettings.PortalId, false);
			Logger.LogDebug("FindMessageTab(): profileTab → {ProfileTab}", profileTab);
            if (profileTab != null)
            {
                var childTabs = TabController.Instance.GetTabsByPortal(profileTab.PortalID).DescendentsOf(profileTab.TabID);
				Logger.LogDebug("FindMessageTab(): childTabs → {ChildTabs}", childTabs);
                foreach (TabInfo tab in childTabs)
                {
                    foreach (KeyValuePair<int, ModuleInfo> kvp in ModuleController.Instance.GetTabModules(tab.TabID))
                    {
                        var module = kvp.Value;
                        if (module.DesktopModule.FriendlyName == "Message Center")
                        {
                            return tab.TabID;
                        }
                    }
                }
            }

            // default to User Profile Page
			Logger.LogDebug("<<< Exiting FindMessageTab()");
            return this.PortalSettings.UserTabId;
        }

        private void InitializeConfig()
        {
			Logger.LogDebug(">>> Entering InitializeConfig()");
            this.ServiceModuleName = "InternalServices";
            this.ServiceAction = "NotificationsService/GetToasts";
			Logger.LogDebug("InitializeConfig(): this.ServiceModuleName → {Arg0}", this.ServiceModuleName);
			Logger.LogDebug("InitializeConfig(): this.ServiceAction → {Arg0}", this.ServiceAction);

            try
            {
                var toastConfig = DataCache.GetCache<IDictionary<string, string>>(ToastCacheKey);
				Logger.LogDebug("InitializeConfig(): toastConfig → {ToastConfig}", toastConfig);
                if (toastConfig == null)
                {
                    var configFile = this.Server.MapPath(Path.Combine(this.TemplateSourceDirectory, "Toast.config"));
					Logger.LogDebug("InitializeConfig(): configFile → {ConfigFile}", configFile);

                    if (File.Exists(configFile))
                    {
                        var xmlDocument = new XmlDocument { XmlResolver = null };
                        using (var configReader = XmlReader.Create(configFile, new XmlReaderSettings { XmlResolver = null, }))
                        {
                            xmlDocument.Load(configReader);
                        }

                        var moduleNameNode = xmlDocument.DocumentElement?.SelectSingleNode("moduleName");
						Logger.LogDebug("InitializeConfig(): moduleNameNode → {ModuleNameNode}", moduleNameNode);
                        var actionNode = xmlDocument.DocumentElement?.SelectSingleNode("action");
						Logger.LogDebug("InitializeConfig(): actionNode → {ActionNode}", actionNode);
                        var scriptsNode = xmlDocument.DocumentElement?.SelectSingleNode("scripts");
						Logger.LogDebug("InitializeConfig(): scriptsNode → {ScriptsNode}", scriptsNode);

                        if (moduleNameNode != null && !string.IsNullOrEmpty(moduleNameNode.InnerText))
                        {
                            this.ServiceModuleName = moduleNameNode.InnerText;
                        }

                        if (actionNode != null && !string.IsNullOrEmpty(actionNode.InnerText))
                        {
                            this.ServiceAction = actionNode.InnerText;
                        }

                        if (scriptsNode != null && !string.IsNullOrEmpty(scriptsNode.InnerText))
                        {
                            this.addtionalScripts.Text = scriptsNode.InnerText;
                            this.addtionalScripts.Visible = true;
                        }
                    }

                    var config = new Dictionary<string, string>()
                    {
                        { "ServiceModuleName", this.ServiceModuleName },
                        { "ServiceAction", this.ServiceAction },
                        { "AddtionalScripts", this.addtionalScripts.Text },
                    };
                    DataCache.SetCache(ToastCacheKey, config);
                }
                else
                {
                    if (!string.IsNullOrEmpty(toastConfig["ServiceModuleName"]))
                    {
                        this.ServiceModuleName = toastConfig["ServiceModuleName"];
                    }

                    if (!string.IsNullOrEmpty(toastConfig["ServiceAction"]))
                    {
                        this.ServiceAction = toastConfig["ServiceAction"];
                    }

                    if (!string.IsNullOrEmpty(toastConfig["AddtionalScripts"]))
                    {
                        this.addtionalScripts.Text = toastConfig["AddtionalScripts"];
                        this.addtionalScripts.Visible = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
			Logger.LogDebug("<<< Exiting InitializeConfig()");
        }
    }
}
