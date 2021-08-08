using Sandbox.Game.Entities;
using System.Collections.Generic;
using System.Reflection;
using Torch.Managers.PatchManager;
using VRage.Collections;
using VRage.Game.Entity;
using VRage.ModAPI;

namespace DePatch.GamePatches
{
	[PatchShim]

	public static class UpdateAfterSimulation100Fix
	{
        private static FieldInfo m_entitiesForUpdate100;

		private static void Patch(PatchContext ctx)
		{
			m_entitiesForUpdate100 = typeof(MyParallelEntityUpdateOrchestrator).GetField("m_entitiesForUpdate100", BindingFlags.NonPublic | BindingFlags.Instance);

			ctx.GetPattern(typeof(MyParallelEntityUpdateOrchestrator).GetMethod("UpdateAfterSimulation100", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).
				Prefixes.Add(typeof(UpdateAfterSimulation100Fix).GetMethod(nameof(UpdateAfterSimulation100Dpatch), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
		}

        private static bool UpdateAfterSimulation100Dpatch(MyParallelEntityUpdateOrchestrator __instance)
		{
			if (DePatchPlugin.Instance.Config.UpdateAfterSimulation100FIX)
				return true;

			var My_entitiesForUpdate100 = (MyDistributedUpdater<List<MyEntity>, MyEntity>)m_entitiesForUpdate100.GetValue(__instance);

			foreach (MyEntity myEntity in My_entitiesForUpdate100)
			{
				if (myEntity != null && !myEntity.MarkedForClose && (myEntity.Flags & EntityFlags.NeedsUpdate100) != (EntityFlags)0 && myEntity.InScene)
				{
					myEntity.UpdateAfterSimulation100();
				}
			}
			return false;
		}
	}
}
