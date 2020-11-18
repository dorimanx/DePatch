using System.Collections.Concurrent;
using System.Collections.Generic;
using HarmonyLib;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace DePatch
{
    [HarmonyPatch(typeof(MySlimBlock), "SendDamage")]
    public class DamageNetwork
    {
        internal static Logger Log = LogManager.GetCurrentClassLogger();
        internal const ushort DAMAGE_CHANNEL = 64467;
        private static readonly ConcurrentDictionary<MyCubeGrid, List<DamageContract>> damageQueue = new ConcurrentDictionary<MyCubeGrid, List<DamageContract>>();

        public static ConcurrentDictionary<MyCubeGrid, List<DamageContract>> GetDamageQueue()
        {
            return damageQueue;
        }

        internal static bool Prefix(MySlimBlock block, float damage, MyStringHash damageType, MyHitInfo? hitInfo, long attackerId)
        {
            if (!DePatchPlugin.Instance.Config.DamageThreading)
            {
                return true;
            }

            if (damage >= 1.0 && block != null && block.CubeGrid != null && !block.CubeGrid.MarkedForClose && !block.CubeGrid.Closed && block.FatBlock != null && !block.FatBlock.MarkedForClose && !block.FatBlock.Closed)
            {
                DamageContract contract = new DamageContract(block.FatBlock.EntityId, damage, damageType, hitInfo, attackerId);
                _ = GetLists(block, contract);
                return false;
            }
            return false;
        }

        private static List<DamageContract> GetLists(MySlimBlock block, DamageContract contract)
        {
            return GetDamageQueue().AddOrUpdate(block.CubeGrid, new List<DamageContract>
            {
                contract
            }, delegate (MyCubeGrid b, List<DamageContract> l)
            {
                l.Add(contract);
                return l;
            });
        }
    }
}
