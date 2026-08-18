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
	using Microsoft.Extensions.Logging;

    public class FakeServiceProvider : IServiceProvider, IServiceScopeFactory, IServiceScope
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly IServiceProvider provider;

        private FakeServiceProvider(IServiceProvider provider)
        {
			_logger.LogDebug(">>> Entering FakeServiceProvider(provider={Provider})", provider);
            this.provider = provider;
			_logger.LogDebug("FakeServiceProvider(provider={Provider}): this.provider → {Arg1}", provider, this.provider);
			_logger.LogDebug("FakeServiceProvider(provider={Provider}): provider → {Provider1}", provider, provider);
			_logger.LogDebug("<<< Exiting FakeServiceProvider(provider={Provider})", provider);
        }

        public IServiceProvider ServiceProvider => this;

        public static FakeServiceProvider Create() => Create(new ServiceCollection());

        public static FakeServiceProvider Create(Action<IServiceCollection> setupServices)
        {
			_logger.LogDebug(">>> Entering Create(setupServices={SetupServices})", setupServices);
            var services = new ServiceCollection();
            setupServices(services);
			_logger.LogDebug("Create(setupServices={SetupServices}): services → {Services}", setupServices, services);
			_logger.LogDebug("<<< Exiting Create(setupServices={SetupServices})", setupServices);
            return Create(services);
        }

        public static FakeServiceProvider Create(IServiceCollection services) => new FakeServiceProvider(services.BuildServiceProvider());

        public static FakeServiceProvider Setup(bool setupHttpContextScope = true) => Setup(Create(), setupHttpContextScope);

        public static FakeServiceProvider Setup(Action<IServiceCollection> setupServices, bool setupHttpContextScope = true) => Setup(Create(setupServices), setupHttpContextScope);

        public static FakeServiceProvider Setup(IServiceCollection services, bool setupHttpContextScope = true) => Setup(Create(services), setupHttpContextScope);

        public static T Setup<T>(T provider, bool setupHttpContextScope = true)
            where T : IServiceProvider
        {
			_logger.LogDebug(">>> Entering Setup(provider={Provider},setupHttpContextScope={SetupHttpContextScope})", provider, setupHttpContextScope);
            Globals.DependencyProvider = provider;
            if (setupHttpContextScope)
			_logger.LogDebug("Setup(provider={Provider},setupHttpContextScope={SetupHttpContextScope}): Globals.DependencyProvider → {Arg2}", provider, setupHttpContextScope, Globals.DependencyProvider);
            {
                HttpContextHelper.RegisterMockHttpContext().Object.SetScope(provider.CreateScope());
            }

			_logger.LogDebug("<<< Exiting Setup(provider={Provider},setupHttpContextScope={SetupHttpContextScope})", provider, setupHttpContextScope);
            return provider;
        }

        public static void Reset()
        {
			_logger.LogDebug(">>> Entering Reset()");
            Globals.DependencyProvider = null;
            HttpContextSource.RegisterInstance(null);
			_logger.LogDebug("Reset(): Globals.DependencyProvider → {Arg0}", Globals.DependencyProvider);
			_logger.LogDebug("<<< Exiting Reset()");
        }

        public object GetService(Type serviceType)
        {
			_logger.LogDebug(">>> Entering GetService(serviceType={ServiceType})", serviceType);
            if (serviceType.IsInstanceOfType(this))
            {
                return this;
            }

            var registeredService = this.provider.GetService(serviceType);
			_logger.LogDebug("GetService(serviceType={ServiceType}): registeredService → {RegisteredService}", serviceType, registeredService);
            if (registeredService is not null)
            {
                return registeredService;
            }

            if (serviceType.IsInterface || serviceType.IsAbstract)
            {
                var mock = Activator.CreateInstance(typeof(Mock<>).MakeGenericType(serviceType));
				_logger.LogDebug("GetService(serviceType={ServiceType}): mock → {Mock}", serviceType, mock);
			_logger.LogDebug("<<< Exiting GetService(serviceType={ServiceType})", serviceType);
                return ((Mock)mock).Object;
            }

            return Activator.CreateInstance(serviceType);
        }

        public IServiceScope CreateScope()
        {
			_logger.LogDebug(">>> Entering CreateScope()");
			_logger.LogDebug("<<< Exiting CreateScope()");
            return this;
        }

        public void Dispose()
        {
			_logger.LogDebug(">>> Entering Dispose()");
            Reset();
            if (this.provider is IDisposable disposable)
            {
                disposable.Dispose();
            }
			_logger.LogDebug("<<< Exiting Dispose()");
        }
    }
}
