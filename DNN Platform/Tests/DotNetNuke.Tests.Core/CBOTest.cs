// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Caching
{
    using System;
    using System.Data;
    using System.Text;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Tests.Utilities.Mocks;
    using NUnit.Framework;

    /// <summary>  Summary description for DataCacheTests.</summary>
    [TestFixture]
    public class CBOTest
    {
        [SetUp]
        public void SetUp()
        {
			mockEventLogger.LogDebug(">>> Entering SetUp()");
            // Create a Container
            ComponentFactory.Container = new SimpleContainer();
            MockComponentProvider.CreateDataCacheProvider();
			mockEventLogger.LogDebug("<<< Exiting SetUp()");
        }

        [Test]
        public void CBO_FillObject_int()
        {
			mockEventLogger.LogDebug(">>> Entering CBO_FillObject_int()");
            var cboTable = new DataTable("CBOTable");
            var colValue = 12;
			mockEventLogger.LogDebug("CBO_FillObject_int(): cboTable → {CboTable}", cboTable);
            cboTable.Columns.Add("IntProp", typeof(int));
            cboTable.Rows.Add(colValue);

            // Assert.AreEqual(12, moq.Object["TestColumn"]);
            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());
			mockEventLogger.LogDebug("CBO_FillObject_int(): result → {Result}", result);

            Assert.That(result, Is.InstanceOf<IntPoco>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IntProp, Is.EqualTo(colValue));
			mockEventLogger.LogDebug("<<< Exiting CBO_FillObject_int()");
        }

        [Test]
        public void CBO_FillObject_string()
        {
			mockEventLogger.LogDebug(">>> Entering CBO_FillObject_string()");
            var cboTable = new DataTable("CBOTable");
            var colValue = Guid.NewGuid().ToString();
			mockEventLogger.LogDebug("CBO_FillObject_string(): colValue → {ColValue}", colValue);
			mockEventLogger.LogDebug("CBO_FillObject_string(): cboTable → {CboTable}", cboTable);
            cboTable.Columns.Add("StringProp", typeof(string));
            cboTable.Rows.Add(colValue);

            // Assert.AreEqual(12, moq.Object["TestColumn"]);
            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());
			mockEventLogger.LogDebug("CBO_FillObject_string(): result → {Result}", result);

            Assert.That(result, Is.InstanceOf<IntPoco>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StringProp, Is.EqualTo(colValue));
			mockEventLogger.LogDebug("<<< Exiting CBO_FillObject_string()");
        }

        [Test]
        public void CBO_FillObject_datetime()
        {
			mockEventLogger.LogDebug(">>> Entering CBO_FillObject_datetime()");
            var cboTable = new DataTable("CBOTable");
            var colValue = new DateTime(2010, 12, 11, 10, 9, 8);
			mockEventLogger.LogDebug("CBO_FillObject_datetime(): colValue → {ColValue}", colValue);
			mockEventLogger.LogDebug("CBO_FillObject_datetime(): cboTable → {CboTable}", cboTable);

            cboTable.Columns.Add("DateTimeProp", typeof(DateTime));
            cboTable.Rows.Add(colValue);

            // Assert.AreEqual(12, moq.Object["TestColumn"]);
            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());
			mockEventLogger.LogDebug("CBO_FillObject_datetime(): result → {Result}", result);

            Assert.That(result, Is.InstanceOf<IntPoco>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.DateTimeProp, Is.EqualTo(colValue));
			mockEventLogger.LogDebug("<<< Exiting CBO_FillObject_datetime()");
        }

        [Test]

        // DNNPRO-13404 - Object does not implement IConvertible
        public void CBO_FillObject_binary()
        {
			mockEventLogger.LogDebug(">>> Entering CBO_FillObject_binary()");
            var cboTable = new DataTable("CBOTable");
            var colValue = Encoding.ASCII.GetBytes("Hello This is test");
			mockEventLogger.LogDebug("CBO_FillObject_binary(): colValue → {ColValue}", colValue);
			mockEventLogger.LogDebug("CBO_FillObject_binary(): cboTable → {CboTable}", cboTable);

            cboTable.Columns.Add("ByteArrayProp", typeof(byte[]));
            cboTable.Rows.Add(colValue);

            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());
			mockEventLogger.LogDebug("CBO_FillObject_binary(): result → {Result}", result);

            Assert.That(result, Is.InstanceOf<IntPoco>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ByteArrayProp, Is.EqualTo(colValue));
			mockEventLogger.LogDebug("<<< Exiting CBO_FillObject_binary()");
        }

        [Test]
        public void CBO_FillObject_binary_to_Array()
        {
			mockEventLogger.LogDebug(">>> Entering CBO_FillObject_binary_to_Array()");
            var cboTable = new DataTable("CBOTable");
            var colValue = Encoding.ASCII.GetBytes("Hello This is test");
			mockEventLogger.LogDebug("CBO_FillObject_binary_to_Array(): colValue → {ColValue}", colValue);
			mockEventLogger.LogDebug("CBO_FillObject_binary_to_Array(): cboTable → {CboTable}", cboTable);

            cboTable.Columns.Add("ArrayProp", typeof(byte[]));
            cboTable.Rows.Add(colValue);

            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());
			mockEventLogger.LogDebug("CBO_FillObject_binary_to_Array(): result → {Result}", result);

            Assert.That(result, Is.InstanceOf<IntPoco>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ArrayProp, Is.EqualTo(colValue));
			mockEventLogger.LogDebug("<<< Exiting CBO_FillObject_binary_to_Array()");
        }

        [Test]
        public void CBO_FillObject_bit()
        {
			mockEventLogger.LogDebug(">>> Entering CBO_FillObject_bit()");
            var cboTable = new DataTable("CBOTable");
            var colValue = true;
			mockEventLogger.LogDebug("CBO_FillObject_bit(): cboTable → {CboTable}", cboTable);

            cboTable.Columns.Add("BitProp", typeof(bool));
            cboTable.Rows.Add(colValue);

            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());
			mockEventLogger.LogDebug("CBO_FillObject_bit(): result → {Result}", result);

            Assert.That(result, Is.InstanceOf<IntPoco>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.BitProp, Is.EqualTo(colValue));
			mockEventLogger.LogDebug("<<< Exiting CBO_FillObject_bit()");
        }

        [Test]
        public void CBO_FillObject_decimal()
        {
			mockEventLogger.LogDebug(">>> Entering CBO_FillObject_decimal()");
            var cboTable = new DataTable("CBOTable");
            decimal colValue = 12.99m;
			mockEventLogger.LogDebug("CBO_FillObject_decimal(): cboTable → {CboTable}", cboTable);

            cboTable.Columns.Add("DecimalProp", typeof(decimal));
            cboTable.Rows.Add(colValue);

            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());
			mockEventLogger.LogDebug("CBO_FillObject_decimal(): result → {Result}", result);

            Assert.That(result, Is.InstanceOf<IntPoco>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.DecimalProp, Is.EqualTo(colValue));
			mockEventLogger.LogDebug("<<< Exiting CBO_FillObject_decimal()");
        }

        [Test]
        public void CBO_FillObject_int_to_boolean_true()
        {
			mockEventLogger.LogDebug(">>> Entering CBO_FillObject_int_to_boolean_true()");
            var cboTable = new DataTable("CBOTable");
            decimal colValue = 1;
			mockEventLogger.LogDebug("CBO_FillObject_int_to_boolean_true(): cboTable → {CboTable}", cboTable);

            cboTable.Columns.Add("BitProp", typeof(int));
            cboTable.Rows.Add(colValue);

            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());
			mockEventLogger.LogDebug("CBO_FillObject_int_to_boolean_true(): result → {Result}", result);

            Assert.That(result, Is.InstanceOf<IntPoco>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.BitProp, Is.EqualTo(true));
			mockEventLogger.LogDebug("<<< Exiting CBO_FillObject_int_to_boolean_true()");
        }

        [Test]
        public void CBO_FillObject_int_to_boolean_false()
        {
			mockEventLogger.LogDebug(">>> Entering CBO_FillObject_int_to_boolean_false()");
            var cboTable = new DataTable("CBOTable");
            decimal colValue = 0;
			mockEventLogger.LogDebug("CBO_FillObject_int_to_boolean_false(): cboTable → {CboTable}", cboTable);

            cboTable.Columns.Add("BitProp", typeof(int));
            cboTable.Rows.Add(colValue);

            var result = CBO.FillObject<IntPoco>(cboTable.CreateDataReader());
			mockEventLogger.LogDebug("CBO_FillObject_int_to_boolean_false(): result → {Result}", result);

            Assert.That(result, Is.InstanceOf<IntPoco>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.BitProp, Is.EqualTo(false));
			mockEventLogger.LogDebug("<<< Exiting CBO_FillObject_int_to_boolean_false()");
        }
    }

    public class IntPoco
    {
        public int IntProp { get; set; }

        public string StringProp { get; set; }

        public DateTime DateTimeProp { get; set; }

        public byte[] ByteArrayProp { get; set; }

        public Array ArrayProp { get; set; }

        public bool BitProp { get; set; }

        public decimal DecimalProp { get; set; }
    }
}
