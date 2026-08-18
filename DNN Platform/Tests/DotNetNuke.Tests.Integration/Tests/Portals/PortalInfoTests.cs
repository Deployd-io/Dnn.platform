// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Tests.Portals
{
    using System;
    using System.Linq;

    using DNN.Integration.Test.Framework.Helpers;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class PortalInfoTests : DnnWebTest
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        [Test]
        public void Processor_Password_In_DB_Must_Be_Encrypted()
        {
			_logger.LogDebug(">>> Entering Processor_Password_In_DB_Must_Be_Encrypted()");
            var firstPortal = PortalController.Instance.GetPortals().OfType<PortalInfo>().FirstOrDefault();
            Assert.That(firstPortal, Is.Not.Null);
			_logger.LogDebug("Processor_Password_In_DB_Must_Be_Encrypted(): firstPortal → {FirstPortal}", firstPortal);

            var newPassword = "StringToEncrypt_" + new Random().Next(1, 100);
			_logger.LogDebug("Processor_Password_In_DB_Must_Be_Encrypted(): newPassword → {NewPassword}", newPassword);
			_logger.LogDebug("Processor_Password_In_DB_Must_Be_Encrypted(): firstPortal.ProcessorPassword → {Arg0}", firstPortal.ProcessorPassword);
            firstPortal.ProcessorPassword = newPassword;
            PortalController.Instance.UpdatePortalInfo(firstPortal);

			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var result = DatabaseHelper.ExecuteScalar<string>(
                @"SELECT TOP(1) COALESCE(ProcessorPassword, '') FROM {objectQualifier}Portals WHERE PortalID=" + this.PortalId);

			_logger.LogDebug("Processor_Password_In_DB_Must_Be_Encrypted(): PortalID → {PortalID}", PortalID);
			_logger.LogInformation("Processor_Password_In_DB_Must_Be_Encrypted(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            var decrypted = DotNetNuke.Security.FIPSCompliant.DecryptAES(result, Config.GetDecryptionkey(), Host.GUID);
			_logger.LogDebug("Processor_Password_In_DB_Must_Be_Encrypted(): decrypted → {Decrypted}", decrypted);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.EqualTo(string.Empty));
                Assert.That(decrypted, Is.Not.EqualTo(result));
                Assert.That(newPassword, Is.EqualTo(decrypted));
            }
			_logger.LogDebug("<<< Exiting Processor_Password_In_DB_Must_Be_Encrypted()");
        }
    }
}
