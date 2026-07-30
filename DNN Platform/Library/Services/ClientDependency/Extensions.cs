// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.ClientDependency
{
    using System.Web.Hosting;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Web.Client.ResourceManager;

    public static class Extensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Registers a font resource by its path.</summary>
        /// <param name="controller">The <see cref="IClientResourceController"/> to extend.</param>
        /// <param name="fontPath">The path to the font resource to register.</param>
        /// <param name="checkIfExists">Whether to check if the script already exists before registering.</param>
        public static void RegisterFont(this IClientResourceController controller, string fontPath, bool checkIfExists = false)
        {
			Logger.LogDebug(">>> Entering RegisterFont(controller={Controller},fontPath={FontPath},checkIfExists={CheckIfExists})", controller, fontPath, checkIfExists);
            if (controller == null)
            {
                throw new System.ArgumentNullException(nameof(controller));
            }

            if (string.IsNullOrEmpty(fontPath))
            {
                throw new System.ArgumentException("Value cannot be null or empty.", nameof(fontPath));
            }

            if (checkIfExists)
            {
                var physicalPath = HostingEnvironment.MapPath(fontPath);
                if (!System.IO.File.Exists(physicalPath))
				Logger.LogDebug("RegisterFont(controller={Controller},fontPath={FontPath},checkIfExists={CheckIfExists}): physicalPath → {PhysicalPath}", controller, fontPath, checkIfExists, physicalPath);
                {
                    return;
                }
            }

            controller.CreateFont(fontPath).Register();
			Logger.LogDebug("<<< Exiting RegisterFont(controller={Controller},fontPath={FontPath},checkIfExists={CheckIfExists})", controller, fontPath, checkIfExists);
        }

        /// <summary>Registers a script resource by its path.</summary>
        /// <param name="controller">The <see cref="IClientResourceController"/> to extend.</param>
        /// <param name="scriptPath">The path to the script resource to register.</param>
        /// <param name="checkIfExists">Whether to check if the script already exists before registering.</param>
        public static void RegisterScript(this IClientResourceController controller, string scriptPath, bool checkIfExists = false)
        {
			Logger.LogDebug(">>> Entering RegisterScript(controller={Controller},scriptPath={ScriptPath},checkIfExists={CheckIfExists})", controller, scriptPath, checkIfExists);
            if (controller == null)
            {
                throw new System.ArgumentNullException(nameof(controller));
            }

            if (string.IsNullOrEmpty(scriptPath))
            {
                throw new System.ArgumentException("Value cannot be null or empty.", nameof(scriptPath));
            }

            if (checkIfExists)
            {
                var physicalPath = HostingEnvironment.MapPath(scriptPath);
                if (!System.IO.File.Exists(physicalPath))
				Logger.LogDebug("RegisterScript(controller={Controller},scriptPath={ScriptPath},checkIfExists={CheckIfExists}): physicalPath → {PhysicalPath}", controller, scriptPath, checkIfExists, physicalPath);
                {
                    return;
                }
            }

            controller.CreateScript(scriptPath).Register();
			Logger.LogDebug("<<< Exiting RegisterScript(controller={Controller},scriptPath={ScriptPath},checkIfExists={CheckIfExists})", controller, scriptPath, checkIfExists);
        }

        /// <summary>Registers a script resource by its path.</summary>
        /// <param name="controller">The <see cref="IClientResourceController"/> to extend.</param>
        /// <param name="scriptPath">The path to the script resource to register.</param>
        /// <param name="priority">The priority for loading the script.</param>
        /// <param name="checkIfExists">Whether to check if the script already exists before registering.</param>
        public static void RegisterScript(this IClientResourceController controller, string scriptPath, FileOrder.Js priority, bool checkIfExists = false)
        {
			Logger.LogDebug(">>> Entering RegisterScript(controller={Controller},scriptPath={ScriptPath},priority={Priority},checkIfExists={CheckIfExists})", controller, scriptPath, priority, checkIfExists);
            if (controller == null)
            {
                throw new System.ArgumentNullException(nameof(controller));
            }

            if (string.IsNullOrEmpty(scriptPath))
            {
                throw new System.ArgumentException("Value cannot be null or empty.", nameof(scriptPath));
            }

            if (checkIfExists)
            {
                var physicalPath = HostingEnvironment.MapPath(scriptPath);
                if (!System.IO.File.Exists(physicalPath))
				Logger.LogDebug("RegisterScript(controller={Controller},scriptPath={ScriptPath},priority={Priority},checkIfExists={CheckIfExists}): physicalPath → {PhysicalPath}", controller, scriptPath, priority, checkIfExists, physicalPath);
                {
                    return;
                }
            }

            controller.CreateScript(scriptPath)
                .SetPriority(priority)
                .Register();
        }

        /// <summary>Registers a stylesheet resource by its path.</summary>
        /// <param name="controller">The <see cref="IClientResourceController"/> to extend.</param>
        /// <param name="stylesheetPath">The path to the stylesheet resource to register.</param>
        /// <param name="checkIfExists">Whether to check if the script already exists before registering.</param>
        public static void RegisterStylesheet(this IClientResourceController controller, string stylesheetPath, bool checkIfExists = false)
        {
			Logger.LogDebug(">>> Entering RegisterStylesheet(controller={Controller},stylesheetPath={StylesheetPath},checkIfExists={CheckIfExists})", controller, stylesheetPath, checkIfExists);
            if (controller == null)
            {
                throw new System.ArgumentNullException(nameof(controller));
            }

            if (string.IsNullOrEmpty(stylesheetPath))
            {
                throw new System.ArgumentException("Value cannot be null or empty.", nameof(stylesheetPath));
            }

            if (checkIfExists)
            {
                var physicalPath = HostingEnvironment.MapPath(stylesheetPath);
                if (!System.IO.File.Exists(physicalPath))
				Logger.LogDebug("RegisterStylesheet(controller={Controller},stylesheetPath={StylesheetPath},checkIfExists={CheckIfExists}): physicalPath → {PhysicalPath}", controller, stylesheetPath, checkIfExists, physicalPath);
                {
                    return;
                }
            }

            controller.CreateStylesheet(stylesheetPath).Register();
			Logger.LogDebug("<<< Exiting RegisterStylesheet(controller={Controller},stylesheetPath={StylesheetPath},checkIfExists={CheckIfExists})", controller, stylesheetPath, checkIfExists);
        }

        /// <summary>Registers a stylesheet resource by its path.</summary>
        /// <param name="controller">The <see cref="IClientResourceController"/> to extend.</param>
        /// <param name="stylesheetPath">The path to the stylesheet resource to register.</param>
        /// <param name="priority">The priority for loading the stylesheet.</param>
        /// <param name="checkIfExists">Whether to check if the script already exists before registering.</param>
        public static void RegisterStylesheet(this IClientResourceController controller, string stylesheetPath, FileOrder.Css priority, bool checkIfExists = false)
        {
			Logger.LogDebug(">>> Entering RegisterStylesheet(controller={Controller},stylesheetPath={StylesheetPath},priority={Priority},checkIfExists={CheckIfExists})", controller, stylesheetPath, priority, checkIfExists);
            if (controller == null)
            {
                throw new System.ArgumentNullException(nameof(controller));
            }

            if (string.IsNullOrEmpty(stylesheetPath))
            {
                throw new System.ArgumentException("Value cannot be null or empty.", nameof(stylesheetPath));
            }

            if (checkIfExists)
            {
                var physicalPath = HostingEnvironment.MapPath(stylesheetPath);
                if (!System.IO.File.Exists(physicalPath))
				Logger.LogDebug("RegisterStylesheet(controller={Controller},stylesheetPath={StylesheetPath},priority={Priority},checkIfExists={CheckIfExists}): physicalPath → {PhysicalPath}", controller, stylesheetPath, priority, checkIfExists, physicalPath);
                {
                    return;
                }
            }

            controller.CreateStylesheet(stylesheetPath)
                .SetPriority(priority)
                .Register();
        }
    }
}
