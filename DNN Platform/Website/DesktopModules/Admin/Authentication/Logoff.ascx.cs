// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Authentication
{
    using System;
    using System.IO;
    using System.Threading;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    /// <summary>The Logoff UserModuleBase is used to log off a registered user.</summary>
    public partial class Logoff(ListController listController, IHostSettings hostSettings) : UserModuleBase(listController, hostSettings)
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Logoff.ascx));
        /// <summary>Initializes a new instance of the <see cref="Logoff"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public Logoff()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering Logoff()");
        }
		Logger.LogDebug("<<< Exiting Logoff()");

        /// <summary>Page_Load runs when the control is loaded.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            try
            {
                // Get the Authentication System associated with the current User
                var authSystem = AuthenticationController.GetAuthenticationType();
				Logger.LogDebug("OnLoad(e={E}): authSystem → {AuthSystem}", e, authSystem);

                if (authSystem != null && !string.IsNullOrEmpty(authSystem.LogoffControlSrc))
                {
                    var authLogoffControl = (AuthenticationLogoffBase)this.LoadControl("~/" + authSystem.LogoffControlSrc);
					Logger.LogDebug("OnLoad(e={E}): authLogoffControl → {AuthLogoffControl}", e, authLogoffControl);

                    // set the control ID to the resource file name ( ie. controlname.ascx = controlname )
                    // this is necessary for the Localization in PageBase
                    authLogoffControl.AuthenticationType = authSystem.AuthenticationType;
					Logger.LogDebug("OnLoad(e={E}): authLogoffControl.AuthenticationType → {Arg1}", e, authLogoffControl.AuthenticationType);
                    authLogoffControl.ID = Path.GetFileNameWithoutExtension(authSystem.LogoffControlSrc) + "_" + authSystem.AuthenticationType;
                    authLogoffControl.LocalResourceFile = authLogoffControl.TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" +
                                                          Path.GetFileNameWithoutExtension(authSystem.LogoffControlSrc);
														Logger.LogDebug("OnLoad(e={E}): authLogoffControl.LocalResourceFile → {Arg1}", e, authLogoffControl.LocalResourceFile);
														Logger.LogDebug("OnLoad(e={E}): authLogoffControl.ModuleConfiguration → {Arg1}", e, authLogoffControl.ModuleConfiguration);
                    authLogoffControl.ModuleConfiguration = this.ModuleConfiguration;

                    authLogoffControl.LogOff += this.UserLogOff;
                    authLogoffControl.Redirect += this.UserRedirect;

                    // Add Login Control to Control
                    this.pnlLogoffContainer.Controls.Add(authLogoffControl);
                }
                else
                {
                    // The current auth system has no custom logoff control so LogOff
                    this.DoLogoff();
                    this.Redirect();
                }
            }
            catch (ThreadAbortException)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, ThreadAbortException.getMessage());
                // Do nothing Response.redirect
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.getMessage());
                Exceptions.ProcessPageLoadException(exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        protected void UserLogOff(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering UserLogOff(sender={Sender},e={E})", sender, e);
            this.DoLogoff();
			Logger.LogDebug("<<< Exiting UserLogOff(sender={Sender},e={E})", sender, e);
        }

        protected void UserRedirect(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering UserRedirect(sender={Sender},e={E})", sender, e);
            this.Redirect();
			Logger.LogDebug("<<< Exiting UserRedirect(sender={Sender},e={E})", sender, e);
        }

        private void Redirect()
        {
			Logger.LogDebug(">>> Entering Redirect()");
            // Redirect browser back to portal
            this.Response.Redirect(AuthenticationController.GetLogoffRedirectURL(this.PortalSettings, this.Request), true);
			Logger.LogDebug("<<< Exiting Redirect()");
        }

        private void DoLogoff()
        {
			Logger.LogDebug(">>> Entering DoLogoff()");
            try
            {
                // Remove user from cache
                if (this.User != null)
                {
                    DataCache.ClearUserCache(this.PortalSettings.PortalId, this.Context.User.Identity.Name);
                }

                var objPortalSecurity = PortalSecurity.Instance;
                objPortalSecurity.SignOut();
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in DoLogoff(): {Arg0}", exc.getMessage());
                Exceptions.ProcessPageLoadException(exc);
            }
			Logger.LogDebug("<<< Exiting DoLogoff()");
        }
    }
}
