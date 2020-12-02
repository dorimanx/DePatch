using System;
using System.Collections.Generic;
using Sandbox.Definitions;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using SpaceEngineers.Game.Entities.Blocks;
using VRage.Game;

namespace DePatch
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

        private static bool IsMatch(MyCubeBlockDefinition definition)
        {
            HashSet<string> blockList = new HashSet<string>(DePatchPlugin.Instance.Config.TargetedBlocks);
            if (blockList.Count == 0)
            {
                return false;
            }
            bool found = false;
            foreach (string name in blockList)
            {
                if (!name.Equals(definition.ToString().Substring(16), StringComparison.OrdinalIgnoreCase))
                {
                    MyObjectBuilderType typeId = definition.Id.TypeId;
                    if (!name.Equals(typeId.ToString().Substring(16), StringComparison.OrdinalIgnoreCase))
                    {
                        MyStringHash subtypeId = definition.Id.SubtypeId;
                        if (!name.Equals(subtypeId.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                    }
                }
                return true;
            }
            return found;
        }

        private static bool OwnerFactionOnline(long owner)
        {
            if (MySession.Static.Players.IsPlayerOnline(owner) || ExemptPlayerOrFaction(owner))
            {
                return true;
            }
            MyFaction faction = MySession.Static.Factions.GetPlayerFaction(owner);
            if (faction == null)
            {
                return false;
            }
            if (faction.IsEveryoneNpc() || ExemptPlayerOrFaction(faction.FactionId))
            {
                return true;
            }

            if (DePatchPlugin.Instance.Config.AllowFactions)
            {
                foreach (KeyValuePair<long, MyFactionMember> member in faction.Members)
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
            MyIdentity identity = MySession.Static.Players.TryGetIdentity(id);
            if (identity != null)
            {
                MyFaction playerFaction = MySession.Static.Factions.GetPlayerFaction(id);
                if (logicException.Contains(identity.DisplayName) || (playerFaction != null && logicException.Contains(playerFaction.Tag)))
                {
                    return true;
                }
            }
            IMyFaction faction = MySession.Static.Factions.TryGetFactionById(id);
            if (faction != null && logicException.Contains(faction.Tag))
            {
                return true;
            }
            return false;
        }
    }
}
