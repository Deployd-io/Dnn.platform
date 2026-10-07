// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
	using Microsoft.Extensions.Logging;

    /// <summary>
    /// Utility class for parsing Content Security Policy headers into ContentSecurityPolicy objects.
    /// </summary>
    public class ContentSecurityPolicyParser
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private static readonly char[] CommaSeparator = new[] { ',' };
        private static readonly char[] SemicolonsSeparator = new[] { ';' };
        private static readonly char[] SpaceSeparator = new[] { ' ' };
        private static readonly char[] EqualSeparator = new[] { '=' };
        private readonly IContentSecurityPolicy policy;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentSecurityPolicyParser"/> class.
        /// </summary>
        /// <param name="policy">The ContentSecurityPolicy instance to populate with parsed directives.</param>
        public ContentSecurityPolicyParser(IContentSecurityPolicy policy)
        {
			_logger.LogError(">>> Entering ContentSecurityPolicyParser(policy={Policy})", policy);
            this.policy = policy ?? throw new ArgumentNullException(nameof(policy));
			_logger.LogDebug("ContentSecurityPolicyParser(policy={Policy}): policy → {Policy1}", policy, policy);
        _logger.LogWarning("<<< Exiting ContentSecurityPolicyParser(policy={Policy})", policy);
        }

        /// <summary>
        /// Parses a CSP header string into the provided ContentSecurityPolicy object.
        /// </summary>
        /// <param name="cspHeader">The CSP header string to parse.</param>
        /// <exception cref="ArgumentException">Thrown when the CSP header is invalid or cannot be parsed.</exception>
        public void Parse(string cspHeader)
        {
			_logger.LogError(">>> Entering Parse(cspHeader={CspHeader})", cspHeader);
            if (string.IsNullOrWhiteSpace(cspHeader))
            {
				_logger.LogError("Throwing ArgumentException from Parse(cspHeader={CspHeader})", cspHeader);
                throw new ArgumentException("CSP header cannot be null or empty", nameof(cspHeader));
            }

            // Split the header into individual directives
            var directives = SplitDirectives(cspHeader);
			_logger.LogError("Parse(cspHeader={CspHeader}): directives → {Directives}", cspHeader, directives);

            foreach (var directive in directives)
            {
                this.ParseDirective(directive);
            }
			_logger.LogWarning("<<< Exiting Parse(cspHeader={CspHeader})", cspHeader);
        }

        /// <summary>
        /// Tries to parse a CSP header string into the provided ContentSecurityPolicy object.
        /// </summary>
        /// <param name="cspHeader">The CSP header string to parse.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        public bool TryParse(string cspHeader)
        {
			_logger.LogError(">>> Entering TryParse(cspHeader={CspHeader})", cspHeader);
            try
            {
                this.Parse(cspHeader);
                return true;
            }
            catch
            {
				_logger.LogError("Exception in TryParse(cspHeader={CspHeader}): {Arg1}", cspHeader, ex.Message);
				_logger.LogWarning("<<< Exiting TryParse(cspHeader={CspHeader})", cspHeader);
                return false;
            }
        }

        /// <summary>
        /// Parses a reporting endpoints header string into the provided ContentSecurityPolicy object.
        /// </summary>
        /// <param name="cspHeader">The CSP header string to parse.</param>
        public void ParseReportingEndpoints(string cspHeader)
        {
			_logger.LogError(">>> Entering ParseReportingEndpoints(cspHeader={CspHeader})", cspHeader);
            if (string.IsNullOrWhiteSpace(cspHeader))
            {
                return;
            }

            // Split the header into individual directives
            var directives = cspHeader.Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries)
                           .Select(d => d.Trim())
                           .Where(d => !string.IsNullOrEmpty(d));

            foreach (var directive in directives)
            {
                this.ParseReportingEndpointsDirective(directive);
            }
			_logger.LogWarning("<<< Exiting ParseReportingEndpoints(cspHeader={CspHeader})", cspHeader);
        }

        /// <summary>
        /// Splits the CSP header into individual directive strings.
        /// </summary>
        private static IEnumerable<string> SplitDirectives(string cspHeader)
        {
			_logger.LogError(">>> Entering SplitDirectives(cspHeader={CspHeader})", cspHeader);
            // CSP directives are separated by semicolons
			_logger.LogWarning("<<< Exiting SplitDirectives(cspHeader={CspHeader})", cspHeader);
            return cspHeader.Split(SemicolonsSeparator, StringSplitOptions.RemoveEmptyEntries)
                           .Select(d => d.Trim())
                           .Where(d => !string.IsNullOrEmpty(d));
        }

        /// <summary>
        /// Parses a source string and adds it to the contributor.
        /// </summary>
        private static void ParseAndAddSource(SourceCspContributor contributor, string source)
        {
			_logger.LogError(">>> Entering ParseAndAddSource(contributor={Contributor},source={Source})", contributor, source);
            var trimmedSource = source.Trim();

			_logger.LogError("ParseAndAddSource(contributor={Contributor},source={Source}): trimmedSource → {TrimmedSource}", contributor, source, trimmedSource);
            // Check for quoted keywords first
            if (CspSourceTypeNameMapper.IsQuotedKeyword(trimmedSource))
            {
                if (CspSourceTypeNameMapper.TryGetSourceType(trimmedSource, out var sourceType))
                {
                    switch (sourceType)
                    {
                        case CspSourceType.Self:
                            contributor.AddSelf();
                            break;

                        case CspSourceType.Inline:
                            contributor.AddInline();
                            break;

                        case CspSourceType.Eval:
                            contributor.AddEval();
                            break;

                        case CspSourceType.None:
                            contributor.AddNone();
                            break;

                        case CspSourceType.StrictDynamic:
                            contributor.AddStrictDynamic();
                            break;
                    }
                }
                else if (CspSourceTypeNameMapper.IsNonceSource(trimmedSource))
                {
                    var quotedValue = trimmedSource.Substring(1, trimmedSource.Length - 2);
					_logger.LogError("ParseAndAddSource(contributor={Contributor},source={Source}): quotedValue → {QuotedValue}", contributor, source, quotedValue);
                    var nonce = quotedValue.Substring(6); // Remove "nonce-" prefix
					_logger.LogError("ParseAndAddSource(contributor={Contributor},source={Source}): nonce → {Nonce}", contributor, source, nonce);
                    contributor.AddNonce(nonce);
                }
                else if (CspSourceTypeNameMapper.IsHashSource(trimmedSource))
                {
                    var quotedValue = trimmedSource.Substring(1, trimmedSource.Length - 2);
                    contributor.AddHash(quotedValue);
                }
            }
            else if (trimmedSource.Contains(":"))
            {
                // Check if it's a scheme
                if (IsScheme(trimmedSource))
                {
                    contributor.AddScheme(trimmedSource);
                }
                else
                {
                    // Treat as host
                    contributor.AddHost(trimmedSource);
                }
            }
            else
            {
                // Treat as host (domain without protocol)
                contributor.AddHost(trimmedSource);
            }
			_logger.LogWarning("<<< Exiting ParseAndAddSource(contributor={Contributor},source={Source})", contributor, source);
        }

        /// <summary>
        /// Checks if a string represents a scheme.
        /// </summary>
        private static bool IsScheme(string source)
        {
			_logger.LogError(">>> Entering IsScheme(source={Source})", source);
            var knownSchemes = new string[] { "http:", "https:", "data:", "blob:", "filesystem:", "wss:", "ws:" };
			_logger.LogError("IsScheme(source={Source}): knownSchemes → {KnownSchemes}", source, knownSchemes);
			_logger.LogWarning("<<< Exiting IsScheme(source={Source})", source);
            return knownSchemes.Contains(source.ToLowerInvariant());
        }

        /// <summary>
        /// Parses a single directive and applies it to the policy.
        /// </summary>
        private void ParseDirective(string directive)
        {
			_logger.LogError(">>> Entering ParseDirective(directive={Directive})", directive);
            var parts = directive.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries);
			_logger.LogError("ParseDirective(directive={Directive}): parts → {Parts}", directive, parts);
            if (parts.Length == 0)
            {
                return;
            }

            var directiveName = parts[0].ToLowerInvariant();
			_logger.LogError("ParseDirective(directive={Directive}): directiveName → {DirectiveName}", directive, directiveName);
            var sources = parts.Skip(1).ToArray();
			_logger.LogError("ParseDirective(directive={Directive}): sources → {Sources}", directive, sources);

            // Try to get the directive type from the name
            if (!CspDirectiveNameMapper.TryGetDirectiveType(directiveName, out var directiveType))
            {
                // Unknown directive - ignore for now
				_logger.LogError("Throwing ArgumentException from ParseDirective(directive={Directive})", directive);
                throw new ArgumentException($"Unknown directive: {directiveName}");
            }

            this.ApplyDirectiveToPolicy(directiveType, sources);
			_logger.LogWarning("<<< Exiting ParseDirective(directive={Directive})", directive);
        }

        /// <summary>
        /// Parses a reporting endpoints directive string into the provided ContentSecurityPolicy object.
        /// </summary>
        /// <param name="directive">The reporting endpoints directive string to parse.</param>
        private void ParseReportingEndpointsDirective(string directive)
        {
			_logger.LogError(">>> Entering ParseReportingEndpointsDirective(directive={Directive})", directive);
            if (string.IsNullOrWhiteSpace(directive))
            {
                return;
            }

            // Split directive into name=value pairs
            var parts = directive.Split(EqualSeparator, StringSplitOptions.RemoveEmptyEntries);
			_logger.LogError("ParseReportingEndpointsDirective(directive={Directive}): parts → {Parts}", directive, parts);
            if (parts.Length != 2)
            {
				_logger.LogError("Throwing ArgumentException from ParseReportingEndpointsDirective(directive={Directive})", directive);
                throw new ArgumentException("Invalid reporting endpoint format. Expected format: name=\"value\"");
            }

            var name = parts[0].Trim();
			_logger.LogError("ParseReportingEndpointsDirective(directive={Directive}): name → {Name}", directive, name);
            var value = parts[1].Trim().Trim('"');
			_logger.LogError("ParseReportingEndpointsDirective(directive={Directive}): value → {Value}", directive, value);

            // Add the reporting endpoint to the policy
            this.policy.AddReportEndpoint(name, value);
			_logger.LogWarning("<<< Exiting ParseReportingEndpointsDirective(directive={Directive})", directive);
        }

        /// <summary>
        /// Applies a parsed directive to the policy object.
        /// </summary>
        private void ApplyDirectiveToPolicy(CspDirectiveType directiveType, string[] sources)
        {
			_logger.LogError(">>> Entering ApplyDirectiveToPolicy(directiveType={DirectiveType},sources={Sources})", directiveType, sources);
            switch (directiveType)
            {
                case CspDirectiveType.SandboxDirective:
                    this.policy.AddSandbox(string.Join(" ", sources));
                    break;

                case CspDirectiveType.PluginTypes:
                    foreach (var source in sources)
                    {
                        this.policy.AddPluginTypes(source);
                    }

                    break;

                case CspDirectiveType.UpgradeInsecureRequests:
                    this.policy.UpgradeInsecureRequests();
                    break;

                case CspDirectiveType.ReportUri:
                    foreach (var source in sources)
                    {
                        this.policy.AddReportEndpoint("default", source);
                    }

                    break;

                case CspDirectiveType.ReportTo:
                    foreach (var source in sources)
                    {
                        this.policy.AddReportTo(source);
                    }

                    break;

                // Source-based directives
                default:
                    var contributor = this.GetSourceContributor(directiveType);
					_logger.LogError("ApplyDirectiveToPolicy(directiveType={DirectiveType},sources={Sources}): contributor → {Contributor}", directiveType, sources, contributor);
                    if (contributor != null)
                    {
                        foreach (var source in sources)
                        {
                            ParseAndAddSource(contributor, source);
                        }
                    }

                    break;
            }
			_logger.LogWarning("<<< Exiting ApplyDirectiveToPolicy(directiveType={DirectiveType},sources={Sources})", directiveType, sources);
        }

        /// <summary>
        /// Gets the appropriate source contributor for a directive type.
        /// </summary>
        private SourceCspContributor GetSourceContributor(CspDirectiveType directiveType)
        {
			_logger.LogError(">>> Entering GetSourceContributor(directiveType={DirectiveType})", directiveType);
			_logger.LogWarning("<<< Exiting GetSourceContributor(directiveType={DirectiveType})", directiveType);
            return directiveType switch
            {
                CspDirectiveType.DefaultSrc => this.policy.DefaultSource,
                CspDirectiveType.ScriptSrc => this.policy.ScriptSource,
                CspDirectiveType.StyleSrc => this.policy.StyleSource,
                CspDirectiveType.ImgSrc => this.policy.ImgSource,
                CspDirectiveType.ConnectSrc => this.policy.ConnectSource,
                CspDirectiveType.FontSrc => this.policy.FontSource,
                CspDirectiveType.ObjectSrc => this.policy.ObjectSource,
                CspDirectiveType.MediaSrc => this.policy.MediaSource,
                CspDirectiveType.FrameSrc => this.policy.FrameSource,
                CspDirectiveType.FrameAncestors => this.policy.FrameAncestors,
                CspDirectiveType.FormAction => this.policy.FormAction,
                CspDirectiveType.BaseUri => this.policy.BaseUriSource,
                _ => null,
            };
        }
    }
}
