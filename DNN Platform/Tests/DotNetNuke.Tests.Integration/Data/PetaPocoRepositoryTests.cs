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

    [TestFixture]
    public class PetaPocoRepositoryTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        // ReSharper disable InconsistentNaming
        private const string connectionStringName = "PetaPoco";
        private readonly string[] _dogAges = Constants.PETAPOCO_DogAges.Split(',');
        private readonly string[] _dogNames = Constants.PETAPOCO_DogNames.Split(',');

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
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
			Logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            DataUtil.DeleteDatabase(Constants.PETAPOCO_DatabaseName);
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void PetaPocoRepository_Constructor_Throws_On_Null_Database()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Constructor_Throws_On_Null_Database()");
            // Arrange
            var mockMapper = new Mock<IMapper>();

			Logger.LogDebug("PetaPocoRepository_Constructor_Throws_On_Null_Database(): mockMapper → {MockMapper}", mockMapper);
            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => new PetaPocoRepository<Dog>(null, mockMapper.Object));
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Constructor_Throws_On_Null_Database()");
        }

        [Test]
        public void PetaPocoRepository_Constructor_Registers_Mapper()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Constructor_Registers_Mapper()");
            // Arrange
            var mockMapper = new Mock<IMapper>();
            var db = new Database(connectionStringName);
			Logger.LogDebug("PetaPocoRepository_Constructor_Registers_Mapper(): mockMapper → {MockMapper}", mockMapper);
			Logger.LogDebug("PetaPocoRepository_Constructor_Registers_Mapper(): db → {Db}", db);

            // Act
#pragma warning disable 168
            var repo = new PetaPocoRepository<Dog>(db, mockMapper.Object);
#pragma warning restore 168
Logger.LogDebug("PetaPocoRepository_Constructor_Registers_Mapper(): repo → {Repo}", repo);

            // Assert
            Assert.That(Mappers.GetMapper(typeof(Dog), mockMapper.Object), Is.SameAs(mockMapper.Object));
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Constructor_Registers_Mapper()");
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        public void PetaPocoRepository_Get_Returns_All_Rows(int count)
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Get_Returns_All_Rows(count={Count})", count);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Get_Returns_All_Rows(count={Count}): db → {Db}", count, db);
			Logger.LogDebug("PetaPocoRepository_Get_Returns_All_Rows(count={Count}): mapper → {Mapper}", count, mapper);

            DataUtil.SetUpDatabase(count);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_Get_Returns_All_Rows(count={Count}): repository → {Repository}", count, repository);
            // Act
            IEnumerable<Dog> dogs = repository.Get();

			Logger.LogDebug("PetaPocoRepository_Get_Returns_All_Rows(count={Count}): dogs → {Dogs}", count, dogs);
			Logger.LogInformation("PetaPocoRepository_Get_Returns_All_Rows(count)={Count}: find query executed in {Arg1} ms", count, __logStart.ElapsedMilliseconds);
            // Assert
            Assert.That(dogs.Count(), Is.EqualTo(count));
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Get_Returns_All_Rows(count={Count})", count);
        }

        [Test]
        public void PetaPocoRepository_Get_Returns_List_Of_Models()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Get_Returns_List_Of_Models()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Get_Returns_List_Of_Models(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Get_Returns_List_Of_Models(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_Get_Returns_List_Of_Models(): repository → {Repository}", repository);
            // Act
            var dogs = repository.Get().ToList();

			Logger.LogDebug("PetaPocoRepository_Get_Returns_List_Of_Models(): dogs → {Dogs}", dogs);
			Logger.LogInformation("PetaPocoRepository_Get_Returns_List_Of_Models(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            for (int i = 0; i < dogs.Count(); i++)
            {
                Assert.That(dogs[i], Is.InstanceOf<Dog>());
            }
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Get_Returns_List_Of_Models()");
        }

        [Test]
        public void PetaPocoRepository_Get_Returns_Models_With_Correct_Properties()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Get_Returns_Models_With_Correct_Properties()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties(): repository → {Repository}", repository);
            // Act
            var dogs = repository.Get();

			Logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties(): dogs → {Dogs}", dogs);
			Logger.LogInformation("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            var dog = dogs.First();
            using (Assert.EnterMultipleScope())
            {
			Logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties(): dog → {Dog}", dog);
                Assert.That(dog.Age.ToString(), Is.EqualTo(this._dogAges[0]));
                Assert.That(dog.Name, Is.EqualTo(this._dogNames[0]));
            }
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Get_Returns_Models_With_Correct_Properties()");
        }

        [Test]
        public void PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper(): mapper → {Mapper}", mapper);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper(): repository → {Repository}", repository);
            // Act
            var dogs = repository.Get();

			Logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper(): dogs → {Dogs}", dogs);
			Logger.LogInformation("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            var dog = dogs.First();
            using (Assert.EnterMultipleScope())
            {
			Logger.LogDebug("PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper(): dog → {Dog}", dog);
                Assert.That(dog.Age.ToString(), Is.EqualTo(this._dogAges[0]));
                Assert.That(dog.Name, Is.EqualTo(this._dogNames[0]));
            }
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Get_Returns_Models_With_Correct_Properties_Using_FluentMapper()");
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id(): repository → {Repository}", repository);
            // Act
            var dog = repository.GetById(Constants.PETAPOCO_ValidDogId);

			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id(): dog → {Dog}", dog);
			Logger.LogInformation("PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            Assert.That(dog, Is.InstanceOf<Dog>());
			Logger.LogDebug("<<< Exiting PetaPocoRepository_GetById_Returns_Instance_Of_Model_If_Valid_Id()");
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Null_If_InValid_Id()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_GetById_Returns_Null_If_InValid_Id()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id(): repository → {Repository}", repository);
            // Act
            var dog = repository.GetById(Constants.PETAPOCO_InvalidDogId);

			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id(): dog → {Dog}", dog);
			Logger.LogInformation("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            Assert.That(dog, Is.Null);
			Logger.LogDebug("<<< Exiting PetaPocoRepository_GetById_Returns_Null_If_InValid_Id()");
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(string.Empty);
			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper(): mapper → {Mapper}", mapper);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper(): repository → {Repository}", repository);
            // Act
            var dog = repository.GetById(Constants.PETAPOCO_InvalidDogId);

			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper(): dog → {Dog}", dog);
			Logger.LogInformation("PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            Assert.That(dog, Is.Null);
			Logger.LogDebug("<<< Exiting PetaPocoRepository_GetById_Returns_Null_If_InValid_Id_Using_FluentMapper()");
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties(): repository → {Repository}", repository);
            // Act
            var dog = repository.GetById(Constants.PETAPOCO_ValidDogId);

			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties(): dog → {Dog}", dog);
			Logger.LogInformation("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(dog.Age, Is.EqualTo(Constants.PETAPOCO_ValidDogAge));
                Assert.That(dog.Name, Is.EqualTo(Constants.PETAPOCO_ValidDogName));
            }
			Logger.LogDebug("<<< Exiting PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties()");
        }

        [Test]
        public void PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(string.Empty);
			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper(): mapper → {Mapper}", mapper);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper(): repository → {Repository}", repository);
            // Act
            var dog = repository.GetById(Constants.PETAPOCO_ValidDogId);

			Logger.LogDebug("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper(): dog → {Dog}", dog);
			Logger.LogInformation("PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(dog.Age, Is.EqualTo(Constants.PETAPOCO_ValidDogAge));
                Assert.That(dog.Name, Is.EqualTo(Constants.PETAPOCO_ValidDogName));
            }
			Logger.LogDebug("<<< Exiting PetaPocoRepository_GetById_Returns_Model_With_Correct_Properties_Using_FluentMapper()");
        }

        [Test]
        public void PetaPocoRepository_Add_Inserts_Item_Into_DataBase()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Add_Inserts_Item_Into_DataBase()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
			Logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase(): repository → {Repository}", repository);
            {
                Age = Constants.PETAPOCO_InsertDogAge,
                Name = Constants.PETAPOCO_InsertDogName,
            };

            // Act
            repository.Insert(dog);
			Logger.LogInformation("PetaPocoRepository_Add_Inserts_Item_Into_DataBase(): save query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount + 1));
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Add_Inserts_Item_Into_DataBase()");
        }

        [Test]
        public void PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ID()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ID()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ID(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ID(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
			Logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ID(): repository → {Repository}", repository);
            {
                Age = Constants.PETAPOCO_InsertDogAge,
                Name = Constants.PETAPOCO_InsertDogName,
            };

            // Act
            repository.Insert(dog);
			Logger.LogInformation("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ID(): save query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            // Assert
            int newId = DataUtil.GetLastAddedRecordID(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName, Constants.TABLENAME_Key);
            Assert.That(newId, Is.EqualTo(Constants.PETAPOCO_RecordCount + 1));
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ID()");
        }

        [Test]
        public void PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
			Logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues(): repository → {Repository}", repository);
            {
                Age = Constants.PETAPOCO_InsertDogAge,
                Name = Constants.PETAPOCO_InsertDogName,
            };

            // Act
            repository.Insert(dog);
			Logger.LogInformation("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues(): save query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            DataRow row = table.Rows[table.Rows.Count - 1];
			Logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues(): table → {Table}", table);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(row["Age"], Is.EqualTo(Constants.PETAPOCO_InsertDogAge));
                Assert.That(row["Name"], Is.EqualTo(Constants.PETAPOCO_InsertDogName));
            }
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues()");
        }

        [Test]
        public void PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper(): mapper → {Mapper}", mapper);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
			Logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper(): repository → {Repository}", repository);
            {
                Age = Constants.PETAPOCO_InsertDogAge,
                Name = Constants.PETAPOCO_InsertDogName,
            };

            // Act
            repository.Insert(dog);
			Logger.LogInformation("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper(): save query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            DataRow row = table.Rows[table.Rows.Count - 1];
			Logger.LogDebug("PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper(): table → {Table}", table);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(row["Age"], Is.EqualTo(Constants.PETAPOCO_InsertDogAge));
                Assert.That(row["Name"], Is.EqualTo(Constants.PETAPOCO_InsertDogName));
            }
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Add_Inserts_Item_Into_DataBase_With_Correct_ColumnValues_Using_FluentMapper()");
        }

        [Test]
        public void PetaPocoRepository_Delete_Deletes_Item_From_DataBase()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Delete_Deletes_Item_From_DataBase()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
			Logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase(): repository → {Repository}", repository);
            {
                ID = Constants.PETAPOCO_DeleteDogId,
                Age = Constants.PETAPOCO_DeleteDogAge,
                Name = Constants.PETAPOCO_DeleteDogName,
            };

            // Act
            repository.Delete(dog);
			Logger.LogInformation("PetaPocoRepository_Delete_Deletes_Item_From_DataBase(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount - 1));
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Delete_Deletes_Item_From_DataBase()");
        }

        [Test]
        public void PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
			Logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID(): repository → {Repository}", repository);
            {
                ID = Constants.PETAPOCO_DeleteDogId,
                Age = Constants.PETAPOCO_DeleteDogAge,
                Name = Constants.PETAPOCO_DeleteDogName,
            };

            // Act
            repository.Delete(dog);
			Logger.LogInformation("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            foreach (DataRow row in table.Rows)
            {
			Logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID(): table → {Table}", table);
                Assert.That((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_DeleteDogId, Is.False);
            }
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID()");
        }

        [Test]
        public void PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper(): mapper → {Mapper}", mapper);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
			Logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper(): repository → {Repository}", repository);
            {
                ID = Constants.PETAPOCO_DeleteDogId,
                Age = Constants.PETAPOCO_DeleteDogAge,
                Name = Constants.PETAPOCO_DeleteDogName,
            };

            // Act
            repository.Delete(dog);
			Logger.LogInformation("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            foreach (DataRow row in table.Rows)
            {
			Logger.LogDebug("PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper(): table → {Table}", table);
                Assert.That((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_DeleteDogId, Is.False);
            }
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Delete_Deletes_Item_From_DataBase_With_Correct_ID_Using_FluentMapper()");
        }

        [Test]
        public void PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
			Logger.LogDebug("PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID(): repository → {Repository}", repository);
            {
                ID = Constants.PETAPOCO_InvalidDogId,
                Age = Constants.PETAPOCO_DeleteDogAge,
                Name = Constants.PETAPOCO_DeleteDogName,
            };

            // Act
            repository.Delete(dog);
			Logger.LogInformation("PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            // Assert
            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount));
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID()");
        }

        [Test]
        public void PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID_Using_FluentMapper()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID_Using_FluentMapper()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID_Using_FluentMapper(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID_Using_FluentMapper(): mapper → {Mapper}", mapper);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
			Logger.LogDebug("PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID_Using_FluentMapper(): repository → {Repository}", repository);
            {
                ID = Constants.PETAPOCO_InvalidDogId,
                Age = Constants.PETAPOCO_DeleteDogAge,
                Name = Constants.PETAPOCO_DeleteDogName,
            };

            // Act
            repository.Delete(dog);
			Logger.LogInformation("PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID_Using_FluentMapper(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            // Assert
            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount));
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Delete_Does_Nothing_With_Invalid_ID_Using_FluentMapper()");
        }

        [Test]
        public void PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase(): repository → {Repository}", repository);
            // Act
            repository.Delete("WHERE ID = @0", Constants.PETAPOCO_DeleteDogId);

			Logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase(): ID → {ID}", ID);
			Logger.LogInformation("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount - 1));
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase()");
        }

        [Test]
        public void PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID(): repository → {Repository}", repository);
            // Act
            repository.Delete("WHERE ID = @0", Constants.PETAPOCO_DeleteDogId);

			Logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID(): ID → {ID}", ID);
			Logger.LogInformation("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            foreach (DataRow row in table.Rows)
            {
			Logger.LogDebug("PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID(): table → {Table}", table);
                Assert.That((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_DeleteDogId, Is.False);
            }
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Delete_Overload_Deletes_Item_From_DataBase_With_Correct_ID()");
        }

        [Test]
        public void PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID(): repository → {Repository}", repository);
            // Act
            repository.Delete("WHERE ID = @0", Constants.PETAPOCO_InvalidDogId);

			Logger.LogDebug("PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID(): ID → {ID}", ID);
			Logger.LogInformation("PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount));
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Delete_Overload_Does_Nothing_With_Invalid_ID()");
        }

        [Test]
        [TestCase(1, "WHERE ID < @0", 2)]
        [TestCase(4, "WHERE Age <= @0", 5)]
        [TestCase(2, "WHERE Name LIKE @0", "B%")]
        public void PetaPocoRepository_Find_Returns_Correct_Rows(int count, string sqlCondition, object arg)
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Find_Returns_Correct_Rows(count={Count},sqlCondition={SqlCondition},arg={Arg})", count, sqlCondition, arg);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Find_Returns_Correct_Rows(count={Count},sqlCondition={SqlCondition},arg={Arg}): db → {Db}", count, sqlCondition, arg, db);
			Logger.LogDebug("PetaPocoRepository_Find_Returns_Correct_Rows(count={Count},sqlCondition={SqlCondition},arg={Arg}): mapper → {Mapper}", count, sqlCondition, arg, mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_Find_Returns_Correct_Rows(count={Count},sqlCondition={SqlCondition},arg={Arg}): repository → {Repository}", count, sqlCondition, arg, repository);
            // Act
            IEnumerable<Dog> dogs = repository.Find(sqlCondition, arg);

			Logger.LogDebug("PetaPocoRepository_Find_Returns_Correct_Rows(count={Count},sqlCondition={SqlCondition},arg={Arg}): dogs → {Dogs}", count, sqlCondition, arg, dogs);
			Logger.LogInformation("PetaPocoRepository_Find_Returns_Correct_Rows(count,arg)={Count},{Arg}: find query executed in {Arg2} ms", count, arg, __logStart.ElapsedMilliseconds);
            // Assert
            Assert.That(dogs.Count(), Is.EqualTo(count));
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Find_Returns_Correct_Rows(count={Count},sqlCondition={SqlCondition},arg={Arg})", count, sqlCondition, arg);
        }

        [Test]
        [TestCase(Constants.PAGE_First, Constants.PAGE_RecordCount)]
        [TestCase(Constants.PAGE_Second, Constants.PAGE_RecordCount)]
        [TestCase(Constants.PAGE_Last, Constants.PAGE_RecordCount)]
        public void PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(int pageIndex, int pageSize)
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(pageIndex={PageIndex},pageSize={PageSize})", pageIndex, pageSize);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(pageIndex={PageIndex},pageSize={PageSize}): db → {Db}", pageIndex, pageSize, db);
			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(pageIndex={PageIndex},pageSize={PageSize}): mapper → {Mapper}", pageIndex, pageSize, mapper);

            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(pageIndex={PageIndex},pageSize={PageSize}): repository → {Repository}", pageIndex, pageSize, repository);
            // Act
            var dogs = repository.GetPage(pageIndex, pageSize);

			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(pageIndex={PageIndex},pageSize={PageSize}): dogs → {Dogs}", pageIndex, pageSize, dogs);
			Logger.LogInformation("PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(pageIndex,pageSize)={PageIndex},{PageSize}: find query executed in {Arg2} ms", pageIndex, pageSize, __logStart.ElapsedMilliseconds);
            // Assert
            Assert.That(dogs.PageSize, Is.EqualTo(pageSize));
			Logger.LogDebug("<<< Exiting PetaPocoRepository_GetPage_Overload_Returns_Page_Of_Rows(pageIndex={PageIndex},pageSize={PageSize})", pageIndex, pageSize);
        }

        [Test]
        public void PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models(): repository → {Repository}", repository);
            // Act
            var dogs = repository.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models(): dogs → {Dogs}", dogs);
			Logger.LogInformation("PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            for (int i = 0; i < dogs.Count(); i++)
            {
                Assert.That(dogs[i], Is.InstanceOf<Dog>());
            }
			Logger.LogDebug("<<< Exiting PetaPocoRepository_GetPage_Overload_Returns_List_Of_Models()");
        }

        [Test]
        public void PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties(): repository → {Repository}", repository);
            // Act
            var dogs = repository.GetPage(Constants.PAGE_First, Constants.PAGE_RecordCount);

			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties(): dogs → {Dogs}", dogs);
			Logger.LogInformation("PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            var dog = dogs.First();
            using (Assert.EnterMultipleScope())
            {
			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties(): dog → {Dog}", dog);
                Assert.That(dog.Age.ToString(), Is.EqualTo(this._dogAges[0]));
                Assert.That(dog.Name, Is.EqualTo(this._dogNames[0]));
            }
			Logger.LogDebug("<<< Exiting PetaPocoRepository_GetPage_Overload_Returns_Models_With_Correct_Properties()");
        }

        [Test]
        [TestCase(Constants.PAGE_First, Constants.PAGE_RecordCount, 1)]
        [TestCase(Constants.PAGE_Second, Constants.PAGE_RecordCount, 6)]
        [TestCase(2, 4, 9)]
        public void PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(int pageIndex, int pageSize, int firstId)
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(pageIndex={PageIndex},pageSize={PageSize},firstId={FirstId})", pageIndex, pageSize, firstId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(pageIndex={PageIndex},pageSize={PageSize},firstId={FirstId}): db → {Db}", pageIndex, pageSize, firstId, db);
			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(pageIndex={PageIndex},pageSize={PageSize},firstId={FirstId}): mapper → {Mapper}", pageIndex, pageSize, firstId, mapper);

            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(pageIndex={PageIndex},pageSize={PageSize},firstId={FirstId}): repository → {Repository}", pageIndex, pageSize, firstId, repository);
            // Act
            var dogs = repository.GetPage(pageIndex, pageSize);

			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(pageIndex={PageIndex},pageSize={PageSize},firstId={FirstId}): dogs → {Dogs}", pageIndex, pageSize, firstId, dogs);
			Logger.LogInformation("PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(pageIndex,pageSize,firstId)={PageIndex},{PageSize},{FirstId}: find query executed in {Arg3} ms", pageIndex, pageSize, firstId, __logStart.ElapsedMilliseconds);
            // Assert
            var dog = dogs.First();
            Assert.That(dog.ID, Is.EqualTo(firstId));
			Logger.LogDebug("<<< Exiting PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(pageIndex={PageIndex},pageSize={PageSize},firstId={FirstId})", pageIndex, pageSize, firstId);
			Logger.LogDebug("PetaPocoRepository_GetPage_Overload_Returns_Correct_Page(pageIndex={PageIndex},pageSize={PageSize},firstId={FirstId}): dog → {Dog}", pageIndex, pageSize, firstId, dog);
        }

        [Test]
        public void PetaPocoRepository_Update_Updates_Item_In_DataBase()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Update_Updates_Item_In_DataBase()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Update_Updates_Item_In_DataBase(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Update_Updates_Item_In_DataBase(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
			Logger.LogDebug("PetaPocoRepository_Update_Updates_Item_In_DataBase(): repository → {Repository}", repository);
            {
                ID = Constants.PETAPOCO_UpdateDogId,
                Age = Constants.PETAPOCO_UpdateDogAge,
                Name = Constants.PETAPOCO_UpdateDogName,
            };

            // Act
            repository.Update(dog);
			Logger.LogInformation("PetaPocoRepository_Update_Updates_Item_In_DataBase(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount));
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Update_Updates_Item_In_DataBase()");
        }

        [Test]
        public void PetaPocoRepository_Update_Updates_Item_With_Correct_ID()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Update_Updates_Item_With_Correct_ID()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Update_Updates_Item_With_Correct_ID(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Update_Updates_Item_With_Correct_ID(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
			Logger.LogDebug("PetaPocoRepository_Update_Updates_Item_With_Correct_ID(): repository → {Repository}", repository);
            {
                ID = Constants.PETAPOCO_UpdateDogId,
                Age = Constants.PETAPOCO_UpdateDogAge,
                Name = Constants.PETAPOCO_UpdateDogName,
            };

            // Act
            repository.Update(dog);
			Logger.LogInformation("PetaPocoRepository_Update_Updates_Item_With_Correct_ID(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            foreach (DataRow row in table.Rows)
            {
			Logger.LogDebug("PetaPocoRepository_Update_Updates_Item_With_Correct_ID(): table → {Table}", table);
                if ((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_UpdateDogId)
                {
                    using (Assert.EnterMultipleScope())
                    {
                        Assert.That(row["Age"], Is.EqualTo(Constants.PETAPOCO_UpdateDogAge));
                        Assert.That(row["Name"], Is.EqualTo(Constants.PETAPOCO_UpdateDogName));
                    }
                }
            }
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Update_Updates_Item_With_Correct_ID()");
        }

        [Test]
        public void PetaPocoRepository_Update_Updates_Item_With_Correct_ID_Using_FluentMapper()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Update_Updates_Item_With_Correct_ID_Using_FluentMapper()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new FluentMapper<Dog>(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Update_Updates_Item_With_Correct_ID_Using_FluentMapper(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Update_Updates_Item_With_Correct_ID_Using_FluentMapper(): mapper → {Mapper}", mapper);
            mapper.TableName(Constants.PETAPOCO_DogTableName);
            mapper.PrimaryKey("ID");
            mapper.Property(d => d.ID, "ID");
            mapper.Property(d => d.Age, "Age");
            mapper.Property(d => d.Name, "Name");

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);
            var dog = new Dog
			Logger.LogDebug("PetaPocoRepository_Update_Updates_Item_With_Correct_ID_Using_FluentMapper(): repository → {Repository}", repository);
            {
                ID = Constants.PETAPOCO_UpdateDogId,
                Age = Constants.PETAPOCO_UpdateDogAge,
                Name = Constants.PETAPOCO_UpdateDogName,
            };

            // Act
            repository.Update(dog);
			Logger.LogInformation("PetaPocoRepository_Update_Updates_Item_With_Correct_ID_Using_FluentMapper(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            foreach (DataRow row in table.Rows)
            {
			Logger.LogDebug("PetaPocoRepository_Update_Updates_Item_With_Correct_ID_Using_FluentMapper(): table → {Table}", table);
                if ((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_UpdateDogId)
                {
                    using (Assert.EnterMultipleScope())
                    {
                        Assert.That(row["Age"], Is.EqualTo(Constants.PETAPOCO_UpdateDogAge));
                        Assert.That(row["Name"], Is.EqualTo(Constants.PETAPOCO_UpdateDogName));
                    }
                }
            }
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Update_Updates_Item_With_Correct_ID_Using_FluentMapper()");
        }

        [Test]
        public void PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase(): repository → {Repository}", repository);
            // Act
            repository.Update("SET Age=@1, Name=@2 WHERE ID=@0", Constants.PETAPOCO_UpdateDogId, Constants.PETAPOCO_UpdateDogAge, Constants.PETAPOCO_UpdateDogName);

			Logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase(): Age → {Age}", Age);
			Logger.LogInformation("PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount));
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Update_Overload_Updates_Item_In_DataBase()");
        }

        [Test]
        public void PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID()
        {
			Logger.LogDebug(">>> Entering PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var db = this.CreatePecaPocoDatabase();
            var mapper = new PetaPocoMapper(string.Empty);
			Logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID(): db → {Db}", db);
			Logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID(): mapper → {Mapper}", mapper);

            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var repository = new PetaPocoRepository<Dog>(db, mapper);

			Logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID(): repository → {Repository}", repository);
            // Act
            repository.Update("SET Age=@1, Name=@2 WHERE ID=@0", Constants.PETAPOCO_UpdateDogId, Constants.PETAPOCO_UpdateDogAge, Constants.PETAPOCO_UpdateDogName);

			Logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID(): Age → {Age}", Age);
			Logger.LogInformation("PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);
            foreach (DataRow row in table.Rows)
            {
			Logger.LogDebug("PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID(): table → {Table}", table);
                if ((int)row[Constants.TABLENAME_Key] == Constants.PETAPOCO_UpdateDogId)
                {
                    using (Assert.EnterMultipleScope())
                    {
                        Assert.That(row["Age"], Is.EqualTo(Constants.PETAPOCO_UpdateDogAge));
                        Assert.That(row["Name"], Is.EqualTo(Constants.PETAPOCO_UpdateDogName));
                    }
                }
            }
			Logger.LogDebug("<<< Exiting PetaPocoRepository_Update_Overload_Updates_Item_With_Correct_ID()");
        }

        private Database CreatePecaPocoDatabase()
        {
			Logger.LogDebug(">>> Entering CreatePecaPocoDatabase()");
            var db = new Database(connectionStringName);

			Logger.LogDebug("CreatePecaPocoDatabase(): db → {Db}", db);
			Logger.LogDebug("<<< Exiting CreatePecaPocoDatabase()");
            return db;
        }

        // ReSharper restore InconsistentNaming
    }
}
