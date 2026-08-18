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
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			mockEventLogger.LogDebug(">>> Entering SetUp()");
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					mockEventLogger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
                    var fakeCachingProvider = new FakeCachingProvider(new Dictionary<string, object>(0));
					mockEventLogger.LogDebug("SetUp(): fakeCachingProvider → {FakeCachingProvider}", fakeCachingProvider);
                    ComponentFactory.RegisterComponentInstance<CachingProvider>(fakeCachingProvider);
                    services.AddSingleton<CachingProvider>(fakeCachingProvider);
                });
				mockEventLogger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			mockEventLogger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			mockEventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [TestCase(typeof(PortableControllerClass), true)]
        [TestCase(typeof(SearchableControllerClass), false)]
        [TestCase(typeof(AllFeaturesControllerClass), true)]
        public void CanGetPortableInstanceFromType(Type businessControllerType, bool implementsPortable)
        {
			mockEventLogger.LogDebug(">>> Entering CanGetPortableInstanceFromType(businessControllerType={BusinessControllerType},implementsPortable={ImplementsPortable})", businessControllerType, implementsPortable);
            var provider = CreateProvider();
            var portable = provider.GetInstance<IPortable>(businessControllerType);
			mockEventLogger.LogDebug("CanGetPortableInstanceFromType(businessControllerType={BusinessControllerType},implementsPortable={ImplementsPortable}): portable → {Portable}", businessControllerType, implementsPortable, portable);
			mockEventLogger.LogDebug("CanGetPortableInstanceFromType(businessControllerType={BusinessControllerType},implementsPortable={ImplementsPortable}): provider → {Provider}", businessControllerType, implementsPortable, provider);

            if (implementsPortable)
            {
                Assert.That(portable, Is.Not.Null);
                Assert.That(portable, Is.InstanceOf<IPortable>());
            }
            else
            {
                Assert.That(portable, Is.Null);
            }
			mockEventLogger.LogDebug("<<< Exiting CanGetPortableInstanceFromType(businessControllerType={BusinessControllerType},implementsPortable={ImplementsPortable})", businessControllerType, implementsPortable);
        }

        [TestCase(typeof(PortableControllerClass), true)]
        [TestCase(typeof(SearchableControllerClass), false)]
        [TestCase(typeof(AllFeaturesControllerClass), true)]
        public void CanGetPortableInstanceFromTypeName(Type businessControllerType, bool implementsPortable)
        {
			mockEventLogger.LogDebug(">>> Entering CanGetPortableInstanceFromTypeName(businessControllerType={BusinessControllerType},implementsPortable={ImplementsPortable})", businessControllerType, implementsPortable);
            var provider = CreateProvider();
            var portable = provider.GetInstance<IPortable>(businessControllerType.AssemblyQualifiedName);
			mockEventLogger.LogDebug("CanGetPortableInstanceFromTypeName(businessControllerType={BusinessControllerType},implementsPortable={ImplementsPortable}): portable → {Portable}", businessControllerType, implementsPortable, portable);
			mockEventLogger.LogDebug("CanGetPortableInstanceFromTypeName(businessControllerType={BusinessControllerType},implementsPortable={ImplementsPortable}): provider → {Provider}", businessControllerType, implementsPortable, provider);

            if (implementsPortable)
            {
                Assert.That(portable, Is.Not.Null);
                Assert.That(portable, Is.InstanceOf<IPortable>());
            }
            else
            {
                Assert.That(portable, Is.Null);
            }
			mockEventLogger.LogDebug("<<< Exiting CanGetPortableInstanceFromTypeName(businessControllerType={BusinessControllerType},implementsPortable={ImplementsPortable})", businessControllerType, implementsPortable);
        }

        [TestCase(typeof(PortableControllerClass), false)]
        [TestCase(typeof(SearchableControllerClass), true)]
        [TestCase(typeof(AllFeaturesControllerClass), true)]
        public void CanGetSearchableInstanceFromType(Type businessControllerType, bool implementsSearchable)
        {
			mockEventLogger.LogDebug(">>> Entering CanGetSearchableInstanceFromType(businessControllerType={BusinessControllerType},implementsSearchable={ImplementsSearchable})", businessControllerType, implementsSearchable);
            var provider = CreateProvider();
            var searchable = provider.GetInstance<ModuleSearchBase>(businessControllerType);
			mockEventLogger.LogDebug("CanGetSearchableInstanceFromType(businessControllerType={BusinessControllerType},implementsSearchable={ImplementsSearchable}): searchable → {Searchable}", businessControllerType, implementsSearchable, searchable);
			mockEventLogger.LogDebug("CanGetSearchableInstanceFromType(businessControllerType={BusinessControllerType},implementsSearchable={ImplementsSearchable}): provider → {Provider}", businessControllerType, implementsSearchable, provider);

            if (implementsSearchable)
            {
                Assert.That(searchable, Is.Not.Null);
                Assert.That(searchable, Is.InstanceOf<ModuleSearchBase>());
            }
            else
            {
                Assert.That(searchable, Is.Null);
            }
			mockEventLogger.LogDebug("<<< Exiting CanGetSearchableInstanceFromType(businessControllerType={BusinessControllerType},implementsSearchable={ImplementsSearchable})", businessControllerType, implementsSearchable);
        }

        [TestCase(typeof(PortableControllerClass), false)]
        [TestCase(typeof(SearchableControllerClass), true)]
        [TestCase(typeof(AllFeaturesControllerClass), true)]
        public void CanGetSearchableInstanceFromTypeName(Type businessControllerType, bool implementsSearchable)
        {
			mockEventLogger.LogDebug(">>> Entering CanGetSearchableInstanceFromTypeName(businessControllerType={BusinessControllerType},implementsSearchable={ImplementsSearchable})", businessControllerType, implementsSearchable);
            var provider = CreateProvider();
            var searchable = provider.GetInstance<ModuleSearchBase>(businessControllerType.AssemblyQualifiedName);
			mockEventLogger.LogDebug("CanGetSearchableInstanceFromTypeName(businessControllerType={BusinessControllerType},implementsSearchable={ImplementsSearchable}): searchable → {Searchable}", businessControllerType, implementsSearchable, searchable);
			mockEventLogger.LogDebug("CanGetSearchableInstanceFromTypeName(businessControllerType={BusinessControllerType},implementsSearchable={ImplementsSearchable}): provider → {Provider}", businessControllerType, implementsSearchable, provider);

            if (implementsSearchable)
            {
                Assert.That(searchable, Is.Not.Null);
                Assert.That(searchable, Is.InstanceOf<ModuleSearchBase>());
            }
            else
            {
                Assert.That(searchable, Is.Null);
            }
			mockEventLogger.LogDebug("<<< Exiting CanGetSearchableInstanceFromTypeName(businessControllerType={BusinessControllerType},implementsSearchable={ImplementsSearchable})", businessControllerType, implementsSearchable);
        }

        [TestCase(typeof(PortableControllerClass))]
        [TestCase(typeof(SearchableControllerClass))]
        [TestCase(typeof(AllFeaturesControllerClass))]
        public void CanGetInstanceFromType(Type businessControllerType)
        {
			mockEventLogger.LogDebug(">>> Entering CanGetInstanceFromType(businessControllerType={BusinessControllerType})", businessControllerType);
            var provider = CreateProvider();
            var controller = provider.GetInstance(businessControllerType);
			mockEventLogger.LogDebug("CanGetInstanceFromType(businessControllerType={BusinessControllerType}): controller → {Controller}", businessControllerType, controller);
			mockEventLogger.LogDebug("CanGetInstanceFromType(businessControllerType={BusinessControllerType}): provider → {Provider}", businessControllerType, provider);

            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.GetType(), Is.EqualTo(businessControllerType));
			mockEventLogger.LogDebug("<<< Exiting CanGetInstanceFromType(businessControllerType={BusinessControllerType})", businessControllerType);
        }

        [TestCase(typeof(PortableControllerClass))]
        [TestCase(typeof(SearchableControllerClass))]
        [TestCase(typeof(AllFeaturesControllerClass))]
        public void CanGetInstanceFromTypeName(Type businessControllerType)
        {
			mockEventLogger.LogDebug(">>> Entering CanGetInstanceFromTypeName(businessControllerType={BusinessControllerType})", businessControllerType);
            var provider = CreateProvider();
            var controller = provider.GetInstance(businessControllerType.AssemblyQualifiedName);
			mockEventLogger.LogDebug("CanGetInstanceFromTypeName(businessControllerType={BusinessControllerType}): controller → {Controller}", businessControllerType, controller);
			mockEventLogger.LogDebug("CanGetInstanceFromTypeName(businessControllerType={BusinessControllerType}): provider → {Provider}", businessControllerType, provider);

            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.GetType(), Is.EqualTo(businessControllerType));
			mockEventLogger.LogDebug("<<< Exiting CanGetInstanceFromTypeName(businessControllerType={BusinessControllerType})", businessControllerType);
        }

        private static BusinessControllerProvider CreateProvider()
        {
			mockEventLogger.LogDebug(">>> Entering CreateProvider()");
			mockEventLogger.LogDebug("<<< Exiting CreateProvider()");
            return new BusinessControllerProvider(Globals.DependencyProvider);
        }

        private class PortableControllerClass : IPortable
        {
            public string ExportModule(int moduleID) => "Done";

            public void ImportModule(int moduleID, string content, string version, int userID)
            {
			mockEventLogger.LogDebug(">>> Entering ImportModule(moduleID={ModuleID},content={Content},version={Version},userID={UserID})", moduleID, content, version, userID);
			mockEventLogger.LogDebug(">>> Entering ExportModule(moduleID={ModuleID})", moduleID);
			mockEventLogger.LogDebug("<<< Exiting ImportModule(moduleID={ModuleID},content={Content},version={Version},userID={UserID})", moduleID, content, version, userID);
            }
        }

        private class SearchableControllerClass : ModuleSearchBase
        {
            public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDateUtc)
            {
				mockEventLogger.LogDebug(">>> Entering GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc})", moduleInfo, beginDateUtc);
				mockEventLogger.LogDebug("<<< Exiting GetModifiedSearchDocuments(moduleInfo={ModuleInfo},beginDateUtc={BeginDateUtc})", moduleInfo, beginDateUtc);
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
			mockEventLogger.LogDebug(">>> Entering UpgradeModule(version={Version})", version);
        }
    }
}
mockEventLogger.LogDebug("<<< Exiting UpgradeModule(version={Version})", version);
