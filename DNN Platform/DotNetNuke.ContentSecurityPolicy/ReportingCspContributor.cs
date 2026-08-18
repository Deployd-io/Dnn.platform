// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
	using Microsoft.Extensions.Logging;

    /// <summary>
    /// Contributor for reporting directives.
    /// </summary>
    public class ReportingCspContributor : BaseCspContributor
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private bool checkSyntax;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingCspContributor"/> class.
        /// </summary>
        /// <param name="directiveType">The reporting directive type (ReportUri or ReportTo).</param>
        /// <param name="checkSyntax">Check syntax of value.</param>
        public ReportingCspContributor(CspDirectiveType directiveType, bool checkSyntax)
        {
			_logger.LogDebug(">>> Entering ReportingCspContributor(directiveType={DirectiveType},checkSyntax={CheckSyntax})", directiveType, checkSyntax);
            if (directiveType != CspDirectiveType.ReportUri && directiveType != CspDirectiveType.ReportTo)
            {
				_logger.LogWarning("Throwing ArgumentException from ReportingCspContributor(directiveType={DirectiveType},checkSyntax={CheckSyntax})", directiveType, checkSyntax);
                throw new ArgumentException("Invalid reporting directive type");
            }

            this.DirectiveType = directiveType;
			_logger.LogDebug("ReportingCspContributor(directiveType={DirectiveType},checkSyntax={CheckSyntax}): this.DirectiveType → {Arg2}", directiveType, checkSyntax, this.DirectiveType);
            this.checkSyntax = checkSyntax;
			_logger.LogDebug("<<< Exiting ReportingCspContributor(directiveType={DirectiveType},checkSyntax={CheckSyntax})", directiveType, checkSyntax);
        }

        /// <summary>
        /// Gets collection of reporting endpoints.
        /// </summary>
        private List<string> ReportingEndpoints { get; } = new List<string>();

        /// <summary>
        /// Adds a reporting endpoint.
        /// </summary>
        /// <param name="endpoint">The URL of the endpoint where reports will be sent.</param>
        public void AddReportingEndpoint(string endpoint)
        {
			_logger.LogDebug(">>> Entering AddReportingEndpoint(endpoint={Endpoint})", endpoint);
            if (this.checkSyntax)
            {
                this.ValidateReportingEndpoint(endpoint);
            }

            if (!this.ReportingEndpoints.Contains(endpoint))
            {
                this.ReportingEndpoints.Add(endpoint);
            }
			_logger.LogDebug("<<< Exiting AddReportingEndpoint(endpoint={Endpoint})", endpoint);
        }

        /// <summary>
        /// Removes a reporting endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to remove.</param>
        public void RemoveReportingEndpoint(string endpoint)
        {
			_logger.LogDebug(">>> Entering RemoveReportingEndpoint(endpoint={Endpoint})", endpoint);
            this.ReportingEndpoints.Remove(endpoint);
			_logger.LogDebug("<<< Exiting RemoveReportingEndpoint(endpoint={Endpoint})", endpoint);
        }

        /// <summary>
        /// Generates the directive string.
        /// </summary>
        /// <returns>The directive string.</returns>
        public override string GenerateDirective()
        {
			_logger.LogDebug(">>> Entering GenerateDirective()");
            if (this.ReportingEndpoints.Count == 0)
            {
			_logger.LogDebug("<<< Exiting GenerateDirective()");
                return string.Empty;
            }

            return $"{CspDirectiveNameMapper.GetDirectiveName(this.DirectiveType)} {string.Join(" ", this.ReportingEndpoints)}";
        }

        private static void ValidateReportTo(string value)
        {
			_logger.LogDebug(">>> Entering ValidateReportTo(value={Value})", value);
            if (string.IsNullOrWhiteSpace(value))
            {
				_logger.LogWarning("Throwing ArgumentException from ValidateReportTo(value={Value})", value);
			_logger.LogDebug("<<< Exiting ValidateReportTo(value={Value})", value);
                throw new ArgumentException("Reporting to cannot be empty");
            }
        }

        private static void ValidateReportUri(string endpoint)
        {
			_logger.LogDebug(">>> Entering ValidateReportUri(endpoint={Endpoint})", endpoint);
            if (string.IsNullOrWhiteSpace(endpoint))
            {
				_logger.LogWarning("Throwing ArgumentException from ValidateReportUri(endpoint={Endpoint})", endpoint);
                throw new ArgumentException("Reporting endpoint cannot be empty");
            }

            // URL validation regex
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out Uri uriResult))
            {
			_logger.LogDebug("<<< Exiting ValidateReportUri(endpoint={Endpoint})", endpoint);
                throw new ArgumentException($"Invalid reporting endpoint: {endpoint}");
            }
        }

        /// <summary>
        /// Validates reporting endpoint.
        /// </summary>
        private void ValidateReportingEndpoint(string value)
        {
			_logger.LogDebug(">>> Entering ValidateReportingEndpoint(value={Value})", value);
            switch (this.DirectiveType)
            {
                case CspDirectiveType.ReportUri:
                    ValidateReportUri(value);
                    break;
                case CspDirectiveType.ReportTo:
                    ValidateReportTo(value);
                    break;

                    // Add more specific validations as needed
            }
			_logger.LogDebug("<<< Exiting ValidateReportingEndpoint(value={Value})", value);
        }
    }
}
