// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.Http;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    /// <summary>Defines properties and methods for DNN-specific API controllers.</summary>
    [DnnExceptionFilter]
    public abstract class DnnApiController : ApiController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnApiController));
        private readonly Lazy<ModuleInfo> activeModule;

        /// <summary>Initializes a new instance of the <see cref="DnnApiController"/> class.</summary>
        protected DnnApiController()
        {
			Logger.LogError(">>> Entering DnnApiController()");
            this.activeModule = new Lazy<ModuleInfo>(this.InitModuleInfo);
			Logger.LogDebug("DnnApiController(): activeModule → {ActiveModule}", activeModule);
        Logger.LogWarning("<<< Exiting DnnApiController()");
        }

        /// <summary>Gets portalSettings for the current portal.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public PortalSettings PortalSettings => PortalSettings.Current;

        /// <summary>Gets userInfo for the current user.</summary>
        public UserInfo UserInfo => this.PortalSettings.UserInfo;

        /// <summary>Gets moduleInfo for the current module.</summary>
        /// <remarks>Will be <see langword="null"/> unless a valid pair of module and tab IDs were provided in the request.</remarks>
        public ModuleInfo ActiveModule => this.activeModule.Value;

        private ModuleInfo InitModuleInfo()
        {
			Logger.LogError(">>> Entering InitModuleInfo()");
            Logger.LogWarning("<<< Exiting InitModuleInfo()");
            return this.Request.FindModuleInfo();
        }
    }
}
