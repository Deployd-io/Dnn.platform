// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    using System;
    using System.Threading;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Web.Mvc.Framework.Controllers;

    public class RequireHostAttribute : AuthorizeAttributeBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RequireHostAttribute));
        private UserInfo user;

        /// <inheritdoc />
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
			Logger.LogError(">>> Entering OnAuthorization(filterContext={FilterContext})", filterContext);
            var controller = filterContext.Controller as IDnnController;

            if (controller == null)
            {
				Logger.LogError("Throwing InvalidOperationException from OnAuthorization(filterContext={FilterContext})", filterContext);
                throw new InvalidOperationException("This attribute can only be applied to Controllers that implement IDnnController");
            }

            this.user = controller.ModuleContext.PortalSettings.UserInfo;
			Logger.LogDebug("OnAuthorization(filterContext={FilterContext}): user → {User}", filterContext, user);
			Logger.LogDebug("OnAuthorization(filterContext={FilterContext}): this.user → {Arg1}", filterContext, this.user);

            base.OnAuthorization(filterContext);
			Logger.LogWarning("<<< Exiting OnAuthorization(filterContext={FilterContext})", filterContext);
        }

        /// <inheritdoc />
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
			Logger.LogError(">>> Entering AuthorizeCore(httpContext={HttpContext})", httpContext);
            var principal = Thread.CurrentPrincipal;
            if (!principal.Identity.IsAuthenticated)
            {
                return false;
            }

            if (this.user != null)
            {
                return this.user.IsSuperUser;
            }

			Logger.LogWarning("<<< Exiting AuthorizeCore(httpContext={HttpContext})", httpContext);
            return false;
        }
    }
}
