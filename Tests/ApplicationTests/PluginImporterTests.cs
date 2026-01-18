using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using IW4MAdmin.Application.API.Master;
using IW4MAdmin.Application.Plugin;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SharedLibraryCore;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Interfaces;

namespace ApplicationTests
{
    [TestFixture]
    public class PluginImporterTests
    {
        private ILogger<PluginImporter> _logger;
        private ApplicationConfiguration _appConfig;
        private IMasterApi _masterApi;
        private IRemoteAssemblyHandler _remoteAssemblyHandler;
        private PluginImporter _importer;
        private string _pluginDir;

        [SetUp]
        public void Setup()
        {
            _logger = A.Fake<ILogger<PluginImporter>>();
            _appConfig = new ApplicationConfiguration { Id = "test_id", SubscriptionId = "test_sub" };
            _masterApi = A.Fake<IMasterApi>();
            _remoteAssemblyHandler = A.Fake<IRemoteAssemblyHandler>();

            _importer = new PluginImporter(_logger, _appConfig, _masterApi, _remoteAssemblyHandler);

            // Setup plugin directory
            _pluginDir = Path.Combine(Utilities.OperatingDirectory, "Plugins");
            if (!Directory.Exists(_pluginDir))
            {
                Directory.CreateDirectory(_pluginDir);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_pluginDir))
            {
                Directory.Delete(_pluginDir, true);
            }
        }

        [Test]
        public async Task DiscoverScriptPlugins_ShouldCallGetPluginSubscription_WhenScriptExists()
        {
             // Create a dummy js file
            File.WriteAllText(Path.Combine(_pluginDir, "test.js"), "dummy content");

            // Arrange
             A.CallTo(() => _masterApi.GetPluginSubscription("test_id", "test_sub"))
                .Returns(Task.FromResult((IEnumerable<PluginSubscriptionContent>)new List<PluginSubscriptionContent>()));

            // Act
            await _importer.DiscoverScriptPluginsAsync();

            // Assert
            A.CallTo(() => _masterApi.GetPluginSubscription("test_id", "test_sub")).MustHaveHappened();
        }
    }
}
