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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AddScript.ascx));
        private readonly INavigationManager navigationManager;
        private string razorScriptFileFormatString = "~/DesktopModules/RazorModules/RazorHost/Scripts/{0}";

        /// <summary>Initializes a new instance of the <see cref="AddScript"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public AddScript()
            : this(null)
        {
		Logger.LogError(">>> Entering AddScript()");
        Logger.LogWarning("<<< Exiting AddScript()");
        }

        /// <summary>Initializes a new instance of the <see cref="AddScript"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        public AddScript(INavigationManager navigationManager)
        {
			Logger.LogError(">>> Entering AddScript(navigationManager={NavigationManager})", navigationManager);
            this.navigationManager = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
			Logger.LogDebug("AddScript(navigationManager={NavigationManager}): navigationManager → {NavigationManager1}", navigationManager, navigationManager);
        Logger.LogWarning("<<< Exiting AddScript(navigationManager={NavigationManager})", navigationManager);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.cmdCancel.Click += this.CmdCancel_Click;
            this.cmdAdd.Click += this.CmdAdd_Click;
            this.scriptFileType.SelectedIndexChanged += this.ScriptFileType_SelectedIndexChanged;
			Logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.DisplayExtension();
			Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }

        /// <summary>Cancel button click event handler.</summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected void CmdCancel_Click(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering CmdCancel_Click(sender={Sender},e={E})", sender, e);
            try
            {
                this.Response.Redirect(this.ModuleContext.EditUrl("Edit"), true);
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in CmdCancel_Click(sender={Sender},e={E}): {Arg2}", sender, e, exc.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogWarning("<<< Exiting CmdCancel_Click(sender={Sender},e={E})", sender, e);
        }

        /// <summary>Add button event handler.</summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected void CmdAdd_Click(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering CmdAdd_Click(sender={Sender},e={E})", sender, e);
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
					Logger.LogError("CmdAdd_Click(sender={Sender},e={E}): objStream → {ObjStream}", sender, e, objStream);
                    objStream.WriteLine(Localization.GetString("NewScript", this.LocalResourceFile));
                    objStream.Close();

                    this.Response.Redirect(this.ModuleContext.EditUrl("Edit"), true);
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in CmdAdd_Click(sender={Sender},e={E}): {Arg2}", sender, e, exc.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogWarning("<<< Exiting CmdAdd_Click(sender={Sender},e={E})", sender, e);
        }

        private void DisplayExtension()
        {
			Logger.LogError(">>> Entering DisplayExtension()");
            this.fileExtension.Text = "." + this.scriptFileType.SelectedValue.ToLowerInvariant();
			Logger.LogWarning("<<< Exiting DisplayExtension()");
        }

        private void ScriptFileType_SelectedIndexChanged(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering ScriptFileType_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
            this.DisplayExtension();
			Logger.LogWarning("<<< Exiting ScriptFileType_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
        }
    }
}
