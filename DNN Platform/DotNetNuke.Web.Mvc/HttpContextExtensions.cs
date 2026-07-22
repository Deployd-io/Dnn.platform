// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Routing
{
    using System.Web;

    using DotNetNuke.Web.Mvc.Framework.Modules;

    public static class HttpContextExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public const string ModuleRequestResultKey = "Dnn_ModuleRequestResult";

        public static ModuleRequestResult GetModuleRequestResult(this HttpContextBase context)
        {
			Logger.LogDebug(">>> Entering GetModuleRequestResult(context={Context})", context);
			Logger.LogDebug("<<< Exiting GetModuleRequestResult(context={Context})", context);
            return context.Items[ModuleRequestResultKey] as ModuleRequestResult;
        }

        public static bool HasModuleRequestResult(this HttpContextBase context)
        {
			Logger.LogDebug(">>> Entering HasModuleRequestResult(context={Context})", context);
			Logger.LogDebug("<<< Exiting HasModuleRequestResult(context={Context})", context);
            return (context.Items[ModuleRequestResultKey] as ModuleRequestResult) != null;
        }

        public static void SetModuleRequestResult(this HttpContextBase context, ModuleRequestResult moduleRequestResult)
        {
			Logger.LogDebug(">>> Entering SetModuleRequestResult(context={Context},moduleRequestResult={ModuleRequestResult})", context, moduleRequestResult);
            context.Items[ModuleRequestResultKey] = moduleRequestResult;
			Logger.LogDebug("<<< Exiting SetModuleRequestResult(context={Context},moduleRequestResult={ModuleRequestResult})", context, moduleRequestResult);
        }
    }
}
