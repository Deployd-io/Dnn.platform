// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Framework;

    public class JavaScriptUtils : ServiceLocator<IJavaScriptUtils, JavaScriptUtils>, IJavaScriptUtils
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JavaScriptUtils));
        /// <inheritdoc />
        public void RegisterJavascriptVariable(string variableName, object value, Page page, Type type)
        {
			Logger.LogDebug(">>> Entering RegisterJavascriptVariable(variableName={VariableName},value={Value},page={Page},type={Type})", variableName, value, page, type);
            var valueAsJson = Json.Serialize(value);

			Logger.LogDebug("RegisterJavascriptVariable(variableName={VariableName},value={Value},page={Page},type={Type}): valueAsJson → {ValueAsJson}", variableName, value, page, type, valueAsJson);
            var script = $"var {variableName} = {valueAsJson};";
			Logger.LogDebug("RegisterJavascriptVariable(variableName={VariableName},value={Value},page={Page},type={Type}): script → {Script}", variableName, value, page, type, script);

            if (ScriptManager.GetCurrent(page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(page, type, variableName, script, true);
            }
            else
            {
                page.ClientScript.RegisterStartupScript(type, variableName, script, true);
            }
			Logger.LogDebug("<<< Exiting RegisterJavascriptVariable(variableName={VariableName},value={Value},page={Page},type={Type})", variableName, value, page, type);
        }

        /// <inheritdoc />
        protected override Func<IJavaScriptUtils> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new JavaScriptUtils();
        }
    }
}
