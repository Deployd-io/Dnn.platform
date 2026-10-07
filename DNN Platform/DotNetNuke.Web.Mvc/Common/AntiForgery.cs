// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Common
{
    using System;

    using DotNetNuke.Framework;

    public class AntiForgery : ServiceLocator<IAntiForgery, AntiForgery>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AntiForgery));
        /// <inheritdoc />
        protected override Func<IAntiForgery> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new AntiForgeryImpl();
        }
    }
}
