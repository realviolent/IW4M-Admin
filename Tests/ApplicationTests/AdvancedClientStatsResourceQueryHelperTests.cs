using Data.Abstractions;
using Data.Context;
using Data.MigrationContext;
using Data.Models;
using Data.Models.Client;
using FakeItEasy;
using IW4MAdmin.Plugins.Stats;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SharedLibraryCore;
using SharedLibraryCore.Dtos;
using SharedLibraryCore.Interfaces;
using Stats.Helpers;
using Stats.Dtos;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System.Collections.Generic;

namespace ApplicationTests
{
    [TestFixture]
    public class AdvancedClientStatsResourceQueryHelperTests
    {
        private AdvancedClientStatsResourceQueryHelper _queryHelper;
        private IDatabaseContextFactory _contextFactory;
        private IManager _manager;
        private ILogger<AdvancedClientStatsResourceQueryHelper> _logger;

        public class TestDatabaseContextFactory : IDatabaseContextFactory
        {
            private readonly string _databaseName;

            public TestDatabaseContextFactory()
            {
                _databaseName = Guid.NewGuid().ToString();
            }

            public DatabaseContext CreateContext(bool? enableTracking = true)
            {
                var options = new DbContextOptionsBuilder<SqliteDatabaseContext>()
                    .UseInMemoryDatabase(databaseName: _databaseName)
                    .Options;

                return new SqliteDatabaseContext(options);
            }
        }

        [SetUp]
        public void Setup()
        {
            _contextFactory = new TestDatabaseContextFactory();
            _manager = A.Fake<IManager>();
            _logger = A.Fake<ILogger<AdvancedClientStatsResourceQueryHelper>>();

            // Mock IManager.GetServers()
            A.CallTo(() => _manager.GetServers()).Returns(new List<Server>());

            _queryHelper = new AdvancedClientStatsResourceQueryHelper(_logger, _contextFactory, _manager);
        }

        [Test]
        public async Task QueryResource_ReturnsEmptyResult_WhenNoStatsFound()
        {
            // Arrange
            var client = new EFClient
            {
                ClientId = 1,
                CurrentAlias = new EFAlias { Name = "TestClient" },
                Level = EFClient.Permission.User,
                GameName = Reference.Game.IW4
            };

            await using var context = _contextFactory.CreateContext(enableTracking: true);
            context.Clients.Add(client);
            await context.SaveChangesAsync();

            var query = new StatsInfoRequest
            {
                ClientId = client.ClientId,
                ServerEndpoint = "127.0.0.1:28960"
            };

            // Act
            var result = await _queryHelper.QueryResource(query);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result.Results);
        }
    }
}
