// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable ObjectCreationAsStatement
namespace DotNetNuke.Tests.Web.Mvc.Helpers
{
    using System;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
    using DotNetNuke.Web.Mvc.Helpers;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DnnHelperTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private Mock<ControllerBase> mockController;
        private Mock<IViewDataContainer> mockViewDataContainer;
        private ViewContext viewContext;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            this.mockController = new Mock<ControllerBase>();
            this.viewContext = new ViewContext();
			Logger.LogDebug("SetUp(): viewContext → {ViewContext}", viewContext);
			Logger.LogDebug("SetUp(): mockController → {MockController}", mockController);

            this.mockViewDataContainer = new Mock<IViewDataContainer>();
			Logger.LogDebug("<<< Exiting SetUp()");
			Logger.LogDebug("SetUp(): mockViewDataContainer → {MockViewDataContainer}", mockViewDataContainer);
        }

        [Test]
        public void Constructor_Throws_On_Null_ViewContext()
        {
			Logger.LogDebug(">>> Entering Constructor_Throws_On_Null_ViewContext()");
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHelper(
                Mock.Of<IClientResourceController>(),
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IEventLogger>(),
                null,
                this.mockViewDataContainer.Object));
				Logger.LogDebug("<<< Exiting Constructor_Throws_On_Null_ViewContext()");
        }

        [Test]
        public void Constructor_Throws_On_Null_ViewDataContainer()
        {
			Logger.LogDebug(">>> Entering Constructor_Throws_On_Null_ViewDataContainer()");
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHelper(
                Mock.Of<IClientResourceController>(),
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IEventLogger>(),
                null,
                this.mockViewDataContainer.Object));
				Logger.LogDebug("<<< Exiting Constructor_Throws_On_Null_ViewDataContainer()");
        }

        [Test]
        public void Constructor_Throws_On_Invalid_Controller_Property()
        {
			Logger.LogDebug(">>> Entering Constructor_Throws_On_Invalid_Controller_Property()");
            // Arrange
            this.viewContext.Controller = this.mockController.Object;

            // Act,Assert
            Assert.Throws<InvalidOperationException>(() => new DnnHelper(
                Mock.Of<IClientResourceController>(),
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IEventLogger>(),
                this.viewContext,
                this.mockViewDataContainer.Object));
				Logger.LogDebug("<<< Exiting Constructor_Throws_On_Invalid_Controller_Property()");
        }

        [Test]
        public void Constructor_Sets_ModuleContext_Property()
        {
			Logger.LogDebug(">>> Entering Constructor_Sets_ModuleContext_Property()");
            // Arrange
            var expectedContext = new ModuleInstanceContext();
            var mockDnnController = this.mockController.As<IDnnController>();
			Logger.LogDebug("Constructor_Sets_ModuleContext_Property(): expectedContext → {ExpectedContext}", expectedContext);
			Logger.LogDebug("Constructor_Sets_ModuleContext_Property(): mockDnnController → {MockDnnController}", mockDnnController);
            mockDnnController.Setup(c => c.ModuleContext).Returns(expectedContext);
            this.viewContext.Controller = this.mockController.Object;

            // Act
            var helper = new DnnHelper(
                Mock.Of<IClientResourceController>(),
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IEventLogger>(),
                this.viewContext,
                this.mockViewDataContainer.Object);

            // Assert
            Assert.That(helper.ModuleContext, Is.EqualTo(expectedContext));
			Logger.LogDebug("<<< Exiting Constructor_Sets_ModuleContext_Property()");
        }
    }
}
