// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework
{
    using System;

    internal class CachedWebPage
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private string[] _inputFields;

        public CachedWebPage(string token, string[] fields)
        {
			Logger.LogDebug(">>> Entering CachedWebPage(token={Token},fields={Fields})", token, fields);
            this.FetchDateTime = DateTime.Now;
            this.VerificationToken = token;
			Logger.LogDebug("CachedWebPage(token={Token},fields={Fields}): this.FetchDateTime → {Arg2}", token, fields, this.FetchDateTime);
			Logger.LogDebug("CachedWebPage(token={Token},fields={Fields}): this.VerificationToken → {Arg2}", token, fields, this.VerificationToken);
            this.InputFields = fields;
			Logger.LogDebug("<<< Exiting CachedWebPage(token={Token},fields={Fields})", token, fields);
			Logger.LogDebug("CachedWebPage(token={Token},fields={Fields}): this.InputFields → {Arg2}", token, fields, this.InputFields);
        }

        public DateTime FetchDateTime { get; private set; } // when was this loaded

        public string VerificationToken { get; private set; }

        public string[] InputFields
        {
            get { return this._inputFields.Clone() as string[]; }
            private set { this._inputFields = value; }
        }
    }
}
