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
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class PetaPocoDataContextTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private const string connectionStringName = "PetaPoco";
        private const string tablePrefix = "dnn_";

        // ReSharper disable InconsistentNaming
        [SetUp]
        public void SetUp()
        {
		_logger.LogDebug(">>> Entering SetUp()");
		_logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
		_logger.LogDebug(">>> Entering TearDown()");
		_logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void PetaPocoDataContext_Constructors_Throw_On_Null_ConnectionString()
        {
			_logger.LogDebug(">>> Entering PetaPocoDataContext_Constructors_Throw_On_Null_ConnectionString()");
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentException>(() => new PetaPocoDataContext(Mock.Of<IHostSettings>(), null));
            Assert.Throws<ArgumentException>(() => new PetaPocoDataContext(Mock.Of<IHostSettings>(), null, tablePrefix));
			_logger.LogDebug("<<< Exiting PetaPocoDataContext_Constructors_Throw_On_Null_ConnectionString()");
        }

        [Test]
        public void PetaPocoDataContext_Constructors_Throw_On_Empty_ConnectionString()
        {
			_logger.LogDebug(">>> Entering PetaPocoDataContext_Constructors_Throw_On_Empty_ConnectionString()");
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentException>(() => new PetaPocoDataContext(string.Empty));
            Assert.Throws<ArgumentException>(() => new PetaPocoDataContext(string.Empty, tablePrefix));
			_logger.LogDebug("<<< Exiting PetaPocoDataContext_Constructors_Throw_On_Empty_ConnectionString()");
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_Database_Property()
        {
			_logger.LogDebug(">>> Entering PetaPocoDataContext_Constructor_Initialises_Database_Property()");
            // Arrange

            // Act
            var context = new PetaPocoDataContext(connectionStringName);
			_logger.LogDebug("PetaPocoDataContext_Constructor_Initialises_Database_Property(): context → {Context}", context);

            // Assert
            Assert.That(Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database"), Is.InstanceOf<Database>());
			_logger.LogDebug("<<< Exiting PetaPocoDataContext_Constructor_Initialises_Database_Property()");
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_TablePrefix_Property()
        {
			_logger.LogDebug(">>> Entering PetaPocoDataContext_Constructor_Initialises_TablePrefix_Property()");
            // Arrange

            // Act
            var context = new PetaPocoDataContext(connectionStringName, tablePrefix);
			_logger.LogDebug("PetaPocoDataContext_Constructor_Initialises_TablePrefix_Property(): context → {Context}", context);

            // Assert
            Assert.That(context.TablePrefix, Is.EqualTo(tablePrefix));
			_logger.LogDebug("<<< Exiting PetaPocoDataContext_Constructor_Initialises_TablePrefix_Property()");
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_Mapper_Property()
        {
			_logger.LogDebug(">>> Entering PetaPocoDataContext_Constructor_Initialises_Mapper_Property()");
            // Arrange

            // Act
            var context = new PetaPocoDataContext(connectionStringName);
			_logger.LogDebug("PetaPocoDataContext_Constructor_Initialises_Mapper_Property(): context → {Context}", context);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(Util.GetPrivateMember<PetaPocoDataContext, IMapper>(context, "_mapper"), Is.InstanceOf<IMapper>());
                Assert.That(Util.GetPrivateMember<PetaPocoDataContext, PetaPocoMapper>(context, "_mapper"), Is.InstanceOf<PetaPocoMapper>());
            }
			_logger.LogDebug("<<< Exiting PetaPocoDataContext_Constructor_Initialises_Mapper_Property()");
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_FluentMappers_Property()
        {
			_logger.LogDebug(">>> Entering PetaPocoDataContext_Constructor_Initialises_FluentMappers_Property()");
            // Arrange

            // Act
            var context = new PetaPocoDataContext(connectionStringName);
			_logger.LogDebug("PetaPocoDataContext_Constructor_Initialises_FluentMappers_Property(): context → {Context}", context);

            // Assert
            Assert.That(context.FluentMappers, Is.InstanceOf<Dictionary<Type, IMapper>>());
            Assert.That(context.FluentMappers, Is.Empty);
			_logger.LogDebug("<<< Exiting PetaPocoDataContext_Constructor_Initialises_FluentMappers_Property()");
        }

        [Test]
        public void PetaPocoDataContext_Constructor_Initialises_Database_Property_With_Correct_Connection()
        {
			_logger.LogDebug(">>> Entering PetaPocoDataContext_Constructor_Initialises_Database_Property_With_Correct_Connection()");
            // Arrange

            // Act
            var context = new PetaPocoDataContext(connectionStringName);
			_logger.LogDebug("PetaPocoDataContext_Constructor_Initialises_Database_Property_With_Correct_Connection(): context → {Context}", context);

            // Assert
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
			_logger.LogDebug("PetaPocoDataContext_Constructor_Initialises_Database_Property_With_Correct_Connection(): db → {Db}", db);
            string connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

            Assert.That(Util.GetPrivateMember<Database, string>(db, "_connectionString"), Is.EqualTo(connectionString));
			_logger.LogDebug("<<< Exiting PetaPocoDataContext_Constructor_Initialises_Database_Property_With_Correct_Connection()");
        }

        [Test]
        public void PetaPocoDataContext_BeginTransaction_Increases_Database_Transaction_Count()
        {
			_logger.LogDebug(">>> Entering PetaPocoDataContext_BeginTransaction_Increases_Database_Transaction_Count()");
            // Arrange
            const int transactionDepth = 1;
            var context = new PetaPocoDataContext(connectionStringName);
			_logger.LogDebug("PetaPocoDataContext_BeginTransaction_Increases_Database_Transaction_Count(): context → {Context}", context);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
			_logger.LogDebug("PetaPocoDataContext_BeginTransaction_Increases_Database_Transaction_Count(): db → {Db}", db);
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            // Act
            context.BeginTransaction();

            // Assert
            Assert.That(Util.GetPrivateMember<Database, int>(db, "_transactionDepth"), Is.EqualTo(transactionDepth + 1));
			_logger.LogDebug("<<< Exiting PetaPocoDataContext_BeginTransaction_Increases_Database_Transaction_Count()");
        }

        [Test]
        public void PetaPocoDataContext_Commit_Decreases_Database_Transaction_Count()
        {
			_logger.LogDebug(">>> Entering PetaPocoDataContext_Commit_Decreases_Database_Transaction_Count()");
            // Arrange
            const int transactionDepth = 2;
            var context = new PetaPocoDataContext(connectionStringName);
			_logger.LogDebug("PetaPocoDataContext_Commit_Decreases_Database_Transaction_Count(): context → {Context}", context);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
			_logger.LogDebug("PetaPocoDataContext_Commit_Decreases_Database_Transaction_Count(): db → {Db}", db);
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            // Act
            context.Commit();

            // Assert
            Assert.That(Util.GetPrivateMember<Database, int>(db, "_transactionDepth"), Is.EqualTo(transactionDepth - 1));
			_logger.LogDebug("<<< Exiting PetaPocoDataContext_Commit_Decreases_Database_Transaction_Count()");
        }

        [Test]
        public void PetaPocoDataContext_Commit_Sets_Database_TransactionCancelled_False()
        {
			_logger.LogDebug(">>> Entering PetaPocoDataContext_Commit_Sets_Database_TransactionCancelled_False()");
            // Arrange
            const int transactionDepth = 2;
            var context = new PetaPocoDataContext(connectionStringName);
			_logger.LogDebug("PetaPocoDataContext_Commit_Sets_Database_TransactionCancelled_False(): context → {Context}", context);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
			_logger.LogDebug("PetaPocoDataContext_Commit_Sets_Database_TransactionCancelled_False(): db → {Db}", db);
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            // Act
            context.Commit();

            // Assert
            Assert.That(Util.GetPrivateMember<Database, bool>(db, "_transactionCancelled"), Is.EqualTo(false));
			_logger.LogDebug("<<< Exiting PetaPocoDataContext_Commit_Sets_Database_TransactionCancelled_False()");
        }

        [Test]
        public void PetaPocoDataContext_RollbackTransaction_Decreases_Database_Transaction_Count()
        {
			_logger.LogDebug(">>> Entering PetaPocoDataContext_RollbackTransaction_Decreases_Database_Transaction_Count()");
            // Arrange
            const int transactionDepth = 2;
            var context = new PetaPocoDataContext(connectionStringName);
			_logger.LogDebug("PetaPocoDataContext_RollbackTransaction_Decreases_Database_Transaction_Count(): context → {Context}", context);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
			_logger.LogDebug("PetaPocoDataContext_RollbackTransaction_Decreases_Database_Transaction_Count(): db → {Db}", db);
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            // Act
            context.RollbackTransaction();

            // Assert
            Assert.That(Util.GetPrivateMember<Database, int>(db, "_transactionDepth"), Is.EqualTo(transactionDepth - 1));
			_logger.LogDebug("<<< Exiting PetaPocoDataContext_RollbackTransaction_Decreases_Database_Transaction_Count()");
        }

        [Test]
        public void PetaPocoDataContext_RollbackTransaction_Sets_Database_TransactionCancelled_True()
        {
			_logger.LogDebug(">>> Entering PetaPocoDataContext_RollbackTransaction_Sets_Database_TransactionCancelled_True()");
            // Arrange
            const int transactionDepth = 2;
            var context = new PetaPocoDataContext(connectionStringName);
			_logger.LogDebug("PetaPocoDataContext_RollbackTransaction_Sets_Database_TransactionCancelled_True(): context → {Context}", context);
            Database db = Util.GetPrivateMember<PetaPocoDataContext, Database>(context, "_database");
			_logger.LogDebug("PetaPocoDataContext_RollbackTransaction_Sets_Database_TransactionCancelled_True(): db → {Db}", db);
            Util.SetPrivateMember(db, "_transactionDepth", transactionDepth);

            // Act
            context.RollbackTransaction();

            // Assert
            Assert.That(Util.GetPrivateMember<Database, bool>(db, "_transactionCancelled"), Is.EqualTo(true));
			_logger.LogDebug("<<< Exiting PetaPocoDataContext_RollbackTransaction_Sets_Database_TransactionCancelled_True()");
        }

        [Test]
        public void PetaPocoDataContext_GetRepository_Returns_Repository()
        {
			_logger.LogDebug(">>> Entering PetaPocoDataContext_GetRepository_Returns_Repository()");
            // Arrange
            var context = new PetaPocoDataContext(connectionStringName);
			_logger.LogDebug("PetaPocoDataContext_GetRepository_Returns_Repository(): context → {Context}", context);

            // Act
            var repo = context.GetRepository<Dog>();
			_logger.LogDebug("PetaPocoDataContext_GetRepository_Returns_Repository(): repo → {Repo}", repo);

            // Assert
            Assert.That(repo, Is.InstanceOf<IRepository<Dog>>());
            Assert.That(repo, Is.InstanceOf<PetaPocoRepository<Dog>>());
			_logger.LogDebug("<<< Exiting PetaPocoDataContext_GetRepository_Returns_Repository()");
        }

        [Test]
        public void PetaPocoDataContext_GetRepository_Sets_Repository_Database_Property()
        {
			_logger.LogDebug(">>> Entering PetaPocoDataContext_GetRepository_Sets_Repository_Database_Property()");
            // Arrange
            var context = new PetaPocoDataContext(connectionStringName);
			_logger.LogDebug("PetaPocoDataContext_GetRepository_Sets_Repository_Database_Property(): context → {Context}", context);

            // Act
            var repo = (PetaPocoRepository<Dog>)context.GetRepository<Dog>();
			_logger.LogDebug("PetaPocoDataContext_GetRepository_Sets_Repository_Database_Property(): repo → {Repo}", repo);

            // Assert
            Assert.That(Util.GetPrivateMember<PetaPocoRepository<Dog>, Database>(repo, "_database"), Is.InstanceOf<Database>());
			_logger.LogDebug("<<< Exiting PetaPocoDataContext_GetRepository_Sets_Repository_Database_Property()");
        }

        [Test]
        public void PetaPocoDataContext_GetRepository_Uses_PetaPocoMapper_If_No_FluentMapper()
        {
			_logger.LogDebug(">>> Entering PetaPocoDataContext_GetRepository_Uses_PetaPocoMapper_If_No_FluentMapper()");
            // Arrange
            var context = new PetaPocoDataContext(connectionStringName);
			_logger.LogDebug("PetaPocoDataContext_GetRepository_Uses_PetaPocoMapper_If_No_FluentMapper(): context → {Context}", context);

            // Act
            var repo = (PetaPocoRepository<Dog>)context.GetRepository<Dog>();
			_logger.LogDebug("PetaPocoDataContext_GetRepository_Uses_PetaPocoMapper_If_No_FluentMapper(): repo → {Repo}", repo);

            // Assert
            Assert.That(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"), Is.InstanceOf<IMapper>());
            Assert.That(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"), Is.InstanceOf<PetaPocoMapper>());
			_logger.LogDebug("<<< Exiting PetaPocoDataContext_GetRepository_Uses_PetaPocoMapper_If_No_FluentMapper()");
        }

        [Test]
        public void PetaPocoDataContext_GetRepository_Uses_FluentMapper_If_FluentMapper_Defined()
        {
			_logger.LogDebug(">>> Entering PetaPocoDataContext_GetRepository_Uses_FluentMapper_If_FluentMapper_Defined()");
            // Arrange
            var context = new PetaPocoDataContext(connectionStringName);
			_logger.LogDebug("PetaPocoDataContext_GetRepository_Uses_FluentMapper_If_FluentMapper_Defined(): context → {Context}", context);
            context.AddFluentMapper<Dog>();

            // Act
            var repo = (PetaPocoRepository<Dog>)context.GetRepository<Dog>();
			_logger.LogDebug("PetaPocoDataContext_GetRepository_Uses_FluentMapper_If_FluentMapper_Defined(): repo → {Repo}", repo);

            // Assert
            Assert.That(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"), Is.InstanceOf<IMapper>());
            Assert.That(Util.GetPrivateMember<PetaPocoRepository<Dog>, IMapper>(repo, "_mapper"), Is.InstanceOf<FluentMapper<Dog>>());
			_logger.LogDebug("<<< Exiting PetaPocoDataContext_GetRepository_Uses_FluentMapper_If_FluentMapper_Defined()");
        }

        // ReSharper restore InconsistentNaming
    }
}
