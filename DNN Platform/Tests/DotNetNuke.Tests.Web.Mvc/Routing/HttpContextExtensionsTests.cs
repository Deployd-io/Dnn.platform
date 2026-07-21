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

    [TestFixture]
    public class HttpContextExtensionsTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [Test]
        public void GetModuleRequestResult_Returns_ModuleRequestResult_If_Present()
        {
			Logger.LogDebug(">>> Entering GetModuleRequestResult_Returns_ModuleRequestResult_If_Present()");
            // Arrange
            var httpContext = MockHelper.CreateMockHttpContext();
            var expectedResult = new ModuleRequestResult();
			Logger.LogDebug("GetModuleRequestResult_Returns_ModuleRequestResult_If_Present(): httpContext → {HttpContext}", httpContext);
			Logger.LogDebug("GetModuleRequestResult_Returns_ModuleRequestResult_If_Present(): expectedResult → {ExpectedResult}", expectedResult);
            httpContext.Items[HttpContextExtensions.ModuleRequestResultKey] = expectedResult;

            // Act
            var result = httpContext.GetModuleRequestResult();

			Logger.LogDebug("GetModuleRequestResult_Returns_ModuleRequestResult_If_Present(): result → {Result}", result);
            // Assert
            Assert.That(result, Is.SameAs(expectedResult));
			Logger.LogDebug("<<< Exiting GetModuleRequestResult_Returns_ModuleRequestResult_If_Present()");
        }

        [Test]
        public void GetModuleRequestResult_Returns_Null_If_Not_Present()
        {
			Logger.LogDebug(">>> Entering GetModuleRequestResult_Returns_Null_If_Not_Present()");
            // Arrange
            var httpContext = MockHelper.CreateMockHttpContext();

			Logger.LogDebug("GetModuleRequestResult_Returns_Null_If_Not_Present(): httpContext → {HttpContext}", httpContext);
            // Act
            var result = httpContext.GetModuleRequestResult();

			Logger.LogDebug("GetModuleRequestResult_Returns_Null_If_Not_Present(): result → {Result}", result);
            // Assert
            Assert.That(result, Is.Null);
			Logger.LogDebug("<<< Exiting GetModuleRequestResult_Returns_Null_If_Not_Present()");
        }

        [Test]
        public void HasModuleRequestResult_Returns_True_If_Present()
        {
			Logger.LogDebug(">>> Entering HasModuleRequestResult_Returns_True_If_Present()");
            // Arrange
            HttpContextBase httpContext = MockHelper.CreateMockHttpContext();
            var expectedResult = new ModuleRequestResult();
			Logger.LogDebug("HasModuleRequestResult_Returns_True_If_Present(): httpContext → {HttpContext}", httpContext);
			Logger.LogDebug("HasModuleRequestResult_Returns_True_If_Present(): expectedResult → {ExpectedResult}", expectedResult);
            httpContext.Items[HttpContextExtensions.ModuleRequestResultKey] = expectedResult;

            // Act and Assert
            Assert.That(httpContext.HasModuleRequestResult(), Is.True);
			Logger.LogDebug("<<< Exiting HasModuleRequestResult_Returns_True_If_Present()");
        }

        [Test]
        public void HasModuleRequestResult_Returns_False_If_Not_Present()
        {
			Logger.LogDebug(">>> Entering HasModuleRequestResult_Returns_False_If_Not_Present()");
            // Arrange
            HttpContextBase httpContext = MockHelper.CreateMockHttpContext();

			Logger.LogDebug("HasModuleRequestResult_Returns_False_If_Not_Present(): httpContext → {HttpContext}", httpContext);
            // Act and Assert
            Assert.That(httpContext.HasModuleRequestResult(), Is.False);
			Logger.LogDebug("<<< Exiting HasModuleRequestResult_Returns_False_If_Not_Present()");
        }

        [Test]
        public void SetModuleRequestResult_Sets_ModuleRequestResult()
        {
			Logger.LogDebug(">>> Entering SetModuleRequestResult_Sets_ModuleRequestResult()");
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            var expectedResult = new ModuleRequestResult();
			Logger.LogDebug("SetModuleRequestResult_Sets_ModuleRequestResult(): context → {Context}", context);
			Logger.LogDebug("SetModuleRequestResult_Sets_ModuleRequestResult(): expectedResult → {ExpectedResult}", expectedResult);

            // Act
            context.SetModuleRequestResult(expectedResult);

            // Assert
            var actual = context.Items[HttpContextExtensions.ModuleRequestResultKey];
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.InstanceOf<ModuleRequestResult>());
			Logger.LogDebug("<<< Exiting SetModuleRequestResult_Sets_ModuleRequestResult()");
        }
    }
}
