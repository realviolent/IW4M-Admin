using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SharedLibraryCore.Dtos;
using SharedLibraryCore.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebfrontCore.Controllers;
using WebfrontCore.ViewModels;
using SharedLibraryCore.Configuration;

namespace ApplicationTests
{
    public class TestableConfigurationController : ConfigurationController
    {
        public TestableConfigurationController(IManager manager, IConfigurationFileService configService) : base(manager, configService)
        {
        }

        public void SetClientLevel(Data.Models.Client.EFClient.Permission level)
        {
            Client.Level = level;
        }
    }

    [TestFixture]
    public class ConfigurationControllerTests
    {
        private IManager _manager;
        private IConfigurationFileService _configService;
        private TestableConfigurationController _controller;

        [SetUp]
        public void Setup()
        {
            SharedLibraryCore.Utilities.CurrentLocalization.LocalizationName = "en-US";
            _manager = A.Fake<IManager>();
            _configService = A.Fake<IConfigurationFileService>();

            // Mock Manager dependencies
            var appSettings = A.Fake<IConfigurationHandler<ApplicationConfiguration>>();
            var config = new ApplicationConfiguration();
            A.CallTo(() => _manager.GetApplicationSettings()).Returns(appSettings);
            A.CallTo(() => appSettings.Configuration()).Returns(config);

             var interactionRegistration = A.Fake<IInteractionRegistration>();
             var alertManager = A.Fake<IAlertManager>();
             A.CallTo(() => _manager.InteractionRegistration).Returns(interactionRegistration);
             A.CallTo(() => _manager.AlertManager).Returns(alertManager);

             var pageList = A.Fake<IPageList>();
             A.CallTo(() => pageList.Pages).Returns(new Dictionary<string, string>());
             A.CallTo(() => _manager.GetPageList()).Returns(pageList);

            _controller = new TestableConfigurationController(_manager, _configService);
        }

        [Test]
        public async Task Files_ReturnsViewWithModel_WhenAuthorized()
        {
             _controller.SetClientLevel(Data.Models.Client.EFClient.Permission.Owner);

             var files = new List<ConfigurationFileDto>
             {
                 new ConfigurationFileDto { FileName = "test.json", Content = "{}" }
             };
             A.CallTo(() => _configService.GetConfigurationFilesAsync()).Returns(files);

             var result = await _controller.Files() as ViewResult;

             Assert.That(result, Is.Not.Null);
             var model = result.Model as IEnumerable<ConfigurationFileInfo>;
             Assert.That(model, Is.Not.Null);
             Assert.That(model.Count(), Is.EqualTo(1));
             Assert.That(model.First().FileName, Is.EqualTo("test.json"));
        }

        [Test]
        public async Task Files_ReturnsUnauthorized_WhenNotAuthorized()
        {
            _controller.SetClientLevel(Data.Models.Client.EFClient.Permission.User);
            var result = await _controller.Files();
            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public async Task PatchFiles_WritesFile_WhenAuthorizedAndFileExists()
        {
            _controller.SetClientLevel(Data.Models.Client.EFClient.Permission.Owner);
            var fileName = "test.json";
            var content = "{}";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            _controller.Request.Body = stream;

            var result = await _controller.PatchFiles(fileName);

            A.CallTo(() => _configService.WriteConfigurationFileAsync(fileName, content)).MustHaveHappened();
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchFiles_ReturnsBadRequest_WhenFileDoesNotExist()
        {
            _controller.SetClientLevel(Data.Models.Client.EFClient.Permission.Owner);
            var fileName = "test.json";
            var content = "{}";
             _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            _controller.Request.Body = stream;

            A.CallTo(() => _configService.WriteConfigurationFileAsync(fileName, content))
                .Throws(new FileNotFoundException());

            var result = await _controller.PatchFiles(fileName);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public void GetNewListItem_Servers_ReturnsServerItemView()
        {
            _controller.SetClientLevel(Data.Models.Client.EFClient.Permission.Owner);
            var result = _controller.GetNewListItem("Servers", 0, -1) as PartialViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("_ServerItem"));
            Assert.That(result.Model, Is.InstanceOf<ApplicationConfiguration>());
        }

        [Test]
        public void GetNewListItem_ServersRules_ReturnsListItemView()
        {
            _controller.SetClientLevel(Data.Models.Client.EFClient.Permission.Owner);
            var result = _controller.GetNewListItem("Servers.Rules", 0, 0) as PartialViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("_ListItem"));
            Assert.That(result.Model, Is.InstanceOf<BindingHelper>());
        }

        [Test]
        public void GetNewListItem_OtherArray_ReturnsListItemView()
        {
            _controller.SetClientLevel(Data.Models.Client.EFClient.Permission.Owner);
            var result = _controller.GetNewListItem("AutoMessages", 0, -1) as PartialViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("_ListItem"));
            Assert.That(result.Model, Is.InstanceOf<BindingHelper>());
        }

        [Test]
        public void GetNewListItem_Unauthorized_ReturnsUnauthorized()
        {
            _controller.SetClientLevel(Data.Models.Client.EFClient.Permission.User);
            var result = _controller.GetNewListItem("Servers", 0, -1);
            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }
    }
}
