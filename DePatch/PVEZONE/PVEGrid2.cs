﻿using System.Collections.Generic;
using System.Linq;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using VRageMath;

namespace DePatch.PVEZONE
{
    internal class PVEGrid2
    {
        private MyCubeGrid cubeGrid2;

        public static Dictionary<MyCubeGrid, PVEGrid2> Grids2 = new Dictionary<MyCubeGrid, PVEGrid2>();

        public PVEGrid2(MyCubeGrid grid2) => cubeGrid2 = grid2;

        public void OnGridEntered2()
        {
            if (MySession.Static.Players.GetOnlinePlayers().Count() > 0)
            {
                var GridOwnedByPlayer = FindOnlineOwner2(cubeGrid2);
                if (GridOwnedByPlayer != null)
                {
                    if (cubeGrid2 != null && cubeGrid2.BigOwners.Count >= 1)
                    {
                        if (cubeGrid2.DisplayName.Contains("Event Horizon at ") || cubeGrid2.DisplayName.Contains("Container MK-"))
                            return;

                        MyVisualScriptLogicProvider.ShowNotification(
                            DePatchPlugin.Instance.Config.PveMessageEntered2.Contains("{0}") ? string.Format(DePatchPlugin.Instance.Config.PveMessageEntered2, cubeGrid2.DisplayName) : DePatchPlugin.Instance.Config.PveMessageEntered2,
                            10000,
                            "White",
                            GridOwnedByPlayer.Identity.IdentityId);
                    }
                }
            }
        }

        public void OnGridLeft2()
        {
            if (MySession.Static.Players.GetOnlinePlayers().Count() > 0)
            {
                var GridOwnedByPlayer = FindOnlineOwner2(cubeGrid2);
                if (GridOwnedByPlayer != null)
                {
                    if (cubeGrid2 != null && cubeGrid2.BigOwners.Count >= 1)
                    {
                        if (cubeGrid2.DisplayName.Contains("Event Horizon at ") || cubeGrid2.DisplayName.Contains("Container MK-"))
                            return;

                        MyVisualScriptLogicProvider.ShowNotification(
                            DePatchPlugin.Instance.Config.PveMessageLeft2.Contains("{0}") ? string.Format(DePatchPlugin.Instance.Config.PveMessageLeft2, cubeGrid2.DisplayName) : DePatchPlugin.Instance.Config.PveMessageLeft2,
                            10000,
                            "White",
                            GridOwnedByPlayer.Identity.IdentityId);
                    }
                }
            }
        }

        public bool InPVEZone2()
        {
            return PVE.PVESphere2.Contains(cubeGrid2.PositionComp.GetPosition()) == ContainmentType.Contains;
        }

        private static MyPlayer FindOnlineOwner2(MyCubeGrid grid)
        {
            var controllingPlayer = MySession.Static.Players.GetControllingPlayer(grid);
            if (controllingPlayer != null)
                return controllingPlayer;

            if (grid.BigOwners.Count < 1)
            {
                var listsmall = grid.SmallOwners.ToList();
                var dictionarysmall = MySession.Static.Players.GetOnlinePlayers().ToDictionary((MyPlayer b) => b.Identity.IdentityId);
                foreach (var item in listsmall)
                {
                    if (dictionarysmall.ContainsKey(item))
                        return dictionarysmall[item];
                }
            }
            var list = grid.BigOwners.ToList();
            list.AddList(grid.SmallOwners);
            var dictionary = MySession.Static.Players.GetOnlinePlayers().ToDictionary((MyPlayer b) => b.Identity.IdentityId);
            foreach (var item in list)
            {
                if (dictionary.ContainsKey(item))
                    return dictionary[item];
            }
            return null;
        }
    }
}
