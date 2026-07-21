// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities.Fakes;

using System;

using DotNetNuke.Abstractions.Application;

public class FakeApplicationStatusInfo(UpgradeStatus status = UpgradeStatus.None, string applicationMapPath = "", string databaseVersion = "10.0.0", bool isInstalled = true, bool incrementalVersionExists = false, int lastAppliedIteration = 0)
    : IApplicationStatusInfo
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
	Logger.LogDebug(">>> Entering FakeApplicationStatusInfo(status={Status},applicationMapPath={ApplicationMapPath},databaseVersion={DatabaseVersion},isInstalled={IsInstalled},incrementalVersionExists={IncrementalVersionExists},lastAppliedIteration={LastAppliedIteration})", status, applicationMapPath, databaseVersion, isInstalled, incrementalVersionExists, lastAppliedIteration);
    public UpgradeStatus Status { get; set; } = status;

    /// <inheritdoc />
    public string ApplicationMapPath { get; set; } = applicationMapPath;

    /// <inheritdoc />
    public Version DatabaseVersion { get; set; } = new Version(databaseVersion);

	Logger.LogDebug("<<< Exiting FakeApplicationStatusInfo(status={Status},applicationMapPath={ApplicationMapPath},databaseVersion={DatabaseVersion},isInstalled={IsInstalled},incrementalVersionExists={IncrementalVersionExists},lastAppliedIteration={LastAppliedIteration})", status, applicationMapPath, databaseVersion, isInstalled, incrementalVersionExists, lastAppliedIteration);
    /// <inheritdoc />
    public bool IsInstalled() => isInstalled;

    /// <inheritdoc />
    public void SetStatus(UpgradeStatus status)
    {
		Logger.LogDebug(">>> Entering SetStatus(status={Status})", status);
        this.Status = status;
		Logger.LogDebug("<<< Exiting SetStatus(status={Status})", status);
		Logger.LogDebug("SetStatus(status={Status}): this.Status → {Arg1}", status, this.Status);
    }

    /// <inheritdoc />
    public void UpdateDatabaseVersion(Version version)
    {
		Logger.LogDebug(">>> Entering UpdateDatabaseVersion(version={Version})", version);
        this.DatabaseVersion = version;
		Logger.LogDebug("<<< Exiting UpdateDatabaseVersion(version={Version})", version);
		Logger.LogDebug("UpdateDatabaseVersion(version={Version}): this.DatabaseVersion → {Arg1}", version, this.DatabaseVersion);
    }

    /// <inheritdoc />
    public void UpdateDatabaseVersionIncrement(Version version, int increment)
    {
		Logger.LogDebug(">>> Entering UpdateDatabaseVersionIncrement(version={Version},increment={Increment})", version, increment);
        this.DatabaseVersion = version;
		Logger.LogDebug("<<< Exiting UpdateDatabaseVersionIncrement(version={Version},increment={Increment})", version, increment);
		Logger.LogDebug("UpdateDatabaseVersionIncrement(version={Version},increment={Increment}): this.DatabaseVersion → {Arg2}", version, increment, this.DatabaseVersion);
    }

    /// <inheritdoc />
    public bool IncrementalVersionExists(Version version) => incrementalVersionExists;

    /// <inheritdoc />
    public int GetLastAppliedIteration(Version version) => lastAppliedIteration;
}
