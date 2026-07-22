// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls.Extensions
{
    using System.Drawing;
    using System.Linq;
    using System.Web.UI.WebControls;

    /// <summary>Extension methods for <see cref="WebControl"/> instances.</summary>
    public static class WebControlExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Adds the <paramref name="cssClass"/> to the <paramref name="control"/>.</summary>
        /// <param name="control">The control.</param>
        /// <param name="cssClass">The CSS class to add.</param>
        public static void AddCssClass(this WebControl control, string cssClass)
        {
			Logger.LogDebug(">>> Entering AddCssClass(control={Control},cssClass={CssClass})", control, cssClass);
            if (string.IsNullOrEmpty(control.CssClass))
            {
                control.CssClass = cssClass;
				Logger.LogDebug("AddCssClass(control={Control},cssClass={CssClass}): control.CssClass → {Arg2}", control, cssClass, control.CssClass);
            }
            else
            {
                var cssClasses = control.CssClass.Split(' ');
                var classExists = cssClasses.Any(@class => @class == cssClass);
				Logger.LogDebug("AddCssClass(control={Control},cssClass={CssClass}): cssClasses → {CssClasses}", control, cssClass, cssClasses);

                if (!classExists)
                {
                    control.CssClass += " " + cssClass;
                }
            }
			Logger.LogDebug("<<< Exiting AddCssClass(control={Control},cssClass={CssClass})", control, cssClass);
        }

        /// <summary>Removes the <paramref name="cssClass"/> from the <paramref name="control"/>.</summary>
        /// <param name="control">The control.</param>
        /// <param name="cssClass">The CSS class.</param>
        public static void RemoveCssClass(this WebControl control, string cssClass)
        {
			Logger.LogDebug(">>> Entering RemoveCssClass(control={Control},cssClass={CssClass})", control, cssClass);
            if (!string.IsNullOrEmpty(control.CssClass))
            {
                var cssClasses = control.CssClass.Split(' ');
                control.CssClass = string.Join(" ", cssClasses.Where(@class => @class != cssClass).ToArray());
				Logger.LogDebug("RemoveCssClass(control={Control},cssClass={CssClass}): cssClasses → {CssClasses}", control, cssClass, cssClasses);
            }
			Logger.LogDebug("<<< Exiting RemoveCssClass(control={Control},cssClass={CssClass})", control, cssClass);
        }

        /// <summary>Gets the orientation of the <paramref name="size"/>.</summary>
        /// <param name="size">The size.</param>
        /// <returns>The orientation.</returns>
        public static Orientation Orientation(this Size size)
        {
			Logger.LogDebug(">>> Entering Orientation(size={Size})", size);
			Logger.LogDebug("<<< Exiting Orientation(size={Size})", size);
            return size.Width > size.Height ?
                System.Web.UI.WebControls.Orientation.Horizontal : System.Web.UI.WebControls.Orientation.Vertical;
        }
    }
}
