// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.ComponentModel
{
    using System;
    using System.Collections;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    /// <summary>  Summary description for ComponentFactoryTests.</summary>
    [TestFixture]
    public class ComponentFactoryTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static IContainer CreateMockContainer()
        {
			Logger.LogDebug(">>> Entering CreateMockContainer()");
            var mockContainer = new Mock<IContainer>();
            IContainer container = mockContainer.Object;
			Logger.LogDebug("CreateMockContainer(): mockContainer → {MockContainer}", mockContainer);
			Logger.LogDebug("CreateMockContainer(): ComponentFactory.Container → {Arg0}", ComponentFactory.Container);
            ComponentFactory.Container = container;
			Logger.LogDebug("<<< Exiting CreateMockContainer()");
            return container;
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            MockComponentProvider.ResetContainer();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void ComponentFactory_InstallComponents_Should_Accept_Empty_InstallersArray()
        {
			Logger.LogDebug(">>> Entering ComponentFactory_InstallComponents_Should_Accept_Empty_InstallersArray()");
            IContainer container = CreateMockContainer();

			Logger.LogDebug("ComponentFactory_InstallComponents_Should_Accept_Empty_InstallersArray(): container → {Container}", container);
            ComponentFactory.InstallComponents(new IComponentInstaller[0] { });
			Logger.LogDebug("<<< Exiting ComponentFactory_InstallComponents_Should_Accept_Empty_InstallersArray()");
        }

        [Test]
        public void ComponentFactory_InstallComponents_Should_Throw_On_Null_InstallerArray()
        {
			Logger.LogDebug(">>> Entering ComponentFactory_InstallComponents_Should_Throw_On_Null_InstallerArray()");
            IContainer container = CreateMockContainer();

			Logger.LogDebug("ComponentFactory_InstallComponents_Should_Throw_On_Null_InstallerArray(): container → {Container}", container);
            Assert.Throws<ArgumentNullException>(() => ComponentFactory.InstallComponents(null));
			Logger.LogDebug("<<< Exiting ComponentFactory_InstallComponents_Should_Throw_On_Null_InstallerArray()");
        }

        [Test]
        public void ComponentFactory_InstallComponents_Should_Throw_On_Null_Installer_In_Array()
        {
			Logger.LogDebug(">>> Entering ComponentFactory_InstallComponents_Should_Throw_On_Null_Installer_In_Array()");
            IContainer container = CreateMockContainer();

			Logger.LogDebug("ComponentFactory_InstallComponents_Should_Throw_On_Null_Installer_In_Array(): container → {Container}", container);
            var mockInstaller = new Mock<IComponentInstaller>();
            mockInstaller.Setup(i => i.InstallComponents(container));
			Logger.LogDebug("ComponentFactory_InstallComponents_Should_Throw_On_Null_Installer_In_Array(): mockInstaller → {MockInstaller}", mockInstaller);

            Assert.Throws<ArgumentNullException>(() => ComponentFactory.InstallComponents(mockInstaller.Object, null));
			Logger.LogDebug("<<< Exiting ComponentFactory_InstallComponents_Should_Throw_On_Null_Installer_In_Array()");
        }

        [Test]
        public void DNNPRO_13443_ComponentFactory_DuplicatedInsert()
        {
			Logger.LogDebug(">>> Entering DNNPRO_13443_ComponentFactory_DuplicatedInsert()");
            // Setup
            var testComp1 = new ArrayList();
            var testComp2 = new ArrayList();
			Logger.LogDebug("DNNPRO_13443_ComponentFactory_DuplicatedInsert(): testComp1 → {TestComp1}", testComp1);
			Logger.LogDebug("DNNPRO_13443_ComponentFactory_DuplicatedInsert(): testComp2 → {TestComp2}", testComp2);

            testComp1.Add("testComp1");
            testComp2.Add("testComp2");

            // Act
            ComponentFactory.RegisterComponentInstance<IList>(testComp1);
            ComponentFactory.RegisterComponentInstance<IList>(testComp2);

            // Assert
            var retreivedComponent = ComponentFactory.GetComponent<IList>();
            Assert.That(retreivedComponent, Is.EqualTo(testComp1));
			Logger.LogDebug("<<< Exiting DNNPRO_13443_ComponentFactory_DuplicatedInsert()");
			Logger.LogDebug("DNNPRO_13443_ComponentFactory_DuplicatedInsert(): retreivedComponent → {RetreivedComponent}", retreivedComponent);
        }
    }
}
