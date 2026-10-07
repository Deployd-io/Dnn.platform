// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers.EventListeners
{
    using System;

    /// <summary>ContainerEventArgs provides a custom EventARgs class for Container Events.</summary>
    public class ContainerEventArgs : EventArgs
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ContainerEventArgs));
        private readonly Container container;

        /// <summary>Initializes a new instance of the <see cref="ContainerEventArgs"/> class.</summary>
        /// <param name="container">The container.</param>
        public ContainerEventArgs(Container container)
        {
			Logger.LogError(">>> Entering ContainerEventArgs(container={Container})", container);
            this.container = container;
			Logger.LogDebug("ContainerEventArgs(container={Container}): container → {Container1}", container, container);
        Logger.LogDebug("ContainerEventArgs(container={Container}): this.container → {Arg1}", container, this.container);
        Logger.LogWarning("<<< Exiting ContainerEventArgs(container={Container})", container);
        }

        public Container Container
        {
            get
            {
                return this.container;
            }
        }
    }
}
