// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints.Filters
{
    public class FilterByHostMenu : IExtensionPointFilter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FilterByHostMenu));
        private readonly bool isHostMenu;

        /// <summary>Initializes a new instance of the <see cref="FilterByHostMenu"/> class.</summary>
        /// <param name="isHostMenu">Whether this filter is for the host menu.</param>
        public FilterByHostMenu(bool isHostMenu)
        {
			Logger.LogError(">>> Entering FilterByHostMenu(isHostMenu={IsHostMenu})", isHostMenu);
            this.isHostMenu = isHostMenu;
        }

        /// <inheritdoc />
        public bool Condition(IExtensionPointData m)
        {
			Logger.LogError(">>> Entering Condition(m={M})", m);
            Logger.LogWarning("<<< Exiting Condition(m={M})", m);
            return !this.isHostMenu || !m.DisableOnHost;
        }
    }
}
