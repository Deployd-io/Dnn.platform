// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Security.Tests.Checks
{
    using System;
    using System.Linq;

    using Dnn.PersonaBar.Security.Components;
    using Dnn.PersonaBar.Security.Components.Checks;
    using DotNetNuke.Maintenance.Telerik;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class CheckTelerikPresenceTests
    {
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
			permissionProviderMock.LogDebug(">>> Entering Setup()");
            this.serviceProvider = FakeServiceProvider.Setup();
			permissionProviderMock.LogDebug("Setup(): serviceProvider → {ServiceProvider}", serviceProvider);
			permissionProviderMock.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			permissionProviderMock.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			permissionProviderMock.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void Execute_WhenError_ReturnsUnverified()
        {
			permissionProviderMock.LogDebug(">>> Entering Execute_WhenError_ReturnsUnverified()");
            // arrange
            var telerikUtilsMock = new Mock<ITelerikUtils>();
			permissionProviderMock.LogDebug("Execute_WhenError_ReturnsUnverified(): telerikUtilsMock → {TelerikUtilsMock}", telerikUtilsMock);

            telerikUtilsMock
                .Setup(x => x.TelerikIsInstalled())
                .Throws<BadImageFormatException>();

            var sut = new CheckTelerikPresence(telerikUtilsMock.Object);
			permissionProviderMock.LogDebug("Execute_WhenError_ReturnsUnverified(): sut → {Sut}", sut);

            // act
            var result = sut.Execute();
			permissionProviderMock.LogDebug("Execute_WhenError_ReturnsUnverified(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // assert
                Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Unverified));
                Assert.That(result.Notes.Count, Is.EqualTo(1));
                Assert.That(result.Notes.First(), Is.EqualTo("An internal error occurred. See logs for details."));
            }
			permissionProviderMock.LogDebug("<<< Exiting Execute_WhenError_ReturnsUnverified()");
        }

        [Test]
        public void Execute_WhenInstalledAndUsed_ReturnsInstalledAndUsed()
        {
			permissionProviderMock.LogDebug(">>> Entering Execute_WhenInstalledAndUsed_ReturnsInstalledAndUsed()");
            // arrange
            var telerikUtilsMock = new Mock<ITelerikUtils>();
			permissionProviderMock.LogDebug("Execute_WhenInstalledAndUsed_ReturnsInstalledAndUsed(): telerikUtilsMock → {TelerikUtilsMock}", telerikUtilsMock);

            telerikUtilsMock
                .Setup(x => x.TelerikIsInstalled())
                .Returns(true);

            telerikUtilsMock
                .Setup(x => x.GetAssembliesThatDependOnTelerik())
                .Returns(() => new[] { "bin\\DotNetNuke.Modules.Mod3.dll" });

            telerikUtilsMock
                .SetupGet(x => x.BinPath)
                .Returns("bin");

            var sut = new CheckTelerikPresence(telerikUtilsMock.Object);
			permissionProviderMock.LogDebug("Execute_WhenInstalledAndUsed_ReturnsInstalledAndUsed(): sut → {Sut}", sut);

            // act
            var result = sut.Execute();
			permissionProviderMock.LogDebug("Execute_WhenInstalledAndUsed_ReturnsInstalledAndUsed(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // assert
                Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Failure));
                Assert.That(result.Notes.Count(), Is.EqualTo(1));
                Assert.That(result.Notes.First(), Does.Contain("* DotNetNuke.Modules.Mod3.dll"));
            }
			permissionProviderMock.LogDebug("<<< Exiting Execute_WhenInstalledAndUsed_ReturnsInstalledAndUsed()");
        }

        [Test]
        public void Execute_WhenInstalledButNotUsed_ReturnsInstalledButNotUsed()
        {
			permissionProviderMock.LogDebug(">>> Entering Execute_WhenInstalledButNotUsed_ReturnsInstalledButNotUsed()");
            // arrange
            var telerikUtilsMock = new Mock<ITelerikUtils>();
			permissionProviderMock.LogDebug("Execute_WhenInstalledButNotUsed_ReturnsInstalledButNotUsed(): telerikUtilsMock → {TelerikUtilsMock}", telerikUtilsMock);

            telerikUtilsMock
                .Setup(x => x.TelerikIsInstalled())
                .Returns(true);

            telerikUtilsMock
                .Setup(x => x.GetAssembliesThatDependOnTelerik())
                .Returns(() => []);

            var sut = new CheckTelerikPresence(telerikUtilsMock.Object);
			permissionProviderMock.LogDebug("Execute_WhenInstalledButNotUsed_ReturnsInstalledButNotUsed(): sut → {Sut}", sut);

            // act
            var result = sut.Execute();
			permissionProviderMock.LogDebug("Execute_WhenInstalledButNotUsed_ReturnsInstalledButNotUsed(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // assert
                Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Failure));
                Assert.That(result.Notes.Count(), Is.EqualTo(1));
            }
			permissionProviderMock.LogDebug("<<< Exiting Execute_WhenInstalledButNotUsed_ReturnsInstalledButNotUsed()");
        }

        [Test]
        public void Execute_WhenNotInstalled_ReturnsNotInstalled()
        {
			permissionProviderMock.LogDebug(">>> Entering Execute_WhenNotInstalled_ReturnsNotInstalled()");
            // arrange
            var telerikUtilsMock = new Mock<ITelerikUtils>();
			permissionProviderMock.LogDebug("Execute_WhenNotInstalled_ReturnsNotInstalled(): telerikUtilsMock → {TelerikUtilsMock}", telerikUtilsMock);

            telerikUtilsMock
                .Setup(x => x.TelerikIsInstalled())
                .Returns(false);

            var sut = new CheckTelerikPresence(telerikUtilsMock.Object);
			permissionProviderMock.LogDebug("Execute_WhenNotInstalled_ReturnsNotInstalled(): sut → {Sut}", sut);

            // act
            var result = sut.Execute();
			permissionProviderMock.LogDebug("Execute_WhenNotInstalled_ReturnsNotInstalled(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // assert
                Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Pass));
                Assert.That(result.Notes.Count(), Is.EqualTo(0));
            }
			permissionProviderMock.LogDebug("<<< Exiting Execute_WhenNotInstalled_ReturnsNotInstalled()");
        }
    }
}
