using ApplicationTests.Fixtures;
using ApplicationTests.Mocks;
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
using SharedLibraryCore.Database.Models;
using Data.Models.Client;

namespace ApplicationTests
{
    public class ConfigurationControllerTests
    {
        private IServiceProvider serviceProvider;
        private IConfigurationFileService configurationFileService;
        private ConfigurationController controller;

        [SetUp]
        public void Setup()
        {
            configurationFileService = A.Fake<IConfigurationFileService>();

            serviceProvider = new ServiceCollection()
                .BuildBase()
                .AddSingleton(configurationFileService)
                .AddSingleton<ConfigurationController>()
                .BuildServiceProvider()
                .SetupTestHooks();

            controller = serviceProvider.GetRequiredService<ConfigurationController>();

            // Set client permission to Owner
            var clientProp = typeof(SharedLibraryCore.BaseController).GetProperty("Client", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            var client = clientProp.GetValue(controller) as SharedLibraryCore.Database.Models.EFClient;
            client.Level = Data.Models.Client.EFClient.Permission.Owner;
        }

        [Test]
        public async Task Files_ReturnsViewWithModel_WhenAuthorized()
        {
            // Arrange
            var files = new List<ConfigurationFileDto>
            {
                new ConfigurationFileDto { FileName = "test.json", Content = "{}" }
            };
            A.CallTo(() => configurationFileService.GetConfigurationFilesAsync())
                .Returns(files);

            // Act
            var result = await controller.Files() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as IEnumerable<ConfigurationFileInfo>;
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Count());
            Assert.AreEqual("test.json", model.First().FileName);
        }

        [Test]
        public async Task Files_ReturnsUnauthorized_WhenNotAuthorized()
        {
            // Arrange
            var clientProp = typeof(SharedLibraryCore.BaseController).GetProperty("Client", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            var client = clientProp.GetValue(controller) as SharedLibraryCore.Database.Models.EFClient;
            client.Level = Data.Models.Client.EFClient.Permission.User;

            // Act
            var result = await controller.Files();

            // Assert
            Assert.IsInstanceOf<UnauthorizedResult>(result);
        }

        [Test]
        public async Task PatchFiles_WritesFile_WhenAuthorizedAndFileExists()
        {
            // Arrange
            var fileName = "test.json";
            var content = "{}";
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Mock Request Body
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            controller.Request.Body = stream;

            // Act
            var result = await controller.PatchFiles(fileName);

            // Assert
            A.CallTo(() => configurationFileService.WriteConfigurationFileAsync(fileName, content))
                .MustHaveHappened();
            Assert.IsInstanceOf<NoContentResult>(result);
        }

         [Test]
        public async Task PatchFiles_ReturnsBadRequest_WhenFileDoesNotExist()
        {
            // Arrange
            var fileName = "test.json";
            var content = "{}";
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Mock Request Body
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            controller.Request.Body = stream;

            A.CallTo(() => configurationFileService.WriteConfigurationFileAsync(fileName, content))
                .Throws(new FileNotFoundException());

            // Act
            var result = await controller.PatchFiles(fileName);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
    }
}
