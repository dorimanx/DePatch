using System;
using System.Collections.Generic;
using HarmonyLib;
using NLog;
using Sandbox.Game.Entities;
using Torch.API;
using Torch.API.Managers;
using Torch.Managers;

namespace DePatch.PVEZONE
{
    public static class FreezerPatch
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public static void ApplyPatch(Harmony harmony, ITorchBase torch)
        {
            if (!torch.Managers.GetManager<PluginManager>().Plugins.TryGetValue(new Guid("3d875183-28f1-4ada-8ef6-b15f126988e2"), out var plugin))
                return;

            Log.Info("Initializing compatibility for Freezer plugin");

            // To DO, replace after new freezer released
            var revealMethod_Old = AccessTools.Method(plugin.GetType().Assembly.GetType("Slime.Freezer", false), "RevealGroup");
            var revealMethod_New = AccessTools.Method(plugin.GetType().Assembly.GetType("Freezer.FreezerLogic", false), "RevealGroup");

            if (revealMethod_Old == null)
            {
                if (revealMethod_New == null)
                    Log.Error("Initializing Freezer plugin Failed, Method Probably Changed");
                else
                {
                    harmony.Patch(revealMethod_New, postfix: new HarmonyMethod(AccessTools.Method(typeof(FreezerPatch), nameof(Postfix))));
                    Log.Info("Compatibility for Freezer plugin initialized");
                }

                return;
            }

            harmony.Patch(revealMethod_Old, postfix: new HarmonyMethod(AccessTools.Method(typeof(FreezerPatch), nameof(Postfix))));
            Log.Info("Compatibility for Freezer plugin initialized");
        }

        private static void Postfix(int __result, object group)
        {
            if (!DePatchPlugin.Instance.Config.Enabled)
                return;

            if (__result < 1)
                return;

            dynamic frozenInfo = group;
            var grids = (List<MyCubeGrid>)frozenInfo.Grids;

            grids.ForEach(MyNewGridPatch.CubeGridInit);
        }
    }
}