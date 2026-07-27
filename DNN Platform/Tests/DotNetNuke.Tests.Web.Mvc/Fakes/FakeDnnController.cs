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

    public class FakeDnnController : DnnController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public ActionResult Action1()
        {
			Logger.LogDebug(">>> Entering Action1()");
			Logger.LogDebug("<<< Exiting Action1()");
            return this.View("Action1");
        }

        public ActionResult Action2()
        {
			Logger.LogDebug(">>> Entering Action2()");
			Logger.LogDebug("<<< Exiting Action2()");
            return this.View("Action2", "Master2");
        }

        public ActionResult Action3(Dog dog)
        {
			Logger.LogDebug(">>> Entering Action3(dog={Dog})", dog);
			Logger.LogDebug("<<< Exiting Action3(dog={Dog})", dog);
            return this.View("Action3", "Master3", dog);
        }

        [FakeHandleExceptionRedirect]
        public ActionResult ActionWithExceptionFilter()
        {
			Logger.LogDebug(">>> Entering ActionWithExceptionFilter()");
            throw new Exception();
        }

        [FakeOnExecutingRedirect]
        public ActionResult ActionWithOnExecutingFilter()
        {
			Logger.LogDebug(">>> Entering ActionWithOnExecutingFilter()");
			Logger.LogDebug("<<< Exiting ActionWithOnExecutingFilter()");
            return this.View("Action1");
        }

        [FakeOnExecutedRedirect]
        public ActionResult ActionWithOnExecutedFilter()
        {
			Logger.LogDebug(">>> Entering ActionWithOnExecutedFilter()");
			Logger.LogDebug("<<< Exiting ActionWithOnExecutedFilter()");
            return this.View("Action1");
        }

        public void MockInitialize(RequestContext requestContext)
        {
			Logger.LogDebug(">>> Entering MockInitialize(requestContext={RequestContext})", requestContext);
            // Mocking out the entire MvcHandler and Controller lifecycle proved to be difficult
            // This method executes the initialization logic that occurs on every request which is
            // executed from the Execute method.
            this.Initialize(requestContext);
			Logger.LogDebug("<<< Exiting MockInitialize(requestContext={RequestContext})", requestContext);
        }
    }
}
