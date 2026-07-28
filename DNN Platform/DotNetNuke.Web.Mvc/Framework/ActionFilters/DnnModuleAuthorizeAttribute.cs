// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Web.Mvc.Framework.Controllers;

    public class DnnModuleAuthorizeAttribute : AuthorizeAttributeBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private ModuleInfo module;

        public DnnModuleAuthorizeAttribute()
        {
			Logger.LogDebug(">>> Entering DnnModuleAuthorizeAttribute()");
            this.AccessLevel = SecurityAccessLevel.Host;
			Logger.LogDebug("<<< Exiting DnnModuleAuthorizeAttribute()");
			Logger.LogDebug("DnnModuleAuthorizeAttribute(): this.AccessLevel → {Arg0}", this.AccessLevel);
        }

        public SecurityAccessLevel AccessLevel { get; set; }

        public string PermissionKey { get; set; }

        /// <inheritdoc />
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
			Logger.LogDebug(">>> Entering OnAuthorization(filterContext={FilterContext})", filterContext);
            var controller = filterContext.Controller as IDnnController;

            if (controller == null)
            {
                throw new InvalidOperationException("This attribute can only be applied to Controllers that implement IDnnController");
            }

            this.module = controller.ModuleContext.Configuration;
			Logger.LogDebug("OnAuthorization(filterContext={FilterContext}): module → {Module}", filterContext, module);
			Logger.LogDebug("OnAuthorization(filterContext={FilterContext}): this.module → {Arg1}", filterContext, this.module);

            base.OnAuthorization(filterContext);
			Logger.LogDebug("<<< Exiting OnAuthorization(filterContext={FilterContext})", filterContext);
        }

        /// <inheritdoc />
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
			Logger.LogDebug(">>> Entering AuthorizeCore(httpContext={HttpContext})", httpContext);
            if (this.module != null)
            {
                return this.HasModuleAccess();
            }

			Logger.LogDebug("<<< Exiting AuthorizeCore(httpContext={HttpContext})", httpContext);
            return false;
        }

        protected virtual bool HasModuleAccess()
        {
			Logger.LogDebug(">>> Entering HasModuleAccess()");
			Logger.LogDebug("<<< Exiting HasModuleAccess()");
            return ModulePermissionController.HasModuleAccess(this.AccessLevel, this.PermissionKey, this.module);
        }
    }
}
