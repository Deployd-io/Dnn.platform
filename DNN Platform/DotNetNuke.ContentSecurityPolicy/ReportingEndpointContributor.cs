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
    public class ReportingEndpointContributor : BaseCspContributor
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private bool checkSyntax;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingEndpointContributor"/> class.
        /// </summary>
        /// <param name="directiveType">The reporting directive type (ReportUri).</param>
        /// <param name="checkSyntax">Check syntax of value.</param>
        public ReportingEndpointContributor(CspDirectiveType directiveType, bool checkSyntax)
        {
			_logger.LogError(">>> Entering ReportingEndpointContributor(directiveType={DirectiveType},checkSyntax={CheckSyntax})", directiveType, checkSyntax);
            if (directiveType != CspDirectiveType.ReportUri)
            {
				_logger.LogError("Throwing ArgumentException from ReportingEndpointContributor(directiveType={DirectiveType},checkSyntax={CheckSyntax})", directiveType, checkSyntax);
                throw new ArgumentException("Invalid reporting directive type");
            }
			_logger.LogWarning("<<< Exiting ReportingEndpointContributor(directiveType={DirectiveType},checkSyntax={CheckSyntax})", directiveType, checkSyntax);

            this.DirectiveType = directiveType;
			_logger.LogDebug("ReportingEndpointContributor(directiveType={DirectiveType},checkSyntax={CheckSyntax}): this.DirectiveType → {Arg2}", directiveType, checkSyntax, this.DirectiveType);
            this.checkSyntax = checkSyntax;
        }

        /// <summary>
        /// Gets collection of reporting endpoints.
        /// </summary>
        private Dictionary<string, string> ReportingEndpoints { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Adds a reporting endpoint.
        /// </summary>
        /// <param name="name">The name of the endpoint where reports will be sent.</param>
        /// <param name="endpoint">The URL of the endpoint where reports will be sent.</param>
        public void AddReportingEndpoint(string name, string endpoint)
        {
			_logger.LogError(">>> Entering AddReportingEndpoint(name={Name},endpoint={Endpoint})", name, endpoint);
            if (this.checkSyntax)
            {
                this.ValidateReportingEndpoint(endpoint);
            }

            if (!this.ReportingEndpoints.ContainsKey(name))
            {
                this.ReportingEndpoints.Add(name, endpoint);
            }
			_logger.LogWarning("<<< Exiting AddReportingEndpoint(name={Name},endpoint={Endpoint})", name, endpoint);
        }

        /// <summary>
        /// Removes a reporting endpoint.
        /// </summary>
        /// <param name="name">The endpoint to remove.</param>
        public void RemoveReportingEndpoint(string name)
        {
			_logger.LogError(">>> Entering RemoveReportingEndpoint(name={Name})", name);
            this.ReportingEndpoints.Remove(name);
			_logger.LogWarning("<<< Exiting RemoveReportingEndpoint(name={Name})", name);
        }

        /// <summary>
        /// Generates the directive string.
        /// </summary>
        /// <returns>The directive string.</returns>
        public override string GenerateDirective()
        {
			_logger.LogError(">>> Entering GenerateDirective()");
            if (this.ReportingEndpoints.Count == 0)
            {
                return string.Empty;
            }

            var endpoints = this.ReportingEndpoints.Select(ep => $"{ep.Key}=\"{ep.Value}\"").ToList();
			_logger.LogError("GenerateDirective(): endpoints → {Endpoints}", endpoints);
			_logger.LogWarning("<<< Exiting GenerateDirective()");
            return $"{string.Join(" ", endpoints)}";
        }

        private static void ValidateReportTo(string value)
        {
			_logger.LogError(">>> Entering ValidateReportTo(value={Value})", value);
            if (string.IsNullOrWhiteSpace(value))
            {
				_logger.LogError("Throwing ArgumentException from ValidateReportTo(value={Value})", value);
                throw new ArgumentException("Reporting to cannot be empty");
            }
			_logger.LogWarning("<<< Exiting ValidateReportTo(value={Value})", value);
        }

        private static void ValidateReportUri(string endpoint)
        {
			_logger.LogError(">>> Entering ValidateReportUri(endpoint={Endpoint})", endpoint);
            if (string.IsNullOrWhiteSpace(endpoint))
            {
				_logger.LogError("Throwing ArgumentException from ValidateReportUri(endpoint={Endpoint})", endpoint);
                throw new ArgumentException("Reporting endpoint cannot be empty");
            }

            // URL validation regex
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out Uri uriResult))
            {
                throw new ArgumentException($"Invalid reporting endpoint: {endpoint}");
            }
			_logger.LogWarning("<<< Exiting ValidateReportUri(endpoint={Endpoint})", endpoint);
        }

        /// <summary>
        /// Validates reporting endpoint.
        /// </summary>
        private void ValidateReportingEndpoint(string value)
        {
			_logger.LogError(">>> Entering ValidateReportingEndpoint(value={Value})", value);
            switch (this.DirectiveType)
            {
                case CspDirectiveType.ReportUri:
                    ValidateReportUri(value);
                    break;
                case CspDirectiveType.ReportTo:
                    ValidateReportTo(value);
                    break;
            }
			_logger.LogWarning("<<< Exiting ValidateReportingEndpoint(value={Value})", value);
        }
    }
}
