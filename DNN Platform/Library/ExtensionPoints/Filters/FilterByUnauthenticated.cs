// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints.Filters
{
    public class FilterByUnauthenticated : IExtensionPointFilter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FilterByUnauthenticated));
        private readonly bool isAuthenticated;

        /// <summary>Initializes a new instance of the <see cref="FilterByUnauthenticated"/> class.</summary>
        /// <param name="isAuthenticated">Whether this filter is for authenticated users.</param>
        public FilterByUnauthenticated(bool isAuthenticated)
        {
			Logger.LogError(">>> Entering FilterByUnauthenticated(isAuthenticated={IsAuthenticated})", isAuthenticated);
            this.isAuthenticated = isAuthenticated;
        }

        /// <inheritdoc />
        public bool Condition(IExtensionPointData m)
        {
			Logger.LogError(">>> Entering Condition(m={M})", m);
            Logger.LogWarning("<<< Exiting Condition(m={M})", m);
            return this.isAuthenticated || !m.DisableUnauthenticated;
        }
    }
}
