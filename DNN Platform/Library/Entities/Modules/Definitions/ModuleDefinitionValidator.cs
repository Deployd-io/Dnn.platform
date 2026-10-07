// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Definitions
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;

    public enum ModuleDefinitionVersion
    {
        /// <summary>An unknown version.</summary>
        VUnknown = 0,

        /// <summary>Version one.</summary>
        V1 = 1,

        /// <summary>Version two.</summary>
        V2 = 2,

        /// <summary>Version two of a skin.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        V2_Skin = 3,

        /// <summary>Version two of a provider.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        V2_Provider = 4,

        /// <summary>Version three.</summary>
        V3 = 5,
    }

    public class ModuleDefinitionValidator : XmlValidatorBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleDefinitionValidator));
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public ModuleDefinitionVersion GetModuleDefinitionVersion(Stream xmlStream)
        {
			Logger.LogError(">>> Entering GetModuleDefinitionVersion(xmlStream={XmlStream})", xmlStream);
            ModuleDefinitionVersion retValue;
            xmlStream.Seek(0, SeekOrigin.Begin);
            var xmlReader = new XmlTextReader(xmlStream)
            {
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit,
            };
            xmlReader.MoveToContent();

            // This test assumes provides a simple validation
            switch (xmlReader.LocalName.ToLowerInvariant())
            {
                case "module":
                    retValue = ModuleDefinitionVersion.V1;
					Logger.LogError("GetModuleDefinitionVersion(xmlStream={XmlStream}): retValue → {RetValue}", xmlStream, retValue);
					Logger.LogDebug("GetModuleDefinitionVersion(xmlStream={XmlStream}): retValue → {RetValue}", xmlStream, retValue);
                    break;
                case "dotnetnuke":
                    switch (xmlReader.GetAttribute("type"))
                    {
                        case "Module":
                            switch (xmlReader.GetAttribute("version"))
                            {
                                case "2.0":
                                    retValue = ModuleDefinitionVersion.V2;
                                    break;
                                case "3.0":
                                    retValue = ModuleDefinitionVersion.V3;
                                    break;
                                default:
                                    return ModuleDefinitionVersion.VUnknown;
                            }

                            break;
                        case "SkinObject":
                            retValue = ModuleDefinitionVersion.V2_Skin;
                            break;
                        case "Provider":
                            retValue = ModuleDefinitionVersion.V2_Provider;
                            break;
                        default:
                            retValue = ModuleDefinitionVersion.VUnknown;
                            break;
                    }

                    break;
                default:
                    retValue = ModuleDefinitionVersion.VUnknown;
                    break;
            }

			Logger.LogWarning("<<< Exiting GetModuleDefinitionVersion(xmlStream={XmlStream})", xmlStream);
            return retValue;
        }

        /// <inheritdoc />
        public override bool Validate(Stream xmlStream)
        {
			Logger.LogError(">>> Entering Validate(xmlStream={XmlStream})", xmlStream);
            this.SchemaSet.Add(string.Empty, this.GetDnnSchemaPath(xmlStream));
			Logger.LogWarning("<<< Exiting Validate(xmlStream={XmlStream})", xmlStream);
            return base.Validate(xmlStream);
        }

        private static string GetLocalizedString(string key)
        {
			Logger.LogError(">>> Entering GetLocalizedString(key={Key})", key);
            var objPortalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
			Logger.LogError("GetLocalizedString(key={Key}): objPortalSettings → {ObjPortalSettings}", key, objPortalSettings);
            if (objPortalSettings == null)
            {
                return key;
            }

			Logger.LogWarning("<<< Exiting GetLocalizedString(key={Key})", key);
            return Localization.GetString(key, objPortalSettings);
        }

        private string GetDnnSchemaPath(Stream xmlStream)
        {
			Logger.LogError(">>> Entering GetDnnSchemaPath(xmlStream={XmlStream})", xmlStream);
            ModuleDefinitionVersion version = this.GetModuleDefinitionVersion(xmlStream);
            string schemaPath = string.Empty;
			Logger.LogError("GetDnnSchemaPath(xmlStream={XmlStream}): version → {Version}", xmlStream, version);
            switch (version)
            {
                case ModuleDefinitionVersion.V2:
                    schemaPath = "components\\ResourceInstaller\\ModuleDef_V2.xsd";
					Logger.LogError("GetDnnSchemaPath(xmlStream={XmlStream}): schemaPath → {SchemaPath}", xmlStream, schemaPath);
                    break;
                case ModuleDefinitionVersion.V3:
                    schemaPath = "components\\ResourceInstaller\\ModuleDef_V3.xsd";
                    break;
                case ModuleDefinitionVersion.V2_Skin:
                    schemaPath = "components\\ResourceInstaller\\ModuleDef_V2Skin.xsd";
                    break;
                case ModuleDefinitionVersion.V2_Provider:
                    schemaPath = "components\\ResourceInstaller\\ModuleDef_V2Provider.xsd";
                    break;
                case ModuleDefinitionVersion.VUnknown:
					Logger.LogError("Throwing UnknownModuleDefinitionVersionException from GetDnnSchemaPath(xmlStream={XmlStream})", xmlStream);
			Logger.LogWarning("<<< Exiting GetDnnSchemaPath(xmlStream={XmlStream})", xmlStream);
                    throw new UnknownModuleDefinitionVersionException(GetLocalizedString("EXCEPTION_LoadFailed"));
            }

            return Path.Combine(Globals.ApplicationMapPath, schemaPath);
        }
    }
}
