// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    public static class EnumerableAssert
    {
        public static void ElementsAreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
			_logger.LogDebug(">>> Entering ElementsAreEqual(expected={Expected},actual={Actual})", expected, actual);
            ElementsMatch(expected, actual, (e, a) => Equals(e, a));
			_logger.LogDebug("<<< Exiting ElementsAreEqual(expected={Expected},actual={Actual})", expected, actual);
        }

        public static void ElementsAreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message)
        {
            ElementsMatch(expected, actual, (e, a) => Equals(e, a), message);
        }

        public static void ElementsAreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string message, params object[] args)
        {
            ElementsMatch(expected, actual, (e, a) => Equals(e, a), message, args);
        }

        public static void ElementsMatch<TExpected, TActual>(IEnumerable<TExpected> expected, IEnumerable<TActual> actual, Func<TExpected, TActual, bool> matcher)
        {
			_logger.LogDebug(">>> Entering ElementsMatch(expected={Expected},actual={Actual},matcher={Matcher})", expected, actual, matcher);
            ElementsMatch(expected, actual, matcher, string.Empty, []);
			_logger.LogDebug("<<< Exiting ElementsMatch(expected={Expected},actual={Actual},matcher={Matcher})", expected, actual, matcher);
        }

        public static void ElementsMatch<TExpected, TActual>(IEnumerable<TExpected> expected, IEnumerable<TActual> actual, Func<TExpected, TActual, bool> matcher, string message)
        {
            ElementsMatch(expected, actual, matcher, message, []);
        }

        public static void ElementsMatch<TExpected, TActual>(IEnumerable<TExpected> expected, IEnumerable<TActual> actual, Func<TExpected, TActual, bool> matcher, string message, params object[] args)
        {
            IEnumerator<TExpected> expectedEnumerator = expected.GetEnumerator();
            IEnumerator<TActual> actualEnumerator = actual.GetEnumerator();
			_logger.LogDebug("ElementsMatch(expected={Expected},actual={Actual},matcher={Matcher},message={Message},args={Args}): actualEnumerator → {ActualEnumerator}", expected, actual, matcher, message, args, actualEnumerator);
			_logger.LogDebug("ElementsMatch(expected={Expected},actual={Actual},matcher={Matcher},message={Message},args={Args}): expectedEnumerator → {ExpectedEnumerator}", expected, actual, matcher, message, args, expectedEnumerator);

            while (actualEnumerator.MoveNext())
            {
                using (Assert.EnterMultipleScope())
				using Microsoft.Extensions.Logging;
                {
                    Assert.That(expectedEnumerator.MoveNext(), Is.True);
					private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
                    Assert.That(matcher(expectedEnumerator.Current, actualEnumerator.Current), Is.True, () => string.Format(message, args));
                }
            }
        }
    }
}
