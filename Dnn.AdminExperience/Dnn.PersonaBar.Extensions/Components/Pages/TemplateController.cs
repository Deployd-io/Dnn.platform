// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Pages.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Xml;

    using Dnn.PersonaBar.Pages.Components.Dto;
    using Dnn.PersonaBar.Pages.Components.Exceptions;
    using Dnn.PersonaBar.Pages.Services.Dto;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Web.UI;

    using Microsoft.Extensions.DependencyInjection;

    public class TemplateController : ServiceLocator<ITemplateController, TemplateController>, ITemplateController
    {
        private const string TemplatesFolderPath = "Templates/";

        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly ITabController tabController;

        /// <summary>Initializes a new instance of the <see cref="TemplateController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public TemplateController()
            : this(null)
        {
			eventLogger.LogError(">>> Entering TemplateController()");
            this.tabController = TabController.Instance;
			eventLogger.LogDebug("TemplateController(): tabController → {TabController}", tabController);
        eventLogger.LogDebug("TemplateController(): this.tabController → {Arg0}", this.tabController);
        eventLogger.LogDebug("TemplateController(businessControllerProvider={BusinessControllerProvider}): this.tabController → {Arg1}", businessControllerProvider, this.tabController);
        eventLogger.LogWarning("<<< Exiting TemplateController()");
        }

        /// <summary>Initializes a new instance of the <see cref="TemplateController"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        public TemplateController(IBusinessControllerProvider businessControllerProvider)
        {
			eventLogger.LogDebug("TemplateController(businessControllerProvider={BusinessControllerProvider}): tabController → {TabController}", businessControllerProvider, tabController);
            this.businessControllerProvider = businessControllerProvider ?? Globals.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
            this.tabController = TabController.Instance;
        }

        /// <inheritdoc />
        public string SaveAsTemplate(PageTemplate template)
        {
			eventLogger.LogError(">>> Entering SaveAsTemplate(template={Template})", template);
            string filename;
            try
            {
				eventLogger.LogError("Exception in SaveAsTemplate(template={Template}): {Arg1}", template, Exception.Message);
                var folder = GetTemplateFolder() ?? CreateTemplateFolder();
				eventLogger.LogError("SaveAsTemplate(template={Template}): folder → {Folder}", template, folder);

                filename = $"{folder.FolderPath}{template.Name}.page.template";
				eventLogger.LogError("SaveAsTemplate(template={Template}): filename → {Filename}", template, filename);
                filename = filename.Replace("/", @"\");

                var xmlTemplate = new XmlDocument { XmlResolver = null };
                var nodePortal = xmlTemplate.AppendChild(xmlTemplate.CreateElement("portal"));
                nodePortal.Attributes?.Append(XmlUtils.CreateAttribute(xmlTemplate, "version", "3.0"));

                // Add template description
                var node = xmlTemplate.CreateElement("description");
                node.InnerXml = HttpUtility.HtmlEncode(template.Description);
                nodePortal.AppendChild(node);

                // Serialize tabs
                var nodeTabs = nodePortal.AppendChild(xmlTemplate.CreateElement("tabs"));
                this.SerializeTab(template, xmlTemplate, nodeTabs);

                // add file to Files table
                const bool permissionsCheckedByCallingMethod = true;
                using var fileContent = new MemoryStream(Encoding.UTF8.GetBytes(xmlTemplate.OuterXml));
                FileManager.Instance.AddFile(
                    folder,
                    $"{template.Name}.page.template",
                    fileContent,
                    true,
                    !permissionsCheckedByCallingMethod,
                    "application/octet-stream");
            }
            catch (DotNetNuke.Services.FileSystem.PermissionsNotMetException)
            {
				eventLogger.LogError("Exception in SaveAsTemplate(template={Template}): {Arg1}", template, PermissionsNotMetException.Message);
				eventLogger.LogDebug("SaveAsTemplate(template={Template}): filename → {Filename}", template, filename);
                throw new TemplateException("Error accessing to the templates folder.");
            }
            catch (Exception)
            {
			eventLogger.LogWarning("<<< Exiting SaveAsTemplate(template={Template})", template);
                throw new TemplateException("Error accessing to the templates folder.");
            }

            return filename;
        }

        /// <inheritdoc />
        public IEnumerable<Template> GetTemplates()
        {
			eventLogger.LogError(">>> Entering GetTemplates()");
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var templateFolder = FolderManager.Instance.GetFolder(portalSettings.PortalId, TemplatesFolderPath);
			eventLogger.LogError("GetTemplates(): templateFolder → {TemplateFolder}", templateFolder);
			eventLogger.LogError("GetTemplates(): portalSettings → {PortalSettings}", portalSettings);

			eventLogger.LogWarning("<<< Exiting GetTemplates()");
            return LoadTemplates(portalSettings.PortalId, templateFolder);
        }

        /// <inheritdoc />
        public int GetDefaultTemplateId(IEnumerable<Template> templates)
        {
			eventLogger.LogError(">>> Entering GetDefaultTemplateId(templates={Templates})", templates);
            var firstOrDefault = templates.FirstOrDefault(t => t.Id == "Default");
			eventLogger.LogError("GetDefaultTemplateId(templates={Templates}): firstOrDefault → {FirstOrDefault}", templates, firstOrDefault);
            if (firstOrDefault != null)
            {
                return firstOrDefault.Value;
            }

			eventLogger.LogWarning("<<< Exiting GetDefaultTemplateId(templates={Templates})", templates);
            return Null.NullInteger;
        }

        /// <inheritdoc />
        public void CreatePageFromTemplate(int templateId, TabInfo tab, int portalId)
        {
			eventLogger.LogError(">>> Entering CreatePageFromTemplate(templateId={TemplateId},tab={Tab},portalId={PortalId})", templateId, tab, portalId);
            // create the page from a template
            if (templateId != Null.NullInteger)
            {
                var xmlDoc = new XmlDocument { XmlResolver = null };
                try
                {
                    // open the XML file
                    var fileId = Convert.ToInt32(templateId);
					eventLogger.LogError("CreatePageFromTemplate(templateId={TemplateId},tab={Tab},portalId={PortalId}): fileId → {FileId}", templateId, tab, portalId, fileId);
                    var templateFile = FileManager.Instance.GetFile(fileId);
					eventLogger.LogError("CreatePageFromTemplate(templateId={TemplateId},tab={Tab},portalId={PortalId}): templateFile → {TemplateFile}", templateId, tab, portalId, templateFile);
                    using var templateStream = FileManager.Instance.GetFileContent(templateFile);
					eventLogger.LogError("CreatePageFromTemplate(templateId={TemplateId},tab={Tab},portalId={PortalId}): templateStream → {TemplateStream}", templateId, tab, portalId, templateStream);
                    using var templateReader = XmlReader.Create(templateStream, new XmlReaderSettings { XmlResolver = null, });
					eventLogger.LogError("CreatePageFromTemplate(templateId={TemplateId},tab={Tab},portalId={PortalId}): templateReader → {TemplateReader}", templateId, tab, portalId, templateReader);
                    xmlDoc.Load(templateReader);
                }
                catch (Exception ex)
                {
					eventLogger.LogError("Exception in CreatePageFromTemplate(templateId={TemplateId},tab={Tab},portalId={PortalId}): {Arg3}", templateId, tab, portalId, ex.Message);
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
					eventLogger.LogError("Throwing PageException from CreatePageFromTemplate(templateId={TemplateId},tab={Tab},portalId={PortalId})", templateId, tab, portalId);
                    throw new PageException(Localization.GetString("BadTemplate"));
                }

                TabController.DeserializePanes(this.businessControllerProvider, xmlDoc.SelectSingleNode("//portal/tabs/tab/panes"), tab.PortalID, tab.TabID, PortalTemplateModuleAction.Ignore, new Hashtable());

                // save tab permissions
                RibbonBarManager.DeserializeTabPermissions(xmlDoc.SelectNodes("//portal/tabs/tab/tabpermissions/permission"), tab);

                var tabIndex = 0;
                var exceptions = string.Empty;

                // ReSharper disable once PossibleNullReferenceException
                foreach (XmlNode tabNode in xmlDoc.SelectSingleNode("//portal/tabs").ChildNodes)
                {
                    // Create second tab onward tabs. Note first tab is already created above.
                    if (tabIndex > 0)
                    {
                        try
                        {
                            TabController.DeserializeTab(this.businessControllerProvider, tabNode, null, portalId, PortalTemplateModuleAction.Replace);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                            exceptions += $"Template Tab # {tabIndex + 1}. Error {ex.Message}<br/>";
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(tab.SkinSrc) && !string.IsNullOrEmpty(XmlUtils.GetNodeValue(tabNode, "skinsrc", string.Empty)))
                        {
                            tab.SkinSrc = XmlUtils.GetNodeValue(tabNode, "skinsrc", string.Empty);
                        }

                        if (string.IsNullOrEmpty(tab.ContainerSrc) && !string.IsNullOrEmpty(XmlUtils.GetNodeValue(tabNode, "containersrc", string.Empty)))
                        {
                            tab.ContainerSrc = XmlUtils.GetNodeValue(tabNode, "containersrc", string.Empty);
                        }

                        bool isSecure;
                        if (bool.TryParse(XmlUtils.GetNodeValue(tabNode, "issecure", string.Empty), out isSecure))
                        {
                            tab.IsSecure = isSecure;
							eventLogger.LogDebug("CreatePageFromTemplate(templateId={TemplateId},tab={Tab},portalId={PortalId}): tab.IsSecure → {Arg3}", templateId, tab, portalId, tab.IsSecure);
                        }

                        TabController.Instance.UpdateTab(tab);
                    }

                    tabIndex++;
                }

                if (!string.IsNullOrEmpty(exceptions))
                {
                    throw new PageException(exceptions);
                }
				eventLogger.LogError(">>> Entering CreateTemplateFolder()");
            eventLogger.LogWarning("<<< Exiting CreateTemplateFolder()");
            }
			eventLogger.LogWarning("<<< Exiting CreatePageFromTemplate(templateId={TemplateId},tab={Tab},portalId={PortalId})", templateId, tab, portalId);
        }

        /// <inheritdoc />
        protected override Func<ITemplateController> GetFactory()
        {
			eventLogger.LogError(">>> Entering GetFactory()");
			eventLogger.LogWarning("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<ITemplateController>;
        }

        private static IFolderInfo GetTemplateFolder()
        {
			eventLogger.LogError(">>> Entering GetTemplateFolder()");
            eventLogger.LogWarning("<<< Exiting GetTemplateFolder()");
            return FolderManager.Instance.GetFolder(PortalSettings.Current.PortalId, TemplatesFolderPath);
        }

        private static IFolderInfo CreateTemplateFolder()
        {
			eventLogger.LogError(">>> Entering CreateTemplateFolder()");
            eventLogger.LogWarning("<<< Exiting CreateTemplateFolder()");
            return FolderManager.Instance.AddFolder(PortalSettings.Current.PortalId, TemplatesFolderPath);
        }

        private static List<Template> LoadTemplates(int portalId, IFolderInfo templateFolder)
        {
			eventLogger.LogError(">>> Entering LoadTemplates(portalId={PortalId},templateFolder={TemplateFolder})", portalId, templateFolder);
            var templates = new List<Template>();
			eventLogger.LogError("LoadTemplates(portalId={PortalId},templateFolder={TemplateFolder}): templates → {Templates}", portalId, templateFolder, templates);
            if (templateFolder == null)
            {
                return templates;
            }

            templates.Add(new Template
            {
                Id = Localization.GetString("None_Specified"),
                Value = Null.NullInteger,
            });

            var files = Globals.GetFileList(portalId, "page.template", false, templateFolder.FolderPath);
			eventLogger.LogError("LoadTemplates(portalId={PortalId},templateFolder={TemplateFolder}): files → {Files}", portalId, templateFolder, files);
            foreach (FileItem file in files)
            {
                if (!int.TryParse(file.Value, out var i))
                {
                    i = 0;
                }

                templates.Add(new Template
                {
                    Id = file.Text.Replace(".page.template", string.Empty),
                    Value = i,
                });
            }

			eventLogger.LogWarning("<<< Exiting LoadTemplates(portalId={PortalId},templateFolder={TemplateFolder})", portalId, templateFolder);
            return templates;
        }

        private void SerializeTab(PageTemplate template, XmlDocument xmlTemplate, XmlNode nodeTabs)
        {
			eventLogger.LogError(">>> Entering SerializeTab(template={Template},xmlTemplate={XmlTemplate},nodeTabs={NodeTabs})", template, xmlTemplate, nodeTabs);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var tab = this.tabController.GetTab(template.TabId, portalSettings.PortalId, false);
			eventLogger.LogError("SerializeTab(template={Template},xmlTemplate={XmlTemplate},nodeTabs={NodeTabs}): tab → {Tab}", template, xmlTemplate, nodeTabs, tab);
			eventLogger.LogError("SerializeTab(template={Template},xmlTemplate={XmlTemplate},nodeTabs={NodeTabs}): portalSettings → {PortalSettings}", template, xmlTemplate, nodeTabs, portalSettings);
            var xmlTab = new XmlDocument { XmlResolver = null };
            var nodeTab = TabController.SerializeTab(this.businessControllerProvider, xmlTab, tab, template.IncludeContent);
			eventLogger.LogError("SerializeTab(template={Template},xmlTemplate={XmlTemplate},nodeTabs={NodeTabs}): nodeTab → {NodeTab}", template, xmlTemplate, nodeTabs, nodeTab);
            nodeTabs.AppendChild(xmlTemplate.ImportNode(nodeTab, true));
			eventLogger.LogWarning("<<< Exiting SerializeTab(template={Template},xmlTemplate={XmlTemplate},nodeTabs={NodeTabs})", template, xmlTemplate, nodeTabs);
        }
    }
}
