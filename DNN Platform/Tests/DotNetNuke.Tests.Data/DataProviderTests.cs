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

    [TestFixture]
    public class DataProviderTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            this.serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IApplicationStatusInfo>()));
			Logger.LogDebug("<<< Exiting SetUp()");
			Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void DataProvider_Instance_Method_Returns_Instance()
        {
			Logger.LogDebug(">>> Entering DataProvider_Instance_Method_Returns_Instance()");
            // Arrange
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(new Dictionary<string, string>()));

            // Act
            var provider = DataProvider.Instance();

			Logger.LogDebug("DataProvider_Instance_Method_Returns_Instance(): provider → {Provider}", provider);
            // Assert
            Assert.That(provider, Is.InstanceOf<DataProvider>());
            Assert.That(provider, Is.InstanceOf<FakeDataProvider>());
			Logger.LogDebug("<<< Exiting DataProvider_Instance_Method_Returns_Instance()");
        }

        [Test]
        public void DataProvider_ConnectionString_Property_Is_Valid()
        {
			Logger.LogDebug(">>> Entering DataProvider_ConnectionString_Property_Is_Valid()");
            // Arrange
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(new Dictionary<string, string>()));

            var connectionString = Config.GetConnectionString();

			Logger.LogDebug("DataProvider_ConnectionString_Property_Is_Valid(): connectionString → {ConnectionString}", connectionString);
            // Act
            var provider = DataProvider.Instance();

			Logger.LogDebug("DataProvider_ConnectionString_Property_Is_Valid(): provider → {Provider}", provider);
            // Assert
            Assert.That(provider.ConnectionString, Is.EqualTo(connectionString));
			Logger.LogDebug("<<< Exiting DataProvider_ConnectionString_Property_Is_Valid()");
        }

        [Test]
        [TestCase("")]
        [TestCase("dbo.")]
        public void DataProvider_DatabaseOwner_Property_Is_Valid(string databaseOwner)
        {
			Logger.LogDebug(">>> Entering DataProvider_DatabaseOwner_Property_Is_Valid(databaseOwner={DatabaseOwner})", databaseOwner);
            // Arrange
            var settings = new Dictionary<string, string> { ["databaseOwner"] = databaseOwner, };

			Logger.LogDebug("DataProvider_DatabaseOwner_Property_Is_Valid(databaseOwner={DatabaseOwner}): settings → {Settings}", databaseOwner, settings);
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(settings));

            // Act
            var provider = DataProvider.Instance();

			Logger.LogDebug("DataProvider_DatabaseOwner_Property_Is_Valid(databaseOwner={DatabaseOwner}): provider → {Provider}", databaseOwner, provider);
            // Assert
            Assert.That(provider.DatabaseOwner, Is.EqualTo(databaseOwner));
			Logger.LogDebug("<<< Exiting DataProvider_DatabaseOwner_Property_Is_Valid(databaseOwner={DatabaseOwner})", databaseOwner);
        }

        [Test]
        [TestCase("")]
        [TestCase("dnn_")]
        public void DataProvider_ObjectQualifier_Property_Is_Valid(string objectQualifier)
        {
			Logger.LogDebug(">>> Entering DataProvider_ObjectQualifier_Property_Is_Valid(objectQualifier={ObjectQualifier})", objectQualifier);
            // Arrange
            var settings = new Dictionary<string, string>();
            settings["objectQualifier"] = objectQualifier;
			Logger.LogDebug("DataProvider_ObjectQualifier_Property_Is_Valid(objectQualifier={ObjectQualifier}): settings → {Settings}", objectQualifier, settings);

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(settings));

            // Act
            var provider = DataProvider.Instance();

			Logger.LogDebug("DataProvider_ObjectQualifier_Property_Is_Valid(objectQualifier={ObjectQualifier}): provider → {Provider}", objectQualifier, provider);
            // Assert
            Assert.That(provider.ObjectQualifier, Is.EqualTo(objectQualifier));
			Logger.LogDebug("<<< Exiting DataProvider_ObjectQualifier_Property_Is_Valid(objectQualifier={ObjectQualifier})", objectQualifier);
        }

        [Test]
        [TestCase("SqlDataProvider")]
        [TestCase("FakeDataProvider")]
        public void DataProvider_ProviderName_Property_Is_Valid(string providerName)
        {
			Logger.LogDebug(">>> Entering DataProvider_ProviderName_Property_Is_Valid(providerName={ProviderName})", providerName);
            // Arrange
            var settings = new Dictionary<string, string>();
            settings["providerName"] = providerName;
			Logger.LogDebug("DataProvider_ProviderName_Property_Is_Valid(providerName={ProviderName}): settings → {Settings}", providerName, settings);

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(settings));

            // Act
            var provider = DataProvider.Instance();

			Logger.LogDebug("DataProvider_ProviderName_Property_Is_Valid(providerName={ProviderName}): provider → {Provider}", providerName, provider);
            // Assert
            Assert.That(provider.ProviderName, Is.EqualTo(providerName));
			Logger.LogDebug("<<< Exiting DataProvider_ProviderName_Property_Is_Valid(providerName={ProviderName})", providerName);
        }

        [Test]
        [TestCase("somePath")]
        [TestCase("someOtherPath")]
        public void DataProvider_ProviderPath_Property_Is_Valid(string providerPath)
        {
			Logger.LogDebug(">>> Entering DataProvider_ProviderPath_Property_Is_Valid(providerPath={ProviderPath})", providerPath);
            // Arrange
            var settings = new Dictionary<string, string>();
            settings["providerPath"] = providerPath;
			Logger.LogDebug("DataProvider_ProviderPath_Property_Is_Valid(providerPath={ProviderPath}): settings → {Settings}", providerPath, settings);

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new FakeDataProvider(settings));

            // Act
            var provider = DataProvider.Instance();

			Logger.LogDebug("DataProvider_ProviderPath_Property_Is_Valid(providerPath={ProviderPath}): provider → {Provider}", providerPath, provider);
            // Assert
            Assert.That(provider.ProviderPath, Is.EqualTo(providerPath));
			Logger.LogDebug("<<< Exiting DataProvider_ProviderPath_Property_Is_Valid(providerPath={ProviderPath})", providerPath);
        }
    }
}
