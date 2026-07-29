// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions
{
    using System;

    /// <summary>An exception related to search indexing.</summary>
    public class SearchException : BasePortalException
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="SearchException"/> class.</summary>
        public SearchException()
        {
		Logger.LogDebug(">>> Entering SearchException()");
		Logger.LogDebug("<<< Exiting SearchException()");
        }

        /// <summary>Initializes a new instance of the <see cref="SearchException"/> class.</summary>
        /// <inheritdoc cref="BasePortalException(string, Exception)"/>
        public SearchException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
