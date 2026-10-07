// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Razor
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.UI.Modules;
	using Microsoft.Extensions.Logging;

    /// <summary>The host for a razor module control.</summary>
    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    public partial class RazorHostControl : ModuleControlBase, IActionable
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly string razorScriptFile;

        private RazorEngine engine;

        /// <summary>Initializes a new instance of the <see cref="RazorHostControl"/> class.</summary>
        /// <param name="scriptFile">The path to the Razor script file.</param>
        public RazorHostControl(string scriptFile)
        {
			_logger.LogError(">>> Entering RazorHostControl(scriptFile={ScriptFile})", scriptFile);
            this.razorScriptFile = scriptFile;
        }

        /// <inheritdoc />
        public ModuleActionCollection ModuleActions
        {
            get
            {
                if (this.Engine.Webpage is IActionable)
                {
                    return (this.Engine.Webpage as IActionable).ModuleActions;
                }

                return new ModuleActionCollection();
            }
        }

        protected virtual string RazorScriptFile
        {
            get { return this.razorScriptFile; }
        }

        private RazorEngine Engine
        {
            get
            {
                if (this.engine == null)
                {
                    this.engine = new RazorEngine(this.RazorScriptFile, this.ModuleContext, this.LocalResourceFile);
                }

                return this.engine;
            }
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			_logger.LogError(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            if (!string.IsNullOrEmpty(this.RazorScriptFile))
            {
                var writer = new StringWriter();
				_logger.LogError("OnPreRender(e={E}): writer → {Writer}", e, writer);
                this.Engine.Render(writer);
                this.Controls.Add(new LiteralControl(HttpUtility.HtmlDecode(writer.ToString())));
            }
			_logger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
        }
    }
}
