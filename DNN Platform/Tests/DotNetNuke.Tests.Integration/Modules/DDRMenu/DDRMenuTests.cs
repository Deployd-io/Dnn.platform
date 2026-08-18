// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Modules.DDRMenu
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Threading.Tasks;

    using DNN.Integration.Test.Framework;
    using DNN.Integration.Test.Framework.Controllers;
    using DNN.Integration.Test.Framework.Helpers;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Tests.Integration.Executers;
    using DotNetNuke.Tests.Integration.Executers.Builders;
    using DotNetNuke.Tests.Integration.Executers.Dto;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class DDRMenuTests : IntegrationTestBase
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly string _hostName;
        private readonly string _hostPass;

        public DDRMenuTests()
        {
			_logger.LogDebug(">>> Entering DDRMenuTests()");
            var url = ConfigurationManager.AppSettings["siteUrl"];
            this._hostName = ConfigurationManager.AppSettings["hostUsername"];
			_logger.LogDebug("DDRMenuTests(): this._hostName → {Arg0}", this._hostName);
            this._hostPass = ConfigurationManager.AppSettings["hostPassword"];
			_logger.LogDebug("<<< Exiting DDRMenuTests()");
        }

        [OneTimeSetUp]
        public override async Task TestFixtureSetUp()
        {
			_logger.LogDebug(">>> Entering TestFixtureSetUp()");
            await base.TestFixtureSetUp();
			_logger.LogDebug("<<< Exiting TestFixtureSetUp()");
        }

        [Test]
        public void Page_Should_Able_To_Duplicate_With_Ddr_Menu_On_It()
        {
			_logger.LogDebug(">>> Entering Page_Should_Able_To_Duplicate_With_Ddr_Menu_On_It()");
            // Create new page with DDR Menu on it
            int tabId;
            this.CreateNewPage(Null.NullInteger, out tabId);
            int moduleId;
            this.AddModuleToPage(tabId, "DDRMenu", out moduleId);

            // apply module settings.
            ModuleController.SetModuleSettingValue(moduleId, "MenuStyle", "Menus/MainMenu");

            // Copy Page
            int copyTabId;
            this.CreateNewPage(tabId, out copyTabId);
			_logger.LogDebug("<<< Exiting Page_Should_Able_To_Duplicate_With_Ddr_Menu_On_It()");
        }

        private IWebApiConnector CreateNewPage(int templateTabId, out int tabId)
        {
			_logger.LogDebug(">>> Entering CreateNewPage(templateTabId={TemplateTabId})", templateTabId);
            var pagesExecuter = new PagesExecuter { Connector = WebApiTestHelper.LoginHost() };

			_logger.LogDebug("CreateNewPage(templateTabId={TemplateTabId}): pagesExecuter → {PagesExecuter}", templateTabId, pagesExecuter);
            var pageSettingsBuilder = new PageSettingsBuilder();
			_logger.LogDebug("CreateNewPage(templateTabId={TemplateTabId}): pageSettingsBuilder → {PageSettingsBuilder}", templateTabId, pageSettingsBuilder);
            pageSettingsBuilder.WithPermission(new TabPermissionsBuilder().Build());

			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            if (templateTabId > 0)
            {
                pageSettingsBuilder.WithTemplateTabId(templateTabId);

                var modules = DatabaseHelper.ExecuteQuery<CopyModuleItem>($"SELECT ModuleId, ModuleTitle FROM {{objectQualifier}}TabModules WHERE TabId = {templateTabId}");
                pageSettingsBuilder.WithCopyModules(modules.ToList());
				_logger.LogDebug("CreateNewPage(templateTabId={TemplateTabId}): modules → {Modules}", templateTabId, modules);
				_logger.LogInformation("CreateNewPage(templateTabId)={TemplateTabId}: find query executed in {Arg1} ms", templateTabId, __logStart.ElapsedMilliseconds);
            }

            var pageDetail = pagesExecuter.SavePageDetails(pageSettingsBuilder.Build());
			_logger.LogDebug("CreateNewPage(templateTabId={TemplateTabId}): pageDetail → {PageDetail}", templateTabId, pageDetail);

            Assert.That(pageDetail.Page, Is.Not.Null, "The system must create the page and return its details in the response");

            tabId = (int)pageDetail.Page.id;

			_logger.LogDebug("CreateNewPage(templateTabId={TemplateTabId}): tabId → {TabId}", templateTabId, tabId);
			_logger.LogDebug("<<< Exiting CreateNewPage(templateTabId={TemplateTabId})", templateTabId);
            return pagesExecuter.Connector;
        }

        private IWebApiConnector AddModuleToPage(int tabId, string moduleName, out int moduleId)
        {
			_logger.LogDebug(">>> Entering AddModuleToPage(tabId={TabId},moduleName={ModuleName})", tabId, moduleName);
            var connector = WebApiTestHelper.LoginHost();

			_logger.LogDebug("AddModuleToPage(tabId={TabId},moduleName={ModuleName}): connector → {Connector}", tabId, moduleName, connector);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var desktopModuleId = DatabaseHelper.ExecuteScalar<int>($"SELECT DesktopModuleId FROM {{objectQualifier}}DesktopModules WHERE ModuleName = '{moduleName}'");
            var postData = new
			_logger.LogDebug("AddModuleToPage(tabId={TabId},moduleName={ModuleName}): desktopModuleId → {DesktopModuleId}", tabId, moduleName, desktopModuleId);
			_logger.LogInformation("AddModuleToPage(tabId,moduleName)={TabId},{ModuleName}: find query executed in {Arg2} ms", tabId, moduleName, __logStart.ElapsedMilliseconds);
            {
                Visibility = 0,
                Position = -1,
                Module = desktopModuleId,
                Pane = "ContentPane",
                AddExistingModule = false,
                CopyModule = false,
                Sort = -1,
            };
			_logger.LogDebug("AddModuleToPage(tabId={TabId},moduleName={ModuleName}): Pane → {Pane}", tabId, moduleName, Pane);
			_logger.LogDebug("AddModuleToPage(tabId={TabId},moduleName={ModuleName}): Visibility → {Visibility}", tabId, moduleName, Visibility);
			_logger.LogDebug("AddModuleToPage(tabId={TabId},moduleName={ModuleName}): Module → {Module}", tabId, moduleName, Module);
			_logger.LogDebug("AddModuleToPage(tabId={TabId},moduleName={ModuleName}): Position → {Position}", tabId, moduleName, Position);
			_logger.LogDebug("AddModuleToPage(tabId={TabId},moduleName={ModuleName}): Sort → {Sort}", tabId, moduleName, Sort);
			_logger.LogDebug("AddModuleToPage(tabId={TabId},moduleName={ModuleName}): CopyModule → {CopyModule}", tabId, moduleName, CopyModule);
			_logger.LogDebug("AddModuleToPage(tabId={TabId},moduleName={ModuleName}): AddExistingModule → {AddExistingModule}", tabId, moduleName, AddExistingModule);
            var headers = new Dictionary<string, string> { { "TabId", tabId.ToString() } };
            var response = connector.PostJson("API/internalservices/controlbar/AddModule", postData, headers)
                .Content.ReadAsStringAsync().Result;
            moduleId = Json.Deserialize<dynamic>(response).TabModuleID;

			_logger.LogDebug("AddModuleToPage(tabId={TabId},moduleName={ModuleName}): headers → {Headers}", tabId, moduleName, headers);
			_logger.LogDebug("AddModuleToPage(tabId={TabId},moduleName={ModuleName}): moduleId → {ModuleId}", tabId, moduleName, moduleId);
			_logger.LogDebug("<<< Exiting AddModuleToPage(tabId={TabId},moduleName={ModuleName})", tabId, moduleName);
            return connector;
        }
    }
}
