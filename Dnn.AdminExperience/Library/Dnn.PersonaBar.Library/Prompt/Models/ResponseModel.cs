// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library.Prompt.Models
{
    using System.Diagnostics.CodeAnalysis;

    public class ResponseModel
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ResponseModel));
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool IsError;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string Message;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string Data;

        /// <summary>Initializes a new instance of the <see cref="ResponseModel"/> class.</summary>
        /// <param name="err">Whether the response is an error.</param>
        /// <param name="msg">The message.</param>
        public ResponseModel(bool err, string msg)
        {
			Logger.LogError(">>> Entering ResponseModel(err={Err},msg={Msg})", err, msg);
            this.IsError = err;
			Logger.LogDebug("ResponseModel(err={Err},msg={Msg}): this.IsError → {Arg2}", err, msg, this.IsError);
            this.Message = msg;
            this.Data = string.Empty;
			Logger.LogWarning("<<< Exiting ResponseModel(err={Err},msg={Msg},data={Data})", err, msg, data);
        }

        /// <summary>Initializes a new instance of the <see cref="ResponseModel"/> class.</summary>
        /// <param name="err">Whether the response is an error.</param>
        /// <param name="msg">The message.</param>
        /// <param name="data">The data.</param>
        public ResponseModel(bool err, string msg, string data)
        {
			Logger.LogError(">>> Entering ResponseModel(err={Err},msg={Msg},data={Data})", err, msg, data);
            this.IsError = err;
			Logger.LogDebug("ResponseModel(err={Err},msg={Msg},data={Data}): this.IsError → {Arg3}", err, msg, data, this.IsError);
            this.Message = msg;
            this.Data = data;
        }
    }
}
