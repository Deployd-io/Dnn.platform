// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy.Tests
{
    using System;
    using System.Linq;

    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    /// <summary>
    /// Unit tests for the ContentSecurityPolicyParser class using instance-based approach.
    /// </summary>
    [TestFixture]
    public class ContentSecurityPolicyParserTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>
        /// Tests parsing of a basic CSP policy.
        /// </summary>
        [Test]
        public void Parse_BasicPolicy_ShouldReturnValidPolicy()
        {
			_logger.LogDebug(">>> Entering Parse_BasicPolicy_ShouldReturnValidPolicy()");
            // Arrange
            var cspHeader = "default-src 'self'";
			_logger.LogDebug("Parse_BasicPolicy_ShouldReturnValidPolicy(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_BasicPolicy_ShouldReturnValidPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_BasicPolicy_ShouldReturnValidPolicy(): parser → {Parser}", parser);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            Assert.That(policy.GeneratePolicy(), Is.EqualTo("default-src 'self'"));
			_logger.LogDebug("<<< Exiting Parse_BasicPolicy_ShouldReturnValidPolicy()");
        }

        /// <summary>
        /// Tests parsing of a policy with multiple sources.
        /// </summary>
        [Test]
        public void Parse_PolicyWithMultipleSources_ShouldReturnValidPolicy()
        {
			_logger.LogDebug(">>> Entering Parse_PolicyWithMultipleSources_ShouldReturnValidPolicy()");
            // Arrange
            var cspHeader = "script-src 'self' 'unsafe-inline' https://cdn.example.com";
			_logger.LogDebug("Parse_PolicyWithMultipleSources_ShouldReturnValidPolicy(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_PolicyWithMultipleSources_ShouldReturnValidPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_PolicyWithMultipleSources_ShouldReturnValidPolicy(): parser → {Parser}", parser);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            var generatedPolicy = policy.GeneratePolicy();
			_logger.LogDebug("Parse_PolicyWithMultipleSources_ShouldReturnValidPolicy(): generatedPolicy → {GeneratedPolicy}", generatedPolicy);
            Assert.That(generatedPolicy, Does.Contain("script-src"));
            Assert.That(generatedPolicy, Does.Contain("'self'"));
            Assert.That(generatedPolicy, Does.Contain("'unsafe-inline'"));
            Assert.That(generatedPolicy, Does.Contain("https://cdn.example.com"));
			_logger.LogDebug("<<< Exiting Parse_PolicyWithMultipleSources_ShouldReturnValidPolicy()");
        }

        /// <summary>
        /// Tests parsing of a policy with nonce values.
        /// </summary>
        [Test]
        public void Parse_PolicyWithNonce_ShouldReturnValidPolicy()
        {
			_logger.LogDebug(">>> Entering Parse_PolicyWithNonce_ShouldReturnValidPolicy()");
            // Arrange
            var cspHeader = "script-src 'self' 'nonce-abc123def456'";
			_logger.LogDebug("Parse_PolicyWithNonce_ShouldReturnValidPolicy(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_PolicyWithNonce_ShouldReturnValidPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_PolicyWithNonce_ShouldReturnValidPolicy(): parser → {Parser}", parser);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            var generatedPolicy = policy.GeneratePolicy();
			_logger.LogDebug("Parse_PolicyWithNonce_ShouldReturnValidPolicy(): generatedPolicy → {GeneratedPolicy}", generatedPolicy);
            Assert.That(generatedPolicy, Does.Contain("script-src"));
            Assert.That(generatedPolicy, Does.Contain("'self'"));
            Assert.That(generatedPolicy, Does.Contain("'nonce-abc123def456'"));
			_logger.LogDebug("<<< Exiting Parse_PolicyWithNonce_ShouldReturnValidPolicy()");
        }

        /// <summary>
        /// Tests parsing of a policy with hash values.
        /// </summary>
        [Test]
        public void Parse_PolicyWithHash_ShouldReturnValidPolicy()
        {
			_logger.LogDebug(">>> Entering Parse_PolicyWithHash_ShouldReturnValidPolicy()");
            // Arrange
            var cspHeader = "style-src 'self' 'sha256-abc123def456789'";
			_logger.LogDebug("Parse_PolicyWithHash_ShouldReturnValidPolicy(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_PolicyWithHash_ShouldReturnValidPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_PolicyWithHash_ShouldReturnValidPolicy(): parser → {Parser}", parser);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            var generatedPolicy = policy.GeneratePolicy();
			_logger.LogDebug("Parse_PolicyWithHash_ShouldReturnValidPolicy(): generatedPolicy → {GeneratedPolicy}", generatedPolicy);
            Assert.That(generatedPolicy, Does.Contain("style-src"));
            Assert.That(generatedPolicy, Does.Contain("'self'"));
            Assert.That(generatedPolicy, Does.Contain("'sha256-abc123def456789'"));
			_logger.LogDebug("<<< Exiting Parse_PolicyWithHash_ShouldReturnValidPolicy()");
        }

        /// <summary>
        /// Tests parsing of a complex policy from the example.
        /// </summary>
        [Test]
        public void Parse_ComplexPolicy_ShouldReturnValidPolicy()
        {
			_logger.LogDebug(">>> Entering Parse_ComplexPolicy_ShouldReturnValidPolicy()");
            // Arrange
            var cspHeader = "default-src 'self'; script-src 'self' 'strict-dynamic'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:; connect-src 'self' wss:; font-src 'self' https://fonts.googleapis.com; frame-ancestors 'none'; upgrade-insecure-requests; report-uri http://csp-report";
			_logger.LogDebug("Parse_ComplexPolicy_ShouldReturnValidPolicy(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_ComplexPolicy_ShouldReturnValidPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_ComplexPolicy_ShouldReturnValidPolicy(): parser → {Parser}", parser);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            var generatedPolicy = policy.GeneratePolicy();
			_logger.LogDebug("Parse_ComplexPolicy_ShouldReturnValidPolicy(): generatedPolicy → {GeneratedPolicy}", generatedPolicy);

            // Check that all expected directives are present
            Assert.That(generatedPolicy, Does.Contain("default-src 'self'"));
            Assert.That(generatedPolicy, Does.Contain("script-src"));
            Assert.That(generatedPolicy, Does.Contain("'strict-dynamic'"));
            Assert.That(generatedPolicy, Does.Contain("style-src"));
            Assert.That(generatedPolicy, Does.Contain("'unsafe-inline'"));
            Assert.That(generatedPolicy, Does.Contain("img-src"));
            Assert.That(generatedPolicy, Does.Contain("data:"));
            Assert.That(generatedPolicy, Does.Contain("blob:"));
            Assert.That(generatedPolicy, Does.Contain("connect-src"));
            Assert.That(generatedPolicy, Does.Contain("wss:"));
            Assert.That(generatedPolicy, Does.Contain("font-src"));
            Assert.That(generatedPolicy, Does.Contain("https://fonts.googleapis.com"));
            Assert.That(generatedPolicy, Does.Contain("frame-ancestors 'none'"));
            Assert.That(generatedPolicy, Does.Contain("upgrade-insecure-requests"));
			_logger.LogDebug("<<< Exiting Parse_ComplexPolicy_ShouldReturnValidPolicy()");
        }

        /// <summary>
        /// Tests parsing of a policy with sandbox directive.
        /// </summary>
        [Test]
        public void Parse_PolicyWithSandbox_ShouldReturnValidPolicy()
        {
			_logger.LogDebug(">>> Entering Parse_PolicyWithSandbox_ShouldReturnValidPolicy()");
            // Arrange
            var cspHeader = "sandbox allow-forms allow-scripts; script-src 'self'";
			_logger.LogDebug("Parse_PolicyWithSandbox_ShouldReturnValidPolicy(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_PolicyWithSandbox_ShouldReturnValidPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_PolicyWithSandbox_ShouldReturnValidPolicy(): parser → {Parser}", parser);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            var generatedPolicy = policy.GeneratePolicy();
			_logger.LogDebug("Parse_PolicyWithSandbox_ShouldReturnValidPolicy(): generatedPolicy → {GeneratedPolicy}", generatedPolicy);
            Assert.That(generatedPolicy, Does.Contain("sandbox"));
            Assert.That(generatedPolicy, Does.Contain("allow-forms"));
            Assert.That(generatedPolicy, Does.Contain("allow-scripts"));
            Assert.That(generatedPolicy, Does.Contain("script-src 'self'"));
			_logger.LogDebug("<<< Exiting Parse_PolicyWithSandbox_ShouldReturnValidPolicy()");
        }

        /// <summary>
        /// Tests parsing of a policy with form-action directive.
        /// </summary>
        [Test]
        public void Parse_PolicyWithFormAction_ShouldReturnValidPolicy()
        {
			_logger.LogDebug(">>> Entering Parse_PolicyWithFormAction_ShouldReturnValidPolicy()");
            // Arrange
            var cspHeader = "form-action 'self' https://secure.example.com";
			_logger.LogDebug("Parse_PolicyWithFormAction_ShouldReturnValidPolicy(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_PolicyWithFormAction_ShouldReturnValidPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_PolicyWithFormAction_ShouldReturnValidPolicy(): parser → {Parser}", parser);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            var generatedPolicy = policy.GeneratePolicy();
			_logger.LogDebug("Parse_PolicyWithFormAction_ShouldReturnValidPolicy(): generatedPolicy → {GeneratedPolicy}", generatedPolicy);
            Assert.That(generatedPolicy, Does.Contain("form-action"));
            Assert.That(generatedPolicy, Does.Contain("'self'"));
            Assert.That(generatedPolicy, Does.Contain("https://secure.example.com"));
			_logger.LogDebug("<<< Exiting Parse_PolicyWithFormAction_ShouldReturnValidPolicy()");
        }

        /// <summary>
        /// Tests parsing of the real-world complex policy from the example.
        /// </summary>
        [Test]
        public void Parse_RealWorldComplexPolicy_ShouldReturnValidPolicy()
        {
			_logger.LogDebug(">>> Entering Parse_RealWorldComplexPolicy_ShouldReturnValidPolicy()");
            // Arrange
            var cspHeader = "default-src 'self'; img-src 'self' https://front.satrabel.be https://www.googletagmanager.com https://region1.google-analytics.com; font-src 'self' https://fonts.gstatic.com; style-src 'self' https://fonts.googleapis.com https://www.googletagmanager.com; frame-ancestors 'self'; frame-src 'self'; form-action 'self'; object-src 'none'; base-uri 'self'; script-src 'nonce-hq9CE6VltPZiiySID0F9914GvPObOnIAN3Qs/0R+AmQ=' 'strict-dynamic'; report-to csp-endpoint; report-uri https://dnncore.satrabel.be/DesktopModules/Csp/Report; connect-src https://www.googletagmanager.com https://region1.google-analytics.com https://www.google-analytics.com; upgrade-insecure-requests";
			_logger.LogDebug("Parse_RealWorldComplexPolicy_ShouldReturnValidPolicy(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_RealWorldComplexPolicy_ShouldReturnValidPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_RealWorldComplexPolicy_ShouldReturnValidPolicy(): parser → {Parser}", parser);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            var generatedPolicy = policy.GeneratePolicy();
			_logger.LogDebug("Parse_RealWorldComplexPolicy_ShouldReturnValidPolicy(): generatedPolicy → {GeneratedPolicy}", generatedPolicy);

            // Check key directives
            Assert.That(generatedPolicy, Does.Contain("default-src 'self'"));
            Assert.That(generatedPolicy, Does.Contain("img-src"));
            Assert.That(generatedPolicy, Does.Contain("https://front.satrabel.be"));
            Assert.That(generatedPolicy, Does.Contain("font-src"));
            Assert.That(generatedPolicy, Does.Contain("https://fonts.gstatic.com"));
            Assert.That(generatedPolicy, Does.Contain("style-src"));
            Assert.That(generatedPolicy, Does.Contain("https://fonts.googleapis.com"));
            Assert.That(generatedPolicy, Does.Contain("frame-ancestors 'self'"));
            Assert.That(generatedPolicy, Does.Contain("frame-src 'self'"));
            Assert.That(generatedPolicy, Does.Contain("form-action 'self'"));
            Assert.That(generatedPolicy, Does.Contain("object-src 'none'"));
            Assert.That(generatedPolicy, Does.Contain("base-uri 'self'"));
            Assert.That(generatedPolicy, Does.Contain("script-src"));
            Assert.That(generatedPolicy, Does.Contain("'nonce-hq9CE6VltPZiiySID0F9914GvPObOnIAN3Qs/0R+AmQ='"));
            Assert.That(generatedPolicy, Does.Contain("'strict-dynamic'"));
            Assert.That(generatedPolicy, Does.Contain("connect-src"));
            Assert.That(generatedPolicy, Does.Contain("https://www.googletagmanager.com"));
            Assert.That(generatedPolicy, Does.Contain("upgrade-insecure-requests"));
			_logger.LogDebug("<<< Exiting Parse_RealWorldComplexPolicy_ShouldReturnValidPolicy()");
        }

        /// <summary>
        /// Tests TryParse with valid input.
        /// </summary>
        [Test]
        public void TryParse_ValidInput_ShouldReturnTrueAndPolicy()
        {
			_logger.LogDebug(">>> Entering TryParse_ValidInput_ShouldReturnTrueAndPolicy()");
            // Arrange
            var cspHeader = "default-src 'self'";
			_logger.LogDebug("TryParse_ValidInput_ShouldReturnTrueAndPolicy(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("TryParse_ValidInput_ShouldReturnTrueAndPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("TryParse_ValidInput_ShouldReturnTrueAndPolicy(): parser → {Parser}", parser);

            // Act
            var result = parser.TryParse(cspHeader);
			_logger.LogDebug("TryParse_ValidInput_ShouldReturnTrueAndPolicy(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(policy, Is.Not.Null);
            Assert.That(policy.GeneratePolicy(), Is.EqualTo("default-src 'self'"));
			_logger.LogDebug("<<< Exiting TryParse_ValidInput_ShouldReturnTrueAndPolicy()");
        }

        /// <summary>
        /// Tests TryParse with invalid input.
        /// </summary>
        [Test]
        public void TryParse_InvalidInput_ShouldReturnFalse()
        {
			_logger.LogDebug(">>> Entering TryParse_InvalidInput_ShouldReturnFalse()");
            // Arrange
            var cspHeader = string.Empty;
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("TryParse_InvalidInput_ShouldReturnFalse(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("TryParse_InvalidInput_ShouldReturnFalse(): parser → {Parser}", parser);

            // Act
            var result = parser.TryParse(cspHeader);
			_logger.LogDebug("TryParse_InvalidInput_ShouldReturnFalse(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.False);
			_logger.LogDebug("<<< Exiting TryParse_InvalidInput_ShouldReturnFalse()");
        }

        /// <summary>
        /// Tests Parse with null input should throw exception.
        /// </summary>
        [Test]
        public void Parse_NullInput_ShouldThrowArgumentException()
        {
			_logger.LogDebug(">>> Entering Parse_NullInput_ShouldThrowArgumentException()");
            // Arrange
            string cspHeader = null;
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_NullInput_ShouldThrowArgumentException(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_NullInput_ShouldThrowArgumentException(): parser → {Parser}", parser);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => parser.Parse(cspHeader));
			_logger.LogDebug("Parse_NullInput_ShouldThrowArgumentException(): exception → {Exception}", exception);
            Assert.That(exception.Message, Does.Contain("CSP header cannot be null or empty"));
			_logger.LogDebug("<<< Exiting Parse_NullInput_ShouldThrowArgumentException()");
        }

        /// <summary>
        /// Tests Parse with empty input should throw exception.
        /// </summary>
        [Test]
        public void Parse_EmptyInput_ShouldThrowArgumentException()
        {
			_logger.LogDebug(">>> Entering Parse_EmptyInput_ShouldThrowArgumentException()");
            // Arrange
            var cspHeader = string.Empty;
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_EmptyInput_ShouldThrowArgumentException(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_EmptyInput_ShouldThrowArgumentException(): parser → {Parser}", parser);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => parser.Parse(cspHeader));
			_logger.LogDebug("Parse_EmptyInput_ShouldThrowArgumentException(): exception → {Exception}", exception);
            Assert.That(exception.Message, Does.Contain("CSP header cannot be null or empty"));
			_logger.LogDebug("<<< Exiting Parse_EmptyInput_ShouldThrowArgumentException()");
        }

        /// <summary>
        /// Tests Parse with whitespace-only input should throw exception.
        /// </summary>
        [Test]
        public void Parse_WhitespaceOnlyInput_ShouldThrowArgumentException()
        {
			_logger.LogDebug(">>> Entering Parse_WhitespaceOnlyInput_ShouldThrowArgumentException()");
            // Arrange
            var cspHeader = "   ";
			_logger.LogDebug("Parse_WhitespaceOnlyInput_ShouldThrowArgumentException(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_WhitespaceOnlyInput_ShouldThrowArgumentException(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_WhitespaceOnlyInput_ShouldThrowArgumentException(): parser → {Parser}", parser);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => parser.Parse(cspHeader));
			_logger.LogDebug("Parse_WhitespaceOnlyInput_ShouldThrowArgumentException(): exception → {Exception}", exception);
            Assert.That(exception.Message, Does.Contain("CSP header cannot be null or empty"));
			_logger.LogDebug("<<< Exiting Parse_WhitespaceOnlyInput_ShouldThrowArgumentException()");
        }

        /// <summary>
        /// Tests parsing with various scheme sources.
        /// </summary>
        [Test]
        public void Parse_PolicyWithSchemes_ShouldReturnValidPolicy()
        {
			_logger.LogDebug(">>> Entering Parse_PolicyWithSchemes_ShouldReturnValidPolicy()");
            // Arrange
            var cspHeader = "img-src 'self' data: https: blob: filesystem:";
			_logger.LogDebug("Parse_PolicyWithSchemes_ShouldReturnValidPolicy(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_PolicyWithSchemes_ShouldReturnValidPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_PolicyWithSchemes_ShouldReturnValidPolicy(): parser → {Parser}", parser);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            var generatedPolicy = policy.GeneratePolicy();
			_logger.LogDebug("Parse_PolicyWithSchemes_ShouldReturnValidPolicy(): generatedPolicy → {GeneratedPolicy}", generatedPolicy);
            Assert.That(generatedPolicy, Does.Contain("img-src"));
            Assert.That(generatedPolicy, Does.Contain("'self'"));
            Assert.That(generatedPolicy, Does.Contain("data:"));
            Assert.That(generatedPolicy, Does.Contain("https:"));
            Assert.That(generatedPolicy, Does.Contain("blob:"));
            Assert.That(generatedPolicy, Does.Contain("filesystem:"));
			_logger.LogDebug("<<< Exiting Parse_PolicyWithSchemes_ShouldReturnValidPolicy()");
        }

        /// <summary>
        /// Tests parsing with various hash algorithms.
        /// </summary>
        [Test]
        public void Parse_PolicyWithDifferentHashAlgorithms_ShouldReturnValidPolicy()
        {
			_logger.LogDebug(">>> Entering Parse_PolicyWithDifferentHashAlgorithms_ShouldReturnValidPolicy()");
            // Arrange
            var cspHeader = "script-src 'sha256-abc123' 'sha384-def456' 'sha512-ghi789'";
			_logger.LogDebug("Parse_PolicyWithDifferentHashAlgorithms_ShouldReturnValidPolicy(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_PolicyWithDifferentHashAlgorithms_ShouldReturnValidPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_PolicyWithDifferentHashAlgorithms_ShouldReturnValidPolicy(): parser → {Parser}", parser);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            var generatedPolicy = policy.GeneratePolicy();
			_logger.LogDebug("Parse_PolicyWithDifferentHashAlgorithms_ShouldReturnValidPolicy(): generatedPolicy → {GeneratedPolicy}", generatedPolicy);
            Assert.That(generatedPolicy, Does.Contain("script-src"));
            Assert.That(generatedPolicy, Does.Contain("'sha256-abc123'"));
            Assert.That(generatedPolicy, Does.Contain("'sha384-def456'"));
            Assert.That(generatedPolicy, Does.Contain("'sha512-ghi789'"));
			_logger.LogDebug("<<< Exiting Parse_PolicyWithDifferentHashAlgorithms_ShouldReturnValidPolicy()");
        }

        /// <summary>
        /// Tests that constructor with null policy throws ArgumentNullException.
        /// </summary>
        [Test]
        public void Constructor_NullPolicy_ShouldThrowArgumentNullException()
        {
			_logger.LogDebug(">>> Entering Constructor_NullPolicy_ShouldThrowArgumentNullException()");
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new ContentSecurityPolicyParser(null));
			_logger.LogDebug("Constructor_NullPolicy_ShouldThrowArgumentNullException(): exception → {Exception}", exception);
            Assert.That(exception.ParamName, Is.EqualTo("policy"));
			_logger.LogDebug("<<< Exiting Constructor_NullPolicy_ShouldThrowArgumentNullException()");
        }

        /// <summary>
        /// Tests that constructor with null policy throws ArgumentNullException.
        /// </summary>
        [Test]
        public void InvalidHost_ShouldThrowArgumentException()
        {
			_logger.LogDebug(">>> Entering InvalidHost_ShouldThrowArgumentException()");
            // Act & Assert
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("InvalidHost_ShouldThrowArgumentException(): policy → {Policy}", policy);

            var exception = Assert.Throws<ArgumentException>(() => policy.ScriptSource.AddHost("http:///x.x"));
			_logger.LogDebug("InvalidHost_ShouldThrowArgumentException(): exception → {Exception}", exception);
            Assert.That(exception.Message, Does.Contain("host"));
			_logger.LogDebug("<<< Exiting InvalidHost_ShouldThrowArgumentException()");
        }

        /// <summary>
        /// Tests that constructor with null policy throws ArgumentNullException.
        /// </summary>
        [Test]
        public void InvalidHost_WithoutSyntaxCheck_ShouldNotThrowException()
        {
			_logger.LogDebug(">>> Entering InvalidHost_WithoutSyntaxCheck_ShouldNotThrowException()");
            // Act & Assert
            var policy = new ContentSecurityPolicy(false);
			_logger.LogDebug("InvalidHost_WithoutSyntaxCheck_ShouldNotThrowException(): policy → {Policy}", policy);

            policy.ScriptSource.AddHost("http:///x.x");
            Assert.That(policy, Is.Not.Null);
			_logger.LogDebug("<<< Exiting InvalidHost_WithoutSyntaxCheck_ShouldNotThrowException()");
        }
    }
}
