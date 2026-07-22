// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel
{
    using System;

    internal sealed class ComponentType : IDisposable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="ComponentType"/> class.</summary>
        /// <param name="baseType">The base type of Components of this ComponentType.</param>
        public ComponentType(Type baseType)
        {
			Logger.LogDebug(">>> Entering ComponentType(baseType={BaseType})", baseType);
            this.BaseType = baseType;
			Logger.LogDebug("<<< Exiting ComponentType(baseType={BaseType})", baseType);
			Logger.LogDebug("ComponentType(baseType={BaseType}): this.BaseType → {Arg1}", baseType, this.BaseType);
        }

        public Type BaseType { get; }

        public ComponentBuilderCollection ComponentBuilders { get; } = new ComponentBuilderCollection();

        /// <inheritdoc />
        public void Dispose()
        {
			Logger.LogDebug(">>> Entering Dispose()");
            this.ComponentBuilders?.Dispose();
			Logger.LogDebug("<<< Exiting Dispose()");
        }
    }
}
