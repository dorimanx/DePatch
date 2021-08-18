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
        private MyCubeGrid cubeGrid;

        public static Dictionary<MyCubeGrid, PVEGrid> Grids = new Dictionary<MyCubeGrid, PVEGrid>();

        public PVEGrid(MyCubeGrid grid) => cubeGrid = grid;

        public void OnGridEntered()
        {
            if (MySession.Static.Players.GetOnlinePlayers().Count() > 0)
            {
                if (cubeGrid != null && cubeGrid.BigOwners.Count > 0)
                {
                    var GridOwnedByPlayer = PVE.FindOnlineOwner(cubeGrid);
                    if (GridOwnedByPlayer != null)
                    {
                        if (cubeGrid.DisplayName.Contains("Event Horizon at ") || cubeGrid.DisplayName.Contains("Container MK-"))
                            return;

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
                if (cubeGrid != null && cubeGrid.BigOwners.Count > 0)
                {
                    var GridOwnedByPlayer = PVE.FindOnlineOwner(cubeGrid);
                    if (GridOwnedByPlayer != null)
                    {
                        if (cubeGrid.DisplayName.Contains("Event Horizon at ") || cubeGrid.DisplayName.Contains("Container MK-"))
                            return;

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
    }
}
