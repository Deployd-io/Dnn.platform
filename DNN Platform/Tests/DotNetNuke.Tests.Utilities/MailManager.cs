//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//
namespace DotNetNuke.Tests.Utilities;

using System.IO;

using DotNetNuke.Entities.Controllers;
using DotNetNuke.Tests.UI.WatiN.Utilities;
using Microsoft.Extensions.Logging;

public class MailManager
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    private static string GetEmailDumpFolderPath()
    {
		_logger.LogDebug(">>> Entering GetEmailDumpFolderPath()");
		_logger.LogDebug("<<< Exiting GetEmailDumpFolderPath()");
        return Directory.GetCurrentDirectory().Replace("\\Fixtures", "\\Packages") + "\\TestEmails";
    }

    public static void ClearDumpFolder()
    {
		_logger.LogDebug(">>> Entering ClearDumpFolder()");
        string emailPath = GetEmailDumpFolderPath();

        if (Directory.Exists(emailPath))
        {
            foreach (var file in Directory.GetFiles(emailPath))
            {
                File.Delete(file);
            }
        }
		_logger.LogDebug("<<< Exiting ClearDumpFolder()");
    }

    public static void SetUpMailDumpFolder()
    {
		_logger.LogDebug(">>> Entering SetUpMailDumpFolder()");
        string emailPath = GetEmailDumpFolderPath();
        var mailDropPath = Directory.GetCurrentDirectory().Replace("\\Fixtures", "\\Community\\Tests\\Packages");
		_logger.LogDebug("SetUpMailDumpFolder(): mailDropPath → {MailDropPath}", mailDropPath);

        if (!Directory.Exists(emailPath))
        {
            Directory.CreateDirectory(emailPath);
        }

        WebConfigManager.UpdateConfigForMailDrop(mailDropPath, emailPath);
        HostController.Instance.Update("SMTPServer", "localhost", false);
		_logger.LogDebug("<<< Exiting SetUpMailDumpFolder()");
    }
}
