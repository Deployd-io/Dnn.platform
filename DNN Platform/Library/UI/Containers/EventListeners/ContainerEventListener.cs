// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Containers.EventListeners
{
    public class ContainerEventListener
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly ContainerEventHandler containerEvent;
        private readonly ContainerEventType type;

        /// <summary>Initializes a new instance of the <see cref="ContainerEventListener"/> class.</summary>
        /// <param name="type">The event type.</param>
        /// <param name="e">The event handler.</param>
        public ContainerEventListener(ContainerEventType type, ContainerEventHandler e)
        {
			Logger.LogDebug(">>> Entering ContainerEventListener(type={Type},e={E})", type, e);
            this.type = type;
            this.containerEvent = e;
			Logger.LogDebug("<<< Exiting ContainerEventListener(type={Type},e={E})", type, e);
			Logger.LogDebug("ContainerEventListener(type={Type},e={E}): this.type → {Arg2}", type, e, this.type);
			Logger.LogDebug("ContainerEventListener(type={Type},e={E}): containerEvent → {ContainerEvent}", type, e, containerEvent);
			Logger.LogDebug("ContainerEventListener(type={Type},e={E}): this.containerEvent → {Arg2}", type, e, this.containerEvent);
			Logger.LogDebug("ContainerEventListener(type={Type},e={E}): type → {Type1}", type, e, type);
        }

        public ContainerEventType EventType
        {
            get
            {
                return this.type;
            }
        }

        public ContainerEventHandler ContainerEvent
        {
            get
            {
                return this.containerEvent;
            }
        }
    }
}
