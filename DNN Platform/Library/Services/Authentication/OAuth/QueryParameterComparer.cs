// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Authentication.OAuth
{
    using System.Collections.Generic;

    /// <summary>Comparer class used to perform the sorting of the query parameters.</summary>
    internal class QueryParameterComparer : IComparer<QueryParameter>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(QueryParameterComparer));
        /// <inheritdoc />
        public int Compare(QueryParameter x, QueryParameter y)
        {
			Logger.LogDebug(">>> Entering Compare(x={X},y={Y})", x, y);
            if (x.Name == y.Name)
            {
			Logger.LogDebug("<<< Exiting Compare(x={X},y={Y})", x, y);
                return string.CompareOrdinal(x.Value, y.Value);
            }

            return string.CompareOrdinal(x.Name, y.Name);
        }
    }
}
