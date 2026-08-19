// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Tests.Portals
{
    using System;
    using System.Security.Cryptography;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class PortalSettingsTests : DnnWebTest
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private string _settingName;
        private string _settingValue;

        [SetUp]
        public void SetUp()
        {
		_logger.LogDebug(">>> Entering SetUp()");

            this._settingName = "NameToCheckFor";
			_logger.LogDebug("SetUp(): this._settingName → {Arg0}", this._settingName);

            // we need different value so when we save we force going to database
            this._settingValue = "ValueToCheckFor_" + new Random().Next(1, 100);
			_logger.LogDebug("<<< Exiting SetUp()");
        }

        [Test]
        public void Saving_Non_Secure_Value_Doesnt_Encrypt_It()
        {
			_logger.LogDebug(">>> Entering Saving_Non_Secure_Value_Doesnt_Encrypt_It()");
            // Act
            PortalController.UpdatePortalSetting(this.PortalId, this._settingName, this._settingValue, true, null, false);
            var result = PortalController.GetPortalSetting(this._settingName, this.PortalId, string.Empty);
			_logger.LogDebug("Saving_Non_Secure_Value_Doesnt_Encrypt_It(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.Not.EqualTo(string.Empty));
            Assert.That(result, Is.EqualTo(this._settingValue));
			_logger.LogDebug("<<< Exiting Saving_Non_Secure_Value_Doesnt_Encrypt_It()");
        }

        [Test]
        public void Saving_Secure_Value_Encrypts_It()
        {
			_logger.LogDebug(">>> Entering Saving_Secure_Value_Encrypts_It()");
            // Act
            PortalController.UpdatePortalSetting(this.PortalId, this._settingName, this._settingValue, true, null, true);

            var result = PortalController.GetPortalSetting(this._settingName, this.PortalId, string.Empty);
			_logger.LogDebug("Saving_Secure_Value_Encrypts_It(): result → {Result}", result);
            var decrypted = DotNetNuke.Security.FIPSCompliant.DecryptAES(HashAlgorithmName.SHA512, result, Config.GetDecryptionkey(), Host.GUID);
			_logger.LogDebug("Saving_Secure_Value_Encrypts_It(): decrypted → {Decrypted}", decrypted);

            // Assert
            Assert.That(result, Is.Not.EqualTo(string.Empty));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.EqualTo(this._settingValue));
                Assert.That(this._settingValue, Is.EqualTo(decrypted));
            }
			_logger.LogDebug("<<< Exiting Saving_Secure_Value_Encrypts_It()");
        }
    }
}
