// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities.Fakes;

using System;

using DotNetNuke.Abstractions.Application;
using Microsoft.Extensions.Logging;

public class FakeApplicationStatusInfo(UpgradeStatus status = UpgradeStatus.None, string applicationMapPath = "", string databaseVersion = "10.0.0", bool isInstalled = true, bool incrementalVersionExists = false, int lastAppliedIteration = 0)
    : IApplicationStatusInfo
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    /// <inheritdoc />
    public UpgradeStatus Status { get; set; } = status;

    /// <inheritdoc />
    public string ApplicationMapPath { get; set; } = applicationMapPath;

    /// <inheritdoc />
    public Version DatabaseVersion { get; set; } = new Version(databaseVersion);

    /// <inheritdoc />
    public bool IsInstalled() => isInstalled;

    /// <inheritdoc />
    public void SetStatus(UpgradeStatus status)
    {
		_logger.LogDebug(">>> Entering SetStatus(status={Status})", status);
        this.Status = status;
		_logger.LogDebug("SetStatus(status={Status}): this.Status → {Arg1}", status, this.Status);
		_logger.LogDebug("<<< Exiting SetStatus(status={Status})", status);
    }

    /// <inheritdoc />
    public void UpdateDatabaseVersion(Version version)
    {
		_logger.LogDebug(">>> Entering UpdateDatabaseVersion(version={Version})", version);
        this.DatabaseVersion = version;
		_logger.LogDebug("UpdateDatabaseVersion(version={Version}): this.DatabaseVersion → {Arg1}", version, this.DatabaseVersion);
		_logger.LogDebug("<<< Exiting UpdateDatabaseVersion(version={Version})", version);
    }

    /// <inheritdoc />
    public void UpdateDatabaseVersionIncrement(Version version, int increment)
    {
		_logger.LogDebug(">>> Entering UpdateDatabaseVersionIncrement(version={Version},increment={Increment})", version, increment);
        this.DatabaseVersion = version;
		_logger.LogDebug("UpdateDatabaseVersionIncrement(version={Version},increment={Increment}): this.DatabaseVersion → {Arg2}", version, increment, this.DatabaseVersion);
		_logger.LogDebug("<<< Exiting UpdateDatabaseVersionIncrement(version={Version},increment={Increment})", version, increment);
    }

    /// <inheritdoc />
    public bool IncrementalVersionExists(Version version) => incrementalVersionExists;

    /// <inheritdoc />
    public int GetLastAppliedIteration(Version version) => lastAppliedIteration;
}
