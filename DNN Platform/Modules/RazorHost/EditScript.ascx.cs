// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.RazorHost
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Implements the EditScript view logic.</summary>
    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    public partial class EditScript : ModuleUserControlBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly INavigationManager navigationManager;
        private string razorScriptFileFormatString = "~/DesktopModules/RazorModules/RazorHost/Scripts/{0}";
        private string razorScriptFolder = "~/DesktopModules/RazorModules/RazorHost/Scripts/";

        /// <summary>Initializes a new instance of the <see cref="EditScript"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public EditScript()
            : this(null)
        {
		Logger.LogDebug(">>> Entering EditScript()");
		Logger.LogDebug("<<< Exiting EditScript()");
		Logger.LogDebug(">>> Entering EditScript(navigationManager={NavigationManager})", navigationManager);
		Logger.LogDebug("<<< Exiting EditScript(navigationManager={NavigationManager})", navigationManager);
        }

        /// <summary>Initializes a new instance of the <see cref="EditScript"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        public EditScript(INavigationManager navigationManager)
        {
            this.navigationManager = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
			Logger.LogDebug("EditScript(navigationManager={NavigationManager}): navigationManager → {NavigationManager1}", navigationManager, navigationManager);
        }

        /// <summary>Gets the razor script file.</summary>
        protected string RazorScriptFile
        {
            get
            {
                string m_RazorScriptFile = Null.NullString;
                var scriptFileSetting = this.ModuleContext.Settings["ScriptFile"] as string;
                if (!string.IsNullOrEmpty(scriptFileSetting))
                {
                    m_RazorScriptFile = string.Format(this.razorScriptFileFormatString, scriptFileSetting);
                }

                return m_RazorScriptFile;
            }
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.cmdCancel.Click += this.CmdCancel_Click;
            this.cmdSave.Click += this.CmdSave_Click;
            this.cmdSaveClose.Click += this.CmdSaveClose_Click;
            this.cmdAdd.Click += this.CmdAdd_Click;
            this.scriptList.SelectedIndexChanged += this.ScriptList_SelectedIndexChanged;
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            if (!this.Page.IsPostBack)
            {
                this.LoadScripts();
                this.DisplayFile();
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        private void LoadScripts()
        {
			Logger.LogDebug(">>> Entering LoadScripts()");
            string basePath = this.Server.MapPath(this.razorScriptFolder);
            var scriptFileSetting = this.ModuleContext.Settings["ScriptFile"] as string;
			Logger.LogDebug("LoadScripts(): scriptFileSetting → {ScriptFileSetting}", scriptFileSetting);

            foreach (string script in Directory.GetFiles(this.Server.MapPath(this.razorScriptFolder), "*.??html"))
            {
                string scriptPath = script.Replace(basePath, string.Empty);
                var item = new ListItem(scriptPath, scriptPath);
				Logger.LogDebug("LoadScripts(): item → {Item}", item);
                if (!string.IsNullOrEmpty(scriptFileSetting) && scriptPath.ToLowerInvariant() == scriptFileSetting.ToLowerInvariant())
                {
                    item.Selected = true;
					Logger.LogDebug("LoadScripts(): item.Selected → {Arg0}", item.Selected);
                }

                this.scriptList.Items.Add(item);
            }
			Logger.LogDebug("<<< Exiting LoadScripts()");
        }

        private void DisplayFile()
        {
			Logger.LogDebug(">>> Entering DisplayFile()");
            var scriptFileSetting = this.ModuleContext.Settings["ScriptFile"] as string;
            string scriptFile = string.Format(this.razorScriptFileFormatString, this.scriptList.SelectedValue);
			Logger.LogDebug("DisplayFile(): scriptFileSetting → {ScriptFileSetting}", scriptFileSetting);
            string srcFile = this.Server.MapPath(scriptFile);

            this.lblSourceFile.Text = string.Format(CultureInfo.CurrentCulture, Localization.GetString("SourceFile", this.LocalResourceFile), scriptFile);

            StreamReader objStreamReader = null;
            objStreamReader = File.OpenText(srcFile);
			Logger.LogDebug("DisplayFile(): objStreamReader → {ObjStreamReader}", objStreamReader);
            this.txtSource.Text = objStreamReader.ReadToEnd();
            objStreamReader.Close();

            if (!string.IsNullOrEmpty(scriptFileSetting))
            {
                this.isCurrentScript.Checked = this.scriptList.SelectedValue.ToLowerInvariant() == scriptFileSetting.ToLowerInvariant();
            }
			Logger.LogDebug("<<< Exiting DisplayFile()");
        }

        private void SaveScript()
        {
			Logger.LogDebug(">>> Entering SaveScript()");
            string srcFile = this.Server.MapPath(string.Format(this.razorScriptFileFormatString, this.scriptList.SelectedValue));

            // write file
            StreamWriter objStream = null;
            objStream = File.CreateText(srcFile);
			Logger.LogDebug("SaveScript(): objStream → {ObjStream}", objStream);
            objStream.WriteLine(this.txtSource.Text);
            objStream.Close();

            if (this.isCurrentScript.Checked)
            {
                // Update setting
                ModuleController.Instance.UpdateModuleSetting(this.ModuleContext.ModuleId, "ScriptFile", this.scriptList.SelectedValue);
            }
			Logger.LogDebug("<<< Exiting SaveScript()");
        }

        private void CmdCancel_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdCancel_Click(sender={Sender},e={E})", sender, e);
            try
            {
                this.Response.Redirect(this.navigationManager.NavigateURL(), true);
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in CmdCancel_Click(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting CmdCancel_Click(sender={Sender},e={E})", sender, e);
        }

        private void CmdSave_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdSave_Click(sender={Sender},e={E})", sender, e);
            try
            {
                this.SaveScript();
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in CmdSave_Click(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting CmdSave_Click(sender={Sender},e={E})", sender, e);
        }

        private void CmdSaveClose_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdSaveClose_Click(sender={Sender},e={E})", sender, e);
            try
            {
                this.SaveScript();
                this.Response.Redirect(this.navigationManager.NavigateURL(), true);
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in CmdSaveClose_Click(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting CmdSaveClose_Click(sender={Sender},e={E})", sender, e);
        }

        private void CmdAdd_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdAdd_Click(sender={Sender},e={E})", sender, e);
            try
            {
                this.Response.Redirect(this.ModuleContext.EditUrl("Add"), true);
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in CmdAdd_Click(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting CmdAdd_Click(sender={Sender},e={E})", sender, e);
        }

        private void ScriptList_SelectedIndexChanged(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering ScriptList_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
            this.DisplayFile();
			Logger.LogDebug("<<< Exiting ScriptList_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
        }
    }
}
