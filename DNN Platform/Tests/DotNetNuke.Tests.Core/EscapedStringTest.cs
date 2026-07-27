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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [Test]
        public void SimpleCase()
        {
			Logger.LogDebug(">>> Entering SimpleCase()");
            DoTest(
                new[] { "first", "second", },
                "first,second");
				Logger.LogDebug("<<< Exiting SimpleCase()");
        }

        [Test]
        public void CombinesWithSpecifiedSeparator()
        {
			Logger.LogDebug(">>> Entering CombinesWithSpecifiedSeparator()");
            Assert.That(
                EscapedString.Combine(new[] { "first", "second", }, ';'),
                Is.EqualTo("first;second"));
				Logger.LogDebug("<<< Exiting CombinesWithSpecifiedSeparator()");
        }

        [Test]
        public void SeparatesWithSpecifiedSeparator()
        {
			Logger.LogDebug(">>> Entering SeparatesWithSpecifiedSeparator()");
            Assert.That(
                EscapedString.Seperate("first]second", ']'),
                Is.EqualTo(["first", "second"]).AsCollection);
				Logger.LogDebug("<<< Exiting SeparatesWithSpecifiedSeparator()");
        }

        [Test]
        public void EmbeddedSeparator()
        {
			Logger.LogDebug(">>> Entering EmbeddedSeparator()");
            DoTest(
                new[] { "fi,rst", "second" },
                @"fi\,rst,second");
				Logger.LogDebug("<<< Exiting EmbeddedSeparator()");
        }

        [Test]
        public void DoubleSeparator()
        {
			Logger.LogDebug(">>> Entering DoubleSeparator()");
            DoTest(
                new[] { "fi,,rst", "second" },
                @"fi\,\,rst,second");
				Logger.LogDebug("<<< Exiting DoubleSeparator()");
        }

        [Test]
        public void MultipleSeparators()
        {
			Logger.LogDebug(">>> Entering MultipleSeparators()");
            DoTest(
                new[] { "fi,rst", ",second," },
                @"fi\,rst,\,second\,");
				Logger.LogDebug("<<< Exiting MultipleSeparators()");
        }

        [Test]
        public void EscapeCharacter()
        {
			Logger.LogDebug(">>> Entering EscapeCharacter()");
            DoTest(
                new[] { @"fi\rst", "second" },
                @"fi\\rst,second");
				Logger.LogDebug("<<< Exiting EscapeCharacter()");
        }

        [Test]
        public void EmbeddedEscapeSequence()
        {
			Logger.LogDebug(">>> Entering EmbeddedEscapeSequence()");
            DoTest(
                new[] { @"fi\,rst", "second" },
                @"fi\\\,rst,second");
				Logger.LogDebug("<<< Exiting EmbeddedEscapeSequence()");
        }

        [Test]
        public void CrazyContrivedStuff()
        {
			Logger.LogDebug(">>> Entering CrazyContrivedStuff()");
            DoTest(
                new[] { @"\\\,,fi\,rst,,\,\\", "second" },
                @"\\\\\\\,\,fi\\\,rst\,\,\\\,\\\\,second");
        }

		Logger.LogDebug("<<< Exiting CrazyContrivedStuff()");
        [Test]
        public void EmptyElement()
        {
			Logger.LogDebug(">>> Entering EmptyElement()");
            DoTest(
                new[] { "first", string.Empty, "third", },
                "first,,third");
				Logger.LogDebug("<<< Exiting EmptyElement()");
        }

        [Test]
        public void MultipleEmptyElements()
        {
			Logger.LogDebug(">>> Entering MultipleEmptyElements()");
            DoTest(
                new[] { string.Empty, string.Empty, string.Empty, },
                ",,");
				Logger.LogDebug("<<< Exiting MultipleEmptyElements()");
        }

        [Test]
        public void EmptyEnumerable()
        {
			Logger.LogDebug(">>> Entering EmptyEnumerable()");
            DoTest(
                Array.Empty<object>(),
                string.Empty);
				Logger.LogDebug("<<< Exiting EmptyEnumerable()");
        }

        [Test]
        public void SingleElement()
        {
			Logger.LogDebug(">>> Entering SingleElement()");
            DoTest(
                new[] { "only item here", },
                "only item here");
				Logger.LogDebug("<<< Exiting SingleElement()");
        }

        [Test]
        public void AllEscapeChars()
        {
			Logger.LogDebug(">>> Entering AllEscapeChars()");
            DoTest(
                new[] { @"\", @"\\", @"\\\" },
                @"\\,\\\\,\\\\\\");
        }

		Logger.LogDebug("<<< Exiting AllEscapeChars()");
        [Test]
        public void AllSeparatorChars()
        {
			Logger.LogDebug(">>> Entering AllSeparatorChars()");
            DoTest(
                new[] { ",", ",,", ",,," },
                @"\,,\,\,,\,\,\,");
				Logger.LogDebug("<<< Exiting AllSeparatorChars()");
        }

        [Test]
        public void AllEscapedSeparators()
        {
			Logger.LogDebug(">>> Entering AllEscapedSeparators()");
            DoTest(
                new[] { @"\,", @"\,\,", @"\,\,\," },
                @"\\\,,\\\,\\\,,\\\,\\\,\\\,");
				Logger.LogDebug("<<< Exiting AllEscapedSeparators()");
        }

        [Test]
        public void TrimWhitespaces()
        {
			Logger.LogDebug(">>> Entering TrimWhitespaces()");
            SeparateTest(
                ["item1", "ite\nm2", "item3", "item4", "item5", "item6", "item7"],
                "item1,ite\nm2,\nitem3, item4,item5\t,\r\nitem6,\vitem7",
                true);
				Logger.LogDebug("<<< Exiting TrimWhitespaces()");
        }

        [Test]
        public void KeepWhitespaces()
        {
			Logger.LogDebug(">>> Entering KeepWhitespaces()");
            SeparateTest(
                ["item1", "ite\nm2", "\nitem3", " item4", "item5\t", "\r\nitem6", "\vitem7"],
                "item1,ite\nm2,\nitem3, item4,item5\t,\r\nitem6,\vitem7",
                false);
				Logger.LogDebug("<<< Exiting KeepWhitespaces()");
        }

        private static void DoTest(IEnumerable enumerable, string s)
        {
			Logger.LogDebug(">>> Entering DoTest(enumerable={Enumerable},s={S})", enumerable, s);
            CombineTest(enumerable, s);
            SeparateTest(enumerable.Cast<string>(), s);
			Logger.LogDebug("<<< Exiting DoTest(enumerable={Enumerable},s={S})", enumerable, s);
        }

        private static void SeparateTest(IEnumerable<string> expected, string data, bool trimWhitespaces = false)
        {
			Logger.LogDebug(">>> Entering SeparateTest(expected={Expected},data={Data},trimWhitespaces={TrimWhitespaces})", expected, data, trimWhitespaces);
            var result = EscapedString.Seperate(data, trimWhitespaces);

			Logger.LogDebug("SeparateTest(expected={Expected},data={Data},trimWhitespaces={TrimWhitespaces}): result → {Result}", expected, data, trimWhitespaces, result);
            Assert.That(result, Is.EqualTo(expected).AsCollection);
			Logger.LogDebug("<<< Exiting SeparateTest(expected={Expected},data={Data},trimWhitespaces={TrimWhitespaces})", expected, data, trimWhitespaces);
        }

        private static void CombineTest(IEnumerable data, string expected)
        {
			Logger.LogDebug(">>> Entering CombineTest(data={Data},expected={Expected})", data, expected);
            string result = EscapedString.Combine(data);

			Logger.LogDebug("CombineTest(data={Data},expected={Expected}): result → {Result}", data, expected, result);
            Assert.That(result, Is.EqualTo(expected));
			Logger.LogDebug("<<< Exiting CombineTest(data={Data},expected={Expected})", data, expected);
        }
    }
}
