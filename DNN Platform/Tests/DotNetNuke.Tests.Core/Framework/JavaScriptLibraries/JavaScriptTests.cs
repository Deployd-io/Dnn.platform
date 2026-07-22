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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string ScriptPrefix = "JSL.";

        private int libraryIdCounter = 20;

        private HttpContextBase httpContext;
        private FakeServiceProvider serviceProvider;
        private Mock<CachingProvider> cachingProvider;

        [SetUp]
        public void Setup()
        {
			Logger.LogDebug(">>> Entering Setup()");
            var mockApplicationStatusInfo = new Mock<IApplicationStatusInfo>();
            mockApplicationStatusInfo.Setup(info => info.Status).Returns(UpgradeStatus.None);

            var mockApplication = new Mock<IApplicationInfo>();
            mockApplication.Setup(app => app.Version).Returns(new Version("1.0.0.0"));
			Logger.LogDebug("Setup(): mockApplication → {MockApplication}", mockApplication);

            var dnnContext = new DotNetNukeContext(mockApplication.Object);

			Logger.LogDebug("Setup(): dnnContext → {DnnContext}", dnnContext);
            MockComponentProvider.CreateLocalizationProvider();
            var dataProviderMock = MockComponentProvider.CreateDataProvider();
            dataProviderMock.Setup(dp => dp.GetProviderPath()).Returns(string.Empty);
            dataProviderMock.Setup(dp => dp.GetVersion()).Returns(dnnContext.Application.Version);

            this.cachingProvider = MockComponentProvider.CreateDataCacheProvider();
			Logger.LogDebug("Setup(): cachingProvider → {CachingProvider}", cachingProvider);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("Setup(): serviceProvider → {ServiceProvider}", serviceProvider);
				Logger.LogDebug("Setup(): services → {Services}", services);
                    services.AddSingleton(mockApplicationStatusInfo.Object);
                    services.AddSingleton(mockApplication.Object);
                    services.AddSingleton(this.cachingProvider.Object);
                    services.AddSingleton<IDnnContext>(dnnContext);
                    services.AddSingleton(dataProviderMock.Object);
                    services.AddSingleton(Mock.Of<IHostSettings>());
                    services.AddSingleton<IClientResourceController, ClientResourceController>();
                });

            this.httpContext = HttpContextSource.Current;
			Logger.LogDebug("Setup(): httpContext → {HttpContext}", httpContext);
			Logger.LogDebug("Setup(): this.httpContext → {Arg0}", this.httpContext);

            this.SetupPackageController();
			Logger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            JavaScriptLibraryController.ClearInstance();
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void CanRegisterLibraryByName()
        {
			Logger.LogDebug(">>> Entering CanRegisterLibraryByName()");
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
			Logger.LogDebug("<<< Exiting CanRegisterLibraryByName()");
        }

        [Test]
        public void CanRegisterLibraryByNameWithMismatchedCase()
        {
			Logger.LogDebug(">>> Entering CanRegisterLibraryByNameWithMismatchedCase()");
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
			Logger.LogDebug("<<< Exiting CanRegisterLibraryByNameWithMismatchedCase()");
        }

        [Test]
        public void CanRegisterLibraryByNameAndVersion()
        {
			Logger.LogDebug(">>> Entering CanRegisterLibraryByNameAndVersion()");
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
			Logger.LogDebug("<<< Exiting CanRegisterLibraryByNameAndVersion()");
        }

        [Test]
        public void CanRegisterLibraryByNameAndExactVersion()
        {
			Logger.LogDebug(">>> Entering CanRegisterLibraryByNameAndExactVersion()");
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
			Logger.LogDebug("<<< Exiting CanRegisterLibraryByNameAndExactVersion()");
        }

        [Test]
        public void CanRegisterLibraryByNameWithMismatchedCaseAndExactVersion()
        {
			Logger.LogDebug(">>> Entering CanRegisterLibraryByNameWithMismatchedCaseAndExactVersion()");
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
			Logger.LogDebug("<<< Exiting CanRegisterLibraryByNameWithMismatchedCaseAndExactVersion()");
        }

        [Test]
        public void FailToRegisterLibraryByNameAndMismatchedVersion()
        {
			Logger.LogDebug(">>> Entering FailToRegisterLibraryByNameAndMismatchedVersion()");
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
			Logger.LogDebug("<<< Exiting FailToRegisterLibraryByNameAndMismatchedVersion()");
        }

        [Test]
        public void FailToRegisterLibraryByNameAndMismatchedExactVersion()
        {
			Logger.LogDebug(">>> Entering FailToRegisterLibraryByNameAndMismatchedExactVersion()");
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
			Logger.LogDebug("<<< Exiting FailToRegisterLibraryByNameAndMismatchedExactVersion()");
        }

        [Test]
        public void CanRegisterLibraryByNameAndSameMinorVersion()
        {
			Logger.LogDebug(">>> Entering CanRegisterLibraryByNameAndSameMinorVersion()");
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
			Logger.LogDebug("<<< Exiting CanRegisterLibraryByNameAndSameMinorVersion()");
        }

        [Test]
        public void CanRegisterLibraryByNameWithMismatchedCaseAndSameMinorVersion()
        {
			Logger.LogDebug(">>> Entering CanRegisterLibraryByNameWithMismatchedCaseAndSameMinorVersion()");
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
			Logger.LogDebug("<<< Exiting CanRegisterLibraryByNameWithMismatchedCaseAndSameMinorVersion()");
        }

        [Test]
        public void FallbackToHighestVersionLibraryWhenDifferentMinorVersion()
        {
			Logger.LogDebug(">>> Entering FallbackToHighestVersionLibraryWhenDifferentMinorVersion()");
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
					Logger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMinorVersion(): JavaScriptLibraryID → {JavaScriptLibraryID}", JavaScriptLibraryID);
					Logger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMinorVersion(): LibraryName → {LibraryName}", LibraryName);
					Logger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMinorVersion(): Version → {Version}", Version);

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 2), SpecificVersion.LatestMinor);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(this.httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId], Is.Not.EqualTo(true));
                Assert.That(this.httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId], Is.EqualTo(true));
            }
			Logger.LogDebug("<<< Exiting FallbackToHighestVersionLibraryWhenDifferentMinorVersion()");
        }

        [Test]
        public void FallbackToHighestVersionLibraryWhenDifferentMinorVersionWithMismatchedCase()
        {
			Logger.LogDebug(">>> Entering FallbackToHighestVersionLibraryWhenDifferentMinorVersionWithMismatchedCase()");
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
					Logger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMinorVersionWithMismatchedCase(): JavaScriptLibraryID → {JavaScriptLibraryID}", JavaScriptLibraryID);
					Logger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMinorVersionWithMismatchedCase(): LibraryName → {LibraryName}", LibraryName);
					Logger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMinorVersionWithMismatchedCase(): Version → {Version}", Version);

            // Act
            JavaScript.RequestRegistration("test", new Version(2, 2, 2), SpecificVersion.LatestMinor);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(this.httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId], Is.Not.EqualTo(true));
                Assert.That(this.httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId], Is.EqualTo(true));
            }
			Logger.LogDebug("<<< Exiting FallbackToHighestVersionLibraryWhenDifferentMinorVersionWithMismatchedCase()");
        }

        [Test]
        public void CanRegisterLibraryByNameAndSameMajorVersion()
        {
			Logger.LogDebug(">>> Entering CanRegisterLibraryByNameAndSameMajorVersion()");
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
			Logger.LogDebug("<<< Exiting CanRegisterLibraryByNameAndSameMajorVersion()");
        }

        [Test]
        public void CanRegisterLibraryByNameWithMismatchedCaseAndSameMajorVersion()
        {
			Logger.LogDebug(">>> Entering CanRegisterLibraryByNameWithMismatchedCaseAndSameMajorVersion()");
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
			Logger.LogDebug("<<< Exiting CanRegisterLibraryByNameWithMismatchedCaseAndSameMajorVersion()");
        }

        [Test]
        public void FallbackToHighestVersionLibraryWhenDifferentMajorVersion()
        {
			Logger.LogDebug(">>> Entering FallbackToHighestVersionLibraryWhenDifferentMajorVersion()");
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
					Logger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMajorVersion(): JavaScriptLibraryID → {JavaScriptLibraryID}", JavaScriptLibraryID);
					Logger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMajorVersion(): LibraryName → {LibraryName}", LibraryName);
					Logger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMajorVersion(): Version → {Version}", Version);

            // Act
            JavaScript.RequestRegistration("Test", new Version(2, 2, 2), SpecificVersion.LatestMajor);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(this.httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId], Is.Not.EqualTo(true));
                Assert.That(this.httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId], Is.EqualTo(true));
            }
			Logger.LogDebug("<<< Exiting FallbackToHighestVersionLibraryWhenDifferentMajorVersion()");
        }

        [Test]
        public void FallbackToHighestVersionLibraryWhenDifferentMajorVersionWithMismatchedCase()
        {
			Logger.LogDebug(">>> Entering FallbackToHighestVersionLibraryWhenDifferentMajorVersionWithMismatchedCase()");
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
					Logger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMajorVersionWithMismatchedCase(): JavaScriptLibraryID → {JavaScriptLibraryID}", JavaScriptLibraryID);
					Logger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMajorVersionWithMismatchedCase(): LibraryName → {LibraryName}", LibraryName);
					Logger.LogDebug("FallbackToHighestVersionLibraryWhenDifferentMajorVersionWithMismatchedCase(): Version → {Version}", Version);

            // Act
            JavaScript.RequestRegistration("test", new Version(2, 2, 2), SpecificVersion.LatestMajor);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(this.httpContext.Items[ScriptPrefix + lowerVersionJavaScriptLibraryId], Is.Not.EqualTo(true));
                Assert.That(this.httpContext.Items[ScriptPrefix + higherVersionJavaScriptLibraryId], Is.EqualTo(true));
            }
			Logger.LogDebug("<<< Exiting FallbackToHighestVersionLibraryWhenDifferentMajorVersionWithMismatchedCase()");
        }

        private void SetupJavaScriptLibraryController(params JavaScriptLibrary[] libraries)
        {
			Logger.LogDebug(">>> Entering SetupJavaScriptLibraryController(libraries={Libraries})", libraries);
            var libraryController = new Mock<IJavaScriptLibraryController>();
            libraryController.Setup(lc => lc.GetLibraries()).Returns(libraries);
			Logger.LogDebug("SetupJavaScriptLibraryController(libraries={Libraries}): libraryController → {LibraryController}", libraries, libraryController);
            libraryController.Setup(lc => lc.GetLibrary(It.IsAny<Func<JavaScriptLibrary, bool>>())).Returns((Func<JavaScriptLibrary, bool> predicate) => libraries.Where(predicate).OrderByDescending(l => l.Version).FirstOrDefault());
            libraryController.Setup(lc => lc.GetLibraries(It.IsAny<Func<JavaScriptLibrary, bool>>())).Returns((Func<JavaScriptLibrary, bool> predicate) => libraries.Where(predicate));
            JavaScriptLibraryController.SetTestableInstance(libraryController.Object);
			Logger.LogDebug("<<< Exiting SetupJavaScriptLibraryController(libraries={Libraries})", libraries);
        }

        private void SetupPackageController(params PackageInfo[] packages)
        {
			Logger.LogDebug(">>> Entering SetupPackageController(packages={Packages})", packages);
            var packageController = new Mock<IPackageController>();
            // Fix: Provide a portalId argument (e.g., 0) to match the required signature
			Logger.LogDebug("SetupPackageController(packages={Packages}): packageController → {PackageController}", packages, packageController);
            packageController.Setup(pc => pc.GetExtensionPackages(It.IsAny<int>()))
                .Returns(packages.ToList());
            packageController.Setup(pc => pc.GetExtensionPackage(It.IsAny<int>(), It.IsAny<Func<PackageInfo, bool>>()))
                .Returns((int portalId, Func<PackageInfo, bool> predicate) => new PackageInfo());
            // Fix: Correct the type of the predicate to match the method signature
            packageController.Setup(pc => pc.GetPackageDependencies(It.IsAny<Func<PackageDependencyInfo, bool>>()))
                .Returns(new List<PackageDependencyInfo>());
            PackageController.SetTestableInstance(packageController.Object);
			Logger.LogDebug("<<< Exiting SetupPackageController(packages={Packages})", packages);
        }
    }
}
