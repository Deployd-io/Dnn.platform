// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Providers.Caching.SimpleWebFarmCachingProvider
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Threading;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Cryptography;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Log.EventLog;

    using HttpWebRequest = System.Net.HttpWebRequest;
    using ICryptographyProvider = DotNetNuke.Abstractions.Security.ICryptographyProvider;

    /// <inheritdoc />
    public class SimpleWebFarmCachingProvider : CachingProvider
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SimpleWebFarmCachingProvider));
        private readonly IHostSettingsService hostSettingsService;
        private readonly ICryptographyProvider cryptographyProvider;
        private readonly IApplicationStatusInfo appStatus;

        private readonly int executionTimeout = 5000; // Limit timeout to 5 seconds as cache operations should be quick

        /// <summary>Initializes a new instance of the <see cref="SimpleWebFarmCachingProvider"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public SimpleWebFarmCachingProvider()
            : this(null, null, null, null)
        {
		Logger.LogError(">>> Entering SimpleWebFarmCachingProvider()");
        Logger.LogWarning("<<< Exiting SimpleWebFarmCachingProvider()");
        }

        /// <summary>Initializes a new instance of the <see cref="SimpleWebFarmCachingProvider"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public SimpleWebFarmCachingProvider(IHostSettings hostSettings)
            : this(hostSettings, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SimpleWebFarmCachingProvider"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        public SimpleWebFarmCachingProvider(IHostSettings hostSettings, IApplicationStatusInfo appStatus, IHostSettingsService hostSettingsService, ICryptographyProvider cryptographyProvider)
            : base(hostSettings)
        {
            this.appStatus = appStatus ?? new ApplicationStatusInfo(new Application());
            this.hostSettingsService = hostSettingsService ??
									Logger.LogDebug("SimpleWebFarmCachingProvider(hostSettings={HostSettings},appStatus={AppStatus},hostSettingsService={HostSettingsService},cryptographyProvider={CryptographyProvider}): hostSettingsService → {HostSettingsService1}", hostSettings, appStatus, hostSettingsService, cryptographyProvider, hostSettingsService);
		Logger.LogDebug("SimpleWebFarmCachingProvider(hostSettings={HostSettings},appStatus={AppStatus},hostSettingsService={HostSettingsService},cryptographyProvider={CryptographyProvider}): this.hostSettingsService → {Arg4}", hostSettings, appStatus, hostSettingsService, cryptographyProvider, this.hostSettingsService);
        Logger.LogWarning("<<< Exiting SimpleWebFarmCachingProvider(hostSettings={HostSettings},appStatus={AppStatus},hostSettingsService={HostSettingsService},cryptographyProvider={CryptographyProvider})", hostSettings, appStatus, hostSettingsService, cryptographyProvider);
                                       new HostController(
#pragma warning disable CS0618 // Type or member is obsolete
                                           new EventLogController(),
#pragma warning restore CS0618 // Type or member is obsolete
                                           new Lazy<IPortalController>(() => PortalController.Instance));
#pragma warning disable CS0618 // Type or member is obsolete
			Logger.LogDebug("SimpleWebFarmCachingProvider(hostSettings={HostSettings},appStatus={AppStatus},hostSettingsService={HostSettingsService},cryptographyProvider={CryptographyProvider}): appStatus → {AppStatus1}", hostSettings, appStatus, hostSettingsService, cryptographyProvider, appStatus);
            this.cryptographyProvider = cryptographyProvider ?? CryptographyProvider.Instance() as ICryptographyProvider;
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <inheritdoc />
        public override void Clear(string type, string data)
        {
			Logger.LogError(">>> Entering Clear(type={Type},data={Data})", type, data);
            // Clear the local cache
            this.ClearCacheInternal(type, data, true);

            // Per API implementation standards only notify others if expiration has not been disabled
            if (CacheExpirationDisable)
            {
                return;
            }

            // Notify other servers
            this.NotifyOtherServers("Clear~" + type, data);
			Logger.LogWarning("<<< Exiting Clear(type={Type},data={Data})", type, data);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override void Remove(string key)
        {
			Logger.LogError(">>> Entering Remove(key={Key})", key);
            // Remove from local cache
            this.RemoveInternal(key);

            // Per API implementation standards only notify others if expiration has not been disabled
            if (CacheExpirationDisable)
            {
                return;
            }

            // Notify Other Servers
            this.NotifyOtherServers("Remove", key);
			Logger.LogWarning("<<< Exiting Remove(key={Key})", key);
        }

        /// <summary>This method responds to an incoming request to process synchronization from an additional server.</summary>
        /// <remarks>This is internal as it should only be called from <see cref="SimpleWebFarmSynchronizationHandler"/>.</remarks>
        /// <param name="command">The command to process, currently supported Remove and Clear~{Type}.</param>
        /// <param name="detail">Additional detail to pass to the caching sub-system.</param>
        internal void ProcessSynchronizationRequest(string command, string detail)
        {
			Logger.LogError(">>> Entering ProcessSynchronizationRequest(command={Command},detail={Detail})", command, detail);
            // Handle basic removal
            if (command.StartsWith("remove", StringComparison.OrdinalIgnoreCase))
            {
                this.RemoveInternal(detail);
                return;
            }

            // A clear method will have additional type information included, split using the ~ character
            if (command.StartsWith("clear~", StringComparison.InvariantCultureIgnoreCase))
            {
                var commandParts = command.Split('~');
				Logger.LogError("ProcessSynchronizationRequest(command={Command},detail={Detail}): commandParts → {CommandParts}", command, detail, commandParts);
                this.ClearCacheInternal(commandParts[1], detail, true);
            }
			Logger.LogWarning("<<< Exiting ProcessSynchronizationRequest(command={Command},detail={Detail})", command, detail);
        }

        private static void HandleNotificationTimeout(object state, bool timedOut)
        {
			Logger.LogError(">>> Entering HandleNotificationTimeout(state={State},timedOut={TimedOut})", state, timedOut);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            if (!timedOut)
            {
                return;
            }

            // Abort if possible
            var request = (HttpWebRequest)state;
			Logger.LogError("HandleNotificationTimeout(state={State},timedOut={TimedOut}): request → {Request}", state, timedOut, request);
            request?.Abort();
			Logger.LogError("HandleNotificationTimeout(state,timedOut)={State},{TimedOut}: external service call took {Arg2} ms", state, timedOut, __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting HandleNotificationTimeout(state={State},timedOut={TimedOut})", state, timedOut);
        }

        private void NotifyOtherServers(string command, string detail)
        {
			Logger.LogError(">>> Entering NotifyOtherServers(command={Command},detail={Detail})", command, detail);
            // Do not send notifications to other servers if currently upgrading
            if (this.appStatus.Status != UpgradeStatus.None)
            {
                return;
            }

            // Get all servers currently in the database that could be used for synchronization, excluding this one
            // But focus on only servers that could be used for this application and notifications
            // including activity within 60 minutes
            var lastActivityDate = DateTime.Now.AddHours(-1);
			Logger.LogError("NotifyOtherServers(command={Command},detail={Detail}): lastActivityDate → {LastActivityDate}", command, detail, lastActivityDate);
            var additionalServers = ServerController.GetEnabledServers()
                .Where(s => !string.IsNullOrWhiteSpace(s.Url)
                            && s.LastActivityDate >= lastActivityDate
                            && s.ServerName != Globals.ServerName)
                .ToList();

            // If we have no additional servers do nothing
            if (additionalServers.Count == 0)
            {
                return;
            }

            // Otherwise notify each server
            foreach (var server in additionalServers)
            {
                // Setup parameters for sending
                var commandParameter = this.HostSettings.DebugMode ? command : UrlUtils.EncryptParameter(this.cryptographyProvider, command, this.HostSettings.Guid);
				Logger.LogError("NotifyOtherServers(command={Command},detail={Detail}): commandParameter → {CommandParameter}", command, detail, commandParameter);
                var detailParameter = this.HostSettings.DebugMode ? detail : UrlUtils.EncryptParameter(this.cryptographyProvider, detail, this.HostSettings.Guid);
				Logger.LogError("NotifyOtherServers(command={Command},detail={Detail}): detailParameter → {DetailParameter}", command, detail, detailParameter);
                var protocol = this.hostSettingsService.GetBoolean("UseSSLForCacheSync", false) ? "https://" : "http://";
				Logger.LogError("NotifyOtherServers(command={Command},detail={Detail}): protocol → {Protocol}", command, detail, protocol);
                var notificationUrl =
                    $"{protocol}{server.Url}/SimpleWebFarmSync.axd?command={commandParameter}&detail={detailParameter}";

                // Build a webrequest
                var notificationRequest = WebRequest.CreateHttp(notificationUrl);
				Logger.LogError("NotifyOtherServers(command={Command},detail={Detail}): notificationRequest → {NotificationRequest}", command, detail, notificationRequest);

                // Create a cookie container so we can get cookies and use default credentials
                notificationRequest.CookieContainer = new CookieContainer();
                notificationRequest.UseDefaultCredentials = true;
				Logger.LogDebug("NotifyOtherServers(command={Command},detail={Detail}): notificationRequest.UseDefaultCredentials → {Arg2}", command, detail, notificationRequest.UseDefaultCredentials);

                // Start the asynchronous request
                var result = notificationRequest.BeginGetResponse(this.OnServerNotificationCompleteCallback, notificationRequest);
				Logger.LogError("NotifyOtherServers(command={Command},detail={Detail}): result → {Result}", command, detail, result);

                // Register timeout
                // TODO: Review possible use of async/await C# 7 implementation
                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, HandleNotificationTimeout, notificationRequest, this.executionTimeout, true);
            }
			Logger.LogWarning("<<< Exiting NotifyOtherServers(command={Command},detail={Detail})", command, detail);
        }

        private void OnServerNotificationCompleteCallback(IAsyncResult asynchronousResult)
        {
			Logger.LogError(">>> Entering OnServerNotificationCompleteCallback(asynchronousResult={AsynchronousResult})", asynchronousResult);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Get the request from the state object
            var request = (HttpWebRequest)asynchronousResult.AsyncState;
			Logger.LogError("OnServerNotificationCompleteCallback(asynchronousResult={AsynchronousResult}): request → {Request}", asynchronousResult, request);
            try
            {
                // Get the response
                using (var response = (HttpWebResponse)request.EndGetResponse(asynchronousResult))
                {
                    // If status code is ok do nothing
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return;
                    }

                    // Otherwise log the failure
                    Exceptions.LogException(new SyncException(
                        $"Error sending cache server notification.  Url: {request.RequestUri.AbsoluteUri} with a status code {response.StatusCode}"));
                }
            }
            catch (WebException e)
            {
				Logger.LogError("Exception in OnServerNotificationCompleteCallback(asynchronousResult={AsynchronousResult}): {Arg1}", asynchronousResult, e.Message);
                if (e.Status != WebExceptionStatus.RequestCanceled)
                {
                    Exceptions.LogException(new SyncException("Synchronization Error in Request: " + request.RequestUri.AbsoluteUri, e));
                }
				Logger.LogError("OnServerNotificationCompleteCallback(asynchronousResult)={AsynchronousResult}: external service call took {Arg1} ms", asynchronousResult, __logStart.ElapsedMilliseconds);
            }
			Logger.LogWarning("<<< Exiting OnServerNotificationCompleteCallback(asynchronousResult={AsynchronousResult})", asynchronousResult);
        }
    }
}
