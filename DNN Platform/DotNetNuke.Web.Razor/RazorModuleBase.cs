// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Razor
{
    using System;
    using System.IO;
    using System.Web.UI;

    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI.Modules;
	using Microsoft.Extensions.Logging;

    /// <summary>A base class for razor modules.</summary>
    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    public partial class RazorModuleBase : ModuleUserControlBase
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        protected virtual string RazorScriptFile
        {
            get
            {
                var scriptFolder = this.AppRelativeTemplateSourceDirectory;
                var fileRoot = Path.GetFileNameWithoutExtension(this.AppRelativeVirtualPath);
                var scriptFile = scriptFolder + "_" + fileRoot + ".cshtml";

                if (!File.Exists(this.Server.MapPath(scriptFile)))
                {
                    // Try VB (vbhtml)
                    scriptFile = scriptFolder + "_" + fileRoot + ".vbhtml";

                    if (!File.Exists(this.Server.MapPath(scriptFile)))
                    {
                        ////Return ""
                        scriptFile = string.Empty;
                    }
                }

                return scriptFile;
            }
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			_logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);
            try
            {
                if (!string.IsNullOrEmpty(this.RazorScriptFile))
                {
                    var razorEngine = new RazorEngine(this.RazorScriptFile, this.ModuleContext, this.LocalResourceFile);
					_logger.LogDebug("OnPreRender(e={E}): razorEngine → {RazorEngine}", e, razorEngine);
                    var writer = new StringWriter();
					_logger.LogDebug("OnPreRender(e={E}): writer → {Writer}", e, writer);
                    razorEngine.Render(writer);

                    this.Controls.Add(new LiteralControl(writer.ToString()));
                }
            }
            catch (Exception ex)
            {
				_logger.LogError("Exception in OnPreRender(e={E}): {Arg1}", e, ex.getMessage());
                Exceptions.ProcessModuleLoadException(this, ex);
            }
			_logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }
    }
}
