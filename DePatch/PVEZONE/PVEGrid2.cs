﻿using System.Collections.Generic;
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

        private MyCubeGrid cubeGrid2;

        public PVEGrid2(MyCubeGrid grid) => cubeGrid2 = grid;

        public void OnGridEntered2()
        {
            int OnlinePlayers = MySession.Static.Players.GetOnlinePlayers().Count();
            if (OnlinePlayers > 0)
            {
                MyPlayer GridOwnedByPlayer = FindOnlineOwner2(cubeGrid2);
                if (GridOwnedByPlayer != null)
                {
                    if (cubeGrid2 != null && cubeGrid2.BigOwners.Count >= 1)
                        MyVisualScriptLogicProvider.ShowNotification(
                            DePatchPlugin.Instance.Config.PveMessageEntered2.Contains("{0}") ? string.Format(DePatchPlugin.Instance.Config.PveMessageEntered2, cubeGrid2.DisplayName) : DePatchPlugin.Instance.Config.PveMessageEntered2,
                            10000,
                            "White",
                            GridOwnedByPlayer.Identity.IdentityId);
                }
            }
        }

        public void OnGridLeft2()
        {
            int OnlinePlayers = MySession.Static.Players.GetOnlinePlayers().Count();
            if (OnlinePlayers > 0)
            {
                MyPlayer GridOwnedByPlayer = FindOnlineOwner2(cubeGrid2);
                if (GridOwnedByPlayer != null)
                {
                    if (cubeGrid2 != null && cubeGrid2.BigOwners.Count >= 1)
                        MyVisualScriptLogicProvider.ShowNotification(
                            DePatchPlugin.Instance.Config.PveMessageLeft2.Contains("{0}") ? string.Format(DePatchPlugin.Instance.Config.PveMessageLeft2, cubeGrid2.DisplayName) : DePatchPlugin.Instance.Config.PveMessageLeft2,
                            10000,
                            "White",
                            GridOwnedByPlayer.Identity.IdentityId);
                }
            }
        }

        public bool InPVEZone2()
        {
            return PVE.PVESphere2.Contains(cubeGrid2.PositionComp.GetPosition()) == ContainmentType.Contains;
        }

        private static MyPlayer FindOnlineOwner2(MyCubeGrid grid)
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