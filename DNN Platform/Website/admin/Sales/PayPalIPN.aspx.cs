// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Sales
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A page which received messages from PayPal.</summary>
    [DnnDeprecated(10, 0, 2, "No replacement")]
    public partial class PayPalIPN : PageBase
    {
        private readonly IHostSettingsService hostSettingsService;

        /// <summary>Initializes a new instance of the <see cref="PayPalIPN"/> class.</summary>
        public PayPalIPN()
            : this(null, null, null, null)
        {
		Logger.LogDebug(">>> Entering PayPalIPN()");
		Logger.LogDebug("<<< Exiting PayPalIPN()");
        }

        /// <summary>Initializes a new instance of the <see cref="PayPalIPN"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        public PayPalIPN(IPortalController portalController, IApplicationStatusInfo appStatus, IHostSettings hostSettings, IHostSettingsService hostSettingsService)
            : base(portalController, appStatus, hostSettings)
        {
            this.hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
			Logger.LogDebug("PayPalIPN(portalController={PortalController},appStatus={AppStatus},hostSettings={HostSettings},hostSettingsService={HostSettingsService}): hostSettingsService → {HostSettingsService1}", portalController, appStatus, hostSettings, hostSettingsService, hostSettingsService);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.InitializeComponent();
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            base.OnLoad(e);
            try
            {
                bool blnValid = true;

				Logger.LogDebug("OnLoad(e={E}): blnValid → {BlnValid}", e, blnValid);
                // string strTransactionID;
                int intRoleID = 0;
                int intPortalID = this.PortalSettings.PortalId;
				Logger.LogDebug("OnLoad(e={E}): intRoleID → {IntRoleID}", e, intRoleID);
                int intUserID = 0;

				Logger.LogDebug("OnLoad(e={E}): intUserID → {IntUserID}", e, intUserID);
                // string strDescription;
                double dblAmount = 0;

				Logger.LogDebug("OnLoad(e={E}): dblAmount → {DblAmount}", e, dblAmount);
                // string strEmail;
                bool blnCancel = false;
                string strPayPalID = Null.NullString;
				Logger.LogDebug("OnLoad(e={E}): blnCancel → {BlnCancel}", e, blnCancel);
				Logger.LogDebug("OnLoad(e={E}): strPayPalID → {StrPayPalID}", e, strPayPalID);
                string strPost = "cmd=_notify-validate";
                foreach (string strName in this.Request.Form)
                {
                    string strValue = this.Request.Form[strName];
                    switch (strName)
                    {
                        case "txn_type": // get the transaction type
                            string strTransactionType = strValue;
                            switch (strTransactionType)
                            {
                                case "subscr_signup":
                                case "subscr_payment":
                                case "web_accept":
                                    break;
                                case "subscr_cancel":
                                    blnCancel = true;
                                    break;
                                default:
                                    blnValid = false;
                                    break;
                            }

                            break;
                        case "payment_status": // verify the status
                            if (strValue != "Completed")
                            {
                                blnValid = false;
                            }

                            break;
                        case "txn_id": // verify the transaction id for duplicates
                                       //                            strTransactionID = strValue;
                            break;
                        case "receiver_email": // verify the PayPalId
                            strPayPalID = strValue;
                            break;
                        case "mc_gross": // verify the price
                            dblAmount = double.Parse(strValue);
                            break;
                        case "item_number": // get the RoleID
                            intRoleID = int.Parse(strValue);

                            // RoleInfo objRole = objRoles.GetRole(intRoleID, intPortalID);
                            break;
                        case "item_name": // get the product description
                                          //                            strDescription = strValue;
                            break;
                        case "custom": // get the UserID
                            intUserID = int.Parse(strValue);
                            break;
                        case "email": // get the email
                                      //                            strEmail = strValue;
                            break;
                    }

                    // reconstruct post for postback validation
                    strPost += string.Format("&{0}={1}", Globals.HTTPPOSTEncode(strName), Globals.HTTPPOSTEncode(strValue));
                }

                // postback to verify the source
                if (blnValid)
                {
                    Dictionary<string, string> settings = this.PortalController.GetPortalSettings(this.PortalSettings.PortalId);
                    string strPayPalURL;

                    // Sandbox mode
                    if (settings.ContainsKey("paypalsandbox") && !string.IsNullOrEmpty(settings["paypalsandbox"]) && settings["paypalsandbox"].Equals("true", StringComparison.InvariantCultureIgnoreCase))
                    {
                        strPayPalURL = "https://www.sandbox.paypal.com/cgi-bin/webscr?";
						Logger.LogDebug("OnLoad(e={E}): strPayPalURL → {StrPayPalURL}", e, strPayPalURL);
                    }
                    else
                    {
                        strPayPalURL = "https://www.paypal.com/cgi-bin/webscr?";
                    }

                    var objRequest = Globals.GetExternalRequest(strPayPalURL);
                    objRequest.Method = "POST";
					Logger.LogDebug("OnLoad(e={E}): objRequest → {ObjRequest}", e, objRequest);
					Logger.LogDebug("OnLoad(e={E}): objRequest.Method → {Arg1}", e, objRequest.Method);
					Logger.LogDebug("OnLoad(e={E}): objRequest.ContentLength → {Arg1}", e, objRequest.ContentLength);
					Logger.LogDebug("OnLoad(e={E}): objRequest.ContentType → {Arg1}", e, objRequest.ContentType);
                    objRequest.ContentLength = strPost.Length;
                    objRequest.ContentType = "application/x-www-form-urlencoded";
                    using (var objStream = new StreamWriter(objRequest.GetRequestStream()))
                    {
                        objStream.Write(strPost);
                    }

                    string strResponse;
                    using (var objResponse = (HttpWebResponse)objRequest.GetResponse())
                    {
                        using (var sr = new StreamReader(objResponse.GetResponseStream()))
                        {
                            strResponse = sr.ReadToEnd();
                        }
                    }

                    switch (strResponse)
                    {
						private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
                        case "VERIFIED":
                            break;
                        default:
                            // possible fraud
                            blnValid = false;
                            break;
                    }
                }

                if (blnValid)
                {
                    int intAdministratorRoleId = 0;
                    string strProcessorID = Null.NullString;
					Logger.LogDebug("OnLoad(e={E}): intAdministratorRoleId → {IntAdministratorRoleId}", e, intAdministratorRoleId);
					Logger.LogDebug("OnLoad(e={E}): strProcessorID → {StrProcessorID}", e, strProcessorID);
                    PortalInfo objPortalInfo = this.PortalController.GetPortal(intPortalID);
                    if (objPortalInfo != null)
					Logger.LogDebug("OnLoad(e={E}): objPortalInfo → {ObjPortalInfo}", e, objPortalInfo);
                    {
                        intAdministratorRoleId = objPortalInfo.AdministratorRoleId;
                        strProcessorID = objPortalInfo.ProcessorUserId.ToLowerInvariant();
                    }

                    if (intRoleID == intAdministratorRoleId)
                    {
                        // admin portal renewal
                        strProcessorID = this.hostSettingsService.GetString("ProcessorUserId").ToLowerInvariant();
                        float portalPrice = objPortalInfo.HostFee;
                        if ((portalPrice.ToString() == dblAmount.ToString()) && (HttpUtility.UrlDecode(strPayPalID.ToLowerInvariant()) == strProcessorID))
                        {
                            this.PortalController.UpdatePortalExpiry(intPortalID, DotNetNuke.Entities.Portals.PortalController.GetActivePortalLanguage(intPortalID));
                        }
                        else
                        {
                            var log = new LogInfo
                            {
                                LogPortalID = intPortalID,
                                LogPortalName = this.PortalSettings.PortalName,
                                LogUserID = intUserID,
                                LogTypeKey = nameof(EventLogType.POTENTIAL_PAYPAL_PAYMENT_FRAUD),
                            };
                            LogController.Instance.AddLog(log);
                        }
                    }
                    else
                    {
                        // user subscription
                        RoleInfo objRoleInfo = RoleController.Instance.GetRole(intPortalID, r => r.RoleID == intRoleID);
                        float rolePrice = objRoleInfo.ServiceFee;
						Logger.LogDebug("OnLoad(e={E}): objRoleInfo → {ObjRoleInfo}", e, objRoleInfo);
						Logger.LogDebug("OnLoad(e={E}): rolePrice → {RolePrice}", e, rolePrice);
                        float trialPrice = objRoleInfo.TrialFee;
                        if ((rolePrice.ToString() == dblAmount.ToString() || trialPrice.ToString() == dblAmount.ToString()) && (HttpUtility.UrlDecode(strPayPalID.ToLowerInvariant()) == strProcessorID))
                        {
                            RoleController.Instance.UpdateUserRole(intPortalID, intUserID, intRoleID, RoleStatus.Approved, false, blnCancel);
                        }
                        else
                        {
                            var log = new LogInfo
                            {
                                LogPortalID = intPortalID,
                                LogPortalName = this.PortalSettings.PortalName,
                                LogUserID = intUserID,
                                LogTypeKey = nameof(EventLogType.POTENTIAL_PAYPAL_PAYMENT_FRAUD),
                            };
                            LogController.Instance.AddLog(log);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, ex.Message);
                Exceptions.ProcessPageLoadException(exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
			Logger.LogInformation("OnLoad(e)={E}: external service call took {Arg1} ms", e, __logStart.ElapsedMilliseconds);
        }

        private void InitializeComponent()
        {
		Logger.LogDebug(">>> Entering InitializeComponent()");
		Logger.LogDebug("<<< Exiting InitializeComponent()");
        }
    }
}
