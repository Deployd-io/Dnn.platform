// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Framework.Modules
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Tests.Web.Mvc.Fakes;
    using DotNetNuke.Tests.Web.Mvc.Fakes.Filters;
    using DotNetNuke.Web.Mvc.Framework.Modules;
    using NUnit.Framework;

    [TestFixture]
    public class ResultCapturingActionInvokerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private ResultCapturingActionInvoker _actionInvoker;

        [SetUp]
        public void Setup()
        {
			Logger.LogDebug(">>> Entering Setup()");
            this._actionInvoker = new ResultCapturingActionInvoker();
			Logger.LogDebug("<<< Exiting Setup()");
			Logger.LogDebug("Setup(): _actionInvoker → {_actionInvoker}", _actionInvoker);
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this._actionInvoker = null;
			Logger.LogDebug("<<< Exiting TearDown()");
			Logger.LogDebug("TearDown(): _actionInvoker → {_actionInvoker}", _actionInvoker);
			Logger.LogDebug("TearDown(): this._actionInvoker → {Arg0}", this._actionInvoker);
        }

        [Test]
        public void InvokeActionResult_Sets_ResultOfLastInvoke()
        {
			Logger.LogDebug(">>> Entering InvokeActionResult_Sets_ResultOfLastInvoke()");
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();

			Logger.LogDebug("InvokeActionResult_Sets_ResultOfLastInvoke(): context → {Context}", context);
            var controller = new FakeController();
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);
			Logger.LogDebug("InvokeActionResult_Sets_ResultOfLastInvoke(): controller → {Controller}", controller);

            // Act
            this._actionInvoker.InvokeAction(controller.ControllerContext, "Index");

            // Assert
            Assert.That(this._actionInvoker.ResultOfLastInvoke, Is.Not.Null);
            Assert.That(this._actionInvoker.ResultOfLastInvoke, Is.InstanceOf<ViewResult>());
			Logger.LogDebug("<<< Exiting InvokeActionResult_Sets_ResultOfLastInvoke()");
        }

        [Test]
        public void ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_True()
        {
			Logger.LogDebug(">>> Entering ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_True()");
            // Arrange
            var httpContextBase = MockHelper.CreateMockHttpContext();
            var controller = this.SetupController(httpContextBase);
			Logger.LogDebug("ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_True(): httpContextBase → {HttpContextBase}", httpContextBase);
			Logger.LogDebug("ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_True(): controller → {Controller}", controller);
			Logger.LogDebug("ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_True(): FakeHandleExceptionRedirectAttribute.IsExceptionHandled → {Arg0}", FakeHandleExceptionRedirectAttribute.IsExceptionHandled);
            FakeHandleExceptionRedirectAttribute.IsExceptionHandled = true;
            var expectedResult = FakeHandleExceptionRedirectAttribute.Result;

            // Act
            this._actionInvoker.InvokeAction(controller.ControllerContext, nameof(FakeDnnController.ActionWithExceptionFilter));

            // Assert
            Assert.That(this._actionInvoker.ResultOfLastInvoke, Is.EqualTo(expectedResult));
			Logger.LogDebug("<<< Exiting ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_True()");
        }

        [Test]
        public void ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_False()
        {
			Logger.LogDebug(">>> Entering ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_False()");
            // Arrange
            var httpContextBase = MockHelper.CreateMockHttpContext();
            var controller = this.SetupController(httpContextBase);
			Logger.LogDebug("ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_False(): httpContextBase → {HttpContextBase}", httpContextBase);
			Logger.LogDebug("ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_False(): controller → {Controller}", controller);
			Logger.LogDebug("ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_False(): FakeHandleExceptionRedirectAttribute.IsExceptionHandled → {Arg0}", FakeHandleExceptionRedirectAttribute.IsExceptionHandled);
            FakeHandleExceptionRedirectAttribute.IsExceptionHandled = false;
            var expectedResult = FakeRedirectAttribute.Result;

            // Act
            Assert.Throws<Exception>(() => this._actionInvoker.InvokeAction(controller.ControllerContext, nameof(FakeDnnController.ActionWithExceptionFilter)));

            // Assert
            Assert.That(this._actionInvoker.ResultOfLastInvoke, Is.EqualTo(expectedResult));
			Logger.LogDebug("<<< Exiting ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_False()");
        }

        [Test]
        public void ActionInvoker_InvokeOnExecutingFilters()
        {
			Logger.LogDebug(">>> Entering ActionInvoker_InvokeOnExecutingFilters()");
            // Arrange
            var httpContextBase = MockHelper.CreateMockHttpContext();
            var controller = this.SetupController(httpContextBase);
			Logger.LogDebug("ActionInvoker_InvokeOnExecutingFilters(): httpContextBase → {HttpContextBase}", httpContextBase);
			Logger.LogDebug("ActionInvoker_InvokeOnExecutingFilters(): controller → {Controller}", controller);
            var expectedResult = FakeRedirectAttribute.Result;

            // Act
            this._actionInvoker.InvokeAction(controller.ControllerContext, nameof(FakeDnnController.ActionWithOnExecutingFilter));

            // Assert
            Assert.That(this._actionInvoker.ResultOfLastInvoke, Is.EqualTo(expectedResult));
			Logger.LogDebug("<<< Exiting ActionInvoker_InvokeOnExecutingFilters()");
        }

        [Test]
        public void ActionInvoker_InvokeOnExecutedFilters()
        {
			Logger.LogDebug(">>> Entering ActionInvoker_InvokeOnExecutedFilters()");
            // Arrange
            var httpContextBase = MockHelper.CreateMockHttpContext();
            var controller = this.SetupController(httpContextBase);
			Logger.LogDebug("ActionInvoker_InvokeOnExecutedFilters(): httpContextBase → {HttpContextBase}", httpContextBase);
			Logger.LogDebug("ActionInvoker_InvokeOnExecutedFilters(): controller → {Controller}", controller);
            var expectedResult = FakeRedirectAttribute.Result;

            // Act
            this._actionInvoker.InvokeAction(controller.ControllerContext, nameof(FakeDnnController.ActionWithOnExecutedFilter));

            // Assert
            Assert.That(this._actionInvoker.ResultOfLastInvoke, Is.EqualTo(expectedResult));
			Logger.LogDebug("<<< Exiting ActionInvoker_InvokeOnExecutedFilters()");
        }

        private FakeDnnController SetupController(HttpContextBase context)
        {
            var controller = new FakeDnnController();
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);
            return controller;
        }
    }
}
