// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data
{
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Tests.Data.Fakes;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class DatabaseConnectionProviderTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        [Test]
        public void DatabaseConnectionProvider_Instance_Method_Returns_Instance()
        {
			_logger.LogDebug(">>> Entering DatabaseConnectionProvider_Instance_Method_Returns_Instance()");
            // Arrange
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DatabaseConnectionProvider>(new FakeDbConnectionProvider());

            // Act
            var provider = DatabaseConnectionProvider.Instance();
			_logger.LogDebug("DatabaseConnectionProvider_Instance_Method_Returns_Instance(): provider → {Provider}", provider);

            // Assert
            Assert.That(provider, Is.InstanceOf<DatabaseConnectionProvider>());
            Assert.That(provider, Is.InstanceOf<FakeDbConnectionProvider>());
			_logger.LogDebug("<<< Exiting DatabaseConnectionProvider_Instance_Method_Returns_Instance()");
        }

        [Test]
        public void DatabaseConnectionProvider_Instance_Method_Returns_Same_Instances()
        {
			_logger.LogDebug(">>> Entering DatabaseConnectionProvider_Instance_Method_Returns_Same_Instances()");
            // Arrange
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DatabaseConnectionProvider>(new FakeDbConnectionProvider());

            // Act
            var provider1 = DatabaseConnectionProvider.Instance();
			_logger.LogDebug("DatabaseConnectionProvider_Instance_Method_Returns_Same_Instances(): provider1 → {Provider1}", provider1);
            var provider2 = DatabaseConnectionProvider.Instance();
			_logger.LogDebug("DatabaseConnectionProvider_Instance_Method_Returns_Same_Instances(): provider2 → {Provider2}", provider2);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(provider1, Is.InstanceOf<DatabaseConnectionProvider>());
                Assert.That(provider2, Is.InstanceOf<DatabaseConnectionProvider>());
            }
            Assert.That(provider2, Is.SameAs(provider1));
			_logger.LogDebug("<<< Exiting DatabaseConnectionProvider_Instance_Method_Returns_Same_Instances()");
        }
    }
}
