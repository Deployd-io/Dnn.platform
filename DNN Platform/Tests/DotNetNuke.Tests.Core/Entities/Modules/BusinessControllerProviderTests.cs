// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Entities.Modules
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Common;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class BusinessControllerProviderTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("SetUp(): services → {Services}", services);
				Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
                    var fakeCachingProvider = new FakeCachingProvider(new Dictionary<string, object>(0));
                    ComponentFactory.RegisterComponentInstance<CachingProvider>(fakeCachingProvider);
					Logger.LogDebug("SetUp(): fakeCachingProvider → {FakeCachingProvider}", fakeCachingProvider);
                    services.AddSingleton<CachingProvider>(fakeCachingProvider);
                });
				Logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [TestCase(typeof(PortableControllerClass), true)]
        [TestCase(typeof(SearchableControllerClass), false)]
        [TestCase(typeof(AllFeaturesControllerClass), true)]
        public void CanGetPortableInstanceFromType(Type businessControllerType, bool implementsPortable)
        {
			Logger.LogDebug(">>> Entering CanGetPortableInstanceFromType(businessControllerType={BusinessControllerType},implementsPortable={ImplementsPortable})", businessControllerType, implementsPortable);
            var provider = CreateProvider();
            var portable = provider.GetInstance<IPortable>(businessControllerType);
			Logger.LogDebug("CanGetPortableInstanceFromType(businessControllerType={BusinessControllerType},implementsPortable={ImplementsPortable}): provider → {Provider}", businessControllerType, implementsPortable, provider);
			Logger.LogDebug("CanGetPortableInstanceFromType(businessControllerType={BusinessControllerType},implementsPortable={ImplementsPortable}): portable → {Portable}", businessControllerType, implementsPortable, portable);

            if (implementsPortable)
            {
                Assert.That(portable, Is.Not.Null);
                Assert.That(portable, Is.InstanceOf<IPortable>());
            }
            else
            {
                Assert.That(portable, Is.Null);
            }
			Logger.LogDebug("<<< Exiting CanGetPortableInstanceFromType(businessControllerType={BusinessControllerType},implementsPortable={ImplementsPortable})", businessControllerType, implementsPortable);
        }

        [TestCase(typeof(PortableControllerClass), true)]
        [TestCase(typeof(SearchableControllerClass), false)]
        [TestCase(typeof(AllFeaturesControllerClass), true)]
        public void CanGetPortableInstanceFromTypeName(Type businessControllerType, bool implementsPortable)
        {
			Logger.LogDebug(">>> Entering CanGetPortableInstanceFromTypeName(businessControllerType={BusinessControllerType},implementsPortable={ImplementsPortable})", businessControllerType, implementsPortable);
            var provider = CreateProvider();
            var portable = provider.GetInstance<IPortable>(businessControllerType.AssemblyQualifiedName);
			Logger.LogDebug("CanGetPortableInstanceFromTypeName(businessControllerType={BusinessControllerType},implementsPortable={ImplementsPortable}): provider → {Provider}", businessControllerType, implementsPortable, provider);
			Logger.LogDebug("CanGetPortableInstanceFromTypeName(businessControllerType={BusinessControllerType},implementsPortable={ImplementsPortable}): portable → {Portable}", businessControllerType, implementsPortable, portable);

            if (implementsPortable)
            {
                Assert.That(portable, Is.Not.Null);
                Assert.That(portable, Is.InstanceOf<IPortable>());
            }
            else
            {
                Assert.That(portable, Is.Null);
            }
			Logger.LogDebug("<<< Exiting CanGetPortableInstanceFromTypeName(businessControllerType={BusinessControllerType},implementsPortable={ImplementsPortable})", businessControllerType, implementsPortable);
        }

        [TestCase(typeof(PortableControllerClass), false)]
        [TestCase(typeof(SearchableControllerClass), true)]
        [TestCase(typeof(AllFeaturesControllerClass), true)]
        public void CanGetSearchableInstanceFromType(Type businessControllerType, bool implementsSearchable)
        {
			Logger.LogDebug(">>> Entering CanGetSearchableInstanceFromType(businessControllerType={BusinessControllerType},implementsSearchable={ImplementsSearchable})", businessControllerType, implementsSearchable);
            var provider = CreateProvider();
            var searchable = provider.GetInstance<ModuleSearchBase>(businessControllerType);
			Logger.LogDebug("CanGetSearchableInstanceFromType(businessControllerType={BusinessControllerType},implementsSearchable={ImplementsSearchable}): provider → {Provider}", businessControllerType, implementsSearchable, provider);
			Logger.LogDebug("CanGetSearchableInstanceFromType(businessControllerType={BusinessControllerType},implementsSearchable={ImplementsSearchable}): searchable → {Searchable}", businessControllerType, implementsSearchable, searchable);

            if (implementsSearchable)
            {
                Assert.That(searchable, Is.Not.Null);
                Assert.That(searchable, Is.InstanceOf<ModuleSearchBase>());
            }
            else
            {
                Assert.That(searchable, Is.Null);
            }
			Logger.LogDebug("<<< Exiting CanGetSearchableInstanceFromType(businessControllerType={BusinessControllerType},implementsSearchable={ImplementsSearchable})", businessControllerType, implementsSearchable);
        }

        [TestCase(typeof(PortableControllerClass), false)]
        [TestCase(typeof(SearchableControllerClass), true)]
        [TestCase(typeof(AllFeaturesControllerClass), true)]
        public void CanGetSearchableInstanceFromTypeName(Type businessControllerType, bool implementsSearchable)
        {
			Logger.LogDebug(">>> Entering CanGetSearchableInstanceFromTypeName(businessControllerType={BusinessControllerType},implementsSearchable={ImplementsSearchable})", businessControllerType, implementsSearchable);
            var provider = CreateProvider();
            var searchable = provider.GetInstance<ModuleSearchBase>(businessControllerType.AssemblyQualifiedName);
			Logger.LogDebug("CanGetSearchableInstanceFromTypeName(businessControllerType={BusinessControllerType},implementsSearchable={ImplementsSearchable}): provider → {Provider}", businessControllerType, implementsSearchable, provider);
			Logger.LogDebug("CanGetSearchableInstanceFromTypeName(businessControllerType={BusinessControllerType},implementsSearchable={ImplementsSearchable}): searchable → {Searchable}", businessControllerType, implementsSearchable, searchable);

            if (implementsSearchable)
            {
                Assert.That(searchable, Is.Not.Null);
                Assert.That(searchable, Is.InstanceOf<ModuleSearchBase>());
            }
            else
            {
                Assert.That(searchable, Is.Null);
            }
			Logger.LogDebug("<<< Exiting CanGetSearchableInstanceFromTypeName(businessControllerType={BusinessControllerType},implementsSearchable={ImplementsSearchable})", businessControllerType, implementsSearchable);
        }

        [TestCase(typeof(PortableControllerClass))]
        [TestCase(typeof(SearchableControllerClass))]
        [TestCase(typeof(AllFeaturesControllerClass))]
        public void CanGetInstanceFromType(Type businessControllerType)
        {
			Logger.LogDebug(">>> Entering CanGetInstanceFromType(businessControllerType={BusinessControllerType})", businessControllerType);
            var provider = CreateProvider();
            var controller = provider.GetInstance(businessControllerType);
			Logger.LogDebug("CanGetInstanceFromType(businessControllerType={BusinessControllerType}): provider → {Provider}", businessControllerType, provider);
			Logger.LogDebug("CanGetInstanceFromType(businessControllerType={BusinessControllerType}): controller → {Controller}", businessControllerType, controller);

            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.GetType(), Is.EqualTo(businessControllerType));
			Logger.LogDebug("<<< Exiting CanGetInstanceFromType(businessControllerType={BusinessControllerType})", businessControllerType);
        }

        [TestCase(typeof(PortableControllerClass))]
        [TestCase(typeof(SearchableControllerClass))]
        [TestCase(typeof(AllFeaturesControllerClass))]
        public void CanGetInstanceFromTypeName(Type businessControllerType)
        {
			Logger.LogDebug(">>> Entering CanGetInstanceFromTypeName(businessControllerType={BusinessControllerType})", businessControllerType);
            var provider = CreateProvider();
            var controller = provider.GetInstance(businessControllerType.AssemblyQualifiedName);
			Logger.LogDebug("CanGetInstanceFromTypeName(businessControllerType={BusinessControllerType}): provider → {Provider}", businessControllerType, provider);
			Logger.LogDebug("CanGetInstanceFromTypeName(businessControllerType={BusinessControllerType}): controller → {Controller}", businessControllerType, controller);

            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.GetType(), Is.EqualTo(businessControllerType));
			Logger.LogDebug("<<< Exiting CanGetInstanceFromTypeName(businessControllerType={BusinessControllerType})", businessControllerType);
        }

        private static BusinessControllerProvider CreateProvider()
        {
			Logger.LogDebug(">>> Entering CreateProvider()");
			Logger.LogDebug("<<< Exiting CreateProvider()");
            return new BusinessControllerProvider(Globals.DependencyProvider);
        }

        private class PortableControllerClass : IPortable
        {
            public string ExportModule(int moduleID) => "Done";

            public void ImportModule(int moduleID, string content, string version, int userID)
            {
			Logger.LogDebug(">>> Entering ImportModule(moduleID={ModuleID},content={Content},version={Version},userID={UserID})", moduleID, content, version, userID);
			Logger.LogDebug(">>> Entering ExportModule(moduleID={ModuleID})", moduleID);
            }
        }

        private class SearchableControllerClass : ModuleSearchBase
        {
            public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDateUtc)
            {
				Logger.LogDebug(">>> Entering GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc})", moduleInfo, beginDateUtc);
				Logger.LogDebug("<<< Exiting GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc})", moduleInfo, beginDateUtc);
                return Array.Empty<SearchDocument>();
            }
        }

        private class AllFeaturesControllerClass : ModuleSearchBase, IPortable, IUpgradeable
        {
            public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDateUtc)
            {
                return Array.Empty<SearchDocument>();
            }

            public string ExportModule(int moduleID) => $"Exported {moduleID}";

            public void ImportModule(int moduleID, string content, string version, int userID)
            {
            }

            public string UpgradeModule(string version) => $"Upgraded to {version}";
			Logger.LogDebug(">>> Entering UpgradeModule(version={Version})", version);
        }
    }
}
