// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Cryptography
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>A base class for an <see cref="ICryptographyProvider"/> implementation.</summary>
    [DnnDeprecated(10, 2, 2, "Use DotNetNuke.Abstractions.Security.ICryptographyProvider")]
    public abstract partial class CryptographyProvider
        : ICryptographyProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CryptographyProvider));
        /// <summary>Gets the algorithm name used by <see cref="EncryptParameter(string,string)"/>.</summary>
        public virtual string EncryptParameterAlgorithmName => nameof(DESCryptoServiceProvider);

        /// <summary>Gets the algorithm name used by <see cref="EncryptString(string,string)"/>.</summary>
        public string EncryptStringAlgorithmName => $"{this.EncryptStringHashAlgorithmName}|{this.EncryptStringSymmetricAlgorithmName}";

        /// <summary>Gets the algorithm name used by <see cref="EncryptString(string,string)"/>.</summary>
        protected virtual string EncryptStringHashAlgorithmName => nameof(MD5CryptoServiceProvider);

        /// <summary>Gets the algorithm name used by <see cref="EncryptString(string,string)"/>.</summary>
        protected virtual string EncryptStringSymmetricAlgorithmName => nameof(TripleDESCryptoServiceProvider);

        /// <summary>Gets an instance of <see cref="CryptographyProvider"/>.</summary>
        /// <returns>A <see cref="CryptographyProvider"/> instance.</returns>
        public static CryptographyProvider Instance()
        {
			Logger.LogError(">>> Entering Instance()");
            Logger.LogWarning("<<< Exiting Instance()");
            return ComponentFactory.GetComponent<CryptographyProvider>();
        }

        /// <inheritdoc />
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Use DotNetNuke.Abstractions.Security.ICryptographyProvider. Scheduled for removal in v12.0.0.")]
        public abstract string EncryptParameter(string message, string passphrase);

        /// <inheritdoc />
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Use DotNetNuke.Abstractions.Security.ICryptographyProvider. Scheduled for removal in v12.0.0.")]
        public abstract string DecryptParameter(string message, string passphrase);

        /// <inheritdoc />
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Use DotNetNuke.Abstractions.Security.ICryptographyProvider. Scheduled for removal in v12.0.0.")]
        public abstract string EncryptString(string message, string passphrase);

        /// <inheritdoc />
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Use DotNetNuke.Abstractions.Security.ICryptographyProvider. Scheduled for removal in v12.0.0.")]
        public abstract string DecryptString(string message, string passphrase);

        /// <inheritdoc cref="DotNetNuke.Abstractions.Security.ICryptographyProvider.DecryptParameter" />
        public string DecryptParameter(string message, string passphrase, string algorithmName)
        {
			Logger.LogError(">>> Entering DecryptParameter(message={Message},passphrase=<redacted>,algorithmName={AlgorithmName})", message, algorithmName);
            if (string.IsNullOrEmpty(passphrase) || string.IsNullOrEmpty(message))
            {
                return string.Empty;
            }

            // convert data to byte array and Base64 decode
            try
            {
                using var algorithm = CreateSymmetricAlgorithm(algorithmName);
				Logger.LogError("DecryptParameter(message={Message},passphrase=<redacted>,algorithmName={AlgorithmName}): algorithm → {Algorithm}", message, algorithmName, algorithm);
                var keyLength = BitsToBytes(algorithm.KeySize);
				Logger.LogError("DecryptParameter(message={Message},passphrase=<redacted>,algorithmName={AlgorithmName}): keyLength → {KeyLength}", message, algorithmName, keyLength);
                var blockLength = BitsToBytes(algorithm.BlockSize);
				Logger.LogError("DecryptParameter(message={Message},passphrase=<redacted>,algorithmName={AlgorithmName}): blockLength → {BlockLength}", message, algorithmName, blockLength);
                if (passphrase.Length < keyLength + blockLength)
                {
                    passphrase += new string('X', (keyLength + blockLength) - passphrase.Length);
                }

                // create encryption keys
                byte[] byteKey = Encoding.UTF8.GetBytes(passphrase.Substring(0, keyLength));
                byte[] byteVector = Encoding.UTF8.GetBytes(passphrase.Substring(keyLength, blockLength));

                byte[] byteData = Convert.FromBase64String(message);

                // decrypt
                using var objMemoryStream = new MemoryStream();
				Logger.LogError("DecryptParameter(message={Message},passphrase=<redacted>,algorithmName={AlgorithmName}): objMemoryStream → {ObjMemoryStream}", message, algorithmName, objMemoryStream);
                using (var objCryptoStream = new CryptoStream(objMemoryStream, algorithm.CreateDecryptor(byteKey, byteVector), CryptoStreamMode.Write))
                {
                    objCryptoStream.Write(byteData, 0, byteData.Length);
                    objCryptoStream.FlushFinalBlock();
                }

                // convert to string
                var array = objMemoryStream.ToArray();
				Logger.LogError("DecryptParameter(message={Message},passphrase=<redacted>,algorithmName={AlgorithmName}): array → {Array}", message, algorithmName, array);
                return Encoding.UTF8.GetString(array);
            }
            catch
            {
				Logger.LogError("Exception in DecryptParameter(message={Message},passphrase=<redacted>,algorithmName={AlgorithmName}): {Arg3}", message, algorithmName, ex.Message);
                // decryption error
				Logger.LogWarning("<<< Exiting DecryptParameter(message={Message},passphrase=<redacted>,algorithmName={AlgorithmName})", message, algorithmName);
                return string.Empty;
            }
        }

        /// <inheritdoc cref="DotNetNuke.Abstractions.Security.ICryptographyProvider.DecryptString" />
        public string DecryptString(string message, string passphrase, string algorithmName, string initializationVector)
        {
			Logger.LogError(">>> Entering DecryptString(message={Message},passphrase=<redacted>,algorithmName={AlgorithmName},initializationVector={InitializationVector})", message, algorithmName, initializationVector);
            byte[] results;
            var utf8 = new UTF8Encoding();
			Logger.LogError("DecryptString(message={Message},passphrase=<redacted>,algorithmName={AlgorithmName},initializationVector={InitializationVector}): utf8 → {Utf8}", message, algorithmName, initializationVector, utf8);

            var algorithmNames = algorithmName.Split(['|',], 2);
			Logger.LogError("DecryptString(message={Message},passphrase=<redacted>,algorithmName={AlgorithmName},initializationVector={InitializationVector}): algorithmNames → {AlgorithmNames}", message, algorithmName, initializationVector, algorithmNames);
            var hashAlgorithmName = algorithmNames[0];
            var symmetricAlgorithmName = algorithmNames[1];

            using (var hashAlgorithm = CreateHashAlgorithm(hashAlgorithmName))
            {
                byte[] key = hashAlgorithm.ComputeHash(utf8.GetBytes(passphrase));
                using var symmetricAlgorithm = CreateSymmetricAlgorithm(symmetricAlgorithmName, key, initializationVector);
				Logger.LogError("DecryptString(message={Message},passphrase=<redacted>,algorithmName={AlgorithmName},initializationVector={InitializationVector}): symmetricAlgorithm → {SymmetricAlgorithm}", message, algorithmName, initializationVector, symmetricAlgorithm);

                byte[] dataToDecrypt = Convert.FromBase64String(message);
                try
                {
                    using var decryptor = symmetricAlgorithm.CreateDecryptor();
					Logger.LogError("DecryptString(message={Message},passphrase=<redacted>,algorithmName={AlgorithmName},initializationVector={InitializationVector}): decryptor → {Decryptor}", message, algorithmName, initializationVector, decryptor);
                    results = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
					Logger.LogError("DecryptString(message={Message},passphrase=<redacted>,algorithmName={AlgorithmName},initializationVector={InitializationVector}): results → {Results}", message, algorithmName, initializationVector, results);
                }
                finally
                {
					Logger.LogDebug("DecryptString(message={Message},passphrase=<redacted>,algorithmName={AlgorithmName},initializationVector={InitializationVector}): results → {Results}", message, algorithmName, initializationVector, results);
                    symmetricAlgorithm.Clear();
                    hashAlgorithm.Clear();
                }
            }

			Logger.LogWarning("<<< Exiting DecryptString(message={Message},passphrase=<redacted>,algorithmName={AlgorithmName},initializationVector={InitializationVector})", message, algorithmName, initializationVector);
            return utf8.GetString(results);
        }

        /// <summary>simple method that uses basic encryption to safely encode parameters.</summary>
        /// <param name="message">the text to be encrypted (encoded).</param>
        /// <param name="passphrase">the key to perform the encryption.</param>
        /// <param name="algorithm">the symmetric algorithm to use for the encryption.</param>
        /// <returns>The encrypted string.</returns>
        protected static string EncryptParameter(string message, string passphrase, SymmetricAlgorithm algorithm)
        {
			Logger.LogError(">>> Entering EncryptParameter(message={Message},passphrase=<redacted>,algorithm={Algorithm})", message, algorithm);
            string value;
            if (!string.IsNullOrEmpty(passphrase))
            {
                var keyLength = BitsToBytes(algorithm.KeySize);
				Logger.LogError("EncryptParameter(message={Message},passphrase=<redacted>,algorithm={Algorithm}): keyLength → {KeyLength}", message, algorithm, keyLength);
                var blockLength = BitsToBytes(algorithm.BlockSize);
				Logger.LogError("EncryptParameter(message={Message},passphrase=<redacted>,algorithm={Algorithm}): blockLength → {BlockLength}", message, algorithm, blockLength);
                if (passphrase.Length < keyLength + blockLength)
                {
                    passphrase += new string('X', (keyLength + blockLength) - passphrase.Length);
                }

                // create encryption keys
                byte[] byteKey = Encoding.UTF8.GetBytes(passphrase.Substring(0, keyLength));
                byte[] byteVector = Encoding.UTF8.GetBytes(passphrase.Substring(keyLength, blockLength));

                // convert data to byte array
                byte[] byteData = Encoding.UTF8.GetBytes(message);

                // encrypt
                using var objMemoryStream = new MemoryStream();
				Logger.LogError("EncryptParameter(message={Message},passphrase=<redacted>,algorithm={Algorithm}): objMemoryStream → {ObjMemoryStream}", message, algorithm, objMemoryStream);
                using var objCryptoStream = new CryptoStream(objMemoryStream, algorithm.CreateEncryptor(byteKey, byteVector), CryptoStreamMode.Write);
				Logger.LogError("EncryptParameter(message={Message},passphrase=<redacted>,algorithm={Algorithm}): objCryptoStream → {ObjCryptoStream}", message, algorithm, objCryptoStream);
                objCryptoStream.Write(byteData, 0, byteData.Length);
                objCryptoStream.FlushFinalBlock();

                // convert to string and Base64 encode
                value = Convert.ToBase64String(objMemoryStream.ToArray());
				Logger.LogError("EncryptParameter(message={Message},passphrase=<redacted>,algorithm={Algorithm}): value → {Value}", message, algorithm, value);
            }
            else
            {
                value = message;
				Logger.LogDebug("EncryptParameter(message={Message},passphrase=<redacted>,algorithm={Algorithm}): value → {Value}", message, algorithm, value);
            }

			Logger.LogWarning("<<< Exiting EncryptParameter(message={Message},passphrase=<redacted>,algorithm={Algorithm})", message, algorithm);
            return value;
        }

        /// <summary>safely encrypt sensitive data.</summary>
        /// <param name="message">the text to be encrypted.</param>
        /// <param name="passphrase">the key to perform the encryption.</param>
        /// <param name="hashAlgorithm">the hash algorithm used to derive the encryption key.</param>
        /// <param name="hashAlgorithmName">the name of the <paramref name="hashAlgorithm"/>.</param>
        /// <param name="createSymmetricAlgorithm">A function which takes the key generated by the <paramref name="hashAlgorithm"/> and creates a symmetric algorithm to do the encryption.</param>
        /// <param name="symmetricAlgorithmName">The name of the algorithm created by <paramref name="createSymmetricAlgorithm"/>.</param>
        /// <returns>A tuple of the encrypted string, the name of the algorithm used, and the initialization vector (or <see langword="null"/> if the algorithm doesn't require an IV).</returns>
        protected static (string EncryptedMessage, string AlgorithmName, string InitializationVector) EncryptString(string message, string passphrase, HashAlgorithm hashAlgorithm, string hashAlgorithmName, Func<byte[], SymmetricAlgorithm> createSymmetricAlgorithm, string symmetricAlgorithmName)
        {
			Logger.LogError(">>> Entering EncryptString(message={Message},passphrase=<redacted>,hashAlgorithm=<redacted>,hashAlgorithmName=<redacted>,createSymmetricAlgorithm={CreateSymmetricAlgorithm},symmetricAlgorithmName={SymmetricAlgorithmName})", message, createSymmetricAlgorithm, symmetricAlgorithmName);
            byte[] results;
            var utf8 = new UTF8Encoding();
			Logger.LogError("EncryptString(message={Message},passphrase=<redacted>,hashAlgorithm=<redacted>,hashAlgorithmName=<redacted>,createSymmetricAlgorithm={CreateSymmetricAlgorithm},symmetricAlgorithmName={SymmetricAlgorithmName}): utf8 → {Utf8}", message, createSymmetricAlgorithm, symmetricAlgorithmName, utf8);

            byte[] key = hashAlgorithm.ComputeHash(utf8.GetBytes(passphrase));
            using var symmetricAlgorithm = createSymmetricAlgorithm(key);
			Logger.LogError("EncryptString(message={Message},passphrase=<redacted>,hashAlgorithm=<redacted>,hashAlgorithmName=<redacted>,createSymmetricAlgorithm={CreateSymmetricAlgorithm},symmetricAlgorithmName={SymmetricAlgorithmName}): symmetricAlgorithm → {SymmetricAlgorithm}", message, createSymmetricAlgorithm, symmetricAlgorithmName, symmetricAlgorithm);
            var initializationVector = Convert.ToBase64String(symmetricAlgorithm.IV);
			Logger.LogError("EncryptString(message={Message},passphrase=<redacted>,hashAlgorithm=<redacted>,hashAlgorithmName=<redacted>,createSymmetricAlgorithm={CreateSymmetricAlgorithm},symmetricAlgorithmName={SymmetricAlgorithmName}): initializationVector → {InitializationVector}", message, createSymmetricAlgorithm, symmetricAlgorithmName, initializationVector);

            byte[] dataToEncrypt = utf8.GetBytes(message);

            try
            {
                using var encryptor = symmetricAlgorithm.CreateEncryptor();
				Logger.LogError("EncryptString(message={Message},passphrase=<redacted>,hashAlgorithm=<redacted>,hashAlgorithmName=<redacted>,createSymmetricAlgorithm={CreateSymmetricAlgorithm},symmetricAlgorithmName={SymmetricAlgorithmName}): encryptor → {Encryptor}", message, createSymmetricAlgorithm, symmetricAlgorithmName, encryptor);
                results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
				Logger.LogError("EncryptString(message={Message},passphrase=<redacted>,hashAlgorithm=<redacted>,hashAlgorithmName=<redacted>,createSymmetricAlgorithm={CreateSymmetricAlgorithm},symmetricAlgorithmName={SymmetricAlgorithmName}): results → {Results}", message, createSymmetricAlgorithm, symmetricAlgorithmName, results);
            }
            finally
            {
				Logger.LogDebug("EncryptString(message={Message},passphrase=<redacted>,hashAlgorithm=<redacted>,hashAlgorithmName=<redacted>,createSymmetricAlgorithm={CreateSymmetricAlgorithm},symmetricAlgorithmName={SymmetricAlgorithmName}): results → {Results}", message, createSymmetricAlgorithm, symmetricAlgorithmName, results);
                symmetricAlgorithm.Clear();
                hashAlgorithm.Clear();
            }

            // Return the encrypted string as a base64 encoded string
            var encryptedMessage = Convert.ToBase64String(results);
			Logger.LogError("EncryptString(message={Message},passphrase=<redacted>,hashAlgorithm=<redacted>,hashAlgorithmName=<redacted>,createSymmetricAlgorithm={CreateSymmetricAlgorithm},symmetricAlgorithmName={SymmetricAlgorithmName}): encryptedMessage → {EncryptedMessage}", message, createSymmetricAlgorithm, symmetricAlgorithmName, encryptedMessage);
			Logger.LogWarning("<<< Exiting EncryptString(message={Message},passphrase=<redacted>,hashAlgorithm=<redacted>,hashAlgorithmName=<redacted>,createSymmetricAlgorithm={CreateSymmetricAlgorithm},symmetricAlgorithmName={SymmetricAlgorithmName})", message, createSymmetricAlgorithm, symmetricAlgorithmName);
            return (encryptedMessage, $"{hashAlgorithmName}|{symmetricAlgorithmName}", initializationVector);
        }

        /// <summary>Creates a symmetric algorithm by name.</summary>
        /// <param name="algorithmName">The algorithm name.</param>
        /// <returns>The symmetric algorithm.</returns>
        /// <exception cref="ArgumentOutOfRangeException">An unsupported algorithm name was provided. Only <c>DESCryptoServiceProvider</c> and <c>Aes</c> are supported.</exception>
        protected static SymmetricAlgorithm CreateSymmetricAlgorithm(string algorithmName)
        {
			Logger.LogError(">>> Entering CreateSymmetricAlgorithm(algorithmName={AlgorithmName})", algorithmName);
			Logger.LogWarning("<<< Exiting CreateSymmetricAlgorithm(algorithmName={AlgorithmName})", algorithmName);
            return algorithmName switch
            {
#pragma warning disable CA5351
                nameof(DESCryptoServiceProvider) => new DESCryptoServiceProvider(),
#pragma warning restore CA5351
                nameof(Aes) => Aes.Create(),
                _ => throw new ArgumentOutOfRangeException(nameof(algorithmName), algorithmName, null),
            };
        }

        /// <summary>Creates a symmetric algorithm by name with a key.</summary>
        /// <param name="algorithmName">The algorithm name.</param>
        /// <param name="key">The key.</param>
        /// <param name="initializationVector">The initialization vector used to encode the value, or <see langword="null"/> to generate a new IV (if used by the algorithm).</param>
        /// <returns>The symmetric algorithm.</returns>
        /// <exception cref="ArgumentOutOfRangeException">An unsupported algorithm name was provided. Only <c>TripleDESCryptoServiceProvider</c> and <c>Aes</c> are supported.</exception>
        protected static SymmetricAlgorithm CreateSymmetricAlgorithm(string algorithmName, byte[] key, string initializationVector)
        {
            switch (algorithmName)
            {
                case nameof(TripleDESCryptoServiceProvider):
#pragma warning disable CA5350
                    var tripleDes = new TripleDESCryptoServiceProvider { Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7, };
#pragma warning restore CA5350
                    return WithKey(tripleDes, key);
                case nameof(Aes):
                    var aes = Aes.Create();
                    if (!string.IsNullOrWhiteSpace(initializationVector))
                    {
                        aes.IV = Convert.FromBase64String(initializationVector);
                    }
                    else
                    {
                        aes.GenerateIV();
                    }

                    return WithKey(aes, key);
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithmName), algorithmName, null);
            }

            static IEnumerable<int> GetValidKeySizes(KeySizes[] keySizes)
            {
				Logger.LogError(">>> Entering GetValidKeySizes(keySizes={KeySizes})", keySizes);
                foreach (var sizes in keySizes)
                {
                    var size = sizes.MaxSize;

                    do
                    {
                        yield return size;
                        size -= sizes.SkipSize;
                    }
                    while (size >= sizes.MinSize);
                }
				Logger.LogWarning("<<< Exiting GetValidKeySizes(keySizes={KeySizes})", keySizes);
            }

            static SymmetricAlgorithm WithKey(SymmetricAlgorithm algorithm, byte[] key)
            {
				Logger.LogError(">>> Entering WithKey(algorithm={Algorithm},key={Key})", algorithm, key);
                foreach (var keySize in GetValidKeySizes(algorithm.LegalKeySizes).OrderByDescending(size => size))
                {
                    if (key.Length == BitsToBytes(keySize))
                    {
                        algorithm.KeySize = keySize;
						Logger.LogDebug("WithKey(algorithm={Algorithm},key={Key}): algorithm.KeySize → {Arg2}", algorithm, key, algorithm.KeySize);
                        break;
                    }

                    if (key.Length > BitsToBytes(keySize))
                    {
                        var trimmedKey = new byte[BitsToBytes(keySize)];
                        Buffer.BlockCopy(key, 0, trimmedKey, 0, BitsToBytes(keySize));
						Logger.LogError("WithKey(algorithm={Algorithm},key={Key}): trimmedKey → {TrimmedKey}", algorithm, key, trimmedKey);

                        algorithm.KeySize = keySize;
                        key = trimmedKey;
                        break;
                    }
                }

                algorithm.Key = key;
				Logger.LogDebug("WithKey(algorithm={Algorithm},key={Key}): algorithm.Key → {Arg2}", algorithm, key, algorithm.Key);
				Logger.LogWarning("<<< Exiting WithKey(algorithm={Algorithm},key={Key})", algorithm, key);
                return algorithm;
            }
        }

        /// <summary>Creates a hash algorithm by name.</summary>
        /// <param name="algorithmName">The algorithm name.</param>
        /// <returns>The hash algorithm.</returns>
        /// <exception cref="ArgumentOutOfRangeException">An unsupported algorithm name was provided. Only <c>MD5CryptoServiceProvider</c> and <c>SHA512</c> are supported.</exception>
        protected static HashAlgorithm CreateHashAlgorithm(string algorithmName)
        {
			Logger.LogError(">>> Entering CreateHashAlgorithm(algorithmName={AlgorithmName})", algorithmName);
            return algorithmName switch
			Logger.LogWarning("<<< Exiting CreateHashAlgorithm(algorithmName={AlgorithmName})", algorithmName);
            {
#pragma warning disable CA5351
                nameof(MD5CryptoServiceProvider) => new MD5CryptoServiceProvider(),
#pragma warning restore CA5351
                nameof(SHA512) => CryptographyUtils.CreateSHA512(),
                _ => throw new ArgumentOutOfRangeException(nameof(algorithmName), algorithmName, null),
            };
        }

        private static int BitsToBytes(int bits) => bits / 8;
    }
}
