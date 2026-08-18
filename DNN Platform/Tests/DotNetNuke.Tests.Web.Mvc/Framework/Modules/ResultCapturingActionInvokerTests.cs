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
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class ResultCapturingActionInvokerTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private ResultCapturingActionInvoker _actionInvoker;

        [SetUp]
        public void Setup()
        {
			_logger.LogDebug(">>> Entering Setup()");
            this._actionInvoker = new ResultCapturingActionInvoker();
			_logger.LogDebug("Setup(): _actionInvoker → {_actionInvoker}", _actionInvoker);
			_logger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			_logger.LogDebug(">>> Entering TearDown()");
            this._actionInvoker = null;
			_logger.LogDebug("TearDown(): _actionInvoker → {_actionInvoker}", _actionInvoker);
			_logger.LogDebug("TearDown(): this._actionInvoker → {Arg0}", this._actionInvoker);
			_logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void InvokeActionResult_Sets_ResultOfLastInvoke()
        {
			_logger.LogDebug(">>> Entering InvokeActionResult_Sets_ResultOfLastInvoke()");
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
			_logger.LogDebug("InvokeActionResult_Sets_ResultOfLastInvoke(): context → {Context}", context);

            var controller = new FakeController();
			_logger.LogDebug("InvokeActionResult_Sets_ResultOfLastInvoke(): controller → {Controller}", controller);
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);

            // Act
            this._actionInvoker.InvokeAction(controller.ControllerContext, "Index");

            // Assert
            Assert.That(this._actionInvoker.ResultOfLastInvoke, Is.Not.Null);
            Assert.That(this._actionInvoker.ResultOfLastInvoke, Is.InstanceOf<ViewResult>());
			_logger.LogDebug("<<< Exiting InvokeActionResult_Sets_ResultOfLastInvoke()");
        }

        [Test]
        public void ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_True()
        {
			_logger.LogDebug(">>> Entering ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_True()");
            // Arrange
            var httpContextBase = MockHelper.CreateMockHttpContext();
			_logger.LogDebug("ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_True(): httpContextBase → {HttpContextBase}", httpContextBase);
			_logger.LogDebug("ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_True(): FakeHandleExceptionRedirectAttribute.IsExceptionHandled → {Arg0}", FakeHandleExceptionRedirectAttribute.IsExceptionHandled);
            var controller = this.SetupController(httpContextBase);
			_logger.LogDebug("ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_True(): controller → {Controller}", controller);
            FakeHandleExceptionRedirectAttribute.IsExceptionHandled = true;
            var expectedResult = FakeHandleExceptionRedirectAttribute.Result;

            // Act
            this._actionInvoker.InvokeAction(controller.ControllerContext, nameof(FakeDnnController.ActionWithExceptionFilter));

            // Assert
            Assert.That(this._actionInvoker.ResultOfLastInvoke, Is.EqualTo(expectedResult));
			_logger.LogDebug("<<< Exiting ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_True()");
        }

        [Test]
        public void ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_False()
        {
			_logger.LogDebug(">>> Entering ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_False()");
            // Arrange
            var httpContextBase = MockHelper.CreateMockHttpContext();
			_logger.LogDebug("ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_False(): httpContextBase → {HttpContextBase}", httpContextBase);
			_logger.LogDebug("ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_False(): FakeHandleExceptionRedirectAttribute.IsExceptionHandled → {Arg0}", FakeHandleExceptionRedirectAttribute.IsExceptionHandled);
            var controller = this.SetupController(httpContextBase);
			_logger.LogDebug("ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_False(): controller → {Controller}", controller);
            FakeHandleExceptionRedirectAttribute.IsExceptionHandled = false;
            var expectedResult = FakeRedirectAttribute.Result;

            // Act
            Assert.Throws<Exception>(() => this._actionInvoker.InvokeAction(controller.ControllerContext, nameof(FakeDnnController.ActionWithExceptionFilter)));

            // Assert
            Assert.That(this._actionInvoker.ResultOfLastInvoke, Is.EqualTo(expectedResult));
			_logger.LogDebug("<<< Exiting ActionInvoker_InvokeExceptionFilters_IsExceptionHandled_False()");
        }

        [Test]
        public void ActionInvoker_InvokeOnExecutingFilters()
        {
			_logger.LogDebug(">>> Entering ActionInvoker_InvokeOnExecutingFilters()");
            // Arrange
            var httpContextBase = MockHelper.CreateMockHttpContext();
			_logger.LogDebug("ActionInvoker_InvokeOnExecutingFilters(): httpContextBase → {HttpContextBase}", httpContextBase);
            var controller = this.SetupController(httpContextBase);
			_logger.LogDebug("ActionInvoker_InvokeOnExecutingFilters(): controller → {Controller}", controller);
            var expectedResult = FakeRedirectAttribute.Result;

            // Act
            this._actionInvoker.InvokeAction(controller.ControllerContext, nameof(FakeDnnController.ActionWithOnExecutingFilter));

            // Assert
            Assert.That(this._actionInvoker.ResultOfLastInvoke, Is.EqualTo(expectedResult));
			_logger.LogDebug("<<< Exiting ActionInvoker_InvokeOnExecutingFilters()");
        }

        [Test]
        public void ActionInvoker_InvokeOnExecutedFilters()
        {
			_logger.LogDebug(">>> Entering ActionInvoker_InvokeOnExecutedFilters()");
            // Arrange
            var httpContextBase = MockHelper.CreateMockHttpContext();
			_logger.LogDebug("ActionInvoker_InvokeOnExecutedFilters(): httpContextBase → {HttpContextBase}", httpContextBase);
            var controller = this.SetupController(httpContextBase);
			_logger.LogDebug("ActionInvoker_InvokeOnExecutedFilters(): controller → {Controller}", controller);
            var expectedResult = FakeRedirectAttribute.Result;

            // Act
            this._actionInvoker.InvokeAction(controller.ControllerContext, nameof(FakeDnnController.ActionWithOnExecutedFilter));

            // Assert
            Assert.That(this._actionInvoker.ResultOfLastInvoke, Is.EqualTo(expectedResult));
			_logger.LogDebug("<<< Exiting ActionInvoker_InvokeOnExecutedFilters()");
        }

        private FakeDnnController SetupController(HttpContextBase context)
        {
			_logger.LogDebug(">>> Entering SetupController(context={Context})", context);
            var controller = new FakeDnnController();
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);
			_logger.LogDebug("SetupController(context={Context}): controller → {Controller}", context, controller);
			_logger.LogDebug("<<< Exiting SetupController(context={Context})", context);
            return controller;
        }
    }
}
