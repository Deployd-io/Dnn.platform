// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FormatAttribute : Attribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly string format;

        /// <summary>Initializes a new instance of the <see cref="FormatAttribute"/> class.</summary>
        /// <param name="format">The format string to use to format the corresponding date/time.</param>
        public FormatAttribute(string format)
        {
			Logger.LogDebug(">>> Entering FormatAttribute(format={Format})", format);
            this.format = format;
			Logger.LogDebug("<<< Exiting FormatAttribute(format={Format})", format);
			Logger.LogDebug("FormatAttribute(format={Format}): this.format → {Arg1}", format, this.format);
        }

        public string Format
        {
            get
            {
                return this.format;
            }
        }
    }
}
