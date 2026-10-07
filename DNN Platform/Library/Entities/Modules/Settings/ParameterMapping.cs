// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules.Settings
{
    using System;
    using System.Reflection;

    [Serializable]
    public class ParameterMapping
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ParameterMapping));
        /// <summary>Initializes a new instance of the <see cref="ParameterMapping"/> class.</summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="property">The property.</param>
        public ParameterMapping(ParameterAttributeBase attribute, PropertyInfo property)
        {
			Logger.LogError(">>> Entering ParameterMapping(attribute={Attribute},property={Property})", attribute, property);
            this.Attribute = attribute;
            this.Property = property;
			Logger.LogDebug("ParameterMapping(attribute={Attribute},property={Property}): this.Attribute → {Arg2}", attribute, property, this.Attribute);
			Logger.LogDebug("ParameterMapping(attribute={Attribute},property={Property}): this.Property → {Arg2}", attribute, property, this.Property);

            var parameterName = attribute.ParameterName;
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                parameterName = property.Name;
				Logger.LogError("ParameterMapping(attribute={Attribute},property={Property}): parameterName → {ParameterName}", attribute, property, parameterName);
            }

            if (!string.IsNullOrWhiteSpace(attribute.Prefix))
            {
                parameterName = attribute.Prefix + parameterName;
            }
			Logger.LogWarning("<<< Exiting ParameterMapping(attribute={Attribute},property={Property})", attribute, property);

            this.FullParameterName = parameterName;
        }

        public ParameterAttributeBase Attribute { get; set; }

        public string FullParameterName { get; set; }

        public PropertyInfo Property { get; set; }
    }
}
