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
    public class PetaPocoMapperTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        // ReSharper disable InconsistentNaming
        [Test]
        public void PetaPocoMapper_Constructor_Initialises_TablePrefix_Property()
        {
			_logger.LogDebug(">>> Entering PetaPocoMapper_Constructor_Initialises_TablePrefix_Property()");
            // Arrange

            // Act
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);
			_logger.LogDebug("PetaPocoMapper_Constructor_Initialises_TablePrefix_Property(): mapper → {Mapper}", mapper);

            // Assert
            Assert.That(Util.GetPrivateMember<PetaPocoMapper, string>(mapper, "_tablePrefix"), Is.EqualTo(Constants.TABLENAME_Prefix));
			_logger.LogDebug("<<< Exiting PetaPocoMapper_Constructor_Initialises_TablePrefix_Property()");
        }

        [Test]
        public void PetaPocoMapper_GetTableInfo_Returns_TableInfo()
        {
			_logger.LogDebug(">>> Entering PetaPocoMapper_GetTableInfo_Returns_TableInfo()");
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);
			_logger.LogDebug("PetaPocoMapper_GetTableInfo_Returns_TableInfo(): mapper → {Mapper}", mapper);

            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));
			_logger.LogDebug("PetaPocoMapper_GetTableInfo_Returns_TableInfo(): ti → {Ti}", ti);

            // Assert
            Assert.That(ti, Is.InstanceOf<TableInfo>());
			_logger.LogDebug("<<< Exiting PetaPocoMapper_GetTableInfo_Returns_TableInfo()");
        }

        [Test]
        public void PetaPocoMapper_Maps_TableName_To_Plural_Of_ObjectName()
        {
			_logger.LogDebug(">>> Entering PetaPocoMapper_Maps_TableName_To_Plural_Of_ObjectName()");
            // Arrange
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoMapper_Maps_TableName_To_Plural_Of_ObjectName(): mapper → {Mapper}", mapper);

            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));
			_logger.LogDebug("PetaPocoMapper_Maps_TableName_To_Plural_Of_ObjectName(): ti → {Ti}", ti);

            // Assert
            Assert.That(ti.TableName, Is.EqualTo(Constants.TABLENAME_Dog));
			_logger.LogDebug("<<< Exiting PetaPocoMapper_Maps_TableName_To_Plural_Of_ObjectName()");
        }

        [Test]
        public void PetaPocoMapper_Adds_Prefix_To_Plural_Of_ObjectName()
        {
			_logger.LogDebug(">>> Entering PetaPocoMapper_Adds_Prefix_To_Plural_Of_ObjectName()");
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);
			_logger.LogDebug("PetaPocoMapper_Adds_Prefix_To_Plural_Of_ObjectName(): mapper → {Mapper}", mapper);

            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));
			_logger.LogDebug("PetaPocoMapper_Adds_Prefix_To_Plural_Of_ObjectName(): ti → {Ti}", ti);

            // Assert
            Assert.That(ti.TableName, Is.EqualTo(Constants.TABLENAME_Prefix + Constants.TABLENAME_Dog));
			_logger.LogDebug("<<< Exiting PetaPocoMapper_Adds_Prefix_To_Plural_Of_ObjectName()");
        }

        [Test]
        public void PetaPocoMapper_Maps_TableName_To_Attribute()
        {
			_logger.LogDebug(">>> Entering PetaPocoMapper_Maps_TableName_To_Attribute()");
            // Arrange
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoMapper_Maps_TableName_To_Attribute(): mapper → {Mapper}", mapper);

            // Act
            var ti = mapper.GetTableInfo(typeof(Person));
			_logger.LogDebug("PetaPocoMapper_Maps_TableName_To_Attribute(): ti → {Ti}", ti);

            // Assert
            Assert.That(ti.TableName, Is.EqualTo(Constants.TABLENAME_Person));
			_logger.LogDebug("<<< Exiting PetaPocoMapper_Maps_TableName_To_Attribute()");
        }

        [Test]
        public void PetaPocoMapper_Adds_Prefix_To_Attribute()
        {
			_logger.LogDebug(">>> Entering PetaPocoMapper_Adds_Prefix_To_Attribute()");
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);
			_logger.LogDebug("PetaPocoMapper_Adds_Prefix_To_Attribute(): mapper → {Mapper}", mapper);

            // Act
            var ti = mapper.GetTableInfo(typeof(Person));
			_logger.LogDebug("PetaPocoMapper_Adds_Prefix_To_Attribute(): ti → {Ti}", ti);

            // Assert
            Assert.That(ti.TableName, Is.EqualTo(Constants.TABLENAME_Prefix + Constants.TABLENAME_Person));
			_logger.LogDebug("<<< Exiting PetaPocoMapper_Adds_Prefix_To_Attribute()");
        }

        [Test]
        public void PetaPocoMapper_Sets_PrimaryKey_To_Attribute()
        {
			_logger.LogDebug(">>> Entering PetaPocoMapper_Sets_PrimaryKey_To_Attribute()");
            // Arrange
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoMapper_Sets_PrimaryKey_To_Attribute(): mapper → {Mapper}", mapper);

            // Act
            var ti = mapper.GetTableInfo(typeof(Person));
			_logger.LogDebug("PetaPocoMapper_Sets_PrimaryKey_To_Attribute(): ti → {Ti}", ti);

            // Assert
            Assert.That(ti.PrimaryKey, Is.EqualTo(Constants.TABLENAME_Person_Key));
			_logger.LogDebug("<<< Exiting PetaPocoMapper_Sets_PrimaryKey_To_Attribute()");
        }

        [Test]
        public void PetaPocoMapper_Sets_PrimaryKey_To_ID_If_No_Attribute()
        {
			_logger.LogDebug(">>> Entering PetaPocoMapper_Sets_PrimaryKey_To_ID_If_No_Attribute()");
            // Arrange
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoMapper_Sets_PrimaryKey_To_ID_If_No_Attribute(): mapper → {Mapper}", mapper);

            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));
			_logger.LogDebug("PetaPocoMapper_Sets_PrimaryKey_To_ID_If_No_Attribute(): ti → {Ti}", ti);

            // Assert
            Assert.That(ti.PrimaryKey, Is.EqualTo(Constants.TABLENAME_Key));
			_logger.LogDebug("<<< Exiting PetaPocoMapper_Sets_PrimaryKey_To_ID_If_No_Attribute()");
        }

        [Test]
        public void PetaPocoMapper_Sets_AutoIncrement_To_True()
        {
			_logger.LogDebug(">>> Entering PetaPocoMapper_Sets_AutoIncrement_To_True()");
            // Arrange
            var mapper = new PetaPocoMapper(string.Empty);
			_logger.LogDebug("PetaPocoMapper_Sets_AutoIncrement_To_True(): mapper → {Mapper}", mapper);

            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));
			_logger.LogDebug("PetaPocoMapper_Sets_AutoIncrement_To_True(): ti → {Ti}", ti);

            // Assert
            Assert.That(ti.AutoIncrement, Is.True);
			_logger.LogDebug("<<< Exiting PetaPocoMapper_Sets_AutoIncrement_To_True()");
        }

        [Test]
        public void PetaPocoMapper_GetColumnInfo_Returns_ColumnInfo()
        {
			_logger.LogDebug(">>> Entering PetaPocoMapper_GetColumnInfo_Returns_ColumnInfo()");
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);
			_logger.LogDebug("PetaPocoMapper_GetColumnInfo_Returns_ColumnInfo(): mapper → {Mapper}", mapper);
            var dogType = typeof(Dog);
			_logger.LogDebug("PetaPocoMapper_GetColumnInfo_Returns_ColumnInfo(): dogType → {DogType}", dogType);
            var dogProperty = dogType.GetProperty(Constants.COLUMNNAME_Name);
			_logger.LogDebug("PetaPocoMapper_GetColumnInfo_Returns_ColumnInfo(): dogProperty → {DogProperty}", dogProperty);

            // Act
            var ci = mapper.GetColumnInfo(dogProperty);
			_logger.LogDebug("PetaPocoMapper_GetColumnInfo_Returns_ColumnInfo(): ci → {Ci}", ci);

            // Assert
            Assert.That(ci, Is.InstanceOf<ColumnInfo>());
			_logger.LogDebug("<<< Exiting PetaPocoMapper_GetColumnInfo_Returns_ColumnInfo()");
        }

        [Test]
        public void PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_Attribute()
        {
			_logger.LogDebug(">>> Entering PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_Attribute()");
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);
			_logger.LogDebug("PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_Attribute(): mapper → {Mapper}", mapper);
            var personType = typeof(Person);
			_logger.LogDebug("PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_Attribute(): personType → {PersonType}", personType);
            var personProperty = personType.GetProperty(Constants.COLUMNNAME_Name);
			_logger.LogDebug("PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_Attribute(): personProperty → {PersonProperty}", personProperty);

            // Act
            var ci = mapper.GetColumnInfo(personProperty);
			_logger.LogDebug("PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_Attribute(): ci → {Ci}", ci);

            // Assert
            Assert.That(ci.ColumnName, Is.EqualTo(Constants.COLUMNNAME_PersonName));
			_logger.LogDebug("<<< Exiting PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_Attribute()");
        }

        [Test]
        public void PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_PropertyName_If_No_Attribute()
        {
			_logger.LogDebug(">>> Entering PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_PropertyName_If_No_Attribute()");
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);
			_logger.LogDebug("PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_PropertyName_If_No_Attribute(): mapper → {Mapper}", mapper);
            var dogType = typeof(Dog);
			_logger.LogDebug("PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_PropertyName_If_No_Attribute(): dogType → {DogType}", dogType);
            var dogProperty = dogType.GetProperty(Constants.COLUMNNAME_Name);
			_logger.LogDebug("PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_PropertyName_If_No_Attribute(): dogProperty → {DogProperty}", dogProperty);

            // Act
            var ci = mapper.GetColumnInfo(dogProperty);
			_logger.LogDebug("PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_PropertyName_If_No_Attribute(): ci → {Ci}", ci);

            // Assert
            Assert.That(ci.ColumnName, Is.EqualTo(Constants.COLUMNNAME_Name));
			_logger.LogDebug("<<< Exiting PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_PropertyName_If_No_Attribute()");
        }

        // ReSharper restore InconsistentNaming
    }
}
