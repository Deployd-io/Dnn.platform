// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common
{
    using System.Web;

    /// <summary>A unit testable alternative to HttpContext.Current.</summary>
    public class HttpContextSource
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static HttpContextBase fakeContext;

        /// <summary>Gets the current HttpContext.</summary>
        public static HttpContextBase Current
        {
            get
            {
                if (fakeContext != null)
                {
                    return fakeContext;
                }

                if (HttpContext.Current != null)
                {
                    return new HttpContextWrapper(HttpContext.Current);
                }

                return null;
            }
        }

        /// <summary>Injects a fake/mock context for unit testing.</summary>
        /// <param name="instance">The fake context to inject.</param>
        public static void RegisterInstance(HttpContextBase instance)
        {
			Logger.LogDebug(">>> Entering RegisterInstance(instance={Instance})", instance);
            fakeContext = instance;
			Logger.LogDebug("<<< Exiting RegisterInstance(instance={Instance})", instance);
			Logger.LogDebug("RegisterInstance(instance={Instance}): fakeContext → {FakeContext}", instance, fakeContext);
        }
    }
}
