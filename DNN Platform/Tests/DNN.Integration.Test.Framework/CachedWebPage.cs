// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework
{
    using System;
	using Microsoft.Extensions.Logging;

    internal class CachedWebPage
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private string[] _inputFields;

        public CachedWebPage(string token, string[] fields)
        {
			_logger.LogDebug(">>> Entering CachedWebPage(token=<redacted>,fields={Fields})", fields);
            this.FetchDateTime = DateTime.Now;
            this.VerificationToken = token;
			_logger.LogDebug("CachedWebPage(token=<redacted>,fields={Fields}): this.FetchDateTime → {Arg2}", fields, this.FetchDateTime);
			_logger.LogDebug("CachedWebPage(token=<redacted>,fields={Fields}): this.VerificationToken → {Arg2}", fields, this.VerificationToken);
            this.InputFields = fields;
			_logger.LogDebug("<<< Exiting CachedWebPage(token=<redacted>,fields={Fields})", fields);
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
