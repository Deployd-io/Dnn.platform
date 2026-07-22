// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;

    using DotNetNuke.Common;

    public class ToolBarMenuButtonRenderer : IExtensionControlRenderer
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public string GetOutput(IExtensionPoint extensionPoint)
        {
			Logger.LogDebug(">>> Entering GetOutput(extensionPoint={ExtensionPoint})", extensionPoint);
            var extension = (IToolBarMenuButtonExtensionPoint)extensionPoint;

			Logger.LogDebug("GetOutput(extensionPoint={ExtensionPoint}): extension → {Extension}", extensionPoint, extension);
            var cssClass = extension.CssClass;
            var action = extension.Action;
            if (!extension.Enabled)
			Logger.LogDebug("GetOutput(extensionPoint={ExtensionPoint}): action → {Action}", extensionPoint, action);
            {
                cssClass += " disabled";
                action = "void(0);";
            }

            var icon = extension.Icon;
            if (icon.StartsWith("~/", StringComparison.Ordinal))
			Logger.LogDebug("GetOutput(extensionPoint={ExtensionPoint}): icon → {Icon}", extensionPoint, icon);
            {
                icon = Globals.ResolveUrl(icon);
            }

            var str = new StringBuilder();
            str.AppendFormat(CultureInfo.InvariantCulture, "<div id='{0}_wrapper' class='{1}_wrapper'>", extension.ButtonId, extension.MenuCssClass);
			Logger.LogDebug("GetOutput(extensionPoint={ExtensionPoint}): str → {Str}", extensionPoint, str);
            str.AppendFormat(
                CultureInfo.InvariantCulture,
                "<button id='{0}' class='{1} {2}' onclick='{3}; return false;' title='{4}'>",
                extension.ButtonId,
                cssClass,
                extension.MenuCssClass,
                action,
                extension.Text);
            str.AppendFormat(
                CultureInfo.InvariantCulture,
                "<span id='{0}_text' style='{1} background-image: url(\"{2}\");'>{3}</span>",
                extension.ButtonId,
                !extension.ShowText ? "text-indent: -10000000px;" : string.Empty,
                extension.ShowIcon ? icon : string.Empty,
                extension.Text);
            str.AppendLine("</button>");
            str.AppendFormat(CultureInfo.InvariantCulture, "<div class='{0}_menu dnnClear'>", extension.MenuCssClass);
            str.AppendLine("<div class='handle'></div>");
            str.AppendLine("<ul>");
            foreach (var item in extension.Items)
            {
                str.AppendLine(GetItemOutput(item));
            }

            str.AppendLine("</ul>");
            str.AppendLine("</div>");
            str.AppendLine("</div>");

			Logger.LogDebug("<<< Exiting GetOutput(extensionPoint={ExtensionPoint})", extensionPoint);
            return str.ToString();
        }

        private static string GetItemOutput(IMenuButtonItemExtensionPoint item)
        {
			Logger.LogDebug(">>> Entering GetItemOutput(item={Item})", item);
            if (string.IsNullOrEmpty(item.Type))
            {
                return $"<li class='{item.CssClass}' id='{item.ItemId}' ><a href='#' onclick='{item.Action}; return false;'><span {item.Attributes}>{item.Text}</span></a></li>";
            }

			Logger.LogDebug("<<< Exiting GetItemOutput(item={Item})", item);
            return $"<li><input type='{item.Type}' name='{item.ItemId}' id='{item.ItemId}' value='{item.Text}' {item.Attributes} onclick='{item.Action}; return false;'/>{item.Text}</li>";
        }
    }
}
