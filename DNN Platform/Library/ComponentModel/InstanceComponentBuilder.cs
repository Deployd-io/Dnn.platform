// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ComponentModel
{
    internal class InstanceComponentBuilder : IComponentBuilder
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(InstanceComponentBuilder));
        private readonly object instance;
        private readonly string name;

        /// <summary>Initializes a new instance of the <see cref="InstanceComponentBuilder"/> class.</summary>
        /// <param name="name">The component name.</param>
        /// <param name="instance">The component instance.</param>
        public InstanceComponentBuilder(string name, object instance)
        {
			Logger.LogError(">>> Entering InstanceComponentBuilder(name={Name},instance={Instance})", name, instance);
			Logger.LogDebug("InstanceComponentBuilder(name={Name},instance={Instance}): instance → {Instance1}", name, instance, instance);
            this.name = name;
            this.instance = instance;
			Logger.LogDebug("InstanceComponentBuilder(name={Name},instance={Instance}): this.name → {Arg2}", name, instance, this.name);
        Logger.LogWarning("<<< Exiting InstanceComponentBuilder(name={Name},instance={Instance})", name, instance);
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
			Logger.LogError(">>> Entering BuildComponent()");
            Logger.LogWarning("<<< Exiting BuildComponent()");
            return this.instance;
        }
    }
}
