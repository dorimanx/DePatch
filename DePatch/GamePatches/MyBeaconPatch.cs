using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities.Cube;
using Torch.Managers.PatchManager;
using VRage.Game;

namespace DePatch.GamePatches
{
    [PatchShim]

    internal static class MyBeaconPatch
    {
        public static void Patch(PatchContext ctx) => ctx.Suffix(typeof(MyBeacon), "Init", typeof(MyBeaconPatch), nameof(BeaconInit), new[] { "objectBuilder", "cubeGrid" });

        private static void BeaconInit(MyBeacon __instance, MyObjectBuilder_CubeBlock objectBuilder)
        {
            if (!DePatchPlugin.Instance.Config.Enabled || !DePatchPlugin.Instance.Config.ReduceBeaconRadius)
                return;

            MyObjectBuilder_Beacon myObjectBuilder_Beacon = (MyObjectBuilder_Beacon)objectBuilder;

            if (myObjectBuilder_Beacon.BroadcastRadius == 20000f || myObjectBuilder_Beacon.BroadcastRadius == 5000f)
            {
                __instance.RadioBroadcaster.BroadcastRadius = 200f;
                __instance.RaisePropertiesChanged();
            }
        }
    }
}