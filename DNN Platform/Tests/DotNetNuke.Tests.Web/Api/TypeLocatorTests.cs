// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api
{
    using System;
    using System.Linq;

    using DotNetNuke.Framework.Internal.Reflection;
    using DotNetNuke.Framework.Reflections;
    using Moq;
    using NUnit.Framework;

    public class TypeLocatorTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [Test]
        public void LocateAllMatchingTypes()
        {
			Logger.LogDebug(">>> Entering LocateAllMatchingTypes()");
            var assembly = new Mock<IAssembly>();
            assembly.Setup(x => x.GetTypes()).Returns(new[] { typeof(TypeLocatorTests), typeof(ServiceRoutingManagerTests) });
			Logger.LogDebug("LocateAllMatchingTypes(): assembly → {Assembly}", assembly);
            var assemblyLocator = new Mock<IAssemblyLocator>();
            assemblyLocator.Setup(x => x.Assemblies).Returns(new[] { assembly.Object });
			Logger.LogDebug("LocateAllMatchingTypes(): assemblyLocator → {AssemblyLocator}", assemblyLocator);

            var typeLocator = new TypeLocator { AssemblyLocator = assemblyLocator.Object };

            var types = typeLocator.GetAllMatchingTypes(x => true).ToList();

			Logger.LogDebug("LocateAllMatchingTypes(): types → {Types}", types);
            Assert.That(types, Is.EquivalentTo(new[] { typeof(TypeLocatorTests), typeof(ServiceRoutingManagerTests) }));
            assembly.Verify(x => x.GetTypes(), Times.Once());
            assemblyLocator.Verify(x => x.Assemblies, Times.Once());
			Logger.LogDebug("<<< Exiting LocateAllMatchingTypes()");
        }
    }
}
