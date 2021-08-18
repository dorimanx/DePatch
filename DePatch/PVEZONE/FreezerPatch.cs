using System;
using System.Collections.Generic;
using DePatch.PVEZONE;
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
            var revealMethod = AccessTools.Method(plugin.GetType().Assembly.GetType("Slime.Freezer", true), "RevealGroup");
            harmony.Patch(revealMethod, postfix: new HarmonyMethod(AccessTools.Method(typeof(FreezerPatch), nameof(Postfix))));
        }

        private static void Postfix(int __result, object group)
        {
            if (__result < 1)
                return;

            dynamic frozenInfo = group;
            var grids = (List<MyCubeGrid>)frozenInfo.Grids;

            grids.ForEach(MyNewGridPatch.CubeGridInit);
        }
    }
}