using DePatch.CoolDown;
using HarmonyLib;
using Sandbox.Game.Entities.Cube;
using System.Threading.Tasks;
using VRage.Game;

namespace DePatch.GamePatches
{
    [HarmonyPatch(typeof(MyAssembler), "UpdateBeforeSimulation100")]
    internal class MyAssemblerPatch
    {
        private static readonly SteamIdCooldownKey LoopRequestID = new SteamIdCooldownKey(76000000000000002);
        private static readonly int LoopCooldown = 40 * 1000;

        private static bool Prefix(MyAssembler __instance)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return true;

            try
            {
                if (__instance != null && __instance.Enabled && __instance.IsFunctional)
                {
                    if (__instance.IsSlave && DePatchPlugin.Instance.Config.DisableAssemblerCoop)
                    {
                        __instance.IsSlave = false;

                        if (!__instance.IsQueueEmpty)
                            __instance.ClearQueue();
                    }
                    if (__instance.RepeatEnabled && DePatchPlugin.Instance.Config.DisableAssemblerLoop)
                    {
                        __instance.RequestRepeatEnabled(false);
                        if (!__instance.IsQueueEmpty)
                            __instance.ClearQueue();
                    }
                    if (DePatchPlugin.Instance.Config.DisableProductionOnShip)
                    {
                        if (!__instance.CubeGrid.IsStatic && __instance.CubeGrid.GridSizeEnum == MyCubeSize.Large)
                        {
                            __instance.Enabled = false;

                            if (!__instance.IsQueueEmpty)
                                __instance.ClearQueue();
                        }
                    }
                }
            }
            catch
            {
            }

            if (DePatchPlugin.Instance.Config.CargoCleanup)
            {
                /// loop for 30 sec till next grid add / remove
                if (!CooldownManager.CheckCooldown(LoopRequestID, null, out var LoopremainingSeconds))
                {
                    var LoopTimer = LoopremainingSeconds;
                    return true;
                }
                CooldownManager.StartCooldown(LoopRequestID, null, LoopCooldown);
                Task.Run(delegate
                {
                    CargoCleanup.SearchAndDeleteItemStacks();
                });
            }
            return true;
        }
    }
}
