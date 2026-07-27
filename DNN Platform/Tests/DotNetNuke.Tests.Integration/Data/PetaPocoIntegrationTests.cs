// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using DotNetNuke.Collections;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Data.PetaPoco;
    using DotNetNuke.Tests.Data.Models;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;
    using PetaPoco;

    [TestFixture]
    public class PetaPocoIntegrationTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string ConnectionStringName = "PetaPoco";

        private Dictionary<Type, IMapper> _mappers;

        // ReSharper disable InconsistentNaming
        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
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

            var dogMapper = new FluentMapper<Dog>(string.Empty)
                    .TableName(Constants.PETAPOCO_DogTableName)
                    .PrimaryKey("ID")
                    .Property(d => d.ID, "ID")
                    .Property(d => d.Age, "Age")
                    .Property(d => d.Name, "Name");

            var catMapper = new FluentMapper<Cat>(string.Empty)
                    .TableName(Constants.PETAPOCO_DogTableName)
                    .Property(d => d.Age, "Age")
                    .Property(d => d.Name, "Name");

            this._mappers = new Dictionary<Type, IMapper> { { typeof(Dog), dogMapper } };
			Logger.LogDebug("SetUp(): _mappers → {_mappers}", _mappers);
            this._mappers = new Dictionary<Type, IMapper> { { typeof(Cat), catMapper } };
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
        public void PetaPoco_Add_Inserts_Item()
        {
			Logger.LogDebug(">>> Entering PetaPoco_Add_Inserts_Item()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var dog = new Dog
            {
                Age = Constants.PETAPOCO_InsertDogAge,
                Name = Constants.PETAPOCO_InsertDogName,
            };

            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                // Act
                dogRepository.Insert(dog);
				Logger.LogInformation("PetaPoco_Add_Inserts_Item(): save query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            }

            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount + 1));
			Logger.LogDebug("<<< Exiting PetaPoco_Add_Inserts_Item()");
        }

        [Test]
        public void PetaPoco_Add_Inserts_Item_Using_FluentMapper()
        {
			Logger.LogDebug(">>> Entering PetaPoco_Add_Inserts_Item_Using_FluentMapper()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var dog = new Dog
            {
                Age = Constants.PETAPOCO_InsertDogAge,
                Name = Constants.PETAPOCO_InsertDogName,
            };

            using (var dataContext = new PetaPocoDataContext(ConnectionStringName, string.Empty, this._mappers))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                // Act
                dogRepository.Insert(dog);
				Logger.LogInformation("PetaPoco_Add_Inserts_Item_Using_FluentMapper(): save query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            }

            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount + 1));
			Logger.LogDebug("<<< Exiting PetaPoco_Add_Inserts_Item_Using_FluentMapper()");
        }

        [Test]
        public void PetaPoco_Delete_Deletes_Item()
        {
			Logger.LogDebug(">>> Entering PetaPoco_Delete_Deletes_Item()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var dog = new Dog
            {
                ID = Constants.PETAPOCO_DeleteDogId,
                Age = Constants.PETAPOCO_DeleteDogAge,
                Name = Constants.PETAPOCO_DeleteDogName,
            };

            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                // Act
                dogRepository.Delete(dog);
				Logger.LogInformation("PetaPoco_Delete_Deletes_Item(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            }

            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount - 1));
			Logger.LogDebug("<<< Exiting PetaPoco_Delete_Deletes_Item()");
        }

        [Test]
        public void PetaPoco_Delete_Deletes_Item_Using_FluentMapper()
        {
			Logger.LogDebug(">>> Entering PetaPoco_Delete_Deletes_Item_Using_FluentMapper()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var dog = new Dog
            {
                ID = Constants.PETAPOCO_DeleteDogId,
                Age = Constants.PETAPOCO_DeleteDogAge,
                Name = Constants.PETAPOCO_DeleteDogName,
            };

            using (var dataContext = new PetaPocoDataContext(ConnectionStringName, string.Empty, this._mappers))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                // Act
                dogRepository.Delete(dog);
				Logger.LogInformation("PetaPoco_Delete_Deletes_Item_Using_FluentMapper(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            }

            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount - 1));
			Logger.LogDebug("<<< Exiting PetaPoco_Delete_Deletes_Item_Using_FluentMapper()");
        }

        [Test]
        public void PetaPoco_Delete_Overload_Deletes_Item()
        {
			Logger.LogDebug(">>> Entering PetaPoco_Delete_Overload_Deletes_Item()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                // Act
                dogRepository.Delete("WHERE ID = @0", Constants.PETAPOCO_DeleteDogId);
				Logger.LogInformation("PetaPoco_Delete_Overload_Deletes_Item(): delete query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            }

            // Assert
            int actualCount = DataUtil.GetRecordCount(
                Constants.PETAPOCO_DatabaseName,
                Constants.PETAPOCO_DogTableName);
            Assert.That(actualCount, Is.EqualTo(Constants.PETAPOCO_RecordCount - 1));
			Logger.LogDebug("<<< Exiting PetaPoco_Delete_Overload_Deletes_Item()");
        }

        [Test]
        [TestCase(1, "WHERE ID < @0", 2)]
        [TestCase(4, "WHERE Age <= @0", 5)]
        [TestCase(2, "WHERE Name LIKE @0", "B%")]
        public void PetaPoco_Find_Returns_Correct_Rows(int count, string sqlCondition, object arg)
        {
			Logger.LogDebug(">>> Entering PetaPoco_Find_Returns_Correct_Rows(count={Count},sqlCondition={SqlCondition},arg={Arg})", count, sqlCondition, arg);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            IEnumerable<Dog> dogs;
            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                // Act
                dogs = dogRepository.Find(sqlCondition, arg);
				Logger.LogInformation("PetaPoco_Find_Returns_Correct_Rows(count,arg)={Count},{Arg}: find query executed in {Arg2} ms", count, arg, __logStart.ElapsedMilliseconds);
            }

            // Assert
            Assert.That(dogs.Count(), Is.EqualTo(count));
			Logger.LogDebug("<<< Exiting PetaPoco_Find_Returns_Correct_Rows(count={Count},sqlCondition={SqlCondition},arg={Arg})", count, sqlCondition, arg);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        public void PetaPoco_Get_Returns_All_Items(int count)
        {
			Logger.LogDebug(">>> Entering PetaPoco_Get_Returns_All_Items(count={Count})", count);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            DataUtil.SetUpDatabase(count);

            IEnumerable<Dog> dogs;
            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                // Act
                dogs = dogRepository.Get();
				Logger.LogInformation("PetaPoco_Get_Returns_All_Items(count)={Count}: find query executed in {Arg1} ms", count, __logStart.ElapsedMilliseconds);
            }

            // Assert
            Assert.That(dogs.Count(), Is.EqualTo(count));
			Logger.LogDebug("<<< Exiting PetaPoco_Get_Returns_All_Items(count={Count})", count);
        }

        [Test]
        [TestCase(Constants.PAGE_First, Constants.PAGE_RecordCount)]
        [TestCase(Constants.PAGE_Second, Constants.PAGE_RecordCount)]
        [TestCase(Constants.PAGE_Last, Constants.PAGE_RecordCount)]
        public void PetaPoco_GetAll_Overload_Returns_Page_Of_Items(int pageIndex, int pageSize)
        {
			Logger.LogDebug(">>> Entering PetaPoco_GetAll_Overload_Returns_Page_Of_Items(pageIndex={PageIndex},pageSize={PageSize})", pageIndex, pageSize);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            DataUtil.SetUpDatabase(Constants.PAGE_TotalCount);

            IPagedList<Dog> dogs;
            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                // Act
                dogs = dogRepository.GetPage(pageIndex, pageSize);
				Logger.LogInformation("PetaPoco_GetAll_Overload_Returns_Page_Of_Items(pageIndex,pageSize)={PageIndex},{PageSize}: find query executed in {Arg2} ms", pageIndex, pageSize, __logStart.ElapsedMilliseconds);
            }

            // Assert
            Assert.That(dogs.PageSize, Is.EqualTo(pageSize));
			Logger.LogDebug("<<< Exiting PetaPoco_GetAll_Overload_Returns_Page_Of_Items(pageIndex={PageIndex},pageSize={PageSize})", pageIndex, pageSize);
        }

        [Test]
        public void PetaPoco_GetById_Returns_Single_Item()
        {
			Logger.LogDebug(">>> Entering PetaPoco_GetById_Returns_Single_Item()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            Dog dog;
            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                // Act
                dog = dogRepository.GetById(Constants.PETAPOCO_ValidDogId);
				Logger.LogInformation("PetaPoco_GetById_Returns_Single_Item(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            }

            // Assert
            Assert.That(dog, Is.InstanceOf<Dog>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(dog.Age, Is.EqualTo(Constants.PETAPOCO_ValidDogAge));
                Assert.That(dog.Name, Is.EqualTo(Constants.PETAPOCO_ValidDogName));
            }
			Logger.LogDebug("<<< Exiting PetaPoco_GetById_Returns_Single_Item()");
        }

        [Test]
        public void PetaPoco_GetById_Returns_Single_Item_Using_FluentMapper()
        {
			Logger.LogDebug(">>> Entering PetaPoco_GetById_Returns_Single_Item_Using_FluentMapper()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            Dog dog;
            using (var dataContext = new PetaPocoDataContext(ConnectionStringName, string.Empty, this._mappers))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                // Act
                dog = dogRepository.GetById(Constants.PETAPOCO_ValidDogId);
				Logger.LogInformation("PetaPoco_GetById_Returns_Single_Item_Using_FluentMapper(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            }

            // Assert
            Assert.That(dog, Is.InstanceOf<Dog>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(dog.Age, Is.EqualTo(Constants.PETAPOCO_ValidDogAge));
                Assert.That(dog.Name, Is.EqualTo(Constants.PETAPOCO_ValidDogName));
            }
			Logger.LogDebug("<<< Exiting PetaPoco_GetById_Returns_Single_Item_Using_FluentMapper()");
        }

        [Test]
        public void PetaPoco_Update_Updates_Item()
        {
			Logger.LogDebug(">>> Entering PetaPoco_Update_Updates_Item()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var dog = new Dog
            {
                ID = Constants.PETAPOCO_UpdateDogId,
                Age = Constants.PETAPOCO_UpdateDogAge,
                Name = Constants.PETAPOCO_UpdateDogName,
            };

            // Act
            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                // Act
                dogRepository.Update(dog);
				Logger.LogInformation("PetaPoco_Update_Updates_Item(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            }

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);

			Logger.LogDebug("PetaPoco_Update_Updates_Item(): table → {Table}", table);
            Assert.That(table.Rows, Has.Count.EqualTo(Constants.PETAPOCO_RecordCount));

            foreach (DataRow row in table.Rows)
            {
                if ((int)row["ID"] == Constants.PETAPOCO_UpdateDogId)
                {
                    using (Assert.EnterMultipleScope())
                    {
                        Assert.That(row["Age"], Is.EqualTo(Constants.PETAPOCO_UpdateDogAge));
                        Assert.That(row["Name"], Is.EqualTo(Constants.PETAPOCO_UpdateDogName));
                    }
                }
            }
			Logger.LogDebug("<<< Exiting PetaPoco_Update_Updates_Item()");
        }

        [Test]
        public void PetaPoco_Update_Updates_Item_Using_FluentMapper()
        {
			Logger.LogDebug(">>> Entering PetaPoco_Update_Updates_Item_Using_FluentMapper()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            var dog = new Dog
            {
                ID = Constants.PETAPOCO_UpdateDogId,
                Age = Constants.PETAPOCO_UpdateDogAge,
                Name = Constants.PETAPOCO_UpdateDogName,
            };

            // Act
            using (var dataContext = new PetaPocoDataContext(ConnectionStringName, string.Empty, this._mappers))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                // Act
                dogRepository.Update(dog);
				Logger.LogInformation("PetaPoco_Update_Updates_Item_Using_FluentMapper(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            }

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);

			Logger.LogDebug("PetaPoco_Update_Updates_Item_Using_FluentMapper(): table → {Table}", table);
            Assert.That(table.Rows, Has.Count.EqualTo(Constants.PETAPOCO_RecordCount));

            foreach (DataRow row in table.Rows)
            {
                if ((int)row["ID"] == Constants.PETAPOCO_UpdateDogId)
                {
                    using (Assert.EnterMultipleScope())
                    {
                        Assert.That(row["Age"], Is.EqualTo(Constants.PETAPOCO_UpdateDogAge));
                        Assert.That(row["Name"], Is.EqualTo(Constants.PETAPOCO_UpdateDogName));
                    }
                }
            }
			Logger.LogDebug("<<< Exiting PetaPoco_Update_Updates_Item_Using_FluentMapper()");
        }

        [Test]
        public void PetaPoco_Update_Overload_Updates_Item()
        {
			Logger.LogDebug(">>> Entering PetaPoco_Update_Overload_Updates_Item()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            DataUtil.SetUpDatabase(Constants.PETAPOCO_RecordCount);

            // Act
            using (var dataContext = new PetaPocoDataContext(ConnectionStringName))
            {
                IRepository<Dog> dogRepository = dataContext.GetRepository<Dog>();

                // Act
                dogRepository.Update("SET Age=@1, Name=@2 WHERE ID=@0", Constants.PETAPOCO_UpdateDogId, Constants.PETAPOCO_UpdateDogAge, Constants.PETAPOCO_UpdateDogName);
				Logger.LogInformation("PetaPoco_Update_Overload_Updates_Item(): update query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            }

            // Assert
            DataTable table = DataUtil.GetTable(Constants.PETAPOCO_DatabaseName, Constants.PETAPOCO_DogTableName);

			Logger.LogDebug("PetaPoco_Update_Overload_Updates_Item(): table → {Table}", table);
            Assert.That(table.Rows, Has.Count.EqualTo(Constants.PETAPOCO_RecordCount));

            foreach (DataRow row in table.Rows)
            {
                if ((int)row["ID"] == Constants.PETAPOCO_UpdateDogId)
                {
                    using (Assert.EnterMultipleScope())
                    {
                        Assert.That(row["Age"], Is.EqualTo(Constants.PETAPOCO_UpdateDogAge));
                        Assert.That(row["Name"], Is.EqualTo(Constants.PETAPOCO_UpdateDogName));
                    }
                }
            }
			Logger.LogDebug("<<< Exiting PetaPoco_Update_Overload_Updates_Item()");
        }

        // ReSharper restore InconsistentNaming
    }
}
