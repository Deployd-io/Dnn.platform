// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ComponentModel
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    internal class InstanceComponentBuilder : IComponentBuilder
    {
        private readonly object instance;
        private readonly string name;

        /// <summary>Initializes a new instance of the <see cref="InstanceComponentBuilder"/> class.</summary>
        /// <param name="name">The component name.</param>
        /// <param name="instance">The component instance.</param>
        public InstanceComponentBuilder(string name, object instance)
        {
			Logger.LogDebug(">>> Entering InstanceComponentBuilder(name={Name},instance={Instance})", name, instance);
            this.name = name;
            this.instance = instance;
			Logger.LogDebug("<<< Exiting InstanceComponentBuilder(name={Name},instance={Instance})", name, instance);
			Logger.LogDebug("InstanceComponentBuilder(name={Name},instance={Instance}): this.name → {Arg2}", name, instance, this.name);
			Logger.LogDebug("InstanceComponentBuilder(name={Name},instance={Instance}): instance → {Instance1}", name, instance, instance);
			Logger.LogDebug("InstanceComponentBuilder(name={Name},instance={Instance}): this.instance → {Arg2}", name, instance, this.instance);
        }

        /// <inheritdoc />
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <inheritdoc />
        public object BuildComponent()
        {
			Logger.LogDebug(">>> Entering BuildComponent()");
			Logger.LogDebug("<<< Exiting BuildComponent()");
            return this.instance;
        }
    }
}
