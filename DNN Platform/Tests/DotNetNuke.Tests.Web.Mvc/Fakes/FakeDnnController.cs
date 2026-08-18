// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Fakes
{
    using System;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Tests.Web.Mvc.Fakes.Filters;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
	using Microsoft.Extensions.Logging;

    public class FakeDnnController : DnnController
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public ActionResult Action1()
        {
			_logger.LogDebug(">>> Entering Action1()");
			_logger.LogDebug("<<< Exiting Action1()");
            return this.View("Action1");
        }

        public ActionResult Action2()
        {
			_logger.LogDebug(">>> Entering Action2()");
			_logger.LogDebug("<<< Exiting Action2()");
            return this.View("Action2", "Master2");
        }

        public ActionResult Action3(Dog dog)
        {
			_logger.LogDebug(">>> Entering Action3(dog={Dog})", dog);
			_logger.LogDebug("<<< Exiting Action3(dog={Dog})", dog);
            return this.View("Action3", "Master3", dog);
        }

        [FakeHandleExceptionRedirect]
        public ActionResult ActionWithExceptionFilter()
        {
			_logger.LogDebug(">>> Entering ActionWithExceptionFilter()");
			_logger.LogWarning("Throwing Exception from ActionWithExceptionFilter()");
			_logger.LogDebug("<<< Exiting ActionWithExceptionFilter()");
            throw new Exception();
        }

        [FakeOnExecutingRedirect]
        public ActionResult ActionWithOnExecutingFilter()
        {
			_logger.LogDebug(">>> Entering ActionWithOnExecutingFilter()");
			_logger.LogDebug("<<< Exiting ActionWithOnExecutingFilter()");
            return this.View("Action1");
        }

        [FakeOnExecutedRedirect]
        public ActionResult ActionWithOnExecutedFilter()
        {
			_logger.LogDebug(">>> Entering ActionWithOnExecutedFilter()");
			_logger.LogDebug("<<< Exiting ActionWithOnExecutedFilter()");
            return this.View("Action1");
        }

        public void MockInitialize(RequestContext requestContext)
        {
			_logger.LogDebug(">>> Entering MockInitialize(requestContext={RequestContext})", requestContext);
            // Mocking out the entire MvcHandler and Controller lifecycle proved to be difficult
            // This method executes the initialization logic that occurs on every request which is
            // executed from the Execute method.
            this.Initialize(requestContext);
			_logger.LogDebug("<<< Exiting MockInitialize(requestContext={RequestContext})", requestContext);
        }
    }
}
