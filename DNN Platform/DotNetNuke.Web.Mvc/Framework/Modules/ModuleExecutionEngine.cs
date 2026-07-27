// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    using System;
    using System.IO;

    using DotNetNuke.Common;
    using DotNetNuke.Web.Mvc.Framework.ActionResults;

    public class ModuleExecutionEngine : IModuleExecutionEngine
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public ModuleRequestResult ExecuteModule(ModuleRequestContext moduleRequestContext)
        {
			Logger.LogDebug(">>> Entering ExecuteModule(moduleRequestContext={ModuleRequestContext})", moduleRequestContext);
            Requires.NotNull("moduleRequestContext", moduleRequestContext);

            if (moduleRequestContext.ModuleApplication != null)
            {
                // Run the module
                return moduleRequestContext.ModuleApplication.ExecuteRequest(moduleRequestContext);
            }

			Logger.LogDebug("<<< Exiting ExecuteModule(moduleRequestContext={ModuleRequestContext})", moduleRequestContext);
            return null;
        }

        /// <inheritdoc />
        public virtual void ExecuteModuleResult(ModuleRequestResult moduleResult, TextWriter writer)
        {
			Logger.LogDebug(">>> Entering ExecuteModuleResult(moduleResult={ModuleResult},writer={Writer})", moduleResult, writer);
            var result = moduleResult.ActionResult as IDnnViewResult;
            if (result != null)
			Logger.LogDebug("ExecuteModuleResult(moduleResult={ModuleResult},writer={Writer}): result → {Result}", moduleResult, writer, result);
            {
                result.ExecuteResult(moduleResult.ControllerContext, writer);
            }
            else
            {
                moduleResult.ActionResult.ExecuteResult(moduleResult.ControllerContext);
            }
			Logger.LogDebug("<<< Exiting ExecuteModuleResult(moduleResult={ModuleResult},writer={Writer})", moduleResult, writer);
        }
    }
}
