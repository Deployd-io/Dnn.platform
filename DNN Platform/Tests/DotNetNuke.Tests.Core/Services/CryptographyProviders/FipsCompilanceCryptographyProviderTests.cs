// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Services.CryptographyProviders
{
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Cryptography;

    using NUnit.Framework;

    [TestFixture]
    public class FipsCompilanceCryptographyProviderTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [Test]
        public void EncryptData_Should_Return_Encrypted_String()
        {
			Logger.LogDebug(">>> Entering EncryptData_Should_Return_Encrypted_String()");
            var message = "Hello world!";
            var encryptionKey = Config.GetDecryptionkey();
			Logger.LogDebug("EncryptData_Should_Return_Encrypted_String(): message → {Message}", message);
			Logger.LogDebug("EncryptData_Should_Return_Encrypted_String(): encryptionKey → {EncryptionKey}", encryptionKey);

            // Arrange
            var provider = new FipsCompilanceCryptographyProvider();

			Logger.LogDebug("EncryptData_Should_Return_Encrypted_String(): provider → {Provider}", provider);
            // Act
            var encryptedValue = provider.EncryptParameter(message, encryptionKey);

			Logger.LogDebug("EncryptData_Should_Return_Encrypted_String(): encryptedValue → {EncryptedValue}", encryptedValue);
            // Assert
            Assert.That(encryptedValue, Is.Not.EqualTo(message));
			Logger.LogDebug("<<< Exiting EncryptData_Should_Return_Encrypted_String()");
        }

        [Test]
        public void DecryptData_Should_Return_Empty_String_If_Data_Is_Not_Encrypted()
        {
			Logger.LogDebug(">>> Entering DecryptData_Should_Return_Empty_String_If_Data_Is_Not_Encrypted()");
            var message = "Hello world!";
            var encryptionKey = Config.GetDecryptionkey();
			Logger.LogDebug("DecryptData_Should_Return_Empty_String_If_Data_Is_Not_Encrypted(): message → {Message}", message);
			Logger.LogDebug("DecryptData_Should_Return_Empty_String_If_Data_Is_Not_Encrypted(): encryptionKey → {EncryptionKey}", encryptionKey);

            // Arrange
            var provider = new FipsCompilanceCryptographyProvider();

			Logger.LogDebug("DecryptData_Should_Return_Empty_String_If_Data_Is_Not_Encrypted(): provider → {Provider}", provider);
            // Act
            var decryptedValue = provider.DecryptParameter(message, encryptionKey);

			Logger.LogDebug("DecryptData_Should_Return_Empty_String_If_Data_Is_Not_Encrypted(): decryptedValue → {DecryptedValue}", decryptedValue);
            // Assert
            Assert.That(decryptedValue, Is.EqualTo(string.Empty));
			Logger.LogDebug("<<< Exiting DecryptData_Should_Return_Empty_String_If_Data_Is_Not_Encrypted()");
        }
    }
}
