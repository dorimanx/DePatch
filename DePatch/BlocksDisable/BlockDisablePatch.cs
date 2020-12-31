using System.Reflection;
using Sandbox.Game.Entities.Cube;
using Torch.Managers.PatchManager;
using System;
using NLog;
using VRage.Game.ModAPI;
using Sandbox.Game.Entities;
using VRage.Game;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.World;
using System.Collections.Generic;
using Sandbox.ModAPI;
using System.Linq;
using VRage.ObjectBuilders;

public enum SpeedingMode
{
    StopGrid,
    DeleteGrid,
    ShowLogOnly
}

namespace DePatch
{
    [PatchShim]
    public static class BlockUpdatePatch
    {
        public static void Patch(PatchContext ctx) => ctx.GetPattern(typeof(MyFunctionalBlock).GetMethod("UpdateBeforeSimulation100", BindingFlags.Instance | BindingFlags.Public)).Prefixes.Add(typeof(BlockUpdatePatch).GetMethod("CheckBlock", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic));

        public static List<MySlimBlock> GetBlocksFromAllGrids(Func<MySlimBlock, bool> filter) { return MyEntities.GetEntities().OfType<MyCubeGrid>().SelectMany(grid => grid.GetBlocks().Where(filter)).ToList(); }

        public static Dictionary<MyDefinitionId, int> CleanupDictionary = new Dictionary<MyDefinitionId, int>();

        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static int StopTick;
        private static int CleanupTick;

        public static bool IsPlayerNearBy(this MySlimBlock block, float distance)
        {
            var players = new List<IMyPlayer>();
            distance *= distance;

            MyAPIGateway.Multiplayer.Players.GetPlayers(players);
            foreach (var x in players)
            {
                if (x.Character != null)
                {
                    var d = (block.WorldPosition - x.Character.GetPosition()).LengthSquared();
                    if (d < distance) return true;
                }
            }
            return false;
        }

        public static void FindAndCleanInventorys(MySlimBlock slimBlock, Dictionary<MyDefinitionId, int> amountDict)
        {
            /* This part of code belong to LordTylus great plugin dev! */
            var typeIdDict = new Dictionary<MyObjectBuilderType, int>();
            var NeedRefresh = false;

            /* get the amounds per TypeId instead of definition */
            foreach (var entry in amountDict)
                typeIdDict.Add(entry.Key.TypeId, entry.Value);

            /* all Inventories */
            for (int i = 0; i < slimBlock.FatBlock.InventoryCount; i++)
            {
                var inventory = slimBlock.FatBlock.GetInventory(i);
                var itemsList = inventory.GetItems();

                /* We loop through the items in reverse otherwise the we run out of bounds. */
                for (int j = itemsList.Count - 1; j >= 0; j--)
                {
                    var item = itemsList[j];
                    var typeId = item.Content.TypeId;

                    /* If that type is not in dictionary ignore. */
                    if (typeIdDict.TryGetValue(typeId, out int value))
                    {
                        value--;

                        /* checking if we are below defined limit */
                        if (value < 0)
                        {
                            inventory.RemoveItemsAt(j);
                            NeedRefresh = true;
                        }

                        /* writing reduced value back in dictionary */
                        typeIdDict[itemsList[j].Content.TypeId] = value;
                    }
                }

                /* We probably (most likely) only need to refresh it once. after we are done fiddling around with it. */
                if (NeedRefresh)
                    inventory.Refresh();
            }
        }

        public static void SearchAndDeleteItemStacks()
        {
            /* This part of code belong to LordTylus great plugin dev! */
            List<MySlimBlock> blocks = GetBlocksFromAllGrids((block) =>
            {
                if (block.FatBlock == null || !block.FatBlock.HasInventory)
                    return false;

                if (block.CubeGrid.PlayerPresenceTier != MyUpdateTiersPlayerPresence.Normal)
                    return false;

                MyDefinitionId OxygenContainer = MyDefinitionId.Parse("MyObjectBuilder_OxygenContainerObject/OxygenBottle");
                MyDefinitionId GasContainer = MyDefinitionId.Parse("MyObjectBuilder_GasContainerObject/HydrogenBottle");
                MyDefinitionId PhysicalGun = MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/");

                CleanupDictionary.Clear();

                CleanupDictionary.Add(OxygenContainer, 70);
                CleanupDictionary.Add(GasContainer, 80);
                CleanupDictionary.Add(PhysicalGun, 150);

                try
                {
                    if (block.FatBlock.GetInventory().ItemCount >= 300)
                        FindAndCleanInventorys(block, CleanupDictionary);

                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
                return false;
            });
        }

        private static void CheckBlock(MyFunctionalBlock __instance)
        {
            if (DePatchPlugin.Instance.Config.Enabled)
            {
                if ((++CleanupTick >= 25) && DePatchPlugin.Instance.Config.CargoCleanup)
                {
                    CleanupTick = 0;
                    SearchAndDeleteItemStacks();
                }

                if (DePatchPlugin.Instance.Config.EnableGridMaxSpeedPurge)
                {
                    if (__instance != null && __instance.CubeGrid.Physics != null && __instance.CubeGrid.PlayerPresenceTier == MyUpdateTiersPlayerPresence.Normal)
                    {
                        float speedlarge = DePatchPlugin.Instance.Config.LargeGridMaxSpeedPurge;
                        float speedsmall = DePatchPlugin.Instance.Config.SmallGridMaxSpeedPurge;
                        switch (((IMyCubeGrid)__instance.CubeGrid).GridSizeEnum)
                        {
                            case MyCubeSize.Large when __instance.CubeGrid.Physics.LinearVelocity.Length() > speedlarge || __instance.CubeGrid.Physics.AngularVelocity.Length() > speedlarge:
                            case MyCubeSize.Small when __instance.CubeGrid.Physics.LinearVelocity.Length() > speedsmall || __instance.CubeGrid.Physics.AngularVelocity.Length() > speedsmall:
                                {
                                    if (DePatchPlugin.Instance.Config.SpeedingModeSelector == SpeedingMode.StopGrid)
                                    {
                                        if (++StopTick >= 30)
                                        {
                                            StopTick = 0;
                                            __instance.CubeGrid.Physics.ClearSpeed();

                                            if (((IMyCubeGrid)__instance.CubeGrid).GridSizeEnum == MyCubeSize.Large)
                                                Log.Error("Large Grid Name =" + ((MyCubeGrid)(IMyCubeGrid)__instance.CubeGrid).DisplayName + " Detected Flying Above Max Speed " + speedlarge + "ms" + " and was STOPPED");

                                            if (((IMyCubeGrid)__instance.CubeGrid).GridSizeEnum == MyCubeSize.Small)
                                                Log.Error("Small Grid Name =" + ((MyCubeGrid)(IMyCubeGrid)__instance.CubeGrid).DisplayName + " Detected Flying Above Max Speed " + speedsmall + "ms" + " and was STOPPED");
                                        }
                                    }
                                    else if (DePatchPlugin.Instance.Config.SpeedingModeSelector == SpeedingMode.ShowLogOnly)
                                    {
                                        if (++StopTick >= 30)
                                        {
                                            StopTick = 0;
                                            if (((IMyCubeGrid)__instance.CubeGrid).GridSizeEnum == MyCubeSize.Large)
                                                Log.Error("Large Grid Name =" + ((MyCubeGrid)(IMyCubeGrid)__instance.CubeGrid).DisplayName + " Detected Flying Above Max Speed " + speedlarge + "ms" + " and was not DELETED");

                                            if (((IMyCubeGrid)__instance.CubeGrid).GridSizeEnum == MyCubeSize.Small)
                                                Log.Error("Small Grid Name =" + ((MyCubeGrid)(IMyCubeGrid)__instance.CubeGrid).DisplayName + " Detected Flying Above Max Speed " + speedsmall + "ms" + " and was not DELETED");
                                        }
                                    }
                                    else if (DePatchPlugin.Instance.Config.SpeedingModeSelector == SpeedingMode.DeleteGrid)
                                    {
                                        foreach (var a in __instance.CubeGrid.GetFatBlocks<MyCockpit>())
                                        {
                                            if (a != null)
                                                a.RemovePilot();
                                        }
                                        foreach (var b in __instance.CubeGrid.GetFatBlocks<MyCryoChamber>())
                                        {
                                            if (b != null)
                                                b.RemovePilot();
                                        }

                                        if (((IMyCubeGrid)__instance.CubeGrid).GridSizeEnum == MyCubeSize.Large)
                                            Log.Error("Large Grid Name =" + ((MyCubeGrid)(IMyCubeGrid)__instance.CubeGrid).DisplayName + " Detected Flying Above Max Speed " + speedlarge + "ms" + " and was DELETED");

                                        if (((IMyCubeGrid)__instance.CubeGrid).GridSizeEnum == MyCubeSize.Small)
                                            Log.Error("Small Grid Name =" + ((MyCubeGrid)(IMyCubeGrid)__instance.CubeGrid).DisplayName + " Detected Flying Above Max Speed " + speedsmall + "ms" + " and was DELETED");

                                        __instance.CubeGrid.Close();
                                    }
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }

                if (DePatchPlugin.Instance.Config.EnableBlockDisabler)
                {
                    if (__instance != null && __instance.IsFunctional && __instance.Enabled)
                    {
                        if (string.Compare("ShipWelder", __instance.BlockDefinition.Id.TypeId.ToString().Substring(16), StringComparison.InvariantCultureIgnoreCase) == 0 ||
                            string.Compare("MyProgrammableBlock", __instance.BlockDefinition.Id.TypeId.ToString().Substring(16), StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            // weldertypes are off in ShipWelderPatch.cs
                            // ProgramBlocksTypes are off in MyProgramBlockSlow.cs
                        }
                        else
                        {
                            if (!MySession.Static.Players.IsPlayerOnline(__instance.OwnerId))
                            {
                                if (PlayersUtility.KeepBlockOff(__instance))
                                    __instance.Enabled = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
