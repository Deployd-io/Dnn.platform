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

    [TestFixture]
    public class PetaPocoMapperTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        // ReSharper disable InconsistentNaming
        [Test]
        public void PetaPocoMapper_Constructor_Initialises_TablePrefix_Property()
        {
			Logger.LogDebug(">>> Entering PetaPocoMapper_Constructor_Initialises_TablePrefix_Property()");
            // Arrange

            // Act
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);

			Logger.LogDebug("PetaPocoMapper_Constructor_Initialises_TablePrefix_Property(): mapper → {Mapper}", mapper);
            // Assert
            Assert.That(Util.GetPrivateMember<PetaPocoMapper, string>(mapper, "_tablePrefix"), Is.EqualTo(Constants.TABLENAME_Prefix));
			Logger.LogDebug("<<< Exiting PetaPocoMapper_Constructor_Initialises_TablePrefix_Property()");
        }

        [Test]
        public void PetaPocoMapper_GetTableInfo_Returns_TableInfo()
        {
			Logger.LogDebug(">>> Entering PetaPocoMapper_GetTableInfo_Returns_TableInfo()");
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);

			Logger.LogDebug("PetaPocoMapper_GetTableInfo_Returns_TableInfo(): mapper → {Mapper}", mapper);
            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));

			Logger.LogDebug("PetaPocoMapper_GetTableInfo_Returns_TableInfo(): ti → {Ti}", ti);
            // Assert
            Assert.That(ti, Is.InstanceOf<TableInfo>());
			Logger.LogDebug("<<< Exiting PetaPocoMapper_GetTableInfo_Returns_TableInfo()");
        }

        [Test]
        public void PetaPocoMapper_Maps_TableName_To_Plural_Of_ObjectName()
        {
			Logger.LogDebug(">>> Entering PetaPocoMapper_Maps_TableName_To_Plural_Of_ObjectName()");
            // Arrange
            var mapper = new PetaPocoMapper(string.Empty);

			Logger.LogDebug("PetaPocoMapper_Maps_TableName_To_Plural_Of_ObjectName(): mapper → {Mapper}", mapper);
            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));

			Logger.LogDebug("PetaPocoMapper_Maps_TableName_To_Plural_Of_ObjectName(): ti → {Ti}", ti);
            // Assert
            Assert.That(ti.TableName, Is.EqualTo(Constants.TABLENAME_Dog));
			Logger.LogDebug("<<< Exiting PetaPocoMapper_Maps_TableName_To_Plural_Of_ObjectName()");
        }

        [Test]
        public void PetaPocoMapper_Adds_Prefix_To_Plural_Of_ObjectName()
        {
			Logger.LogDebug(">>> Entering PetaPocoMapper_Adds_Prefix_To_Plural_Of_ObjectName()");
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);

			Logger.LogDebug("PetaPocoMapper_Adds_Prefix_To_Plural_Of_ObjectName(): mapper → {Mapper}", mapper);
            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));

			Logger.LogDebug("PetaPocoMapper_Adds_Prefix_To_Plural_Of_ObjectName(): ti → {Ti}", ti);
            // Assert
            Assert.That(ti.TableName, Is.EqualTo(Constants.TABLENAME_Prefix + Constants.TABLENAME_Dog));
			Logger.LogDebug("<<< Exiting PetaPocoMapper_Adds_Prefix_To_Plural_Of_ObjectName()");
        }

        [Test]
        public void PetaPocoMapper_Maps_TableName_To_Attribute()
        {
			Logger.LogDebug(">>> Entering PetaPocoMapper_Maps_TableName_To_Attribute()");
            // Arrange
            var mapper = new PetaPocoMapper(string.Empty);

			Logger.LogDebug("PetaPocoMapper_Maps_TableName_To_Attribute(): mapper → {Mapper}", mapper);
            // Act
            var ti = mapper.GetTableInfo(typeof(Person));

			Logger.LogDebug("PetaPocoMapper_Maps_TableName_To_Attribute(): ti → {Ti}", ti);
            // Assert
            Assert.That(ti.TableName, Is.EqualTo(Constants.TABLENAME_Person));
			Logger.LogDebug("<<< Exiting PetaPocoMapper_Maps_TableName_To_Attribute()");
        }

        [Test]
        public void PetaPocoMapper_Adds_Prefix_To_Attribute()
        {
			Logger.LogDebug(">>> Entering PetaPocoMapper_Adds_Prefix_To_Attribute()");
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);

			Logger.LogDebug("PetaPocoMapper_Adds_Prefix_To_Attribute(): mapper → {Mapper}", mapper);
            // Act
            var ti = mapper.GetTableInfo(typeof(Person));

			Logger.LogDebug("PetaPocoMapper_Adds_Prefix_To_Attribute(): ti → {Ti}", ti);
            // Assert
            Assert.That(ti.TableName, Is.EqualTo(Constants.TABLENAME_Prefix + Constants.TABLENAME_Person));
			Logger.LogDebug("<<< Exiting PetaPocoMapper_Adds_Prefix_To_Attribute()");
        }

        [Test]
        public void PetaPocoMapper_Sets_PrimaryKey_To_Attribute()
        {
			Logger.LogDebug(">>> Entering PetaPocoMapper_Sets_PrimaryKey_To_Attribute()");
            // Arrange
            var mapper = new PetaPocoMapper(string.Empty);

			Logger.LogDebug("PetaPocoMapper_Sets_PrimaryKey_To_Attribute(): mapper → {Mapper}", mapper);
            // Act
            var ti = mapper.GetTableInfo(typeof(Person));

			Logger.LogDebug("PetaPocoMapper_Sets_PrimaryKey_To_Attribute(): ti → {Ti}", ti);
            // Assert
            Assert.That(ti.PrimaryKey, Is.EqualTo(Constants.TABLENAME_Person_Key));
			Logger.LogDebug("<<< Exiting PetaPocoMapper_Sets_PrimaryKey_To_Attribute()");
        }

        [Test]
        public void PetaPocoMapper_Sets_PrimaryKey_To_ID_If_No_Attribute()
        {
			Logger.LogDebug(">>> Entering PetaPocoMapper_Sets_PrimaryKey_To_ID_If_No_Attribute()");
            // Arrange
            var mapper = new PetaPocoMapper(string.Empty);

			Logger.LogDebug("PetaPocoMapper_Sets_PrimaryKey_To_ID_If_No_Attribute(): mapper → {Mapper}", mapper);
            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));

			Logger.LogDebug("PetaPocoMapper_Sets_PrimaryKey_To_ID_If_No_Attribute(): ti → {Ti}", ti);
            // Assert
            Assert.That(ti.PrimaryKey, Is.EqualTo(Constants.TABLENAME_Key));
			Logger.LogDebug("<<< Exiting PetaPocoMapper_Sets_PrimaryKey_To_ID_If_No_Attribute()");
        }

        [Test]
        public void PetaPocoMapper_Sets_AutoIncrement_To_True()
        {
			Logger.LogDebug(">>> Entering PetaPocoMapper_Sets_AutoIncrement_To_True()");
            // Arrange
            var mapper = new PetaPocoMapper(string.Empty);

			Logger.LogDebug("PetaPocoMapper_Sets_AutoIncrement_To_True(): mapper → {Mapper}", mapper);
            // Act
            var ti = mapper.GetTableInfo(typeof(Dog));

			Logger.LogDebug("PetaPocoMapper_Sets_AutoIncrement_To_True(): ti → {Ti}", ti);
            // Assert
            Assert.That(ti.AutoIncrement, Is.True);
			Logger.LogDebug("<<< Exiting PetaPocoMapper_Sets_AutoIncrement_To_True()");
        }

        [Test]
        public void PetaPocoMapper_GetColumnInfo_Returns_ColumnInfo()
        {
			Logger.LogDebug(">>> Entering PetaPocoMapper_GetColumnInfo_Returns_ColumnInfo()");
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);
            var dogType = typeof(Dog);
			Logger.LogDebug("PetaPocoMapper_GetColumnInfo_Returns_ColumnInfo(): mapper → {Mapper}", mapper);
			Logger.LogDebug("PetaPocoMapper_GetColumnInfo_Returns_ColumnInfo(): dogType → {DogType}", dogType);
            var dogProperty = dogType.GetProperty(Constants.COLUMNNAME_Name);

			Logger.LogDebug("PetaPocoMapper_GetColumnInfo_Returns_ColumnInfo(): dogProperty → {DogProperty}", dogProperty);
            // Act
            var ci = mapper.GetColumnInfo(dogProperty);

			Logger.LogDebug("PetaPocoMapper_GetColumnInfo_Returns_ColumnInfo(): ci → {Ci}", ci);
            // Assert
            Assert.That(ci, Is.InstanceOf<ColumnInfo>());
			Logger.LogDebug("<<< Exiting PetaPocoMapper_GetColumnInfo_Returns_ColumnInfo()");
        }

        [Test]
        public void PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_Attribute()
        {
			Logger.LogDebug(">>> Entering PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_Attribute()");
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);
            var personType = typeof(Person);
			Logger.LogDebug("PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_Attribute(): mapper → {Mapper}", mapper);
			Logger.LogDebug("PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_Attribute(): personType → {PersonType}", personType);
            var personProperty = personType.GetProperty(Constants.COLUMNNAME_Name);

			Logger.LogDebug("PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_Attribute(): personProperty → {PersonProperty}", personProperty);
            // Act
            var ci = mapper.GetColumnInfo(personProperty);

			Logger.LogDebug("PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_Attribute(): ci → {Ci}", ci);
            // Assert
            Assert.That(ci.ColumnName, Is.EqualTo(Constants.COLUMNNAME_PersonName));
			Logger.LogDebug("<<< Exiting PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_Attribute()");
        }

        [Test]
        public void PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_PropertyName_If_No_Attribute()
        {
			Logger.LogDebug(">>> Entering PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_PropertyName_If_No_Attribute()");
            // Arrange
            var mapper = new PetaPocoMapper(Constants.TABLENAME_Prefix);
            var dogType = typeof(Dog);
			Logger.LogDebug("PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_PropertyName_If_No_Attribute(): mapper → {Mapper}", mapper);
			Logger.LogDebug("PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_PropertyName_If_No_Attribute(): dogType → {DogType}", dogType);
            var dogProperty = dogType.GetProperty(Constants.COLUMNNAME_Name);

			Logger.LogDebug("PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_PropertyName_If_No_Attribute(): dogProperty → {DogProperty}", dogProperty);
            // Act
            var ci = mapper.GetColumnInfo(dogProperty);

			Logger.LogDebug("PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_PropertyName_If_No_Attribute(): ci → {Ci}", ci);
            // Assert
            Assert.That(ci.ColumnName, Is.EqualTo(Constants.COLUMNNAME_Name));
			Logger.LogDebug("<<< Exiting PetaPocoMapper_GetColumnInfo_Maps_ColumnName_To_PropertyName_If_No_Attribute()");
        }

        // ReSharper restore InconsistentNaming
    }
}
