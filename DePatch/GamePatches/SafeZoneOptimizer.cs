using NLog;
using Sandbox.Game.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;
using Torch.Managers.PatchManager;
using Torch.Managers.PatchManager.MSIL;

namespace DePatch.GamePatches
{
    /* suspended till more results.
    [PatchShim]
    public static class SafeZoneOptimizer
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // original code by Ryo, ryo#0771

        static readonly string[] _methodNames =
        {
            "UpdateOnceBeforeFrame",
            "UpdateBeforeSimulation",
            "Simulate",
            "UpdateAfterSimulation",
            "UpdateBeforeSimulation10",
            "UpdateAfterSimulation10",
            "UpdateBeforeSimulation100",
            "UpdateAfterSimulation100",
        };

        public static void Patch(PatchContext ctx)
        {
            var transpile = typeof(SafeZoneOptimizer).GetMethod_RYO(nameof(Transpile));

            foreach (var methodName in _methodNames)
            {
                var method = typeof(MySafeZone).GetMethod_RYO(methodName);
                ctx.GetPattern(method).Transpilers.Add(transpile);
            }
        }

        static IEnumerable<MsilInstruction> Transpile(IEnumerable<MsilInstruction> instructions)
        {
            yield return new MsilInstruction(OpCodes.Ret);
        }
    }
    */
}
