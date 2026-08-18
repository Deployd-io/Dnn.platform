// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Framework
{
    using System;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    public class ServicesFrameworkTests
    {
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
			mockEventLogger.LogDebug(">>> Entering Setup()");
            this.serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IPortalController>()));

			mockEventLogger.LogDebug("Setup(): serviceProvider → {ServiceProvider}", serviceProvider);
            var simulator = new Instance.Utilities.HttpSimulator.HttpSimulator("/", "c:\\");
			mockEventLogger.LogDebug("Setup(): simulator → {Simulator}", simulator);
            simulator.SimulateRequest(new Uri("http://localhost/dnn/Default.aspx"));
			mockEventLogger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			mockEventLogger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			mockEventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void RequestingAjaxAntiForgeryIsNoted()
        {
			mockEventLogger.LogDebug(">>> Entering RequestingAjaxAntiForgeryIsNoted()");
            var servicesFramework = new ServicesFrameworkImpl(
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IEventLogger>());
            servicesFramework.RequestAjaxAntiForgerySupport();

            Assert.That(servicesFramework.IsAjaxAntiForgerySupportRequired, Is.True);
			mockEventLogger.LogDebug("<<< Exiting RequestingAjaxAntiForgeryIsNoted()");
        }

        [Test]
        public void NoAjaxAntiForgeryRequestMeansNotRequired()
        {
			mockEventLogger.LogDebug(">>> Entering NoAjaxAntiForgeryRequestMeansNotRequired()");
            var servicesFramework = new ServicesFrameworkImpl(
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IEventLogger>());
            Assert.That(servicesFramework.IsAjaxAntiForgerySupportRequired, Is.False);
			mockEventLogger.LogDebug("<<< Exiting NoAjaxAntiForgeryRequestMeansNotRequired()");
        }
    }
}
