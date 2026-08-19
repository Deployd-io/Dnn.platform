// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Entities.Modules;

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;

using NUnit.Framework;

[TestFixture]
public class DesktopModuleInfoTests
{
    [Test]
    public void ReadXml_WithUpgradeable_ReadsOneSupportedFeature()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithUpgradeable_ReadsOneSupportedFeature()");
        var desktopModule = ReadXml(
            """
            <desktopModule>
                <supportedFeatures>
                    <supportedFeature type="Upgradeable" />
                </supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(desktopModule.IsUpgradeable, Is.True);
            Assert.That(desktopModule.IsPortable, Is.False);
            Assert.That(desktopModule.IsSearchable, Is.False);
            Assert.That(desktopModule.SupportedFeatures, Is.EqualTo((int)DesktopModuleSupportedFeature.IsUpgradeable));
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithUpgradeable_ReadsOneSupportedFeature()");
    }

    [Test]
    public void ReadXml_WithPortable_ReadsOneSupportedFeature()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithPortable_ReadsOneSupportedFeature()");
        var desktopModule = ReadXml(
            """
            <desktopModule>
                <supportedFeatures>
                    <supportedFeature type="Portable" />
                </supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(desktopModule.IsUpgradeable, Is.False);
            Assert.That(desktopModule.IsPortable, Is.True);
            Assert.That(desktopModule.IsSearchable, Is.False);
            Assert.That(desktopModule.SupportedFeatures, Is.EqualTo((int)DesktopModuleSupportedFeature.IsPortable));
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithPortable_ReadsOneSupportedFeature()");
    }

    [Test]
    public void ReadXml_WithSearchable_ReadsOneSupportedFeature()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithSearchable_ReadsOneSupportedFeature()");
        var desktopModule = ReadXml(
            """
            <desktopModule>
                <supportedFeatures>
                    <supportedFeature type="Searchable" />
                </supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(desktopModule.IsUpgradeable, Is.False);
            Assert.That(desktopModule.IsPortable, Is.False);
            Assert.That(desktopModule.IsSearchable, Is.True);
            Assert.That(desktopModule.SupportedFeatures, Is.EqualTo((int)DesktopModuleSupportedFeature.IsSearchable));
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithSearchable_ReadsOneSupportedFeature()");
    }

    [Test]
    public void ReadXml_WithSearchableAndPortable_ReadsTwoSupportedFeatures()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithSearchableAndPortable_ReadsTwoSupportedFeatures()");
        var desktopModule = ReadXml(
            """
            <desktopModule>
                <supportedFeatures>
                    <supportedFeature type="Searchable" />
                    <supportedFeature type="Portable" />
                </supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(desktopModule.IsUpgradeable, Is.False);
            Assert.That(desktopModule.IsPortable, Is.True);
            Assert.That(desktopModule.IsSearchable, Is.True);
            Assert.That(desktopModule.SupportedFeatures, Is.EqualTo((int)DesktopModuleSupportedFeature.IsSearchable + (int)DesktopModuleSupportedFeature.IsPortable));
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithSearchableAndPortable_ReadsTwoSupportedFeatures()");
    }

    [Test]
    public void ReadXml_WithSearchableAndUpgradeable_ReadsTwoSupportedFeatures()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithSearchableAndUpgradeable_ReadsTwoSupportedFeatures()");
        var desktopModule = ReadXml(
            """
            <desktopModule>
                <supportedFeatures>
                    <supportedFeature type="Searchable" />
                    <supportedFeature type="Upgradeable" />
                </supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(desktopModule.IsUpgradeable, Is.True);
            Assert.That(desktopModule.IsPortable, Is.False);
            Assert.That(desktopModule.IsSearchable, Is.True);
            Assert.That(desktopModule.SupportedFeatures, Is.EqualTo((int)DesktopModuleSupportedFeature.IsSearchable + (int)DesktopModuleSupportedFeature.IsUpgradeable));
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithSearchableAndUpgradeable_ReadsTwoSupportedFeatures()");
    }

    [Test] public void ReadXml_WithPortableAndUpgradeable_ReadsTwoSupportedFeatures()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithPortableAndUpgradeable_ReadsTwoSupportedFeatures()");
        var desktopModule = ReadXml(
            """
            <desktopModule>
                <supportedFeatures>
                    <supportedFeature type="Portable" />
                    <supportedFeature type="Upgradeable" />
                </supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(desktopModule.IsUpgradeable, Is.True);
            Assert.That(desktopModule.IsPortable, Is.True);
            Assert.That(desktopModule.IsSearchable, Is.False);
            Assert.That(desktopModule.SupportedFeatures, Is.EqualTo((int)DesktopModuleSupportedFeature.IsPortable + (int)DesktopModuleSupportedFeature.IsUpgradeable));
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithPortableAndUpgradeable_ReadsTwoSupportedFeatures()");
    }

    [Test] public void ReadXml_WithAllThreeFeatures_ReadsThreeSupportedFeatures()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithAllThreeFeatures_ReadsThreeSupportedFeatures()");
        var desktopModule = ReadXml(
            """
            <desktopModule>
                <supportedFeatures>
                    <supportedFeature type="Portable" />
                    <supportedFeature type="Upgradeable" />
                    <supportedFeature type="Searchable" />
                </supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(desktopModule.IsUpgradeable, Is.True);
            Assert.That(desktopModule.IsPortable, Is.True);
            Assert.That(desktopModule.IsSearchable, Is.True);
            Assert.That(desktopModule.SupportedFeatures, Is.EqualTo((int)DesktopModuleSupportedFeature.IsPortable + (int)DesktopModuleSupportedFeature.IsUpgradeable + (int)DesktopModuleSupportedFeature.IsSearchable));
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithAllThreeFeatures_ReadsThreeSupportedFeatures()");
    }

    [Test]
    public void ReadXml_WithEmptySupportedFeatures_DoesNotInitializeFeatures()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithEmptySupportedFeatures_DoesNotInitializeFeatures()");
        var desktopModule = ReadXml(
            """
            <desktopModule>
                <supportedFeatures></supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(desktopModule.SupportedFeatures, Is.EqualTo(Null.NullInteger));
            Assert.That(desktopModule.IsUpgradeable, Is.False);
            Assert.That(desktopModule.IsPortable, Is.False);
            Assert.That(desktopModule.IsSearchable, Is.False);
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithEmptySupportedFeatures_DoesNotInitializeFeatures()");
    }

    [Test]
    public void ReadXml_WithEmptySupportedFeaturesIncludingWhiteSpace_DoesNotInitializeFeatures()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithEmptySupportedFeaturesIncludingWhiteSpace_DoesNotInitializeFeatures()");
        var desktopModule = ReadXml(
            """
            <desktopModule>
                <supportedFeatures>
                </supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(desktopModule.SupportedFeatures, Is.EqualTo(Null.NullInteger));
            Assert.That(desktopModule.IsUpgradeable, Is.False);
            Assert.That(desktopModule.IsPortable, Is.False);
            Assert.That(desktopModule.IsSearchable, Is.False);
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithEmptySupportedFeaturesIncludingWhiteSpace_DoesNotInitializeFeatures()");
    }

    [Test]
    public void ReadXml_WithNoSupportedFeatures_DoesNotInitializeFeatures()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithNoSupportedFeatures_DoesNotInitializeFeatures()");
        var desktopModule = ReadXml(
            """
            <desktopModule>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(desktopModule.SupportedFeatures, Is.EqualTo(Null.NullInteger));
            Assert.That(desktopModule.IsUpgradeable, Is.False);
            Assert.That(desktopModule.IsPortable, Is.False);
            Assert.That(desktopModule.IsSearchable, Is.False);
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithNoSupportedFeatures_DoesNotInitializeFeatures()");
    }

    [Test]
    public void ReadXml_WithSelfClosingSupportedFeatures_DoesNotInitializeFeatures()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithSelfClosingSupportedFeatures_DoesNotInitializeFeatures()");
        var desktopModule = ReadXml(
            """
            <desktopModule>
                <supportedFeatures />
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(desktopModule.SupportedFeatures, Is.EqualTo(Null.NullInteger));
            Assert.That(desktopModule.IsUpgradeable, Is.False);
            Assert.That(desktopModule.IsPortable, Is.False);
            Assert.That(desktopModule.IsSearchable, Is.False);
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithSelfClosingSupportedFeatures_DoesNotInitializeFeatures()");
    }

    [Test]
    public void ReadXml_WithSelfClosingSupportedFeatures_ReadsRemainingInformationCorrectly()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithSelfClosingSupportedFeatures_ReadsRemainingInformationCorrectly()");
        var desktopModule = ReadXml(
            """
            <desktopModule>
                <moduleName>Console</moduleName>
                <supportedFeatures />
                <foldername>Admin/Console</foldername>
                <businessControllerClass>Dnn.Modules.Console.Components.BusinessController</businessControllerClass>
                <moduleDefinitions>
                    <moduleDefinition>
                        <friendlyName>Console2</friendlyName>
                        <moduleControls>
                            <moduleControl>
                                <controlKey/>
                                <controlSrc>DesktopModules/Admin/Console/ViewConsole.ascx</controlSrc>
                                <controlTitle>Console</controlTitle>
                                <controlType>View</controlType>
                                <iconFile></iconFile>
                                <helpUrl>http://help.dotnetnuke.com/070100/default.htm#Documentation/Building Your Site/Installed Modules/Console/About the Console Module.htm</helpUrl>
                                <viewOrder>0</viewOrder>
                                <supportsPartialRendering>True</supportsPartialRendering>
                                <supportsPopUps>True</supportsPopUps>
                            </moduleControl>
                            <moduleControl>
                                <controlKey>Settings</controlKey>
                                <controlSrc>DesktopModules/Admin/Console/Settings.ascx</controlSrc>
                                <controlTitle>Console Settings</controlTitle>
                                <controlType>Admin</controlType>
                                <iconFile></iconFile>
                                <helpUrl></helpUrl>
                                <viewOrder>0</viewOrder>
                                <supportsPartialRendering>True</supportsPartialRendering>
                                <supportsPopUps>True</supportsPopUps>
                            </moduleControl>
                        </moduleControls>
                    </moduleDefinition>
                </moduleDefinitions>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(desktopModule.ModuleName, Is.EqualTo("Console"));
            Assert.That(desktopModule.FolderName, Is.EqualTo("Admin/Console"));
            Assert.That(desktopModule.BusinessControllerClass, Is.EqualTo("Dnn.Modules.Console.Components.BusinessController"));
            Assert.That(desktopModule.ModuleDefinitions, Has.Count.EqualTo(1));
            var definition = desktopModule.ModuleDefinitions.Values.Single();
			mockEventLogger.LogDebug("ReadXml_WithSelfClosingSupportedFeatures_ReadsRemainingInformationCorrectly(): definition → {Definition}", definition);
            Assert.That(definition.FriendlyName, Is.EqualTo("Console2"));
            Assert.That(definition.ModuleControls, Has.Count.EqualTo(2));
            var viewControl = definition.ModuleControls[string.Empty];
            Assert.That(viewControl.ControlKey, Is.Empty);
            Assert.That(viewControl.ControlSrc, Is.EqualTo("DesktopModules/Admin/Console/ViewConsole.ascx"));
            Assert.That(viewControl.ControlTitle, Is.EqualTo("Console"));
            Assert.That(viewControl.ControlType, Is.EqualTo(SecurityAccessLevel.View));
            Assert.That(viewControl.IconFile, Is.Empty);
            Assert.That(viewControl.HelpURL, Is.EqualTo("http://help.dotnetnuke.com/070100/default.htm#Documentation/Building Your Site/Installed Modules/Console/About the Console Module.htm"));
            Assert.That(viewControl.ViewOrder, Is.Zero);
            Assert.That(viewControl.SupportsPartialRendering, Is.True);
            Assert.That(viewControl.SupportsPopUps, Is.True);
            var settingsControl = definition.ModuleControls["Settings"];
			mockEventLogger.LogDebug("ReadXml_WithSelfClosingSupportedFeatures_ReadsRemainingInformationCorrectly(): settingsControl → {SettingsControl}", settingsControl);
            Assert.That(settingsControl.ControlKey, Is.EqualTo("Settings"));
            Assert.That(settingsControl.ControlSrc, Is.EqualTo("DesktopModules/Admin/Console/Settings.ascx"));
            Assert.That(settingsControl.ControlTitle, Is.EqualTo("Console Settings"));
            Assert.That(settingsControl.ControlType, Is.EqualTo(SecurityAccessLevel.Admin));
            Assert.That(settingsControl.IconFile, Is.Empty);
            Assert.That(settingsControl.HelpURL, Is.Empty);
            Assert.That(settingsControl.ViewOrder, Is.Zero);
            Assert.That(settingsControl.SupportsPartialRendering, Is.True);
            Assert.That(settingsControl.SupportsPopUps, Is.True);
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithSelfClosingSupportedFeatures_ReadsRemainingInformationCorrectly()");
    }

    [Test]
    public void ReadXml_WithSelfClosingModuleDefinitions_ReadsRemainingInformationCorrectly()
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml_WithSelfClosingModuleDefinitions_ReadsRemainingInformationCorrectly()");
        var desktopModule = ReadXml(
            """
            <desktopModule>
                <moduleName>Console</moduleName>
                <moduleDefinitions />
                <foldername>Admin/Console</foldername>
                <businessControllerClass>Dnn.Modules.Console.Components.BusinessController</businessControllerClass>
                <supportedFeatures>
                    <supportedFeature type="Upgradeable" />
                </supportedFeatures>
            </desktopModule>
            """);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(desktopModule.ModuleName, Is.EqualTo("Console"));
            Assert.That(desktopModule.FolderName, Is.EqualTo("Admin/Console"));
            Assert.That(desktopModule.BusinessControllerClass, Is.EqualTo("Dnn.Modules.Console.Components.BusinessController"));
            Assert.That(desktopModule.ModuleDefinitions, Has.Count.EqualTo(0));
            Assert.That(desktopModule.IsUpgradeable, Is.True);
            Assert.That(desktopModule.IsPortable, Is.False);
            Assert.That(desktopModule.IsSearchable, Is.False);
            Assert.That(desktopModule.SupportedFeatures, Is.EqualTo((int)DesktopModuleSupportedFeature.IsUpgradeable));
        }
		mockEventLogger.LogDebug("<<< Exiting ReadXml_WithSelfClosingModuleDefinitions_ReadsRemainingInformationCorrectly()");
    }

    private static DesktopModuleInfo ReadXml([StringSyntax(StringSyntaxAttribute.Xml)] string xml)
    {
		mockEventLogger.LogDebug(">>> Entering ReadXml(xml={Xml})", xml);
        var desktopModule = new DesktopModuleInfo();

		mockEventLogger.LogDebug("ReadXml(xml={Xml}): desktopModule → {DesktopModule}", xml, desktopModule);
        using var textReader = new StringReader(xml);
		mockEventLogger.LogDebug("ReadXml(xml={Xml}): textReader → {TextReader}", xml, textReader);
        using var xmlReader = XmlReader.Create(textReader);
		mockEventLogger.LogDebug("ReadXml(xml={Xml}): xmlReader → {XmlReader}", xml, xmlReader);
        xmlReader.Read();
        desktopModule.ReadXml(xmlReader);

		mockEventLogger.LogDebug("<<< Exiting ReadXml(xml={Xml})", xml);
        return desktopModule;
    }
}
