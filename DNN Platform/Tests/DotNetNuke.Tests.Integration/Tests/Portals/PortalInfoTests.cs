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

    [TestFixture]
    public class PortalInfoTests : DnnWebTest
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [Test]
        public void Processor_Password_In_DB_Must_Be_Encrypted()
        {
			Logger.LogDebug(">>> Entering Processor_Password_In_DB_Must_Be_Encrypted()");
            var firstPortal = PortalController.Instance.GetPortals().OfType<PortalInfo>().FirstOrDefault();
            Assert.That(firstPortal, Is.Not.Null);
			Logger.LogDebug("Processor_Password_In_DB_Must_Be_Encrypted(): firstPortal → {FirstPortal}", firstPortal);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();

            var newPassword = "StringToEncrypt_" + new Random().Next(1, 100);
            firstPortal.ProcessorPassword = newPassword;
			Logger.LogDebug("Processor_Password_In_DB_Must_Be_Encrypted(): newPassword → {NewPassword}", newPassword);
			Logger.LogDebug("Processor_Password_In_DB_Must_Be_Encrypted(): firstPortal.ProcessorPassword → {Arg0}", firstPortal.ProcessorPassword);
            PortalController.Instance.UpdatePortalInfo(firstPortal);

            var result = DatabaseHelper.ExecuteScalar<string>(
                @"SELECT TOP(1) COALESCE(ProcessorPassword, '') FROM {objectQualifier}Portals WHERE PortalID=" + this.PortalId);

			Logger.LogDebug("Processor_Password_In_DB_Must_Be_Encrypted(): PortalID → {PortalID}", PortalID);
			Logger.LogInformation("Processor_Password_In_DB_Must_Be_Encrypted(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            var decrypted = DotNetNuke.Security.FIPSCompliant.DecryptAES(result, Config.GetDecryptionkey(), Host.GUID);

			Logger.LogDebug("Processor_Password_In_DB_Must_Be_Encrypted(): decrypted → {Decrypted}", decrypted);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.EqualTo(string.Empty));
                Assert.That(decrypted, Is.Not.EqualTo(result));
                Assert.That(newPassword, Is.EqualTo(decrypted));
            }
			Logger.LogDebug("<<< Exiting Processor_Password_In_DB_Must_Be_Encrypted()");
        }
    }
}
