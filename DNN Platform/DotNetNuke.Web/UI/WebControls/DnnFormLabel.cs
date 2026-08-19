// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>A form label control.</summary>
    public class DnnFormLabel : Panel
    {
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="DnnFormLabel"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public DnnFormLabel()
            : this(null, null)
        {
		eventLogger.LogDebug(">>> Entering DnnFormLabel()");
		eventLogger.LogDebug("<<< Exiting DnnFormLabel()");
		eventLogger.LogDebug(">>> Entering DnnFormLabel(appStatus={AppStatus},eventLogger={EventLogger})", appStatus, eventLogger);
		eventLogger.LogDebug("DnnFormLabel(appStatus={AppStatus},eventLogger={EventLogger}): eventLogger → {EventLogger1}", appStatus, eventLogger, eventLogger);
        }

        /// <summary>Initializes a new instance of the <see cref="DnnFormLabel"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        public DnnFormLabel(IApplicationStatusInfo appStatus, IEventLogger eventLogger)
        {
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
			eventLogger.LogDebug("<<< Exiting DnnFormLabel(appStatus={AppStatus},eventLogger={EventLogger})", appStatus, eventLogger);
        }

        /// <summary>Gets or sets the ID of the associated control.</summary>
        public string AssociatedControlID { get; set; }

        /// <summary>Gets or sets the path to the local resource file.</summary>
        public string LocalResourceFile { get; set; }

        /// <summary>Gets or sets the resource key of the label.</summary>
        public string ResourceKey { get; set; }

        /// <summary>Gets or sets the resource key of the tooltip.</summary>
        public string ToolTipKey { get; set; }

        /// <summary>Gets or sets a value indicating whether the field is required.</summary>
        public bool RequiredField { get; set; }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
			eventLogger.LogDebug(">>> Entering CreateChildControls()");
            string toolTipText = this.LocalizeString(this.ToolTipKey);
            if (string.IsNullOrEmpty(this.CssClass))
            {
                this.CssClass = "dnnLabel";
				eventLogger.LogDebug("CreateChildControls(): this.CssClass → {Arg0}", this.CssClass);
            }
            else if (!this.CssClass.Contains("dnnLabel"))
            {
                this.CssClass += " dnnLabel";
            }

            ////var outerPanel = new Panel();
            ////outerPanel.CssClass = "dnnLabel";
            ////Controls.Add(outerPanel);
            var outerLabel = new System.Web.UI.HtmlControls.HtmlGenericControl { TagName = "label", };
			eventLogger.LogDebug("CreateChildControls(): outerLabel → {OuterLabel}", outerLabel);
            this.Controls.Add(outerLabel);

            var label = new Label { ID = "Label", Text = this.LocalizeString(this.ResourceKey), };
			eventLogger.LogDebug("CreateChildControls(): label → {Label}", label);
            if (this.RequiredField)
            {
                label.CssClass += " dnnFormRequired";
            }

            outerLabel.Controls.Add(label);

            var link = new LinkButton { ID = "Link", CssClass = "dnnFormHelp", TabIndex = -1, };
			eventLogger.LogDebug("CreateChildControls(): link → {Link}", link);
            link.Attributes.Add("aria-label", "Help");
            this.Controls.Add(link);

            if (!string.IsNullOrEmpty(toolTipText))
            {
                ////CssClass += "dnnLabel";
                var tooltipPanel = new Panel { CssClass = "dnnTooltip", };
				eventLogger.LogDebug("CreateChildControls(): tooltipPanel → {TooltipPanel}", tooltipPanel);
                this.Controls.Add(tooltipPanel);

                var panel = new Panel { ID = "Help", CssClass = "dnnFormHelpContent dnnClear", };
				eventLogger.LogDebug("CreateChildControls(): panel → {Panel}", panel);
                tooltipPanel.Controls.Add(panel);

                var helpLabel = new Label { ID = "Text", CssClass = "dnnHelpText", Text = this.LocalizeString(this.ToolTipKey), };
				eventLogger.LogDebug("CreateChildControls(): helpLabel → {HelpLabel}", helpLabel);
                panel.Controls.Add(helpLabel);

                var pinLink = new HyperLink { CssClass = "pinHelp", };
				eventLogger.LogDebug("CreateChildControls(): pinLink → {PinLink}", pinLink);
                pinLink.Attributes.Add("href", "#");
                pinLink.Attributes.Add("aria-label", "Pin");
                panel.Controls.Add(pinLink);

                JavaScript.RegisterClientReference(this.Page, ClientAPI.ClientNamespaceReferences.dnn);
                JavaScript.RequestRegistration(this.appStatus, this.eventLogger, PortalSettings.Current, CommonJs.DnnPlugins);

                ////ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/Scripts/initTooltips.js");
            }
			eventLogger.LogDebug("<<< Exiting CreateChildControls()");
        }

        /// <summary>Gets the localized string corresponding to the <paramref name="key"/>.</summary>
        /// <param name="key">The resource key to find.</param>
        /// <returns>The localized text.</returns>
        protected string LocalizeString(string key)
        {
			eventLogger.LogDebug(">>> Entering LocalizeString(key={Key})", key);
			eventLogger.LogDebug("<<< Exiting LocalizeString(key={Key})", key);
            return Localization.GetString(key, this.LocalResourceFile);
        }
    }
}
