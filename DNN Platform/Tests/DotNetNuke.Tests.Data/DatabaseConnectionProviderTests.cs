// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data
{
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Tests.Data.Fakes;
    using NUnit.Framework;

    [TestFixture]
    public class DatabaseConnectionProviderTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [Test]
        public void DatabaseConnectionProvider_Instance_Method_Returns_Instance()
        {
			Logger.LogDebug(">>> Entering DatabaseConnectionProvider_Instance_Method_Returns_Instance()");
            // Arrange
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DatabaseConnectionProvider>(new FakeDbConnectionProvider());

            // Act
            var provider = DatabaseConnectionProvider.Instance();

			Logger.LogDebug("DatabaseConnectionProvider_Instance_Method_Returns_Instance(): provider → {Provider}", provider);
            // Assert
            Assert.That(provider, Is.InstanceOf<DatabaseConnectionProvider>());
            Assert.That(provider, Is.InstanceOf<FakeDbConnectionProvider>());
			Logger.LogDebug("<<< Exiting DatabaseConnectionProvider_Instance_Method_Returns_Instance()");
        }

        [Test]
        public void DatabaseConnectionProvider_Instance_Method_Returns_Same_Instances()
        {
			Logger.LogDebug(">>> Entering DatabaseConnectionProvider_Instance_Method_Returns_Same_Instances()");
            // Arrange
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DatabaseConnectionProvider>(new FakeDbConnectionProvider());

            // Act
            var provider1 = DatabaseConnectionProvider.Instance();
            var provider2 = DatabaseConnectionProvider.Instance();
			Logger.LogDebug("DatabaseConnectionProvider_Instance_Method_Returns_Same_Instances(): provider1 → {Provider1}", provider1);
			Logger.LogDebug("DatabaseConnectionProvider_Instance_Method_Returns_Same_Instances(): provider2 → {Provider2}", provider2);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(provider1, Is.InstanceOf<DatabaseConnectionProvider>());
                Assert.That(provider2, Is.InstanceOf<DatabaseConnectionProvider>());
            }
            Assert.That(provider2, Is.SameAs(provider1));
			Logger.LogDebug("<<< Exiting DatabaseConnectionProvider_Instance_Method_Returns_Same_Instances()");
        }
    }
}
