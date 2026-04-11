using Sandbox.Definitions;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.EntityComponents;
using Torch.Managers.PatchManager;

namespace DePatch.KEEN_BUG_FIXES
{
    [PatchShim]
    public static class AssemblerStatePatch
    {
        public static void Patch(PatchContext ctx) => ctx.Suffix(typeof(MyAssembler), typeof(AssemblerStatePatch), nameof(CheckInventory));

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
                    if (__instance.CurrentState == MyAssembler.StateEnum.NotEnoughPower)
                        __result = MyAssembler.StateEnum.Ok;
                }
            }
        }
    }
}
