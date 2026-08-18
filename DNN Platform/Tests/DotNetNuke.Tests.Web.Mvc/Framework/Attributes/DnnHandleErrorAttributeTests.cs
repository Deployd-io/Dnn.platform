// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Framework.Attributes
{
    using System;
    using System.Web.Mvc;

    using DotNetNuke.Tests.Web.Mvc.Fakes;
    using DotNetNuke.Web.Mvc.Framework.ActionFilters;
    using Moq;
    using Moq.Protected;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class DnnHandleErrorAttributeTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        [Test]
        public void OnException_Throws_InvalidOPerationExcption_IfNotUsedIn_IDnnController()
        {
			_logger.LogDebug(">>> Entering OnException_Throws_InvalidOPerationExcption_IfNotUsedIn_IDnnController()");
            // Arrange
            const string expectedMessage = "This attribute can only be applied to Controllers that implement IDnnController";
            var sut = new DnnHandleErrorAttribute();
			_logger.LogDebug("OnException_Throws_InvalidOPerationExcption_IfNotUsedIn_IDnnController(): sut → {Sut}", sut);

            var exceptionContext = new ExceptionContext();
			_logger.LogDebug("OnException_Throws_InvalidOPerationExcption_IfNotUsedIn_IDnnController(): exceptionContext → {ExceptionContext}", exceptionContext);

            // Act / Assert
            var ex = Assert.Throws<InvalidOperationException>(() => sut.OnException(exceptionContext));
			_logger.LogDebug("OnException_Throws_InvalidOPerationExcption_IfNotUsedIn_IDnnController(): ex → {Ex}", ex);
            Assert.That(ex.Message, Is.EqualTo(expectedMessage));
			_logger.LogDebug("<<< Exiting OnException_Throws_InvalidOPerationExcption_IfNotUsedIn_IDnnController()");
        }

        [Test]
        public void OnException_Logs_TheException()
        {
			_logger.LogDebug(">>> Entering OnException_Logs_TheException()");
            // Arrange
            var testException = new Exception();
			_logger.LogDebug("OnException_Logs_TheException(): testException → {TestException}", testException);
            var exceptionContext = new ExceptionContext
            {
                Controller = new FakeDnnController(),
                Exception = testException,
            };

            var mockDnnHandleErrorAttribute = new Mock<DnnHandleErrorAttribute> { CallBase = true };
            mockDnnHandleErrorAttribute.Protected().Setup("LogException", testException);

            var sut = mockDnnHandleErrorAttribute.Object;

            // Act
            sut.OnException(exceptionContext);

            // Assert
            mockDnnHandleErrorAttribute.VerifyAll();
			_logger.LogDebug("<<< Exiting OnException_Logs_TheException()");
        }
    }
}
