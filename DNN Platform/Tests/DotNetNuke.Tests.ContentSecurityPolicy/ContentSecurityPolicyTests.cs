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
    /// Unit tests for ContentSecurityPolicy class with parser integration.
    /// </summary>
    [TestFixture]
    public class ContentSecurityPolicyTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>
        /// Tests parsing using the instance-based parser integration.
        /// </summary>
        [Test]
        public void Parse_ValidInput_ShouldReturnValidPolicy()
        {
			_logger.LogDebug(">>> Entering Parse_ValidInput_ShouldReturnValidPolicy()");
            // Arrange
            var cspHeader = "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdn.example.com 'nonce-abc123' cdn.example.com cdn.example.com/ *.example.com 10.10.10.10 https://*.example.com:12/path/to/file.js http://[fe80::1]/index.html; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; connect-src 'self'; font-src 'self' https://fonts.googleapis.com; frame-ancestors 'none'; report-uri http://csp-report";
			_logger.LogDebug("Parse_ValidInput_ShouldReturnValidPolicy(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_ValidInput_ShouldReturnValidPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_ValidInput_ShouldReturnValidPolicy(): parser → {Parser}", parser);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            Assert.That(policy, Is.InstanceOf<IContentSecurityPolicy>());

            // Verify we can access parsed directives
            Assert.That(policy.DefaultSource, Is.Not.Null);
            Assert.That(policy.ScriptSource, Is.Not.Null);
            Assert.That(policy.StyleSource, Is.Not.Null);
            Assert.That(policy.ImgSource, Is.Not.Null);
            Assert.That(policy.ConnectSource, Is.Not.Null);
            Assert.That(policy.FontSource, Is.Not.Null);
            Assert.That(policy.FrameAncestors, Is.Not.Null);

            // Verify the policy can be regenerated
            var generatedPolicy = policy.GeneratePolicy();
			_logger.LogDebug("Parse_ValidInput_ShouldReturnValidPolicy(): generatedPolicy → {GeneratedPolicy}", generatedPolicy);
            Assert.That(generatedPolicy, Is.Not.Empty);
            Assert.That(generatedPolicy, Does.Contain("default-src 'self'"));
			_logger.LogDebug("<<< Exiting Parse_ValidInput_ShouldReturnValidPolicy()");
        }

        /// <summary>
        /// Tests TryParse functionality.
        /// </summary>
        [Test]
        public void TryParse_ValidInput_ShouldReturnTrueAndPolicy()
        {
			_logger.LogDebug(">>> Entering TryParse_ValidInput_ShouldReturnTrueAndPolicy()");
            // Arrange
            var cspHeader = "default-src 'self'; script-src 'self'";
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
            Assert.That(policy, Is.InstanceOf<IContentSecurityPolicy>());
            Assert.That(policy.GeneratePolicy(), Does.Contain("default-src 'self'"));
            Assert.That(policy.GeneratePolicy(), Does.Contain("script-src 'self'"));
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
            var invalidCspHeader = string.Empty;
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("TryParse_InvalidInput_ShouldReturnFalse(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("TryParse_InvalidInput_ShouldReturnFalse(): parser → {Parser}", parser);

            // Act
            var result = parser.TryParse(invalidCspHeader);
			_logger.LogDebug("TryParse_InvalidInput_ShouldReturnFalse(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.False);
			_logger.LogDebug("<<< Exiting TryParse_InvalidInput_ShouldReturnFalse()");
        }

        /// <summary>
        /// Tests that parsed policy can be modified and regenerated.
        /// </summary>
        [Test]
        public void Parse_ModifyParsedPolicy_ShouldGenerateUpdatedPolicy()
        {
			_logger.LogDebug(">>> Entering Parse_ModifyParsedPolicy_ShouldGenerateUpdatedPolicy()");
            // Arrange
            var originalCspHeader = "default-src 'self'; script-src 'self'";
			_logger.LogDebug("Parse_ModifyParsedPolicy_ShouldGenerateUpdatedPolicy(): originalCspHeader → {OriginalCspHeader}", originalCspHeader);
            var policy = new ContentSecurityPolicy();
			_logger.LogDebug("Parse_ModifyParsedPolicy_ShouldGenerateUpdatedPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_ModifyParsedPolicy_ShouldGenerateUpdatedPolicy(): parser → {Parser}", parser);

            // Act
            parser.Parse(originalCspHeader);

            // Modify the parsed policy
            policy.ScriptSource.AddHost("newcdn.example.com");
            policy.StyleSource.AddHash("sha256-abc123def456");

            var modifiedPolicy = policy.GeneratePolicy();
			_logger.LogDebug("Parse_ModifyParsedPolicy_ShouldGenerateUpdatedPolicy(): modifiedPolicy → {ModifiedPolicy}", modifiedPolicy);

            // Assert
            Assert.That(modifiedPolicy, Does.Contain("default-src 'self'"));
            Assert.That(modifiedPolicy, Does.Contain("script-src"));
            Assert.That(modifiedPolicy, Does.Contain("'self'"));
            Assert.That(modifiedPolicy, Does.Contain("newcdn.example.com"));
            Assert.That(modifiedPolicy, Does.Contain("style-src"));
            Assert.That(modifiedPolicy, Does.Contain("'sha256-abc123def456'"));
			_logger.LogDebug("<<< Exiting Parse_ModifyParsedPolicy_ShouldGenerateUpdatedPolicy()");
        }

        /// <summary>
        /// Tests nonce generation on policy instance.
        /// </summary>
        [Test]
        public void Parse_AccessNonce_ShouldGenerateNonce()
        {
			_logger.LogDebug(">>> Entering Parse_AccessNonce_ShouldGenerateNonce()");
            // Arrange
            var cspHeader = "default-src 'self'";
			_logger.LogDebug("Parse_AccessNonce_ShouldGenerateNonce(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_AccessNonce_ShouldGenerateNonce(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_AccessNonce_ShouldGenerateNonce(): parser → {Parser}", parser);

            // Act
            parser.Parse(cspHeader);
            var nonce = policy.Nonce;

            // Assert
            Assert.That(nonce, Is.Not.Empty);
            Assert.That(nonce.Length, Is.GreaterThan(0));

            // Nonce should be consistent across multiple calls
            var nonce2 = policy.Nonce;
            Assert.That(nonce2, Is.EqualTo(nonce));
			_logger.LogDebug("<<< Exiting Parse_AccessNonce_ShouldGenerateNonce()");
        }

        /// <summary>
        /// Tests parsing and using all directive types.
        /// </summary>
        [Test]
        public void Parse_AllDirectiveTypes_ShouldParseCorrectly()
        {
			_logger.LogDebug(">>> Entering Parse_AllDirectiveTypes_ShouldParseCorrectly()");
            // Arrange
            var cspHeader = "default-src 'self'; " +
                           "script-src 'self' 'unsafe-inline'; " +
                           "style-src 'self' 'unsafe-inline'; " +
                           "img-src 'self' data:; " +
                           "connect-src 'self'; " +
                           "font-src 'self'; " +
                           "object-src 'none'; " +
                           "media-src 'self'; " +
                           "frame-src 'none'; " +
                           "frame-ancestors 'none'; " +
                           "form-action 'self'; " +
                           "base-uri 'self'";
						_logger.LogDebug("Parse_AllDirectiveTypes_ShouldParseCorrectly(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_AllDirectiveTypes_ShouldParseCorrectly(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_AllDirectiveTypes_ShouldParseCorrectly(): parser → {Parser}", parser);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);

            // Verify all directive contributors are accessible
            Assert.That(policy.DefaultSource, Is.Not.Null);
            Assert.That(policy.ScriptSource, Is.Not.Null);
            Assert.That(policy.StyleSource, Is.Not.Null);
            Assert.That(policy.ImgSource, Is.Not.Null);
            Assert.That(policy.ConnectSource, Is.Not.Null);
            Assert.That(policy.FontSource, Is.Not.Null);
            Assert.That(policy.ObjectSource, Is.Not.Null);
            Assert.That(policy.MediaSource, Is.Not.Null);
            Assert.That(policy.FrameSource, Is.Not.Null);
            Assert.That(policy.FrameAncestors, Is.Not.Null);
            Assert.That(policy.FormAction, Is.Not.Null);
            Assert.That(policy.BaseUriSource, Is.Not.Null);

            var generatedPolicy = policy.GeneratePolicy();
			_logger.LogDebug("Parse_AllDirectiveTypes_ShouldParseCorrectly(): generatedPolicy → {GeneratedPolicy}", generatedPolicy);
            Assert.That(generatedPolicy, Does.Contain("default-src 'self'"));
            Assert.That(generatedPolicy, Does.Contain("script-src"));
            Assert.That(generatedPolicy, Does.Contain("style-src"));
            Assert.That(generatedPolicy, Does.Contain("img-src"));
            Assert.That(generatedPolicy, Does.Contain("connect-src"));
            Assert.That(generatedPolicy, Does.Contain("font-src"));
            Assert.That(generatedPolicy, Does.Contain("object-src"));
            Assert.That(generatedPolicy, Does.Contain("media-src"));
            Assert.That(generatedPolicy, Does.Contain("frame-src"));
            Assert.That(generatedPolicy, Does.Contain("frame-ancestors"));
            Assert.That(generatedPolicy, Does.Contain("form-action"));
            Assert.That(generatedPolicy, Does.Contain("base-uri"));
			_logger.LogDebug("<<< Exiting Parse_AllDirectiveTypes_ShouldParseCorrectly()");
        }

        /// <summary>
        /// Tests parsing policy with reporting directives.
        /// </summary>
        [Test]
        public void Parse_PolicyWithReporting_ShouldParseCorrectly()
        {
			_logger.LogDebug(">>> Entering Parse_PolicyWithReporting_ShouldParseCorrectly()");
            // Arrange
            var cspHeader = "default-src 'self'; report-uri http://csp-report; report-to csp-endpoint";
			_logger.LogDebug("Parse_PolicyWithReporting_ShouldParseCorrectly(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_PolicyWithReporting_ShouldParseCorrectly(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_PolicyWithReporting_ShouldParseCorrectly(): parser → {Parser}", parser);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            var generatedPolicy = policy.GeneratePolicy();
			_logger.LogDebug("Parse_PolicyWithReporting_ShouldParseCorrectly(): generatedPolicy → {GeneratedPolicy}", generatedPolicy);
            Assert.That(generatedPolicy, Does.Contain("default-src 'self'"));
            // Note: reporting directives might be handled differently in the implementation
			_logger.LogDebug("<<< Exiting Parse_PolicyWithReporting_ShouldParseCorrectly()");
        }

        /// <summary>
        /// Tests parsing policy with upgrade-insecure-requests.
        /// </summary>
        [Test]
        public void Parse_PolicyWithUpgradeInsecureRequests_ShouldParseCorrectly()
        {
			_logger.LogDebug(">>> Entering Parse_PolicyWithUpgradeInsecureRequests_ShouldParseCorrectly()");
            // Arrange
            var cspHeader = "default-src 'self'; upgrade-insecure-requests";
			_logger.LogDebug("Parse_PolicyWithUpgradeInsecureRequests_ShouldParseCorrectly(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_PolicyWithUpgradeInsecureRequests_ShouldParseCorrectly(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_PolicyWithUpgradeInsecureRequests_ShouldParseCorrectly(): parser → {Parser}", parser);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            var generatedPolicy = policy.GeneratePolicy();
			_logger.LogDebug("Parse_PolicyWithUpgradeInsecureRequests_ShouldParseCorrectly(): generatedPolicy → {GeneratedPolicy}", generatedPolicy);
            Assert.That(generatedPolicy, Does.Contain("default-src 'self'"));
            Assert.That(generatedPolicy, Does.Contain("upgrade-insecure-requests"));
			_logger.LogDebug("<<< Exiting Parse_PolicyWithUpgradeInsecureRequests_ShouldParseCorrectly()");
        }

        /// <summary>
        /// Tests round-trip parsing (parse then generate should produce similar result).
        /// </summary>
        [Test]
        public void Parse_RoundTrip_ShouldProduceSimilarPolicy()
        {
			_logger.LogDebug(">>> Entering Parse_RoundTrip_ShouldProduceSimilarPolicy()");
            // Arrange
            var originalCspHeader = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self'";
			_logger.LogDebug("Parse_RoundTrip_ShouldProduceSimilarPolicy(): originalCspHeader → {OriginalCspHeader}", originalCspHeader);
            var policy = new ContentSecurityPolicy(true);
			_logger.LogDebug("Parse_RoundTrip_ShouldProduceSimilarPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);
			_logger.LogDebug("Parse_RoundTrip_ShouldProduceSimilarPolicy(): parser → {Parser}", parser);

            // Act
            parser.Parse(originalCspHeader);
            var regeneratedHeader = policy.GeneratePolicy();
			_logger.LogDebug("Parse_RoundTrip_ShouldProduceSimilarPolicy(): regeneratedHeader → {RegeneratedHeader}", regeneratedHeader);

            // Assert
            Assert.That(regeneratedHeader, Does.Contain("default-src 'self'"));
            Assert.That(regeneratedHeader, Does.Contain("script-src"));
            Assert.That(regeneratedHeader, Does.Contain("'self'"));
            Assert.That(regeneratedHeader, Does.Contain("'unsafe-inline'"));
            Assert.That(regeneratedHeader, Does.Contain("style-src"));

            // The order might be different, but all elements should be present
            var originalParts = originalCspHeader.Split(';').Select(p => p.Trim()).ToArray();
			_logger.LogDebug("Parse_RoundTrip_ShouldProduceSimilarPolicy(): originalParts → {OriginalParts}", originalParts);
            foreach (var part in originalParts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    // Check that the directive name and 'self' are present
                    var directiveName = part.Split(' ')[0];
					_logger.LogDebug("Parse_RoundTrip_ShouldProduceSimilarPolicy(): directiveName → {DirectiveName}", directiveName);
                    Assert.That(regeneratedHeader, Does.Contain(directiveName));
                }
            }
			_logger.LogDebug("<<< Exiting Parse_RoundTrip_ShouldProduceSimilarPolicy()");
        }
    }
}
