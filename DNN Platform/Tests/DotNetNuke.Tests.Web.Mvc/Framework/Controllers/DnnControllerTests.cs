// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Framework.Controllers
{
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Tests.Web.Mvc.Fakes;
    using DotNetNuke.Web.Mvc.Framework.ActionResults;
    using NUnit.Framework;

    [TestFixture]
    public class DnnControllerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string TestViewName = "Foo";

        [Test]
        public void ActivePage_Property_Is_Null_If_PortalSettings_Not_Set_In_Context()
        {
			Logger.LogDebug(">>> Entering ActivePage_Property_Is_Null_If_PortalSettings_Not_Set_In_Context()");
            // Arrange
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

			Logger.LogDebug("ActivePage_Property_Is_Null_If_PortalSettings_Not_Set_In_Context(): httpContextBase → {HttpContextBase}", httpContextBase);
            // Act
            var controller = this.SetupController(httpContextBase);

			Logger.LogDebug("ActivePage_Property_Is_Null_If_PortalSettings_Not_Set_In_Context(): controller → {Controller}", controller);
            // Assert
            Assert.That(controller.ActivePage, Is.Null);
			Logger.LogDebug("<<< Exiting ActivePage_Property_Is_Null_If_PortalSettings_Not_Set_In_Context()");
        }

        [Test]
        public void PortalSettings_Property_Is_Null_If_Not_Set_In_Context()
        {
			Logger.LogDebug(">>> Entering PortalSettings_Property_Is_Null_If_Not_Set_In_Context()");
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();

			Logger.LogDebug("PortalSettings_Property_Is_Null_If_Not_Set_In_Context(): context → {Context}", context);
            // Act
            var controller = this.SetupController(context);

			Logger.LogDebug("PortalSettings_Property_Is_Null_If_Not_Set_In_Context(): controller → {Controller}", controller);
            // Assert
            Assert.That(controller.PortalSettings, Is.Null);
			Logger.LogDebug("<<< Exiting PortalSettings_Property_Is_Null_If_Not_Set_In_Context()");
        }

        [Test]
        public void ResultOfLastExecute_Returns_ViewResult()
        {
			Logger.LogDebug(">>> Entering ResultOfLastExecute_Returns_ViewResult()");
            // Arrange
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

			Logger.LogDebug("ResultOfLastExecute_Returns_ViewResult(): httpContextBase → {HttpContextBase}", httpContextBase);
            // Act
            var controller = this.SetupController(httpContextBase);
            controller.ActionInvoker.InvokeAction(controller.ControllerContext, "Action1");
			Logger.LogDebug("ResultOfLastExecute_Returns_ViewResult(): controller → {Controller}", controller);

            // Assert
            Assert.That(controller.ResultOfLastExecute, Is.Not.Null);
            Assert.That(controller.ResultOfLastExecute, Is.InstanceOf<DnnViewResult>());
			Logger.LogDebug("<<< Exiting ResultOfLastExecute_Returns_ViewResult()");
        }

        [Test]
        public void User_Property_Is_Null_If_PortalSettings_Not_Set_In_Context()
        {
			Logger.LogDebug(">>> Entering User_Property_Is_Null_If_PortalSettings_Not_Set_In_Context()");
            // Arrange
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

			Logger.LogDebug("User_Property_Is_Null_If_PortalSettings_Not_Set_In_Context(): httpContextBase → {HttpContextBase}", httpContextBase);
            // Act
            var controller = this.SetupController(httpContextBase);

			Logger.LogDebug("User_Property_Is_Null_If_PortalSettings_Not_Set_In_Context(): controller → {Controller}", controller);
            // Assert
            Assert.That(controller.User, Is.Null);
			Logger.LogDebug("<<< Exiting User_Property_Is_Null_If_PortalSettings_Not_Set_In_Context()");
        }

        [Test]
        public void View_Returns_DnnViewResult()
        {
			Logger.LogDebug(">>> Entering View_Returns_DnnViewResult()");
            // Arrange
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

			Logger.LogDebug("View_Returns_DnnViewResult(): httpContextBase → {HttpContextBase}", httpContextBase);
            // Act
            var controller = this.SetupController(httpContextBase);
            var viewResult = controller.Action1();
			Logger.LogDebug("View_Returns_DnnViewResult(): controller → {Controller}", controller);
			Logger.LogDebug("View_Returns_DnnViewResult(): viewResult → {ViewResult}", viewResult);

            // Assert
            Assert.That(viewResult, Is.InstanceOf<DnnViewResult>());
			Logger.LogDebug("<<< Exiting View_Returns_DnnViewResult()");
        }

        [Test]
        public void View_Returns_DnnViewResult_With_Correct_ViewName()
        {
			Logger.LogDebug(">>> Entering View_Returns_DnnViewResult_With_Correct_ViewName()");
            // Arrange
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

			Logger.LogDebug("View_Returns_DnnViewResult_With_Correct_ViewName(): httpContextBase → {HttpContextBase}", httpContextBase);
            // Act
            var controller = this.SetupController(httpContextBase);
            var viewResult = controller.Action1();
			Logger.LogDebug("View_Returns_DnnViewResult_With_Correct_ViewName(): controller → {Controller}", controller);
			Logger.LogDebug("View_Returns_DnnViewResult_With_Correct_ViewName(): viewResult → {ViewResult}", viewResult);

            // Assert
            var dnnViewResult = viewResult as DnnViewResult;
            Assert.That(dnnViewResult, Is.Not.Null);
            Assert.That(dnnViewResult.ViewName, Is.EqualTo("Action1"));
			Logger.LogDebug("<<< Exiting View_Returns_DnnViewResult_With_Correct_ViewName()");
        }

        [Test]
        public void View_Returns_DnnViewResult_With_Correct_MasterName()
        {
			Logger.LogDebug(">>> Entering View_Returns_DnnViewResult_With_Correct_MasterName()");
            // Arrange
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

			Logger.LogDebug("View_Returns_DnnViewResult_With_Correct_MasterName(): httpContextBase → {HttpContextBase}", httpContextBase);
            // Act
            var controller = this.SetupController(httpContextBase);
            var viewResult = controller.Action2();
			Logger.LogDebug("View_Returns_DnnViewResult_With_Correct_MasterName(): controller → {Controller}", controller);
			Logger.LogDebug("View_Returns_DnnViewResult_With_Correct_MasterName(): viewResult → {ViewResult}", viewResult);

            // Assert
            var dnnViewResult = viewResult as DnnViewResult;
            Assert.That(dnnViewResult, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(dnnViewResult.ViewName, Is.EqualTo("Action2"));
                Assert.That(dnnViewResult.MasterName, Is.EqualTo("Master2"));
            }
			Logger.LogDebug("<<< Exiting View_Returns_DnnViewResult_With_Correct_MasterName()");
        }

        [Test]
        public void View_Returns_DnnViewResult_With_Correct_ViewData()
        {
			Logger.LogDebug(">>> Entering View_Returns_DnnViewResult_With_Correct_ViewData()");
            // Arrange
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

			Logger.LogDebug("View_Returns_DnnViewResult_With_Correct_ViewData(): httpContextBase → {HttpContextBase}", httpContextBase);
            // Act
            var controller = this.SetupController(httpContextBase);
            controller.ViewData.Add("key", "value");
			Logger.LogDebug("View_Returns_DnnViewResult_With_Correct_ViewData(): controller → {Controller}", controller);
            var viewResult = controller.Action2();

			Logger.LogDebug("View_Returns_DnnViewResult_With_Correct_ViewData(): viewResult → {ViewResult}", viewResult);
            // Assert
            var dnnViewResult = viewResult as DnnViewResult;
            Assert.That(dnnViewResult, Is.Not.Null);
            Assert.That(dnnViewResult.ViewData["key"], Is.EqualTo("value"));
			Logger.LogDebug("<<< Exiting View_Returns_DnnViewResult_With_Correct_ViewData()");
        }

        [Test]
        public void View_Returns_DnnViewResult_With_Correct_Model()
        {
			Logger.LogDebug(">>> Entering View_Returns_DnnViewResult_With_Correct_Model()");
            // Arrange
            var dog = new Dog() { Name = "Fluffy" };
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();
			Logger.LogDebug("View_Returns_DnnViewResult_With_Correct_Model(): dog → {Dog}", dog);
			Logger.LogDebug("View_Returns_DnnViewResult_With_Correct_Model(): httpContextBase → {HttpContextBase}", httpContextBase);

            // Act
            var controller = this.SetupController(httpContextBase);
            var viewResult = controller.Action3(dog);
			Logger.LogDebug("View_Returns_DnnViewResult_With_Correct_Model(): controller → {Controller}", controller);
			Logger.LogDebug("View_Returns_DnnViewResult_With_Correct_Model(): viewResult → {ViewResult}", viewResult);

            // Assert
            var dnnViewResult = viewResult as DnnViewResult;
            Assert.That(dnnViewResult, Is.Not.Null);
            Assert.That(dnnViewResult.ViewData.Model, Is.EqualTo(dog));
			Logger.LogDebug("<<< Exiting View_Returns_DnnViewResult_With_Correct_Model()");
        }

        [Test]
        public void View_Returns_DnnViewResult_With_Correct_ViewEngines()
        {
			Logger.LogDebug(">>> Entering View_Returns_DnnViewResult_With_Correct_ViewEngines()");
            // Arrange
            var dog = new Dog() { Name = "Fluffy" };
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();
			Logger.LogDebug("View_Returns_DnnViewResult_With_Correct_ViewEngines(): dog → {Dog}", dog);
			Logger.LogDebug("View_Returns_DnnViewResult_With_Correct_ViewEngines(): httpContextBase → {HttpContextBase}", httpContextBase);

            // Act
            var controller = this.SetupController(httpContextBase);
            var viewResult = controller.Action3(dog);
			Logger.LogDebug("View_Returns_DnnViewResult_With_Correct_ViewEngines(): controller → {Controller}", controller);
			Logger.LogDebug("View_Returns_DnnViewResult_With_Correct_ViewEngines(): viewResult → {ViewResult}", viewResult);

            // Assert
            var dnnViewResult = viewResult as DnnViewResult;
            Assert.That(dnnViewResult, Is.Not.Null);
            Assert.That(dnnViewResult.ViewEngineCollection, Is.EqualTo(controller.ViewEngineCollection));
			Logger.LogDebug("<<< Exiting View_Returns_DnnViewResult_With_Correct_ViewEngines()");
        }

        [Test]
        public void Initialize_CreatesInstance_Of_DnnUrlHelper()
        {
			Logger.LogDebug(">>> Entering Initialize_CreatesInstance_Of_DnnUrlHelper()");
            // Arrange
            HttpContextBase httpContextBase = MockHelper.CreateMockHttpContext();

			Logger.LogDebug("Initialize_CreatesInstance_Of_DnnUrlHelper(): httpContextBase → {HttpContextBase}", httpContextBase);
            // Act
            var controller = this.SetupController(httpContextBase);
            controller.MockInitialize(httpContextBase.Request.RequestContext);
			Logger.LogDebug("Initialize_CreatesInstance_Of_DnnUrlHelper(): controller → {Controller}", controller);

            // Assert
            Assert.That(controller.Url, Is.Not.Null);
			Logger.LogDebug("<<< Exiting Initialize_CreatesInstance_Of_DnnUrlHelper()");
        }

        private FakeDnnController SetupController(HttpContextBase context)
        {
            var controller = new FakeDnnController();
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);
            return controller;
        }
    }
}
