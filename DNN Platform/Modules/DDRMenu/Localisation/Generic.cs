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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IBusinessControllerProvider businessControllerProvider = businessControllerProvider ?? Globals.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
		Logger.LogDebug(">>> Entering Generic(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings})", businessControllerProvider, hostSettings);
		Logger.LogDebug("Generic(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings}): businessControllerProvider → {BusinessControllerProvider1}", businessControllerProvider, hostSettings, businessControllerProvider);
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.DependencyProvider.GetRequiredService<IHostSettings>();
        private bool haveChecked;
		Logger.LogDebug("Generic(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings}): hostSettings → {HostSettings1}", businessControllerProvider, hostSettings, hostSettings);
        private object locApi;
        private MethodInfo locTab;
        private MethodInfo locNodes;

        /// <summary>Initializes a new instance of the <see cref="Generic"/> class.</summary>
		Logger.LogDebug("<<< Exiting Generic(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings})", businessControllerProvider, hostSettings);
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public Generic()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Generic"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public Generic(IBusinessControllerProvider businessControllerProvider)
            : this(businessControllerProvider, null)
        {
		Logger.LogError("Exception in Generic(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings}): {Arg2}", businessControllerProvider, hostSettings, ex.Message);
        }

        /// <inheritdoc />
        public bool HaveApi()
        {
			Logger.LogDebug(">>> Entering HaveApi()");
            if (!this.haveChecked)
            {
                var modules = DesktopModuleController.GetDesktopModules(this.hostSettings, PortalSettings.Current.PortalId);
                foreach (var module in modules.Values.Where(m => !string.IsNullOrEmpty(m.BusinessControllerClass)))
                {
				Logger.LogDebug("HaveApi(): modules → {Modules}", modules);
                    try
                    {
                        var controllerType = Reflection.CreateType(module.BusinessControllerClass);
                        this.locTab = controllerType.GetMethod("LocaliseTab", [typeof(TabInfo), typeof(int),]);
						Logger.LogDebug("HaveApi(): controllerType → {ControllerType}", controllerType);
						Logger.LogDebug("HaveApi(): locTab → {LocTab}", locTab);
                        if (this.locTab != null)
                        {
                            if (!this.locTab.IsStatic)
                            {
                                this.locApi = this.businessControllerProvider.GetInstance(controllerType);
								Logger.LogDebug("HaveApi(): locApi → {LocApi}", locApi);
                            }

                            break;
                        }

                        this.locNodes = controllerType.GetMethod("LocaliseNodes", [typeof(DNNNodeCollection),]);
						Logger.LogDebug("HaveApi(): locNodes → {LocNodes}", locNodes);
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
					Logger.LogError("Exception in Generic(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings}): {Arg2}", businessControllerProvider, hostSettings, ex.Message);
                    }
					Logger.LogError("Exception in HaveApi(): {Arg0}", ex.Message);
                }

                this.haveChecked = true;
				Logger.LogDebug("HaveApi(): this.haveChecked → {Arg0}", this.haveChecked);
            }

			Logger.LogDebug("<<< Exiting HaveApi()");
            return this.locTab != null || this.locNodes != null;
        }

        /// <inheritdoc />
        public TabInfo LocaliseTab(TabInfo tab, int portalId)
        {
			Logger.LogDebug(">>> Entering LocaliseTab(tab={Tab},portalId={PortalId})", tab, portalId);
			Logger.LogDebug("<<< Exiting LocaliseTab(tab={Tab},portalId={PortalId})", tab, portalId);
            return (TabInfo)this.locTab?.Invoke(this.locApi, [tab, portalId,]);
        }

        /// <inheritdoc />
        public DNNNodeCollection LocaliseNodes(DNNNodeCollection nodes)
        {
			Logger.LogDebug(">>> Entering LocaliseNodes(nodes={Nodes})", nodes);
			Logger.LogDebug("<<< Exiting LocaliseNodes(nodes={Nodes})", nodes);
            return (DNNNodeCollection)this.locNodes?.Invoke(this.locApi, [nodes,]);
        }
    }
}
