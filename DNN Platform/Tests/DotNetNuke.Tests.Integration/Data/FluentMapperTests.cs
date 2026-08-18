// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Data
{
    using System;

    using DotNetNuke.Data.PetaPoco;
    using DotNetNuke.Tests.Data.Models;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;
    using PetaPoco;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class FluentMapperTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        [Test]
        public void FluentMapper_Constructor_Initialises_TablePrefix_Property()
        {
			_logger.LogDebug(">>> Entering FluentMapper_Constructor_Initialises_TablePrefix_Property()");
            // Arrange

            // Act
            var mapper = new FluentMapper<Dog>(Constants.TABLENAME_Prefix);
			_logger.LogDebug("FluentMapper_Constructor_Initialises_TablePrefix_Property(): mapper → {Mapper}", mapper);

            // Assert
            Assert.That(Util.GetPrivateMember<FluentMapper<Dog>, string>(mapper, "_tablePrefix"), Is.EqualTo(Constants.TABLENAME_Prefix));
			_logger.LogDebug("<<< Exiting FluentMapper_Constructor_Initialises_TablePrefix_Property()");
        }

        [Test]
        public void FluentMapper_GetTableInfo_Returns_TableInfo()
        {
			_logger.LogDebug(">>> Entering FluentMapper_GetTableInfo_Returns_TableInfo()");
            // Arrange
            var mapper = new FluentMapper<Dog>(Constants.TABLENAME_Prefix);
			_logger.LogDebug("FluentMapper_GetTableInfo_Returns_TableInfo(): mapper → {Mapper}", mapper);

            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));
			_logger.LogDebug("FluentMapper_GetTableInfo_Returns_TableInfo(): ti → {Ti}", ti);

            // Assert
            Assert.That(ti, Is.InstanceOf<TableInfo>());
			_logger.LogDebug("<<< Exiting FluentMapper_GetTableInfo_Returns_TableInfo()");
        }

        [Test]
        public void FluentMapper_Maps_TableName_To_Value_Provided()
        {
			_logger.LogDebug(">>> Entering FluentMapper_Maps_TableName_To_Value_Provided()");
            // Arrange
            var mapper = new FluentMapper<Dog>(string.Empty);
			_logger.LogDebug("FluentMapper_Maps_TableName_To_Value_Provided(): mapper → {Mapper}", mapper);
            mapper.TableName(Constants.TABLENAME_Dog);

            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));
			_logger.LogDebug("FluentMapper_Maps_TableName_To_Value_Provided(): ti → {Ti}", ti);

            // Assert
            Assert.That(ti.TableName, Is.EqualTo(Constants.TABLENAME_Dog));
			_logger.LogDebug("<<< Exiting FluentMapper_Maps_TableName_To_Value_Provided()");
        }

        [Test]
        public void FluentMapper_Adds_TablePrefix_To_Value_Provided()
        {
			_logger.LogDebug(">>> Entering FluentMapper_Adds_TablePrefix_To_Value_Provided()");
            // Arrange
            var mapper = new FluentMapper<Dog>(Constants.TABLENAME_Prefix);
			_logger.LogDebug("FluentMapper_Adds_TablePrefix_To_Value_Provided(): mapper → {Mapper}", mapper);
            mapper.TableName(Constants.TABLENAME_Dog);

            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));
			_logger.LogDebug("FluentMapper_Adds_TablePrefix_To_Value_Provided(): ti → {Ti}", ti);

            // Assert
            Assert.That(ti.TableName, Is.EqualTo(Constants.TABLENAME_Prefix + Constants.TABLENAME_Dog));
			_logger.LogDebug("<<< Exiting FluentMapper_Adds_TablePrefix_To_Value_Provided()");
        }

        [Test]
        public void FluentMapper_Does_Not_Map_TableName_To_Attribute()
        {
			_logger.LogDebug(">>> Entering FluentMapper_Does_Not_Map_TableName_To_Attribute()");
            // Arrange
            var mapper = new FluentMapper<Person>(string.Empty);
			_logger.LogDebug("FluentMapper_Does_Not_Map_TableName_To_Attribute(): mapper → {Mapper}", mapper);
            mapper.TableName(Constants.TABLENAME_Dog);

            // Act
            var ti = mapper.GetTableInfo(typeof(Person));
			_logger.LogDebug("FluentMapper_Does_Not_Map_TableName_To_Attribute(): ti → {Ti}", ti);

            // Assert
            Assert.That(ti.TableName, Is.Not.EqualTo(Constants.TABLENAME_Person));
            Assert.That(ti.TableName, Is.EqualTo(Constants.TABLENAME_Dog));
			_logger.LogDebug("<<< Exiting FluentMapper_Does_Not_Map_TableName_To_Attribute()");
        }

        [Test]
        public void FluentMapper_Sets_TableName_To_Null_If_Not_Provided()
        {
			_logger.LogDebug(">>> Entering FluentMapper_Sets_TableName_To_Null_If_Not_Provided()");
            // Arrange
            var mapper = new FluentMapper<Person>(string.Empty);
			_logger.LogDebug("FluentMapper_Sets_TableName_To_Null_If_Not_Provided(): mapper → {Mapper}", mapper);

            // Act
            var ti = mapper.GetTableInfo(typeof(Person));
			_logger.LogDebug("FluentMapper_Sets_TableName_To_Null_If_Not_Provided(): ti → {Ti}", ti);

            // Assert
            Assert.That(ti.TableName, Is.Null);
			_logger.LogDebug("<<< Exiting FluentMapper_Sets_TableName_To_Null_If_Not_Provided()");
        }

        [Test]
        public void FluentMapper_Sets_PrimaryKey_To_Provided_Value()
        {
			_logger.LogDebug(">>> Entering FluentMapper_Sets_PrimaryKey_To_Provided_Value()");
            // Arrange
            var mapper = new FluentMapper<Person>(string.Empty);
			_logger.LogDebug("FluentMapper_Sets_PrimaryKey_To_Provided_Value(): mapper → {Mapper}", mapper);
            mapper.PrimaryKey(Constants.TABLENAME_Key);

            // Act
            var ti = mapper.GetTableInfo(typeof(Person));
			_logger.LogDebug("FluentMapper_Sets_PrimaryKey_To_Provided_Value(): ti → {Ti}", ti);

            // Assert
            Assert.That(ti.PrimaryKey, Is.EqualTo(Constants.TABLENAME_Key));
			_logger.LogDebug("<<< Exiting FluentMapper_Sets_PrimaryKey_To_Provided_Value()");
        }

        [Test]
        public void FluentMapper_Does_Not_Set_PrimaryKey_To_Attribute()
        {
			_logger.LogDebug(">>> Entering FluentMapper_Does_Not_Set_PrimaryKey_To_Attribute()");
            // Arrange
            var mapper = new FluentMapper<Person>(string.Empty);
			_logger.LogDebug("FluentMapper_Does_Not_Set_PrimaryKey_To_Attribute(): mapper → {Mapper}", mapper);
            mapper.PrimaryKey(Constants.TABLENAME_Key);

            // Act
            var ti = mapper.GetTableInfo(typeof(Person));
			_logger.LogDebug("FluentMapper_Does_Not_Set_PrimaryKey_To_Attribute(): ti → {Ti}", ti);

            // Assert
            Assert.That(ti.PrimaryKey, Is.Not.EqualTo(Constants.TABLENAME_Person_Key));
			_logger.LogDebug("<<< Exiting FluentMapper_Does_Not_Set_PrimaryKey_To_Attribute()");
        }

        [Test]
        public void FluentMapper_Sets_PrimaryKey_To_Null_If_Not_Provided()
        {
			_logger.LogDebug(">>> Entering FluentMapper_Sets_PrimaryKey_To_Null_If_Not_Provided()");
            // Arrange
            var mapper = new FluentMapper<Person>(string.Empty);
			_logger.LogDebug("FluentMapper_Sets_PrimaryKey_To_Null_If_Not_Provided(): mapper → {Mapper}", mapper);

            // Act
            var ti = mapper.GetTableInfo(typeof(Person));
			_logger.LogDebug("FluentMapper_Sets_PrimaryKey_To_Null_If_Not_Provided(): ti → {Ti}", ti);

            // Assert
            Assert.That(ti.PrimaryKey, Is.Null);
			_logger.LogDebug("<<< Exiting FluentMapper_Sets_PrimaryKey_To_Null_If_Not_Provided()");
        }

        [Test]
        public void FluentMapper_Sets_AutoIncrement_To_False()
        {
			_logger.LogDebug(">>> Entering FluentMapper_Sets_AutoIncrement_To_False()");
            // Arrange
            var mapper = new FluentMapper<Person>(string.Empty);
			_logger.LogDebug("FluentMapper_Sets_AutoIncrement_To_False(): mapper → {Mapper}", mapper);

            // Act
            var ti = mapper.GetTableInfo(typeof(Person));
			_logger.LogDebug("FluentMapper_Sets_AutoIncrement_To_False(): ti → {Ti}", ti);

            // Assert
            Assert.That(ti.AutoIncrement, Is.False);
			_logger.LogDebug("<<< Exiting FluentMapper_Sets_AutoIncrement_To_False()");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void FluentMapper_Sets_AutoIncrement_To_Value_Provided(bool autoIncrement)
        {
			_logger.LogDebug(">>> Entering FluentMapper_Sets_AutoIncrement_To_Value_Provided(autoIncrement={AutoIncrement})", autoIncrement);
            // Arrange
            var mapper = new FluentMapper<Person>(string.Empty);
			_logger.LogDebug("FluentMapper_Sets_AutoIncrement_To_Value_Provided(autoIncrement={AutoIncrement}): mapper → {Mapper}", autoIncrement, mapper);
            mapper.PrimaryKey("ID", autoIncrement);

            // Act
            var ti = mapper.GetTableInfo(typeof(Person));
			_logger.LogDebug("FluentMapper_Sets_AutoIncrement_To_Value_Provided(autoIncrement={AutoIncrement}): ti → {Ti}", autoIncrement, ti);

            // Assert
            Assert.That(ti.AutoIncrement, Is.EqualTo(autoIncrement));
			_logger.LogDebug("<<< Exiting FluentMapper_Sets_AutoIncrement_To_Value_Provided(autoIncrement={AutoIncrement})", autoIncrement);
        }

        [Test]
        public void FluentMapper_GetColumnInfo_Returns_Null_If_Not_Provided()
        {
			_logger.LogDebug(">>> Entering FluentMapper_GetColumnInfo_Returns_Null_If_Not_Provided()");
            // Arrange
            var mapper = new FluentMapper<Dog>(Constants.TABLENAME_Prefix);
			_logger.LogDebug("FluentMapper_GetColumnInfo_Returns_Null_If_Not_Provided(): mapper → {Mapper}", mapper);

            var dogType = typeof(Dog);
			_logger.LogDebug("FluentMapper_GetColumnInfo_Returns_Null_If_Not_Provided(): dogType → {DogType}", dogType);
            var dogProperty = dogType.GetProperty(Constants.COLUMNNAME_Name);
			_logger.LogDebug("FluentMapper_GetColumnInfo_Returns_Null_If_Not_Provided(): dogProperty → {DogProperty}", dogProperty);

            // Act
            var ci = mapper.GetColumnInfo(dogProperty);
			_logger.LogDebug("FluentMapper_GetColumnInfo_Returns_Null_If_Not_Provided(): ci → {Ci}", ci);

            // Assert
            Assert.That(ci, Is.Null);
			_logger.LogDebug("<<< Exiting FluentMapper_GetColumnInfo_Returns_Null_If_Not_Provided()");
        }

        [Test]
        public void FluentMapper_GetColumnInfo_Returns_ColumnInfo()
        {
			_logger.LogDebug(">>> Entering FluentMapper_GetColumnInfo_Returns_ColumnInfo()");
            // Arrange
            var mapper = new FluentMapper<Dog>(Constants.TABLENAME_Prefix);
			_logger.LogDebug("FluentMapper_GetColumnInfo_Returns_ColumnInfo(): mapper → {Mapper}", mapper);
            mapper.Property(d => d.Name, Constants.COLUMNNAME_Name);

            var dogType = typeof(Dog);
			_logger.LogDebug("FluentMapper_GetColumnInfo_Returns_ColumnInfo(): dogType → {DogType}", dogType);
            var dogProperty = dogType.GetProperty(Constants.COLUMNNAME_Name);
			_logger.LogDebug("FluentMapper_GetColumnInfo_Returns_ColumnInfo(): dogProperty → {DogProperty}", dogProperty);

            // Act
            var ci = mapper.GetColumnInfo(dogProperty);
			_logger.LogDebug("FluentMapper_GetColumnInfo_Returns_ColumnInfo(): ci → {Ci}", ci);

            // Assert
            Assert.That(ci, Is.InstanceOf<ColumnInfo>());
			_logger.LogDebug("<<< Exiting FluentMapper_GetColumnInfo_Returns_ColumnInfo()");
        }

        [Test]
        public void FluentMapper_GetColumnInfo_Maps_ColumnName_To_Value_Provided()
        {
			_logger.LogDebug(">>> Entering FluentMapper_GetColumnInfo_Maps_ColumnName_To_Value_Provided()");
            // Arrange
            var mapper = new FluentMapper<Dog>(Constants.TABLENAME_Prefix);
			_logger.LogDebug("FluentMapper_GetColumnInfo_Maps_ColumnName_To_Value_Provided(): mapper → {Mapper}", mapper);
            mapper.Property(d => d.Name, Constants.COLUMNNAME_Name);

            var personType = typeof(Dog);
			_logger.LogDebug("FluentMapper_GetColumnInfo_Maps_ColumnName_To_Value_Provided(): personType → {PersonType}", personType);
            var personProperty = personType.GetProperty(Constants.COLUMNNAME_Name);
			_logger.LogDebug("FluentMapper_GetColumnInfo_Maps_ColumnName_To_Value_Provided(): personProperty → {PersonProperty}", personProperty);

            // Act
            var ci = mapper.GetColumnInfo(personProperty);
			_logger.LogDebug("FluentMapper_GetColumnInfo_Maps_ColumnName_To_Value_Provided(): ci → {Ci}", ci);

            // Assert
            Assert.That(ci.ColumnName, Is.EqualTo(Constants.COLUMNNAME_Name));
			_logger.LogDebug("<<< Exiting FluentMapper_GetColumnInfo_Maps_ColumnName_To_Value_Provided()");
        }

        [Test]
        public void FluentMapper_GetColumnInfo_Does_Not_Map_ColumnName_To_Attribute()
        {
			_logger.LogDebug(">>> Entering FluentMapper_GetColumnInfo_Does_Not_Map_ColumnName_To_Attribute()");
            // Arrange
            var mapper = new FluentMapper<Person>(Constants.TABLENAME_Prefix);
			_logger.LogDebug("FluentMapper_GetColumnInfo_Does_Not_Map_ColumnName_To_Attribute(): mapper → {Mapper}", mapper);
            mapper.Property(d => d.Name, Constants.COLUMNNAME_Name);

            var personType = typeof(Person);
			_logger.LogDebug("FluentMapper_GetColumnInfo_Does_Not_Map_ColumnName_To_Attribute(): personType → {PersonType}", personType);
            var personProperty = personType.GetProperty(Constants.COLUMNNAME_Name);
			_logger.LogDebug("FluentMapper_GetColumnInfo_Does_Not_Map_ColumnName_To_Attribute(): personProperty → {PersonProperty}", personProperty);

            // Act
            var ci = mapper.GetColumnInfo(personProperty);
			_logger.LogDebug("FluentMapper_GetColumnInfo_Does_Not_Map_ColumnName_To_Attribute(): ci → {Ci}", ci);

            // Assert
            Assert.That(ci.ColumnName, Is.Not.EqualTo(Constants.COLUMNNAME_PersonName));
			_logger.LogDebug("<<< Exiting FluentMapper_GetColumnInfo_Does_Not_Map_ColumnName_To_Attribute()");
        }
    }
}
