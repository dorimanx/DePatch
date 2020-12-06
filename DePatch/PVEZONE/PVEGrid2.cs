using System.Collections.Generic;
using System.Linq;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using VRageMath;

namespace DePatch
{
    internal class PVEGrid2
    {
        public static readonly Dictionary<MyCubeGrid, PVEGrid2> Grids2 = new Dictionary<MyCubeGrid, PVEGrid2>();

        private MyCubeGrid cubeGrid;

        public MyCubeGrid CubeGrid { get => cubeGrid; set => cubeGrid = value; }

        public PVEGrid2(MyCubeGrid grid)
        {
            CubeGrid = grid;
        }

        public void OnGridEntered2()
        {
            if (CubeGrid.BigOwners.Count >= 1)
                MyVisualScriptLogicProvider.ShowNotification(DePatchPlugin.Instance.Config.PveMessageEntered2.Contains("{0}") ? string.Format(DePatchPlugin.Instance.Config.PveMessageEntered2, CubeGrid.DisplayName) : DePatchPlugin.Instance.Config.PveMessageEntered2, 10000, "White", PVEGrid2.FindOnlineOwner2(CubeGrid).Identity.IdentityId);
        }

        public void OnGridLeft2()
        {
            if (CubeGrid.BigOwners.Count >= 1)
                MyVisualScriptLogicProvider.ShowNotification(DePatchPlugin.Instance.Config.PveMessageLeft2.Contains("{0}") ? string.Format(DePatchPlugin.Instance.Config.PveMessageLeft2, CubeGrid.DisplayName) : DePatchPlugin.Instance.Config.PveMessageLeft2, 10000, "White", PVEGrid2.FindOnlineOwner2(CubeGrid).Identity.IdentityId);
        }

        public bool InPVEZone2()
        {
            return PVE.PVESphere2.Contains(CubeGrid.PositionComp.GetPosition()) == ContainmentType.Contains;
        }

        private static MyPlayer FindOnlineOwner2(MyCubeGrid grid)
        {
            MyPlayer controllingPlayer = MySession.Static.Players.GetControllingPlayer(grid);
            List<long> list = grid.BigOwners.ToList<long>();
            list.AddList(grid.SmallOwners);
            Dictionary<long, MyPlayer> dictionary = MySession.Static.Players.GetOnlinePlayers().ToDictionary((MyPlayer b) => b.Identity.IdentityId);

            if (controllingPlayer != null)
            {
                return controllingPlayer;
            }
            if (grid.BigOwners.Count < 1)
            {
                return null;
            }
            foreach (long item in list)
            {
                if (dictionary.ContainsKey(item))
                {
                    return dictionary[item];
                }
            }
            return null;
        }
    }
}
