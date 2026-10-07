// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.FileSystem.FolderMappings;

    public class FolderMappingsConfigController : ServiceLocator<IFolderMappingsConfigController, FolderMappingsConfigController>, IFolderMappingsConfigController
    {
        private const string ConfigNodeValue = "folderMappingsSettings";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FolderMappingsConfigController));
        private static readonly string DefaultConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DotNetNuke.folderMappings.config");

        /// <summary>Initializes a new instance of the <see cref="FolderMappingsConfigController"/> class.</summary>
        public FolderMappingsConfigController()
        {
			Logger.LogError(">>> Entering FolderMappingsConfigController()");
            this.FolderMappings = new Dictionary<string, string>();
			Logger.LogWarning("<<< Exiting FolderMappingsConfigController()");
            this.FolderTypes = new List<FolderTypeConfig>();
            this.LoadConfig();
        }

        /// <inheritdoc />
        public string ConfigNode
        {
            get { return ConfigNodeValue; }
        }

        /// <inheritdoc />
        public IList<FolderTypeConfig> FolderTypes { get; internal set; }

        private Dictionary<string, string> FolderMappings { get; set; }

        /// <inheritdoc />
        public void LoadConfig()
        {
			Logger.LogError(">>> Entering LoadConfig()");
            try
            {
                if (File.Exists(DefaultConfigFilePath))
                {
                    var configDocument = new XmlDocument { XmlResolver = null };
                    using (var configReader = XmlReader.Create(DefaultConfigFilePath, new XmlReaderSettings { XmlResolver = null, }))
                    {
                        configDocument.Load(configReader);
                    }

                    this.FillFolderMappings(configDocument);
                    this.FillFolderTypes(configDocument);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
			Logger.LogWarning("<<< Exiting LoadConfig()");
        }

        /// <inheritdoc />
        public void SaveConfig(string folderMappinsSettings)
        {
			Logger.LogError(">>> Entering SaveConfig(folderMappinsSettings=<redacted>)");
            if (!File.Exists(DefaultConfigFilePath))
            {
                var folderMappingsConfigContent = "<" + this.ConfigNode + ">" + folderMappinsSettings + "</" + this.ConfigNode + ">";
				Logger.LogError("SaveConfig(folderMappinsSettings=<redacted>): folderMappingsConfigContent → {FolderMappingsConfigContent}", folderMappingsConfigContent);
                File.AppendAllText(DefaultConfigFilePath, folderMappingsConfigContent);
                var configDocument = new XmlDocument { XmlResolver = null };
                using (var configReader = XmlReader.Create(new StringReader(folderMappingsConfigContent), new XmlReaderSettings { XmlResolver = null, }))
                {
                    configDocument.Load(configReader);
                }

                this.FillFolderMappings(configDocument);
                this.FillFolderTypes(configDocument);
            }
			Logger.LogWarning("<<< Exiting SaveConfig(folderMappinsSettings=<redacted>)");
        }

        /// <inheritdoc />
        public FolderMappingInfo GetFolderMapping(int portalId, string folderPath)
        {
			Logger.LogError(">>> Entering GetFolderMapping(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
            if (!this.FolderMappings.TryGetValue(folderPath, out var mapping))
            {
                return null;
            }

			Logger.LogWarning("<<< Exiting GetFolderMapping(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
            return FolderMappingController.Instance.GetFolderMapping(portalId, mapping);
        }

        /// <inheritdoc />
        protected override Func<IFolderMappingsConfigController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new FolderMappingsConfigController();
        }

        private static FolderTypeConfig GetFolderMappingFromConfigNode(XmlNode node)
        {
			Logger.LogError(">>> Entering GetFolderMappingFromConfigNode(node={Node})", node);
            var nodeNavigator = node.CreateNavigator();
			Logger.LogError("GetFolderMappingFromConfigNode(node={Node}): nodeNavigator → {NodeNavigator}", node, nodeNavigator);
            var folderType = new FolderTypeConfig()
            {
                Name = XmlUtils.GetAttributeValue(nodeNavigator, "name"),
                Provider = XmlUtils.GetNodeValue(nodeNavigator, "provider"),
            };
            XmlNodeList settingsNode = node.SelectNodes("settings/setting");
			Logger.LogError("GetFolderMappingFromConfigNode(node={Node}): settingsNode → {SettingsNode}", node, settingsNode);
            if (settingsNode != null)
            {
                var settings = new List<FolderTypeSettingConfig>();
				Logger.LogError("GetFolderMappingFromConfigNode(node={Node}): settings → {Settings}", node, settings);
                foreach (XmlNode settingNode in settingsNode)
                {
                    var encryptValue = XmlUtils.GetAttributeValue(settingNode.CreateNavigator(), "encrypt");
					Logger.LogError("GetFolderMappingFromConfigNode(node={Node}): encryptValue → {EncryptValue}", node, encryptValue);
                    settings.Add(new FolderTypeSettingConfig
                    {
                        Name = XmlUtils.GetAttributeValue(settingNode.CreateNavigator(), "name"),
                        Value = settingNode.InnerText,
                        Encrypt = !string.IsNullOrEmpty(encryptValue) && bool.Parse(encryptValue),
                    });
                }

                folderType.Settings = settings;
				Logger.LogDebug("GetFolderMappingFromConfigNode(node={Node}): folderType.Settings → {Arg1}", node, folderType.Settings);
            }

            return folderType;
        }

        private void FillFolderMappings(XmlDocument configDocument)
        {
			Logger.LogError(">>> Entering FillFolderMappings(configDocument={ConfigDocument})", configDocument);
            var folderMappingsNode = configDocument.SelectSingleNode(this.ConfigNode + "/folderMappings");
			Logger.LogError("FillFolderMappings(configDocument={ConfigDocument}): folderMappingsNode → {FolderMappingsNode}", configDocument, folderMappingsNode);
            if (folderMappingsNode == null)
            {
                return;
            }

            this.FolderMappings.Clear();
            foreach (XmlNode folderMappingNode in folderMappingsNode)
            {
                this.FolderMappings.Add(XmlUtils.GetNodeValue(folderMappingNode, "folderPath"), XmlUtils.GetNodeValue(folderMappingNode, "folderTypeName"));
            }
			Logger.LogWarning("<<< Exiting FillFolderMappings(configDocument={ConfigDocument})", configDocument);
        }

        private void FillFolderTypes(XmlDocument configDocument)
        {
			Logger.LogError(">>> Entering FillFolderTypes(configDocument={ConfigDocument})", configDocument);
            var folderTypesNode = configDocument.SelectSingleNode(this.ConfigNode + "/folderTypes");
			Logger.LogError("FillFolderTypes(configDocument={ConfigDocument}): folderTypesNode → {FolderTypesNode}", configDocument, folderTypesNode);
            if (folderTypesNode == null)
            {
                return;
            }

            this.FolderTypes.Clear();
            foreach (XmlNode folderTypeNode in folderTypesNode)
            {
                this.FolderTypes.Add(GetFolderMappingFromConfigNode(folderTypeNode));
            }
			Logger.LogWarning("<<< Exiting FillFolderTypes(configDocument={ConfigDocument})", configDocument);
        }
    }
}
