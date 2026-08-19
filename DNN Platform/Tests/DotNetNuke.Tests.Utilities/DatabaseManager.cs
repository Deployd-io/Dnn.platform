//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//

namespace DotNetNuke.Tests.Utilities;

using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading;

using Microsoft.SqlServer.Management.Smo;

using NUnit.Framework;
using Microsoft.Extensions.Logging;
public static class DatabaseManager
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    #region Private Methods

    private static void AttachDatabase(string databaseName, string databaseFile)
    {
		_logger.LogDebug(">>> Entering AttachDatabase(databaseName={DatabaseName},databaseFile={DatabaseFile})", databaseName, databaseFile);
        // Connect to the SQL Server
        var server = new Server("(local)");
		_logger.LogDebug("AttachDatabase(databaseName={DatabaseName},databaseFile={DatabaseFile}): server → {Server}", databaseName, databaseFile, server);

        // Attach the database
        server.AttachDatabase(databaseName, new StringCollection { databaseFile });
        while (server.Databases[databaseName].State != SqlSmoState.Existing)
        {
            Thread.Sleep(100);
        }
		_logger.LogDebug("<<< Exiting AttachDatabase(databaseName={DatabaseName},databaseFile={DatabaseFile})", databaseName, databaseFile);
    }

    private static string CopyDatabase(TestContext context, string databaseName)
    {
		_logger.LogDebug(">>> Entering CopyDatabase(context={Context},databaseName={DatabaseName})", context, databaseName);
        // Find the target database file
        string targetDatabasePath = EnsureDatabaseExists(databaseName);

        // Create the test database directory if it does not exist
        string testDatabaseDirectory = Path.GetFullPath(String.Format(@"Databases\TestDatabases\{0}", context.Test.FullName));
        if (!Directory.Exists(testDatabaseDirectory))
        {
            Directory.CreateDirectory(testDatabaseDirectory);
        }

        // Copy the database to the test database directory
        string destinationRoot = Path.Combine(testDatabaseDirectory, databaseName);
        string databasePath = String.Concat(destinationRoot, ".mdf");
        File.Copy(targetDatabasePath, databasePath);
		_logger.LogDebug("<<< Exiting CopyDatabase(context={Context},databaseName={DatabaseName})", context, databaseName);
        return databasePath;
    }

    #endregion

    #region Public Methods

    public static void CopyAndAttachDatabase(TestContext context, string databaseName)
    {
		_logger.LogDebug(">>> Entering CopyAndAttachDatabase(context={Context},databaseName={DatabaseName})", context, databaseName);
        string destinationPath = CopyDatabase(context, databaseName);

        // Attach the copied database to SQL Server
        AttachDatabase(databaseName, destinationPath);
		_logger.LogDebug("<<< Exiting CopyAndAttachDatabase(context={Context},databaseName={DatabaseName})", context, databaseName);
    }

    public static void DropDatabase(string databaseName)
    {
		_logger.LogDebug(">>> Entering DropDatabase(databaseName={DatabaseName})", databaseName);
        // Connect to the SQL Server
        var server = new Server("(local)");
		_logger.LogDebug("DropDatabase(databaseName={DatabaseName}): server → {Server}", databaseName, server);

        // Drop the database
        Database db = server.Databases[databaseName];
        if (db != null)
        {
            server.KillDatabase(databaseName);
            while (server.Databases[databaseName] != null)
            {
                Thread.Sleep(100);
            }
        }
		_logger.LogDebug("<<< Exiting DropDatabase(databaseName={DatabaseName})", databaseName);
    }

    public static string EnsureDatabaseExists(string databaseName)
    {
		_logger.LogDebug(">>> Entering EnsureDatabaseExists(databaseName={DatabaseName})", databaseName);
        string targetDatabasePath = GetTestDatabasePath(databaseName);
        if (!File.Exists(targetDatabasePath))
        {
			_logger.LogWarning("Throwing InvalidOperationException from EnsureDatabaseExists(databaseName={DatabaseName})", databaseName);
		_logger.LogDebug("<<< Exiting EnsureDatabaseExists(databaseName={DatabaseName})", databaseName);
            throw new InvalidOperationException(String.Format("Could not find test database {0}. Searched: {1}", databaseName, targetDatabasePath));
        }
        return targetDatabasePath;
    }

    public static string GetTestDatabasePath(string databaseName)
    {
		_logger.LogDebug(">>> Entering GetTestDatabasePath(databaseName={DatabaseName})", databaseName);
		_logger.LogDebug("<<< Exiting GetTestDatabasePath(databaseName={DatabaseName})", databaseName);
        return Path.GetFullPath(String.Format(@"Databases\{0}.mdf", databaseName));
    }

    #endregion
}
