// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Connectors.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Script.Serialization;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;

    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Connections;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;

    [MenuPermission(MenuName = "Dnn.Connectors")]
    public class ConnectorsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ConnectorsController));
        private readonly IServiceProvider serviceProvider;
        private readonly IConnectionsManager connectionsManager;

        public ConnectorsController(IServiceProvider serviceProvider, IConnectionsManager connectionsManager)
        {
			Logger.LogDebug(">>> Entering ConnectorsController(serviceProvider={ServiceProvider},connectionsManager={ConnectionsManager})", serviceProvider, connectionsManager);
            this.serviceProvider = serviceProvider;
            this.connectionsManager = connectionsManager;
			Logger.LogDebug("<<< Exiting ConnectorsController(serviceProvider={ServiceProvider},connectionsManager={ConnectionsManager})", serviceProvider, connectionsManager);
			Logger.LogDebug("ConnectorsController(serviceProvider={ServiceProvider},connectionsManager={ConnectionsManager}): this.serviceProvider → {Arg2}", serviceProvider, connectionsManager, this.serviceProvider);
			Logger.LogDebug("ConnectorsController(serviceProvider={ServiceProvider},connectionsManager={ConnectionsManager}): connectionsManager → {ConnectionsManager1}", serviceProvider, connectionsManager, connectionsManager);
			Logger.LogDebug("ConnectorsController(serviceProvider={ServiceProvider},connectionsManager={ConnectionsManager}): this.connectionsManager → {Arg2}", serviceProvider, connectionsManager, this.connectionsManager);
			Logger.LogDebug("ConnectorsController(serviceProvider={ServiceProvider},connectionsManager={ConnectionsManager}): serviceProvider → {ServiceProvider1}", serviceProvider, connectionsManager, serviceProvider);
        }

        [HttpGet]
        public HttpResponseMessage GetConnections()
        {
			Logger.LogDebug(">>> Entering GetConnections()");
            var connections = this.GetConnections(this.PortalId);
            connections.ForEach(x =>
            {
			Logger.LogDebug("GetConnections(): connections → {Connections}", connections);
                {
                    x.HasConfig(this.PortalSettings.PortalId);
                }
            });

            var connectionsResponse = connections
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    type = c.Type,
                    displayName = c.DisplayName,
                    connected = c.HasConfig(this.PortalSettings.PortalId),
                    icon = Globals.ResolveUrl(c.IconUrl),
                    pluginFolder = Globals.ResolveUrl(c.PluginFolder),
                    configurations = c.GetConfig(this.PortalSettings.PortalId),
                    supportsMultiple = c.SupportsMultiple,
                })
                .OrderBy(connection => connection.name)
                .ToList();
					Logger.LogDebug("GetConnections(): name → {Name}", name);
					Logger.LogDebug("GetConnections(): type → {Type}", type);
					Logger.LogDebug("GetConnections(): displayName → {DisplayName}", displayName);
					Logger.LogDebug("GetConnections(): connected → {Connected}", connected);
					Logger.LogDebug("GetConnections(): icon → {Icon}", icon);
					Logger.LogDebug("GetConnections(): pluginFolder → {PluginFolder}", pluginFolder);
					Logger.LogDebug("GetConnections(): configurations → {Configurations}", configurations);
					Logger.LogDebug("GetConnections(): supportsMultiple → {SupportsMultiple}", supportsMultiple);
			Logger.LogDebug("<<< Exiting GetConnections()");
            return this.Request.CreateResponse(HttpStatusCode.OK, connectionsResponse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveConnection(object postData)
        {
			Logger.LogDebug(">>> Entering SaveConnection(postData={PostData})", postData);
            try
            {
                var jsonData = DotNetNuke.Common.Utilities.Json.Serialize(postData);
                var serializer = new JavaScriptSerializer();
				Logger.LogDebug("SaveConnection(postData={PostData}): jsonData → {JsonData}", postData, jsonData);
				Logger.LogDebug("SaveConnection(postData={PostData}): serializer → {Serializer}", postData, serializer);
                serializer.RegisterConverters([new DynamicJsonConverter(),]);

                dynamic postObject = serializer.Deserialize<object>(jsonData);

                var name = postObject.name;
                var displayName = postObject.displayName;
				Logger.LogDebug("SaveConnection(postData={PostData}): name → {Name}", postData, name);
                var id = postObject.id;
                var connectors = this.GetConnections(this.PortalId);
				Logger.LogDebug("SaveConnection(postData={PostData}): id → {Id}", postData, id);
				Logger.LogDebug("SaveConnection(postData={PostData}): connectors → {Connectors}", postData, connectors);
                connectors.ForEach(x =>
                    {
                        {
                            x.HasConfig(this.PortalSettings.PortalId);
                        }
                    });

                connectors = connectors.Where(
                            c =>
                                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();

                var connector = connectors.FirstOrDefault(c =>
                    (c.SupportsMultiple && (c.Id == id || string.IsNullOrEmpty(c.Id))) ||
                    !c.SupportsMultiple);
					Logger.LogDebug("SaveConnection(postData={PostData}): connector → {Connector}", postData, connector);

                if (connector == null && string.IsNullOrEmpty(id) && connectors.Any(x => x.SupportsMultiple))
                {
                    connector = connectors.First();
                    connector.Id = null;
					Logger.LogDebug("SaveConnection(postData={PostData}): connector.Id → {Arg1}", postData, connector.Id);
					Logger.LogDebug("SaveConnection(postData={PostData}): connector.DisplayName → {Arg1}", postData, connector.DisplayName);
                    connector.DisplayName = null;
                }

                if (connector != null && !string.IsNullOrEmpty(displayName) && connector.DisplayName != displayName)
                {
                    connector.DisplayName = string.IsNullOrEmpty(displayName) ? string.Empty : displayName;
                }

                bool validated = false;
                if (connector != null)
                {
                    var configs = GetConfigAsDictionary(postObject.configurations);
                    string customErrorMessage;
					Logger.LogDebug("SaveConnection(postData={PostData}): configs → {Configs}", postData, configs);
                    var saved = connector.SaveConfig(this.PortalSettings.PortalId, configs, ref validated, out customErrorMessage);

					Logger.LogDebug("SaveConnection(postData={PostData}): saved → {Saved}", postData, saved);
                    if (!saved)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            Success = false,
                            Validated = validated,
                            Message = string.IsNullOrEmpty(customErrorMessage)
                                ? Localization.GetString("ErrSavingConnectorSettings.Text", Constants.SharedResources)
                                : customErrorMessage,
                        });
							Logger.LogDebug("SaveConnection(postData={PostData}): Success → {Success}", postData, Success);
							Logger.LogDebug("SaveConnection(postData={PostData}): Validated → {Validated}", postData, Validated);
							Logger.LogDebug("SaveConnection(postData={PostData}): Message → {Message}", postData, Message);
                    }
                }

                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    new
                    {
                        Success = true,
                        Validated = validated,
                        connector?.Id,
                    });
            }
            catch (Exception ex)
            {
                if (ex is ConnectorArgumentException)
                {
                    Logger.Warn(ex);
                }
                else
                {
                    Logger.Error(ex);
                }

				Logger.LogDebug("<<< Exiting SaveConnection(postData={PostData})", postData);
                return this.Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    new { Success = false, Message = ex.Message });
            }
        }

        /// <summary>Delete a connector. Supported only for connectors with SupportsMultiple=true.</summary>
        /// <param name="postData">The connection name and ID.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteConnection(object postData)
        {
			Logger.LogDebug(">>> Entering DeleteConnection(postData={PostData})", postData);
            try
            {
                var jsonData = DotNetNuke.Common.Utilities.Json.Serialize(postData);
                var serializer = new JavaScriptSerializer();
				Logger.LogDebug("DeleteConnection(postData={PostData}): jsonData → {JsonData}", postData, jsonData);
				Logger.LogDebug("DeleteConnection(postData={PostData}): serializer → {Serializer}", postData, serializer);
                serializer.RegisterConverters([new DynamicJsonConverter(),]);
                dynamic postObject = serializer.Deserialize<object>(jsonData);

                var name = postObject.name;
                var id = postObject.id;
				Logger.LogDebug("DeleteConnection(postData={PostData}): name → {Name}", postData, name);
				Logger.LogDebug("DeleteConnection(postData={PostData}): id → {Id}", postData, id);
                var connectors = this.GetConnections(this.PortalId);
                connectors.ForEach(x =>
                {
				Logger.LogDebug("DeleteConnection(postData={PostData}): connectors → {Connectors}", postData, connectors);
                    {
                        x.HasConfig(this.PortalSettings.PortalId);
                    }
                });
                connectors = connectors.Where(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();

                var connector = connectors.FirstOrDefault(c => c.SupportsMultiple && c.Id == id);
                if (connector != null)
				Logger.LogDebug("DeleteConnection(postData={PostData}): connector → {Connector}", postData, connector);
                {
                    connector.DeleteConnector(this.PortalId);
                    return this.Request.CreateResponse(
                        HttpStatusCode.OK,
                        new
                        {
                            Success = true,
                        });
							Logger.LogDebug("DeleteConnection(postData={PostData}): Success → {Success}", postData, Success);
                }

                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    new
                    {
                        Success = true,
                        Message =
                            Localization.GetString("ErrConnectorNotFound.Text", Components.Constants.LocalResourcesFile),
                    });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting DeleteConnection(postData={PostData})", postData);
                return this.Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet]
        public HttpResponseMessage GetConnectionLocalizedString(string name, string culture)
        {
            var connections = this.GetConnections(this.PortalId);
            connections.ForEach(x =>
            {
                {
                    x.HasConfig(this.PortalSettings.PortalId);
                }
            });
            var connection = connections.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (connection == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var resourceFile = $"{connection.PluginFolder}/App_LocalResources/SharedResources.resx";
            IDictionary<string, string> localizedStrings;
            try
            {
                localizedStrings = Library.Controllers.LocalizationController.Instance.GetLocalizedDictionary(resourceFile, culture);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                localizedStrings = new Dictionary<string, string>();
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, localizedStrings);
        }

        private static Dictionary<string, string> GetConfigAsDictionary(dynamic configurations)
        {
            var configs = new Dictionary<string, string>();

            for (var i = 0; i < configurations.Count; i++)
            {
                var c = configurations[i];
                if (!configs.ContainsKey(c.name))
                {
                    configs.Add(c.name, c.value);
                }
                else
                {
                    configs[c.name] = c.value;
                }
            }

            return configs;
        }

        private List<IConnector> GetConnections(int portalId)
        {
            var connectors = this.connectionsManager.GetConnectors(this.serviceProvider);
            var allConnectors = new List<IConnector>();
            foreach (var con in connectors)
            {
                allConnectors.AddRange(con.GetConnectors(portalId));
            }

			Logger.LogDebug("<<< Exiting GetConnections(portalId={PortalId})", portalId);
            return allConnectors;
        }
    }
}
