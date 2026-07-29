// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.ClientCapability
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using DotNetNuke.ComponentModel;

    public abstract class ClientCapabilityProvider : IClientCapabilityProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static IClientCapability CurrentClientCapability
        {
            get
            {
                return Instance().GetClientCapability(HttpContext.Current.Request);
            }
        }

        /// <summary>Gets a value indicating whether this provider supports detecting if the device is a tablet.</summary>
        public virtual bool SupportsTabletDetection => true;

        public static ClientCapabilityProvider Instance()
        {
			Logger.LogDebug(">>> Entering Instance()");
			Logger.LogDebug("<<< Exiting Instance()");
            return ComponentFactory.GetComponent<ClientCapabilityProvider>();
        }

        /// <inheritdoc />
        public abstract IClientCapability GetClientCapability(string userAgent);

        /// <inheritdoc />
        public abstract IClientCapability GetClientCapabilityById(string clientId);

        /// <inheritdoc />
        public abstract IDictionary<string, List<string>> GetAllClientCapabilityValues();

        /// <inheritdoc />
        public abstract IQueryable<IClientCapability> GetAllClientCapabilities();

        /// <inheritdoc />
        public virtual IClientCapability GetClientCapability(HttpRequest httpRequest)
        {
			Logger.LogDebug(">>> Entering GetClientCapability(httpRequest={HttpRequest})", httpRequest);
            IClientCapability clientCapability = this.GetClientCapability(httpRequest.UserAgent);
            clientCapability.FacebookRequest = FacebookRequestController.GetFacebookDetailsFromRequest(httpRequest);
			Logger.LogDebug("GetClientCapability(httpRequest={HttpRequest}): clientCapability → {ClientCapability}", httpRequest, clientCapability);

			Logger.LogDebug("<<< Exiting GetClientCapability(httpRequest={HttpRequest})", httpRequest);
            return clientCapability;
        }
    }
}
