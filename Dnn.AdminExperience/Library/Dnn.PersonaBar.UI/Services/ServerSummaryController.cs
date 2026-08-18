// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.UI.Services.DTO;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Installer;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A Persona Bar API controller for the server summary.</summary>
    [MenuPermission(Scope = ServiceScope.Regular)]
    public class ServerSummaryController : PersonaBarApiController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServerSummaryController));
        private readonly IHostSettings hostSettings;
        private readonly IApplicationInfo application;

        /// <summary>Initializes a new instance of the <see cref="ServerSummaryController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ServerSummaryController()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering ServerSummaryController()");
		Logger.LogDebug("<<< Exiting ServerSummaryController()");
		Logger.LogDebug(">>> Entering ServerSummaryController(hostSettings={HostSettings},application={Application})", hostSettings, application);
		Logger.LogDebug("ServerSummaryController(hostSettings={HostSettings},application={Application}): application → {Application1}", hostSettings, application, application);
        }

        /// <summary>Initializes a new instance of the <see cref="ServerSummaryController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="application">The application.</param>
        public ServerSummaryController(IHostSettings hostSettings, IApplicationInfo application)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.application = application ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationInfo>();
			Logger.LogDebug("<<< Exiting ServerSummaryController(hostSettings={HostSettings},application={Application})", hostSettings, application);
        }

        /// <summary>Return server info.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A response with server info.</returns>
        [HttpGet]
        public HttpResponseMessage GetServerInfo(CancellationToken cancellationToken)
        {
			Logger.LogDebug(">>> Entering GetServerInfo(cancellationToken=<redacted>)");
            try
            {
                var isHost = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
				Logger.LogDebug("GetServerInfo(cancellationToken=<redacted>): isHost → {IsHost}", isHost);
                var response = new
                {
                    ProductName = this.application.Description,
                    ProductVersion = "v. " + Globals.FormatVersion(this.application.Version, true),
                    FrameworkVersion = isHost ? Globals.FormattedNetFrameworkVersion : string.Empty,
                    ServerName = isHost ? Globals.ServerName : string.Empty,
                    LicenseVisible = isHost && this.GetVisibleSetting("LicenseVisible"),
                    DocCenterVisible = this.GetVisibleSetting("DocCenterVisible"),
                    Update = this.UpdateInfo(cancellationToken),
                };
				Logger.LogDebug("GetServerInfo(cancellationToken=<redacted>): DocCenterVisible → {DocCenterVisible}", DocCenterVisible);
				Logger.LogDebug("GetServerInfo(cancellationToken=<redacted>): ServerName → {ServerName}", ServerName);
				Logger.LogDebug("GetServerInfo(cancellationToken=<redacted>): Update → {Update}", Update);
				Logger.LogDebug("GetServerInfo(cancellationToken=<redacted>): FrameworkVersion → {FrameworkVersion}", FrameworkVersion);
				Logger.LogDebug("GetServerInfo(cancellationToken=<redacted>): ProductName → {ProductName}", ProductName);
				Logger.LogDebug("GetServerInfo(cancellationToken=<redacted>): LicenseVisible → {LicenseVisible}", LicenseVisible);
				Logger.LogDebug("GetServerInfo(cancellationToken=<redacted>): ProductVersion → {ProductVersion}", ProductVersion);

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
				Logger.LogDebug("<<< Exiting GetServerInfo(cancellationToken=<redacted>)");
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }

        /// <summary>Returns update information about current framework version.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A serialized <see cref="FrameworkQueryDTO"/> object.</returns>
        [HttpGet]
        public HttpResponseMessage GetUpdateInfo(CancellationToken cancellationToken)
        {
			Logger.LogDebug(">>> Entering GetUpdateInfo(cancellationToken=<redacted>)");
			Logger.LogDebug(">>> Entering UpdateInfo(cancellationToken=<redacted>)");
			Logger.LogDebug("<<< Exiting GetUpdateInfo(cancellationToken=<redacted>)");
            return this.Request.CreateResponse(HttpStatusCode.OK, this.UpdateInfo(cancellationToken));
        }

        private bool GetVisibleSetting(string settingName)
        {
			Logger.LogDebug(">>> Entering GetVisibleSetting(settingName={SettingName})", settingName);
            var portalSettings = PortalController.Instance.GetPortalSettings(this.PortalId);
			Logger.LogDebug("GetVisibleSetting(settingName={SettingName}): portalSettings → {PortalSettings}", settingName, portalSettings);
			Logger.LogDebug("<<< Exiting GetVisibleSetting(settingName={SettingName})", settingName);
            return !portalSettings.ContainsKey(settingName)
                   || string.IsNullOrEmpty(portalSettings[settingName])
                   || portalSettings[settingName] == "true";
        }

        private FrameworkQueryDTO UpdateInfo(CancellationToken cancellationToken)
        {
            var updateInfo = new FrameworkQueryDTO();
            if (HttpContextSource.Current == null || !this.UserInfo.IsSuperUser)
            {
                return updateInfo;
            }

            if (!this.hostSettings.CheckUpgrade)
            {
                return updateInfo;
            }

            return CBO.GetCachedObject<FrameworkQueryDTO>(
                this.hostSettings,
                new CacheItemArgs("DnnUpdateUrl") { ParamList = { updateInfo, }, },
                this.RetrieveUpdateUrl);
        }

        private FrameworkQueryDTO RetrieveUpdateUrl(CacheItemArgs args)
        {
			Logger.LogDebug(">>> Entering RetrieveUpdateUrl(args={Args})", args);
            try
            {
                var coreVersion = Globals.FormatVersion(this.application.Version, "00", 3, string.Empty);
				Logger.LogDebug("RetrieveUpdateUrl(args={Args}): coreVersion → {CoreVersion}", args, coreVersion);
                var portalCount = PortalController.Instance.GetPortals().Count;
				Logger.LogDebug("RetrieveUpdateUrl(args={Args}): portalCount → {PortalCount}", args, portalCount);
                var osVersion = Globals.FormatVersion(Globals.OperatingSystemVersion, "00", 2, string.Empty);
				Logger.LogDebug("RetrieveUpdateUrl(args={Args}): osVersion → {OsVersion}", args, osVersion);
                var netVersion = Globals.FormatVersion(Globals.NETFrameworkVersion, "00", 2, string.Empty);
				Logger.LogDebug("RetrieveUpdateUrl(args={Args}): netVersion → {NetVersion}", args, netVersion);
                var dbVersion = Globals.FormatVersion(Globals.DatabaseEngineVersion, "00", 2, string.Empty);
				Logger.LogDebug("RetrieveUpdateUrl(args={Args}): dbVersion → {DbVersion}", args, dbVersion);
                var url = $"{this.application.UpgradeUrl}/Update/FrameworkStatus?core={coreVersion}&type={this.application.Type}&name={this.application.Name}&id={this.hostSettings.Guid}&no={portalCount}&os={osVersion}&net={netVersion}&db={dbVersion}";
				Logger.LogDebug("RetrieveUpdateUrl(args={Args}): url → {Url}", args, url);
                var response = this.GetJsonObject((FrameworkQueryDTO)args.ParamList[0], url);
				Logger.LogDebug("RetrieveUpdateUrl(args={Args}): response → {Response}", args, response);
                if (response.Version.Length == 6)
                {
                    response.Version = $"v. {response.Version.Substring(0, 2)}.{response.Version.Substring(2, 2)}.{response.Version.Substring(4, 2)}";
                }

                return response;
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in RetrieveUpdateUrl(args={Args}): {Arg1}", args, ex.getMessage());
                Exceptions.LogException(ex);
            }

			Logger.LogDebug("<<< Exiting RetrieveUpdateUrl(args={Args})", args);
            return new FrameworkQueryDTO();
        }

        private T GetJsonObject<T>(T initial, string url)
        {
            var request = Globals.GetExternalRequest(this.hostSettings, url);
            using var response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return initial;
            }

            using var dataStream = response.GetResponseStream();
            using var reader = new StreamReader(dataStream);
            var responseFromServer = reader.ReadToEnd();
            Newtonsoft.Json.JsonConvert.PopulateObject(responseFromServer, initial);
            return initial;
        }
    }
}
