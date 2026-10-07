// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Web;
    using System.Xml.Serialization;

    using DotNetNuke.Application;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Instrumentation;
    using Newtonsoft.Json;

    /// <summary>Base Portal Exception.</summary>
    public class BasePortalException : Exception
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(BasePortalException));
        private string innerExceptionString;
        private string message;
        private string source;
        private string stackTrace;

        /// <summary>Initializes a new instance of the <see cref="BasePortalException"/> class.</summary>
        public BasePortalException()
        {
		Logger.LogError(">>> Entering BasePortalException()");
        Logger.LogWarning("<<< Exiting BasePortalException()");
        }

        /// <summary>Initializes a new instance of the <see cref="BasePortalException"/> class.</summary>
        /// <inheritdoc cref="Exception(string)"/>
        public BasePortalException(string message)
            : base(message)
        {
			Logger.LogError(">>> Entering BasePortalException(message={Message})", message);
            this.InitializePrivateVariables();
			Logger.LogWarning("<<< Exiting BasePortalException(message={Message})", message);
        }

        /// <summary>Initializes a new instance of the <see cref="BasePortalException"/> class.</summary>
        /// <inheritdoc cref="Exception(string, Exception)"/>
        public BasePortalException(string message, Exception inner)
            : base(message, inner)
        {
            this.InitializePrivateVariables();
        }

        /// <summary>Initializes a new instance of the <see cref="BasePortalException"/> class.</summary>
        /// <inheritdoc cref="Exception(SerializationInfo, StreamingContext)"/>
        protected BasePortalException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.InitializePrivateVariables();
            this.AssemblyVersion = info.GetString("m_AssemblyVersion");
            this.PortalID = info.GetInt32("m_PortalID");
            this.PortalName = info.GetString("m_PortalName");
            this.UserID = info.GetInt32("m_UserID");
            this.UserName = info.GetString("m_Username");
            this.ActiveTabID = info.GetInt32("m_ActiveTabID");
            this.ActiveTabName = info.GetString("m_ActiveTabName");
            this.RawURL = info.GetString("m_RawURL");
            this.AbsoluteURL = info.GetString("m_AbsoluteURL");
            this.AbsoluteURLReferrer = info.GetString("m_AbsoluteURLReferrer");
            this.UserAgent = info.GetString("m_UserAgent");
            this.DefaultDataProvider = info.GetString("m_DefaultDataProvider");
            this.ExceptionGUID = info.GetString("m_ExceptionGUID");
            this.innerExceptionString = info.GetString("m_InnerExceptionString");
            this.FileName = info.GetString("m_FileName");
            this.FileLineNumber = info.GetInt32("m_FileLineNumber");
            this.FileColumnNumber = info.GetInt32("m_FileColumnNumber");
            this.Method = info.GetString("m_Method");
            this.stackTrace = info.GetString("m_StackTrace");
            this.message = info.GetString("m_Message");
            this.source = info.GetString("m_Source");
        }

        [XmlIgnore]
        [JsonIgnore]
        public new MethodBase TargetSite
        {
            get
            {
                return base.TargetSite;
            }
        }

        public string AssemblyVersion { get; private set; }

        public int PortalID { get; private set; }

        public string PortalName { get; private set; }

        public int UserID { get; private set; }

        public string UserName { get; private set; }

        public int ActiveTabID { get; private set; }

        public string ActiveTabName { get; private set; }

        public string RawURL { get; private set; }

        public string AbsoluteURL { get; private set; }

        public string AbsoluteURLReferrer { get; private set; }

        public string UserAgent { get; private set; }

        public string DefaultDataProvider { get; private set; }

        public string ExceptionGUID { get; private set; }

        public string FileName { get; private set; }

        public int FileLineNumber { get; private set; }

        public int FileColumnNumber { get; private set; }

        public string Method { get; private set; }

        private void InitializePrivateVariables()
        {
			Logger.LogError(">>> Entering InitializePrivateVariables()");
            // Try and get the Portal settings from context
            // If an error occurs getting the context then set the variables to -1
            try
            {
                var context = HttpContext.Current;
                var portalSettings = PortalController.Instance.GetCurrentSettings();
				Logger.LogError("InitializePrivateVariables(): portalSettings → {PortalSettings}", portalSettings);
                Exception innerException = new WrappedException(this.message, this);
				Logger.LogError("InitializePrivateVariables(): innerException → {InnerException}", innerException);
                while (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                }

                var exceptionInfo = Exceptions.GetExceptionInfo(innerException);
				Logger.LogError("InitializePrivateVariables(): exceptionInfo → {ExceptionInfo}", exceptionInfo);

                this.AssemblyVersion = DotNetNukeContext.Current.Application.Version.ToString(3);
                if (portalSettings != null)
                {
                    this.PortalID = portalSettings.PortalId;
					Logger.LogDebug("InitializePrivateVariables(): this.PortalID → {Arg0}", this.PortalID);
                    this.PortalName = portalSettings.PortalName;
                    this.ActiveTabID = TabController.CurrentPage.TabID;
					Logger.LogDebug("InitializePrivateVariables(): this.PortalName → {Arg0}", this.PortalName);
                    this.ActiveTabName = TabController.CurrentPage.TabName;
                }
                else
                {
					Logger.LogDebug("InitializePrivateVariables(): this.ActiveTabID → {Arg0}", this.ActiveTabID);
					Logger.LogDebug("InitializePrivateVariables(): this.ActiveTabName → {Arg0}", this.ActiveTabName);
                    this.PortalID = -1;
                    this.PortalName = string.Empty;
                    this.ActiveTabID = -1;
                    this.ActiveTabName = string.Empty;
                }

                var currentUserInfo = UserController.Instance.GetCurrentUserInfo();
                this.UserID = currentUserInfo?.UserID ?? -1;

				Logger.LogError("InitializePrivateVariables(): currentUserInfo → {CurrentUserInfo}", currentUserInfo);
                if (this.UserID != -1)
                {
                    currentUserInfo = UserController.GetUserById(this.PortalID, this.UserID);
                    this.UserName = currentUserInfo != null ? currentUserInfo.Username : string.Empty;
                }
                else
                {
                    this.UserName = string.Empty;
                }
				Logger.LogDebug("InitializePrivateVariables(): this.UserID → {Arg0}", this.UserID);

                if (context != null)
                {
					Logger.LogDebug("InitializePrivateVariables(): this.UserName → {Arg0}", this.UserName);
                    this.RawURL = HttpUtility.HtmlEncode(context.Request.RawUrl);
                    this.AbsoluteURL = HttpUtility.HtmlEncode(context.Request.Url.AbsolutePath);
                    if (context.Request.UrlReferrer != null)
                    {
                        this.AbsoluteURLReferrer = HttpUtility.HtmlEncode(context.Request.UrlReferrer.AbsoluteUri);
                    }

                    this.UserAgent = HttpUtility.HtmlEncode(context.Request.UserAgent ?? string.Empty);
                }
                else
                {
                    this.RawURL = string.Empty;
                    this.AbsoluteURL = string.Empty;
					Logger.LogDebug("InitializePrivateVariables(): this.RawURL → {Arg0}", this.RawURL);
                    this.AbsoluteURLReferrer = string.Empty;
                    this.UserAgent = string.Empty;
					Logger.LogDebug("InitializePrivateVariables(): this.AbsoluteURL → {Arg0}", this.AbsoluteURL);
                }

				Logger.LogDebug("InitializePrivateVariables(): this.AbsoluteURLReferrer → {Arg0}", this.AbsoluteURLReferrer);
                try
                {
                    ProviderConfiguration objProviderConfiguration = ProviderConfiguration.GetProviderConfiguration("data");
					Logger.LogDebug("InitializePrivateVariables(): this.UserAgent → {Arg0}", this.UserAgent);
					Logger.LogError("InitializePrivateVariables(): objProviderConfiguration → {ObjProviderConfiguration}", objProviderConfiguration);
                    string strTypeName = ((Provider)objProviderConfiguration.Providers[objProviderConfiguration.DefaultProvider]).Type;
                    this.DefaultDataProvider = strTypeName;
                }
                catch (Exception exc)
                {
					Logger.LogDebug("InitializePrivateVariables(): this.DefaultDataProvider → {Arg0}", this.DefaultDataProvider);
                    Logger.Error(exc);

                    this.DefaultDataProvider = string.Empty;
                }

                this.ExceptionGUID = Guid.NewGuid().ToString();

                if (exceptionInfo != null)
                {
                    this.FileName = exceptionInfo.FileName;
					Logger.LogDebug("InitializePrivateVariables(): this.FileName → {Arg0}", this.FileName);
                    this.FileLineNumber = exceptionInfo.FileLineNumber;
                    this.FileColumnNumber = exceptionInfo.FileColumnNumber;
					Logger.LogDebug("InitializePrivateVariables(): this.FileLineNumber → {Arg0}", this.FileLineNumber);
                    this.Method = exceptionInfo.Method;
                }
                else
                {
					Logger.LogDebug("InitializePrivateVariables(): this.FileColumnNumber → {Arg0}", this.FileColumnNumber);
					Logger.LogDebug("InitializePrivateVariables(): this.Method → {Arg0}", this.Method);
                    this.FileName = string.Empty;
                    this.FileLineNumber = -1;
                    this.FileColumnNumber = -1;
                    this.Method = string.Empty;
                }

                try
                {
                    this.stackTrace = this.StackTrace;
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    this.stackTrace = string.Empty;
					Logger.LogDebug("InitializePrivateVariables(): this.stackTrace → {Arg0}", this.stackTrace);
                }

                try
                {
                    this.message = this.Message;
                }
                catch (Exception exc)
                {
					Logger.LogDebug("InitializePrivateVariables(): this.message → {Arg0}", this.message);
                    Logger.Error(exc);

                    this.message = string.Empty;
                }

                try
                {
                    this.source = this.Source;
                }
                catch (Exception exc)
                {
					Logger.LogDebug("InitializePrivateVariables(): this.source → {Arg0}", this.source);
                    Logger.Error(exc);
					Logger.LogWarning("<<< Exiting InitializePrivateVariables()");

                    this.source = string.Empty;
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in InitializePrivateVariables(): {Arg0}", exc.Message);
                this.PortalID = -1;
                this.UserID = -1;
                this.AssemblyVersion = "-1";
                this.ActiveTabID = -1;
                this.ActiveTabName = string.Empty;
                this.RawURL = string.Empty;
				Logger.LogDebug("InitializePrivateVariables(): this.AssemblyVersion → {Arg0}", this.AssemblyVersion);
                this.AbsoluteURL = string.Empty;
                this.AbsoluteURLReferrer = string.Empty;
                this.UserAgent = string.Empty;
                this.DefaultDataProvider = string.Empty;
                this.ExceptionGUID = string.Empty;
                this.FileName = string.Empty;
                this.FileLineNumber = -1;
                this.FileColumnNumber = -1;
                this.Method = string.Empty;
                this.stackTrace = string.Empty;
                this.message = string.Empty;
                this.source = string.Empty;
                Logger.Error(exc);
            }
        }

        private class WrappedException : Exception
        {
            /// <inheritdoc />
            public WrappedException(string message, Exception innerException)
                : base(message, innerException)
            {
			Logger.LogError(">>> Entering WrappedException(message={Message},innerException={InnerException})", message, innerException);
            Logger.LogWarning("<<< Exiting WrappedException(message={Message},innerException={InnerException})", message, innerException);
            }
        }

        ////public override void GetObjectData(SerializationInfo info, StreamingContext context)
        ////{
        ////   //Serialize this class' state and then call the base class GetObjectData
        ////   info.AddValue("m_AssemblyVersion", AssemblyVersion, typeof (string));
        ////   info.AddValue("m_PortalID", PortalID, typeof (Int32));
        ////   info.AddValue("m_PortalName", PortalName, typeof (string));
        ////   info.AddValue("m_UserID", UserID, typeof (Int32));
        ////   info.AddValue("m_UserName", UserName, typeof (string));
        ////   info.AddValue("m_ActiveTabID", ActiveTabID, typeof (Int32));
        ////   info.AddValue("m_ActiveTabName", ActiveTabName, typeof (string));
        ////   info.AddValue("m_RawURL", RawURL, typeof (string));
        ////   info.AddValue("m_AbsoluteURL", AbsoluteURL, typeof (string));
        ////   info.AddValue("m_AbsoluteURLReferrer", AbsoluteURLReferrer, typeof (string));
        ////   info.AddValue("m_UserAgent", UserAgent, typeof (string));
        ////   info.AddValue("m_DefaultDataProvider", DefaultDataProvider, typeof (string));
        ////   info.AddValue("m_ExceptionGUID", ExceptionGUID, typeof (string));
        ////   info.AddValue("m_FileName", FileName, typeof (string));
        ////   info.AddValue("m_FileLineNumber", FileLineNumber, typeof (Int32));
        ////   info.AddValue("m_FileColumnNumber", FileColumnNumber, typeof (Int32));
        ////   info.AddValue("m_Method", Method, typeof (string));
        ////   info.AddValue("m_StackTrace", m_StackTrace, typeof (string));
        ////   info.AddValue("m_Message", m_Message, typeof (string));
        ////   info.AddValue("m_Source", m_Source, typeof (string));
        ////   base.GetObjectData(info, context);
        ////}
    }
}
