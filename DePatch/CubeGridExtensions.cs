using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.World;

namespace DePatch
{
    public static class CubeGridExtensions
    {
        public static bool IsFriendlyPlayer(this MyCubeGrid grid, ulong steamId)
        {
            if (grid.BigOwners.Count < 1) return true;

            var adminPlayerID = MySession.Static.IsUserAdmin(steamId);

            var playerID = MySession.Static.Players.TryGetIdentityId(steamId);
            var Playerfaction = MySession.Static.Factions.TryGetPlayerFaction(playerID);

            var gridID = grid.BigOwners.FirstOrDefault();
            if (gridID == default) return true;

            var gridFaction = MySession.Static.Factions.TryGetPlayerFaction(gridID);

            if (adminPlayerID)
                return true;

            if (playerID == gridID)
                return true;

            if (Playerfaction != null && gridFaction != null)
            {
                if (Playerfaction == gridFaction)
                    return true;
            }

            return false;
        }
    }
}