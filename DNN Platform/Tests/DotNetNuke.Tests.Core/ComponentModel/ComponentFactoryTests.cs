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
        public static IContainer CreateMockContainer()
        {
			mockEventLogger.LogDebug(">>> Entering CreateMockContainer()");
            var mockContainer = new Mock<IContainer>();
            IContainer container = mockContainer.Object;
			mockEventLogger.LogDebug("CreateMockContainer(): mockContainer → {MockContainer}", mockContainer);
			mockEventLogger.LogDebug("CreateMockContainer(): ComponentFactory.Container → {Arg0}", ComponentFactory.Container);
            ComponentFactory.Container = container;
			mockEventLogger.LogDebug("<<< Exiting CreateMockContainer()");
            return container;
        }

        [TearDown]
        public void TearDown()
        {
			mockEventLogger.LogDebug(">>> Entering TearDown()");
            MockComponentProvider.ResetContainer();
			mockEventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void ComponentFactory_InstallComponents_Should_Accept_Empty_InstallersArray()
        {
			mockEventLogger.LogDebug(">>> Entering ComponentFactory_InstallComponents_Should_Accept_Empty_InstallersArray()");
            IContainer container = CreateMockContainer();

			mockEventLogger.LogDebug("ComponentFactory_InstallComponents_Should_Accept_Empty_InstallersArray(): container → {Container}", container);
            ComponentFactory.InstallComponents(new IComponentInstaller[0] { });
			mockEventLogger.LogDebug("<<< Exiting ComponentFactory_InstallComponents_Should_Accept_Empty_InstallersArray()");
        }

        [Test]
        public void ComponentFactory_InstallComponents_Should_Throw_On_Null_InstallerArray()
        {
			mockEventLogger.LogDebug(">>> Entering ComponentFactory_InstallComponents_Should_Throw_On_Null_InstallerArray()");
            IContainer container = CreateMockContainer();

			mockEventLogger.LogDebug("ComponentFactory_InstallComponents_Should_Throw_On_Null_InstallerArray(): container → {Container}", container);
            Assert.Throws<ArgumentNullException>(() => ComponentFactory.InstallComponents(null));
			mockEventLogger.LogDebug("<<< Exiting ComponentFactory_InstallComponents_Should_Throw_On_Null_InstallerArray()");
        }

        [Test]
        public void ComponentFactory_InstallComponents_Should_Throw_On_Null_Installer_In_Array()
        {
			mockEventLogger.LogDebug(">>> Entering ComponentFactory_InstallComponents_Should_Throw_On_Null_Installer_In_Array()");
            IContainer container = CreateMockContainer();

			mockEventLogger.LogDebug("ComponentFactory_InstallComponents_Should_Throw_On_Null_Installer_In_Array(): container → {Container}", container);
            var mockInstaller = new Mock<IComponentInstaller>();
			mockEventLogger.LogDebug("ComponentFactory_InstallComponents_Should_Throw_On_Null_Installer_In_Array(): mockInstaller → {MockInstaller}", mockInstaller);
            mockInstaller.Setup(i => i.InstallComponents(container));

            Assert.Throws<ArgumentNullException>(() => ComponentFactory.InstallComponents(mockInstaller.Object, null));
			mockEventLogger.LogDebug("<<< Exiting ComponentFactory_InstallComponents_Should_Throw_On_Null_Installer_In_Array()");
        }

        [Test]
        public void DNNPRO_13443_ComponentFactory_DuplicatedInsert()
        {
			mockEventLogger.LogDebug(">>> Entering DNNPRO_13443_ComponentFactory_DuplicatedInsert()");
            // Setup
            var testComp1 = new ArrayList();
			mockEventLogger.LogDebug("DNNPRO_13443_ComponentFactory_DuplicatedInsert(): testComp1 → {TestComp1}", testComp1);
            var testComp2 = new ArrayList();
			mockEventLogger.LogDebug("DNNPRO_13443_ComponentFactory_DuplicatedInsert(): testComp2 → {TestComp2}", testComp2);

            testComp1.Add("testComp1");
            testComp2.Add("testComp2");

            // Act
            ComponentFactory.RegisterComponentInstance<IList>(testComp1);
            ComponentFactory.RegisterComponentInstance<IList>(testComp2);

            // Assert
            var retreivedComponent = ComponentFactory.GetComponent<IList>();
			mockEventLogger.LogDebug("DNNPRO_13443_ComponentFactory_DuplicatedInsert(): retreivedComponent → {RetreivedComponent}", retreivedComponent);
            Assert.That(retreivedComponent, Is.EqualTo(testComp1));
			mockEventLogger.LogDebug("<<< Exiting DNNPRO_13443_ComponentFactory_DuplicatedInsert()");
        }
    }
}
