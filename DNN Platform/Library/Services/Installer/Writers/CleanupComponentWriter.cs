// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// The CleanupComponentWriter class handles creating the manifest for Cleanup
    /// Component(s).
    /// </summary>
    public class CleanupComponentWriter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CleanupComponentWriter));
        private readonly SortedList<string, InstallFile> files;
        private string basePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="CleanupComponentWriter"/> class.
        /// Constructs the ContainerComponentWriter.
        /// </summary>
        /// <param name="basePath">Base Path.</param>
        /// <param name="files">A Dictionary of files.</param>
        public CleanupComponentWriter(string basePath, SortedList<string, InstallFile> files)
        {
			Logger.LogDebug(">>> Entering CleanupComponentWriter(basePath={BasePath},files={Files})", basePath, files);
            this.files = files;
            this.basePath = basePath;
			Logger.LogDebug("CleanupComponentWriter(basePath={BasePath},files={Files}): this.files → {Arg2}", basePath, files, this.files);
			Logger.LogDebug("<<< Exiting CleanupComponentWriter(basePath={BasePath},files={Files})", basePath, files);
        }

        public virtual void WriteManifest(XmlWriter writer)
        {
			Logger.LogDebug(">>> Entering WriteManifest(writer={Writer})", writer);
            foreach (KeyValuePair<string, InstallFile> kvp in this.files)
            {
                // Start component Element
                writer.WriteStartElement("component");
                writer.WriteAttributeString("type", "Cleanup");
                writer.WriteAttributeString("fileName", kvp.Value.Name);
                writer.WriteAttributeString("version", Path.GetFileNameWithoutExtension(kvp.Value.Name));

                // End component Element
                writer.WriteEndElement();
            }
			Logger.LogDebug("<<< Exiting WriteManifest(writer={Writer})", writer);
        }
    }
}
