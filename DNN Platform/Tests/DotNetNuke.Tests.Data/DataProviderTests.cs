// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data
{
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Tests.Data.Fakes;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class DataProviderTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			_logger.LogDebug(">>> Entering SetUp()");
            this.serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IApplicationStatusInfo>()));
			_logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
			_logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			_logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			_logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void DataProvider_Instance_Method_Returns_Instance()
        {
			_logger.LogDebug(">>> Entering DataProvider_Instance_Method_Returns_Instance()");
            // Arrange
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(new Dictionary<string, string>()));

            // Act
            var provider = DataProvider.Instance();
			_logger.LogDebug("DataProvider_Instance_Method_Returns_Instance(): provider → {Provider}", provider);

            // Assert
            Assert.That(provider, Is.InstanceOf<DataProvider>());
            Assert.That(provider, Is.InstanceOf<FakeDataProvider>());
			_logger.LogDebug("<<< Exiting DataProvider_Instance_Method_Returns_Instance()");
        }

        [Test]
        public void DataProvider_ConnectionString_Property_Is_Valid()
        {
			_logger.LogDebug(">>> Entering DataProvider_ConnectionString_Property_Is_Valid()");
            // Arrange
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(new Dictionary<string, string>()));

            var connectionString = Config.GetConnectionString();
			_logger.LogDebug("DataProvider_ConnectionString_Property_Is_Valid(): connectionString → {ConnectionString}", connectionString);

            // Act
            var provider = DataProvider.Instance();
			_logger.LogDebug("DataProvider_ConnectionString_Property_Is_Valid(): provider → {Provider}", provider);

            // Assert
            Assert.That(provider.ConnectionString, Is.EqualTo(connectionString));
			_logger.LogDebug("<<< Exiting DataProvider_ConnectionString_Property_Is_Valid()");
        }

        [Test]
        [TestCase("")]
        [TestCase("dbo.")]
        public void DataProvider_DatabaseOwner_Property_Is_Valid(string databaseOwner)
        {
			_logger.LogDebug(">>> Entering DataProvider_DatabaseOwner_Property_Is_Valid(databaseOwner={DatabaseOwner})", databaseOwner);
            // Arrange
            var settings = new Dictionary<string, string> { ["databaseOwner"] = databaseOwner, };
			_logger.LogDebug("DataProvider_DatabaseOwner_Property_Is_Valid(databaseOwner={DatabaseOwner}): settings → {Settings}", databaseOwner, settings);

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(settings));

            // Act
            var provider = DataProvider.Instance();
			_logger.LogDebug("DataProvider_DatabaseOwner_Property_Is_Valid(databaseOwner={DatabaseOwner}): provider → {Provider}", databaseOwner, provider);

            // Assert
            Assert.That(provider.DatabaseOwner, Is.EqualTo(databaseOwner));
			_logger.LogDebug("<<< Exiting DataProvider_DatabaseOwner_Property_Is_Valid(databaseOwner={DatabaseOwner})", databaseOwner);
        }

        [Test]
        [TestCase("")]
        [TestCase("dnn_")]
        public void DataProvider_ObjectQualifier_Property_Is_Valid(string objectQualifier)
        {
			_logger.LogDebug(">>> Entering DataProvider_ObjectQualifier_Property_Is_Valid(objectQualifier={ObjectQualifier})", objectQualifier);
            // Arrange
            var settings = new Dictionary<string, string>();
			_logger.LogDebug("DataProvider_ObjectQualifier_Property_Is_Valid(objectQualifier={ObjectQualifier}): settings → {Settings}", objectQualifier, settings);
            settings["objectQualifier"] = objectQualifier;

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(settings));

            // Act
            var provider = DataProvider.Instance();
			_logger.LogDebug("DataProvider_ObjectQualifier_Property_Is_Valid(objectQualifier={ObjectQualifier}): provider → {Provider}", objectQualifier, provider);

            // Assert
            Assert.That(provider.ObjectQualifier, Is.EqualTo(objectQualifier));
			_logger.LogDebug("<<< Exiting DataProvider_ObjectQualifier_Property_Is_Valid(objectQualifier={ObjectQualifier})", objectQualifier);
        }

        [Test]
        [TestCase("SqlDataProvider")]
        [TestCase("FakeDataProvider")]
        public void DataProvider_ProviderName_Property_Is_Valid(string providerName)
        {
			_logger.LogDebug(">>> Entering DataProvider_ProviderName_Property_Is_Valid(providerName={ProviderName})", providerName);
            // Arrange
            var settings = new Dictionary<string, string>();
			_logger.LogDebug("DataProvider_ProviderName_Property_Is_Valid(providerName={ProviderName}): settings → {Settings}", providerName, settings);
            settings["providerName"] = providerName;

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(settings));

            // Act
            var provider = DataProvider.Instance();
			_logger.LogDebug("DataProvider_ProviderName_Property_Is_Valid(providerName={ProviderName}): provider → {Provider}", providerName, provider);

            // Assert
            Assert.That(provider.ProviderName, Is.EqualTo(providerName));
			_logger.LogDebug("<<< Exiting DataProvider_ProviderName_Property_Is_Valid(providerName={ProviderName})", providerName);
        }

        [Test]
        [TestCase("somePath")]
        [TestCase("someOtherPath")]
        public void DataProvider_ProviderPath_Property_Is_Valid(string providerPath)
        {
			_logger.LogDebug(">>> Entering DataProvider_ProviderPath_Property_Is_Valid(providerPath={ProviderPath})", providerPath);
            // Arrange
            var settings = new Dictionary<string, string>();
			_logger.LogDebug("DataProvider_ProviderPath_Property_Is_Valid(providerPath={ProviderPath}): settings → {Settings}", providerPath, settings);
            settings["providerPath"] = providerPath;

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(settings));

            // Act
            var provider = DataProvider.Instance();
			_logger.LogDebug("DataProvider_ProviderPath_Property_Is_Valid(providerPath={ProviderPath}): provider → {Provider}", providerPath, provider);

            // Assert
            Assert.That(provider.ProviderPath, Is.EqualTo(providerPath));
			_logger.LogDebug("<<< Exiting DataProvider_ProviderPath_Property_Is_Valid(providerPath={ProviderPath})", providerPath);
        }
    }
}
