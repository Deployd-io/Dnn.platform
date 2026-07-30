// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Utilities.Fakes
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    public class FakeServiceProvider : IServiceProvider, IServiceScopeFactory, IServiceScope
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IServiceProvider provider;

        private FakeServiceProvider(IServiceProvider provider)
        {
			Logger.LogDebug(">>> Entering FakeServiceProvider(provider={Provider})", provider);
            this.provider = provider;
			Logger.LogDebug("<<< Exiting FakeServiceProvider(provider={Provider})", provider);
			Logger.LogDebug("FakeServiceProvider(provider={Provider}): provider → {Provider1}", provider, provider);
			Logger.LogDebug("FakeServiceProvider(provider={Provider}): this.provider → {Arg1}", provider, this.provider);
        }

        public IServiceProvider ServiceProvider => this;

        public static FakeServiceProvider Create() => Create(new ServiceCollection());

        public static FakeServiceProvider Create(Action<IServiceCollection> setupServices)
        {
			Logger.LogDebug(">>> Entering Create(setupServices={SetupServices})", setupServices);
            var services = new ServiceCollection();
            setupServices(services);
			Logger.LogDebug("Create(setupServices={SetupServices}): services → {Services}", setupServices, services);
			Logger.LogDebug("<<< Exiting Create(setupServices={SetupServices})", setupServices);
            return Create(services);
        }

        public static FakeServiceProvider Create(IServiceCollection services) => new FakeServiceProvider(services.BuildServiceProvider());

        public static FakeServiceProvider Setup(bool setupHttpContextScope = true) => Setup(Create(), setupHttpContextScope);

        public static FakeServiceProvider Setup(Action<IServiceCollection> setupServices, bool setupHttpContextScope = true) => Setup(Create(setupServices), setupHttpContextScope);

        public static FakeServiceProvider Setup(IServiceCollection services, bool setupHttpContextScope = true) => Setup(Create(services), setupHttpContextScope);

        public static T Setup<T>(T provider, bool setupHttpContextScope = true)
            where T : IServiceProvider
        {
			Logger.LogDebug(">>> Entering Setup(provider={Provider},setupHttpContextScope={SetupHttpContextScope})", provider, setupHttpContextScope);
            Globals.DependencyProvider = provider;
            if (setupHttpContextScope)
			Logger.LogDebug("Setup(provider={Provider},setupHttpContextScope={SetupHttpContextScope}): Globals.DependencyProvider → {Arg2}", provider, setupHttpContextScope, Globals.DependencyProvider);
            {
                HttpContextHelper.RegisterMockHttpContext().Object.SetScope(provider.CreateScope());
            }

            return provider;
        }

        public static void Reset()
        {
			Logger.LogDebug(">>> Entering Reset()");
            Globals.DependencyProvider = null;
            HttpContextSource.RegisterInstance(null);
			Logger.LogDebug("<<< Exiting Reset()");
			Logger.LogDebug("Reset(): Globals.DependencyProvider → {Arg0}", Globals.DependencyProvider);
        }

        public object GetService(Type serviceType)
        {
			Logger.LogDebug(">>> Entering GetService(serviceType={ServiceType})", serviceType);
            if (serviceType.IsInstanceOfType(this))
            {
                return this;
            }

            var registeredService = this.provider.GetService(serviceType);
            if (registeredService is not null)
			Logger.LogDebug("GetService(serviceType={ServiceType}): registeredService → {RegisteredService}", serviceType, registeredService);
            {
                return registeredService;
            }

            if (serviceType.IsInterface || serviceType.IsAbstract)
            {
                var mock = Activator.CreateInstance(typeof(Mock<>).MakeGenericType(serviceType));
                return ((Mock)mock).Object;
            }

			Logger.LogDebug("<<< Exiting GetService(serviceType={ServiceType})", serviceType);
            return Activator.CreateInstance(serviceType);
        }

        public IServiceScope CreateScope()
        {
			Logger.LogDebug(">>> Entering CreateScope()");
			Logger.LogDebug("<<< Exiting CreateScope()");
            return this;
        }

        public void Dispose()
        {
			Logger.LogDebug(">>> Entering Dispose()");
            Reset();
            if (this.provider is IDisposable disposable)
            {
                disposable.Dispose();
            }
			Logger.LogDebug("<<< Exiting Dispose()");
        }
    }
}
