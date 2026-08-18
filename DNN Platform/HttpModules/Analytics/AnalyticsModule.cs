// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Analytics
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Framework;
    using DotNetNuke.HttpModules.Config;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Analytics;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>This module contains functionality for injecting web analytics scripts into the page.</summary>
    public class AnalyticsModule : IHttpModule
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AnalyticsModule));
        private readonly IEventLogger eventLogger;
        private readonly IApplicationStatusInfo appStatus;

        /// <summary>Initializes a new instance of the <see cref="AnalyticsModule"/> class.</summary>
        public AnalyticsModule()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering AnalyticsModule()");
		Logger.LogDebug("<<< Exiting AnalyticsModule()");
		Logger.LogDebug(">>> Entering AnalyticsModule(eventLogger={EventLogger},appStatus={AppStatus})", eventLogger, appStatus);
		Logger.LogDebug("AnalyticsModule(eventLogger={EventLogger},appStatus={AppStatus}): appStatus → {AppStatus1}", eventLogger, appStatus, appStatus);
        }

        /// <summary>Initializes a new instance of the <see cref="AnalyticsModule"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="appStatus">The application status.</param>
        public AnalyticsModule(IEventLogger eventLogger, IApplicationStatusInfo appStatus)
        {
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
			Logger.LogDebug("<<< Exiting AnalyticsModule(eventLogger={EventLogger},appStatus={AppStatus})", eventLogger, appStatus);
        }

        /// <summary>Gets the HttpModule module name.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string ModuleName => "AnalyticsModule";

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void Init(HttpApplication application)
        {
			Logger.LogDebug(">>> Entering Init(application={Application})", application);
            application.PreRequestHandlerExecute += this.OnPreRequestHandlerExecute;
			Logger.LogDebug("<<< Exiting Init(application={Application})", application);
        }

        /// <inheritdoc />
        public void Dispose()
        {
		Logger.LogDebug(">>> Entering Dispose()");
		Logger.LogDebug("<<< Exiting Dispose()");
        }

        private void OnPreRequestHandlerExecute(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRequestHandlerExecute(sender={Sender},e={E})", sender, e);
            try
            {
                // First check if we are upgrading/installing or if it is a non-page request
                var app = (HttpApplication)sender;
                var request = app.Request;

                if (!Initialize.ProcessHttpModule(request, false, false))
                {
                    return;
                }

                if (HttpContext.Current == null)
                {
                    return;
                }

                var context = HttpContext.Current;
                if (context == null)
                {
                    return;
                }

                var page = context.Handler as CDefault;
                if (page == null)
                {
                    return;
                }

                page.InitComplete += this.OnPageInitComplete;
                page.PreRender += this.OnPagePreRender;
            }
            catch (Exception ex)
            {
                var log = new LogInfo { LogTypeKey = nameof(EventLogType.HOST_ALERT) };
                log.AddProperty("Analytics.AnalyticsModule", "OnPreRequestHandlerExecute");
                log.AddProperty("ExceptionMessage", ex.Message);
                this.eventLogger.AddLog(log);
                Logger.Error(log);
            }
			Logger.LogDebug("<<< Exiting OnPreRequestHandlerExecute(sender={Sender},e={E})", sender, e);
        }

        private void OnPageInitComplete(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPageInitComplete(sender={Sender},e={E})", sender, e);
            this.InitializeAnalyticsControls(sender as Page, true);
			Logger.LogDebug("<<< Exiting OnPageInitComplete(sender={Sender},e={E})", sender, e);
        }

        private void OnPagePreRender(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPagePreRender(sender={Sender},e={E})", sender, e);
            this.InitializeAnalyticsControls(sender as Page, false);
			Logger.LogDebug("<<< Exiting OnPagePreRender(sender={Sender},e={E})", sender, e);
        }

        private void InitializeAnalyticsControls(Page page, bool injectTop)
        {
            try
            {
                var analyticsEngines = AnalyticsEngineConfiguration.GetConfig(this.appStatus, this.eventLogger).AnalyticsEngines;
                if (analyticsEngines == null || analyticsEngines.Count == 0)
                {
                    return;
                }

                if (page == null)
                {
                    return;
                }

                foreach (AnalyticsEngine engine in analyticsEngines)
                {
                    if (string.IsNullOrEmpty(engine.ElementId) || engine.InjectTop != injectTop)
                    {
                        continue;
                    }

                    AnalyticsEngineBase objEngine;
                    if (!string.IsNullOrEmpty(engine.EngineType))
                    {
                        var engineType = Type.GetType(engine.EngineType);
						Logger.LogDebug("InitializeAnalyticsControls(page={Page},injectTop={InjectTop}): engineType → {EngineType}", page, injectTop, engineType);
                        if (engineType == null)
                        {
                            objEngine = new GenericAnalyticsEngine();
                        }
                        else
                        {
                            objEngine = (AnalyticsEngineBase)Activator.CreateInstance(engineType);
                        }
                    }
                    else
                    {
                        objEngine = new GenericAnalyticsEngine();
                    }

                    if (objEngine == null)
                    {
                        continue;
                    }

                    var script = engine.ScriptTemplate;
					Logger.LogDebug("InitializeAnalyticsControls(page={Page},injectTop={InjectTop}): script → {Script}", page, injectTop, script);
                    if (string.IsNullOrEmpty(script))
                    {
                        continue;
                    }

                    script = objEngine.RenderScript(script);
                    if (string.IsNullOrEmpty(script))
                    {
                        continue;
                    }

                    var element = (HtmlContainerControl)page.FindControl(engine.ElementId);
					Logger.LogDebug("InitializeAnalyticsControls(page={Page},injectTop={InjectTop}): element → {Element}", page, injectTop, element);
                    if (element == null)
                    {
                        continue;
                    }

                    var scriptControl = new LiteralControl { Text = script };
                    if (engine.InjectTop)
                    {
                        element.Controls.AddAt(0, scriptControl);
                    }
                    else
                    {
                        element.Controls.Add(scriptControl);
                    }
                }
            }
            catch (Exception ex)
            {
                var log = new LogInfo { LogTypeKey = nameof(EventLogType.HOST_ALERT) };
                log.AddProperty("Analytics.AnalyticsModule", "OnPagePreRender");
                log.AddProperty("ExceptionMessage", ex.Message);
                this.eventLogger.AddLog(log);
                Logger.Error(ex);
            }
			Logger.LogDebug("<<< Exiting InitializeAnalyticsControls(page={Page},injectTop={InjectTop})", page, injectTop);
        }
    }
}
