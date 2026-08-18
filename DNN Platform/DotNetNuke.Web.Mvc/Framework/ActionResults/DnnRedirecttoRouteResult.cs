// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.ActionResults
{
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
    using DotNetNuke.Web.Mvc.Helpers;
    using Microsoft.Extensions.DependencyInjection;

    internal class DnnRedirecttoRouteResult : RedirectToRouteResult
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnRedirecttoRouteResult));
        public DnnRedirecttoRouteResult(string actionName, string controllerName, string routeName, RouteValueDictionary routeValues, bool permanent)
            : base(routeName, routeValues, permanent)
        {
			Logger.LogDebug(">>> Entering DnnRedirecttoRouteResult(actionName={ActionName},controllerName={ControllerName},routeName={RouteName},routeValues={RouteValues},permanent={Permanent})", actionName, controllerName, routeName, routeValues, permanent);
            this.NavigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
            this.ActionName = actionName;
			Logger.LogDebug("DnnRedirecttoRouteResult(actionName={ActionName},controllerName={ControllerName},routeName={RouteName},routeValues={RouteValues},permanent={Permanent}): this.ActionName → {Arg5}", actionName, controllerName, routeName, routeValues, permanent, this.ActionName);
			Logger.LogDebug("<<< Exiting DnnRedirecttoRouteResult(actionName={ActionName},controllerName={ControllerName},routeName={RouteName},routeValues={RouteValues},permanent={Permanent})", actionName, controllerName, routeName, routeValues, permanent);
            this.ControllerName = controllerName;
        }

        public DnnRedirecttoRouteResult(string actionName, string controllerName, string routeName, RouteValueDictionary routeValues, bool permanent, DnnUrlHelper url)
            : this(actionName, controllerName, routeName, routeValues, permanent)
        {
            this.Url = url;
        }

        public DnnUrlHelper Url { get; private set; }

        public string ActionName { get; private set; }

        public string ControllerName { get; private set; }

        protected INavigationManager NavigationManager { get; }

        /// <inheritdoc />
        public override void ExecuteResult(ControllerContext context)
        {
			Logger.LogDebug(">>> Entering ExecuteResult(context={Context})", context);
            Requires.NotNull("context", context);

            Guard.Against(context.IsChildAction, "Cannot Redirect In Child Action");

            string url;
            if (this.Url != null && context.Controller is IDnnController)
            {
                url = this.Url.Action(this.ActionName, this.ControllerName);
				Logger.LogDebug("ExecuteResult(context={Context}): url → {Url}", context, url);
				Logger.LogInformation("ExecuteResult(context={Context}): url → {Url}", context, url);
            }
            else
            {
                // TODO - match other actions
                url = this.NavigationManager.NavigateURL();
            }

            if (this.Permanent)
            {
                context.HttpContext.Response.RedirectPermanent(url, true);
            }
            else
            {
                context.HttpContext.Response.Redirect(url, true);
            }
			Logger.LogDebug("<<< Exiting ExecuteResult(context={Context})", context);
        }
    }
}
