// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Authentication.OAuth
{
    /// <summary>Provides an internal structure to sort the query parameter.</summary>
    public class QueryParameter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(QueryParameter));
        /// <summary>Initializes a new instance of the <see cref="QueryParameter"/> class.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public QueryParameter(string name, string value)
        {
			Logger.LogDebug(">>> Entering QueryParameter(name={Name},value={Value})", name, value);
            this.Name = name;
            this.Value = value;
			Logger.LogDebug("QueryParameter(name={Name},value={Value}): this.Name → {Arg2}", name, value, this.Name);
			Logger.LogDebug("<<< Exiting QueryParameter(name={Name},value={Value})", name, value);
        }

        public string Name { get; private set; }

        public string Value { get; private set; }
    }
}
