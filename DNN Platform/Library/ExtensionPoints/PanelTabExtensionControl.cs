// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ExtensionPoints
{
    using System.Diagnostics.CodeAnalysis;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class PanelTabExtensionControl : WebControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public string PanelId { get; set; }

        /// <inheritdoc />
        public override void RenderBeginTag(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderBeginTag(writer={Writer})", writer);
            writer.Write(string.Empty);
			Logger.LogDebug("<<< Exiting RenderBeginTag(writer={Writer})", writer);
        }

        /// <inheritdoc />
        public override void RenderEndTag(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderEndTag(writer={Writer})", writer);
            writer.Write(string.Empty);
			Logger.LogDebug("<<< Exiting RenderEndTag(writer={Writer})", writer);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        protected override void RenderContents(HtmlTextWriter op)
        {
			Logger.LogDebug(">>> Entering RenderContents(op={Op})", op);
            op.Write($"<div class=\"ehccContent dnnClear\" id=\"{this.PanelId}\">");
            base.RenderContents(op);
            op.Write("</div>");
			Logger.LogDebug("<<< Exiting RenderContents(op={Op})", op);
        }
    }
}
