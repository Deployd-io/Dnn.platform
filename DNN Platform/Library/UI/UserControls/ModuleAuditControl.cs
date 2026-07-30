// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.UserControls
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    public abstract class ModuleAuditControl : UserControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Label lblCreatedBy;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Label lblUpdatedBy;
        private const string MyFileName = "ModuleAuditControl.ascx";

        private static readonly Regex CheckDateColumnRegex = new Regex(@"^-?\d+$", RegexOptions.Compiled);
        private string systemUser;

        /// <summary>Initializes a new instance of the <see cref="ModuleAuditControl"/> class.</summary>
        public ModuleAuditControl()
        {
			Logger.LogDebug(">>> Entering ModuleAuditControl()");
            this.LastModifiedDate = string.Empty;
            this.LastModifiedByUser = string.Empty;
			Logger.LogDebug("ModuleAuditControl(): this.LastModifiedDate → {Arg0}", this.LastModifiedDate);
			Logger.LogDebug("ModuleAuditControl(): this.LastModifiedByUser → {Arg0}", this.LastModifiedByUser);
            this.CreatedByUser = string.Empty;
            this.CreatedDate = string.Empty;
			Logger.LogDebug("<<< Exiting ModuleAuditControl()");
			Logger.LogDebug("ModuleAuditControl(): this.CreatedByUser → {Arg0}", this.CreatedByUser);
			Logger.LogDebug("ModuleAuditControl(): this.CreatedDate → {Arg0}", this.CreatedDate);
        }

        public string CreatedDate { private get; set; }

        public string CreatedByUser { private get; set; }

        public string LastModifiedByUser { private get; set; }

        public string LastModifiedDate { private get; set; }

        public BaseEntityInfo Entity
        {
            set
            {
                if (value != null)
                {
                    var entity = new EntityInfo();
                    entity.CreatedByUserID = value.CreatedByUserID;
                    entity.CreatedOnDate = value.CreatedOnDate;
                    entity.LastModifiedByUserID = value.LastModifiedByUserID;
                    entity.LastModifiedOnDate = value.LastModifiedOnDate;

                    this.ViewState["Entity"] = entity;
                }
                else
                {
                    this.ViewState["Entity"] = null;
                }
            }
        }

        private string DisplayMode => (this.Request.QueryString["Display"] ?? string.Empty).ToLowerInvariant();

        private EntityInfo Model
        {
            get { return this.ViewState["Entity"] as EntityInfo; }
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            try
            {
                if (this.Model != null)
                {
                    this.CreatedByUser = this.Model.CreatedByUserID.ToString(CultureInfo.CurrentCulture);
                    this.CreatedDate = this.Model.CreatedOnDate.ToString(CultureInfo.CurrentCulture);
                    this.LastModifiedByUser = this.Model.LastModifiedByUserID.ToString(CultureInfo.CurrentCulture);
                    this.LastModifiedDate = this.Model.LastModifiedOnDate.ToString(CultureInfo.CurrentCulture);
                }

                // check to see if updated check is redundant
                var isCreatorAndUpdater = this.CreatedByUser == this.LastModifiedByUser &&
                    Globals.NumberMatchRegex.IsMatch(this.CreatedByUser) && Globals.NumberMatchRegex.IsMatch(this.LastModifiedByUser);

                this.systemUser = Localization.GetString("SystemUser", Localization.GetResourceFile(this, MyFileName));
                var displayMode = this.DisplayMode;
                if (displayMode != "editor" && displayMode != "settings")
                {
                    this.ShowCreatedString();
                    this.ShowUpdatedString(isCreatorAndUpdater);
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        private void ShowCreatedString()
        {
			Logger.LogDebug(">>> Entering ShowCreatedString()");
            if (CheckDateColumnRegex.IsMatch(this.CreatedByUser))
            {
                if (int.Parse(this.CreatedByUser, CultureInfo.CurrentCulture) == Null.NullInteger)
                {
                    this.CreatedByUser = this.systemUser;
					Logger.LogDebug("ShowCreatedString(): this.CreatedByUser → {Arg0}", this.CreatedByUser);
                }
                else
                {
                    // contains a UserID
                    UserInfo userInfo = UserController.GetUserById(PortalController.Instance.GetCurrentSettings().PortalId, int.Parse(this.CreatedByUser, CultureInfo.CurrentCulture));
                    if (userInfo != null)
					Logger.LogDebug("ShowCreatedString(): userInfo → {UserInfo}", userInfo);
                    {
                        this.CreatedByUser = userInfo.DisplayName;
                    }
                }
            }

            string createdString = Localization.GetString("CreatedBy", Localization.GetResourceFile(this, MyFileName));
            this.lblCreatedBy.Text = string.Format(CultureInfo.CurrentCulture, createdString, this.CreatedByUser, this.CreatedDate);
			Logger.LogDebug("<<< Exiting ShowCreatedString()");
        }

        private void ShowUpdatedString(bool isCreatorAndUpdater)
        {
            // check to see if audit contains update information
            if (string.IsNullOrEmpty(this.LastModifiedDate))
            {
                return;
            }

            if (CheckDateColumnRegex.IsMatch(this.LastModifiedByUser))
            {
                if (isCreatorAndUpdater)
                {
                    this.LastModifiedByUser = this.CreatedByUser;
                }
                else if (int.Parse(this.LastModifiedByUser, CultureInfo.CurrentCulture) == Null.NullInteger)
                {
                    this.LastModifiedByUser = this.systemUser;
                }
                else
                {
                    // contains a UserID
                    UserInfo userInfo = UserController.GetUserById(PortalController.Instance.GetCurrentSettings().PortalId, int.Parse(this.LastModifiedByUser, CultureInfo.CurrentCulture));
                    if (userInfo != null)
                    {
                        this.LastModifiedByUser = userInfo.DisplayName;
                    }
                }
            }

            string updatedByString = Localization.GetString("UpdatedBy", Localization.GetResourceFile(this, MyFileName));
            this.lblUpdatedBy.Text = string.Format(CultureInfo.CurrentCulture, updatedByString, this.LastModifiedByUser, this.LastModifiedDate);
			Logger.LogDebug("<<< Exiting ShowUpdatedString(isCreatorAndUpdater={IsCreatorAndUpdater})", isCreatorAndUpdater);
        }

        [Serializable]
        private class EntityInfo
        {
            public int CreatedByUserID { get; set; }

            public DateTime CreatedOnDate { get; set; }

            public int LastModifiedByUserID { get; set; }

            public DateTime LastModifiedOnDate { get; set; }
        }
    }
}
