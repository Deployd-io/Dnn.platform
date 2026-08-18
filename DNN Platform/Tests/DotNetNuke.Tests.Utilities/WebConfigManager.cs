//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//
namespace DotNetNuke.Tests.UI.WatiN.Utilities;

using System;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;
using System.Xml.XPath;
using System.Reflection;
using System.Web.Configuration;

using DotNetNuke.Services.Installer;

public static class WebConfigManager
{
    public static string GetWebPath()
    {
		_logger.LogDebug(">>> Entering GetWebPath()");
        var webPath = Directory.GetCurrentDirectory().Replace("\\Tests\\Fixtures", "\\Website");

		_logger.LogDebug("GetWebPath(): webPath → {WebPath}", webPath);
        if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultPhysicalAppPath"]))
        {
            webPath = ConfigurationManager.AppSettings["DefaultPhysicalAppPath"];
        }

		_logger.LogDebug("<<< Exiting GetWebPath()");
        return webPath;
    }

    /// <summary>Updates the web.config so that the site will run in full trust.</summary>
    public static void UpdateConfigForFullTrust()
    {
		_logger.LogDebug(">>> Entering UpdateConfigForFullTrust()");
        var physicalPath = GetWebPath();
        var webConfig = XDocument.Load(Path.Combine(physicalPath, "web.config"));
		_logger.LogDebug("UpdateConfigForFullTrust(): webConfig → {WebConfig}", webConfig);
        var trustLevel = webConfig.XPathSelectElement("configuration/system.web/trust");
		_logger.LogDebug("UpdateConfigForFullTrust(): trustLevel → {TrustLevel}", trustLevel);
		_logger.LogDebug("UpdateConfigForFullTrust(): physicalPath → {PhysicalPath}", physicalPath);
        using (var outfile = new StreamWriter(Directory.GetCurrentDirectory() + @"\log.txt"))
        {
            outfile.Write(physicalPath);
            outfile.Write(trustLevel.ToString());
        }
        trustLevel.Attribute("level").Value = "Full";
        webConfig.Save(Path.Combine(physicalPath, "web.config"));
		_logger.LogDebug("<<< Exiting UpdateConfigForFullTrust()");
    }

    /// <summary>Updates the web.config so that the site will run in medium trust.</summary>
    public static void UpdateConfigForMediumTrust()
    {
		_logger.LogDebug(">>> Entering UpdateConfigForMediumTrust()");
        var physicalPath = GetWebPath();
        var webConfig = XDocument.Load(Path.Combine(physicalPath, "web.config"));
		_logger.LogDebug("UpdateConfigForMediumTrust(): webConfig → {WebConfig}", webConfig);
        var trustLevel = webConfig.XPathSelectElement("configuration/system.web/trust");
		_logger.LogDebug("UpdateConfigForMediumTrust(): trustLevel → {TrustLevel}", trustLevel);
		_logger.LogDebug("UpdateConfigForMediumTrust(): physicalPath → {PhysicalPath}", physicalPath);
        using (var outfile = new StreamWriter(Directory.GetCurrentDirectory() + @"\log.txt"))
        {
            outfile.Write(physicalPath);
            outfile.Write(trustLevel.ToString());
        }
        trustLevel.Attribute("level").Value = "Medium";
        webConfig.Save(Path.Combine(physicalPath, "web.config"));
		_logger.LogDebug("<<< Exiting UpdateConfigForMediumTrust()");
    }

    /// <summary>Updates the web.config file to drop emails to a local folder.</summary>
    /// <param name="mailDropPath">The path to the mailDrop.xml file that contains the xml for the mail dump.</param>
    /// <param name="emailPath">The path that emails will be sent to.</param>
    public static void UpdateConfigForMailDrop(string mailDropPath, string emailPath)
    {
		_logger.LogDebug(">>> Entering UpdateConfigForMailDrop(mailDropPath={MailDropPath},emailPath={EmailPath})", mailDropPath, emailPath);
        var physicalPath = GetWebPath();
        var mailDropFragment = XDocument.Load(Path.Combine(mailDropPath, "mailDrop.xml"));
		_logger.LogDebug("UpdateConfigForMailDrop(mailDropPath={MailDropPath},emailPath={EmailPath}): mailDropFragment → {MailDropFragment}", mailDropPath, emailPath, mailDropFragment);
        var specifiedPickupDirectory = mailDropFragment.XPathSelectElement("configuration/nodes/node/system.net/mailSettings/smtp/specifiedPickupDirectory");
		_logger.LogDebug("UpdateConfigForMailDrop(mailDropPath={MailDropPath},emailPath={EmailPath}): specifiedPickupDirectory → {SpecifiedPickupDirectory}", mailDropPath, emailPath, specifiedPickupDirectory);
		_logger.LogDebug("UpdateConfigForMailDrop(mailDropPath={MailDropPath},emailPath={EmailPath}): physicalPath → {PhysicalPath}", mailDropPath, emailPath, physicalPath);
        specifiedPickupDirectory.Attribute("pickupDirectoryLocation").Value = emailPath;
        mailDropFragment.Save(Path.Combine(physicalPath, "UpdatedMailDrop.xml"));
        var mailDrop = new FileStream(Path.Combine(physicalPath, "UpdatedMailDrop.xml"), FileMode.Open, FileAccess.Read);
		_logger.LogDebug("UpdateConfigForMailDrop(mailDropPath={MailDropPath},emailPath={EmailPath}): mailDrop → {MailDrop}", mailDropPath, emailPath, mailDrop);
        try
        {
            var fileName = $"{physicalPath}\\web.config";
			_logger.LogDebug("UpdateConfigForMailDrop(mailDropPath={MailDropPath},emailPath={EmailPath}): fileName → {FileName}", mailDropPath, emailPath, fileName);
            var targetDocument = new XmlDocument();
			_logger.LogDebug("UpdateConfigForMailDrop(mailDropPath={MailDropPath},emailPath={EmailPath}): targetDocument → {TargetDocument}", mailDropPath, emailPath, targetDocument);
            using (var xmlReader = XmlReader.Create(fileName, new XmlReaderSettings { XmlResolver = null, }))
            {
                targetDocument.Load(xmlReader);
            }

            var mailNodes = (from mail in targetDocument.DocumentElement.ChildNodes.Cast<XmlNode>()
                where mail.Name == "system.net"
                select mail).ToList();
            if (mailNodes.Count == 0)
            {
                var merge = new XmlMerge(mailDrop, string.Empty, string.Empty);
				_logger.LogDebug("UpdateConfigForMailDrop(mailDropPath={MailDropPath},emailPath={EmailPath}): merge → {Merge}", mailDropPath, emailPath, merge);
                merge.UpdateConfig(targetDocument);
                targetDocument.Save(fileName);
            }
        }
        finally
        {
            mailDrop.Close();
        }
		_logger.LogDebug("<<< Exiting UpdateConfigForMailDrop(mailDropPath={MailDropPath},emailPath={EmailPath})", mailDropPath, emailPath);
    }

    /// <summary>Updates the web.config so that the site will run in full trust.</summary>
    /// <param name="physicalPath">The path for the folder containing the web.config.</param>
    public static void UpdateConfigForFullTrust(string physicalPath)
    {
	_logger.LogDebug(">>> Entering UpdateConfigForFullTrust(physicalPath={PhysicalPath})", physicalPath);

        var webConfig = XDocument.Load(Path.Combine(physicalPath, "web.config"));
		_logger.LogDebug("UpdateConfigForFullTrust(physicalPath={PhysicalPath}): webConfig → {WebConfig}", physicalPath, webConfig);
        var trustLevel = webConfig.XPathSelectElement("configuration/system.web/trust");
		_logger.LogDebug("UpdateConfigForFullTrust(physicalPath={PhysicalPath}): trustLevel → {TrustLevel}", physicalPath, trustLevel);
        using (var outfile = new StreamWriter(Directory.GetCurrentDirectory() + @"\log.txt"))
		using Microsoft.Extensions.Logging;
        {
            outfile.Write(physicalPath);
            outfile.Write(trustLevel.ToString());
        }
        trustLevel.Attribute("level").Value = "Full";
        webConfig.Save(Path.Combine(physicalPath, "web.config"));
		_logger.LogDebug("<<< Exiting UpdateConfigForFullTrust(physicalPath={PhysicalPath})", physicalPath);
    }

    public static void SyncConfig(string sitePath)
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        var configFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
		_logger.LogDebug(">>> Entering SyncConfig(sitePath={SitePath})", sitePath);
        var webConfigPath = Path.Combine(sitePath, "web.config");
		_logger.LogDebug("SyncConfig(sitePath={SitePath}): webConfigPath → {WebConfigPath}", sitePath, webConfigPath);

        var dllConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
		_logger.LogDebug("SyncConfig(sitePath={SitePath}): dllConfig → {DllConfig}", sitePath, dllConfig);
        var webConfig = XDocument.Load(webConfigPath);
		_logger.LogDebug("SyncConfig(sitePath={SitePath}): webConfig → {WebConfig}", sitePath, webConfig);

        var keySection = dllConfig.GetSection("system.web/machineKey") as MachineKeySection;
		_logger.LogDebug("SyncConfig(sitePath={SitePath}): keySection → {KeySection}", sitePath, keySection);
        var webNode = webConfig.XPathSelectElement("configuration/system.web/machineKey");
		_logger.LogDebug("SyncConfig(sitePath={SitePath}): webNode → {WebNode}", sitePath, webNode);

        if (keySection.ValidationKey != webNode.Attribute("validationKey").Value)
        {
            keySection.ValidationKey = webNode.Attribute("validationKey").Value;
            keySection.DecryptionKey = webNode.Attribute("decryptionKey").Value;

            dllConfig.Save();

            Type type = Assembly.GetAssembly(typeof(System.Web.TraceContext)).GetType("System.Web.Configuration.RuntimeConfig");
			_logger.LogDebug("SyncConfig(sitePath={SitePath}): type → {Type}", sitePath, type);
            var fieldInfo = type.GetField("s_clientRuntimeConfig", BindingFlags.NonPublic | BindingFlags.Static);
			_logger.LogDebug("SyncConfig(sitePath={SitePath}): fieldInfo → {FieldInfo}", sitePath, fieldInfo);
            fieldInfo.SetValue(null, null);
            ConfigurationManager.RefreshSection("system.web/machineKey");
        }
		_logger.LogDebug("<<< Exiting SyncConfig(sitePath={SitePath})", sitePath);
    }

    public static void TouchConfig(string sitePath)
    {
		_logger.LogDebug(">>> Entering TouchConfig(sitePath={SitePath})", sitePath);
        var webConfigPath = Path.Combine(sitePath, "web.config");
        File.SetLastWriteTime(webConfigPath, DateTime.Now);
		_logger.LogDebug("TouchConfig(sitePath={SitePath}): webConfigPath → {WebConfigPath}", sitePath, webConfigPath);
		_logger.LogDebug("<<< Exiting TouchConfig(sitePath={SitePath})", sitePath);
    }
}
