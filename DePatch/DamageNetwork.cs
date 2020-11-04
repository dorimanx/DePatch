using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x0600001A RID: 26 RVA: 0x00002A7C File Offset: 0x00000C7C
		public static ConcurrentDictionary<MyCubeGrid, List<DamageContract>> DamageQueue { get; } = new ConcurrentDictionary<MyCubeGrid, List<DamageContract>>();

		// Token: 0x0600001B RID: 27 RVA: 0x00002A84 File Offset: 0x00000C84
		internal static bool Prefix(MySlimBlock block, float damage, MyStringHash damageType, MyHitInfo? hitInfo, long attackerId)
		{
			if (!DePatchPlugin.Instance.Config.DamageThreading)
			{
				return true;
			}
			if (damage < 1f || block == null || block.CubeGrid == null || block.CubeGrid.MarkedForClose || block.CubeGrid.Closed || block.FatBlock == null || block.FatBlock.MarkedForClose || block.FatBlock.Closed)
			{
				return false;
			}
			DamageContract contract = new DamageContract(block.FatBlock.EntityId, damage, damageType, hitInfo, attackerId);
			DamageNetwork.DamageQueue.AddOrUpdate(block.CubeGrid, new List<DamageContract>
			{
				contract
			}, delegate(MyCubeGrid b, List<DamageContract> l)
			{
				l.Add(contract);
				return l;
			});
			return false;
		}

		// Token: 0x0400000B RID: 11
		internal const ushort DAMAGE_CHANNEL = 64467;

		// Token: 0x0400000C RID: 12
		internal static Logger Log = LogManager.GetCurrentClassLogger();
	}
}
