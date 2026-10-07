// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;

    internal class RedirectController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RedirectController));
        /// <summary>Cancels a redirect.</summary>
        /// <param name="result">The URL action to update.</param>
        /// <param name="context">The HTTP context.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <param name="message">The message to add to <see cref="UrlAction.DebugMessages"/>.</param>
        internal static void CancelRedirect(ref UrlAction result, HttpContext context, FriendlyUrlSettings settings, string message)
        {
			Logger.LogError(">>> Entering CancelRedirect(result={Result},context={Context},settings={Settings},message={Message})", result, context, settings, message);
            result.Action = ActionType.Continue;
            result.Reason = RedirectReason.Not_Redirected;
			Logger.LogDebug("CancelRedirect(result={Result},context={Context},settings={Settings},message={Message}): result.Reason → {Arg4}", result, context, settings, message, result.Reason);
			Logger.LogDebug("CancelRedirect(result={Result},context={Context},settings={Settings},message={Message}): result.Action → {Arg4}", result, context, settings, message, result.Action);
            result.FinalUrl = null;
			Logger.LogDebug("CancelRedirect(result={Result},context={Context},settings={Settings},message={Message}): result.FinalUrl → {Arg4}", result, context, settings, message, result.FinalUrl);

            // clean the path for the rewrite
            NameValueCollection queryString = null;
			Logger.LogError("CancelRedirect(result={Result},context={Context},settings={Settings},message={Message}): queryString → {QueryString}", result, context, settings, message, queryString);
            if (context != null)
            {
                queryString = context.Request.QueryString;
            }

            result.RewritePath = RedirectTokens.RemoveAnyRedirectTokens(result.RewritePath, queryString);

            // redo the rewrite to fix up the problem.  The user has ticked 'permanent redirect' but hasn't supplied a forwarding Url
            if (context != null)
            {
                // if no context supplied, means no rewrite was required because querystring didn't contain do301 action
                ////RewriterUtils.RewriteUrl(context, result.RewritePath, settings.RebaseClientPath);
                RewriterUtils.RewriteUrl(context, result.RewritePath);
            }

            result.DebugMessages.Add(message);
			Logger.LogWarning("<<< Exiting CancelRedirect(result={Result},context={Context},settings={Settings},message={Message})", result, context, settings, message);
        }

        /// <summary>Checks for a redirect based on a module friendly url provider rule.</summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="result">The URL action to update.</param>
        /// <param name="queryStringCol">The query string.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <param name="parentTraceId">The parent trace ID.</param>
        /// <returns><see langword="true"/> if the <paramref name="result"/> is a redirect, otherwise <see langword="false"/>.</returns>
        internal static bool CheckForModuleProviderRedirect(
            Uri requestUri,
            ref UrlAction result,
            NameValueCollection queryStringCol,
            FriendlyUrlSettings settings,
            Guid parentTraceId)
        {
			Logger.LogError(">>> Entering CheckForModuleProviderRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId})", requestUri, result, queryStringCol, settings, parentTraceId);
            var messages = new List<string>();
            string location;
			Logger.LogError("CheckForModuleProviderRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): messages → {Messages}", requestUri, result, queryStringCol, settings, parentTraceId, messages);
            bool redirected = ExtensionUrlProviderController.CheckForRedirect(
                requestUri,
                result,
                queryStringCol,
                settings,
                out location,
                ref messages,
                parentTraceId);
            if (messages != null)
            {
                result.DebugMessages.AddRange(messages);
            }

            if (redirected)
            {
                result.FinalUrl = location;
				Logger.LogDebug("CheckForModuleProviderRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): result.FinalUrl → {Arg5}", requestUri, result, queryStringCol, settings, parentTraceId, result.FinalUrl);
				Logger.LogDebug("CheckForModuleProviderRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): result.Reason → {Arg5}", requestUri, result, queryStringCol, settings, parentTraceId, result.Reason);
                result.Action = ActionType.Redirect301;
				Logger.LogDebug("CheckForModuleProviderRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId}): result.Action → {Arg5}", requestUri, result, queryStringCol, settings, parentTraceId, result.Action);
                result.Reason = RedirectReason.Custom_Redirect;
            }

			Logger.LogWarning("<<< Exiting CheckForModuleProviderRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings},parentTraceId={ParentTraceId})", requestUri, result, queryStringCol, settings, parentTraceId);
            return redirected;
        }

        internal static bool CheckForParameterRedirect(
            Uri requestUri,
            ref UrlAction result,
            NameValueCollection queryStringCol,
            FriendlyUrlSettings settings)
        {
			Logger.LogError(">>> Entering CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings})", requestUri, result, queryStringCol, settings);
            // check for parameter replaced works by inspecting the parameters on a rewritten request, comparing
            // them agains the list of regex expressions on the friendlyurls.config file, and redirecting to the same page
            // but with new parameters, if there was a match
            bool redirect = false;
			Logger.LogError("CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): redirect → {Redirect}", requestUri, result, queryStringCol, settings, redirect);

            // get the redirect actions for this portal
            var messages = new List<string>();
			Logger.LogError("CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): messages → {Messages}", requestUri, result, queryStringCol, settings, messages);
            Dictionary<int, List<ParameterRedirectAction>> redirectActions = CacheController.GetParameterRedirects(settings, result.PortalId, ref messages);
            if (redirectActions != null && redirectActions.Count > 0)
            {
                try
                {
                    string rewrittenUrl = result.RewritePath ?? result.RawUrl;

                    List<ParameterRedirectAction> parmRedirects = null;

                    // find the matching redirects for the tabid
                    int tabId = result.TabId;
					Logger.LogError("CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): tabId → {TabId}", requestUri, result, queryStringCol, settings, tabId);
                    if (tabId > -1)
                    {
                        if (redirectActions.TryGetValue(tabId, out var parameterRedirectActions))
                        {
                            // find the right set of replaced actions for this tab
                            parmRedirects = parameterRedirectActions;
							Logger.LogError("CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): parmRedirects → {ParmRedirects}", requestUri, result, queryStringCol, settings, parmRedirects);
                        }
                    }

                    // check for 'all tabs' redirections
                    if (redirectActions.TryGetValue(-1, out var allRedirects))
                    {
                        // -1 means 'all tabs' - rewriting across all tabs
                        // initialise to empty collection if there are no specific tab redirects
                        if (parmRedirects == null)
                        {
                            parmRedirects = [];
                        }

                        // add in the all redirects
                        parmRedirects.AddRange(allRedirects); // add the 'all' range to the tab range
                        tabId = result.TabId;
                    }

                    if (redirectActions.TryGetValue(-2, out var defaultRedirects) && result.OriginalPath.ToLowerInvariant().Contains("default.aspx"))
                    {
                        // for the default.aspx page
                        parmRedirects ??= [];

                        parmRedirects.AddRange(defaultRedirects); // add the default.aspx redirects to the list
                        tabId = result.TabId;
                    }

                    // 726 : allow for site-root redirects, ie redirects where no page match
                    if (redirectActions.TryGetValue(-3, out var siteRootRedirects))
                    {
                        // request is for site root
                        parmRedirects ??= [];

                        parmRedirects.AddRange(siteRootRedirects); // add the site root redirects to the collection
                    }

                    // OK what we have now is a list of redirects for the currently requested tab (either because it was specified by tab id,
                    // or because there is a replaced for 'all tabs'
                    if (parmRedirects is { Count: > 0 } && rewrittenUrl != null)
                    {
                        foreach (ParameterRedirectAction parmRedirect in parmRedirects)
                        {
                            // regex test each replaced to see if there is a match between the parameter string
                            // and the parmRedirect
                            string compareWith = rewrittenUrl;
							Logger.LogError("CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): compareWith → {CompareWith}", requestUri, result, queryStringCol, settings, compareWith);
                            var redirectRegex = RegexUtils.GetCachedRegex(
                                parmRedirect.LookFor,
                                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                            Match regexMatch = redirectRegex.Match(compareWith);
							Logger.LogError("CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): regexMatch → {RegexMatch}", requestUri, result, queryStringCol, settings, regexMatch);
                            bool success = regexMatch.Success;
							Logger.LogError("CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): success → {Success}", requestUri, result, queryStringCol, settings, success);
                            bool siteRootTried = false;
							Logger.LogError("CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): siteRootTried → {SiteRootTried}", requestUri, result, queryStringCol, settings, siteRootTried);

                            // if no match, but there is a site root redirect to try
                            if (!success && parmRedirect.TabId == -3)
                            {
                                siteRootTried = true;
                                compareWith = result.OriginalPathNoAlias;
                                regexMatch = redirectRegex.Match(compareWith);
                                success = regexMatch.Success;
                            }

                            if (!success)
                            {
                                result.DebugMessages.Add(parmRedirect.Name + " redirect not matched (" + rewrittenUrl +
                                                         ")");
                                if (siteRootTried)
                                {
                                    result.DebugMessages.Add(parmRedirect.Name + " redirect not matched [site root] (" +
                                                             result.OriginalPathNoAlias + ")");
                                }
                            }
                            else
                            {
                                // success! there was a match in the parameters
                                string parms = redirectRegex.Replace(compareWith, parmRedirect.RedirectTo);
								Logger.LogError("CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): parms → {Parms}", requestUri, result, queryStringCol, settings, parms);
                                if (siteRootTried)
                                {
                                    result.DebugMessages.Add(parmRedirect.Name + " redirect matched [site root] with (" +
                                                             result.OriginalPathNoAlias + "), replaced with " + parms);
                                }
                                else
                                {
                                    result.DebugMessages.Add(parmRedirect.Name + " redirect matched with (" +
                                                             compareWith + "), replaced with " + parms);
                                }

                                string finalUrl = string.Empty;
								Logger.LogError("CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): finalUrl → {FinalUrl}", requestUri, result, queryStringCol, settings, finalUrl);

                                // now we need to generate the friendly Url

                                // first check to see if the parameter replacement string has a destination tabid specified
                                if (parms.ToLowerInvariant().Contains("tabid/"))
                                {
                                    // if so, using a feature whereby the dest tabid can be changed within the parameters, which will
                                    // redirect the page as well as redirecting the parameter values
                                    string[] parmParts = parms.Split('/');
                                    bool tabIdNext = false;
									Logger.LogError("CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): tabIdNext → {TabIdNext}", requestUri, result, queryStringCol, settings, tabIdNext);
                                    foreach (string parmPart in parmParts)
                                    {
                                        if (tabIdNext)
                                        {
                                            // changes the tabid of page, effects a page redirect along with a parameter redirect
                                            if (int.TryParse(parmPart, out tabId))
                                            {
                                                parms = parms.Replace("tabid/" + tabId.ToString(CultureInfo.InvariantCulture), string.Empty);
                                            }

                                            // remove the tabid/xx from the path
                                            break; // that's it, we're finished
                                        }

                                        if (parmPart.Equals("tabid", StringComparison.OrdinalIgnoreCase))
                                        {
                                            tabIdNext = true;
                                        }
                                    }
                                }
                                else if (tabId == -1)
                                {
                                    // find the home tabid for this portal
                                    // 735 : switch to custom method for getting portal
                                    PortalInfo portal = CacheController.GetPortal(result.PortalId, true);
									Logger.LogError("CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): portal → {Portal}", requestUri, result, queryStringCol, settings, portal);
                                    tabId = portal.HomeTabId;
                                }

                                if (parmRedirect.ChangeToSiteRoot)
                                {
                                    // when change to siteroot requested, new path goes directly off the portal alias
                                    // so set the finalUrl as the poratl alias
                                    finalUrl = result.Scheme + result.HttpAlias + "/";
                                }
                                else
                                {
                                    // if the tabid has been supplied, do a friendly url provider lookup to get the correct format for the tab url
                                    if (tabId > -1)
                                    {
                                        TabInfo tab = TabController.Instance.GetTab(tabId, result.PortalId, false);
										Logger.LogError("CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): tab → {Tab}", requestUri, result, queryStringCol, settings, tab);
                                        if (tab != null)
                                        {
                                            string path = Globals.glbDefaultPage + TabIndexController.CreateRewritePath(tab.TabID, string.Empty);
                                            string friendlyUrlNoParms = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(
                                                tab,
                                                path,
                                                Globals.glbDefaultPage,
                                                result.HttpAlias,
                                                false,
                                                settings,
                                                Guid.Empty);
                                            if (!friendlyUrlNoParms.EndsWith("/", StringComparison.Ordinal))
                                            {
                                                friendlyUrlNoParms += "/";
                                            }

                                            finalUrl = friendlyUrlNoParms;
                                        }

                                        if (tab == null)
                                        {
                                            result.DebugMessages.Add(parmRedirect.Name +
                                                                     " tabId in redirect rule (tabId:" +
                                                                     tabId.ToString(CultureInfo.InvariantCulture) + ", portalId:" +
                                                                     result.PortalId.ToString(CultureInfo.InvariantCulture) +
                                                                     " ), tab was not found");
                                        }
                                        else
                                        {
                                            result.DebugMessages.Add(parmRedirect.Name +
                                                                     " tabId in redirect rule (tabId:" +
                                                                     tabId.ToString(CultureInfo.InvariantCulture) + ", portalId:" +
                                                                     result.PortalId.ToString(CultureInfo.InvariantCulture) + " ), tab found : " +
                                                                     tab.TabName);
                                        }
                                    }
                                }

                                if (parms.StartsWith("//", StringComparison.Ordinal))
                                {
                                    parms = parms.Substring(2);
                                }

                                if (parms.StartsWith("/", StringComparison.Ordinal))
                                {
                                    parms = parms.Substring(1);
                                }

                                if (settings.PageExtensionUsageType != PageExtensionUsageType.Never)
                                {
                                    if (parms.EndsWith("/", StringComparison.Ordinal))
                                    {
                                        parms = parms.TrimEnd('/');
                                    }

                                    if (parms.Length > 0)
                                    {
                                        // we are adding more parms onto the end, so remove the page extension
                                        // from the parameter list
                                        // 946 : exception when settings.PageExtension value is empty
                                        parms += settings.PageExtension;

                                        // 816: if page extension is /, then don't do this
                                        if (settings.PageExtension != "/" &&
                                            string.IsNullOrEmpty(settings.PageExtension) == false)
                                        {
                                            finalUrl = finalUrl.Replace(settings.PageExtension, string.Empty);
                                        }
                                    }
                                    else
                                    {
                                        // we are removing all the parms altogether, so
                                        // the url needs to end in the page extension only
                                        // 816: if page extension is /, then don't do this
                                        if (settings.PageExtension != "/" &&
                                            string.IsNullOrEmpty(settings.PageExtension) == false)
                                        {
                                            finalUrl = finalUrl.Replace(
                                                settings.PageExtension + "/",
                                                settings.PageExtension);
                                        }
                                    }
                                }

                                // put the replaced parms back on the end
                                finalUrl += parms;

                                // set the final url
                                result.FinalUrl = finalUrl;
								Logger.LogDebug("CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): result.FinalUrl → {Arg4}", requestUri, result, queryStringCol, settings, result.FinalUrl);
                                result.Reason = RedirectReason.Custom_Redirect;
								Logger.LogDebug("CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): result.Reason → {Arg4}", requestUri, result, queryStringCol, settings, result.Reason);
                                switch (parmRedirect.Action)
                                {
                                    case "301":
                                        result.Action = ActionType.Redirect301;
										Logger.LogDebug("CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): result.Action → {Arg4}", requestUri, result, queryStringCol, settings, result.Action);
                                        break;
                                    case "302":
                                        result.Action = ActionType.Redirect302;
                                        break;
                                    case "404":
                                        result.Action = ActionType.Output404;
                                        break;
                                }

                                redirect = true;
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
					Logger.LogError("Exception in CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings}): {Arg4}", requestUri, result, queryStringCol, settings, ex.Message);
                    Services.Exceptions.Exceptions.LogException(ex);
                    messages.Add("Exception: " + ex.Message + "\n" + ex.StackTrace);
                }
                finally
                {
                    if (messages.Count > 0)
                    {
                        result.DebugMessages.AddRange(messages);
                    }
                }
            }

			Logger.LogWarning("<<< Exiting CheckForParameterRedirect(requestUri={RequestUri},result={Result},queryStringCol={QueryStringCol},settings={Settings})", requestUri, result, queryStringCol, settings);
            return redirect;
        }

        /// <summary>
        /// Gets a redirect Url for when the tab has a specified external Url that is of type 'TabType.Tab'.  This covers both
        /// 'PermanentRedirect' and 'ExternalUrl' scenarios, where the settings are to redirect the value.
        /// </summary>
        /// <param name="tab">The tab info.</param>
        /// <param name="settings">The friendly URL settings.</param>
        /// <param name="cleanPath">The cleaned path.</param>
        /// <param name="result">The URL action.</param>
        /// <param name="permRedirect">Whether it's a permanent redirect.</param>
        /// <param name="parentTraceId">The parent trace ID.</param>
        /// <returns>The friendly URL or <see langword="null"/>.</returns>
        /// <remarks>823 : Moved from CheckForRedirects to allow call earlier in pipeline.</remarks>
        internal static string GetTabRedirectUrl(
            TabInfo tab,
            FriendlyUrlSettings settings,
            string cleanPath,
            UrlAction result,
            out bool permRedirect,
            Guid parentTraceId)
        {
            string bestFriendlyUrl = null;

            // 592 : check for permanent redirect
            permRedirect = tab.PermanentRedirect;
            int redirectTabId;
            if (int.TryParse(tab.Url, out redirectTabId))
            {
                // ok, redirecting to a new tab, specified by the tabid in the Url field
                TabInfo redirectTab = TabController.Instance.GetTab(redirectTabId, tab.PortalID, false);
                if (redirectTab != null)
                {
                    // now get the friendly url for that tab
                    bestFriendlyUrl = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(
                        redirectTab,
                        cleanPath,
                        Globals.glbDefaultPage,
                        result.HttpAlias,
                        false,
                        settings,
                        parentTraceId);
                }
            }
            else
            {
                // use the url, as specified in the dnn tab url property
                // 776 : when no url, don't redirect
                if (tab.Url != string.Empty)
                {
                    bestFriendlyUrl = tab.Url;
                }
                else
                {
                    // 776: no actual Url for the 'perm redirect'.
                    // this is a mistake on the part of the person who created the perm redirect
                    // but don't create a redirect or there will be a terminal loop
                    permRedirect = false;
                }
            }

			Logger.LogWarning("<<< Exiting GetTabRedirectUrl(tab={Tab},settings={Settings},cleanPath={CleanPath},result={Result},parentTraceId={ParentTraceId})", tab, settings, cleanPath, result, parentTraceId);
            return bestFriendlyUrl;
        }
    }
}
