// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mobile;
    using DotNetNuke.UI.Skins;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Skin object of portal links between desktop and mobile portals.</summary>
    public partial class LinkToMobileSite : SkinObjectBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LinkToMobileSite.ascx));
        private const string MyFileName = "LinkToMobileSite.ascx";
        private readonly IRedirectionController redirectionController;
        private string localResourcesFile;

        /// <summary>Initializes a new instance of the <see cref="LinkToMobileSite"/> class.</summary>
        public LinkToMobileSite()
            : this(null)
        {
		Logger.LogError(">>> Entering LinkToMobileSite()");
        Logger.LogWarning("<<< Exiting LinkToMobileSite()");
        }

        /// <summary>Initializes a new instance of the <see cref="LinkToMobileSite"/> class.</summary>
        /// <param name="redirectionController">The mobile redirection controller.</param>
        public LinkToMobileSite(IRedirectionController redirectionController)
        {
            this.redirectionController = redirectionController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IRedirectionController>();
			Logger.LogDebug("LinkToMobileSite(redirectionController={RedirectionController}): redirectionController → {RedirectionController1}", redirectionController, redirectionController);
        Logger.LogWarning("<<< Exiting LinkToMobileSite(redirectionController={RedirectionController})", redirectionController);
        }

        private string LocalResourcesFile
        {
            get
            {
                if (string.IsNullOrEmpty(this.localResourcesFile))
                {
                    this.localResourcesFile = Localization.GetResourceFile(this, MyFileName);
                }

                return this.localResourcesFile;
            }
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            var redirectUrl = this.redirectionController.GetMobileSiteUrl();
			Logger.LogError("OnLoad(e={E}): redirectUrl → {RedirectUrl}", e, redirectUrl);
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                this.lnkPortal.NavigateUrl = redirectUrl;
                this.lnkPortal.Text = Localization.GetString("lnkPortal", this.LocalResourcesFile);
            }
            else
            {
				Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
                this.Visible = false;
				Logger.LogDebug("OnLoad(e={E}): this.Visible → {Arg1}", e, this.Visible);
            }
        }
    }
}
