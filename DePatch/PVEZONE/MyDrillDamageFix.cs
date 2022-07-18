using System.Linq;
using System.Reflection;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using Torch.Managers.PatchManager;
using VRageMath;

namespace DePatch.PVEZONE
{
    [PatchShim]

    internal static class MyDrillDamageFix
    {
        private static FieldInfo drillEntity;

        public static void Patch(PatchContext ctx)
        {
            drillEntity = typeof(MyDrillBase).EasyField("m_drillEntity");
            ctx.Prefix(typeof(MyDrillBase), typeof(MyDrillDamageFix), nameof(TryDrillBlocks));
        }

        private static bool TryDrillBlocks(MyDrillBase __instance, ref bool __result)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.PveZoneEnabled)
                return true;

            if (drillEntity.GetValue(__instance) is MyHandDrill handDrill)
            {
                var myPlayer = MySession.Static.Players.GetOnlinePlayers().ToList().Find((MyPlayer b) => b.Identity.IdentityId == handDrill.OwnerIdentityId);

                if (myPlayer == null || myPlayer.Character == null)
                    return true;

                var PlayerPosition = myPlayer.Character.PositionComp.GetPosition();

                if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
                {
                    var zone1 = false;
                    var zone2 = false;

                    if (PVE.PVESphere.Contains(PlayerPosition) == ContainmentType.Contains)
                        zone1 = true;
                    if (PVE.PVESphere2.Contains(PlayerPosition) == ContainmentType.Contains)
                        zone2 = true;

                    if (zone1 || zone2)
                    {
                        __result = false;
                        return false;
                    }
                }
                else if (PVE.PVESphere.Contains(PlayerPosition) == ContainmentType.Contains)
                {
                    __result = false;
                    return false;
                }
            }
            else if (DePatchPlugin.Instance.Config.PveZoneEnabled2)
            {
                var zone1 = false;
                var zone2 = false;
                if (drillEntity.GetValue(__instance) is MyShipDrill myShipDrill && PVE.EntitiesInZone.Contains(myShipDrill.CubeGrid.EntityId))
                    zone1 = true;
                if (drillEntity.GetValue(__instance) is MyShipDrill myShipDrill2 && PVE.EntitiesInZone2.Contains(myShipDrill2.CubeGrid.EntityId))
                    zone2 = true;

                if (zone1 || zone2)
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
