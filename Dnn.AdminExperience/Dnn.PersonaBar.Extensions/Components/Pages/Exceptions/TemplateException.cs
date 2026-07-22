// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components.Exceptions
{
    using System;

    public class TemplateException : Exception
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="TemplateException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        public TemplateException(string message)
            : base(message)
        {
		Logger.LogDebug(">>> Entering TemplateException(message={Message})", message);
		Logger.LogDebug("<<< Exiting TemplateException(message={Message})", message);
        }
    }
}
