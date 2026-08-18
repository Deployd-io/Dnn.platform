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
    [OneTimeSetUp]
    public void SetUp()
    {
	mockEventLogger.LogDebug(">>> Entering SetUp()");
	mockEventLogger.LogDebug("<<< Exiting SetUp()");
    }

    [OneTimeTearDown]
    public void TearDown()
    {
		mockEventLogger.LogDebug(">>> Entering TearDown()");
        MockComponentProvider.ResetContainer();
		mockEventLogger.LogDebug("<<< Exiting TearDown()");
    }
}
