// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Utilities
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;

    /// <summary>Library responsible for interacting with DNN Client API.</summary>
    public partial class DNNClientAPI
    {
        private static readonly Hashtable ObjEnabledClientPersonalizationKeys = new Hashtable();

        public enum MinMaxPersistanceType
        {
			private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DNNClientAPI));
            /// <summary>No min-max persistence.</summary>
            None = 0,

            /// <summary>Store min-max via page state.</summary>
            Page = 1,

            /// <summary>Store min-max via cookie.</summary>
            Cookie = 2,

            /// <summary>Store min-max via personalization.</summary>
            Personalization = 3,
        }

        public enum PageCallBackType
        {
            /// <summary>Get personalization.</summary>
            GetPersonalization = 0,

            /// <summary>Set personalization.</summary>
            SetPersonalization = 1,
        }

        public static void SetInitialFocus(Page objPage, Control control)
        {
			Logger.LogDebug(">>> Entering SetInitialFocus(objPage={ObjPage},control={Control})", objPage, control);
            AddBodyOnLoad(objPage, "__dnn_SetInitialFocus", "__dnn_SetInitialFocus('" + control.ClientID + "');");
			Logger.LogDebug("<<< Exiting SetInitialFocus(objPage={ObjPage},control={Control})", objPage, control);
        }

        public static void SetScrollTop(Page objPage)
        {
			Logger.LogDebug(">>> Entering SetScrollTop(objPage={ObjPage})", objPage);
            AddBodyOnLoad(objPage, "__dnn_setScrollTop", "__dnn_setScrollTop();");
			Logger.LogDebug("<<< Exiting SetScrollTop(objPage={ObjPage})", objPage);
        }

        public static void EnableDragDrop(Page objPage)
        {
			Logger.LogDebug(">>> Entering EnableDragDrop(objPage={ObjPage})", objPage);
            AddBodyOnLoad(objPage, "__dnn_enableDragDrop", "__dnn_enableDragDrop();");
			Logger.LogDebug("<<< Exiting EnableDragDrop(objPage={ObjPage})", objPage);
        }

        public static void ShowModalPage(Page objPage, string url)
        {
			Logger.LogDebug(">>> Entering ShowModalPage(objPage={ObjPage},url={Url})", objPage, url);
            AddBodyOnLoad(objPage, "__dnn_ShowModalPage", "__dnn_ShowModalPage('" + url + "');");
			Logger.LogDebug("<<< Exiting ShowModalPage(objPage={ObjPage},url={Url})", objPage, url);
        }

        public static void InitFileManager(Page objPage)
        {
			Logger.LogDebug(">>> Entering InitFileManager(objPage={ObjPage})", objPage);
            AddBodyOnLoad(objPage, "initFileManager", "initFileManager();");
			Logger.LogDebug("<<< Exiting InitFileManager(objPage={ObjPage})", objPage);
        }

        /// <summary>Allows any module to have drag and drop functionality enabled.</summary>
        /// <param name="objTitle">Title element that responds to the click and dragged.</param>
        /// <param name="objContainer">Container.</param>
        /// <param name="moduleID">Module ID.</param>
        /// <remarks>
        /// This sub also will send down information to notify the client of the panes that have been defined in the current skin.
        /// </remarks>
        public static void EnableContainerDragAndDrop(Control objTitle, Control objContainer, int moduleID)
        {
			Logger.LogDebug(">>> Entering EnableContainerDragAndDrop(objTitle={ObjTitle},objContainer={ObjContainer},moduleID={ModuleID})", objTitle, objContainer, moduleID);
            if (ClientAPI.ClientAPIDisabled() == false && ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.Positioning))
            {
                EnableDragDrop(objTitle.Page);

                JavaScript.RegisterClientReference(objTitle.Page, ClientAPI.ClientNamespaceReferences.dnn_dom_positioning);
                ClientAPI.RegisterClientVariable(objTitle.Page, "__dnn_dragDrop", objContainer.ClientID + " " + objTitle.ClientID + " " + moduleID + ";", false);
                string strPanes = string.Empty;
                string strPaneNames = string.Empty;
                var objPortalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
				Logger.LogDebug("EnableContainerDragAndDrop(objTitle={ObjTitle},objContainer={ObjContainer},moduleID={ModuleID}): objPortalSettings → {ObjPortalSettings}", objTitle, objContainer, moduleID, objPortalSettings);

                Control objCtl;
                foreach (string strPane in objPortalSettings.ActiveTab.Panes)
                {
                    objCtl = Common.Globals.FindControlRecursive(objContainer.Parent, strPane);
                    if (objCtl != null)
                    {
                        strPanes += objCtl.ClientID + ";";
                    }

                    strPaneNames += strPane + ";";
                }

                ClientAPI.RegisterClientVariable(objTitle.Page, "__dnn_Panes", strPanes, true);
                ClientAPI.RegisterClientVariable(objTitle.Page, "__dnn_PaneNames", strPaneNames, true);
            }
			Logger.LogDebug("<<< Exiting EnableContainerDragAndDrop(objTitle={ObjTitle},objContainer={ObjContainer},moduleID={ModuleID})", objTitle, objContainer, moduleID);
        }

        /// <summary>Allows a button and a content area to support client side min/max functionality.</summary>
        /// <param name="objButton">Control that when clicked causes content area to be hidden/shown.</param>
        /// <param name="objContent">Content area that is hidden/shown.</param>
        /// <param name="blnDefaultMin">If content area is to be defaulted to minimized pass in true.</param>
        /// <param name="ePersistanceType">How to store current state of min/max.  Cookie, Page, None.</param>
        /// <remarks>
        /// This method's purpose is to provide a higher level of abstraction between the ClientAPI and the module developer.
        /// </remarks>
        public static void EnableMinMax(Control objButton, Control objContent, bool blnDefaultMin, MinMaxPersistanceType ePersistanceType)
        {
			Logger.LogDebug(">>> Entering EnableMinMax(objButton={ObjButton},objContent={ObjContent},blnDefaultMin={BlnDefaultMin},ePersistanceType={EPersistanceType})", objButton, objContent, blnDefaultMin, ePersistanceType);
            EnableMinMax(objButton, objContent, -1, blnDefaultMin, string.Empty, string.Empty, ePersistanceType);
			Logger.LogDebug("<<< Exiting EnableMinMax(objButton={ObjButton},objContent={ObjContent},blnDefaultMin={BlnDefaultMin},ePersistanceType={EPersistanceType})", objButton, objContent, blnDefaultMin, ePersistanceType);
        }

        public static void EnableMinMax(Control objButton, Control objContent, int intModuleId, bool blnDefaultMin, MinMaxPersistanceType ePersistanceType)
        {
			Logger.LogDebug(">>> Entering EnableMinMax(objButton={ObjButton},objContent={ObjContent},intModuleId={IntModuleId},blnDefaultMin={BlnDefaultMin},ePersistanceType={EPersistanceType})", objButton, objContent, intModuleId, blnDefaultMin, ePersistanceType);
            EnableMinMax(objButton, objContent, intModuleId, blnDefaultMin, string.Empty, string.Empty, ePersistanceType);
			Logger.LogDebug("<<< Exiting EnableMinMax(objButton={ObjButton},objContent={ObjContent},intModuleId={IntModuleId},blnDefaultMin={BlnDefaultMin},ePersistanceType={EPersistanceType})", objButton, objContent, intModuleId, blnDefaultMin, ePersistanceType);
        }

        public static void EnableMinMax(Control objButton, Control objContent, bool blnDefaultMin, string strMinIconLoc, string strMaxIconLoc, MinMaxPersistanceType ePersistanceType)
        {
			Logger.LogDebug(">>> Entering EnableMinMax(objButton={ObjButton},objContent={ObjContent},blnDefaultMin={BlnDefaultMin},strMinIconLoc={StrMinIconLoc},strMaxIconLoc={StrMaxIconLoc},ePersistanceType={EPersistanceType})", objButton, objContent, blnDefaultMin, strMinIconLoc, strMaxIconLoc, ePersistanceType);
            EnableMinMax(objButton, objContent, -1, blnDefaultMin, strMinIconLoc, strMaxIconLoc, ePersistanceType);
			Logger.LogDebug("<<< Exiting EnableMinMax(objButton={ObjButton},objContent={ObjContent},blnDefaultMin={BlnDefaultMin},strMinIconLoc={StrMinIconLoc},strMaxIconLoc={StrMaxIconLoc},ePersistanceType={EPersistanceType})", objButton, objContent, blnDefaultMin, strMinIconLoc, strMaxIconLoc, ePersistanceType);
        }

        /// <summary>Allows a button and a content area to support client side min/max functionality.</summary>
        /// <param name="objButton">Control that when clicked causes content area to be hidden/shown.</param>
        /// <param name="objContent">Content area that is hidden/shown.</param>
        /// <param name="intModuleId">Module id of button/content, used only for persistance type of Cookie.</param>
        /// <param name="blnDefaultMin">If content area is to be defaulted to minimized pass in true.</param>
        /// <param name="strMinIconLoc">Location of minimized icon.</param>
        /// <param name="strMaxIconLoc">Location of maximized icon.</param>
        /// <param name="ePersistanceType">How to store current state of min/max.  Cookie, Page, None.</param>
        /// <remarks>
        /// This method's purpose is to provide a higher level of abstraction between the ClientAPI and the module developer.
        /// </remarks>
        public static void EnableMinMax(Control objButton, Control objContent, int intModuleId, bool blnDefaultMin, string strMinIconLoc, string strMaxIconLoc, MinMaxPersistanceType ePersistanceType)
        {
			Logger.LogDebug(">>> Entering EnableMinMax(objButton={ObjButton},objContent={ObjContent},intModuleId={IntModuleId},blnDefaultMin={BlnDefaultMin},strMinIconLoc={StrMinIconLoc},strMaxIconLoc={StrMaxIconLoc},ePersistanceType={EPersistanceType})", objButton, objContent, intModuleId, blnDefaultMin, strMinIconLoc, strMaxIconLoc, ePersistanceType);
            EnableMinMax(objButton, objContent, intModuleId, blnDefaultMin, strMinIconLoc, strMaxIconLoc, ePersistanceType, 5);
			Logger.LogDebug("<<< Exiting EnableMinMax(objButton={ObjButton},objContent={ObjContent},intModuleId={IntModuleId},blnDefaultMin={BlnDefaultMin},strMinIconLoc={StrMinIconLoc},strMaxIconLoc={StrMaxIconLoc},ePersistanceType={EPersistanceType})", objButton, objContent, intModuleId, blnDefaultMin, strMinIconLoc, strMaxIconLoc, ePersistanceType);
        }

        public static void EnableMinMax(Control objButton, Control objContent, bool blnDefaultMin, string strMinIconLoc, string strMaxIconLoc, MinMaxPersistanceType ePersistanceType, string strPersonalizationNamingCtr, string strPersonalizationKey)
        {
			Logger.LogDebug(">>> Entering EnableMinMax(objButton={ObjButton},objContent={ObjContent},blnDefaultMin={BlnDefaultMin},strMinIconLoc={StrMinIconLoc},strMaxIconLoc={StrMaxIconLoc},ePersistanceType={EPersistanceType},strPersonalizationNamingCtr={StrPersonalizationNamingCtr},strPersonalizationKey={StrPersonalizationKey})", objButton, objContent, blnDefaultMin, strMinIconLoc, strMaxIconLoc, ePersistanceType, strPersonalizationNamingCtr, strPersonalizationKey);
            EnableMinMax(objButton, objContent, -1, blnDefaultMin, strMinIconLoc, strMaxIconLoc, ePersistanceType, 5, strPersonalizationNamingCtr, strPersonalizationKey);
			Logger.LogDebug("<<< Exiting EnableMinMax(objButton={ObjButton},objContent={ObjContent},blnDefaultMin={BlnDefaultMin},strMinIconLoc={StrMinIconLoc},strMaxIconLoc={StrMaxIconLoc},ePersistanceType={EPersistanceType},strPersonalizationNamingCtr={StrPersonalizationNamingCtr},strPersonalizationKey={StrPersonalizationKey})", objButton, objContent, blnDefaultMin, strMinIconLoc, strMaxIconLoc, ePersistanceType, strPersonalizationNamingCtr, strPersonalizationKey);
        }

        public static void EnableMinMax(Control objButton, Control objContent, int intModuleId, bool blnDefaultMin, string strMinIconLoc, string strMaxIconLoc, MinMaxPersistanceType ePersistanceType, int intAnimationFrames)
        {
			Logger.LogDebug(">>> Entering EnableMinMax(objButton={ObjButton},objContent={ObjContent},intModuleId={IntModuleId},blnDefaultMin={BlnDefaultMin},strMinIconLoc={StrMinIconLoc},strMaxIconLoc={StrMaxIconLoc},ePersistanceType={EPersistanceType},intAnimationFrames={IntAnimationFrames})", objButton, objContent, intModuleId, blnDefaultMin, strMinIconLoc, strMaxIconLoc, ePersistanceType, intAnimationFrames);
            EnableMinMax(objButton, objContent, intModuleId, blnDefaultMin, strMinIconLoc, strMaxIconLoc, ePersistanceType, intAnimationFrames, null, null);
			Logger.LogDebug("<<< Exiting EnableMinMax(objButton={ObjButton},objContent={ObjContent},intModuleId={IntModuleId},blnDefaultMin={BlnDefaultMin},strMinIconLoc={StrMinIconLoc},strMaxIconLoc={StrMaxIconLoc},ePersistanceType={EPersistanceType},intAnimationFrames={IntAnimationFrames})", objButton, objContent, intModuleId, blnDefaultMin, strMinIconLoc, strMaxIconLoc, ePersistanceType, intAnimationFrames);
        }

        public static void EnableMinMax(Control objButton, Control objContent, int intModuleId, bool blnDefaultMin, string strMinIconLoc, string strMaxIconLoc, MinMaxPersistanceType ePersistanceType, int intAnimationFrames, string strPersonalizationNamingCtr, string strPersonalizationKey)
        {
			Logger.LogDebug(">>> Entering EnableMinMax(objButton={ObjButton},objContent={ObjContent},intModuleId={IntModuleId},blnDefaultMin={BlnDefaultMin},strMinIconLoc={StrMinIconLoc},strMaxIconLoc={StrMaxIconLoc},ePersistanceType={EPersistanceType},intAnimationFrames={IntAnimationFrames},strPersonalizationNamingCtr={StrPersonalizationNamingCtr},strPersonalizationKey={StrPersonalizationKey})", objButton, objContent, intModuleId, blnDefaultMin, strMinIconLoc, strMaxIconLoc, ePersistanceType, intAnimationFrames, strPersonalizationNamingCtr, strPersonalizationKey);
            if (ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.DHTML))
            {
                JavaScript.RegisterClientReference(objButton.Page, ClientAPI.ClientNamespaceReferences.dnn_dom);
                switch (ePersistanceType)
                {
                    case MinMaxPersistanceType.None:
                        AddAttribute(objButton, "onclick", "if (__dnn_SectionMaxMin(this,  '" + objContent.ClientID + "')) return false;");
                        if (!string.IsNullOrEmpty(strMinIconLoc))
                        {
                            AddAttribute(objButton, "max_icon", strMaxIconLoc);
                            AddAttribute(objButton, "min_icon", strMinIconLoc);
                        }

                        break;
                    case MinMaxPersistanceType.Page:
                        AddAttribute(objButton, "onclick", "if (__dnn_SectionMaxMin(this,  '" + objContent.ClientID + "')) return false;");
                        if (!string.IsNullOrEmpty(strMinIconLoc))
                        {
                            AddAttribute(objButton, "max_icon", strMaxIconLoc);
                            AddAttribute(objButton, "min_icon", strMinIconLoc);
                        }

                        break;
                    case MinMaxPersistanceType.Cookie:
                        if (intModuleId != -1)
                        {
                            AddAttribute(objButton, "onclick", "if (__dnn_ContainerMaxMin_OnClick(this, '" + objContent.ClientID + "')) return false;");
                            ClientAPI.RegisterClientVariable(objButton.Page, "containerid_" + objContent.ClientID, intModuleId.ToString(CultureInfo.InvariantCulture), true);
                            ClientAPI.RegisterClientVariable(objButton.Page, "cookieid_" + objContent.ClientID, "_Module" + intModuleId + "_Visible", true);

                            ClientAPI.RegisterClientVariable(objButton.Page, "min_icon_" + intModuleId, strMinIconLoc, true);
                            ClientAPI.RegisterClientVariable(objButton.Page, "max_icon_" + intModuleId, strMaxIconLoc, true);

                            ClientAPI.RegisterClientVariable(objButton.Page, "max_text", Localization.GetString("Maximize"), true);
                            ClientAPI.RegisterClientVariable(objButton.Page, "min_text", Localization.GetString("Minimize"), true);

                            if (blnDefaultMin)
                            {
                                ClientAPI.RegisterClientVariable(objButton.Page, "__dnn_" + intModuleId + ":defminimized", "true", true);
                            }
                        }

                        break;
                    case MinMaxPersistanceType.Personalization:
                        // Regardless if we determine whether or not the browser supports client-side personalization
                        // we need to store these keys to properly display or hide the content (They are needed in MinMaxContentVisible)
                        AddAttribute(objButton, "userctr", strPersonalizationNamingCtr);
                        AddAttribute(objButton, "userkey", strPersonalizationKey);
                        if (EnableClientPersonalization(strPersonalizationNamingCtr, strPersonalizationKey, objButton.Page))
                        {
                            AddAttribute(objButton, "onclick", "if (__dnn_SectionMaxMin(this,  '" + objContent.ClientID + "')) return false;");
                            if (!string.IsNullOrEmpty(strMinIconLoc))
                            {
                                AddAttribute(objButton, "max_icon", strMaxIconLoc);
                                AddAttribute(objButton, "min_icon", strMinIconLoc);
                            }
                        }

                        break;
                }
            }

            if (MinMaxContentVisibile(objButton, intModuleId, blnDefaultMin, ePersistanceType))
            {
                if (ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.DHTML))
                {
                    AddStyleAttribute(objContent, "display", string.Empty);
                }
                else
                {
                    objContent.Visible = true;
					Logger.LogDebug("EnableMinMax(objButton={ObjButton},objContent={ObjContent},intModuleId={IntModuleId},blnDefaultMin={BlnDefaultMin},strMinIconLoc={StrMinIconLoc},strMaxIconLoc={StrMaxIconLoc},ePersistanceType={EPersistanceType},intAnimationFrames={IntAnimationFrames},strPersonalizationNamingCtr={StrPersonalizationNamingCtr},strPersonalizationKey={StrPersonalizationKey}): objContent.Visible → {Arg10}", objButton, objContent, intModuleId, blnDefaultMin, strMinIconLoc, strMaxIconLoc, ePersistanceType, intAnimationFrames, strPersonalizationNamingCtr, strPersonalizationKey, objContent.Visible);
                }

                if (!string.IsNullOrEmpty(strMinIconLoc))
                {
                    SetMinMaxProperties(objButton, strMinIconLoc, Localization.GetString("Minimize"), Localization.GetString("Minimize"));
                }
            }
            else
            {
                if (ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.DHTML))
                {
                    AddStyleAttribute(objContent, "display", "none");
                }
                else
                {
                    objContent.Visible = false;
                }

                if (!string.IsNullOrEmpty(strMaxIconLoc))
                {
                    SetMinMaxProperties(objButton, strMaxIconLoc, Localization.GetString("Maximize"), Localization.GetString("Maximize"));
                }
            }

            if (intAnimationFrames != 5)
            {
                ClientAPI.RegisterClientVariable(objButton.Page, "animf_" + objContent.ClientID, intAnimationFrames.ToString(CultureInfo.InvariantCulture), true);
            }
			Logger.LogDebug("<<< Exiting EnableMinMax(objButton={ObjButton},objContent={ObjContent},intModuleId={IntModuleId},blnDefaultMin={BlnDefaultMin},strMinIconLoc={StrMinIconLoc},strMaxIconLoc={StrMaxIconLoc},ePersistanceType={EPersistanceType},intAnimationFrames={IntAnimationFrames},strPersonalizationNamingCtr={StrPersonalizationNamingCtr},strPersonalizationKey={StrPersonalizationKey})", objButton, objContent, intModuleId, blnDefaultMin, strMinIconLoc, strMaxIconLoc, ePersistanceType, intAnimationFrames, strPersonalizationNamingCtr, strPersonalizationKey);
        }

        public static bool MinMaxContentVisibile(Control objButton, bool blnDefaultMin, MinMaxPersistanceType ePersistanceType)
        {
			Logger.LogDebug(">>> Entering MinMaxContentVisibile(objButton={ObjButton},blnDefaultMin={BlnDefaultMin},ePersistanceType={EPersistanceType})", objButton, blnDefaultMin, ePersistanceType);
			Logger.LogDebug("<<< Exiting MinMaxContentVisibile(objButton={ObjButton},blnDefaultMin={BlnDefaultMin},ePersistanceType={EPersistanceType})", objButton, blnDefaultMin, ePersistanceType);
			Logger.LogDebug("<<< Exiting MinMaxContentVisibile(objButton={ObjButton},blnDefaultMin={BlnDefaultMin},ePersistanceType={EPersistanceType},value={Value})", objButton, blnDefaultMin, ePersistanceType, value);
			Logger.LogDebug("<<< Exiting MinMaxContentVisibile(objButton={ObjButton},intModuleId={IntModuleId},blnDefaultMin={BlnDefaultMin},ePersistanceType={EPersistanceType})", objButton, intModuleId, blnDefaultMin, ePersistanceType);
            return MinMaxContentVisibile(objButton, -1, blnDefaultMin, ePersistanceType);
        }

        public static void MinMaxContentVisibile(Control objButton, bool blnDefaultMin, MinMaxPersistanceType ePersistanceType, bool value)
        {
            MinMaxContentVisibile(objButton, -1, blnDefaultMin, ePersistanceType, value);
        }

        public static bool MinMaxContentVisibile(Control objButton, int intModuleId, bool blnDefaultMin, MinMaxPersistanceType ePersistanceType)
        {
            if (HttpContext.Current != null)
            {
                switch (ePersistanceType)
                {
                    case MinMaxPersistanceType.Page:
                        string sExpanded = ClientAPI.GetClientVariable(objButton.Page, objButton.ClientID + ":exp");
                        if (!string.IsNullOrEmpty(sExpanded))
                        {
                            return sExpanded == "1" ? true : false;
                        }
                        else
                        {
                            return !blnDefaultMin;
                        }

                    case MinMaxPersistanceType.Cookie:
                        if (intModuleId != -1)
                        {
                            HttpCookie objModuleVisible = HttpContext.Current.Request.Cookies["_Module" + intModuleId + "_Visible"];
                            if (objModuleVisible != null)
                            {
                                return objModuleVisible.Value != "false";
                            }
                            else
                            {
                                return !blnDefaultMin;
                            }
                        }
                        else
                        {
                            return true;
                        }

                    case MinMaxPersistanceType.Personalization:
                        string strVisible = Convert.ToString(Personalization.GetProfile(Globals.GetAttribute(objButton, "userctr"), Globals.GetAttribute(objButton, "userkey")), CultureInfo.InvariantCulture);
                        if (string.IsNullOrEmpty(strVisible))
                        {
                            return blnDefaultMin;
                        }
                        else
                        {
                            return Convert.ToBoolean(strVisible);
                        }

                    default:
                        return !blnDefaultMin;
                }
            }

            return Null.NullBoolean;
        }

        public static void MinMaxContentVisibile(Control objButton, int intModuleId, bool blnDefaultMin, MinMaxPersistanceType ePersistanceType, bool value)
        {
            if (HttpContext.Current != null)
            {
                switch (ePersistanceType)
                {
                    case MinMaxPersistanceType.Page:
                        ClientAPI.RegisterClientVariable(objButton.Page, objButton.ClientID + ":exp", Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture), true);
                        break;
                    case MinMaxPersistanceType.Cookie:
                        var objModuleVisible = new HttpCookie("_Module" + intModuleId + "_Visible", value.ToString().ToLowerInvariant())
                        {
                            Expires = DateTime.MaxValue,
                            Path = !string.IsNullOrEmpty(Common.Globals.ApplicationPath) ? Common.Globals.ApplicationPath : "/",
                        };
                        HttpContext.Current.Response.AppendCookie(objModuleVisible);
                        break;
                    case MinMaxPersistanceType.Personalization:
                        Personalization.SetProfile(Globals.GetAttribute(objButton, "userctr"), Globals.GetAttribute(objButton, "userkey"), value.ToString());
                        break;
                }
            }
        }

        // enables callbacks for request, and registers personalization key to be accessible from client
        // returns true when browser is capable of callbacks
        public static bool EnableClientPersonalization(string strNamingContainer, string strKey, Page objPage)
        {
			Logger.LogDebug(">>> Entering EnableClientPersonalization(strNamingContainer={StrNamingContainer},strKey={StrKey},objPage={ObjPage})", strNamingContainer, strKey, objPage);
            if (ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.XMLHTTP))
            {
                // Instead of sending the callback js function down to the client, we are hardcoding
                // it on the client.  DNN owns the interface, so there is no worry about an outside
                // entity changing it on us.  We are simply calling this here to register all the appropriate
                // js libraries
                ClientAPI.GetCallbackEventReference(objPage, string.Empty, string.Empty, string.Empty, string.Empty);

                // in order to limit the keys that can be accessed and written we are storing the enabled keys
                // in this shared hash table
                lock (ObjEnabledClientPersonalizationKeys.SyncRoot)
                {
                    if (IsPersonalizationKeyRegistered(strNamingContainer + ClientAPI.CUSTOM_COLUMN_DELIMITER + strKey) == false)
                    {
                        ObjEnabledClientPersonalizationKeys.Add(strNamingContainer + ClientAPI.CUSTOM_COLUMN_DELIMITER + strKey, string.Empty);
                    }
                }

			Logger.LogDebug("<<< Exiting EnableClientPersonalization(strNamingContainer={StrNamingContainer},strKey={StrKey},objPage={ObjPage})", strNamingContainer, strKey, objPage);
                return true;
            }

            return false;
        }

        public static bool IsPersonalizationKeyRegistered(string strKey)
        {
			Logger.LogDebug(">>> Entering IsPersonalizationKeyRegistered(strKey={StrKey})", strKey);
			Logger.LogDebug("<<< Exiting IsPersonalizationKeyRegistered(strKey={StrKey})", strKey);
            return ObjEnabledClientPersonalizationKeys.ContainsKey(strKey);
        }

        private static void AddBodyOnLoad(Page objPage, string scriptKey, string strJSFunction)
        {
			Logger.LogDebug(">>> Entering AddBodyOnLoad(objPage={ObjPage},scriptKey={ScriptKey},strJSFunction={StrJSFunction})", objPage, scriptKey, strJSFunction);
            JavaScript.RegisterClientReference(objPage, ClientAPI.ClientNamespaceReferences.dnn);
            objPage.ClientScript.RegisterStartupScript(objPage.GetType(), scriptKey, strJSFunction, true);
			Logger.LogDebug("<<< Exiting AddBodyOnLoad(objPage={ObjPage},scriptKey={ScriptKey},strJSFunction={StrJSFunction})", objPage, scriptKey, strJSFunction);
        }

        private static void SetMinMaxProperties(Control objButton, string strImage, string strToolTip, string strAltText)
        {
			Logger.LogDebug(">>> Entering SetMinMaxProperties(objButton={ObjButton},strImage={StrImage},strToolTip={StrToolTip},strAltText={StrAltText})", objButton, strImage, strToolTip, strAltText);
            if (objButton is LinkButton)
            {
                var objLB = (LinkButton)objButton;
				Logger.LogDebug("SetMinMaxProperties(objButton={ObjButton},strImage={StrImage},strToolTip={StrToolTip},strAltText={StrAltText}): objLB → {ObjLB}", objButton, strImage, strToolTip, strAltText, objLB);
				Logger.LogDebug("SetMinMaxProperties(objButton={ObjButton},strImage={StrImage},strToolTip={StrToolTip},strAltText={StrAltText}): objLB.ToolTip → {Arg4}", objButton, strImage, strToolTip, strAltText, objLB.ToolTip);
                objLB.ToolTip = strToolTip;
                if (objLB.Controls.Count > 0)
                {
                    SetImageProperties(objLB.Controls[0], strImage, strToolTip, strAltText);
                }
            }
            else if (objButton is Image)
            {
                SetImageProperties(objButton, strImage, strToolTip, strAltText);
            }
            else if (objButton is ImageButton)
            {
                SetImageProperties(objButton, strImage, strToolTip, strAltText);
            }
			Logger.LogDebug("<<< Exiting SetMinMaxProperties(objButton={ObjButton},strImage={StrImage},strToolTip={StrToolTip},strAltText={StrAltText})", objButton, strImage, strToolTip, strAltText);
        }

        private static void SetImageProperties(Control objControl, string strImage, string strToolTip, string strAltText)
        {
			Logger.LogDebug(">>> Entering SetImageProperties(objControl={ObjControl},strImage={StrImage},strToolTip={StrToolTip},strAltText={StrAltText})", objControl, strImage, strToolTip, strAltText);
            if (objControl is Image)
            {
                var objImage = (Image)objControl;
				Logger.LogDebug("SetImageProperties(objControl={ObjControl},strImage={StrImage},strToolTip={StrToolTip},strAltText={StrAltText}): objImage → {ObjImage}", objControl, strImage, strToolTip, strAltText, objImage);
				Logger.LogDebug("SetImageProperties(objControl={ObjControl},strImage={StrImage},strToolTip={StrToolTip},strAltText={StrAltText}): objImage.ImageUrl → {Arg4}", objControl, strImage, strToolTip, strAltText, objImage.ImageUrl);
				Logger.LogDebug("SetImageProperties(objControl={ObjControl},strImage={StrImage},strToolTip={StrToolTip},strAltText={StrAltText}): objImage.AlternateText → {Arg4}", objControl, strImage, strToolTip, strAltText, objImage.AlternateText);
				Logger.LogDebug("SetImageProperties(objControl={ObjControl},strImage={StrImage},strToolTip={StrToolTip},strAltText={StrAltText}): objImage.ToolTip → {Arg4}", objControl, strImage, strToolTip, strAltText, objImage.ToolTip);
                objImage.ImageUrl = strImage;
                objImage.AlternateText = strAltText;
                objImage.ToolTip = strToolTip;
            }
            else if (objControl is ImageButton)
            {
                var objImage = (ImageButton)objControl;
                objImage.ImageUrl = strImage;
                objImage.AlternateText = strAltText;
                objImage.ToolTip = strToolTip;
            }
            else if (objControl is HtmlImage)
            {
                var objImage = (HtmlImage)objControl;
                objImage.Src = strImage;
				Logger.LogDebug("SetImageProperties(objControl={ObjControl},strImage={StrImage},strToolTip={StrToolTip},strAltText={StrAltText}): objImage.Src → {Arg4}", objControl, strImage, strToolTip, strAltText, objImage.Src);
				Logger.LogDebug("SetImageProperties(objControl={ObjControl},strImage={StrImage},strToolTip={StrToolTip},strAltText={StrAltText}): objImage.Alt → {Arg4}", objControl, strImage, strToolTip, strAltText, objImage.Alt);
                objImage.Alt = strAltText;
            }
			Logger.LogDebug("<<< Exiting SetImageProperties(objControl={ObjControl},strImage={StrImage},strToolTip={StrToolTip},strAltText={StrAltText})", objControl, strImage, strToolTip, strAltText);
        }

        private static void AddAttribute(Control objControl, string strName, string strValue)
        {
			Logger.LogDebug(">>> Entering AddAttribute(objControl={ObjControl},strName={StrName},strValue={StrValue})", objControl, strName, strValue);
            if (objControl is HtmlControl)
            {
                ((HtmlControl)objControl).Attributes.Add(strName, strValue);
            }
            else if (objControl is WebControl)
            {
                ((WebControl)objControl).Attributes.Add(strName, strValue);
            }
			Logger.LogDebug("<<< Exiting AddAttribute(objControl={ObjControl},strName={StrName},strValue={StrValue})", objControl, strName, strValue);
        }

        private static void AddStyleAttribute(Control objControl, string strName, string strValue)
        {
			Logger.LogDebug(">>> Entering AddStyleAttribute(objControl={ObjControl},strName={StrName},strValue={StrValue})", objControl, strName, strValue);
            if (objControl is HtmlControl)
            {
                if (!string.IsNullOrEmpty(strValue))
                {
                    ((HtmlControl)objControl).Style.Add(strName, strValue);
                }
                else
                {
                    ((HtmlControl)objControl).Style.Remove(strName);
                }
            }
            else if (objControl is WebControl)
            {
                if (!string.IsNullOrEmpty(strValue))
                {
                    ((WebControl)objControl).Style.Add(strName, strValue);
                }
                else
                {
                    ((WebControl)objControl).Style.Remove(strName);
                }
            }
			Logger.LogDebug("<<< Exiting AddStyleAttribute(objControl={ObjControl},strName={StrName},strValue={StrValue})", objControl, strName, strValue);
        }
    }
}
