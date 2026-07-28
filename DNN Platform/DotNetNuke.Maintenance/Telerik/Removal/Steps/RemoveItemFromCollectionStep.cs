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

    /// <inheritdoc cref="IRemoveItemFromCollectionStep" />
    internal sealed class RemoveItemFromCollectionStep : XmlStepBase, IRemoveItemFromCollectionStep
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="RemoveItemFromCollectionStep"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="applicationStatusInfo">An instance of <see cref="IApplicationStatusInfo"/>.</param>
        public RemoveItemFromCollectionStep(
            ILoggerSource loggerSource,
            ILocalizer localizer,
            IApplicationStatusInfo applicationStatusInfo)
            : base(loggerSource, localizer, applicationStatusInfo)
        {
        }

        /// <inheritdoc />
        [Required]
        public string CollectionPath { get; set; }

        /// <inheritdoc />
        [Required]
        public string AttributeNamesToIncludeInSearch { get; set; }

        /// <inheritdoc />
        [Required]
        public string SearchTerm { get; set; }

        /// <inheritdoc />
        protected override void ProcessXml(XmlDocument doc)
        {
			Logger.LogDebug(">>> Entering ProcessXml(doc={Doc})", doc);
            this.Success = true;

			Logger.LogDebug("ProcessXml(doc={Doc}): this.Success → {Arg1}", doc, this.Success);
            var collection = doc.SelectSingleNode(this.CollectionPath);
            if (collection is null)
			Logger.LogDebug("ProcessXml(doc={Doc}): collection → {Collection}", doc, collection);
            {
                this.Notes = this.LocalizeFormat("UninstallStepSectionNotFound", this.CollectionPath);
                return;
            }

            var matchCount = 0;

            collection.SelectNodes("add|remove", this.NamespaceManager)
                .Cast<XmlElement>()
                .Select(e => new { Item = e, Text = this.JoinAttributesToIncludeInSearch(e) })
                .Where(x => x.Text.IndexOf(this.SearchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                .Select(x => x.Item)
                .ToList()
                .ForEach(item =>
                {
                    collection.RemoveChild(item);
                    matchCount++;
                });

            this.Notes = matchCount > 0
				Logger.LogDebug("ProcessXml(doc={Doc}): this.Notes → {Arg1}", doc, this.Notes);
                ? this.LocalizeFormat("UninstallStepCountOfMatchesFound", matchCount)
                : this.Localize("UninstallStepNoMatchesFound");
				Logger.LogDebug("<<< Exiting ProcessXml(doc={Doc})", doc);
        }

        private string JoinAttributesToIncludeInSearch(XmlElement element)
        {
			Logger.LogDebug(">>> Entering JoinAttributesToIncludeInSearch(element={Element})", element);
            var attributeValues = this.AttributeNamesToIncludeInSearch.Split(',')
                .Select(attrName => element.GetAttribute(attrName.Trim()));

			Logger.LogDebug("<<< Exiting JoinAttributesToIncludeInSearch(element={Element})", element);
            return string.Join("|", attributeValues);
        }
    }
}
