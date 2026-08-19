// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Modules
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;

    /// <summary>Determines whether any given module can be injected into the page.</summary>
    internal class ModuleInjectionManager
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleInjectionManager));
        private readonly IEnumerable<IModuleInjectionFilter> filters;

        /// <summary>Initializes a new instance of the <see cref="ModuleInjectionManager"/> class.</summary>
        /// <param name="filters">The filters.</param>
        public ModuleInjectionManager(IEnumerable<IModuleInjectionFilter> filters)
        {
			Logger.LogDebug(">>> Entering ModuleInjectionManager(filters={Filters})", filters);
            this.filters = filters.ToList();
			Logger.LogDebug("ModuleInjectionManager(filters={Filters}): filters → {Filters1}", filters, filters);
			Logger.LogDebug("<<< Exiting ModuleInjectionManager(filters={Filters})", filters);
        }

        /// <summary>Determines whether the given <paramref name="module"/> passes all of the module injection filters.</summary>
        /// <param name="module">The module to inject.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns><see langword="true"/> if the module can be injected, otherwise <see langword="false"/>.</returns>
        public bool CanInjectModule(ModuleInfo module, PortalSettings portalSettings)
        {
			Logger.LogDebug(">>> Entering CanInjectModule(module={Module},portalSettings={PortalSettings})", module, portalSettings);
			Logger.LogDebug("<<< Exiting CanInjectModule(module={Module},portalSettings={PortalSettings})", module, portalSettings);
            return this.filters.All(filter => filter.CanInjectModule(module, portalSettings));
        }
    }
}
