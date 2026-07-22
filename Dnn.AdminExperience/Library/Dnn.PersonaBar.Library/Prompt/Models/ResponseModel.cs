// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library.Prompt.Models
{
    using System.Diagnostics.CodeAnalysis;

    public class ResponseModel
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
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
			Logger.LogDebug(">>> Entering ResponseModel(err={Err},msg={Msg})", err, msg);
            this.IsError = err;
            this.Message = msg;
			Logger.LogDebug("ResponseModel(err={Err},msg={Msg}): this.IsError → {Arg2}", err, msg, this.IsError);
			Logger.LogDebug("ResponseModel(err={Err},msg={Msg}): this.Message → {Arg2}", err, msg, this.Message);
            this.Data = string.Empty;
			Logger.LogDebug("<<< Exiting ResponseModel(err={Err},msg={Msg})", err, msg);
			Logger.LogDebug("ResponseModel(err={Err},msg={Msg}): this.Data → {Arg2}", err, msg, this.Data);
        }

        /// <summary>Initializes a new instance of the <see cref="ResponseModel"/> class.</summary>
        /// <param name="err">Whether the response is an error.</param>
        /// <param name="msg">The message.</param>
        /// <param name="data">The data.</param>
        public ResponseModel(bool err, string msg, string data)
        {
			Logger.LogDebug(">>> Entering ResponseModel(err={Err},msg={Msg},data={Data})", err, msg, data);
            this.IsError = err;
            this.Message = msg;
			Logger.LogDebug("ResponseModel(err={Err},msg={Msg},data={Data}): this.IsError → {Arg3}", err, msg, data, this.IsError);
			Logger.LogDebug("ResponseModel(err={Err},msg={Msg},data={Data}): this.Message → {Arg3}", err, msg, data, this.Message);
            this.Data = data;
			Logger.LogDebug("<<< Exiting ResponseModel(err={Err},msg={Msg},data={Data})", err, msg, data);
        }
    }
}
