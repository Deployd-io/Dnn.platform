// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework.Providers
{
    using System.Configuration;
    using System.Xml;

    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>Configuration section for a provider.</summary>
    [DnnDeprecated(7, 0, 0, "It is no longer used to load provider configurations, as there are medium trust issues", RemovalVersion = 10)]
    internal partial class ProviderConfigurationHandler : IConfigurationSectionHandler
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public virtual object Create(object parent, object context, XmlNode node)
        {
			Logger.LogDebug(">>> Entering Create(parent={Parent},context={Context},node={Node})", parent, context, node);
            var objProviderConfiguration = new ProviderConfiguration();
            objProviderConfiguration.LoadValuesFromConfigurationXml(node);
			Logger.LogDebug("Create(parent={Parent},context={Context},node={Node}): objProviderConfiguration → {ObjProviderConfiguration}", parent, context, node, objProviderConfiguration);
			Logger.LogDebug("<<< Exiting Create(parent={Parent},context={Context},node={Node})", parent, context, node);
            return objProviderConfiguration;
        }
    }
}
