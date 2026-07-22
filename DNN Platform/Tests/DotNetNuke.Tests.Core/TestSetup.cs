//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//
namespace DotNetNuke.Tests.Core;

using DotNetNuke.Tests.Utilities.Mocks;

using NUnit.Framework;

[SetUpFixture]
internal class TestSetup
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    [OneTimeSetUp]
    public void SetUp()
    {
	Logger.LogDebug(">>> Entering SetUp()");
	Logger.LogDebug("<<< Exiting SetUp()");
    }

    [OneTimeTearDown]
    public void TearDown()
    {
		Logger.LogDebug(">>> Entering TearDown()");
        MockComponentProvider.ResetContainer();
		Logger.LogDebug("<<< Exiting TearDown()");
    }
}
