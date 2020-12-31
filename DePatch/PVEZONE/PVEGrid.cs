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

        public PVEGrid(MyCubeGrid grid) => cubeGrid = grid;

        public void OnGridEntered()
        {
            int OnlinePlayers = MySession.Static.Players.GetOnlinePlayers().Count();
            if (OnlinePlayers > 0)
            {
                MyPlayer GridOwnedByPlayer = FindOnlineOwner(cubeGrid);
                if (GridOwnedByPlayer != null)
                {
                    if (cubeGrid != null && cubeGrid.BigOwners.Count >= 1)
                    {
                        if ((cubeGrid.DisplayName != "Event Horizon at Universe Gate") || (cubeGrid.DisplayName != "Event Horizon at Stargate") || (cubeGrid.DisplayName != "Event Horizon at Atlantis Gate"))
                        {
                            MyVisualScriptLogicProvider.ShowNotification(
                                DePatchPlugin.Instance.Config.PveMessageEntered.Contains("{0}") ? string.Format(DePatchPlugin.Instance.Config.PveMessageEntered, cubeGrid.DisplayName) : DePatchPlugin.Instance.Config.PveMessageEntered,
                                10000,
                                "White",
                                GridOwnedByPlayer.Identity.IdentityId);
                        }
                    }
                }
            }
        }

        public void OnGridLeft()
        {
            int OnlinePlayers = MySession.Static.Players.GetOnlinePlayers().Count();
            if (OnlinePlayers > 0)
            {
                MyPlayer GridOwnedByPlayer = FindOnlineOwner(cubeGrid);
                if (GridOwnedByPlayer != null)
                {
                    if (cubeGrid != null && cubeGrid.BigOwners.Count >= 1)
                    {
                        if ((cubeGrid.DisplayName != "Event Horizon at Universe Gate") || (cubeGrid.DisplayName != "Event Horizon at Stargate") || (cubeGrid.DisplayName != "Event Horizon at Atlantis Gate"))
                        {
                            MyVisualScriptLogicProvider.ShowNotification(
                            DePatchPlugin.Instance.Config.PveMessageLeft.Contains("{0}") ? string.Format(DePatchPlugin.Instance.Config.PveMessageLeft, cubeGrid.DisplayName) : DePatchPlugin.Instance.Config.PveMessageLeft,
                            10000,
                            "White",
                            GridOwnedByPlayer.Identity.IdentityId);
                        }
                    }
                }
            }
        }

        public bool InPVEZone()
        {
            return PVE.PVESphere.Contains(cubeGrid.PositionComp.GetPosition()) == ContainmentType.Contains;
        }

        private static MyPlayer FindOnlineOwner(MyCubeGrid grid)
        {
            MyPlayer controllingPlayer = MySession.Static.Players.GetControllingPlayer(grid);
            if (controllingPlayer != null)
            {
                return controllingPlayer;
            }
            if (grid.BigOwners.Count < 1)
            {
                List<long> listsmall = grid.SmallOwners.ToList();
                Dictionary<long, MyPlayer> dictionarysmall = MySession.Static.Players.GetOnlinePlayers().ToDictionary((MyPlayer b) => b.Identity.IdentityId);
                foreach (long item in listsmall)
                {
                    if (dictionarysmall.ContainsKey(item))
                    {
                        return dictionarysmall[item];
                    }
                }
            }
            List<long> list = grid.BigOwners.ToList();
            list.AddList(grid.SmallOwners);
            Dictionary<long, MyPlayer> dictionary = MySession.Static.Players.GetOnlinePlayers().ToDictionary((MyPlayer b) => b.Identity.IdentityId);
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
