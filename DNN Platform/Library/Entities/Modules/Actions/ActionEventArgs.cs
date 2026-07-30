// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Actions
{
    using System;

    /// <summary>ActionEventArgs provides a custom EventARgs class for Action Events.</summary>
    public class ActionEventArgs : EventArgs
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly ModuleAction action;
        private readonly ModuleInfo moduleConfiguration;

        /// <summary>Initializes a new instance of the <see cref="ActionEventArgs"/> class.</summary>
        /// <param name="action">The action.</param>
        /// <param name="moduleConfiguration">The module info.</param>
        public ActionEventArgs(ModuleAction action, ModuleInfo moduleConfiguration)
        {
			Logger.LogDebug(">>> Entering ActionEventArgs(action={Action},moduleConfiguration={ModuleConfiguration})", action, moduleConfiguration);
            this.action = action;
            this.moduleConfiguration = moduleConfiguration;
			Logger.LogDebug("<<< Exiting ActionEventArgs(action={Action},moduleConfiguration={ModuleConfiguration})", action, moduleConfiguration);
			Logger.LogDebug("ActionEventArgs(action={Action},moduleConfiguration={ModuleConfiguration}): this.action → {Arg2}", action, moduleConfiguration, this.action);
			Logger.LogDebug("ActionEventArgs(action={Action},moduleConfiguration={ModuleConfiguration}): moduleConfiguration → {ModuleConfiguration1}", action, moduleConfiguration, moduleConfiguration);
			Logger.LogDebug("ActionEventArgs(action={Action},moduleConfiguration={ModuleConfiguration}): this.moduleConfiguration → {Arg2}", action, moduleConfiguration, this.moduleConfiguration);
			Logger.LogDebug("ActionEventArgs(action={Action},moduleConfiguration={ModuleConfiguration}): action → {Action1}", action, moduleConfiguration, action);
        }

        public ModuleAction Action
        {
            get
            {
                return this.action;
            }
        }

        public ModuleInfo ModuleConfiguration
        {
            get
            {
                return this.moduleConfiguration;
            }
        }
    }
}
