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

    public static class ResultAssert
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static void IsEmpty(ActionResult result)
        {
			Logger.LogDebug(">>> Entering IsEmpty(result={Result})", result);
            Assert.That(result, Is.InstanceOf<EmptyResult>());
			Logger.LogDebug("<<< Exiting IsEmpty(result={Result})", result);
        }

        public static void IsUnauthorized(ActionResult result)
        {
			Logger.LogDebug(">>> Entering IsUnauthorized(result={Result})", result);
            Assert.That(result, Is.InstanceOf<HttpUnauthorizedResult>());
			Logger.LogDebug("<<< Exiting IsUnauthorized(result={Result})", result);
        }

        public static void IsView(ActionResult result, string viewName)
        {
			Logger.LogDebug(">>> Entering IsView(result={Result},viewName={ViewName})", result, viewName);
            IsView(result, viewName, string.Empty, new RouteValueDictionary());
			Logger.LogDebug("<<< Exiting IsView(result={Result},viewName={ViewName})", result, viewName);
        }

        public static void IsView(ActionResult result, string viewName, string masterName, RouteValueDictionary expectedViewData)
        {
			Logger.LogDebug(">>> Entering IsView(result={Result},viewName={ViewName},masterName={MasterName},expectedViewData={ExpectedViewData})", result, viewName, masterName, expectedViewData);
            ViewResult viewResult = result.AssertCast<ViewResult>();
            StringsEqualOrBothNullOrEmpty(viewName, viewResult.ViewName);
			Logger.LogDebug("IsView(result={Result},viewName={ViewName},masterName={MasterName},expectedViewData={ExpectedViewData}): viewResult → {ViewResult}", result, viewName, masterName, expectedViewData, viewResult);
            StringsEqualOrBothNullOrEmpty(viewName, viewResult.ViewName);

            DictionaryAssert.ContainsEntries(expectedViewData, viewResult.ViewData);
			Logger.LogDebug("<<< Exiting IsView(result={Result},viewName={ViewName},masterName={MasterName},expectedViewData={ExpectedViewData})", result, viewName, masterName, expectedViewData);
        }

        private static TCast AssertCast<TCast>(this ActionResult result)
            where TCast : class
        {
			Logger.LogDebug(">>> Entering AssertCast(result={Result})", result);
            var castResult = result as TCast;
            Assert.That(castResult, Is.Not.Null);
            return castResult;
        }

        private static void StringsEqualOrBothNullOrEmpty(string expected, string actual)
        {
			Logger.LogDebug(">>> Entering StringsEqualOrBothNullOrEmpty(expected={Expected},actual={Actual})", expected, actual);
            if (string.IsNullOrEmpty(expected))
            {
                Assert.That(actual, Is.Null.Or.Empty);
            }
            else
            {
                Assert.That(actual, Is.EqualTo(expected));
            }
			Logger.LogDebug("<<< Exiting StringsEqualOrBothNullOrEmpty(expected={Expected},actual={Actual})", expected, actual);
        }
    }
}
