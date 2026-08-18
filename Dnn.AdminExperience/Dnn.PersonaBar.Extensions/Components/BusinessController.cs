// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;

    using Dnn.PersonaBar.Library.Repository;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;

    public class BusinessController : IUpgradeable
    {
        /// <inheritdoc />
        public string UpgradeModule(string version)
        {
			eventLogger.LogDebug(">>> Entering UpgradeModule(version={Version})", version);
            switch (version)
            {
                case "01.04.00":
                    UpdateMenuController();
                    break;

                case "01.05.00":
                    if (TelerikAssemblyExists())
                    {
                        UpdateTelerikEncryptionKey("Telerik.Web.UI.DialogParametersEncryptionKey");
                    }

                    break;
            }

			eventLogger.LogDebug("<<< Exiting UpgradeModule(version={Version})", version);
            return string.Empty;
        }

        private static string UpdateTelerikEncryptionKey(string keyName)
        {
			eventLogger.LogDebug(">>> Entering UpdateTelerikEncryptionKey(keyName={KeyName})", keyName);
            var strError = string.Empty;
            var currentKey = Config.GetSetting(keyName);
			eventLogger.LogDebug("UpdateTelerikEncryptionKey(keyName={KeyName}): currentKey → {CurrentKey}", keyName, currentKey);
            if (string.IsNullOrEmpty(currentKey) || currentKey.Length < 40)
            {
                try
                {
                    // open the web.config
                    var xmlConfig = Config.Load();
					eventLogger.LogDebug("UpdateTelerikEncryptionKey(keyName={KeyName}): xmlConfig → {XmlConfig}", keyName, xmlConfig);

                    // save the current config file
                    Config.BackupConfig();

                    // create a random Telerik encryption key and add it under <appSettings>
                    var newKey = PortalSecurity.Instance.CreateKey(32);
					eventLogger.LogDebug("UpdateTelerikEncryptionKey(keyName={KeyName}): newKey → {NewKey}", keyName, newKey);
                    newKey = Convert.ToBase64String(Encoding.ASCII.GetBytes(newKey));
                    Config.AddAppSetting(xmlConfig, keyName, newKey);

                    // save a copy of the existing web.config
                    var backupFolder = $@"{Globals.glbConfigFolder}Backup_{DateTime.Now.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture)}\";
					eventLogger.LogDebug("UpdateTelerikEncryptionKey(keyName={KeyName}): backupFolder → {BackupFolder}", keyName, backupFolder);
                    strError += Config.Save(xmlConfig, backupFolder + "web_.config") + Environment.NewLine;

                    // save the web.config
                    strError += Config.Save(xmlConfig) + Environment.NewLine;
                }
                catch (Exception ex)
                {
					eventLogger.LogError("Exception in UpdateTelerikEncryptionKey(keyName={KeyName}): {Arg1}", keyName, ex.getMessage());
                    strError += ex.Message;
                }
            }

			eventLogger.LogDebug("<<< Exiting UpdateTelerikEncryptionKey(keyName={KeyName})", keyName);
            return strError;
        }

        private static void UpdateMenuController()
        {
			eventLogger.LogDebug(">>> Entering UpdateMenuController()");
            PersonaBarRepository.Instance.UpdateMenuController(Dnn.PersonaBar.Vocabularies.Components.Constants.MenuIdentifier, string.Empty);
			eventLogger.LogDebug("<<< Exiting UpdateMenuController()");
        }

        private static bool TelerikAssemblyExists()
        {
			eventLogger.LogDebug(">>> Entering TelerikAssemblyExists()");
			eventLogger.LogDebug("<<< Exiting TelerikAssemblyExists()");
            return File.Exists(Path.Combine(Globals.ApplicationMapPath, @"bin\Telerik.Web.UI.dll"));
        }
    }
}
