// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Data.PetaPoco
{
    using System;

    using global::PetaPoco;

    [CLSCompliant(false)]
    public class FluentColumnMap
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="FluentColumnMap"/> class.</summary>
        public FluentColumnMap()
        {
		Logger.LogDebug(">>> Entering FluentColumnMap()");
		Logger.LogDebug("<<< Exiting FluentColumnMap()");
        }

        /// <summary>Initializes a new instance of the <see cref="FluentColumnMap"/> class.</summary>
        /// <param name="columnInfo">Information about the column.</param>
        public FluentColumnMap(ColumnInfo columnInfo)
            : this(columnInfo, null)
        {
		Logger.LogDebug(">>> Entering FluentColumnMap(columnInfo={ColumnInfo})", columnInfo);
        }
		Logger.LogDebug("<<< Exiting FluentColumnMap(columnInfo={ColumnInfo})", columnInfo);

        /// <summary>Initializes a new instance of the <see cref="FluentColumnMap"/> class.</summary>
        /// <param name="columnInfo">Information about the column.</param>
        /// <param name="fromDbConverter">A function to convert the database value to the class value.</param>
        public FluentColumnMap(ColumnInfo columnInfo, Func<object, object> fromDbConverter)
            : this(columnInfo, fromDbConverter, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FluentColumnMap"/> class.</summary>
        /// <param name="columnInfo">Information about the column.</param>
        /// <param name="fromDbConverter">A function to convert the database value to the class value.</param>
        /// <param name="toDbConverter">A function to convert the class value to the database value.</param>
        public FluentColumnMap(ColumnInfo columnInfo, Func<object, object> fromDbConverter, Func<object, object> toDbConverter)
        {
            this.ColumnInfo = columnInfo;
            this.FromDbConverter = fromDbConverter;
            this.ToDbConverter = toDbConverter;
        }

        public ColumnInfo ColumnInfo { get; set; }

        public Func<object, object> FromDbConverter { get; set; }

        public Func<object, object> ToDbConverter { get; set; }
    }
}
