// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Razor
{
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.UI.Modules;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public class Startup : IDnnStartup
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
		Logger.LogDebug(">>> Entering ConfigureServices(services={Services})", services);
#pragma warning disable CS0618 // Type or member is obsolete
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IModuleControlFactory, RazorModuleControlFactory>());
#pragma warning restore CS0618 // Type or member is obsolete
		Logger.LogDebug("<<< Exiting ConfigureServices(services={Services})", services);
        }
    }
}
