// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.RazorHost
{
    using System;
    using System.IO;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Implements the AddScript view logic.</summary>
    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    public partial class AddScript : ModuleUserControlBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly INavigationManager navigationManager;
        private string razorScriptFileFormatString = "~/DesktopModules/RazorModules/RazorHost/Scripts/{0}";

        /// <summary>Initializes a new instance of the <see cref="AddScript"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public AddScript()
            : this(null)
        {
		Logger.LogDebug(">>> Entering AddScript()");
		Logger.LogDebug("<<< Exiting AddScript()");
		Logger.LogDebug(">>> Entering AddScript(navigationManager={NavigationManager})", navigationManager);
		Logger.LogDebug("<<< Exiting AddScript(navigationManager={NavigationManager})", navigationManager);
        }

        /// <summary>Initializes a new instance of the <see cref="AddScript"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        public AddScript(INavigationManager navigationManager)
        {
            this.navigationManager = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
			Logger.LogDebug("AddScript(navigationManager={NavigationManager}): navigationManager → {NavigationManager1}", navigationManager, navigationManager);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.cmdCancel.Click += this.CmdCancel_Click;
            this.cmdAdd.Click += this.CmdAdd_Click;
            this.scriptFileType.SelectedIndexChanged += this.ScriptFileType_SelectedIndexChanged;
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.DisplayExtension();
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        /// <summary>Cancel button click event handler.</summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected void CmdCancel_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdCancel_Click(sender={Sender},e={E})", sender, e);
            try
            {
                this.Response.Redirect(this.ModuleContext.EditUrl("Edit"), true);
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in CmdCancel_Click(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting CmdCancel_Click(sender={Sender},e={E})", sender, e);
        }

        /// <summary>Add button event handler.</summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected void CmdAdd_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdAdd_Click(sender={Sender},e={E})", sender, e);
            try
            {
                if (!this.ModuleContext.PortalSettings.UserInfo.IsSuperUser)
                {
                    this.Response.Redirect(this.navigationManager.NavigateURL("Access Denied"), true);
                }

                if (this.Page.IsValid)
                {
                    string scriptFileName = "_" + Path.GetFileNameWithoutExtension(this.fileName.Text) + "." + this.scriptFileType.SelectedValue.ToLowerInvariant();

                    string srcFile = this.Server.MapPath(string.Format(this.razorScriptFileFormatString, scriptFileName));

                    // write file
                    StreamWriter objStream = null;
                    objStream = File.CreateText(srcFile);
					Logger.LogDebug("CmdAdd_Click(sender={Sender},e={E}): objStream → {ObjStream}", sender, e, objStream);
                    objStream.WriteLine(Localization.GetString("NewScript", this.LocalResourceFile));
                    objStream.Close();

                    this.Response.Redirect(this.ModuleContext.EditUrl("Edit"), true);
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in CmdAdd_Click(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting CmdAdd_Click(sender={Sender},e={E})", sender, e);
        }

        private void DisplayExtension()
        {
			Logger.LogDebug(">>> Entering DisplayExtension()");
            this.fileExtension.Text = "." + this.scriptFileType.SelectedValue.ToLowerInvariant();
			Logger.LogDebug("<<< Exiting DisplayExtension()");
        }

        private void ScriptFileType_SelectedIndexChanged(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering ScriptFileType_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
            this.DisplayExtension();
			Logger.LogDebug("<<< Exiting ScriptFileType_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
        }
    }
}
