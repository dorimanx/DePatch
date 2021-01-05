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
            var player = MySession.Static.Players.TryGetPlayerBySteamId(steamId);
            if (player == null) return false;
            var owner = grid.BigOwners.FirstOrDefault();
            if (owner == default) return true;
            MySession.Static.Players.TryGetPlayerId(owner, out var ownerId);
            var ownerPlayer = MySession.Static.Players.GetPlayerById(ownerId);
            if (ownerPlayer == null) return MySession.Static.IsUserAdmin(steamId);
            var relation = ownerPlayer.GetRelationTo(player.Identity.IdentityId);
            return relation.IsFriendly();
        }
    }
}