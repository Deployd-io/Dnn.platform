// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.DDRMenu
{
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Web.DDRMenu.Localisation;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>Registers DDR Menu services.</summary>
    public class Startup : IDnnStartup
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
			Logger.LogDebug(">>> Entering ConfigureServices(services={Services})", services);
            services.AddScoped<ILocaliser, Localiser>();
            services.TryAddEnumerable(ServiceDescriptor.Scoped<ILocalisation, Generic>());
			Logger.LogDebug("<<< Exiting ConfigureServices(services={Services})", services);
        }
    }
}
