// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Fakes
{
    using System.Web.Mvc;
	using Microsoft.Extensions.Logging;

    public class FakeController : Controller
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public ActionResult Index()
        {
			_logger.LogDebug(">>> Entering Index()");
			_logger.LogDebug("<<< Exiting Index()");
            return new ViewResult();
        }
    }
}
