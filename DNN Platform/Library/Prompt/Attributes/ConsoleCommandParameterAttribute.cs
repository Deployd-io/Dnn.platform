// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Prompt
{
    using System;

    /// <summary>An attribute decorating a property representing a parameter to a Prompt command.</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ConsoleCommandParameterAttribute : Attribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="ConsoleCommandParameterAttribute"/> class.</summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="descriptionKey">The resource key for the parameter description.</param>
        /// <param name="required">Whether the parameter is required.</param>
        /// <param name="defaultValue">The default value for the parameter.</param>
        public ConsoleCommandParameterAttribute(string name, string descriptionKey, bool required, string defaultValue)
        {
			Logger.LogDebug(">>> Entering ConsoleCommandParameterAttribute(name={Name},descriptionKey={DescriptionKey},required={Required},defaultValue={DefaultValue})", name, descriptionKey, required, defaultValue);
            this.Name = name;
            this.Required = required;
			Logger.LogDebug("ConsoleCommandParameterAttribute(name={Name},descriptionKey={DescriptionKey},required={Required},defaultValue={DefaultValue}): this.Name → {Arg4}", name, descriptionKey, required, defaultValue, this.Name);
			Logger.LogDebug("ConsoleCommandParameterAttribute(name={Name},descriptionKey={DescriptionKey},required={Required},defaultValue={DefaultValue}): this.Required → {Arg4}", name, descriptionKey, required, defaultValue, this.Required);
            this.DefaultValue = defaultValue;
            this.DescriptionKey = descriptionKey;
			Logger.LogDebug("<<< Exiting ConsoleCommandParameterAttribute(name={Name},descriptionKey={DescriptionKey},required={Required},defaultValue={DefaultValue})", name, descriptionKey, required, defaultValue);
			Logger.LogDebug("ConsoleCommandParameterAttribute(name={Name},descriptionKey={DescriptionKey},required={Required},defaultValue={DefaultValue}): this.DefaultValue → {Arg4}", name, descriptionKey, required, defaultValue, this.DefaultValue);
			Logger.LogDebug("ConsoleCommandParameterAttribute(name={Name},descriptionKey={DescriptionKey},required={Required},defaultValue={DefaultValue}): this.DescriptionKey → {Arg4}", name, descriptionKey, required, defaultValue, this.DescriptionKey);
			Logger.LogDebug(">>> Entering ConsoleCommandParameterAttribute(name={Name},descriptionKey={DescriptionKey},required={Required})", name, descriptionKey, required);
			Logger.LogDebug("<<< Exiting ConsoleCommandParameterAttribute(name={Name},descriptionKey={DescriptionKey},required={Required})", name, descriptionKey, required);
        }

        /// <summary>Initializes a new instance of the <see cref="ConsoleCommandParameterAttribute"/> class.</summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="descriptionKey">The resource key for the parameter description.</param>
        /// <param name="required">Whether the parameter is required.</param>
        public ConsoleCommandParameterAttribute(string name, string descriptionKey, bool required)
            : this(name, descriptionKey, required, string.Empty)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ConsoleCommandParameterAttribute"/> class for an optional parameter.</summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="descriptionKey">The resource key for the parameter description.</param>
        /// <param name="defaultValue">The default value for the parameter.</param>
        public ConsoleCommandParameterAttribute(string name, string descriptionKey, string defaultValue)
            : this(name, descriptionKey, false, defaultValue)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ConsoleCommandParameterAttribute"/> class for an optional parameter.</summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="descriptionKey">The resource key for the parameter description.</param>
        public ConsoleCommandParameterAttribute(string name, string descriptionKey)
            : this(name, descriptionKey, false, string.Empty)
        {
        }

        /// <summary>Gets or sets the name used in commands to access this parameter.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets a value indicating whether the parameter is required.</summary>
        public bool Required { get; set; }

        /// <summary>Gets or sets the default value serialized as string.</summary>
        public string DefaultValue { get; set; }

        /// <summary>Gets or sets the resource key for the description of this parameter.</summary>
        public string DescriptionKey { get; set; }
    }
}
