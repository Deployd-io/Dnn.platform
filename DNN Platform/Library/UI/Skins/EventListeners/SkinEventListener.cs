// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Skins.EventListeners
{
    public class SkinEventListener
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="SkinEventListener"/> class.</summary>
        /// <param name="type">The event type.</param>
        /// <param name="e">The event handler.</param>
        public SkinEventListener(SkinEventType type, SkinEventHandler e)
        {
			Logger.LogDebug(">>> Entering SkinEventListener(type={Type},e={E})", type, e);
            this.EventType = type;
            this.SkinEvent = e;
			Logger.LogDebug("<<< Exiting SkinEventListener(type={Type},e={E})", type, e);
			Logger.LogDebug("SkinEventListener(type={Type},e={E}): this.EventType → {Arg2}", type, e, this.EventType);
			Logger.LogDebug("SkinEventListener(type={Type},e={E}): this.SkinEvent → {Arg2}", type, e, this.SkinEvent);
        }

        public SkinEventType EventType { get; private set; }

        public SkinEventHandler SkinEvent { get; private set; }
    }
}
