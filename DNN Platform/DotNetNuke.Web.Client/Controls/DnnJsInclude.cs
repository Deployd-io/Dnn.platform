// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Web.Client.Cdf;
    using DotNetNuke.Web.Client.ResourceManager;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Registers a JavaScript resource.</summary>
    public class DnnJsInclude : ClientResourceInclude
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnJsInclude));
        private readonly IClientResourceController clientResourceController;

        /// <summary>Initializes a new instance of the <see cref="DnnJsInclude"/> class with default settings.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public DnnJsInclude()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnJsInclude"/> class with default settings.</summary>
        /// <param name="clientResourceController">The client resources controller.</param>
        public DnnJsInclude(IClientResourceController clientResourceController)
        {
			Logger.LogError(">>> Entering DnnJsInclude(clientResourceController={ClientResourceController})", clientResourceController);
            this.clientResourceController = clientResourceController ?? DependencyInjection.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
			Logger.LogWarning("<<< Exiting DnnJsInclude(clientResourceController={ClientResourceController})", clientResourceController);
            this.ForceProvider = ClientResourceProviders.DefaultJsProvider;
			Logger.LogDebug("DnnJsInclude(clientResourceController={ClientResourceController}): this.ForceProvider → {Arg1}", clientResourceController, this.ForceProvider);
            this.DependencyType = ClientDependencyType.Javascript;
        }

        /// <inheritdoc cref="IScriptResource.Async" />
        public bool Async { get; set; }

        /// <inheritdoc cref="IScriptResource.Defer" />
        public bool Defer { get; set; }

        /// <inheritdoc cref="IScriptResource.NoModule" />
        public bool NoModule { get; set; }

        /// <inheritdoc />
        protected override void OnLoad(System.EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            var script = this.clientResourceController.CreateScript(this.FilePath, this.PathNameAlias);
			Logger.LogError("OnLoad(e={E}): script → {Script}", e, script);
            if (this.Async)
            {
                script = script.SetAsync();
            }

            if (this.Defer)
            {
                script = script.SetDefer();
            }

            if (this.NoModule)
            {
                script = script.SetNoModule();
            }

            this.RegisterResource(script);
            base.OnLoad(e);
			Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering Render(writer={Writer})", writer);
            if (this.AddTag || this.Context.IsDebuggingEnabled)
            {
                writer.Write("<!--CDF(Javascript|{0}|{1}|{2})-->", this.FilePath, this.ForceProvider, this.Priority);
            }
			Logger.LogWarning("<<< Exiting Render(writer={Writer})", writer);
        }
    }
}
