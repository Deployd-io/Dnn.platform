// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy.Tests
{
    using System;

    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    /// <summary>
    /// Unit tests for the CspSourceTypeNameMapper class.
    /// </summary>
    [TestFixture]
    public class CspSourceTypeNameMapperTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>
        /// Tests GetSourceTypeName with all known source types.
        /// </summary>
        [Test]
        public void GetSourceTypeName_AllKnownTypes_ShouldReturnCorrectNames()
        {
			_logger.LogDebug(">>> Entering GetSourceTypeName_AllKnownTypes_ShouldReturnCorrectNames()");
            // Arrange & Act & Assert
            Assert.That(CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.Host), Is.EqualTo("host"));
            Assert.That(CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.Scheme), Is.EqualTo("scheme"));
            Assert.That(CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.Self), Is.EqualTo("'self'"));
            Assert.That(CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.Inline), Is.EqualTo("'unsafe-inline'"));
            Assert.That(CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.Eval), Is.EqualTo("'unsafe-eval'"));
            Assert.That(CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.Nonce), Is.EqualTo("nonce"));
            Assert.That(CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.Hash), Is.EqualTo("hash"));
            Assert.That(CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.None), Is.EqualTo("'none'"));
            Assert.That(CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.StrictDynamic), Is.EqualTo("'strict-dynamic'"));
			_logger.LogDebug("<<< Exiting GetSourceTypeName_AllKnownTypes_ShouldReturnCorrectNames()");
        }

        /// <summary>
        /// Tests GetSourceType with all known source names.
        /// </summary>
        [Test]
        public void GetSourceType_AllKnownNames_ShouldReturnCorrectTypes()
        {
			_logger.LogDebug(">>> Entering GetSourceType_AllKnownNames_ShouldReturnCorrectTypes()");
            // Arrange & Act & Assert
            Assert.That(CspSourceTypeNameMapper.GetSourceType("'self'"), Is.EqualTo(CspSourceType.Self));
            Assert.That(CspSourceTypeNameMapper.GetSourceType("'unsafe-inline'"), Is.EqualTo(CspSourceType.Inline));
            Assert.That(CspSourceTypeNameMapper.GetSourceType("'unsafe-eval'"), Is.EqualTo(CspSourceType.Eval));
            Assert.That(CspSourceTypeNameMapper.GetSourceType("'none'"), Is.EqualTo(CspSourceType.None));
            Assert.That(CspSourceTypeNameMapper.GetSourceType("'strict-dynamic'"), Is.EqualTo(CspSourceType.StrictDynamic));
			_logger.LogDebug("<<< Exiting GetSourceType_AllKnownNames_ShouldReturnCorrectTypes()");
        }

        /// <summary>
        /// Tests GetSourceType with case insensitive input.
        /// </summary>
        [Test]
        public void GetSourceType_CaseInsensitiveInput_ShouldReturnCorrectType()
        {
			_logger.LogDebug(">>> Entering GetSourceType_CaseInsensitiveInput_ShouldReturnCorrectType()");
            // Arrange & Act & Assert
            Assert.That(CspSourceTypeNameMapper.GetSourceType("'SELF'"), Is.EqualTo(CspSourceType.Self));
            Assert.That(CspSourceTypeNameMapper.GetSourceType("'Unsafe-Inline'"), Is.EqualTo(CspSourceType.Inline));
            Assert.That(CspSourceTypeNameMapper.GetSourceType("'UNSAFE-EVAL'"), Is.EqualTo(CspSourceType.Eval));
            Assert.That(CspSourceTypeNameMapper.GetSourceType("'NONE'"), Is.EqualTo(CspSourceType.None));
            Assert.That(CspSourceTypeNameMapper.GetSourceType("'STRICT-DYNAMIC'"), Is.EqualTo(CspSourceType.StrictDynamic));
			_logger.LogDebug("<<< Exiting GetSourceType_CaseInsensitiveInput_ShouldReturnCorrectType()");
        }

        /// <summary>
        /// Tests GetSourceType with unknown source name.
        /// </summary>
        [Test]
        public void GetSourceType_UnknownSourceName_ShouldThrowArgumentException()
        {
			_logger.LogDebug(">>> Entering GetSourceType_UnknownSourceName_ShouldThrowArgumentException()");
            // Arrange
            var unknownSource = "'unknown-source'";
			_logger.LogDebug("GetSourceType_UnknownSourceName_ShouldThrowArgumentException(): unknownSource → {UnknownSource}", unknownSource);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CspSourceTypeNameMapper.GetSourceType(unknownSource));
			_logger.LogDebug("GetSourceType_UnknownSourceName_ShouldThrowArgumentException(): exception → {Exception}", exception);
            Assert.That(exception.Message, Does.Contain($"Unknown source name: {unknownSource}"));
			_logger.LogDebug("<<< Exiting GetSourceType_UnknownSourceName_ShouldThrowArgumentException()");
        }

        /// <summary>
        /// Tests GetSourceType with null input.
        /// </summary>
        [Test]
        public void GetSourceType_NullInput_ShouldThrowArgumentException()
        {
			_logger.LogDebug(">>> Entering GetSourceType_NullInput_ShouldThrowArgumentException()");
            // Arrange
            string sourceName = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CspSourceTypeNameMapper.GetSourceType(sourceName));
			_logger.LogDebug("GetSourceType_NullInput_ShouldThrowArgumentException(): exception → {Exception}", exception);
            Assert.That(exception.Message, Does.Contain("Source name cannot be null or empty"));
			_logger.LogDebug("<<< Exiting GetSourceType_NullInput_ShouldThrowArgumentException()");
        }

        /// <summary>
        /// Tests GetSourceType with empty input.
        /// </summary>
        [Test]
        public void GetSourceType_EmptyInput_ShouldThrowArgumentException()
        {
			_logger.LogDebug(">>> Entering GetSourceType_EmptyInput_ShouldThrowArgumentException()");
            // Arrange
            var sourceName = string.Empty;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CspSourceTypeNameMapper.GetSourceType(sourceName));
			_logger.LogDebug("GetSourceType_EmptyInput_ShouldThrowArgumentException(): exception → {Exception}", exception);
            Assert.That(exception.Message, Does.Contain("Source name cannot be null or empty"));
			_logger.LogDebug("<<< Exiting GetSourceType_EmptyInput_ShouldThrowArgumentException()");
        }

        /// <summary>
        /// Tests TryGetSourceType with valid source names.
        /// </summary>
        [Test]
        public void TryGetSourceType_ValidSourceNames_ShouldReturnTrueAndCorrectType()
        {
			_logger.LogDebug(">>> Entering TryGetSourceType_ValidSourceNames_ShouldReturnTrueAndCorrectType()");
            // Arrange & Act & Assert
            Assert.That(CspSourceTypeNameMapper.TryGetSourceType("'self'", out var selfType), Is.True);
            Assert.That(selfType, Is.EqualTo(CspSourceType.Self));

            Assert.That(CspSourceTypeNameMapper.TryGetSourceType("'unsafe-inline'", out var inlineType), Is.True);
            Assert.That(inlineType, Is.EqualTo(CspSourceType.Inline));

            Assert.That(CspSourceTypeNameMapper.TryGetSourceType("'none'", out var noneType), Is.True);
            Assert.That(noneType, Is.EqualTo(CspSourceType.None));
			_logger.LogDebug("<<< Exiting TryGetSourceType_ValidSourceNames_ShouldReturnTrueAndCorrectType()");
        }

        /// <summary>
        /// Tests TryGetSourceType with invalid source names.
        /// </summary>
        [Test]
        public void TryGetSourceType_InvalidSourceNames_ShouldReturnFalseAndDefaultType()
        {
			_logger.LogDebug(">>> Entering TryGetSourceType_InvalidSourceNames_ShouldReturnFalseAndDefaultType()");
            // Arrange & Act & Assert
            Assert.That(CspSourceTypeNameMapper.TryGetSourceType("'unknown-source'", out var type1), Is.False);
            Assert.That(type1, Is.EqualTo(default(CspSourceType)));

            Assert.That(CspSourceTypeNameMapper.TryGetSourceType(null, out var type2), Is.False);
            Assert.That(type2, Is.EqualTo(default(CspSourceType)));

            Assert.That(CspSourceTypeNameMapper.TryGetSourceType(string.Empty, out var type3), Is.False);
            Assert.That(type3, Is.EqualTo(default(CspSourceType)));

            Assert.That(CspSourceTypeNameMapper.TryGetSourceType("example.com", out var type4), Is.False);
            Assert.That(type4, Is.EqualTo(default(CspSourceType)));
			_logger.LogDebug("<<< Exiting TryGetSourceType_InvalidSourceNames_ShouldReturnFalseAndDefaultType()");
        }

        /// <summary>
        /// Tests IsQuotedKeyword with various inputs.
        /// </summary>
        [Test]
        public void IsQuotedKeyword_VariousInputs_ShouldReturnCorrectResults()
        {
			_logger.LogDebug(">>> Entering IsQuotedKeyword_VariousInputs_ShouldReturnCorrectResults()");
            // Arrange & Act & Assert
            Assert.That(CspSourceTypeNameMapper.IsQuotedKeyword("'self'"), Is.True);
            Assert.That(CspSourceTypeNameMapper.IsQuotedKeyword("'unsafe-inline'"), Is.True);
            Assert.That(CspSourceTypeNameMapper.IsQuotedKeyword("'none'"), Is.True);
            Assert.That(CspSourceTypeNameMapper.IsQuotedKeyword("'nonce-abc123'"), Is.True);
            Assert.That(CspSourceTypeNameMapper.IsQuotedKeyword("'sha256-abc123'"), Is.True);

            Assert.That(CspSourceTypeNameMapper.IsQuotedKeyword("example.com"), Is.False);
            Assert.That(CspSourceTypeNameMapper.IsQuotedKeyword("https:"), Is.False);
            Assert.That(CspSourceTypeNameMapper.IsQuotedKeyword("self"), Is.False); // Missing quotes
            Assert.That(CspSourceTypeNameMapper.IsQuotedKeyword("'self"), Is.False); // Missing closing quote
            Assert.That(CspSourceTypeNameMapper.IsQuotedKeyword("self'"), Is.False); // Missing opening quote
            Assert.That(CspSourceTypeNameMapper.IsQuotedKeyword(null), Is.False);
            Assert.That(CspSourceTypeNameMapper.IsQuotedKeyword(string.Empty), Is.False);
            Assert.That(CspSourceTypeNameMapper.IsQuotedKeyword("   "), Is.False);
			_logger.LogDebug("<<< Exiting IsQuotedKeyword_VariousInputs_ShouldReturnCorrectResults()");
        }

        /// <summary>
        /// Tests IsNonceSource with various inputs.
        /// </summary>
        [Test]
        public void IsNonceSource_VariousInputs_ShouldReturnCorrectResults()
        {
			_logger.LogDebug(">>> Entering IsNonceSource_VariousInputs_ShouldReturnCorrectResults()");
            // Arrange & Act & Assert
            Assert.That(CspSourceTypeNameMapper.IsNonceSource("'nonce-abc123'"), Is.True);
            Assert.That(CspSourceTypeNameMapper.IsNonceSource("'nonce-xyz789def'"), Is.True);
            Assert.That(CspSourceTypeNameMapper.IsNonceSource("'nonce-'"), Is.True); // Edge case: empty nonce value

            Assert.That(CspSourceTypeNameMapper.IsNonceSource("'self'"), Is.False);
            Assert.That(CspSourceTypeNameMapper.IsNonceSource("'unsafe-inline'"), Is.False);
            Assert.That(CspSourceTypeNameMapper.IsNonceSource("'sha256-abc123'"), Is.False);
            Assert.That(CspSourceTypeNameMapper.IsNonceSource("nonce-abc123"), Is.False); // Missing quotes
            Assert.That(CspSourceTypeNameMapper.IsNonceSource("'nonce-abc123"), Is.False); // Missing closing quote
            Assert.That(CspSourceTypeNameMapper.IsNonceSource("nonce-abc123'"), Is.False); // Missing opening quote
            Assert.That(CspSourceTypeNameMapper.IsNonceSource("example.com"), Is.False);
            Assert.That(CspSourceTypeNameMapper.IsNonceSource(null), Is.False);
            Assert.That(CspSourceTypeNameMapper.IsNonceSource(string.Empty), Is.False);
            Assert.That(CspSourceTypeNameMapper.IsNonceSource("   "), Is.False);
			_logger.LogDebug("<<< Exiting IsNonceSource_VariousInputs_ShouldReturnCorrectResults()");
        }

        /// <summary>
        /// Tests IsHashSource with various inputs.
        /// </summary>
        [Test]
        public void IsHashSource_VariousInputs_ShouldReturnCorrectResults()
        {
			_logger.LogDebug(">>> Entering IsHashSource_VariousInputs_ShouldReturnCorrectResults()");
            // Arrange & Act & Assert
            Assert.That(CspSourceTypeNameMapper.IsHashSource("'sha256-abc123'"), Is.True);
            Assert.That(CspSourceTypeNameMapper.IsHashSource("'sha384-def456'"), Is.True);
            Assert.That(CspSourceTypeNameMapper.IsHashSource("'sha512-ghi789'"), Is.True);

            Assert.That(CspSourceTypeNameMapper.IsHashSource("'self'"), Is.False);
            Assert.That(CspSourceTypeNameMapper.IsHashSource("'unsafe-inline'"), Is.False);
            Assert.That(CspSourceTypeNameMapper.IsHashSource("'nonce-abc123'"), Is.False);
            Assert.That(CspSourceTypeNameMapper.IsHashSource("sha256-abc123"), Is.False); // Missing quotes
            Assert.That(CspSourceTypeNameMapper.IsHashSource("'sha256-abc123"), Is.False); // Missing closing quote
            Assert.That(CspSourceTypeNameMapper.IsHashSource("sha256-abc123'"), Is.False); // Missing opening quote
            Assert.That(CspSourceTypeNameMapper.IsHashSource("'md5-abc123'"), Is.False); // Unsupported hash algorithm
            Assert.That(CspSourceTypeNameMapper.IsHashSource("example.com"), Is.False);
            Assert.That(CspSourceTypeNameMapper.IsHashSource(null), Is.False);
            Assert.That(CspSourceTypeNameMapper.IsHashSource(string.Empty), Is.False);
            Assert.That(CspSourceTypeNameMapper.IsHashSource("   "), Is.False);
			_logger.LogDebug("<<< Exiting IsHashSource_VariousInputs_ShouldReturnCorrectResults()");
        }

        /// <summary>
        /// Tests round-trip conversion for supported source types.
        /// </summary>
        [Test]
        public void RoundTripConversion_SupportedSourceTypes_ShouldReturnOriginalType()
        {
			_logger.LogDebug(">>> Entering RoundTripConversion_SupportedSourceTypes_ShouldReturnOriginalType()");
            // Note: Only testing source types that have direct string representations
            var supportedTypes = new[]
            {
                CspSourceType.Self,
                CspSourceType.Inline,
                CspSourceType.Eval,
                CspSourceType.None,
                CspSourceType.StrictDynamic,
            };

            // Act & Assert
            foreach (var originalType in supportedTypes)
            {
                var sourceName = CspSourceTypeNameMapper.GetSourceTypeName(originalType);
				_logger.LogDebug("RoundTripConversion_SupportedSourceTypes_ShouldReturnOriginalType(): sourceName → {SourceName}", sourceName);
                var convertedType = CspSourceTypeNameMapper.GetSourceType(sourceName);
				_logger.LogDebug("RoundTripConversion_SupportedSourceTypes_ShouldReturnOriginalType(): convertedType → {ConvertedType}", convertedType);
                Assert.That(convertedType, Is.EqualTo(originalType), $"Round-trip conversion failed for {originalType}");
            }
			_logger.LogDebug("<<< Exiting RoundTripConversion_SupportedSourceTypes_ShouldReturnOriginalType()");
        }

        /// <summary>
        /// Tests that non-quoted source types throw exceptions when passed to GetSourceType.
        /// </summary>
        [Test]
        public void GetSourceType_NonQuotedSourceTypes_ShouldThrowException()
        {
			_logger.LogDebug(">>> Entering GetSourceType_NonQuotedSourceTypes_ShouldThrowException()");
            // These types don't have direct string representations that can be parsed back
            var nonQuotedTypes = new[]
            {
                CspSourceType.Host,
                CspSourceType.Scheme,
                CspSourceType.Nonce,
                CspSourceType.Hash,
            };

            foreach (var sourceType in nonQuotedTypes)
            {
                var sourceName = CspSourceTypeNameMapper.GetSourceTypeName(sourceType);
				_logger.LogDebug("GetSourceType_NonQuotedSourceTypes_ShouldThrowException(): sourceName → {SourceName}", sourceName);

                // These should throw exceptions when trying to parse them back
                Assert.Throws<ArgumentException>(() => CspSourceTypeNameMapper.GetSourceType(sourceName));
            }
			_logger.LogDebug("<<< Exiting GetSourceType_NonQuotedSourceTypes_ShouldThrowException()");
        }
    }
}
