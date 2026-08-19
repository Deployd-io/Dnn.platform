// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel
{
    using DotNetNuke.Collections.Internal;

    internal class ComponentBuilderCollection : SharedDictionary<string, IComponentBuilder>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ComponentBuilderCollection));
        internal IComponentBuilder DefaultBuilder { get; set; }

        internal void AddBuilder(IComponentBuilder builder, bool setDefault)
        {
			Logger.LogDebug(">>> Entering AddBuilder(builder={Builder},setDefault={SetDefault})", builder, setDefault);
            if (!this.ContainsKey(builder.Name))
            {
                this[builder.Name] = builder;

                if (setDefault && this.DefaultBuilder == null)
                {
                    this.DefaultBuilder = builder;
					Logger.LogDebug("AddBuilder(builder={Builder},setDefault={SetDefault}): this.DefaultBuilder → {Arg2}", builder, setDefault, this.DefaultBuilder);
                }
            }
			Logger.LogDebug("<<< Exiting AddBuilder(builder={Builder},setDefault={SetDefault})", builder, setDefault);
        }
    }
}
