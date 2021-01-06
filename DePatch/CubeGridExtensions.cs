using System.Linq;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using Torch;
using VRage.Game;

namespace DePatch
{
    public static class CubeGridExtensions
    {
        public static bool IsFriendlyPlayer(this MyCubeGrid grid, ulong steamId)
        {
            var faction =
                MySession.Static.Factions.TryGetPlayerFaction(MySession.Static.Players.TryGetIdentityId(steamId));
            if (grid.BigOwners.Count < 1) return true;
            return faction != null && faction.IsFriendly(grid.BigOwners.FirstOrDefault());
        }
    }
}