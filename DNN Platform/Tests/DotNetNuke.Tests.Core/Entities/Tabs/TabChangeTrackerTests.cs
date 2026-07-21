namespace DotNetNuke.Tests.Core.Entities.Tabs
{
    using System;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using Moq;
    using NUnit.Framework;

    /// <summary>Contains UTs for <see cref="TabChangeTracker"/>.</summary>
    [TestFixture]
    public class TabChangeTrackerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>UT for <see cref="TabChangeTracker.TrackModuleModification(ModuleInfo, int, int)" />.</summary>
        [Test]
        public void TrackModuleModification_WithSharedModule_ThrowsException()
        {
			Logger.LogDebug(">>> Entering TrackModuleModification_WithSharedModule_ThrowsException()");
            // Arrange
            var tabChangeTracker = new TabChangeTracker();
            var mockedModuleController = new Mock<IModuleController>();
			Logger.LogDebug("TrackModuleModification_WithSharedModule_ThrowsException(): tabChangeTracker → {TabChangeTracker}", tabChangeTracker);
			Logger.LogDebug("TrackModuleModification_WithSharedModule_ThrowsException(): mockedModuleController → {MockedModuleController}", mockedModuleController);
            mockedModuleController
                .Setup(s => s.IsSharedModule(It.IsAny<ModuleInfo>()))
                .Returns(true);
            ServiceLocator<IModuleController, ModuleController>.SetTestableInstance(mockedModuleController.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => tabChangeTracker.TrackModuleModification(null, 1, 0));
            Assert.That(exception.Data?[TabChangeTracker.IsModuleDoesNotBelongToPage], Is.EqualTo(true));
			Logger.LogDebug("<<< Exiting TrackModuleModification_WithSharedModule_ThrowsException()");
			Logger.LogDebug("TrackModuleModification_WithSharedModule_ThrowsException(): exception → {Exception}", exception);
        }
    }
}
