// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using DotNetNuke.Common;
    using DotNetNuke.DependencyInjection.Extensions;

    /// <summary>Gets action filters for web API actions.</summary>
    /// <remarks>Replaces <see cref="ActionDescriptorFilterProvider"/>.</remarks>
    internal class DnnActionFilterProvider : IFilterProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnActionFilterProvider));
        private readonly IServiceProvider container;

        /// <summary>Initializes a new instance of the <see cref="DnnActionFilterProvider"/> class.</summary>
        /// <param name="container">The dependency injection container.</param>
        public DnnActionFilterProvider(IServiceProvider container)
        {
			Logger.LogError(">>> Entering DnnActionFilterProvider(container={Container})", container);
            this.container = container;
			Logger.LogDebug("DnnActionFilterProvider(container={Container}): container → {Container1}", container, container);
        Logger.LogDebug("DnnActionFilterProvider(container={Container}): this.container → {Arg1}", container, this.container);
        Logger.LogWarning("<<< Exiting DnnActionFilterProvider(container={Container})", container);
        }

        /// <inheritdoc />
        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
			Logger.LogError(">>> Entering GetFilters(configuration={Configuration},actionDescriptor={ActionDescriptor})", configuration, actionDescriptor);
            Requires.NotNull("actionDescriptor", actionDescriptor);

            var controllerFilters = actionDescriptor.ControllerDescriptor.GetFilters().Select(instance => new FilterInfo(instance, FilterScope.Controller));
			Logger.LogError("GetFilters(configuration={Configuration},actionDescriptor={ActionDescriptor}): controllerFilters → {ControllerFilters}", configuration, actionDescriptor, controllerFilters);
            var actionFilters = actionDescriptor.GetFilters().Select(instance => new FilterInfo(instance, FilterScope.Action));
			Logger.LogError("GetFilters(configuration={Configuration},actionDescriptor={ActionDescriptor}): actionFilters → {ActionFilters}", configuration, actionDescriptor, actionFilters);

            var allFilters = controllerFilters.Concat(actionFilters).ToList();
			Logger.LogError("GetFilters(configuration={Configuration},actionDescriptor={ActionDescriptor}): allFilters → {AllFilters}", configuration, actionDescriptor, allFilters);

            var overrideFilterPresent = allFilters.Any(x => x.Instance is IOverrideDefaultAuthLevel);
			Logger.LogError("GetFilters(configuration={Configuration},actionDescriptor={ActionDescriptor}): overrideFilterPresent → {OverrideFilterPresent}", configuration, actionDescriptor, overrideFilterPresent);

            if (!overrideFilterPresent)
            {
                allFilters.Add(new FilterInfo(new RequireHostAttribute(), FilterScope.Action));
            }

            foreach (var filterInfo in allFilters)
            {
                this.container.BuildUp(filterInfo.Instance);
            }

			Logger.LogWarning("<<< Exiting GetFilters(configuration={Configuration},actionDescriptor={ActionDescriptor})", configuration, actionDescriptor);
            return allFilters;
        }
    }
}
