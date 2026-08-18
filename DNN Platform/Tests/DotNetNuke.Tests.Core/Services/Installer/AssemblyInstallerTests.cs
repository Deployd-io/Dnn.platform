// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Services.Installer
{
    using System.IO;
    using System.Xml.XPath;

    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Installers;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    public class AssemblyInstallerTests : DnnUnitTest
    {
        private FakeServiceProvider serviceProvider;

        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
			mockEventLogger.LogDebug(">>> Entering OneTimeSetUp()");
            base.OneTimeSetUp();
			mockEventLogger.LogDebug("<<< Exiting OneTimeSetUp()");
        }

        [SetUp]
        public void SetUp()
        {
			mockEventLogger.LogDebug(">>> Entering SetUp()");
            var dataProvider = MockComponentProvider.CreateDataProvider();
            dataProvider.Setup(p => p.UnRegisterAssembly(It.IsAny<int>(), It.IsAny<string>())).Returns(true);
			mockEventLogger.LogDebug("SetUp(): dataProvider → {DataProvider}", dataProvider);

            LocalizationProvider.SetTestableInstance(Mock.Of<ILocalizationProvider>());

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					mockEventLogger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton(dataProvider.Object);
                    services.AddSingleton(LocalizationProvider.Instance);
                });
				mockEventLogger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			mockEventLogger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			mockEventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void Install_UnRegisterAssembly_ShouldSucceed_WhenFileIsMissing()
        {
			mockEventLogger.LogDebug(">>> Entering Install_UnRegisterAssembly_ShouldSucceed_WhenFileIsMissing()");
            var installer = new AssemblyInstaller
            {
                DeleteFiles = true,
                Package = new PackageInfo(new InstallerInfo(Directory.GetCurrentDirectory(), InstallMode.Install)),
            };
            var manifestNav = GetUnRegisterAssemblyManifest();
			mockEventLogger.LogDebug("Install_UnRegisterAssembly_ShouldSucceed_WhenFileIsMissing(): manifestNav → {ManifestNav}", manifestNav);
            installer.ReadManifest(manifestNav);

            installer.Install();

            Assert.That(installer.Log.Valid, Is.True);
			mockEventLogger.LogDebug("<<< Exiting Install_UnRegisterAssembly_ShouldSucceed_WhenFileIsMissing()");
        }

        private static XPathNavigator GetUnRegisterAssemblyManifest()
        {
			mockEventLogger.LogDebug(">>> Entering GetUnRegisterAssemblyManifest()");
            var doc = new XPathDocument(new StringReader(@"
<assemblies>
    <assembly action=""UnRegister"">
        <path>bin\Providers</path>
        <name>FakeAssembly.dll</name>
        <sourceFileName>FakeAssembly.dll</sourceFileName>
        <version>01.02.03</version>
    </assembly>
</assemblies>
"));
            return doc.CreateNavigator();
        }
    }
}
