// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library
{
    using Dnn.PersonaBar.Library.Containers;
    using Dnn.PersonaBar.Library.Controllers;

    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Entities.Users;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Register services used by the Persona Bar library.</summary>
    internal sealed class Startup : IDnnStartup
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Startup));
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
			Logger.LogDebug(">>> Entering ConfigureServices(services={Services})", services);
            services.AddSingleton<IPersonaBarContainer, PersonaBarContainer>();
            services.AddSingleton<IPersonaBarController, PersonaBarController>();
            services.AddSingleton<IPersonaBarUserSettingsController, PersonaBarUserSettingsController>();
			Logger.LogDebug("<<< Exiting ConfigureServices(services={Services})", services);
        }
    }
}
