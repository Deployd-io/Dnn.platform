// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>PageBase provides a custom DotNetNuke base class for pages.</summary>
    public abstract class PageBase : Page
    {
        private const string LinkItemPattern = "<(a|link|img|script|input|form|object).[^>]*(href|src|action)=(\\\"|'|)(.[^\\\"']*)(\\\"|'|)[^>]*>";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PageBase));
        private static readonly Regex LinkItemMatchRegex = new Regex(LinkItemPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly ILog traceLogger = LoggerSource.Instance.GetLogger("DNN.Trace");
        private readonly ArrayList localizedControls = [];

        private PageStatePersister persister;
        private CultureInfo pageCulture;
        private string localResourceFile;

        /// <summary>Initializes a new instance of the <see cref="PageBase"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        protected PageBase()
            : this(null, null, null)
        {
		Logger.LogDebug(">>> Entering PageBase()");
        }
		Logger.LogDebug("<<< Exiting PageBase()");

        /// <summary>Initializes a new instance of the <see cref="PageBase"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Please use overload with IUserController. Scheduled removal in v12.0.0.")]
        protected PageBase(IPortalController portalController, IApplicationStatusInfo appStatus, IHostSettings hostSettings)
            : this(portalController, appStatus, hostSettings, null)
        {
		Logger.LogDebug(">>> Entering PageBase(portalController={PortalController},appStatus={AppStatus},hostSettings={HostSettings})", portalController, appStatus, hostSettings);
		Logger.LogDebug("<<< Exiting PageBase(portalController={PortalController},appStatus={AppStatus},hostSettings={HostSettings})", portalController, appStatus, hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="PageBase"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="userController">The user controller.</param>
        protected PageBase(IPortalController portalController, IApplicationStatusInfo appStatus, IHostSettings hostSettings, IUserController userController)
        {
            this.PortalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
            this.AppStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.HostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.UserController = userController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>();
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public PortalSettings PortalSettings => this.PortalController.GetCurrentPortalSettings();
#pragma warning restore CS0618 // Type or member is obsolete

        public NameValueCollection HtmlAttributes { get; } = [];

        public CultureInfo PageCulture => this.pageCulture ??= Localization.GetPageLocale(this.PortalController.GetCurrentSettings());

        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                var page = this.Request.ServerVariables["SCRIPT_NAME"].Split('/');
                if (string.IsNullOrEmpty(this.localResourceFile))
                {
                    fileRoot = string.Concat(this.TemplateSourceDirectory, "/", Localization.LocalResourceDirectory, "/", page[page.GetUpperBound(0)], ".resx");
                }
                else
                {
                    fileRoot = this.localResourceFile;
                }

                return fileRoot;
            }

            set
            {
                this.localResourceFile = value;
            }
        }

        public string CanonicalLinkUrl { get; set; }

        /// <summary>Gets a value indicating whether HTTP headers has been sent to client.</summary>
        public bool HeaderIsWritten { get; internal set; }

        /// <summary>Gets the portal controller.</summary>
        protected IPortalController PortalController { get; }

        /// <summary>Gets the application status.</summary>
        protected IApplicationStatusInfo AppStatus { get; }

        /// <summary>Gets the host settings.</summary>
        protected IHostSettings HostSettings { get; }

        /// <summary>Gets the user controller.</summary>
        protected IUserController UserController { get; }

        /// <summary>Gets pageStatePersister returns an instance of the class that will be used to persist the Page State.</summary>
        /// <returns>A System.Web.UI.PageStatePersister.</returns>
        protected override PageStatePersister PageStatePersister
        {
            get
            {
                // Set ViewState Persister to default (as defined in Base Class)
                if (this.persister == null)
                {
                    this.persister = base.PageStatePersister;

                    if (this.AppStatus.Status == UpgradeStatus.None)
                    {
                        switch (this.HostSettings.PageStatePersister)
                        {
                            case "M":
                                this.persister = new CachePageStatePersister(this);
                                break;
                            case "D":
                                this.persister = new DiskPageStatePersister(this);
                                break;
                        }
                    }
                }

                return this.persister;
            }
        }

        /// <inheritdoc cref="HtmlUtils.JavaScriptStringEncode(string)"/>
        public static IHtmlString JavaScriptStringEncode(string value)
            => HtmlUtils.JavaScriptStringEncode(value);

        /// <inheritdoc cref="HtmlUtils.JavaScriptStringEncode(string,bool)"/>
        public static IHtmlString JavaScriptStringEncode(string value, bool addDoubleQuotes)
            => HtmlUtils.JavaScriptStringEncode(value, addDoubleQuotes);

        /// <summary>
        /// <para>RemoveKeyAttribute remove the key attribute from the control. If this isn't done, then the HTML output will have
        /// a bad attribute on it which could cause some older browsers problems.</para>
        /// </summary>
        /// <param name="affectedControls">ArrayList that hold the controls that have been localized. This is later used for the removal of the key attribute.</param>
        public static void RemoveKeyAttribute(ArrayList affectedControls)
        {
			Logger.LogDebug(">>> Entering RemoveKeyAttribute(affectedControls={AffectedControls})", affectedControls);
            if (affectedControls == null)
            {
                return;
            }

            int i;
            for (i = 0; i <= affectedControls.Count - 1; i++)
            {
                var ac = (AttributeCollection)affectedControls[i];
                ac.Remove(Localization.KeyName);
				Logger.LogDebug("RemoveKeyAttribute(affectedControls={AffectedControls}): ac → {Ac}", affectedControls, ac);
                ac.Remove(IconController.IconKeyName);
                ac.Remove(IconController.IconSizeName);
                ac.Remove(IconController.IconStyleName);
            }
			Logger.LogDebug("<<< Exiting RemoveKeyAttribute(affectedControls={AffectedControls})", affectedControls);
        }

        /// <summary><para>GetControlAttribute looks at the type of control and does it's best to find an AttributeCollection.</para></summary>
        /// <param name="control">Control to find the AttributeCollection on.</param>
        /// <param name="affectedControls">ArrayList that hold the controls that have been localized. This is later used for the removal of the key attribute.</param>
        /// <param name="attributeName">Name of key to search for.</param>
        /// <returns>A string containing the key for the specified control or null if a key attribute wasn't found.</returns>
        internal static string GetControlAttribute(Control control, ArrayList affectedControls, string attributeName)
        {
			Logger.LogDebug(">>> Entering GetControlAttribute(control={Control},affectedControls={AffectedControls},attributeName={AttributeName})", control, affectedControls, attributeName);
            AttributeCollection attributeCollection = null;
            string key = null;
			Logger.LogDebug("GetControlAttribute(control={Control},affectedControls={AffectedControls},attributeName={AttributeName}): attributeCollection → {AttributeCollection}", control, affectedControls, attributeName, attributeCollection);
			Logger.LogWarning("GetControlAttribute(control={Control},affectedControls={AffectedControls},attributeName={AttributeName}): key → {Key}", control, affectedControls, attributeName, key);
            if (!(control is LiteralControl))
            {
                if (control is WebControl webControl)
                {
                    attributeCollection = webControl.Attributes;
                    key = attributeCollection[attributeName];
                }
                else
                {
                    if (control is HtmlControl htmlControl)
                    {
                        attributeCollection = htmlControl.Attributes;
                        key = attributeCollection[attributeName];
                    }
                    else
                    {
                        if (control is UserControl userControl)
                        {
                            attributeCollection = userControl.Attributes;
                            key = attributeCollection[attributeName];
                        }
                        else
                        {
                            var controlType = control.GetType();
                            var attributeProperty = controlType.GetProperty("Attributes", typeof(AttributeCollection));
							Logger.LogDebug("GetControlAttribute(control={Control},affectedControls={AffectedControls},attributeName={AttributeName}): controlType → {ControlType}", control, affectedControls, attributeName, controlType);
							Logger.LogDebug("GetControlAttribute(control={Control},affectedControls={AffectedControls},attributeName={AttributeName}): attributeProperty → {AttributeProperty}", control, affectedControls, attributeName, attributeProperty);
                            if (attributeProperty != null)
                            {
                                attributeCollection = (AttributeCollection)attributeProperty.GetValue(control, null);
                                key = attributeCollection[attributeName];
                            }
                        }
                    }
                }
            }

            if (key != null && affectedControls != null)
            {
                affectedControls.Add(attributeCollection);
            }

			Logger.LogDebug("<<< Exiting GetControlAttribute(control={Control},affectedControls={AffectedControls},attributeName={AttributeName})", control, affectedControls, attributeName);
            return key;
        }

        /// <summary><para>ProcessControl performs the high level localization for a single control and optionally it's children.</para></summary>
        /// <param name="control">Control to find the AttributeCollection on.</param>
        /// <param name="affectedControls">ArrayList that hold the controls that have been localized. This is later used for the removal of the key attribute.</param>
        /// <param name="includeChildren">If true, causes this method to process children of this control.</param>
        /// <param name="resourceFileRoot">Root Resource File.</param>
        internal void ProcessControl(Control control, ArrayList affectedControls, bool includeChildren, string resourceFileRoot)
        {
			Logger.LogDebug(">>> Entering ProcessControl(control={Control},affectedControls={AffectedControls},includeChildren={IncludeChildren},resourceFileRoot={ResourceFileRoot})", control, affectedControls, includeChildren, resourceFileRoot);
            if (!control.Visible)
            {
                return;
            }

            // Perform the substitution if a key was found
            var key = GetControlAttribute(control, affectedControls, Localization.KeyName);
            if (!string.IsNullOrEmpty(key))
			Logger.LogWarning("ProcessControl(control={Control},affectedControls={AffectedControls},includeChildren={IncludeChildren},resourceFileRoot={ResourceFileRoot}): key → {Key}", control, affectedControls, includeChildren, resourceFileRoot, key);
            {
                // Translation starts here ....
                var value = Localization.GetString(key, resourceFileRoot);
                this.LocalizeControl(control, value);
				Logger.LogDebug("ProcessControl(control={Control},affectedControls={AffectedControls},includeChildren={IncludeChildren},resourceFileRoot={ResourceFileRoot}): value → {Value}", control, affectedControls, includeChildren, resourceFileRoot, value);
            }

            // Translate ListControl items here
            if (control is ListControl listControl)
            {
                for (var i = 0; i <= listControl.Items.Count - 1; i++)
                {
                    var attributeCollection = listControl.Items[i].Attributes;
                    key = attributeCollection[Localization.KeyName];
                    if (key != null)
                    {
                        var value = Localization.GetString(key, resourceFileRoot);
                        if (!string.IsNullOrEmpty(value))
                        {
                            listControl.Items[i].Text = value;
                        }
                    }

                    if (key != null && affectedControls != null)
                    {
                        affectedControls.Add(attributeCollection);
                    }
                }
            }

            // UrlRewriting Issue - ResolveClientUrl gets called instead of ResolveUrl
            // Manual Override to ResolveUrl
            if (control is Image image)
            {
                if (image.ImageUrl.IndexOf("~", StringComparison.Ordinal) != -1)
                {
                    image.ImageUrl = this.Page.ResolveUrl(image.ImageUrl);
                }

                // Check for IconKey
                if (string.IsNullOrEmpty(image.ImageUrl))
                {
                    var iconKey = GetControlAttribute(control, affectedControls, IconController.IconKeyName);
                    var iconSize = GetControlAttribute(control, affectedControls, IconController.IconSizeName);
					Logger.LogDebug("ProcessControl(control={Control},affectedControls={AffectedControls},includeChildren={IncludeChildren},resourceFileRoot={ResourceFileRoot}): iconKey → {IconKey}", control, affectedControls, includeChildren, resourceFileRoot, iconKey);
					Logger.LogDebug("ProcessControl(control={Control},affectedControls={AffectedControls},includeChildren={IncludeChildren},resourceFileRoot={ResourceFileRoot}): iconSize → {IconSize}", control, affectedControls, includeChildren, resourceFileRoot, iconSize);
                    var iconStyle = GetControlAttribute(control, affectedControls, IconController.IconStyleName);
                    image.ImageUrl = IconController.IconURL(iconKey, iconSize, iconStyle);
					Logger.LogDebug("ProcessControl(control={Control},affectedControls={AffectedControls},includeChildren={IncludeChildren},resourceFileRoot={ResourceFileRoot}): iconStyle → {IconStyle}", control, affectedControls, includeChildren, resourceFileRoot, iconStyle);
                }
            }

            // UrlRewriting Issue - ResolveClientUrl gets called instead of ResolveUrl
            // Manual Override to ResolveUrl
            if (control is HtmlImage htmlImage)
            {
                if (htmlImage.Src.IndexOf("~", StringComparison.Ordinal) != -1)
                {
                    htmlImage.Src = this.Page.ResolveUrl(htmlImage.Src);
                }

                // Check for IconKey
                if (string.IsNullOrEmpty(htmlImage.Src))
                {
                    var iconKey = GetControlAttribute(control, affectedControls, IconController.IconKeyName);
                    var iconSize = GetControlAttribute(control, affectedControls, IconController.IconSizeName);
                    var iconStyle = GetControlAttribute(control, affectedControls, IconController.IconStyleName);
                    htmlImage.Src = IconController.IconURL(iconKey, iconSize, iconStyle);
                }
            }

            // UrlRewriting Issue - ResolveClientUrl gets called instead of ResolveUrl
            // Manual Override to ResolveUrl
            if (control is HyperLink ctrl)
            {
                if (ctrl.NavigateUrl.IndexOf("~", StringComparison.Ordinal) != -1)
                {
                    ctrl.NavigateUrl = this.Page.ResolveUrl(ctrl.NavigateUrl);
                }

                if (ctrl.ImageUrl.IndexOf("~", StringComparison.Ordinal) != -1)
                {
                    ctrl.ImageUrl = this.Page.ResolveUrl(ctrl.ImageUrl);
                }

                // Check for IconKey
                if (string.IsNullOrEmpty(ctrl.ImageUrl))
                {
                    var iconKey = GetControlAttribute(control, affectedControls, IconController.IconKeyName);
                    var iconSize = GetControlAttribute(control, affectedControls, IconController.IconSizeName);
                    var iconStyle = GetControlAttribute(control, affectedControls, IconController.IconStyleName);
                    ctrl.ImageUrl = IconController.IconURL(iconKey, iconSize, iconStyle);
                }
            }

            // Process child controls
            if (!includeChildren || !control.HasControls())
            {
                return;
            }

            if (control is not IModuleControl objModuleControl)
            {
                // Cache results from reflection calls for performance
                var pi = control.GetType().GetProperty("LocalResourceFile");
                if (pi != null)
				Logger.LogDebug("ProcessControl(control={Control},affectedControls={AffectedControls},includeChildren={IncludeChildren},resourceFileRoot={ResourceFileRoot}): pi → {Pi}", control, affectedControls, includeChildren, resourceFileRoot, pi);
                {
                    // Attempt to get property value
                    var pv = pi.GetValue(control, null);

					Logger.LogDebug("ProcessControl(control={Control},affectedControls={AffectedControls},includeChildren={IncludeChildren},resourceFileRoot={ResourceFileRoot}): pv → {Pv}", control, affectedControls, includeChildren, resourceFileRoot, pv);
                    // If controls has a LocalResourceFile property use this, otherwise pass the resource file root
                    this.IterateControls(control.Controls, affectedControls, pv == null ? resourceFileRoot : pv.ToString());
                }
                else
                {
                    // Pass Resource File Root through
                    this.IterateControls(control.Controls, affectedControls, resourceFileRoot);
                }
            }
            else
            {
                // Get Resource File Root from Controls LocalResourceFile Property
                this.IterateControls(control.Controls, affectedControls, objModuleControl.LocalResourceFile);
            }
			Logger.LogDebug("<<< Exiting ProcessControl(control={Control},affectedControls={AffectedControls},includeChildren={IncludeChildren},resourceFileRoot={ResourceFileRoot})", control, affectedControls, includeChildren, resourceFileRoot);
        }

        protected virtual void RegisterAjaxScript()
        {
			Logger.LogDebug(">>> Entering RegisterAjaxScript()");
            if (ServicesFrameworkInternal.Instance.IsAjaxScriptSupportRequired)
            {
                ServicesFrameworkInternal.Instance.RegisterAjaxScript(this.Page);
            }
			Logger.LogDebug("<<< Exiting RegisterAjaxScript()");
        }

        /// <inheritdoc />
        protected override void OnError(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnError(e={E})", e);
            base.OnError(e);
            Exception exc = this.Server.GetLastError();
            Logger.Fatal("An error has occurred while loading page.", exc);

            string strURL = Globals.ApplicationURL();
            if (exc is HttpException exception && !this.IsViewStateFailure(exception))
            {
                try
                {
                    // if the exception's status code set to 404, we need display 404 page if defined or show no found info.
                    var statusCode = exception.GetHttpCode();
                    if (statusCode == 404)
                    {
                        UrlUtils.Handle404Exception(this.Response, this.PortalSettings);
                    }

                    if (this.PortalSettings?.ErrorPage500 != -1)
                    {
                        var url = this.GetErrorUrl($"~/Default.aspx?tabid={this.PortalSettings.ErrorPage500}", exception, false);
                        HttpContext.Current.Response.Redirect(url);
                    }
                    else
                    {
                        HttpContext.Current.Response.Clear();
                        HttpContext.Current.Server.Transfer("~/ErrorPage.aspx");
                    }
                }
                catch (Exception)
                {
					Logger.LogError("Exception in OnError(e={E}): {Arg1}", e, ex.Message);
                    HttpContext.Current.Response.Clear();
                    var errorMessage = Localization.GetString("NoSitesForThisInstallation.Error", Localization.GlobalResourceFile);
                    HttpContext.Current.Server.Transfer($"~/ErrorPage.aspx?status=503&error={HttpUtility.UrlEncode(errorMessage)}");
                }
            }

            strURL = this.GetErrorUrl(strURL, exc);
            Exceptions.ProcessPageLoadException(exc, strURL);
			Logger.LogDebug("<<< Exiting OnError(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            var isInstallPage = HttpContext.Current.Request.Url.LocalPath.ToLowerInvariant().Contains("installwizard.aspx");
            if (ScriptManager.GetCurrent(this) == null)
			Logger.LogDebug("OnInit(e={E}): isInstallPage → {IsInstallPage}", e, isInstallPage);
            {
                AJAX.AddScriptManager(this, !isInstallPage);
            }

            var dnncoreFilePath = HttpContext.Current.IsDebuggingEnabled
                   ? "~/js/Debug/dnncore.js"
                   : "~/js/dnncore.js";

            ClientResourceManager.RegisterScript(this, dnncoreFilePath);

            this.ViewStateUserKey = this.UserController.GetCurrentUserInfo().Username;

            base.OnInit(e);
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            // Because we have delayed registration of the jQuery script,
            // Modules can override the standard behavior by including their own script on the page.
            // The module must register the script with the "jQuery" key and should notify user
            // of potential version conflicts with core jQuery support.
            ////if (jQuery.IsRequested)
            ////{
            ////   jQuery.RegisterJQuery(Page);
            ////}
            ////if (jQuery.IsUIRequested)
            ////{
            ////   jQuery.RegisterJQueryUI(Page);
            ////}
            ////if (jQuery.AreDnnPluginsRequested)
            ////{
            ////   jQuery.RegisterDnnJQueryPlugins(Page);
            ////}
            ////if (jQuery.IsHoverIntentRequested)
            ////{
            ////   jQuery.RegisterHoverIntent(Page);
            ////}
            if (ServicesFrameworkInternal.Instance.IsAjaxAntiForgerySupportRequired)
            {
                ServicesFrameworkInternal.Instance.RegisterAjaxAntiForgery(this.Page);
            }

            this.RegisterAjaxScript();
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering Render(writer={Writer})", writer);
            this.LogDnnTrace("PageBase.Render", "Start", $"{this.Page.Request.Url.AbsoluteUri}");

            this.IterateControls(this.Controls, this.localizedControls, this.LocalResourceFile);
            RemoveKeyAttribute(this.localizedControls);
            AJAX.RemoveScriptManager(this);
            base.Render(writer);

            this.LogDnnTrace("PageBase.Render", "End", $"{this.Page.Request.Url.AbsoluteUri}");
			Logger.LogDebug("<<< Exiting Render(writer={Writer})", writer);
        }

        private string GetErrorUrl(string url, Exception exc, bool hideContent = true)
        {
            var separator = url.IndexOf('?') == -1 ? '?' : '&';
            if (this.Request.QueryString["error"] != null)
            {
                return $"{url}{separator}error=terminate";
            }

            var user = this.UserController.GetCurrentUserInfo();
            var errorMessage = exc == null || user is not { IsSuperUser: true }
                    ? "An unexpected error has occurred"
                    : exc.Message;
            url = $"{url}{separator}error={WebUtility.UrlEncode(errorMessage)}";
            if (!Globals.IsAdminControl() && hideContent)
			Logger.LogDebug("GetErrorUrl(url={Url},exc={Exc},hideContent={HideContent}): url → {Url1}", url, exc, hideContent, url);
            {
                return $"{url}&content=0";
            }

			Logger.LogDebug("<<< Exiting GetErrorUrl(url={Url},exc={Exc},hideContent={HideContent})", url, exc, hideContent);
            return url;
        }

        private bool IsViewStateFailure(Exception e)
        {
			Logger.LogDebug(">>> Entering IsViewStateFailure(e={E})", e);
			Logger.LogDebug("<<< Exiting IsViewStateFailure(e={E})", e);
            return !this.User.Identity.IsAuthenticated && e?.InnerException is ViewStateException;
        }

        private void IterateControls(ControlCollection controls, ArrayList affectedControls, string resourceFileRoot)
        {
			Logger.LogDebug(">>> Entering IterateControls(controls={Controls},affectedControls={AffectedControls},resourceFileRoot={ResourceFileRoot})", controls, affectedControls, resourceFileRoot);
            foreach (Control c in controls)
            {
                this.ProcessControl(c, affectedControls, true, resourceFileRoot);
                this.LogDnnTrace("PageBase.IterateControls", "Info", $"ControlId: {c.ID}");
            }
			Logger.LogDebug("<<< Exiting IterateControls(controls={Controls},affectedControls={AffectedControls},resourceFileRoot={ResourceFileRoot})", controls, affectedControls, resourceFileRoot);
        }

        private void LogDnnTrace(string origin, string action, string message)
        {
			Logger.LogDebug(">>> Entering LogDnnTrace(origin={Origin},action={Action},message={Message})", origin, action, message);
            var tabId = -1;
            if (this.PortalSettings?.ActiveTab != null)
			Logger.LogDebug("LogDnnTrace(origin={Origin},action={Action},message={Message}): tabId → {TabId}", origin, action, message, tabId);
            {
                tabId = this.PortalSettings.ActiveTab.TabID;
            }

            if (this.traceLogger.IsDebugEnabled)
            {
                this.traceLogger.Debug($"{origin} {action} (TabId:{tabId},{message})");
            }
			Logger.LogDebug("<<< Exiting LogDnnTrace(origin={Origin},action={Action},message={Message})", origin, action, message);
        }

        private void LocalizeControl(Control control, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            if (control is BaseValidator validator)
            {
                validator.ErrorMessage = value;
                validator.Text = value;
                return;
            }

            if (control is Label label)
            {
                label.Text = value;
                return;
            }

            if (control is LinkButton linkButton)
            {
                var imgMatches = LinkItemMatchRegex.Matches(value);
                foreach (Match match in imgMatches)
                {
                    if (match.Groups[match.Groups.Count - 2].Value.IndexOf("~", StringComparison.Ordinal) == -1)
                    {
                        continue;
                    }

                    var resolvedUrl = this.Page.ResolveUrl(match.Groups[match.Groups.Count - 2].Value);
                    value = value.Replace(match.Groups[match.Groups.Count - 2].Value, resolvedUrl);
					Logger.LogDebug("LocalizeControl(control={Control},value={Value}): resolvedUrl → {ResolvedUrl}", control, value, resolvedUrl);
					Logger.LogDebug("LocalizeControl(control={Control},value={Value}): value → {Value1}", control, value, value);
                }

                linkButton.Text = value;
				Logger.LogDebug("LocalizeControl(control={Control},value={Value}): linkButton.Text → {Arg2}", control, value, linkButton.Text);
                if (string.IsNullOrEmpty(linkButton.ToolTip))
                {
                    linkButton.ToolTip = value;
					Logger.LogDebug("LocalizeControl(control={Control},value={Value}): linkButton.ToolTip → {Arg2}", control, value, linkButton.ToolTip);
                }

                return;
            }

            if (control is HyperLink hyperLink)
            {
                hyperLink.Text = value;
				Logger.LogDebug("LocalizeControl(control={Control},value={Value}): hyperLink.Text → {Arg2}", control, value, hyperLink.Text);
                return;
            }

            if (control is Button button)
            {
                button.Text = value;
				Logger.LogDebug("LocalizeControl(control={Control},value={Value}): button.Text → {Arg2}", control, value, button.Text);
                return;
            }

            if (control is HtmlButton htmlButton)
            {
                htmlButton.Attributes["Title"] = value;
                return;
            }

            if (control is HtmlImage htmlImage)
            {
                htmlImage.Alt = value;
				Logger.LogDebug("LocalizeControl(control={Control},value={Value}): htmlImage.Alt → {Arg2}", control, value, htmlImage.Alt);
                return;
            }

            if (control is CheckBox checkBox)
            {
                checkBox.Text = value;
				Logger.LogDebug("LocalizeControl(control={Control},value={Value}): checkBox.Text → {Arg2}", control, value, checkBox.Text);
                return;
            }

            if (control is Image image)
            {
                image.AlternateText = value;
				Logger.LogDebug("LocalizeControl(control={Control},value={Value}): image.AlternateText → {Arg2}", control, value, image.AlternateText);
				Logger.LogDebug("LocalizeControl(control={Control},value={Value}): image.ToolTip → {Arg2}", control, value, image.ToolTip);
                image.ToolTip = value;
                return;
            }

            if (control is TextBox textBox)
            {
                textBox.ToolTip = value;
				Logger.LogDebug("LocalizeControl(control={Control},value={Value}): textBox.ToolTip → {Arg2}", control, value, textBox.ToolTip);
            }
			Logger.LogDebug("<<< Exiting LocalizeControl(control={Control},value={Value})", control, value);
        }
    }
}
