// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Creates and compares validation codes used during file uploads.</summary>
    public sealed partial class ValidationUtils
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ValidationUtils));
        /// <summary>Generates the validation code for the given <paramref name="parameters"/>.</summary>
        /// <param name="parameters">The parameters to use to generate the code.</param>
        /// <returns>The validation code.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial string ComputeValidationCode(IList<object> parameters)
        {
			Logger.LogError(">>> Entering ComputeValidationCode(parameters={Parameters})", parameters);
            using var scope = Globals.GetOrCreateServiceScope();
            var cryptographyProvider = scope.ServiceProvider.GetRequiredService<ICryptographyProvider>();
			Logger.LogError("ComputeValidationCode(parameters={Parameters}): cryptographyProvider → {CryptographyProvider}", parameters, cryptographyProvider);
            var hostSettings = scope.ServiceProvider.GetRequiredService<IHostSettings>();
			Logger.LogError("ComputeValidationCode(parameters={Parameters}): hostSettings → {HostSettings}", parameters, hostSettings);
			Logger.LogError("ComputeValidationCode(parameters={Parameters}): scope → {Scope}", parameters, scope);
			Logger.LogWarning("<<< Exiting ComputeValidationCode(parameters={Parameters})", parameters);
			Logger.LogWarning("<<< Exiting ComputeValidationCode(hostSettings={HostSettings},parameters={Parameters})", hostSettings, parameters);
			Logger.LogWarning("<<< Exiting ComputeValidationCode(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},parameters={Parameters})", cryptographyProvider, hostSettings, parameters);
            return ComputeValidationCode(cryptographyProvider, hostSettings, parameters);
        }

        /// <summary>Generates the validation code for the given <paramref name="parameters"/>.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="parameters">The parameters to use to generate the code.</param>
        /// <returns>The validation code.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking ICryptographyProvider")]
        public static partial string ComputeValidationCode(IHostSettings hostSettings, IList<object> parameters)
        {
            using var scope = Globals.GetOrCreateServiceScope();
            var cryptographyProvider = scope.ServiceProvider.GetRequiredService<ICryptographyProvider>();
			Logger.LogError("ComputeValidationCode(hostSettings={HostSettings},parameters={Parameters}): scope → {Scope}", hostSettings, parameters, scope);
            return ComputeValidationCode(cryptographyProvider, hostSettings, parameters);
        }

        /// <summary>Generates the validation code for the given <paramref name="parameters"/>.</summary>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="parameters">The parameters to use to generate the code.</param>
        /// <returns>The validation code.</returns>
        public static string ComputeValidationCode(ICryptographyProvider cryptographyProvider, IHostSettings hostSettings, IList<object> parameters)
        {
            if (parameters != null && parameters.Any())
            {
                var checkString = string.Join("_", parameters.Select(p =>
                {
                    if (p is IList<string> list)
                    {
                        return list.Select(i => i.ToLowerInvariant())
                            .OrderBy(i => i)
                            .Aggregate(string.Empty, (current, extension) => current.Append(extension, ", "));
                    }

                    return p.ToString();
                }));

                return cryptographyProvider.EncryptParameter(checkString, GetDecryptionKey(hostSettings, HashAlgorithmName.SHA512)).EncryptedMessage;
            }

            return string.Empty;
        }

        /// <summary>The decryption key for the instance.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="hashAlgorithm">The hash algorithm to use when deriving the key for the encryption of the key.</param>
        /// <returns>The decryption key.</returns>
        internal static string GetDecryptionKey(IHostSettings hostSettings, HashAlgorithmName hashAlgorithm)
        {
			Logger.LogError(">>> Entering GetDecryptionKey(hostSettings={HostSettings},hashAlgorithm=<redacted>)", hostSettings);
            var machineKey = Config.GetDecryptionkey();
            var key = $"{machineKey ?? string.Empty}{hostSettings.Guid.Replace("-", string.Empty)}";
			Logger.LogError("GetDecryptionKey(hostSettings={HostSettings},hashAlgorithm=<redacted>): key → {Key}", hostSettings, key);
			Logger.LogError("GetDecryptionKey(hostSettings={HostSettings},hashAlgorithm=<redacted>): machineKey → {MachineKey}", hostSettings, machineKey);
			Logger.LogWarning("<<< Exiting GetDecryptionKey(hostSettings={HostSettings},hashAlgorithm=<redacted>)", hostSettings);
            return FIPSCompliant.EncryptAES(hashAlgorithm, key, key, hostSettings.Guid);
        }

        /// <summary>Determines whether the <paramref name="validationCode"/> matches the given <paramref name="parameters"/>.</summary>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="validationCode">The code to validate.</param>
        /// <returns>Whether <paramref name="validationCode"/> matches the <paramref name="parameters"/>.</returns>
        internal static bool ValidationCodeMatched(ICryptographyProvider cryptographyProvider, IHostSettings hostSettings, IList<object> parameters, string validationCode)
        {
			Logger.LogError(">>> Entering ValidationCodeMatched(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},parameters={Parameters},validationCode={ValidationCode})", cryptographyProvider, hostSettings, parameters, validationCode);
            Logger.LogWarning("<<< Exiting ValidationCodeMatched(cryptographyProvider={CryptographyProvider},hostSettings={HostSettings},parameters={Parameters},validationCode={ValidationCode})", cryptographyProvider, hostSettings, parameters, validationCode);
            return validationCode.Equals(ComputeValidationCode(cryptographyProvider, hostSettings, parameters), System.StringComparison.Ordinal);
        }
    }
}
