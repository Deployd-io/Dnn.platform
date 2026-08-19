// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RequiredAttribute : Attribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RequiredAttribute));
        private readonly bool required;

        /// <summary>Initializes a new instance of the <see cref="RequiredAttribute"/> class.</summary>
        /// <param name="required">Whether the value is required.</param>
        public RequiredAttribute(bool required)
        {
			Logger.LogDebug(">>> Entering RequiredAttribute(required={Required})", required);
            this.required = required;
			Logger.LogDebug("RequiredAttribute(required={Required}): this.required → {Arg1}", required, this.required);
			Logger.LogDebug("<<< Exiting RequiredAttribute(required={Required})", required);
        }

        public bool Required
        {
            get
            {
                return this.required;
            }
        }
    }
}
