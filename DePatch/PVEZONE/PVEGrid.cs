using System.Collections.Generic;
using System.Linq;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using VRageMath;

namespace DePatch
{
    internal class PVEGrid
    {
        public static readonly Dictionary<MyCubeGrid, PVEGrid> Grids = new Dictionary<MyCubeGrid, PVEGrid>();

        private MyCubeGrid cubeGrid;

        public int Tick;

        public MyCubeGrid CubeGrid { get => cubeGrid; set => cubeGrid = value; }

        public PVEGrid(MyCubeGrid grid)
        {
            CubeGrid = grid;
        }

        public void OnGridEntered()
        {
            if (CubeGrid.BigOwners.Count >= 1)
                MyVisualScriptLogicProvider.ShowNotification(DePatchPlugin.Instance.Config.PveMessageEntered.Contains("{0}") ? string.Format(DePatchPlugin.Instance.Config.PveMessageEntered, CubeGrid.DisplayName) : DePatchPlugin.Instance.Config.PveMessageEntered, 10000, "White", PVEGrid.FindOnlineOwner(CubeGrid).Identity.IdentityId);
        }

        public void OnGridLeft()
        {
            if (CubeGrid.BigOwners.Count >= 1)
                MyVisualScriptLogicProvider.ShowNotification(DePatchPlugin.Instance.Config.PveMessageLeft.Contains("{0}") ? string.Format(DePatchPlugin.Instance.Config.PveMessageLeft, CubeGrid.DisplayName) : DePatchPlugin.Instance.Config.PveMessageLeft, 10000, "White", PVEGrid.FindOnlineOwner(CubeGrid).Identity.IdentityId);
        }

        public bool InPVEZone()
        {
            return PVE.PVESphere.Contains(CubeGrid.PositionComp.GetPosition()) == ContainmentType.Contains;
        }

        private static MyPlayer FindOnlineOwner(MyCubeGrid grid)
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
