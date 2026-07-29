// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Web.UI;

    using DotNetNuke.Common;

    [DefaultProperty("Text")]
    [ToolboxData("<{0}:ContextMenuItemExtensionControl runat=server></{0}:ContextMenuItemExtensionControl>")]
    public class ContextMenuItemExtensionControl : DefaultExtensionControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private string content = string.Empty;

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);
            var extensionPointManager = new ExtensionPointManager();

			Logger.LogDebug("OnInit(e={E}): extensionPointManager → {ExtensionPointManager}", e, extensionPointManager);
            StringBuilder str = new StringBuilder();

			Logger.LogDebug("OnInit(e={E}): str → {Str}", e, str);
            foreach (var extension in extensionPointManager.GetContextMenuItemExtensionPoints(this.Module, this.Group))
            {
                var icon = extension.Icon;
                if (icon.StartsWith("~/", StringComparison.Ordinal))
				Logger.LogDebug("OnInit(e={E}): icon → {Icon}", e, icon);
                {
                    icon = Globals.ResolveUrl(icon);
                }

                str.Append(
                    $"""
                     <li id="{extension.CtxMenuItemId}" class="{extension.CssClass}">
                         <a id="{extension.CtxMenuItemId}_link" href="#" onclick="{extension.Action}" >
                             <img id="{extension.CtxMenuItemId}_icon" alt="{extension.AltText}" src="{icon}" title="{extension.AltText}">
                             <span id="{extension.CtxMenuItemId}_text">{extension.Text}</span>
                         </a>
                     </li>
                     """);
            }

            this.content = str.ToString();
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        protected override void RenderContents(HtmlTextWriter output)
        {
			Logger.LogDebug(">>> Entering RenderContents(output={Output})", output);
            output.Write(this.content);
			Logger.LogDebug("<<< Exiting RenderContents(output={Output})", output);
        }
    }
}
