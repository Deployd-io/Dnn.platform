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
			eventLogger.LogDebug(">>> Entering TemplateController()");
            this.tabController = TabController.Instance;
			eventLogger.LogDebug("TemplateController(): tabController → {TabController}", tabController);
			eventLogger.LogDebug("TemplateController(): this.tabController → {Arg0}", this.tabController);
			eventLogger.LogDebug("<<< Exiting TemplateController()");
			eventLogger.LogDebug("TemplateController(businessControllerProvider={BusinessControllerProvider}): this.tabController → {Arg1}", businessControllerProvider, this.tabController);
        }

        /// <summary>Initializes a new instance of the <see cref="TemplateController"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        public TemplateController(IBusinessControllerProvider businessControllerProvider)
        {
            this.businessControllerProvider = businessControllerProvider ?? Globals.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
            this.tabController = TabController.Instance;
			eventLogger.LogDebug("TemplateController(businessControllerProvider={BusinessControllerProvider}): tabController → {TabController}", businessControllerProvider, tabController);
        }

        /// <inheritdoc />
        public string SaveAsTemplate(PageTemplate template)
        {
			eventLogger.LogDebug(">>> Entering SaveAsTemplate(template={Template})", template);
            string filename;
            try
            {
                var folder = GetTemplateFolder() ?? CreateTemplateFolder();
				eventLogger.LogDebug("SaveAsTemplate(template={Template}): folder → {Folder}", template, folder);

                filename = $"{folder.FolderPath}{template.Name}.page.template";
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
			eventLogger.LogDebug("SaveAsTemplate(template={Template}): node → {Node}", template, node);
			eventLogger.LogDebug("SaveAsTemplate(template={Template}): nodePortal → {NodePortal}", template, nodePortal);
			eventLogger.LogDebug("SaveAsTemplate(template={Template}): fileContent → {FileContent}", template, fileContent);
			eventLogger.LogDebug("SaveAsTemplate(template={Template}): filename → {Filename}", template, filename);
            {
                throw new TemplateException("Error accessing to the templates folder.");
            }
            catch (Exception)
			eventLogger.LogError("Exception in SaveAsTemplate(template={Template}): {Arg1}", template, PermissionsNotMetException.Message);
            {
				eventLogger.LogError("Exception in SaveAsTemplate(template={Template}): {Arg1}", template, Exception.Message);
			eventLogger.LogDebug("<<< Exiting SaveAsTemplate(template={Template})", template);
                throw new TemplateException("Error accessing to the templates folder.");
            }

            return filename;
        }

        /// <inheritdoc />
        public IEnumerable<Template> GetTemplates()
        {
			eventLogger.LogDebug(">>> Entering GetTemplates()");
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var templateFolder = FolderManager.Instance.GetFolder(portalSettings.PortalId, TemplatesFolderPath);
			eventLogger.LogDebug("GetTemplates(): templateFolder → {TemplateFolder}", templateFolder);
			eventLogger.LogDebug("GetTemplates(): portalSettings → {PortalSettings}", portalSettings);

			eventLogger.LogDebug("<<< Exiting GetTemplates()");
            return LoadTemplates(portalSettings.PortalId, templateFolder);
        }

        /// <inheritdoc />
        public int GetDefaultTemplateId(IEnumerable<Template> templates)
        {
            var firstOrDefault = templates.FirstOrDefault(t => t.Id == "Default");
            if (firstOrDefault != null)
            {
                return firstOrDefault.Value;
            }

            return Null.NullInteger;
        }

        /// <inheritdoc />
        public void CreatePageFromTemplate(int templateId, TabInfo tab, int portalId)
        {
            // create the page from a template
            if (templateId != Null.NullInteger)
            {
                var xmlDoc = new XmlDocument { XmlResolver = null };
                try
                {
                    // open the XML file
                    var fileId = Convert.ToInt32(templateId);
                    var templateFile = FileManager.Instance.GetFile(fileId);
                    using var templateStream = FileManager.Instance.GetFileContent(templateFile);
                    using var templateReader = XmlReader.Create(templateStream, new XmlReaderSettings { XmlResolver = null, });
                    xmlDoc.Load(templateReader);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
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
                        }

                        TabController.Instance.UpdateTab(tab);
                    }

                    tabIndex++;
                }

                if (!string.IsNullOrEmpty(exceptions))
                {
                    throw new PageException(exceptions);
                }
				eventLogger.LogDebug(">>> Entering CreateTemplateFolder()");
				eventLogger.LogDebug("<<< Exiting CreateTemplateFolder()");
            }
			eventLogger.LogDebug("<<< Exiting CreatePageFromTemplate(templateId={TemplateId},tab={Tab},portalId={PortalId})", templateId, tab, portalId);
        }

        /// <inheritdoc />
        protected override Func<ITemplateController> GetFactory()
        {
			eventLogger.LogDebug(">>> Entering GetFactory()");
			eventLogger.LogDebug("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<ITemplateController>;
        }

        private static IFolderInfo GetTemplateFolder()
        {
			eventLogger.LogDebug(">>> Entering GetTemplateFolder()");
			eventLogger.LogDebug("<<< Exiting GetTemplateFolder()");
            return FolderManager.Instance.GetFolder(PortalSettings.Current.PortalId, TemplatesFolderPath);
        }

        private static IFolderInfo CreateTemplateFolder()
        {
			eventLogger.LogDebug(">>> Entering CreateTemplateFolder()");
			eventLogger.LogDebug("<<< Exiting CreateTemplateFolder()");
            return FolderManager.Instance.AddFolder(PortalSettings.Current.PortalId, TemplatesFolderPath);
        }

        private static List<Template> LoadTemplates(int portalId, IFolderInfo templateFolder)
        {
            var templates = new List<Template>();
            if (templateFolder == null)
			eventLogger.LogDebug("LoadTemplates(portalId={PortalId},templateFolder={TemplateFolder}): templates → {Templates}", portalId, templateFolder, templates);
            {
                return templates;
            }

            templates.Add(new Template
            {
                Id = Localization.GetString("None_Specified"),
                Value = Null.NullInteger,
            });

            var files = Globals.GetFileList(portalId, "page.template", false, templateFolder.FolderPath);
			eventLogger.LogDebug("LoadTemplates(portalId={PortalId},templateFolder={TemplateFolder}): files → {Files}", portalId, templateFolder, files);
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

			eventLogger.LogDebug("<<< Exiting LoadTemplates(portalId={PortalId},templateFolder={TemplateFolder})", portalId, templateFolder);
            return templates;
        }

        private void SerializeTab(PageTemplate template, XmlDocument xmlTemplate, XmlNode nodeTabs)
        {
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var tab = this.tabController.GetTab(template.TabId, portalSettings.PortalId, false);
            var xmlTab = new XmlDocument { XmlResolver = null };
            var nodeTab = TabController.SerializeTab(this.businessControllerProvider, xmlTab, tab, template.IncludeContent);
            nodeTabs.AppendChild(xmlTemplate.ImportNode(nodeTab, true));
			eventLogger.LogDebug("<<< Exiting SerializeTab(template={Template},xmlTemplate={XmlTemplate},nodeTabs={NodeTabs})", template, xmlTemplate, nodeTabs);
        }
    }
}
