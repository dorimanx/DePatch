using System.Collections.Generic;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRage.Game.ModAPI;
using VRage.Utils;
using System.Linq;

namespace DePatch
{
    [HarmonyPatch(typeof(MySlimBlock), "SendDamageBatch")]
    public class DefomationNetwork
    {
        internal static bool Prefix(
          Dictionary<MySlimBlock, float> blocks,
          MyStringHash damageType,
          long attackerId)
        {
            if (!DePatchPlugin.Instance.Config.DamageThreading)
            {
                return true;
            }
            if (blocks.Count >= 1)
            {
                foreach (var (key, contract) in from KeyValuePair<MySlimBlock, float> block in blocks
                                                let damage = block.Value
                                                let key = block.Key
                                                where (double)damage >= 1.0 && key != null && (key.CubeGrid != null && !key.CubeGrid.MarkedForClose) && (!key.CubeGrid.Closed && key.FatBlock != null && (!key.FatBlock.MarkedForClose && !key.FatBlock.Closed))
                                                let contract = new DamageContract(key.FatBlock.EntityId, damage, damageType, new MyHitInfo?(), attackerId)
                                                select (key, contract))
                {
                    _ = DamageNetwork.GetDamageQueue().AddOrUpdate(key.CubeGrid, new List<DamageContract>
                    {
                        contract
                    }, delegate (MyCubeGrid b, List<DamageContract> l)
                    {
                        l.Add(contract);
                        return l;
                    });
                }

                return false;
            }
            return false;
        }
    }
}
