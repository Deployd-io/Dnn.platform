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
	using Microsoft.Extensions.Logging;

    /// <inheritdoc cref="IRemoveItemFromCollectionStep" />
    internal sealed class RemoveItemFromCollectionStep : XmlStepBase, IRemoveItemFromCollectionStep
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
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
		_logger.LogDebug(">>> Entering RemoveItemFromCollectionStep(loggerSource={LoggerSource},localizer={Localizer},applicationStatusInfo={ApplicationStatusInfo})", loggerSource, localizer, applicationStatusInfo);
        }
		_logger.LogDebug("<<< Exiting RemoveItemFromCollectionStep(loggerSource={LoggerSource},localizer={Localizer},applicationStatusInfo={ApplicationStatusInfo})", loggerSource, localizer, applicationStatusInfo);

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
			_logger.LogDebug(">>> Entering ProcessXml(doc={Doc})", doc);
            this.Success = true;

			_logger.LogDebug("ProcessXml(doc={Doc}): this.Success → {Arg1}", doc, this.Success);
            var collection = doc.SelectSingleNode(this.CollectionPath);
			_logger.LogDebug("ProcessXml(doc={Doc}): collection → {Collection}", doc, collection);
            if (collection is null)
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
                ? this.LocalizeFormat("UninstallStepCountOfMatchesFound", matchCount)
                : this.Localize("UninstallStepNoMatchesFound");
				_logger.LogDebug("ProcessXml(doc={Doc}): this.Notes → {Arg1}", doc, this.Notes);
				_logger.LogDebug("<<< Exiting ProcessXml(doc={Doc})", doc);
        }

        private string JoinAttributesToIncludeInSearch(XmlElement element)
        {
			_logger.LogDebug(">>> Entering JoinAttributesToIncludeInSearch(element={Element})", element);
            var attributeValues = this.AttributeNamesToIncludeInSearch.Split(',')
                .Select(attrName => element.GetAttribute(attrName.Trim()));

			_logger.LogDebug("<<< Exiting JoinAttributesToIncludeInSearch(element={Element})", element);
            return string.Join("|", attributeValues);
        }
    }
}
