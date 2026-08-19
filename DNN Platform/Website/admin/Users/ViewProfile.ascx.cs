// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Users
{
    using System;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Exceptions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A control which displays a user's profile.</summary>
    public partial class ViewProfile : UserModuleBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ViewProfile.ascx));
        private readonly ICryptographyProvider cryptographyProvider;

        /// <summary>Initializes a new instance of the <see cref="ViewProfile"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Use overload with ICryptographyProvider. Scheduled for removal in v12.0.0.")]
        public ViewProfile()
        {
		Logger.LogDebug(">>> Entering ViewProfile()");
		Logger.LogDebug("<<< Exiting ViewProfile()");
        }

        /// <summary>Initializes a new instance of the <see cref="ViewProfile"/> class.</summary>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public ViewProfile(ICryptographyProvider cryptographyProvider)
            : this(cryptographyProvider, null, null)
        {
		Logger.LogDebug(">>> Entering ViewProfile(cryptographyProvider={CryptographyProvider})", cryptographyProvider);
        }
		Logger.LogDebug("<<< Exiting ViewProfile(cryptographyProvider={CryptographyProvider})", cryptographyProvider);

        /// <summary>Initializes a new instance of the <see cref="ViewProfile"/> class.</summary>
        /// <param name="cryptographyProvider">The cryptography provider.</param>
        /// <param name="listController">The list controller.</param>
        /// <param name="hostSettings">The host settings.</param>
        public ViewProfile(ICryptographyProvider cryptographyProvider, ListController listController, IHostSettings hostSettings)
            : base(listController, hostSettings)
        {
            this.cryptographyProvider = cryptographyProvider ?? this.DependencyProvider.GetRequiredService<ICryptographyProvider>();
			Logger.LogDebug("ViewProfile(cryptographyProvider={CryptographyProvider},listController={ListController},hostSettings={HostSettings}): cryptographyProvider → {CryptographyProvider1}", cryptographyProvider, listController, hostSettings, cryptographyProvider);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.UserId = Null.NullInteger;
			Logger.LogDebug("OnInit(e={E}): this.UserId → {Arg1}", e, this.UserId);
            if (this.Context.Request.QueryString["userticket"] != null)
            {
                this.UserId = int.Parse(UrlUtils.DecryptParameter(this.cryptographyProvider, this.Context.Request.QueryString["userticket"]));
            }

            this.ctlProfile.ID = "Profile";
            this.ctlProfile.UserId = this.UserId;
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            try
            {
                if (this.ctlProfile.UserProfile == null)
                {
                    this.lblNoProperties.Visible = true;
                    return;
                }

                this.ctlProfile.DataBind();
                if (!this.ctlProfile.UserProfile.ProfileProperties.Any(profProperty => profProperty.Visible))
                {
                    this.lblNoProperties.Visible = true;
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.getMessage());
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }
    }
}
