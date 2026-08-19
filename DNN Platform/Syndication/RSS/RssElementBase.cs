// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Collections.Generic;

    /// <summary>The base class for all RSS elements (item, image, channel) has collection of attributes.</summary>
    public abstract class RssElementBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RssElementBase));
        /// <summary>Gets the attributes.</summary>
        protected internal Dictionary<string, string> Attributes { get; private set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>When overridden in a derived class, initializes the attributes to the defaults.</summary>
        public virtual void SetDefaults()
        {
		Logger.LogDebug(">>> Entering SetDefaults()");
		Logger.LogDebug("<<< Exiting SetDefaults()");
        }

        /// <summary>Sets the element attributes.</summary>
        /// <param name="attributes">The attributes.</param>
        internal void SetAttributes(Dictionary<string, string> attributes)
        {
			Logger.LogDebug(">>> Entering SetAttributes(attributes={Attributes})", attributes);
            this.Attributes = attributes;
			Logger.LogDebug("SetAttributes(attributes={Attributes}): this.Attributes → {Arg1}", attributes, this.Attributes);
			Logger.LogDebug("<<< Exiting SetAttributes(attributes={Attributes})", attributes);
        }

        /// <summary>Gets the attribute value.</summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>The value or <see cref="string.Empty"/>.</returns>
        protected string GetAttributeValue(string attributeName)
        {
			Logger.LogDebug(">>> Entering GetAttributeValue(attributeName={AttributeName})", attributeName);
            string attributeValue;

            if (!this.Attributes.TryGetValue(attributeName, out attributeValue))
            {
                attributeValue = string.Empty;
				Logger.LogDebug("GetAttributeValue(attributeName={AttributeName}): attributeValue → {AttributeValue}", attributeName, attributeValue);
            }

			Logger.LogDebug("<<< Exiting GetAttributeValue(attributeName={AttributeName})", attributeName);
            return attributeValue;
        }

        /// <summary>Sets the value of an attribute.</summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="attributeValue">The attribute value.</param>
        protected void SetAttributeValue(string attributeName, string attributeValue)
        {
			Logger.LogDebug(">>> Entering SetAttributeValue(attributeName={AttributeName},attributeValue={AttributeValue})", attributeName, attributeValue);
            this.Attributes[attributeName] = attributeValue;
			Logger.LogDebug("<<< Exiting SetAttributeValue(attributeName={AttributeName},attributeValue={AttributeValue})", attributeName, attributeValue);
        }
    }
}
