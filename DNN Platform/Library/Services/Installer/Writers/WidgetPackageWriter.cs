// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System;
    using System.IO;
    using System.Xml;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Installer.Packages;

    /// <summary>The WidgetPackageWriter class.</summary>
    public class WidgetPackageWriter : PackageWriterBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WidgetPackageWriter));
        /// <summary>Initializes a new instance of the <see cref="WidgetPackageWriter"/> class.</summary>
        /// <param name="package">The package info.</param>
        public WidgetPackageWriter(PackageInfo package)
            : base(package)
        {
			Logger.LogError(">>> Entering WidgetPackageWriter(package={Package})", package);
            string company = package.Name;
			Logger.LogError("WidgetPackageWriter(package={Package}): company → {Company}", package, company);
            if (company.Contains(".", StringComparison.Ordinal))
            {
                company = company.Substring(0, company.IndexOf(".", StringComparison.Ordinal));
            }

			Logger.LogWarning("<<< Exiting WidgetPackageWriter(package={Package})", package);
            this.BasePath = Path.Combine(@"Resources\Widgets\User", company);
        }

        /// <inheritdoc />
        public override bool IncludeAssemblies
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc />
        protected override void GetFiles(bool includeSource, bool includeAppCode)
        {
			Logger.LogError(">>> Entering GetFiles(includeSource={IncludeSource},includeAppCode={IncludeAppCode})", includeSource, includeAppCode);
            // Call base class method with includeAppCode = false
            base.GetFiles(includeSource, false);
			Logger.LogWarning("<<< Exiting GetFiles(includeSource={IncludeSource},includeAppCode={IncludeAppCode})", includeSource, includeAppCode);
        }

        /// <inheritdoc />
        protected override void WriteFilesToManifest(XmlWriter writer)
        {
			Logger.LogError(">>> Entering WriteFilesToManifest(writer={Writer})", writer);
            string company = this.Package.Name.Substring(0, this.Package.Name.IndexOf(".", StringComparison.Ordinal));
			Logger.LogWarning("<<< Exiting WriteFilesToManifest(writer={Writer})", writer);
            var widgetFileWriter = new WidgetComponentWriter(company, this.Files, this.Package);
			Logger.LogError("WriteFilesToManifest(writer={Writer}): widgetFileWriter → {WidgetFileWriter}", writer, widgetFileWriter);
			Logger.LogError("WriteFilesToManifest(writer={Writer}): company → {Company}", writer, company);
            widgetFileWriter.WriteManifest(writer);
        }
    }
}
