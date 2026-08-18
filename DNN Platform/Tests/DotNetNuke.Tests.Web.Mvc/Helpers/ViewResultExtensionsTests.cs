// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Helpers
{
    using System;
    using System.IO;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Web.Mvc.Framework.ActionResults;
    using Moq;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class DnnViewResultTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        [Test]
        public void ExecuteResult_Throws_With_Null_ControllerContext()
        {
			_logger.LogDebug(">>> Entering ExecuteResult_Throws_With_Null_ControllerContext()");
            // Arrange
            var result = new DnnViewResult();
			_logger.LogDebug("ExecuteResult_Throws_With_Null_ControllerContext(): result → {Result}", result);

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => result.ExecuteResult(null, new StringWriter()));
			_logger.LogDebug("<<< Exiting ExecuteResult_Throws_With_Null_ControllerContext()");
        }

        [Test]
        public void ExecuteResult_Throws_With_Null_TextWriter()
        {
			_logger.LogDebug(">>> Entering ExecuteResult_Throws_With_Null_TextWriter()");
            // Arrange
            var result = new DnnViewResult();
			_logger.LogDebug("ExecuteResult_Throws_With_Null_TextWriter(): result → {Result}", result);
            var context = MockHelper.CreateMockControllerContext();
			_logger.LogDebug("ExecuteResult_Throws_With_Null_TextWriter(): context → {Context}", context);

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => result.ExecuteResult(context, null));
			_logger.LogDebug("<<< Exiting ExecuteResult_Throws_With_Null_TextWriter()");
        }

        [Test]
        public void ExecuteResult_Throws_With_Empty_ViewName_And_No_Action()
        {
			_logger.LogDebug(">>> Entering ExecuteResult_Throws_With_Empty_ViewName_And_No_Action()");
            // Arrange
            var result = new DnnViewResult();
			_logger.LogDebug("ExecuteResult_Throws_With_Empty_ViewName_And_No_Action(): result → {Result}", result);
            var context = MockHelper.CreateMockControllerContext();
			_logger.LogDebug("ExecuteResult_Throws_With_Empty_ViewName_And_No_Action(): context → {Context}", context);

            // Act, Assert
            Assert.Throws<InvalidOperationException>(() => result.ExecuteResult(context, new StringWriter()));
			_logger.LogDebug("<<< Exiting ExecuteResult_Throws_With_Empty_ViewName_And_No_Action()");
        }

        [Test]
        public void ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null()
        {
			_logger.LogDebug(">>> Entering ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null()");
            // Arrange
            var result = new DnnViewResult();
			_logger.LogDebug("ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null(): result → {Result}", result);

            var routeData = new RouteData();
			_logger.LogDebug("ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null(): routeData → {RouteData}", routeData);
            routeData.Values["controller"] = "Controller";
            routeData.Values["action"] = "Action";
            routeData.Values["id"] = "Id";
            var context = MockHelper.CreateMockControllerContext(routeData);
			_logger.LogDebug("ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null(): context → {Context}", context);

            var mockView = new Mock<IView>();
			_logger.LogDebug("ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null(): mockView → {MockView}", mockView);
            var mockViewEngine = new Mock<IViewEngine>();
			_logger.LogDebug("ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null(): mockViewEngine → {MockViewEngine}", mockViewEngine);
            mockViewEngine.Setup(v => v.FindView(It.IsAny<ControllerContext>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new ViewEngineResult(mockView.Object, mockViewEngine.Object));
            result.ViewEngineCollection.Clear();
            result.ViewEngineCollection.Add(mockViewEngine.Object);

            // Act
            result.ExecuteResult(context, new StringWriter());

            // Assert
            Assert.That(result.View, Is.SameAs(mockView.Object));
			_logger.LogDebug("<<< Exiting ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null()");
        }
    }
}
