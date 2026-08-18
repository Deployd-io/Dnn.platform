// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Framework.JavaScriptLibraries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;
    using DotNetNuke.Web.Client.ResourceManager;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    public class JavaScriptTests
    {
        private const string ScriptPrefix = "JSL.";

        private int libraryIdCounter = 20;

        private HttpContextBase httpContext;
        private FakeServiceProvider serviceProvider;
        private Mock<CachingProvider> cachingProvider;

        [SetUp]
        public void Setup()
        {
			mockEventLogger.LogDebug(">>> Entering Setup()");
            var mockApplicationStatusInfo = new Mock<IApplicationStatusInfo>();
            mockApplicationStatusInfo.Setup(info => info.Status).Returns(UpgradeStatus.None);

            var mockApplication = new Mock<IApplicationInfo>();
			mockEventLogger.LogDebug("Setup(): mockApplication → {MockApplication}", mockApplication);
            mockApplication.Setup(app => app.Version).Returns(new Version("1.0.0.0"));

            var dnnContext = new DotNetNukeContext(mockApplication.Object);
			mockEventLogger.LogDebug("Setup(): dnnContext → {DnnContext}", dnnContext);

            MockComponentProvider.CreateLocalizationProvider();
            var dataProviderMock = MockComponentProvider.CreateDataProvider();
			mockEventLogger.LogDebug("Setup(): dataProviderMock → {DataProviderMock}", dataProviderMock);
            dataProviderMock.Setup(dp => dp.GetProviderPath()).Returns(string.Empty);
            dataProviderMock.Setup(dp => dp.GetVersion()).Returns(dnnContext.Application.Version);

            this.cachingProvider = MockComponentProvider.CreateDataCacheProvider();
			mockEventLogger.LogDebug("Setup(): cachingProvider → {CachingProvider}", cachingProvider);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					mockEventLogger.LogDebug("Setup(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton(mockApplicationStatusInfo.Object);
                    services.AddSingleton(mockApplication.Object);
                    services.AddSingleton(this.cachingProvider.Object);
                    services.AddSingleton<IDnnContext>(dnnContext);
                    services.AddSingleton(dataProviderMock.Object);
                    services.AddSingleton(Mock.Of<IHostSettings>());
                    services.AddSingleton<IClientResourceController, ClientResourceController>();
                });

            this.httpContext = HttpContextSource.Current;
			mockEventLogger.LogDebug("Setup(): httpContext → {HttpContext}", httpContext);
			mockEventLogger.LogDebug("Setup(): this.httpContext → {Arg0}", this.httpContext);

            this.SetupPackageController();
			mockEventLogger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			mockEventLogger.LogDebug(">>> Entering TearDown()");
            JavaScriptLibraryController.ClearInstance();
            this.serviceProvider.Dispose();
			mockEventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void CanRegisterLibraryByName()
        {
			mockEventLogger.LogDebug(">>> Entering CanRegisterLibraryByName()");
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("Test");

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
			mockEventLogger.LogDebug("<<< Exiting CanRegisterLibraryByName()");
        }

        [Test]
        public void CanRegisterLibraryByNameWithMismatchedCase()
        {
			mockEventLogger.LogDebug(">>> Entering CanRegisterLibraryByNameWithMismatchedCase()");
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("test");

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
			mockEventLogger.LogDebug("<<< Exiting CanRegisterLibraryByNameWithMismatchedCase()");
        }

        [Test]
        public void CanRegisterLibraryByNameAndVersion()
        {
			mockEventLogger.LogDebug(">>> Entering CanRegisterLibraryByNameAndVersion()");
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 2));

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
			mockEventLogger.LogDebug("<<< Exiting CanRegisterLibraryByNameAndVersion()");
        }

        [Test]
        public void CanRegisterLibraryByNameAndExactVersion()
        {
			mockEventLogger.LogDebug(">>> Entering CanRegisterLibraryByNameAndExactVersion()");
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 2), SpecificVersion.Exact);

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
			mockEventLogger.LogDebug("<<< Exiting CanRegisterLibraryByNameAndExactVersion()");
        }

        [Test]
        public void CanRegisterLibraryByNameWithMismatchedCaseAndExactVersion()
        {
			mockEventLogger.LogDebug(">>> Entering CanRegisterLibraryByNameWithMismatchedCaseAndExactVersion()");
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("test", new Version(2, 2, 2), SpecificVersion.Exact);

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
			mockEventLogger.LogDebug("<<< Exiting CanRegisterLibraryByNameWithMismatchedCaseAndExactVersion()");
        }

        [Test]
        public void FailToRegisterLibraryByNameAndMismatchedVersion()
        {
			mockEventLogger.LogDebug(">>> Entering FailToRegisterLibraryByNameAndMismatchedVersion()");
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 0));

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.Not.EqualTo(true));
			mockEventLogger.LogDebug("<<< Exiting FailToRegisterLibraryByNameAndMismatchedVersion()");
        }

        [Test]
        public void FailToRegisterLibraryByNameAndMismatchedExactVersion()
        {
			mockEventLogger.LogDebug(">>> Entering FailToRegisterLibraryByNameAndMismatchedExactVersion()");
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 0), SpecificVersion.Exact);

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.Not.EqualTo(true));
			mockEventLogger.LogDebug("<<< Exiting FailToRegisterLibraryByNameAndMismatchedExactVersion()");
        }

        [Test]
        public void CanRegisterLibraryByNameAndSameMinorVersion()
        {
			mockEventLogger.LogDebug(">>> Entering CanRegisterLibraryByNameAndSameMinorVersion()");
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 0), SpecificVersion.LatestMinor);

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
			mockEventLogger.LogDebug("<<< Exiting CanRegisterLibraryByNameAndSameMinorVersion()");
        }

        [Test]
        public void CanRegisterLibraryByNameWithMismatchedCaseAndSameMinorVersion()
        {
			mockEventLogger.LogDebug(">>> Entering CanRegisterLibraryByNameWithMismatchedCaseAndSameMinorVersion()");
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("test", new Version(2, 2, 0), SpecificVersion.LatestMinor);

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
			mockEventLogger.LogDebug("<<< Exiting CanRegisterLibraryByNameWithMismatchedCaseAndSameMinorVersion()");
        }

        [Test]
        public void FallbackToHighestVersionLibraryWhenDifferentMinorVersion()
        {
			mockEventLogger.LogDebug(">>> Entering FallbackToHighestVersionLibraryWhenDifferentMinorVersion()");
            // Arrange
            int lowerVersionJavaScriptLibraryId = this.libraryIdCounter++;
            int higherVersionJavaScriptLibraryId = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(
                new JavaScriptLibrary
                {
                    JavaScriptLibraryID = lowerVersionJavaScriptLibraryId,
                    LibraryName = "Test",
                    Version = new Version(2, 1, 0),
                }, new JavaScriptLibrary
                {
                    JavaScriptLibraryID = higherVersionJavaScriptLibraryId,
                    LibraryName = "Test",
                    Version = new Version(3, 3, 3),
                });
				mockEventLogger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMinorVersion(): Version → {Version}", Version);
				mockEventLogger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMinorVersion(): LibraryName → {LibraryName}", LibraryName);

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 2), SpecificVersion.LatestMinor);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(this.httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId], Is.Not.EqualTo(true));
                Assert.That(this.httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId], Is.EqualTo(true));
            }
			mockEventLogger.LogDebug("<<< Exiting FallbackToHighestVersionLibraryWhenDifferentMinorVersion()");
        }

        [Test]
        public void FallbackToHighestVersionLibraryWhenDifferentMinorVersionWithMismatchedCase()
        {
			mockEventLogger.LogDebug(">>> Entering FallbackToHighestVersionLibraryWhenDifferentMinorVersionWithMismatchedCase()");
            // Arrange
            int lowerVersionJavaScriptLibraryId = this.libraryIdCounter++;
            int higherVersionJavaScriptLibraryId = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(
                new JavaScriptLibrary
                {
                    JavaScriptLibraryID = lowerVersionJavaScriptLibraryId,
                    LibraryName = "test",
                    Version = new Version(2, 1, 0),
                }, new JavaScriptLibrary
                {
                    JavaScriptLibraryID = higherVersionJavaScriptLibraryId,
                    LibraryName = "Test",
                    Version = new Version(3, 3, 3),
                });
				mockEventLogger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMinorVersionWithMismatchedCase(): Version → {Version}", Version);
				mockEventLogger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMinorVersionWithMismatchedCase(): LibraryName → {LibraryName}", LibraryName);

            // Act
            JavaScript.RequestRegistration("test", new Version(2, 2, 2), SpecificVersion.LatestMinor);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(this.httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId], Is.Not.EqualTo(true));
                Assert.That(this.httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId], Is.EqualTo(true));
            }
			mockEventLogger.LogDebug("<<< Exiting FallbackToHighestVersionLibraryWhenDifferentMinorVersionWithMismatchedCase()");
        }

        [Test]
        public void CanRegisterLibraryByNameAndSameMajorVersion()
        {
			mockEventLogger.LogDebug(">>> Entering CanRegisterLibraryByNameAndSameMajorVersion()");
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 1, 1), SpecificVersion.LatestMajor);

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
			mockEventLogger.LogDebug("<<< Exiting CanRegisterLibraryByNameAndSameMajorVersion()");
        }

        [Test]
        public void CanRegisterLibraryByNameWithMismatchedCaseAndSameMajorVersion()
        {
			mockEventLogger.LogDebug(">>> Entering CanRegisterLibraryByNameWithMismatchedCaseAndSameMajorVersion()");
            // Arrange
            int JavaScriptLibraryID = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(new JavaScriptLibrary
            {
                JavaScriptLibraryID = JavaScriptLibraryID,
                LibraryName = "Test",
                Version = new Version(2, 2, 2),
            });

            // Act
            JavaScript.RequestRegistration("test", new Version(2, 1, 1), SpecificVersion.LatestMajor);

            // Assert
            Assert.That(this.httpContext.Items[ScriptPrefix + JavaScriptLibraryID], Is.EqualTo(true));
			mockEventLogger.LogDebug("<<< Exiting CanRegisterLibraryByNameWithMismatchedCaseAndSameMajorVersion()");
        }

        [Test]
        public void FallbackToHighestVersionLibraryWhenDifferentMajorVersion()
        {
			mockEventLogger.LogDebug(">>> Entering FallbackToHighestVersionLibraryWhenDifferentMajorVersion()");
            // Arrange
            int lowerVersionJavaScriptLibraryId = this.libraryIdCounter++;
            int higherVersionJavaScriptLibraryId = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(
                new JavaScriptLibrary
                {
                    JavaScriptLibraryID = lowerVersionJavaScriptLibraryId,
                    LibraryName = "Test",
                    Version = new Version(1, 2, 2),
                }, new JavaScriptLibrary
                {
                    JavaScriptLibraryID = higherVersionJavaScriptLibraryId,
                    LibraryName = "Test",
                    Version = new Version(3, 3, 3),
                });
				mockEventLogger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMajorVersion(): Version → {Version}", Version);
				mockEventLogger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMajorVersion(): LibraryName → {LibraryName}", LibraryName);

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 2), SpecificVersion.LatestMajor);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(this.httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId], Is.Not.EqualTo(true));
                Assert.That(this.httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId], Is.EqualTo(true));
            }
			mockEventLogger.LogDebug("<<< Exiting FallbackToHighestVersionLibraryWhenDifferentMajorVersion()");
        }

        [Test]
        public void FallbackToHighestVersionLibraryWhenDifferentMajorVersionWithMismatchedCase()
        {
			mockEventLogger.LogDebug(">>> Entering FallbackToHighestVersionLibraryWhenDifferentMajorVersionWithMismatchedCase()");
            // Arrange
            int lowerVersionJavaScriptLibraryId = this.libraryIdCounter++;
            int higherVersionJavaScriptLibraryId = this.libraryIdCounter++;
            this.SetupJavaScriptLibraryController(
                new JavaScriptLibrary
                {
                    JavaScriptLibraryID = lowerVersionJavaScriptLibraryId,
                    LibraryName = "test",
                    Version = new Version(1, 2, 2),
                }, new JavaScriptLibrary
                {
                    JavaScriptLibraryID = higherVersionJavaScriptLibraryId,
                    LibraryName = "Test",
                    Version = new Version(3, 3, 3),
                });
				mockEventLogger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMajorVersionWithMismatchedCase(): LibraryName → {LibraryName}", LibraryName);
				mockEventLogger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMajorVersionWithMismatchedCase(): Version → {Version}", Version);

            // Act
            JavaScript.RequestRegistration("test", new Version(2, 2, 2), SpecificVersion.LatestMajor);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(this.httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId], Is.Not.EqualTo(true));
                Assert.That(this.httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId], Is.EqualTo(true));
            }
			mockEventLogger.LogDebug("<<< Exiting FallbackToHighestVersionLibraryWhenDifferentMajorVersionWithMismatchedCase()");
        }

        private void SetupJavaScriptLibraryController(params JavaScriptLibrary[] libraries)
        {
			mockEventLogger.LogDebug(">>> Entering SetupJavaScriptLibraryController(libraries={Libraries})", libraries);
            var libraryController = new Mock<IJavaScriptLibraryController>();
            libraryController.Setup(lc => lc.GetLibraries()).Returns(libraries);
			mockEventLogger.LogDebug("SetupJavaScriptLibraryController(libraries={Libraries}): libraryController → {LibraryController}", libraries, libraryController);
            libraryController.Setup(lc => lc.GetLibrary(It.IsAny<Func<JavaScriptLibrary, bool>>())).Returns((Func<JavaScriptLibrary, bool> predicate) => libraries.Where(predicate).OrderByDescending(l => l.Version).FirstOrDefault());
            libraryController.Setup(lc => lc.GetLibraries(It.IsAny<Func<JavaScriptLibrary, bool>>())).Returns((Func<JavaScriptLibrary, bool> predicate) => libraries.Where(predicate));
            JavaScriptLibraryController.SetTestableInstance(libraryController.Object);
			mockEventLogger.LogDebug("<<< Exiting SetupJavaScriptLibraryController(libraries={Libraries})", libraries);
        }

        private void SetupPackageController(params PackageInfo[] packages)
        {
			mockEventLogger.LogDebug(">>> Entering SetupPackageController(packages={Packages})", packages);
            var packageController = new Mock<IPackageController>();
            // Fix: Provide a portalId argument (e.g., 0) to match the required signature
			mockEventLogger.LogDebug("SetupPackageController(packages={Packages}): packageController → {PackageController}", packages, packageController);
            packageController.Setup(pc => pc.GetExtensionPackages(It.IsAny<int>()))
                .Returns(packages.ToList());
            packageController.Setup(pc => pc.GetExtensionPackage(It.IsAny<int>(), It.IsAny<Func<PackageInfo, bool>>()))
                .Returns((int portalId, Func<PackageInfo, bool> predicate) => new PackageInfo());
            // Fix: Correct the type of the predicate to match the method signature
            packageController.Setup(pc => pc.GetPackageDependencies(It.IsAny<Func<PackageDependencyInfo, bool>>()))
                .Returns(new List<PackageDependencyInfo>());
            PackageController.SetTestableInstance(packageController.Object);
			mockEventLogger.LogDebug("<<< Exiting SetupPackageController(packages={Packages})", packages);
        }
    }
}
