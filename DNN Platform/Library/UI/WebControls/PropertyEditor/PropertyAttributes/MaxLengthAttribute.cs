// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    using DotNetNuke.Common.Utilities;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MaxLengthAttribute : Attribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MaxLengthAttribute));
        private readonly int length = Null.NullInteger;

        /// <summary>Initializes a new instance of the <see cref="MaxLengthAttribute"/> class.</summary>
        /// <param name="length">The maximum length.</param>
        public MaxLengthAttribute(int length)
        {
			Logger.LogDebug(">>> Entering MaxLengthAttribute(length={Length})", length);
            this.length = length;
			Logger.LogDebug("MaxLengthAttribute(length={Length}): this.length → {Arg1}", length, this.length);
			Logger.LogDebug("MaxLengthAttribute(length={Length}): length → {Length1}", length, length);
			Logger.LogDebug("<<< Exiting MaxLengthAttribute(length={Length})", length);
        }

        public int Length
        {
            get
            {
                return this.length;
            }
        }
    }
}
