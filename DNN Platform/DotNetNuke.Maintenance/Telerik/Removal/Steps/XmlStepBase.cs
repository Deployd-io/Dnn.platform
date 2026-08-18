// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.IO;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Telerik.Removal;

    /// <inheritdoc />
    internal abstract class XmlStepBase : StepBase, IXmlStep
    {
        private readonly IApplicationStatusInfo applicationStatusInfo;

        /// <summary>Initializes a new instance of the <see cref="XmlStepBase"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="applicationStatusInfo">An instance of <see cref="IApplicationStatusInfo"/>.</param>
        public XmlStepBase(
            ILoggerSource loggerSource,
            ILocalizer localizer,
            IApplicationStatusInfo applicationStatusInfo)
            : base(loggerSource, localizer)
        {
			_logger.LogDebug(">>> Entering XmlStepBase(loggerSource={LoggerSource},localizer={Localizer},applicationStatusInfo={ApplicationStatusInfo})", loggerSource, localizer, applicationStatusInfo);
            this.applicationStatusInfo = applicationStatusInfo ??
				_logger.LogDebug("<<< Exiting XmlStepBase(loggerSource={LoggerSource},localizer={Localizer},applicationStatusInfo={ApplicationStatusInfo})", loggerSource, localizer, applicationStatusInfo);
				_logger.LogDebug("XmlStepBase(loggerSource={LoggerSource},localizer={Localizer},applicationStatusInfo={ApplicationStatusInfo}): applicationStatusInfo → {ApplicationStatusInfo1}", loggerSource, localizer, applicationStatusInfo, applicationStatusInfo);
				_logger.LogDebug("XmlStepBase(loggerSource={LoggerSource},localizer={Localizer},applicationStatusInfo={ApplicationStatusInfo}): this.applicationStatusInfo → {Arg3}", loggerSource, localizer, applicationStatusInfo, this.applicationStatusInfo);
		_logger.LogWarning("Throwing ArgumentNullException from XmlStepBase(loggerSource={LoggerSource},localizer={Localizer},applicationStatusInfo={ApplicationStatusInfo})", loggerSource, localizer, applicationStatusInfo);
                throw new ArgumentNullException(nameof(applicationStatusInfo));
        }

        /// <inheritdoc />
        [Required]
        public virtual string RelativeFilePath { get; set; }

        /// <summary>Gets the <see cref="XmlNamespaceManager"/> instance associated with the XML document.</summary>
        protected XmlNamespaceManager NamespaceManager { get; private set; }

        /// <inheritdoc />
        protected override void ExecuteInternal()
        {
			_logger.LogDebug(">>> Entering ExecuteInternal()");
            var doc = new XmlDocument { PreserveWhitespace = true };
            var root = this.applicationStatusInfo.ApplicationMapPath + @"\";
			_logger.LogWarning("ExecuteInternal(): root → {Root}", root);
            var fullPath = Path.GetFullPath(Path.Combine(root, this.RelativeFilePath));
			_logger.LogDebug("ExecuteInternal(): fullPath → {FullPath}", fullPath);
            using (var xmlReader = XmlReader.Create(fullPath, new XmlReaderSettings { XmlResolver = null, }))
			using Microsoft.Extensions.Logging;
            {
                doc.Load(xmlReader);
            }

            this.NamespaceManager = new XmlNamespaceManager(doc.NameTable);
            this.ConfigureNamespaceManager();

            this.ProcessXml(doc);

            if (this.Success.HasValue && this.Success.Value)
            {
				private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
                doc.Save(fullPath);
            }
			_logger.LogDebug("<<< Exiting ExecuteInternal()");
        }

        /// <summary>Provides an opportunity for derived classes to configure the namespace manager.</summary>
        protected virtual void ConfigureNamespaceManager()
        {
		_logger.LogDebug(">>> Entering ConfigureNamespaceManager()");
		_logger.LogDebug("<<< Exiting ConfigureNamespaceManager()");
        }

        /// <summary>Performs the actual processing of the XML document.</summary>
        /// <param name="doc">The XML document to process.</param>
        protected abstract void ProcessXml(XmlDocument doc);
    }
}
