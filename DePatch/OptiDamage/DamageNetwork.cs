using System.Collections.Concurrent;
using System.Collections.Generic;
using HarmonyLib;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace DePatch.OptiDamage
{
    //[HarmonyPatch(typeof(MySlimBlock), "SendDamage")]  DISABLE THIS
    public class DamageNetwork
    {
        internal static Logger Log = LogManager.GetCurrentClassLogger();
        internal const ushort DAMAGE_CHANNEL = 64467;
        public static ConcurrentDictionary<MyCubeGrid, List<DamageContract>> DamageQueue { get; } = new ConcurrentDictionary<MyCubeGrid, List<DamageContract>>();

        internal static bool Prefix(MySlimBlock block, float damage, MyStringHash damageType, MyHitInfo? hitInfo, long attackerId)
        {
            if (DePatchPlugin.Instance.Config.DamageThreading)
            {
                if (damage < 1f || block == null || block.CubeGrid == null || block.CubeGrid.MarkedForClose || block.CubeGrid.Closed || block.FatBlock == null || block.FatBlock.MarkedForClose || block.FatBlock.Closed)
                {
                    return false;
                }
                var contract = new DamageContract(block.FatBlock.EntityId, damage, damageType, hitInfo, attackerId);
                _ = DamageNetwork.DamageQueue.AddOrUpdate(block.CubeGrid, new List<DamageContract>
            {
                contract
            }, delegate (MyCubeGrid b, List<DamageContract> l)
            {
                l.Add(contract);
                return l;
            });
                return false;
            }
            return true;
        }
    }
}
