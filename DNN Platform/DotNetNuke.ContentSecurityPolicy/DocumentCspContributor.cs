// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Contributor for document-level directives.
    /// </summary>
    public class DocumentCspContributor : BaseCspContributor
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>
        /// Compiled regex for validating MIME type format (type/subtype).
        /// </summary>
        private static readonly Regex MimeTypeRegex = new Regex(@"^[a-zA-Z0-9][a-zA-Z0-9!#$&\-\^_.+]*/[a-zA-Z0-9][a-zA-Z0-9!#$&\-\^_.+]*$", RegexOptions.Compiled);

        private static readonly char[] SpaceSeparator = new[] { ' ' };

        private string directiveValue;

        private bool checkSyntax;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentCspContributor"/> class.
        /// </summary>
        /// <param name="directiveType">The directive type to create the contributor for.</param>
        /// <param name="value">The value of the directive.</param>
        /// <param name="checkSyntax">Check syntax of value.</param>
        public DocumentCspContributor(CspDirectiveType directiveType, string value, bool checkSyntax)
        {
			Logger.LogDebug(">>> Entering DocumentCspContributor(directiveType={DirectiveType},value={Value},checkSyntax={CheckSyntax})", directiveType, value, checkSyntax);
            this.checkSyntax = checkSyntax;
            this.DirectiveType = directiveType;
			Logger.LogDebug("DocumentCspContributor(directiveType={DirectiveType},value={Value},checkSyntax={CheckSyntax}): this.checkSyntax → {Arg3}", directiveType, value, checkSyntax, this.checkSyntax);
			Logger.LogDebug("DocumentCspContributor(directiveType={DirectiveType},value={Value},checkSyntax={CheckSyntax}): this.DirectiveType → {Arg3}", directiveType, value, checkSyntax, this.DirectiveType);
            this.DirectiveValue = value;
			Logger.LogDebug("<<< Exiting DocumentCspContributor(directiveType={DirectiveType},value={Value},checkSyntax={CheckSyntax})", directiveType, value, checkSyntax);
			Logger.LogDebug("DocumentCspContributor(directiveType={DirectiveType},value={Value},checkSyntax={CheckSyntax}): this.DirectiveValue → {Arg3}", directiveType, value, checkSyntax, this.DirectiveValue);
        }

        /// <summary>
        /// Gets or Sets value of the document directive.
        /// </summary>
        public string DirectiveValue
        {
            get
            {
                return this.directiveValue;
            }

            set
            {
                if (this.checkSyntax)
                {
                    ValidateDirectiveValue(this.DirectiveType, value);
                }

                this.directiveValue = value;
            }
        }

        /// <summary>
        /// Generates the directive string.
        /// </summary>
        /// <returns>The directive string.</returns>
        public override string GenerateDirective()
        {
			Logger.LogDebug(">>> Entering GenerateDirective()");
            if (this.DirectiveType == CspDirectiveType.UpgradeInsecureRequests)
            {
                return $"{CspDirectiveNameMapper.GetDirectiveName(this.DirectiveType)}";
            }

            if (string.IsNullOrWhiteSpace(this.DirectiveValue))
            {
                return string.Empty;
            }

			Logger.LogDebug("<<< Exiting GenerateDirective()");
            return $"{CspDirectiveNameMapper.GetDirectiveName(this.DirectiveType)} {this.DirectiveValue}";
        }

        /// <summary>
        /// Validates plugin types (MIME types).
        /// </summary>
        private static void ValidatePluginTypes(string value)
        {
			Logger.LogDebug(">>> Entering ValidatePluginTypes(value={Value})", value);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Plugin types cannot be empty");
            }

            var types = value.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries);

			Logger.LogDebug("ValidatePluginTypes(value={Value}): types → {Types}", value, types);
            if (types.Any(t => !MimeTypeRegex.IsMatch(t)))
            {
                throw new ArgumentException($"Invalid MIME type format: {value}");
            }
			Logger.LogDebug("<<< Exiting ValidatePluginTypes(value={Value})", value);
        }

        /// <summary>
        /// Validates sandbox directive values.
        /// </summary>
        private static void ValidateSandboxDirective(string value)
        {
			Logger.LogDebug(">>> Entering ValidateSandboxDirective(value={Value})", value);
            var validSandboxValues = new string[]
            {
                "allow-forms",
                "allow-modals",
                "allow-orientation-lock",
                "allow-pointer-lock",
                "allow-popups",
                "allow-popups-to-escape-sandbox",
                "allow-presentation",
                "allow-same-origin",
                "allow-scripts",
                "allow-top-navigation",
                "allow-top-navigation-by-user-activation",
            };

            var values = value.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries);

			Logger.LogDebug("ValidateSandboxDirective(value={Value}): values → {Values}", value, values);
            if (values.Any(v => !validSandboxValues.Contains(v)))
            {
                throw new ArgumentException($"Invalid sandbox directive value: {value}");
            }
			Logger.LogDebug("<<< Exiting ValidateSandboxDirective(value={Value})", value);
        }

        /// <summary>
        /// Validates directive value based on directive type.
        /// </summary>
        private static void ValidateDirectiveValue(CspDirectiveType type, string value)
        {
			Logger.LogDebug(">>> Entering ValidateDirectiveValue(type={Type},value={Value})", type, value);
            switch (type)
            {
                case CspDirectiveType.PluginTypes:
                    ValidatePluginTypes(value);
                    break;
                case CspDirectiveType.SandboxDirective:
                    ValidateSandboxDirective(value);
                    break;
            }
			Logger.LogDebug("<<< Exiting ValidateDirectiveValue(type={Type},value={Value})", type, value);
        }
    }
}
