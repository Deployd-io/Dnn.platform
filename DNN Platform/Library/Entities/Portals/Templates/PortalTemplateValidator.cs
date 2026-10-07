// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    using DotNetNuke.Common;

    /// <summary>The PortalTemplateValidator Class is used to validate the Portal Template.</summary>
    public class PortalTemplateValidator : XmlValidatorBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalTemplateValidator));
        public bool Validate(string xmlFilename, string schemaFileName)
        {
			Logger.LogError(">>> Entering Validate(xmlFilename={XmlFilename},schemaFileName={SchemaFileName})", xmlFilename, schemaFileName);
            this.SchemaSet.Add(string.Empty, schemaFileName);
			Logger.LogWarning("<<< Exiting Validate(xmlFilename={XmlFilename},schemaFileName={SchemaFileName})", xmlFilename, schemaFileName);
            return this.Validate(xmlFilename);
        }
    }
}
