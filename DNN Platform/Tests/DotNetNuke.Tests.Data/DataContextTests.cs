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

    [TestFixture]
    public class DataContextTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            this.serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IApplicationStatusInfo>()));
            ComponentFactory.Container = new SimpleContainer();
			Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
            ComponentFactory.RegisterComponentInstance<DataProvider>(new SqlDataProvider(Mock.Of<IApplicationStatusInfo>()));
            ComponentFactory.RegisterComponentSettings<SqlDataProvider>(new Dictionary<string, string>
            {
                { "name", "SqlDataProvider" },
                { "type", "DotNetNuke.Data.SqlDataProvider, DotNetNuke" },
                { "connectionStringName", "SiteSqlServer" },
                { "objectQualifier", string.Empty },
                { "databaseOwner", "dbo." },
            });
			Logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void DataContext_Instance_Method_Returns_PetaPocoDataContext()
        {
			Logger.LogDebug(">>> Entering DataContext_Instance_Method_Returns_PetaPocoDataContext()");
            // Arrange

            // Act
            var context = DataContext.Instance(Mock.Of<IHostSettings>());

			Logger.LogDebug("DataContext_Instance_Method_Returns_PetaPocoDataContext(): context → {Context}", context);
            // Assert
            Assert.That(context, Is.InstanceOf<IDataContext>());
            Assert.That(context, Is.InstanceOf<PetaPocoDataContext>());
			Logger.LogDebug("<<< Exiting DataContext_Instance_Method_Returns_PetaPocoDataContext()");
        }

        [Test]
        public void DataContext_Instance_Method_Returns_Default_PetaPocoDataContext_Instance()
        {
			Logger.LogDebug(">>> Entering DataContext_Instance_Method_Returns_Default_PetaPocoDataContext_Instance()");
            // Arrange
            var connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;

            // Act
            var context = (PetaPocoDataContext)DataContext.Instance(Mock.Of<IHostSettings>());

			Logger.LogDebug("DataContext_Instance_Method_Returns_Default_PetaPocoDataContext_Instance(): context → {Context}", context);
            // Assert
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "database");
            Assert.That(Util.GetPrivateMember<Database, string>(db, "_connectionString"), Is.EqualTo(connectionString));
			Logger.LogDebug("<<< Exiting DataContext_Instance_Method_Returns_Default_PetaPocoDataContext_Instance()");
			Logger.LogDebug("DataContext_Instance_Method_Returns_Default_PetaPocoDataContext_Instance(): db → {Db}", db);
        }

        [Test]
        [TestCase("PetaPoco")]
        [TestCase("Test")]
        public void DataContext_Instance_Method_Returns_Named_PetaPocoDataContext_Instance(string name)
        {
			Logger.LogDebug(">>> Entering DataContext_Instance_Method_Returns_Named_PetaPocoDataContext_Instance(name={Name})", name);
            // Arrange
            var connectionString = ConfigurationManager.ConnectionStrings[name].ConnectionString;

            // Act
            var context = (PetaPocoDataContext)DataContext.Instance(Mock.Of<IHostSettings>(), name);

			Logger.LogDebug("DataContext_Instance_Method_Returns_Named_PetaPocoDataContext_Instance(name={Name}): context → {Context}", name, context);
            // Assert
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "database");
            Assert.That(Util.GetPrivateMember<Database, string>(db, "_connectionString"), Is.EqualTo(connectionString));
			Logger.LogDebug("<<< Exiting DataContext_Instance_Method_Returns_Named_PetaPocoDataContext_Instance(name={Name})", name);
			Logger.LogDebug("DataContext_Instance_Method_Returns_Named_PetaPocoDataContext_Instance(name={Name}): db → {Db}", name, db);
        }
    }
}
