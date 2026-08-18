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
        private Mock<CachingProvider> _mockCache;

        [SetUp]
        public void SetUp()
        {
			mockEventLogger.LogDebug(">>> Entering SetUp()");
            this._mockCache = MockComponentProvider.CreateDataCacheProvider();
			mockEventLogger.LogDebug("SetUp(): _mockCache → {_mockCache}", _mockCache);
			mockEventLogger.LogDebug("<<< Exiting SetUp()");
        }

        [Test]
        public void DoesNothingToSimpleText()
        {
			mockEventLogger.LogDebug(">>> Entering DoesNothingToSimpleText()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.False);
                Assert.That(result, Is.EqualTo("abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"));
            }
			mockEventLogger.LogDebug("<<< Exiting DoesNothingToSimpleText()");
        }

        [Test]
        public void RemoveSpace()
        {
			mockEventLogger.LogDebug(">>> Entering RemoveSpace()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("123 abc", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.False);
                Assert.That(result, Is.EqualTo("123abc"));
            }
			mockEventLogger.LogDebug("<<< Exiting RemoveSpace()");
        }

        [Test]
        public void ReplaceSpaceWithHyphen()
        {
			mockEventLogger.LogDebug(">>> Entering ReplaceSpaceWithHyphen()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("123 abc", CreateFriendlyUrlOptions(replaceSpaceWith: "-"), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.False);
                Assert.That(result, Is.EqualTo("123-abc"));
            }
			mockEventLogger.LogDebug("<<< Exiting ReplaceSpaceWithHyphen()");
        }

        [Test]
        public void RemoveApostrophe()
        {
			mockEventLogger.LogDebug(">>> Entering RemoveApostrophe()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("Fred's House", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("FredsHouse"));
            }
			mockEventLogger.LogDebug("<<< Exiting RemoveApostrophe()");
        }

        [Test]
        public void RemoveCharactersInReplaceList()
        {
			mockEventLogger.LogDebug(">>> Entering RemoveCharactersInReplaceList()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl(@"a b&c$d+e,f/g?h~i#j<k>l(m)n¿o¡p«q»r!s""t", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("abcdefghijklmnopqrst"));
            }
			mockEventLogger.LogDebug("<<< Exiting RemoveCharactersInReplaceList()");
        }

        [Test]
        public void ReplaceCharactersInReplaceList()
        {
			mockEventLogger.LogDebug(">>> Entering ReplaceCharactersInReplaceList()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl(@"a b&c$d+e,f/g?h~i#j<k>l(m)n¿o¡p«q»r!s""t", CreateFriendlyUrlOptions(replaceSpaceWith: "_"), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("a_b_c_d_e_f_g_h_i_j_k_l_m_n_o_p_q_r_s_t"));
            }
			mockEventLogger.LogDebug("<<< Exiting ReplaceCharactersInReplaceList()");
        }

        [Test]
        public void RemoveCharactersInReplaceListWhenReplacementCharacterIsNotAMatchingCharacter()
        {
			mockEventLogger.LogDebug(">>> Entering RemoveCharactersInReplaceListWhenReplacementCharacterIsNotAMatchingCharacter()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl(@"a b&c$d+e,f/g?h~i#j<k>l(m)n¿o¡p«q»r!s""t", CreateFriendlyUrlOptions(replaceSpaceWith: "."), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("abcdefghijklmnopqrst"));
            }
			mockEventLogger.LogDebug("<<< Exiting RemoveCharactersInReplaceListWhenReplacementCharacterIsNotAMatchingCharacter()");
        }

        [Test]
        public void RemovePunctuatuation()
        {
			mockEventLogger.LogDebug(">>> Entering RemovePunctuatuation()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("Dr. Cousteau, where are you?", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("DrCousteauwhereareyou"));
            }
			mockEventLogger.LogDebug("<<< Exiting RemovePunctuatuation()");
        }

        [Test]
        public void RemoveDoubleReplacements()
        {
			mockEventLogger.LogDebug(">>> Entering RemoveDoubleReplacements()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("This, .Has Lots Of---Replacements   Don't you think?", CreateFriendlyUrlOptions(replaceSpaceWith: "-"), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("This-Has-Lots-Of-Replacements-Dont-you-think"));
            }
			mockEventLogger.LogDebug("<<< Exiting RemoveDoubleReplacements()");
        }

        [Test]
        public void DoNotRemoveDoubleReplacements()
        {
			mockEventLogger.LogDebug(">>> Entering DoNotRemoveDoubleReplacements()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("This, ,Has Lots Of---Replacements   Don't you think?", CreateFriendlyUrlOptions(replaceDoubleChars: false, replaceSpaceWith: "-"), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("This---Has-Lots-Of---Replacements---Dont-you-think"));
            }
			mockEventLogger.LogDebug("<<< Exiting DoNotRemoveDoubleReplacements()");
        }

        [Test]
        public void DoNotConvertVietnameseDiacritics()
        {
			mockEventLogger.LogDebug(">>> Entering DoNotConvertVietnameseDiacritics()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("DấuNgãSắcHuyềnNặngHỏi", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.False);
                Assert.That(result, Is.EqualTo("DấuNgãSắcHuyềnNặngHỏi"));
            }
			mockEventLogger.LogDebug("<<< Exiting DoNotConvertVietnameseDiacritics()");
        }

        [Test]
        public void DoNotConvertFrenchDiacritics()
        {
			mockEventLogger.LogDebug(">>> Entering DoNotConvertFrenchDiacritics()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("CrèmeFraîcheCédille", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.False);
                Assert.That(result, Is.EqualTo("CrèmeFraîcheCédille"));
            }
			mockEventLogger.LogDebug("<<< Exiting DoNotConvertFrenchDiacritics()");
        }

        [Test]
        public void DoNotConvertRussianDiacritics()
        {
			mockEventLogger.LogDebug(">>> Entering DoNotConvertRussianDiacritics()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("писа́тьбо́льшая", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.False);
                Assert.That(result, Is.EqualTo("писа́тьбо́льшая"));
            }
			mockEventLogger.LogDebug("<<< Exiting DoNotConvertRussianDiacritics()");
        }

        [Test]
        public void DoNotConvertLeoneseDiacritics()
        {
			mockEventLogger.LogDebug(">>> Entering DoNotConvertLeoneseDiacritics()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("ñavidá", CreateFriendlyUrlOptions(), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.False);
                Assert.That(result, Is.EqualTo("ñavidá"));
            }
			mockEventLogger.LogDebug("<<< Exiting DoNotConvertLeoneseDiacritics()");
        }

        [Test]
        public void ConvertVietnameseDiacritics()
        {
			mockEventLogger.LogDebug(">>> Entering ConvertVietnameseDiacritics()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("DấuNgãSắcHuyềnNặngHỏi", CreateFriendlyUrlOptions(autoAsciiConvert: true), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("DauNgaSacHuyenNangHoi"));
            }
			mockEventLogger.LogDebug("<<< Exiting ConvertVietnameseDiacritics()");
        }

        [Test]
        public void ConvertFrenchDiacritics()
        {
			mockEventLogger.LogDebug(">>> Entering ConvertFrenchDiacritics()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("CrèmeFraîcheCédille", CreateFriendlyUrlOptions(autoAsciiConvert: true), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("CremeFraicheCedille"));
            }
			mockEventLogger.LogDebug("<<< Exiting ConvertFrenchDiacritics()");
        }

        [Test]
        public void ConvertRussianDiacritics()
        {
			mockEventLogger.LogDebug(">>> Entering ConvertRussianDiacritics()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("писа́тьбо́льшая", CreateFriendlyUrlOptions(autoAsciiConvert: true), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("писатьбольшая"));
            }
			mockEventLogger.LogDebug("<<< Exiting ConvertRussianDiacritics()");
        }

        [Test]
        public void ConvertLeoneseDiacritics()
        {
			mockEventLogger.LogDebug(">>> Entering ConvertLeoneseDiacritics()");
            bool replacedUnwantedChars;
            string result = FriendlyUrlController.CleanNameForUrl("ñavidá", CreateFriendlyUrlOptions(autoAsciiConvert: true), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("navida"));
            }
			mockEventLogger.LogDebug("<<< Exiting ConvertLeoneseDiacritics()");
        }

        [Test]
        public void ReplaceBeforeConvertingDiacritics()
        {
			mockEventLogger.LogDebug(">>> Entering ReplaceBeforeConvertingDiacritics()");
            bool replacedUnwantedChars;
            var replacements = new Dictionary<string, string>(1) { { "ñ", "nn" }, };
			mockEventLogger.LogDebug("ReplaceBeforeConvertingDiacritics(): replacements → {Replacements}", replacements);
            string result = FriendlyUrlController.CleanNameForUrl("Carreño", CreateFriendlyUrlOptions(replaceCharacterDictionary: replacements), out replacedUnwantedChars);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(replacedUnwantedChars, Is.True);
                Assert.That(result, Is.EqualTo("Carrenno"));
            }
			mockEventLogger.LogDebug("<<< Exiting ReplaceBeforeConvertingDiacritics()");
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
