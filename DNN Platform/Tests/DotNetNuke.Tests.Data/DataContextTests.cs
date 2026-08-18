// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data
{
    using System.Collections.Generic;
    using System.Configuration;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Data.PetaPoco;
    using DotNetNuke.Tests.Utilities;

    using Moq;

    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using PetaPoco;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class DataContextTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			_logger.LogDebug(">>> Entering SetUp()");
            this.serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IApplicationStatusInfo>()));
            ComponentFactory.Container = new SimpleContainer();
			_logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
            ComponentFactory.RegisterComponentInstance<DataProvider>(new SqlDataProvider(Mock.Of<IApplicationStatusInfo>()));
            ComponentFactory.RegisterComponentSettings<SqlDataProvider>(new Dictionary<string, string>
            {
                { "name", "SqlDataProvider" },
                { "type", "DotNetNuke.Data.SqlDataProvider, DotNetNuke" },
                { "connectionStringName", "SiteSqlServer" },
                { "objectQualifier", string.Empty },
                { "databaseOwner", "dbo." },
            });
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
        public void DataContext_Instance_Method_Returns_PetaPocoDataContext()
        {
			_logger.LogDebug(">>> Entering DataContext_Instance_Method_Returns_PetaPocoDataContext()");
            // Arrange

            // Act
            var context = DataContext.Instance(Mock.Of<IHostSettings>());
			_logger.LogDebug("DataContext_Instance_Method_Returns_PetaPocoDataContext(): context → {Context}", context);

            // Assert
            Assert.That(context, Is.InstanceOf<IDataContext>());
            Assert.That(context, Is.InstanceOf<PetaPocoDataContext>());
			_logger.LogDebug("<<< Exiting DataContext_Instance_Method_Returns_PetaPocoDataContext()");
        }

        [Test]
        public void DataContext_Instance_Method_Returns_Default_PetaPocoDataContext_Instance()
        {
			_logger.LogDebug(">>> Entering DataContext_Instance_Method_Returns_Default_PetaPocoDataContext_Instance()");
            // Arrange
            var connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;

            // Act
            var context = (PetaPocoDataContext)DataContext.Instance(Mock.Of<IHostSettings>());
			_logger.LogDebug("DataContext_Instance_Method_Returns_Default_PetaPocoDataContext_Instance(): context → {Context}", context);

            // Assert
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "database");
			_logger.LogDebug("DataContext_Instance_Method_Returns_Default_PetaPocoDataContext_Instance(): db → {Db}", db);
            Assert.That(Util.GetPrivateMember<Database, string>(db, "_connectionString"), Is.EqualTo(connectionString));
			_logger.LogDebug("<<< Exiting DataContext_Instance_Method_Returns_Default_PetaPocoDataContext_Instance()");
        }

        [Test]
        [TestCase("PetaPoco")]
        [TestCase("Test")]
        public void DataContext_Instance_Method_Returns_Named_PetaPocoDataContext_Instance(string name)
        {
			_logger.LogDebug(">>> Entering DataContext_Instance_Method_Returns_Named_PetaPocoDataContext_Instance(name={Name})", name);
            // Arrange
            var connectionString = ConfigurationManager.ConnectionStrings[name].ConnectionString;

            // Act
            var context = (PetaPocoDataContext)DataContext.Instance(Mock.Of<IHostSettings>(), name);
			_logger.LogDebug("DataContext_Instance_Method_Returns_Named_PetaPocoDataContext_Instance(name={Name}): context → {Context}", name, context);

            // Assert
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "database");
			_logger.LogDebug("DataContext_Instance_Method_Returns_Named_PetaPocoDataContext_Instance(name={Name}): db → {Db}", name, db);
            Assert.That(Util.GetPrivateMember<Database, string>(db, "_connectionString"), Is.EqualTo(connectionString));
			_logger.LogDebug("<<< Exiting DataContext_Instance_Method_Returns_Named_PetaPocoDataContext_Instance(name={Name})", name);
        }
    }
}
