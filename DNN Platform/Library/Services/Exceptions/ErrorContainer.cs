// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions
{
    using System;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.UI.Skins.Controls;

    using Localization = DotNetNuke.Services.Localization.Localization;

    public class ErrorContainer : Control
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ErrorContainer));
        /// <summary>Initializes a new instance of the <see cref="ErrorContainer"/> class.</summary>
        /// <param name="strError">The error message.</param>
        public ErrorContainer(string strError)
        {
			Logger.LogError(">>> Entering ErrorContainer(strError={StrError})", strError);
            this.Container = FormatException(strError);
			Logger.LogWarning("<<< Exiting ErrorContainer(strError={StrError})", strError);
        }

        /// <summary>Initializes a new instance of the <see cref="ErrorContainer"/> class.</summary>
        /// <param name="strError">The error message.</param>
        /// <param name="exc">The exception.</param>
        public ErrorContainer(string strError, Exception exc)
        {
			Logger.LogError(">>> Entering ErrorContainer(strError={StrError},exc={Exc})", strError, exc);
            UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
			Logger.LogError("ErrorContainer(strError={StrError},exc={Exc}): objUserInfo → {ObjUserInfo}", strError, exc, objUserInfo);
            if (objUserInfo.IsSuperUser)
            {
				Logger.LogWarning("<<< Exiting ErrorContainer(strError={StrError})", strError);
                this.Container = FormatException(strError, exc);
            }
            else
            {
                this.Container = FormatException(strError);
            }
        }

        /// <summary>Initializes a new instance of the <see cref="ErrorContainer"/> class.</summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="strError">The error message.</param>
        /// <param name="exc">The exception.</param>
        public ErrorContainer(PortalSettings portalSettings, string strError, Exception exc)
            : this(strError, exc)
        {
        }

        public ModuleMessage Container { get; set; }

        private static ModuleMessage FormatException(string strError)
        {
			Logger.LogError(">>> Entering FormatException(strError={StrError})", strError);
            ModuleMessage m;
            m = UI.Skins.Skin.GetModuleMessageControl(Localization.GetString("ErrorOccurred"), strError, ModuleMessage.ModuleMessageType.RedError);
			Logger.LogError("FormatException(strError={StrError}): m → {M}", strError, m);
			Logger.LogWarning("<<< Exiting FormatException(strError={StrError})", strError);
			Logger.LogWarning("<<< Exiting FormatException(strError={StrError},exc={Exc})", strError, exc);
            return m;
        }

        private static ModuleMessage FormatException(string strError, Exception exc)
        {
            ModuleMessage m;
            if (exc != null)
            {
                m = UI.Skins.Skin.GetModuleMessageControl(strError, HttpUtility.HtmlEncode(exc.ToString()), ModuleMessage.ModuleMessageType.RedError);
            }
            else
            {
                m = UI.Skins.Skin.GetModuleMessageControl(Localization.GetString("ErrorOccurred"), strError, ModuleMessage.ModuleMessageType.RedError);
            }

            return m;
        }
    }
}
