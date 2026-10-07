// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library.Prompt.Models
{
    using System.IO;

    using DotNetNuke.Internal.SourceGenerators;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>A console result that represents an error.</summary>
    [DnnDeprecated(9, 7, 0, "Moved to DotNetNuke.Prompt in the core library project")]
    public partial class ConsoleErrorResultModel : ConsoleResultModel
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ConsoleErrorResultModel));
        /// <summary>Initializes a new instance of the <see cref="ConsoleErrorResultModel"/> class.</summary>
        public ConsoleErrorResultModel()
        {
			Logger.LogError(">>> Entering ConsoleErrorResultModel()");
            this.IsError = true;
			Logger.LogDebug("ConsoleErrorResultModel(): this.IsError → {Arg0}", this.IsError);
        Logger.LogDebug("ConsoleErrorResultModel(errMessage={ErrMessage}): Output → {Output}", errMessage, Output);
        Logger.LogWarning("<<< Exiting ConsoleErrorResultModel()");
            this.Output = Localization.GetString("Prompt_InvalidSyntax", LocalResourcesFile, true);
        }

        /// <summary>Initializes a new instance of the <see cref="ConsoleErrorResultModel"/> class.</summary>
        /// <param name="errMessage">The error message.</param>
        public ConsoleErrorResultModel(string errMessage)
        {
			Logger.LogError(">>> Entering ConsoleErrorResultModel(errMessage={ErrMessage})", errMessage);
            this.IsError = true;
			Logger.LogWarning("<<< Exiting ConsoleErrorResultModel(errMessage={ErrMessage})", errMessage);
			Logger.LogDebug("ConsoleErrorResultModel(errMessage={ErrMessage}): this.IsError → {Arg1}", errMessage, this.IsError);
            this.Output = errMessage;
        }

        private static string LocalResourcesFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/App_LocalResources/SharedResources.resx");
    }
}
