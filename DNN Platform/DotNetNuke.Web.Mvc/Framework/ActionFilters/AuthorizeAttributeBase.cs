// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;

    using Microsoft.Extensions.DependencyInjection;

    public abstract class AuthorizeAttributeBase : FilterAttribute, IAuthorizationFilter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static bool IsAnonymousAttributePresent(AuthorizationContext filterContext)
        {
			Logger.LogDebug(">>> Entering IsAnonymousAttributePresent(filterContext={FilterContext})", filterContext);
			Logger.LogDebug("<<< Exiting IsAnonymousAttributePresent(filterContext={FilterContext})", filterContext);
            return filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true)
                                     || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), inherit: true);
        }

        /// <inheritdoc />
        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
			Logger.LogDebug(">>> Entering OnAuthorization(filterContext={FilterContext})", filterContext);
            Requires.NotNull("filterContext", filterContext);

            if (this.SkipAuthorization(filterContext))
            {
                return;
            }

            if (this.AuthorizeCore(filterContext.HttpContext))
            {
                this.HandleAuthorizedRequest(filterContext);
            }
            else
            {
                this.HandleUnauthorizedRequest(filterContext);
            }
			Logger.LogDebug("<<< Exiting OnAuthorization(filterContext={FilterContext})", filterContext);
        }

        protected virtual bool AuthorizeCore(HttpContextBase httpContext)
        {
			Logger.LogDebug(">>> Entering AuthorizeCore(httpContext={HttpContext})", httpContext);
			Logger.LogDebug("<<< Exiting AuthorizeCore(httpContext={HttpContext})", httpContext);
            return true;
        }

        protected virtual void HandleAuthorizedRequest(AuthorizationContext filterContext)
        {
			Logger.LogDebug(">>> Entering HandleAuthorizedRequest(filterContext={FilterContext})", filterContext);
            HttpCachePolicyBase cachePolicy = filterContext.HttpContext.Response.Cache;
            cachePolicy.SetProxyMaxAge(new TimeSpan(0));
            cachePolicy.AddValidationCallback(this.CacheValidateHandler, null /* data */);
			Logger.LogDebug("<<< Exiting HandleAuthorizedRequest(filterContext={FilterContext})", filterContext);
        }

        protected virtual void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
			Logger.LogDebug(">>> Entering HandleUnauthorizedRequest(filterContext={FilterContext})", filterContext);
            const string failureMessage = "Authorization has been denied for this request.";
            var authFilterContext = new AuthFilterContext(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), filterContext, failureMessage);
			Logger.LogDebug("HandleUnauthorizedRequest(filterContext={FilterContext}): authFilterContext → {AuthFilterContext}", filterContext, authFilterContext);
            authFilterContext.HandleUnauthorizedRequest();

            ////filterContext.HttpContext.Response.Redirect(Globals.AccessDeniedURL());
			Logger.LogDebug("<<< Exiting HandleUnauthorizedRequest(filterContext={FilterContext})", filterContext);
        }

        protected virtual HttpValidationStatus OnCacheAuthorization(HttpContextBase httpContext)
        {
			Logger.LogDebug(">>> Entering OnCacheAuthorization(httpContext={HttpContext})", httpContext);
            Requires.NotNull("httpContext", httpContext);

            bool isAuthorized = this.AuthorizeCore(httpContext);
			Logger.LogDebug("<<< Exiting OnCacheAuthorization(httpContext={HttpContext})", httpContext);
            return isAuthorized ? HttpValidationStatus.Valid : HttpValidationStatus.IgnoreThisRequest;
        }

        /// <summary>Skips this authorization step if anonymous attribute is applied, override if auth should never be skipped, or other conditions are required.</summary>
        /// <param name="filterContext">The context.</param>
        /// <returns>Whether the <see cref="AllowAnonymousAttribute"/> is present.</returns>
        protected virtual bool SkipAuthorization(AuthorizationContext filterContext)
        {
			Logger.LogDebug(">>> Entering SkipAuthorization(filterContext={FilterContext})", filterContext);
			Logger.LogDebug("<<< Exiting SkipAuthorization(filterContext={FilterContext})", filterContext);
            return IsAnonymousAttributePresent(filterContext);
        }

        private void CacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
        {
			Logger.LogDebug(">>> Entering CacheValidateHandler(context={Context},data={Data},validationStatus={ValidationStatus})", context, data, validationStatus);
            validationStatus = this.OnCacheAuthorization(new HttpContextWrapper(context));
			Logger.LogDebug("<<< Exiting CacheValidateHandler(context={Context},data={Data},validationStatus={ValidationStatus})", context, data, validationStatus);
			Logger.LogDebug("CacheValidateHandler(context={Context},data={Data},validationStatus={ValidationStatus}): validationStatus → {ValidationStatus1}", context, data, validationStatus, validationStatus);
        }
    }
}
