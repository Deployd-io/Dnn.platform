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
        [Test]
        public void EncryptData_Should_Return_Encrypted_String()
        {
			mockEventLogger.LogDebug(">>> Entering EncryptData_Should_Return_Encrypted_String()");
            var message = "Hello world!";
            var encryptionKey = Config.GetDecryptionkey();
			mockEventLogger.LogDebug("EncryptData_Should_Return_Encrypted_String(): encryptionKey → {EncryptionKey}", encryptionKey);
			mockEventLogger.LogDebug("EncryptData_Should_Return_Encrypted_String(): message → {Message}", message);

            // Arrange
            var provider = new FipsCompilanceCryptographyProvider();
			mockEventLogger.LogDebug("EncryptData_Should_Return_Encrypted_String(): provider → {Provider}", provider);

            // Act
            var encryptedValue = provider.EncryptParameter(message, encryptionKey);
			mockEventLogger.LogDebug("EncryptData_Should_Return_Encrypted_String(): encryptedValue → {EncryptedValue}", encryptedValue);

            // Assert
            Assert.That(encryptedValue, Is.Not.EqualTo(message));
			mockEventLogger.LogDebug("<<< Exiting EncryptData_Should_Return_Encrypted_String()");
        }

        [Test]
        public void DecryptData_Should_Return_Empty_String_If_Data_Is_Not_Encrypted()
        {
			mockEventLogger.LogDebug(">>> Entering DecryptData_Should_Return_Empty_String_If_Data_Is_Not_Encrypted()");
            var message = "Hello world!";
            var encryptionKey = Config.GetDecryptionkey();
			mockEventLogger.LogDebug("DecryptData_Should_Return_Empty_String_If_Data_Is_Not_Encrypted(): encryptionKey → {EncryptionKey}", encryptionKey);
			mockEventLogger.LogDebug("DecryptData_Should_Return_Empty_String_If_Data_Is_Not_Encrypted(): message → {Message}", message);

            // Arrange
            var provider = new FipsCompilanceCryptographyProvider();
			mockEventLogger.LogDebug("DecryptData_Should_Return_Empty_String_If_Data_Is_Not_Encrypted(): provider → {Provider}", provider);

            // Act
            var decryptedValue = provider.DecryptParameter(message, encryptionKey);
			mockEventLogger.LogDebug("DecryptData_Should_Return_Empty_String_If_Data_Is_Not_Encrypted(): decryptedValue → {DecryptedValue}", decryptedValue);

            // Assert
            Assert.That(decryptedValue, Is.EqualTo(string.Empty));
			mockEventLogger.LogDebug("<<< Exiting DecryptData_Should_Return_Empty_String_If_Data_Is_Not_Encrypted()");
        }
    }
}
