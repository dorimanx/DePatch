using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;

/* this code is obsolete, leaving it for reference.

namespace DePatch.GamePatches
{
    public static class CargoCleanup
    {
        public static List<MySlimBlock> GetBlocksFromAllGrids(Func<MySlimBlock, bool> filter) { return MyEntities.GetEntities().OfType<MyCubeGrid>().SelectMany(grid => grid.GetBlocks().Where(filter)).ToList(); }

        private static Dictionary<MyDefinitionId, int> CleanupDictionary = new Dictionary<MyDefinitionId, int>();

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static bool DictionaryUpdated = false;

        private static void FindAndCleanInventorys(MySlimBlock slimBlock, Dictionary<MyDefinitionId, int> amountDict)
        {
            // This part of code belong to LordTylus great plugin dev!
            var typeIdDict = new Dictionary<MyObjectBuilderType, int>();
            var NeedRefresh = false;

            // get the amounds per TypeId instead of definition
            foreach (var entry in amountDict)
                typeIdDict.Add(entry.Key.TypeId, entry.Value);

            // all Inventories
            for (int i = 0; i < slimBlock.FatBlock.InventoryCount; i++)
            {
                var inventory = slimBlock.FatBlock.GetInventory(i);
                var itemsList = inventory.GetItems();

                // We loop through the items in reverse otherwise the we run out of bounds.
                for (int j = itemsList.Count - 1; j >= 0; j--)
                {
                    var item = itemsList[j];
                    var typeId = item.Content.TypeId;

                    // If that type is not in dictionary ignore.
                    if (typeIdDict.TryGetValue(typeId, out int value))
                    {
                        value--;

                        // checking if we are below defined limit
                        if (value < 0)
                        {
                            inventory.RemoveItemsOfType(1, item.Content);
                            NeedRefresh = true;
                        }

                        // writing reduced value back in dictionary
                        typeIdDict[itemsList[j].Content.TypeId] = value;
                    }
                }

                // We probably (most likely) only need to refresh it once. after we are done fiddling around with it.
                if (NeedRefresh)
                    inventory.Refresh();
            }
        }

        public static void SearchAndDeleteItemStacks()
        {
            // This part of code belong to LordTylus great plugin dev!
            List<MySlimBlock> blocks = GetBlocksFromAllGrids((block) =>
            {
                if (block.FatBlock == null || !block.FatBlock.HasInventory)
                    return true;

                if (block.CubeGrid.PlayerPresenceTier != MyUpdateTiersPlayerPresence.Normal)
                    return true;

                if (block.FatBlock.GetInventory().ItemCount <= 500)
                    return true;

                if (!DictionaryUpdated)
                {
                    MyDefinitionId OxygenContainer = MyDefinitionId.Parse("MyObjectBuilder_OxygenContainerObject/OxygenBottle");
                    MyDefinitionId GasContainer = MyDefinitionId.Parse("MyObjectBuilder_GasContainerObject/HydrogenBottle");
                    MyDefinitionId PhysicalGun = MyDefinitionId.Parse("MyObjectBuilder_PhysicalGunObject/");

                    CleanupDictionary.Add(OxygenContainer, 70);
                    CleanupDictionary.Add(GasContainer, 80);
                    CleanupDictionary.Add(PhysicalGun, 150);
                    DictionaryUpdated = true;
                }

                try
                {
                    FindAndCleanInventorys(block, CleanupDictionary);
                    return true;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    return true;
                }
            });
        }
    }
}
*/
