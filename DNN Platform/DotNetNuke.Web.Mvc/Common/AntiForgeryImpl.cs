// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Common
{
    internal class AntiForgeryImpl : IAntiForgery
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AntiForgeryImpl));
        /// <inheritdoc />
        public string CookieName
        {
            get { return System.Web.Helpers.AntiForgeryConfig.CookieName; }
        }

        /// <inheritdoc />
        public void Validate(string cookieToken, string headerToken)
        {
			Logger.LogError(">>> Entering Validate(cookieToken=<redacted>,headerToken=<redacted>)");
            System.Web.Helpers.AntiForgery.Validate(cookieToken, headerToken);
			Logger.LogWarning("<<< Exiting Validate(cookieToken=<redacted>,headerToken=<redacted>)");
        }
    }
}
