using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using VRageMath;

namespace DePatch
{
	internal class PVEGrid
	{
		// Token: 0x06000010 RID: 16 RVA: 0x0000247B File Offset: 0x0000067B
		public PVEGrid(MyCubeGrid grid)
		{
			this.cubeGrid = grid;
		}

		// Token: 0x06000011 RID: 17 RVA: 0x0000248C File Offset: 0x0000068C
		public void OnGridEntered()
		{
			MyVisualScriptLogicProvider.ShowNotification(DePatchPlugin.Instance.Config.PveMessageEntered.Contains("{0}") ? string.Format(DePatchPlugin.Instance.Config.PveMessageEntered, this.cubeGrid.DisplayName) : DePatchPlugin.Instance.Config.PveMessageEntered, 10000, "White", PVEGrid.FindOnlineOwner(this.cubeGrid).Identity.IdentityId);
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002508 File Offset: 0x00000708
		public void OnGridLeft()
		{
			MyVisualScriptLogicProvider.ShowNotification(DePatchPlugin.Instance.Config.PveMessageLeft.Contains("{0}") ? string.Format(DePatchPlugin.Instance.Config.PveMessageLeft, this.cubeGrid.DisplayName) : DePatchPlugin.Instance.Config.PveMessageLeft, 10000, "White", PVEGrid.FindOnlineOwner(this.cubeGrid).Identity.IdentityId);
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002584 File Offset: 0x00000784
		public bool InPVEZone()
		{
			return PVE.PVESphere.Contains(this.cubeGrid.PositionComp.GetPosition()) == ContainmentType.Contains;
		}

		// Token: 0x06000014 RID: 20 RVA: 0x000025A4 File Offset: 0x000007A4
		private static MyPlayer FindOnlineOwner(MyCubeGrid grid)
		{
			MyPlayer controllingPlayer = MySession.Static.Players.GetControllingPlayer(grid);
			if (controllingPlayer != null)
			{
				return controllingPlayer;
			}
			if (grid.BigOwners.Count < 1)
			{
				return null;
			}
			List<long> list = grid.BigOwners.ToList<long>();
			list.AddList(grid.SmallOwners);
			Dictionary<long, MyPlayer> dictionary = MySession.Static.Players.GetOnlinePlayers().ToDictionary((MyPlayer b) => b.Identity.IdentityId);
			foreach (long key in list)
			{
				if (dictionary.ContainsKey(key))
				{
					return dictionary[key];
				}
			}
			return null;
		}

		// Token: 0x04000004 RID: 4
		public static readonly Dictionary<MyCubeGrid, PVEGrid> Grids = new Dictionary<MyCubeGrid, PVEGrid>();

		// Token: 0x04000005 RID: 5
		private MyCubeGrid cubeGrid;

		// Token: 0x04000006 RID: 6
		public int Tick;
	}
}
