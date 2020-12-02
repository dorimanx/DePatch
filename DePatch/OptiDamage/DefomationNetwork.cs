using System.Collections.Generic;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRage.Utils;

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
            if (DePatchPlugin.Instance.Config.DamageThreading)
            {
                if (blocks.Count < 1)
                {
                    return false;
                }
                foreach (KeyValuePair<MySlimBlock, float> keyValuePair in blocks)
                {
                    MySlimBlock key = keyValuePair.Key;
                    if (keyValuePair.Value >= 1f && key != null &&
                        key.CubeGrid != null &&
                        !key.CubeGrid.MarkedForClose &&
                        !key.CubeGrid.Closed && key.FatBlock != null &&
                        !key.FatBlock.MarkedForClose &&
                        !key.FatBlock.Closed)
                    {
                        DamageContract contract = new DamageContract(key.FatBlock.EntityId, keyValuePair.Value, damageType, null, attackerId);
                        _ = DamageNetwork.DamageQueue.AddOrUpdate(key.CubeGrid, new List<DamageContract>
                    {
                        contract
                    }, delegate (MyCubeGrid b, List<DamageContract> l)
                    {
                        l.Add(contract);
                        return l;
                    });
                    }
                }
                return false;
            }
            return true;
        }
    }
}
