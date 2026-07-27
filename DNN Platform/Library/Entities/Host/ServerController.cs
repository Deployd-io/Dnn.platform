// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Host
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Caching;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Controller for web servers.</summary>
    public partial class ServerController
    {
        public const string DefaultUrlAdapter = "DotNetNuke.Entities.Host.ServerWebRequestAdapter, DotNetNuke";

        private const string CacheKey = "WebServers";
        private const int CacheTimeout = 20;
        private const CacheItemPriority CachePriority = CacheItemPriority.High;
        private static readonly DataProvider DataProvider = DataProvider.Instance();
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServerController));

        public static bool UseAppName
        {
            get
            {
                var uniqueServers = new Dictionary<string, string>();
                foreach (ServerInfo server in GetEnabledServers())
                {
                    uniqueServers[server.ServerName] = server.IISAppName;
					Logger.LogDebug(">>> Entering GetEnabledServers()");
					Logger.LogDebug("<<< Exiting GetEnabledServers()");
                }

                return uniqueServers.Count < GetEnabledServers().Count;
            }
        }

        public static void ClearCachedServers()
        {
			Logger.LogDebug(">>> Entering ClearCachedServers()");
            DataCache.RemoveCache(CacheKey);
			Logger.LogDebug("<<< Exiting ClearCachedServers()");
        }

        public static void DeleteServer(int serverID)
        {
			Logger.LogDebug(">>> Entering DeleteServer(serverID={ServerID})", serverID);
            DataProvider.Instance().DeleteServer(serverID);
            ClearCachedServers();
			Logger.LogDebug("<<< Exiting DeleteServer(serverID={ServerID})", serverID);
        }

        public static List<ServerInfo> GetEnabledServers()
        {
			Logger.LogDebug(">>> Entering GetEnabledServers()");
            return GetServers()?.Where(i => i.Enabled).ToList() ?? new List<ServerInfo>();
        }

        /// <summary>
        /// Gets the servers, order by last activity date in descending order.
        /// </summary>
        /// <param name="lastMinutes">The number of recent minutes activity had to occur.</param>
        /// <returns>A list of servers with activity within the specified minutes.</returns>
        public static List<ServerInfo> GetEnabledServersWithActivity(int lastMinutes = 10)
        {
			Logger.LogDebug(">>> Entering GetEnabledServersWithActivity(lastMinutes={LastMinutes})", lastMinutes);
			Logger.LogDebug("<<< Exiting GetEnabledServersWithActivity(lastMinutes={LastMinutes})", lastMinutes);
            return GetServersNoCache()
                .Where(i => i.Enabled == true && DateTime.Now.Subtract(i.LastActivityDate).TotalMinutes <= lastMinutes)
                .OrderByDescending(i => i.LastActivityDate)
                .ToList();
        }

        /// <summary>
        /// Gets the servers, that have no activtiy in the specified time frame.
        /// </summary>
        /// <param name="lastMinutes">The number of recent minutes activity had to occur.</param>
        /// <returns>A list of servers with no activity for the specified minutes. Defaults to 24 hours.</returns>
        public static List<ServerInfo> GetInActiveServers(int lastMinutes = 1440)
        {
			Logger.LogDebug(">>> Entering GetInActiveServers(lastMinutes={LastMinutes})", lastMinutes);
			Logger.LogDebug("<<< Exiting GetInActiveServers(lastMinutes={LastMinutes})", lastMinutes);
            return GetServersNoCache()
                .Where(i => DateTime.Now.Subtract(i.LastActivityDate).TotalMinutes > lastMinutes && i.ServerName != Environment.MachineName)
                .OrderByDescending(i => i.LastActivityDate)
                .ToList();
        }

        public static string GetExecutingServerName()
        {
			Logger.LogDebug(">>> Entering GetExecutingServerName()");
            string executingServerName = Globals.ServerName;
            if (UseAppName)
            {
                executingServerName += "-" + Globals.IISAppName;
            }

            Logger.Debug("GetExecutingServerName:" + executingServerName);
			Logger.LogDebug("<<< Exiting GetExecutingServerName()");
            return executingServerName;
        }

        public static string GetServerName(ServerInfo webServer)
        {
			Logger.LogDebug(">>> Entering GetServerName(webServer={WebServer})", webServer);
            string serverName = webServer.ServerName;
            if (UseAppName)
            {
                serverName += "-" + webServer.IISAppName;
            }

            Logger.Debug("GetServerName:" + serverName);
			Logger.LogDebug("<<< Exiting GetServerName(webServer={WebServer})", webServer);
            return serverName;
        }

        /// <summary>Gets the servers.</summary>
        /// <returns>A list of servers.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial List<ServerInfo> GetServers()
            => GetServers(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>());
			Logger.LogDebug(">>> Entering GetServers()");

        /// <summary>Gets the servers.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>A list of servers.</returns>
        public static List<ServerInfo> GetServers(IHostSettings hostSettings)
        {
			Logger.LogDebug("<<< Exiting GetServers()");
            return CBO.GetCachedObject<List<ServerInfo>>(
                hostSettings,
                new CacheItemArgs(CacheKey, CacheTimeout, CachePriority),
                GetServersCallBack);
        }

        public static void UpdateServer(ServerInfo server)
        {
			Logger.LogDebug(">>> Entering UpdateServer(server={Server})", server);
            DataProvider.Instance().UpdateServer(server.ServerID, server.Url, server.UniqueId, server.Enabled, server.ServerGroup);
            ClearCachedServers();
			Logger.LogDebug("<<< Exiting UpdateServer(server={Server})", server);
        }

        public static void UpdateServerActivity(ServerInfo server)
        {
			Logger.LogDebug(">>> Entering UpdateServerActivity(server={Server})", server);
            var allServers = GetServers();
            var existServer = allServers.FirstOrDefault(s => s.ServerName == server.ServerName && s.IISAppName == server.IISAppName);
			Logger.LogDebug("UpdateServerActivity(server={Server}): allServers → {AllServers}", server, allServers);
			Logger.LogDebug("UpdateServerActivity(server={Server}): existServer → {ExistServer}", server, existServer);
			Logger.LogDebug("UpdateServerActivity(server={Server}): server.ServerID → {Arg1}", server, server.ServerID);
            var serverId = DataProvider.Instance().UpdateServerActivity(server.ServerName, server.IISAppName, server.CreatedDate, server.LastActivityDate, server.PingFailureCount, server.Enabled);

			Logger.LogDebug("UpdateServerActivity(server={Server}): serverId → {ServerId}", server, serverId);
            server.ServerID = serverId;
            if (existServer == null
                || string.IsNullOrEmpty(existServer.Url)
                || (string.IsNullOrEmpty(existServer.UniqueId) && !string.IsNullOrEmpty(GetServerUniqueId())))
            {
                // try to detect the server url from url adapter.
                server.Url = existServer == null || string.IsNullOrEmpty(existServer.Url) ? GetServerUrl() : existServer.Url;

                // try to detect the server unique id from url adapter.
                server.UniqueId = existServer == null || string.IsNullOrEmpty(existServer.UniqueId) ? GetServerUniqueId() : existServer.UniqueId;

                UpdateServer(server);
                ClearCachedServers(); // Only clear the cache if we added a server
            }
            else
            {
                // Just update the existing item in the cache
                existServer.LastActivityDate = server.LastActivityDate;
				Logger.LogDebug("UpdateServerActivity(server={Server}): existServer.LastActivityDate → {Arg1}", server, existServer.LastActivityDate);
				Logger.LogDebug("UpdateServerActivity(server={Server}): existServer.Enabled → {Arg1}", server, existServer.Enabled);
                existServer.Enabled = server.Enabled;
                DataCache.SetCache(ServerController.CacheKey, allServers);
				Logger.LogDebug(">>> Entering GetServerUniqueId()");
				Logger.LogDebug("<<< Exiting GetServerUniqueId()");
            }

            // log the server info
            var log = new LogInfo();
            log.AddProperty(existServer != null ? "Server Updated" : "Add New Server", server.ServerName);
			Logger.LogDebug("UpdateServerActivity(server={Server}): log → {Log}", server, log);
            log.AddProperty("IISAppName", server.IISAppName);
            log.AddProperty("Last Activity Date", server.LastActivityDate.ToString(CultureInfo.InvariantCulture));
            log.LogTypeKey = existServer != null ? nameof(EventLogType.WEBSERVER_UPDATED)
                                        : nameof(EventLogType.WEBSERVER_CREATED);
            LogController.Instance.AddLog(log);
			Logger.LogDebug("<<< Exiting UpdateServerActivity(server={Server})", server);
        }

        public static IServerWebRequestAdapter GetServerWebRequestAdapter()
        {
			Logger.LogDebug(">>> Entering GetServerWebRequestAdapter()");
            var adapterConfig = HostController.Instance.GetString("WebServer_ServerRequestAdapter", DefaultUrlAdapter);
            var adapterType = Reflection.CreateType(adapterConfig);
			Logger.LogDebug("GetServerWebRequestAdapter(): adapterConfig → {AdapterConfig}", adapterConfig);
			Logger.LogDebug("GetServerWebRequestAdapter(): adapterType → {AdapterType}", adapterType);
			Logger.LogDebug("<<< Exiting GetServerWebRequestAdapter()");
            return Reflection.CreateInstance(adapterType) as IServerWebRequestAdapter;
        }

        private static List<ServerInfo> GetServersNoCache()
        {
			Logger.LogDebug(">>> Entering GetServersNoCache()");
			Logger.LogDebug("<<< Exiting GetServersNoCache()");
            return CBO.FillCollection<ServerInfo>(DataProvider.GetServers());
        }

        private static object GetServersCallBack(CacheItemArgs cacheItemArgs)
        {
			Logger.LogDebug(">>> Entering GetServersCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
			Logger.LogDebug("<<< Exiting GetServersCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return CBO.FillCollection<ServerInfo>(DataProvider.GetServers());
        }

        private static string GetServerUrl()
        {
			Logger.LogDebug(">>> Entering GetServerUrl()");
            try
            {
                var adpapter = GetServerWebRequestAdapter();
                if (adpapter == null)
				Logger.LogDebug("GetServerUrl(): adpapter → {Adpapter}", adpapter);
                {
                    return string.Empty;
                }

                return adpapter.GetServerUrl();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting GetServerUrl()");
                return string.Empty;
            }
        }

        private static string GetServerUniqueId()
        {
			Logger.LogDebug(">>> Entering GetServerUniqueId()");
            try
            {
                var adpapter = GetServerWebRequestAdapter();
                if (adpapter == null)
				Logger.LogDebug("GetServerUniqueId(): adpapter → {Adpapter}", adpapter);
                {
                    return string.Empty;
                }

                return adpapter.GetServerUniqueId();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return string.Empty;
            }
			Logger.LogDebug("<<< Exiting GetServerUniqueId()");
        }
    }
}
