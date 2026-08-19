// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules
{
    using System;
    using System.IO;
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules.Html5;

    /// <inheritdoc cref="IModuleControlPipeline" />
    [DnnDeprecated(9, 4, 0, "This implementation has moved to DotNetNuke.ModulePipeline.ModuleControlPipeline")]
    public partial class ModuleControlFactory
    {
        private static readonly ILog TracelLogger = LoggerSource.Instance.GetLogger("DNN.Trace");

        /// <inheritdoc cref="IModuleControlPipeline.LoadModuleControl(TemplateControl,ModuleInfo,string,string)" />
        [DnnDeprecated(9, 4, 0, "This implementation has moved to DotNetNuke.ModulePipeline.ModuleControlPipeline")]
        public static partial Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlKey, string controlSrc)
        {
			TracelLogger.LogDebug(">>> Entering LoadModuleControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration},controlKey={ControlKey},controlSrc={ControlSrc})", containerControl, moduleConfiguration, controlKey, controlSrc);
            if (TracelLogger.IsDebugEnabled)
            {
                TracelLogger.Debug($"ModuleControlFactory.LoadModuleControl Start (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

            Control control = null;
			TracelLogger.LogDebug("LoadModuleControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration},controlKey={ControlKey},controlSrc={ControlSrc}): control → {Control}", containerControl, moduleConfiguration, controlKey, controlSrc, control);
            IModuleControlFactory controlFactory = GetModuleControlFactory(controlSrc);
			TracelLogger.LogDebug("LoadModuleControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration},controlKey={ControlKey},controlSrc={ControlSrc}): controlFactory → {ControlFactory}", containerControl, moduleConfiguration, controlKey, controlSrc, controlFactory);

            if (controlFactory != null)
            {
                control = controlFactory.CreateControl(containerControl, controlKey, controlSrc);
            }

            // set the control ID to the resource file name ( ie. controlname.ascx = controlname )
            // this is necessary for the Localization in PageBase
            if (control != null)
            {
                control.ID = Path.GetFileNameWithoutExtension(controlSrc);

                var moduleControl = control as IModuleControl;

                if (moduleControl != null)
                {
                    moduleControl.ModuleContext.Configuration = moduleConfiguration;
                }
            }

            if (TracelLogger.IsDebugEnabled)
            {
                TracelLogger.Debug($"ModuleControlFactory.LoadModuleControl End (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

			TracelLogger.LogDebug("<<< Exiting LoadModuleControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration},controlKey={ControlKey},controlSrc={ControlSrc})", containerControl, moduleConfiguration, controlKey, controlSrc);
			TracelLogger.LogDebug("<<< Exiting LoadModuleControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration})", containerControl, moduleConfiguration);
            return control;
        }

        /// <inheritdoc cref="IModuleControlPipeline.LoadModuleControl(TemplateControl,ModuleInfo)" />
        [DnnDeprecated(9, 4, 0, "This implementation has moved to DotNetNuke.ModulePipeline.ModuleControlPipeline")]
        public static partial Control LoadModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration)
        {
            if (TracelLogger.IsDebugEnabled)
            {
                TracelLogger.Debug($"ModuleControlFactory.LoadModuleControl Start (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

            Control control = null;
            IModuleControlFactory controlFactory = GetModuleControlFactory(moduleConfiguration.ModuleControl.ControlSrc);

            if (controlFactory != null)
            {
                control = controlFactory.CreateModuleControl(containerControl, moduleConfiguration);
            }

            // set the control ID to the resource file name ( ie. controlname.ascx = controlname )
            // this is necessary for the Localization in PageBase
            if (control != null)
            {
                control.ID = Path.GetFileNameWithoutExtension(moduleConfiguration.ModuleControl.ControlSrc);

                var moduleControl = control as IModuleControl;

                if (moduleControl != null)
                {
                    moduleControl.ModuleContext.Configuration = moduleConfiguration;
                }
            }

            if (TracelLogger.IsDebugEnabled)
            {
                TracelLogger.Debug($"ModuleControlFactory.LoadModuleControl End (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

            return control;
        }

        /// <inheritdoc cref="IModuleControlPipeline.LoadSettingsControl" />
        [DnnDeprecated(9, 4, 0, "This implementation has moved to DotNetNuke.ModulePipeline.ModuleControlPipeline")]
        public static partial Control LoadSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc)
        {
			TracelLogger.LogDebug(">>> Entering LoadSettingsControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", containerControl, moduleConfiguration, controlSrc);
            if (TracelLogger.IsDebugEnabled)
            {
                TracelLogger.Debug($"ModuleControlFactory.LoadSettingsControl Start (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

            Control control = null;
			TracelLogger.LogDebug("LoadSettingsControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc}): control → {Control}", containerControl, moduleConfiguration, controlSrc, control);
            IModuleControlFactory controlFactory = GetModuleControlFactory(controlSrc);
			TracelLogger.LogDebug("LoadSettingsControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc}): controlFactory → {ControlFactory}", containerControl, moduleConfiguration, controlSrc, controlFactory);

            if (controlFactory != null)
            {
                control = controlFactory.CreateSettingsControl(containerControl, moduleConfiguration, controlSrc);
            }

            // set the control ID to the resource file name ( ie. controlname.ascx = controlname )
            // this is necessary for the Localization in PageBase
            if (control != null)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(controlSrc);
				TracelLogger.LogDebug("LoadSettingsControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc}): fileNameWithoutExtension → {FileNameWithoutExtension}", containerControl, moduleConfiguration, controlSrc, fileNameWithoutExtension);
                if (fileNameWithoutExtension != null)
                {
                    control.ID = fileNameWithoutExtension.Replace('.', '-');
                }

                var settingsControl = control as ISettingsControl;

                if (settingsControl != null)
                {
                    settingsControl.ModuleContext.Configuration = moduleConfiguration;
                }
            }

            if (TracelLogger.IsDebugEnabled)
            {
                TracelLogger.Debug($"ModuleControlFactory.LoadSettingsControl End (TabId:{moduleConfiguration.TabID},ModuleId:{moduleConfiguration.ModuleID}): ModuleControlSource:{moduleConfiguration.ModuleControl.ControlSrc}");
            }

			TracelLogger.LogDebug("<<< Exiting LoadSettingsControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", containerControl, moduleConfiguration, controlSrc);
            return control;
        }

        /// <inheritdoc cref="IModuleControlPipeline.CreateCachedControl" />
        [DnnDeprecated(9, 4, 0, "This implementation has moved to DotNetNuke.ModulePipeline.ModuleControlPipeline")]
        public static partial Control CreateCachedControl(string cachedContent, ModuleInfo moduleConfiguration)
        {
			TracelLogger.LogDebug(">>> Entering CreateCachedControl(cachedContent={CachedContent},moduleConfiguration={ModuleConfiguration})", cachedContent, moduleConfiguration);
            var moduleControl = new CachedModuleControl(cachedContent);
            moduleControl.ModuleContext.Configuration = moduleConfiguration;
			TracelLogger.LogDebug("CreateCachedControl(cachedContent={CachedContent},moduleConfiguration={ModuleConfiguration}): moduleControl → {ModuleControl}", cachedContent, moduleConfiguration, moduleControl);
			TracelLogger.LogDebug("<<< Exiting CreateCachedControl(cachedContent={CachedContent},moduleConfiguration={ModuleConfiguration})", cachedContent, moduleConfiguration);
            return moduleControl;
        }

        /// <inheritdoc cref="IModuleControlPipeline.CreateModuleControl" />
        [DnnDeprecated(9, 4, 0, "This implementation has moved to DotNetNuke.ModulePipeline.ModuleControlPipeline")]
        public static partial Control CreateModuleControl(ModuleInfo moduleConfiguration)
        {
			TracelLogger.LogDebug(">>> Entering CreateModuleControl(moduleConfiguration={ModuleConfiguration})", moduleConfiguration);
            string extension = Path.GetExtension(moduleConfiguration.ModuleControl.ControlSrc.ToLowerInvariant());
            var moduleControl = new ModuleControlBase();
			TracelLogger.LogDebug("CreateModuleControl(moduleConfiguration={ModuleConfiguration}): moduleControl → {ModuleControl}", moduleConfiguration, moduleControl);
            moduleControl.ModuleContext.Configuration = moduleConfiguration;

            switch (extension)
            {
                case ".mvc":
                    var segments = moduleConfiguration.ModuleControl.ControlSrc.Replace(".mvc", string.Empty).Split('/');
					TracelLogger.LogDebug("CreateModuleControl(moduleConfiguration={ModuleConfiguration}): segments → {Segments}", moduleConfiguration, segments);

                    moduleControl.LocalResourceFile = $"~/DesktopModules/MVC/{moduleConfiguration.DesktopModule.FolderName}/{Localization.LocalResourceDirectory}/{segments[0]}.resx";
					TracelLogger.LogDebug("CreateModuleControl(moduleConfiguration={ModuleConfiguration}): moduleControl.LocalResourceFile → {Arg1}", moduleConfiguration, moduleControl.LocalResourceFile);
                    break;
                default:
                    moduleControl.LocalResourceFile = moduleConfiguration.ModuleControl.ControlSrc.Replace(Path.GetFileName(moduleConfiguration.ModuleControl.ControlSrc), string.Empty) +
                                        Localization.LocalResourceDirectory + "/" +
                                        Path.GetFileName(moduleConfiguration.ModuleControl.ControlSrc);
                    break;
            }

			TracelLogger.LogDebug("<<< Exiting CreateModuleControl(moduleConfiguration={ModuleConfiguration})", moduleConfiguration);
            return moduleControl;
        }

        [DnnDeprecated(9, 4, 0, "This implementation has moved to DotNetNuke.ModulePipeline.ModuleControlPipeline")]
        private static partial IModuleControlFactory GetModuleControlFactory(string controlSrc)
        {
			TracelLogger.LogDebug(">>> Entering GetModuleControlFactory(controlSrc={ControlSrc})", controlSrc);
            string extension = Path.GetExtension(controlSrc.ToLowerInvariant());

            IModuleControlFactory controlFactory = null;
			TracelLogger.LogDebug("GetModuleControlFactory(controlSrc={ControlSrc}): controlFactory → {ControlFactory}", controlSrc, controlFactory);
            Type factoryType;
            switch (extension)
            {
                case ".ascx":
                    controlFactory = new WebFormsModuleControlFactory();
                    break;
                case ".html":
                case ".htm":
                    controlFactory = new Html5ModuleControlFactory();
                    break;
                case ".cshtml":
                case ".vbhtml":
                    factoryType = Reflection.CreateType("DotNetNuke.Web.Razor.RazorModuleControlFactory");
                    if (factoryType != null)
                    {
                        controlFactory = Reflection.CreateObject(factoryType) as IModuleControlFactory;
                    }

                    break;
                case ".mvc":
                    factoryType = Reflection.CreateType("DotNetNuke.Web.Mvc.MvcModuleControlFactory");
                    if (factoryType != null)
                    {
                        controlFactory = Reflection.CreateObject(factoryType) as IModuleControlFactory;
                    }

                    break;
                default:
                    controlFactory = new ReflectedModuleControlFactory();
                    break;
            }

			TracelLogger.LogDebug("<<< Exiting GetModuleControlFactory(controlSrc={ControlSrc})", controlSrc);
            return controlFactory;
        }
    }
}
