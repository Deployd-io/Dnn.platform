// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;

    /// <summary>A language label control.</summary>
    public class DnnLanguageLabel : CompositeControl, ILocalizable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnLanguageLabel));
        private Image flag;

        private Label label;

        /// <summary>Initializes a new instance of the <see cref="DnnLanguageLabel"/> class.</summary>
        public DnnLanguageLabel()
        {
			Logger.LogDebug(">>> Entering DnnLanguageLabel()");
            this.Localize = true;
			Logger.LogDebug("DnnLanguageLabel(): this.Localize → {Arg0}", this.Localize);
			Logger.LogDebug("<<< Exiting DnnLanguageLabel()");
        }

        /// <summary>Gets or sets the display type.</summary>
        public CultureDropDownTypes DisplayType { get; set; }

        /// <summary>Gets or sets the language.</summary>
        public string Language
        {
            get
            {
                return (string)this.ViewState["Language"];
            }

            set
            {
                this.ViewState["Language"] = value;
            }
        }

        /// <inheritdoc />
        public bool Localize { get; set; }

        /// <inheritdoc />
        public string LocalResourceFile { get; set; }

        /// <inheritdoc />
        public virtual void LocalizeStrings()
        {
		Logger.LogDebug(">>> Entering LocalizeStrings()");
		Logger.LogDebug("<<< Exiting LocalizeStrings()");
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);
            this.LocalResourceFile = Utilities.GetLocalResourceFile(this);
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <summary>
        ///   CreateChildControls overrides the Base class's method to correctly build the
        ///   control based on the configuration.
        /// </summary>
        protected override void CreateChildControls()
        {
			Logger.LogDebug(">>> Entering CreateChildControls()");
            // First clear the controls collection
            this.Controls.Clear();

            this.flag = new Image { ViewStateMode = ViewStateMode.Disabled };
			Logger.LogDebug("CreateChildControls(): flag → {Flag}", flag);
			Logger.LogDebug("CreateChildControls(): this.flag → {Arg0}", this.flag);
            this.Controls.Add(this.flag);

            this.Controls.Add(new LiteralControl("&nbsp;"));

            this.label = new Label();
			Logger.LogDebug("CreateChildControls(): label → {Label}", label);
            this.label.ViewStateMode = ViewStateMode.Disabled;
            this.Controls.Add(this.label);

            // Call base class's method
            base.CreateChildControls();
			Logger.LogDebug("<<< Exiting CreateChildControls()");
        }

        /// <summary>OnPreRender runs just before the control is rendered.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            if (string.IsNullOrEmpty(this.Language))
            {
                this.flag.ImageUrl = "~/images/Flags/none.gif";
            }
            else
            {
                this.flag.ImageUrl = $"~/images/Flags/{this.Language}.gif";
            }

            if (this.DisplayType == 0)
            {
                var portalSettings = PortalController.Instance.GetCurrentSettings();
				Logger.LogDebug("OnPreRender(e={E}): portalSettings → {PortalSettings}", e, portalSettings);
                string viewTypePersonalizationKey = "ViewType" + portalSettings.PortalId;
                string viewType = Convert.ToString(Personalization.GetProfile("LanguageDisplayMode", viewTypePersonalizationKey), CultureInfo.InvariantCulture);
                switch (viewType)
                {
                    case "NATIVE":
                        this.DisplayType = CultureDropDownTypes.NativeName;
						Logger.LogDebug("OnPreRender(e={E}): this.DisplayType → {Arg1}", e, this.DisplayType);
                        break;
                    case "ENGLISH":
                        this.DisplayType = CultureDropDownTypes.EnglishName;
                        break;
                    default:
                        this.DisplayType = CultureDropDownTypes.DisplayName;
                        break;
                }
            }

            string localeName = null;
			Logger.LogDebug("OnPreRender(e={E}): localeName → {LocaleName}", e, localeName);
            if (string.IsNullOrEmpty(this.Language))
            {
                localeName = Localization.GetString("NeutralCulture", Localization.GlobalResourceFile);
            }
            else
            {
                localeName = Localization.GetLocaleName(this.Language, this.DisplayType);
            }

            this.label.Text = localeName;
            this.flag.AlternateText = localeName;
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }
    }
}
