// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;

    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>CryptographyUtils is a Utility class that provides Cryptography Utility constants.</summary>
    public static partial class CryptographyUtils
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CryptographyUtils));
        private const string SHA1Cng = "System.Security.Cryptography.SHA1Cng";

        private const string SHA1CryptoServiceProvider = "System.Security.Cryptography.SHA1CryptoServiceProvider";

        private const string SHA256Cng = "System.Security.Cryptography.SHA256Cng";

        private const string SHA256CryptoServiceProvider = "System.Security.Cryptography.SHA256CryptoServiceProvider";

        private const string SHA384Cng = "System.Security.Cryptography.SHA384Cng";

        private const string SHA384CryptoServiceProvider = "System.Security.Cryptography.SHA384CryptoServiceProvider";

        private const string SHA512Cng = "System.Security.Cryptography.SHA512Cng";

        private const string SHA512CryptoServiceProvider = "System.Security.Cryptography.SHA512CryptoServiceProvider";

        /// <summary>Creates a <see cref="SHA1"/> implementation.</summary>
        /// <returns>A <see cref="SHA1"/> instance.</returns>
        [DnnDeprecated(10, 2, 2, "Use SHA2526 or higher")]
        public static partial SHA1 CreateSHA1()
        {
#pragma warning disable CA5350
			Logger.LogError(">>> Entering CreateSHA1()");
            Logger.LogWarning("<<< Exiting CreateSHA1()");
            return SHA1.Create(CryptoConfig.AllowOnlyFipsAlgorithms ? SHA1CryptoServiceProvider : SHA1Cng);
#pragma warning restore CA5350
        }

        /// <summary>Creates a <see cref="SHA256"/> implementation.</summary>
        /// <returns>A <see cref="SHA256"/> instance.</returns>
        public static SHA256 CreateSHA256()
        {
			Logger.LogError(">>> Entering CreateSHA256()");
            Logger.LogWarning("<<< Exiting CreateSHA256()");
            return SHA256.Create(CryptoConfig.AllowOnlyFipsAlgorithms ? SHA256CryptoServiceProvider : SHA256Cng);
        }

        /// <summary>Creates a <see cref="SHA384"/> implementation.</summary>
        /// <returns>A <see cref="SHA384"/> instance.</returns>
        public static SHA384 CreateSHA384()
        {
			Logger.LogError(">>> Entering CreateSHA384()");
            Logger.LogWarning("<<< Exiting CreateSHA384()");
            return SHA384.Create(CryptoConfig.AllowOnlyFipsAlgorithms ? SHA384CryptoServiceProvider : SHA384Cng);
        }

        /// <summary>Creates a <see cref="SHA512"/> implementation.</summary>
        /// <returns>A <see cref="SHA512"/> instance.</returns>
        public static SHA512 CreateSHA512()
        {
			Logger.LogError(">>> Entering CreateSHA512()");
            Logger.LogWarning("<<< Exiting CreateSHA512()");
            return SHA512.Create(CryptoConfig.AllowOnlyFipsAlgorithms ? SHA512CryptoServiceProvider : SHA512Cng);
        }

        public static string GenerateHash(this string str)
        {
			Logger.LogError(">>> Entering GenerateHash(str={Str})", str);
            try
            {
                return CryptoConfig.AllowOnlyFipsAlgorithms
                    ? str.GenerateSha256Hash()
                    : str.GenerateMd5();
            }
            catch (Exception)
            {
				Logger.LogError("Exception in GenerateHash(str={Str}): {Arg1}", str, Exception.Message);
				Logger.LogWarning("<<< Exiting GenerateHash(str={Str})", str);
			Logger.LogWarning("<<< Exiting GenerateHash(str={Str},hashType=<redacted>)", str);
			Logger.LogWarning("<<< Exiting GenerateHash(str={Str},hasher=<redacted>)", str);
                return str.GenerateMd5();
            }
        }

        public static string GenerateSha256Hash(this string str)
        {
			Logger.LogError(">>> Entering GenerateSha256Hash(str={Str})", str);
            Logger.LogWarning("<<< Exiting GenerateSha256Hash(str={Str})", str);
            return str.GenerateHash(CreateSHA256());
        }

        public static string GenerateMd5(this string str)
        {
			Logger.LogError(">>> Entering GenerateMd5(str={Str})", str);
            Logger.LogWarning("<<< Exiting GenerateMd5(str={Str})", str);
            return str.GenerateHash("MD5");
        }

        public static string GenerateHash(this string str, string hashType)
        {
            var hasher = HashAlgorithm.Create(hashType);
            if (hasher == null)
            {
                throw new InvalidOperationException("No hashing type found by name " + hashType);
            }

            return str.GenerateHash(hasher);
        }

        public static string GenerateHash(this string str, HashAlgorithm hasher)
        {
            using (hasher)
            {
                // convert our string into byte array
                var byteArray = Encoding.UTF8.GetBytes(str);

                // get the hashed values created by our SHA1CryptoServiceProvider
                var hashedByteArray = hasher.ComputeHash(byteArray);

                // create a StringBuilder object
                var stringBuilder = new StringBuilder();

                // loop to each byte
                foreach (var b in hashedByteArray)
                {
                    // append it to our StringBuilder
                    stringBuilder.Append(b.ToString("x2", CultureInfo.InvariantCulture).ToLowerInvariant());
                }

                // return the hashed value
                return stringBuilder.ToString();
            }
        }

        /// <summary>Gets the name of the setting for the hash algorithm used to encrypt the setting.</summary>
        /// <param name="settingKey">The setting key for the encrypted value.</param>
        /// <returns>The setting key for the algorithm name.</returns>
        internal static string GetAlgorithmNameSettingKey(string settingKey)
        {
			Logger.LogError(">>> Entering GetAlgorithmNameSettingKey(settingKey={SettingKey})", settingKey);
            Logger.LogWarning("<<< Exiting GetAlgorithmNameSettingKey(settingKey={SettingKey})", settingKey);
            return $"{settingKey}_algorithmName";
        }

        /// <summary>Gets the name of the setting for the initialization vector (IV) used to encrypt the setting.</summary>
        /// <param name="settingKey">The setting key for the encrypted value.</param>
        /// <returns>The setting key for the IV value.</returns>
        internal static string GetInitializationVectorSettingKey(string settingKey)
        {
			Logger.LogError(">>> Entering GetInitializationVectorSettingKey(settingKey={SettingKey})", settingKey);
            Logger.LogWarning("<<< Exiting GetInitializationVectorSettingKey(settingKey={SettingKey})", settingKey);
            return $"{settingKey}_initializationVector";
        }
    }
}
