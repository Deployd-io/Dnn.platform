// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.ExtensionPoints.Filters;

    [DefaultProperty("Text")]
    [ToolboxData("<{0}:ToolBarButtonExtensionControl runat=server></{0}:ToolBarButtonExtensionControl>")]
    public class ToolBarButtonExtensionControl : DefaultExtensionControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private IExtensionControlRenderer btnRenderer;

        [Bindable(true)]
        [DefaultValue(false)]
        public bool IsHost { get; set; }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        protected override void RenderContents(HtmlTextWriter output)
        {
			Logger.LogDebug(">>> Entering RenderContents(output={Output})", output);
            var extensionPointManager = new ExtensionPointManager();

			Logger.LogDebug("RenderContents(output={Output}): extensionPointManager → {ExtensionPointManager}", output, extensionPointManager);
            var str = new StringBuilder();

			Logger.LogDebug("RenderContents(output={Output}): str → {Str}", output, str);
            var filter = new CompositeFilter()
                .And(new FilterByHostMenu(this.IsHost))
                .And(new FilterByUnauthenticated(HttpContext.Current.Request.IsAuthenticated));

            foreach (var extension in extensionPointManager.GetToolBarButtonExtensionPoints(this.Module, this.Group, filter))
            {
                if (extension is IToolBarMenuButtonExtensionPoint)
                {
                    this.btnRenderer = new ToolBarMenuButtonRenderer();
					Logger.LogDebug("RenderContents(output={Output}): btnRenderer → {BtnRenderer}", output, btnRenderer);
                }
                else
                {
                    extension.ModuleContext = this.ModuleContext;
					Logger.LogDebug("RenderContents(output={Output}): extension.ModuleContext → {Arg1}", output, extension.ModuleContext);
                    this.btnRenderer = new ToolBarButtonRenderer();
                }

                str.AppendFormat(CultureInfo.InvariantCulture, this.btnRenderer.GetOutput(extension));
            }

            output.Write(str.ToString());
			Logger.LogDebug("<<< Exiting RenderContents(output={Output})", output);
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering Render(writer={Writer})", writer);
            this.RenderContents(writer);
			Logger.LogDebug("<<< Exiting Render(writer={Writer})", writer);
        }
    }
}
