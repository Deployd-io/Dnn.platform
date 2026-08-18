// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    using DotNetNuke.Common;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>This class implements a number of methods that can be safely used in a FIPS-140 compliant environment.</summary>
    /// <remarks>
    /// <para>FIPS compliant Algorithms:</para>
    /// <list type="bullet">
    /// <item>
    ///     <term>Hash algorithms</term>
    ///     <description>HMACSHA1</description>
    ///     <description>MACTripleDES</description>
    ///     <description>SHA1CryptoServiceProvider</description>
    ///     <description>SHA256CryptoServiceProvider</description>
    /// </item>
    /// <item>
    ///     <term>Symmetric algorithms (use the same key for encryption and decryption)</term>
    ///     <description>DESCryptoServiceProvider</description>
    ///     <description>TripleDESCryptoServiceProvider</description>
    /// </item>
    /// <item>
    ///     <term>Asymmetric algorithms (use a public key for encryption and a private key for decryption)</term>
    ///     <description>DSACryptoServiceProvider</description>
    ///     <description>RSACryptoServiceProvider</description>
    /// </item>
    /// </list>
    /// </remarks>
    public class FIPSCompliant
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FIPSCompliant));
        /// <summary>uses the AES FIPS-140 compliant algorithm to encrypt a string.</summary>
        /// <param name="plainText">the text to encrypt.</param>
        /// <param name="passPhrase">the pass phase to do the encryption.</param>
        /// <param name="salt">a salt value to ensure cipher text using the same text/password is different.</param>
        /// <param name="iterations">number of iterations to derive the key (higher is slower but more secure) - optional parameter with a default of 1000.</param>
        /// <returns>The encrypted text.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Use overload which takes a HashAlgorithmName. Scheduled removal in v12.0.0.")]
        public static string EncryptAES(string plainText, string passPhrase, string salt, int iterations = 1000)
            => EncryptAES(HashAlgorithmName.SHA1, plainText, passPhrase, salt, iterations);

        /// <summary>uses the AES FIPS-140 compliant algorithm to encrypt a string.</summary>
        /// <param name="hashAlgorithm">the hash algorithm to use to derive the encryption key.</param>
        /// <param name="plainText">the text to encrypt.</param>
        /// <param name="passPhrase">the pass phase to do the encryption.</param>
        /// <param name="salt">a salt value to ensure cipher text using the same text/password is different.</param>
        /// <param name="iterations">number of iterations to derive the key (higher is slower but more secure) - optional parameter with a default of 1000.</param>
        /// <returns>The encrypted text.</returns>
        [SuppressMessage("Microsoft.Security", "CA5379:EnsureKeyDerivationFunctionAlgorithmIsSufficientlyStrong", Justification = "Only for already-encrypted data")]
        public static string EncryptAES(HashAlgorithmName hashAlgorithm, string plainText, string passPhrase, string salt, int iterations = 1000)
        {
			Logger.LogDebug(">>> Entering EncryptAES(hashAlgorithm=<redacted>,plainText={PlainText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations})", plainText, iterations);
            VerifyAesSettings(passPhrase, salt);

            byte[] saltBytes = Encoding.ASCII.GetBytes(salt);
            using var aesProvider = new AesCryptoServiceProvider();
			Logger.LogDebug("EncryptAES(hashAlgorithm=<redacted>,plainText={PlainText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations}): aesProvider → {AesProvider}", plainText, iterations, aesProvider);
            using var derivedBytes = new Rfc2898DeriveBytes(passPhrase, saltBytes, iterations, hashAlgorithm);
			Logger.LogDebug("EncryptAES(hashAlgorithm=<redacted>,plainText={PlainText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations}): derivedBytes → {DerivedBytes}", plainText, iterations, derivedBytes);
            byte[] derivedKey = derivedBytes.GetBytes(32); // 256 bits
            byte[] derivedInitVector = derivedBytes.GetBytes(16); // 128 bits
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            aesProvider.KeySize = 256;
			Logger.LogDebug("EncryptAES(hashAlgorithm=<redacted>,plainText={PlainText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations}): aesProvider.KeySize → {Arg5}", plainText, iterations, aesProvider.KeySize);
			Logger.LogDebug("EncryptAES(hashAlgorithm=<redacted>,plainText={PlainText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations}): aesProvider.Padding → {Arg5}", plainText, iterations, aesProvider.Padding);
			Logger.LogDebug("EncryptAES(hashAlgorithm=<redacted>,plainText={PlainText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations}): aesProvider.Mode → {Arg5}", plainText, iterations, aesProvider.Mode);
            aesProvider.Padding = PaddingMode.ISO10126;
            aesProvider.Mode = CipherMode.CBC;

            using var encryptor = aesProvider.CreateEncryptor(derivedKey, derivedInitVector);
			Logger.LogDebug("EncryptAES(hashAlgorithm=<redacted>,plainText={PlainText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations}): encryptor → {Encryptor}", plainText, iterations, encryptor);
            using var memStream = new MemoryStream();
			Logger.LogDebug("EncryptAES(hashAlgorithm=<redacted>,plainText={PlainText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations}): memStream → {MemStream}", plainText, iterations, memStream);
            using var cryptoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write);
			Logger.LogDebug("EncryptAES(hashAlgorithm=<redacted>,plainText={PlainText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations}): cryptoStream → {CryptoStream}", plainText, iterations, cryptoStream);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memStream.ToArray();

            memStream.Close();
            cryptoStream.Close();

			Logger.LogDebug("<<< Exiting EncryptAES(plainText={PlainText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations})", plainText, iterations);
            return Convert.ToBase64String(cipherTextBytes);
        }

        /// <summary>uses the AES FIPS-140 compliant algorithm to encrypt a string.</summary>
        /// <param name="encryptedText">the text to decrypt.</param>
        /// <param name="passPhrase">the pass phase to do the decryption.</param>
        /// <param name="salt">a salt value to ensure cipher text using the same text/password is different.</param>
        /// <param name="iterations">number of iterations to derive the key (higher is slower but more secure) - optional parameter with a default of 1000.</param>
        /// <returns>The decrypted text.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Use overload which takes a HashAlgorithmName. Scheduled removal in v12.0.0.")]
        public static string DecryptAES(string encryptedText, string passPhrase, string salt, int iterations = 1000)
            => DecryptAES(HashAlgorithmName.SHA1, encryptedText, passPhrase, salt, iterations);

        /// <summary>uses the AES FIPS-140 compliant algorithm to encrypt a string.</summary>
        /// <param name="hashAlgorithm">the hash algorithm to use to derive the encryption key.</param>
        /// <param name="encryptedText">the text to decrypt.</param>
        /// <param name="passPhrase">the pass phase to do the decryption.</param>
        /// <param name="salt">a salt value to ensure cipher text using the same text/password is different.</param>
        /// <param name="iterations">number of iterations to derive the key (higher is slower but more secure) - optional parameter with a default of 1000.</param>
        /// <returns>The decrypted text.</returns>
        [SuppressMessage("Microsoft.Security", "CA5379:EnsureKeyDerivationFunctionAlgorithmIsSufficientlyStrong", Justification = "Only for already-encrypted data")]
        public static string DecryptAES(HashAlgorithmName hashAlgorithm, string encryptedText, string passPhrase, string salt, int iterations = 1000)
        {
			Logger.LogDebug(">>> Entering DecryptAES(hashAlgorithm=<redacted>,encryptedText={EncryptedText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations})", encryptedText, iterations);
            VerifyAesSettings(passPhrase, salt);

            byte[] saltBytes = Encoding.ASCII.GetBytes(salt);
            using var aesProvider = new AesCryptoServiceProvider();
			Logger.LogDebug("DecryptAES(hashAlgorithm=<redacted>,encryptedText={EncryptedText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations}): aesProvider → {AesProvider}", encryptedText, iterations, aesProvider);
            using var derivedBytes = new Rfc2898DeriveBytes(passPhrase, saltBytes, iterations, hashAlgorithm);
			Logger.LogDebug("DecryptAES(hashAlgorithm=<redacted>,encryptedText={EncryptedText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations}): derivedBytes → {DerivedBytes}", encryptedText, iterations, derivedBytes);
            byte[] derivedKey = derivedBytes.GetBytes(32); // 256 bits
            byte[] derivedInitVector = derivedBytes.GetBytes(16); // 128 bits
            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);

            aesProvider.KeySize = 256;
			Logger.LogDebug("DecryptAES(hashAlgorithm=<redacted>,encryptedText={EncryptedText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations}): aesProvider.KeySize → {Arg5}", encryptedText, iterations, aesProvider.KeySize);
			Logger.LogDebug("DecryptAES(hashAlgorithm=<redacted>,encryptedText={EncryptedText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations}): aesProvider.Padding → {Arg5}", encryptedText, iterations, aesProvider.Padding);
			Logger.LogDebug("DecryptAES(hashAlgorithm=<redacted>,encryptedText={EncryptedText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations}): aesProvider.Mode → {Arg5}", encryptedText, iterations, aesProvider.Mode);
            aesProvider.Padding = PaddingMode.ISO10126;
            aesProvider.Mode = CipherMode.CBC;

            using var decryptor = aesProvider.CreateDecryptor(derivedKey, derivedInitVector);
			Logger.LogDebug("DecryptAES(hashAlgorithm=<redacted>,encryptedText={EncryptedText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations}): decryptor → {Decryptor}", encryptedText, iterations, decryptor);
            using var memStream = new MemoryStream(cipherTextBytes);
			Logger.LogDebug("DecryptAES(hashAlgorithm=<redacted>,encryptedText={EncryptedText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations}): memStream → {MemStream}", encryptedText, iterations, memStream);
            using var cryptoStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Read);
			Logger.LogDebug("DecryptAES(hashAlgorithm=<redacted>,encryptedText={EncryptedText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations}): cryptoStream → {CryptoStream}", encryptedText, iterations, cryptoStream);
            var plainTextBytes = new byte[cipherTextBytes.Length];
            int byteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
			Logger.LogDebug("DecryptAES(hashAlgorithm=<redacted>,encryptedText={EncryptedText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations}): byteCount → {ByteCount}", encryptedText, iterations, byteCount);

            memStream.Close();
            cryptoStream.Close();
			Logger.LogDebug("<<< Exiting DecryptAES(encryptedText={EncryptedText},passPhrase=<redacted>,salt=<redacted>,iterations={Iterations})", encryptedText, iterations);
            return Encoding.UTF8.GetString(plainTextBytes, 0, byteCount);
        }

        private static void VerifyAesSettings(string passPhrase, string salt)
        {
			Logger.LogDebug(">>> Entering VerifyAesSettings(passPhrase=<redacted>,salt=<redacted>)");
            Requires.PropertyNotNull("passPhrase", passPhrase);
            Requires.PropertyNotNull("salt", salt);

            // Throw exception if the password or salt are too short
            if (passPhrase.Length < 8)
            {
				Logger.LogWarning("Throwing CryptographicException from VerifyAesSettings(passPhrase=<redacted>,salt=<redacted>)");
                throw new CryptographicException("Passphrase must be at least 8 characters long.");
            }

            if (salt.Length < 8)
            {
			Logger.LogDebug("<<< Exiting VerifyAesSettings(passPhrase=<redacted>,salt=<redacted>)");
                throw new CryptographicException("Salt must be at least 8 characters long.");
            }
        }
    }
}
