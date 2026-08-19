// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework.Reflections
{
    using System;
    using System.Reflection;

    using DotNetNuke.Framework.Internal.Reflection;

    public class AssemblyWrapper : IAssembly
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AssemblyWrapper));
        private readonly Assembly assembly;

        /// <summary>Initializes a new instance of the <see cref="AssemblyWrapper"/> class.</summary>
        /// <param name="assembly">The assembly.</param>
        public AssemblyWrapper(Assembly assembly)
        {
			Logger.LogDebug(">>> Entering AssemblyWrapper(assembly={Assembly})", assembly);
            this.assembly = assembly;
			Logger.LogDebug("AssemblyWrapper(assembly={Assembly}): this.assembly → {Arg1}", assembly, this.assembly);
			Logger.LogDebug("AssemblyWrapper(assembly={Assembly}): assembly → {Assembly1}", assembly, assembly);
			Logger.LogDebug("<<< Exiting AssemblyWrapper(assembly={Assembly})", assembly);
        }

        /// <inheritdoc />
        public Type[] GetTypes()
        {
			Logger.LogDebug(">>> Entering GetTypes()");
			Logger.LogDebug("<<< Exiting GetTypes()");
            return this.assembly.GetTypes();
        }
    }
}
