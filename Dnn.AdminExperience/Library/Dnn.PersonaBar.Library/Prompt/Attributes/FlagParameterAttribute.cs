// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Prompt.Attributes
{
    using System;

    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>Attribute to define the help for the flag parameter.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    [DnnDeprecated(9, 7, 0, "Moved to DotNetNuke.Prompt in the core library project and is now a Property Attribute called CommandParameter")]
    public partial class FlagParameterAttribute : Attribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="FlagParameterAttribute"/> class.</summary>
        /// <param name="flag">The flag name.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The expected type.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="required">Whether the flag is required.</param>
        public FlagParameterAttribute(string flag, string description, string type, string defaultValue, bool required)
        {
			Logger.LogDebug(">>> Entering FlagParameterAttribute(flag={Flag},description={Description},type={Type},defaultValue={DefaultValue},required={Required})", flag, description, type, defaultValue, required);
            this.Flag = flag;
            this.Type = type;
			Logger.LogDebug("FlagParameterAttribute(flag={Flag},description={Description},type={Type},defaultValue={DefaultValue},required={Required}): this.Flag → {Arg5}", flag, description, type, defaultValue, required, this.Flag);
			Logger.LogDebug("FlagParameterAttribute(flag={Flag},description={Description},type={Type},defaultValue={DefaultValue},required={Required}): this.Type → {Arg5}", flag, description, type, defaultValue, required, this.Type);
            this.Required = required;
            this.DefaultValue = defaultValue;
            this.Description = description;
			Logger.LogDebug("<<< Exiting FlagParameterAttribute(flag={Flag},description={Description},type={Type},defaultValue={DefaultValue},required={Required})", flag, description, type, defaultValue, required);
			Logger.LogDebug("FlagParameterAttribute(flag={Flag},description={Description},type={Type},defaultValue={DefaultValue},required={Required}): this.Required → {Arg5}", flag, description, type, defaultValue, required, this.Required);
			Logger.LogDebug("FlagParameterAttribute(flag={Flag},description={Description},type={Type},defaultValue={DefaultValue},required={Required}): this.DefaultValue → {Arg5}", flag, description, type, defaultValue, required, this.DefaultValue);
			Logger.LogDebug("FlagParameterAttribute(flag={Flag},description={Description},type={Type},defaultValue={DefaultValue},required={Required}): this.Description → {Arg5}", flag, description, type, defaultValue, required, this.Description);
			Logger.LogDebug(">>> Entering FlagParameterAttribute(flag={Flag},description={Description},type={Type},defaultValue={DefaultValue})", flag, description, type, defaultValue);
			Logger.LogDebug("<<< Exiting FlagParameterAttribute(flag={Flag},description={Description},type={Type},defaultValue={DefaultValue})", flag, description, type, defaultValue);
        }

        /// <summary>Initializes a new instance of the <see cref="FlagParameterAttribute"/> class.</summary>
        /// <param name="flag">The flag name.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The expected type.</param>
        /// <param name="defaultValue">The default value.</param>
        public FlagParameterAttribute(string flag, string description, string type, string defaultValue)
            : this(flag, description, type, defaultValue, false)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FlagParameterAttribute"/> class.</summary>
        /// <param name="flag">The flag name.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The expected type.</param>
        public FlagParameterAttribute(string flag, string description, string type)
            : this(flag, description, type, string.Empty, false)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FlagParameterAttribute"/> class.</summary>
        /// <param name="flag">The flag name.</param>
        /// <param name="description">The description.</param>
        /// <param name="type">The expected type.</param>
        /// <param name="required">Whether the flag is required.</param>
        public FlagParameterAttribute(string flag, string description, string type, bool required)
            : this(flag, description, type, string.Empty, required)
        {
        }

        /// <summary>Gets or sets name of the flag.</summary>
        public string Flag { get; set; }

        /// <summary>Gets or sets type of the flag value expected.</summary>
        public string Type { get; set; }

        /// <summary>Gets or sets a value indicating whether is flag required or not.</summary>
        public bool Required { get; set; }

        /// <summary>Gets or sets default value of the flag.</summary>
        public string DefaultValue { get; set; }

        /// <summary>Gets or sets description of flag.</summary>
        public string Description { get; set; }
    }
}
