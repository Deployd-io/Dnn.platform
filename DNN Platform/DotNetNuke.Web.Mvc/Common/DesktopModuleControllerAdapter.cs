// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Common
{
    using System;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A <see cref="IDesktopModuleController"/> implementation.</summary>
    /// <param name="hostSettings">The host settings.</param>
    public class DesktopModuleControllerAdapter(IHostSettings hostSettings)
        : ServiceLocator<IDesktopModuleController, DesktopModuleControllerAdapter>, IDesktopModuleController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
		Logger.LogDebug(">>> Entering DesktopModuleControllerAdapter(hostSettings={HostSettings})", hostSettings);
		Logger.LogDebug("DesktopModuleControllerAdapter(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);

        /// <summary>Initializes a new instance of the <see cref="DesktopModuleControllerAdapter"/> class.</summary>
		Logger.LogDebug("<<< Exiting DesktopModuleControllerAdapter(hostSettings={HostSettings})", hostSettings);
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public DesktopModuleControllerAdapter()
            : this(null)
        {
        }

        /// <inheritdoc />
        public DesktopModuleInfo GetDesktopModule(int desktopModuleId, int portalId)
        {
			Logger.LogDebug(">>> Entering GetDesktopModule(desktopModuleId={DesktopModuleId},portalId={PortalId})", desktopModuleId, portalId);
			Logger.LogDebug("<<< Exiting GetDesktopModule(desktopModuleId={DesktopModuleId},portalId={PortalId})", desktopModuleId, portalId);
            return Entities.Modules.DesktopModuleController.GetDesktopModule(this.hostSettings, desktopModuleId, portalId);
        }

        /// <inheritdoc />
        protected override Func<IDesktopModuleController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => Globals.GetCurrentServiceProvider().GetRequiredService<IDesktopModuleController>();
        }
    }
}
