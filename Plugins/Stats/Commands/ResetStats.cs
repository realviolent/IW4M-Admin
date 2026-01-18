using Microsoft.EntityFrameworkCore;
using SharedLibraryCore;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using Data.Abstractions;
using Data.Models.Client.Stats;
using IW4MAdmin.Plugins.Stats.Helpers;
using Stats.Config;

namespace IW4MAdmin.Plugins.Stats.Commands
{
    public class ResetStats : Command
    {
        private readonly IDatabaseContextFactory _contextFactory;
        private readonly StatManager _statManager;
        private readonly StatsConfiguration _statsConfig;

        public ResetStats(CommandConfiguration config, ITranslationLookup translationLookup, 
            IDatabaseContextFactory contextFactory, StatManager statManager, StatsConfiguration statsConfig) : base(config, translationLookup)
        {
            Name = "resetstats";
            Description = translationLookup["PLUGINS_STATS_COMMANDS_RESET_DESC"];
            Alias = "rs";
            Permission = EFClient.Permission.User;
            RequiresTarget = false;
            AllowImpersonation = true;

            _contextFactory = contextFactory;
            _statManager = statManager;
            _statsConfig = statsConfig;
        }

        public override async Task ExecuteAsync(GameEvent gameEvent)
        {
            if (gameEvent.Origin.ClientNumber >= 0)
            {
                var serverId = (gameEvent.Owner as IGameServer).LegacyDatabaseId;

                await using var context = _contextFactory.CreateContext();
                var clientStats = await context.Set<EFClientStatistics>()
                    .Where(s => s.ClientId == gameEvent.Origin.ClientId)
                    .Where(s => s.ServerId == serverId)
                    .FirstOrDefaultAsync();
                
                // want to prevent resetting stats before they've gotten any kills
                if (clientStats != null)
                {
                    clientStats.Deaths = 0;
                    clientStats.Kills = 0;
                    clientStats.SPM = 0.0;
                    clientStats.Skill = 0.0;
                    clientStats.TimePlayed = 0;
                    clientStats.EloRating = _statsConfig.BaseEloRating;
                    await context.SaveChangesAsync();
                }

                // reset the cached version
                _statManager.ResetStats(gameEvent.Origin);

                gameEvent.Origin.Tell(_translationLookup["PLUGINS_STATS_COMMANDS_RESET_SUCCESS"]);
            }

            else
            {
                gameEvent.Origin.Tell(_translationLookup["PLUGINS_STATS_COMMANDS_RESET_FAIL"]);
            }
        }
    }
}
