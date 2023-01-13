using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.Game.World;

namespace DePatch
{
    public static class CubeGridExtensions
    {
        public static bool IsFriendlyPlayer(this MyCubeGrid grid, ulong steamId)
        {
            if (grid.BigOwners.Count < 1) return true;

            var playerID = MySession.Static.Players.TryGetIdentityId(steamId);
            var Playerfaction = MySession.Static.Factions.TryGetPlayerFaction(playerID);

            var gridID = grid.BigOwners.FirstOrDefault();
            if (gridID == default) return true;

            var gridFaction = MySession.Static.Factions.TryGetPlayerFaction(gridID);

            if (playerID == gridID)
                return true;

            if (Playerfaction != null && gridFaction != null)
            {
                if (Playerfaction.FactionId == gridFaction.FactionId)
                    return true;
            }

            return false;
        }

        public static bool IsMatchForbidden(MyCubeBlockDefinition definition)
        {
            HashSet<string> blockList = new HashSet<string>(DePatchPlugin.Instance.Config.ForbiddenBlocksList);
            if (blockList.Count == 0)
                return false;

            foreach (string name in blockList)
            {
                if (name.Equals(definition.ToString().Substring(16), StringComparison.OrdinalIgnoreCase) ||
                    name.Equals(definition.Id.SubtypeId.ToString(), StringComparison.OrdinalIgnoreCase) ||
                    name.Equals(definition.Id.TypeId.ToString(), StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}