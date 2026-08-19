// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Web;

    using DotNetNuke.Web.Mvc.Common;

    public class ValidateAntiForgeryTokenAttribute : AuthorizeAttributeBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ValidateAntiForgeryTokenAttribute));
        public virtual bool IsAuthenticated(HttpContextBase httpContext)
        {
			Logger.LogDebug(">>> Entering IsAuthenticated(httpContext={HttpContext})", httpContext);
            try
            {
                if (Thread.CurrentPrincipal.Identity.IsAuthenticated)
                {
                    var headers = httpContext.Request.Headers;
                    var form = httpContext.Request.Form;

                    // Try to fetch the token from Headers. (Used with Dnn service framework.).
                    // If not found then fetch it from form fields. (Would be used with standard MVC call).
                    var token = headers.GetValues("RequestVerificationToken")?.FirstOrDefault() ?? form.GetValues("__RequestVerificationToken")?.FirstOrDefault();
					Logger.LogWarning("IsAuthenticated(httpContext={HttpContext}): token → {Token}", httpContext, token);

                    var cookieValue = this.GetAntiForgeryCookieValue(httpContext);
					Logger.LogDebug("IsAuthenticated(httpContext={HttpContext}): cookieValue → {CookieValue}", httpContext, cookieValue);
                    if (token != null)
                    {
                        AntiForgery.Instance.Validate(cookieValue, token);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
				Logger.LogError("Exception in IsAuthenticated(httpContext={HttpContext}): {Arg1}", httpContext, Exception.Message);
			Logger.LogDebug("<<< Exiting IsAuthenticated(httpContext={HttpContext})", httpContext);
                return false;
            }

            return true;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected string GetAntiForgeryCookieValue(HttpContextBase context)
        {
			Logger.LogDebug(">>> Entering GetAntiForgeryCookieValue(context={Context})", context);
            foreach (var cookieValue in context.Request.Headers.GetValues("Cookie"))
            {
                var nameIndex = cookieValue.IndexOf(AntiForgery.Instance.CookieName, StringComparison.InvariantCultureIgnoreCase);
				Logger.LogDebug("GetAntiForgeryCookieValue(context={Context}): nameIndex → {NameIndex}", context, nameIndex);

                if (nameIndex > -1)
                {
                    var valueIndex = nameIndex + AntiForgery.Instance.CookieName.Length + 1;
                    var valueEndIndex = cookieValue.Substring(valueIndex).IndexOf(';');
					Logger.LogDebug("GetAntiForgeryCookieValue(context={Context}): valueEndIndex → {ValueEndIndex}", context, valueEndIndex);
                    return valueEndIndex > -1 ? cookieValue.Substring(valueIndex, valueEndIndex) : cookieValue.Substring(valueIndex);
                }
            }

			Logger.LogDebug("<<< Exiting GetAntiForgeryCookieValue(context={Context})", context);
            return string.Empty;
        }

        /// <inheritdoc />
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
			Logger.LogDebug(">>> Entering AuthorizeCore(httpContext={HttpContext})", httpContext);
            if (!this.IsAuthenticated(httpContext))
            {
			Logger.LogDebug("<<< Exiting AuthorizeCore(httpContext={HttpContext})", httpContext);
                return false;
            }

            return true;
        }
    }
}
