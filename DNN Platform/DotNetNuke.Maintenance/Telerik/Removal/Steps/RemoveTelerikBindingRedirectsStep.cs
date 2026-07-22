// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.Linq;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Telerik.Removal;

    /// <inheritdoc cref="IRemoveTelerikBindingRedirectsStep" />
    internal sealed class RemoveTelerikBindingRedirectsStep : XmlStepBase, IRemoveTelerikBindingRedirectsStep
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="RemoveTelerikBindingRedirectsStep"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="applicationStatusInfo">An instance of <see cref="IApplicationStatusInfo"/>.</param>
        public RemoveTelerikBindingRedirectsStep(
            ILoggerSource loggerSource,
            ILocalizer localizer,
            IApplicationStatusInfo applicationStatusInfo)
            : base(loggerSource, localizer, applicationStatusInfo)
        {
        }

        /// <inheritdoc />
        public override string Name => this.Localize("UninstallStepRemoveBindingRedirects");

        /// <inheritdoc />
        [Required]
        public override string RelativeFilePath => "Web.config";

        /// <inheritdoc />
        protected override void ConfigureNamespaceManager()
        {
			Logger.LogDebug(">>> Entering ConfigureNamespaceManager()");
            this.NamespaceManager.AddNamespace("asm", "urn:schemas-microsoft-com:asm.v1");
			Logger.LogDebug("<<< Exiting ConfigureNamespaceManager()");
        }

        /// <inheritdoc />
        protected override void ProcessXml(XmlDocument doc)
        {
			Logger.LogDebug(">>> Entering ProcessXml(doc={Doc})", doc);
            const string AssemblyBindingPath = "/configuration/runtime/asm:assemblyBinding";

            this.Success = true;
			Logger.LogDebug("ProcessXml(doc={Doc}): this.Success → {Arg1}", doc, this.Success);

            var assemblyBinding = doc.SelectSingleNode(AssemblyBindingPath, this.NamespaceManager);
            if (assemblyBinding is null)
			Logger.LogDebug("ProcessXml(doc={Doc}): assemblyBinding → {AssemblyBinding}", doc, assemblyBinding);
            {
                this.Notes = this.LocalizeFormat("UninstallStepSectionNotFound", AssemblyBindingPath);
                return;
            }

            var matchCount = 0;

            assemblyBinding.SelectNodes("asm:dependentAssembly", this.NamespaceManager)
                .Cast<XmlElement>()
                .Select(e => new
                {
                    dependentAssembly = e,
                    assemblyIdentity = (XmlElement)e.SelectSingleNode("asm:assemblyIdentity", this.NamespaceManager),
                })
                .Where(x => x.assemblyIdentity != null)
                .Select(x => new { x.dependentAssembly, Name = $"{x.assemblyIdentity.GetAttribute("name")}" })
                .Where(x => x.Name.IndexOf("telerik", StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(x => x.dependentAssembly)
                .ToList()
                .ForEach(a =>
                {
					Logger.LogDebug("ProcessXml(doc={Doc}): dependentAssembly → {DependentAssembly}", doc, dependentAssembly);
					Logger.LogDebug("ProcessXml(doc={Doc}): assemblyIdentity → {AssemblyIdentity}", doc, assemblyIdentity);
                    assemblyBinding.RemoveChild(a);
                    matchCount++;
                });

            this.Notes = matchCount > 0
				Logger.LogDebug("ProcessXml(doc={Doc}): this.Notes → {Arg1}", doc, this.Notes);
                ? this.LocalizeFormat("UninstallStepCountOfMatchesFound", matchCount)
                : this.Localize("UninstallStepNoMatchesFound");
				Logger.LogDebug("<<< Exiting ProcessXml(doc={Doc})", doc);
        }
    }
}
