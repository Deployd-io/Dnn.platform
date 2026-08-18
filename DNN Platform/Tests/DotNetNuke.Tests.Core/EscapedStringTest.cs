// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class EscapedStringTest
    {
        [Test]
        public void SimpleCase()
        {
			mockEventLogger.LogDebug(">>> Entering SimpleCase()");
            DoTest(
                new[] { "first", "second", },
                "first,second");
				mockEventLogger.LogDebug("<<< Exiting SimpleCase()");
        }

        [Test]
        public void CombinesWithSpecifiedSeparator()
        {
			mockEventLogger.LogDebug(">>> Entering CombinesWithSpecifiedSeparator()");
            Assert.That(
                EscapedString.Combine(new[] { "first", "second", }, ';'),
                Is.EqualTo("first;second"));
				mockEventLogger.LogDebug("<<< Exiting CombinesWithSpecifiedSeparator()");
        }

        [Test]
        public void SeparatesWithSpecifiedSeparator()
        {
			mockEventLogger.LogDebug(">>> Entering SeparatesWithSpecifiedSeparator()");
            Assert.That(
                EscapedString.Seperate("first]second", ']'),
                Is.EqualTo(["first", "second"]).AsCollection);
				mockEventLogger.LogDebug("<<< Exiting SeparatesWithSpecifiedSeparator()");
        }

        [Test]
        public void EmbeddedSeparator()
        {
			mockEventLogger.LogDebug(">>> Entering EmbeddedSeparator()");
            DoTest(
                new[] { "fi,rst", "second" },
                @"fi\,rst,second");
				mockEventLogger.LogDebug("<<< Exiting EmbeddedSeparator()");
        }

        [Test]
        public void DoubleSeparator()
        {
			mockEventLogger.LogDebug(">>> Entering DoubleSeparator()");
            DoTest(
                new[] { "fi,,rst", "second" },
                @"fi\,\,rst,second");
				mockEventLogger.LogDebug("<<< Exiting DoubleSeparator()");
        }

        [Test]
        public void MultipleSeparators()
        {
			mockEventLogger.LogDebug(">>> Entering MultipleSeparators()");
            DoTest(
                new[] { "fi,rst", ",second," },
                @"fi\,rst,\,second\,");
				mockEventLogger.LogDebug("<<< Exiting MultipleSeparators()");
        }

        [Test]
        public void EscapeCharacter()
        {
			mockEventLogger.LogDebug(">>> Entering EscapeCharacter()");
            DoTest(
                new[] { @"fi\rst", "second" },
                @"fi\\rst,second");
				mockEventLogger.LogDebug("<<< Exiting EscapeCharacter()");
        }

        [Test]
        public void EmbeddedEscapeSequence()
        {
			mockEventLogger.LogDebug(">>> Entering EmbeddedEscapeSequence()");
            DoTest(
                new[] { @"fi\,rst", "second" },
                @"fi\\\,rst,second");
				mockEventLogger.LogDebug("<<< Exiting EmbeddedEscapeSequence()");
        }

        [Test]
        public void CrazyContrivedStuff()
        {
			mockEventLogger.LogDebug(">>> Entering CrazyContrivedStuff()");
            DoTest(
                new[] { @"\\\,,fi\,rst,,\,\\", "second" },
                @"\\\\\\\,\,fi\\\,rst\,\,\\\,\\\\,second");
				mockEventLogger.LogDebug("<<< Exiting CrazyContrivedStuff()");
        }

        [Test]
        public void EmptyElement()
        {
			mockEventLogger.LogDebug(">>> Entering EmptyElement()");
            DoTest(
                new[] { "first", string.Empty, "third", },
                "first,,third");
				mockEventLogger.LogDebug("<<< Exiting EmptyElement()");
        }

        [Test]
        public void MultipleEmptyElements()
        {
			mockEventLogger.LogDebug(">>> Entering MultipleEmptyElements()");
            DoTest(
                new[] { string.Empty, string.Empty, string.Empty, },
                ",,");
				mockEventLogger.LogDebug("<<< Exiting MultipleEmptyElements()");
        }

        [Test]
        public void EmptyEnumerable()
        {
			mockEventLogger.LogDebug(">>> Entering EmptyEnumerable()");
            DoTest(
                Array.Empty<object>(),
                string.Empty);
				mockEventLogger.LogDebug("<<< Exiting EmptyEnumerable()");
        }

        [Test]
        public void SingleElement()
        {
			mockEventLogger.LogDebug(">>> Entering SingleElement()");
            DoTest(
                new[] { "only item here", },
                "only item here");
				mockEventLogger.LogDebug("<<< Exiting SingleElement()");
        }

        [Test]
        public void AllEscapeChars()
        {
			mockEventLogger.LogDebug(">>> Entering AllEscapeChars()");
            DoTest(
                new[] { @"\", @"\\", @"\\\" },
			mockEventLogger.LogDebug("<<< Exiting AllEscapeChars()");
                @"\\,\\\\,\\\\\\");
        }

        [Test]
        public void AllSeparatorChars()
        {
			mockEventLogger.LogDebug(">>> Entering AllSeparatorChars()");
            DoTest(
                new[] { ",", ",,", ",,," },
                @"\,,\,\,,\,\,\,");
				mockEventLogger.LogDebug("<<< Exiting AllSeparatorChars()");
        }

        [Test]
        public void AllEscapedSeparators()
        {
			mockEventLogger.LogDebug(">>> Entering AllEscapedSeparators()");
            DoTest(
                new[] { @"\,", @"\,\,", @"\,\,\," },
                @"\\\,,\\\,\\\,,\\\,\\\,\\\,");
				mockEventLogger.LogDebug("<<< Exiting AllEscapedSeparators()");
        }

        [Test]
        public void TrimWhitespaces()
        {
			mockEventLogger.LogDebug(">>> Entering TrimWhitespaces()");
            SeparateTest(
                ["item1", "ite\nm2", "item3", "item4", "item5", "item6", "item7"],
                "item1,ite\nm2,\nitem3, item4,item5\t,\r\nitem6,\vitem7",
                true);
				mockEventLogger.LogDebug("<<< Exiting TrimWhitespaces()");
        }

        [Test]
        public void KeepWhitespaces()
        {
			mockEventLogger.LogDebug(">>> Entering KeepWhitespaces()");
            SeparateTest(
                ["item1", "ite\nm2", "\nitem3", " item4", "item5\t", "\r\nitem6", "\vitem7"],
                "item1,ite\nm2,\nitem3, item4,item5\t,\r\nitem6,\vitem7",
                false);
				mockEventLogger.LogDebug("<<< Exiting KeepWhitespaces()");
        }

        private static void DoTest(IEnumerable enumerable, string s)
        {
			mockEventLogger.LogDebug(">>> Entering DoTest(enumerable={Enumerable},s={S})", enumerable, s);
            CombineTest(enumerable, s);
            SeparateTest(enumerable.Cast<string>(), s);
			mockEventLogger.LogDebug("<<< Exiting DoTest(enumerable={Enumerable},s={S})", enumerable, s);
        }

        private static void SeparateTest(IEnumerable<string> expected, string data, bool trimWhitespaces = false)
        {
			mockEventLogger.LogDebug(">>> Entering SeparateTest(expected={Expected},data={Data},trimWhitespaces={TrimWhitespaces})", expected, data, trimWhitespaces);
            var result = EscapedString.Seperate(data, trimWhitespaces);

			mockEventLogger.LogDebug("SeparateTest(expected={Expected},data={Data},trimWhitespaces={TrimWhitespaces}): result → {Result}", expected, data, trimWhitespaces, result);
            Assert.That(result, Is.EqualTo(expected).AsCollection);
			mockEventLogger.LogDebug("<<< Exiting SeparateTest(expected={Expected},data={Data},trimWhitespaces={TrimWhitespaces})", expected, data, trimWhitespaces);
        }

        private static void CombineTest(IEnumerable data, string expected)
        {
			mockEventLogger.LogDebug(">>> Entering CombineTest(data={Data},expected={Expected})", data, expected);
            string result = EscapedString.Combine(data);

			mockEventLogger.LogDebug("CombineTest(data={Data},expected={Expected}): result → {Result}", data, expected, result);
            Assert.That(result, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting CombineTest(data={Data},expected={Expected})", data, expected);
        }
    }
}
