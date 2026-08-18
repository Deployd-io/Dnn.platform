namespace DotNetNuke.Tests.Data
{
    using System.Reflection;

    using DotNetNuke.Data.PetaPoco;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class PetaPocoExtTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        [TestCase(null, null, null)]
        [TestCase(null, new object[0], null)]
        [TestCase(null, new object[] { "" }, null)]
        [TestCase("", null, "")]
        [TestCase("", new object[0], "")]
        [TestCase("", new object[] { "" }, "")]
        [TestCase(
            "declare @test varchar(20) = 'test@test.com'\nselect @test",
            null,
            "declare @@test varchar(20) = 'test@@test.com'\nselect @@test")]
        [TestCase(
            "declare @test varchar(20) = 'test@test.com'\nselect @test",
            new object[0],
            "declare @@test varchar(20) = 'test@@test.com'\nselect @@test")]
        [TestCase(
            "declare @test varchar(20) = 'test@test.com'\nselect @test",
            new object[] { "" },
            "declare @test varchar(20) = 'test@test.com'\nselect @test")]
        public void NormalizeSql_WithAtCharAndEmptyArguments_EscapesAtCharSuccessfully(
            string sql,
            object[] args,
            string expected)
        {
			_logger.LogDebug(">>> Entering NormalizeSql_WithAtCharAndEmptyArguments_EscapesAtCharSuccessfully(sql={Sql},args={Args},expected={Expected})", sql, args, expected);
            // Arrange
            const string NormalizeSqlMethodName = "NormalizeSql";
            var normalizeSqlMethod = typeof(PetaPocoExt)
                .GetMethod(
                    NormalizeSqlMethodName,
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    [typeof(string), typeof(object[])],
                    null);

            // Act
            var result = normalizeSqlMethod.Invoke(
                null,
                new object[]
                {
                    sql,
                    args,
                });

            // Assert
            Assert.That(result, Is.EqualTo(expected));
			_logger.LogDebug("<<< Exiting NormalizeSql_WithAtCharAndEmptyArguments_EscapesAtCharSuccessfully(sql={Sql},args={Args},expected={Expected})", sql, args, expected);
        }
    }
}
