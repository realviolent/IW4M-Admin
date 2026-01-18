using NUnit.Framework;
using System.Diagnostics;
using System.Threading.Tasks;
using IW4MAdmin.Application.Factories;
using SharedLibraryCore.Interfaces;

namespace ApplicationTests
{
    [TestFixture]
    public class ConfigurationHandlerPerformanceTests
    {
        public class TestConfiguration : IBaseConfiguration
        {
            public IBaseConfiguration Generate() => new TestConfiguration();
            public string Name() => "TestConfiguration";
        }

        [Test]
        public async Task Compare_Sync_Vs_Async_Factory()
        {
            var factory = new ConfigurationHandlerFactory();
            string configName = "PerformanceTestConfig";

            // Warmup
            try { factory.GetConfigurationHandler<TestConfiguration>(configName); } catch {}
            try { await factory.GetConfigurationHandlerAsync<TestConfiguration>(configName); } catch {}

            int iterations = 100;

            // Measure Sync
            var swSync = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                factory.GetConfigurationHandler<TestConfiguration>(configName);
            }
            swSync.Stop();

            // Measure Async
            var swAsync = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                await factory.GetConfigurationHandlerAsync<TestConfiguration>(configName);
            }
            swAsync.Stop();

            TestContext.WriteLine($"Sync: {swSync.ElapsedMilliseconds}ms");
            TestContext.WriteLine($"Async: {swAsync.ElapsedMilliseconds}ms");
        }
    }
}
