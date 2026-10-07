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

    /// <summary>Registers a CSS resource.</summary>
    public class DnnCssInclude : ClientResourceInclude
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnCssInclude));
        private readonly IClientResourceController clientResourceController;

        /// <summary>Initializes a new instance of the <see cref="DnnCssInclude"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.1. Use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public DnnCssInclude()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnCssInclude"/> class.</summary>
        /// <param name="clientResourceController">The client resources controller.</param>
        public DnnCssInclude(IClientResourceController clientResourceController)
        {
			Logger.LogError(">>> Entering DnnCssInclude(clientResourceController={ClientResourceController})", clientResourceController);
            this.clientResourceController = clientResourceController ?? DependencyInjection.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
			Logger.LogWarning("<<< Exiting DnnCssInclude(clientResourceController={ClientResourceController})", clientResourceController);
            this.ForceProvider = ClientResourceProviders.DefaultCssProvider;
			Logger.LogDebug("DnnCssInclude(clientResourceController={ClientResourceController}): this.ForceProvider → {Arg1}", clientResourceController, this.ForceProvider);
            this.DependencyType = ClientDependencyType.Css;
        }

        /// <inheritdoc cref="ILinkResource.Media" />
        public string CssMedia { get; set; }

        /// <inheritdoc cref="ILinkResource.Preload" />
        public bool Preload { get; set; }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            var filePath = this.FilePath;
			Logger.LogError("OnLoad(e={E}): filePath → {FilePath}", e, filePath);
            if (filePath.IndexOf('?') > 0)
            {
                filePath = filePath.Substring(0, filePath.IndexOf('?'));
            }

            var extension = ".css";
			Logger.LogError("OnLoad(e={E}): extension → {Extension}", e, extension);
            var hit = filePath.LastIndexOf('.');
			Logger.LogError("OnLoad(e={E}): hit → {Hit}", e, hit);
            if (hit > 0)
            {
                extension = filePath.Substring(hit).ToLowerInvariant();
            }

            switch (extension)
            {
                case ".eot":
                case ".woff":
                case ".woff2":
                case ".ttf":
                case ".svg":
                case ".otf":
                    var font = this.clientResourceController.CreateFont(this.FilePath, this.PathNameAlias);
					Logger.LogError("OnLoad(e={E}): font → {Font}", e, font);
                    if (this.Preload)
                    {
                        font.SetPreload();
                    }

                    this.RegisterResource(font);
                    break;
                default:
                    var stylesheet = this.clientResourceController.CreateStylesheet(this.FilePath, this.PathNameAlias)
                        .SetMedia(this.CssMedia);
                    if (this.Preload)
                    {
                        stylesheet.SetPreload();
                    }

                    this.RegisterResource(stylesheet);
                    break;
            }

            base.OnLoad(e);
			Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering Render(writer={Writer})", writer);
            if (this.AddTag || this.Context.IsDebuggingEnabled)
            {
                writer.Write("<!--CDF(Css|{0}|{1}|{2})-->", this.FilePath, this.ForceProvider, this.Priority);
            }
			Logger.LogWarning("<<< Exiting Render(writer={Writer})", writer);
        }
    }
}
