// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components.Exceptions
{
    using System;

    public class BulkPagesException : Exception
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="BulkPagesException"/> class.</summary>
        /// <param name="field">The invalid field.</param>
        /// <param name="message">The message that describes the error.</param>
        public BulkPagesException(string field, string message)
            : base(message)
        {
			Logger.LogDebug(">>> Entering BulkPagesException(field={Field},message={Message})", field, message);
            this.Field = field;
			Logger.LogDebug("<<< Exiting BulkPagesException(field={Field},message={Message})", field, message);
			Logger.LogDebug("BulkPagesException(field={Field},message={Message}): this.Field → {Arg2}", field, message, this.Field);
        }

        public string Field { get; set; }
    }
}
