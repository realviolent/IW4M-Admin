using NUnit.Framework;
using FakeItEasy;
using IW4MAdmin.Plugins.Stats.Commands;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Interfaces;
using Data.Abstractions;
using IW4MAdmin.Plugins.Stats.Helpers;
using Stats.Config;
using SharedLibraryCore;
using System.Threading.Tasks;
using Data.Models.Client.Stats;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using SharedLibraryCore.Database.Models;
using Microsoft.Extensions.DependencyInjection;
using ApplicationTests.Fixtures;

namespace ApplicationTests
{
    [TestFixture]
    public class ResetStatsTests
    {
        private ResetStats _resetStatsCommand;
        private CommandConfiguration _commandConfig;
        private ITranslationLookup _translationLookup;
        private IDatabaseContextFactory _contextFactory;
        private StatManager _statManager;
        private StatsConfiguration _statsConfig;
        private IServiceProvider _serviceProvider;

        [SetUp]
        public void Setup()
        {
            _commandConfig = new CommandConfiguration();
            _translationLookup = A.Fake<ITranslationLookup>();
            _statsConfig = new StatsConfiguration { BaseEloRating = 500.0 }; // Custom rating for test
            _statManager = A.Fake<StatManager>();

             _serviceProvider = new ServiceCollection()
                .BuildBase()
                .AddSingleton(A.Fake<IConfigurationHandler<StatsConfiguration>>())
                .AddSingleton<StatsResourceQueryHelper>()
                .AddSingleton(new ServerConfiguration() { IPAddress = "127.0.0.1", Port = 28960 })
                .AddSingleton(_statManager)
                .BuildServiceProvider();

            _contextFactory = _serviceProvider.GetRequiredService<IDatabaseContextFactory>();

            _resetStatsCommand = new ResetStats(_commandConfig, _translationLookup, _contextFactory, _statManager, _statsConfig);
        }

        [Test]
        public async Task ExecuteAsync_ResetsStatsWithConfiguredEloRating()
        {
            // Arrange
            var client = ClientGenerators.CreateDatabaseClient();
            client.ClientId = 1;

            var server = A.Fake<IGameServer>();
            A.CallTo(() => server.LegacyDatabaseId).Returns(123);

            var gameEvent = new GameEvent
            {
                Origin = client,
                Owner = server
            };

            await using var context = _contextFactory.CreateContext();
            var clientStats = new EFClientStatistics
            {
                ClientId = client.ClientId,
                ServerId = 123,
                Kills = 100,
                Deaths = 50,
                EloRating = 1200.0
            };
            context.Set<EFClientStatistics>().Add(clientStats);
            await context.SaveChangesAsync();

            // Act
            await _resetStatsCommand.ExecuteAsync(gameEvent);

            // Assert
            await using var verifyContext = _contextFactory.CreateContext();
            var updatedStats = await verifyContext.Set<EFClientStatistics>()
                .FirstOrDefaultAsync(s => s.ClientId == client.ClientId && s.ServerId == 123);

            Assert.IsNotNull(updatedStats);
            Assert.AreEqual(0, updatedStats.Kills);
            Assert.AreEqual(0, updatedStats.Deaths);
            Assert.AreEqual(500.0, updatedStats.EloRating); // Should match _statsConfig.BaseEloRating
        }
    }
}
