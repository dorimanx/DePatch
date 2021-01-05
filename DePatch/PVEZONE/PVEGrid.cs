using System.Collections.Generic;
using System.Linq;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using VRageMath;

namespace DePatch.PVEZONE
{
    internal class PVEGrid
    {
        public static Dictionary<MyCubeGrid, PVEGrid> Grids = new Dictionary<MyCubeGrid, PVEGrid>();

        private MyCubeGrid cubeGrid;

        public PVEGrid(MyCubeGrid grid) => cubeGrid = grid;

        public void OnGridEntered()
        {
            if (MySession.Static.Players.GetOnlinePlayers().Count() > 0)
            {
                var GridOwnedByPlayer = FindOnlineOwner(cubeGrid);
                if (GridOwnedByPlayer != null)
                {
                    if (cubeGrid != null && cubeGrid.BigOwners.Count >= 1)
                    {
                        if (cubeGrid.DisplayName.Contains("Event Horizon at ") || cubeGrid.DisplayName.Contains("Container MK-"))
                        {
                            return;
                        }
                        MyVisualScriptLogicProvider.ShowNotification(
                            DePatchPlugin.Instance.Config.PveMessageEntered.Contains("{0}") ? string.Format(DePatchPlugin.Instance.Config.PveMessageEntered, cubeGrid.DisplayName) : DePatchPlugin.Instance.Config.PveMessageEntered,
                            10000,
                            "White",
                            GridOwnedByPlayer.Identity.IdentityId);
                    }
                }
            }
        }

        public void OnGridLeft()
        {
            if (MySession.Static.Players.GetOnlinePlayers().Count() > 0)
            {
                var GridOwnedByPlayer = FindOnlineOwner(cubeGrid);
                if (GridOwnedByPlayer != null)
                {
                    if (cubeGrid != null && cubeGrid.BigOwners.Count >= 1)
                    {
                        if (cubeGrid.DisplayName.Contains("Event Horizon at ") || cubeGrid.DisplayName.Contains("Container MK-"))
                        {
                            return;
                        }
                        MyVisualScriptLogicProvider.ShowNotification(
                            DePatchPlugin.Instance.Config.PveMessageLeft.Contains("{0}") ? string.Format(DePatchPlugin.Instance.Config.PveMessageLeft, cubeGrid.DisplayName) : DePatchPlugin.Instance.Config.PveMessageLeft,
                            10000,
                            "White",
                            GridOwnedByPlayer.Identity.IdentityId);
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
            var controllingPlayer = MySession.Static.Players.GetControllingPlayer(grid);
            if (controllingPlayer != null)
            {
                return controllingPlayer;
            }
            if (grid.BigOwners.Count < 1)
            {
                var listsmall = grid.SmallOwners.ToList();
                var dictionarysmall = MySession.Static.Players.GetOnlinePlayers().ToDictionary((MyPlayer b) => b.Identity.IdentityId);
                foreach (var item in listsmall)
                {
                    if (dictionarysmall.ContainsKey(item))
                    {
                        return dictionarysmall[item];
                    }
                }
            }
            var list = grid.BigOwners.ToList();
            list.AddList(grid.SmallOwners);
            var dictionary = MySession.Static.Players.GetOnlinePlayers().ToDictionary((MyPlayer b) => b.Identity.IdentityId);
            foreach (var item in list)
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
