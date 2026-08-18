// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Abstract class to allow derived classes of different implementations of Url Rewriter.</summary>
    public abstract class UrlRewriterBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UrlRewriterBase));
        /// <summary>Initializes a new instance of the <see cref="UrlRewriterBase"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        protected UrlRewriterBase()
            : this(null, null, null, null)
        {
		Logger.LogDebug(">>> Entering UrlRewriterBase()");
        }

		Logger.LogDebug("<<< Exiting UrlRewriterBase()");
        /// <summary>Initializes a new instance of the <see cref="UrlRewriterBase"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        protected UrlRewriterBase(IHostSettings hostSettings, IPortalAliasService portalAliasService, IHostSettingsService hostSettingsService, IPortalController portalController)
        {
            this.HostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.PortalAliasService = portalAliasService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>();
            this.HostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
            this.PortalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        }

        /// <summary>Gets the host settings.</summary>
        protected IHostSettings HostSettings { get; }

        /// <summary>Gets the portal alias service.</summary>
        protected IPortalAliasService PortalAliasService { get; }

        /// <summary>Gets the host settings service.</summary>
        protected IHostSettingsService HostSettingsService { get; }

        /// <summary>Gets the portal controller.</summary>
        protected IPortalController PortalController { get; }

        internal abstract void RewriteUrl(object sender, EventArgs e);

        protected static void AutoAddAlias(IHostSettings hostSettings, IPortalAliasService portalAliasService, HttpContextBase context)
        {
			Logger.LogDebug(">>> Entering AutoAddAlias(hostSettings={HostSettings},portalAliasService={PortalAliasService},context={Context})", hostSettings, portalAliasService, context);
            var portalId = hostSettings.HostPortalId;

            // the domain name was not found so try using the host portal's first alias
            if (portalId > Null.NullInteger)
            {
                IPortalAliasInfo portalAliasInfo = new PortalAliasInfo();
				Logger.LogDebug("AutoAddAlias(hostSettings={HostSettings},portalAliasService={PortalAliasService},context={Context}): portalAliasInfo → {PortalAliasInfo}", hostSettings, portalAliasService, context, portalAliasInfo);
                portalAliasInfo.HttpAlias = Globals.GetDomainName(context.Request, true);
                portalAliasInfo.PortalId = portalId;
				Logger.LogDebug("AutoAddAlias(hostSettings={HostSettings},portalAliasService={PortalAliasService},context={Context}): portalAliasInfo.PortalId → {Arg3}", hostSettings, portalAliasService, context, portalAliasInfo.PortalId);
                portalAliasService.AddPortalAlias(portalAliasInfo);

                context.Response.Redirect(context.Request.Url.ToString(), true);
            }
			Logger.LogDebug("<<< Exiting AutoAddAlias(hostSettings={HostSettings},portalAliasService={PortalAliasService},context={Context})", hostSettings, portalAliasService, context);
			Logger.LogDebug(">>> Entering AutoAddAlias(context={Context})", context);
        }

        protected static void AutoAddAlias(HttpContext context)
        {
            var serviceProvider = context.GetScope().ServiceProvider;
            AutoAddAlias(serviceProvider.GetRequiredService<IHostSettings>(), serviceProvider.GetRequiredService<IPortalAliasService>(), new HttpContextWrapper(context));
			Logger.LogDebug("<<< Exiting AutoAddAlias(context={Context})", context);
        }

        protected static bool CanAutoAddPortalAlias(IHostSettingsService hostSettingsService, IPortalController portalController)
        {
			Logger.LogDebug(">>> Entering CanAutoAddPortalAlias(hostSettingsService={HostSettingsService},portalController={PortalController})", hostSettingsService, portalController);
            bool autoAddPortalAlias = hostSettingsService.GetBoolean("AutoAddPortalAlias");
			Logger.LogDebug("<<< Exiting CanAutoAddPortalAlias(hostSettingsService={HostSettingsService},portalController={PortalController})", hostSettingsService, portalController);
			Logger.LogDebug("<<< Exiting CanAutoAddPortalAlias()");
            return autoAddPortalAlias && (portalController.GetPortals().Count == 1);
        }

        protected static bool CanAutoAddPortalAlias()
        {
            var serviceProvider = Globals.GetCurrentServiceProvider();
			Logger.LogDebug("CanAutoAddPortalAlias(): serviceProvider → {ServiceProvider}", serviceProvider);
            return CanAutoAddPortalAlias(
                serviceProvider.GetRequiredService<IHostSettingsService>(),
                serviceProvider.GetRequiredService<IPortalController>());
        }

        protected void AutoAddAlias(HttpContextBase context) => AutoAddAlias(this.HostSettings, this.PortalAliasService, context);

        protected bool AutoAddPortalAliasEnabled() => CanAutoAddPortalAlias(this.HostSettingsService, this.PortalController);
    }
}
