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
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class DnnHelperTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private Mock<ControllerBase> mockController;
        private Mock<IViewDataContainer> mockViewDataContainer;
        private ViewContext viewContext;

        [SetUp]
        public void SetUp()
        {
			_logger.LogDebug(">>> Entering SetUp()");
            this.mockController = new Mock<ControllerBase>();
            this.viewContext = new ViewContext();
			_logger.LogDebug("SetUp(): viewContext → {ViewContext}", viewContext);
			_logger.LogDebug("SetUp(): mockController → {MockController}", mockController);

            this.mockViewDataContainer = new Mock<IViewDataContainer>();
			_logger.LogDebug("SetUp(): mockViewDataContainer → {MockViewDataContainer}", mockViewDataContainer);
			_logger.LogDebug("<<< Exiting SetUp()");
        }

        [Test]
        public void Constructor_Throws_On_Null_ViewContext()
        {
			_logger.LogDebug(">>> Entering Constructor_Throws_On_Null_ViewContext()");
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHelper(
                Mock.Of<IClientResourceController>(),
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IEventLogger>(),
                null,
                this.mockViewDataContainer.Object));
				_logger.LogDebug("<<< Exiting Constructor_Throws_On_Null_ViewContext()");
        }

        [Test]
        public void Constructor_Throws_On_Null_ViewDataContainer()
        {
			_logger.LogDebug(">>> Entering Constructor_Throws_On_Null_ViewDataContainer()");
            // Act,Assert
            Assert.Throws<ArgumentNullException>(() => new DnnHelper(
                Mock.Of<IClientResourceController>(),
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IEventLogger>(),
                null,
                this.mockViewDataContainer.Object));
				_logger.LogDebug("<<< Exiting Constructor_Throws_On_Null_ViewDataContainer()");
        }

        [Test]
        public void Constructor_Throws_On_Invalid_Controller_Property()
        {
			_logger.LogDebug(">>> Entering Constructor_Throws_On_Invalid_Controller_Property()");
            // Arrange
            this.viewContext.Controller = this.mockController.Object;

            // Act,Assert
            Assert.Throws<InvalidOperationException>(() => new DnnHelper(
                Mock.Of<IClientResourceController>(),
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IEventLogger>(),
                this.viewContext,
                this.mockViewDataContainer.Object));
				_logger.LogDebug("<<< Exiting Constructor_Throws_On_Invalid_Controller_Property()");
        }

        [Test]
        public void Constructor_Sets_ModuleContext_Property()
        {
			_logger.LogDebug(">>> Entering Constructor_Sets_ModuleContext_Property()");
            // Arrange
            var expectedContext = new ModuleInstanceContext();
			_logger.LogDebug("Constructor_Sets_ModuleContext_Property(): expectedContext → {ExpectedContext}", expectedContext);
            var mockDnnController = this.mockController.As<IDnnController>();
			_logger.LogDebug("Constructor_Sets_ModuleContext_Property(): mockDnnController → {MockDnnController}", mockDnnController);
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
			_logger.LogDebug("<<< Exiting Constructor_Sets_ModuleContext_Property()");
        }
    }
}
