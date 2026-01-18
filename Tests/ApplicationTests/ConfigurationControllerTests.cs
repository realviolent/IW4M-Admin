using NUnit.Framework;
using WebfrontCore.Controllers;
using SharedLibraryCore;
using SharedLibraryCore.Interfaces;
using SharedLibraryCore.Configuration;
using Data.Models.Client;
using Microsoft.AspNetCore.Mvc;
using FakeItEasy;
using WebfrontCore.ViewModels;
using System.Collections.Generic;

namespace ApplicationTests
{
    public class TestableConfigurationController : ConfigurationController
    {
        public TestableConfigurationController(IManager manager) : base(manager)
        {
        }

        public void SetClientLevel(EFClient.Permission level)
        {
            Client.Level = level;
        }
    }

    [TestFixture]
    public class ConfigurationControllerTests
    {
        private IManager _manager;
        private TestableConfigurationController _controller;

        [SetUp]
        public void Setup()
        {
            Utilities.CurrentLocalization.LocalizationName = "en-US";
            _manager = A.Fake<IManager>();
            // ConfigurationController constructor calls new ApplicationConfigurationValidator()
            // It calls base(manager) which initializes Client.

            // We need to mock Manager.GetApplicationSettings().Configuration() because BaseController accesses it.
            var appSettings = A.Fake<IConfigurationHandler<ApplicationConfiguration>>();
            var config = new ApplicationConfiguration();
            A.CallTo(() => _manager.GetApplicationSettings()).Returns(appSettings);
            A.CallTo(() => appSettings.Configuration()).Returns(config);

            // Also BaseController accesses Manager.InteractionRegistration, AlertManager
             var interactionRegistration = A.Fake<IInteractionRegistration>();
             var alertManager = A.Fake<IAlertManager>();
             A.CallTo(() => _manager.InteractionRegistration).Returns(interactionRegistration);
             A.CallTo(() => _manager.AlertManager).Returns(alertManager);

             // Also Manager.GetPageList()
             var pageList = A.Fake<IPageList>();
             A.CallTo(() => pageList.Pages).Returns(new Dictionary<string, string>());
             A.CallTo(() => _manager.GetPageList()).Returns(pageList);

            _controller = new TestableConfigurationController(_manager);
        }

        [Test]
        public void GetNewListItem_Servers_ReturnsServerItemView()
        {
            // Arrange
            _controller.SetClientLevel(EFClient.Permission.Owner);

            // Act
            var result = _controller.GetNewListItem("Servers", 0, -1) as PartialViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("_ServerItem"));
            Assert.That(result.Model, Is.InstanceOf<ApplicationConfiguration>());
        }

        [Test]
        public void GetNewListItem_ServersRules_ReturnsListItemView()
        {
            // Arrange
            _controller.SetClientLevel(EFClient.Permission.Owner);

            // Act
            var result = _controller.GetNewListItem("Servers.Rules", 0, 0) as PartialViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("_ListItem"));
            Assert.That(result.Model, Is.InstanceOf<BindingHelper>());
        }

        [Test]
        public void GetNewListItem_OtherArray_ReturnsListItemView()
        {
             // Arrange
            _controller.SetClientLevel(EFClient.Permission.Owner);

            // Act
            var result = _controller.GetNewListItem("AutoMessages", 0, -1) as PartialViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("_ListItem"));
            Assert.That(result.Model, Is.InstanceOf<BindingHelper>());
        }

        [Test]
        public void GetNewListItem_Unauthorized_ReturnsUnauthorized()
        {
             // Arrange
            _controller.SetClientLevel(EFClient.Permission.User);

            // Act
            var result = _controller.GetNewListItem("Servers", 0, -1);

            // Assert
            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }
    }
}
