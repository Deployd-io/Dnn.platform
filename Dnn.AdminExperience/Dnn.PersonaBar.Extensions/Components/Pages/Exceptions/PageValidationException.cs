// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components.Exceptions
{
    using System;

    public class PageValidationException : Exception
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public PageValidationException(string field, string message)
            : base(message)
        {
			Logger.LogDebug(">>> Entering PageValidationException(field={Field},message={Message})", field, message);
            this.Field = field;
			Logger.LogDebug("<<< Exiting PageValidationException(field={Field},message={Message})", field, message);
			Logger.LogDebug("PageValidationException(field={Field},message={Message}): this.Field → {Arg2}", field, message, this.Field);
        }

        public string Field { get; set; }
    }
}
