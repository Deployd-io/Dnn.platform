// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Admin.Security
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    /// <summary>The MemberServices UserModuleBase is used to manage a User's services.</summary>
    public partial class MemberServices(ListController listController, IHostSettings hostSettings)
        : UserModuleBase(listController, hostSettings)
    {
		Logger.LogDebug(">>> Entering MemberServices(listController={ListController},hostSettings={HostSettings})", listController, hostSettings);
        /// <summary>Initializes a new instance of the <see cref="MemberServices"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public MemberServices()
            : this(null, null)
        {
		Logger.LogError("Exception in MemberServices(listController={ListController},hostSettings={HostSettings}): {Arg2}", listController, hostSettings, ex.Message);
		Logger.LogDebug("<<< Exiting MemberServices(listController={ListController},hostSettings={HostSettings})", listController, hostSettings);
        }

        public delegate void SubscriptionUpdatedEventHandler(object sender, SubscriptionUpdatedEventArgs e);

        public event SubscriptionUpdatedEventHandler SubscriptionUpdated;

        /// <summary>DataBind binds the data to the controls.</summary>
        public override void DataBind()
        {
			Logger.LogDebug(">>> Entering DataBind()");
            if (this.Request.IsAuthenticated)
            {
                Localization.LocalizeDataGrid(ref this.grdServices, this.LocalResourceFile);
                this.grdServices.DataSource = RoleController.Instance.GetUserRoles(this.UserInfo, false);
                this.grdServices.DataBind();

                // if no service available then hide options
                this.ServicesRow.Visible = this.grdServices.Items.Count > 0;
            }
			Logger.LogDebug("<<< Exiting DataBind()");
        }

        /// <summary>Raises the SubscriptionUpdated Event.</summary>
        /// <param name="e">The event arguments.</param>
        public void OnSubscriptionUpdated(SubscriptionUpdatedEventArgs e)
        {
			Logger.LogDebug(">>> Entering OnSubscriptionUpdated(e={E})", e);
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            if (this.SubscriptionUpdated != null)
            {
                this.SubscriptionUpdated(this, e);
            }
			Logger.LogDebug("<<< Exiting OnSubscriptionUpdated(e={E})", e);
        }

        /// <summary>FormatExpiryDate formats the expiry date and filters out null-values.</summary>
        /// <param name="expiryDate">The date to format.</param>
        /// <returns>The correctly formatted date.</returns>
        protected string FormatExpiryDate(DateTime expiryDate)
        {
			Logger.LogDebug(">>> Entering FormatExpiryDate(expiryDate={ExpiryDate})", expiryDate);
            string formatExpiryDate = Null.NullString;
            try
			Logger.LogDebug("FormatExpiryDate(expiryDate={ExpiryDate}): formatExpiryDate → {FormatExpiryDate}", expiryDate, formatExpiryDate);
            {
                if (!Null.IsNull(expiryDate))
                {
                    if (expiryDate > DateTime.Today)
                    {
                        formatExpiryDate = expiryDate.ToShortDateString();
                    }
                    else
                    {
                        formatExpiryDate = Localization.GetString("Expired", this.LocalResourceFile);
                    }
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in MemberServices(listController={ListController},hostSettings={HostSettings}): {Arg2}", listController, hostSettings, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
				Logger.LogError("Exception in FormatExpiryDate(expiryDate={ExpiryDate}): {Arg1}", expiryDate, ex.Message);
            }

			Logger.LogDebug("<<< Exiting FormatExpiryDate(expiryDate={ExpiryDate})", expiryDate);
            return formatExpiryDate;
        }

        /// <summary>FormatPrice formats the Fee amount and filters out null-values.</summary>
        /// <param name="price">The price to format.</param>
        /// <param name="period">Period of price.</param>
        /// <param name="frequency">Frenquency of price.</param>
        /// <returns>The correctly formatted price.</returns>
        protected string FormatPrice(float price, int period, string frequency)
        {
			Logger.LogDebug(">>> Entering FormatPrice(price={Price},period={Period},frequency={Frequency})", price, period, frequency);
            string formatPrice = Null.NullString;
            try
			Logger.LogDebug("FormatPrice(price={Price},period={Period},frequency={Frequency}): formatPrice → {FormatPrice}", price, period, frequency, formatPrice);
            {
                switch (frequency)
                {
                    case "N":
                    case "":
                        formatPrice = Localization.GetString("NoFee", this.LocalResourceFile);
                        break;
                    case "O":
                        formatPrice = this.FormatPrice(price);
                        break;
                    default:
                        formatPrice = string.Format(CultureInfo.CurrentCulture, Localization.GetString("Fee", this.LocalResourceFile), this.FormatPrice(price), period, Localization.GetString("Frequency_" + frequency, this.LocalResourceFile));
                        break;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
				Logger.LogError("Exception in FormatPrice(price={Price},period={Period},frequency={Frequency}): {Arg3}", price, period, frequency, ex.Message);
            }

			Logger.LogDebug("<<< Exiting FormatPrice(price={Price},period={Period},frequency={Frequency})", price, period, frequency);
			Logger.LogDebug("<<< Exiting FormatPrice(price={Price})", price);
            return formatPrice;
        }

        /// <summary>FormatTrial formats the Trial Fee amount and filters out null-values.</summary>
        /// <param name="price">The price to format.</param>
        /// <param name="period">Period of price.</param>
        /// <param name="frequency">Frenquency of price.</param>
        /// <returns>The correctly formatted price.</returns>
        protected string FormatTrial(float price, int period, string frequency)
        {
			Logger.LogDebug(">>> Entering FormatTrial(price={Price},period={Period},frequency={Frequency})", price, period, frequency);
            string formatTrial = Null.NullString;
            try
			Logger.LogDebug("FormatTrial(price={Price},period={Period},frequency={Frequency}): formatTrial → {FormatTrial}", price, period, frequency, formatTrial);
            {
                switch (frequency)
                {
                    case "N":
                    case "":
                        formatTrial = Localization.GetString("NoFee", this.LocalResourceFile);
                        break;
                    case "O":
                        formatTrial = this.FormatPrice(price);
                        break;
                    default:
                        formatTrial = string.Format(
                            Localization.GetString("TrialFee", this.LocalResourceFile),
                            this.FormatPrice(price),
                            period,
                            Localization.GetString("Frequency_" + frequency, this.LocalResourceFile));
                        break;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
				Logger.LogError("Exception in FormatTrial(price={Price},period={Period},frequency={Frequency}): {Arg3}", price, period, frequency, ex.Message);
            }

			Logger.LogDebug("<<< Exiting FormatTrial(price={Price},period={Period},frequency={Frequency})", price, period, frequency);
            return formatTrial;
        }

        /// <summary>FormatURL correctly formats a URL.</summary>
        /// <returns>The correctly formatted url.</returns>
        protected string FormatURL()
        {
			Logger.LogDebug(">>> Entering FormatURL()");
            string formatURL = Null.NullString;
            try
            {
                string serverPath = this.Request.ApplicationPath;
                if (!serverPath.EndsWith("/", StringComparison.Ordinal))
                {
                    serverPath += "/";
                }

                formatURL = serverPath + "Register.aspx?tabid=" + this.TabId;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
				Logger.LogError("Exception in FormatURL(): {Arg0}", ex.Message);
            }

			Logger.LogDebug("<<< Exiting FormatURL()");
            return formatURL;
        }

        /// <summary>ServiceText gets the Service Text (Cancel or Subscribe).</summary>
        /// <param name="subscribed">The service state.</param>
        /// <param name="expiryDate">The service expiry date.</param>
        /// <returns>The correctly formatted text.</returns>
        protected string ServiceText(bool subscribed, DateTime expiryDate)
        {
			Logger.LogDebug(">>> Entering ServiceText(subscribed={Subscribed},expiryDate={ExpiryDate})", subscribed, expiryDate);
            string serviceText = Null.NullString;
            try
			Logger.LogDebug("ServiceText(subscribed={Subscribed},expiryDate={ExpiryDate}): serviceText → {ServiceText}", subscribed, expiryDate, serviceText);
            {
                if (!subscribed)
                {
                    serviceText = Localization.GetString("Subscribe", this.LocalResourceFile);
                }
                else
                {
                    serviceText = Localization.GetString("Unsubscribe", this.LocalResourceFile);
                    if (!Null.IsNull(expiryDate))
                    {
                        if (expiryDate < DateTime.Today)
                        {
                            serviceText = Localization.GetString("Renew", this.LocalResourceFile);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
				Logger.LogError("Exception in ServiceText(subscribed={Subscribed},expiryDate={ExpiryDate}): {Arg2}", subscribed, expiryDate, ex.Message);
            }

			Logger.LogDebug("<<< Exiting ServiceText(subscribed={Subscribed},expiryDate={ExpiryDate})", subscribed, expiryDate);
            return serviceText;
        }

        protected bool ShowSubscribe(int roleID)
        {
            var objRole = RoleController.Instance.GetRole(this.PortalSettings.PortalId, r => r.RoleID == roleID);
            return objRole.IsPublic && objRole.ServiceFee == 0.0;
        }

        protected bool ShowTrial(int roleID)
        {
            bool showTrial = Null.NullBoolean;
            RoleInfo objRole = RoleController.Instance.GetRole(this.PortalSettings.PortalId, r => r.RoleID == roleID);
            if (string.IsNullOrEmpty(objRole.TrialFrequency) || objRole.TrialFrequency == "N" || (objRole.IsPublic && objRole.ServiceFee == 0.0))
            {
                showTrial = Null.NullBoolean;
            }
            else if (objRole.IsPublic && objRole.TrialFee == 0.0)
            {
                // Use Trial?
                UserRoleInfo objUserRole = RoleController.Instance.GetUserRole(this.PortalId, this.UserInfo.UserID, roleID);
                if ((objUserRole == null) || (!objUserRole.IsTrialUsed))
                {
                    showTrial = true;
                }
            }

            return showTrial;
        }

        /// <summary>Page_Load runs when the control is loaded.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.cmdRSVP.Click += this.CmdRSVP_Click;
            this.grdServices.ItemCommand += this.grdServices_ItemCommand;
            this.lblRSVP.Text = string.Empty;
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        protected void grdServices_ItemCommand(object source, DataGridCommandEventArgs e)
        {
			Logger.LogDebug(">>> Entering grdServices_ItemCommand(source={Source},e={E})", source, e);
            string commandName = e.CommandName;
            int roleID = Convert.ToInt32(e.CommandArgument);
            if (commandName == Localization.GetString("Subscribe", this.LocalResourceFile) || commandName == Localization.GetString("Renew", this.LocalResourceFile))
			Logger.LogDebug("grdServices_ItemCommand(source={Source},e={E}): roleID → {RoleID}", source, e, roleID);
            {
                // Subscribe
                this.Subscribe(roleID, false);
            }
            else if (commandName == Localization.GetString("Unsubscribe", this.LocalResourceFile))
            {
                // Unsubscribe
                this.Subscribe(roleID, true);
            }
            else if (commandName == "UseTrial")
            {
                // Use Trial
                this.UseTrial(roleID);
            }

            // Rebind Grid
            this.DataBind();
			Logger.LogDebug("<<< Exiting grdServices_ItemCommand(source={Source},e={E})", source, e);
        }

        /// <summary>FormatPrice formats the Fee amount and filters out null-values.</summary>
        /// <param name="price">The price to format.</param>
        /// <returns>The correctly formatted price.</returns>
        private string FormatPrice(float price)
        {
            string formatPrice = Null.NullString;
            try
            {
                if (price != Null.NullSingle)
                {
                    formatPrice = price.ToString("##0.00");
                }
                else
                {
                    formatPrice = string.Empty;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

            return formatPrice;
        }

        private void Subscribe(int roleID, bool cancel)
        {
            RoleInfo objRole = RoleController.Instance.GetRole(this.PortalSettings.PortalId, r => r.RoleID == roleID);

            if (objRole.IsPublic && objRole.ServiceFee == 0.0)
            {
                RoleController.Instance.UpdateUserRole(this.PortalId, this.UserInfo.UserID, roleID, RoleStatus.Approved, false, cancel);

                // Raise SubscriptionUpdated Event
                this.OnSubscriptionUpdated(new SubscriptionUpdatedEventArgs(cancel, objRole.RoleName));
            }
            else
            {
                if (!cancel)
                {
                    this.Response.Redirect("~/admin/Sales/PayPalSubscription.aspx?tabid=" + this.TabId + "&RoleID=" + roleID, true);
                }
                else
                {
                    this.Response.Redirect("~/admin/Sales/PayPalSubscription.aspx?tabid=" + this.TabId + "&RoleID=" + roleID + "&cancel=1", true);
                }
            }
        }

        private void UseTrial(int roleID)
        {
            RoleInfo objRole = RoleController.Instance.GetRole(this.PortalSettings.PortalId, r => r.RoleID == roleID);

            if (objRole.IsPublic && objRole.TrialFee == 0.0)
            {
                RoleController.Instance.UpdateUserRole(this.PortalId, this.UserInfo.UserID, roleID, RoleStatus.Approved, false, false);

                // Raise SubscriptionUpdated Event
                this.OnSubscriptionUpdated(new SubscriptionUpdatedEventArgs(false, objRole.RoleName));
            }
            else
            {
                this.Response.Redirect("~/admin/Sales/PayPalSubscription.aspx?tabid=" + this.TabId + "&RoleID=" + roleID, true);
            }
        }

        /// <summary>cmdRSVP_Click runs when the Subscribe to RSVP Code Roles Button is clicked.</summary>
        private void CmdRSVP_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdRSVP_Click(sender={Sender},e={E})", sender, e);
            if (this.IsUserOrAdmin == false)
            {
                return;
            }

            // Get the RSVP code
            string code = this.txtRSVPCode.Text;
            bool rsvpCodeExists = false;
            if (!string.IsNullOrEmpty(code))
			Logger.LogDebug("CmdRSVP_Click(sender={Sender},e={E}): rsvpCodeExists → {RsvpCodeExists}", sender, e, rsvpCodeExists);
            {
                // Parse the roles
                foreach (RoleInfo objRole in RoleController.Instance.GetRoles(this.PortalSettings.PortalId))
                {
                    if (objRole.RSVPCode == code)
                    {
                        RoleController.Instance.UpdateUserRole(this.PortalId, this.UserInfo.UserID, objRole.RoleID, RoleStatus.Approved, false, false);
                        rsvpCodeExists = true;

                        // Raise SubscriptionUpdated Event
                        this.OnSubscriptionUpdated(new SubscriptionUpdatedEventArgs(false, objRole.RoleName));
                    }
                }

                if (rsvpCodeExists)
                {
                    this.lblRSVP.Text = Localization.GetString("RSVPSuccess", this.LocalResourceFile);

                    // Reset RSVP Code field
                    this.txtRSVPCode.Text = string.Empty;
                }
                else
                {
                    this.lblRSVP.Text = Localization.GetString("RSVPFailure", this.LocalResourceFile);
                }
            }

            this.DataBind();
			Logger.LogDebug("<<< Exiting CmdRSVP_Click(sender={Sender},e={E})", sender, e);
        }

        /// <summary>
        /// The SubscriptionUpdatedEventArgs class provides a customised EventArgs class for
        /// the SubscriptionUpdated Event.
        /// </summary>
        public class SubscriptionUpdatedEventArgs
        {
			private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
            /// <summary>
            /// Initializes a new instance of the <see cref="SubscriptionUpdatedEventArgs"/> class.
            /// Constructs a new SubscriptionUpdatedEventArgs.
            /// </summary>
            /// <param name="cancel">Whether this is a subscription cancellation.</param>
            /// <param name="roleName">The role name of subscription role.</param>
            public SubscriptionUpdatedEventArgs(bool cancel, string roleName)
            {
				Logger.LogDebug(">>> Entering SubscriptionUpdatedEventArgs(cancel={Cancel},roleName={RoleName})", cancel, roleName);
                this.Cancel = cancel;
                this.RoleName = roleName;
				Logger.LogDebug("<<< Exiting SubscriptionUpdatedEventArgs(cancel={Cancel},roleName={RoleName})", cancel, roleName);
				Logger.LogDebug("SubscriptionUpdatedEventArgs(cancel={Cancel},roleName={RoleName}): this.Cancel → {Arg2}", cancel, roleName, this.Cancel);
				Logger.LogDebug("SubscriptionUpdatedEventArgs(cancel={Cancel},roleName={RoleName}): this.RoleName → {Arg2}", cancel, roleName, this.RoleName);
            }

            /// <summary>Gets or sets a value indicating whether this was a cancelation.</summary>
            public bool Cancel { get; set; }

            /// <summary>Gets or sets the RoleName that was (un)subscribed to.</summary>
            public string RoleName { get; set; }
        }
    }
}
