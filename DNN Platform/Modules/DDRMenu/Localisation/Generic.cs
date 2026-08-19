// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.DDRMenu.Localisation
{
    using System;
    using System.Linq;
    using System.Reflection;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Implements generic localization support.</summary>
    /// <param name="businessControllerProvider">The business controller provider.</param>
    /// <param name="hostSettings">The host settings.</param>
    public class Generic(IBusinessControllerProvider businessControllerProvider, IHostSettings hostSettings)
        : ILocalisation
    {
        private readonly IBusinessControllerProvider businessControllerProvider = businessControllerProvider ?? Globals.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.DependencyProvider.GetRequiredService<IHostSettings>();
        private bool haveChecked;
        private object locApi;
        private MethodInfo locTab;
        private MethodInfo locNodes;

        /// <summary>Initializes a new instance of the <see cref="Generic"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public Generic()
            : this(null, null)
        {
		eventLogger.LogDebug(">>> Entering Generic()");
        }
		eventLogger.LogDebug("<<< Exiting Generic()");

        /// <summary>Initializes a new instance of the <see cref="Generic"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public Generic(IBusinessControllerProvider businessControllerProvider)
            : this(businessControllerProvider, null)
        {
        }

        /// <inheritdoc />
        public bool HaveApi()
        {
			eventLogger.LogDebug(">>> Entering HaveApi()");
            if (!this.haveChecked)
            {
                var modules = DesktopModuleController.GetDesktopModules(this.hostSettings, PortalSettings.Current.PortalId);
				eventLogger.LogDebug("HaveApi(): modules → {Modules}", modules);
                foreach (var module in modules.Values.Where(m => !string.IsNullOrEmpty(m.BusinessControllerClass)))
                {
                    try
                    {
                        var controllerType = Reflection.CreateType(module.BusinessControllerClass);
						eventLogger.LogDebug("HaveApi(): controllerType → {ControllerType}", controllerType);
                        this.locTab = controllerType.GetMethod("LocaliseTab", [typeof(TabInfo), typeof(int),]);
						eventLogger.LogDebug("HaveApi(): locTab → {LocTab}", locTab);
                        if (this.locTab != null)
                        {
                            if (!this.locTab.IsStatic)
                            {
                                this.locApi = this.businessControllerProvider.GetInstance(controllerType);
								eventLogger.LogDebug("HaveApi(): locApi → {LocApi}", locApi);
                            }

                            break;
                        }

                        this.locNodes = controllerType.GetMethod("LocaliseNodes", [typeof(DNNNodeCollection),]);
						eventLogger.LogDebug("HaveApi(): locNodes → {LocNodes}", locNodes);
                        if (this.locNodes != null)
                        {
                            if (!this.locNodes.IsStatic)
                            {
                                this.locApi = this.businessControllerProvider.GetInstance(controllerType);
                            }

                            break;
                        }
                    }

                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
					eventLogger.LogError("Exception in HaveApi(): {Arg0}", ex.Message);
                    }
                }

                this.haveChecked = true;
				eventLogger.LogDebug("HaveApi(): this.haveChecked → {Arg0}", this.haveChecked);
            }

			eventLogger.LogDebug("<<< Exiting HaveApi()");
            return this.locTab != null || this.locNodes != null;
        }

        /// <inheritdoc />
        public TabInfo LocaliseTab(TabInfo tab, int portalId)
        {
			eventLogger.LogDebug(">>> Entering LocaliseTab(tab={Tab},portalId={PortalId})", tab, portalId);
			eventLogger.LogDebug("<<< Exiting LocaliseTab(tab={Tab},portalId={PortalId})", tab, portalId);
            return (TabInfo)this.locTab?.Invoke(this.locApi, [tab, portalId,]);
        }

        /// <inheritdoc />
        public DNNNodeCollection LocaliseNodes(DNNNodeCollection nodes)
        {
			eventLogger.LogDebug(">>> Entering LocaliseNodes(nodes={Nodes})", nodes);
			eventLogger.LogDebug("<<< Exiting LocaliseNodes(nodes={Nodes})", nodes);
            return (DNNNodeCollection)this.locNodes?.Invoke(this.locApi, [nodes,]);
        }
    }
}
