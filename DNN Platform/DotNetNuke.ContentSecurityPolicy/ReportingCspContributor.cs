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

    /// <summary>
    /// Contributor for reporting directives.
    /// </summary>
    public class ReportingCspContributor : BaseCspContributor
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private bool checkSyntax;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingCspContributor"/> class.
        /// </summary>
        /// <param name="directiveType">The reporting directive type (ReportUri or ReportTo).</param>
        /// <param name="checkSyntax">Check syntax of value.</param>
        public ReportingCspContributor(CspDirectiveType directiveType, bool checkSyntax)
        {
			Logger.LogDebug(">>> Entering ReportingCspContributor(directiveType={DirectiveType},checkSyntax={CheckSyntax})", directiveType, checkSyntax);
            if (directiveType != CspDirectiveType.ReportUri && directiveType != CspDirectiveType.ReportTo)
            {
                throw new ArgumentException("Invalid reporting directive type");
            }

            this.DirectiveType = directiveType;
			Logger.LogDebug("ReportingCspContributor(directiveType={DirectiveType},checkSyntax={CheckSyntax}): this.DirectiveType → {Arg2}", directiveType, checkSyntax, this.DirectiveType);
            this.checkSyntax = checkSyntax;
			Logger.LogDebug("<<< Exiting ReportingCspContributor(directiveType={DirectiveType},checkSyntax={CheckSyntax})", directiveType, checkSyntax);
			Logger.LogDebug("ReportingCspContributor(directiveType={DirectiveType},checkSyntax={CheckSyntax}): this.checkSyntax → {Arg2}", directiveType, checkSyntax, this.checkSyntax);
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
			Logger.LogDebug(">>> Entering AddReportingEndpoint(endpoint={Endpoint})", endpoint);
            if (this.checkSyntax)
            {
                this.ValidateReportingEndpoint(endpoint);
            }

            if (!this.ReportingEndpoints.Contains(endpoint))
            {
                this.ReportingEndpoints.Add(endpoint);
            }
			Logger.LogDebug("<<< Exiting AddReportingEndpoint(endpoint={Endpoint})", endpoint);
        }

        /// <summary>
        /// Removes a reporting endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to remove.</param>
        public void RemoveReportingEndpoint(string endpoint)
        {
			Logger.LogDebug(">>> Entering RemoveReportingEndpoint(endpoint={Endpoint})", endpoint);
            this.ReportingEndpoints.Remove(endpoint);
			Logger.LogDebug("<<< Exiting RemoveReportingEndpoint(endpoint={Endpoint})", endpoint);
        }

        /// <summary>
        /// Generates the directive string.
        /// </summary>
        /// <returns>The directive string.</returns>
        public override string GenerateDirective()
        {
			Logger.LogDebug(">>> Entering GenerateDirective()");
            if (this.ReportingEndpoints.Count == 0)
            {
                return string.Empty;
            }

			Logger.LogDebug("<<< Exiting GenerateDirective()");
            return $"{CspDirectiveNameMapper.GetDirectiveName(this.DirectiveType)} {string.Join(" ", this.ReportingEndpoints)}";
        }

        private static void ValidateReportTo(string value)
        {
			Logger.LogDebug(">>> Entering ValidateReportTo(value={Value})", value);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Reporting to cannot be empty");
            }
			Logger.LogDebug("<<< Exiting ValidateReportTo(value={Value})", value);
        }

        private static void ValidateReportUri(string endpoint)
        {
			Logger.LogDebug(">>> Entering ValidateReportUri(endpoint={Endpoint})", endpoint);
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Reporting endpoint cannot be empty");
            }

            // URL validation regex
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out Uri uriResult))
            {
                throw new ArgumentException($"Invalid reporting endpoint: {endpoint}");
            }
			Logger.LogDebug("<<< Exiting ValidateReportUri(endpoint={Endpoint})", endpoint);
        }

        /// <summary>
        /// Validates reporting endpoint.
        /// </summary>
        private void ValidateReportingEndpoint(string value)
        {
			Logger.LogDebug(">>> Entering ValidateReportingEndpoint(value={Value})", value);
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
			Logger.LogDebug("<<< Exiting ValidateReportingEndpoint(value={Value})", value);
        }
    }
}
