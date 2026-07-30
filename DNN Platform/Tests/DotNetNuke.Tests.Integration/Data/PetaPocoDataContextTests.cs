// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Data
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Data;
    using DotNetNuke.Data.PetaPoco;
    using DotNetNuke.Tests.Data.Models;
    using DotNetNuke.Tests.Utilities;

    using Moq;

    using NUnit.Framework;
    using PetaPoco;

    [TestFixture]
    public class PetaPocoDataContextTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string connectionStringName = "PetaPoco";
        private const string tablePrefix = "dnn_";

        // ReSharper disable InconsistentNaming
        [SetUp]
        public void SetUp()
        {
		Logger.LogDebug(">>> Entering SetUp()");
		Logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
		Logger.LogDebug(">>> Entering TearDown()");
		Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void PetaPocoDataContext_Constructors_Throw_On_Null_ConnectionString()
        {
			Logger.LogDebug(">>> Entering PetaPocoDataContext_Constructors_Throw_On_Null_ConnectionString()");
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentException>(() => new PetaPocoDataContext(Mock.Of<IHostSettings>(), null));
            Assert.Throws<ArgumentException>(() => new PetaPocoDataContext(Mock.Of<IHostSettings>(), null, tablePrefix));
			Logger.LogDebug("<<< Exiting PetaPocoDataContext_Constructors_Throw_On_Null_ConnectionString()");
        }

        [Test]
        public void PetaPocoDataContext_Constructors_Throw_On_Empty_ConnectionString()
        {
			Logger.LogDebug(">>> Entering PetaPocoDataContext_Constructors_Throw_On_Empty_ConnectionString()");
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentException>(() => new PetaPocoDataContext(string.Empty));
            Assert.Throws<ArgumentException>(() => new PetaPocoDataContext(string.Empty, tablePrefix));
			Logger.LogDebug("<<< Exiting PetaPocoDataContext_Constructors_Throw_On_Empty_ConnectionString()");
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_Database_Property()
        {
			Logger.LogDebug(">>> Entering PetaPocoDataContext_Constructor_Initialises_Database_Property()");
            // Arrange

            // Act
            var context = new PetaPocoDataContext(connectionStringName);

			Logger.LogDebug("PetaPocoDataContext_Constructor_Initialises_Database_Property(): context → {Context}", context);
            // Assert
            Assert.That(Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database"), Is.InstanceOf<Database>());
			Logger.LogDebug("<<< Exiting PetaPocoDataContext_Constructor_Initialises_Database_Property()");
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_TablePrefix_Property()
        {
			Logger.LogDebug(">>> Entering PetaPocoDataContext_Constructor_Initialises_TablePrefix_Property()");
            // Arrange

            // Act
            var context = new PetaPocoDataContext(connectionStringName, tablePrefix);

			Logger.LogDebug("PetaPocoDataContext_Constructor_Initialises_TablePrefix_Property(): context → {Context}", context);
            // Assert
            Assert.That(context.TablePrefix, Is.EqualTo(tablePrefix));
			Logger.LogDebug("<<< Exiting PetaPocoDataContext_Constructor_Initialises_TablePrefix_Property()");
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_Mapper_Property()
        {
			Logger.LogDebug(">>> Entering PetaPocoDataContext_Constructor_Initialises_Mapper_Property()");
            // Arrange

            // Act
            var context = new PetaPocoDataContext(connectionStringName);

			Logger.LogDebug("PetaPocoDataContext_Constructor_Initialises_Mapper_Property(): context → {Context}", context);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(Util.GetPrivateMember<PetaPocoDataContext, IMapper>(context, "_mapper"), Is.InstanceOf<IMapper>());
                Assert.That(Util.GetPrivateMember<PetaPocoDataContext, PetaPocoMapper>(context, "_mapper"), Is.InstanceOf<PetaPocoMapper>());
            }
			Logger.LogDebug("<<< Exiting PetaPocoDataContext_Constructor_Initialises_Mapper_Property()");
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_FluentMappers_Property()
        {
			Logger.LogDebug(">>> Entering PetaPocoDataContext_Constructor_Initialises_FluentMappers_Property()");
            // Arrange

            // Act
            var context = new PetaPocoDataContext(connectionStringName);

			Logger.LogDebug("PetaPocoDataContext_Constructor_Initialises_FluentMappers_Property(): context → {Context}", context);
            // Assert
            Assert.That(context.FluentMappers, Is.InstanceOf<Dictionary<Type, IMapper>>());
            Assert.That(context.FluentMappers, Is.Empty);
			Logger.LogDebug("<<< Exiting PetaPocoDataContext_Constructor_Initialises_FluentMappers_Property()");
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_Database_Property_With_Correct_Connection()
        {
			Logger.LogDebug(">>> Entering PetaPocoDataContext_Constructor_Initialises_Database_Property_With_Correct_Connection()");
            // Arrange

            // Act
            var context = new PetaPocoDataContext(connectionStringName);

			Logger.LogDebug("PetaPocoDataContext_Constructor_Initialises_Database_Property_With_Correct_Connection(): context → {Context}", context);
            // Assert
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
            string connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
			Logger.LogDebug("PetaPocoDataContext_Constructor_Initialises_Database_Property_With_Correct_Connection(): db → {Db}", db);

            Assert.That(Util.GetPrivateMember<Database, string>(db, "_connectionString"), Is.EqualTo(connectionString));
			Logger.LogDebug("<<< Exiting PetaPocoDataContext_Constructor_Initialises_Database_Property_With_Correct_Connection()");
        }

        [Test]
        public void PetaPocoDataContext_BeginTransaction_Increases_Database_Transaction_Count()
        {
			Logger.LogDebug(">>> Entering PetaPocoDataContext_BeginTransaction_Increases_Database_Transaction_Count()");
            // Arrange
            const int transactionDepth = 1;
            var context = new PetaPocoDataContext(connectionStringName);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
			Logger.LogDebug("PetaPocoDataContext_BeginTransaction_Increases_Database_Transaction_Count(): context → {Context}", context);
			Logger.LogDebug("PetaPocoDataContext_BeginTransaction_Increases_Database_Transaction_Count(): db → {Db}", db);
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            // Act
            context.BeginTransaction();

            // Assert
            Assert.That(Util.GetPrivateMember<Database, int>(db, "_transactionDepth"), Is.EqualTo(transactionDepth + 1));
			Logger.LogDebug("<<< Exiting PetaPocoDataContext_BeginTransaction_Increases_Database_Transaction_Count()");
        }

        [Test]
        public void PetaPocoDataContext_Commit_Decreases_Database_Transaction_Count()
        {
			Logger.LogDebug(">>> Entering PetaPocoDataContext_Commit_Decreases_Database_Transaction_Count()");
            // Arrange
            const int transactionDepth = 2;
            var context = new PetaPocoDataContext(connectionStringName);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
			Logger.LogDebug("PetaPocoDataContext_Commit_Decreases_Database_Transaction_Count(): context → {Context}", context);
			Logger.LogDebug("PetaPocoDataContext_Commit_Decreases_Database_Transaction_Count(): db → {Db}", db);
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            // Act
            context.Commit();

            // Assert
            Assert.That(Util.GetPrivateMember<Database, int>(db, "_transactionDepth"), Is.EqualTo(transactionDepth - 1));
			Logger.LogDebug("<<< Exiting PetaPocoDataContext_Commit_Decreases_Database_Transaction_Count()");
        }

        [Test]
        public void PetaPocoDataContext_Commit_Sets_Database_TransactionCancelled_False()
        {
			Logger.LogDebug(">>> Entering PetaPocoDataContext_Commit_Sets_Database_TransactionCancelled_False()");
            // Arrange
            const int transactionDepth = 2;
            var context = new PetaPocoDataContext(connectionStringName);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
			Logger.LogDebug("PetaPocoDataContext_Commit_Sets_Database_TransactionCancelled_False(): context → {Context}", context);
			Logger.LogDebug("PetaPocoDataContext_Commit_Sets_Database_TransactionCancelled_False(): db → {Db}", db);
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            // Act
            context.Commit();

            // Assert
            Assert.That(Util.GetPrivateMember<Database, bool>(db, "_transactionCancelled"), Is.EqualTo(false));
			Logger.LogDebug("<<< Exiting PetaPocoDataContext_Commit_Sets_Database_TransactionCancelled_False()");
        }

        [Test]
        public void PetaPocoDataContext_RollbackTransaction_Decreases_Database_Transaction_Count()
        {
			Logger.LogDebug(">>> Entering PetaPocoDataContext_RollbackTransaction_Decreases_Database_Transaction_Count()");
            // Arrange
            const int transactionDepth = 2;
            var context = new PetaPocoDataContext(connectionStringName);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
			Logger.LogDebug("PetaPocoDataContext_RollbackTransaction_Decreases_Database_Transaction_Count(): context → {Context}", context);
			Logger.LogDebug("PetaPocoDataContext_RollbackTransaction_Decreases_Database_Transaction_Count(): db → {Db}", db);
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            // Act
            context.RollbackTransaction();

            // Assert
            Assert.That(Util.GetPrivateMember<Database, int>(db, "_transactionDepth"), Is.EqualTo(transactionDepth - 1));
			Logger.LogDebug("<<< Exiting PetaPocoDataContext_RollbackTransaction_Decreases_Database_Transaction_Count()");
        }

        [Test]
        public void PetaPocoDataContext_RollbackTransaction_Sets_Database_TransactionCancelled_True()
        {
			Logger.LogDebug(">>> Entering PetaPocoDataContext_RollbackTransaction_Sets_Database_TransactionCancelled_True()");
            // Arrange
            const int transactionDepth = 2;
            var context = new PetaPocoDataContext(connectionStringName);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
			Logger.LogDebug("PetaPocoDataContext_RollbackTransaction_Sets_Database_TransactionCancelled_True(): context → {Context}", context);
			Logger.LogDebug("PetaPocoDataContext_RollbackTransaction_Sets_Database_TransactionCancelled_True(): db → {Db}", db);
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            // Act
            context.RollbackTransaction();

            // Assert
            Assert.That(Util.GetPrivateMember<Database, bool>(db, "_transactionCancelled"), Is.EqualTo(true));
			Logger.LogDebug("<<< Exiting PetaPocoDataContext_RollbackTransaction_Sets_Database_TransactionCancelled_True()");
        }

        [Test]
        public void PetaPocoDataContext_GetRepository_Returns_Repository()
        {
			Logger.LogDebug(">>> Entering PetaPocoDataContext_GetRepository_Returns_Repository()");
            // Arrange
            var context = new PetaPocoDataContext(connectionStringName);

			Logger.LogDebug("PetaPocoDataContext_GetRepository_Returns_Repository(): context → {Context}", context);
            // Act
            var repo = context.GetRepository<Dog>();

			Logger.LogDebug("PetaPocoDataContext_GetRepository_Returns_Repository(): repo → {Repo}", repo);
            // Assert
            Assert.That(repo, Is.InstanceOf<IRepository<Dog>>());
            Assert.That(repo, Is.InstanceOf<PetaPocoRepository<Dog>>());
			Logger.LogDebug("<<< Exiting PetaPocoDataContext_GetRepository_Returns_Repository()");
        }

        [Test]
        public void PetaPocoDataContext_GetRepository_Sets_Repository_Database_Property()
        {
			Logger.LogDebug(">>> Entering PetaPocoDataContext_GetRepository_Sets_Repository_Database_Property()");
            // Arrange
            var context = new PetaPocoDataContext(connectionStringName);

			Logger.LogDebug("PetaPocoDataContext_GetRepository_Sets_Repository_Database_Property(): context → {Context}", context);
            // Act
            var repo = (PetaPocoRepository<Dog>)context.GetRepository<Dog>();

			Logger.LogDebug("PetaPocoDataContext_GetRepository_Sets_Repository_Database_Property(): repo → {Repo}", repo);
            // Assert
            Assert.That(Util.GetPrivateMember<PetaPocoRepository<Dog>, Database>(repo, "_database"), Is.InstanceOf<Database>());
			Logger.LogDebug("<<< Exiting PetaPocoDataContext_GetRepository_Sets_Repository_Database_Property()");
        }

        [Test]
        public void PetaPocoDataContext_GetRepository_Uses_PetaPocoMapper_If_No_FluentMapper()
        {
			Logger.LogDebug(">>> Entering PetaPocoDataContext_GetRepository_Uses_PetaPocoMapper_If_No_FluentMapper()");
            // Arrange
            var context = new PetaPocoDataContext(connectionStringName);

			Logger.LogDebug("PetaPocoDataContext_GetRepository_Uses_PetaPocoMapper_If_No_FluentMapper(): context → {Context}", context);
            // Act
            var repo = (PetaPocoRepository<Dog>)context.GetRepository<Dog>();

			Logger.LogDebug("PetaPocoDataContext_GetRepository_Uses_PetaPocoMapper_If_No_FluentMapper(): repo → {Repo}", repo);
            // Assert
            Assert.That(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"), Is.InstanceOf<IMapper>());
            Assert.That(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"), Is.InstanceOf<PetaPocoMapper>());
			Logger.LogDebug("<<< Exiting PetaPocoDataContext_GetRepository_Uses_PetaPocoMapper_If_No_FluentMapper()");
        }

        [Test]
        public void PetaPocoDataContext_GetRepository_Uses_FluentMapper_If_FluentMapper_Defined()
        {
			Logger.LogDebug(">>> Entering PetaPocoDataContext_GetRepository_Uses_FluentMapper_If_FluentMapper_Defined()");
            // Arrange
            var context = new PetaPocoDataContext(connectionStringName);
            context.AddFluentMapper<Dog>();
			Logger.LogDebug("PetaPocoDataContext_GetRepository_Uses_FluentMapper_If_FluentMapper_Defined(): context → {Context}", context);

            // Act
            var repo = (PetaPocoRepository<Dog>)context.GetRepository<Dog>();

			Logger.LogDebug("PetaPocoDataContext_GetRepository_Uses_FluentMapper_If_FluentMapper_Defined(): repo → {Repo}", repo);
            // Assert
            Assert.That(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"), Is.InstanceOf<IMapper>());
            Assert.That(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"), Is.InstanceOf<FluentMapper<Dog>>());
			Logger.LogDebug("<<< Exiting PetaPocoDataContext_GetRepository_Uses_FluentMapper_If_FluentMapper_Defined()");
        }

        // ReSharper restore InconsistentNaming
    }
}
