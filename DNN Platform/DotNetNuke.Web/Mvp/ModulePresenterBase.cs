// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvp
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.Web.Validators;
    using WebFormsMvp;

    /// <summary>Represents a class that is a presenter for a module in a Web Forms Model-View-Presenter application.</summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    [DnnDeprecated(9, 2, 0, "Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
    public abstract partial class ModulePresenterBase<TView> : Presenter<TView>
        where TView : class, IModuleViewBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="ModulePresenterBase{TView}"/> class.</summary>
        /// <param name="view">The view.</param>
        protected ModulePresenterBase(TView view)
            : base(view)
        {
			Logger.LogDebug(">>> Entering ModulePresenterBase(view={View})", view);
            // Try and cast view to Control to get common control properties
            var control = view as Control;
            if (control != null && control.Page != null)
            {
                this.IsPostBack = control.Page.IsPostBack;
				Logger.LogDebug("ModulePresenterBase(view={View}): this.IsPostBack → {Arg1}", view, this.IsPostBack);
            }

            // Try and cast view to IModuleControl to get the Context
            var moduleControl = view as IModuleControl;
            if (moduleControl != null)
            {
                this.LocalResourceFile = moduleControl.LocalResourceFile;
				Logger.LogDebug("ModulePresenterBase(view={View}): this.LocalResourceFile → {Arg1}", view, this.LocalResourceFile);
                this.ModuleContext = moduleControl.ModuleContext;
            }
			Logger.LogDebug("ModulePresenterBase(view={View}): this.ModuleContext → {Arg1}", view, this.ModuleContext);

            this.Validator = new Validator(new DataAnnotationsObjectValidator());

            view.Initialize += this.InitializeInternal;
            view.Load += this.LoadInternal;
			Logger.LogDebug("<<< Exiting ModulePresenterBase(view={View})", view);
        }

        /// <summary>Gets or sets a value indicating whether to automatically data-bind the view.</summary>
        public bool AutoDataBind { get; set; }

        /// <summary>Gets or sets the module info.</summary>
        public ModuleInfo ModuleInfo { get; set; }

        /// <summary>Gets or sets a value indicating whether the view is editable.</summary>
        public bool IsEditable { get; set; }

        /// <summary>Gets or sets a value indicating whether the page is in a post-back.</summary>
        public bool IsPostBack { get; set; }

        /// <summary>Gets or sets a value indicating whether the current user is a superuser.</summary>
        public bool IsSuperUser { get; set; }

        /// <summary>Gets or sets the path to the resource file associated with the view.</summary>
        public string LocalResourceFile { get; set; }

        /// <summary>Gets or sets the module context.</summary>
        public ModuleInstanceContext ModuleContext { get; set; }

        /// <summary>Gets or sets the module ID.</summary>
        public int ModuleId { get; set; }

        /// <summary>Gets or sets the portal ID.</summary>
        public int PortalId { get; set; }

        /// <summary>Gets or sets the tab ID.</summary>
        public int TabId { get; set; }

        /// <summary>Gets or sets the user ID.</summary>
        public int UserId { get; set; }

        /// <summary>Gets or sets the settings.</summary>
        public Dictionary<string, string> Settings { get; set; }

        /// <summary>Gets or sets the validator.</summary>
        public Validator Validator { get; set; }

        /// <summary>Gets a value indicating whether the module allows anonymous access.</summary>
        protected internal virtual bool AllowAnonymousAccess
        {
            get
            {
                return true;
            }
        }

        /// <summary>Gets a value indicating whether the current user is authorized.</summary>
        protected internal virtual bool IsUserAuthorized
        {
            get
            {
                return true;
            }
        }

        /// <summary>Restores the view state.</summary>
        /// <param name="stateBag">The state bag.</param>
        public virtual void RestoreState(StateBag stateBag)
        {
			Logger.LogDebug(">>> Entering RestoreState(stateBag={StateBag})", stateBag);
            AttributeBasedViewStateSerializer.DeSerialize(this, stateBag);
			Logger.LogDebug("<<< Exiting RestoreState(stateBag={StateBag})", stateBag);
        }

        /// <summary>Saves the view state.</summary>
        /// <param name="stateBag">The state bag.</param>
        public virtual void SaveState(StateBag stateBag)
        {
			Logger.LogDebug(">>> Entering SaveState(stateBag={StateBag})", stateBag);
            AttributeBasedViewStateSerializer.Serialize(this, stateBag);
			Logger.LogDebug("<<< Exiting SaveState(stateBag={StateBag})", stateBag);
        }

        /// <summary>Checks the auth policy.</summary>
        /// <returns><see langword="true"/> if the current user is authorized, otherwise <see langword="false"/>.</returns>
        protected internal virtual bool CheckAuthPolicy()
        {
			Logger.LogDebug(">>> Entering CheckAuthPolicy()");
            if (this.UserId == Null.NullInteger && !this.AllowAnonymousAccess)
            {
                this.OnNoCurrentUser();
                return false;
            }

            if (!this.IsUserAuthorized)
            {
                this.OnUnauthorizedUser();
                return false;
            }

			Logger.LogDebug("<<< Exiting CheckAuthPolicy()");
            return true;
        }

        /// <summary>Sets the property values based on the <see cref="ModuleContext"/>.</summary>
        protected virtual void LoadFromContext()
        {
			Logger.LogDebug(">>> Entering LoadFromContext()");
            if (this.ModuleContext != null)
            {
                this.ModuleInfo = this.ModuleContext.Configuration;
				Logger.LogDebug("LoadFromContext(): this.ModuleInfo → {Arg0}", this.ModuleInfo);
				Logger.LogDebug("LoadFromContext(): this.UserId → {Arg0}", this.UserId);
                this.IsEditable = this.ModuleContext.IsEditable;
                this.IsSuperUser = this.ModuleContext.PortalSettings.UserInfo.IsSuperUser;
				Logger.LogDebug("LoadFromContext(): this.IsEditable → {Arg0}", this.IsEditable);
				Logger.LogDebug("LoadFromContext(): this.IsSuperUser → {Arg0}", this.IsSuperUser);
                this.ModuleId = this.ModuleContext.ModuleId;
                this.PortalId = this.ModuleContext.PortalId;
                this.Settings = new Dictionary<string, string>();
                foreach (object key in this.ModuleContext.Settings.Keys)
                {
				Logger.LogDebug("LoadFromContext(): this.ModuleId → {Arg0}", this.ModuleId);
				Logger.LogDebug("LoadFromContext(): this.PortalId → {Arg0}", this.PortalId);
                    this.Settings[key.ToString()] = (string)this.ModuleContext.Settings[key];
                }

                this.TabId = this.ModuleContext.TabId;
				Logger.LogDebug("LoadFromContext(): this.TabId → {Arg0}", this.TabId);
                this.UserId = this.ModuleContext.PortalSettings.UserInfo.UserID;
            }
			Logger.LogDebug("<<< Exiting LoadFromContext()");
        }

        /// <summary>Gets the localized string corresponding to the <paramref name="key"/>.</summary>
        /// <param name="key">The resource key to find.</param>
        /// <returns>The localized text.</returns>
        protected virtual string LocalizeString(string key)
        {
			Logger.LogDebug(">>> Entering LocalizeString(key={Key})", key);
            string localizedString;
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(this.LocalResourceFile))
            {
                localizedString = Localization.GetString(key, this.LocalResourceFile);
				Logger.LogDebug("LocalizeString(key={Key}): localizedString → {LocalizedString}", key, localizedString);
            }
            else
            {
                localizedString = Null.NullString;
            }

			Logger.LogDebug("<<< Exiting LocalizeString(key={Key})", key);
            return localizedString;
        }

        /// <summary>A method called when the initialize event is triggered.</summary>
        protected virtual void OnInit()
        {
		Logger.LogDebug(">>> Entering OnInit()");
		Logger.LogDebug("<<< Exiting OnInit()");
        }

        /// <summary>A method called when the load event is triggered.</summary>
        protected virtual void OnLoad()
        {
		Logger.LogDebug(">>> Entering OnLoad()");
		Logger.LogDebug("<<< Exiting OnLoad()");
        }

        /// <summary>A method called when there is no current user and the module does not support anonymous access.</summary>
        protected virtual void OnNoCurrentUser()
        {
			Logger.LogDebug(">>> Entering OnNoCurrentUser()");
            this.RedirectToLogin();
			Logger.LogDebug("<<< Exiting OnNoCurrentUser()");
        }

        /// <summary>A method called when the current user is not authorized for the module.</summary>
        protected virtual void OnUnauthorizedUser()
        {
			Logger.LogDebug(">>> Entering OnUnauthorizedUser()");
            this.RedirectToAccessDenied();
			Logger.LogDebug("<<< Exiting OnUnauthorizedUser()");
        }

        /// <summary>Redirects to the access denied page.</summary>
        protected void RedirectToAccessDenied()
        {
			Logger.LogDebug(">>> Entering RedirectToAccessDenied()");
            this.Response.Redirect(TestableGlobals.Instance.AccessDeniedURL(), true);
			Logger.LogDebug("<<< Exiting RedirectToAccessDenied()");
        }

        /// <summary>Redirects to the current page (without any query-string values).</summary>
        protected void RedirectToCurrentPage()
        {
			Logger.LogDebug(">>> Entering RedirectToCurrentPage()");
            this.Response.Redirect(TestableGlobals.Instance.NavigateURL(), true);
			Logger.LogDebug("<<< Exiting RedirectToCurrentPage()");
        }

        /// <summary>Redirects to the login page.</summary>
        protected void RedirectToLogin()
        {
			Logger.LogDebug(">>> Entering RedirectToLogin()");
            this.Response.Redirect(TestableGlobals.Instance.LoginURL(this.Request.RawUrl, false), true);
			Logger.LogDebug("<<< Exiting RedirectToLogin()");
        }

        /// <summary>Processes an exception from the module loading.</summary>
        /// <param name="ex">The exception to process.</param>
        protected void ProcessModuleLoadException(Exception ex)
        {
			Logger.LogDebug(">>> Entering ProcessModuleLoadException(ex={Ex})", ex);
            this.View.ProcessModuleLoadException(ex);
			Logger.LogDebug("<<< Exiting ProcessModuleLoadException(ex={Ex})", ex);
        }

        /// <summary>Show a module message.</summary>
        /// <param name="messageHeader">The message header.</param>
        /// <param name="message">The message text.</param>
        /// <param name="messageType">The message type.</param>
        protected void ShowMessage(string messageHeader, string message, ModuleMessage.ModuleMessageType messageType)
        {
			Logger.LogDebug(">>> Entering ShowMessage(messageHeader={MessageHeader},message={Message},messageType={MessageType})", messageHeader, message, messageType);
            this.ShowMessage(messageHeader, message, messageType, true);
			Logger.LogDebug("<<< Exiting ShowMessage(messageHeader={MessageHeader},message={Message},messageType={MessageType})", messageHeader, message, messageType);
			Logger.LogDebug(">>> Entering ShowMessage(message={Message},messageType={MessageType})", message, messageType);
			Logger.LogDebug("<<< Exiting ShowMessage(message={Message},messageType={MessageType})", message, messageType);
        }

        /// <summary>Show a module message.</summary>
        /// <param name="message">The message text.</param>
        /// <param name="messageType">The message type.</param>
        protected void ShowMessage(string message, ModuleMessage.ModuleMessageType messageType)
        {
            this.ShowMessage(message, messageType, true);
        }

        /// <summary>Show a module message.</summary>
        /// <param name="message">The message text.</param>
        /// <param name="messageType">The message type.</param>
        /// <param name="localize">Whether to localize the message.</param>
        protected void ShowMessage(string message, ModuleMessage.ModuleMessageType messageType, bool localize)
        {
			Logger.LogDebug(">>> Entering ShowMessage(message={Message},messageType={MessageType},localize={Localize})", message, messageType, localize);
            this.ShowMessage(string.Empty, message, messageType, localize);
			Logger.LogDebug("<<< Exiting ShowMessage(message={Message},messageType={MessageType},localize={Localize})", message, messageType, localize);
        }

        /// <summary>Show a module message.</summary>
        /// <param name="messageHeader">The message header.</param>
        /// <param name="message">The message text.</param>
        /// <param name="messageType">The message type.</param>
        /// <param name="localize">Whether to localize the message and header.</param>
        protected void ShowMessage(string messageHeader, string message, ModuleMessage.ModuleMessageType messageType, bool localize)
        {
			Logger.LogDebug(">>> Entering ShowMessage(messageHeader={MessageHeader},message={Message},messageType={MessageType},localize={Localize})", messageHeader, message, messageType, localize);
            if (!string.IsNullOrEmpty(message))
            {
                if (localize)
                {
                    messageHeader = this.LocalizeString(messageHeader);
                    message = this.LocalizeString(message);
					Logger.LogDebug("ShowMessage(messageHeader={MessageHeader},message={Message},messageType={MessageType},localize={Localize}): messageHeader → {MessageHeader1}", messageHeader, message, messageType, localize, messageHeader);
					Logger.LogDebug("ShowMessage(messageHeader={MessageHeader},message={Message},messageType={MessageType},localize={Localize}): message → {Message1}", messageHeader, message, messageType, localize, message);
                }

                this.View.ShowMessage(messageHeader, message, messageType);
            }
			Logger.LogDebug("<<< Exiting ShowMessage(messageHeader={MessageHeader},message={Message},messageType={MessageType},localize={Localize})", messageHeader, message, messageType, localize);
        }

        private void InitializeInternal(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering InitializeInternal(sender={Sender},e={E})", sender, e);
            this.LoadFromContext();
            this.OnInit();
			Logger.LogDebug("<<< Exiting InitializeInternal(sender={Sender},e={E})", sender, e);
        }

        private void LoadInternal(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering LoadInternal(sender={Sender},e={E})", sender, e);
            if (this.CheckAuthPolicy())
            {
                this.OnLoad();
            }
			Logger.LogDebug("<<< Exiting LoadInternal(sender={Sender},e={E})", sender, e);
        }
    }
}
