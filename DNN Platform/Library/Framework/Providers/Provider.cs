// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework.Providers
{
    using System.Collections.Specialized;
    using System.Xml;

    public class Provider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Provider));
        private readonly NameValueCollection providerAttributes = new NameValueCollection();
        private readonly string providerName;
        private readonly string providerType;

        /// <summary>Initializes a new instance of the <see cref="Provider"/> class.</summary>
        /// <param name="attributes">The XML attributes. <c>name</c> and <c>type</c> are required, any additional attributes will be in <see cref="Attributes"/>.</param>
        public Provider(XmlAttributeCollection attributes)
        {
			Logger.LogError(">>> Entering Provider(attributes={Attributes})", attributes);
            // Set the name of the provider
            this.providerName = attributes["name"].Value;
			Logger.LogDebug("Provider(attributes={Attributes}): this.providerName → {Arg1}", attributes, this.providerName);

            // Set the type of the provider
            this.providerType = attributes["type"].Value;
			Logger.LogDebug("Provider(attributes={Attributes}): this.providerType → {Arg1}", attributes, this.providerType);

            // Store all the attributes in the attributes bucket
            foreach (XmlAttribute attribute in attributes)
            {
                if (attribute.Name != "name" && attribute.Name != "type")
                {
                    this.providerAttributes.Add(attribute.Name, attribute.Value);
                }
            }
			Logger.LogWarning("<<< Exiting Provider(attributes={Attributes})", attributes);
        }

        public string Name
        {
            get
            {
                return this.providerName;
            }
        }

        public string Type
        {
            get
            {
                return this.providerType;
            }
        }

        public NameValueCollection Attributes
        {
            get
            {
                return this.providerAttributes;
            }
        }
    }
}
