// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>Base class of XmlValidator.</summary>
    public class XmlValidatorBase : IDisposable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private XmlTextReader reader;

        /// <summary>Initializes a new instance of the <see cref="XmlValidatorBase"/> class.</summary>
        public XmlValidatorBase()
        {
			Logger.LogDebug(">>> Entering XmlValidatorBase()");
            this.Errors = new ArrayList();
            this.SchemaSet = new XmlSchemaSet();
			Logger.LogDebug("<<< Exiting XmlValidatorBase()");
        }

        /// <summary>Gets the schema set.</summary>
        public XmlSchemaSet SchemaSet { get; }

        /// <summary>Gets or sets the errors.</summary>
        /// <value>The errors.</value>
        public ArrayList Errors { get; set; }

        /// <summary>Determines whether this instance is valid.</summary>
        /// <returns><see langword="true"/> if this instance is valid; otherwise, <see langword="false"/>.</returns>
        public bool IsValid()
        {
			Logger.LogDebug(">>> Entering IsValid()");
            // There is a bug here which I haven't been able to fix.
            // If the XML Instance does not include a reference to the
            // schema, then the validation fails.  If the reference exists
            // the validation works correctly.

            // Create a validating reader
            var settings = new XmlReaderSettings();
            settings.Schemas = this.SchemaSet;
			Logger.LogDebug("IsValid(): settings → {Settings}", settings);
			Logger.LogDebug("IsValid(): settings.Schemas → {Arg0}", settings.Schemas);
			Logger.LogDebug("IsValid(): settings.ValidationType → {Arg0}", settings.ValidationType);
            settings.ValidationType = ValidationType.Schema;

            // Set the validation event handler.
            settings.ValidationEventHandler += this.ValidationCallBack;
            using var vreader = XmlReader.Create(this.reader, settings);

			Logger.LogDebug("IsValid(): vreader → {Vreader}", vreader);
            // Read and validate the XML data.
            while (vreader.Read())
            {
            }

            vreader.Close();
			Logger.LogDebug("<<< Exiting IsValid()");
            return this.Errors.Count == 0;
        }

        /// <summary>Validates the specified XML stream.</summary>
        /// <param name="xmlStream">The XML stream.</param>
        /// <returns><see langword="true"/> if the XML is valid, otherwise <see langword="false"/>.</returns>
        public virtual bool Validate(Stream xmlStream)
        {
			Logger.LogDebug(">>> Entering Validate(xmlStream={XmlStream})", xmlStream);
            xmlStream.Seek(0, SeekOrigin.Begin);
            this.reader = new XmlTextReader(xmlStream)
            {
			Logger.LogDebug("Validate(xmlStream={XmlStream}): reader → {Reader}", xmlStream, reader);
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit,
            };
			Logger.LogDebug("<<< Exiting Validate(xmlStream={XmlStream})", xmlStream);
            return this.IsValid();
        }

        /// <summary>Validates the specified filename.</summary>
        /// <param name="filename">The filename.</param>
        /// <returns><see langword="true"/> if the XML is valid, otherwise <see langword="false"/>.</returns>
        public virtual bool Validate(string filename)
        {
            this.reader = new XmlTextReader(filename)
            {
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit,
            };
			Logger.LogDebug("<<< Exiting Validate(filename={Filename})", filename);
            return this.IsValid();
        }

        /// <inheritdoc />
        public void Dispose()
        {
			Logger.LogDebug(">>> Entering Dispose()");
            this.Dispose(true);
            GC.SuppressFinalize(this);
			Logger.LogDebug("<<< Exiting Dispose()");
        }

        protected virtual void Dispose(bool disposing)
        {
			Logger.LogDebug(">>> Entering Dispose(disposing={Disposing})", disposing);
            if (disposing)
            {
                this.reader?.Dispose();
            }
			Logger.LogDebug("<<< Exiting Dispose(disposing={Disposing})", disposing);
        }

        /// <summary>Validations the call back.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="System.Xml.Schema.ValidationEventArgs"/> instance containing the event data.</param>
        protected void ValidationCallBack(object sender, ValidationEventArgs args)
        {
			Logger.LogDebug(">>> Entering ValidationCallBack(sender={Sender},args={Args})", sender, args);
            this.Errors.Add(args.Message);
			Logger.LogDebug("<<< Exiting ValidationCallBack(sender={Sender},args={Args})", sender, args);
        }
    }
}
