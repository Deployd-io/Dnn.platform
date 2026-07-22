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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
			Logger.LogDebug(">>> Entering Setup()");
            this.serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IPortalController>()));

			Logger.LogDebug("Setup(): serviceProvider → {ServiceProvider}", serviceProvider);
            var simulator = new Instance.Utilities.HttpSimulator.HttpSimulator("/", "c:\\");
            simulator.SimulateRequest(new Uri("http://localhost/dnn/Default.aspx"));
			Logger.LogDebug("<<< Exiting Setup()");
			Logger.LogDebug("Setup(): simulator → {Simulator}", simulator);
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void RequestingAjaxAntiForgeryIsNoted()
        {
			Logger.LogDebug(">>> Entering RequestingAjaxAntiForgeryIsNoted()");
            var servicesFramework = new ServicesFrameworkImpl(
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IEventLogger>());
            servicesFramework.RequestAjaxAntiForgerySupport();

            Assert.That(servicesFramework.IsAjaxAntiForgerySupportRequired, Is.True);
			Logger.LogDebug("<<< Exiting RequestingAjaxAntiForgeryIsNoted()");
        }

        [Test]
        public void NoAjaxAntiForgeryRequestMeansNotRequired()
        {
			Logger.LogDebug(">>> Entering NoAjaxAntiForgeryRequestMeansNotRequired()");
            var servicesFramework = new ServicesFrameworkImpl(
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IEventLogger>());
            Assert.That(servicesFramework.IsAjaxAntiForgerySupportRequired, Is.False);
			Logger.LogDebug("<<< Exiting NoAjaxAntiForgeryRequestMeansNotRequired()");
        }
    }
}
