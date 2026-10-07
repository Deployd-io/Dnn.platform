// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Prompt
{
    using DotNetNuke.Services.Localization;

    /// <summary>This is used to return the results of a command error to the client.</summary>
    public class ConsoleErrorResultModel : ConsoleResultModel
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
            this.Output = Localization.GetString("Prompt_InvalidSyntax", Constants.DefaultPromptResourceFile, true);
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
    }
}
