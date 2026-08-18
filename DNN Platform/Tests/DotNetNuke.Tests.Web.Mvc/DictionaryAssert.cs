// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc
{
    using System.Collections.Generic;
    using System.Web.Routing;

    using NUnit.Framework;

    public static class DictionaryAssert
    {
        public static void ContainsEntries(object expected, IDictionary<string, object> actual)
        {
            ContainsEntries(new RouteValueDictionary(expected), actual);
			_logger.LogDebug(">>> Entering ContainsEntries(expected={Expected},actual={Actual})", expected, actual);
			_logger.LogDebug("<<< Exiting ContainsEntries(expected={Expected},actual={Actual})", expected, actual);
        }

        public static void ContainsEntries(IDictionary<string, object> expected, IDictionary<string, object> actual)
        {
            foreach (KeyValuePair<string, object> pair in expected)
            {
                using (Assert.EnterMultipleScope())
				using Microsoft.Extensions.Logging;
                {
                    Assert.That(actual, Does.ContainKey(pair.Key));
					private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
                    Assert.That(actual[pair.Key], Is.EqualTo(pair.Value));
                }
            }
        }
    }
}
