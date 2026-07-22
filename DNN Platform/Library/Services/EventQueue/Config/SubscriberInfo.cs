// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.EventQueue.Config
{
    using System;

    using DotNetNuke.Security;

    [Serializable]
    public class SubscriberInfo
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="SubscriberInfo"/> class.</summary>
        public SubscriberInfo()
        {
			Logger.LogDebug(">>> Entering SubscriberInfo()");
            this.ID = Guid.NewGuid().ToString();
            this.Name = string.Empty;
			Logger.LogDebug("SubscriberInfo(): this.Name → {Arg0}", this.Name);
            this.Description = string.Empty;
            this.Address = string.Empty;
			Logger.LogDebug("SubscriberInfo(): this.Description → {Arg0}", this.Description);
			Logger.LogDebug("SubscriberInfo(): this.Address → {Arg0}", this.Address);
            var oPortalSecurity = PortalSecurity.Instance;
            this.PrivateKey = oPortalSecurity.CreateKey(16);
			Logger.LogDebug("<<< Exiting SubscriberInfo()");
			Logger.LogDebug("SubscriberInfo(subscriberName={SubscriberName}): this.Name → {Arg1}", subscriberName, this.Name);
        }

        /// <summary>Initializes a new instance of the <see cref="SubscriberInfo"/> class.</summary>
        /// <param name="subscriberName">The subscriber name.</param>
        public SubscriberInfo(string subscriberName)
            : this()
        {
            this.Name = subscriberName;
        }

        public string ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Address { get; set; }

        public string PrivateKey { get; set; }
    }
}
