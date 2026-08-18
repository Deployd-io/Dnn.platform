// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.DependencyInjection
{
    using System;
    using System.Web.Http.Filters;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.DependencyInjection.Extensions;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public partial class BuildUpExtensionsTests
    {
        // The event logger is used in the test Filters
        private IEventLogger eventLogger;
        private IServiceProvider container;

        [SetUp]
        public void Setup()
        {
			eventLogger.LogDebug(">>> Entering Setup()");
            CBO.SetTestableInstance(new MockCBO());

            this.eventLogger = Mock.Of<IEventLogger>();
			eventLogger.LogDebug("Setup(): eventLogger → {EventLogger}", eventLogger);

            var mockContainer = new Mock<IServiceProvider>();
			eventLogger.LogDebug("Setup(): mockContainer → {MockContainer}", mockContainer);
            mockContainer.Setup(provider => provider.GetService(typeof(IEventLogger))).Returns(this.eventLogger);

            this.container = mockContainer.Object;
			eventLogger.LogDebug("Setup(): this.container → {Arg0}", this.container);
			eventLogger.LogDebug("Setup(): container → {Container}", container);
			eventLogger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			eventLogger.LogDebug(">>> Entering TearDown()");
            this.eventLogger = null;
            this.container = null;
			eventLogger.LogDebug("TearDown(): this.eventLogger → {Arg0}", this.eventLogger);
			eventLogger.LogDebug("TearDown(): container → {Container}", container);
			eventLogger.LogDebug("TearDown(): this.container → {Arg0}", this.container);
            CBO.ClearInstance();
			eventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void CheckParameter_ContainerIsNullTest()
        {
			eventLogger.LogDebug(">>> Entering CheckParameter_ContainerIsNullTest()");
            var container = default(IServiceProvider);

			eventLogger.LogDebug("CheckParameter_ContainerIsNullTest(): container → {Container}", container);
            // This should invoke without exceptions
            container.BuildUp(Mock.Of<IFilter>());
			eventLogger.LogDebug("<<< Exiting CheckParameter_ContainerIsNullTest()");
        }

        [Test]
        public void CheckParameter_Filter_IsNullTest()
        {
			eventLogger.LogDebug(">>> Entering CheckParameter_Filter_IsNullTest()");
            var container = Mock.Of<IServiceProvider>();

			eventLogger.LogDebug("CheckParameter_Filter_IsNullTest(): container → {Container}", container);
            // This should invoke without exceptions
            container.BuildUp(null);
			eventLogger.LogDebug("<<< Exiting CheckParameter_Filter_IsNullTest()");
        }

        [Test]
        public void BuildUp_PrivateProperty_Test()
        {
			eventLogger.LogDebug(">>> Entering BuildUp_PrivateProperty_Test()");
            var filter = new PrivateFilterAttribute();

			eventLogger.LogDebug("BuildUp_PrivateProperty_Test(): filter → {Filter}", filter);
            this.container.BuildUp(filter);
            filter.OnActionExecuted(null);

            this.VerifyEventLoggerInvoked();
			eventLogger.LogDebug("<<< Exiting BuildUp_PrivateProperty_Test()");
        }

        [Test]
        public void BuildUp_ProtectedProperty_Test()
        {
			eventLogger.LogDebug(">>> Entering BuildUp_ProtectedProperty_Test()");
            var filter = new ProtectedFilterAttribute();

			eventLogger.LogDebug("BuildUp_ProtectedProperty_Test(): filter → {Filter}", filter);
            this.container.BuildUp(filter);
            filter.OnActionExecuted(null);

			eventLogger.LogDebug("<<< Exiting BuildUp_ProtectedProperty_Test()");
            this.VerifyEventLoggerInvoked();

        }

        [Test]
        public void BuildUp_ProtectedInternalProperty_Test()
        {
			eventLogger.LogDebug(">>> Entering BuildUp_ProtectedInternalProperty_Test()");
            var filter = new ProtectedInternalFilterAttribute();

			eventLogger.LogDebug("BuildUp_ProtectedInternalProperty_Test(): filter → {Filter}", filter);
            this.container.BuildUp(filter);
            filter.OnActionExecuted(null);

            this.VerifyEventLoggerInvoked();
			eventLogger.LogDebug("<<< Exiting BuildUp_ProtectedInternalProperty_Test()");
        }

        [Test]
        public void BuildUp_InternalProperty_Test()
        {
			eventLogger.LogDebug(">>> Entering BuildUp_InternalProperty_Test()");
            var filter = new InternalFilterAttribute();

			eventLogger.LogDebug("BuildUp_InternalProperty_Test(): filter → {Filter}", filter);
            this.container.BuildUp(filter);
            filter.OnActionExecuted(null);

            this.VerifyEventLoggerInvoked();
			eventLogger.LogDebug("<<< Exiting BuildUp_InternalProperty_Test()");
        }

        [Test]
        public void BuildUp_NoDependencyAttribute_Test()
        {
			eventLogger.LogDebug(">>> Entering BuildUp_NoDependencyAttribute_Test()");
            var filter = new PublicNoDependencyAttributeFilterAttribute();

			eventLogger.LogDebug("BuildUp_NoDependencyAttribute_Test(): filter → {Filter}", filter);
            this.container.BuildUp(filter);
            Assert.Throws<NullReferenceException>(() => filter.OnActionExecuted(null));

            this.VerifyEventLoggerInvoked(Times.Never());
			eventLogger.LogDebug("<<< Exiting BuildUp_NoDependencyAttribute_Test()");
        }

        [Test]
        public void BuildUp_NoSetter_Test()
        {
			eventLogger.LogDebug(">>> Entering BuildUp_NoSetter_Test()");
            var filter = new NoSetFilterAttribute();

			eventLogger.LogDebug("BuildUp_NoSetter_Test(): filter → {Filter}", filter);
            this.container.BuildUp(filter);
            Assert.Throws<NullReferenceException>(() => filter.OnActionExecuted(null));

            this.VerifyEventLoggerInvoked(Times.Never());
			eventLogger.LogDebug("<<< Exiting BuildUp_NoSetter_Test()");
        }

        private void VerifyEventLoggerInvoked() => this.VerifyEventLoggerInvoked(Times.Once());

        private void VerifyEventLoggerInvoked(Times times) =>
            Mock.Get(this.eventLogger)
                .Verify(logger => logger.AddLog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EventLogType>()), times);
			eventLogger.LogDebug(">>> Entering VerifyEventLoggerInvoked(times={Times})", times);

    }
}
