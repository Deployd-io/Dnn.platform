// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Web.Api.Auth.ApiTokens;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;

    /// <summary>Attribute to authorize apis based on the api token used.</summary>
    public class ApiTokenAuthorizeAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ApiTokenAuthorizeAttribute));
        /// <summary>Initializes a new instance of the <see cref="ApiTokenAuthorizeAttribute"/> class.</summary>
        /// <param name="key">The Key to authenticate api.</param>
        /// <param name="resourceFile">The resource file for the token api.</param>
        /// <param name="scope">The required api token scope.</param>
        public ApiTokenAuthorizeAttribute(string key, string resourceFile, ApiTokenScope scope)
        {
			Logger.LogDebug(">>> Entering ApiTokenAuthorizeAttribute(key={Key},resourceFile={ResourceFile},scope={Scope})", key, resourceFile, scope);
            this.Key = key;
            this.ResourceFile = resourceFile;
			Logger.LogDebug("ApiTokenAuthorizeAttribute(key={Key},resourceFile={ResourceFile},scope={Scope}): this.Key → {Arg3}", key, resourceFile, scope, this.Key);
			Logger.LogDebug("ApiTokenAuthorizeAttribute(key={Key},resourceFile={ResourceFile},scope={Scope}): this.ResourceFile → {Arg3}", key, resourceFile, scope, this.ResourceFile);
            this.Scope = scope;
			Logger.LogDebug("<<< Exiting ApiTokenAuthorizeAttribute(key={Key},resourceFile={ResourceFile},scope={Scope})", key, resourceFile, scope);
        }

        /// <summary>Gets or sets the Key for the api token.</summary>
        public string Key { get; set; }

        /// <summary>Gets or sets the resource file for the api token.</summary>
        public string ResourceFile { get; set; }

        /// <summary>Gets or sets the required api token scope.</summary>
        public ApiTokenScope Scope { get; set; }

        [Dependency]
        private IApiTokenController ApiTokenController { get; set; }

        [Dependency]
        private ApiTokenSettingsRepository ApiTokenSettingsRepository { get; set; }

        /// <summary>Check if the request is authorized.</summary>
        /// <param name="context">The authentication filter context.</param>
        /// <returns><see langword="true"/> if authorized, <see langword="false"/> otherwise.</returns>
        public override bool IsAuthorized(AuthFilterContext context)
        {
			Logger.LogDebug(">>> Entering IsAuthorized(context={Context})", context);
            var token = this.ApiTokenController.GetCurrentThreadApiToken();
            if (token == null)
			Logger.LogWarning("IsAuthorized(context={Context}): token → {Token}", context, token);
            {
                return false;
            }

            if (token.Scope != ApiTokenScope.Host)
            {
                var settings = ApiTokenSettings.GetSettings(this.ApiTokenSettingsRepository, PortalController.Instance.GetCurrentSettings().PortalId);
				Logger.LogDebug("IsAuthorized(context={Context}): settings → {Settings}", context, settings);
                if (!settings.AllowApiTokens)
                {
                    return false;
                }
            }

            var scopeMatch = false;
			Logger.LogDebug("IsAuthorized(context={Context}): scopeMatch → {ScopeMatch}", context, scopeMatch);
            switch (this.Scope)
            {
                case ApiTokenScope.Host:
                    scopeMatch = token.Scope == ApiTokenScope.Host;
                    break;
                case ApiTokenScope.Portal:
                    scopeMatch = token.Scope == ApiTokenScope.Portal || token.Scope == ApiTokenScope.Host;
                    break;
                case ApiTokenScope.User:
                    scopeMatch = token.Scope == ApiTokenScope.User && UserController.Instance.GetCurrentUserInfo() != null;
                    break;
            }

            if (scopeMatch)
            {
			Logger.LogDebug("<<< Exiting IsAuthorized(context={Context})", context);
                return token.TokenKeys.Contains(this.Key.ToLowerInvariant());
            }

            return false;
        }
    }
}
