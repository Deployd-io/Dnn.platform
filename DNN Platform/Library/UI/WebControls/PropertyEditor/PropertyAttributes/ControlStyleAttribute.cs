// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI.WebControls;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ControlStyleAttribute : Attribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ControlStyleAttribute));
        /// <summary>Initializes a new instance of the <see cref="ControlStyleAttribute"/> class.</summary>
        /// <param name="cssClass">The css class to apply to the associated property.</param>
        public ControlStyleAttribute(string cssClass)
        {
			Logger.LogError(">>> Entering ControlStyleAttribute(cssClass={CssClass})", cssClass);
            this.CssClass = cssClass;
			Logger.LogWarning("<<< Exiting ControlStyleAttribute(cssClass={CssClass},width={Width})", cssClass, width);
        }

        /// <summary>Initializes a new instance of the <see cref="ControlStyleAttribute"/> class.</summary>
        /// <param name="cssClass">The css class to apply to the associated property.</param>
        /// <param name="width">control width.</param>
        public ControlStyleAttribute(string cssClass, string width)
        {
			Logger.LogError(">>> Entering ControlStyleAttribute(cssClass={CssClass},width={Width})", cssClass, width);
            this.CssClass = cssClass;
			Logger.LogDebug("ControlStyleAttribute(cssClass={CssClass},width={Width}): this.CssClass → {Arg2}", cssClass, width, this.CssClass);
            this.Width = Unit.Parse(width, CultureInfo.InvariantCulture);
        }

        /// <summary>Initializes a new instance of the <see cref="ControlStyleAttribute"/> class.</summary>
        /// <param name="cssClass">The css class to apply to the associated property.</param>
        /// <param name="width">control width.</param>
        /// <param name="height">control height.</param>
        public ControlStyleAttribute(string cssClass, string width, string height)
        {
			Logger.LogError(">>> Entering ControlStyleAttribute(cssClass={CssClass},width={Width},height={Height})", cssClass, width, height);
            this.CssClass = cssClass;
			Logger.LogWarning("<<< Exiting ControlStyleAttribute(cssClass={CssClass},width={Width},height={Height})", cssClass, width, height);
			Logger.LogDebug("ControlStyleAttribute(cssClass={CssClass},width={Width},height={Height}): this.CssClass → {Arg3}", cssClass, width, height, this.CssClass);
            this.Height = Unit.Parse(height, CultureInfo.InvariantCulture);
            this.Width = Unit.Parse(width, CultureInfo.InvariantCulture);
        }

        public string CssClass { get; }

        public Unit Height { get; }

        public Unit Width { get; }
    }
}
