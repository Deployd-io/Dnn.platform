//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//
namespace DotNetNuke.Tests.Utilities;

using System.IO;

using DotNetNuke.Entities.Controllers;
using DotNetNuke.Tests.UI.WatiN.Utilities;

public class MailManager
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    private static string GetEmailDumpFolderPath()
    {
		Logger.LogDebug(">>> Entering GetEmailDumpFolderPath()");
		Logger.LogDebug("<<< Exiting GetEmailDumpFolderPath()");
        return Directory.GetCurrentDirectory().Replace("\\Fixtures", "\\Packages") + "\\TestEmails";
    }

    public static void ClearDumpFolder()
    {
		Logger.LogDebug(">>> Entering ClearDumpFolder()");
        string emailPath = GetEmailDumpFolderPath();

        if (Directory.Exists(emailPath))
        {
            foreach (var file in Directory.GetFiles(emailPath))
            {
                File.Delete(file);
            }
        }
		Logger.LogDebug("<<< Exiting ClearDumpFolder()");
    }

    public static void SetUpMailDumpFolder()
    {
		Logger.LogDebug(">>> Entering SetUpMailDumpFolder()");
        string emailPath = GetEmailDumpFolderPath();
        var mailDropPath = Directory.GetCurrentDirectory().Replace("\\Fixtures", "\\Community\\Tests\\Packages");
		Logger.LogDebug("SetUpMailDumpFolder(): mailDropPath → {MailDropPath}", mailDropPath);

        if (!Directory.Exists(emailPath))
        {
            Directory.CreateDirectory(emailPath);
        }

        WebConfigManager.UpdateConfigForMailDrop(mailDropPath, emailPath);
        HostController.Instance.Update("SMTPServer", "localhost", false);
		Logger.LogDebug("<<< Exiting SetUpMailDumpFolder()");
    }
}
