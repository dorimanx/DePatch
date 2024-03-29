﻿using System;
using System.Linq;
using Sandbox.Engine.Utils;
using Sandbox.Game;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using Torch.Managers.PatchManager;
using VRage.Game;

namespace DePatch.GamePatches
{
    [PatchShim]

    internal static class MyRefineryPatch
    {
        public static void Patch(PatchContext ctx)
        {
            ctx.Prefix(typeof(MyRefinery), typeof(MyRefineryPatch), nameof(DoUpdateTimerTick));
        }

        private static void DoUpdateTimerTick(MyRefinery __instance)
        {
            if (DePatchPlugin.Instance.Config.Enabled)
            {
                if (MyFakes.FORCE_ADD_TRASH_REMOVAL_MENU)
                    MyFakes.FORCE_ADD_TRASH_REMOVAL_MENU = false;

                if (__instance == null)
                    return;

                var blockSubType = __instance.BlockDefinition.Id.SubtypeName;
                var LargeSmallSheld = "LargeShipSmallShieldGeneratorBase";
                var LargeLargeShield = "LargeShipLargeShieldGeneratorBase";
                var SmallSmallShield = "SmallShipSmallShieldGeneratorBase";
                var SmallMicroShield = "SmallShipMicroShieldGeneratorBase";

                if (DePatchPlugin.Instance.Config.DisableProductionOnShip && !__instance.CubeGrid.IsStatic && __instance.Enabled)
                {
                    if (string.Compare(LargeSmallSheld, blockSubType, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                        string.Compare(LargeLargeShield, blockSubType, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                        string.Compare(SmallSmallShield, blockSubType, StringComparison.InvariantCultureIgnoreCase) == 0 ||
                        string.Compare(SmallMicroShield, blockSubType, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        // do nothing
                    }
                    else
                        __instance.Enabled = false;
                }

                if (__instance.CubeGrid?.IsStatic == false && __instance.CubeGrid.GridSizeEnum == MyCubeSize.Large && DePatchPlugin.Instance.Config.ShieldsAntiHack)
                {
                    if (MySession.Static.Players.IdentityIsNpc(__instance.CubeGrid.BigOwners.FirstOrDefault()))
                        return;

                    var ShieldsBlocks = __instance.CubeGrid.GridSystems.TerminalSystem.Blocks.OfType<MyRefinery>().Where(x => x.BlockDefinition.Id.SubtypeName.Contains(LargeSmallSheld));

                    if (ShieldsBlocks.Count() > 8)
                    {
                        var gridGroups = ShieldsBlocks.GroupBy(b => b.CubeGrid).ToList();
                        var topGridGroup = gridGroups.OrderByDescending(b => b.Key.BlocksCount).First();
                        var AlertPlayer = false;

                        if (topGridGroup.Count() > 8)
                            topGridGroup = null;

                        foreach (var item in __instance.CubeGrid.GridSystems.TerminalSystem.Blocks.OfType<MyFunctionalBlock>().Where(b => b.CubeGrid != topGridGroup?.Key))
                        {
                            if (item.CubeGrid.IsStatic) continue;
                            if (item.Enabled == false) continue;

                            foreach (var grid in gridGroups)
                            {
                                if (item is MyRefinery && item.BlockDefinition.Id.SubtypeName.Contains(LargeSmallSheld) && item.Enabled)
                                {
                                    item.Enabled = false;
                                    AlertPlayer = true;
                                }
                            }
                        }
                        if (MySession.Static.Players.GetOnlinePlayers().Count() > 0)
                        {
                            if (AlertPlayer)
                            {
                                var controllingPlayer = MySession.Static.Players.GetControllingPlayer(__instance.CubeGrid);
                                if (controllingPlayer != null)
                                {
                                    MyVisualScriptLogicProvider.ShowNotification("You cannot connect more than 8 shields!!! Extra shields are turned OFF!\nБольше 8 щитов подключить нельзя !!! Дополнительные щиты выключены", 20000, MyFontEnum.Red, controllingPlayer.Identity.IdentityId);
                                }
                                else
                                {
                                    var list = __instance.CubeGrid.BigOwners.ToList();
                                    var dictionary = MySession.Static.Players.GetOnlinePlayers().ToDictionary((MyPlayer b) => b.Identity.IdentityId);
                                    foreach (var item in list)
                                    {
                                        if (dictionary.ContainsKey(item))
                                        {
                                            if (dictionary[item].Identity.IdentityId != 0)
                                                MyVisualScriptLogicProvider.ShowNotification("You cannot connect more than 8 shields!!! Extra shields are turned OFF!\nБольше 8 щитов подключить нельзя !!! Дополнительные  щиты выключены", 20000, MyFontEnum.Red, dictionary[item].Identity.IdentityId);
                                        }
                                    }
                                }
                                AlertPlayer = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
