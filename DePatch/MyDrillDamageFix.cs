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
		// Token: 0x06000009 RID: 9 RVA: 0x00002224 File Offset: 0x00000424
		private static bool Prefix(MyDrillBase __instance, ref bool __result, MyCubeGrid grid)
		{
			if (!DePatchPlugin.Instance.Config.PveZoneEnabled)
			{
				return true;
			}
			object value = MyDrillDamageFix.drillEntity.GetValue(__instance);
			MyHandDrill handDrill = value as MyHandDrill;
			if (handDrill != null)
			{
				MyPlayer myPlayer = MySession.Static.Players.GetOnlinePlayers().ToList<MyPlayer>().Find((MyPlayer b) => b.Identity.IdentityId == handDrill.OwnerIdentityId);
				if (PVE.PVESphere.Contains(myPlayer.Character.PositionComp.GetPosition()) == ContainmentType.Contains)
				{
					__result = false;
					return false;
				}
			}
			else
			{
				MyShipDrill myShipDrill = MyDrillDamageFix.drillEntity.GetValue(__instance) as MyShipDrill;
				if (myShipDrill != null && PVE.EntitiesInZone.Contains(myShipDrill.CubeGrid.EntityId))
				{
					__result = false;
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000022EB File Offset: 0x000004EB
		// Note: this type is marked as 'beforefieldinit'.
		static MyDrillDamageFix()
		{
			FieldInfo field = typeof(MyDrillBase).GetField("m_drillEntity", BindingFlags.Instance | BindingFlags.NonPublic);
			if (field == null)
			{
				throw new MissingFieldException("m_drillEntity is missing");
			}
			MyDrillDamageFix.drillEntity = field;
		}

		// Token: 0x04000003 RID: 3
		private static FieldInfo drillEntity;
	}
}
