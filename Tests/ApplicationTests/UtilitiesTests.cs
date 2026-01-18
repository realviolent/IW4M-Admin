using NUnit.Framework;
using SharedLibraryCore;
using Moq;
using SharedLibraryCore.Interfaces;
using SharedLibraryCore.Configuration;
using Microsoft.Extensions.Logging;
using SharedLibraryCore.Helpers;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Data.Models;
using System;
using SharedLibraryCore.Database.Models;

namespace ApplicationTests
{
    [TestFixture]
    public class UtilitiesTests
    {
        [Test]
        public void TestCapClientNameLengthReachesMax()
        {
            string originalName = "SomeVeryLongName";
            string expectedName = "SomeVeryLong...";
            int maxLength = originalName.Length - 1;

            string cappedName = originalName.CapClientName(maxLength);

            Assert.AreEqual(expectedName, cappedName);
        }

        [Test]
        public void TestCapClientNameRetainsOriginal()
        {
            string originalName = "Short";
            int maxLength = originalName.Length;

            string cappedName = originalName.CapClientName(maxLength);

            Assert.AreEqual(originalName, cappedName);
        }

        [Test]
        public async Task GetMappedDvarValueOrDefaultAsync_ReturnsValueFromInfoResponse_WhenKeyExists()
        {
            // Arrange
            var mockServer = new MockServer();
            var mockRconParser = new Mock<IRConParser>();
            var mockConnection = new Mock<IRConConnection>();

            mockServer.RconParser = mockRconParser.Object;
            mockServer.SetRemoteConnection(mockConnection.Object);

            string dvarName = "mapname";
            string mappedDvarName = "mapname";
            string infoResponseName = "mapname";
            string expectedValue = "mp_terminal";

            var infoResponse = new Dictionary<string, string>
            {
                { "mapname", expectedValue }
            };

            mockRconParser.Setup(p => p.GetOverrideDvarName(dvarName)).Returns(mappedDvarName);
            mockRconParser.Setup(p => p.GetDefaultDvarValue<string>(mappedDvarName)).Returns(null as string);

            // Act
            var result = await mockServer.GetMappedDvarValueOrDefaultAsync<string>(dvarName, infoResponseName, infoResponse);

            // Assert
            Assert.AreEqual(expectedValue, result.Value);
            mockRconParser.Verify(p => p.GetDvarAsync<string>(It.IsAny<IRConConnection>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetMappedDvarValueOrDefaultAsync_ReturnsValueFromRCon_WhenKeyDoesNotExistInInfoResponse()
        {
            // Arrange
            var mockServer = new MockServer();
            var mockRconParser = new Mock<IRConParser>();
            var mockConnection = new Mock<IRConConnection>();

            mockServer.RconParser = mockRconParser.Object;
            mockServer.SetRemoteConnection(mockConnection.Object);

            string dvarName = "sv_hostname";
            string mappedDvarName = "sv_hostname";
            string expectedValue = "My Server";

            var infoResponse = new Dictionary<string, string>(); // Empty info response

            mockRconParser.Setup(p => p.GetOverrideDvarName(dvarName)).Returns(mappedDvarName);
            mockRconParser.Setup(p => p.GetDefaultDvarValue<string>(mappedDvarName)).Returns(null as string);
            mockRconParser.Setup(p => p.GetDvarAsync<string>(mockConnection.Object, mappedDvarName, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dvar<string> { Value = expectedValue, Name = mappedDvarName });

            // Act
            var result = await mockServer.GetMappedDvarValueOrDefaultAsync<string>(dvarName, infoResponse: infoResponse);

            // Assert
            Assert.AreEqual(expectedValue, result.Value);
        }

        [Test]
        public async Task GetMappedDvarValueOrDefaultAsync_ReturnsValueFromRCon_WhenInfoResponseIsNull()
        {
             // Arrange
            var mockServer = new MockServer();
            var mockRconParser = new Mock<IRConParser>();
            var mockConnection = new Mock<IRConConnection>();

            mockServer.RconParser = mockRconParser.Object;
            mockServer.SetRemoteConnection(mockConnection.Object);

            string dvarName = "sv_hostname";
            string mappedDvarName = "sv_hostname";
            string expectedValue = "My Server";

            mockRconParser.Setup(p => p.GetOverrideDvarName(dvarName)).Returns(mappedDvarName);
            mockRconParser.Setup(p => p.GetDefaultDvarValue<string>(mappedDvarName)).Returns(null as string);
            mockRconParser.Setup(p => p.GetDvarAsync<string>(mockConnection.Object, mappedDvarName, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dvar<string> { Value = expectedValue, Name = mappedDvarName });

            // Act
            var result = await mockServer.GetMappedDvarValueOrDefaultAsync<string>(dvarName, infoResponse: null);

            // Assert
            Assert.AreEqual(expectedValue, result.Value);
        }

        [Test]
        public async Task GetMappedDvarValueOrDefaultAsync_UsesDefaultValue_WhenFetchFails()
        {
             // Arrange
            var mockServer = new MockServer();
            var mockRconParser = new Mock<IRConParser>();
            var mockConnection = new Mock<IRConConnection>();

            mockServer.RconParser = mockRconParser.Object;
            mockServer.SetRemoteConnection(mockConnection.Object);

            string dvarName = "sv_hostname";
            string mappedDvarName = "sv_hostname";
            string defaultValue = "Default Server Name";

            mockRconParser.Setup(p => p.GetOverrideDvarName(dvarName)).Returns(mappedDvarName);
            mockRconParser.Setup(p => p.GetDefaultDvarValue<string>(mappedDvarName)).Returns(defaultValue);
            mockRconParser.Setup(p => p.GetDvarAsync<string>(mockConnection.Object, mappedDvarName, defaultValue, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dvar<string> { Value = defaultValue, Name = mappedDvarName });

            // Act
            var result = await mockServer.GetMappedDvarValueOrDefaultAsync<string>(dvarName, infoResponse: null);

            // Assert
            Assert.AreEqual(defaultValue, result.Value);
        }
    }

    public class MockServer : Server
    {
        public MockServer() : base(
            new Mock<ILogger<Server>>().Object,
            new Mock<SharedLibraryCore.Interfaces.ILogger>().Object,
            new ServerConfiguration(),
            CreateMockManager(),
            new Mock<IRConConnectionFactory>().Object,
            new Mock<IGameLogReaderFactory>().Object,
            CreateMockServiceProvider())
        {
        }

        private static IManager CreateMockManager()
        {
            var mockManager = new Mock<IManager>();
            var mockConfigHandler = new Mock<IConfigurationHandler<ApplicationConfiguration>>();
            var appConfig = new ApplicationConfiguration();

            mockConfigHandler.Setup(c => c.Configuration()).Returns(appConfig);
            mockManager.Setup(m => m.GetApplicationSettings()).Returns(mockConfigHandler.Object);

            return mockManager.Object;
        }

        private static IServiceProvider CreateMockServiceProvider()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            var defaultSettings = new DefaultSettings();

            mockServiceProvider.Setup(x => x.GetService(typeof(DefaultSettings))).Returns(defaultSettings);

            return mockServiceProvider.Object;
        }

        public void SetRemoteConnection(IRConConnection connection)
        {
            RemoteConnection = connection;
        }

        // Implement abstract members
        public override Task Kick(string reason, EFClient target, EFClient origin, EFPenalty originalPenalty) => Task.CompletedTask;
        public override Task<string[]> ExecuteCommandAsync(string command, CancellationToken token = default) => Task.FromResult(Array.Empty<string>());
        public override Task SetDvarAsync(string name, object value, CancellationToken token = default) => Task.CompletedTask;
        public override Task<EFClient> OnClientConnected(EFClient P) => Task.FromResult(P);
        public override Task OnClientDisconnected(EFClient client) => Task.CompletedTask;
        protected override Task<bool> ProcessEvent(GameEvent E) => Task.FromResult(true);
        public override Task ExecuteEvent(GameEvent E) => Task.CompletedTask;
        public override Task TempBan(string reason, TimeSpan length, EFClient target, EFClient origin) => Task.CompletedTask;
        public override Task Ban(string reason, EFClient target, EFClient origin, bool isEvade = false) => Task.CompletedTask;
        public override Task Warn(string reason, EFClient target, EFClient origin) => Task.CompletedTask;
        public override Task Unban(string reason, EFClient target, EFClient origin) => Task.CompletedTask;
        public override void InitializeTokens() { }
        public override Task<long> GetIdForServer(Server server = null) => Task.FromResult(0L);
        public override long LegacyDatabaseId => 0;
    }
}
