using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.EntityComponents;
using Torch.Managers.PatchManager;
using VRage;
using VRage.Game;

namespace DePatch.KEEN_BUG_FIXES
{
    [PatchShim]
    public static class AssemblerStatePatch
    {
        public static void Patch(PatchContext ctx) => ctx.Suffix(typeof(MyAssembler), typeof(AssemblerStatePatch), nameof(CheckInventory));

        private static bool CheckInventoryCapacity(MyInventory inventory, MyBlueprintDefinitionBase.Item[] items, MyFixedPoint amountMultiplier)
        {
            MyFixedPoint myFixedPoint = 0;
            foreach (MyBlueprintDefinitionBase.Item item in items)
            {
                MyPhysicalItemDefinition physicalItemDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(item.Id);
                if (physicalItemDefinition != null)
                {
                    myFixedPoint += (MyFixedPoint)physicalItemDefinition.Volume * item.Amount * amountMultiplier;
                }
            }

            return inventory.CurrentVolume + myFixedPoint <= inventory.MaxVolume;
        }

        private static bool CheckInventoryContents(MyInventory inventory, MyBlueprintDefinitionBase.Item[] item, MyFixedPoint amountMultiplier)
        {
            for (int i = 0; i < item.Length; i++)
            {
                if (!inventory.ContainItems(new MyFixedPoint?(item[i].Amount * amountMultiplier), item[i].Id, MyItemFlags.None))
                {
                    return false;
                }
            }
            return true;
        }

        private static void CheckInventory(MyAssembler __instance, ref MyAssembler.StateEnum __result, MyBlueprintDefinitionBase blueprint)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

            // This code was been provided by @fin_mk Discord, great graditude for sharing the fix.

            if (__instance == null || !__instance.Enabled)
                return;

            var AssemblerDef = (MyProductionBlockDefinition)__instance.BlockDefinition;

            if (AssemblerDef != null && MyResourceDistributorComponent.ElectricityId != null)
            {
                var electroExist = __instance.ResourceSink.IsPowerAvailable(MyResourceDistributorComponent.ElectricityId, AssemblerDef.OperationalPowerConsumption);

                if (!electroExist)
                {
                    __result = MyAssembler.StateEnum.NotEnoughPower;
                }
                else
                {
                    MyFixedPoint amountMultiplier = (MyFixedPoint)(1f / __instance.GetEfficiencyMultiplierForBlueprint(blueprint));

                    if (__instance.DisassembleEnabled)
                    {
                        if (!CheckInventoryCapacity(__instance.InputInventory, blueprint.Prerequisites, amountMultiplier))
                        {
                            __result = MyAssembler.StateEnum.InventoryFull;
                            return;
                        }

                        if (!CheckInventoryContents(__instance.OutputInventory, blueprint.Results, 1))
                        {
                            __result = MyAssembler.StateEnum.MissingItems;
                            return;
                        }
                    }
                    else
                    {
                        if (!CheckInventoryCapacity(__instance.OutputInventory, blueprint.Results, 1))
                        {
                            __result = MyAssembler.StateEnum.InventoryFull;
                            return;
                        }

                        if (!CheckInventoryContents(__instance.InputInventory, blueprint.Prerequisites, amountMultiplier))
                        {
                            __result = MyAssembler.StateEnum.MissingItems;
                            return;
                        }
                    }

                    if (__instance.CurrentState == MyAssembler.StateEnum.NotEnoughPower)
                        __result = MyAssembler.StateEnum.Ok;
                }
            }
        }
    }
}
