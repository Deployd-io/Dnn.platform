// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.UI.Modules;
    using Microsoft.VisualBasic.CompilerServices;

    /// <summary>Exceptions class provides operation to log most of the exceptions occurred in system.</summary>
    /// <remarks>
    /// <para>
    /// Logging is an import part of a good architecture. A good practice is to provide enough logging to accurately be able to identify which
    /// code caused an exception. Some exceptions are expected and you can display information to the user to help him figure out what happened.
    /// It is however bad practice to catch any and all exceptions (general exception) and display this to users. In such situations it is best
    /// to provide the user with a general message and only log the details in a way that the details are either just visible to administrators
    /// (Event Log) or only to SuperUsers (hosts) in the server logs.
    /// </para>
    /// <para>
    /// Currently we implement a lot of custom exception to use in different levels:
    /// <list type="bullet">
    /// <item><see cref="ModuleLoadException"/></item>
    /// <item><see cref="ObjectHydrationException"/></item>
    /// <item><see cref="PageLoadException"/></item>
    /// <item><see cref="SchedulerException"/></item>
    /// <item><see cref="SearchException"/></item>
    /// <item><see cref="SecurityException"/></item>
    /// </list>
    /// </para>
    /// </remarks>
    [StandardModule]
    public sealed class Exceptions
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Exceptions));

        /// <summary>Gets the exception info.</summary>
        /// <param name="e">The exception.</param>
        /// <returns>Exception info.</returns>
        public static ExceptionInfo GetExceptionInfo(Exception e)
        {
			Logger.LogError(">>> Entering GetExceptionInfo(e={E})", e);
            var objExceptionInfo = new ExceptionInfo(e);

            while (e.InnerException != null)
            {
                e = e.InnerException;
            }

            var st = new StackTrace(e, true);
            var sf = st.GetFrame(0);
            if (sf != null)
            {
                try
                {
                    // Get the corresponding method for that stack frame.
                    var mi = sf.GetMethod();

                    // Get the namespace where that method is defined.
                    var res = mi.DeclaringType.Namespace + ".";

                    // Append the type name.
                    res += mi.DeclaringType.Name + ".";

                    // Append the name of the method.
                    res += mi.Name;
                    objExceptionInfo.Method = res;
					Logger.LogDebug("GetExceptionInfo(e={E}): objExceptionInfo.Method → {Arg1}", e, objExceptionInfo.Method);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    objExceptionInfo.Method = "N/A - Reflection Permission required";
                }

                if (!string.IsNullOrEmpty(sf.GetFileName()))
                {
                    objExceptionInfo.FileName = sf.GetFileName();
                    objExceptionInfo.FileColumnNumber = sf.GetFileColumnNumber();
                    objExceptionInfo.FileLineNumber = sf.GetFileLineNumber();
                }
            }

			Logger.LogWarning("<<< Exiting GetExceptionInfo(e={E})", e);
            return objExceptionInfo;
        }

        public static void ProcessHttpException()
        {
			Logger.LogError(">>> Entering ProcessHttpException()");
            var notFoundErrorString = Localization.GetString("ResourceNotFound", Localization.SharedResourceFile);
			Logger.LogWarning("<<< Exiting ProcessHttpException()");
            var exc = new HttpException(404, notFoundErrorString);
			Logger.LogError("ProcessHttpException(): exc → {Exc}", exc);
			Logger.LogError("ProcessHttpException(): notFoundErrorString → {NotFoundErrorString}", notFoundErrorString);
        Logger.LogWarning("<<< Exiting ProcessHttpException(exc={Exc})", exc);
            ProcessHttpException(exc, HttpContext.Current.Request.RawUrl);
        }

        public static void ProcessHttpException(string url)
        {
			Logger.LogError(">>> Entering ProcessHttpException(url={Url})", url);
			Logger.LogError("ProcessHttpException(url={Url}): exc → {Exc}", url, exc);
            var notFoundErrorString = Localization.GetString("ResourceNotFound", Localization.SharedResourceFile);
			Logger.LogWarning("<<< Exiting ProcessHttpException(url={Url})", url);
            var exc = new HttpException(404, notFoundErrorString);
            ProcessHttpException(exc, url);
        }

        public static void ProcessHttpException(HttpException exc)
        {
			Logger.LogError(">>> Entering ProcessHttpException(exc={Exc})", exc);
            ProcessHttpException(exc, HttpContext.Current.Request.RawUrl);
        }

        public static void ProcessHttpException(HttpRequest request)
        {
			Logger.LogError(">>> Entering ProcessHttpException(request={Request})", request);
			Logger.LogError("ProcessHttpException(request={Request}): exc → {Exc}", request, exc);
            var notFoundErrorString = Localization.GetString("ResourceNotFound", Localization.SharedResourceFile);
			Logger.LogWarning("<<< Exiting ProcessHttpException(request={Request})", request);
            var exc = new HttpException(404, notFoundErrorString);
            ProcessHttpException(exc, request.RawUrl);
        }

        /// <summary>Processes the module load exception.</summary>
        /// <param name="objPortalModuleBase">The control.</param>
        /// <param name="exc">The exception.</param>
        public static void ProcessModuleLoadException(PortalModuleBase objPortalModuleBase, Exception exc)
        {
			Logger.LogError(">>> Entering ProcessModuleLoadException(objPortalModuleBase={ObjPortalModuleBase},exc={Exc})", objPortalModuleBase, exc);
            ProcessModuleLoadException((Control)objPortalModuleBase, exc);
			Logger.LogWarning("<<< Exiting ProcessModuleLoadException(objPortalModuleBase={ObjPortalModuleBase},exc={Exc})", objPortalModuleBase, exc);
        }

        /// <summary>Processes the module load exception.</summary>
        /// <param name="objPortalModuleBase">The control.</param>
        /// <param name="exc">The exception.</param>
        /// <param name="displayErrorMessage">if set to <see langword="true"/> display error message.</param>
        public static void ProcessModuleLoadException(PortalModuleBase objPortalModuleBase, Exception exc, bool displayErrorMessage)
        {
			Logger.LogError(">>> Entering ProcessModuleLoadException(objPortalModuleBase={ObjPortalModuleBase},exc={Exc},displayErrorMessage={DisplayErrorMessage})", objPortalModuleBase, exc, displayErrorMessage);
            ProcessModuleLoadException((Control)objPortalModuleBase, exc, displayErrorMessage);
			Logger.LogWarning("<<< Exiting ProcessModuleLoadException(objPortalModuleBase={ObjPortalModuleBase},exc={Exc},displayErrorMessage={DisplayErrorMessage})", objPortalModuleBase, exc, displayErrorMessage);
        }

        /// <summary>Processes the module load exception.</summary>
        /// <param name="friendlyMessage">The friendly message.</param>
        /// <param name="objPortalModuleBase">The control.</param>
        /// <param name="exc">The exception.</param>
        /// <param name="displayErrorMessage">if set to <see langword="true"/> display error message.</param>
        public static void ProcessModuleLoadException(string friendlyMessage, PortalModuleBase objPortalModuleBase, Exception exc, bool displayErrorMessage)
        {
			Logger.LogError(">>> Entering ProcessModuleLoadException(friendlyMessage={FriendlyMessage},objPortalModuleBase={ObjPortalModuleBase},exc={Exc},displayErrorMessage={DisplayErrorMessage})", friendlyMessage, objPortalModuleBase, exc, displayErrorMessage);
            ProcessModuleLoadException(friendlyMessage, (Control)objPortalModuleBase, exc, displayErrorMessage);
			Logger.LogWarning("<<< Exiting ProcessModuleLoadException(friendlyMessage={FriendlyMessage},objPortalModuleBase={ObjPortalModuleBase},exc={Exc},displayErrorMessage={DisplayErrorMessage})", friendlyMessage, objPortalModuleBase, exc, displayErrorMessage);
        }

        /// <summary>Processes the module load exception.</summary>
        /// <param name="ctrl">The control.</param>
        /// <param name="exc">The exception.</param>
        public static void ProcessModuleLoadException(Control ctrl, Exception exc)
        {
			Logger.LogError(">>> Entering ProcessModuleLoadException(ctrl={Ctrl},exc={Exc})", ctrl, exc);
            // Exit Early if ThreadAbort Exception
            if (ThreadAbortCheck(exc))
            {
                return;
            }

            ProcessModuleLoadException(ctrl, exc, true);
			Logger.LogWarning("<<< Exiting ProcessModuleLoadException(ctrl={Ctrl},exc={Exc})", ctrl, exc);
        }

        /// <summary>Processes the module load exception.</summary>
        /// <param name="ctrl">The control.</param>
        /// <param name="exc">The exception.</param>
        /// <param name="displayErrorMessage">if set to <see langword="true"/> displays an error message.</param>
        public static void ProcessModuleLoadException(Control ctrl, Exception exc, bool displayErrorMessage)
        {
			Logger.LogError(">>> Entering ProcessModuleLoadException(ctrl={Ctrl},exc={Exc},displayErrorMessage={DisplayErrorMessage})", ctrl, exc, displayErrorMessage);
            // Exit Early if ThreadAbort Exception
            if (ThreadAbortCheck(exc))
            {
                return;
            }

            string friendlyMessage = Localization.GetString("ErrorOccurred");
			Logger.LogError("ProcessModuleLoadException(ctrl={Ctrl},exc={Exc},displayErrorMessage={DisplayErrorMessage}): friendlyMessage → {FriendlyMessage}", ctrl, exc, displayErrorMessage, friendlyMessage);
            var ctrlModule = ctrl as IModuleControl;
            if (ctrlModule == null)
            {
                // Regular Control
                friendlyMessage = Localization.GetString("ErrorOccurred");
            }
            else
            {
                // IModuleControl
                string moduleTitle = Null.NullString;
				Logger.LogError("ProcessModuleLoadException(ctrl={Ctrl},exc={Exc},displayErrorMessage={DisplayErrorMessage}): moduleTitle → {ModuleTitle}", ctrl, exc, displayErrorMessage, moduleTitle);
                if (ctrlModule != null && ctrlModule.ModuleContext.Configuration != null)
                {
                    moduleTitle = ctrlModule.ModuleContext.Configuration.ModuleTitle;
                }

                friendlyMessage = string.Format(CultureInfo.CurrentCulture, Localization.GetString("ModuleUnavailable"), moduleTitle);
            }

            ProcessModuleLoadException(friendlyMessage, ctrl, exc, displayErrorMessage);
			Logger.LogWarning("<<< Exiting ProcessModuleLoadException(ctrl={Ctrl},exc={Exc},displayErrorMessage={DisplayErrorMessage})", ctrl, exc, displayErrorMessage);
        }

        /// <summary>Processes the module load exception.</summary>
        /// <param name="friendlyMessage">The friendly message.</param>
        /// <param name="ctrl">The control.</param>
        /// <param name="exc">The exception.</param>
        public static void ProcessModuleLoadException(string friendlyMessage, Control ctrl, Exception exc)
        {
			Logger.LogError(">>> Entering ProcessModuleLoadException(friendlyMessage={FriendlyMessage},ctrl={Ctrl},exc={Exc})", friendlyMessage, ctrl, exc);
            // Exit Early if ThreadAbort Exception
            if (ThreadAbortCheck(exc))
            {
                return;
            }

            ProcessModuleLoadException(friendlyMessage, ctrl, exc, true);
			Logger.LogWarning("<<< Exiting ProcessModuleLoadException(friendlyMessage={FriendlyMessage},ctrl={Ctrl},exc={Exc})", friendlyMessage, ctrl, exc);
        }

        /// <summary>Processes the module load exception.</summary>
        /// <param name="friendlyMessage">The friendly message.</param>
        /// <param name="ctrl">The control.</param>
        /// <param name="exc">The exception.</param>
        /// <param name="displayErrorMessage">if set to <see langword="true"/> display error message.</param>
        public static void ProcessModuleLoadException(string friendlyMessage, Control ctrl, Exception exc, bool displayErrorMessage)
        {
			Logger.LogError(">>> Entering ProcessModuleLoadException(friendlyMessage={FriendlyMessage},ctrl={Ctrl},exc={Exc},displayErrorMessage={DisplayErrorMessage})", friendlyMessage, ctrl, exc, displayErrorMessage);
            // Exit Early if ThreadAbort Exception
            if (ThreadAbortCheck(exc))
            {
                return;
            }

            try
            {
                if (!Host.UseCustomErrorMessages)
                {
					Logger.LogError("Throwing ModuleLoadException from ProcessModuleLoadException(friendlyMessage={FriendlyMessage},ctrl={Ctrl},exc={Exc},displayErrorMessage={DisplayErrorMessage})", friendlyMessage, ctrl, exc, displayErrorMessage);
                    throw new ModuleLoadException(friendlyMessage, exc);
                }
                else
                {
                    var ctrlModule = ctrl as IModuleControl;
                    ModuleLoadException lex = null;
                    var friendlyMessageOverride = exc.Message;

                    if (Host.DebugMode && exc is HttpParseException)
                    {
                        var httpParseError = (HttpParseException)exc;
                        friendlyMessageOverride = string.Concat(exc.Message, " in ", httpParseError.VirtualPath, ":line ", httpParseError.Line);
                    }

                    if (ctrlModule == null)
                    {
                        lex = new ModuleLoadException(friendlyMessageOverride, exc);
                    }
                    else
                    {
                        lex = new ModuleLoadException(friendlyMessageOverride, exc, ctrlModule.ModuleContext.Configuration);
                    }

                    // publish the exception
                    var objExceptionLog = new ExceptionLogController();
                    objExceptionLog.AddLog(lex);

                    // Some modules may want to suppress an error message
                    // and just log the exception.
                    if (displayErrorMessage)
                    {
                        PlaceHolder errorPlaceholder = null;
                        if (ctrl.Parent != null)
                        {
                            errorPlaceholder = (PlaceHolder)ctrl.Parent.FindControl("MessagePlaceHolder");
                        }

                        if (errorPlaceholder != null)
                        {
                            // hide the module
                            ctrl.Visible = false;
							Logger.LogDebug("ProcessModuleLoadException(friendlyMessage={FriendlyMessage},ctrl={Ctrl},exc={Exc},displayErrorMessage={DisplayErrorMessage}): ctrl.Visible → {Arg4}", friendlyMessage, ctrl, exc, displayErrorMessage, ctrl.Visible);
                            errorPlaceholder.Visible = true;
							Logger.LogDebug("ProcessModuleLoadException(friendlyMessage={FriendlyMessage},ctrl={Ctrl},exc={Exc},displayErrorMessage={DisplayErrorMessage}): errorPlaceholder.Visible → {Arg4}", friendlyMessage, ctrl, exc, displayErrorMessage, errorPlaceholder.Visible);
                            errorPlaceholder.Controls.Add(new ErrorContainer(friendlyMessage, lex).Container);
                        }
                        else
                        {
                            // there's no ErrorPlaceholder, add it to the module's control collection
                            ctrl.Controls.Add(new ErrorContainer(friendlyMessage, lex).Container);
                        }
                    }
                }
            }
            catch (Exception exc2)
            {
                Logger.Fatal(exc2);
                ProcessPageLoadException(exc2);
            }

            Logger.ErrorFormat(CultureInfo.InvariantCulture, "FriendlyMessage=\"{0}\" ctrl=\"{1}\" exc=\"{2}\"", friendlyMessage, ctrl, exc);
			Logger.LogWarning("<<< Exiting ProcessModuleLoadException(friendlyMessage={FriendlyMessage},ctrl={Ctrl},exc={Exc},displayErrorMessage={DisplayErrorMessage})", friendlyMessage, ctrl, exc, displayErrorMessage);
        }

        /// <summary>Processes the page load exception.</summary>
        /// <param name="exc">The exception.</param>
        public static void ProcessPageLoadException(Exception exc)
        {
			Logger.LogError(">>> Entering ProcessPageLoadException(exc={Exc})", exc);
            string appURL = Globals.ApplicationURL();
            if (appURL.IndexOf("?", StringComparison.Ordinal) == Null.NullInteger)
            {
                appURL += "?def=ErrorMessage";
            }
            else
            {
                appURL += "&def=ErrorMessage";
            }

            ProcessPageLoadException(exc, appURL);
			Logger.LogWarning("<<< Exiting ProcessPageLoadException(exc={Exc})", exc);
        }

        /// <summary>Processes the page load exception.</summary>
        /// <param name="exc">The exception.</param>
        /// <param name="url">The URL.</param>
        public static void ProcessPageLoadException(Exception exc, string url)
        {
			Logger.LogError(">>> Entering ProcessPageLoadException(exc={Exc},url={Url})", exc, url);
            Logger.Error(url, exc);
            if (ThreadAbortCheck(exc))
            {
                return;
            }

            if (!Host.UseCustomErrorMessages)
            {
				Logger.LogError("Throwing PageLoadException from ProcessPageLoadException(exc={Exc},url={Url})", exc, url);
                throw new PageLoadException(exc == null ? string.Empty : exc.Message, exc);
            }
            else
            {
                var lex = new PageLoadException(exc == null ? string.Empty : exc.Message, exc);

                // publish the exception
                var objExceptionLog = new ExceptionLogController();
                objExceptionLog.AddLog(lex);
                if (!string.IsNullOrEmpty(url))
                {
                    // redirect
                    if (url.Contains("error=terminate", StringComparison.OrdinalIgnoreCase))
                    {
                        HttpContext.Current.Response.Clear();
                        HttpContext.Current.Server.Transfer("~/ErrorPage.aspx");
                    }
                    else
                    {
                        HttpContext.Current.Response.Redirect(url, true);
                    }
                }
            }
			Logger.LogWarning("<<< Exiting ProcessPageLoadException(exc={Exc},url={Url})", exc, url);
        }

        /// <summary>Logs a <see cref="ModuleLoadException"/>.</summary>
        /// <param name="exc">The exception.</param>
        public static void LogException(ModuleLoadException exc)
        {
			Logger.LogError(">>> Entering LogException(exc={Exc})", exc);
            Logger.Error(exc);
            var objExceptionLog = new ExceptionLogController();
            objExceptionLog.AddLog(exc, ExceptionLogController.ExceptionLogType.MODULE_LOAD_EXCEPTION);
			Logger.LogWarning("<<< Exiting LogException(exc={Exc})", exc);
        }

        /// <summary>Logs a <see cref="PageLoadException"/>.</summary>
        /// <param name="exc">The exception.</param>
        public static void LogException(PageLoadException exc)
        {
            Logger.Error(exc);
            var objExceptionLog = new ExceptionLogController();
            objExceptionLog.AddLog(exc, ExceptionLogController.ExceptionLogType.PAGE_LOAD_EXCEPTION);
        }

        /// <summary>Logs a <see cref="SchedulerException"/>.</summary>
        /// <param name="exc">The exception.</param>
        public static void LogException(SchedulerException exc)
        {
            Logger.Error(exc);
            var objExceptionLog = new ExceptionLogController();
            objExceptionLog.AddLog(exc, ExceptionLogController.ExceptionLogType.SCHEDULER_EXCEPTION);
        }

        /// <summary>Logs a <see cref="SecurityException"/>.</summary>
        /// <param name="exc">The exception.</param>
        public static void LogException(SecurityException exc)
        {
            Logger.Error(exc);
            var objExceptionLog = new ExceptionLogController();
            objExceptionLog.AddLog(exc, ExceptionLogController.ExceptionLogType.SECURITY_EXCEPTION);
        }

        /// <summary>Logs an exception.</summary>
        /// <param name="exc">The exception.</param>
        public static void LogException(Exception exc)
        {
            Logger.Error(exc);
            var objExceptionLog = new ExceptionLogController();
            objExceptionLog.AddLog(exc, ExceptionLogController.ExceptionLogType.GENERAL_EXCEPTION);
        }

        /// <summary>Processes the scheduler exception.</summary>
        /// <param name="exc">The exception.</param>
        public static void ProcessSchedulerException(Exception exc)
        {
			Logger.LogError(">>> Entering ProcessSchedulerException(exc={Exc})", exc);
            Logger.Error(exc);
            var objExceptionLog = new ExceptionLogController();
            objExceptionLog.AddLog(exc, ExceptionLogController.ExceptionLogType.SCHEDULER_EXCEPTION);
			Logger.LogWarning("<<< Exiting ProcessSchedulerException(exc={Exc})", exc);
        }

        /// <summary>Logs the search exception.</summary>
        /// <param name="exc">The exception.</param>
        public static void LogSearchException(SearchException exc)
        {
			Logger.LogError(">>> Entering LogSearchException(exc={Exc})", exc);
            Logger.Error(exc);
            var objExceptionLog = new ExceptionLogController();
            objExceptionLog.AddLog(exc, ExceptionLogController.ExceptionLogType.SEARCH_INDEXER_EXCEPTION);
			Logger.LogWarning("<<< Exiting LogSearchException(exc={Exc})", exc);
        }

        /// <summary>Resets a thread abort if the exception is a <see cref="ThreadAbortException"/>.</summary>
        /// <param name="exc">The exception.</param>
        /// <returns><see langword="true"/> if <paramref name="exc"/> is a <see cref="ThreadAbortException"/> instance, otherwise <see langword="false"/>.</returns>
        private static bool ThreadAbortCheck(Exception exc)
        {
			Logger.LogError(">>> Entering ThreadAbortCheck(exc={Exc})", exc);
            if (exc is ThreadAbortException)
            {
                Thread.ResetAbort();
                return true;
            }
            else
            {
				Logger.LogWarning("<<< Exiting ThreadAbortCheck(exc={Exc})", exc);
                return false;
            }
        }

        private static void ProcessHttpException(HttpException exc, string url)
        {
			Logger.LogError(">>> Entering ProcessHttpException(exc={Exc},url={Url})", exc, url);
            var notFoundErrorString = Localization.GetString("ResourceNotFound", Localization.SharedResourceFile);
            Logger.Error(notFoundErrorString + ": - " + url, exc);

            var log = new LogInfo
            {
                BypassBuffering = true,
                LogTypeKey = nameof(EventLogType.HOST_ALERT),
            };
            log.LogProperties.Add(new LogDetailInfo(notFoundErrorString, "URL"));
            var context = HttpContext.Current;
            if (context != null)
            {
                log.LogProperties.Add(new LogDetailInfo("URL:", url));
            }

            LogController.Instance.AddLog(log);

			Logger.LogWarning("<<< Exiting ProcessHttpException(exc={Exc},url={Url})", exc, url);
            throw exc;
        }
    }
}
