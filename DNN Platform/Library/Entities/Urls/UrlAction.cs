// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;

    /// <summary>The UrlAction class keeps state of the current Request throughout the rewriting process.</summary>
    public class UrlAction
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UrlAction));
        private List<string> licensedProviders;
        private PortalAliasInfo portalAlias;

        // 829 add in constructor that works around physical path length restriction

        /// <summary>Initializes a new instance of the <see cref="UrlAction"/> class.</summary>
        /// <param name="request">An HTTP request.</param>
        public UrlAction(HttpRequest request)
        {
			Logger.LogError(">>> Entering UrlAction(request={Request})", request);
            this.BrowserType = BrowserTypes.Normal;
			Logger.LogDebug("UrlAction(request={Request}): this.BrowserType → {Arg1}", request, this.BrowserType);
            this.CanRewrite = StateBoolean.NotSet;
            this.Action = ActionType.Continue;
			Logger.LogDebug("UrlAction(request={Request}): this.CanRewrite → {Arg1}", request, this.CanRewrite);
            string physicalPath = string.Empty;
            try
            {
                physicalPath = request.PhysicalPath;
				Logger.LogError("UrlAction(request={Request}): physicalPath → {PhysicalPath}", request, physicalPath);
            }
            catch (PathTooLongException)
            {
                // don't handle exception, but put something into the physical path
                physicalPath = request.ApplicationPath; // will be no file for this location
				Logger.LogError("Exception in UrlAction(request={Request}): {Arg1}", request, PathTooLongException.Message);
				Logger.LogDebug("UrlAction(request={Request}): this.Action → {Arg1}", request, this.Action);
                this.VirtualPath = StateBoolean.True;
            }
            catch (ArgumentException)
            {
                // don't handle exception, but put something into the physical path
				Logger.LogDebug("UrlAction(request={Request}): this.VirtualPath → {Arg1}", request, this.VirtualPath);
                physicalPath = request.ApplicationPath; // will be no file for this location
				Logger.LogError("Exception in UrlAction(request={Request}): {Arg1}", request, ArgumentException.Message);
                this.VirtualPath = StateBoolean.True;
            }
            finally
            {
                this.Constructor(request.Url.Scheme, request.ApplicationPath, physicalPath);
            }
        Logger.LogWarning("<<< Exiting UrlAction(request={Request})", request);
        }

        /// <summary>Initializes a new instance of the <see cref="UrlAction"/> class.</summary>
        /// <param name="scheme">The scheme of a URL.</param>
        /// <param name="applicationPath">The application path of a URL.</param>
        /// <param name="physicalPath">The corresponding physical path.</param>
        public UrlAction(string scheme, string applicationPath, string physicalPath)
        {
			Logger.LogError(">>> Entering UrlAction(scheme={Scheme},applicationPath={ApplicationPath},physicalPath={PhysicalPath})", scheme, applicationPath, physicalPath);
			Logger.LogDebug("UrlAction(scheme={Scheme},applicationPath={ApplicationPath},physicalPath={PhysicalPath}): this.CanRewrite → {Arg3}", scheme, applicationPath, physicalPath, this.CanRewrite);
            this.BrowserType = BrowserTypes.Normal;
			Logger.LogWarning("<<< Exiting UrlAction(scheme={Scheme},applicationPath={ApplicationPath},physicalPath={PhysicalPath})", scheme, applicationPath, physicalPath);
			Logger.LogDebug("UrlAction(scheme={Scheme},applicationPath={ApplicationPath},physicalPath={PhysicalPath}): this.BrowserType → {Arg3}", scheme, applicationPath, physicalPath, this.BrowserType);
            this.CanRewrite = StateBoolean.NotSet;
            this.Action = ActionType.Continue;
            this.Constructor(scheme, applicationPath, physicalPath);
        }

        public Uri Url { get; set; }

        public bool DoRewrite { get; set; }

        public bool FriendlyRewrite { get; set; }

        // friendlyRewrite means it was rewritten without looking up the tabid in the url
        public bool BypassCachedDictionary { get; set; }

        public string RewritePath { get; set; }

        public string RawUrl { get; set; }

        public string DebugData { get; set; }

        public string PhysicalPath { get; set; }

        public StateBoolean VirtualPath { get; set; }

        public string ApplicationPath { get; set; }

        public bool RebuildRequested { get; set; }

        public string FinalUrl { get; set; }

        public string Scheme { get; set; }

        public bool IsSecureConnection { get; set; }

        public bool IsSSLOffloaded { get; set; }

        public string DomainName { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Breaking change")]
        public Exception Ex { get; set; }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        public string dictKey { get; set; }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        public string dictVal { get; set; }

        public List<string> DebugMessages { get; set; }

        public int TabId { get; set; }

        public int PortalId { get; set; }

        public RedirectReason Reason { get; set; }

        public string HttpAlias { get; set; }

        public ActionType Action { get; set; }

        public string CultureCode { get; set; }

        public string OriginalPath { get; private set; }

        public string OriginalPathNoAlias { get; private set; }

        public bool RedirectAllowed { get; private set; }

        public StateBoolean CanRewrite { get; set; }

        // the alias for the current request
        public PortalAliasInfo PortalAlias
        {
            get
            {
                return this.portalAlias;
            }

            set
            {
                if (value != null)
                {
                    this.PortalId = value.PortalID;
                    this.HttpAlias = value.HTTPAlias;
                }

                this.portalAlias = value;
            }
        }

        // the primary alias, if different to the current alias
        public PortalAliasInfo PrimaryAlias { get; set; }

        public DotNetNuke.Entities.Portals.PortalSettings.PortalAliasMapping PortalAliasMapping { get; set; }

        public bool CustomParmRewrite { get; set; }

        // 737 : mobile browser identification
#pragma warning disable CS0618 // Type or member is obsolete
        public BrowserTypes BrowserType { get; private set; }
#pragma warning restore CS0618 // Type or member is obsolete

        public bool IsPhysicalResource { get; set; }

        public string UnlicensedProviderMessage { get; set; }

        public bool UnlicensedProviderCalled { get; set; }

        /// <summary>
        /// Sets the action value, but checks to ensure that the action is
        /// not being 'downgraded' (example: cannot set 'Redirect301' to 'CheckFor301').
        /// </summary>
        /// <param name="newAction">The new action type.</param>
        public void SetActionWithNoDowngrade(ActionType newAction)
        {
			Logger.LogError(">>> Entering SetActionWithNoDowngrade(newAction={NewAction})", newAction);
            switch (newAction)
            {
                case ActionType.CheckFor301:
                    if (this.Action != ActionType.Redirect301
                        && this.Action != ActionType.Redirect302
                        && this.Action != ActionType.Redirect302Now
                        && this.Action != ActionType.Output404)
                    {
                        this.Action = newAction;
						Logger.LogDebug("SetActionWithNoDowngrade(newAction={NewAction}): this.Action → {Arg1}", newAction, this.Action);
                    }

                    break;
                default:
                    this.Action = newAction;
                    break;
            }
			Logger.LogWarning("<<< Exiting SetActionWithNoDowngrade(newAction={NewAction})", newAction);
        }

        public void AddLicensedProviders(List<string> licensedProviders)
        {
			Logger.LogError(">>> Entering AddLicensedProviders(licensedProviders={LicensedProviders})", licensedProviders);
            if (this.licensedProviders == null)
            {
                this.licensedProviders = new List<string>();
				Logger.LogDebug("AddLicensedProviders(licensedProviders={LicensedProviders}): licensedProviders → {LicensedProviders1}", licensedProviders, licensedProviders);
            }

            foreach (string lp in licensedProviders)
            {
                if (this.licensedProviders.Contains(lp.ToLowerInvariant()) == false)
                {
                    this.licensedProviders.Add(lp.ToLowerInvariant());
                }
            }
			Logger.LogWarning("<<< Exiting AddLicensedProviders(licensedProviders={LicensedProviders})", licensedProviders);
        }

        public void AddLicensedProvider(string providerName)
        {
			Logger.LogError(">>> Entering AddLicensedProvider(providerName={ProviderName})", providerName);
            if (this.licensedProviders == null)
            {
                this.licensedProviders = new List<string>();
				Logger.LogDebug("AddLicensedProvider(providerName={ProviderName}): licensedProviders → {LicensedProviders}", providerName, licensedProviders);
            }

            if (this.licensedProviders.Contains(providerName.ToLowerInvariant()) == false)
            {
                this.licensedProviders.Add(providerName.ToLowerInvariant());
            }
			Logger.LogWarning("<<< Exiting AddLicensedProvider(providerName={ProviderName})", providerName);
        }

        public bool IsProviderLicensed(string providerName)
        {
			Logger.LogError(">>> Entering IsProviderLicensed(providerName={ProviderName})", providerName);
            if (this.licensedProviders == null)
            {
                return false;
            }

			Logger.LogWarning("<<< Exiting IsProviderLicensed(providerName={ProviderName})", providerName);
            return this.licensedProviders.Contains(providerName.ToLowerInvariant());
        }

        /// <summary>Copies the original request path to the OriginalPath variables (originalPath, originalPathNoAlias).</summary>
        /// <param name="path">The original path.</param>
        /// <param name="settings">The URL settings.</param>
        public void SetOriginalPath(string path, FriendlyUrlSettings settings)
        {
			Logger.LogError(">>> Entering SetOriginalPath(path={Path},settings={Settings})", path, settings);
            this.OriginalPath = path;
			Logger.LogDebug("SetOriginalPath(path={Path},settings={Settings}): this.OriginalPath → {Arg2}", path, settings, this.OriginalPath);
            this.OriginalPathNoAlias = path;
            if (!string.IsNullOrEmpty(this.HttpAlias) && path.Contains(this.HttpAlias))
            {
                this.OriginalPathNoAlias = path.Substring(path.IndexOf(this.HttpAlias, StringComparison.Ordinal) + this.HttpAlias.Length);
				Logger.LogDebug("SetOriginalPath(path={Path},settings={Settings}): this.OriginalPathNoAlias → {Arg2}", path, settings, this.OriginalPathNoAlias);
            }
        }

        public void SetBrowserType(HttpRequest request, HttpResponse response, FriendlyUrlSettings settings)
        {
			Logger.LogError(">>> Entering SetBrowserType(request={Request},response={Response},settings={Settings})", request, response, settings);
            // set the mobile browser type
            if (request != null && response != null && settings != null)
            {
                this.BrowserType = FriendlyUrlController.GetBrowserType(request, response, settings);
            }
			Logger.LogWarning("<<< Exiting SetBrowserType(request={Request},response={Response},settings={Settings})", request, response, settings);
        }

        public void SetRedirectAllowed(string path, FriendlyUrlSettings settings)
        {
			Logger.LogError(">>> Entering SetRedirectAllowed(path={Path},settings={Settings})", path, settings);
            try
            {
                if (!string.IsNullOrEmpty(settings.DoNotRedirectRegex))
                {
                    // if a regex match, redirect Not allowed
                    var doNotRedirectRegex = RegexUtils.GetCachedRegex(settings.DoNotRedirectRegex, RegexOptions.IgnoreCase);
					Logger.LogError("SetRedirectAllowed(path={Path},settings={Settings}): doNotRedirectRegex → {DoNotRedirectRegex}", path, settings, doNotRedirectRegex);
                    this.RedirectAllowed = !doNotRedirectRegex.IsMatch(path);
                }
                else
                {
                    this.RedirectAllowed = true;
                }
				Logger.LogDebug("SetRedirectAllowed(path={Path},settings={Settings}): this.RedirectAllowed → {Arg2}", path, settings, this.RedirectAllowed);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in SetRedirectAllowed(path={Path},settings={Settings}): {Arg2}", path, settings, ex.Message);
                this.RedirectAllowed = true; // default : true, unless regex allows it.  So if regex causes an exception

                // then we should allow the redirect
                UrlRewriterUtils.LogExceptionInRequest(ex, "Not Set", this);
				Logger.LogWarning("<<< Exiting SetRedirectAllowed(path={Path},settings={Settings})", path, settings);
                this.Ex = ex;
				Logger.LogDebug("SetRedirectAllowed(path={Path},settings={Settings}): this.Ex → {Arg2}", path, settings, this.Ex);
            }
        }

        private void Constructor(string scheme, string applicationPath, string physicalPath)
        {
			Logger.LogError(">>> Entering Constructor(scheme={Scheme},applicationPath={ApplicationPath},physicalPath={PhysicalPath})", scheme, applicationPath, physicalPath);
            if (!scheme.EndsWith("://", StringComparison.Ordinal))
            {
				Logger.LogWarning("<<< Exiting Constructor(scheme={Scheme},applicationPath={ApplicationPath},physicalPath={PhysicalPath})", scheme, applicationPath, physicalPath);
                this.Scheme = scheme + "://";
            }
            else
            {
                this.Scheme = scheme;
				Logger.LogDebug("Constructor(scheme={Scheme},applicationPath={ApplicationPath},physicalPath={PhysicalPath}): this.Scheme → {Arg3}", scheme, applicationPath, physicalPath, this.Scheme);
            }

			Logger.LogDebug("Constructor(scheme={Scheme},applicationPath={ApplicationPath},physicalPath={PhysicalPath}): this.PortalId → {Arg3}", scheme, applicationPath, physicalPath, this.PortalId);
            this.ApplicationPath = applicationPath;
			Logger.LogDebug("Constructor(scheme={Scheme},applicationPath={ApplicationPath},physicalPath={PhysicalPath}): this.ApplicationPath → {Arg3}", scheme, applicationPath, physicalPath, this.ApplicationPath);
            string domainPath = applicationPath.Replace(scheme, string.Empty);
            this.DomainName = domainPath.Contains("/") ? domainPath.Substring(0, domainPath.IndexOf('/')) : domainPath;
            this.PhysicalPath = physicalPath;
			Logger.LogDebug("Constructor(scheme={Scheme},applicationPath={ApplicationPath},physicalPath={PhysicalPath}): this.PhysicalPath → {Arg3}", scheme, applicationPath, physicalPath, this.PhysicalPath);
            this.PortalId = -1;
            this.TabId = -1;
            this.Reason = RedirectReason.Not_Redirected;
            this.FriendlyRewrite = false;
            this.BypassCachedDictionary = false;
			Logger.LogDebug("Constructor(scheme={Scheme},applicationPath={ApplicationPath},physicalPath={PhysicalPath}): this.TabId → {Arg3}", scheme, applicationPath, physicalPath, this.TabId);
			Logger.LogDebug("Constructor(scheme={Scheme},applicationPath={ApplicationPath},physicalPath={PhysicalPath}): this.Reason → {Arg3}", scheme, applicationPath, physicalPath, this.Reason);
			Logger.LogDebug("Constructor(scheme={Scheme},applicationPath={ApplicationPath},physicalPath={PhysicalPath}): this.FriendlyRewrite → {Arg3}", scheme, applicationPath, physicalPath, this.FriendlyRewrite);
            this.VirtualPath = StateBoolean.NotSet;
            this.IsSecureConnection = false;
            this.IsSSLOffloaded = false;
            this.DebugMessages = new List<string>();
            this.CultureCode = null;
        }
    }
}
