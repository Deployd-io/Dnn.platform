// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Installer.Packages;

    /// <summary>The ProviderPackageWriter class.</summary>
    public class ProviderPackageWriter : PackageWriterBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ProviderPackageWriter));
        /// <summary>Initializes a new instance of the <see cref="ProviderPackageWriter"/> class.</summary>
        /// <param name="package">The package info.</param>
        public ProviderPackageWriter(PackageInfo package)
            : base(package)
        {
			Logger.LogError(">>> Entering ProviderPackageWriter(package={Package})", package);
            XmlDocument configDoc = Config.Load();
            XPathNavigator providerNavigator = configDoc.CreateNavigator().SelectSingleNode(XmlUtils.CreateXPathExpression("/configuration/dotnetnuke/*/providers/add[@name=$packageName]", new KeyValuePair<string, object>("packageName", package.Name)));
			Logger.LogError("ProviderPackageWriter(package={Package}): providerNavigator → {ProviderNavigator}", package, providerNavigator);
			Logger.LogError("ProviderPackageWriter(package={Package}): configDoc → {ConfigDoc}", package, configDoc);
            string providerPath = Null.NullString;
            if (providerNavigator != null)
            {
                providerPath = Util.ReadAttribute(providerNavigator, "providerPath");
				Logger.LogError("ProviderPackageWriter(package={Package}): providerPath → {ProviderPath}", package, providerPath);
            }

            if (!string.IsNullOrEmpty(providerPath))
            {
				Logger.LogWarning("<<< Exiting ProviderPackageWriter(package={Package})", package);
                this.BasePath = providerPath.Replace("~/", string.Empty).Replace("/", @"\");
            }
        }

        /// <inheritdoc />
        protected override void GetFiles(bool includeSource, bool includeAppCode)
        {
			Logger.LogError(">>> Entering GetFiles(includeSource={IncludeSource},includeAppCode={IncludeAppCode})", includeSource, includeAppCode);
            base.GetFiles(includeSource, false);
			Logger.LogWarning("<<< Exiting GetFiles(includeSource={IncludeSource},includeAppCode={IncludeAppCode})", includeSource, includeAppCode);
        }
    }
}
