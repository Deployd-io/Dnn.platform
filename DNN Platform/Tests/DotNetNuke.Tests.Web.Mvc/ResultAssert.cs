// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc
{
    using System;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Web.Mvc.Framework.ActionResults;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    public static class ResultAssert
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public static void IsEmpty(ActionResult result)
        {
			_logger.LogDebug(">>> Entering IsEmpty(result={Result})", result);
            Assert.That(result, Is.InstanceOf<EmptyResult>());
			_logger.LogDebug("<<< Exiting IsEmpty(result={Result})", result);
        }

        public static void IsUnauthorized(ActionResult result)
        {
			_logger.LogDebug(">>> Entering IsUnauthorized(result={Result})", result);
            Assert.That(result, Is.InstanceOf<HttpUnauthorizedResult>());
			_logger.LogDebug("<<< Exiting IsUnauthorized(result={Result})", result);
        }

        public static void IsView(ActionResult result, string viewName)
        {
			_logger.LogDebug(">>> Entering IsView(result={Result},viewName={ViewName})", result, viewName);
            IsView(result, viewName, string.Empty, new RouteValueDictionary());
			_logger.LogDebug("<<< Exiting IsView(result={Result},viewName={ViewName})", result, viewName);
        }

        public static void IsView(ActionResult result, string viewName, string masterName, RouteValueDictionary expectedViewData)
        {
			_logger.LogDebug(">>> Entering IsView(result={Result},viewName={ViewName},masterName={MasterName},expectedViewData={ExpectedViewData})", result, viewName, masterName, expectedViewData);
            ViewResult viewResult = result.AssertCast<ViewResult>();
            StringsEqualOrBothNullOrEmpty(viewName, viewResult.ViewName);
			_logger.LogDebug("IsView(result={Result},viewName={ViewName},masterName={MasterName},expectedViewData={ExpectedViewData}): viewResult → {ViewResult}", result, viewName, masterName, expectedViewData, viewResult);
            StringsEqualOrBothNullOrEmpty(viewName, viewResult.ViewName);

            DictionaryAssert.ContainsEntries(expectedViewData, viewResult.ViewData);
			_logger.LogDebug("<<< Exiting IsView(result={Result},viewName={ViewName},masterName={MasterName},expectedViewData={ExpectedViewData})", result, viewName, masterName, expectedViewData);
        }

        private static TCast AssertCast<TCast>(this ActionResult result)
            where TCast : class
        {
			_logger.LogDebug(">>> Entering AssertCast(result={Result})", result);
            var castResult = result as TCast;
            Assert.That(castResult, Is.Not.Null);
			_logger.LogDebug("<<< Exiting AssertCast(result={Result})", result);
            return castResult;
        }

        private static void StringsEqualOrBothNullOrEmpty(string expected, string actual)
        {
			_logger.LogDebug(">>> Entering StringsEqualOrBothNullOrEmpty(expected={Expected},actual={Actual})", expected, actual);
            if (string.IsNullOrEmpty(expected))
            {
                Assert.That(actual, Is.Null.Or.Empty);
            }
            else
            {
                Assert.That(actual, Is.EqualTo(expected));
            }
			_logger.LogDebug("<<< Exiting StringsEqualOrBothNullOrEmpty(expected={Expected},actual={Actual})", expected, actual);
        }
    }
}
