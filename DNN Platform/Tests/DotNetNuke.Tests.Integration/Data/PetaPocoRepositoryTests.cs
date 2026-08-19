// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Data.PetaPoco;
    using DotNetNuke.Tests.Data.Models;
    using DotNetNuke.Tests.Utilities;
    using Moq;
    using NUnit.Framework;
    using PetaPoco;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class PetaPocoRepositoryTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        // ReSharper disable InconsistentNaming
        private const string connectionStringName = "PetaPoco";
        private readonly string[] _dogAges = Constants.PETAPOCO_DogAges.Split(',');
        private readonly string[] _dogNames = Constants.PETAPOCO_DogNames.Split(',');

        [SetUp]
        public void SetUp()
        {
			_logger.LogDebug(">>> Entering SetUp()");
            Mappers.Revoke(typeof(Dog));
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new SqlDataProvider());
            ComponentFactory.RegisterComponentSettings<SqlDataProvider>(new Dictionary<string, string>()
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
            DataUtil.DeleteDatabase(Constants.PETAPOCO_DatabaseName);
			_logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void PetaPocoRepository_Constructor_Throws_On_Null_Database()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Constructor_Throws_On_Null_Database()");
            // Arrange
            var mockMapper = new Mock<IMapper>();
			_logger.LogDebug("PetaPocoRepository_Constructor_Throws_On_Null_Database(): mockMapper → {MockMapper}", mockMapper);

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => new PetaPocoRepository<Dog>(null, mockMapper.Object));
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Constructor_Throws_On_Null_Database()");
        }

        [Test]
        public void PetaPocoRepository_Constructor_Registers_Mapper()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Constructor_Registers_Mapper()");
            // Arrange
            var mockMapper = new Mock<IMapper>();
			_logger.LogDebug("PetaPocoRepository_Constructor_Registers_Mapper(): mockMapper → {MockMapper}", mockMapper);
            var db = new Database(connectionStringName);
			_logger.LogDebug("PetaPocoRepository_Constructor_Registers_Mapper(): db → {Db}", db);

			_logger.LogDebug("<<< Exiting PetaPocoRepository_Constructor_Registers_Mapper()");
            // Act
#pragma warning disable 168
            var repo = new PetaPocoRepository<Dog>(db, mockMapper.Object);
			_logger.LogDebug("PetaPocoRepository_Constructor_Registers_Mapper(): repo → {Repo}", repo);
#pragma warning restore 168

            // Assert
            Assert.That(Mappers.GetMapper(typeof(Dog), mockMapper.Object), Is.SameAs(mockMapper.Object));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        public void PetaPocoRepository_Get_Returns_All_Rows(int count)
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Get_Returns_All_Rows(count={Count})", count);
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Get_Returns_All_Rows(count={Count}): db → {Db}", count, db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Get_Returns_All_Rows(count={Count}): mapper → {Mapper}", count, mapper);

            DataUtil.SetUpDatabase(count);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Get_Returns_All_Rows(count={Count}): repository → {Repository}", count, repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            IEnumerable<Dog> dogs = repository.Get();

			_logger.LogDebug("PetaPocoRepository_Get_Returns_All_Rows(count={Count}): dogs → {Dogs}", count, dogs);
			_logger.LogInformation("PetaPocoRepository_Get_Returns_All_Rows(count)={Count}: find query executed in {Arg1} ms", count, __logStart.ElapsedMilliseconds);
            // Assert
            Assert.That(dogs.Count(), Is.EqualTo(count));
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Get_Returns_All_Rows(count={Count})", count);
        }

        [Test]
        public void PetaPocoRepository_Get_Returns_List_Of_Models()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Get_Returns_List_Of_Models()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Get_Returns_List_Of_Models(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Get_Returns_List_Of_Models(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Get_Returns_List_Of_Models(): repository → {Repository}", repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var dogs = repository.Get().ToList();

			_logger.LogDebug("PetaPocoRepository_Get_Returns_List_Of_Models(): dogs → {Dogs}", dogs);
			_logger.LogInformation("PetaPocoRepository_Get_Returns_List_Of_Models(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            for (int i = 0; i < dogs.Count(); i++)
            {
                Assert.That(dogs[i], Is.InstanceOf<Dog>());
            }
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Get_Returns_List_Of_Models()");
        }

        [Test]
        public void PetaPocoRepository_Get_Returns_Models_With_Correct_Properties()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Get_Returns_Models_With_Correct_Properties()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties(): repository → {Repository}", repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var dogs = repository.Get();

			_logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties(): dogs → {Dogs}", dogs);
			_logger.LogInformation("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            var dog = dogs.First();
			_logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties(): dog → {Dog}", dog);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(dog.Age.ToString(), Is.EqualTo(this._dogAges[0]));
                Assert.That(dog.Name, Is.EqualTo(this._dogNames[0]));
            }
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Get_Returns_Models_With_Correct_Properties()");
        }

        [Test]
        public void PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper(): db → {Db}", db);
            var mapper = new FluentMapper<Dog>(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper(): mapper → {Mapper}", mapper);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper(): repository → {Repository}", repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var dogs = repository.Get();

			_logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper(): dogs → {Dogs}", dogs);
			_logger.LogInformation("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            var dog = dogs.First();
			_logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper(): dog → {Dog}", dog);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(dog.Age.ToString(), Is.EqualTo(this._dogAges[0]));
                Assert.That(dog.Name, Is.EqualTo(this._dogNames[0]));
            }
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper()");
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id(): repository → {Repository}", repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var dog = repository.GetById(Constants.PETAPOCO_ValidDogId);

			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id(): dog → {Dog}", dog);
			_logger.LogInformation("PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            Assert.That(dog, Is.InstanceOf<Dog>());
			_logger.LogDebug("<<< Exiting PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id()");
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Null_If_InValid_Id()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_GetById_Returns_Null_If_InValid_Id()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id(): repository → {Repository}", repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var dog = repository.GetById(Constants.PETAPOCO_InvalidDogId);

			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id(): dog → {Dog}", dog);
			_logger.LogInformation("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            Assert.That(dog, Is.Null);
			_logger.LogDebug("<<< Exiting PetaPocoRepository_GetById_Returns_Null_If_InValid_Id()");
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper(): db → {Db}", db);
            var mapper = new FluentMapper<Dog>(string.Empty);
			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper(): mapper → {Mapper}", mapper);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper(): repository → {Repository}", repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var dog = repository.GetById(Constants.PETAPOCO_InvalidDogId);

			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper(): dog → {Dog}", dog);
			_logger.LogInformation("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            Assert.That(dog, Is.Null);
			_logger.LogDebug("<<< Exiting PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper()");
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties(): repository → {Repository}", repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var dog = repository.GetById(Constants.PETAPOCO_ValidDogId);

			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties(): dog → {Dog}", dog);
			_logger.LogInformation("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(dog.Age, Is.EqualTo(Constants.PETAPOCO_ValidDogAge));
                Assert.That(dog.Name, Is.EqualTo(Constants.PETAPOCO_ValidDogName));
            }
			_logger.LogDebug("<<< Exiting PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties()");
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper(): db → {Db}", db);
            var mapper = new FluentMapper<Dog>(string.Empty);
			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper(): mapper → {Mapper}", mapper);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper(): repository → {Repository}", repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var dog = repository.GetById(Constants.PETAPOCO_ValidDogId);

			_logger.LogDebug("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper(): dog → {Dog}", dog);
			_logger.LogInformation("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(dog.Age, Is.EqualTo(Constants.PETAPOCO_ValidDogAge));
                Assert.That(dog.Name, Is.EqualTo(Constants.PETAPOCO_ValidDogName));
            }
			_logger.LogDebug("<<< Exiting PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper()");
        }

        [Test]
        public void PetaPocoRepository_Add_Inserts_Item_Into_DataBase()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Add_Inserts_Item_Into_DataBase()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase(): repository → {Repository}", repository);
            var dog = new Dog
            {
                Age = Constants.PETAPOCO_InsertDogAge,
                Name = Constants.PETAPOCO_InsertDogName,
            };

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			_logger.LogInformation("PetaPocoRepository_Add_Inserts_Item_Into_DataBase(): save query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            repository.Insert(dog);

            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount + 1));
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Add_Inserts_Item_Into_DataBase()");
        }

        [Test]
        public void PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ID()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ID()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ID(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ID(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ID(): repository → {Repository}", repository);
            var dog = new Dog
            {
                Age = Constants.PETAPOCO_InsertDogAge,
                Name = Constants.PETAPOCO_InsertDogName,
            };

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			_logger.LogInformation("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ID(): save query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            repository.Insert(dog);

            // Assert
            int newId = DataUtil.GetLastAddedRecordID(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName, Constants.TABLENAME_Key);
            Assert.That(newId, Is.EqualTo(Constants.PETAPOCO_RecordCount + 1));
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ID()");
        }

        [Test]
        public void PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues(): repository → {Repository}", repository);
            var dog = new Dog
            {
                Age = Constants.PETAPOCO_InsertDogAge,
                Name = Constants.PETAPOCO_InsertDogName,
            };

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			_logger.LogInformation("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues(): save query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            repository.Insert(dog);

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
			_logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues(): table → {Table}", table);
            DataRow row = table.Rows[table.Rows.Count - 1];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(row["Age"], Is.EqualTo(Constants.PETAPOCO_InsertDogAge));
                Assert.That(row["Name"], Is.EqualTo(Constants.PETAPOCO_InsertDogName));
            }
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues()");
        }

        [Test]
        public void PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper(): db → {Db}", db);
            var mapper = new FluentMapper<Dog>(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper(): mapper → {Mapper}", mapper);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper(): repository → {Repository}", repository);
            var dog = new Dog
            {
                Age = Constants.PETAPOCO_InsertDogAge,
                Name = Constants.PETAPOCO_InsertDogName,
            };

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			_logger.LogInformation("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper(): save query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            repository.Insert(dog);

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
			_logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper(): table → {Table}", table);
            DataRow row = table.Rows[table.Rows.Count - 1];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(row["Age"], Is.EqualTo(Constants.PETAPOCO_InsertDogAge));
                Assert.That(row["Name"], Is.EqualTo(Constants.PETAPOCO_InsertDogName));
            }
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper()");
        }

        [Test]
        public void PetaPocoRepository_Delete_Deletes_Item_From_DataBase()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Delete_Deletes_Item_From_DataBase()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase(): repository → {Repository}", repository);
            var dog = new Dog
            {
                ID = Constants.PETAPOCO_DeleteDogId,
                Age = Constants.PETAPOCO_DeleteDogAge,
                Name = Constants.PETAPOCO_DeleteDogName,
            };

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			_logger.LogInformation("PetaPocoRepository_Delete_Deletes_Item_From_DataBase(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            repository.Delete(dog);

            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount - 1));
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Delete_Deletes_Item_From_DataBase()");
        }

        [Test]
        public void PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID(): repository → {Repository}", repository);
            var dog = new Dog
            {
                ID = Constants.PETAPOCO_DeleteDogId,
                Age = Constants.PETAPOCO_DeleteDogAge,
                Name = Constants.PETAPOCO_DeleteDogName,
            };

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			_logger.LogInformation("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            repository.Delete(dog);

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
			_logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID(): table → {Table}", table);
            foreach (DataRow row in table.Rows)
            {
                Assert.That((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_DeleteDogId, Is.False);
            }
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID()");
        }

        [Test]
        public void PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper(): db → {Db}", db);
            var mapper = new FluentMapper<Dog>(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper(): mapper → {Mapper}", mapper);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper(): repository → {Repository}", repository);
            var dog = new Dog
            {
                ID = Constants.PETAPOCO_DeleteDogId,
                Age = Constants.PETAPOCO_DeleteDogAge,
                Name = Constants.PETAPOCO_DeleteDogName,
            };

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			_logger.LogInformation("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            repository.Delete(dog);

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
			_logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper(): table → {Table}", table);
            foreach (DataRow row in table.Rows)
            {
                Assert.That((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_DeleteDogId, Is.False);
            }
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper()");
        }

        [Test]
        public void PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID(): repository → {Repository}", repository);
            var dog = new Dog
            {
                ID = Constants.PETAPOCO_InvalidDogId,
                Age = Constants.PETAPOCO_DeleteDogAge,
                Name = Constants.PETAPOCO_DeleteDogName,
            };

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			_logger.LogInformation("PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            repository.Delete(dog);

            // Assert
            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount));
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID()");
        }

        [Test]
        public void PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID_Using_FluentMapper()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID_Using_FluentMapper()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID_Using_FluentMapper(): db → {Db}", db);
            var mapper = new FluentMapper<Dog>(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID_Using_FluentMapper(): mapper → {Mapper}", mapper);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID_Using_FluentMapper(): repository → {Repository}", repository);
            var dog = new Dog
            {
                ID = Constants.PETAPOCO_InvalidDogId,
                Age = Constants.PETAPOCO_DeleteDogAge,
                Name = Constants.PETAPOCO_DeleteDogName,
            };

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			_logger.LogInformation("PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID_Using_FluentMapper(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            repository.Delete(dog);

            // Assert
            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount));
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID_Using_FluentMapper()");
        }

        [Test]
        public void PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase(): repository → {Repository}", repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            repository.Delete("WHERE ID = @0", Constants.PETAPOCO_DeleteDogId);

			_logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase(): ID → {ID}", ID);
			_logger.LogInformation("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount - 1));
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase()");
        }

        [Test]
        public void PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID(): repository → {Repository}", repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            repository.Delete("WHERE ID = @0", Constants.PETAPOCO_DeleteDogId);

			_logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID(): ID → {ID}", ID);
			_logger.LogInformation("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
			_logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID(): table → {Table}", table);
            foreach (DataRow row in table.Rows)
            {
                Assert.That((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_DeleteDogId, Is.False);
            }
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID()");
        }

        [Test]
        public void PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID(): repository → {Repository}", repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            repository.Delete("WHERE ID = @0", Constants.PETAPOCO_InvalidDogId);

			_logger.LogDebug("PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID(): ID → {ID}", ID);
			_logger.LogInformation("PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount));
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID()");
        }

        [Test]
        [TestCase(1, "WHERE ID < @0", 2)]
        [TestCase(4, "WHERE Age <= @0", 5)]
        [TestCase(2, "WHERE Name LIKE @0", "B%")]
        public void PetaPocoRepository_Find_Returns_Correct_Rows(int count, string sqlCondition, object arg)
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Find_Returns_Correct_Rows(count={Count},sqlCondition={SqlCondition},arg={Arg})", count, sqlCondition, arg);
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Find_Returns_Correct_Rows(count={Count},sqlCondition={SqlCondition},arg={Arg}): db → {Db}", count, sqlCondition, arg, db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Find_Returns_Correct_Rows(count={Count},sqlCondition={SqlCondition},arg={Arg}): mapper → {Mapper}", count, sqlCondition, arg, mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Find_Returns_Correct_Rows(count={Count},sqlCondition={SqlCondition},arg={Arg}): repository → {Repository}", count, sqlCondition, arg, repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            IEnumerable<Dog> dogs = repository.Find(sqlCondition, arg);

			_logger.LogDebug("PetaPocoRepository_Find_Returns_Correct_Rows(count={Count},sqlCondition={SqlCondition},arg={Arg}): dogs → {Dogs}", count, sqlCondition, arg, dogs);
			_logger.LogInformation("PetaPocoRepository_Find_Returns_Correct_Rows(count,arg)={Count},{Arg}: find query executed in {Arg2} ms", count, arg, __logStart.ElapsedMilliseconds);
            // Assert
            Assert.That(dogs.Count(), Is.EqualTo(count));
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Find_Returns_Correct_Rows(count={Count},sqlCondition={SqlCondition},arg={Arg})", count, sqlCondition, arg);
        }

        [Test]
        [TestCase(Constants.PAGE_First, Constants.PAGE_RecordCount)]
        [TestCase(Constants.PAGE_Second, Constants.PAGE_RecordCount)]
        [TestCase(Constants.PAGE_Last, Constants.PAGE_RecordCount)]
        public void PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(int pageIndex, int pageSize)
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(pageIndex={PageIndex},pageSize={PageSize})", pageIndex, pageSize);
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(pageIndex={PageIndex},pageSize={PageSize}): db → {Db}", pageIndex, pageSize, db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(pageIndex={PageIndex},pageSize={PageSize}): mapper → {Mapper}", pageIndex, pageSize, mapper);

            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(pageIndex={PageIndex},pageSize={PageSize}): repository → {Repository}", pageIndex, pageSize, repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var dogs = repository.GetPage(pageIndex, pageSize);

			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(pageIndex={PageIndex},pageSize={PageSize}): dogs → {Dogs}", pageIndex, pageSize, dogs);
			_logger.LogInformation("PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(pageIndex,pageSize)={PageIndex},{PageSize}: find query executed in {Arg2} ms", pageIndex, pageSize, __logStart.ElapsedMilliseconds);
            // Assert
            Assert.That(dogs.PageSize, Is.EqualTo(pageSize));
			_logger.LogDebug("<<< Exiting PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(pageIndex={PageIndex},pageSize={PageSize})", pageIndex, pageSize);
        }

        [Test]
        public void PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models(): repository → {Repository}", repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var dogs = repository.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models(): dogs → {Dogs}", dogs);
			_logger.LogInformation("PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            for (int i = 0; i < dogs.Count(); i++)
            {
                Assert.That(dogs[i], Is.InstanceOf<Dog>());
            }
			_logger.LogDebug("<<< Exiting PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models()");
        }

        [Test]
        public void PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties(): repository → {Repository}", repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var dogs = repository.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties(): dogs → {Dogs}", dogs);
			_logger.LogInformation("PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            var dog = dogs.First();
			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties(): dog → {Dog}", dog);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(dog.Age.ToString(), Is.EqualTo(this._dogAges[0]));
                Assert.That(dog.Name, Is.EqualTo(this._dogNames[0]));
            }
			_logger.LogDebug("<<< Exiting PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties()");
        }

        [Test]
        [TestCase(Constants.PAGE_First, Constants.PAGE_RecordCount, 1)]
        [TestCase(Constants.PAGE_Second, Constants.PAGE_RecordCount, 6)]
        [TestCase(2, 4, 9)]
        public void PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(int pageIndex, int pageSize, int firstId)
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(pageIndex={PageIndex},pageSize={PageSize},firstId={FirstId})", pageIndex, pageSize, firstId);
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(pageIndex={PageIndex},pageSize={PageSize},firstId={FirstId}): db → {Db}", pageIndex, pageSize, firstId, db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(pageIndex={PageIndex},pageSize={PageSize},firstId={FirstId}): mapper → {Mapper}", pageIndex, pageSize, firstId, mapper);

            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(pageIndex={PageIndex},pageSize={PageSize},firstId={FirstId}): repository → {Repository}", pageIndex, pageSize, firstId, repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var dogs = repository.GetPage(pageIndex, pageSize);

			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(pageIndex={PageIndex},pageSize={PageSize},firstId={FirstId}): dogs → {Dogs}", pageIndex, pageSize, firstId, dogs);
			_logger.LogInformation("PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(pageIndex,pageSize,firstId)={PageIndex},{PageSize},{FirstId}: find query executed in {Arg3} ms", pageIndex, pageSize, firstId, __logStart.ElapsedMilliseconds);
            // Assert
            var dog = dogs.First();
			_logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(pageIndex={PageIndex},pageSize={PageSize},firstId={FirstId}): dog → {Dog}", pageIndex, pageSize, firstId, dog);
            Assert.That(dog.ID, Is.EqualTo(firstId));
			_logger.LogDebug("<<< Exiting PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(pageIndex={PageIndex},pageSize={PageSize},firstId={FirstId})", pageIndex, pageSize, firstId);
        }

        [Test]
        public void PetaPocoRepository_Update_Updates_Item_In_DataBase()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Update_Updates_Item_In_DataBase()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Update_Updates_Item_In_DataBase(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Update_Updates_Item_In_DataBase(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Update_Updates_Item_In_DataBase(): repository → {Repository}", repository);
            var dog = new Dog
            {
                ID = Constants.PETAPOCO_UpdateDogId,
                Age = Constants.PETAPOCO_UpdateDogAge,
                Name = Constants.PETAPOCO_UpdateDogName,
            };

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			_logger.LogInformation("PetaPocoRepository_Update_Updates_Item_In_DataBase(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            repository.Update(dog);

            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount));
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Update_Updates_Item_In_DataBase()");
        }

        [Test]
        public void PetaPocoRepository_Update_Updates_Item_With_Correct_ID()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Update_Updates_Item_With_Correct_ID()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Update_Updates_Item_With_Correct_ID(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Update_Updates_Item_With_Correct_ID(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Update_Updates_Item_With_Correct_ID(): repository → {Repository}", repository);
            var dog = new Dog
            {
                ID = Constants.PETAPOCO_UpdateDogId,
                Age = Constants.PETAPOCO_UpdateDogAge,
                Name = Constants.PETAPOCO_UpdateDogName,
            };

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			_logger.LogInformation("PetaPocoRepository_Update_Updates_Item_With_Correct_ID(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            repository.Update(dog);

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
			_logger.LogDebug("PetaPocoRepository_Update_Updates_Item_With_Correct_ID(): table → {Table}", table);
            foreach (DataRow row in table.Rows)
            {
                if ((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_UpdateDogId)
                {
                    using (Assert.EnterMultipleScope())
                    {
                        Assert.That(row["Age"], Is.EqualTo(Constants.PETAPOCO_UpdateDogAge));
                        Assert.That(row["Name"], Is.EqualTo(Constants.PETAPOCO_UpdateDogName));
                    }
                }
            }
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Update_Updates_Item_With_Correct_ID()");
        }

        [Test]
        public void PetaPocoRepository_Update_Updates_Item_With_Correct_ID_Using_FluentMapper()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Update_Updates_Item_With_Correct_ID_Using_FluentMapper()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Update_Updates_Item_With_Correct_ID_Using_FluentMapper(): db → {Db}", db);
            var mapper = new FluentMapper<Dog>(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Update_Updates_Item_With_Correct_ID_Using_FluentMapper(): mapper → {Mapper}", mapper);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Update_Updates_Item_With_Correct_ID_Using_FluentMapper(): repository → {Repository}", repository);
            var dog = new Dog
            {
                ID = Constants.PETAPOCO_UpdateDogId,
                Age = Constants.PETAPOCO_UpdateDogAge,
                Name = Constants.PETAPOCO_UpdateDogName,
            };

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			_logger.LogInformation("PetaPocoRepository_Update_Updates_Item_With_Correct_ID_Using_FluentMapper(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            repository.Update(dog);

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
			_logger.LogDebug("PetaPocoRepository_Update_Updates_Item_With_Correct_ID_Using_FluentMapper(): table → {Table}", table);
            foreach (DataRow row in table.Rows)
            {
                if ((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_UpdateDogId)
                {
                    using (Assert.EnterMultipleScope())
                    {
                        Assert.That(row["Age"], Is.EqualTo(Constants.PETAPOCO_UpdateDogAge));
                        Assert.That(row["Name"], Is.EqualTo(Constants.PETAPOCO_UpdateDogName));
                    }
                }
            }
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Update_Updates_Item_With_Correct_ID_Using_FluentMapper()");
        }

        [Test]
        public void PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase(): repository → {Repository}", repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            repository.Update("SET Age=@1, Name=@2 WHERE ID=@0", Constants.PETAPOCO_UpdateDogId, Constants.PETAPOCO_UpdateDogAge, Constants.PETAPOCO_UpdateDogName);

			_logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase(): Age → {Age}", Age);
			_logger.LogInformation("PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount));
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase()");
        }

        [Test]
        public void PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID()
        {
			_logger.LogDebug(">>> Entering PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID()");
            // Arrange
            var db = this.CreatePecaPocoDatabase();
			_logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID(): db → {Db}", db);
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
			_logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID(): repository → {Repository}", repository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            repository.Update("SET Age=@1, Name=@2 WHERE ID=@0", Constants.PETAPOCO_UpdateDogId, Constants.PETAPOCO_UpdateDogAge, Constants.PETAPOCO_UpdateDogName);

			_logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID(): Age → {Age}", Age);
			_logger.LogInformation("PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
			_logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID(): table → {Table}", table);
            foreach (DataRow row in table.Rows)
            {
                if ((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_UpdateDogId)
                {
                    using (Assert.EnterMultipleScope())
                    {
                        Assert.That(row["Age"], Is.EqualTo(Constants.PETAPOCO_UpdateDogAge));
                        Assert.That(row["Name"], Is.EqualTo(Constants.PETAPOCO_UpdateDogName));
                    }
                }
            }
			_logger.LogDebug("<<< Exiting PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID()");
        }

        private Database CreatePecaPocoDatabase()
        {
			_logger.LogDebug(">>> Entering CreatePecaPocoDatabase()");
            var db = new Database(connectionStringName);

			_logger.LogDebug("CreatePecaPocoDatabase(): db → {Db}", db);
			_logger.LogDebug("<<< Exiting CreatePecaPocoDatabase()");
            return db;
        }

        // ReSharper restore InconsistentNaming
    }
}
