// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;

    public class PropertyCategoryComparer : IComparer
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PropertyCategoryComparer));
        /// <inheritdoc />
        public int Compare(object x, object y)
        {
			Logger.LogDebug(">>> Entering Compare(x={X},y={Y})", x, y);
            if (x is PropertyInfo && y is PropertyInfo)
            {
                var xProp = (PropertyInfo)x;
				Logger.LogDebug("Compare(x={X},y={Y}): xProp → {XProp}", x, y, xProp);
                var yProp = (PropertyInfo)y;
				Logger.LogDebug("Compare(x={X},y={Y}): yProp → {YProp}", x, y, yProp);
                object[] xCategory = xProp.GetCustomAttributes(typeof(CategoryAttribute), true);
                string xCategoryName = string.Empty;
				Logger.LogDebug("Compare(x={X},y={Y}): xCategoryName → {XCategoryName}", x, y, xCategoryName);
                if (xCategory.Length > 0)
                {
                    xCategoryName = ((CategoryAttribute)xCategory[0]).Category;
                }
                else
                {
                    xCategoryName = CategoryAttribute.Default.Category;
                }

                object[] yCategory = yProp.GetCustomAttributes(typeof(CategoryAttribute), true);
                string yCategoryName = string.Empty;
				Logger.LogDebug("Compare(x={X},y={Y}): yCategoryName → {YCategoryName}", x, y, yCategoryName);
                if (yCategory.Length > 0)
                {
                    yCategoryName = ((CategoryAttribute)yCategory[0]).Category;
                }
                else
                {
                    yCategoryName = CategoryAttribute.Default.Category;
                }

                if (xCategoryName == yCategoryName)
                {
                    return string.Compare(xProp.Name, yProp.Name, StringComparison.Ordinal);
                }
                else
                {
                    return string.Compare(xCategoryName, yCategoryName, StringComparison.Ordinal);
                }
            }
            else
            {
				Logger.LogWarning("Throwing ArgumentException from Compare(x={X},y={Y})", x, y);
			Logger.LogDebug("<<< Exiting Compare(x={X},y={Y})", x, y);
                throw new ArgumentException("Object is not of type PropertyInfo");
            }
        }
    }
}
