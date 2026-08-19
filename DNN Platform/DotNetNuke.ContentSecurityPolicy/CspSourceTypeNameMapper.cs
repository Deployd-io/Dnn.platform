// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;
	using Microsoft.Extensions.Logging;

    /// <summary>
    /// Utility class for converting source types to their string representations.
    /// </summary>
    public static class CspSourceTypeNameMapper
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>
        /// Gets the source type name string.
        /// </summary>
        /// <param name="sourceType">The source type to get the name for.</param>
        /// <returns>The source type name string.</returns>
        public static string GetSourceTypeName(CspSourceType sourceType)
        {
			_logger.LogDebug(">>> Entering GetSourceTypeName(sourceType={SourceType})", sourceType);
			_logger.LogDebug("GetSourceTypeName(sourceType={SourceType}): CspSourceType.Inline → {Arg1}", sourceType, CspSourceType.Inline);
			_logger.LogDebug("GetSourceTypeName(sourceType={SourceType}): CspSourceType.Nonce → {Arg1}", sourceType, CspSourceType.Nonce);
			_logger.LogDebug("GetSourceTypeName(sourceType={SourceType}): CspSourceType.None → {Arg1}", sourceType, CspSourceType.None);
			_logger.LogDebug("GetSourceTypeName(sourceType={SourceType}): CspSourceType.Eval → {Arg1}", sourceType, CspSourceType.Eval);
			_logger.LogDebug("<<< Exiting GetSourceTypeName(sourceType={SourceType})", sourceType);
            return sourceType switch
            {
                CspSourceType.Host => "host",
                CspSourceType.Scheme => "scheme",
                CspSourceType.Self => "'self'",
                CspSourceType.Inline => "'unsafe-inline'",
                CspSourceType.Eval => "'unsafe-eval'",
                CspSourceType.Nonce => "nonce",
                CspSourceType.Hash => "hash",
                CspSourceType.None => "'none'",
                CspSourceType.StrictDynamic => "'strict-dynamic'",
                _ => throw new ArgumentException("Unknown source type"),
            };
        }

        /// <summary>
        /// Gets the source type from a source name string.
        /// </summary>
        /// <param name="sourceName">The source name to get the type for.</param>
        /// <returns>The source type.</returns>
        /// <exception cref="ArgumentException">Thrown when the source name is unknown.</exception>
        public static CspSourceType GetSourceType(string sourceName)
        {
			_logger.LogDebug(">>> Entering GetSourceType(sourceName={SourceName})", sourceName);
            if (string.IsNullOrWhiteSpace(sourceName))
            {
				_logger.LogWarning("Throwing ArgumentException from GetSourceType(sourceName={SourceName})", sourceName);
			_logger.LogDebug("<<< Exiting GetSourceType(sourceName={SourceName})", sourceName);
                throw new ArgumentException("Source name cannot be null or empty", nameof(sourceName));
            }

            return sourceName.ToLowerInvariant() switch
            {
                "'self'" => CspSourceType.Self,
                "'unsafe-inline'" => CspSourceType.Inline,
                "'unsafe-eval'" => CspSourceType.Eval,
                "'none'" => CspSourceType.None,
                "'strict-dynamic'" => CspSourceType.StrictDynamic,
                _ => throw new ArgumentException($"Unknown source name: {sourceName}"),
            };
        }

        /// <summary>
        /// Tries to get the source type from a source name string.
        /// </summary>
        /// <param name="sourceName">The source name to get the type for.</param>
        /// <param name="sourceType">The source type, or default if parsing failed.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        public static bool TryGetSourceType(string sourceName, out CspSourceType sourceType)
        {
			_logger.LogDebug(">>> Entering TryGetSourceType(sourceName={SourceName})", sourceName);
            sourceType = default;

			_logger.LogDebug("TryGetSourceType(sourceName={SourceName}): sourceType → {SourceType}", sourceName, sourceType);
            try
            {
                sourceType = GetSourceType(sourceName);
                return true;
            }
            catch
            {
				_logger.LogError("Exception in TryGetSourceType(sourceName={SourceName}): {Arg1}", sourceName, ex.Message);
				_logger.LogDebug("<<< Exiting TryGetSourceType(sourceName={SourceName})", sourceName);
                return false;
            }
        }

        /// <summary>
        /// Checks if a source string represents a quoted keyword.
        /// </summary>
        /// <param name="source">The source string to check.</param>
        /// <returns>True if the source is a quoted keyword, false otherwise.</returns>
        public static bool IsQuotedKeyword(string source)
        {
			_logger.LogDebug(">>> Entering IsQuotedKeyword(source={Source})", source);
            if (string.IsNullOrWhiteSpace(source))
            {
			_logger.LogDebug("<<< Exiting IsQuotedKeyword(source={Source})", source);
                return false;
            }

            return source.StartsWith("'", StringComparison.OrdinalIgnoreCase) && source.EndsWith("'", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if a source string represents a nonce value.
        /// </summary>
        /// <param name="source">The source string to check.</param>
        /// <returns>True if the source is a nonce value, false otherwise.</returns>
        public static bool IsNonceSource(string source)
        {
			_logger.LogDebug(">>> Entering IsNonceSource(source={Source})", source);
            if (string.IsNullOrWhiteSpace(source))
            {
			_logger.LogDebug("<<< Exiting IsNonceSource(source={Source})", source);
                return false;
            }

            return source.StartsWith("'nonce-", StringComparison.OrdinalIgnoreCase) && source.EndsWith("'", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if a source string represents a hash value.
        /// </summary>
        /// <param name="source">The source string to check.</param>
        /// <returns>True if the source is a hash value, false otherwise.</returns>
        public static bool IsHashSource(string source)
        {
			_logger.LogDebug(">>> Entering IsHashSource(source={Source})", source);
            if (string.IsNullOrWhiteSpace(source))
            {
			_logger.LogDebug("<<< Exiting IsHashSource(source={Source})", source);
                return false;
            }

            return source.StartsWith("'", StringComparison.OrdinalIgnoreCase) && source.EndsWith("'", StringComparison.OrdinalIgnoreCase) &&
                   (source.Contains("sha256-") || source.Contains("sha384-") || source.Contains("sha512-"));
        }
    }
}
