using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using VRageMath;

namespace DePatch
{
    [HarmonyPatch(typeof(MyDrillBase), "TryDrillBlocks")]
    internal class MyDrillDamageFix
    {
        private static FieldInfo drillEntity = typeof(MyDrillBase).GetField("m_drillEntity", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new MissingFieldException("m_drillEntity is missing");


        private static bool Prefix(MyDrillBase __instance, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
                return true;

            if (drillEntity.GetValue(__instance) is MyHandDrill handDrill)
            {
                MyPlayer myPlayer = MySession.Static.Players.GetOnlinePlayers().ToList().Find((MyPlayer b) => b.Identity.IdentityId == handDrill.OwnerIdentityId);
                if (PVE.PVESphere.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                {
                    __result = false;
                    return false;
                }
            }
            else
            {
                if (drillEntity.GetValue(__instance) is MyShipDrill myShipDrill && PVE.EntitiesInZone.Contains(myShipDrill.CubeGrid.EntityId))
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }
}
