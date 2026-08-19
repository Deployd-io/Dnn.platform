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
		Logger.LogDebug(">>> Entering DnnCssInclude()");
        }

		Logger.LogDebug("<<< Exiting DnnCssInclude()");
        /// <summary>Initializes a new instance of the <see cref="DnnCssInclude"/> class.</summary>
        /// <param name="clientResourceController">The client resources controller.</param>
        public DnnCssInclude(IClientResourceController clientResourceController)
        {
            this.clientResourceController = clientResourceController ?? DependencyInjection.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
            this.ForceProvider = ClientResourceProviders.DefaultCssProvider;
			Logger.LogDebug("DnnCssInclude(clientResourceController={ClientResourceController}): this.ForceProvider → {Arg1}", clientResourceController, this.ForceProvider);
            this.DependencyType = ClientDependencyType.Css;
            this.Priority = (int)FileOrder.Css.DefaultPriority;
        }

        /// <inheritdoc cref="ILinkResource.Media" />
        public string CssMedia { get; set; }

        /// <inheritdoc cref="ILinkResource.Preload" />
        public bool Preload { get; set; }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            var filePath = this.FilePath;
            if (filePath.IndexOf('?') > 0)
            {
                filePath = filePath.Substring(0, filePath.IndexOf('?'));
            }

            var extension = ".css";
			Logger.LogDebug("OnLoad(e={E}): extension → {Extension}", e, extension);
            var hit = filePath.LastIndexOf('.');
			Logger.LogDebug("OnLoad(e={E}): hit → {Hit}", e, hit);
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
					Logger.LogDebug("OnLoad(e={E}): font → {Font}", e, font);
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
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering Render(writer={Writer})", writer);
            if (this.AddTag || this.Context.IsDebuggingEnabled)
            {
                writer.Write("<!--CDF(Css|{0}|{1}|{2})-->", this.FilePath, this.ForceProvider, this.Priority);
            }
			Logger.LogDebug("<<< Exiting Render(writer={Writer})", writer);
        }
    }
}
