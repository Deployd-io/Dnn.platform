// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Razor
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.UI.Modules;
	using Microsoft.Extensions.Logging;

    /// <summary>A control factory for razor modules.</summary>
    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    public partial class RazorModuleControlFactory : BaseModuleControlFactory
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <inheritdoc />
        public override int Priority => 100;

        /// <inheritdoc />
        public override bool SupportsControl(ModuleInfo moduleConfiguration, string controlSrc)
        {
			_logger.LogDebug(">>> Entering SupportsControl(moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", moduleConfiguration, controlSrc);
			_logger.LogDebug("<<< Exiting SupportsControl(moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", moduleConfiguration, controlSrc);
            return controlSrc.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase) ||
                   controlSrc.EndsWith(".vbhtml", StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override Control CreateControl(TemplateControl containerControl, string controlKey, string controlSrc)
        {
			_logger.LogDebug(">>> Entering CreateControl(containerControl={ContainerControl},controlKey={ControlKey},controlSrc={ControlSrc})", containerControl, controlKey, controlSrc);
			_logger.LogDebug("<<< Exiting CreateControl(containerControl={ContainerControl},controlKey={ControlKey},controlSrc={ControlSrc})", containerControl, controlKey, controlSrc);
            return new RazorHostControl("~/" + controlSrc);
        }

        /// <inheritdoc />
        public override Control CreateModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration)
        {
			_logger.LogDebug(">>> Entering CreateModuleControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration})", containerControl, moduleConfiguration);
			_logger.LogDebug("<<< Exiting CreateModuleControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration})", containerControl, moduleConfiguration);
            return this.CreateControl(containerControl, string.Empty, moduleConfiguration.ModuleControl.ControlSrc);
        }

        /// <inheritdoc />
        public override Control CreateSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc)
        {
			_logger.LogDebug(">>> Entering CreateSettingsControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", containerControl, moduleConfiguration, controlSrc);
			_logger.LogDebug("<<< Exiting CreateSettingsControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", containerControl, moduleConfiguration, controlSrc);
            return this.CreateControl(containerControl, string.Empty, controlSrc);
        }
    }
}
