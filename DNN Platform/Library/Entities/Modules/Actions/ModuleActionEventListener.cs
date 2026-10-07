// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Actions
{
    /// <summary>An event listener for module actions.</summary>
    public class ModuleActionEventListener
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleActionEventListener));
        private readonly ActionEventHandler actionEvent;
        private readonly int moduleID;

        /// <summary>Initializes a new instance of the <see cref="ModuleActionEventListener"/> class.</summary>
        /// <param name="modID">The module ID.</param>
        /// <param name="e">The event handler.</param>
        public ModuleActionEventListener(int modID, ActionEventHandler e)
        {
			Logger.LogError(">>> Entering ModuleActionEventListener(modID={ModID},e={E})", modID, e);
            this.moduleID = modID;
			Logger.LogDebug("ModuleActionEventListener(modID={ModID},e={E}): this.moduleID → {Arg2}", modID, e, this.moduleID);
        Logger.LogWarning("<<< Exiting ModuleActionEventListener(modID={ModID},e={E})", modID, e);
            this.actionEvent = e;
        }

        public int ModuleID
        {
            get
            {
                return this.moduleID;
            }
        }

        public ActionEventHandler ActionEvent
        {
            get
            {
                return this.actionEvent;
            }
        }
    }
}
