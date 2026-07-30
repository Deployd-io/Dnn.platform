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

    [TestFixture]
    public class DnnViewResultTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [Test]
        public void ExecuteResult_Throws_With_Null_ControllerContext()
        {
			Logger.LogDebug(">>> Entering ExecuteResult_Throws_With_Null_ControllerContext()");
            // Arrange
            var result = new DnnViewResult();

			Logger.LogDebug("ExecuteResult_Throws_With_Null_ControllerContext(): result → {Result}", result);
            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => result.ExecuteResult(null, new StringWriter()));
			Logger.LogDebug("<<< Exiting ExecuteResult_Throws_With_Null_ControllerContext()");
        }

        [Test]
        public void ExecuteResult_Throws_With_Null_TextWriter()
        {
			Logger.LogDebug(">>> Entering ExecuteResult_Throws_With_Null_TextWriter()");
            // Arrange
            var result = new DnnViewResult();
            var context = MockHelper.CreateMockControllerContext();
			Logger.LogDebug("ExecuteResult_Throws_With_Null_TextWriter(): result → {Result}", result);
			Logger.LogDebug("ExecuteResult_Throws_With_Null_TextWriter(): context → {Context}", context);

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => result.ExecuteResult(context, null));
			Logger.LogDebug("<<< Exiting ExecuteResult_Throws_With_Null_TextWriter()");
        }

        [Test]
        public void ExecuteResult_Throws_With_Empty_ViewName_And_No_Action()
        {
			Logger.LogDebug(">>> Entering ExecuteResult_Throws_With_Empty_ViewName_And_No_Action()");
            // Arrange
            var result = new DnnViewResult();
            var context = MockHelper.CreateMockControllerContext();
			Logger.LogDebug("ExecuteResult_Throws_With_Empty_ViewName_And_No_Action(): result → {Result}", result);
			Logger.LogDebug("ExecuteResult_Throws_With_Empty_ViewName_And_No_Action(): context → {Context}", context);

            // Act, Assert
            Assert.Throws<InvalidOperationException>(() => result.ExecuteResult(context, new StringWriter()));
			Logger.LogDebug("<<< Exiting ExecuteResult_Throws_With_Empty_ViewName_And_No_Action()");
        }

        [Test]
        public void ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null()
        {
			Logger.LogDebug(">>> Entering ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null()");
            // Arrange
            var result = new DnnViewResult();

			Logger.LogDebug("ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null(): result → {Result}", result);
            var routeData = new RouteData();
            routeData.Values["controller"] = "Controller";
			Logger.LogDebug("ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null(): routeData → {RouteData}", routeData);
            routeData.Values["action"] = "Action";
            routeData.Values["id"] = "Id";
            var context = MockHelper.CreateMockControllerContext(routeData);

			Logger.LogDebug("ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null(): context → {Context}", context);
            var mockView = new Mock<IView>();
            var mockViewEngine = new Mock<IViewEngine>();
			Logger.LogDebug("ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null(): mockView → {MockView}", mockView);
			Logger.LogDebug("ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null(): mockViewEngine → {MockViewEngine}", mockViewEngine);
            mockViewEngine.Setup(v => v.FindView(It.IsAny<ControllerContext>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new ViewEngineResult(mockView.Object, mockViewEngine.Object));
            result.ViewEngineCollection.Clear();
            result.ViewEngineCollection.Add(mockViewEngine.Object);

            // Act
            result.ExecuteResult(context, new StringWriter());

            // Assert
            Assert.That(result.View, Is.SameAs(mockView.Object));
			Logger.LogDebug("<<< Exiting ExecuteResult_Calls_ViewEngine_FindView_If_View_Is_Null()");
        }
    }
}
