// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Routing
{
    using System.Web;

    using DotNetNuke.Web.Mvc.Framework.Modules;
    using DotNetNuke.Web.Mvc.Routing;
    using Moq;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class HttpContextExtensionsTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        [Test]
        public void GetModuleRequestResult_Returns_ModuleRequestResult_If_Present()
        {
			_logger.LogDebug(">>> Entering GetModuleRequestResult_Returns_ModuleRequestResult_If_Present()");
            // Arrange
            var httpContext = MockHelper.CreateMockHttpContext();
			_logger.LogDebug("GetModuleRequestResult_Returns_ModuleRequestResult_If_Present(): httpContext → {HttpContext}", httpContext);
            var expectedResult = new ModuleRequestResult();
			_logger.LogDebug("GetModuleRequestResult_Returns_ModuleRequestResult_If_Present(): expectedResult → {ExpectedResult}", expectedResult);
            httpContext.Items[HttpContextExtensions.ModuleRequestResultKey] = expectedResult;

            // Act
            var result = httpContext.GetModuleRequestResult();
			_logger.LogDebug("GetModuleRequestResult_Returns_ModuleRequestResult_If_Present(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.SameAs(expectedResult));
			_logger.LogDebug("<<< Exiting GetModuleRequestResult_Returns_ModuleRequestResult_If_Present()");
        }

        [Test]
        public void GetModuleRequestResult_Returns_Null_If_Not_Present()
        {
			_logger.LogDebug(">>> Entering GetModuleRequestResult_Returns_Null_If_Not_Present()");
            // Arrange
            var httpContext = MockHelper.CreateMockHttpContext();
			_logger.LogDebug("GetModuleRequestResult_Returns_Null_If_Not_Present(): httpContext → {HttpContext}", httpContext);

            // Act
            var result = httpContext.GetModuleRequestResult();
			_logger.LogDebug("GetModuleRequestResult_Returns_Null_If_Not_Present(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.Null);
			_logger.LogDebug("<<< Exiting GetModuleRequestResult_Returns_Null_If_Not_Present()");
        }

        [Test]
        public void HasModuleRequestResult_Returns_True_If_Present()
        {
			_logger.LogDebug(">>> Entering HasModuleRequestResult_Returns_True_If_Present()");
            // Arrange
            HttpContextBase httpContext = MockHelper.CreateMockHttpContext();
			_logger.LogDebug("HasModuleRequestResult_Returns_True_If_Present(): httpContext → {HttpContext}", httpContext);
            var expectedResult = new ModuleRequestResult();
			_logger.LogDebug("HasModuleRequestResult_Returns_True_If_Present(): expectedResult → {ExpectedResult}", expectedResult);
            httpContext.Items[HttpContextExtensions.ModuleRequestResultKey] = expectedResult;

            // Act and Assert
            Assert.That(httpContext.HasModuleRequestResult(), Is.True);
			_logger.LogDebug("<<< Exiting HasModuleRequestResult_Returns_True_If_Present()");
        }

        [Test]
        public void HasModuleRequestResult_Returns_False_If_Not_Present()
        {
			_logger.LogDebug(">>> Entering HasModuleRequestResult_Returns_False_If_Not_Present()");
            // Arrange
            HttpContextBase httpContext = MockHelper.CreateMockHttpContext();
			_logger.LogDebug("HasModuleRequestResult_Returns_False_If_Not_Present(): httpContext → {HttpContext}", httpContext);

            // Act and Assert
            Assert.That(httpContext.HasModuleRequestResult(), Is.False);
			_logger.LogDebug("<<< Exiting HasModuleRequestResult_Returns_False_If_Not_Present()");
        }

        [Test]
        public void SetModuleRequestResult_Sets_ModuleRequestResult()
        {
			_logger.LogDebug(">>> Entering SetModuleRequestResult_Sets_ModuleRequestResult()");
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
			_logger.LogDebug("SetModuleRequestResult_Sets_ModuleRequestResult(): context → {Context}", context);
            var expectedResult = new ModuleRequestResult();
			_logger.LogDebug("SetModuleRequestResult_Sets_ModuleRequestResult(): expectedResult → {ExpectedResult}", expectedResult);

            // Act
            context.SetModuleRequestResult(expectedResult);

            // Assert
            var actual = context.Items[HttpContextExtensions.ModuleRequestResultKey];
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.InstanceOf<ModuleRequestResult>());
			_logger.LogDebug("<<< Exiting SetModuleRequestResult_Sets_ModuleRequestResult()");
        }
    }
}
