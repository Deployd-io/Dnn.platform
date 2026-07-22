// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Authentication
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Common;

    /// <summary>
    /// The AuthenticationConfigBase class provides base configuration class for the
    /// Authentication providers.
    /// </summary>
    [Serializable]
    public abstract class AuthenticationConfigBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="AuthenticationConfigBase"/> class.</summary>
        public AuthenticationConfigBase()
        {
		Logger.LogDebug(">>> Entering AuthenticationConfigBase()");
		Logger.LogDebug("<<< Exiting AuthenticationConfigBase()");
        }

        /// <summary>Initializes a new instance of the <see cref="AuthenticationConfigBase"/> class.</summary>
        /// <param name="portalID">The portal ID.</param>
        protected AuthenticationConfigBase(int portalID)
            : this()
        {
            this.PortalID = portalID;
			Logger.LogDebug("AuthenticationConfigBase(portalID={PortalID}): this.PortalID → {Arg1}", portalID, this.PortalID);
        }

        [Browsable(false)]
        public int PortalID { get; set; }

        /// <summary>Gets the Dependency Provider to resolve registered services with the container.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected IServiceProvider DependencyProvider => Globals.GetCurrentServiceProvider();
    }
}
