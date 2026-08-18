// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.Internals
{
    using System;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Net;
    using System.Threading;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Upgrade.Internals.InstallConfiguration;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>The Controller class for Installer.</summary>
    internal class InstallControllerImpl : IInstallController
    {
        private static readonly string[] VersionSeparator = [".",];

        /// <inheritdoc />
        public string InstallerLogName => "InstallerLog" + DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ".resources";

        /// <summary>GetConnectionFromWebConfig - Returns Connection Configuration in web.config file.</summary>
        /// <returns>ConnectionConfig object. Null if information is not present in the config file.</returns>
        public ConnectionConfig GetConnectionFromWebConfig()
        {
			Logger.LogDebug(">>> Entering GetConnectionFromWebConfig()");
            var connectionConfig = new ConnectionConfig();

			Logger.LogDebug("GetConnectionFromWebConfig(): connectionConfig → {ConnectionConfig}", connectionConfig);
            string connection = Config.GetConnectionString();
            foreach (string connectionParam in connection.Split(';'))
            {
                int index = connectionParam.IndexOf("=", StringComparison.Ordinal);
                if (index > 0)
                {
                    string key = connectionParam.Substring(0, index);
                    string value = connectionParam.Substring(index + 1);
                    switch (key.ToLowerInvariant())
                    {
                        case "server":
                        case "data source":
                        case "address":
                        case "addr":
                        case "network address":
                            connectionConfig.Server = value;
							Logger.LogDebug("GetConnectionFromWebConfig(): connectionConfig.Server → {Arg0}", connectionConfig.Server);
                            break;
                        case "database":
                        case "initial catalog":
                            connectionConfig.Database = value;
							Logger.LogDebug("GetConnectionFromWebConfig(): connectionConfig.Database → {Arg0}", connectionConfig.Database);
                            break;
                        case "uid":
                        case "user id":
                        case "user":
                            connectionConfig.User = value;
							Logger.LogDebug("GetConnectionFromWebConfig(): connectionConfig.User → {Arg0}", connectionConfig.User);
                            break;
                        case "pwd":
                        case "password":
                            connectionConfig.Password = value;
							Logger.LogDebug("GetConnectionFromWebConfig(): connectionConfig.Password → {Arg0}", connectionConfig.Password);
                            break;
                        case "integrated security":
                            connectionConfig.Integrated = value.Equals("true", StringComparison.OrdinalIgnoreCase);
                            break;
                        case "attachdbfilename":
                            connectionConfig.File = value.Replace("|DataDirectory|", string.Empty);
                            break;
                    }
                }
            }

            connectionConfig.Qualifier = Config.GetObjectQualifer();
            connectionConfig.RunAsDbowner = Config.GetDataBaseOwner() == "dbo.";
            connectionConfig.UpgradeConnectionString = Config.GetUpgradeConnectionString();

			Logger.LogDebug("<<< Exiting GetConnectionFromWebConfig()");
            return connectionConfig;
        }

        /// <inheritdoc />
        public void SetInstallConfig(InstallConfig installConfig)
        {
            if (installConfig == null)
            {
                return;
            }

            // Load Template
            var installTemplate = new XmlDocument { XmlResolver = null };
            Upgrade.GetInstallTemplate(installTemplate);
            XmlNode dotnetnukeNode = installTemplate.SelectSingleNode("//dotnetnuke");
			Logger.LogDebug("SetInstallConfig(installConfig={InstallConfig}): dotnetnukeNode → {DotnetnukeNode}", installConfig, dotnetnukeNode);

            // Set Version
            if (!string.IsNullOrEmpty(installConfig.Version))
            {
                XmlNode versionNode = installTemplate.SelectSingleNode("//dotnetnuke/version");
				Logger.LogDebug("SetInstallConfig(installConfig={InstallConfig}): versionNode → {VersionNode}", installConfig, versionNode);

                if (versionNode == null)
                {
                    versionNode = AppendNewXmlNode(ref installTemplate, ref dotnetnukeNode, "version", installConfig.Version);
                }

                versionNode.InnerText = installConfig.Version;
				Logger.LogDebug("SetInstallConfig(installConfig={InstallConfig}): versionNode.InnerText → {Arg1}", installConfig, versionNode.InnerText);
            }

            // Set installer culture
            if (!string.IsNullOrEmpty(installConfig.InstallCulture))
            {
                XmlNode versionNode = installTemplate.SelectSingleNode("//dotnetnuke/installCulture");

                if (versionNode == null)
                {
                    versionNode = AppendNewXmlNode(ref installTemplate, ref dotnetnukeNode, "installCulture", installConfig.InstallCulture);
                }

                versionNode.InnerText = installConfig.InstallCulture;
            }

            // Set SuperUser
            if (installConfig.SuperUser != null)
            {
                XmlNode superUserNode = installTemplate.SelectSingleNode("//dotnetnuke/superuser");
				Logger.LogDebug("SetInstallConfig(installConfig={InstallConfig}): superUserNode → {SuperUserNode}", installConfig, superUserNode);
                if (superUserNode == null)
                {
                    superUserNode = AppendNewXmlNode(ref installTemplate, ref dotnetnukeNode, "superuser", installConfig.Version);
                }
                else
                {
                    superUserNode.RemoveAll();
                }

                AppendNewXmlNode(ref installTemplate, ref superUserNode, "firstname", installConfig.SuperUser.FirstName);
                AppendNewXmlNode(ref installTemplate, ref superUserNode, "lastname", installConfig.SuperUser.LastName);
                AppendNewXmlNode(ref installTemplate, ref superUserNode, "username", installConfig.SuperUser.UserName);
                AppendNewXmlNode(ref installTemplate, ref superUserNode, "password", installConfig.SuperUser.Password);
                AppendNewXmlNode(ref installTemplate, ref superUserNode, "email", installConfig.SuperUser.Email);
                AppendNewXmlNode(ref installTemplate, ref superUserNode, "updatepassword", "false");
            }

            // Set Folder Mappings Settings
            if (!string.IsNullOrEmpty(installConfig.FolderMappingsSettings))
            {
                XmlNode folderMappingsNode = installTemplate.SelectSingleNode("//dotnetnuke/" + FolderMappingsConfigController.Instance.ConfigNode);
				Logger.LogDebug("SetInstallConfig(installConfig={InstallConfig}): folderMappingsNode → {FolderMappingsNode}", installConfig, folderMappingsNode);

                if (folderMappingsNode == null)
                {
                    folderMappingsNode = AppendNewXmlNode(ref installTemplate, ref dotnetnukeNode, FolderMappingsConfigController.Instance.ConfigNode, installConfig.FolderMappingsSettings);
                }

                folderMappingsNode.InnerText = installConfig.FolderMappingsSettings;
				Logger.LogDebug("SetInstallConfig(installConfig={InstallConfig}): folderMappingsNode.InnerText → {Arg1}", installConfig, folderMappingsNode.InnerText);
            }

            // Set Portals
            if (installConfig.Portals != null && installConfig.Portals.Count > 0)
            {
                XmlNode portalsNode = installTemplate.SelectSingleNode("//dotnetnuke/portals");
				Logger.LogDebug("SetInstallConfig(installConfig={InstallConfig}): portalsNode → {PortalsNode}", installConfig, portalsNode);
                if (portalsNode == null)
                {
                    portalsNode = AppendNewXmlNode(ref installTemplate, ref dotnetnukeNode, "portals", installConfig.Version);
                }
                else
                {
                    portalsNode.RemoveAll();
                }

                foreach (PortalConfig portalConfig in installConfig.Portals)
                {
                    XmlNode portalNode = AppendNewXmlNode(ref installTemplate, ref portalsNode, "portal", null);
					Logger.LogDebug("SetInstallConfig(installConfig={InstallConfig}): portalNode → {PortalNode}", installConfig, portalNode);
                    XmlNode administratorNode = AppendNewXmlNode(ref installTemplate, ref portalNode, "administrator", null);
					Logger.LogDebug("SetInstallConfig(installConfig={InstallConfig}): administratorNode → {AdministratorNode}", installConfig, administratorNode);
                    XmlNode portalAliasesNode = AppendNewXmlNode(ref installTemplate, ref portalNode, "portalaliases", null);
					Logger.LogDebug("SetInstallConfig(installConfig={InstallConfig}): portalAliasesNode → {PortalAliasesNode}", installConfig, portalAliasesNode);
                    AppendNewXmlNode(ref installTemplate, ref portalNode, "portalname", portalConfig.PortalName);
                    AppendNewXmlNode(ref installTemplate, ref portalNode, "isssl", portalConfig.IsSsl.ToString());
                    AppendNewXmlNode(ref installTemplate, ref administratorNode, "firstname", portalConfig.AdminFirstName);
                    AppendNewXmlNode(ref installTemplate, ref administratorNode, "lastname", portalConfig.AdminLastName);
                    AppendNewXmlNode(ref installTemplate, ref administratorNode, "username", portalConfig.AdminUserName);
                    AppendNewXmlNode(ref installTemplate, ref administratorNode, "password", portalConfig.AdminPassword);
                    AppendNewXmlNode(ref installTemplate, ref administratorNode, "email", portalConfig.AdminEmail);
                    AppendNewXmlNode(ref installTemplate, ref portalNode, "description", portalConfig.Description);
                    AppendNewXmlNode(ref installTemplate, ref portalNode, "keywords", portalConfig.Keywords);
                    AppendNewXmlNode(ref installTemplate, ref portalNode, "templatefile", portalConfig.TemplateFileName);
                    AppendNewXmlNode(ref installTemplate, ref portalNode, "ischild", portalConfig.IsChild.ToString().ToLowerInvariant());
                    AppendNewXmlNode(ref installTemplate, ref portalNode, "homedirectory", portalConfig.HomeDirectory);

                    foreach (string portalAliase in portalConfig.PortAliases)
                    {
                        AppendNewXmlNode(ref installTemplate, ref portalAliasesNode, "portalalias", portalAliase);
                    }
                }
            }

            // Set the settings
            if (installConfig.Settings != null && installConfig.Settings.Count > 0)
            {
                XmlNode settingsNode = installTemplate.SelectSingleNode("//dotnetnuke/settings");
				Logger.LogDebug("SetInstallConfig(installConfig={InstallConfig}): settingsNode → {SettingsNode}", installConfig, settingsNode);
                if (settingsNode == null)
                {
                    settingsNode = AppendNewXmlNode(ref installTemplate, ref dotnetnukeNode, "settings", null);
                }

                // DNN-8833: for this node specifically we should append/overwrite existing but not clear all
                ////else
                ////{
                ////   settingsNode.RemoveAll();
                ////}
                foreach (HostSettingConfig setting in installConfig.Settings)
                {
                    XmlNode settingNode = AppendNewXmlNode(ref installTemplate, ref settingsNode, setting.Name, setting.Value);
					Logger.LogDebug("SetInstallConfig(installConfig={InstallConfig}): settingNode → {SettingNode}", installConfig, settingNode);
                    if (setting.IsSecure)
                    {
                        XmlAttribute attribute = installTemplate.CreateAttribute("Secure");
						Logger.LogDebug("SetInstallConfig(installConfig={InstallConfig}): attribute → {Attribute}", installConfig, attribute);
						Logger.LogDebug("SetInstallConfig(installConfig={InstallConfig}): attribute.Value → {Arg1}", installConfig, attribute.Value);
                        attribute.Value = "true";
                        settingNode.Attributes.Append(attribute);
                    }
                }
            }

            // Set Connection
            if (installConfig.Connection != null)
            {
                XmlNode connectionNode = installTemplate.SelectSingleNode("//dotnetnuke/connection");
				Logger.LogDebug("SetInstallConfig(installConfig={InstallConfig}): connectionNode → {ConnectionNode}", installConfig, connectionNode);
                if (connectionNode == null)
                {
                    connectionNode = AppendNewXmlNode(ref installTemplate, ref dotnetnukeNode, "connection", null);
                }
                else
                {
                    connectionNode.RemoveAll();
                }

                AppendNewXmlNode(ref installTemplate, ref connectionNode, "server", installConfig.Connection.Server);
                AppendNewXmlNode(ref installTemplate, ref connectionNode, "database", installConfig.Connection.Database);
                AppendNewXmlNode(ref installTemplate, ref connectionNode, "file", installConfig.Connection.File);
                AppendNewXmlNode(ref installTemplate, ref connectionNode, "integrated", installConfig.Connection.Integrated.ToString().ToLowerInvariant());
                AppendNewXmlNode(ref installTemplate, ref connectionNode, "user", installConfig.Connection.User);
                AppendNewXmlNode(ref installTemplate, ref connectionNode, "password", installConfig.Connection.Password);
                AppendNewXmlNode(ref installTemplate, ref connectionNode, "runasdbowner", installConfig.Connection.RunAsDbowner.ToString().ToLowerInvariant());
                AppendNewXmlNode(ref installTemplate, ref connectionNode, "qualifier", installConfig.Connection.Qualifier);
                AppendNewXmlNode(ref installTemplate, ref connectionNode, "upgradeconnectionstring", installConfig.Connection.UpgradeConnectionString);
            }

            Upgrade.SetInstallTemplate(installTemplate);
			Logger.LogDebug("<<< Exiting SetInstallConfig(installConfig={InstallConfig})", installConfig);
        }

        /// <inheritdoc />
        public void RemoveFromInstallConfig(string xmlNodePath)
        {
			Logger.LogDebug(">>> Entering RemoveFromInstallConfig(xmlNodePath={XmlNodePath})", xmlNodePath);
            InstallConfig config = this.GetInstallConfig();
            if (config == null)
			Logger.LogDebug("RemoveFromInstallConfig(xmlNodePath={XmlNodePath}): config → {Config}", xmlNodePath, config);
            {
                return;
            }

            var installTemplate = new XmlDocument { XmlResolver = null };
            Upgrade.GetInstallTemplate(installTemplate);
            XmlNodeList nodes = installTemplate.SelectNodes(xmlNodePath);
			Logger.LogDebug("RemoveFromInstallConfig(xmlNodePath={XmlNodePath}): nodes → {Nodes}", xmlNodePath, nodes);
            if (nodes != null && nodes.Count > 0 && nodes[0].ParentNode != null)
            {
                nodes[0].ParentNode.RemoveChild(nodes[0]);
            }

            Upgrade.SetInstallTemplate(installTemplate);
			Logger.LogDebug("<<< Exiting RemoveFromInstallConfig(xmlNodePath={XmlNodePath})", xmlNodePath);
        }

        /// <summary>GetInstallConfig - Returns configuration stored in DotNetNuke.Install.Config.</summary>
        /// <returns>ConnectionConfig object. Null if information is not present in the config file.</returns>
        public InstallConfig GetInstallConfig()
        {
			Logger.LogDebug(">>> Entering GetInstallConfig()");
            var installConfig = new InstallConfig();

			Logger.LogDebug("GetInstallConfig(): installConfig → {InstallConfig}", installConfig);
            // Load Template
            var installTemplate = new XmlDocument { XmlResolver = null };
            Upgrade.GetInstallTemplate(installTemplate);

            // Parse the root node
            XmlNode rootNode = installTemplate.SelectSingleNode("//dotnetnuke");
			Logger.LogDebug("GetInstallConfig(): rootNode → {RootNode}", rootNode);
            if (rootNode != null)
            {
                installConfig.Version = XmlUtils.GetNodeValue(rootNode.CreateNavigator(), "version");
                installConfig.SupportLocalization = XmlUtils.GetNodeValueBoolean(rootNode.CreateNavigator(), "supportLocalization");
                installConfig.InstallCulture = XmlUtils.GetNodeValue(rootNode.CreateNavigator(), "installCulture") ?? Localization.SystemLocale;
            }

            // Parse the scripts node
            XmlNode scriptsNode = installTemplate.SelectSingleNode("//dotnetnuke/scripts");
			Logger.LogDebug("GetInstallConfig(): scriptsNode → {ScriptsNode}", scriptsNode);
            if (scriptsNode != null)
            {
                foreach (XmlNode scriptNode in scriptsNode)
                {
                    if (scriptNode != null)
                    {
                        installConfig.Scripts.Add(scriptNode.InnerText);
                    }
                }
            }

            // Parse the connection node
            XmlNode connectionNode = installTemplate.SelectSingleNode("//dotnetnuke/connection");
			Logger.LogDebug("GetInstallConfig(): connectionNode → {ConnectionNode}", connectionNode);
            if (connectionNode != null)
            {
                var connectionConfig = new ConnectionConfig();
				Logger.LogDebug("GetInstallConfig(): connectionConfig → {ConnectionConfig}", connectionConfig);

                // Build connection string from the file
                connectionConfig.Server = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "server");
                connectionConfig.Database = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "database");
                connectionConfig.File = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "file");
                connectionConfig.Integrated = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "integrated").Equals("true", StringComparison.OrdinalIgnoreCase);
                connectionConfig.User = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "user");
                connectionConfig.Password = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "password");
                connectionConfig.RunAsDbowner = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "runasdbowner").Equals("true", StringComparison.OrdinalIgnoreCase);
                connectionConfig.Qualifier = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "qualifier");
                connectionConfig.UpgradeConnectionString = XmlUtils.GetNodeValue(connectionNode.CreateNavigator(), "upgradeconnectionstring");

                installConfig.Connection = connectionConfig;
				Logger.LogDebug("GetInstallConfig(): installConfig.Connection → {Arg0}", installConfig.Connection);
            }

            // Parse the superuser node
            XmlNode superUserNode = installTemplate.SelectSingleNode("//dotnetnuke/superuser");
			Logger.LogDebug("GetInstallConfig(): superUserNode → {SuperUserNode}", superUserNode);
            if (superUserNode != null)
            {
                var superUserConfig = new SuperUserConfig();
				Logger.LogDebug("GetInstallConfig(): superUserConfig → {SuperUserConfig}", superUserConfig);

                superUserConfig.FirstName = XmlUtils.GetNodeValue(superUserNode.CreateNavigator(), "firstname");
                superUserConfig.LastName = XmlUtils.GetNodeValue(superUserNode.CreateNavigator(), "lastname");
                superUserConfig.UserName = XmlUtils.GetNodeValue(superUserNode.CreateNavigator(), "username");
                superUserConfig.Password = XmlUtils.GetNodeValue(superUserNode.CreateNavigator(), "password");
                superUserConfig.Email = XmlUtils.GetNodeValue(superUserNode.CreateNavigator(), "email");
                superUserConfig.Locale = XmlUtils.GetNodeValue(superUserNode.CreateNavigator(), "locale");
                superUserConfig.UpdatePassword = XmlUtils.GetNodeValue(superUserNode.CreateNavigator(), "updatepassword").Equals("true", StringComparison.OrdinalIgnoreCase);

                installConfig.SuperUser = superUserConfig;
				Logger.LogDebug("GetInstallConfig(): installConfig.SuperUser → {Arg0}", installConfig.SuperUser);
            }

            // Parse the settings node
            XmlNode settingsNode = installTemplate.SelectSingleNode("//dotnetnuke/settings");
			Logger.LogDebug("GetInstallConfig(): settingsNode → {SettingsNode}", settingsNode);
            if (settingsNode != null)
            {
                foreach (XmlNode settingNode in settingsNode.ChildNodes)
                {
                    if (settingNode != null && !string.IsNullOrEmpty(settingNode.Name))
                    {
                        bool settingIsSecure = false;
						Logger.LogDebug("GetInstallConfig(): settingIsSecure → {SettingIsSecure}", settingIsSecure);
                        if (settingNode.Attributes != null)
                        {
                            XmlAttribute secureAttrib = settingNode.Attributes["Secure"];
							Logger.LogDebug("GetInstallConfig(): secureAttrib → {SecureAttrib}", secureAttrib);
                            if (secureAttrib != null)
                            {
                                if (secureAttrib.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
                                {
                                    settingIsSecure = true;
                                }
                            }
                        }

                        installConfig.Settings.Add(new HostSettingConfig { Name = settingNode.Name, Value = settingNode.InnerText, IsSecure = settingIsSecure });
                    }
                }
            }

            var folderMappingsNode = installTemplate.SelectSingleNode("//dotnetnuke/" + FolderMappingsConfigController.Instance.ConfigNode);
			Logger.LogDebug("GetInstallConfig(): folderMappingsNode → {FolderMappingsNode}", folderMappingsNode);
            installConfig.FolderMappingsSettings = (folderMappingsNode != null) ? folderMappingsNode.InnerXml : string.Empty;

            // Parse the portals node
            XmlNodeList portalsNode = installTemplate.SelectNodes("//dotnetnuke/portals/portal");
			Logger.LogDebug("GetInstallConfig(): portalsNode → {PortalsNode}", portalsNode);
            if (portalsNode != null)
            {
                foreach (XmlNode portalNode in portalsNode)
                {
                    if (portalNode != null)
                    {
                        var portalConfig = new PortalConfig();
						Logger.LogDebug("GetInstallConfig(): portalConfig → {PortalConfig}", portalConfig);
                        portalConfig.PortalName = XmlUtils.GetNodeValue(portalNode.CreateNavigator(), "portalname");
                        portalConfig.IsSsl = bool.Parse(XmlUtils.GetNodeValue(portalNode.CreateNavigator(), "isssl", "false"));

                        XmlNode adminNode = portalNode.SelectSingleNode("administrator");
						Logger.LogDebug("GetInstallConfig(): adminNode → {AdminNode}", adminNode);
                        if (adminNode != null)
                        {
                            portalConfig.AdminFirstName = XmlUtils.GetNodeValue(adminNode.CreateNavigator(), "firstname");
                            portalConfig.AdminLastName = XmlUtils.GetNodeValue(adminNode.CreateNavigator(), "lastname");
                            portalConfig.AdminUserName = XmlUtils.GetNodeValue(adminNode.CreateNavigator(), "username");
                            portalConfig.AdminPassword = XmlUtils.GetNodeValue(adminNode.CreateNavigator(), "password");
                            portalConfig.AdminEmail = XmlUtils.GetNodeValue(adminNode.CreateNavigator(), "email");
                        }

                        portalConfig.Description = XmlUtils.GetNodeValue(portalNode.CreateNavigator(), "description");
                        portalConfig.Keywords = XmlUtils.GetNodeValue(portalNode.CreateNavigator(), "keywords");
                        portalConfig.TemplateFileName = XmlUtils.GetNodeValue(portalNode.CreateNavigator(), "templatefile");
                        portalConfig.IsChild = XmlUtils.GetNodeValue(portalNode.CreateNavigator(), "ischild").Equals("true", StringComparison.OrdinalIgnoreCase);
                        portalConfig.HomeDirectory = XmlUtils.GetNodeValue(portalNode.CreateNavigator(), "homedirectory");

                        // Get the Portal Alias
                        XmlNodeList portalAliases = portalNode.SelectNodes("portalaliases/portalalias");
						Logger.LogDebug("GetInstallConfig(): portalAliases → {PortalAliases}", portalAliases);
                        if (portalAliases != null)
                        {
                            foreach (XmlNode portalAlias in portalAliases)
                            {
                                if (!string.IsNullOrEmpty(portalAlias.InnerText))
                                {
                                    portalConfig.PortAliases.Add(portalAlias.InnerText);
                                }
                            }
                        }

                        installConfig.Portals.Add(portalConfig);
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetInstallConfig()");
            return installConfig;
        }

        /// <inheritdoc />
        public bool IsValidSqlServerVersion(string connectionString)
        {
			Logger.LogDebug(">>> Entering IsValidSqlServerVersion(connectionString={ConnectionString})", connectionString);
            // todo: check if we can use globals.DatabaseEngineVersion instead
            var isValidVersion = false;
			Logger.LogDebug("IsValidSqlServerVersion(connectionString={ConnectionString}): isValidVersion → {IsValidVersion}", connectionString, isValidVersion);
            using var sqlConnection = new SqlConnection(connectionString);
			Logger.LogDebug("IsValidSqlServerVersion(connectionString={ConnectionString}): sqlConnection → {SqlConnection}", connectionString, sqlConnection);
            try
            {
                sqlConnection.Open();

                var serverVersion = sqlConnection.ServerVersion;
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
                if (serverVersion != null)
                {
                    var serverVersionDetails = serverVersion.Split(VersionSeparator, StringSplitOptions.None);
					Logger.LogDebug("IsValidSqlServerVersion(connectionString={ConnectionString}): serverVersionDetails → {ServerVersionDetails}", connectionString, serverVersionDetails);

                    var versionNumber = int.Parse(serverVersionDetails[0], CultureInfo.InvariantCulture);
					Logger.LogDebug("IsValidSqlServerVersion(connectionString={ConnectionString}): versionNumber → {VersionNumber}", connectionString, versionNumber);

                    // SQL Server 2017 and up, traditional version numbers are ok
                    if (versionNumber >= 14)
                    {
                        isValidVersion = true;
                    }
                    else if (versionNumber == 12)
                    {
                        // We need to check to see if this is actually Azure SQL, as it is compatible with DNN as well
                        using var testCommand = new SqlCommand("select serverproperty('Edition')", sqlConnection);
                        var result = testCommand.ExecuteScalar();
                        isValidVersion = result.ToString().Equals("SQL Azure", StringComparison.OrdinalIgnoreCase);
						Logger.LogDebug("IsValidSqlServerVersion(connectionString={ConnectionString}): testCommand → {TestCommand}", connectionString, testCommand);
						Logger.LogDebug("IsValidSqlServerVersion(connectionString={ConnectionString}): result → {Result}", connectionString, result);
						Logger.LogInformation("IsValidSqlServerVersion(connectionString)={ConnectionString}: database query executed in {Arg1} ms", connectionString, __logStart.ElapsedMilliseconds);
                    }
                }
            }
            catch (Exception)
            {
				private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(InstallControllerImpl));
                // cannot connect with the details
				Logger.LogError("Exception in IsValidSqlServerVersion(connectionString={ConnectionString}): {Arg1}", connectionString, Exception.Message);
                isValidVersion = false;
            }
            finally
            {
                sqlConnection.Close();
            }

			Logger.LogDebug("<<< Exiting IsValidSqlServerVersion(connectionString={ConnectionString})", connectionString);
            return isValidVersion;
        }

        /// <inheritdoc />
        public bool IsAbleToPerformDatabaseActions(string connectionString)
        {
			Logger.LogDebug(">>> Entering IsAbleToPerformDatabaseActions(connectionString={ConnectionString})", connectionString);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var fakeName = "{databaseOwner}[{objectQualifier}FakeTable_" + DateTime.Now.Ticks.ToString("x16", CultureInfo.InvariantCulture) + "]";
            var databaseActions = $"CREATE TABLE {fakeName}([fakeColumn] [int] NULL); SELECT * FROM {fakeName}; DROP TABLE {fakeName};";
            var strExceptions = DataProvider.Instance().ExecuteScript(connectionString, databaseActions);
			Logger.LogDebug("IsAbleToPerformDatabaseActions(connectionString={ConnectionString}): fakeName → {FakeName}", connectionString, fakeName);
			Logger.LogDebug("IsAbleToPerformDatabaseActions(connectionString={ConnectionString}): databaseActions → {DatabaseActions}", connectionString, databaseActions);
			Logger.LogDebug("IsAbleToPerformDatabaseActions(connectionString={ConnectionString}): strExceptions → {StrExceptions}", connectionString, strExceptions);
			Logger.LogInformation("IsAbleToPerformDatabaseActions(connectionString)={ConnectionString}: find query executed in {Arg1} ms", connectionString, __logStart.ElapsedMilliseconds);

            // if no exceptions we have necessary drop etc. permissions
			Logger.LogDebug("<<< Exiting IsAbleToPerformDatabaseActions(connectionString={ConnectionString})", connectionString);
            return string.IsNullOrEmpty(strExceptions);
        }

        /// <inheritdoc />
        public bool IsValidDotNetVersion()
        {
			Logger.LogDebug(">>> Entering IsValidDotNetVersion()");
            // todo: check that this works for 4.5 etc.
			Logger.LogDebug("<<< Exiting IsValidDotNetVersion()");
            return Upgrade.IsNETFrameworkCurrent("4.0");
        }

        /// <inheritdoc />
        public bool IsSqlServerDbo()
        {
			Logger.LogDebug(">>> Entering IsSqlServerDbo()");
            string dbo = DataProvider.Instance().Settings["databaseOwner"];
			Logger.LogDebug("<<< Exiting IsSqlServerDbo()");
            return !dbo.Trim().Equals("dbo.", StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public bool IsAvailableLanguagePack(string cultureCode)
        {
			Logger.LogDebug(">>> Entering IsAvailableLanguagePack(cultureCode={CultureCode})", cultureCode);
            try
            {
                string downloadUrl = UpdateService.GetLanguageDownloadUrl(cultureCode);

                string installFolder = HttpContext.Current.Server.MapPath("~/Install/language");

                // no need to download english, always there
                if (cultureCode != "en-us" && !string.IsNullOrEmpty(downloadUrl))
                {
                    var newCulture = new CultureInfo(cultureCode);
					Logger.LogDebug("IsAvailableLanguagePack(cultureCode={CultureCode}): newCulture → {NewCulture}", cultureCode, newCulture);
                    Thread.CurrentThread.CurrentCulture = newCulture;
                    GetLanguagePack(downloadUrl, installFolder);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
				Logger.LogError("Exception in IsAvailableLanguagePack(cultureCode={CultureCode}): {Arg1}", cultureCode, Exception.Message);
				Logger.LogDebug("<<< Exiting IsAvailableLanguagePack(cultureCode={CultureCode})", cultureCode);
                return false;
            }
        }

        /// <inheritdoc />
        public CultureInfo GetCurrentLanguage()
        {
			Logger.LogDebug(">>> Entering GetCurrentLanguage()");
            CultureInfo pageCulture = null;

			Logger.LogDebug("GetCurrentLanguage(): pageCulture → {PageCulture}", pageCulture);
            // 1. querystring
            pageCulture = GetCultureFromQs();

            // 2. cookie
            pageCulture = this.GetCultureFromCookie();

            // 3. browser
            pageCulture = this.GetCultureFromBrowser();

			Logger.LogDebug("<<< Exiting GetCurrentLanguage()");
            return pageCulture;
        }

        /// <summary>Tests the Database Connection using the database connection config.</summary>
        /// <param name="config">The connection config.</param>
        /// <returns>The connection string, or an error message (prefixed with <c>"ERROR:"</c>).</returns>
        public string TestDatabaseConnection(ConnectionConfig config)
        {
            DbConnectionStringBuilder builder = DataProvider.Instance().GetConnectionStringBuilder();
            string owner = string.Empty;
            string objectQualifier = config.Qualifier;

            builder["data source"] = config.Server;
            builder["integrated security"] = config.Integrated;
            builder["uid"] = config.User;
            builder["pwd"] = config.Password;

            if (!string.IsNullOrEmpty(config.File))
            {
                builder["attachDbFilename"] = "|DataDirectory|" + config.File;
                builder["user instance"] = true;
            }
            else
            {
                builder["initial catalog"] = config.Database;
            }

            if (config.RunAsDbowner)
            {
                owner = "dbo.";
            }

            return DataProvider.Instance().TestDatabaseConnection(builder, owner, objectQualifier);
        }

        /// <inheritdoc />
        public CultureInfo GetCultureFromCookie()
        {
			Logger.LogDebug(">>> Entering GetCultureFromCookie()");
            var langCookie = HttpContext.Current.Request.Cookies["language"];
            var language = langCookie != null ? langCookie.Value : @"en-US";
			Logger.LogDebug("GetCultureFromCookie(): language → {Language}", language);
            var culture = new CultureInfo(language);
			Logger.LogDebug("GetCultureFromCookie(): culture → {Culture}", culture);
			Logger.LogDebug("GetCultureFromCookie(): langCookie → {LangCookie}", langCookie);
			Logger.LogDebug("<<< Exiting GetCultureFromCookie()");
            return culture;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public CultureInfo GetCultureFromBrowser()
        {
			Logger.LogDebug(">>> Entering GetCultureFromBrowser()");
            CultureInfo culture = null;
            foreach (string userLang in HttpContext.Current.Request.UserLanguages)
            {
			Logger.LogDebug("GetCultureFromBrowser(): culture → {Culture}", culture);
                // split userlanguage by ";"... all but the first language will contain a preferrence index eg. ;q=.5
                string language = userLang.Split(';')[0];
				Logger.LogDebug("GetCultureFromBrowser(): language → {Language}", language);
                culture = new CultureInfo(language);
            }

			Logger.LogDebug("<<< Exiting GetCultureFromBrowser()");
            return culture;
        }

        private static XmlNode AppendNewXmlNode(ref XmlDocument document, ref XmlNode parentNode, string elementName, string elementValue)
        {
			Logger.LogDebug(">>> Entering AppendNewXmlNode(document={Document},parentNode={ParentNode},elementName={ElementName},elementValue={ElementValue})", document, parentNode, elementName, elementValue);
            XmlNode newNode = document.CreateNode(XmlNodeType.Element, elementName, null);
            if (!string.IsNullOrEmpty(elementValue))
			Logger.LogDebug("AppendNewXmlNode(document={Document},parentNode={ParentNode},elementName={ElementName},elementValue={ElementValue}): newNode → {NewNode}", document, parentNode, elementName, elementValue, newNode);
            {
                newNode.InnerText = elementValue;
				Logger.LogDebug("AppendNewXmlNode(document={Document},parentNode={ParentNode},elementName={ElementName},elementValue={ElementValue}): newNode.InnerText → {Arg4}", document, parentNode, elementName, elementValue, newNode.InnerText);
            }

            parentNode.AppendChild(newNode);
			Logger.LogDebug("<<< Exiting AppendNewXmlNode(document={Document},parentNode={ParentNode},elementName={ElementName},elementValue={ElementValue})", document, parentNode, elementName, elementValue);
            return newNode;
        }

        private static CultureInfo GetCultureFromQs()
        {
			Logger.LogDebug(">>> Entering GetCultureFromQs()");
            if (HttpContext.Current == null || HttpContext.Current.Request["language"] == null)
            {
                return null;
            }

            string language = HttpContext.Current.Request["language"];
			Logger.LogDebug("GetCultureFromQs(): language → {Language}", language);
            var culture = new CultureInfo(language);
			Logger.LogDebug("GetCultureFromQs(): culture → {Culture}", culture);
			Logger.LogDebug("<<< Exiting GetCultureFromQs()");
            return culture;
        }

        private static void GetLanguagePack(string downloadUrl, string installFolder)
        {
			Logger.LogDebug(">>> Entering GetLanguagePack(downloadUrl={DownloadUrl},installFolder={InstallFolder})", downloadUrl, installFolder);
            string myfile = string.Empty;
            WebResponse wr = Util.GetExternalRequest(
                downloadUrl,
                null,
                null,
                null,
                null,
                null,
                -1,
                null,
                null,
                false,
                "DotNetNuke-Appgallery/1.0.0.0(Microsoft Windows NT 6.1.7600.0",
                "wpi://2.1.0.0/Microsoft Windows NT 6.1.7600.0",
                out myfile,
                10000);
				Logger.LogDebug("GetLanguagePack(downloadUrl={DownloadUrl},installFolder={InstallFolder}): myfile → {Myfile}", downloadUrl, installFolder, myfile);

            // use fixed name for later installation
            myfile = "installlanguage.resources";
            Util.DeployExtension(wr, myfile, installFolder);
			Logger.LogDebug("<<< Exiting GetLanguagePack(downloadUrl={DownloadUrl},installFolder={InstallFolder})", downloadUrl, installFolder);
        }
    }
}
