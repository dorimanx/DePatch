using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using SpaceEngineers.Game.Entities.Blocks;
using Torch.Managers.PatchManager;
using VRage.Game;

namespace DePatch
{
	[PatchShim]
	public static class MySpaceBallPatch
	{
		// Token: 0x06000099 RID: 153 RVA: 0x00004650 File Offset: 0x00002850
		public static void Patch(PatchContext ctx)
		{
			ctx.GetPattern(MySpaceBallPatch.Update).Prefixes.Add(MySpaceBallPatch.UpdatePatch);
			ctx.GetPattern(MySpaceBallPatch.UpdateMass).Prefixes.Add(MySpaceBallPatch.UpdateMassPatch);
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00004688 File Offset: 0x00002888
		public static void PatchMethod(MySpaceBall __instance)
		{
			if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.RemoveMass)
			{
				return;
			}
			((MySpaceBallDefinition)__instance.BlockDefinition).MaxVirtualMass = 0f;
		}

		// Token: 0x0600009B RID: 155 RVA: 0x000046C2 File Offset: 0x000028C2
		public static void PatchMassMethod(MySpaceBall __instance)
		{
			if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.RemoveMass)
			{
				return;
			}
			__instance.VirtualMass = 0f;
		}

		// Token: 0x0600009C RID: 156 RVA: 0x000046F4 File Offset: 0x000028F4
		// Note: this type is marked as 'beforefieldinit'.
		static MySpaceBallPatch()
		{
			MethodInfo method = typeof(MySpaceBallPatch).GetMethod("PatchMethod", BindingFlags.Static | BindingFlags.Public);
			if (method == null)
			{
				throw new Exception("Failed to find patch method");
			}
			MySpaceBallPatch.UpdatePatch = method;
			MySpaceBallPatch.UpdateMass = typeof(MySpaceBall).GetMethod("RefreshPhysicsBody", BindingFlags.Instance | BindingFlags.NonPublic);
			MethodInfo method2 = typeof(MySpaceBallPatch).GetMethod("PatchMassMethod", BindingFlags.Static | BindingFlags.Public);
			if (method2 == null)
			{
				throw new Exception("Failed to find patch method");
			}
			MySpaceBallPatch.UpdateMassPatch = method2;
		}

		// Token: 0x04000054 RID: 84
		internal static readonly MethodInfo Update = typeof(MySpaceBall).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public, null, new Type[]
		{
			typeof(MyObjectBuilder_CubeBlock),
			typeof(MyCubeGrid)
		}, new ParameterModifier[0]);

		// Token: 0x04000055 RID: 85
		internal static readonly MethodInfo UpdatePatch;

		// Token: 0x04000056 RID: 86
		internal static readonly MethodInfo UpdateMass;

		// Token: 0x04000057 RID: 87
		internal static readonly MethodInfo UpdateMassPatch;
	}
}
