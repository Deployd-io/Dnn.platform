// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel.DataAnnotations
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class TableNameAttribute : Attribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="TableNameAttribute"/> class.</summary>
        /// <param name="tableName">The name of the table.</param>
        public TableNameAttribute(string tableName)
        {
			Logger.LogDebug(">>> Entering TableNameAttribute(tableName={TableName})", tableName);
            this.TableName = tableName;
			Logger.LogDebug("<<< Exiting TableNameAttribute(tableName={TableName})", tableName);
			Logger.LogDebug("TableNameAttribute(tableName={TableName}): this.TableName → {Arg1}", tableName, this.TableName);
        }

        public string TableName { get; set; }
    }
}
