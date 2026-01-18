using System.Collections.Generic;
using System.Linq;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace SharedLibraryCore
{
    public static class ServerExtensions
    {
        /// <summary>
        ///     Get a player by name
        /// </summary>
        /// <param name="server">Server to search in</param>
        /// <param name="pName">EFClient name to search for</param>
        /// <returns>Matching player if found</returns>
        public static List<EFClient> GetClientByName(this IGameServer server, string pName)
        {
            if (string.IsNullOrEmpty(pName))
            {
                return new List<EFClient>();
            }

            pName = pName.Trim().StripColors();

            var quoteSplit = pName.Split('"');
            var literal = false;
            if (quoteSplit.Length > 1)
            {
                pName = quoteSplit[1];
                literal = true;
            }

            var clients = server.ConnectedClients;

            if (literal)
            {
                return clients.Where(p => p.Name?.StripColors()?.ToLower() == pName.ToLower()).ToList();
            }

            return clients.Where(p => (p.Name?.StripColors()?.ToLower() ?? "").Contains(pName.ToLower()))
                .ToList();
        }
    }
}
