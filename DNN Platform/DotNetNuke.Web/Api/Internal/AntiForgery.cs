// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#nullable enable
namespace DotNetNuke.Web.Api.Internal
{
    using System;

    using DotNetNuke.Framework;

    /// <summary>The <see cref="IAntiForgery"/> service locator.</summary>
    public class AntiForgery : ServiceLocator<IAntiForgery, AntiForgery>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AntiForgery));
        /// <inheritdoc />
        protected override Func<IAntiForgery> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new AntiForgeryImpl();
        }
    }
}
