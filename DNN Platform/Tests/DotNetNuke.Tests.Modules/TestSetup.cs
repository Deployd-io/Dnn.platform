//
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
//

namespace DotNetNuke.Tests.Core;

using DotNetNuke.Tests.Utilities.Mocks;

using NUnit.Framework;
using Microsoft.Extensions.Logging;

[SetUpFixture]
internal class TestSetup
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    [OneTimeSetUp]
    public void SetUp()
    {
	_logger.LogDebug(">>> Entering SetUp()");
	_logger.LogDebug("<<< Exiting SetUp()");
    }

    [OneTimeTearDown]
    public void TearDown()
    {
		_logger.LogDebug(">>> Entering TearDown()");
        MockComponentProvider.ResetContainer();
		_logger.LogDebug("<<< Exiting TearDown()");
    }
}
