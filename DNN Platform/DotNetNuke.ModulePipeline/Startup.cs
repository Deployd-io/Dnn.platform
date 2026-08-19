// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ModulePipeline
{
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.UI.Modules;
    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc />
    public class Startup : IDnnStartup
    {
		private static readonly ILog TraceLogger = LoggerSource.Instance.GetLogger("DNN.Trace");
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
			TraceLogger.LogDebug(">>> Entering ConfigureServices(services={Services})", services);
            services.AddSingleton<IModuleControlPipeline, ModuleControlPipeline>();
			TraceLogger.LogDebug("<<< Exiting ConfigureServices(services={Services})", services);
        }
    }
}
