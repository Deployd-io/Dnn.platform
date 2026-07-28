// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;

    /// <summary>Information about a ribbon bar tool.</summary>
    [Serializable]
    public class RibbonBarToolInfo
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="RibbonBarToolInfo"/> class.</summary>
        public RibbonBarToolInfo()
        {
			Logger.LogDebug(">>> Entering RibbonBarToolInfo()");
            this.ControlKey = string.Empty;
            this.ModuleFriendlyName = string.Empty;
			Logger.LogDebug("RibbonBarToolInfo(): this.ControlKey → {Arg0}", this.ControlKey);
			Logger.LogDebug("RibbonBarToolInfo(): this.ModuleFriendlyName → {Arg0}", this.ModuleFriendlyName);
            this.LinkWindowTarget = string.Empty;
            this.ToolName = string.Empty;
			Logger.LogDebug("<<< Exiting RibbonBarToolInfo()");
			Logger.LogDebug("RibbonBarToolInfo(): this.LinkWindowTarget → {Arg0}", this.LinkWindowTarget);
			Logger.LogDebug("RibbonBarToolInfo(): this.ToolName → {Arg0}", this.ToolName);
        }

        /// <summary>Initializes a new instance of the <see cref="RibbonBarToolInfo"/> class.</summary>
        /// <param name="toolName">The tool name.</param>
        /// <param name="isHostTool"><see langword="true"/> if this is a host-level tool, otherwise <see langword="false"/>.</param>
        /// <param name="useButton"><see langword="true"/> if this tool uses a button, otherwise <see langword="false"/>.</param>
        /// <param name="linkWindowTarget">The window name of the target of the link.</param>
        /// <param name="moduleFriendlyName">The friendly name of the module.</param>
        /// <param name="controlKey">The control key.</param>
        /// <param name="showAsPopUp"><see langword="true"/> if this tool should display in a pop-up, otherwise <see langword="false"/>.</param>
        public RibbonBarToolInfo(string toolName, bool isHostTool, bool useButton, string linkWindowTarget, string moduleFriendlyName, string controlKey, bool showAsPopUp)
        {
			Logger.LogDebug(">>> Entering RibbonBarToolInfo(toolName={ToolName},isHostTool={IsHostTool},useButton={UseButton},linkWindowTarget={LinkWindowTarget},moduleFriendlyName={ModuleFriendlyName},controlKey={ControlKey},showAsPopUp={ShowAsPopUp})", toolName, isHostTool, useButton, linkWindowTarget, moduleFriendlyName, controlKey, showAsPopUp);
            this.ToolName = toolName;
            this.IsHostTool = isHostTool;
			Logger.LogDebug("RibbonBarToolInfo(toolName={ToolName},isHostTool={IsHostTool},useButton={UseButton},linkWindowTarget={LinkWindowTarget},moduleFriendlyName={ModuleFriendlyName},controlKey={ControlKey},showAsPopUp={ShowAsPopUp}): this.ToolName → {Arg7}", toolName, isHostTool, useButton, linkWindowTarget, moduleFriendlyName, controlKey, showAsPopUp, this.ToolName);
			Logger.LogDebug("RibbonBarToolInfo(toolName={ToolName},isHostTool={IsHostTool},useButton={UseButton},linkWindowTarget={LinkWindowTarget},moduleFriendlyName={ModuleFriendlyName},controlKey={ControlKey},showAsPopUp={ShowAsPopUp}): this.IsHostTool → {Arg7}", toolName, isHostTool, useButton, linkWindowTarget, moduleFriendlyName, controlKey, showAsPopUp, this.IsHostTool);
			Logger.LogDebug("RibbonBarToolInfo(toolName={ToolName},isHostTool={IsHostTool},useButton={UseButton},linkWindowTarget={LinkWindowTarget},moduleFriendlyName={ModuleFriendlyName},controlKey={ControlKey},showAsPopUp={ShowAsPopUp}): this.LinkWindowTarget → {Arg7}", toolName, isHostTool, useButton, linkWindowTarget, moduleFriendlyName, controlKey, showAsPopUp, this.LinkWindowTarget);
			Logger.LogDebug("RibbonBarToolInfo(toolName={ToolName},isHostTool={IsHostTool},useButton={UseButton},linkWindowTarget={LinkWindowTarget},moduleFriendlyName={ModuleFriendlyName},controlKey={ControlKey},showAsPopUp={ShowAsPopUp}): this.ModuleFriendlyName → {Arg7}", toolName, isHostTool, useButton, linkWindowTarget, moduleFriendlyName, controlKey, showAsPopUp, this.ModuleFriendlyName);
			Logger.LogDebug("RibbonBarToolInfo(toolName={ToolName},isHostTool={IsHostTool},useButton={UseButton},linkWindowTarget={LinkWindowTarget},moduleFriendlyName={ModuleFriendlyName},controlKey={ControlKey},showAsPopUp={ShowAsPopUp}): this.ControlKey → {Arg7}", toolName, isHostTool, useButton, linkWindowTarget, moduleFriendlyName, controlKey, showAsPopUp, this.ControlKey);
            this.UseButton = useButton;
            this.LinkWindowTarget = linkWindowTarget;
            this.ModuleFriendlyName = moduleFriendlyName;
			Logger.LogDebug("RibbonBarToolInfo(toolName={ToolName},isHostTool={IsHostTool},useButton={UseButton},linkWindowTarget={LinkWindowTarget},moduleFriendlyName={ModuleFriendlyName},controlKey={ControlKey},showAsPopUp={ShowAsPopUp}): this.UseButton → {Arg7}", toolName, isHostTool, useButton, linkWindowTarget, moduleFriendlyName, controlKey, showAsPopUp, this.UseButton);
            this.ControlKey = controlKey;
            this.ShowAsPopUp = showAsPopUp;
			Logger.LogDebug("<<< Exiting RibbonBarToolInfo(toolName={ToolName},isHostTool={IsHostTool},useButton={UseButton},linkWindowTarget={LinkWindowTarget},moduleFriendlyName={ModuleFriendlyName},controlKey={ControlKey},showAsPopUp={ShowAsPopUp})", toolName, isHostTool, useButton, linkWindowTarget, moduleFriendlyName, controlKey, showAsPopUp);
			Logger.LogDebug("RibbonBarToolInfo(toolName={ToolName},isHostTool={IsHostTool},useButton={UseButton},linkWindowTarget={LinkWindowTarget},moduleFriendlyName={ModuleFriendlyName},controlKey={ControlKey},showAsPopUp={ShowAsPopUp}): this.ShowAsPopUp → {Arg7}", toolName, isHostTool, useButton, linkWindowTarget, moduleFriendlyName, controlKey, showAsPopUp, this.ShowAsPopUp);
        }

        /// <summary>Gets or sets the control key.</summary>
        public string ControlKey { get; set; }

        /// <summary>Gets or sets a value indicating whether this is a host tool.</summary>
        public bool IsHostTool { get; set; }

        /// <summary>Gets or sets the window target for the link.</summary>
        public string LinkWindowTarget { get; set; }

        /// <summary>Gets or sets the friendly name of the module.</summary>
        public string ModuleFriendlyName { get; set; }

        /// <summary>Gets or sets a value indicating whether to show the tool as a pop-up.</summary>
        public bool ShowAsPopUp { get; set; }

        /// <summary>Gets or sets the tool name.</summary>
        public string ToolName { get; set; }

        /// <summary>Gets or sets a value indicating whether to use a button.</summary>
        public bool UseButton { get; set; }
    }
}
