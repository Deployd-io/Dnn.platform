// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class FriendlyUrlControllerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private Mock<CachingProvider> _mockCache;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            this._mockCache = MockComponentProvider.CreateDataCacheProvider();
			Logger.LogDebug("<<< Exiting SetUp()");
			Logger.LogDebug("SetUp(): _mockCache → {_mockCache}", _mockCache);
        }

        [Test]
        public void DoesNothingToSimpleText()
        {
			Logger.LogDebug(">>> Entering DoesNothingToSimpleText()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.False);
                Assert.That(result, Is.EqualTo("abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"));
            }
			Logger.LogDebug("<<< Exiting DoesNothingToSimpleText()");
        }

        [Test]
        public void RemoveSpace()
        {
			Logger.LogDebug(">>> Entering RemoveSpace()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("123 abc", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.False);
                Assert.That(result, Is.EqualTo("123abc"));
            }
			Logger.LogDebug("<<< Exiting RemoveSpace()");
        }

        [Test]
        public void ReplaceSpaceWithHyphen()
        {
			Logger.LogDebug(">>> Entering ReplaceSpaceWithHyphen()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("123 abc", CreateFriendlyUrlOptions(replaceSpaceWith: "-"), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.False);
                Assert.That(result, Is.EqualTo("123-abc"));
            }
			Logger.LogDebug("<<< Exiting ReplaceSpaceWithHyphen()");
        }

        [Test]
        public void RemoveApostrophe()
        {
			Logger.LogDebug(">>> Entering RemoveApostrophe()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("Fred's House", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("FredsHouse"));
            }
			Logger.LogDebug("<<< Exiting RemoveApostrophe()");
        }

        [Test]
        public void RemoveCharactersInReplaceList()
        {
			Logger.LogDebug(">>> Entering RemoveCharactersInReplaceList()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl(@"a b&c$d+e,f/g?h~i#j<k>l(m)n¿o¡p«q»r!s""t", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("abcdefghijklmnopqrst"));
            }
			Logger.LogDebug("<<< Exiting RemoveCharactersInReplaceList()");
        }

        [Test]
        public void ReplaceCharactersInReplaceList()
        {
			Logger.LogDebug(">>> Entering ReplaceCharactersInReplaceList()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl(@"a b&c$d+e,f/g?h~i#j<k>l(m)n¿o¡p«q»r!s""t", CreateFriendlyUrlOptions(replaceSpaceWith: "_"), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("a_b_c_d_e_f_g_h_i_j_k_l_m_n_o_p_q_r_s_t"));
            }
			Logger.LogDebug("<<< Exiting ReplaceCharactersInReplaceList()");
        }

        [Test]
        public void RemoveCharactersInReplaceListWhenReplacementCharacterIsNotAMatchingCharacter()
        {
			Logger.LogDebug(">>> Entering RemoveCharactersInReplaceListWhenReplacementCharacterIsNotAMatchingCharacter()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl(@"a b&c$d+e,f/g?h~i#j<k>l(m)n¿o¡p«q»r!s""t", CreateFriendlyUrlOptions(replaceSpaceWith: "."), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("abcdefghijklmnopqrst"));
            }
			Logger.LogDebug("<<< Exiting RemoveCharactersInReplaceListWhenReplacementCharacterIsNotAMatchingCharacter()");
        }

        [Test]
        public void RemovePunctuatuation()
        {
			Logger.LogDebug(">>> Entering RemovePunctuatuation()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("Dr. Cousteau, where are you?", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("DrCousteauwhereareyou"));
            }
			Logger.LogDebug("<<< Exiting RemovePunctuatuation()");
        }

        [Test]
        public void RemoveDoubleReplacements()
        {
			Logger.LogDebug(">>> Entering RemoveDoubleReplacements()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("This, .Has Lots Of---Replacements   Don't you think?", CreateFriendlyUrlOptions(replaceSpaceWith: "-"), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("This-Has-Lots-Of-Replacements-Dont-you-think"));
            }
			Logger.LogDebug("<<< Exiting RemoveDoubleReplacements()");
        }

        [Test]
        public void DoNotRemoveDoubleReplacements()
        {
			Logger.LogDebug(">>> Entering DoNotRemoveDoubleReplacements()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("This, ,Has Lots Of---Replacements   Don't you think?", CreateFriendlyUrlOptions(replaceDoubleChars: false, replaceSpaceWith: "-"), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("This---Has-Lots-Of---Replacements---Dont-you-think"));
            }
			Logger.LogDebug("<<< Exiting DoNotRemoveDoubleReplacements()");
        }

        [Test]
        public void DoNotConvertVietnameseDiacritics()
        {
			Logger.LogDebug(">>> Entering DoNotConvertVietnameseDiacritics()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("DấuNgãSắcHuyềnNặngHỏi", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.False);
                Assert.That(result, Is.EqualTo("DấuNgãSắcHuyềnNặngHỏi"));
            }
			Logger.LogDebug("<<< Exiting DoNotConvertVietnameseDiacritics()");
        }

        [Test]
        public void DoNotConvertFrenchDiacritics()
        {
			Logger.LogDebug(">>> Entering DoNotConvertFrenchDiacritics()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("CrèmeFraîcheCédille", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.False);
                Assert.That(result, Is.EqualTo("CrèmeFraîcheCédille"));
            }
			Logger.LogDebug("<<< Exiting DoNotConvertFrenchDiacritics()");
        }

        [Test]
        public void DoNotConvertRussianDiacritics()
        {
			Logger.LogDebug(">>> Entering DoNotConvertRussianDiacritics()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("писа́тьбо́льшая", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.False);
                Assert.That(result, Is.EqualTo("писа́тьбо́льшая"));
            }
			Logger.LogDebug("<<< Exiting DoNotConvertRussianDiacritics()");
        }

        [Test]
        public void DoNotConvertLeoneseDiacritics()
        {
			Logger.LogDebug(">>> Entering DoNotConvertLeoneseDiacritics()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("ñavidá", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.False);
                Assert.That(result, Is.EqualTo("ñavidá"));
            }
			Logger.LogDebug("<<< Exiting DoNotConvertLeoneseDiacritics()");
        }

        [Test]
        public void ConvertVietnameseDiacritics()
        {
			Logger.LogDebug(">>> Entering ConvertVietnameseDiacritics()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("DấuNgãSắcHuyềnNặngHỏi", CreateFriendlyUrlOptions(autoAsciiConvert: true), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("DauNgaSacHuyenNangHoi"));
            }
			Logger.LogDebug("<<< Exiting ConvertVietnameseDiacritics()");
        }

        [Test]
        public void ConvertFrenchDiacritics()
        {
			Logger.LogDebug(">>> Entering ConvertFrenchDiacritics()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("CrèmeFraîcheCédille", CreateFriendlyUrlOptions(autoAsciiConvert: true), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("CremeFraicheCedille"));
            }
			Logger.LogDebug("<<< Exiting ConvertFrenchDiacritics()");
        }

        [Test]
        public void ConvertRussianDiacritics()
        {
			Logger.LogDebug(">>> Entering ConvertRussianDiacritics()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("писа́тьбо́льшая", CreateFriendlyUrlOptions(autoAsciiConvert: true), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("писатьбольшая"));
            }
			Logger.LogDebug("<<< Exiting ConvertRussianDiacritics()");
        }

        [Test]
        public void ConvertLeoneseDiacritics()
        {
			Logger.LogDebug(">>> Entering ConvertLeoneseDiacritics()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("ñavidá", CreateFriendlyUrlOptions(autoAsciiConvert: true), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("navida"));
            }
			Logger.LogDebug("<<< Exiting ConvertLeoneseDiacritics()");
        }

        [Test]
        public void ReplaceBeforeConvertingDiacritics()
        {
			Logger.LogDebug(">>> Entering ReplaceBeforeConvertingDiacritics()");
            bool replacedUnwantedChars;
            var replacements = new Dictionary<string, string>(1) { { "ñ", "nn" }, };
            string result = FriendlyUrlController.CleanNameForUrl("Carreño", CreateFriendlyUrlOptions(replaceCharacterDictionary: replacements), out replacedUnwantedChars);
			Logger.LogDebug("ReplaceBeforeConvertingDiacritics(): replacements → {Replacements}", replacements);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("Carrenno"));
            }
			Logger.LogDebug("<<< Exiting ReplaceBeforeConvertingDiacritics()");
        }

        private static FriendlyUrlOptions CreateFriendlyUrlOptions(
            string replaceSpaceWith = FriendlyUrlSettings.ReplaceSpaceWithNothing,
            string spaceEncodingValue = FriendlyUrlSettings.SpaceEncodingHex,
            bool autoAsciiConvert = false,
            string regexMatch = @"[^\w\d _-]",
            string illegalChars = @"<>/\?:&=+|%#",
            string replaceChars = @" &$+,/?~#<>()¿¡«»!""",
            bool replaceDoubleChars = true,
            Dictionary<string, string> replaceCharacterDictionary = null,
            PageExtensionUsageType pageExtensionUsageType = PageExtensionUsageType.Never,
            string pageExtension = ".aspx")
        {
            replaceCharacterDictionary = replaceCharacterDictionary ?? new Dictionary<string, string>(0);
            return new FriendlyUrlOptions
            {
                PunctuationReplacement = (replaceSpaceWith != FriendlyUrlSettings.ReplaceSpaceWithNothing)
                                                ? replaceSpaceWith
                                                : string.Empty,
                SpaceEncoding = spaceEncodingValue,
                MaxUrlPathLength = 200,
                ConvertDiacriticChars = autoAsciiConvert,
                RegexMatch = regexMatch,
                IllegalChars = illegalChars,
                ReplaceChars = replaceChars,
                ReplaceDoubleChars = replaceDoubleChars,
                ReplaceCharWithChar = replaceCharacterDictionary,
                PageExtension = (pageExtensionUsageType == PageExtensionUsageType.Never)
                                        ? string.Empty
                                        : pageExtension,
            };
        }
    }
}
