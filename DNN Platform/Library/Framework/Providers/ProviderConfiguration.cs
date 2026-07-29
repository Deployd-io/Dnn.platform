// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework.Providers
{
    using System.Collections;
    using System.Xml;

    using DotNetNuke.Common.Utilities;

    public class ProviderConfiguration
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly Hashtable providers = new Hashtable();
        private string defaultProvider;

        public string DefaultProvider
        {
            get
            {
                return this.defaultProvider;
            }
        }

        public Hashtable Providers
        {
            get
            {
                return this.providers;
            }
        }

        public static ProviderConfiguration GetProviderConfiguration(string strProvider)
        {
			Logger.LogDebug(">>> Entering GetProviderConfiguration(strProvider={StrProvider})", strProvider);
			Logger.LogDebug("<<< Exiting GetProviderConfiguration(strProvider={StrProvider})", strProvider);
            return (ProviderConfiguration)Config.GetSection("dotnetnuke/" + strProvider);
        }

        public Provider GetDefaultProvider()
        {
			Logger.LogDebug(">>> Entering GetDefaultProvider()");
			Logger.LogDebug("<<< Exiting GetDefaultProvider()");
            return (Provider)this.providers[this.defaultProvider];
        }

        internal void LoadValuesFromConfigurationXml(XmlNode node)
        {
			Logger.LogDebug(">>> Entering LoadValuesFromConfigurationXml(node={Node})", node);
            XmlAttributeCollection attributeCollection = node.Attributes;

            // Get the default provider
            this.defaultProvider = attributeCollection["defaultProvider"].Value;
			Logger.LogDebug("LoadValuesFromConfigurationXml(node={Node}): this.defaultProvider → {Arg1}", node, this.defaultProvider);

            // Read child nodes
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "providers")
                {
                    this.GetProviders(child);
                }
            }
			Logger.LogDebug("<<< Exiting LoadValuesFromConfigurationXml(node={Node})", node);
        }

        internal void GetProviders(XmlNode node)
        {
			Logger.LogDebug(">>> Entering GetProviders(node={Node})", node);
            foreach (XmlNode provider in node.ChildNodes)
            {
                switch (provider.Name)
                {
                    case "add":
                        this.Providers.Add(provider.Attributes["name"].Value, new Provider(provider.Attributes));
                        break;
                    case "remove":
                        this.Providers.Remove(provider.Attributes["name"].Value);
                        break;
                    case "clear":
                        this.Providers.Clear();
                        break;
                }
            }
			Logger.LogDebug("<<< Exiting GetProviders(node={Node})", node);
        }
    }
}
