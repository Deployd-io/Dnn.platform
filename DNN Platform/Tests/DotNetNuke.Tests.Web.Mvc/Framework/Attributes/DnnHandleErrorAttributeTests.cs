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

    [TestFixture]
    public class DnnHandleErrorAttributeTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [Test]
        public void OnException_Throws_InvalidOPerationExcption_IfNotUsedIn_IDnnController()
        {
			Logger.LogDebug(">>> Entering OnException_Throws_InvalidOPerationExcption_IfNotUsedIn_IDnnController()");
            // Arrange
            const string expectedMessage = "This attribute can only be applied to Controllers that implement IDnnController";
            var sut = new DnnHandleErrorAttribute();
			Logger.LogDebug("OnException_Throws_InvalidOPerationExcption_IfNotUsedIn_IDnnController(): sut → {Sut}", sut);

            var exceptionContext = new ExceptionContext();

			Logger.LogDebug("OnException_Throws_InvalidOPerationExcption_IfNotUsedIn_IDnnController(): exceptionContext → {ExceptionContext}", exceptionContext);
            // Act / Assert
            var ex = Assert.Throws<InvalidOperationException>(() => sut.OnException(exceptionContext));
            Assert.That(ex.Message, Is.EqualTo(expectedMessage));
			Logger.LogDebug("<<< Exiting OnException_Throws_InvalidOPerationExcption_IfNotUsedIn_IDnnController()");
			Logger.LogDebug("OnException_Throws_InvalidOPerationExcption_IfNotUsedIn_IDnnController(): ex → {Ex}", ex);
        }

        [Test]
        public void OnException_Logs_TheException()
        {
			Logger.LogDebug(">>> Entering OnException_Logs_TheException()");
            // Arrange
            var testException = new Exception();
            var exceptionContext = new ExceptionContext
			Logger.LogDebug("OnException_Logs_TheException(): testException → {TestException}", testException);
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
			Logger.LogDebug("<<< Exiting OnException_Logs_TheException()");
        }
    }
}
