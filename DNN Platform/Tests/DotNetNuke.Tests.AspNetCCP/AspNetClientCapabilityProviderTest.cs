// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.AspNetClientCapabilityProvider
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Web.Hosting;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Providers.AspNetClientCapabilityProvider;
    using DotNetNuke.Tests.Utilities.Mocks;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class AspNetClientCapabilityProviderTest
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private const string iphoneUserAgent = "Mozilla/5.0 (iPod; U; CPU iPhone OS 4_0 like Mac OS X; en-us) AppleWebKit/532.9 (KHTML, like Gecko) Version/4.0.5 Mobile/8A293 Safari/6531.22.7";
        private Providers.AspNetClientCapabilityProvider.AspNetClientCapabilityProvider _clientCapabilityProvider;

        [SetUp]
        public void Setup()
        {
			_logger.LogDebug(">>> Entering Setup()");
            this._clientCapabilityProvider = new Providers.AspNetClientCapabilityProvider.AspNetClientCapabilityProvider();

			_logger.LogDebug("Setup(): _clientCapabilityProvider → {_clientCapabilityProvider}", _clientCapabilityProvider);
            ComponentFactory.Container = new SimpleContainer();
            var dataProvider = MockComponentProvider.CreateDataProvider();
			_logger.LogDebug("Setup(): dataProvider → {DataProvider}", dataProvider);
            dataProvider.Setup(d => d.GetProviderPath()).Returns(string.Empty);
            MockComponentProvider.CreateDataCacheProvider();
            MockComponentProvider.CreateEventLogController();

            // create the bin folder
            var folderPath = HostingEnvironment.ApplicationPhysicalPath + "bin";
			_logger.LogDebug("Setup(): folderPath → {FolderPath}", folderPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
			_logger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
		_logger.LogDebug(">>> Entering TearDown()");
		_logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void AspNetClientCapabilityProvider_GetClientCapabilityById_ThrowsException_For_Empty_ClientCapabilityId()
        {
			_logger.LogDebug(">>> Entering AspNetClientCapabilityProvider_GetClientCapabilityById_ThrowsException_For_Empty_ClientCapabilityId()");
            // Act
            string nullClientCapabilityId = string.Empty;
            Assert.Throws<ArgumentException>(() => this._clientCapabilityProvider.GetClientCapabilityById(nullClientCapabilityId));
			_logger.LogDebug("<<< Exiting AspNetClientCapabilityProvider_GetClientCapabilityById_ThrowsException_For_Empty_ClientCapabilityId()");
        }

        [Test]
        public void AspNetClientCapabilityProvider_GetClientCapabilityById_ThrowsException_For_Null_ClientCapabilityId()
        {
			_logger.LogDebug(">>> Entering AspNetClientCapabilityProvider_GetClientCapabilityById_ThrowsException_For_Null_ClientCapabilityId()");
            // Act
            string nullClientCapabilityId = null;
            Assert.Throws<ArgumentException>(() => this._clientCapabilityProvider.GetClientCapabilityById(nullClientCapabilityId));
			_logger.LogDebug("<<< Exiting AspNetClientCapabilityProvider_GetClientCapabilityById_ThrowsException_For_Null_ClientCapabilityId()");
        }
    }
}
