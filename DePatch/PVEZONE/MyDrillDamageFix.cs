using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.Game.World;
using VRageMath;

namespace DePatch
{
	[HarmonyPatch(typeof(MyDrillBase), "TryDrillBlocks")]
	internal class MyDrillDamageFix
	{
		private static FieldInfo drillEntity;

        public static FieldInfo DrillEntity { get => drillEntity; set => drillEntity = value; }

        private static bool Prefix(MyDrillBase __instance, ref bool __result, MyCubeGrid grid)
		{
			if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
				return true;

            if (MyDrillDamageFix.DrillEntity.GetValue(__instance) is MyHandDrill handDrill)
            {
                if (PVE.PVESphere.Contains(MySession.Static.Players.GetOnlinePlayers().ToList().Find((MyPlayer b) => b.Identity.IdentityId == handDrill.OwnerIdentityId).Character.PositionComp.GetPosition()) == ContainmentType.Contains)
                {
                    __result = false;
                    return false;
                }
            }
            else
            {
                if (MyDrillDamageFix.DrillEntity.GetValue(__instance) is MyShipDrill myShipDrill && PVE.EntitiesInZone.Contains(myShipDrill.CubeGrid.EntityId))
                {
                    __result = false;
                    return false;
                }
            }
            return true;
		}

		static MyDrillDamageFix()
		{
            if (typeof(MyDrillBase).GetField("m_drillEntity", BindingFlags.Instance | BindingFlags.NonPublic) is null)
				throw new MissingFieldException("m_drillEntity is missing");

            MyDrillDamageFix.DrillEntity = typeof(MyDrillBase).GetField("m_drillEntity", BindingFlags.Instance | BindingFlags.NonPublic);
		}
	}
}
