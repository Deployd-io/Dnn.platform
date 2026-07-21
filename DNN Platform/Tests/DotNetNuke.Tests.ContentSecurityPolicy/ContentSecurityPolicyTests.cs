// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy.Tests
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    /// <summary>
    /// Unit tests for ContentSecurityPolicy class with parser integration.
    /// </summary>
    [TestFixture]
    public class ContentSecurityPolicyTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>
        /// Tests parsing using the instance-based parser integration.
        /// </summary>
        [Test]
        public void Parse_ValidInput_ShouldReturnValidPolicy()
        {
			Logger.LogDebug(">>> Entering Parse_ValidInput_ShouldReturnValidPolicy()");
            // Arrange
            var cspHeader = "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdn.example.com 'nonce-abc123' cdn.example.com cdn.example.com/ *.example.com 10.10.10.10 https://*.example.com:12/path/to/file.js http://[fe80::1]/index.html; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; connect-src 'self'; font-src 'self' https://fonts.googleapis.com; frame-ancestors 'none'; report-uri http://csp-report";
            var policy = new ContentSecurityPolicy(true);
			Logger.LogDebug("Parse_ValidInput_ShouldReturnValidPolicy(): cspHeader → {CspHeader}", cspHeader);
			Logger.LogDebug("Parse_ValidInput_ShouldReturnValidPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);

			Logger.LogDebug("Parse_ValidInput_ShouldReturnValidPolicy(): parser → {Parser}", parser);
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
            Assert.That(generatedPolicy, Is.Not.Empty);
			Logger.LogDebug("Parse_ValidInput_ShouldReturnValidPolicy(): generatedPolicy → {GeneratedPolicy}", generatedPolicy);
            Assert.That(generatedPolicy, Does.Contain("default-src 'self'"));
			Logger.LogDebug("<<< Exiting Parse_ValidInput_ShouldReturnValidPolicy()");
        }

        /// <summary>
        /// Tests TryParse functionality.
        /// </summary>
        [Test]
        public void TryParse_ValidInput_ShouldReturnTrueAndPolicy()
        {
			Logger.LogDebug(">>> Entering TryParse_ValidInput_ShouldReturnTrueAndPolicy()");
            // Arrange
            var cspHeader = "default-src 'self'; script-src 'self'";
            var policy = new ContentSecurityPolicy(true);
			Logger.LogDebug("TryParse_ValidInput_ShouldReturnTrueAndPolicy(): cspHeader → {CspHeader}", cspHeader);
			Logger.LogDebug("TryParse_ValidInput_ShouldReturnTrueAndPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);

			Logger.LogDebug("TryParse_ValidInput_ShouldReturnTrueAndPolicy(): parser → {Parser}", parser);
            // Act
            var result = parser.TryParse(cspHeader);

			Logger.LogDebug("TryParse_ValidInput_ShouldReturnTrueAndPolicy(): result → {Result}", result);
            // Assert
            Assert.That(result, Is.True);
            Assert.That(policy, Is.Not.Null);
            Assert.That(policy, Is.InstanceOf<IContentSecurityPolicy>());
            Assert.That(policy.GeneratePolicy(), Does.Contain("default-src 'self'"));
            Assert.That(policy.GeneratePolicy(), Does.Contain("script-src 'self'"));
			Logger.LogDebug("<<< Exiting TryParse_ValidInput_ShouldReturnTrueAndPolicy()");
        }

        /// <summary>
        /// Tests TryParse with invalid input.
        /// </summary>
        [Test]
        public void TryParse_InvalidInput_ShouldReturnFalse()
        {
			Logger.LogDebug(">>> Entering TryParse_InvalidInput_ShouldReturnFalse()");
            // Arrange
            var invalidCspHeader = string.Empty;
            var policy = new ContentSecurityPolicy(true);
            var parser = new ContentSecurityPolicyParser(policy);
			Logger.LogDebug("TryParse_InvalidInput_ShouldReturnFalse(): policy → {Policy}", policy);
			Logger.LogDebug("TryParse_InvalidInput_ShouldReturnFalse(): parser → {Parser}", parser);

            // Act
            var result = parser.TryParse(invalidCspHeader);

			Logger.LogDebug("TryParse_InvalidInput_ShouldReturnFalse(): result → {Result}", result);
            // Assert
            Assert.That(result, Is.False);
			Logger.LogDebug("<<< Exiting TryParse_InvalidInput_ShouldReturnFalse()");
        }

        /// <summary>
        /// Tests that parsed policy can be modified and regenerated.
        /// </summary>
        [Test]
        public void Parse_ModifyParsedPolicy_ShouldGenerateUpdatedPolicy()
        {
			Logger.LogDebug(">>> Entering Parse_ModifyParsedPolicy_ShouldGenerateUpdatedPolicy()");
            // Arrange
            var originalCspHeader = "default-src 'self'; script-src 'self'";
            var policy = new ContentSecurityPolicy();
			Logger.LogDebug("Parse_ModifyParsedPolicy_ShouldGenerateUpdatedPolicy(): originalCspHeader → {OriginalCspHeader}", originalCspHeader);
			Logger.LogDebug("Parse_ModifyParsedPolicy_ShouldGenerateUpdatedPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);

			Logger.LogDebug("Parse_ModifyParsedPolicy_ShouldGenerateUpdatedPolicy(): parser → {Parser}", parser);
            // Act
            parser.Parse(originalCspHeader);

            // Modify the parsed policy
            policy.ScriptSource.AddHost("newcdn.example.com");
            policy.StyleSource.AddHash("sha256-abc123def456");

            var modifiedPolicy = policy.GeneratePolicy();

			Logger.LogDebug("Parse_ModifyParsedPolicy_ShouldGenerateUpdatedPolicy(): modifiedPolicy → {ModifiedPolicy}", modifiedPolicy);
            // Assert
            Assert.That(modifiedPolicy, Does.Contain("default-src 'self'"));
            Assert.That(modifiedPolicy, Does.Contain("script-src"));
            Assert.That(modifiedPolicy, Does.Contain("'self'"));
            Assert.That(modifiedPolicy, Does.Contain("newcdn.example.com"));
            Assert.That(modifiedPolicy, Does.Contain("style-src"));
            Assert.That(modifiedPolicy, Does.Contain("'sha256-abc123def456'"));
			Logger.LogDebug("<<< Exiting Parse_ModifyParsedPolicy_ShouldGenerateUpdatedPolicy()");
        }

        /// <summary>
        /// Tests nonce generation on policy instance.
        /// </summary>
        [Test]
        public void Parse_AccessNonce_ShouldGenerateNonce()
        {
			Logger.LogDebug(">>> Entering Parse_AccessNonce_ShouldGenerateNonce()");
            // Arrange
            var cspHeader = "default-src 'self'";
            var policy = new ContentSecurityPolicy(true);
			Logger.LogDebug("Parse_AccessNonce_ShouldGenerateNonce(): cspHeader → {CspHeader}", cspHeader);
			Logger.LogDebug("Parse_AccessNonce_ShouldGenerateNonce(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);

			Logger.LogDebug("Parse_AccessNonce_ShouldGenerateNonce(): parser → {Parser}", parser);
            // Act
            parser.Parse(cspHeader);
            var nonce = policy.Nonce;

            // Assert
            Assert.That(nonce, Is.Not.Empty);
            Assert.That(nonce.Length, Is.GreaterThan(0));

            // Nonce should be consistent across multiple calls
            var nonce2 = policy.Nonce;
            Assert.That(nonce2, Is.EqualTo(nonce));
			Logger.LogDebug("<<< Exiting Parse_AccessNonce_ShouldGenerateNonce()");
        }

        /// <summary>
        /// Tests parsing and using all directive types.
        /// </summary>
        [Test]
        public void Parse_AllDirectiveTypes_ShouldParseCorrectly()
        {
			Logger.LogDebug(">>> Entering Parse_AllDirectiveTypes_ShouldParseCorrectly()");
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
						Logger.LogDebug("Parse_AllDirectiveTypes_ShouldParseCorrectly(): cspHeader → {CspHeader}", cspHeader);
            var policy = new ContentSecurityPolicy(true);
            var parser = new ContentSecurityPolicyParser(policy);
			Logger.LogDebug("Parse_AllDirectiveTypes_ShouldParseCorrectly(): policy → {Policy}", policy);
			Logger.LogDebug("Parse_AllDirectiveTypes_ShouldParseCorrectly(): parser → {Parser}", parser);

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
            Assert.That(generatedPolicy, Does.Contain("default-src 'self'"));
			Logger.LogDebug("Parse_AllDirectiveTypes_ShouldParseCorrectly(): generatedPolicy → {GeneratedPolicy}", generatedPolicy);
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
			Logger.LogDebug("<<< Exiting Parse_AllDirectiveTypes_ShouldParseCorrectly()");
        }

        /// <summary>
        /// Tests parsing policy with reporting directives.
        /// </summary>
        [Test]
        public void Parse_PolicyWithReporting_ShouldParseCorrectly()
        {
			Logger.LogDebug(">>> Entering Parse_PolicyWithReporting_ShouldParseCorrectly()");
            // Arrange
            var cspHeader = "default-src 'self'; report-uri http://csp-report; report-to csp-endpoint";
            var policy = new ContentSecurityPolicy(true);
			Logger.LogDebug("Parse_PolicyWithReporting_ShouldParseCorrectly(): cspHeader → {CspHeader}", cspHeader);
			Logger.LogDebug("Parse_PolicyWithReporting_ShouldParseCorrectly(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);

			Logger.LogDebug("Parse_PolicyWithReporting_ShouldParseCorrectly(): parser → {Parser}", parser);
            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            var generatedPolicy = policy.GeneratePolicy();
            Assert.That(generatedPolicy, Does.Contain("default-src 'self'"));
			Logger.LogDebug("Parse_PolicyWithReporting_ShouldParseCorrectly(): generatedPolicy → {GeneratedPolicy}", generatedPolicy);
            // Note: reporting directives might be handled differently in the implementation
			Logger.LogDebug("<<< Exiting Parse_PolicyWithReporting_ShouldParseCorrectly()");
        }

        /// <summary>
        /// Tests parsing policy with upgrade-insecure-requests.
        /// </summary>
        [Test]
        public void Parse_PolicyWithUpgradeInsecureRequests_ShouldParseCorrectly()
        {
			Logger.LogDebug(">>> Entering Parse_PolicyWithUpgradeInsecureRequests_ShouldParseCorrectly()");
            // Arrange
            var cspHeader = "default-src 'self'; upgrade-insecure-requests";
            var policy = new ContentSecurityPolicy(true);
			Logger.LogDebug("Parse_PolicyWithUpgradeInsecureRequests_ShouldParseCorrectly(): cspHeader → {CspHeader}", cspHeader);
			Logger.LogDebug("Parse_PolicyWithUpgradeInsecureRequests_ShouldParseCorrectly(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);

			Logger.LogDebug("Parse_PolicyWithUpgradeInsecureRequests_ShouldParseCorrectly(): parser → {Parser}", parser);
            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            var generatedPolicy = policy.GeneratePolicy();
            Assert.That(generatedPolicy, Does.Contain("default-src 'self'"));
			Logger.LogDebug("Parse_PolicyWithUpgradeInsecureRequests_ShouldParseCorrectly(): generatedPolicy → {GeneratedPolicy}", generatedPolicy);
            Assert.That(generatedPolicy, Does.Contain("upgrade-insecure-requests"));
			Logger.LogDebug("<<< Exiting Parse_PolicyWithUpgradeInsecureRequests_ShouldParseCorrectly()");
        }

        /// <summary>
        /// Tests round-trip parsing (parse then generate should produce similar result).
        /// </summary>
        [Test]
        public void Parse_RoundTrip_ShouldProduceSimilarPolicy()
        {
			Logger.LogDebug(">>> Entering Parse_RoundTrip_ShouldProduceSimilarPolicy()");
            // Arrange
            var originalCspHeader = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self'";
            var policy = new ContentSecurityPolicy(true);
			Logger.LogDebug("Parse_RoundTrip_ShouldProduceSimilarPolicy(): originalCspHeader → {OriginalCspHeader}", originalCspHeader);
			Logger.LogDebug("Parse_RoundTrip_ShouldProduceSimilarPolicy(): policy → {Policy}", policy);
            var parser = new ContentSecurityPolicyParser(policy);

			Logger.LogDebug("Parse_RoundTrip_ShouldProduceSimilarPolicy(): parser → {Parser}", parser);
            // Act
            parser.Parse(originalCspHeader);
            var regeneratedHeader = policy.GeneratePolicy();

			Logger.LogDebug("Parse_RoundTrip_ShouldProduceSimilarPolicy(): regeneratedHeader → {RegeneratedHeader}", regeneratedHeader);
            // Assert
            Assert.That(regeneratedHeader, Does.Contain("default-src 'self'"));
            Assert.That(regeneratedHeader, Does.Contain("script-src"));
            Assert.That(regeneratedHeader, Does.Contain("'self'"));
            Assert.That(regeneratedHeader, Does.Contain("'unsafe-inline'"));
            Assert.That(regeneratedHeader, Does.Contain("style-src"));

            // The order might be different, but all elements should be present
            var originalParts = originalCspHeader.Split(';').Select(p => p.Trim()).ToArray();
            foreach (var part in originalParts)
            {
			Logger.LogDebug("Parse_RoundTrip_ShouldProduceSimilarPolicy(): originalParts → {OriginalParts}", originalParts);
                if (!string.IsNullOrEmpty(part))
                {
                    // Check that the directive name and 'self' are present
                    var directiveName = part.Split(' ')[0];
                    Assert.That(regeneratedHeader, Does.Contain(directiveName));
					Logger.LogDebug("Parse_RoundTrip_ShouldProduceSimilarPolicy(): directiveName → {DirectiveName}", directiveName);
                }
            }
			Logger.LogDebug("<<< Exiting Parse_RoundTrip_ShouldProduceSimilarPolicy()");
        }
    }
}
