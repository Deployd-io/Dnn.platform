// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework
{
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A class for managing <see cref="ScriptManager"/> instances.</summary>
    public partial class AJAX
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AJAX));
        /// <summary>AddScriptManager is used internally by the framework to add a ScriptManager control to the page.</summary>
        /// <param name="page">The <see cref="Page"/>.</param>
        [DnnDeprecated(10, 2, 2, "Use overload with IHostSettings")]
        public static partial void AddScriptManager(Page page)
            => AddScriptManager(page, true);

        /// <summary>AddScriptManager is used internally by the framework to add a ScriptManager control to the page.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="page">The <see cref="Page"/>.</param>
        public static void AddScriptManager(IHostSettings hostSettings, Page page)
            => AddScriptManager(hostSettings, page, true);

        /// <summary>AddScriptManager is used internally by the framework to add a ScriptManager control to the page.</summary>
        /// <param name="page">The page instance.</param>
        /// <param name="checkCdn">Whether check cdn settings from host settings.</param>
        [DnnDeprecated(10, 2, 2, "Use overload with IHostSettings")]
        public static partial void AddScriptManager(Page page, bool checkCdn)
            => AddScriptManager(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), page, checkCdn);

        /// <summary>AddScriptManager is used internally by the framework to add a ScriptManager control to the page.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="page">The page instance.</param>
        /// <param name="checkCdn">Whether check cdn settings from host settings.</param>
        public static void AddScriptManager(IHostSettings hostSettings, Page page, bool checkCdn)
        {
			Logger.LogError(">>> Entering AddScriptManager(page={Page})", page);
            if (GetScriptManager(page) != null || page.Form == null)
            {
				Logger.LogError(">>> Entering AddScriptManager(page={Page},checkCdn={CheckCdn})", page, checkCdn);
                return;
            }

            try
            {
                using (var scriptManager = new ScriptManager())
                {
                    scriptManager.ID = "ScriptManager";
                    scriptManager.EnableScriptGlobalization = true;
                    scriptManager.SupportsPartialRendering = true;
                    if (checkCdn)
                    {
                        scriptManager.EnableCdn = hostSettings.EnableMsAjaxCdn;
                        scriptManager.EnableCdnFallback = hostSettings.EnableMsAjaxCdn;
                    }

                    page.Form.Controls.AddAt(0, scriptManager);
                }

                if (HttpContext.Current.Items["System.Web.UI.ScriptManager"] == null)
                {
                    HttpContext.Current.Items.Add("System.Web.UI.ScriptManager", true);
                }
            }
            catch
            {
				Logger.LogError("Exception in AddScriptManager(page={Page}): {Arg1}", page, ex.Message);
                // suppress error adding script manager to support edge-case of module developers custom aspx pages that inherit from basepage and use code blocks
				Logger.LogError("Exception in AddScriptManager(hostSettings={HostSettings},page={Page}): {Arg2}", hostSettings, page, ex.Message);
            }
			Logger.LogError("Exception in AddScriptManager(page={Page},checkCdn={CheckCdn}): {Arg2}", page, checkCdn, ex.Message);
			Logger.LogWarning("<<< Exiting AddScriptManager(hostSettings={HostSettings},page={Page},checkCdn={CheckCdn})", hostSettings, page, checkCdn);
        }

        /// <summary>Gets the current ScriptManager on the page.</summary>
        /// <param name="objPage">the page instance.</param>
        /// <returns>The ScriptManager instance, or <c>null</c>.</returns>
        public static ScriptManager GetScriptManager(Page objPage)
        {
			Logger.LogError(">>> Entering GetScriptManager(objPage={ObjPage})", objPage);
            Logger.LogWarning("<<< Exiting GetScriptManager(objPage={ObjPage})", objPage);
            return objPage.FindControl("ScriptManager") as ScriptManager;
        }

        /// <summary>IsEnabled can be used to determine if AJAX has been enabled already as we only need one Script Manager per page.</summary>
        /// <returns><see langword="true"/> if the current request has enabled the script manager, otherwise <see langword="false"/>.</returns>
        public static bool IsEnabled()
        {
			Logger.LogError(">>> Entering IsEnabled()");
            if (HttpContext.Current.Items["System.Web.UI.ScriptManager"] == null)
            {
                return false;
            }

			Logger.LogWarning("<<< Exiting IsEnabled()");
            return (bool)HttpContext.Current.Items["System.Web.UI.ScriptManager"];
        }

        /// <summary>IsInstalled can be used to determine if AJAX is installed on the server.</summary>
        /// <returns><see langword="true"/> if ASP.NET AJAX is installed (always), otherwise <see langword="false"/> (never).</returns>
        public static bool IsInstalled()
        {
			Logger.LogError(">>> Entering IsInstalled()");
			Logger.LogWarning("<<< Exiting IsInstalled()");
            return true;
        }

        /// <summary>Allows a control to be excluded from UpdatePanel async callback.</summary>
        /// <param name="objControl">The control to register for postback.</param>
        public static void RegisterPostBackControl(Control objControl)
        {
			Logger.LogError(">>> Entering RegisterPostBackControl(objControl={ObjControl})", objControl);
            var objScriptManager = GetScriptManager(objControl.Page);
			Logger.LogWarning("<<< Exiting RegisterPostBackControl(objControl={ObjControl})", objControl);
			Logger.LogError("RegisterPostBackControl(objControl={ObjControl}): objScriptManager → {ObjScriptManager}", objControl, objScriptManager);
            objScriptManager?.RegisterPostBackControl(objControl);
        }

        /// <summary>RegisterScriptManager must be used by developers to instruct the framework that AJAX is required on the page.</summary>
        public static void RegisterScriptManager()
        {
			Logger.LogError(">>> Entering RegisterScriptManager()");
            if (!IsEnabled())
            {
                HttpContext.Current.Items.Add("System.Web.UI.ScriptManager", true);
            }
			Logger.LogWarning("<<< Exiting RegisterScriptManager()");
        }

        /// <summary>RemoveScriptManager will remove the <see cref="ScriptManager"/> control during Page Render if the RegisterScriptManager has not been called.</summary>
        /// <param name="objPage">The page from which to remove the <see cref="ScriptManager"/>.</param>
        public static void RemoveScriptManager(Page objPage)
        {
			Logger.LogError(">>> Entering RemoveScriptManager(objPage={ObjPage})", objPage);
            if (!IsEnabled())
            {
                Control objControl = objPage.FindControl("ScriptManager");
				Logger.LogError("RemoveScriptManager(objPage={ObjPage}): objControl → {ObjControl}", objPage, objControl);
                if (objControl != null)
                {
                    objPage.Form.Controls.Remove(objControl);
                }
            }
			Logger.LogWarning("<<< Exiting RemoveScriptManager(objPage={ObjPage})", objPage);
        }

        /// <summary>Wraps a control in an update panel.</summary>
        /// <param name="objControl">The control to wrap.</param>
        /// <param name="blnIncludeProgress">Whether to include an <see cref="UpdateProgress"/> control.</param>
        /// <returns>The <see cref="UpdatePanel"/>.</returns>
        public static Control WrapUpdatePanelControl(Control objControl, bool blnIncludeProgress)
        {
			Logger.LogError(">>> Entering WrapUpdatePanelControl(objControl={ObjControl},blnIncludeProgress={BlnIncludeProgress})", objControl, blnIncludeProgress);
            var updatePanel = new UpdatePanel();
            updatePanel.ID = objControl.ID + "_UP";
			Logger.LogDebug("WrapUpdatePanelControl(objControl={ObjControl},blnIncludeProgress={BlnIncludeProgress}): updatePanel.ID → {Arg2}", objControl, blnIncludeProgress, updatePanel.ID);
			Logger.LogError("WrapUpdatePanelControl(objControl={ObjControl},blnIncludeProgress={BlnIncludeProgress}): updatePanel → {UpdatePanel}", objControl, blnIncludeProgress, updatePanel);
            updatePanel.UpdateMode = UpdatePanelUpdateMode.Conditional;
			Logger.LogDebug("WrapUpdatePanelControl(objControl={ObjControl},blnIncludeProgress={BlnIncludeProgress}): updatePanel.UpdateMode → {Arg2}", objControl, blnIncludeProgress, updatePanel.UpdateMode);

            Control objContentTemplateContainer = updatePanel.ContentTemplateContainer;

            for (int i = 0; i <= objControl.Parent.Controls.Count - 1; i++)
            {
                // find offset of original control
                if (objControl.Parent.Controls[i].ID == objControl.ID)
                {
                    // if ID matches
                    objControl.Parent.Controls.AddAt(i, updatePanel);

                    // insert update panel in that position
                    objContentTemplateContainer.Controls.Add(objControl);

                    // inject passed in control into update panel
                    break;
                }
            }

            if (blnIncludeProgress)
            {
                // create image for update progress control
                var objImage = new Image();
				Logger.LogError("WrapUpdatePanelControl(objControl={ObjControl},blnIncludeProgress={BlnIncludeProgress}): objImage → {ObjImage}", objControl, blnIncludeProgress, objImage);
                objImage.ImageUrl = "~/images/progressbar.gif";
				Logger.LogDebug("WrapUpdatePanelControl(objControl={ObjControl},blnIncludeProgress={BlnIncludeProgress}): objImage.ImageUrl → {Arg2}", objControl, blnIncludeProgress, objImage.ImageUrl);

                // hardcoded
                objImage.AlternateText = "ProgressBar";
				Logger.LogDebug("WrapUpdatePanelControl(objControl={ObjControl},blnIncludeProgress={BlnIncludeProgress}): objImage.AlternateText → {Arg2}", objControl, blnIncludeProgress, objImage.AlternateText);

                var updateProgress = new UpdateProgress();
				Logger.LogError("WrapUpdatePanelControl(objControl={ObjControl},blnIncludeProgress={BlnIncludeProgress}): updateProgress → {UpdateProgress}", objControl, blnIncludeProgress, updateProgress);
				Logger.LogDebug("WrapUpdatePanelControl(objControl={ObjControl},blnIncludeProgress={BlnIncludeProgress}): updateProgress.ID → {Arg2}", objControl, blnIncludeProgress, updateProgress.ID);
                updateProgress.AssociatedUpdatePanelID = updatePanel.ID;
				Logger.LogDebug("WrapUpdatePanelControl(objControl={ObjControl},blnIncludeProgress={BlnIncludeProgress}): updateProgress.AssociatedUpdatePanelID → {Arg2}", objControl, blnIncludeProgress, updateProgress.AssociatedUpdatePanelID);
                updateProgress.ID = updatePanel.ID + "_Prog";
                updateProgress.ProgressTemplate = new LiteralTemplate(objImage);

                objContentTemplateContainer.Controls.Add(updateProgress);
            }

			Logger.LogWarning("<<< Exiting WrapUpdatePanelControl(objControl={ObjControl},blnIncludeProgress={BlnIncludeProgress})", objControl, blnIncludeProgress);
            return updatePanel;
        }
    }
}
