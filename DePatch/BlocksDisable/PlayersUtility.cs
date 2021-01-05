using System;
using System.Collections.Generic;
using Sandbox.Definitions;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using SpaceEngineers.Game.Entities.Blocks;
using VRage.Game;

namespace DePatch.BlocksDisable
{
    public static class PlayersUtility
    {
        public static bool KeepBlockOff(MyFunctionalBlock block)
        {
            if (MySession.Static.Players.IdentityIsNpc(block.OwnerId))
            {
                return false;
            }
            if (IsMatch(block.BlockDefinition))
            {
                return !OwnerFactionOnline(block.OwnerId);
            }
            return false;
        }

        public static bool KeepBlockOffWelder(MyShipWelder block)
        {
            if (MySession.Static.Players.IdentityIsNpc(block.OwnerId))
            {
                return false;
            }
            if (IsMatch(block.BlockDefinition))
            {
                return !OwnerFactionOnline(block.OwnerId);
            }
            return false;
        }

        public static bool KeepBlockOffProgramBlocks(MyProgrammableBlock block)
        {
            if (MySession.Static.Players.IdentityIsNpc(block.OwnerId))
            {
                return false;
            }
            if (IsMatch(block.BlockDefinition))
            {
                return !OwnerFactionOnline(block.OwnerId);
            }
            return false;
        }

        private static bool IsMatch(MyCubeBlockDefinition definition)
        {
            HashSet<string> blockList = new HashSet<string>(DePatchPlugin.Instance.Config.TargetedBlocks);
            if (blockList.Count == 0)
            {
                return false;
            }
            foreach (string name in blockList)
            {
                if (!name.Equals(definition.ToString().Substring(16), StringComparison.OrdinalIgnoreCase))
                {
                    if (!name.Equals(definition.Id.TypeId.ToString().Substring(16), StringComparison.OrdinalIgnoreCase))
                    {
                        if (!name.Equals(definition.Id.SubtypeId.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private static bool OwnerFactionOnline(long owner)
        {
            if (MySession.Static.Players.IsPlayerOnline(owner) || ExemptPlayerOrFaction(owner))
            {
                return true;
            }
            if (MySession.Static.Factions.GetPlayerFaction(owner) == null)
            {
                return false;
            }
            if (MySession.Static.Factions.GetPlayerFaction(owner).IsEveryoneNpc() || ExemptPlayerOrFaction(MySession.Static.Factions.GetPlayerFaction(owner).FactionId))
            {
                return true;
            }

            if (DePatchPlugin.Instance.Config.AllowFactions)
            {
                foreach (KeyValuePair<long, MyFactionMember> member in MySession.Static.Factions.GetPlayerFaction(owner).Members)
                {
                    if (MySession.Static.Players.IsPlayerOnline(member.Key))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool ExemptPlayerOrFaction(long id)
        {
            HashSet<string> logicException = new HashSet<string>(DePatchPlugin.Instance.Config.ExemptedFactions);
            if (logicException.Count == 0)
            {
                return false;
            }
            if (logicException.Contains(id.ToString()))
            {
                return true;
            }
            if (MySession.Static.Players.TryGetIdentity(id) != null)
            {
                if (logicException.Contains(MySession.Static.Players.TryGetIdentity(id).DisplayName) ||
                    (MySession.Static.Factions.GetPlayerFaction(id) != null &&
                    logicException.Contains(MySession.Static.Factions.GetPlayerFaction(id).Tag)))
                {
                    return true;
                }
            }
            if (MySession.Static.Factions.TryGetFactionById(id) != null && logicException.Contains(MySession.Static.Factions.TryGetFactionById(id).Tag))
            {
                return true;
            }
            return false;
        }
    }
}
