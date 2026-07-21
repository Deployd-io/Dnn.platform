// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System.Net.Http;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;

    /// <summary>Determines web API authorization based on module permissions.</summary>
    public class DnnModuleAuthorizeAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="DnnModuleAuthorizeAttribute"/> class.</summary>
        public DnnModuleAuthorizeAttribute()
        {
			Logger.LogDebug(">>> Entering DnnModuleAuthorizeAttribute()");
            this.AccessLevel = SecurityAccessLevel.Host;
			Logger.LogDebug("<<< Exiting DnnModuleAuthorizeAttribute()");
			Logger.LogDebug("DnnModuleAuthorizeAttribute(): this.AccessLevel → {Arg0}", this.AccessLevel);
        }

        /// <summary>Gets or sets a custom permission key to check for access.</summary>
        public string PermissionKey { get; set; }

        /// <summary>Gets or sets an access level to verify the user has.</summary>
        public SecurityAccessLevel AccessLevel { get; set; }

        /// <inheritdoc />
        public override bool IsAuthorized(AuthFilterContext context)
        {
			Logger.LogDebug(">>> Entering IsAuthorized(context={Context})", context);
            var activeModule = this.FindModuleInfo(context.ActionContext.Request);

			Logger.LogDebug("IsAuthorized(context={Context}): activeModule → {ActiveModule}", context, activeModule);
            if (activeModule != null)
            {
                return ModulePermissionController.HasModuleAccess(this.AccessLevel, this.PermissionKey, activeModule);
            }

			Logger.LogDebug("<<< Exiting IsAuthorized(context={Context})", context);
            return false;
        }

        /// <summary>Finds the module associated with the request.</summary>
        /// <param name="request">The web API request.</param>
        /// <returns>A <see cref="ModuleInfo"/> instance or <see langword="null"/>.</returns>
        protected virtual ModuleInfo FindModuleInfo(HttpRequestMessage request)
        {
			Logger.LogDebug(">>> Entering FindModuleInfo(request={Request})", request);
			Logger.LogDebug("<<< Exiting FindModuleInfo(request={Request})", request);
            return request.FindModuleInfo();
        }
    }
}
