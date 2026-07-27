// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.ComponentModel
{
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.ComponentModel;

    using NUnit.Framework;

    [TestFixture]
    public class SimpleContainerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [Test]
        public void GetComponentListSupportsInterfaces()
        {
			Logger.LogDebug(">>> Entering GetComponentListSupportsInterfaces()");
            var container = new SimpleContainer();
            container.RegisterComponent<IList>("payload", ComponentLifeStyleType.Singleton);
			Logger.LogDebug("GetComponentListSupportsInterfaces(): container → {Container}", container);

            var retrieved = container.GetComponentList(typeof(IList));

			Logger.LogDebug("GetComponentListSupportsInterfaces(): retrieved → {Retrieved}", retrieved);
            Assert.That(retrieved, Is.EqualTo(new List<string> { "payload" }).AsCollection);
			Logger.LogDebug("<<< Exiting GetComponentListSupportsInterfaces()");
        }

        [Test]
        public void RegisterComponentInstance_Must_Register_In_ComponentsList()
        {
			Logger.LogDebug(">>> Entering RegisterComponentInstance_Must_Register_In_ComponentsList()");
            var container = new SimpleContainer();
            ComponentFactory.Container = container;
			Logger.LogDebug("RegisterComponentInstance_Must_Register_In_ComponentsList(): container → {Container}", container);
			Logger.LogDebug("RegisterComponentInstance_Must_Register_In_ComponentsList(): ComponentFactory.Container → {Arg0}", ComponentFactory.Container);

            container.RegisterComponentInstance("test", typeof(IList<string>), new List<string>());

            Assert.That(ComponentFactory.GetComponents<IList<string>>().Keys, Does.Contain("test"));
			Logger.LogDebug("<<< Exiting RegisterComponentInstance_Must_Register_In_ComponentsList()");
        }
    }
}
