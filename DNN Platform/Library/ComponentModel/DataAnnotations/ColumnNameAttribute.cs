// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel.DataAnnotations
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnNameAttribute : Attribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="ColumnNameAttribute"/> class.</summary>
        /// <param name="columnName">The name of the column.</param>
        public ColumnNameAttribute(string columnName)
        {
			Logger.LogDebug(">>> Entering ColumnNameAttribute(columnName={ColumnName})", columnName);
            this.ColumnName = columnName;
			Logger.LogDebug("<<< Exiting ColumnNameAttribute(columnName={ColumnName})", columnName);
			Logger.LogDebug("ColumnNameAttribute(columnName={ColumnName}): this.ColumnName → {Arg1}", columnName, this.ColumnName);
        }

        public string ColumnName { get; set; }
    }
}
