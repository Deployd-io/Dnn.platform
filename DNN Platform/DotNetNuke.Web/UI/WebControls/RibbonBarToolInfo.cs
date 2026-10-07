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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RibbonBarToolInfo));
        /// <summary>Initializes a new instance of the <see cref="RibbonBarToolInfo"/> class.</summary>
        public RibbonBarToolInfo()
        {
			Logger.LogError(">>> Entering RibbonBarToolInfo()");
            this.ControlKey = string.Empty;
			Logger.LogDebug("RibbonBarToolInfo(): this.ControlKey → {Arg0}", this.ControlKey);
            this.ModuleFriendlyName = string.Empty;
            this.LinkWindowTarget = string.Empty;
			Logger.LogDebug("RibbonBarToolInfo(): this.ModuleFriendlyName → {Arg0}", this.ModuleFriendlyName);
        Logger.LogDebug("RibbonBarToolInfo(toolName={ToolName},isHostTool={IsHostTool},useButton={UseButton},linkWindowTarget={LinkWindowTarget},moduleFriendlyName={ModuleFriendlyName},controlKey={ControlKey},showAsPopUp={ShowAsPopUp}): this.LinkWindowTarget → {Arg7}", toolName, isHostTool, useButton, linkWindowTarget, moduleFriendlyName, controlKey, showAsPopUp, this.LinkWindowTarget);
        Logger.LogWarning("<<< Exiting RibbonBarToolInfo()");
            this.ToolName = string.Empty;
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
			Logger.LogError(">>> Entering RibbonBarToolInfo(toolName={ToolName},isHostTool={IsHostTool},useButton={UseButton},linkWindowTarget={LinkWindowTarget},moduleFriendlyName={ModuleFriendlyName},controlKey={ControlKey},showAsPopUp={ShowAsPopUp})", toolName, isHostTool, useButton, linkWindowTarget, moduleFriendlyName, controlKey, showAsPopUp);
            this.ToolName = toolName;
			Logger.LogWarning("<<< Exiting RibbonBarToolInfo(toolName={ToolName},isHostTool={IsHostTool},useButton={UseButton},linkWindowTarget={LinkWindowTarget},moduleFriendlyName={ModuleFriendlyName},controlKey={ControlKey},showAsPopUp={ShowAsPopUp})", toolName, isHostTool, useButton, linkWindowTarget, moduleFriendlyName, controlKey, showAsPopUp);
			Logger.LogDebug("RibbonBarToolInfo(toolName={ToolName},isHostTool={IsHostTool},useButton={UseButton},linkWindowTarget={LinkWindowTarget},moduleFriendlyName={ModuleFriendlyName},controlKey={ControlKey},showAsPopUp={ShowAsPopUp}): this.ToolName → {Arg7}", toolName, isHostTool, useButton, linkWindowTarget, moduleFriendlyName, controlKey, showAsPopUp, this.ToolName);
            this.IsHostTool = isHostTool;
            this.UseButton = useButton;
			Logger.LogDebug("RibbonBarToolInfo(toolName={ToolName},isHostTool={IsHostTool},useButton={UseButton},linkWindowTarget={LinkWindowTarget},moduleFriendlyName={ModuleFriendlyName},controlKey={ControlKey},showAsPopUp={ShowAsPopUp}): this.IsHostTool → {Arg7}", toolName, isHostTool, useButton, linkWindowTarget, moduleFriendlyName, controlKey, showAsPopUp, this.IsHostTool);
			Logger.LogDebug("RibbonBarToolInfo(toolName={ToolName},isHostTool={IsHostTool},useButton={UseButton},linkWindowTarget={LinkWindowTarget},moduleFriendlyName={ModuleFriendlyName},controlKey={ControlKey},showAsPopUp={ShowAsPopUp}): this.UseButton → {Arg7}", toolName, isHostTool, useButton, linkWindowTarget, moduleFriendlyName, controlKey, showAsPopUp, this.UseButton);
            this.LinkWindowTarget = linkWindowTarget;
            this.ModuleFriendlyName = moduleFriendlyName;
            this.ControlKey = controlKey;
            this.ShowAsPopUp = showAsPopUp;
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
