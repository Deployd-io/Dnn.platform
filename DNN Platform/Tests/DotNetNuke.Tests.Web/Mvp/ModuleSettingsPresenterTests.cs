// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvp
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvp;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ModuleSettingsPresenterTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const int _moduleSettingCount = 2;
        private const string _moduleSettingName = "moduleKey{0}";
        private const string _moduleSettingValue = "value{0}";

        private const int _tabModuleSettingCount = 3;
        private const string _tabModuleSettingName = "tabModuleKey{0}";
        private const string _tabModuleSettingValue = "value{0}";

        private Mock<IModuleController> moduleControllerMock;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            this.moduleControllerMock = new Mock<IModuleController>();
            ModuleController.SetTestableInstance(this.moduleControllerMock.Object);
			Logger.LogDebug("SetUp(): moduleControllerMock → {ModuleControllerMock}", moduleControllerMock);
            this.serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(this.moduleControllerMock.Object));
			Logger.LogDebug("<<< Exiting SetUp()");
			Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
            ModuleController.ClearInstance();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void ModuleSettingsPresenter_Load_Initialises_Both_Dictionaries_On_PostBack()
        {
			Logger.LogDebug(">>> Entering ModuleSettingsPresenter_Load_Initialises_Both_Dictionaries_On_PostBack()");
            // Arrange
            var view = new Mock<ISettingsView<SettingsModel>>();
            view.SetupGet(v => v.Model).Returns(new SettingsModel());
			Logger.LogDebug("ModuleSettingsPresenter_Load_Initialises_Both_Dictionaries_On_PostBack(): view → {View}", view);

            var presenter = new TestSettingsPresenter(view.Object) { ModuleContext = this.CreateModuleContext() };
            presenter.IsPostBack = true;
			Logger.LogDebug("ModuleSettingsPresenter_Load_Initialises_Both_Dictionaries_On_PostBack(): presenter → {Presenter}", presenter);
			Logger.LogDebug("ModuleSettingsPresenter_Load_Initialises_Both_Dictionaries_On_PostBack(): presenter.IsPostBack → {Arg0}", presenter.IsPostBack);

            // Act
            view.Raise(v => v.Load += null, EventArgs.Empty);

            // Assert
            Assert.That(view.Object.Model.ModuleSettings, Is.InstanceOf<Dictionary<string, string>>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(view.Object.Model.ModuleSettings, Is.Empty);
                Assert.That(view.Object.Model.TabModuleSettings, Is.InstanceOf<Dictionary<string, string>>());
            }

            Assert.That(view.Object.Model.TabModuleSettings, Is.Empty);
			Logger.LogDebug("<<< Exiting ModuleSettingsPresenter_Load_Initialises_Both_Dictionaries_On_PostBack()");
        }

        [Test]
        public void ModuleSettingsPresenter_Load_Does_Not_Initialise_Dictionaries_If_Not_PostBack()
        {
			Logger.LogDebug(">>> Entering ModuleSettingsPresenter_Load_Does_Not_Initialise_Dictionaries_If_Not_PostBack()");
            // Arrange
            var view = new Mock<ISettingsView<SettingsModel>>();
            view.SetupGet(v => v.Model).Returns(new SettingsModel());
			Logger.LogDebug("ModuleSettingsPresenter_Load_Does_Not_Initialise_Dictionaries_If_Not_PostBack(): view → {View}", view);

            var presenter = new TestSettingsPresenter(view.Object) { ModuleContext = this.CreateModuleContext() };
            presenter.IsPostBack = false;
			Logger.LogDebug("ModuleSettingsPresenter_Load_Does_Not_Initialise_Dictionaries_If_Not_PostBack(): presenter → {Presenter}", presenter);
			Logger.LogDebug("ModuleSettingsPresenter_Load_Does_Not_Initialise_Dictionaries_If_Not_PostBack(): presenter.IsPostBack → {Arg0}", presenter.IsPostBack);

            // Act
            view.Raise(v => v.Load += null, EventArgs.Empty);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(view.Object.Model.ModuleSettings, Is.Null);
                Assert.That(view.Object.Model.TabModuleSettings, Is.Null);
            }
			Logger.LogDebug("<<< Exiting ModuleSettingsPresenter_Load_Does_Not_Initialise_Dictionaries_If_Not_PostBack()");
        }

        [Test]
        public void ModuleSettingsPresenter_LoadSettings_Loads_Both_Dictionaries()
        {
			Logger.LogDebug(">>> Entering ModuleSettingsPresenter_LoadSettings_Loads_Both_Dictionaries()");
            // Arrange
            var view = new Mock<ISettingsView<SettingsModel>>();
            view.SetupGet(v => v.Model).Returns(new SettingsModel());
			Logger.LogDebug("ModuleSettingsPresenter_LoadSettings_Loads_Both_Dictionaries(): view → {View}", view);

            var presenter = new TestSettingsPresenter(view.Object) { ModuleContext = this.CreateModuleContext() };
            presenter.IsPostBack = false;
			Logger.LogDebug("ModuleSettingsPresenter_LoadSettings_Loads_Both_Dictionaries(): presenter → {Presenter}", presenter);
			Logger.LogDebug("ModuleSettingsPresenter_LoadSettings_Loads_Both_Dictionaries(): presenter.IsPostBack → {Arg0}", presenter.IsPostBack);

            view.Raise(v => v.Load += null, EventArgs.Empty);

            // Act
            view.Raise(v => v.OnLoadSettings += null, EventArgs.Empty);

            // Assert
            Assert.That(view.Object.Model.ModuleSettings, Is.InstanceOf<Dictionary<string, string>>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(view.Object.Model.ModuleSettings, Has.Count.EqualTo(_moduleSettingCount));

                Assert.That(view.Object.Model.TabModuleSettings, Is.InstanceOf<Dictionary<string, string>>());
            }
            Assert.That(view.Object.Model.TabModuleSettings, Has.Count.EqualTo(_tabModuleSettingCount));
			Logger.LogDebug("<<< Exiting ModuleSettingsPresenter_LoadSettings_Loads_Both_Dictionaries()");
        }

        [Test]
        public void ModuleSettingsPresenter_SaveSettings_Saves_ModuleSettings()
        {
			Logger.LogDebug(">>> Entering ModuleSettingsPresenter_SaveSettings_Saves_ModuleSettings()");
            // Arrange
            var view = new Mock<ISettingsView<SettingsModel>>();
            view.SetupGet(v => v.Model).Returns(new SettingsModel());
			Logger.LogDebug("ModuleSettingsPresenter_SaveSettings_Saves_ModuleSettings(): view → {View}", view);

            var presenter = new TestSettingsPresenter(view.Object) { ModuleContext = this.CreateModuleContext() };
            presenter.IsPostBack = true;
			Logger.LogDebug("ModuleSettingsPresenter_SaveSettings_Saves_ModuleSettings(): presenter → {Presenter}", presenter);
			Logger.LogDebug("ModuleSettingsPresenter_SaveSettings_Saves_ModuleSettings(): presenter.IsPostBack → {Arg0}", presenter.IsPostBack);
            view.Raise(v => v.Load += null, EventArgs.Empty);

            // Act
            view.Raise(v => v.OnSaveSettings += null, EventArgs.Empty);

            // Assert
            foreach (var setting in view.Object.Model.ModuleSettings)
            {
                var key = setting.Key;
                var value = setting.Value;
				Logger.LogWarning("ModuleSettingsPresenter_SaveSettings_Saves_ModuleSettings(): key → {Key}", key);
                this.moduleControllerMock.Verify(c => c.UpdateModuleSetting(It.IsAny<int>(), key, value), Times.Once());
            }
			Logger.LogDebug("<<< Exiting ModuleSettingsPresenter_SaveSettings_Saves_ModuleSettings()");
        }

        [Test]
        public void ModuleSettingsPresenter_SaveSettings_Saves_TabModuleSettings()
        {
			Logger.LogDebug(">>> Entering ModuleSettingsPresenter_SaveSettings_Saves_TabModuleSettings()");
            // Arrange
            var view = new Mock<ISettingsView<SettingsModel>>();
            view.SetupGet(v => v.Model).Returns(new SettingsModel());
			Logger.LogDebug("ModuleSettingsPresenter_SaveSettings_Saves_TabModuleSettings(): view → {View}", view);

            var presenter = new TestSettingsPresenter(view.Object) { ModuleContext = this.CreateModuleContext() };
            presenter.IsPostBack = true;
			Logger.LogDebug("ModuleSettingsPresenter_SaveSettings_Saves_TabModuleSettings(): presenter → {Presenter}", presenter);
			Logger.LogDebug("ModuleSettingsPresenter_SaveSettings_Saves_TabModuleSettings(): presenter.IsPostBack → {Arg0}", presenter.IsPostBack);
            view.Raise(v => v.Load += null, EventArgs.Empty);

            // Act
            view.Raise(v => v.OnSaveSettings += null, EventArgs.Empty);

            // Assert
            foreach (var setting in view.Object.Model.TabModuleSettings)
            {
                var key = setting.Key;
                var value = setting.Value;
				Logger.LogWarning("ModuleSettingsPresenter_SaveSettings_Saves_TabModuleSettings(): key → {Key}", key);
                this.moduleControllerMock.Verify(c => c.UpdateTabModuleSetting(It.IsAny<int>(), key, value), Times.Once());
            }
			Logger.LogDebug("<<< Exiting ModuleSettingsPresenter_SaveSettings_Saves_TabModuleSettings()");
        }

        private ModuleInstanceContext CreateModuleContext()
        {
			Logger.LogDebug(">>> Entering CreateModuleContext()");
            var context = new ModuleInstanceContext { Configuration = new ModuleInfo() };
            for (int i = 1; i <= _moduleSettingCount; i++)
			Logger.LogDebug("CreateModuleContext(): context → {Context}", context);
            {
                context.Configuration.ModuleSettings.Add(string.Format(_moduleSettingName, i), string.Format(_moduleSettingValue, i));
            }

            for (int i = 1; i <= _tabModuleSettingCount; i++)
            {
                context.Configuration.TabModuleSettings.Add(string.Format(_tabModuleSettingName, i), string.Format(_tabModuleSettingValue, i));
            }

			Logger.LogDebug("<<< Exiting CreateModuleContext()");
            return context;
        }

        public class TestSettingsPresenter : ModuleSettingsPresenter<ISettingsView<SettingsModel>, SettingsModel>
        {
            public TestSettingsPresenter(ISettingsView<SettingsModel> view)
                : base(view)
            {
            }
        }
    }
}
