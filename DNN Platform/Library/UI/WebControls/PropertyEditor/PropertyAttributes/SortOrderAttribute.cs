// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SortOrderAttribute : Attribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="SortOrderAttribute"/> class.</summary>
        /// <param name="order">The sort order of the property.</param>
        public SortOrderAttribute(int order)
        {
			Logger.LogDebug(">>> Entering SortOrderAttribute(order={Order})", order);
            this.Order = order;
			Logger.LogDebug("<<< Exiting SortOrderAttribute(order={Order})", order);
			Logger.LogDebug("SortOrderAttribute(order={Order}): this.Order → {Arg1}", order, this.Order);
        }

        public static int DefaultOrder
        {
            get
            {
                return int.MaxValue;
            }
        }

        public int Order { get; set; }
    }
}
