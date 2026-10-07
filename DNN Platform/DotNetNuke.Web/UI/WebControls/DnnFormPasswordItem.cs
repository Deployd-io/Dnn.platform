// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A password control.</summary>
    public class DnnFormPasswordItem : DnnFormItemBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnFormPasswordItem));
        private readonly IClientResourceController clientResourceController;
        private TextBox password;

        /// <summary>Initializes a new instance of the <see cref="DnnFormPasswordItem"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public DnnFormPasswordItem()
            : this(null, null, null)
        {
		Logger.LogError(">>> Entering DnnFormPasswordItem()");
        Logger.LogWarning("<<< Exiting DnnFormPasswordItem()");
        }

        /// <summary>Initializes a new instance of the <see cref="DnnFormPasswordItem"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        public DnnFormPasswordItem(IApplicationStatusInfo appStatus, IEventLogger eventLogger, IClientResourceController clientResourceController)
            : base(appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(), eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>())
        {
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
			Logger.LogDebug("DnnFormPasswordItem(appStatus={AppStatus},eventLogger={EventLogger},clientResourceController={ClientResourceController}): clientResourceController → {ClientResourceController1}", appStatus, eventLogger, clientResourceController, clientResourceController);
        Logger.LogWarning("<<< Exiting DnnFormPasswordItem(appStatus={AppStatus},eventLogger={EventLogger},clientResourceController={ClientResourceController})", appStatus, eventLogger, clientResourceController);
        }

        /// <summary>Gets or sets the CSS class for the text box.</summary>
        public string TextBoxCssClass
        {
            get => this.ViewState.GetValue("TextBoxCssClass", string.Empty);
            set => this.ViewState.SetValue("TextBoxCssClass", value, string.Empty);
        }

        /// <summary>Gets or sets the CSS class for the container.</summary>
        public string ContainerCssClass
        {
            get => this.ViewState.GetValue("ContainerCssClass", string.Empty);
            set => this.ViewState.SetValue("ContainerCssClass", value, string.Empty);
        }

        /// <inheritdoc cref="DnnFormItemBase.CreateControlInternal"/>
        protected override WebControl CreateControlInternal(Control container)
        {
			Logger.LogError(">>> Entering CreateControlInternal(container={Container})", container);
            this.password = new TextBox()
            {
				Logger.LogWarning("CreateControlInternal(container={Container}): password → {Password}", container, password);
                ID = this.ID + "_TextBox",
                TextMode = TextBoxMode.Password,
                CssClass = this.TextBoxCssClass,
                MaxLength = 39, // ensure password cannot be cut if too long
                Text = Convert.ToString(this.Value, CultureInfo.InvariantCulture), // Load from ControlState
            };
            this.password.Attributes.Add("autocomplete", "off");
            this.password.Attributes.Add("aria-label", this.DataField);
            this.password.TextChanged += this.TextChanged;

            var passwordContainer = new Panel() { ID = "passwordContainer", CssClass = this.ContainerCssClass };
			Logger.LogError("CreateControlInternal(container={Container}): passwordContainer → {PasswordContainer}", container, passwordContainer);

            // add control hierarchy to the container
            container.Controls.Add(passwordContainer);

            passwordContainer.Controls.Add(this.password);

            // return input control that can be used for validation
            return this.password;
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);
            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.jquery.tooltip.js");
            this.clientResourceController.RegisterScript("~/Resources/Shared/scripts/dnn.PasswordStrength.js");

            this.clientResourceController.RegisterStylesheet("~/Resources/Shared/stylesheets/dnn.PasswordStrength.css", FileOrder.Css.ResourceCss);

            JavaScript.RequestRegistration(this.AppStatus, this.EventLogger, this.PortalSettings, CommonJs.DnnPlugins);
			Logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogError(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            var options = new DnnPaswordStrengthOptions();
			Logger.LogError("OnPreRender(e={E}): options → {Options}", e, options);
            var optionsAsJsonString = Json.Serialize(options);
			Logger.LogError("OnPreRender(e={E}): optionsAsJsonString → {OptionsAsJsonString}", e, optionsAsJsonString);
            var script =
                $"dnn.initializePasswordStrength('.{this.TextBoxCssClass}', {optionsAsJsonString});{Environment.NewLine}";

            if (ScriptManager.GetCurrent(this.Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "PasswordStrength", script, true);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "PasswordStrength", script, true);
            }
			Logger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
        }

        private void TextChanged(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering TextChanged(sender={Sender},e={E})", sender, e);
            this.UpdateDataSource(this.Value, this.password.Text, this.DataField);
			Logger.LogWarning("<<< Exiting TextChanged(sender={Sender},e={E})", sender, e);
        }
    }
}
