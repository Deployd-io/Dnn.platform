// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components
{
    using System;
    using System.Threading;

    using DotNetNuke.Framework;

    public class CloneModuleExecutionContext : ServiceLocator<ICloneModuleExecutionContext, CloneModuleExecutionContext>, ICloneModuleExecutionContext
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string CloneModuleSlotName = "CloneModuleContext";

        /// <inheritdoc />
        public void SetCloneModuleContext(bool cloneModule)
        {
			Logger.LogDebug(">>> Entering SetCloneModuleContext(cloneModule={CloneModule})", cloneModule);
            var slot = Thread.GetNamedDataSlot(CloneModuleSlotName);
            Thread.SetData(slot, cloneModule ? bool.TrueString : bool.FalseString);
			Logger.LogDebug("<<< Exiting SetCloneModuleContext(cloneModule={CloneModule})", cloneModule);
			Logger.LogDebug("SetCloneModuleContext(cloneModule={CloneModule}): slot → {Slot}", cloneModule, slot);
        }

        /// <inheritdoc />
        protected override Func<ICloneModuleExecutionContext> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new CloneModuleExecutionContext();
        }
    }
}
