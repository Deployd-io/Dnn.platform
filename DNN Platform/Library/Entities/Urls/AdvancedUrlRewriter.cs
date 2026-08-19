// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Principal;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Security;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.EventQueue;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The advanced URL rewriter.</summary>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="portalAliasService">The portal alias service.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="appStatus">The application status.</param>
    /// <param name="portalGroupController">The portal group controller.</param>
    public class AdvancedUrlRewriter(IHostSettings hostSettings, IPortalAliasService portalAliasService, IHostSettingsService hostSettingsService, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController)
        : UrlRewriterBase(hostSettings, portalAliasService, hostSettingsService, portalController)
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AdvancedUrlRewriter));
        private const string ProductName = "AdvancedUrlRewriter";
        private static readonly Regex DefaultPageRegex = new Regex(@"(?<!(\?.+))/" + Globals.glbDefaultPage, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex AumDebugRegex = new Regex(@"(&|\?)_aumdebug=[A-Z]+(?:&|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex RewritePathRx = new Regex("(?:&(?<parm>.[^&]+)=$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex UrlSlashesRegex = new Regex("[\\\\/]\\.\\.[\\\\/]", RegexOptions.Compiled);
        private static readonly Regex AliasUrlRegex = new Regex(@"(?:^(?<http>http[s]{0,1}://){0,1})(?:(?<alias>_ALIAS_)(?<path>$|\?[\w]*|/[\w]*))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private readonly IPortalAliasService portalAliasService = portalAliasService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>();
        private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        private readonly IPortalGroupController portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();
        private FriendlyUrlSettings settings;

        /// <summary>Initializes a new instance of the <see cref="AdvancedUrlRewriter"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public AdvancedUrlRewriter()
            : this(null, null, null, null)
        {
		Logger.LogDebug(">>> Entering AdvancedUrlRewriter()");
        }

        /// <summary>Initializes a new instance of the <see cref="AdvancedUrlRewriter"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public AdvancedUrlRewriter(IHostSettings hostSettings, IPortalAliasService portalAliasService, IHostSettingsService hostSettingsService, IPortalController portalController)
            : this(hostSettings, portalAliasService, hostSettingsService, portalController, null, null)
        {
		Logger.LogDebug("<<< Exiting AdvancedUrlRewriter()");
        }

        public void ProcessTestRequestWithContext(
            HttpContext context,
            Uri requestUri,
            bool useFriendlyUrls,
            UrlAction result,
            FriendlyUrlSettings settings)
        {
            Guid parentTraceId = Guid.Empty;
            this.settings = settings;
            this.ProcessRequest(
                context,
                requestUri,
                useFriendlyUrls,
                result,
                settings,
                false,
                parentTraceId);
        }

        internal static void RewriteAsChildAliasRoot(
            HttpContext context,
            UrlAction result,
            string aliasQueryString,
            FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering RewriteAsChildAliasRoot(context={Context},result={Result},aliasQueryString={AliasQueryString},settings={Settings})", context, result, aliasQueryString, settings);
            string culture = null;
			Logger.LogDebug("RewriteAsChildAliasRoot(context={Context},result={Result},aliasQueryString={AliasQueryString},settings={Settings}): culture → {Culture}", context, result, aliasQueryString, settings, culture);

            // look for specific alias to rewrite language parameter
            var primaryAliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(result.PortalId).ToList();
			Logger.LogDebug("RewriteAsChildAliasRoot(context={Context},result={Result},aliasQueryString={AliasQueryString},settings={Settings}): primaryAliases → {PrimaryAliases}", context, result, aliasQueryString, settings, primaryAliases);
            if (result.PortalId > -1 && result.HttpAlias != null)
            {
                culture = primaryAliases.GetCultureByPortalIdAndAlias(result.PortalId, result.HttpAlias);
            }

            if (string.IsNullOrEmpty(culture))
            {
                // 732 : when no culture returned can be "" as well as null : no culture causes no rewrite, which results in redirect to parent alias
                // set the default culture code here
                // 735 : switch to custom method for getting portal
                PortalInfo pi = CacheController.GetPortal(result.PortalId, false);
				Logger.LogDebug("RewriteAsChildAliasRoot(context={Context},result={Result},aliasQueryString={AliasQueryString},settings={Settings}): pi → {Pi}", context, result, aliasQueryString, settings, pi);
                if (pi != null)
                {
                    culture = pi.DefaultLanguage;
                }
            }

            if (!string.IsNullOrEmpty(culture))
            {
                // a culture was identified for the alias root
                if (RewriteController.AddLanguageCodeToRewritePath(ref aliasQueryString, culture))
                {
                    result.CultureCode = culture;
					Logger.LogDebug("RewriteAsChildAliasRoot(context={Context},result={Result},aliasQueryString={AliasQueryString},settings={Settings}): result.CultureCode → {Arg4}", context, result, aliasQueryString, settings, result.CultureCode);
                }

                result.DoRewrite = true;
				Logger.LogDebug("RewriteAsChildAliasRoot(context={Context},result={Result},aliasQueryString={AliasQueryString},settings={Settings}): result.DoRewrite → {Arg4}", context, result, aliasQueryString, settings, result.DoRewrite);
				Logger.LogDebug("RewriteAsChildAliasRoot(context={Context},result={Result},aliasQueryString={AliasQueryString},settings={Settings}): result.RewritePath → {Arg4}", context, result, aliasQueryString, settings, result.RewritePath);
                result.RewritePath = "~/" + Globals.glbDefaultPage + aliasQueryString;

                // the expected /default.aspx path (defaultPageUrl) matches the requested Url (/default.aspx)
                if (context != null)
                {
                    // only do if not testing
                    RewriterUtils.RewriteUrl(context, result.RewritePath);
                }
            }
			Logger.LogDebug("<<< Exiting RewriteAsChildAliasRoot(context={Context},result={Result},aliasQueryString={AliasQueryString},settings={Settings})", context, result, aliasQueryString, settings);
        }

        internal static bool CheckForChildPortalRootUrl(string requestUrl, UrlAction result, out string aliasQueryString)
        {
			Logger.LogDebug(">>> Entering CheckForChildPortalRootUrl(requestUrl={RequestUrl},result={Result})", requestUrl, result);
            bool isChildPortalRootUrl = false;

			Logger.LogDebug("CheckForChildPortalRootUrl(requestUrl={RequestUrl},result={Result}): isChildPortalRootUrl → {IsChildPortalRootUrl}", requestUrl, result, isChildPortalRootUrl);
            // what we are going to test for here is that if this is a child portal request, for the /default.aspx of the child portal
            // then we are going to avoid the core 302 redirect to ?alias=portalAlias by rewriting to the /default.aspx of the site root
            // 684 : don't convert querystring items to lower case
            // do the check by constructing what a child alias url would look like and compare it with the requested urls
            // 912 : when requested without a valid portal alias, portalAlias is null.  Refuse and return false.
            aliasQueryString = null;
            if (result.PortalAlias is { HTTPAlias: not null })
            {
                string defaultPageUrl = result.Scheme + result.PortalAlias.HTTPAlias + "/" +
                                        Globals.glbDefaultPage.ToLowerInvariant(); // child alias Url with /default.aspx

                // 660 : look for a querystring on the site root for a child portal, and handle it if so
                if (string.Equals(requestUrl.ToLowerInvariant(), defaultPageUrl, StringComparison.Ordinal))
                {
                    // exact match : that's the alias root
                    isChildPortalRootUrl = true;
                    aliasQueryString = string.Empty;
                }

                if (!isChildPortalRootUrl && requestUrl.Contains("?"))
                {
                    // is we didn't get an exact match but there is a querystring, then investigate
                    string[] requestUrlParts = requestUrl.Split('?');
                    if (requestUrlParts.GetUpperBound(0) > 0)
                    {
                        string rootPart = requestUrlParts[0];
                        string queryString = requestUrlParts[1];
                        if (string.Equals(rootPart, defaultPageUrl, StringComparison.OrdinalIgnoreCase))
                        {
                            // rewrite, but put in the querystring on the rewrite path
                            isChildPortalRootUrl = true;
                            aliasQueryString = "?" + queryString;

                            // 674: check for 301 if this value is a tabid/xx - otherwise the url will just evaluate as is
                            if (queryString.StartsWith("tabid=", StringComparison.OrdinalIgnoreCase))
                            {
                                result.Action = ActionType.CheckFor301;
								Logger.LogDebug("CheckForChildPortalRootUrl(requestUrl={RequestUrl},result={Result}): result.Action → {Arg2}", requestUrl, result, result.Action);
                            }
                        }
                    }
                }
            }

			Logger.LogDebug("<<< Exiting CheckForChildPortalRootUrl(requestUrl={RequestUrl},result={Result})", requestUrl, result);
            return isChildPortalRootUrl;
        }

        /// <summary>Make sure any redirect to the site root doesn't append the nasty /default.aspx on the end.</summary>
        /// <param name="alias">The portal alias.</param>
        /// <param name="destUrl">The destination URL.</param>
        /// <returns><paramref name="destUrl"/> without <see cref="Globals.glbDefaultPage"/> at the end.</returns>
        internal static string CheckForSiteRootRedirect(string alias, string destUrl)
        {
			Logger.LogDebug(">>> Entering CheckForSiteRootRedirect(alias={Alias},destUrl={DestUrl})", alias, destUrl);
            // 540 - don't append /default.aspx onto the end of a site root redirect.
            if (destUrl.EndsWith(alias + "/" + Globals.glbDefaultPage, StringComparison.InvariantCultureIgnoreCase))
            {
                // this is just the portal alias root + /defualt.aspx.
                // we don't want that, just the portalAliasRoot + "/"
                string aliasPlusSlash = alias + "/";

                // get everything up to the end of the portal alias
                destUrl = destUrl.Substring(0, destUrl.IndexOf(aliasPlusSlash, StringComparison.Ordinal) + aliasPlusSlash.Length);
				Logger.LogDebug("CheckForSiteRootRedirect(alias={Alias},destUrl={DestUrl}): destUrl → {DestUrl1}", alias, destUrl, destUrl);
            }

			Logger.LogDebug("<<< Exiting CheckForSiteRootRedirect(alias={Alias},destUrl={DestUrl})", alias, destUrl);
            return destUrl;
        }

        /// <inheritdoc />
        internal override void RewriteUrl(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering RewriteUrl(sender={Sender},e={E})", sender, e);
            Guid parentTraceId = Guid.Empty;
            const bool debug = true;
            bool failedInitialization = false;
			Logger.LogDebug("RewriteUrl(sender={Sender},e={E}): failedInitialization → {FailedInitialization}", sender, e, failedInitialization);
            bool ignoreForInstall = false;
			Logger.LogDebug("RewriteUrl(sender={Sender},e={E}): ignoreForInstall → {IgnoreForInstall}", sender, e, ignoreForInstall);
            var app = (HttpApplication)sender;
			Logger.LogDebug("RewriteUrl(sender={Sender},e={E}): app → {App}", sender, e, app);
            try
            {
                // 875 : completely ignore install/upgrade requests immediately
                ignoreForInstall = IgnoreRequestForInstall(app.Request);

                if (ignoreForInstall == false)
                {
                    this.settings = new FriendlyUrlSettings(-1);
					Logger.LogDebug("RewriteUrl(sender={Sender},e={E}): settings → {Settings}", sender, e, settings);

                    SecurityCheck(app);
                }
            }
            catch (Exception ex)
            {
                // exception handling for advanced Url Rewriting requests
                failedInitialization = true;
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                if (app.Context != null)
                {
                    ShowDebugData(app.Context, app.Request.Url.AbsoluteUri, null, ex);
                    var action = new UrlAction(app.Request) { Action = ActionType.Output404 };
					Logger.LogDebug("RewriteUrl(sender={Sender},e={E}): action → {Action}", sender, e, action);
                    Handle404OrException(this.settings, app.Context, ex, action, false, debug);
                }
                else
                {
                    throw;
                }
            }

            if (!failedInitialization && !ignoreForInstall)
            {
                // if made it through there and not installing, go to next call.  Not in exception catch because it implements it's own top-level exception handling
                var request = app.Context.Request;

                // 829 : change constructor to stop using physical path
                var result = new UrlAction(request)
                {
                    IsSecureConnection = request.IsSecureConnection,
                    IsSSLOffloaded = UrlUtils.IsSslOffloadEnabled(request),
                    RawUrl = request.RawUrl,
                };
                this.ProcessRequest(
                    app.Context,
                    app.Context.Request.Url,
                    Host.UseFriendlyUrls,
                    result,
                    this.settings,
                    true,
                    parentTraceId);
            }
			Logger.LogDebug("<<< Exiting RewriteUrl(sender={Sender},e={E})", sender, e);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected bool IsPortalAliasIncorrect(
            HttpContext context,
            HttpRequest request,
            Uri requestUri,
            UrlAction result,
            NameValueCollection queryStringCol,
            FriendlyUrlSettings settings,
            Guid parentTraceId,
            out string httpAlias)
        {
            // now check to make sure it's the primary portal alias for this portal/language/browser
			Logger.LogDebug(">>> Entering IsPortalAliasIncorrect(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId})", context, request, requestUri, result, queryStringCol, settings, parentTraceId);
            bool incorrectAlias = false;
			Logger.LogDebug("IsPortalAliasIncorrect(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): incorrectAlias → {IncorrectAlias}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, incorrectAlias);
            httpAlias = null;

			Logger.LogDebug("IsPortalAliasIncorrect(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): httpAlias → {HttpAlias}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, httpAlias);
            // if (result.RedirectAllowed && result.PortalId > -1)
            if (result.PortalId > -1)
            {
                // portal has been identified
                var portalAliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(result.PortalId).ToList();
				Logger.LogDebug("IsPortalAliasIncorrect(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): portalAliases → {PortalAliases}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, portalAliases);

                // if we're not on the primary alias, and portalaliasmapping is set to redirect, we might need to be redirected
                var redirectToPrimary = !result.PortalAlias.IsPrimary && result.PortalAliasMapping == PortalSettings.PortalAliasMapping.Redirect;

                // forceAlias used in querystring?
                var forceAliasInQueryString = queryStringCol != null && queryStringCol["forceAlias"] != null && queryStringCol["forceAlias"] != "true";
				Logger.LogDebug("IsPortalAliasIncorrect(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): forceAliasInQueryString → {ForceAliasInQueryString}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, forceAliasInQueryString);
                if (redirectToPrimary || forceAliasInQueryString)
                {
                    if (portalAliases.Count > 0)
                    {
                        string checkAlias = result.HttpAlias;
						Logger.LogDebug("IsPortalAliasIncorrect(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): checkAlias → {CheckAlias}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, checkAlias);
                        bool continueLoop = true;
						Logger.LogDebug("IsPortalAliasIncorrect(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): continueLoop → {ContinueLoop}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, continueLoop);
                        bool triedWWW = false;
						Logger.LogDebug("IsPortalAliasIncorrect(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): triedWWW → {TriedWWW}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, triedWWW);
                        while (httpAlias == null && continueLoop)
                        {
                            if (portalAliases.ContainsAlias(result.PortalId, checkAlias))
                            {
                                if (portalAliases.Count > 0)
                                {
                                    // var cpa = portalAliases.GetAliasByPortalIdAndSettings(result);
                                    string url = requestUri.ToString();
									Logger.LogDebug("IsPortalAliasIncorrect(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): url → {Url}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, url);
                                    RewriteController.CheckLanguageMatch(ref url, result);
                                    var cpa = portalAliases
                                        .Where(a => a.IsPrimary || result.PortalAliasMapping != PortalSettings.PortalAliasMapping.Redirect)
                                        .GetAliasByPortalIdAndSettings(result.PortalId, result, result.CultureCode, result.BrowserType);

                                    if (cpa != null)
                                    {
                                        httpAlias = cpa.HTTPAlias;
                                        continueLoop = false;
                                    }

                                    if (string.IsNullOrEmpty(result.CultureCode) && cpa == null)
                                    {
                                        // if there is a specific culture for this portal alias, then check that
                                        string culture = portalAliases.GetCultureByPortalIdAndAlias(result.PortalId, result.HttpAlias);
										Logger.LogDebug("IsPortalAliasIncorrect(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): culture → {Culture}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, culture);

                                        // if this matches the alias of the request, then we know we have the correct alias because it is a specific culture
                                        if (!string.IsNullOrEmpty(culture))
                                        {
                                            continueLoop = false;
                                        }
                                    }
                                }
                            }

                            // check whether to still go on or not
                            if (continueLoop)
                            {
                                // this alias doesn't exist in the list
                                // check if it has a www on it - if not, try adding, if it does, try removing
                                if (!triedWWW)
                                {
                                    triedWWW = true; // now tried adding/removing www
                                    if (checkAlias.StartsWith("www.", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        checkAlias = checkAlias.Substring(4);
                                    }
                                    else
                                    {
                                        checkAlias = "www." + checkAlias;
                                    }
                                }
                                else
                                {
                                    // last thing to try, get the default language and see if there is a portal alias for that
                                    // thus, any aliases not identified as belonging to a language are redirected back to the
                                    // alias named for the default language
                                    continueLoop = false;

                                    // 735 : switch to custom method for getting portal
                                    PortalInfo pi = CacheController.GetPortal(result.PortalId, false);
									Logger.LogDebug("IsPortalAliasIncorrect(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): pi → {Pi}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, pi);
                                    if (pi != null)
                                    {
                                        string cultureCode = pi.DefaultLanguage;
                                        if (!string.IsNullOrEmpty(cultureCode))
                                        {
                                            var primaryPortalAlias = portalAliases.GetAliasByPortalIdAndSettings(result.PortalId, result, cultureCode, settings);
											Logger.LogDebug("IsPortalAliasIncorrect(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): primaryPortalAlias → {PrimaryPortalAlias}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, primaryPortalAlias);
                                            if (primaryPortalAlias != null)
                                            {
                                                httpAlias = primaryPortalAlias.HTTPAlias;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // check to see if it is a custom tab alias - in that case, it is allowed to be requested for the tab
                    if (CheckIfAliasIsCustomTabAlias(ref result, httpAlias, settings))
                    {
                        // change the primary alias to the custom tab alias that has been requested.
                        result.PrimaryAlias = result.PortalAlias;
						Logger.LogDebug("IsPortalAliasIncorrect(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): result.PrimaryAlias → {Arg7}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, result.PrimaryAlias);
                    }
                    else
                        if (httpAlias != null && !string.Equals(httpAlias, result.HttpAlias, StringComparison.OrdinalIgnoreCase))
                    {
                        incorrectAlias = true;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting IsPortalAliasIncorrect(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId})", context, request, requestUri, result, queryStringCol, settings, parentTraceId);
            return incorrectAlias;
        }

        private static void ShowDebugData(HttpContext context, string requestUri, UrlAction result, Exception ex)
        {
			Logger.LogDebug(">>> Entering ShowDebugData(context={Context},requestUri={RequestUri},result={Result},ex={Ex})", context, requestUri, result, ex);
            if (context != null)
            {
                HttpResponse response = context.Response;
				Logger.LogDebug("ShowDebugData(context={Context},requestUri={RequestUri},result={Result},ex={Ex}): response → {Response}", context, requestUri, result, ex, response);

                // handle null responses wherever they might be found - this routine must be tolerant to all kinds of invalid inputs
                if (requestUri == null)
                {
                    requestUri = "null Uri";
					Logger.LogDebug("ShowDebugData(context={Context},requestUri={RequestUri},result={Result},ex={Ex}): requestUri → {RequestUri1}", context, requestUri, result, ex, requestUri);
                }

                string finalUrl = "null final Url";
				Logger.LogDebug("ShowDebugData(context={Context},requestUri={RequestUri},result={Result},ex={Ex}): finalUrl → {FinalUrl}", context, requestUri, result, ex, finalUrl);
                string rewritePath = "null rewrite path";
				Logger.LogDebug("ShowDebugData(context={Context},requestUri={RequestUri},result={Result},ex={Ex}): rewritePath → {RewritePath}", context, requestUri, result, ex, rewritePath);
                string action = "null action";
				Logger.LogDebug("ShowDebugData(context={Context},requestUri={RequestUri},result={Result},ex={Ex}): action → {Action}", context, requestUri, result, ex, action);
                if (result != null)
                {
                    finalUrl = result.FinalUrl;
                    action = result.Action.ToString();
                    rewritePath = result.RewritePath;
                }

                // format up the error message to show
                const string debugMsg = "{0}, {1}, {2}, {3}, {4}, {5}, {6}";
                string productVer = DotNetNukeContext.Current.Application.Version.ToString();
                string portalSettings = string.Empty;
				Logger.LogDebug("ShowDebugData(context={Context},requestUri={RequestUri},result={Result},ex={Ex}): portalSettings → {PortalSettings}", context, requestUri, result, ex, portalSettings);
                string browser = "Unknown";
				Logger.LogDebug("ShowDebugData(context={Context},requestUri={RequestUri},result={Result},ex={Ex}): browser → {Browser}", context, requestUri, result, ex, browser);

                // 949 : don't rely on 'result' being non-null
                if (result != null)
                {
                    browser = result.BrowserType.ToString();
                }

                if (context.Items.Contains("PortalSettings"))
                {
                    var ps = (PortalSettings)context.Items["PortalSettings"];
					Logger.LogDebug("ShowDebugData(context={Context},requestUri={RequestUri},result={Result},ex={Ex}): ps → {Ps}", context, requestUri, result, ex, ps);
                    if (ps != null)
                    {
                        portalSettings = ps.PortalId.ToString(CultureInfo.InvariantCulture);
                        if (ps.PortalAlias != null)
                        {
                            portalSettings += ":" + ps.PortalAlias.HTTPAlias;
                        }
                    }
                }

                response.AppendHeader(
                    "X-" + ProductName + "-Debug",
                    string.Format(
                        CultureInfo.InvariantCulture,
                        debugMsg,
                        requestUri,
                        finalUrl,
                        rewritePath,
                        action,
                        productVer,
                        portalSettings,
                        browser));
                int msgNum = 1;
                if (result != null)
                {
                    foreach (string msg in result.DebugMessages)
                    {
                        response.AppendHeader("X-" + ProductName + "-Debug-" + msgNum.ToString("00", CultureInfo.InvariantCulture), msg);
                        msgNum++;
                    }
                }

                if (ex != null)
                {
                    response.AppendHeader("X-" + ProductName + "-Ex", ex.Message);
                }
            }
			Logger.LogDebug("<<< Exiting ShowDebugData(context={Context},requestUri={RequestUri},result={Result},ex={Ex})", context, requestUri, result, ex);
        }

        private static void Handle404OrException(FriendlyUrlSettings settings, HttpContext context, Exception ex, UrlAction result, bool transfer, bool showDebug)
        {
			Logger.LogDebug(">>> Entering Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug})", settings, context, ex, result, transfer, showDebug);
            // handle Auto-Add Alias
            if (result.Action == ActionType.Output404 && CanAutoAddPortalAlias())
            {
                // Need to determine if this is a real 404 or a possible new alias.
                var portalId = Host.HostPortalID;
                if (portalId > Null.NullInteger)
                {
                    if (string.IsNullOrEmpty(result.DomainName))
                    {
                        result.DomainName = Globals.GetDomainName(context.Request); // parse the domain name out of the request
                    }

                    // Get all the existing aliases
                    var aliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId).ToList();
					Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): aliases → {Aliases}", settings, context, ex, result, transfer, showDebug, aliases);

                    bool autoaddAlias;
                    bool isPrimary = false;
					Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): isPrimary → {IsPrimary}", settings, context, ex, result, transfer, showDebug, isPrimary);
                    if (aliases.Count == 0)
                    {
                        autoaddAlias = true;
                        isPrimary = true;
                    }
                    else
                    {
                        autoaddAlias = true;
                        foreach (var alias in aliases)
                        {
                            if (result.DomainName.StartsWith(alias.HTTPAlias, StringComparison.OrdinalIgnoreCase)
                                    && result.DomainName.Length >= alias.HTTPAlias.Length)
                            {
                                autoaddAlias = false;
                                break;
                            }
                        }
                    }

                    if (autoaddAlias)
                    {
                        var portalAliasInfo = new PortalAliasInfo
                        {
                            PortalID = portalId,
                            HTTPAlias = result.DomainName,
                            IsPrimary = isPrimary,
                        };
                        PortalAliasController.Instance.AddPortalAlias(portalAliasInfo);

                        context.Response.Redirect(context.Request.Url.ToString(), true);
                    }
                }
            }

            if (context != null)
            {
                HttpRequest request = context.Request;
                HttpResponse response = context.Response;
				Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): response → {Response}", settings, context, ex, result, transfer, showDebug, response);
                HttpServerUtility server = context.Server;

                const string errorPageHtmlHeader = @"<html><head><title>{0}</title></head><body>";
                const string errorPageHtmlFooter = @"</body></html>";
                var errorPageHtml = new StringWriter();
				Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): errorPageHtml → {ErrorPageHtml}", settings, context, ex, result, transfer, showDebug, errorPageHtml);
                CustomErrorsSection ceSection = null;
				Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): ceSection → {CeSection}", settings, context, ex, result, transfer, showDebug, ceSection);

                // 876 : security catch for custom error reading
                try
                {
                    ceSection = (CustomErrorsSection)WebConfigurationManager.GetSection("system.web/customErrors");
                }

                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {
					Logger.LogError("Exception in Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): {Arg6}", settings, context, ex, result, transfer, showDebug, Exception.Message);
                    // on some medium trust environments, this will throw an exception for trying to read the custom Errors
                    // do nothing
                }

                /* 454 new 404/500 error handling routine */
                bool useDNNTab = false;
				Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): useDNNTab → {UseDNNTab}", settings, context, ex, result, transfer, showDebug, useDNNTab);
                int errTabId = -1;
				Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): errTabId → {ErrTabId}", settings, context, ex, result, transfer, showDebug, errTabId);
                string errUrl = null;
				Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): errUrl → {ErrUrl}", settings, context, ex, result, transfer, showDebug, errUrl);
                string status = string.Empty;
				Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): status → {Status}", settings, context, ex, result, transfer, showDebug, status);
                bool isPostback = false;
				Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): isPostback → {IsPostback}", settings, context, ex, result, transfer, showDebug, isPostback);
                if (settings != null)
                {
                    if (request.RequestType == "POST")
                    {
                        isPostback = true;
                    }

                    if (result != null && ex != null)
                    {
                        result.DebugMessages.Add("Exception: " + ex.Message);
                        result.DebugMessages.Add("Stack Trace: " + ex.StackTrace);
                        if (ex.InnerException != null)
                        {
                            result.DebugMessages.Add("Inner Ex : " + ex.InnerException.Message);
                            result.DebugMessages.Add("Stack Trace: " + ex.InnerException.StackTrace);
                        }
                        else
                        {
                            result.DebugMessages.Add("Inner Ex : null");
                        }
                    }

                    string errRH;
                    string errRV;
                    int statusCode;
                    if (result != null && result.Action != ActionType.Output404)
                    {
                        // output everything but 404 (usually 500)
                        if (settings.TabId500 > -1)
                        {
                            // tabid specified for 500 error page, use that
                            useDNNTab = true;
                            errTabId = settings.TabId500;
                        }

                        errUrl = settings.Url500;
                        errRH = "X-UrlRewriter-500";
                        errRV = "500 Rewritten to {0} : {1}";
                        statusCode = 500;
                        status = "500 Internal Server Error";
                    }
                    else
                    {
                        // output 404 error
                        // if the tabid is specified for a 404 page, then use that
                        if (settings.TabId404 > -1)
                        {
                            useDNNTab = true;
                            errTabId = settings.TabId404;
                        }

                        // with 404 errors, there's an option to catch certain urls and use an external url for extra processing.
                        if (!string.IsNullOrEmpty(settings.Regex404))
                        {
                            try
                            {
                                // 944 : check the original Url in case the requested Url has been rewritten before discovering it's a 404 error
                                string requestedUrl = request.Url.ToString();
								Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): requestedUrl → {RequestedUrl}", settings, context, ex, result, transfer, showDebug, requestedUrl);
                                if (result != null && !string.IsNullOrEmpty(result.OriginalPath))
                                {
                                    requestedUrl = result.OriginalPath;
                                }

                                var regex404 = RegexUtils.GetCachedRegex(settings.Regex404, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
								Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): regex404 → {Regex404}", settings, context, ex, result, transfer, showDebug, regex404);
                                if (regex404.IsMatch(requestedUrl))
                                {
                                    useDNNTab = false;

                                    // if we have a match in the 404 regex value, then don't use the tabid
                                }
                            }
                            catch (Exception regexEx)
                            {
								Logger.LogError("Exception in Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): {Arg6}", settings, context, ex, result, transfer, showDebug, regexEx.getMessage());
                                // .some type of exception : output in response header, and go back to using the tabid
                                response.AppendHeader("X-UrlRewriter-404Exception", regexEx.Message);
                            }
                        }

                        errUrl = settings.Url404;
                        errRH = "X-UrlRewriter-404";
                        errRV = "404 Rewritten to {0} : {1} : Reason {2}";
                        status = "404 Not Found";
                        statusCode = 404;
                    }

                    // check for 404 logging
                    if (result == null || result.Action == ActionType.Output404)
                    {
                        // Log 404 errors to Event Log
                        UrlRewriterUtils.Log404(request, settings, result);
                    }

                    // 912 : use unhandled 404 switch
                    string reason404 = null;
					Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): reason404 → {Reason404}", settings, context, ex, result, transfer, showDebug, reason404);
                    bool unhandled404 = true;
					Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): unhandled404 → {Unhandled404}", settings, context, ex, result, transfer, showDebug, unhandled404);
                    if (useDNNTab && errTabId > -1)
                    {
                        unhandled404 = false; // we're handling it here
                        TabInfo errTab = TabController.Instance.GetTab(errTabId, result.PortalId, true);
						Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): errTab → {ErrTab}", settings, context, ex, result, transfer, showDebug, errTab);
                        if (errTab != null)
                        {
                            bool redirect = false;
							Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): redirect → {Redirect}", settings, context, ex, result, transfer, showDebug, redirect);

                            // ok, valid tabid.  what we're going to do is to load up this tab via a rewrite of the url, and then change the output status
                            string reason = "Not Found";
							Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): reason → {Reason}", settings, context, ex, result, transfer, showDebug, reason);
                            if (result != null)
                            {
                                reason = result.Reason.ToString();
                            }

                            response.AppendHeader(
                                errRH,
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    errRV,
                                    "DNN Tab",
                                    $"{errTab.TabName}(Tabid:{errTabId})",
                                    reason));

                            // show debug messages even if in debug mode
                            if (context != null && response != null && result != null && showDebug)
                            {
                                ShowDebugData(context, result.OriginalPath, result, null);
                            }

                            if (!isPostback)
                            {
                                response.ClearContent();
                                response.StatusCode = statusCode;
								Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): response.StatusCode → {Arg6}", settings, context, ex, result, transfer, showDebug, response.StatusCode);
								Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): response.Status → {Arg6}", settings, context, ex, result, transfer, showDebug, response.Status);
                                response.Status = status;
                            }
                            else
                            {
                                redirect = true;

                                // redirect postbacks as you can't postback successfully to a server.transfer
                            }

                            errUrl = Globals.glbDefaultPage + TabIndexController.CreateRewritePath(errTab.TabID, string.Empty);

                            // have to update the portal settings with the new tabid
                            PortalSettings ps = null;
							Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): ps → {Ps}", settings, context, ex, result, transfer, showDebug, ps);
                            if (context != null && context.Items != null)
                            {
                                if (context.Items.Contains("PortalSettings"))
                                {
                                    ps = (PortalSettings)context.Items["PortalSettings"];
                                    context.Items.Remove("PortalSettings"); // nix it from the context
                                }
                            }

                            if (ps != null && ps.PortalAlias != null)
                            {
                                ps = new PortalSettings(errTabId, ps.PortalAlias);
                            }
                            else
                            {
                                if (result.HttpAlias != null && result.PortalId > -1)
                                {
                                    PortalAliasInfo pa = PortalAliasController.Instance.GetPortalAlias(result.HttpAlias, result.PortalId);
									Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): pa → {Pa}", settings, context, ex, result, transfer, showDebug, pa);
                                    ps = new PortalSettings(errTabId, pa);
                                }
                                else
                                {
                                    // 912 : handle 404 when no valid portal can be identified
                                    // results when iis is configured to handle portal alias, but
                                    // DNN isn't.  This always returns 404 because a multi-portal site
                                    // can't just show the 404 page of the host site.
                                    ArrayList portals = Portals.PortalController.Instance.GetPortals();
									Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): portals → {Portals}", settings, context, ex, result, transfer, showDebug, portals);
                                    if (portals != null && portals.Count == 1)
                                    {
                                        // single portal install, load up portal settings for this portal
                                        var singlePortal = (PortalInfo)portals[0];
										Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): singlePortal → {SinglePortal}", settings, context, ex, result, transfer, showDebug, singlePortal);

                                        // list of aliases from database
                                        var aliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(singlePortal.PortalID).ToList();

                                        // list of aliases from Advanced Url settings
                                        List<string> chosen = aliases.GetAliasesForPortalId(singlePortal.PortalID);
										Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): chosen → {Chosen}", settings, context, ex, result, transfer, showDebug, chosen);
                                        PortalAliasInfo useFor404 = null;
										Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): useFor404 → {UseFor404}", settings, context, ex, result, transfer, showDebug, useFor404);

                                        // go through all aliases and either get the first valid one, or the first
                                        // as chosen in the advanced url management settings
                                        foreach (var pa in aliases)
                                        {
                                            if (useFor404 == null)
                                            {
                                                useFor404 = pa; // first one by default
                                            }

                                            // matching?
                                            if (chosen != null && chosen.Count > 0)
                                            {
                                                if (chosen.Contains(pa.HTTPAlias))
                                                {
                                                    useFor404 = pa;
                                                }
                                            }
                                            else
                                            {
                                                break; // no further checking
                                            }
                                        }

                                        // now configure that as the portal settings
                                        if (useFor404 != null)
                                        {
                                            // create portal settings context for identified portal alias in single portal install
                                            ps = new PortalSettings(errTabId, useFor404);
                                        }
                                    }
                                    else
                                    {
                                        reason404 = "Requested domain name is not configured as valid website";
                                        unhandled404 = true;
                                    }
                                }
                            }

                            if (ps != null)
                            {
                                // re-add the context items portal settings back in
                                context.Items.Add("PortalSettings", ps);
                            }

                            if (redirect)
                            {
                                errUrl = TestableGlobals.Instance.NavigateURL();
                                response.Redirect(errUrl, true); // redirect and end response.

                                // It will mean the user will have to postback again, but it will work the second time
                            }
                            else
                            {
                                if (transfer)
                                {
                                    // execute a server transfer to the default.aspx?tabid=xx url
                                    // 767 : object not set error on extensionless 404 errors
                                    if (context.User == null)
                                    {
                                        context.User = GetCurrentPrincipal(context);
                                    }

                                    response.TrySkipIisCustomErrors = true;
									Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): response.TrySkipIisCustomErrors → {Arg6}", settings, context, ex, result, transfer, showDebug, response.TrySkipIisCustomErrors);

                                    // 881 : spoof the basePage object so that the client dependency framework
                                    // is satisfied it's working with a page-based handler
                                    IHttpHandler spoofPage = new CDefault();
									Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): spoofPage → {SpoofPage}", settings, context, ex, result, transfer, showDebug, spoofPage);
									Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): context.Handler → {Arg6}", settings, context, ex, result, transfer, showDebug, context.Handler);
                                    context.Handler = spoofPage;
                                    server.Transfer("~/" + errUrl, true);
                                }
                                else
                                {
                                    context.RewritePath("~/Default.aspx", false);
                                    response.TrySkipIisCustomErrors = true;
                                    response.Status = "404 Not Found";
                                    response.StatusCode = 404;
                                }
                            }
                        }
                    }

                    // 912 : change to new if statement to handle cases where the TabId404 couldn't be handled correctly
                    if (unhandled404)
                    {
                        // process the error on the external Url by rewriting to the external url
                        if (!string.IsNullOrEmpty(errUrl))
                        {
                            response.ClearContent();
                            response.TrySkipIisCustomErrors = true;
                            string reason = "Not Found";
                            if (result != null)
                            {
                                reason = result.Reason.ToString();
                            }

                            response.AppendHeader(errRH, string.Format(CultureInfo.InvariantCulture, errRV, "Url", errUrl, reason));
                            if (reason404 != null)
                            {
                                response.AppendHeader("X-Url-Master-404-Data", reason404);
                            }

                            response.StatusCode = statusCode;
                            response.Status = status;
                            server.Transfer("~/" + errUrl, true);
                        }
                        else
                        {
                            errorPageHtml.Write(status + "<br>The requested Url does not return any valid content.");
                            if (reason404 != null)
                            {
                                errorPageHtml.Write(status + "<br>" + reason404);
                            }

                            errorPageHtml.Write("<div style='font-weight:bolder'>Administrators</div>");
							Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): style → {Style}", settings, context, ex, result, transfer, showDebug, style);
                            errorPageHtml.Write("<div>Change this message by configuring a specific 404 Error Page or Url for this website.</div>");

                            // output a reason for the 404
                            string reason = string.Empty;
                            if (result != null)
                            {
                                reason = result.Reason.ToString();
                            }

                            if (!string.IsNullOrEmpty(errRH) && !string.IsNullOrEmpty(reason))
                            {
                                response.AppendHeader(errRH, reason);
                            }

                            response.StatusCode = statusCode;
                            response.Status = status;
                        }
                    }
                }
                else
                {
                    // fallback output if not valid settings
                    if (result != null && result.Action == ActionType.Output404)
                    {
                        // don't restate the requested Url to prevent cross site scripting
                        errorPageHtml.Write("404 Not Found<br>The requested Url does not return any valid content.");
                        response.StatusCode = 404;
                        response.Status = "404 Not Found";
                    }
                    else
                    {
                        // error, especially if invalid result object
                        errorPageHtml.Write("500 Server Error<br><div style='font-weight:bolder'>An error occured during processing : if possible, check the event log of the server</div>");
                        response.StatusCode = 500;
                        response.Status = "500 Internal Server Error";
                        if (result != null)
                        {
                            result.Action = ActionType.Output500;
							Logger.LogDebug("Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug}): result.Action → {Arg6}", settings, context, ex, result, transfer, showDebug, result.Action);
                        }
                    }
                }

                if (ex != null)
                {
                    if (context != null)
                    {
                        if (context.Items.Contains("UrlRewrite:Exception") == false)
                        {
                            context.Items.Add("UrlRewrite:Exception", ex.Message);
                            context.Items.Add("UrlRewrite:StackTrace", ex.StackTrace);
                        }
                    }

                    if (ceSection is { Mode: CustomErrorsMode.Off, })
                    {
                        errorPageHtml.Write(errorPageHtmlHeader);
                        errorPageHtml.Write($"<div style='font-weight:bolder'>Exception:</div><div>{ex.Message}</div>");
                        errorPageHtml.Write($"<div style='font-weight:bolder'>Stack Trace:</div><div>{ex.StackTrace}</div>");
                        errorPageHtml.Write("<div style='font-weight:bolder'>Administrators</div>");
                        errorPageHtml.Write("<div>You can see this exception because the customErrors attribute in the web.config is set to 'off'.  Change this value to 'on' or 'RemoteOnly' to show Error Handling</div>");
                        try
                        {
                            if (errUrl != null && errUrl.StartsWith("~", StringComparison.Ordinal))
                            {
                                errUrl = VirtualPathUtility.ToAbsolute(errUrl);
                            }
                        }
                        finally
                        {
                            if (errUrl != null)
                            {
                                errorPageHtml.Write($"<div>The error handling would have shown this page : <a href='{errUrl}'>{errUrl}</a></div>");
                            }
                            else
                            {
                                errorPageHtml.Write("<div>The error handling could not determine the correct page to show.</div>");
                            }
                        }
                    }
                }

                string errorPageHtmlBody = errorPageHtml.ToString();
                if (errorPageHtmlBody.Length > 0)
                {
                    response.Write(errorPageHtmlHeader);
                    response.Write(errorPageHtmlBody);
                    response.Write(errorPageHtmlFooter);
                }

                if (ex != null)
                {
                    UrlRewriterUtils.LogExceptionInRequest(ex, status, result);
                }
            }
			Logger.LogDebug("<<< Exiting Handle404OrException(settings={Settings},context={Context},ex={Ex},result={Result},transfer={Transfer},showDebug={ShowDebug})", settings, context, ex, result, transfer, showDebug);
        }

        private static GenericPrincipal GetCurrentPrincipal(HttpContext context)
        {
			Logger.LogDebug(">>> Entering GetCurrentPrincipal(context={Context})", context);
            // Extract the forms authentication cookie
            var authCookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];
            var currentPrincipal = new GenericPrincipal(new GenericIdentity(string.Empty), []);
			Logger.LogDebug("GetCurrentPrincipal(context={Context}): currentPrincipal → {CurrentPrincipal}", context, currentPrincipal);

            try
            {
                if (authCookie != null)
                {
                    var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
					Logger.LogDebug("GetCurrentPrincipal(context={Context}): authTicket → {AuthTicket}", context, authTicket);
                    if (authTicket is { Expired: false, })
                    {
                        var roles = authTicket.UserData.Split('|');
						Logger.LogDebug("GetCurrentPrincipal(context={Context}): roles → {Roles}", context, roles);
                        var id = new FormsIdentity(authTicket);
						Logger.LogDebug("GetCurrentPrincipal(context={Context}): id → {Id}", context, id);
                        currentPrincipal = new GenericPrincipal(id, roles);
                    }
                }
            }
            catch (Exception)
            {
				Logger.LogError("Exception in GetCurrentPrincipal(context={Context}): {Arg1}", context, Exception.Message);
                // do nothing here.
            }

			Logger.LogDebug("<<< Exiting GetCurrentPrincipal(context={Context})", context);
            return currentPrincipal;
        }

        private static bool CheckForDebug(HttpRequest request, NameValueCollection queryStringCol, bool debugEnabled)
        {
			Logger.LogDebug(">>> Entering CheckForDebug(request={Request},queryStringCol={QueryStringCol},debugEnabled={DebugEnabled})", request, queryStringCol, debugEnabled);
            string debugValue = string.Empty;
            bool retVal = false;
			Logger.LogDebug("CheckForDebug(request={Request},queryStringCol={QueryStringCol},debugEnabled={DebugEnabled}): retVal → {RetVal}", request, queryStringCol, debugEnabled, retVal);
			Logger.LogDebug("CheckForDebug(request={Request},queryStringCol={QueryStringCol},debugEnabled={DebugEnabled}): debugValue → {DebugValue}", request, queryStringCol, debugEnabled, debugValue);

            if (debugEnabled)
            {
                const string debugToken = "_aumdebug";
                if (queryStringCol?[debugToken] != null)
                {
                    debugValue = queryStringCol[debugToken];
                }
                else
                {
                    if (request != null)
                    {
                        debugValue = request.Params.Get("HTTP_" + debugToken.ToUpperInvariant());
                    }

                    if (debugValue == null)
                    {
                        debugValue = "false";
                    }
                }
            }

            switch (debugValue.ToLowerInvariant())
            {
                case "true":
                    retVal = true;
                    break;
            }

			Logger.LogDebug("<<< Exiting CheckForDebug(request={Request},queryStringCol={QueryStringCol},debugEnabled={DebugEnabled})", request, queryStringCol, debugEnabled);
            return retVal;
        }

        private static bool CheckForTabExternalForwardOrRedirect(
            HttpContext context,
            ref UrlAction result,
            HttpResponse response,
            FriendlyUrlSettings settings,
            Guid parentTraceId)
        {
			Logger.LogDebug(">>> Entering CheckForTabExternalForwardOrRedirect(context={Context},result={Result},response={Response},settings={Settings},parentTraceId={ParentTraceId})", context, result, response, settings, parentTraceId);
            bool finished = false;
			Logger.LogDebug("CheckForTabExternalForwardOrRedirect(context={Context},result={Result},response={Response},settings={Settings},parentTraceId={ParentTraceId}): finished → {Finished}", context, result, response, settings, parentTraceId, finished);
            HttpRequest request = null;
			Logger.LogDebug("CheckForTabExternalForwardOrRedirect(context={Context},result={Result},response={Response},settings={Settings},parentTraceId={ParentTraceId}): request → {Request}", context, result, response, settings, parentTraceId, request);
            if (context != null)
            {
                request = context.Request;
            }

            try
            {
                // check for external forwarding or a permanent redirect request
                // 592 : check for permanent redirect (823 : moved location from 'checkForRedirects')
                if (result.TabId > -1 && result.PortalId > -1 &&
                    (settings.ForwardExternalUrlsType != DNNPageForwardType.NoForward ||
                     result.Reason == RedirectReason.Tab_Permanent_Redirect))
                {
                    bool allowRedirect = !(result.RewritePath != null && result.RewritePath.ToLowerInvariant().Contains("&ctl=tab"));
					Logger.LogDebug("CheckForTabExternalForwardOrRedirect(context={Context},result={Result},response={Response},settings={Settings},parentTraceId={ParentTraceId}): allowRedirect → {AllowRedirect}", context, result, response, settings, parentTraceId, allowRedirect);

                    // 594 : do not redirect settings pages for external urls
                    if (allowRedirect)
                    {
                        TabInfo tab;
                        allowRedirect = CheckFor301RedirectExclusion(result.TabId, result.PortalId, false, out tab, settings);
                        if (allowRedirect)
                        {
                            // 772 : not redirecting file type Urls when requested.
                            bool permanentRedirect = false;
                            string redirectUrl = null;
							Logger.LogDebug("CheckForTabExternalForwardOrRedirect(context={Context},result={Result},response={Response},settings={Settings},parentTraceId={ParentTraceId}): redirectUrl → {RedirectUrl}", context, result, response, settings, parentTraceId, redirectUrl);
                            string cleanPath = null;
							Logger.LogDebug("CheckForTabExternalForwardOrRedirect(context={Context},result={Result},response={Response},settings={Settings},parentTraceId={ParentTraceId}): cleanPath → {CleanPath}", context, result, response, settings, parentTraceId, cleanPath);
                            bool doRedirect = false;
							Logger.LogDebug("CheckForTabExternalForwardOrRedirect(context={Context},result={Result},response={Response},settings={Settings},parentTraceId={ParentTraceId}): doRedirect → {DoRedirect}", context, result, response, settings, parentTraceId, doRedirect);
                            switch (tab.TabType)
                            {
                                case TabType.File:
                                    // have to fudge in a portal settings object for this to work - shortcoming of LinkClick URl generation
                                    var portalSettings = new PortalSettings(result.TabId, result.PortalAlias);
									Logger.LogDebug("CheckForTabExternalForwardOrRedirect(context={Context},result={Result},response={Response},settings={Settings},parentTraceId={ParentTraceId}): portalSettings → {PortalSettings}", context, result, response, settings, parentTraceId, portalSettings);
                                    if (context != null)
                                    {
                                        context.Items.Add("PortalSettings", portalSettings);
                                        result.Reason = RedirectReason.File_Url;
										Logger.LogDebug("CheckForTabExternalForwardOrRedirect(context={Context},result={Result},response={Response},settings={Settings},parentTraceId={ParentTraceId}): result.Reason → {Arg5}", context, result, response, settings, parentTraceId, result.Reason);
                                        string fileUrl = Globals.LinkClick(tab.Url, tab.TabID, -1);
                                        context.Items.Remove("PortalSettings");

                                        // take back out again, because it will be done further downstream
                                        // do a check to make sure we're not repeating the Url again, because the tabid is set, but we don't want to touch
                                        // a linkclick url
                                        if (!result.OriginalPathNoAlias.EndsWith(HttpUtility.UrlDecode(fileUrl), true, CultureInfo.InvariantCulture))
                                        {
                                            redirectUrl = fileUrl;
                                        }
                                    }

                                    if (redirectUrl != null)
                                    {
                                        doRedirect = true;
                                    }

                                    break;
                                case TabType.Url:
                                    result.Reason = RedirectReason.Tab_External_Url;
                                    redirectUrl = tab.Url;
                                    if (redirectUrl != null)
                                    {
                                        doRedirect = true;
                                        if (tab.PermanentRedirect)
                                        {
                                            result.Action = ActionType.Redirect301;
											Logger.LogDebug("CheckForTabExternalForwardOrRedirect(context={Context},result={Result},response={Response},settings={Settings},parentTraceId={ParentTraceId}): result.Action → {Arg5}", context, result, response, settings, parentTraceId, result.Action);
                                        }
                                        else
                                        {
                                            result.Action = ActionType.Redirect302;
                                        }
                                    }

                                    break;
                                case TabType.Tab:
                                    // get the redirect path of the specific tab, as long as we have a valid request to work from
                                    if (request != null)
                                    {
                                        // get the rewrite or requested path in a clean format, suitable for input to the friendly url provider
                                        cleanPath = RewriteController.GetRewriteOrRequestedPath(result, request.Url);

                                        // 727 prevent redirectLoop with do301 in querystring
                                        if (result.Action == ActionType.Redirect301 ||
                                            result.Action == ActionType.Redirect302)
                                        {
                                            cleanPath = RedirectTokens.RemoveAnyRedirectTokens(
                                                cleanPath,
                                                request.QueryString);
                                        }

                                        // get the redirect Url from the friendly url provider using the tab, path and settings
                                        redirectUrl = RedirectController.GetTabRedirectUrl(
                                            tab,
                                            settings,
                                            cleanPath,
                                            result,
                                            out permanentRedirect,
                                            parentTraceId);
                                    }

                                    // check to make sure there isn't a blank redirect Url
                                    if (redirectUrl == null)
                                    {
                                        // problem : no redirect Url to redirect to
                                        // solution : cancel the redirect
                                        string message = "Permanent Redirect chosen for Tab " +
                                                         tab.TabPath.Replace("//", "/") +
                                                         " but forwarding Url was not valid";
                                        RedirectController.CancelRedirect(ref result, context, settings, message);
                                    }
                                    else
                                    {
                                        // if there was a redirect Url, set the redirect action and set the type of redirect going to use
                                        doRedirect = true;
                                        if (permanentRedirect)
                                        {
                                            result.Action = ActionType.Redirect301;
                                            result.Reason = RedirectReason.Tab_Permanent_Redirect;
                                        }
                                        else
                                        {
                                            // not a permanent redirect, check if the page forwarding is set
                                            result.Action = ActionType.Redirect302;
                                            result.Reason = RedirectReason.Tab_Temporary_Redirect;
                                        }

                                        // should be already set, anyway
                                        result.RewritePath = cleanPath;
										Logger.LogDebug("CheckForTabExternalForwardOrRedirect(context={Context},result={Result},response={Response},settings={Settings},parentTraceId={ParentTraceId}): result.RewritePath → {Arg5}", context, result, response, settings, parentTraceId, result.RewritePath);
                                    }

                                    break;
                                default:
                                    // only concern here is if permanent redirect is requested, but there is no external url specified
                                    if (result.Reason == RedirectReason.Tab_Permanent_Redirect)
                                    {
                                        bool permRedirect = tab.PermanentRedirect;
                                        if (permRedirect)
                                        {
                                            // problem : permanent redirect marked, but no forwarding url supplied
                                            // solution : cancel redirect
                                            string message = "Permanent Redirect chosen for Tab " +
                                                             tab.TabPath.Replace("//", "/") +
                                                             " but no forwarding Url Supplied";
                                            RedirectController.CancelRedirect(ref result, context, settings, message);
                                        }
                                    }

                                    break;
                            }

                            // do the redirect we have specified
                            if (doRedirect &&
                                (result.Action == ActionType.Redirect301 || result.Action == ActionType.Redirect302))
                            {
                                result.FinalUrl = redirectUrl;
								Logger.LogDebug("CheckForTabExternalForwardOrRedirect(context={Context},result={Result},response={Response},settings={Settings},parentTraceId={ParentTraceId}): result.FinalUrl → {Arg5}", context, result, response, settings, parentTraceId, result.FinalUrl);
                                if (result.Action == ActionType.Redirect301)
                                {
                                    if (response != null)
                                    {
                                        // perform a 301 redirect to the external url of the tab
                                        response.AppendHeader(
                                            "X-Redirect-Reason",
                                            result.Reason.ToString().Replace("_", " ") + " Requested");
                                        response.RedirectPermanent(result.FinalUrl);
                                    }
                                }
                                else
                                {
                                    if (result.Action == ActionType.Redirect302)
                                    {
                                        if (response != null)
                                        {
                                            // perform a 301 redirect to the external url of the tab
                                            response.AppendHeader(
                                                "X-Redirect-Reason",
                                                result.Reason.ToString().Replace("_", " ") + " Requested");
                                            response.Redirect(result.FinalUrl);
                                        }
                                    }
                                }

                                finished = true;
                            }
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
				Logger.LogError("Exception in CheckForTabExternalForwardOrRedirect(context={Context},result={Result},response={Response},settings={Settings},parentTraceId={ParentTraceId}): {Arg5}", context, result, response, settings, parentTraceId, ThreadAbortException.getMessage());
                // do nothing, a threadAbortException will have occured from using a server.transfer or response.redirect within the code block.  This is the highest
                // level try/catch block, so we handle it here.
            }

			Logger.LogDebug("<<< Exiting CheckForTabExternalForwardOrRedirect(context={Context},result={Result},response={Response},settings={Settings},parentTraceId={ParentTraceId})", context, result, response, settings, parentTraceId);
            return finished;
        }

        /// <summary>Redirects an alias if that is allowed by the settings.</summary>
        /// <param name="httpAlias">The HTTP alias.</param>
        /// <param name="result">The URL action to update.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <returns><see langword="true"/> if the <paramref name="result"/> is a redirect, otherwise <see langword="false"/>.</returns>
        private static bool RedirectPortalAlias(string httpAlias, ref UrlAction result, FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering RedirectPortalAlias(httpAlias={HttpAlias},result={Result},settings={Settings})", httpAlias, result, settings);
            bool redirected = false;

			Logger.LogDebug("RedirectPortalAlias(httpAlias={HttpAlias},result={Result},settings={Settings}): redirected → {Redirected}", httpAlias, result, settings, redirected);
            // redirect to primary alias
            if (result.PortalAliasMapping == PortalSettings.PortalAliasMapping.Redirect && result.RedirectAllowed)
            {
                if (result.Reason == RedirectReason.Wrong_Portal_Alias_For_Browser_Type || result.Reason == RedirectReason.Wrong_Portal_Alias_For_Culture ||
                    result.Reason == RedirectReason.Wrong_Portal_Alias_For_Culture_And_Browser)
                {
                    redirected = ConfigurePortalAliasRedirect(ref result, result.HttpAlias, httpAlias, false, result.Reason, settings.InternalAliasList, settings);
                }
                else
                {
                    redirected = ConfigurePortalAliasRedirect(ref result, result.HttpAlias, httpAlias, false, settings.InternalAliasList, settings);
                }
            }

			Logger.LogDebug("<<< Exiting RedirectPortalAlias(httpAlias={HttpAlias},result={Result},settings={Settings})", httpAlias, result, settings);
            return redirected;
        }

        private static bool ConfigurePortalAliasRedirect(
            ref UrlAction result,
            string wrongAlias,
            string rightAlias,
            bool ignoreCustomAliasTabs,
            List<InternalAlias> internalAliases,
            FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering ConfigurePortalAliasRedirect(result={Result},wrongAlias={WrongAlias},rightAlias={RightAlias},ignoreCustomAliasTabs={IgnoreCustomAliasTabs},internalAliases={InternalAliases},settings={Settings})", result, wrongAlias, rightAlias, ignoreCustomAliasTabs, internalAliases, settings);
				Logger.LogDebug("<<< Exiting ConfigurePortalAliasRedirect(result={Result},wrongAlias={WrongAlias},rightAlias={RightAlias},ignoreCustomAliasTabs={IgnoreCustomAliasTabs},internalAliases={InternalAliases},settings={Settings})", result, wrongAlias, rightAlias, ignoreCustomAliasTabs, internalAliases, settings);
            return ConfigurePortalAliasRedirect(
                ref result,
                wrongAlias,
                rightAlias,
                ignoreCustomAliasTabs,
                RedirectReason.Wrong_Portal_Alias,
                internalAliases,
                settings);
				Logger.LogDebug(">>> Entering ConfigurePortalAliasRedirect(result={Result},wrongAlias={WrongAlias},rightAlias={RightAlias},ignoreCustomAliasTabs={IgnoreCustomAliasTabs},redirectReason={RedirectReason},internalAliases={InternalAliases},settings={Settings})", result, wrongAlias, rightAlias, ignoreCustomAliasTabs, redirectReason, internalAliases, settings);
				Logger.LogDebug("ConfigurePortalAliasRedirect(result={Result},wrongAlias={WrongAlias},rightAlias={RightAlias},ignoreCustomAliasTabs={IgnoreCustomAliasTabs},redirectReason={RedirectReason},internalAliases={InternalAliases},settings={Settings}): result.Action → {Arg7}", result, wrongAlias, rightAlias, ignoreCustomAliasTabs, redirectReason, internalAliases, settings, result.Action);
				Logger.LogDebug("ConfigurePortalAliasRedirect(result={Result},wrongAlias={WrongAlias},rightAlias={RightAlias},ignoreCustomAliasTabs={IgnoreCustomAliasTabs},redirectReason={RedirectReason},internalAliases={InternalAliases},settings={Settings}): result.Reason → {Arg7}", result, wrongAlias, rightAlias, ignoreCustomAliasTabs, redirectReason, internalAliases, settings, result.Reason);
				Logger.LogDebug("ConfigurePortalAliasRedirect(result={Result},wrongAlias={WrongAlias},rightAlias={RightAlias},ignoreCustomAliasTabs={IgnoreCustomAliasTabs},redirectReason={RedirectReason},internalAliases={InternalAliases},settings={Settings}): result.FinalUrl → {Arg7}", result, wrongAlias, rightAlias, ignoreCustomAliasTabs, redirectReason, internalAliases, settings, result.FinalUrl);
				Logger.LogDebug("<<< Exiting ConfigurePortalAliasRedirect(result={Result},wrongAlias={WrongAlias},rightAlias={RightAlias},ignoreCustomAliasTabs={IgnoreCustomAliasTabs},redirectReason={RedirectReason},internalAliases={InternalAliases},settings={Settings})", result, wrongAlias, rightAlias, ignoreCustomAliasTabs, redirectReason, internalAliases, settings);
        }

        /// <summary>Checks to see whether the specified alias is a customTabAlias.</summary>
        /// <param name="result">The URL action to update.</param>
        /// <param name="httpAlias">The HTTP alias.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <returns><see langword="true"/> if the alias is a custom tab alias, otherwise <see langword="false"/>.</returns>
        private static bool CheckIfAliasIsCustomTabAlias(ref UrlAction result, string httpAlias, FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering CheckIfAliasIsCustomTabAlias(result={Result},httpAlias={HttpAlias},settings={Settings})", result, httpAlias, settings);
            List<string> customAliasesForTabs = TabIndexController.GetCustomPortalAliases(settings);
            bool isACustomTabAlias = false;
			Logger.LogDebug("CheckIfAliasIsCustomTabAlias(result={Result},httpAlias={HttpAlias},settings={Settings}): isACustomTabAlias → {IsACustomTabAlias}", result, httpAlias, settings, isACustomTabAlias);
			Logger.LogDebug("CheckIfAliasIsCustomTabAlias(result={Result},httpAlias={HttpAlias},settings={Settings}): customAliasesForTabs → {CustomAliasesForTabs}", result, httpAlias, settings, customAliasesForTabs);
            if (customAliasesForTabs != null && customAliasesForTabs.Count > 0)
            {
                // remove any customAliases that are also primary aliases.
                foreach (var cpa in PortalAliasController.Instance.GetPortalAliasesByPortalId(result.PortalId))
                {
                    if (cpa.IsPrimary == true && customAliasesForTabs.Contains(cpa.HTTPAlias))
                    {
                        customAliasesForTabs.Remove(cpa.HTTPAlias);
                    }
                }

                isACustomTabAlias = customAliasesForTabs.Contains(httpAlias.ToLowerInvariant());
            }

			Logger.LogDebug("<<< Exiting CheckIfAliasIsCustomTabAlias(result={Result},httpAlias={HttpAlias},settings={Settings})", result, httpAlias, settings);
            return isACustomTabAlias;
        }

        /// <summary>Checks to see whether the specified alias is a customTabAlias for the TabId in result.</summary>
        /// <param name="result">The URL action to update.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <returns><see langword="true"/> if the current alias is a custom tab alias, otherwise <see langword="false"/>.</returns>
        private static bool CheckIfAliasIsCurrentTabCustomTabAlias(ref UrlAction result, FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering CheckIfAliasIsCurrentTabCustomTabAlias(result={Result},settings={Settings})", result, settings);
            var customAliasesForTab = TabController.Instance.GetCustomAliases(result.TabId, result.PortalId);
            bool isCurrentTabCustomTabAlias = false;
			Logger.LogDebug("CheckIfAliasIsCurrentTabCustomTabAlias(result={Result},settings={Settings}): isCurrentTabCustomTabAlias → {IsCurrentTabCustomTabAlias}", result, settings, isCurrentTabCustomTabAlias);
			Logger.LogDebug("CheckIfAliasIsCurrentTabCustomTabAlias(result={Result},settings={Settings}): customAliasesForTab → {CustomAliasesForTab}", result, settings, customAliasesForTab);
            if (customAliasesForTab is { Count: > 0 })
            {
                // see if we have a customAlias for the current CultureCode
                if (customAliasesForTab.TryGetValue(result.CultureCode, out var alias))
                {
                    // if it is for the current culture, we need to know if it's a primary alias
                    var tabPortalAlias = PortalAliasController.Instance.GetPortalAlias(alias);
					Logger.LogDebug("CheckIfAliasIsCurrentTabCustomTabAlias(result={Result},settings={Settings}): tabPortalAlias → {TabPortalAlias}", result, settings, tabPortalAlias);
                    if (tabPortalAlias is { IsPrimary: false })
                    {
                        // it's not a primary alias, so must be a custom tab alias
                        isCurrentTabCustomTabAlias = true;
                    }
                }
            }

            // if it's not a custom alias for the current tab, we'll need to change the result
            if (!isCurrentTabCustomTabAlias)
            {
                result.Action = ActionType.Redirect301;
				Logger.LogDebug("CheckIfAliasIsCurrentTabCustomTabAlias(result={Result},settings={Settings}): result.Action → {Arg2}", result, settings, result.Action);
				Logger.LogDebug("CheckIfAliasIsCurrentTabCustomTabAlias(result={Result},settings={Settings}): result.Reason → {Arg2}", result, settings, result.Reason);
                result.Reason = RedirectReason.Wrong_Portal_Alias;
            }

			Logger.LogDebug("<<< Exiting CheckIfAliasIsCurrentTabCustomTabAlias(result={Result},settings={Settings})", result, settings);
            return isCurrentTabCustomTabAlias;
        }

        /// <summary>Configures the result object to set the correct Alias redirect parameters and destination URL.</summary>
        /// <param name="result">The URL action to update.</param>
        /// <param name="wrongAlias">The alias to redirect from.</param>
        /// <param name="rightAlias">The alias to redirect to.</param>
        /// <param name="ignoreCustomAliasTabs">Whether to ignore custom aliases for the page.</param>
        /// <param name="redirectReason">The redirect reason.</param>
        /// <param name="internalAliases">A list of internal aliases.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <returns><see langword="true"/> if the <paramref name="result"/> is a redirect, otherwise <see langword="false"/>.</returns>
        private static bool ConfigurePortalAliasRedirect(
            ref UrlAction result,
            string wrongAlias,
            string rightAlias,
            bool ignoreCustomAliasTabs,
            RedirectReason redirectReason,
            List<InternalAlias> internalAliases,
            FriendlyUrlSettings settings)
        {
            // wrong alias for the portal
            // check to see if the wrong portal alias could be a custom alias for a tab
            bool doRedirect;
            if (ignoreCustomAliasTabs == false)
            {
                // check out custom alias tabs collection
                // if an alias is a custom tab alias for a specific tab, then don't redirect
                // if we have the TabId, we'll need to check if the alias is valid for the current tab
                if (result.TabId > 0 && CheckIfAliasIsCurrentTabCustomTabAlias(ref result, settings))
                {
                    doRedirect = false;
                }
                else if (result.TabId < 0 && CheckIfAliasIsCustomTabAlias(ref result, wrongAlias, settings))
                {
                    doRedirect = false;
                }
                else
                {
                    doRedirect = true;
                }
            }
            else
            {
                doRedirect = true; // do redirect, ignore custom alias entries for tabs
            }

            // check to see if it is an internal alias.  These are used to block redirects
            // to allow for reverse proxy requests, which must change the rewritten alias
            // while leaving the requested alias
            bool internalAliasFound = false;
			Logger.LogDebug("ConfigurePortalAliasRedirect(result={Result},wrongAlias={WrongAlias},rightAlias={RightAlias},ignoreCustomAliasTabs={IgnoreCustomAliasTabs},redirectReason={RedirectReason},internalAliases={InternalAliases},settings={Settings}): internalAliasFound → {InternalAliasFound}", result, wrongAlias, rightAlias, ignoreCustomAliasTabs, redirectReason, internalAliases, settings, internalAliasFound);
            if (doRedirect && internalAliases is { Count: > 0 })
            {
                if (internalAliases.Any(ia => string.Equals(ia.HttpAlias, wrongAlias, StringComparison.OrdinalIgnoreCase)))
                {
                    internalAliasFound = true;
                    doRedirect = false;
                }
            }

            // if still need to do redirect, then set the settings that will cause the redirect (redirect not done here)
            if (doRedirect)
            {
                result.Action = ActionType.Redirect301;
                result.Reason = redirectReason;
                var destUrl = result.OriginalPath;
				Logger.LogDebug("ConfigurePortalAliasRedirect(result={Result},wrongAlias={WrongAlias},rightAlias={RightAlias},ignoreCustomAliasTabs={IgnoreCustomAliasTabs},redirectReason={RedirectReason},internalAliases={InternalAliases},settings={Settings}): destUrl → {DestUrl}", result, wrongAlias, rightAlias, ignoreCustomAliasTabs, redirectReason, internalAliases, settings, destUrl);
                if (result.OriginalPath.Contains(wrongAlias))
                {
                    destUrl = result.OriginalPath.Replace(wrongAlias, rightAlias);
                }
                else if (result.OriginalPath.ToLowerInvariant().Contains(wrongAlias))
                {
                    destUrl = result.OriginalPath.ToLowerInvariant().Replace(wrongAlias, rightAlias);
                }

                if (redirectReason == RedirectReason.Wrong_Portal_Alias_For_Culture ||
                    redirectReason == RedirectReason.Wrong_Portal_Alias_For_Culture_And_Browser)
                {
                    destUrl = destUrl.Replace("/language/" + result.CultureCode, string.Empty);
                }

                destUrl = CheckForSiteRootRedirect(rightAlias, destUrl);
                result.FinalUrl = destUrl;
            }
            else
            {
                // 838 : don't overwrite the reason if already have checkfor301
                // and don't do a check on the basis that an internal alias was found
                if (result.Action != ActionType.CheckFor301 && internalAliasFound == false)
                {
                    // set status to 'check for redirect'
                    result.Action = ActionType.CheckFor301;
                    result.Reason = RedirectReason.Custom_Tab_Alias;
                }
            }

            return doRedirect;
        }

        private static string MakeUrlWithAlias(Uri requestUri, string httpAlias)
        {
			Logger.LogDebug(">>> Entering MakeUrlWithAlias(requestUri={RequestUri},httpAlias={HttpAlias})", requestUri, httpAlias);
			Logger.LogDebug("<<< Exiting MakeUrlWithAlias(requestUri={RequestUri},httpAlias={HttpAlias})", requestUri, httpAlias);
            return requestUri.AbsoluteUri.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)
                             ? "https://" + httpAlias.Replace("*.", string.Empty) + "/"
			Logger.LogDebug("<<< Exiting MakeUrlWithAlias(requestUri={RequestUri},alias={Alias})", requestUri, alias);
                             : "http://" + httpAlias.Replace("*.", string.Empty) + "/";
        }

        private static string MakeUrlWithAlias(Uri requestUri, PortalAliasInfo alias)
        {
            return MakeUrlWithAlias(requestUri, alias.HTTPAlias);
        }

        /// <summary>Determines if this is a request from an install / upgrade url.</summary>
        /// <param name="physicalPath">The physical path of the request.</param>
        /// <param name="refererPath">The referrer path for the request.</param>
        /// <param name="requestedDomain">The requested domain.</param>
        /// <param name="refererDomain">The domain of the referrer.</param>
        /// <returns><see langword="true"/> if the request is for an install URL, otherwise <see langword="false"/>.</returns>
        /// <remarks>
        /// //875 : cater for the upgradewizard.aspx Url that is new to DNN 6.1.
        /// </remarks>
        private static bool IgnoreRequestForInstall(string physicalPath, string refererPath, string requestedDomain, string refererDomain)
        {
			Logger.LogDebug(">>> Entering IgnoreRequestForInstall(physicalPath={PhysicalPath},refererPath={RefererPath},requestedDomain={RequestedDomain},refererDomain={RefererDomain})", physicalPath, refererPath, requestedDomain, refererDomain);
            if (physicalPath.EndsWith("install.aspx", true, CultureInfo.InvariantCulture)
                || physicalPath.EndsWith("installwizard.aspx", true, CultureInfo.InvariantCulture)
                || physicalPath.EndsWith("upgradewizard.aspx", true, CultureInfo.InvariantCulture)
                || Globals.Status == Globals.UpgradeStatus.Install
                || Globals.Status == Globals.UpgradeStatus.Upgrade)
            {
                return true;
            }

            // 954 : DNN 7.0 compatibility
            // check for /default.aspx which is default Url launched from the Upgrade/Install wizard page
            // 961 : check domain as well as path for the referer
            if (!physicalPath.EndsWith(Globals.glbDefaultPage, true, CultureInfo.InvariantCulture)
                && refererPath != null
                && string.Equals(requestedDomain, refererDomain, StringComparison.OrdinalIgnoreCase)
                && (refererPath.EndsWith("install.aspx", true, CultureInfo.InvariantCulture)
                    || refererPath.EndsWith("installwizard.aspx", true, CultureInfo.InvariantCulture)
                    || refererPath.EndsWith("upgradewizard.aspx", true, CultureInfo.InvariantCulture)))
            {
			Logger.LogDebug("<<< Exiting IgnoreRequestForInstall(physicalPath={PhysicalPath},refererPath={RefererPath},requestedDomain={RequestedDomain},refererDomain={RefererDomain})", physicalPath, refererPath, requestedDomain, refererDomain);
                return true;
            }

			Logger.LogDebug("<<< Exiting IgnoreRequestForInstall(request={Request})", request);
            return false;
        }

        private static bool IgnoreRequestForWebServer(IPortalAliasService portalAliasService, string requestedPath)
        {
			Logger.LogDebug(">>> Entering IgnoreRequestForWebServer(portalAliasService={PortalAliasService},requestedPath={RequestedPath})", portalAliasService, requestedPath);
            // Should standardize comparison methods
            if (requestedPath.IndexOf("synchronizecache.aspx", StringComparison.OrdinalIgnoreCase) > 1
                || requestedPath.EndsWith("keepalive.aspx", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Get the root
            var rootPath = requestedPath.Substring(0, requestedPath.LastIndexOf("/", StringComparison.Ordinal));
			Logger.LogDebug("IgnoreRequestForWebServer(portalAliasService={PortalAliasService},requestedPath={RequestedPath}): rootPath → {RootPath}", portalAliasService, requestedPath, rootPath);
            rootPath = rootPath.Substring(rootPath.IndexOf("://", StringComparison.Ordinal) + 3);

            // Check if this is a WebServer and not a portalalias.
            // if can auto add portal alias enabled, then return false, alias will add later.
            var alias = portalAliasService.GetPortalAlias(rootPath);
			Logger.LogDebug("IgnoreRequestForWebServer(portalAliasService={PortalAliasService},requestedPath={RequestedPath}): alias → {Alias}", portalAliasService, requestedPath, alias);
            if (alias != null || CanAutoAddPortalAlias())
            {
                return false;
            }

            // Check if this is a WebServer
            var server = ServerController.GetEnabledServers().SingleOrDefault(s => s.Url == rootPath);
			Logger.LogDebug("IgnoreRequestForWebServer(portalAliasService={PortalAliasService},requestedPath={RequestedPath}): server → {Server}", portalAliasService, requestedPath, server);
            if (server != null)
            {
			Logger.LogDebug("<<< Exiting IgnoreRequestForWebServer(portalAliasService={PortalAliasService},requestedPath={RequestedPath})", portalAliasService, requestedPath);
                return true;
            }

            return false;
        }

        private static bool IgnoreRequestForInstall(HttpRequest request)
        {
            try
            {
                string physicalPath = request.PhysicalPath;
                string requestedDomain = request.Url.Host;
                string refererPath = null, refererDomain = null;
                if (request.UrlReferrer != null)
                {
                    refererDomain = request.UrlReferrer.Host;
                    refererPath = request.UrlReferrer.LocalPath;
                }

                return IgnoreRequestForInstall(physicalPath, refererPath, requestedDomain, refererDomain);
            }
            catch (PathTooLongException)
            {
                // catch and handle this exception, caused by an excessively long file path based on the
                // mapped virtual url
                return false;
            }
            catch (ArgumentException)
            {
                // catch and handle this exception, caused by an invalid character in the file path based on the
                // mapped virtual url
                return false;
            }
            catch (UriFormatException)
            {
                // catch and handle this exception, caused by an invalid hostname in the referrer
                return false;
            }
        }

        private static bool IgnoreRequest(IPortalAliasService portalAliasService, UrlAction result, string requestedPath, string ignoreRegexPattern, HttpRequest request)
        {
			Logger.LogDebug(">>> Entering IgnoreRequest(portalAliasService={PortalAliasService},result={Result},requestedPath={RequestedPath},ignoreRegexPattern={IgnoreRegexPattern},request={Request})", portalAliasService, result, requestedPath, ignoreRegexPattern, request);
            bool retVal = false;

			Logger.LogDebug("IgnoreRequest(portalAliasService={PortalAliasService},result={Result},requestedPath={RequestedPath},ignoreRegexPattern={IgnoreRegexPattern},request={Request}): retVal → {RetVal}", portalAliasService, result, requestedPath, ignoreRegexPattern, request, retVal);
            // check if we are upgrading/installing
            // 829 : use result physical path instead of request physical path
            // 875 : cater for the upgradewizard.aspx Url that is new to DNN 6.1
            if (request != null && (IgnoreRequestForInstall(request) || IgnoreRequestForWebServer(portalAliasService, requestedPath)))
            {
                // ignore all install requests
                retVal = true;
            }
            else if (request != null && request.Path.EndsWith("imagechallenge.captcha.aspx", StringComparison.InvariantCultureIgnoreCase))
            {
                retVal = true;
            }
            else
            {
                try
                {
                    if (ignoreRegexPattern.Length > 0)
                    {
                        var ignoreRegex = RegexUtils.GetCachedRegex(ignoreRegexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
						Logger.LogDebug("IgnoreRequest(portalAliasService={PortalAliasService},result={Result},requestedPath={RequestedPath},ignoreRegexPattern={IgnoreRegexPattern},request={Request}): ignoreRegex → {IgnoreRegex}", portalAliasService, result, requestedPath, ignoreRegexPattern, request, ignoreRegex);
                        if (ignoreRegex.IsMatch(requestedPath))
                        {
                            retVal = true;
                        }
                    }
                }
                catch (Exception ex)
                {
					Logger.LogError("Exception in IgnoreRequest(portalAliasService={PortalAliasService},result={Result},requestedPath={RequestedPath},ignoreRegexPattern={IgnoreRegexPattern},request={Request}): {Arg5}", portalAliasService, result, requestedPath, ignoreRegexPattern, request, ex.getMessage());
                    UrlRewriterUtils.LogExceptionInRequest(ex, "Not Set", result);
                    result.Ex = ex;
					Logger.LogDebug("IgnoreRequest(portalAliasService={PortalAliasService},result={Result},requestedPath={RequestedPath},ignoreRegexPattern={IgnoreRegexPattern},request={Request}): result.Ex → {Arg5}", portalAliasService, result, requestedPath, ignoreRegexPattern, request, result.Ex);
                }
            }

			Logger.LogDebug("<<< Exiting IgnoreRequest(portalAliasService={PortalAliasService},result={Result},requestedPath={RequestedPath},ignoreRegexPattern={IgnoreRegexPattern},request={Request})", portalAliasService, result, requestedPath, ignoreRegexPattern, request);
            return retVal;
        }

        private static void CheckForRewrite(
            IHostSettings hostSettings,
            IPortalController portalController,
            IApplicationStatusInfo appStatus,
            IPortalGroupController portalGroupController,
            string fullUrl,
            string querystring,
            UrlAction result,
            bool useFriendlyUrls,
            NameValueCollection queryStringCol,
            FriendlyUrlSettings settings,
            out bool isPhysicalResource,
            Guid parentTraceId)
        {
			Logger.LogDebug(">>> Entering CheckForRewrite(hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},fullUrl={FullUrl},querystring={Querystring},result={Result},useFriendlyUrls={UseFriendlyUrls},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId})", hostSettings, portalController, appStatus, portalGroupController, fullUrl, querystring, result, useFriendlyUrls, queryStringCol, settings, parentTraceId);
            bool checkForRewrites;

            // just check to make sure it isn't a physical resource on the server
            RewriteController.IdentifyByPhysicalResource(
                result.PhysicalPath,
                fullUrl,
                queryStringCol,
                ref result,
                useFriendlyUrls,
                settings,
                out isPhysicalResource,
                out checkForRewrites,
                parentTraceId);

            if (checkForRewrites && RewriteController.CanRewriteRequest(result, fullUrl, settings))
            {
                bool doSiteUrlProcessing = false;
				Logger.LogDebug("CheckForRewrite(hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},fullUrl={FullUrl},querystring={Querystring},result={Result},useFriendlyUrls={UseFriendlyUrls},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): doSiteUrlProcessing → {DoSiteUrlProcessing}", hostSettings, portalController, appStatus, portalGroupController, fullUrl, querystring, result, useFriendlyUrls, queryStringCol, settings, parentTraceId, doSiteUrlProcessing);

                // 728 new regex expression to pass values straight onto the siteurls.config file
                if (!string.IsNullOrEmpty(settings.UseSiteUrlsRegex))
                {
                    var siteUrlsRegex = RegexUtils.GetCachedRegex(settings.UseSiteUrlsRegex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
					Logger.LogDebug("CheckForRewrite(hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},fullUrl={FullUrl},querystring={Querystring},result={Result},useFriendlyUrls={UseFriendlyUrls},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): siteUrlsRegex → {SiteUrlsRegex}", hostSettings, portalController, appStatus, portalGroupController, fullUrl, querystring, result, useFriendlyUrls, queryStringCol, settings, parentTraceId, siteUrlsRegex);
                    doSiteUrlProcessing = siteUrlsRegex.IsMatch(fullUrl);
                }

                // if a virtual request, and not starting with the siteUrls.config file, go on to find the rewritten path
                if (!doSiteUrlProcessing)
                {
                    // looks up the page index to find the correct Url
                    bool doRewrite = RewriteController.IdentifyByTabPathEx(hostSettings, portalController, appStatus, portalGroupController, fullUrl, querystring, result, queryStringCol, settings, parentTraceId);
                    if (!doRewrite)
                    {
                        doSiteUrlProcessing = true;
                    }
                }

                if (doSiteUrlProcessing)
                {
                    // 728 : compare requests against the siteurls.config file, either if no other match was found, or if we want to skip the rest of the processing
                    // the standard DNN way of rewriting, using expressions found in the siteurls.config file
                    RewriteController.IdentifyByRegEx(fullUrl, querystring, result.ApplicationPath, ref result, settings, parentTraceId);
                }
            }
			Logger.LogDebug("<<< Exiting CheckForRewrite(hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController},fullUrl={FullUrl},querystring={Querystring},result={Result},useFriendlyUrls={UseFriendlyUrls},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId})", hostSettings, portalController, appStatus, portalGroupController, fullUrl, querystring, result, useFriendlyUrls, queryStringCol, settings, parentTraceId);
        }

        private static bool CheckForRedirects(
            Uri requestUri,
            string fullUrl,
            NameValueCollection queryStringCol,
            UrlAction result,
            string requestType,
            FriendlyUrlSettings settings,
            int portalHomeTabId)
        {
			Logger.LogDebug(">>> Entering CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId})", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId);
            bool redirected = false;
			Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): redirected → {Redirected}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, redirected);
            if (queryStringCol["error"] == null && queryStringCol["message"] == null && requestType != "POST")
            {
                // if the / is missing from an extension-less request, then check for a 301 redirect
                if (settings.PageExtensionUsageType == PageExtensionUsageType.Never)
                {
                    // 575 check on absolutePath instead of absoluteUri : this ignores query strings and fragments like #
                    // 610 don't always end with '/' - reverses previous setting
                    // 687 don't double-check 301 redirects.  'CheckFor301' is less concise than 'Redirect301'
                    // DNN-21906: if the redirect is for splash page, then we should continue the 302 redirect.
                    if (requestUri.AbsolutePath.EndsWith("/", StringComparison.Ordinal) && result.Action != ActionType.Redirect301 && result.Reason != RedirectReason.Requested_SplashPage)
                    {
                        result.Action = ActionType.CheckFor301;
						Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): result.Action → {Arg7}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, result.Action);
                    }
                }

                if (settings.RedirectWrongCase && result.Action == ActionType.Continue)
                {
                    result.Action = ActionType.CheckFor301;
                }

                string scheme = requestUri.Scheme + Uri.SchemeDelimiter;
                bool queryStringHas301Parm = queryStringCol["do301"] != null;

                // 727 : keep a bool value if there is a do301 request in the querystring
                // check for a 301 request in the query string, or an explicit 301 or 302 request
                // 2.0 - check for explicit do301=true instead of just do301 key
                string do301Val = queryStringCol["do301"];
                if (result.TabId > -1
                    && (result.Action == ActionType.Redirect301
                        || (do301Val != null && do301Val == "true")
                        || result.Action == ActionType.Redirect302))
                {
                    // valid tab, specific 301 redirect, rewrite hint for specific 301 redirect, or specific 302 redirect
                    // we have ordered a 301 redirect earlier in the code
                    // get the url for redirection by re-submitting the path into the Friendly Url Provider
                    string pathOnly = RewriteController.GetRewriteOrRequestedPath(result, requestUri);
					Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): pathOnly → {PathOnly}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, pathOnly);

                    // 727 prevent redirectLoop with do301 in querystring
                    if (result.Action == ActionType.Redirect301 || queryStringHas301Parm || result.Action == ActionType.Redirect302)
                    {
                        pathOnly = RedirectTokens.RemoveAnyRedirectTokens(pathOnly, queryStringCol);
                    }

                    // check for exclusion by regex for this url
                    if (result.RedirectAllowed)
                    {
                        // get the tab so we know where to go
                        TabInfo tab;
                        bool checkRedirect = CheckFor301RedirectExclusion(result.TabId, result.PortalId, true, out tab, settings);

                        if (checkRedirect)
                        {
                            if ((result.Reason == RedirectReason.Deleted_Page || result.Reason == RedirectReason.Disabled_Page)
                                && portalHomeTabId > 0
                                && settings.DeletedTabHandlingType == DeletedTabHandlingType.Do301RedirectToPortalHome)
                            {
                                // redirecting to home page
                                TabInfo homeTab = TabController.Instance.GetTab(portalHomeTabId, result.PortalId, false);
								Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): homeTab → {HomeTab}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, homeTab);
                                if (homeTab != null)
                                {
                                    string homePageUrl = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(
                                        homeTab,
                                        pathOnly,
                                        Globals.glbDefaultPage,
                                        result.HttpAlias,
                                        false,
                                        settings,
                                        Guid.Empty);
                                    result.Action = ActionType.Redirect301;
                                    result.FinalUrl = homePageUrl;
									Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): result.FinalUrl → {Arg7}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, result.FinalUrl);
									Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): result.RewritePath → {Arg7}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, result.RewritePath);
                                    result.RewritePath = pathOnly;
                                    redirected = true;
                                }
                            }
                            else
                            {
                                // get the rewrite or requested path in a clean format, suitable for input to the friendly url provider
                                string cleanPath = RewriteController.GetRewriteOrRequestedPath(result, requestUri);
								Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): cleanPath → {CleanPath}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, cleanPath);

                                // 727 prevent redirectLoop with do301 in querystring
                                // also check for existing in path of do301 token
                                if (result.Action == ActionType.Redirect301 || do301Val != null || result.Action == ActionType.Redirect302)
                                {
                                    cleanPath = RedirectTokens.RemoveAnyRedirectTokens(cleanPath, queryStringCol);
                                }

                                // get best friendly url from friendly url provider
                                string bestFriendlyUrl = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(
                                    tab,
                                    cleanPath,
                                    Globals.glbDefaultPage,
                                    result.HttpAlias,
                                    false,
                                    settings,
                                    Guid.Empty);

                                // get what the friendly Url for this tab should be and stick it in as the redirect
                                // 727 : using boolean because we wanted to get rid of the do301 before calculating the correct url
                                if (queryStringHas301Parm)
                                {
                                    result.Action = ActionType.Redirect301;
                                    if (result.Reason == RedirectReason.Not_Redirected)
                                    {
                                        result.Reason = RedirectReason.Unfriendly_Url_1;
										Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): result.Reason → {Arg7}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, result.Reason);
                                    }
                                }

                                result.FinalUrl = bestFriendlyUrl;
                                result.RewritePath = pathOnly;
                                redirected = true; // mark as redirected
                            }
                        }
                        else
                        {
                            // redirect disallowed
                            // 618: dont' clear if 302 redirect selected
                            if (result.Action != ActionType.Redirect302Now || result.Action != ActionType.Redirect302)
                            {
                                RedirectController.CancelRedirect(ref result, null, settings, "Redirect requested but cancelled because disallowed");
                            }
                        }
                    }
                }
                else if (result.TabId > -1 && result.RedirectAllowed && result.Action == ActionType.CheckFor301)
                {
                    // 301 check was requested in earlier processing
                    // get the tab controller and retrieve the tab the request is for
                    // don't redirect unless allowed, the tab is valid, and it's not an admin or super tab
                    if (settings.RedirectUnfriendly)
                    {
                        TabInfo tab;
                        bool allowRedirect = CheckFor301RedirectExclusion(result.TabId, result.PortalId, true, out tab, settings);
						Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): allowRedirect → {AllowRedirect}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, allowRedirect);
                        if (allowRedirect && tab != null)
                        {
                            // remove the http alias from the url. Do this by putting the url back together from the request and removing the alias
                            string rewritePathOnly;
                            if (result.DoRewrite)
                            {
                                rewritePathOnly = result.RewritePath;
                                var pos = rewritePathOnly.IndexOf("default.aspx", StringComparison.OrdinalIgnoreCase);
								Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): pos → {Pos}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, pos);
								Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): rewritePathOnly → {RewritePathOnly}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, rewritePathOnly);
                                if (pos > Null.NullInteger)
                                {
                                    rewritePathOnly = rewritePathOnly.Substring(pos);
                                }
                            }
                            else
                            {
                                rewritePathOnly = requestUri.Host + requestUri.PathAndQuery;
                            }

                            // remove the http alias from the path
                            var pathAliasEnd = rewritePathOnly.IndexOf(result.PortalAlias.HTTPAlias, StringComparison.InvariantCultureIgnoreCase);
							Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): pathAliasEnd → {PathAliasEnd}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, pathAliasEnd);
                            var queryStringIndex = rewritePathOnly.IndexOf("?", StringComparison.InvariantCultureIgnoreCase);
							Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): queryStringIndex → {QueryStringIndex}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, queryStringIndex);
                            if (pathAliasEnd > Null.NullInteger && (queryStringIndex == Null.NullInteger || pathAliasEnd < queryStringIndex))
                            {
                                rewritePathOnly = rewritePathOnly.Substring(pathAliasEnd + result.PortalAlias.HTTPAlias.Length);
                            }

                            // now check to see if need to remove /default.aspx from the end of the requested Url
                            string requestedUrl = fullUrl;
							Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): requestedUrl → {RequestedUrl}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, requestedUrl);
                            int requestedUrlAliasEnd = requestedUrl.IndexOf(result.PortalAlias.HTTPAlias, StringComparison.InvariantCultureIgnoreCase)
                                                        + (result.PortalAlias.HTTPAlias + "/").Length;
                            if (requestedUrlAliasEnd > Null.NullInteger)
                            {
                                // 818 : when a site root is used for a custom page Url, then check for max length within bounds
                                if ((requestedUrl.Length - requestedUrlAliasEnd) >= 12 && requestedUrl.Substring(requestedUrlAliasEnd).Equals("default.aspx", StringComparison.OrdinalIgnoreCase))
                                {
                                    requestedUrl = requestedUrl.Substring(0, requestedUrl.Length - 12);

                                    // 12 = default.aspx length
                                }
                            }

                            // what happens here is that the request is reverse-engineered to see if it matches what the friendly Url shoudl have been
                            // get what the friendly Url for this tab should be
                            string bestFriendlyUrl;

                            // 819 : leaving /do301/check in Url because not using cleanPath to remove from
                            string cleanPath = RedirectTokens.RemoveAnyRedirectTokensAndReasons(rewritePathOnly);

                            ////string cleanPath = rewritePathOnly.Replace("&do301=check","");//remove check parameter if it exists
                            ////cleanPath = cleanPath.Replace("&do301=true", "");//don't pass through internal redirect check parameter
                            cleanPath = cleanPath.Replace("&_aumdebug=true", string.Empty); // remove debug parameter if it exists

                            Match match = RewritePathRx.Match(rewritePathOnly ?? string.Empty);
							Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): match → {Match}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, match);
                            if (match.Success)
                            {
                                // when the pathOnly value ends with '=' it means there is a query string pair with a key and no value
                                // make the assumption that this was passed in as a page name OTHER than default page
                                string pageName = match.Groups["parm"].Value; // get the last parameter in the list

                                cleanPath = cleanPath.Replace(match.Value, string.Empty);

                                // remove the last parameter from the path

                                // generate teh friendly URl name with the last parameter as the page name, not a query string parameter
                                bestFriendlyUrl = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(
                                    tab,
                                    cleanPath,
                                    pageName + settings.PageExtension,
                                    result.HttpAlias,
                                    false,
                                    settings,
                                    Guid.Empty);
                            }
                            else
                            {
                                bestFriendlyUrl = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(
                                    tab,
                                    cleanPath,
                                    Globals.glbDefaultPage,
                                    result.HttpAlias,
                                    false,
                                    settings,
                                    Guid.Empty);
                            }

                            // if the incoming request doesn't match the 'most friendly' url, a 301 Moved Permanently status is returned, along with the friendly url
                            // check the bestFriendlyUrl against either the url, or rawUrl (with and without host)
                            // in each case, the aumdebug parameter will be searched for and replaced
                            var urlDecode = HttpUtility.UrlDecode(requestedUrl);
							Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): urlDecode → {UrlDecode}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, urlDecode);
                            if (urlDecode != null)
                            {
                                string rawUrlWithHost = StripDebugParameter(urlDecode.ToLowerInvariant());
								Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): rawUrlWithHost → {RawUrlWithHost}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, rawUrlWithHost);

                                ////string rawUrlWithHost = StripDebugParameter(System.Web.HttpUtility.UrlDecode(scheme + requestUri.Host + requestUri.PathAndQuery).ToLowerInvariant());
                                string rawUrlWithHostNoScheme = StripDebugParameter(rawUrlWithHost.Replace(scheme, string.Empty));
								Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): rawUrlWithHostNoScheme → {RawUrlWithHostNoScheme}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, rawUrlWithHostNoScheme);
                                string bestFriendlyNoScheme = StripDebugParameter(bestFriendlyUrl.ToLowerInvariant().Replace(scheme, string.Empty));
								Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): bestFriendlyNoScheme → {BestFriendlyNoScheme}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, bestFriendlyNoScheme);
                                string requestedPathNoScheme = StripDebugParameter(requestUri.AbsoluteUri.Replace(scheme, string.Empty).ToLowerInvariant());
								Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): requestedPathNoScheme → {RequestedPathNoScheme}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, requestedPathNoScheme);
                                string rawUrlLowerCase = StripDebugParameter(requestUri.AbsoluteUri.ToLowerInvariant());
								Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): rawUrlLowerCase → {RawUrlLowerCase}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, rawUrlLowerCase);

                                // check to see if just an alias redirect of an internal alias
                                var primaryAliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(result.PortalId).ToList();
								Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): primaryAliases → {PrimaryAliases}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, primaryAliases);

                                if (settings.InternalAliasList != null && settings.InternalAliasList.Count > 0 && primaryAliases.Count > 0)
                                {
                                    var cpa = primaryAliases.GetAliasByPortalIdAndSettings(result);
									Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): cpa → {Cpa}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, cpa);
                                    if (cpa != null)
                                    {
                                        string chosenAlias = cpa.HTTPAlias.ToLowerInvariant();
                                        foreach (InternalAlias ia in settings.InternalAliasList)
                                        {
                                            string internalAlias = ia.HttpAlias.ToLowerInvariant();
                                            if (requestedPathNoScheme.Contains(internalAlias))
                                            {
                                                // an internal alias has been used.
                                                // replace this in the comparison charts to do a 'fair' comparison
                                                requestedPathNoScheme = requestedPathNoScheme.Replace(internalAlias, chosenAlias);
                                                rawUrlWithHost = rawUrlWithHost.Replace(scheme + internalAlias, scheme + chosenAlias);
                                                rawUrlWithHostNoScheme = rawUrlWithHostNoScheme.Replace(internalAlias, chosenAlias);
                                                rawUrlLowerCase = rawUrlLowerCase.Replace(internalAlias, chosenAlias);
                                                break;
                                            }
                                        }
                                    }
                                }

                                // DNN-9158: prevent SSL Offloading infinite redirects
                                if (!result.IsSecureConnection && result.IsSSLOffloaded && bestFriendlyNoScheme.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                                {
                                    bestFriendlyNoScheme = $"http://{bestFriendlyNoScheme.Substring(8)}";
                                }

                                if (!(bestFriendlyNoScheme == requestedPathNoScheme
                                      || bestFriendlyNoScheme == rawUrlWithHost
                                      || HttpUtility.UrlDecode(bestFriendlyNoScheme) == rawUrlWithHost
                                      || bestFriendlyNoScheme == rawUrlWithHostNoScheme
                                      || bestFriendlyNoScheme == HttpUtility.UrlDecode(requestedPathNoScheme)
                                      || HttpUtility.UrlDecode(bestFriendlyNoScheme) == HttpUtility.UrlDecode(requestedPathNoScheme)
                                      || bestFriendlyNoScheme == rawUrlLowerCase))
                                {
                                    redirected = true;
                                    result.Action = ActionType.Redirect301;
                                    result.FinalUrl = bestFriendlyUrl;
                                    if (result.Reason != RedirectReason.Custom_Tab_Alias &&
                                        result.Reason != RedirectReason.Deleted_Page &&
                                        result.Reason != RedirectReason.Disabled_Page)
                                    {
                                        result.Reason = RedirectReason.Unfriendly_Url_2;
                                    }

                                    result.DebugMessages.Add("Compared :" + bestFriendlyNoScheme + " [generated] -> " + requestedPathNoScheme + " [requested with no scheme]");
                                    result.DebugMessages.Add("Compared :" + bestFriendlyNoScheme + " [generated] -> " + rawUrlWithHost + " [requested with host and scheme]");
                                    result.DebugMessages.Add("Compared :" + bestFriendlyNoScheme + " [generated] -> " + rawUrlWithHostNoScheme + " [requested with host, no scheme]");
                                    result.DebugMessages.Add("Compared :" + bestFriendlyNoScheme + " [generated] -> " + HttpUtility.UrlDecode(requestedPathNoScheme) + " [requested and decoded]");
                                    result.DebugMessages.Add("Compared :" + bestFriendlyNoScheme + " [generated] -> " + rawUrlLowerCase + " [requested raw Url]");
                                }
                            }
                        }
                    }
                }

                if (result.RedirectAllowed && settings.RedirectWrongCase)
                {
                    // check for redirects where a redirectToSubDomain is specified,
                    // redirect for Wrong case is specified, and there is a valid tab and it's not already redirected somewhere else
                    bool doRedirect = false;
					Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): doRedirect → {DoRedirect}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, doRedirect);
                    string redirectPath = redirected ? result.FinalUrl : requestUri.AbsoluteUri;
					Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): redirectPath → {RedirectPath}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, redirectPath);
                    string redirectPathOnly = redirectPath;
					Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): redirectPathOnly → {RedirectPathOnly}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, redirectPathOnly);
                    if (redirectPathOnly.Contains("?"))
                    {
                        redirectPathOnly = redirectPathOnly.Substring(0, redirectPathOnly.IndexOf("?", StringComparison.Ordinal));
                    }

                    // Thanks Etienne for the fix for Diacritic Characters Terminal Loop!
                    // if the url contains url encoded characters, they appear here uppercase -> %C3%83%C2
                    // decode the url to get back the original character and do proper casing comparison
                    string urlDecodedRedirectPath = HttpUtility.UrlDecode(redirectPathOnly);

                    // check for wrong case redirection
                    if (urlDecodedRedirectPath != null && (settings.RedirectWrongCase && !string.Equals(urlDecodedRedirectPath, urlDecodedRedirectPath.ToLowerInvariant(), StringComparison.Ordinal)))
                    {
                        TabInfo tab;
                        bool allowRedirect = CheckFor301RedirectExclusion(result.TabId, result.PortalId, true, out tab, settings);

                        if (allowRedirect && !string.IsNullOrEmpty(settings.ForceLowerCaseRegex))
                        {
                            // don't allow redirect if excluded from redirecting in the force lower case regex pattern (606)
                            var forceLowerCaseRegex = RegexUtils.GetCachedRegex(settings.ForceLowerCaseRegex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
							Logger.LogDebug("CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId}): forceLowerCaseRegex → {ForceLowerCaseRegex}", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId, forceLowerCaseRegex);
                            allowRedirect = !forceLowerCaseRegex.IsMatch(redirectPath);
                        }

                        if (allowRedirect)
                        {
                            // special case : when IIS automatically places /default.aspx on the end of the string,
                            // then don't try and redirect to the lower case /default.aspx, just let it through.
                            // we don't know whether IIS appended /Default.aspx on the end, however, we can guess
                            // if the redirectDefault.aspx is turned on (511)
                            if (!settings.RedirectDefaultPage && redirectPathOnly.EndsWith(Globals.glbDefaultPage, StringComparison.InvariantCultureIgnoreCase))
                            {
                                // ignore this, because it's just a redirect of the /Default.aspx to /default.aspx
                            }
                            else
                            {
                                redirectPath = redirectPath.Replace(redirectPathOnly, redirectPathOnly.ToLowerInvariant());
                                doRedirect = true;
                                result.Reason = RedirectReason.Not_Lower_Case;
                            }
                        }
                    }

                    if (doRedirect)
                    {
                        result.Action = ActionType.Redirect301;
                        result.FinalUrl = CheckForSiteRootRedirect(result.PortalAlias.HTTPAlias, redirectPath);
                        redirected = true;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting CheckForRedirects(requestUri={RequestUri},fullUrl={FullUrl},queryStringCol={QueryStringCol},result={Result},requestType={RequestType},settings={Settings},portalHomeTabId={PortalHomeTabId})", requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId);
            return redirected;
        }

        private static string StripDebugParameter(string url)
        {
			Logger.LogDebug(">>> Entering StripDebugParameter(url={Url})", url);
			Logger.LogDebug("<<< Exiting StripDebugParameter(url={Url})", url);
            return AumDebugRegex.Replace(url, string.Empty);
        }

        private static bool CheckFor301RedirectExclusion(int tabId, int portalId, bool checkBaseUrls, out TabInfo tab, FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering CheckFor301RedirectExclusion(tabId={TabId},portalId={PortalId},checkBaseUrls={CheckBaseUrls},settings={Settings})", tabId, portalId, checkBaseUrls, settings);
            bool doRedirect = false;
            tab = TabController.Instance.GetTab(tabId, portalId, false);

			Logger.LogDebug("CheckFor301RedirectExclusion(tabId={TabId},portalId={PortalId},checkBaseUrls={CheckBaseUrls},settings={Settings}): tab → {Tab}", tabId, portalId, checkBaseUrls, settings, tab);
			Logger.LogDebug("CheckFor301RedirectExclusion(tabId={TabId},portalId={PortalId},checkBaseUrls={CheckBaseUrls},settings={Settings}): doRedirect → {DoRedirect}", tabId, portalId, checkBaseUrls, settings, doRedirect);
            // don't redirect unless allowed, the tab is valid, and it's not an admin or super tab
            if (tab != null && tab.IsSuperTab == false && !tab.DoNotRedirect)
            {
                if (checkBaseUrls)
                {
                    // no redirect for friendly url purposes if the tab is in the 'base friendly urls' section
                    doRedirect = !RewriteController.IsExcludedFromFriendlyUrls(tab, settings, true);
                }
                else
                {
                    doRedirect = true;
                }
            }

			Logger.LogDebug("<<< Exiting CheckFor301RedirectExclusion(tabId={TabId},portalId={PortalId},checkBaseUrls={CheckBaseUrls},settings={Settings})", tabId, portalId, checkBaseUrls, settings);
            return doRedirect;
        }

        private static PortalAliasInfo GetPortalAlias(FriendlyUrlSettings settings, string requestUrl, out bool redirectAlias, out bool isPrimaryAlias, out string wrongAlias)
        {
			Logger.LogDebug(">>> Entering GetPortalAlias(settings={Settings},requestUrl={RequestUrl})", settings, requestUrl);
            PortalAliasInfo aliasInfo = null;
            redirectAlias = false;
            wrongAlias = null;
            isPrimaryAlias = false;
            OrderedDictionary portalAliases = TabIndexController.GetPortalAliases(settings);
			Logger.LogDebug("GetPortalAlias(settings={Settings},requestUrl={RequestUrl}): portalAliases → {PortalAliases}", settings, requestUrl, portalAliases);
			Logger.LogDebug("GetPortalAlias(settings={Settings},requestUrl={RequestUrl}): redirectAlias → {RedirectAlias}", settings, requestUrl, redirectAlias);
			Logger.LogDebug("GetPortalAlias(settings={Settings},requestUrl={RequestUrl}): wrongAlias → {WrongAlias}", settings, requestUrl, wrongAlias);
			Logger.LogDebug("GetPortalAlias(settings={Settings},requestUrl={RequestUrl}): isPrimaryAlias → {IsPrimaryAlias}", settings, requestUrl, isPrimaryAlias);
            foreach (string alias in portalAliases.Keys)
            {
                var urlToMatch = requestUrl;
				Logger.LogDebug("GetPortalAlias(settings={Settings},requestUrl={RequestUrl}): urlToMatch → {UrlToMatch}", settings, requestUrl, urlToMatch);

                // in fact, requested url should contain alias
				Logger.LogDebug("GetPortalAlias(settings={Settings},requestUrl={RequestUrl}): aliasInfo → {AliasInfo}", settings, requestUrl, aliasInfo);
                // for better performance, need to check whether we want to proceed with a whole url matching or not
                // if alias is not a part of url -> let's proceed to the next iteration
                var aliasIndex = urlToMatch.IndexOf(alias, StringComparison.InvariantCultureIgnoreCase);
				Logger.LogDebug("GetPortalAlias(settings={Settings},requestUrl={RequestUrl}): aliasIndex → {AliasIndex}", settings, requestUrl, aliasIndex);
                if (aliasIndex < 0)
                {
                    continue;
                }
                else
                {
                    // we do not accept URL if the first occurence of alias is presented somewhere in the query string
                    var queryIndex = urlToMatch.IndexOf("?", StringComparison.InvariantCultureIgnoreCase);
					Logger.LogDebug("GetPortalAlias(settings={Settings},requestUrl={RequestUrl}): queryIndex → {QueryIndex}", settings, requestUrl, queryIndex);
                    if (queryIndex >= 0 && queryIndex < aliasIndex)
                    {
                        // alias is in the query string, go to the next alias
                        continue;
                    }

                    // we are fine here, lets prepare URL to be validated in regex
                    urlToMatch = urlToMatch.ReplaceIgnoreCase(alias, "_ALIAS_");
                }

                // check whether requested URL has the right URL format containing existing alias
                // i.e. url is http://dnndev.me/site1/query?string=test, alias is dnndev.me/site1
                // in the below expression we will validate following value http://_ALIAS_/query?string=test
                var aliasMatch = AliasUrlRegex.Match(urlToMatch);
				Logger.LogDebug("GetPortalAlias(settings={Settings},requestUrl={RequestUrl}): aliasMatch → {AliasMatch}", settings, requestUrl, aliasMatch);
                if (aliasMatch.Success)
                {
                    // check for mobile browser and matching
                    var aliasEx = (PortalAliasInfo)portalAliases[alias];
					Logger.LogDebug("GetPortalAlias(settings={Settings},requestUrl={RequestUrl}): aliasEx → {AliasEx}", settings, requestUrl, aliasEx);
                    redirectAlias = aliasEx.Redirect;
                    if (redirectAlias)
                    {
                        wrongAlias = alias;
                    }

                    isPrimaryAlias = aliasEx.IsPrimary;
                    aliasInfo = aliasEx;
                    break;
                }
            }

			Logger.LogDebug("<<< Exiting GetPortalAlias(settings={Settings},requestUrl={RequestUrl})", settings, requestUrl);
            return aliasInfo;
        }

        private static bool CheckForSecureRedirect(
            PortalSettings portalSettings,
            Uri requestUri,
            UrlAction result,
            NameValueCollection queryStringCol,
            FriendlyUrlSettings settings)
        {
			Logger.LogDebug(">>> Entering CheckForSecureRedirect(portalSettings={PortalSettings},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings})", portalSettings, requestUri, result, queryStringCol, settings);
            bool redirectSecure = false;
			Logger.LogDebug("CheckForSecureRedirect(portalSettings={PortalSettings},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): redirectSecure → {RedirectSecure}", portalSettings, requestUri, result, queryStringCol, settings, redirectSecure);
            string url = requestUri.ToString();
			Logger.LogDebug("CheckForSecureRedirect(portalSettings={PortalSettings},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): url → {Url}", portalSettings, requestUri, result, queryStringCol, settings, url);

            // 889 : don't run secure redirect code for physical resources or requests that aren't a rewritten Url
            if (result.IsPhysicalResource == false && result.TabId >= 0)
            {
                // no secure redirection for physical resources, only tab-specific requests can be redirected for ssl connections
                if (portalSettings.ActiveTab != null)
                {
                    result.DebugMessages.Add($"ActiveTab: {portalSettings.ActiveTab.TabID}/{portalSettings.ActiveTab.TabName} IsSecure: {portalSettings.ActiveTab.IsSecure}");

                    switch (portalSettings.SSLSetup)
                    {
                        case Abstractions.Security.SiteSslSetup.On:
                            if (!result.IsSecureConnection)
                            {
                                redirectSecure = true;
                                url = url.Replace("http://", "https://");
                            }

                            break;
                        case Abstractions.Security.SiteSslSetup.Advanced:
                            // 717 : check page is secure, connection is not secure
                            // 952 : support SSl Offloading in DNN 6.2+
                            if (portalSettings.ActiveTab.IsSecure && !result.IsSecureConnection && !result.IsSSLOffloaded)
                            {
                                redirectSecure = true;
                                string stdUrl = portalSettings.STDURL;
                                string sslUrl = portalSettings.SSLURL;
                                if (string.IsNullOrEmpty(result.HttpAlias) == false)
                                {
                                    stdUrl = result.HttpAlias;
                                }

                                url = url.Replace("http://", "https://");
                                url = ReplaceDomainName(url, stdUrl, sslUrl);
                            }

                            if (portalSettings.SSLEnforced)
                            {
                                // Prevent browser's mixed-content error in case we open a secure PopUp or a secure iframe
                                // from an unsecure page
                                if (!portalSettings.ActiveTab.IsSecure &&
                                    result.IsSecureConnection &&
                                    !UrlUtils.IsPopUp(url))
                                {
                                    // has connection already been forced to secure?
                                    if (queryStringCol["ssl"] == null)
                                    {
                                        // no? well this page shouldn't be secure
                                        string stdUrl = portalSettings.STDURL;
                                        string sslUrl = portalSettings.SSLURL;
                                        url = url.Replace("https://", "http://");
                                        url = ReplaceDomainName(url, sslUrl, stdUrl);
                                        redirectSecure = true;
                                    }
                                }
                            }

                            break;
                    }
                }

                if (redirectSecure)
                {
                    // now check to see if excluded.  Why now? because fewer requests are made to redirect secure,
                    // so we don't have to check the exclusion as often.
                    bool exclude = false;
					Logger.LogDebug("CheckForSecureRedirect(portalSettings={PortalSettings},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): exclude → {Exclude}", portalSettings, requestUri, result, queryStringCol, settings, exclude);
                    if (!string.IsNullOrEmpty(settings.DoNotRedirectSecureRegex))
                    {
                        // match the raw url
                        var doNotRedirectSecureRegex = RegexUtils.GetCachedRegex(settings.DoNotRedirectSecureRegex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
						Logger.LogDebug("CheckForSecureRedirect(portalSettings={PortalSettings},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): doNotRedirectSecureRegex → {DoNotRedirectSecureRegex}", portalSettings, requestUri, result, queryStringCol, settings, doNotRedirectSecureRegex);
                        exclude = doNotRedirectSecureRegex.IsMatch(result.RawUrl);
                    }

                    if (!exclude)
                    {
                        result.Action = ActionType.Redirect302Now;
						Logger.LogDebug("CheckForSecureRedirect(portalSettings={PortalSettings},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): result.Action → {Arg5}", portalSettings, requestUri, result, queryStringCol, settings, result.Action);
						Logger.LogDebug("CheckForSecureRedirect(portalSettings={PortalSettings},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): result.Reason → {Arg5}", portalSettings, requestUri, result, queryStringCol, settings, result.Reason);
                        result.Reason = RedirectReason.Secure_Page_Requested;

                        // 760 : get the culture specific home page tabid for a redirect comparison
                        int homePageTabId = portalSettings.HomeTabId;
                        homePageTabId = TabPathHelper.GetHomePageTabIdForCulture(
                            portalSettings.DefaultLanguage,
                            portalSettings.PortalId,
                            result.CultureCode,
                            homePageTabId);
                        if (result.TabId == homePageTabId)
                        {
                            // replace the /default.aspx in the Url if it was found
                            url = DefaultPageRegex.Replace(url, "/");
                        }

                        result.FinalUrl = url;
						Logger.LogDebug("CheckForSecureRedirect(portalSettings={PortalSettings},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): result.FinalUrl → {Arg5}", portalSettings, requestUri, result, queryStringCol, settings, result.FinalUrl);
                    }
                    else
                    {
                        // 702 : change return value if exclusion has occured
                        redirectSecure = false;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting CheckForSecureRedirect(portalSettings={PortalSettings},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings})", portalSettings, requestUri, result, queryStringCol, settings);
            return redirectSecure;
        }

        private static string ReplaceDomainName(string url, string replaceDomain, string withDomain)
        {
			Logger.LogDebug(">>> Entering ReplaceDomainName(url={Url},replaceDomain={ReplaceDomain},withDomain={WithDomain})", url, replaceDomain, withDomain);
            if (replaceDomain != string.Empty && withDomain != string.Empty)
            {
                // 951 : change find/replace routine to regex for more accurate replacement
                // (previous method gives false positives if the SSL Url is contained within the STD url)
                string find = @"(?<=https?://)" + Regex.Escape(withDomain);
                if (Regex.IsMatch(url, find, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) == false)
                {
                    string replaceFind = @"(?<=https?://)" + Regex.Escape(replaceDomain);
                    url = Regex.Replace(url, replaceFind, withDomain, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
					Logger.LogDebug("ReplaceDomainName(url={Url},replaceDomain={ReplaceDomain},withDomain={WithDomain}): url → {Url1}", url, replaceDomain, withDomain, url);
                }
            }

			Logger.LogDebug("<<< Exiting ReplaceDomainName(url={Url},replaceDomain={ReplaceDomain},withDomain={WithDomain})", url, replaceDomain, withDomain);
            return url;
        }

        private static void SecurityCheck(HttpApplication app)
        {
			Logger.LogDebug(">>> Entering SecurityCheck(app={App})", app);
            HttpRequest request = app.Request;
            HttpServerUtility server = app.Server;

            // 675 : unnecessarily strict url validation
            // URL validation
            // check for ".." escape characters commonly used by hackers to traverse the folder tree on the server
            // the application should always use the exact relative location of the resource it is requesting
            var strURL = request.Url.AbsolutePath;
            var strDoubleDecodeURL = server.UrlDecode(server.UrlDecode(request.Url.AbsolutePath)) ?? string.Empty;
			Logger.LogDebug("SecurityCheck(app={App}): strDoubleDecodeURL → {StrDoubleDecodeURL}", app, strDoubleDecodeURL);
            if (UrlSlashesRegex.IsMatch(strURL) || UrlSlashesRegex.IsMatch(strDoubleDecodeURL))
            {
				Logger.LogWarning("Throwing HttpException from SecurityCheck(app={App})", app);
			Logger.LogDebug("<<< Exiting SecurityCheck(app={App})", app);
                throw new HttpException(404, "Not Found");
            }
        }

        private void ProcessRequest(
            HttpContext context,
            Uri requestUri,
            bool useFriendlyUrls,
            UrlAction result,
            FriendlyUrlSettings settings,
            bool allowSettingsChange,
            Guid parentTraceId)
        {
            bool finished = false;
            bool showDebug = false;
            bool postRequest = false;

            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            string requestType = request.RequestType;
            NameValueCollection queryStringCol = request.QueryString;

            try
            {
                string fullUrl, querystring;

                // 699: get the full url based on the request and the querystring, rather than the requestUri.ToString()
                // there is a difference in encoding, which can corrupt results when an encoded value is in the querystring
                RewriteController.GetUrlWithQuerystring(request, requestUri, out fullUrl, out querystring);

                showDebug = CheckForDebug(request, queryStringCol, settings.AllowDebugCode);
                string ignoreRegex = settings.IgnoreRegex;
                bool ignoreRequest = IgnoreRequest(this.portalAliasService, result, fullUrl, ignoreRegex, request);
                bool redirectAlias = false;
                if (!ignoreRequest)
                {
                    // set original path
                    context.Items["UrlRewrite:OriginalUrl"] = requestUri.AbsoluteUri;

                    // set the path of the result object, and determine if a redirect is allowed on this request
                    result.SetOriginalPath(requestUri.ToString(), settings);

                    // 737 : set the mobile browser
                    result.SetBrowserType(request, response, settings);

                    // add to context
                    context.Items["UrlRewrite:BrowserType"] = result.BrowserType.ToString();

                    // 839 : split out this check
                    result.SetRedirectAllowed(result.OriginalPath, settings);

                    // find the portal alias first
                    var requestedAlias = GetPortalAlias(settings, fullUrl, out redirectAlias, out _, out var wrongAlias);
                    if (requestedAlias != null)
                    {
                        // 827 : now get the correct settings for this portal (if not a test request)
                        // 839 : separate out redirect check as well and move above first redirect test (ConfigurePortalAliasRedirect)
                        if (allowSettingsChange)
                        {
                            settings = new FriendlyUrlSettings(requestedAlias.PortalID);
                            result.SetRedirectAllowed(result.OriginalPath, settings);
                        }

                        result.PortalAlias = requestedAlias;
                        result.PrimaryAlias = requestedAlias; // this is the primary alias
                        result.PortalId = requestedAlias.PortalID;
                        result.CultureCode = requestedAlias.CultureCode;

                        // get the portal alias mapping for this portal
                        result.PortalAliasMapping = PortalSettingsController.Instance().GetPortalAliasMappingMode(requestedAlias.PortalID);

                        // if requested alias wasn't the primary, we have a replacement, redirects are allowed and the portal alias mapping mode is redirect
                        // then do a redirect based on the wrong portal
                        if ((redirectAlias && wrongAlias != null) && result.RedirectAllowed && result.PortalAliasMapping != PortalSettings.PortalAliasMapping.Redirect)
                        {
                            // this is the alias, we are going to enforce it as the primary alias
                            result.PortalAlias = requestedAlias;
                            result.PrimaryAlias = requestedAlias;

                            // going to redirect this alias because it is incorrect
                            // or do we just want to mark as 'check for 301??'
                            redirectAlias = ConfigurePortalAliasRedirect(
                                ref result,
                                wrongAlias,
                                requestedAlias.HTTPAlias,
                                false,
                                settings.InternalAliasList,
                                settings);
                        }
                        else
                        {
                            // do not redirect the wrong alias, but set the primary alias value
                            if (wrongAlias != null)
                            {
                                // get the portal alias info for the requested alias (which is the wrong one)
                                // and set that as the alias, but also set the found alias as the primary
                                PortalAliasInfo wrongAliasInfo = PortalAliasController.Instance.GetPortalAlias(wrongAlias);
                                if (wrongAliasInfo != null)
                                {
                                    result.PortalAlias = wrongAliasInfo;
                                    result.PrimaryAlias = requestedAlias;
                                }
                            }
                        }
                    }
                }

                ignoreRegex = settings.IgnoreRegex;
                ignoreRequest = IgnoreRequest(this.portalAliasService, result, fullUrl, ignoreRegex, request);
                if (ignoreRequest)
                {
                    return;
                }

                // check to see if a post request
                if (request.RequestType == "POST")
                {
                    postRequest = true;
                }

                // check the portal alias again.  This time, in more depth now that the portal Id is known
                // this check handles browser types/language specific aliases & mobile aliases
                if (!redirectAlias && this.IsPortalAliasIncorrect(context, request, requestUri, result, queryStringCol, settings, parentTraceId, out var primaryHttpAlias))
                {
                    // it was an incorrect alias
                    var primaryAlias = (PortalAliasInfo)this.portalAliasService.GetPortalAlias(primaryHttpAlias);
                    if (primaryAlias != null)
                    {
                        result.PrimaryAlias = primaryAlias;
                    }

                    // try and redirect the alias if the settings allow it
                    redirectAlias = RedirectPortalAlias(primaryHttpAlias, ref result, settings);
                }

                if (redirectAlias)
                {
                    // not correct alias for portal : will be redirected
                    // perform a 301 redirect if one has already been found
                    response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                    response.RedirectPermanent(result.FinalUrl, false);
                    finished = true;
                }

                if (!finished)
                {
                    // Check to see if this to be rewritten into default.aspx?tabId=nn format
                    // this call is the main rewriting matching call.  It makes the decision on whether it is a
                    // physical file, whether it is toe be rewritten or redirected by way of a stored rule

                    // Check if we have a standard url
                    var uri = new Uri(fullUrl);
                    if (uri.PathAndQuery.StartsWith("/" + Globals.glbDefaultPage, StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.DoRewrite = true;
                        result.Action = ActionType.CheckFor301;
                        result.RewritePath = Globals.glbDefaultPage + uri.Query;
                    }
                    else
                    {
                        CheckForRewrite(this.HostSettings, this.PortalController, this.appStatus, this.portalGroupController, fullUrl, querystring, result, useFriendlyUrls, queryStringCol, settings, out bool _, parentTraceId);
                    }

                    // return 404 if there is no portal alias for a rewritten request
                    if (result.DoRewrite && result.PortalAlias == null)
                    {
                        // 882 : move this logic in from where it was before to here
                        // so that non-rewritten requests don't trip over it
                        // no portal alias found for the request : that's a 404 error
                        result.Action = ActionType.Output404;
                        result.Reason = RedirectReason.No_Portal_Alias;

                        Handle404OrException(settings, context, null, result, false, showDebug);
                        finished = true; // cannot fulfil request unless correct portal alias specified
                    }
                }

                // now we may know the TabId. If the current alias is not the same as the primary alias,
                // we should check if the current alias is indeed a valid custom alias for the current tab.
                if (result.TabId > 0 && result.HttpAlias != result.PrimaryAlias.HTTPAlias && !CheckIfAliasIsCurrentTabCustomTabAlias(ref result, settings))
                {
                    // it was an incorrect alias
                    // try and redirect the alias if the settings allow it
                    if (RedirectPortalAlias(result.PrimaryAlias.HTTPAlias, ref result, settings))
                    {
                        // not correct alias for tab : will be redirected
                        // perform a 301 redirect if one has already been found
                        response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                        response.RedirectPermanent(result.FinalUrl, false);
                        finished = true;
                    }
                }

                if (!finished && result.DoRewrite)
                {
                    // check the identified portal alias details for any extra rewrite information required
                    // this includes the culture and the skin, which can be placed into the rewrite path
                    // This logic is here because it will catch any Urls which are excluded from rewriting
                    var primaryAliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(result.PortalId).ToList();

                    if (result.PortalId > -1 && result.HttpAlias != null)
                    {
                        string culture;
                        string skin;
                        BrowserTypes browserType;
                        primaryAliases.GetSettingsByPortalIdAndAlias(
                            result.PortalId,
                            result.HttpAlias,
                            out culture,
                            out browserType,
                            out skin);

                        // add language code to path if it exists (not null) and if it's not already there
                        string rewritePath = result.RewritePath;
                        if (RewriteController.AddLanguageCodeToRewritePath(ref rewritePath, culture))
                        {
                            result.CultureCode = culture;
                        }

                        // 852: add skinSrc to path if it exists and if it's not already there
                        RewriteController.AddSkinToRewritePath(result.TabId, result.PortalId, ref rewritePath, skin, out var debugMessage);
                        result.RewritePath = rewritePath; // reset back from ref temp var
                        if (debugMessage != null)
                        {
                            result.DebugMessages.Add(debugMessage);
                        }
                    }
                }

                if (!finished && result.DoRewrite)
                {
                    // if so, do the rewrite
                    if (result.RewritePath.StartsWith(result.Scheme, StringComparison.OrdinalIgnoreCase) || !result.RewritePath.StartsWith(Globals.glbDefaultPage, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!result.RewritePath.Contains(Globals.glbDefaultPage, StringComparison.OrdinalIgnoreCase))
                        {
                            RewriterUtils.RewriteUrl(context, "~/" + result.RewritePath);
                        }
                        else
                        {
                            // if there is no TabId and we have the domain
                            if (!result.RewritePath.Contains("tabId=", StringComparison.OrdinalIgnoreCase))
                            {
                                RewriterUtils.RewriteUrl(context, "~/" + result.RewritePath);
                            }
                            else
                            {
                                RewriterUtils.RewriteUrl(context, result.RewritePath);
                            }
                        }
                    }
                    else
                    {
                        RewriterUtils.RewriteUrl(context, "~/" + result.RewritePath);
                    }
                }

                // confirm which portal the request is for
                if (!finished)
                {
                    this.IdentifyPortalAlias(context, request, requestUri, result, queryStringCol, settings, parentTraceId);
                    if (result.Action == ActionType.Redirect302Now)
                    {
                        // performs a 302 redirect if requested
                        response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                        response.Redirect(result.FinalUrl, false);
                        finished = true;
                    }
                    else
                    {
                        if (result.Action == ActionType.Redirect301 && !string.IsNullOrEmpty(result.FinalUrl))
                        {
                            finished = true;

                            // perform a 301 redirect if one has already been found
                            response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                            response.RedirectPermanent(result.FinalUrl, false);
                        }
                    }
                }

                if (!finished)
                {
                    // check to see if this tab has an external url that should be forwared or not
                    finished = CheckForTabExternalForwardOrRedirect(context, ref result, response, settings, parentTraceId);
                }

                // check for a parameter redirect (we had to do all the previous processing to know we are on the right portal and identify the tabid)
                // if the CustomParmRewrite flag is set, it means we already rewrote these parameters, so they have to be correct, and aren't subject to
                // redirection.  The only reason to do a custom parm rewrite is to interpret already-friendly parameters
                if (!finished
                    && !postRequest /* either request is null, or it's not a post - 551 */
                    && result.HttpAlias != null /* must have a http alias */
                    && !result.CustomParmRewrite && /* not custom rewritten parms */
                    ((settings.EnableCustomProviders &&
                      RedirectController.CheckForModuleProviderRedirect(requestUri, ref result, queryStringCol, settings, parentTraceId))

                     // 894 : allow disable of all custom providers
                     ||
                     RedirectController.CheckForParameterRedirect(requestUri, ref result, queryStringCol, settings)))
                {
                    // 301 redirect to new location based on parameter match
                    if (response != null)
                    {
                        switch (result.Action)
                        {
                            case ActionType.Redirect301:
                                response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                                response.RedirectPermanent(result.FinalUrl);
                                break;
                            case ActionType.Redirect302:
                                response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                                response.Redirect(result.FinalUrl);
                                break;
                            case ActionType.Output404:
                                response.AppendHeader("X-Result-Reason", result.Reason.ToString().Replace("_", " "));
                                Handle404OrException(settings, context, null, result, true, showDebug);
                                break;
                        }
                    }

                    finished = true;
                }

                // shifted until after the 301 redirect code to allow redirects to be checked for pages which have no rewrite value
                // look for a 404 result from the rewrite, because of a deleted page or rule
                if (!finished && result.Action == ActionType.Output404)
                {
                    if (result.OriginalPath.Equals(result.HttpAlias, StringComparison.OrdinalIgnoreCase)
                        && result.PortalAlias != null
                        && result.Reason != RedirectReason.Deleted_Page
                        && result.Reason != RedirectReason.Disabled_Page)
                    {
                        // Request for domain with no page identified (and no home page set in Site Settings)
                        result.Action = ActionType.Continue;
                    }
                    else
                    {
                        finished = true;
                        response.AppendHeader("X-Result-Reason", result.Reason.ToString().Replace("_", " "));

                        if (showDebug)
                        {
                            ShowDebugData(context, requestUri.AbsoluteUri, result, null);
                        }

                        // show the 404 page if configured
                        result.Reason = RedirectReason.Requested_404;
                        Handle404OrException(settings, context, null, result, true, showDebug);
                    }
                }

                if (!finished)
                {
                    // add the portal settings to the app context if the portal alias has been found and is correct
                    if (result.PortalId != -1 && result.PortalAlias != null)
                    {
                        // for invalid tab id other than -1, show the 404 page
                        TabInfo tabInfo = TabController.Instance.GetTab(result.TabId, result.PortalId, false);
                        if (tabInfo == null && result.TabId > -1)
                        {
                            finished = true;

                            if (showDebug)
                            {
                                ShowDebugData(context, requestUri.AbsoluteUri, result, null);
                            }

                            // show the 404 page if configured
                            result.Action = ActionType.Output404;
                            result.Reason = RedirectReason.Requested_404;
                            response.AppendHeader("X-Result-Reason", result.Reason.ToString().Replace("_", " "));
                            Handle404OrException(settings, context, null, result, true, showDebug);
                        }
                        else
                        {
                            Globals.SetApplicationName(result.PortalId);

                            // load the PortalSettings into current context
                            var portalSettings = new PortalSettings(result.TabId, result.PortalAlias);

                            // set the primary alias if one was specified
                            if (result.PrimaryAlias != null)
                            {
                                portalSettings.PrimaryAlias = result.PrimaryAlias;
                            }

                            if (result.CultureCode != null && fullUrl.Contains(result.CultureCode) &&
                                portalSettings.DefaultLanguage == result.CultureCode)
                            {
                                // when the request culture code is the same as the portal default, check for a 301 redirect, because we try and remove the language from the url where possible
                                result.Action = ActionType.CheckFor301;
                            }

                            int portalHomeTabId = portalSettings.HomeTabId;
                            if (context != null && portalSettings != null && !context.Items.Contains("PortalSettings"))
                            {
                                context.Items.Add("PortalSettings", portalSettings);

                                // load PortalSettings and HostSettings dictionaries into current context
                                // specifically for use in DotNetNuke.Web.Client, which can't reference DotNetNuke.dll to get settings the normal way
                                context.Items.Add("PortalSettingsDictionary", Portals.PortalController.Instance.GetPortalSettings(portalSettings.PortalId));
                                context.Items.Add("HostSettingsDictionary", HostController.Instance.GetSettingsDictionary());
                            }

                            // check if a secure redirection is needed
                            // this would be done earlier in the piece, but need to know the portal settings, tabid etc before processing it
                            bool redirectSecure = CheckForSecureRedirect(portalSettings, requestUri, result, queryStringCol, settings);
                            if (redirectSecure)
                            {
                                if (response != null)
                                {
                                    // 702 : don't check final url until checked for null reference first
                                    if (result.FinalUrl != null)
                                    {
                                        if (result.FinalUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (showDebug)
                                            {
                                                /*
                                                    string debugMsg = "{0}, {1}, {2}, {3}, {4}";
                                                    string productVer = System.Reflection.Assembly.GetExecutingAssembly().GetName(false).Version.ToString();
                                                    response.AppendHeader("X-" + prodName + "-Debug", string.Format(debugMsg, requestUri.AbsoluteUri, result.FinalUrl, result.RewritePath, result.Action, productVer));
                                                     */
                                                ShowDebugData(context, fullUrl, result, null);
                                            }

                                            response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                                            response.RedirectPermanent(result.FinalUrl);
                                            finished = true;
                                        }
                                        else
                                        {
                                            if (settings.SSLClientRedirect)
                                            {
                                                // redirect back to http version, use client redirect
                                                response.Clear();

                                                // add a refresh header to the response
                                                response.AddHeader("Refresh", "0;URL=" + result.FinalUrl);

                                                // add the clientside javascript redirection script
                                                var finalUrl = HttpUtility.HtmlEncode(result.FinalUrl);
                                                response.Write("<html><head><title></title>");
                                                response.Write(@"<!-- <script language=""javascript"">window.location.replace(""" + finalUrl + @""")</script> -->");
                                                response.Write("</head><body><div><a href='" + finalUrl + "'>" + finalUrl + "</a></div></body></html>");
                                                if (showDebug)
                                                {
                                                    /*
                                                        string debugMsg = "{0}, {1}, {2}, {3}, {4}";
                                                        string productVer = System.Reflection.Assembly.GetExecutingAssembly().GetName(false).Version.ToString();
                                                        response.AppendHeader("X-" + prodName + "-Debug", string.Format(debugMsg, requestUri.AbsoluteUri, result.FinalUrl, result.RewritePath, result.Action, productVer));
                                                         */
                                                    ShowDebugData(context, fullUrl, result, null);
                                                }

                                                // send the response
                                                // 891 : reinstate the response.end to stop the entire page loading
                                                response.End();
                                                finished = true;
                                            }
                                            else
                                            {
                                                response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                                                response.RedirectPermanent(result.FinalUrl);
                                                finished = true;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // check for, and do a 301 redirect if required
                                if (CheckForRedirects(requestUri, fullUrl, queryStringCol, result, requestType, settings, portalHomeTabId))
                                {
                                    if (response != null)
                                    {
                                        if (result.Action == ActionType.Redirect301)
                                        {
                                            response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                                            response.RedirectPermanent(result.FinalUrl, false);
                                            finished = true;
                                        }
                                        else if (result.Action == ActionType.Redirect302)
                                        {
                                            response.AppendHeader("X-Redirect-Reason", result.Reason.ToString().Replace("_", " ") + " Requested");
                                            response.Redirect(result.FinalUrl, false);
                                            finished = true;
                                        }
                                    }
                                }
                                else
                                {
                                    // 612 : Don't clear out a 302 redirect if set
                                    if (result.Action != ActionType.Redirect302 &&
                                        result.Action != ActionType.Redirect302Now)
                                    {
                                        result.Reason = RedirectReason.Not_Redirected;
                                        result.FinalUrl = null;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // alias does not exist in database
                        // and all attempts to find another have failed
                        // this should only happen if the HostPortal does not have any aliases
                        result.Action = ActionType.Output404;
                        if (response != null)
                        {
                            if (showDebug)
                            {
                                ShowDebugData(context, fullUrl, result, null);
                            }

                            result.Reason = RedirectReason.Requested_404;

                            // 912 : change 404 type to transfer to allow transfer to main portal in single-portal installs
                            Handle404OrException(settings, context, null, result, true, showDebug);
                            finished = true;
                        }
                    }
                }

                // 404 page ??
                if (settings.TabId404 > 0 && settings.TabId404 == result.TabId)
                {
                    string status = queryStringCol["status"];
                    if (status == "404")
                    {
                        // respond with a 404 error
                        result.Action = ActionType.Output404;
                        result.Reason = RedirectReason.Requested_404_In_Url;
                        Handle404OrException(settings, context, null, result, true, showDebug);
                    }
                }
                else
                {
                    if (!result.DoRewrite && result.CanRewrite != StateBoolean.False && !finished &&
                        result.Action == ActionType.Continue)
                    {
                        // 739 : catch no-extension 404 errors
                        string pathWithNoQs = result.OriginalPath;
                        if (pathWithNoQs.Contains("?", StringComparison.Ordinal))
                        {
                            pathWithNoQs = pathWithNoQs.Substring(0, pathWithNoQs.IndexOf("?", StringComparison.Ordinal));
                        }

                        if (!pathWithNoQs.Substring(pathWithNoQs.Length - 5, 5).Contains(".", StringComparison.Ordinal))
                        {
                            // no page extension, output a 404 if the Url is not found
                            // 766 : check for physical path before passing off as a 404 error
                            // 829 : change to use action physical path
                            // 893 : filter by regex pattern to exclude urls which are valid, but show up as extensionless
                            var extensionlessUrlsRegex = RegexUtils.GetCachedRegex(settings.ValidExtensionlessUrlsRegex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                            if ((request != null && Directory.Exists(result.PhysicalPath)) || extensionlessUrlsRegex.IsMatch(pathWithNoQs))
                            {
                                // do nothing : it's a request for a valid physical path, maybe including a default document
                                result.VirtualPath = StateBoolean.False;
                            }
                            else
                            {
                                if (!Globals.ServicesFrameworkRegex.IsMatch(context.Request.RawUrl))
                                {
                                    // no physical path, intercept the request and hand out a 404 error
                                    result.Action = ActionType.Output404;
                                    result.Reason = RedirectReason.Page_404;
                                    result.VirtualPath = StateBoolean.True;

                                    // add in a message to explain this 404, becaue it can be cryptic
                                    result.DebugMessages.Add("404 Reason : Not found and no extension");
                                    Handle404OrException(settings, context, null, result, true, showDebug);
                                }
                            }
                        }
                    }
                }

                // show debug messages after extensionless-url special 404 handling
                if (showDebug)
                {
                    ShowDebugData(context, fullUrl, result, null);
                }
            }
            catch (ThreadAbortException)
            {
                // do nothing, a threadAbortException will have occured from using a server.transfer or response.redirect within the code block.  This is the highest
                // level try/catch block, so we handle it here.
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                if (showDebug)
                {
                    Services.Exceptions.Exceptions.LogException(ex);
                }

                if (response != null)
                {
                    if (showDebug)
                    {
                        ShowDebugData(context, requestUri.AbsoluteUri, result, ex);
                    }

                    if (result != null)
                    {
                        result.Ex = ex;
                        result.Reason = RedirectReason.Exception;
                    }

                    Handle404OrException(settings, context, ex, result, false, showDebug);
                }
                else
                {
                    if (result != null && result.DebugMessages != null)
                    {
                        result.DebugMessages.Add("Exception: " + ex.Message);
                        result.DebugMessages.Add("Stack Trace: " + ex.StackTrace);
                    }

                    throw;
                }
            }
            finally
            {
                // 809 : add in new code copied from urlRewrite class in standard Url Rewrite module
                if (context != null && context.Items["FirstRequest"] != null)
                {
                    context.Items.Remove("FirstRequest");

                    // process any messages in the eventQueue for the Application_Start_FIrstRequest event
                    EventQueueController.ProcessMessages("Application_Start_FirstRequest");
                }
            }
			Logger.LogDebug("<<< Exiting ProcessRequest(context={Context},requestUri={RequestUri},useFriendlyUrls={UseFriendlyUrls},result={Result},settings={Settings},allowSettingsChange={AllowSettingsChange},parentTraceId={ParentTraceId})", context, requestUri, useFriendlyUrls, result, settings, allowSettingsChange, parentTraceId);
        }

        private void IdentifyPortalAlias(
            HttpContext context,
            HttpRequest request,
            Uri requestUri,
            UrlAction result,
            NameValueCollection queryStringCol,
            FriendlyUrlSettings settings,
            Guid parentTraceId)
        {
            // get the domain name of the request, if it isn't already supplied
			Logger.LogDebug(">>> Entering IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId})", context, request, requestUri, result, queryStringCol, settings, parentTraceId);
            if (request != null && string.IsNullOrEmpty(result.DomainName))
            {
                result.DomainName = Globals.GetDomainName(request); // parse the domain name out of the request
            }

            // get tabId from querystring ( this is mandatory for maintaining portal context for child portals )
            if (queryStringCol["tabid"] != null)
            {
                string raw = queryStringCol["tabid"];
                int tabId;
                if (int.TryParse(raw, out tabId))
                {
                    result.TabId = tabId;
					Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): result.TabId → {Arg7}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, result.TabId);
                }
                else
                {
                    // couldn't parse tab id
                    // split in two?
                    string[] tabids = raw.Split(',');
                    if (tabids.GetUpperBound(0) > 0)
                    {
                        // hmm more than one tabid
                        if (int.TryParse(tabids[0], out tabId))
                        {
                            result.TabId = tabId;

                            // but we want to warn against this!
                            var ex =
                                new IllegalRequestException(
                                    "Illegal request exception : Two TabId parameters provided in a single request: " +
                                    requestUri);
                            UrlRewriterUtils.LogExceptionInRequest(ex, "Not Set", result);

                            result.Ex = ex;
							Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): result.Ex → {Arg7}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, result.Ex);
                        }
                        else
                        {
                            // yeah, nothing, divert to 404
                            result.Action = ActionType.Output404;
							Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): result.Action → {Arg7}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, result.Action);
                            var ex =
                                new IllegalRequestException(
                                    "Illegal request exception : TabId parameters in query string, but invalid TabId requested : " +
                                    requestUri);
                            UrlRewriterUtils.LogExceptionInRequest(ex, "Not Set", result);
                            result.Ex = ex;
                        }
                    }
                }
            }

            // get PortalId from querystring ( this is used for host menu options as well as child portal navigation )
            if (queryStringCol["portalid"] != null)
            {
                string raw = queryStringCol["portalid"];
                int portalId;
                if (int.TryParse(raw, out portalId))
                {
                    // 848 : if portal already found is different to portal id in querystring, then load up different alias
                    // this is so the portal settings will be loaded correctly.
                    if (result.PortalId != portalId)
                    {
                        // portal id different to what we expected
                        result.PortalId = portalId;
						Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): result.PortalId → {Arg7}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, result.PortalId);

                        // check the loaded portal alias, because it might be wrong
                        if (result.PortalAlias != null && result.PortalAlias.PortalID != portalId)
                        {
                            // yes, the identified portal alias is wrong.  Find the correct alias for this portal
                            PortalAliasInfo pa = TabIndexController.GetPortalAliasByPortal(portalId, result.DomainName);
							Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): pa → {Pa}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, pa);
                            if (pa != null)
                            {
                                // note: sets portal id and portal alias
                                result.PortalAlias = pa;
								Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): result.PortalAlias → {Arg7}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, result.PortalAlias);
                            }
                        }
                    }
                }
            }
            else
            {
                // check for a portal alias if there's no portal Id in the query string
                // check for absence of captcha value, because the captcha string re-uses the alias querystring value
                if (queryStringCol["alias"] != null && queryStringCol["captcha"] == null)
                {
                    string alias = queryStringCol["alias"];
					Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): alias → {Alias}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, alias);
                    PortalAliasInfo portalAlias = PortalAliasController.Instance.GetPortalAlias(alias);
					Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): portalAlias → {PortalAlias}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, portalAlias);
                    if (portalAlias != null)
                    {
                        // ok the portal alias was found by the alias name
                        // check if the alias contains the domain name
                        if (alias.Contains(result.DomainName) == false)
                        {
                            // replaced to the domain defined in the alias
                            if (request != null)
                            {
                                string redirectDomain = Globals.GetPortalDomainName(alias, request, true);

                                // retVal.Url = redirectDomain;
                                result.FinalUrl = redirectDomain;
								Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): result.FinalUrl → {Arg7}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, result.FinalUrl);
                                result.Action = ActionType.Redirect302Now;
                                result.Reason = RedirectReason.Alias_In_Url;
								Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): result.Reason → {Arg7}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, result.Reason);
                            }
                        }
                        else
                        {
                            // the alias is the same as the current domain
                            result.HttpAlias = portalAlias.HTTPAlias;
							Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): result.HttpAlias → {Arg7}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, result.HttpAlias);
                            result.PortalAlias = portalAlias;
                            result.PortalId = portalAlias.PortalID;

                            // don't use this crap though - we don't want ?alias=portalAlias in our Url
                            if (result.RedirectAllowed)
                            {
                                string redirect = requestUri.Scheme + Uri.SchemeDelimiter + result.PortalAlias.HTTPAlias +
                                                  "/";
                                result.Action = ActionType.Redirect301;
                                result.FinalUrl = redirect;
                                result.Reason = RedirectReason.Unfriendly_Url_Child_Portal;
                            }
                        }
                    }
                }
            }

            // first try and identify the portal using the tabId, but only if we identified this tab by looking up the tabid
            // from the original url
            // 668 : error in child portal redirects to child portal home page because of mismatch in tab/domain name
            if (result.TabId != -1 && !result.FriendlyRewrite)
            {
                // get the alias from the tabid, but only if it is for a tab in that domain
                // 2.0 : change to compare retrieved alias to the already-set httpAlias
                string httpAliasFromTab = PortalAliasController.GetPortalAliasByTab(result.TabId, result.DomainName);
                if (httpAliasFromTab != null)
                {
                    // 882 : account for situation when portalAlias is null.
                    if ((result.PortalAlias != null && !string.Equals(result.PortalAlias.HTTPAlias, httpAliasFromTab, StringComparison.OrdinalIgnoreCase))
                        || result.PortalAlias == null)
                    {
                        // 691 : change logic to force change in portal alias context rather than force back.
                        // This is because the tabid in the query string should take precedence over the portal alias
                        // to handle parent.com/default.aspx?tabid=xx where xx lives in parent.com/child/
                        var tab = TabController.Instance.GetTab(result.TabId, Null.NullInteger, false);
						Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): tab → {Tab}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, tab);

                        // when result alias is null or result alias is different from tab-identified portalAlias
                        if (tab != null && (result.PortalAlias == null || tab.PortalID != result.PortalAlias.PortalID))
                        {
                            // the tabid is different to the identified portalid from the original alias identified
                            // so get a new alias
                            PortalAliasInfo tabPortalAlias = PortalAliasController.Instance.GetPortalAlias(httpAliasFromTab, tab.PortalID);
							Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): tabPortalAlias → {TabPortalAlias}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, tabPortalAlias);
                            if (tabPortalAlias != null)
                            {
                                result.PortalId = tabPortalAlias.PortalID;
                                result.PortalAlias = tabPortalAlias;
                                result.Action = ActionType.CheckFor301;
                                result.Reason = RedirectReason.Wrong_Portal;
                            }
                        }
                    }
                }
            }

            // if no alias, try and set by using the identified http alias or domain name
            if (result.PortalAlias == null)
            {
                if (!string.IsNullOrEmpty(result.HttpAlias))
                {
                    result.PortalAlias = PortalAliasController.Instance.GetPortalAlias(result.HttpAlias);
                }
                else
                {
                    result.PortalAlias = PortalAliasController.Instance.GetPortalAlias(result.DomainName);
                    if (result.PortalAlias == null && result.DomainName.EndsWith("/", StringComparison.Ordinal))
                    {
                        result.DomainName = result.DomainName.TrimEnd('/');
                        result.PortalAlias = PortalAliasController.Instance.GetPortalAlias(result.DomainName);
                    }
                }
            }

            if (result.PortalId == -1)
            {
                if (!requestUri.LocalPath.EndsWith(Globals.glbDefaultPage, StringComparison.InvariantCultureIgnoreCase))
                {
                    // allows requests for aspx pages in custom folder locations to be processed
                    return;
                }

                // the domain name was not found so try using the host portal's first alias
                if (Host.HostPortalID != -1)
                {
                    result.PortalId = Host.HostPortalID;

                    // use the host portal, but replaced to the host portal home page
                    var aliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(result.PortalId).ToList();
					Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): aliases → {Aliases}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, aliases);
                    if (aliases.Count > 0)
                    {
                        string alias = null;

                        // get the alias as the chosen portal alias for the host portal based on the result culture code
                        var cpa = aliases.GetAliasByPortalIdAndSettings(result.PortalId, result, result.CultureCode, settings);
						Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): cpa → {Cpa}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, cpa);
                        if (cpa != null)
                        {
                            alias = cpa.HTTPAlias;
                        }

                        if (alias != null)
                        {
                            result.Action = ActionType.Redirect301;
                            result.Reason = RedirectReason.Host_Portal_Used;
                            string destUrl = MakeUrlWithAlias(requestUri, alias);
							Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): destUrl → {DestUrl}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, destUrl);
                            destUrl = CheckForSiteRootRedirect(alias, destUrl);
                            result.FinalUrl = destUrl;
                        }
                        else
                        {
                            // Get the first Alias for the host portal
                            result.PortalAlias = aliases[result.PortalId];
                            string url = MakeUrlWithAlias(requestUri, result.PortalAlias);
							Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): url → {Url}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, url);
                            if (result.TabId != -1)
                            {
                                url += requestUri.Query;
                            }

                            result.FinalUrl = url;
                            result.Reason = RedirectReason.Host_Portal_Used;
                            result.Action = ActionType.Redirect302Now;
                        }
                    }
                }
            }

            // double check to make sure we still have the correct alias now that all other information is known (ie tab, portal, culture)
            // 770 : redirect alias based on tab id when custom alias used
            if (result.TabId == -1 && result.Action == ActionType.CheckFor301 &&
                result.Reason == RedirectReason.Custom_Tab_Alias)
            {
                // here because the portal alias matched, but no tab was found, and because there are custom tab aliases used for this portal
                // need to redirect back to the chosen portal alias and keep the current path.
                string wrongAlias = result.HttpAlias; // it's a valid alias, but only for certain tabs
                var primaryAliases = PortalAliasController.Instance.GetPortalAliasesByPortalId(result.PortalId).ToList();
				Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): primaryAliases → {PrimaryAliases}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, primaryAliases);
                if (primaryAliases != null && result.PortalId > -1)
                {
                    // going to look for the correct alias based on the culture of the request
                    string requestCultureCode = result.CultureCode;
					Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): requestCultureCode → {RequestCultureCode}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, requestCultureCode);

                    // if that didn't work use the default language of the portal
                    if (requestCultureCode == null)
                    {
                        // this might end up in a double redirect if the path of the Url is for a specific language as opposed
                        // to a path belonging to the default language domain
                        PortalInfo portal = Portals.PortalController.Instance.GetPortal(result.PortalId);
						Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): portal → {Portal}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, portal);
                        if (portal != null)
                        {
                            requestCultureCode = portal.DefaultLanguage;
                        }
                    }

                    // now that the culture code is known, look up the correct portal alias for this portalid/culture code
                    var cpa = primaryAliases.GetAliasByPortalIdAndSettings(result.PortalId, result, requestCultureCode, settings);
                    if (cpa != null)
                    {
                        // if an alias was found that matches the request and the culture code, then run with that
                        string rightAlias = cpa.HTTPAlias;

                        // will cause a redirect to the primary portal alias - we know now that there was no custom alias tab
                        // found, so it's just a plain wrong alias
                        ConfigurePortalAliasRedirect(ref result, wrongAlias, rightAlias, true, settings.InternalAliasList, settings);
                    }
                }
            }
            else
            {
                // then check to make sure it's the chosen portal alias for this portal
                // 627 : don't do it if we're redirecting to the host portal
                if (result.RedirectAllowed && result.Reason != RedirectReason.Host_Portal_Used)
                {
                    string primaryAlias;

                    // checking again in case the rewriting operation changed the values for the valid portal alias
                    bool incorrectAlias = this.IsPortalAliasIncorrect(context, request, requestUri, result, queryStringCol, settings, parentTraceId, out primaryAlias);
					Logger.LogDebug("IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): incorrectAlias → {IncorrectAlias}", context, request, requestUri, result, queryStringCol, settings, parentTraceId, incorrectAlias);
                    if (incorrectAlias)
                    {
                        RedirectPortalAlias(primaryAlias, ref result, settings);
                    }
                }
            }

            // check to see if we have to avoid the core 302 redirect for the portal alias that is in the /defualt.aspx
            // for child portals
            // exception to that is when a custom alias is used but no rewrite has taken place
            if (result.DoRewrite == false && (result.Action == ActionType.Continue
                                              ||
                                              (result.Action == ActionType.CheckFor301 &&
                                               result.Reason == RedirectReason.Custom_Tab_Alias)))
            {
                string aliasQuerystring;
                bool isChildAliasRootUrl = CheckForChildPortalRootUrl(requestUri.AbsoluteUri, result, out aliasQuerystring);
                if (isChildAliasRootUrl)
                {
                    RewriteAsChildAliasRoot(context, result, aliasQuerystring, settings);
                }
            }
			Logger.LogDebug("<<< Exiting IdentifyPortalAlias(context={Context},request={Request},requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId})", context, request, requestUri, result, queryStringCol, settings, parentTraceId);
        }
    }
}
