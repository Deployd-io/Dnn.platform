// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints.Filters
{
    using System.Collections.Generic;
    using System.Linq;

    public class CompositeFilter : IExtensionPointFilter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CompositeFilter));
        private readonly List<IExtensionPointFilter> filters;

        /// <summary>Initializes a new instance of the <see cref="CompositeFilter"/> class.</summary>
        public CompositeFilter()
        {
			Logger.LogDebug(">>> Entering CompositeFilter()");
            this.filters = [];
			Logger.LogDebug("CompositeFilter(): filters → {Filters}", filters);
			Logger.LogDebug("CompositeFilter(): this.filters → {Arg0}", this.filters);
			Logger.LogDebug("<<< Exiting CompositeFilter()");
        }

        public CompositeFilter And(IExtensionPointFilter filter)
        {
			Logger.LogDebug(">>> Entering And(filter={Filter})", filter);
            this.filters.Add(filter);
			Logger.LogDebug("<<< Exiting And(filter={Filter})", filter);
            return this;
        }

        /// <inheritdoc />
        public bool Condition(IExtensionPointData m)
        {
			Logger.LogDebug(">>> Entering Condition(m={M})", m);
			Logger.LogDebug("<<< Exiting Condition(m={M})", m);
            return this.filters.All(f => f.Condition(m));
        }
    }
}
