// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;

    /// <summary>
    /// Utility class for converting source types to their string representations.
    /// </summary>
    public static class CspSourceTypeNameMapper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>
        /// Gets the source type name string.
        /// </summary>
        /// <param name="sourceType">The source type to get the name for.</param>
        /// <returns>The source type name string.</returns>
        public static string GetSourceTypeName(CspSourceType sourceType)
        {
			Logger.LogDebug(">>> Entering GetSourceTypeName(sourceType={SourceType})", sourceType);
			Logger.LogDebug("GetSourceTypeName(sourceType={SourceType}): CspSourceType.Host → {Arg1}", sourceType, CspSourceType.Host);
			Logger.LogDebug("GetSourceTypeName(sourceType={SourceType}): CspSourceType.Scheme → {Arg1}", sourceType, CspSourceType.Scheme);
			Logger.LogDebug("GetSourceTypeName(sourceType={SourceType}): CspSourceType.Self → {Arg1}", sourceType, CspSourceType.Self);
			Logger.LogDebug("GetSourceTypeName(sourceType={SourceType}): CspSourceType.Inline → {Arg1}", sourceType, CspSourceType.Inline);
			Logger.LogDebug("GetSourceTypeName(sourceType={SourceType}): CspSourceType.Eval → {Arg1}", sourceType, CspSourceType.Eval);
			Logger.LogDebug("GetSourceTypeName(sourceType={SourceType}): CspSourceType.Nonce → {Arg1}", sourceType, CspSourceType.Nonce);
			Logger.LogDebug("GetSourceTypeName(sourceType={SourceType}): CspSourceType.Hash → {Arg1}", sourceType, CspSourceType.Hash);
			Logger.LogDebug("GetSourceTypeName(sourceType={SourceType}): CspSourceType.None → {Arg1}", sourceType, CspSourceType.None);
			Logger.LogDebug("GetSourceTypeName(sourceType={SourceType}): CspSourceType.StrictDynamic → {Arg1}", sourceType, CspSourceType.StrictDynamic);
			Logger.LogDebug("<<< Exiting GetSourceTypeName(sourceType={SourceType})", sourceType);
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
			Logger.LogDebug(">>> Entering GetSourceType(sourceName={SourceName})", sourceName);
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                throw new ArgumentException("Source name cannot be null or empty", nameof(sourceName));
            }

			Logger.LogDebug("<<< Exiting GetSourceType(sourceName={SourceName})", sourceName);
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
			Logger.LogDebug(">>> Entering TryGetSourceType(sourceName={SourceName})", sourceName);
            sourceType = default;

			Logger.LogDebug("TryGetSourceType(sourceName={SourceName}): sourceType → {SourceType}", sourceName, sourceType);
            try
            {
                sourceType = GetSourceType(sourceName);
                return true;
            }
            catch
            {
				Logger.LogError("Exception in TryGetSourceType(sourceName={SourceName}): {Arg1}", sourceName, ex.Message);
				Logger.LogDebug("<<< Exiting TryGetSourceType(sourceName={SourceName})", sourceName);
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
			Logger.LogDebug(">>> Entering IsQuotedKeyword(source={Source})", source);
            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

			Logger.LogDebug("<<< Exiting IsQuotedKeyword(source={Source})", source);
            return source.StartsWith("'", StringComparison.OrdinalIgnoreCase) && source.EndsWith("'", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if a source string represents a nonce value.
        /// </summary>
        /// <param name="source">The source string to check.</param>
        /// <returns>True if the source is a nonce value, false otherwise.</returns>
        public static bool IsNonceSource(string source)
        {
			Logger.LogDebug(">>> Entering IsNonceSource(source={Source})", source);
            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

			Logger.LogDebug("<<< Exiting IsNonceSource(source={Source})", source);
            return source.StartsWith("'nonce-", StringComparison.OrdinalIgnoreCase) && source.EndsWith("'", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if a source string represents a hash value.
        /// </summary>
        /// <param name="source">The source string to check.</param>
        /// <returns>True if the source is a hash value, false otherwise.</returns>
        public static bool IsHashSource(string source)
        {
			Logger.LogDebug(">>> Entering IsHashSource(source={Source})", source);
            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

			Logger.LogDebug("<<< Exiting IsHashSource(source={Source})", source);
            return source.StartsWith("'", StringComparison.OrdinalIgnoreCase) && source.EndsWith("'", StringComparison.OrdinalIgnoreCase) &&
                   (source.Contains("sha256-") || source.Contains("sha384-") || source.Contains("sha512-"));
        }
    }
}
