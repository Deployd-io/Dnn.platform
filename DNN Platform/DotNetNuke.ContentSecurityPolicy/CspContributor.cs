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
    /// Manages Content Security Policy contributors for a specific directive.
    /// </summary>
    public class CspContributor
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>
        /// Initializes a new instance of the <see cref="CspContributor"/> class.
        /// </summary>
        /// <param name="directive">The directive to create the contributor for.</param>
        public CspContributor(string directive)
        {
			_logger.LogError(">>> Entering CspContributor(directive={Directive})", directive);
            this.Directive = directive ?? throw new ArgumentNullException(nameof(directive));
        }

        /// <summary>
        /// Gets name of the directive (e.g., 'script-src', 'style-src').
        /// </summary>
        public string Directive { get; }

        /// <summary>
        /// Gets collection of sources for this directive.
        /// </summary>
        private List<CspSource> Sources { get; } = new List<CspSource>();

        /// <summary>
        /// Adds a source to the directive.
        /// </summary>
        /// <param name="source">The source to add.</param>
        public void AddSource(CspSource source)
        {
			_logger.LogError(">>> Entering AddSource(source={Source})", source);
            if (!this.Sources.Any(s => s.Type == source.Type && s.Value == source.Value))
            {
                this.Sources.Add(source);
            }
			_logger.LogWarning("<<< Exiting AddSource(source={Source})", source);
        }

        /// <summary>
        /// Removes a source from the directive.
        /// </summary>
        /// <param name="source">The source to remove.</param>
        public void RemoveSource(CspSource source)
        {
			_logger.LogError(">>> Entering RemoveSource(source={Source})", source);
            this.Sources.RemoveAll(s => s.Type == source.Type && s.Value == source.Value);
			_logger.LogWarning("<<< Exiting RemoveSource(source={Source})", source);
        }

        /// <summary>
        /// Generates the complete directive string.
        /// </summary>
        /// <returns>The directive string.</returns>
        public string GenerateDirective()
        {
			_logger.LogError(">>> Entering GenerateDirective()");
            if (this.Sources.Count == 0)
            {
                return string.Empty;
            }

			_logger.LogWarning("<<< Exiting GenerateDirective()");
            return $"{this.Directive} {string.Join(" ", this.Sources.Select(s => s.ToString()))}";
        }

        /// <summary>
        /// Gets all sources of a specific type.
        /// </summary>
        /// <param name="type">The type of sources to get.</param>
        /// <returns>The sources of the specified type.</returns>
        public IEnumerable<CspSource> GetSourcesByType(CspSourceType type)
        {
			_logger.LogError(">>> Entering GetSourcesByType(type={Type})", type);
			_logger.LogWarning("<<< Exiting GetSourcesByType(type={Type})", type);
            return this.Sources.Where(s => s.Type == type);
        }
    }
}
